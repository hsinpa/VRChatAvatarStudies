using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class BasicShaderSettings : YMT_FeatureBase
    {

        private static bool _BasicShaderSettings_Foldout = true;
        private static int _StencilNo_Setting = 0;

        private static MaterialProperty _Clipping_Level = null;
        private static MaterialProperty _Tweak_transparency = null;
        private static bool _debugFolderFoldOut = false;

        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _Clipping_Level, "_Clipping_Level");
            ymtoon.FindProp(ref _Tweak_transparency, "_Tweak_transparency");
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;

            DrawFoldOutMenu(ref _BasicShaderSettings_Foldout, "【Rendering Settings】", Color.gray,
             () =>
            {
                DrawContentWithIndent(() =>
                {
                    CullingSettings.Draw(ymtoon, m_MaterialEditor);

                    BlendModeSettings.Draw(ymtoon, m_MaterialEditor);
                    var currentBlendMode = BlendModeSettings.blendMode;

                    if (material.HasProperty("_StencilNo"))
                    {

                        GUI_SetStencilNo(material);
                    }

                    if (currentBlendMode == BlendModeSettings.BlendMode.Cutout)
                    {
                        GUI_SetClippingMask(material);

                    }
                    else if (currentBlendMode == BlendModeSettings.BlendMode.Transparent)
                    {
                        GUI_SetTransparencySetting(material);

                    }

                    DrawFoldOutSubMenu(ref _debugFolderFoldOut, "Debug", () =>
                    {
                        GUI_OptionMenu(ymtoon, material);
                    });

                    // DrawFoldOutSubMenu(ref _debugFolderFoldOut, "Debug", () => DebugDraw(material));
                });
            });
        }

        private static void GUI_SetStencilNo(Material material)
        {
            EditorGUILayout.LabelField("For _StencilMask or _StencilOut Shader", EditorStyles.boldLabel);
            _StencilNo_Setting = material.GetInt("_StencilNo");
            int _Current_StencilNo = _StencilNo_Setting;
            _Current_StencilNo = (int)EditorGUILayout.IntField("Stencil No.", _Current_StencilNo);
            SetInt(material, "_StencilNo", _Current_StencilNo);
        }

        private static void GUI_SetClippingMask(Material material)
        {
            DrawContentWithIndent(() =>
            {
                DrawToggleButton(material, "Inverse Clipping Mask", "_Inverse_Clipping");
                m_MaterialEditor.RangeProperty(_Clipping_Level, "Clipping Level");
            });
        }

        private static void GUI_SetTransparencySetting(Material material)
        {
            DrawContentWithIndent(() =>
            {
                m_MaterialEditor.RangeProperty(_Tweak_transparency, "Transparency Level");
            });
        }

        private static void GUI_OptionMenu(YMToon2GUI ymtoon, Material material)
        {

            DrawExecuteButton("Disable All Debug Mode", () => { DisableAllDebugCheck(material); });

            EditorGUILayout.Space();

            ShaderKeywordSettings.Draw(ymtoon, m_MaterialEditor);

            DrawExecuteButton("Repair", () => { Repair(material); });

            // VRChatSettings.Draw (ymtoon, m_MaterialEditor);
        }

        private static void DisableAllDebugCheck(Material material)
        {
            AdditionalLightingSettings.DisableAllDebugDraw(material);
            BaseColorSettings.DisableAllDebugDraw(material);
            BasicShaderSettings.DisableAllDebugDraw(material);
            BlendModeSettings.DisableAllDebugDraw(material);
            CullingSettings.DisableAllDebugDraw(material);
            EmissiveSettings.DisableAllDebugDraw(material);
            FakeSubSurfaceScatteringSettings.DisableAllDebugDraw(material);
            SpecularSettings.DisableAllDebugDraw(material);
            NormalMapSettings.DisableAllDebugDraw(material);
            OutlineSettings.DisableAllDebugDraw(material);
            RimLightSettings.DisableAllDebugDraw(material);
            MatCapSettings.DisableAllDebugDraw(material);
            TessellationSettings.DisableAllDebugDraw(material);
        }

        private static readonly Dictionary<string, float> fixParamPairs = new Dictionary<string, float> {
            //これがオンのままだとsceneの光源を回しても動かない
            { "_Use_OwnDirLight", 0.0f },

            //stepが0になっているとうまく計算できない場合がある
            { "_1st_Shade_Step", 0.5f },
            { "_1st_Shade_Feather", 0.0001f },
            { "_2nd_Shade_Step", 0.0f },
            { "_2nd_Shade_Feather", 0.0001f },
        };

        private static readonly HashSet<string> toggleParamCandidates = new HashSet<string> { "is", "use" };

        private static void Repair(Material material)
        {
            var fixParamStrings = "fix targets \n\n";

            foreach (var key in fixParamPairs.Keys)
            {
                var isToggle = false;
                foreach (var candidate in toggleParamCandidates)
                {
                    isToggle = key.ToLower().Contains(candidate);
                    if (isToggle == true) { break; }
                }

                //bool っぽいやつは true false で出したい
                var param = isToggle ?
                    (fixParamPairs[key] >= 1.0f ? "true" : "false") :
                    fixParamPairs[key].ToString();

                fixParamStrings += key + " → " + param + "\n";
            }

            if (!EditorUtility.DisplayDialog("Fix Some Troubles",
                    "不具合が多分治りますが、一部パラメータが初期値に戻ります\n\n" + fixParamStrings,
                    "OK", "No"))
            {
                return;
            }

            foreach (var key in fixParamPairs.Keys)
            {
                SetFloat(material, key, fixParamPairs[key]);
                Debug.Log(key + " " + fixParamPairs[key]);
            }

            Debug.Log("たぶんなおったはず");
        }

        new protected static void DebugDraw(Material material)
        {

        }

        new public static void DisableAllDebugDraw(Material material)
        {

        }
    }

}
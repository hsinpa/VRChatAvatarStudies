using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class RimLightSettings : YMT_FeatureBase
    {

        private static bool _RimLight_Foldout = true;
        private static bool _AdvancedOutline_Foldout = false;

        private static string DEBUG_RIM_LIGHT => "_DEBUG_RIM_LIGHT";

        private static MaterialProperty _1st_RimLightColor = null;
        private static MaterialProperty _1st_RimLight_Power = null;
        private static MaterialProperty _1st_RimLight_InsideMask = null;
        private static MaterialProperty _2nd_RimLightColor = null;
        private static MaterialProperty _2nd_RimLight_Power = null;
        private static MaterialProperty _2nd_RimLight_InsideMask = null;
        private static MaterialProperty _Tweak_LightDirection_MaskLevel = null;
        private static MaterialProperty _Ap_RimLightColor = null;
        private static MaterialProperty _Ap_RimLight_Power = null;
        private static MaterialProperty _Tweak_1st_RimLightMaskLevel = null;
        private static MaterialProperty _Tweak_2nd_RimLightMaskLevel = null;

        private static bool _debugFolderFoldOut = false;

        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _1st_RimLightColor, "_1st_RimLightColor");
            ymtoon.FindProp(ref _1st_RimLight_Power, "_1st_RimLight_Power");
            ymtoon.FindProp(ref _1st_RimLight_InsideMask, "_1st_RimLight_InsideMask");

            ymtoon.FindProp(ref _2nd_RimLightColor, "_2nd_RimLightColor");
            ymtoon.FindProp(ref _2nd_RimLight_Power, "_2nd_RimLight_Power");
            ymtoon.FindProp(ref _2nd_RimLight_InsideMask, "_2nd_RimLight_InsideMask");

            ymtoon.FindProp(ref _Tweak_LightDirection_MaskLevel, "_Tweak_LightDirection_MaskLevel");

            ymtoon.FindProp(ref _Tweak_1st_RimLightMaskLevel, "_Tweak_1st_RimLightMaskLevel");
            ymtoon.FindProp(ref _Tweak_2nd_RimLightMaskLevel, "_Tweak_2nd_RimLightMaskLevel");

            ymtoon.FindProp(ref _Ap_RimLightColor, "_Ap_RimLightColor");
            ymtoon.FindProp(ref _Ap_RimLight_Power, "_Ap_RimLight_Power");


            BackwardCompatible(ymtoon);
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;

            UITabColorDic.TryGetValue(UITab.RimLight, out Color color);
            DrawFoldOutMenu(ref _RimLight_Foldout, "【RimLight】", color,
                () =>
                {
                    EditorGUILayout.Space();
                    GUI_RimLight(material);
                }
            );
            BackwardFollowValue();
        }

        private static void GUI_RimLight(Material material)
        {

            var useRimLight = DrawToggleButton(material, "RimLight", "_RimLight");

            if (useRimLight)
            {
                DrawContentWithIndent(() =>
                {
                    EditorGUILayout.LabelField("RimLight Mask : BaseOptMap.r", EditorStyles.boldLabel);
                    m_MaterialEditor.ColorProperty(_1st_RimLightColor, "1st RimLight Color");
                    m_MaterialEditor.RangeProperty(_1st_RimLight_Power, "1st RimLight Power");

                    m_MaterialEditor.RangeProperty(_1st_RimLight_InsideMask, "1st RimLight Inside Mask");
                    DrawToggleButton(material, "1st RimLight Feather", "_1st_RimLight_Feather");
                    DrawToggleButton(material, "1st RimLight Mode", "_Is_1st_RimLight_Addtive", "Multiply", "Additive");
                    m_MaterialEditor.RangeProperty(_Tweak_1st_RimLightMaskLevel, "1st RimLight Mask Level");

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    m_MaterialEditor.ColorProperty(_2nd_RimLightColor, "2nd RimLight Color");
                    m_MaterialEditor.RangeProperty(_2nd_RimLight_Power, "2nd RimLight Power");

                    m_MaterialEditor.RangeProperty(_2nd_RimLight_InsideMask, "2nd RimLight Inside Mask");
                    DrawToggleButton(material, "2nd RimLight Feather", "_2nd_RimLight_Feather");
                    DrawToggleButton(material, "2nd RimLight Mode", "_Is_2nd_RimLight_Addtive", "Multiply", "Additive");

                    m_MaterialEditor.RangeProperty(_Tweak_2nd_RimLightMaskLevel, "2nd RimLight Mask Level");

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    DrawFoldOutSubMenu(ref _AdvancedOutline_Foldout, "Advanced Settings", () =>
                    {
                        var useLightDirectionMask = DrawToggleButton(material, "LightDirection Mask", "_LightDirection_MaskOn");

                        if (useLightDirectionMask)
                        {
                            DrawContentWithIndent(() =>
                            {
                                m_MaterialEditor.RangeProperty(_Tweak_LightDirection_MaskLevel, "LightDirection MaskLevel");

                                var addApRimLight = DrawToggleButton(material, "Antipodean(Ap)_RimLight", "_Add_Antipodean_RimLight");

                                if (addApRimLight)
                                {
                                    DrawContentWithIndent(() =>
                                    {
                                        EditorGUILayout.LabelField("Ap_RimLight Settings", EditorStyles.boldLabel);
                                        m_MaterialEditor.ColorProperty(_Ap_RimLightColor, "Ap_RimLight Color");
                                        m_MaterialEditor.RangeProperty(_Ap_RimLight_Power, "Ap_RimLight Power");

                                        DrawToggleButton(material, "Ap_RimLight Feather", "_Ap_RimLight_Feather");
                                        EditorGUILayout.Space();
                                    });
                                }
                            });
                        }
                    });

                    EditorGUILayout.Space();
                });
                DrawFoldOutSubMenu(ref _debugFolderFoldOut, "Debug", () => DebugDraw(material));
            }
            else
            {
                DisableAllDebugDraw(material);
                _debugFolderFoldOut = false;
            }

            EditorGUILayout.Space();

        }
        #region 後方互換 しばらくしたら消す
        private static MaterialProperty _RimLightColor = null;
        private static MaterialProperty _RimLight_Power = null;
        private static MaterialProperty _RimLight_InsideMask = null;
        private static MaterialProperty _Tweak_RimLightMaskLevel = null;

        private static void BackwardCompatible(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _RimLightColor, "_RimLightColor");
            ymtoon.FindProp(ref _RimLight_Power, "_RimLight_Power");
            ymtoon.FindProp(ref _RimLight_InsideMask, "_RimLight_InsideMask");
            ymtoon.FindProp(ref _Tweak_RimLightMaskLevel, "_Tweak_RimLightMaskLevel");
            SetBackwardCompatibleValue<Color>(_RimLightColor, _1st_RimLightColor);
            SetBackwardCompatibleValue<float>(_RimLight_Power, _1st_RimLight_Power);
            SetBackwardCompatibleValue<float>(_RimLight_InsideMask, _1st_RimLight_InsideMask);
            SetBackwardCompatibleValue<float>(_Tweak_RimLightMaskLevel, _Tweak_1st_RimLightMaskLevel);
        }

        private static void BackwardFollowValue()
        {
            SetBackwardFollowValue<Color>(_RimLightColor, _1st_RimLightColor);
            SetBackwardFollowValue<float>(_RimLight_Power, _1st_RimLight_Power);
            SetBackwardFollowValue<float>(_RimLight_InsideMask, _1st_RimLight_InsideMask);
            SetBackwardFollowValue<float>(_Tweak_RimLightMaskLevel, _Tweak_1st_RimLightMaskLevel);
        }
        #endregion
        new protected static void DebugDraw(Material material)
        {
            DrawContentWithIndent(() =>
            {
                DrawKeywordToggle(material, "Debug RimLight", DEBUG_RIM_LIGHT);
            });
        }

        new public static void DisableAllDebugDraw(Material material)
        {
            SetKeyword(material, DEBUG_RIM_LIGHT, false);
        }
    }

}
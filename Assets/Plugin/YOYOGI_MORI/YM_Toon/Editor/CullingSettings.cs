using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class CullingSettings : YMT_FeatureBase
    {

        public enum CullingMode
        {
            CullingOff,
            FrontCulling,
            BackCulling
        }

        public static CullingMode cullingMode = CullingMode.BackCulling;
        private static string CULL_MODE => "_CullMode";
        private static string ZWRITE4FORWARD => "_ZWrite4Forward";
        private static MaterialProperty _ZWrite4Forward = null;
        // private static bool _debugFolderFoldOut = false;

        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _ZWrite4Forward, ZWRITE4FORWARD);
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;

            GUI_SetCullingMode(material);
            // DrawFoldOutSubMenu(ref _debugFolderFoldOut, "Debug", () => DebugDraw(material));
            EditorGUILayout.Space();

        }

        private static void GUI_SetCullingMode(Material material)
        {
            cullingMode = (CullingMode)material.GetInt(CULL_MODE);
            var preCullingMode = cullingMode;

            ShaderValidateCheck(material);

            cullingMode = (CullingMode)EditorGUILayout.EnumPopup("Culling Mode", cullingMode);
            SetFloat(material, CULL_MODE, (int)cullingMode);

            if (cullingMode == preCullingMode) { return; }

            switch (cullingMode)
            {
                case CullingMode.BackCulling:
                    {
                        SetShader(material, "YOYOGI_MORI/YMToon/Base");
                        break;
                    }
                case CullingMode.FrontCulling:
                    {
                        SetShader(material, "Hidden/YOYOGI_MORI/YMToon/Base/Front");
                        break;
                    }
                case CullingMode.CullingOff:
                    {
                        SetShader(material, "Hidden/YOYOGI_MORI/YMToon/Base/Off");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private static void ShaderValidateCheck(Material material)
        {
            //これだけは発生するはず
            if (material.shader == Shader.Find("YOYOGI_MORI/YMToon/Base") && cullingMode != CullingMode.BackCulling)
            {
                cullingMode = CullingMode.BackCulling;
            }

            //ねんのため
            if (material.shader == Shader.Find("Hidden/YOYOGI_MORI/YMToon/Base/Front") && cullingMode != CullingMode.FrontCulling)
            {
                cullingMode = CullingMode.FrontCulling;
            }

            if (material.shader == Shader.Find("Hidden/YOYOGI_MORI/YMToon/Base/Off") && cullingMode != CullingMode.CullingOff)
            {
                cullingMode = CullingMode.CullingOff;
            }
        }

        new protected static void DebugDraw(Material material)
        {

        }

        new public static void DisableAllDebugDraw(Material material)
        {

        }
    }

}

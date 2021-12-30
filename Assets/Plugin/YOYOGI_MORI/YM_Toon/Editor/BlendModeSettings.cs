using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class BlendModeSettings : YMT_FeatureBase
    {

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Transparent,
            // Trans_Clipping // Physically plausible transparency mode, implemented as alpha pre-multiply
        }

        private static string[] BlendNames => System.Enum.GetNames(typeof(BlendMode));

        private static string RENDER_TYPE => "RenderType";
        private static string SRC_BLEND => "_SrcBlend";
        private static string DST_BLEND => "_DstBlend";
        private static string ZWRITE => "_ZWrite";
        private static string ZWRITE4TRANS => "_ZWrite4Trans";

        private static MaterialProperty _Mode = null;
        private static MaterialProperty _ZWrite4Trans = null;
        public static BlendMode blendMode = BlendMode.Opaque;
        // private static bool _debugFolderFoldOut = false;

        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _Mode, "_Mode");
            ymtoon.FindProp(ref _ZWrite4Trans, "_ZWrite4Trans");
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;

            var material = m_MaterialEditor.target as Material;
            blendMode = BlendModePopup(material);
            // DrawFoldOutSubMenu(ref _debugFolderFoldOut, "Debug", () => DebugDraw(material));
        }
        private static BlendMode BlendModePopup(Material material)
        {
            var mode = (BlendMode)_Mode.floatValue;
            var preMode = mode;

            mode = (BlendMode)EditorGUILayout.Popup("Rendering Mode", (int)mode, BlendNames);

            if (mode != preMode)
            {
                _Mode.floatValue = (float)mode;
                SetupMaterialWithBlendMode(material, mode);
            }

            if (mode != BlendMode.Opaque)
            {
                _ZWrite4Trans.floatValue = DrawToggleButton(material, "ZWrite for Transparent", ZWRITE4TRANS) ? 1.0f : 0.0f;
            }

            EditorGUILayout.Space();

            m_MaterialEditor.RenderQueueField();
            return mode;
        }

        private static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    SetOverrideTag(material, RENDER_TYPE, "");
                    SetInt(material, SRC_BLEND, (int)UnityEngine.Rendering.BlendMode.One);
                    SetInt(material, DST_BLEND, (int)UnityEngine.Rendering.BlendMode.Zero);
                    SetInt(material, ZWRITE, 1);
                    SetInt(material, ZWRITE4TRANS, 1);
                    SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                    SetKeyword(material, "_ALPHATEST_ON", false);
                    SetRenderQueue(material, -1);
                    break;
                case BlendMode.Cutout:
                    SetOverrideTag(material, RENDER_TYPE, "TransparentCutout");
                    SetInt(material, SRC_BLEND, (int)UnityEngine.Rendering.BlendMode.One);
                    SetInt(material, DST_BLEND, (int)UnityEngine.Rendering.BlendMode.Zero);
                    SetInt(material, ZWRITE, 1);
                    SetInt(material, ZWRITE4TRANS, (int)_ZWrite4Trans.floatValue);
                    SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                    SetKeyword(material, "_ALPHATEST_ON", true);
                    SetRenderQueue(material, -1);
                    break;

                case BlendMode.Transparent:
                    SetOverrideTag(material, RENDER_TYPE, "Transparent");
                    SetInt(material, SRC_BLEND, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    SetInt(material, DST_BLEND, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    SetInt(material, ZWRITE, 0);
                    SetInt(material, ZWRITE4TRANS, (int)_ZWrite4Trans.floatValue);
                    SetKeyword(material, "_ALPHAPREMULTIPLY_ON", true);
                    SetKeyword(material, "_ALPHATEST_ON", false);
                    SetRenderQueue(material, (int)UnityEngine.Rendering.RenderQueue.Transparent);
                    break;
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
using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class EmissiveSettings : YMT_FeatureBase
    {

        public enum _EmissiveMode
        {
            SimpleEmissive,
            EmissiveAnimation
        }

        public static _EmissiveMode emissiveMode = _EmissiveMode.SimpleEmissive;

        private static bool _Emissive_Foldout = true;
        private static string DEBUG_EMISSIVE => "_DEBUG_EMISSIVE";

        private static MaterialProperty _EmissionMap = null;
        private static MaterialProperty _Emissive_Color = null;

        private static MaterialProperty _Base_Speed = null;
        private static MaterialProperty _Scroll_EmissiveU = null;
        private static MaterialProperty _Scroll_EmissiveV = null;
        private static MaterialProperty _Rotate_EmissiveUV = null;

        private static MaterialProperty _ColorShift = null;
        private static MaterialProperty _ColorShift_Speed = null;
        private static MaterialProperty _ViewShift = null;
        private static bool _debugFolderFoldOut = false;


        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _EmissionMap, "_EmissionMap");
            ymtoon.FindProp(ref _Emissive_Color, "_Emissive_Color");

            ymtoon.FindProp(ref _Base_Speed, "_Base_Speed");

            ymtoon.FindProp(ref _Scroll_EmissiveU, "_Scroll_EmissiveU");
            ymtoon.FindProp(ref _Scroll_EmissiveV, "_Scroll_EmissiveV");
            ymtoon.FindProp(ref _Rotate_EmissiveUV, "_Rotate_EmissiveUV");

            ymtoon.FindProp(ref _ColorShift, "_ColorShift");
            ymtoon.FindProp(ref _ColorShift_Speed, "_ColorShift_Speed");
            ymtoon.FindProp(ref _ViewShift, "_ViewShift");
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;

            UITabColorDic.TryGetValue(UITab.Emissive, out Color color);
            DrawFoldOutMenu(ref _Emissive_Foldout, "【Emissive】", color,
                () =>
                {
                    EditorGUILayout.Space();
                    GUI_Emissive(material);
                }
            );
        }

        private static void GUI_Emissive(Material material)
        {
            var useEmissive = DrawToggleButton(material, "Emissive", "_Use_Emissive");
            if (useEmissive)
            {
                EditorGUILayout.Space();
                DrawContentWithIndent(() =>
                {
                    m_MaterialEditor.TexturePropertyMiniThumbnail(GUILayoutUtility.GetRect(16, 24), _EmissionMap, Styles.emissionMapText.text, "");
                    DrawFixedSizeProp("", m_MaterialEditor, _Emissive_Color, -24f);
                    // m_MaterialEditor.TextureScaleOffsetProperty(_EmissionMap);
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

        new protected static void DebugDraw(Material material)
        {
            DrawContentWithIndent(() =>
            {
                DrawKeywordToggle(material, "Debug Emissive", DEBUG_EMISSIVE);
            });
        }

        new public static void DisableAllDebugDraw(Material material)
        {
            SetKeyword(material, DEBUG_EMISSIVE, false);

        }
    }

}
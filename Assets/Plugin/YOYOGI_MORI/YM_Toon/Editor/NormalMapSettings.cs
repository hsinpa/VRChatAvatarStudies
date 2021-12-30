using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class NormalMapSettings : YMT_FeatureBase
    {

        private static bool _NormalMap_Foldout = true;
        private static MaterialProperty _BumpMap = null;
        private static MaterialProperty _BumpScale = null;

        private static string DEBUG_ORIGINAL_NORMAL => "_DEBUG_ORIGINAL_NORMAL";
        private static string DEBUG_MAPPED_NORMAL => "_DEBUG_MAPPED_NORMAL";
        private static bool _debugFolderFoldOut = false;

        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _BumpMap, "_BumpMap");
            ymtoon.FindProp(ref _BumpScale, "_BumpScale");
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;

            UITabColorDic.TryGetValue(UITab.Normal, out Color color);
            DrawFoldOutMenu(ref _NormalMap_Foldout, "【Normal】", color,
             () =>
            {
                GUI_NormalMap(material);
            });
        }

        private static void GUI_NormalMap(Material material)
        {
            var useNormalMap = DrawToggleButton(material, "Normal", "_Use_Normal");
            if (useNormalMap)
            {
                m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, _BumpMap, _BumpScale);
                // m_MaterialEditor.TextureScaleOffsetProperty(_BumpMap);
                EditorGUILayout.Space();
                DrawFoldOutSubMenu(ref _debugFolderFoldOut, "Debug", () => DebugDraw(material));
            }
            else
            {
                DisableAllDebugDraw(material);
            }

            material.SetInt("_Is_NormalMapToBase", (_BumpMap != null && useNormalMap) ? 1 : 0);
            EditorGUILayout.Space();
        }

        new protected static void DebugDraw(Material material)
        {
            DrawContentWithIndent(() =>
            {
                DrawKeywordToggle(material, "Debug Original Normal", DEBUG_ORIGINAL_NORMAL);
                DrawKeywordToggle(material, "Debug Normal Map", DEBUG_MAPPED_NORMAL);
            });
        }

        new public static void DisableAllDebugDraw(Material material)
        {
            SetKeyword(material, DEBUG_ORIGINAL_NORMAL, false);
            SetKeyword(material, DEBUG_MAPPED_NORMAL, false);
        }
    }

}
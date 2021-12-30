using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class OutlineSettings : YMT_FeatureBase
    {
        private static bool _Outline_Foldout = false;
        private static bool _AdvancedOutline_Foldout = false;

        private static MaterialProperty _Outline_Width = null;
        private static MaterialProperty _Outline_Color = null;
        private static MaterialProperty _Is_BlendBaseColorWeight = null;
        private static MaterialProperty _Farthest_Distance = null;
        private static MaterialProperty _Nearest_Distance = null;
        private static MaterialProperty _OutlineMap = null;
        private static MaterialProperty _BakedNormal = null;
        // private static bool _debugFolderFoldOut = false;

        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _Outline_Width, "_Outline_Width");
            ymtoon.FindProp(ref _Outline_Color, "_Outline_Color");
            ymtoon.FindProp(ref _Is_BlendBaseColorWeight, "_Is_BlendBaseColorWeight");
            ymtoon.FindProp(ref _Farthest_Distance, "_Farthest_Distance");
            ymtoon.FindProp(ref _Nearest_Distance, "_Nearest_Distance");
            ymtoon.FindProp(ref _OutlineMap, "_OutlineMap");
            ymtoon.FindProp(ref _BakedNormal, "_BakedNormal");
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;

            UITabColorDic.TryGetValue(UITab.Outline, out Color color);
            DrawFoldOutMenu(ref _Outline_Foldout, "【Outline】", color,
                () =>
                {
                    EditorGUILayout.Space();
                    GUI_Outline(material);
                    // DrawFoldOutSubMenu(ref _debugFolderFoldOut, "Debug", () => DebugDraw(material));
                }
            );
        }

        private static void GUI_Outline(Material material)
        {
            EditorGUILayout.LabelField("Outline Mask : BaseOptMap.g", EditorStyles.boldLabel);

            m_MaterialEditor.RangeProperty(_Outline_Width, "Outline Width");
            DrawFixedSizeProp("Outline Color", m_MaterialEditor, _Outline_Color);

            var isBlendBaseColor = DrawToggleButton(material, "Blend BaseColor to Outline", "_Is_BlendBaseColor");

            if (isBlendBaseColor)
            {
                DrawContentWithIndent(() =>
                {
                    m_MaterialEditor.RangeProperty(_Is_BlendBaseColorWeight, "BlendBaseColorWeight");
                    EditorGUILayout.Space();
                });
            }

            DrawFoldOutSubMenu(ref _AdvancedOutline_Foldout, "Advanced Settings", () =>
            {
                EditorGUILayout.LabelField("Camera Distance for Outline Width");
                DrawContentWithIndent(() =>
                {
                    m_MaterialEditor.FloatProperty(_Farthest_Distance, "Farthest Distance to Vanish");
                    m_MaterialEditor.FloatProperty(_Nearest_Distance, "Nearest Distance to Draw with Outline Width");
                });

                var useOutlineTexture = DrawToggleButton(material, "Use Outline Texture", "_Is_OutlineMap");
                EditorGUILayout.Space();

                if (useOutlineTexture)
                {
                    m_MaterialEditor.TexturePropertySingleLine(Styles.outlineTexText, _OutlineMap);
                    EditorGUILayout.Space();
                }

                var useBakedNormal = DrawToggleButton(material, "Use Baked Normal for Outline", "_Is_BakedNormal");
                EditorGUILayout.Space();

                if (useBakedNormal)
                {
                    m_MaterialEditor.TexturePropertySingleLine(Styles.bakedNormalOutlineText, _BakedNormal);
                    EditorGUILayout.Space();
                }
            });
        }

        new protected static void DebugDraw(Material material)
        {

        }

        new public static void DisableAllDebugDraw(Material material)
        {

        }
    }

}
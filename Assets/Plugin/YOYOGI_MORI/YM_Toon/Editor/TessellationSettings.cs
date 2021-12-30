using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class TessellationSettings : YMT_FeatureBase
    {

        private static bool _Tessellation_Foldout = true;

        private static MaterialProperty _TessEdgeLength = null;
        private static MaterialProperty _TessPhongStrength = null;
        private static MaterialProperty _TessExtrusionAmount = null;
        // private static bool _debugFolderFoldOut = false;

        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _TessEdgeLength, "_TessEdgeLength");
            ymtoon.FindProp(ref _TessPhongStrength, "_TessPhongStrength");
            ymtoon.FindProp(ref _TessExtrusionAmount, "_TessExtrusionAmount");
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;
            if (!material.HasProperty("_TessEdgeLength")) { return; }

            DrawFoldOutMenu(ref _Tessellation_Foldout, "【DX11 Phong Tessellation Settings】", Color.gray,
                () =>
                {
                    EditorGUILayout.Space();
                    GUI_Tessellation(material);
                    // DrawFoldOutSubMenu(ref _debugFolderFoldOut, "Debug", () => DebugDraw(material));
                }
            );
        }

        private static void GUI_Tessellation(Material material)
        {
            EditorGUILayout.LabelField("Technique : DX11 Phong Tessellation", EditorStyles.boldLabel);
            m_MaterialEditor.RangeProperty(_TessEdgeLength, "Edge Length");
            m_MaterialEditor.RangeProperty(_TessPhongStrength, "Phong Strength");
            m_MaterialEditor.RangeProperty(_TessExtrusionAmount, "Extrusion Amount");

            EditorGUILayout.Space();
        }

        new protected static void DebugDraw(Material material)
        {

        }

        new public static void DisableAllDebugDraw(Material material)
        {

        }
    }

}
//Unitychan Toon Shader ver.2.0
//UTS2GUI.cs for UTS2 v.2.0.7.5
//nobuyuki@unity3d.com
//https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
//(C)Unity Technologies Japan/UCL
using UnityEditor;
using UnityEngine;
using System.IO;

namespace YoyogiMori
{
    public class YMToon2GUI : ShaderGUI
    {
        private MaterialProperty[] m_props;
        private Material m_material;
        [HideInInspector] public bool isChanged = false;

        private static string versionTextPath => "Assets/YOYOGI MORI/YM_Toon/Version.txt";
        private static string versionStr = "";
        public void FindProp(ref MaterialProperty targetProp, string propName)
        {
            targetProp = FindProperty(propName, m_props, false);
        }

        private void ShowShaderVersion()
        {
            if (File.Exists(versionTextPath))
            {
                using (StreamReader reader = new StreamReader(versionTextPath))
                {
                    string readStr = null;
                    while ((readStr = reader.ReadLine()) != null)
                    {
                        versionStr = readStr;
                    }
                }
            }
            if (!string.IsNullOrEmpty(versionStr))
            {
                EditorGUILayout.LabelField($"Ver: {versionStr}", EditorStyles.boldLabel);
            }
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            EditorGUIUtility.fieldWidth = 0;
            m_material = materialEditor.target as Material;
            m_props = props;

            EditorGUI.BeginChangeCheck();

            ShowShaderVersion();

            EditorGUILayout.Space();

            BasicShaderSettings.Draw(this, materialEditor);

            EditorGUILayout.Space();

            BaseColorSettings.Draw(this, materialEditor);

            EditorGUILayout.Space();

            NormalMapSettings.Draw(this, materialEditor);

            EditorGUILayout.Space();

            SpecularSettings.Draw(this, materialEditor);

            EditorGUILayout.Space();

            MatCapSettings.Draw(this, materialEditor);

            EditorGUILayout.Space();

            RimLightSettings.Draw(this, materialEditor);

            EditorGUILayout.Space();

            EmissiveSettings.Draw(this, materialEditor);

            EditorGUILayout.Space();

            FakeSubSurfaceScatteringSettings.Draw(this, materialEditor);

            EditorGUILayout.Space();

            OutlineSettings.Draw(this, materialEditor);

            EditorGUILayout.Space();

            AdditionalLightingSettings.Draw(this, materialEditor);

            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.PropertiesChanged();

                materialEditor.RegisterPropertyChangeUndo("Mat Changed");
            }

            if (isChanged)
            {
                Undo.RecordObject(m_material, "Mat param Changed");
                Debug.Log("Mat param Changed");
                isChanged = false;
            }
        }
    }
}

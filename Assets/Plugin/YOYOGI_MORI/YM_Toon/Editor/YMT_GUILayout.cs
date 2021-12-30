using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{
    public static class YMT_GUILayout
    {

        public static GUILayoutOption[] ShortButtonStyle => new GUILayoutOption[] { GUILayout.Width(130) };
        public static GUILayoutOption[] MiddleButtonStyle => new GUILayoutOption[] { GUILayout.Width(130) };

        /// <summary>
        /// タイトルとToggleボタンを作る　
        /// </summary>
        /// <param name="material"></param>
        /// <param name="label"></param>
        /// <param name="prefixLabel"></param>
        /// <param name="shaderParamName"></param>
        /// <returns>shaderParamがEnableかどうか</returns>
        [SerializeField]
        public static bool DrawToggleButton(Material material, string prefixLabel, string shaderParamName, string disableLabel = "Off", string enableLabel = "Active")
        {

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(prefixLabel);
            if (material.GetFloat(shaderParamName) == 0)
            {
                if (GUILayout.Button(disableLabel, ShortButtonStyle))
                {
                    SetFloat(material, shaderParamName, 1);
                }
            }
            else
            {
                if (GUILayout.Button(enableLabel, ShortButtonStyle))
                {
                    SetFloat(material, shaderParamName, 0);
                }
            }
            EditorGUILayout.EndHorizontal();

            return material.GetFloat(shaderParamName) == 1;
        }

        /// <summary>
        /// なにかの処理を実行する用のボタン
        /// </summary>
        /// <param name="prefixLabel"></param>
        /// <param name="OnExecuted"></param>
        public static void DrawExecuteButton(string prefixLabel, Action OnExecuted = null, string executeLabel = "Execute")
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(prefixLabel);
            if (GUILayout.Button(executeLabel, MiddleButtonStyle))
            {
                OnExecuted?.Invoke();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// PropをInspector幅に合わせて変に見きれないように描画するやつ
        /// 主にTexturePropertySingleLineみたいにColor描画するときに使う
        /// </summary>
        /// <param name="label"></param>
        /// <param name="editor"></param>
        /// <param name="prop"></param>
        public static void DrawFixedSizeProp(string label, MaterialEditor editor, MaterialProperty prop, float yOffset = 0.0f)
        {
            var indentLevel = EditorGUI.indentLevel;
            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel = 0;
            var control1stRect = EditorGUILayout.GetControlRect(true, yOffset, EditorStyles.layerMaskField, new GUILayoutOption[0]);
            control1stRect.position = new Vector2(control1stRect.position.x, control1stRect.position.y - 18f + yOffset);
            var leftAligned1stRect = MaterialEditor.GetLeftAlignedFieldRect(control1stRect);
            editor.ShaderProperty(leftAligned1stRect, prop, string.Empty);
            EditorGUI.indentLevel = indentLevel;
        }

        /// <summary>
        /// OnIndentedの中身をindentした状態で書く
        /// </summary>
        /// <param name="OnIndented"></param>
        /// <param name="indentLevel"></param>
        public static void DrawContentWithIndent(Action OnIndented = null, int indentLevel = 1)
        {
            EditorGUI.indentLevel += indentLevel;
            OnIndented?.Invoke();
            EditorGUI.indentLevel -= indentLevel;
        }

        /// <summary>
        /// Shader Keyword の toggle を書く
        /// </summary>
        /// <param name="material"></param>
        /// <param name="label"></param>
        /// <param name="shaderKeyWord"></param>
        public static void DrawKeywordToggle(Material material, string label, string shaderKeyWord)
        {
            bool keywordEnable = material.IsKeywordEnabled(shaderKeyWord);
            keywordEnable = EditorGUILayout.Toggle(label, keywordEnable);
            SetKeyword(material, shaderKeyWord, keywordEnable);
        }

        public static void DrawFoldOutMenu(ref bool foldState, string label, Color bgColor, Action OnGuiFunc = null)
        {
            var originalBgColor = GUI.backgroundColor;
            GUI.backgroundColor = bgColor;
            foldState = Foldout(foldState, label);
            GUI.backgroundColor = originalBgColor;
            if (foldState)
            {
                DrawContentWithIndent(() => OnGuiFunc?.Invoke());
            }
            EditorGUILayout.Space();
        }

        public static bool Foldout(bool display, string title)
        {
            var style = new GUIStyle("ShurikenModuleTitle")
            {
                font = new GUIStyle(EditorStyles.boldLabel).font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = 22,
                contentOffset = new Vector2(20f, -2f)
            };

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }

        public static void DrawFoldOutSubMenu(ref bool foldState, string label, Action OnGuiFunc = null)
        {
            var originalBgColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.gray;
            foldState = FoldoutSubMenu(foldState, label);
            GUI.backgroundColor = originalBgColor;
            if (foldState)
            {
                DrawContentWithIndent(() => OnGuiFunc?.Invoke());
            }
        }

        public static bool FoldoutSubMenu(bool display, string title)
        {
            var style = new GUIStyle("ShurikenModuleTitle")
            {
                font = new GUIStyle(EditorStyles.boldLabel).font,
                border = new RectOffset(5, 7, 4, 4),
                padding = new RectOffset(5, 7, 4, 4),
                fixedHeight = 22,
                contentOffset = new Vector2(32f, -2f)
            };

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            rect.x += 16;
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 16f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }
        private static Color MakeColor(string code)
        {
            ColorUtility.TryParseHtmlString(code, out Color color);
            return color;
        }

        public enum UITab
        {
            Base,
            Normal,
            Specular,
            MatCap,
            RimLight,
            Emissive,
            SSS,
            Outline
        }

        public static readonly Dictionary<UITab, Color> UITabColorDic = new Dictionary<UITab, Color>
        {
            {UITab.Base, MakeColor("#FFA07A")},
            {UITab.Normal, MakeColor("#B0ADFF")},
            {UITab.Specular, MakeColor("#FFEB87")},
            {UITab.MatCap, MakeColor("#B2FF93")},
            {UITab.RimLight, MakeColor("#8BFCE9")},
            {UITab.Emissive, MakeColor("#FFE8B6")},
            {UITab.SSS, MakeColor("#FDB8A2")},
            {UITab.Outline, MakeColor("#555555")},
        };

    }

}

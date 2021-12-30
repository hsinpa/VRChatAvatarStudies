using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class BaseColorSettings : YMT_FeatureBase
    {

        private static bool _BasicThreeColors_Foldout = true;
        private static bool _BaseColor_AdvancedSettings_Foldout = true;

        private static MaterialProperty _MainTex = null;
        private static MaterialProperty _BaseColor = null;
        private static MaterialProperty _1st_ShadeMap = null;
        private static MaterialProperty _1st_ShadeColor = null;
        private static MaterialProperty _2nd_ShadeColor = null;
        private static MaterialProperty _BaseOptMap = null;

        private static MaterialProperty _Tweak_1stShadingGradeMapLevel = null;


        private static MaterialProperty _1st_Shade_Step = null;
        private static MaterialProperty _1st_Shade_Feather = null;
        private static MaterialProperty _2nd_Shade_Step = null;
        private static MaterialProperty _2nd_Shade_Feather = null;
        private static MaterialProperty _Tweak_ReceivedShadowsLevel = null;

        private static string DEBUG_1ST_SHADEMASK => "_DEBUG_1ST_SHADEMASK";
        private static string DEBUG_2ND_SHADEMASK => "_DEBUG_2ND_SHADEMASK";
        private static string DEBUG_1ST_SHADING_GRADEMASK => "_DEBUG_1ST_SHADING_GRADEMASK";
        private static string DEBUG_BASE_COLOR_ONLY => "_DEBUG_BASE_COLOR_ONLY";
        private static string DEBUG_BASE_MAP_ALPHA => "_DEBUG_BASE_MAP_ALPHA";

        private static bool _debugFolderFoldOut = false;

        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _MainTex, "_MainTex");
            ymtoon.FindProp(ref _BaseColor, "_BaseColor");

            ymtoon.FindProp(ref _1st_ShadeMap, "_1st_ShadeMap");
            ymtoon.FindProp(ref _1st_ShadeColor, "_1st_ShadeColor");
            ymtoon.FindProp(ref _2nd_ShadeColor, "_2nd_ShadeColor");

            ymtoon.FindProp(ref _BaseOptMap, "_BaseOptMap");

            ymtoon.FindProp(ref _Tweak_1stShadingGradeMapLevel, "_Tweak_1stShadingGradeMapLevel");

            ymtoon.FindProp(ref _1st_Shade_Step, "_1st_Shade_Step");
            ymtoon.FindProp(ref _1st_Shade_Feather, "_1st_Shade_Feather");
            ymtoon.FindProp(ref _2nd_Shade_Step, "_2nd_Shade_Step");
            ymtoon.FindProp(ref _2nd_Shade_Feather, "_2nd_Shade_Feather");

            ymtoon.FindProp(ref _Tweak_ReceivedShadowsLevel, "_Tweak_ReceivedShadowsLevel");

            BackwardCompatible(ymtoon);
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;

            UITabColorDic.TryGetValue(UITab.Base, out Color color);
            DrawFoldOutMenu(ref _BasicThreeColors_Foldout, "【Base】", color,
                () =>
                {
                    GUI_BaseColors(material);
                    DrawFoldOutSubMenu(ref _debugFolderFoldOut, "Debug", () => DebugDraw(material));
                }
            );
            BackwardFollow();

        }

        private static void GUI_BaseColors(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.baseColorText, _MainTex, _BaseColor);

            EditorGUILayout.Space();

            m_MaterialEditor.TexturePropertyMiniThumbnail(GUILayoutUtility.GetRect(16, 24), _1st_ShadeMap, "Shade Map", "");

            DrawContentWithIndent(() =>
            {
                DrawFixedSizeProp("1st Shade Color", m_MaterialEditor, _1st_ShadeColor);
                DrawFixedSizeProp("2nd Shade Color", m_MaterialEditor, _2nd_ShadeColor);
            });

            EditorGUILayout.Space();

            m_MaterialEditor.TexturePropertySingleLine(Styles.baseOptMapText, _BaseOptMap);

            EditorGUILayout.Space();

            if (_1st_Shade_Step.floatValue < _2nd_Shade_Step.floatValue)
            {
                EditorGUILayout.HelpBox("【Warning】 Please \"1st Shade Step\" is larger than \"2nd Shade Step\"", MessageType.Warning);
            }
            m_MaterialEditor.RangeProperty(_1st_Shade_Step, "1st Shade Step");
            m_MaterialEditor.RangeProperty(_1st_Shade_Feather, "1st Shade Feather");
            m_MaterialEditor.RangeProperty(_2nd_Shade_Step, "2nd Shade Step");
            m_MaterialEditor.RangeProperty(_2nd_Shade_Feather, "2nd Shade Feather");

            EditorGUILayout.Space();
            EditorGUILayout.Space();


            DrawFoldOutSubMenu(ref _BaseColor_AdvancedSettings_Foldout, "Advanced Settings", () =>
            {
                EditorGUILayout.LabelField("Shading Grade Map : ShadeMap.a", EditorStyles.boldLabel);

                DrawContentWithIndent(() =>
                {
                    var use1stShadingMask = DrawToggleButton(material, "Shading Grade Map", "_Use_1stShadeMapAlpha_As_ShadowMask");

                    if (use1stShadingMask)
                    {
                        DrawContentWithIndent(() =>
                        {
                            m_MaterialEditor.ShaderProperty(_Tweak_1stShadingGradeMapLevel, "Tweak 1stShadingGradeMapLevel");

                            EditorGUILayout.Space();
                        });
                    }
                    EditorGUILayout.Space();
                });

                m_MaterialEditor.RangeProperty(_Tweak_ReceivedShadowsLevel, "Received Shadows Level");

                EditorGUILayout.Space();
            });

            EditorGUILayout.Space();
        }

        #region 後方互換 しばらくしたら消す

        private static MaterialProperty _BaseColor_Step = null;
        private static MaterialProperty _BaseShade_Feather = null;
        private static MaterialProperty _ShadeColor_Step = null;
        private static MaterialProperty _1st2nd_Shades_Feather = null;
        private static MaterialProperty _Tweak_SystemShadowsLevel = null;
        private static MaterialProperty _CombinedMask = null;


        private static void BackwardCompatible(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _BaseColor_Step, "_BaseColor_Step");
            ymtoon.FindProp(ref _BaseShade_Feather, "_BaseShade_Feather");

            ymtoon.FindProp(ref _ShadeColor_Step, "_ShadeColor_Step");
            ymtoon.FindProp(ref _1st2nd_Shades_Feather, "_1st2nd_Shades_Feather");

            ymtoon.FindProp(ref _Tweak_SystemShadowsLevel, "_Tweak_SystemShadowsLevel");

            ymtoon.FindProp(ref _CombinedMask, "_CombinedMask");

            SetBackwardCompatibleValue<float>(_BaseColor_Step, _1st_Shade_Step);
            SetBackwardCompatibleValue<float>(_BaseShade_Feather, _1st_Shade_Feather);
            SetBackwardCompatibleValue<float>(_ShadeColor_Step, _2nd_Shade_Step);
            SetBackwardCompatibleValue<float>(_1st2nd_Shades_Feather, _2nd_Shade_Feather);

            SetBackwardCompatibleValue<float>(_Tweak_SystemShadowsLevel, _Tweak_ReceivedShadowsLevel);
            SetBackwardCompatibleValue<Texture>(_CombinedMask, _BaseOptMap);
        }

        private static void BackwardFollow()
        {
            SetBackwardFollowValue<float>(_BaseColor_Step, _1st_Shade_Step);
            SetBackwardFollowValue<float>(_BaseShade_Feather, _1st_Shade_Feather);
            SetBackwardFollowValue<float>(_ShadeColor_Step, _2nd_Shade_Step);
            SetBackwardFollowValue<float>(_1st2nd_Shades_Feather, _2nd_Shade_Feather);
            SetBackwardFollowValue<float>(_Tweak_SystemShadowsLevel, _Tweak_ReceivedShadowsLevel);
            SetBackwardFollowValue<Texture>(_CombinedMask, _BaseOptMap);

        }

        #endregion

        new protected static void DebugDraw(Material material)
        {
            DrawContentWithIndent(() =>
            {
                DrawKeywordToggle(material, "Debug Base Color Only", DEBUG_BASE_COLOR_ONLY);
                DrawKeywordToggle(material, "Debug Base Map Alpha", DEBUG_BASE_MAP_ALPHA);

                DrawKeywordToggle(material, "Debug 1st Shading GradeMask", DEBUG_1ST_SHADING_GRADEMASK);
                DrawKeywordToggle(material, "Debug 1st Shade Mask", DEBUG_1ST_SHADEMASK);
                DrawKeywordToggle(material, "Debug 2nd Shade Mask", DEBUG_2ND_SHADEMASK);
            });
        }

        new public static void DisableAllDebugDraw(Material material)
        {
            SetKeyword(material, DEBUG_BASE_COLOR_ONLY, false);
            SetKeyword(material, DEBUG_BASE_MAP_ALPHA, false);
            SetKeyword(material, DEBUG_1ST_SHADING_GRADEMASK, false);
            SetKeyword(material, DEBUG_1ST_SHADEMASK, false);
            SetKeyword(material, DEBUG_2ND_SHADEMASK, false);
        }
    }

}
using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class SpecularSettings : YMT_FeatureBase
    {

        private static bool _Specular_Foldout = true;
        private static bool _Specular_Advanced_Foldout = false;
        private static string DEBUG_SPECULAR_MASK => "_DEBUG_SPECULAR_MASK";
        private static string DEBUG_SPECULAR_UVSET => "_DEBUG_SPECULAR_UVSET";
        private static string DEBUG_SPECULAR_1ST_OPT_R => "_DEBUG_SPECULAR_1ST_OPT_R";
        private static string DEBUG_SPECULAR_1ST_OPT_G => "_DEBUG_SPECULAR_1ST_OPT_G";
        private static string DEBUG_SPECULAR_1ST_OPT_B => "_DEBUG_SPECULAR_1ST_OPT_B";
        private static string DEBUG_SPECULAR_1ST_OPT_A => "_DEBUG_SPECULAR_1ST_OPT_A";
        private static string DEBUG_SPECULAR_2ND_OPT_R => "_DEBUG_SPECULAR_2ND_OPT_R";
        private static string DEBUG_SPECULAR_2ND_OPT_G => "_DEBUG_SPECULAR_2ND_OPT_G";
        private static string DEBUG_SPECULAR_2ND_OPT_B => "_DEBUG_SPECULAR_2ND_OPT_B";
        private static string DEBUG_SPECULAR_2ND_OPT_A => "_DEBUG_SPECULAR_2ND_OPT_A";
        private static string DEBUG_SPECULAR => "_DEBUG_SPECULAR";

        private static MaterialProperty _SpecularMap = null;
        private static MaterialProperty _Use_2ndUV_As_SpecularMapMask = null;

        private static MaterialProperty _1st_Specular = null;
        private static MaterialProperty _1st_Specular_Power = null;
        private static MaterialProperty _2nd_Specular_Power = null;
        private static MaterialProperty _Tweak_Specular_Feather_Level = null;

        private static MaterialProperty _1st_SpecularMapMaskScaler = null;
        private static MaterialProperty _1st_SpecularMapMaskOffset = null;
        private static MaterialProperty _2nd_SpecularMapMaskScaler = null;
        private static MaterialProperty _2nd_SpecularMapMaskOffset = null;

        private static MaterialProperty _1stAnisoHighLightPower = null;
        private static MaterialProperty _1stAnisoHighLightStrength = null;
        private static MaterialProperty _2ndAnisoHighLightPower = null;
        private static MaterialProperty _2ndAnisoHighLightStrength = null;
        private static MaterialProperty _1st_ShiftTangent = null;
        private static MaterialProperty _2nd_ShiftTangent = null;

        private static MaterialProperty _1st_SpecularOptMap = null;
        private static MaterialProperty _2nd_SpecularOptMap = null;
        private static bool _debugFolderFoldOut = false;

        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _SpecularMap, "_SpecularMap");
            ymtoon.FindProp(ref _1st_ShiftTangent, "_1st_ShiftTangent");
            ymtoon.FindProp(ref _2nd_ShiftTangent, "_2nd_ShiftTangent");
            ymtoon.FindProp(ref _1st_Specular, "_Specular");
            ymtoon.FindProp(ref _Tweak_Specular_Feather_Level, "_Tweak_Specular_Feather_Level");

            ymtoon.FindProp(ref _1st_Specular_Power, "_1st_Specular_Power");
            ymtoon.FindProp(ref _2nd_Specular_Power, "_2nd_Specular_Power");

            ymtoon.FindProp(ref _1st_SpecularMapMaskScaler, "_1st_SpecularMapMaskScaler");
            ymtoon.FindProp(ref _1st_SpecularMapMaskOffset, "_1st_SpecularMapMaskOffset");

            ymtoon.FindProp(ref _2nd_SpecularMapMaskScaler, "_2nd_SpecularMapMaskScaler");
            ymtoon.FindProp(ref _2nd_SpecularMapMaskOffset, "_2nd_SpecularMapMaskOffset");

            ymtoon.FindProp(ref _Use_2ndUV_As_SpecularMapMask, "_Use_2ndUV_As_SpecularMapMask");

            ymtoon.FindProp(ref _1stAnisoHighLightPower, "_1stAnisoHighLightPower");
            ymtoon.FindProp(ref _1stAnisoHighLightStrength, "_1stAnisoHighLightStrength");
            ymtoon.FindProp(ref _2ndAnisoHighLightPower, "_2ndAnisoHighLightPower");
            ymtoon.FindProp(ref _2ndAnisoHighLightStrength, "_2ndAnisoHighLightStrength");


            ymtoon.FindProp(ref _1st_SpecularOptMap, "_1st_SpecularOptMap");
            ymtoon.FindProp(ref _2nd_SpecularOptMap, "_2nd_SpecularOptMap");
            BackwardCompatible(ymtoon);
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;

            UITabColorDic.TryGetValue(UITab.Specular, out Color color);
            DrawFoldOutMenu(ref _Specular_Foldout, "【Specular】", color,
                () =>
                {
                    EditorGUILayout.Space();
                    GUI_Specular(material);
                }
            );
            BackwardFollow();
        }

        private static void GUI_Specular(Material material)
        {
            var useSpecular = DrawToggleButton(material, "Specular", "_Use_Specular");

            EditorGUILayout.Space();

            if (useSpecular)
            {
                DrawContentWithIndent(() =>
                {
                    m_MaterialEditor.TexturePropertySingleLine(Styles.specularText, _SpecularMap, _1st_Specular);
                    // m_MaterialEditor.TextureScaleOffsetProperty(_SpecularMap);

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Specular OptMap R: Noise(can be Tiled), G: Noise Mask, B: Feather", EditorStyles.boldLabel);
                    DrawContentWithIndent(() =>
                    {
                        m_MaterialEditor.TexturePropertySingleLine(Styles.specular1stOptMapText, _1st_SpecularOptMap);
                        m_MaterialEditor.TextureScaleOffsetProperty(_1st_SpecularOptMap);

                        EditorGUILayout.Space();
                    });

                    var useAnisoHighLight = DrawToggleButton(material, "Use Aniso Hair HighLight", "_Use_Aniso_HighLight");

                    EditorGUILayout.Space();

                    if (useAnisoHighLight)
                    {
                        DrawContentWithIndent(() =>
                        {
                            m_MaterialEditor.RangeProperty(_1stAnisoHighLightPower, "1st AnisoHighLight Power");
                            m_MaterialEditor.RangeProperty(_1stAnisoHighLightStrength, "1st AnisoHighLight Strength");
                            m_MaterialEditor.RangeProperty(_1st_ShiftTangent, "1st ShiftTangent");

                            EditorGUILayout.Space();

                            m_MaterialEditor.RangeProperty(_2ndAnisoHighLightPower, "2nd AnisoHighLight Power");
                            m_MaterialEditor.RangeProperty(_2ndAnisoHighLightStrength, "2nd AnisoHighLight Strength");
                            m_MaterialEditor.RangeProperty(_2nd_ShiftTangent, "2nd ShiftTangent");
                        });
                        EditorGUILayout.Space();
                    }
                    else
                    {

                        m_MaterialEditor.RangeProperty(_1st_Specular_Power, "1st Specular Power");

                        EditorGUILayout.Space();

                        var isSpecularMode = DrawToggleButton(material, "Specular Mode", "_Is_SpecularToSpecular");
                        if (isSpecularMode)
                        {
                            DrawContentWithIndent(() =>
                            {
                                m_MaterialEditor.RangeProperty(_Tweak_Specular_Feather_Level, "Tweak Specular Feather Level");
                            });
                        }

                        EditorGUILayout.Space();

                        DrawFoldOutSubMenu(ref _Specular_Advanced_Foldout, "Advanced Settings", () =>
                        {
                            //TODO: ホントに使ってないことが分かったら消す
                            // DrawToggleButton(material, "Use 2ndUV As SpecularMask", "_Use_2ndUV_As_SpecularMapMask");
                            // EditorGUILayout.Space();

                            m_MaterialEditor.RangeProperty(_1st_SpecularMapMaskScaler, "1st SpecularMapMask Scaler");
                            m_MaterialEditor.RangeProperty(_1st_SpecularMapMaskOffset, "1st SpecularMapMask Offset");

                            EditorGUILayout.Space();

                            DrawToggleButton(material, "Mul LightColor To Specular", "_Is_LightColor_Specular");

                            EditorGUILayout.Space();

                            var use2ndOptMap = DrawToggleButton(material, "Use 2nd OptMap", "_Use_2nd_Specular_OptMap");
                            if (use2ndOptMap)
                            {
                                DrawContentWithIndent(() =>
                                {
                                    m_MaterialEditor.TexturePropertySingleLine(Styles.specular2ndOptMapText, _2nd_SpecularOptMap);
                                    m_MaterialEditor.TextureScaleOffsetProperty(_2nd_SpecularOptMap);

                                    EditorGUILayout.Space();

                                    m_MaterialEditor.RangeProperty(_2nd_Specular_Power, "2nd Specular Power");

                                    EditorGUILayout.Space();

                                    m_MaterialEditor.RangeProperty(_2nd_SpecularMapMaskScaler, "2nd SpecularMapMask Scaler");
                                    m_MaterialEditor.RangeProperty(_2nd_SpecularMapMaskOffset, "2nd SpecularMapMask Offset");

                                });
                            }
                            EditorGUILayout.Space();
                        });

                        EditorGUILayout.Space();
                    }
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
        private static MaterialProperty _HighColorMap = null;

        private static MaterialProperty _1st_HighColor = null;
        private static MaterialProperty _1st_HighColor_Power = null;
        private static MaterialProperty _2nd_HighColor_Power = null;
        private static MaterialProperty _Tweak_HighColor_Feather_Level = null;

        private static MaterialProperty _1st_HighColorMapMaskScaler = null;
        private static MaterialProperty _1st_HighColorMapMaskOffset = null;
        private static MaterialProperty _2nd_HighColorMapMaskScaler = null;
        private static MaterialProperty _2nd_HighColorMapMaskOffset = null;

        private static MaterialProperty _1st_HighColorOptMap = null;
        private static MaterialProperty _2nd_HighColorOptMap = null;
        private static MaterialProperty _HighColor_Power = null;
        private static MaterialProperty _HighColorMapMaskScaler = null;
        private static MaterialProperty _HighColorMapMaskOffset = null;
        private static MaterialProperty _HighColorOptMap = null;
        private static MaterialProperty _ShiftTangent = null;

        private static MaterialProperty _AnisotropicHighLightPowerLow = null;
        private static MaterialProperty _AnisotropicHighLightStrengthLow = null;
        private static MaterialProperty _AnisotropicHighLightPowerHigh = null;
        private static MaterialProperty _AnisotropicHighLightStrengthHigh = null;

        private static void BackwardCompatible(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _HighColorMap, "_HighColorMap");
            ymtoon.FindProp(ref _1st_HighColor, "_HighColor");
            ymtoon.FindProp(ref _Tweak_HighColor_Feather_Level, "_Tweak_HighColor_Feather_Level");

            ymtoon.FindProp(ref _1st_HighColor_Power, "_1st_HighColor_Power");
            ymtoon.FindProp(ref _2nd_HighColor_Power, "_2nd_HighColor_Power");

            ymtoon.FindProp(ref _1st_HighColorMapMaskScaler, "_1st_HighColorMapMaskScaler");
            ymtoon.FindProp(ref _1st_HighColorMapMaskOffset, "_1st_HighColorMapMaskOffset");

            ymtoon.FindProp(ref _2nd_HighColorMapMaskScaler, "_2nd_HighColorMapMaskScaler");
            ymtoon.FindProp(ref _2nd_HighColorMapMaskOffset, "_2nd_HighColorMapMaskOffset");



            ymtoon.FindProp(ref _1st_HighColorOptMap, "_1st_HighColorOptMap");
            ymtoon.FindProp(ref _2nd_HighColorOptMap, "_2nd_HighColorOptMap");
            ymtoon.FindProp(ref _HighColor_Power, "_HighColor_Power");
            ymtoon.FindProp(ref _HighColorMapMaskScaler, "_HighColorMapMaskScaler");
            ymtoon.FindProp(ref _HighColorMapMaskOffset, "_HighColorMapMaskOffset");
            ymtoon.FindProp(ref _HighColorOptMap, "_HighColorOptMap");
            ymtoon.FindProp(ref _AnisotropicHighLightPowerLow, "_AnisotropicHighLightPowerLow");
            ymtoon.FindProp(ref _AnisotropicHighLightStrengthLow, "_AnisotropicHighLightStrengthLow");
            ymtoon.FindProp(ref _AnisotropicHighLightPowerHigh, "_AnisotropicHighLightPowerHigh");
            ymtoon.FindProp(ref _AnisotropicHighLightStrengthHigh, "_AnisotropicHighLightStrengthHigh");
            ymtoon.FindProp(ref _ShiftTangent, "_ShiftTangent");

            SetBackwardCompatibleValue<Texture>(_HighColorMap, _SpecularMap);
            SetBackwardCompatibleValue<Color>(_1st_HighColor, _1st_Specular);
            SetBackwardCompatibleValue<float>(_Tweak_HighColor_Feather_Level, _Tweak_Specular_Feather_Level);

            // SetBackwardCompatibleValue<float>(_HighColor_Power, _1st_Specular_Power);
            SetBackwardCompatibleValue<float>(_1st_HighColor_Power, _1st_Specular_Power);

            SetBackwardCompatibleValue<float>(_2nd_HighColor_Power, _2nd_Specular_Power);

            SetBackwardCompatibleValue<float>(_HighColorMapMaskScaler, _1st_SpecularMapMaskScaler);
            SetBackwardCompatibleValue<float>(_1st_HighColorMapMaskScaler, _1st_SpecularMapMaskScaler);

            SetBackwardCompatibleValue<float>(_HighColorMapMaskOffset, _1st_SpecularMapMaskOffset);
            SetBackwardCompatibleValue<float>(_1st_HighColorMapMaskOffset, _1st_SpecularMapMaskOffset);

            SetBackwardCompatibleValue<Texture>(_HighColorOptMap, _1st_HighColorOptMap);
            SetBackwardCompatibleValue<Vector4>(_HighColorOptMap, _1st_HighColorOptMap);

            SetBackwardCompatibleValue<float>(_ShiftTangent, _2nd_ShiftTangent);

            SetBackwardCompatibleValue<float>(_2nd_HighColorMapMaskScaler, _2nd_SpecularMapMaskScaler);
            SetBackwardCompatibleValue<float>(_2nd_HighColorMapMaskOffset, _2nd_SpecularMapMaskOffset);

        }

        private static void BackwardFollow()
        {
            SetBackwardFollowValue<Texture>(_HighColorMap, _SpecularMap);
            SetBackwardFollowValue<Color>(_1st_HighColor, _1st_Specular);
            SetBackwardFollowValue<float>(_Tweak_HighColor_Feather_Level, _Tweak_Specular_Feather_Level);

            SetBackwardFollowValue<float>(_HighColor_Power, _1st_HighColor_Power);
            SetBackwardFollowValue<float>(_1st_HighColor_Power, _1st_Specular_Power);

            SetBackwardFollowValue<float>(_2nd_HighColor_Power, _2nd_Specular_Power);

            SetBackwardFollowValue<float>(_HighColorMapMaskScaler, _1st_SpecularMapMaskScaler);
            SetBackwardFollowValue<float>(_1st_HighColorMapMaskScaler, _1st_SpecularMapMaskScaler);

            SetBackwardFollowValue<float>(_HighColorMapMaskOffset, _1st_SpecularMapMaskOffset);
            SetBackwardFollowValue<float>(_1st_HighColorMapMaskOffset, _1st_SpecularMapMaskOffset);

            SetBackwardFollowValue<Texture>(_HighColorOptMap, _1st_HighColorOptMap);
            SetBackwardFollowValue<Vector4>(_HighColorOptMap, _1st_HighColorOptMap);

            SetBackwardFollowValue<float>(_ShiftTangent, _2nd_ShiftTangent);

            SetBackwardFollowValue<float>(_2nd_HighColorMapMaskScaler, _2nd_SpecularMapMaskScaler);
            SetBackwardFollowValue<float>(_2nd_HighColorMapMaskOffset, _2nd_SpecularMapMaskOffset);

        }
        #endregion

        new protected static void DebugDraw(Material material)
        {
            DrawContentWithIndent(() =>
            {
                DrawKeywordToggle(material, "Debug Specular UVSet", DEBUG_SPECULAR_UVSET);
                DrawKeywordToggle(material, "Debug Specular 1st Opt Map r", DEBUG_SPECULAR_1ST_OPT_R);
                DrawKeywordToggle(material, "Debug Specular 1st Opt Map g", DEBUG_SPECULAR_1ST_OPT_G);
                DrawKeywordToggle(material, "Debug Specular 1st Opt Map b", DEBUG_SPECULAR_1ST_OPT_B);
                DrawKeywordToggle(material, "Debug Specular 1st Opt Map a", DEBUG_SPECULAR_1ST_OPT_A);
                DrawKeywordToggle(material, "Debug Specular 2nd Opt Map r", DEBUG_SPECULAR_2ND_OPT_R);
                DrawKeywordToggle(material, "Debug Specular 2nd Opt Map g", DEBUG_SPECULAR_2ND_OPT_G);
                DrawKeywordToggle(material, "Debug Specular 2nd Opt Map b", DEBUG_SPECULAR_2ND_OPT_B);
                DrawKeywordToggle(material, "Debug Specular 2nd Opt Map a", DEBUG_SPECULAR_2ND_OPT_A);
                DrawKeywordToggle(material, "Debug Specular Mask", DEBUG_SPECULAR_MASK);
                DrawKeywordToggle(material, "Debug Specular", DEBUG_SPECULAR);
            });
        }

        new public static void DisableAllDebugDraw(Material material)
        {
            SetKeyword(material, DEBUG_SPECULAR_UVSET, false);
            SetKeyword(material, DEBUG_SPECULAR_1ST_OPT_R, false);
            SetKeyword(material, DEBUG_SPECULAR_1ST_OPT_G, false);
            SetKeyword(material, DEBUG_SPECULAR_1ST_OPT_B, false);
            SetKeyword(material, DEBUG_SPECULAR_1ST_OPT_A, false);
            SetKeyword(material, DEBUG_SPECULAR_2ND_OPT_R, false);
            SetKeyword(material, DEBUG_SPECULAR_2ND_OPT_G, false);
            SetKeyword(material, DEBUG_SPECULAR_2ND_OPT_B, false);
            SetKeyword(material, DEBUG_SPECULAR_2ND_OPT_A, false);
            SetKeyword(material, DEBUG_SPECULAR_MASK, false);
            SetKeyword(material, DEBUG_SPECULAR, false);

        }
    }

}
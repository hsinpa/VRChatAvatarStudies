using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori
{

    public class MatCapSettings : YMT_FeatureBase
    {
        private static bool _MatCap_Foldout = true;
        private static bool _MatCap_Advanced_Foldout = false;
        private static string DEBUG_IS_MIRROR => "_DEBUG_IS_MIRROR";
        private static string DEBUG_MATCAP_SAMPLER => "_DEBUG_MATCAP_SAMPLER";
        private static string DEBUG_MATCAP_MASK => "_DEBUG_MATCAP_MASK";
        private static string DEBUG_MATCAP => "_DEBUG_MATCAP";

        private static MaterialProperty _MatCap = null;
        private static MaterialProperty _MatCap_Sampler = null;
        private static MaterialProperty _BlurLevelMatCap = null;
        private static MaterialProperty _MatCapColor = null;
        private static MaterialProperty _Is_LightColor_MatCap = null;
        private static MaterialProperty _Is_UseTweakMatCapOnShadow = null;
        private static MaterialProperty _TweakMatCapOnShadow = null;
        private static MaterialProperty _MatCapMask = null;
        private static MaterialProperty _Tweak_MatCapMaskLevel = null;
        private static MaterialProperty _Is_Ortho = null;
        private static bool _debugFolderFoldOut = false;

        new protected static void FindProps(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _MatCap, "_MatCap");
            ymtoon.FindProp(ref _MatCap_Sampler, "_MatCap_Sampler");
            ymtoon.FindProp(ref _BlurLevelMatCap, "_BlurLevelMatCap");
            ymtoon.FindProp(ref _MatCapColor, "_MatCapColor");
            ymtoon.FindProp(ref _Is_LightColor_MatCap, "_Is_LightColor_MatCap");
            ymtoon.FindProp(ref _Is_UseTweakMatCapOnShadow, "_Is_UseTweakMatCapOnShadow");
            ymtoon.FindProp(ref _TweakMatCapOnShadow, "_TweakMatCapOnShadow");
            ymtoon.FindProp(ref _MatCapMask, "_MatCapMask");
            ymtoon.FindProp(ref _Tweak_MatCapMaskLevel, "_Tweak_MatCapMaskLevel");
            ymtoon.FindProp(ref _Is_Ortho, "_Is_Ortho");
            BackwardCompatible(ymtoon);
        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor)
        {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;

            UITabColorDic.TryGetValue(UITab.MatCap, out Color color);
            DrawFoldOutMenu(ref _MatCap_Foldout, "【MatCap】", color,
                () =>
                {
                    EditorGUILayout.Space();
                    GUI_Emissive(material);
                }
            );
            BackwardFollow();
        }

        private static void GUI_Emissive(Material material)
        {
            var useMatCap = DrawToggleButton(material, "MatCap", "_MatCap");

            EditorGUILayout.Space();

            if (useMatCap)
            {
                DrawContentWithIndent(() =>
                {
                    m_MaterialEditor.TexturePropertySingleLine(Styles.matCapSamplerText, _MatCap_Sampler, _MatCapColor);
                    // m_MaterialEditor.TextureScaleOffsetProperty(_MatCap_Sampler);

                    m_MaterialEditor.RangeProperty(_TweakMatCapOnShadow, "MatCap Power on Shadow");

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("MatCap Mask", EditorStyles.boldLabel);
                    DrawContentWithIndent(() =>
                    {
                        m_MaterialEditor.TexturePropertySingleLine(Styles.matCapMaskText, _MatCapMask);
                        // m_MaterialEditor.TextureScaleOffsetProperty(_MatCapMask);
                        m_MaterialEditor.RangeProperty(_Tweak_MatCapMaskLevel, "MatCap Mask Level");
                    });
                    EditorGUILayout.Space();
                });
                DrawFoldOutSubMenu(ref _MatCap_Advanced_Foldout, "Advanced Settings", () =>
                {
                    m_MaterialEditor.RangeProperty(_BlurLevelMatCap, "Blur Level of MatCap Sampler");
                    EditorGUILayout.Space();
                    DrawToggleButton(material, "MatCap Projection Camera", "_Is_Ortho", "Orthographic", "Perspective");
                });

                EditorGUILayout.Space();

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

        private static MaterialProperty _Set_MatcapMask = null;
        private static MaterialProperty _Tweak_MatcapMaskLevel = null;


        private static void BackwardCompatible(YMToon2GUI ymtoon)
        {
            ymtoon.FindProp(ref _Set_MatcapMask, "_Set_MatcapMask");
            ymtoon.FindProp(ref _Tweak_MatcapMaskLevel, "_Tweak_MatcapMaskLevel");

            SetBackwardCompatibleValue<Texture>(_Set_MatcapMask, _MatCapMask);
            SetBackwardCompatibleValue<float>(_Tweak_MatcapMaskLevel, _Tweak_MatCapMaskLevel);
        }

        private static void BackwardFollow()
        {
            SetBackwardFollowValue<Texture>(_Set_MatcapMask, _MatCapMask);
            SetBackwardFollowValue<float>(_Tweak_MatcapMaskLevel, _Tweak_MatCapMaskLevel);
        }

        #endregion

        new protected static void DebugDraw(Material material)
        {
            DrawContentWithIndent(() =>
            {
                DrawKeywordToggle(material, "Debug is Mirror", DEBUG_IS_MIRROR);
                DrawKeywordToggle(material, "Debug MatCap Sampler", DEBUG_MATCAP_SAMPLER);
                DrawKeywordToggle(material, "Debug MatCap Mask", DEBUG_MATCAP_MASK);
                DrawKeywordToggle(material, "Debug MatCap", DEBUG_MATCAP);
            });
        }

        new public static void DisableAllDebugDraw(Material material)
        {
            SetKeyword(material, DEBUG_IS_MIRROR, false);
            SetKeyword(material, DEBUG_MATCAP_SAMPLER, false);
            SetKeyword(material, DEBUG_MATCAP_MASK, false);
            SetKeyword(material, DEBUG_MATCAP, false);
        }
    }

}
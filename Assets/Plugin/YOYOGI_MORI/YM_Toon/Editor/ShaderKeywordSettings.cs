using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static YoyogiMori.YMT_GUILayout;
using static YoyogiMori.YMT_SetParamUtils;

namespace YoyogiMori {
    public class ShaderKeywordSettings : YMT_FeatureBase {

        public static HashSet<string> keywordBlacklist = new HashSet<string>(new string[] {
            // Unity Keywords, these don't matter at all. (They should be loaded)
            // All Keywords that are in Standard Unity Shaders
            "_ALPHABLEND_ON",
            "_ALPHAMODULATE_ON",
            "_ALPHAPREMULTIPLY_ON",
            "_ALPHATEST_ON",
            "_COLORADDSUBDIFF_ON",
            "_COLORCOLOR_ON",
            "_COLOROVERLAY_ON",
            "_DETAIL_MULX2",
            "_EMISSION",
            "_FADING_ON",
            "_GLOSSYREFLECTIONS_OFF",
            "_GLOSSYREFLECTIONS_OFF",
            "_MAPPING_6_FRAMES_LAYOUT",
            "_METALLICGLOSSMAP",
            "_NORMALMAP",
            "_PARALLAXMAP",
            "_REQUIRE_UV2",
            "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A",
            "_SPECGLOSSMAP",
            "_SPECULARHIGHLIGHTS_OFF",
            "_SPECULARHIGHLIGHTS_OFF",
            "_SUNDISK_HIGH_QUALITY",
            "_SUNDISK_NONE",
            "_SUNDISK_SIMPLE",
            "_TERRAIN_NORMAL_MAP",
            "BILLBOARD_FACE_CAMERA_POS",
            "EFFECT_BUMP",
            "EFFECT_HUE_VARIATION",
            "ETC1_EXTERNAL_ALPHA",
            "GEOM_TYPE_BRANCH",
            "GEOM_TYPE_BRANCH_DETAIL",
            "GEOM_TYPE_FROND",
            "GEOM_TYPE_LEAF",
            "GEOM_TYPE_MESH",
            "LOD_FADE_CROSSFADE",
            "PIXELSNAP_ON",
            "SOFTPARTICLES_ON",
            "STEREO_INSTANCING_ON",
            "STEREO_MULTIVIEW_ON",
            "UNITY_HDR_ON",
            "UNITY_SINGLE_PASS_STEREO",
            "UNITY_UI_ALPHACLIP",
            "UNITY_UI_CLIP_RECT",
            // Post Processing Stack V1 and V2
            // This is mostly just safe keeping somewhere
            "FOG_OFF",
            "FOG_LINEAR",
            "FOG_EXP",
            "FOG_EXP2",
            "ANTI_FLICKER",
            "UNITY_COLORSPACE_GAMMA",
            "SOURCE_GBUFFER",
            "AUTO_KEY_VALUE",
            "GRAIN",
            "DITHERING",
            "TONEMAPPING_NEUTRAL",
            "TONEMAPPING_FILMIC",
            "CHROMATIC_ABERRATION",
            "DEPTH_OF_FIELD",
            "DEPTH_OF_FIELD_COC_VIEW",
            "BLOOM",
            "BLOOM_LENS_DIRT",
            "COLOR_GRADING",
            "COLOR_GRADING_LOG_VIEW",
            "USER_LUT",
            "VIGNETTE_CLASSIC",
            "VIGNETTE_MASKED",
            "FXAA",
            "FXAA_LOW",
            "FXAA_KEEP_ALPHA",
            "STEREO_INSTANCING_ENABLED",
            "STEREO_DOUBLEWIDE_TARGET",
            "TONEMAPPING_ACES",
            "TONEMAPPING_CUSTOM",
            "APPLY_FORWARD_FOG",
            "DISTORT",
            "CHROMATIC_ABERRATION_LOW",
            "BLOOM_LOW",
            "VIGNETTE",
            "FINALPASS",
            "COLOR_GRADING_HDR_3D",
            "COLOR_GRADING_HDR",
            "AUTO_EXPOSURE"
        });
        new protected static void FindProps(YMToon2GUI ymtoon) {

        }

        new public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor) {
            FindProps(ymtoon);
            m_MaterialEditor = materialEditor;
            var material = m_MaterialEditor.target as Material;

            //VRCのshader keyword utility 相当を強制的にやる
            //別にsafetyでもないが安全そうなほうが押してくれそう
            DrawExecuteButton("Remove ShaderKeyword safety", () => {
                GUI_ShaderKeywordSettings(material);
            });

            EditorGUILayout.Space();

        }
        private static void GUI_ShaderKeywordSettings(Material material) {
            foreach (string keyword in material.shaderKeywords) {
                if (!keywordBlacklist.Contains(keyword)) {
                    SetKeyword(material, keyword, false);
                    Debug.Log("remove: " + keyword);
                }
            }
        }

        new protected static void DebugDraw(Material material) {

        }

        new public static void DisableAllDebugDraw(Material material) {

        }
    }

}
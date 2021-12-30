#ifndef YMTOON_PARAMS
#define YMTOON_PARAMS
//TODO: Standard に引っ張られてリネームした _MainTex や _BumpMapは治す

uniform float4 _BaseColor;

uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
uniform sampler2D _1st_ShadeMap; uniform float4 _1st_ShadeMap_ST;
uniform float4 _1st_ShadeColor;
uniform float _Use_1stShadeMapAlpha_As_ShadowMask;
uniform float _Tweak_1stShadingGradeMapLevel;

uniform float4 _2nd_ShadeColor;
uniform fixed _Is_LightColor_2nd_Shade;

uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
uniform half _BumpScale;
uniform fixed _Is_NormalMapToBase;
uniform float _Tweak_ReceivedShadowsLevel;

uniform float _1st_Shade_Step;
uniform float _1st_Shade_Feather;
uniform float _2nd_Shade_Step;
uniform float _2nd_Shade_Feather;

uniform fixed _Use_Specular;
uniform float4 _Specular;
uniform sampler2D _SpecularMap; uniform float4 _SpecularMap_ST;
uniform sampler2D _1st_SpecularOptMap; uniform float4 _1st_SpecularOptMap_ST;
uniform sampler2D _2nd_SpecularOptMap; uniform float4 _2nd_SpecularOptMap_ST;
uniform float _Use_2ndUV_As_SpecularMapMask;
uniform float _Use_2nd_Specular_OptMap;

uniform fixed _Is_LightColor_Specular;
uniform float _1st_Specular_Power;
uniform float _2nd_Specular_Power;
uniform float _Tweak_Specular_Feather_Level;
uniform fixed _Is_SpecularToSpecular;
uniform fixed _Is_BlendAddToHiColor; // deprecated
uniform fixed _Is_UseTweakSpecularOnShadow;
uniform float _TweakSpecularOnShadow;

uniform float _1st_SpecularMapMaskScaler;
uniform float _1st_SpecularMapMaskOffset;
uniform float _2nd_SpecularMapMaskScaler;
uniform float _2nd_SpecularMapMaskOffset;

uniform sampler2D _BaseOptMap; uniform float4 _BaseOptMap_ST;

uniform fixed _RimLight;
uniform float4 _1st_RimLightColor;
uniform float4 _2nd_RimLightColor;
uniform fixed _Is_LightColor_RimLight;
uniform float _1st_RimLight_Power;
uniform float _2nd_RimLight_Power;
uniform float _1st_RimLight_InsideMask;
uniform float _2nd_RimLight_InsideMask;
uniform fixed _1st_RimLight_Feather;
uniform fixed _2nd_RimLight_Feather;
uniform fixed _Is_1st_RimLight_Addtive;
uniform fixed _Is_2nd_RimLight_Addtive;
uniform fixed _LightDirection_MaskOn;
uniform float _Tweak_LightDirection_MaskLevel;
uniform fixed _Add_Antipodean_RimLight;
uniform float4 _Ap_RimLightColor;
uniform fixed _Is_LightColor_Ap_RimLight;
uniform float _Ap_RimLight_Power;
uniform fixed _Ap_RimLight_Feather;

uniform float _Tweak_1st_RimLightMaskLevel;
uniform float _Tweak_2nd_RimLightMaskLevel;

uniform fixed _MatCap;
uniform sampler2D _MatCap_Sampler; uniform float4 _MatCap_Sampler_ST;
uniform float4 _MatCapColor;
uniform fixed _Is_LightColor_MatCap;
uniform float _TweakMatCapOnShadow;
uniform sampler2D _MatCapMask; uniform float4 _MatCapMask_ST;
uniform float _Tweak_MatCapMaskLevel;
uniform fixed _Is_Ortho;
uniform fixed _BlurLevelMatCap;

uniform fixed _Use_Emissive;
uniform sampler2D _EmissionMap; uniform float4 _EmissionMap_ST;
uniform float4 _Emissive_Color;

uniform fixed _Is_ViewCoord_Scroll;
uniform float _Rotate_EmissiveUV;
uniform float _Base_Speed;
uniform float _Scroll_EmissiveU;
uniform float _Scroll_EmissiveV;
uniform fixed _Is_PingPong_Base;
uniform float4 _ColorShift;
uniform float4 _ViewShift;
uniform float _ColorShift_Speed;
uniform fixed _Is_ColorShift;
uniform fixed _Is_ViewShift;
uniform float3 emissive;

uniform fixed _Is_Filter_HiCutPointLightColor;

#ifdef _ALPHATEST_ON
    uniform float _Clipping_Level;
    uniform fixed _Inverse_Clipping;

#elif _ALPHAPREMULTIPLY_ON
    uniform float _Tweak_transparency;
#endif

uniform fixed _Use_SSS;
uniform float _Use_As_Skin;
uniform float4 _SkinSSSMulColor;
uniform float4 _HSVOffset;
uniform sampler2D _GrabPassTexture;
uniform half _FakeTransparentWeight;

uniform float _Use_OwnDirLight;
uniform float4 _OwnDirectionalLightDir;
uniform float4 _OwnDirectionalLightColor;

uniform float _1stAnisoHighLightPower;
uniform float _1stAnisoHighLightStrength;
uniform float _2ndAnisoHighLightPower;
uniform float _2ndAnisoHighLightStrength;
uniform float _1st_ShiftTangent;
uniform float _2nd_ShiftTangent;

uniform float _Use_Aniso_HighLight;

uniform sampler2D _VAT_PositionMap; uniform float4 _VAT_PositionMap_ST;
uniform sampler2D _VAT_RotationMap; uniform float4 _VAT_RotationMap_ST;
uniform sampler2D _VAT_NormalMap; uniform float4 _VAT_NormalMap_ST;

uniform int _VAT_numOfFrames;
uniform float _VAT_packedNormal;

uniform float _VAT_paddedX;
uniform float _VAT_paddedY;
uniform float _VAT_pivMax;
uniform float _VAT_pivMin;
uniform float _VAT_posMax;
uniform float _VAT_posMin;

uniform float _VAT_speed;
//--

struct YMT_VertexInput
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 texcoord0 : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
    float isCullBack : TEXCOORD2;
    float4 vertexColor : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct YMT_VertexOutput
{
    float4 pos : SV_POSITION;
    float4 uv0 : TEXCOORD0; //xy uv0 , zw uv1
    float4 posWorld : TEXCOORD2;  //xyz:posworld // w: isBackFace
    float4 tangentToWorldAndPackedData[3] : TEXCOORD3;
    float4 opt : TEXCOORD6; // x : mirror flag
    UNITY_SHADOW_COORDS(8)
    UNITY_VERTEX_INPUT_INSTANCE_ID
    // float4 grabPos  : TEXCOORD7;

};

#endif

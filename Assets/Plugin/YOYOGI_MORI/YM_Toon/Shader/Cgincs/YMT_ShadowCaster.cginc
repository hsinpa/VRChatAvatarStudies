//UCTS_ShadowCaster.cginc
//Unitychan Toon Shader ver.2.0
//v.2.0.7.5
//nobuyuki@unity3d.com
//https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
//(C)Unity Technologies Japan/UCL
//#pragma multi_compile _IS_CLIPPING_OFF _ALPHATEST_ON  _IS_CLIPPING_TRANSMODE
//

#ifndef UNITY_STANDARD_SHADOW_INCLUDED
#define UNITY_STANDARD_SHADOW_INCLUDED

#include "YMT_Params.cginc"

#include "UnityCG.cginc"

#if defined(SHADER_TARGET_SURFACE_ANALYSIS) || (SHADER_TARGET < 30) || defined(SHADER_API_GLES)
    #undef UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS
#endif

#if (defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)) && defined(UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS)
    #define UNITY_STANDARD_USE_DITHER_MASK 1
#endif

#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
    #define UNITY_STANDARD_USE_SHADOW_UVS 1
#endif

#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
    #define UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT 1
#endif

#ifdef UNITY_STEREO_INSTANCING_ENABLED
    #define UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT 1
#endif


#ifdef UNITY_STANDARD_USE_DITHER_MASK
    sampler3D _DitherMaskLOD;
#endif

struct VertexInput
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 texcoord0 : TEXCOORD0;
    float2 texcoord1 : TEXCOORD1;
    float4 vertexColor : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput
{
    float4 pos : SV_POSITION;
    float2 uv0 : TEXCOORD1;
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        float2 tex : TEXCOORD2;
    #endif
    UNITY_VERTEX_OUTPUT_STEREO
};


VertexOutput vert(VertexInput v)
{
    VertexOutput o = (VertexOutput)0;
    o.uv0 = v.texcoord0;

    //TRANSFER_SHADOW_CASTER_NOPOS
    //NOTE: 下のを使うと計算がおかしいっぽいのでとりあえずUnityObjectToClipPosを使う
    // o.pos = UnityClipSpaceShadowCasterPos(vatComverted.vertex, vatComverted.normal);
    o.pos = UnityObjectToClipPos(v.vertex);
    o.pos = UnityApplyLinearShadowBias(o.pos);

    UNITY_SETUP_INSTANCE_ID(v);
    #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    #endif

    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        o.tex = TRANSFORM_TEX(v.texcoord0, _MainTex);
    #endif

    return o;
}


float4 frag(VertexOutput i) : SV_TARGET
{
    #ifdef _ALPHATEST_ON
        //_Clipping
        float2 Set_UV0 = i.uv0;
        float4 _MainTex_var = tex2D(_MainTex, TRANSFORM_TEX(Set_UV0, _MainTex));
        clip(_MainTex_var.a + _Clipping_Level - 0.5);

        // #elif defined(_ALPHATEST_ON) || (defined(_ALPHATEST_ON) && defined (_ALPHAPREMULTIPLY_ON))
    #elif _ALPHAPREMULTIPLY_ON
        //_TransClipping
        float2 Set_UV0 = i.uv0;
        float4 _MainTex_var = tex2D(_MainTex, TRANSFORM_TEX(Set_UV0, _MainTex));
        float Set_Clipping = saturate((_MainTex_var.a));
        clip(Set_Clipping - 0.5);

    #else

    #endif


    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        #if defined(_ALPHAPREMULTIPLY_ON)
            #if defined(UNITY_STANDARD_USE_DITHER_MASK)
                // Use dither mask for alpha blended shadows, based on pixel position xy
                // and alpha level. Our dither texture is 4x4x16.
                half tranparencyOffset = saturate(_Tweak_transparency + 1.0);
                half alphaRef = tex3D(_DitherMaskLOD, float3(i.pos.xy * 0.25, Set_Clipping * 0.9375 * tranparencyOffset)).a;
                clip(alphaRef - 0.01);
            #endif
        #endif
    #endif

    SHADOW_CASTER_FRAGMENT(i)
}

#endif

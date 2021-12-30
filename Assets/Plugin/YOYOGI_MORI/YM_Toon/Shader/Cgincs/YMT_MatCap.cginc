#ifndef YMTOON_MATCAP
#define YMTOON_MATCAP

#include "YMT_Params.cginc"
#include "YMT_Utils.cginc"

struct YMT_MatCap
{
    float3 calcedNormal;
    float3 viewDirection;
    float3 Set_LightColor;
    float Set_FinalShadowMask;
    float3 Set_RimLight;
    float3 _RimLight_var;
};

inline float3 CalcMatCap(YMT_VertexOutput i, YMT_MatCap p)
{
    float2 Set_UV0 = i.uv0.xy;

    //v.2.0.6 : CameraRolling Stabilizer
    //鏡スクリプト判定：_sign_Mirror = -1 なら、鏡の中と判定.
    //v.2.0.7
    fixed _sign_Mirror = i.opt.x;
    #ifdef _DEBUG_IS_MIRROR
        return float4(_sign_Mirror, _sign_Mirror, _sign_Mirror, 1);
    #endif

    float3 viewNormal = (mul(UNITY_MATRIX_V, float4(p.calcedNormal, 0))).rgb;

    float3 NormalBlend_MatCapUV_Detail = viewNormal.rgb * float3(-1, -1, 1);
    float3 NormalBlend_MatCapUV_Base = (mul(UNITY_MATRIX_V, float4(p.viewDirection, 0)).rgb * float3(-1, -1, 1)) + float3(0, 0, 1);
    float3 noSknewViewNormal = NormalBlend_MatCapUV_Base * dot(NormalBlend_MatCapUV_Base, NormalBlend_MatCapUV_Detail) / NormalBlend_MatCapUV_Base.b - NormalBlend_MatCapUV_Detail;

    float2 _ViewNormalAsMatCapUV = (lerp(noSknewViewNormal, viewNormal, _Is_Ortho).rg * 0.5) + 0.5;

    //鏡の中ならUV左右反転.
    if (_sign_Mirror < 0)
    {
        _ViewNormalAsMatCapUV.x = 1 - _ViewNormalAsMatCapUV.x;
    }
    else
    {
        _ViewNormalAsMatCapUV = _ViewNormalAsMatCapUV;
    }

    //v.2.0.6 : LOD of MatCap
    float4 _MatCap_Sampler_var = tex2Dlod(_MatCap_Sampler, float4(TRANSFORM_TEX(_ViewNormalAsMatCapUV, _MatCap_Sampler), 0.0, _BlurLevelMatCap));

    #ifdef _DEBUG_MATCAP_SAMPLER
        return _MatCap_Sampler_var;
    #endif

    //MatCapMask
    float4 _Set_MatCapMask_var = tex2D(_MatCapMask, TRANSFORM_TEX(Set_UV0, _MatCapMask));

    float _Tweak_MatCapMaskLevel_var = saturate(_Set_MatCapMask_var.g + _Tweak_MatCapMaskLevel);

    #ifdef _DEBUG_MATCAP_MASK
        return float4(_Tweak_MatCapMaskLevel_var, _Tweak_MatCapMaskLevel_var, _Tweak_MatCapMaskLevel_var, 1);
    #endif

    float3 _Is_LightColor_MatCap_var = lerp((_MatCap_Sampler_var.rgb * _MatCapColor.rgb),
    ((_MatCap_Sampler_var.rgb * _MatCapColor.rgb) * p.Set_LightColor), _Is_LightColor_MatCap);

    //v.2.0.6 : ShadowMask on MatCap in Blend mode : multiply
    float3 Set_MatCap = (_Is_LightColor_MatCap_var * ((1.0 - p.Set_FinalShadowMask) + (p.Set_FinalShadowMask * _TweakMatCapOnShadow)));

    #ifdef _DEBUG_MATCAP
        return float4(Set_MatCap * _Tweak_MatCapMaskLevel_var, 1);
    #endif

    //Composition: RimLight and MatCap as finalColor
    //Broke down finalColor composition
    float3 matCapColorOnAddMode = p._RimLight_var + Set_MatCap * _Tweak_MatCapMaskLevel_var;
    return matCapColorOnAddMode;
}

#endif

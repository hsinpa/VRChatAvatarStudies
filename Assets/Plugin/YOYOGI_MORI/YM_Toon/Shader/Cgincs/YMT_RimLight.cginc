#ifndef YMTOON_RIMLIGHT
#define YMTOON_RIMLIGHT

#include "YMT_Params.cginc"

struct YMT_RimLightInput
{
    float3 calcedNormal;
    float3 viewDirection;
    float3 lightDirection;
    float3 Set_LightColor;
    float rimLightMask;
};

struct YMT_RimLightOutput
{
    float3 Set_1st_RimLight;
    float _1st_LightDirection_MaskOn_var;
    float3 Set_2nd_RimLight;
    float _2nd_LightDirection_MaskOn_var;
    float3 Set_APRimLight;
    float _APLightDirection_MaskOn_var;
};

struct YMT_RimLightResult
{
    float3 Set_RimLight;
    float _LightDirection_MaskOn_var;
};

//TODO: ApRimLightの扱いがまとまり次第検討する
//sample
// YMT_RimLightResult _1st_Rim_Result = _CalcRimLight(p,
//                                         _1st_RimLightColor.rgb,
//                                         normalDir,
//                                         _1st_RimLight_Power,
//                                         _1st_RimLight_InsideMask,
//                                         _VertHalfLambert_var,
//                                         i.posWorld.w);
inline YMT_RimLightResult _CalcRimLight(YMT_RimLightInput p,
float3 _RimLightColor,
float3 normalDir,
float _RimLight_Power,
float _RimLight_InsideMask,
float _VertHalfLambert_var,
float _Tweak_RimLightMaskLevel,
float isCullBack)
{
    YMT_RimLightResult res = (YMT_RimLightResult)0;

    _1st_RimLight_InsideMask = max(_1st_RimLight_InsideMask, 0.0001);
    _2nd_RimLight_InsideMask = max(_2nd_RimLight_InsideMask, 0.0001);

    float3 _Is_LightColor_RimLight_var = lerp(_RimLightColor.rgb,
    _1st_RimLightColor.rgb * p.Set_LightColor,
    _Is_LightColor_RimLight);

    float _RimArea_var = (1.0 - dot(p.calcedNormal, p.viewDirection));

    float _RimLightPower_var = pow(_RimArea_var, exp2(lerp(3, 0, _RimLight_Power)));

    float _RimLight_InsideMask_var = saturate(lerp(
        step(_RimLight_InsideMask, _RimLightPower_var),
        (_RimLightPower_var - _RimLight_InsideMask) / (1.0 - _RimLight_InsideMask),
        _1st_RimLight_Feather
    ));

    float _LightDirection_MaskOn_var = lerp(_RimLight_InsideMask_var,
    (saturate((_RimLight_InsideMask_var
    - ((1.0 - _VertHalfLambert_var) + _Tweak_LightDirection_MaskLevel))
    )),
    _LightDirection_MaskOn);

    float3 Set_RimLight = saturate(p.rimLightMask + _Tweak_RimLightMaskLevel)
    * _Is_LightColor_RimLight_var * _LightDirection_MaskOn_var * isCullBack;

    res.Set_RimLight = Set_RimLight;
    res._LightDirection_MaskOn_var = _LightDirection_MaskOn_var;
    return res;
}

inline YMT_RimLightOutput CalcRimLight(YMT_VertexOutput i, YMT_RimLightInput p)
{
    YMT_RimLightOutput rlo = (YMT_RimLightOutput)0;

    float3 normalDir = i.tangentToWorldAndPackedData[2].xyz;
    float2 Set_UV0 = i.uv0.xy;
    float _VertHalfLambert_var = 0.5 * dot(normalDir, p.lightDirection) + 0.5;

    //1st rim light
    float3 _Is_LightColor_1st_RimLight_var = lerp(_1st_RimLightColor.rgb,
    _1st_RimLightColor.rgb * p.Set_LightColor,
    _Is_LightColor_RimLight);

    float _1st_RimArea_var = (1.0 - dot(p.calcedNormal, p.viewDirection));

    float _1st_RimLightPower_var = pow(_1st_RimArea_var, exp2(lerp(3, 0, _1st_RimLight_Power)));

    float _1st_RimLight_InsideMask_var = saturate(lerp(
        step(_1st_RimLight_InsideMask, _1st_RimLightPower_var),
        (_1st_RimLightPower_var - _1st_RimLight_InsideMask) / (1.0 - _1st_RimLight_InsideMask),
        _1st_RimLight_Feather
    ));

    float _1st_LightDirection_MaskOn_var = saturate(p.rimLightMask + _Tweak_1st_RimLightMaskLevel)
    * lerp(_1st_RimLight_InsideMask_var,
    (saturate((_1st_RimLight_InsideMask_var
    - ((1.0 - _VertHalfLambert_var) + _Tweak_LightDirection_MaskLevel))
    )),
    _LightDirection_MaskOn)
    * i.posWorld.w;

    float3 Set_1st_RimLight = _1st_LightDirection_MaskOn_var * _Is_LightColor_1st_RimLight_var ;

    //2nd rim light
    float3 _Is_LightColor_2nd_RimLight_var = lerp(_2nd_RimLightColor.rgb,
    _2nd_RimLightColor.rgb * p.Set_LightColor,
    _Is_LightColor_RimLight);

    float _2nd_RimArea_var = (1.0 - dot(p.calcedNormal, p.viewDirection));

    float _2nd_RimLightPower_var = pow(_2nd_RimArea_var, exp2(lerp(3, 0, _2nd_RimLight_Power)));

    float _2nd_RimLight_InsideMask_var = saturate(lerp(
        step(_2nd_RimLight_InsideMask, _2nd_RimLightPower_var),
        (_2nd_RimLightPower_var - _2nd_RimLight_InsideMask) / (1.0 - _2nd_RimLight_InsideMask),
        _2nd_RimLight_Feather
    ));

    float _2nd_LightDirection_MaskOn_var = saturate(p.rimLightMask + _Tweak_2nd_RimLightMaskLevel)
    * lerp(_2nd_RimLight_InsideMask_var,
    (saturate((_2nd_RimLight_InsideMask_var
    - ((1.0 - _VertHalfLambert_var) + _Tweak_LightDirection_MaskLevel))
    )),
    _LightDirection_MaskOn)
    * i.posWorld.w;

    float3 Set_2nd_RimLight = _Is_LightColor_2nd_RimLight_var * _2nd_LightDirection_MaskOn_var;

    //Ap RimLight
    //NOTE: Ap は一旦1stを元にする
    float _APRimArea_var = _1st_RimArea_var;
    float _ApRimLightPower_var = pow(_APRimArea_var, exp2(lerp(3, 0, _Ap_RimLight_Power)));

    float3 _Is_LightColor_APRimLight_var = lerp(_Ap_RimLightColor.rgb,
    _Ap_RimLightColor.rgb * p.Set_LightColor,
    _Is_LightColor_Ap_RimLight);

    float _APRimLight_InsideMask_var = saturate((lerp(
        step(_1st_RimLight_InsideMask, _ApRimLightPower_var),
        (_ApRimLightPower_var - _1st_RimLight_InsideMask) / (1.0 - _1st_RimLight_InsideMask),
        _Ap_RimLight_Feather)));

    float _APLightDirection_MaskOn_var = saturate((_APRimLight_InsideMask_var
    - (saturate(_VertHalfLambert_var) + _Tweak_LightDirection_MaskLevel)));

    float3 Set_APRimLight = saturate(p.rimLightMask + _Tweak_1st_RimLightMaskLevel)
    * _Is_LightColor_APRimLight_var * _APLightDirection_MaskOn_var * i.posWorld.w;


    rlo.Set_1st_RimLight = Set_1st_RimLight * _RimLight;
    rlo._1st_LightDirection_MaskOn_var = _1st_LightDirection_MaskOn_var * _RimLight;
    rlo.Set_2nd_RimLight = Set_2nd_RimLight * _RimLight;
    rlo._2nd_LightDirection_MaskOn_var = _2nd_LightDirection_MaskOn_var * _RimLight;
    rlo.Set_APRimLight = Set_APRimLight * _Add_Antipodean_RimLight * _RimLight;
    rlo._APLightDirection_MaskOn_var = _APLightDirection_MaskOn_var * _Add_Antipodean_RimLight * _RimLight;

    return rlo;
}

#endif

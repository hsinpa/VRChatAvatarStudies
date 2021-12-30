#ifndef YMTOON_SPECULAR
#define YMTOON_SPECULAR

#include "YMT_Params.cginc"
#include "YMT_Utils.cginc"

//TODO: _Is_SpecularToSpecular リネーム

struct YMT_Specular
{
    float3 halfDirection;
    float3 calcedNormal;
    float3 viewDirection;
    float3 lightDirection;
    float3 Set_LightColor;
    float Set_FinalShadowMask;
    float Set_1st2ndShadowMask;
};

inline float3 CalcSpecular(YMT_VertexOutput i, YMT_Specular p)
{
    float2 Set_UV0 = i.uv0.xy;
    float2 Set_UV1 = i.uv0.zw;

    //_SpecularMap_var.a : Specular Mask
    float4 _SpecularMap_var = tex2D(_SpecularMap, TRANSFORM_TEX(_Use_2ndUV_As_SpecularMapMask ? Set_UV1 : Set_UV0, _SpecularMap));

    #ifdef _DEBUG_SPECULAR_UVSET
        return _Use_2ndUV_As_SpecularMapMask ? float4(Set_UV1.x, Set_UV1.y, 0, 1) : float4(Set_UV0.x, Set_UV0.y, 0, 1);
    #endif

    //r: noise, g:noise mask, b: feather
    float4 _1st_SpecularOptMap_var = tex2D(_1st_SpecularOptMap, TRANSFORM_TEX(Set_UV0, _1st_SpecularOptMap));
    //TODO: Textureの整理 一旦手元でTilingを分担する
    float4 _1st_SpecularOptMap_Feather_var = tex2D(_1st_SpecularOptMap, TRANSFORM_TEX(Set_UV0, _MainTex));

    float4 _2nd_SpecularOptMap_var = tex2D(_2nd_SpecularOptMap, TRANSFORM_TEX(Set_UV0, _2nd_SpecularOptMap));
    //TODO: Textureの整理 一旦手元でTilingを分担する
    float4 _2nd_SpecularOptMap_Feather_var = tex2D(_2nd_SpecularOptMap, TRANSFORM_TEX(Set_UV0, _MainTex));

    #ifdef _DEBUG_SPECULAR_1ST_OPT_R
        return float4(_1st_SpecularOptMap_var.r, _1st_SpecularOptMap_var.r, _1st_SpecularOptMap_var.r, 1.0);
    #endif

    #ifdef _DEBUG_SPECULAR_1ST_OPT_G
        return float4(_1st_SpecularOptMap_Feather_var.g, _1st_SpecularOptMap_Feather_var.g, _1st_SpecularOptMap_Feather_var.g, 1.0);
    #endif

    #ifdef _DEBUG_SPECULAR_1ST_OPT_B
        return float4(_1st_SpecularOptMap_Feather_var.b, _1st_SpecularOptMap_Feather_var.b, _1st_SpecularOptMap_Feather_var.b, 1.0);
    #endif

    #ifdef _DEBUG_SPECULAR_1ST_OPT_A
        return float4(_1st_SpecularOptMap_Feather_var.a, _1st_SpecularOptMap_Feather_var.a, _1st_SpecularOptMap_Feather_var.a, 1.0);
    #endif

    #ifdef _DEBUG_SPECULAR_2ND_OPT_R
        return float4(_2nd_SpecularOptMap_var.r, _2nd_SpecularOptMap_var.r, _2nd_SpecularOptMap_var.r, 1.0);
    #endif

    #ifdef _DEBUG_SPECULAR_2ND_OPT_G
        return float4(_2nd_SpecularOptMap_Feather_var.g, _2nd_SpecularOptMap_Feather_var.g, _2nd_SpecularOptMap_Feather_var.g, 1.0);
    #endif

    #ifdef _DEBUG_SPECULAR_2ND_OPT_B
        return float4(_2nd_SpecularOptMap_Feather_var.b, _2nd_SpecularOptMap_Feather_var.b, _2nd_SpecularOptMap_Feather_var.b, 1.0);
    #endif

    #ifdef _DEBUG_SPECULAR_2ND_OPT_A
        return float4(_2nd_SpecularOptMap_Feather_var.a, _2nd_SpecularOptMap_Feather_var.a, _2nd_SpecularOptMap_Feather_var.a, 1.0);
    #endif

    float _Specular_Area_var = 0.5 * dot(p.halfDirection, p.calcedNormal) + 0.5;

    float _1st_Specular_var = saturate(_Specular_Area_var - saturate(((1 - _1st_SpecularOptMap_var.r) * _1st_SpecularOptMap_Feather_var.g) * _1st_SpecularMapMaskScaler - _1st_SpecularMapMaskOffset));
    float _2nd_Specular_var = saturate(_Specular_Area_var - saturate(((1 - _2nd_SpecularOptMap_var.r) * _2nd_SpecularOptMap_Feather_var.g) * _2nd_SpecularMapMaskScaler - _2nd_SpecularMapMaskOffset));

    float _1st_TweakSpecularMask_var = lerp((1.0 - step(_1st_Specular_var, (1.0 - pow(_1st_Specular_Power, 5)))),
    pow(_1st_Specular_var, exp2(lerp(11, 1, _1st_Specular_Power))),
    _Is_SpecularToSpecular);

    float _2nd_TweakSpecularMask_var = lerp((1.0 - step(_2nd_Specular_var, (1.0 - pow(_2nd_Specular_Power, 5)))),
    pow(_2nd_Specular_var, exp2(lerp(11, 1, _2nd_Specular_Power))),
    _Is_SpecularToSpecular);

    //NOTE: 一旦BaseStep の Min 0.01 をいれてる
    //NOTE: _Is_SpecularToSpecular : false の時は効かない
    // _TweakSpecularMask_var = saturate((_TweakSpecularMask_var - 0.01) / (saturate(_SpecularMask * _Tweak_Specular_Feather_Level) ));
    _1st_TweakSpecularMask_var = saturate((_1st_TweakSpecularMask_var - 0.01) / (saturate(_1st_SpecularOptMap_Feather_var.b * max(0.0001, _Tweak_Specular_Feather_Level))));
    _2nd_TweakSpecularMask_var = saturate((_2nd_TweakSpecularMask_var - 0.01) / (saturate(_2nd_SpecularOptMap_Feather_var.b * max(0.0001, _Tweak_Specular_Feather_Level))));

    float _TweakSpecularMask_var = saturate(_1st_TweakSpecularMask_var + _2nd_TweakSpecularMask_var * _Use_2nd_Specular_OptMap);

    float3 binormalObj = normalize(i.tangentToWorldAndPackedData[1].xyz);
    float3 shiftedLowT = ShiftTangent(binormalObj, p.calcedNormal, _1st_ShiftTangent);
    float anisoLow = StrandSpecular(shiftedLowT, p.viewDirection, p.lightDirection, _1stAnisoHighLightPower, _1stAnisoHighLightStrength);

    float3 shiftedHighT = ShiftTangent(binormalObj, p.calcedNormal, _SpecularMap_var.a + _2nd_ShiftTangent);
    float anisoHigh = StrandSpecular(shiftedHighT, p.viewDirection, p.lightDirection, _2ndAnisoHighLightPower, _2ndAnisoHighLightStrength);
    float anisoMask = saturate(saturate(anisoLow) + saturate(anisoHigh));

    _TweakSpecularMask_var = _Use_Aniso_HighLight ? anisoMask : _TweakSpecularMask_var;

    #ifdef _DEBUG_SPECULAR_MASK
        return half4(_TweakSpecularMask_var, _TweakSpecularMask_var, _TweakSpecularMask_var, 1);
    #endif

    float3 _Specular_var = lerp((_SpecularMap_var.rgb * _Specular.rgb),
    (_SpecularMap_var.rgb * _Specular.rgb) * p.Set_LightColor,
    _Is_LightColor_Specular)
    * _TweakSpecularMask_var;

    float3 Set_Specular = lerp(_Specular_var,
    _Specular_var * ((1.0 - p.Set_FinalShadowMask)
    + (p.Set_FinalShadowMask * _TweakSpecularOnShadow)),
    _Is_UseTweakSpecularOnShadow);

    #ifdef _DEBUG_SPECULAR
        return half4(_Specular_var, 1);
    #endif

    return Set_Specular * _Use_Specular;
}

#endif

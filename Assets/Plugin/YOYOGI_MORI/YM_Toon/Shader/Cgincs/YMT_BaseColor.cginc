#ifndef YMTOON_BASE_COLOR
#define YMTOON_BASE_COLOR

#include "YMT_Params.cginc"

struct YMT_BaseColorInput
{
    float3 lightColor;
    float4 _MainTex_var;
    float attenuation;
    float _HalfLambert_var;
};

struct YMT_BaseColorOutput
{
    float3 Set_LightColor;
    float Set_FinalShadowMask;
    float Set_1st2ndShadowMask;
    float3 Set_FinalBaseColor;
    float4 debug_Color;
};

inline YMT_BaseColorOutput CalcBaseColor(YMT_VertexOutput i, YMT_BaseColorInput p)
{
    YMT_BaseColorOutput bcout = (YMT_BaseColorOutput)0;
    float2 Set_UV0 = i.uv0.xy;

    _1st_Shade_Step = max(_1st_Shade_Step, 0.0001);
    _1st_Shade_Feather = max(_1st_Shade_Feather, 0.0001);
    _2nd_Shade_Step = max(_2nd_Shade_Step, 0.0001);
    _2nd_Shade_Feather = max(_2nd_Shade_Feather, 0.0001);

    #ifdef _IS_PASS_FWDBASE
        float3 Set_LightColor = p.lightColor.rgb;

    #elif _IS_PASS_FWDDELTA
        float _LightIntensity = lerp(0, LuminanceRec709(_LightColor0) * p.attenuation, _WorldSpaceLightPos0.w);

        //Filtering the high intensity zone of PointLights
        float3 Set_LightColor = lerp(p.lightColor,
        lerp(p.lightColor, min(p.lightColor, _LightColor0.rgb * p.attenuation * _1st_Shade_Step), _WorldSpaceLightPos0.w),
        _Is_Filter_HiCutPointLightColor);

    #endif

    bcout.Set_LightColor = Set_LightColor;

    float3 Set_BaseColor = (_BaseColor.rgb * p._MainTex_var.rgb) * Set_LightColor;

    float _BaseColorStep_Feather_Sub = _1st_Shade_Step - _1st_Shade_Feather;
    float _1stShadeColorStep_Feather_Sub = _2nd_Shade_Step - _2nd_Shade_Feather;

    float4 _1st_ShadeMap_var = tex2D(_1st_ShadeMap, TRANSFORM_TEX(Set_UV0, _1st_ShadeMap));
    float3 Set_1st_ShadeColor = (_1st_ShadeColor.rgb * _1st_ShadeMap_var.rgb) * Set_LightColor;

    //NOTE: 1st/2ndは同じTextureだけど色は変えたいので一部2ndでも1stのTextureを参照しています
    float3 Set_2nd_ShadeColor = (_2nd_ShadeColor.rgb * _1st_ShadeMap_var.rgb) * Set_LightColor;

    #ifdef _DEBUG_BASE_COLOR_ONLY
        bcout.debug_Color = float4(Set_BaseColor, 1);
        return bcout;
    #endif

    #ifdef _DEBUG_BASE_MAP_ALPHA
        bcout.debug_Color = float4(p._MainTex_var.a, p._MainTex_var.a, p._MainTex_var.a, 1);
        return bcout;
    #endif

    #ifdef _IS_PASS_FWDBASE

        float _1stShadingGradeMap_var = _Use_1stShadeMapAlpha_As_ShadowMask ? _1st_ShadeMap_var.a : 1;

        #ifdef _DEBUG_1ST_SHADING_GRADEMASK
            bcout.debug_Color = float4(_1stShadingGradeMap_var, _1stShadingGradeMap_var, _1stShadingGradeMap_var, 1);
            return bcout;
        #endif

        float _1stShadingGradeMapLevel_var = _1stShadingGradeMap_var < 0.95 ? _1stShadingGradeMap_var + _Tweak_1stShadingGradeMapLevel : 1;

        float _ReceivedShadowsLevel_var = saturate(p.attenuation + _Tweak_ReceivedShadowsLevel);

        float Set_FinalShadowMask = saturate((1.0 + (saturate(_1stShadingGradeMapLevel_var)
        * (p._HalfLambert_var * saturate(_ReceivedShadowsLevel_var)
        - (_BaseColorStep_Feather_Sub))
        * (-1.0)
        )
        / (_1st_Shade_Step - (_BaseColorStep_Feather_Sub))));

        float Set_1st2ndShadowMask = saturate((1.0 + (saturate(_1stShadingGradeMapLevel_var)
        * (p._HalfLambert_var * saturate(_ReceivedShadowsLevel_var)
        - (_1stShadeColorStep_Feather_Sub))
        * (-1.0)
        )
        / (_2nd_Shade_Step - (_1stShadeColorStep_Feather_Sub))));

    #elif _IS_PASS_FWDDELTA

        float Set_FinalShadowMask = saturate((1.0 + ((p._HalfLambert_var * saturate(1.0 + _Tweak_ReceivedShadowsLevel)
        - (_BaseColorStep_Feather_Sub)) * (-1.0))
        / (_1st_Shade_Step - (_BaseColorStep_Feather_Sub)))
        );

        float Set_1st2ndShadowMask = saturate((1.0 + ((p._HalfLambert_var - (_1stShadeColorStep_Feather_Sub))
        * (-1.0))
        / (_2nd_Shade_Step - (_1stShadeColorStep_Feather_Sub))));
    #endif


    #ifdef _DEBUG_1ST_SHADEMASK
        bcout.debug_Color = half4(Set_FinalShadowMask, Set_FinalShadowMask, Set_FinalShadowMask, 1);
        return bcout;
    #endif

    #ifdef _DEBUG_2ND_SHADEMASK
        bcout.debug_Color = half4(Set_1st2ndShadowMask, Set_1st2ndShadowMask, Set_1st2ndShadowMask, 1);
        return bcout;
    #endif

    float3 Set_FinalBaseColor = lerp(Set_BaseColor,
    lerp(Set_1st_ShadeColor,
    Set_2nd_ShadeColor,
    Set_1st2ndShadowMask),
    Set_FinalShadowMask);

    bcout.Set_FinalBaseColor = Set_FinalBaseColor;
    bcout.Set_FinalShadowMask = Set_FinalShadowMask;
    bcout.Set_1st2ndShadowMask = Set_1st2ndShadowMask;
    return bcout;
}

#endif

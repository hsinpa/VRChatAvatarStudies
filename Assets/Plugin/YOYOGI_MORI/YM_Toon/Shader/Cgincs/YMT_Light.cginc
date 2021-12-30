#ifndef YMTOON_LIGHT
#define YMTOON_LIGHT

#include "YMT_Params.cginc"
#include "YMT_Utils.cginc"

struct YMT_LightInput
{
    float3 calcedNormal;
};

struct YMT_LightOutput
{
    float attenuation;
    float3 lightDirection;
    float3 lightColor;
};

inline YMT_LightOutput CalcLight(YMT_VertexOutput i, YMT_LightInput p)
{
    YMT_LightOutput lout = (YMT_LightOutput)0;

    UNITY_LIGHT_ATTENUATION(attenuation, i, i.posWorld.xyz);
    lout.attenuation = attenuation;

    #ifdef _IS_PASS_FWDBASE

        float4 worldSpaceLightPos0 = _Use_OwnDirLight ? float4(_OwnDirectionalLightDir.xyz, 1) : _WorldSpaceLightPos0;
        float4 lightColor0 = _Use_OwnDirLight ? float4(_OwnDirectionalLightColor.xyz, 1) : _LightColor0;

        float3 defaultLightDirection = normalize(UNITY_MATRIX_V[2].xyz + UNITY_MATRIX_V[1].xyz);
        float3 defaultLightColor = saturate(max(half3(0.05, 0.05, 0.05),
        max(ShadeSH9(half4(0.0, 0.0, 0.0, 1.0)), ShadeSH9(half4(0.0, -1.0, 0.0, 1.0)).rgb))
        );

        lout.lightDirection = normalize(lerp(defaultLightDirection, worldSpaceLightPos0.xyz, saturate(_Use_OwnDirLight + any(_WorldSpaceLightPos0.xyz))));

        lout.lightColor = max(defaultLightColor, saturate(lightColor0.rgb));

    #elif _IS_PASS_FWDDELTA

        //_WorldSpaceLightPos0.w == 1 == PointLight
        lout.lightDirection = normalize(lerp(
            _WorldSpaceLightPos0.xyz,
            _WorldSpaceLightPos0.xyz - i.posWorld.xyz,
            _WorldSpaceLightPos0.w));

        float3 normalDir = i.tangentToWorldAndPackedData[2].xyz;
        float3 addPassLightColor = (0.5 * dot(p.calcedNormal, lout.lightDirection) + 0.5)
        * _LightColor0.rgb * attenuation;

        //ref: Grayscale
        float pureIntencity = max(0.001, LuminanceRec709(_LightColor0));
        lout.lightColor = max(0, lerp(0, min(addPassLightColor, addPassLightColor / pureIntencity), _WorldSpaceLightPos0.w));

    #endif
    return lout;
}

#endif

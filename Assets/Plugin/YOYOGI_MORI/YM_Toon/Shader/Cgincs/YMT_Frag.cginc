//UCTS_DoubleShadeWithFeather.cginc
//Unitychan Toon Shader ver.2.0
//v.2.0.7.5
//nobuyuki@unity3d.com
//https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
//(C)Unity Technologies Japan/UCL

//NOTE: 命名規則
//YMT_Params.cgincにあるものは _ 始まり
//有効 / 無効 を意味するパラメータは _UseHoge
//Texture名は全部 _HogeMap

//基本的に全てlower Camel Case
//関連cgincは YMT_Hoge.cginc

//数字から始まる変数名はリネームすること
//textureを展開した値 : _var
//structで値が返ってくる関数内でデバッグ表示する場合はstructに debug color を持たせる(暫定)

#ifndef YMTOON_Frag
#define YMTOON_Frag

#define _TANGENT_TO_WORLD

#include "YMT_Params.cginc"
#include "YMT_Utils.cginc"

#include "YMT_Light.cginc"

#include "YMT_BaseColor.cginc"

#include "YMT_Specular.cginc"
#include "YMT_RimLight.cginc"
#include "YMT_MatCap.cginc"
#include "YMT_FakeSSS.cginc"

float4 fragForward(YMT_VertexOutput i)
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    float3 normalDir = normalize(i.tangentToWorldAndPackedData[2].xyz);
    float3x3 tangentTransform = float3x3(i.tangentToWorldAndPackedData[0].xyz, i.tangentToWorldAndPackedData[1].xyz, normalDir);
    float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
    float2 Set_UV0 = i.uv0.xy;
    float2 Set_UV1 = i.uv0.zw;

    float3 _BumpMap_var = UnpackScaleNormal(tex2D(_BumpMap, TRANSFORM_TEX(Set_UV0, _BumpMap)), _BumpScale);
    float3 normalDirection = normalize(mul(_BumpMap_var.rgb, tangentTransform)); // Perturbed normals
    float3 calcedNormal = lerp(normalDir, normalDirection, _Is_NormalMapToBase);

    // correct mipmapping
    float2 dx = ddx(Set_UV0);
    float2 dy = ddy(Set_UV0);

    float4 _MainTex_var = tex2Dgrad(_MainTex, Set_UV0, dx, dy);

    // Cutout or Transparent のとき
    //NOTE: (defined(_ALPHATEST_ON) && defined (_ALPHAPREMULTIPLY_ON)) にはなり得ないっぽい
    // #if defined(_ALPHATEST_ON) || (defined(_ALPHATEST_ON) && defined (_ALPHAPREMULTIPLY_ON))
    #if _ALPHATEST_ON
        float baseMapAlpha = saturate((lerp(_MainTex_var.a, (1.0 - _MainTex_var.a), _Inverse_Clipping)));
        clip(baseMapAlpha + _Clipping_Level - 0.5);

    #elif _ALPHAPREMULTIPLY_ON
        float baseMapAlpha = _MainTex_var.a;
    #endif

    //-----------------------------------------------------------------------------------------------------------
    //-- Light
    YMT_LightInput lin = (YMT_LightInput)0;
    lin.calcedNormal = calcedNormal;
    YMT_LightOutput lout = CalcLight(i, lin);

    float3 lightDirection = lout.lightDirection;
    float3 lightColor = lout.lightColor;
    float attenuation = lout.attenuation;

    //-----------------------------------------------------------------------------------------------------------
    //-- base calc
    float3 halfDirection = normalize(viewDirection + lightDirection);
    float _HalfLambert_var = 0.5 * dot(calcedNormal, lightDirection) + 0.5;

    float4 _BaseOptMap_var = tex2D(_BaseOptMap, TRANSFORM_TEX(Set_UV0, _BaseOptMap));
    float viewDot = (dot(viewDirection, calcedNormal) + 1.0) * 0.5;

    #ifdef _DEBUG_ORIGINAL_NORMAL
        return half4(normalDir, 1);
    #endif

    #ifdef _DEBUG_MAPPED_NORMAL
        return half4(normalDirection, 1);
    #endif

    //-----------------------------------------------------------------------------------------------------------
    //base color
    YMT_BaseColorInput bcin = (YMT_BaseColorInput)0;
    bcin.lightColor = lightColor;
    bcin._MainTex_var = _MainTex_var;
    bcin.attenuation = attenuation;
    bcin._HalfLambert_var = _HalfLambert_var;
    YMT_BaseColorOutput bcout = CalcBaseColor(i, bcin);

    float3 Set_LightColor = bcout.Set_LightColor;
    float Set_FinalShadowMask = bcout.Set_FinalShadowMask;
    float Set_1st2ndShadowMask = bcout.Set_1st2ndShadowMask;
    float3 Set_FinalBaseColor = bcout.Set_FinalBaseColor;


    #if defined(_DEBUG_BASE_COLOR_ONLY) || defined(_DEBUG_BASE_MAP_ALPHA) || defined(_DEBUG_1ST_SHADING_GRADEMASK) || defined(_DEBUG_1ST_SHADEMASK) || defined(_DEBUG_2ND_SHADEMASK)
        return bcout.debug_Color;
    #endif

    //-----------------------------------------------------------------------------------------------------------
    //-- Specular
    YMT_Specular hc = (YMT_Specular)0;
    hc.halfDirection = halfDirection;
    hc.calcedNormal = calcedNormal;
    hc.viewDirection = viewDirection;
    hc.lightDirection = lightDirection;
    hc.Set_LightColor = Set_LightColor;
    hc.Set_FinalShadowMask = Set_FinalShadowMask;
    hc.Set_1st2ndShadowMask = Set_1st2ndShadowMask;

    float3 calcedSpecular = CalcSpecular(i, hc);

    #if defined(_DEBUG_SPECULAR_UVSET) || defined(_DEBUG_SPECULAR_MASK) || defined(_DEBUG_SPECULAR)
        return float4(calcedSpecular, 1);
    #endif
    #if defined(_DEBUG_SPECULAR_1ST_OPT_R) || defined(_DEBUG_SPECULAR_1ST_OPT_G) || defined(_DEBUG_SPECULAR_1ST_OPT_B) || defined(_DEBUG_SPECULAR_1ST_OPT_A)
        return float4(calcedSpecular, 1);
    #endif
    #if defined(_DEBUG_SPECULAR_2ND_OPT_R) || defined(_DEBUG_SPECULAR_2ND_OPT_G) || defined(_DEBUG_SPECULAR_2ND_OPT_B) || defined(_DEBUG_SPECULAR_2ND_OPT_A)
        return float4(calcedSpecular, 1);
    #endif

    float3 Set_Specular = Set_FinalBaseColor + calcedSpecular;

    //-----------------------------------------------------------------------------------------------------------
    //- base path
    #ifdef _IS_PASS_FWDBASE
        //-----------------------------------------------------------------------------------------------------------
        //-- rim light

        YMT_RimLightInput rli = (YMT_RimLightInput)0;
        rli.calcedNormal = calcedNormal;
        rli.viewDirection = viewDirection;
        rli.lightDirection = lightDirection;
        rli.Set_LightColor = Set_LightColor;
        rli.rimLightMask = _BaseOptMap_var.r;

        YMT_RimLightOutput rlo = CalcRimLight(i, rli);

        #ifdef _DEBUG_RIM_LIGHT
            return float4(rlo.Set_1st_RimLight, 1);
        #endif

        float3 _1st_RimLight_var = _Is_1st_RimLight_Addtive
        ? Set_Specular + rlo.Set_1st_RimLight
        : Set_Specular * saturate(1.0 - rlo._1st_LightDirection_MaskOn_var) + Set_Specular * rlo.Set_1st_RimLight;

        float3 _2nd_RimLight_var = _Is_2nd_RimLight_Addtive
        ? _1st_RimLight_var + rlo.Set_2nd_RimLight
        : _1st_RimLight_var * saturate(1.0 - rlo._2nd_LightDirection_MaskOn_var) + _1st_RimLight_var * rlo.Set_2nd_RimLight;

        float3 _RimLight_var = rlo.Set_APRimLight + _2nd_RimLight_var;

        //-----------------------------------------------------------------------------------------------------------
        //-- MatCap
        YMT_MatCap mc = (YMT_MatCap)0;
        mc.calcedNormal = calcedNormal;
        mc.viewDirection = viewDirection;
        mc.Set_LightColor = Set_LightColor;
        mc.Set_FinalShadowMask = Set_FinalShadowMask;
        mc.Set_RimLight = rlo.Set_1st_RimLight;
        mc._RimLight_var = _RimLight_var;

        float3 matCapColorFinal = CalcMatCap(i, mc);

        float3 finalColor = lerp(_RimLight_var, matCapColorFinal, _MatCap);
        // float3 _MatCap_var = matCapColorFinal * _MatCap;

        //-----------------------------------------------------------------------------------------------------------
        //-- env
        float3 envLightColor = DecodeLightProbe(calcedNormal) < float3(1, 1, 1)
        ? DecodeLightProbe(calcedNormal) : float3(1, 1, 1);

        float envLightIntensity = LuminanceRec709(envLightColor) < 1 ? LuminanceRec709(envLightColor) : 1;

        //-----------------------------------------------------------------------------------------------------------
        //-- emission
        float4 _EmissionMap_var = tex2D(_EmissionMap, TRANSFORM_TEX(Set_UV0, _EmissionMap));
        emissive = _EmissionMap_var.rgb * _Emissive_Color.rgb * _EmissionMap_var.a * _Use_Emissive;

        #ifdef _DEBUG_EMISSIVE
            return float4(emissive, 1);
        #endif

        //-----------------------------------------------------------------------------------------------------------
        //-- Final Composition
        finalColor = saturate(finalColor) + emissive;

        YMT_FakeSSS fs = (YMT_FakeSSS)0;
        fs.sssMask = _BaseOptMap_var.b;
        fs.finalColor = finalColor;
        finalColor = CalcFakeSSS(i, fs);

        //-----------------------------------------------------------------------------------------------------------
        //- addtive path
    #elif _IS_PASS_FWDDELTA
        float3 finalColor = Set_FinalBaseColor + lerp(Set_Specular, float3(0, 0, 0), _Is_Filter_HiCutPointLightColor);
        finalColor = saturate(finalColor);

    #endif


    #ifdef _ALPHAPREMULTIPLY_ON
        float Set_Opacity = saturate((baseMapAlpha + _Tweak_transparency));

        #ifdef _IS_PASS_FWDBASE
            fixed4 finalRGBA = fixed4(finalColor, Set_Opacity);
        #elif _IS_PASS_FWDDELTA
            fixed4 finalRGBA = fixed4(finalColor * Set_Opacity, 0);
        #endif

    #else
        #ifdef _IS_PASS_FWDBASE
            fixed4 finalRGBA = fixed4(finalColor, 1);
        #elif _IS_PASS_FWDDELTA
            fixed4 finalRGBA = fixed4(finalColor, 0);
        #endif

    #endif
    return finalRGBA;
}

#endif

//UCTS_Outline.cginc
//Unitychan Toon Shader ver.2.0
//v.2.0.7.5
//nobuyuki@unity3d.com
//https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project
//(C)Unity Technologies Japan/UCL
// 2018/08/23 N.Kobayashi (Unity Technologies Japan)
// カメラオフセット付きアウトライン（BaseColorライトカラー反映修正版）
// 2017/06/05 PS4対応版

#ifndef YMTOON_OUTLINE
#define YMTOON_OUTLINE

#include "YMT_Params.cginc"

#include "YMT_Light.cginc"
#include "YMT_Utils.cginc"

#define _TANGENT_TO_WORLD


uniform fixed _Is_LightColor_Outline;

uniform float4 _Color;
uniform float _Outline_Width;
uniform float _Farthest_Distance;
uniform float _Nearest_Distance;
uniform float4 _Outline_Color;
uniform fixed _Is_BlendBaseColor;
uniform fixed _Is_BlendBaseColorWeight;

uniform sampler2D _OutlineMap; uniform float4 _OutlineMap_ST;
uniform fixed _Is_OutlineMap;
//Baked Normal Texture for Outline
uniform sampler2D _BakedNormal; uniform float4 _BakedNormal_ST;
uniform fixed _Is_BakedNormal;

YMT_VertexOutput vert(YMT_VertexInput v)
{
    YMT_VertexOutput o = (YMT_VertexOutput)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.uv0.xy = v.texcoord0;
    float4 objPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
    float2 Set_UV0 = o.uv0.xy;

    float4 _BaseOptMap_var = tex2Dlod(_BaseOptMap, float4(TRANSFORM_TEX(Set_UV0, _BaseOptMap), 0.0, 0));
    o.uv0.w = _BaseOptMap_var.g;

    //baked Normal Texture for Outline
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);

    //UTSではここを通る
    #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(normalize(mul((float3x3)unity_ObjectToWorld, v.tangent.xyz)), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        //tangent
        o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
        //binormal
        o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
        //normal
        o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndPackedData[0].xyz = 0;
        o.tangentToWorldAndPackedData[1].xyz = 0;
        o.tangentToWorldAndPackedData[2].xyz = normalWorld;
    #endif

    float3x3 tangentTransform = float3x3(o.tangentToWorldAndPackedData[0].xyz, o.tangentToWorldAndPackedData[1].xyz, o.tangentToWorldAndPackedData[2].xyz);

    //UnpackNormal()が使えないので、以下で展開。使うテクスチャはBump指定をしないこと.
    float4 _BakedNormal_var = (tex2Dlod(_BakedNormal, float4(TRANSFORM_TEX(Set_UV0, _BakedNormal), 0.0, 0)) * 2 - 1);
    float3 _BakedNormalDir = normalize(mul(_BakedNormal_var.rgb, tangentTransform));

    float Set_Outline_Width = _Outline_Width * 0.001
    * smoothstep(_Farthest_Distance, _Nearest_Distance, distance(objPos.rgb, _WorldSpaceCameraPos))
    * _BaseOptMap_var.g;

    float4 _ClipCameraPos = mul(UNITY_MATRIX_VP, float4(_WorldSpaceCameraPos.xyz, 1));

    // baked Normal Texture for Outline
    o.pos = UnityObjectToClipPos(lerp(
        float4(v.vertex.xyz + v.normal * Set_Outline_Width, 1),
        float4(v.vertex.xyz + _BakedNormalDir * Set_Outline_Width, 1),
        _Is_BakedNormal));
    o.posWorld.xyz = mul(unity_ObjectToWorld, v.vertex).xyz;

    UNITY_TRANSFER_SHADOW(o, v.texcoord0);

    return o;
}

float4 frag(YMT_VertexOutput i) : SV_Target
{

    //Outline幅が 0 or OutlineMaskの値的に描画したくないときはここで終わる
    if (_Outline_Width <= 0.0 || i.uv0.w <= 0.0)
    {
        clip(-1);
        return 1;
    }

    #ifdef _ALPHAPREMULTIPLY_ON
        clip(-1);
        return 1;
    #endif

    float2 Set_UV0 = i.uv0.xy;

    // correct mipmapping
    float2 dx = ddx(Set_UV0);
    float2 dy = ddy(Set_UV0);

    float4 _MainTex_var = tex2Dgrad(_MainTex, Set_UV0, dx, dy);

    #if defined(_ALPHATEST_ON)
        float baseMapAlpha = saturate((lerp(_MainTex_var.a, (1.0 - _MainTex_var.a), _Inverse_Clipping)));
        clip(baseMapAlpha + _Clipping_Level - 0.5);
    #endif

    _Color = _BaseColor;
    float4 objPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));

    half3 ambientSkyColor = unity_AmbientSky.rgb;

    //-- Light
    YMT_LightInput lin = (YMT_LightInput)0;
    lin.calcedNormal = normalize(i.tangentToWorldAndPackedData[2].xyz);;
    YMT_LightOutput lout = CalcLight(i, lin);

    float3 lightDirection = lout.lightDirection;
    float3 calcedLightColor = lout.lightColor;
    float attenuation = lout.attenuation;

    float3 lightColor = saturate(calcedLightColor * attenuation + ambientSkyColor.rgb);
    float lightColorIntensity = LuminanceRec709(lightColor);

    lightColor = lightColorIntensity < 1 ? lightColor : lightColor / lightColorIntensity;
    lightColor = lerp(half3(1.0, 1.0, 1.0), lightColor, _Is_LightColor_Outline);

    float3 Set_BaseColor = _BaseColor.rgb * _MainTex_var.rgb;
    float3 _Is_BlendBaseColor_var = lerp(_Outline_Color.rgb * lightColor,
    _Outline_Color.rgb * Set_BaseColor * Set_BaseColor * lightColor,
    _Is_BlendBaseColor * _Is_BlendBaseColorWeight);

    float3 _OutlineMap_var = tex2D(_OutlineMap, TRANSFORM_TEX(Set_UV0, _OutlineMap));

    float3 Set_Outline_Color = lerp(_Is_BlendBaseColor_var, _OutlineMap_var.rgb * _Outline_Color.rgb * lightColor, _Is_OutlineMap);
    return float4(Set_Outline_Color, 1);
}
#endif

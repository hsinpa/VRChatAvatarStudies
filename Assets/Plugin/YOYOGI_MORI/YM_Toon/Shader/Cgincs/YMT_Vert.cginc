#ifndef YMTOON_VERT
#define YMTOON_VERT
#define _TANGENT_TO_WORLD

#include "YMT_Params.cginc"

inline YMT_VertexOutput CalcVertForward(YMT_VertexInput v)
{
    YMT_VertexOutput o = (YMT_VertexOutput)0;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.uv0.xy = v.texcoord0;
    o.uv0.zw = v.texcoord1;
    o.posWorld.xyz = mul(unity_ObjectToWorld, v.vertex).xyz;
    o.posWorld.w = v.isCullBack;
    o.pos = UnityObjectToClipPos(v.vertex);

    float3 crossFwd = cross(UNITY_MATRIX_V[0], UNITY_MATRIX_V[1]);
    o.opt.x = dot(crossFwd, UNITY_MATRIX_V[2]) < 0 ? 1 : - 1;

    UNITY_TRANSFER_SHADOW(o, v.texcoord0);

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

    // o.grabPos = ComputeGrabScreenPos(o.pos);
    return o;
}

#endif

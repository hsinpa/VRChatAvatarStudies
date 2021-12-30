#ifndef YMTOON_FAKE_SSS
#define YMTOON_FAKE_SSS

#include "YMT_Params.cginc"
#include "YMT_Utils.cginc"

struct YMT_FakeSSS
{
    float sssMask;
    float3 finalColor;
};

#ifdef _IS_PASS_FWDBASE
    inline float3 CalcFakeSSS(YMT_VertexOutput i, YMT_FakeSSS p)
    {
        float viewLightHalfL = dot(-UNITY_MATRIX_V[2].xyz, _WorldSpaceLightPos0.xyz) * 0.5 + 0.5;
        float sssWeight = p.sssMask * easeInExpo(viewLightHalfL);
        _HSVOffset.x = _HSVOffset.x * rcp(360);
        float3 shiftedColor = shiftColor(_Use_As_Skin ?(p.finalColor.rgb * _SkinSSSMulColor.rgb) : p.finalColor.rgb,
        _HSVOffset.xyz,
        sssWeight);

        #ifdef _DEBUG_SSS_SHIFTED_W_MASK
            float shadow = p.sssMask;
            p.sssMask = saturate(p.sssMask * shadow);
            return half4(shadow, shadow, shadow, 1);

        #elif _DEBUG_SSS_SHIFTED
            return half4(shiftedColor.xyz, 1);
        #endif

        // float2 grabUV = float2(i.grabPos.x / i.grabPos.w, i.grabPos.y / i.grabPos.w);
        // float4 grabMap = tex2D(_GrabPassTexture, grabUV);
        // finalColor = lerp(finalColor, shiftedColor * lerp(1, grabMap.rgb, _FakeTransparentWeight), sssWeight);

        return lerp(p.finalColor, shiftedColor, sssWeight * _Use_SSS);
    }
#endif

#endif

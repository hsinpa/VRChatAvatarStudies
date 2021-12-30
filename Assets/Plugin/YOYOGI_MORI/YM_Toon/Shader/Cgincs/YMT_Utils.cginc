#ifndef YMTOON_UTILS
#define YMTOON_UTILS

// UV回転をする関数：RotateUV()
//float2 rotatedUV = RotateUV(i.uv0, (_angular_Verocity*3.141592654), float2(0.5, 0.5), _Time.g);
inline float2 RotateUV(float2 _uv, float _radian, float2 _piv, float _time)
{
    float RotateUV_ang = _radian;
    float RotateUV_cos = cos(_time * RotateUV_ang);
    float RotateUV_sin = sin(_time * RotateUV_ang);
    return(mul(_uv - _piv, float2x2(RotateUV_cos, -RotateUV_sin, RotateUV_sin, RotateUV_cos)) + _piv);
}

inline float LuminanceRec709(float3 color)
{
    return dot(color, float3(0.299, 0.587, 0.114));
}

inline fixed3 DecodeLightProbe(fixed3 N)
{
    return ShadeSH9(float4(N, 1));
}

inline float easeInExpo(float t)
{
    return(t == 0.0) ? 0.0 : pow(2.0, 10.0 * (t - 1.0));
}

inline float3 rgb2hsv(float3 rgb)
{
    float3 hsv;
    float maxValue = max(rgb.r, max(rgb.g, rgb.b));
    float minValue = min(rgb.r, min(rgb.g, rgb.b));
    float delta = maxValue - minValue;

    hsv.z = maxValue;

    if (maxValue != 0.0)
    {
        hsv.y = delta / maxValue;
    }
    else
    {
        hsv.y = 0.0;
    }

    if (hsv.y > 0.0)
    {
        if (rgb.r == maxValue)
        {
            hsv.x = (rgb.g - rgb.b) / delta;
        }
        else if (rgb.g == maxValue)
        {
            hsv.x = 2 + (rgb.b - rgb.r) / delta;
        }
        else
        {
            hsv.x = 4 + (rgb.r - rgb.g) / delta;
        }
        hsv.x /= 6.0;
        if (hsv.x < 0)
        {
            hsv.x += 1.0;
        }
    }

    return hsv;
}

inline float3 hsv2rgb(float3 hsv)
{
    float3 rgb;

    if (hsv.y == 0)
    {
        rgb.r = rgb.g = rgb.b = hsv.z;
    }
    else
    {
        hsv.x *= 6.0;
        float i = floor(hsv.x);
        float f = hsv.x - i;
        float aa = hsv.z * (1 - hsv.y);
        float bb = hsv.z * (1 - (hsv.y * f));
        float cc = hsv.z * (1 - (hsv.y * (1 - f)));
        if (i < 1)
        {
            rgb.r = hsv.z;
            rgb.g = cc;
            rgb.b = aa;
        }
        else if (i < 2)
        {
            rgb.r = bb;
            rgb.g = hsv.z;
            rgb.b = aa;
        }
        else if (i < 3)
        {
            rgb.r = aa;
            rgb.g = hsv.z;
            rgb.b = cc;
        }
        else if (i < 4)
        {
            rgb.r = aa;
            rgb.g = bb;
            rgb.b = hsv.z;
        }
        else if (i < 5)
        {
            rgb.r = cc;
            rgb.g = aa;
            rgb.b = hsv.z;
        }
        else
        {
            rgb.r = hsv.z;
            rgb.g = aa;
            rgb.b = bb;
        }
    }
    return rgb;
}

inline float3 shiftColor(float3 rgb, half3 shift, half weight)
{
    float3 hsv = rgb2hsv(rgb);

    hsv.x += shift.x;
    if (1.0 <= hsv.x)
    {
        hsv.x -= 1.0;
    }

    hsv.x = lerp(hsv.x, 0.0, weight);
    hsv.y *= shift.y;
    hsv.z *= shift.z;

    return hsv2rgb(hsv);
}

inline float3 ShiftTangent(float3 T, float3 N, float shift)
{
    float3 shiftedT = T + shift * N;
    return normalize(shiftedT);
}

inline float StrandSpecular(float3 T, float3 V, float3 L, float exponent, float strength)
{
    float3 H = normalize(L + V);
    float dotTH = dot(T, H);
    float sinTH = sqrt(1.0 - dotTH * dotTH);
    float dirAtten = smoothstep(-1.0, 0.0, dotTH);
    return dirAtten * pow(sinTH, exponent) * strength;
}

#endif

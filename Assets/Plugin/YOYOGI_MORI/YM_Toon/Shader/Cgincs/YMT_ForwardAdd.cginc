#include "YMT_Vert.cginc"
#include "YMT_Frag.cginc"

YMT_VertexOutput vert(YMT_VertexInput v)
{
    return CalcVertForward(v);
}

half4 frag(YMT_VertexOutput i) : SV_Target
{
    return fragForward(i);
}

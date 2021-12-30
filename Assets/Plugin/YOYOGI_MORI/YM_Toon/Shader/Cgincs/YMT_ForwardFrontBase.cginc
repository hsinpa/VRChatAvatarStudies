#include "YMT_Vert.cginc"
#include "YMT_Frag.cginc"

YMT_VertexOutput vert(YMT_VertexInput v)
{
    v.isCullBack = 0;
    return CalcVertForward(v);
}

half4 frag(YMT_VertexOutput i) : SV_Target
{
    return fragForward(i);
}

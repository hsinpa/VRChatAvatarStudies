Shader "Hsinpa/TelevisionShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        

        _UVOffset("UV Offset", Vector) = (0,0,0,0)
        _UVScale("UV Scale", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "include/Filter_TVTube.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;


            float2 _UVOffset;
            float2 _UVScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 ar_uv = i.uv;

                float aspectRatio = _MainTex_TexelSize.z / _MainTex_TexelSize.w;
                ar_uv.x *= aspectRatio;

                // sample the texture
                fixed4 col = tex2D(_MainTex, (ar_uv * _UVScale) + _UVOffset);

                return GetTVTubeFilter(_MainTex, i.uv, ar_uv, _MainTex_TexelSize);

                return col;
            }
            ENDCG
        }
    }
}

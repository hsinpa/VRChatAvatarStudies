Shader "Hsinpa/StencilVisible"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Tint", Color) = (0,0,0,1)
        _Smoothness("Smoothness", Range(0, 1)) = 0
        _Metallic("Metalness", Range(0, 1)) = 0
        [HDR] _Emission("Emission", Color) = (0,0,0)

        [IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}
        LOD 100

        Pass
        {

            Stencil {
                Ref [_StencilRef]
                Comp Equal
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}

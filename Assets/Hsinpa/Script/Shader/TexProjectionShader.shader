Shader "Hsinpa/TexProjectionShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 projectedTexcoord  : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            uniform sampler2D _ProjectTex;
            float4 _ProjectTex_ST;

            fixed4 _Color;

            uniform float4x4 _TextureProjectMatrix;

            v2f vert (appdata v)
            {
                fixed4 worldPosition = mul(unity_ObjectToWorld, v.vertex);


                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.projectedTexcoord = mul(_TextureProjectMatrix, worldPosition);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 projectedTexcoord = i.projectedTexcoord.xyz / i.projectedTexcoord.w;

                float4 projectedTexColor = tex2D(_ProjectTex, projectedTexcoord.xy);


                bool inRange =
                    projectedTexcoord.x >= 0.0 &&
                    projectedTexcoord.x <= 1.0 &&
                    projectedTexcoord.y >= 0.0 &&
                    projectedTexcoord.y <= 1.0 &&
                    projectedTexColor.a > 0;

                float projectedAmount = inRange ? 1.0 : 0.0;
                projectedTexColor = projectedTexColor * _Color;
                float4 col = lerp(_Color, projectedTexColor, projectedAmount);

                return col;
            }
            ENDCG
        }

        //Pass
        //{
        //    Tags{ "LightMode" = "ShadowCaster" }
        //    CGPROGRAM
        //    #pragma vertex VSMain
        //    #pragma fragment PSMain

        //    float4 VSMain(float4 vertex:POSITION) : SV_POSITION
        //    {
        //        return UnityObjectToClipPos(vertex);
        //    }

        //    float4 PSMain(float4 vertex:SV_POSITION) : SV_TARGET
        //    {
        //        return 0;
        //    }

        //    ENDCG
        //}
    }
}

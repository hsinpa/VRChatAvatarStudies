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

        Tags { "RenderType"="Opaque"}
        LOD 100

        Cull Back

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

                fixed4 tvtube_col = GetTVTubeFilter(col, i.uv, ar_uv);


                //return GetTVTubeFilter(_MainTex, i.uv, ar_uv, _MainTex_TexelSize);

                return tvtube_col;
            }
                ENDCG
        }

        //Pass
        //{
        //    Tags {"LightMode" = "Meta"}

        //    CGPROGRAM

        //    #include "UnityStandardMeta.cginc"
        //    #pragma vertex vert_meta
        //    #pragma fragment frag_meta_custom

        //    fixed4 frag_meta_custom(v2f_meta i) : SV_Target
        //    {
        //        fixed4 col = tex2D(_MainTex, i.uv) * 10.0;

        //        // Colors                
        //        //fixed4 col = fixed4(2,0,0,1); // The emission color

        //        // Calculate emission
        //        UnityMetaInput metaIN;
        //        UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
        //        metaIN.Albedo = col.rgb*0;
        //        metaIN.Emission = col.rgb;
        //        return UnityMetaFragment(metaIN);
        //    }

        //    ENDCG
        //}
    }
}

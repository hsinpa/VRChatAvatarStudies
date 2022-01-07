//Filter_TVTube.cginc
//Modify from https://www.shadertoy.com/view/Xtf3zX

#ifndef FILTER_TVTUBE
#define FILTER_TVTUBE

	float _Vignette_Strength;
	float _Vignette_Specular;

	float CalVignette(float2 center_ar_uv, float distance_to_center) {
	    float vignette = (1.0 - abs(center_ar_uv.x)) * (1.0 - abs(center_ar_uv.y)) / (1.0 + distance_to_center);
	    vignette *= vignette * _Vignette_Specular;
	    vignette *= max(0.0, 1.0 - 2.75 * max(abs(center_ar_uv.x), abs(center_ar_uv.y)));
	    vignette = pow(vignette, _Vignette_Specular);
	    
	    return vignette;
	}

	float4 GetTVTubeFilter(sampler2D _MainTex, float2 uv, float2 ar_uv, float4 texel_size) {

		float2 center_uv = uv - 0.5;
		float2 center_ar_uv = ar_uv	 - 0.5;

	    // measure distance from center
	    float dd = dot(center_uv, center_uv);
	    float dd2 = dot(center_ar_uv, center_ar_uv);

	    // warp
	    center_uv = (center_uv * dd) * 0.4 + center_uv * 0.6;
	    center_ar_uv = (center_ar_uv * dd2) * 0.4 + center_ar_uv * 0.6;

		return float4(1, 0, 0, 1);
	}

#endif
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

	float4 GetTVTubeFilter(fixed4 color, float2 uv, float2 ar_uv) {
		float2 filter_uv = uv - 0.5;
		float2 filter_ar_uv = ar_uv	 - 0.5;

	    // measure distance from center
	    float dd = dot(filter_uv, filter_uv);
	    float dd2 = dot(filter_ar_uv, filter_ar_uv);

	    // warp
		filter_uv = (filter_uv * dd) * 0.4 + filter_uv * 0.6;
		filter_ar_uv = (filter_ar_uv * dd2) * 0.4 + filter_ar_uv * 0.6;

		float vignette = CalVignette(filter_ar_uv, dd2);

		//Restore
		filter_uv += 0.5;
		filter_ar_uv += 0.5;

		//Apply Vertical Scanliness
		float v = abs(sin(filter_uv.x * 270.0 + _Time.w));
			v += abs(sin(filter_uv.x * 380.0 + _Time.y * 1.1));
			v += abs(sin(filter_uv.x * 300.0 + _Time.y * 1.8));
			v = lerp(v, 0.5, 0.9) - 0.1;

		if (v > 0.5) 
		{
			color = 1.0 - (1.0 - 2.0 * (v - 0.5)) * (1.0 - color);
		}
		else 
		{
			color = (2.0 * v) * color;
		}

		return color;
	}

#endif
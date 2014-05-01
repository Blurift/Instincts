Shader "2D Terrain/Basic" {
	Properties {
		_Texture1 ("Texture 1", 2D) = "white" {}
		_Tex1Scale("Tex 1 Scale",float) = 1.0
		_Texture2 ("Texture 2", 2D) = "white" {}
		_Tex2Scale("Tex 2 Scale",float) = 1.0
		_Texture3 ("Texture 3", 2D) = "white" {}
		_Tex3Scale("Tex 3 Scale",float) = 1.0
		_Splat ("Splat Map (RGB)", 2D) = "white" {}
		_SplatScale("Splat Scale",float) = 1.0
	}
	SubShader {
		Tags
		{
			"RenderType"="Opaque"
		}
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert
		#include "UnityCG.cginc"

		sampler2D _Texture1;
		sampler2D _Texture2;
		sampler2D _Texture3;
		sampler2D _Splat;
		float _Tex1Scale;
		float _Tex2Scale;
		float _Tex3Scale;

		float _SplatScale;

		

		struct Input {
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutput o) {
		
			float2 UV = IN.worldPos.xy*0.1;
			
			
			half4 tex1 = tex2D (_Texture1, UV*_Tex1Scale - floor(UV));
			half4 tex2 = tex2D (_Texture2, UV*_Tex2Scale - floor(UV));
			half4 tex3 = tex2D (_Texture3, UV*_Tex3Scale - floor(UV));
			
			half4 splat = tex2D (_Splat, UV * _SplatScale);
			
			float4 f1 = tex1 * splat.r;
			float4 f2 = tex2 * splat.g;
			float4 f3 = tex3 * splat.b;

			
			float3 c = saturate(f1+f2+f3).rgb;
			
			o.Albedo = c.rgb;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

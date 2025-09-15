Shader "Custom/WorldPositionShader" {
	Properties {
		_MainTex ("Texture (RGBA)", 2D) = "white" {}
		_Color ("Tint Color", Vector) = (1,1,1,1)
		_TexTiling ("Texture Tiling", Vector) = (1,1,0,0)
		_TexOffset ("Texture Offset", Vector) = (0,0,0,0)
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_NormalStrength ("Normal Strength", Range(0, 2)) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}
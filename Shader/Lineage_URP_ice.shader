Shader "Lineage/URP/ice" {
	Properties {
		[Header(Texture)] [Enum(Off, 0, On, 1)] _Zwrite ("Zwrite", Float) = 0
		_MainTex ("RGB(diffuse)),A:(OA))", 2D) = "white" {}
		[Normal] _Normal ("normal", 2D) = "bump" {}
		_SpecTex ("RGB(SpecTex),A(SpecTex_pow)", 2D) = "white" {}
		_EmiTex ("RGB:emisstion", 2D) = "black" {}
		_Cubmap ("Cubmap", Cube) = "_skybox" {}
		[Header(Diffuse)] [HDR] _MainCol ("diffuseColor", Vector) = (1,1,1,1)
		_EnvDiffInt ("EnvDiffInt", Range(0, 1)) = 0.2
		_EnvUpCol ("topColor", Vector) = (1,1,1,1)
		_EnvSideCol ("SideColor", Vector) = (0.5,0.5,0.5,1)
		_EnvDownCol ("DownColor", Vector) = (1,1,1,1)
		[Header(specular)] [PowerSlider(0.5)] _SpecPow ("SpecPow", Range(1, 200)) = 10
		_EnvSpecInt ("EnvSpecInt", Range(0, 5)) = 1
		_FresnelPow ("FresnelPow", Range(0, 10)) = 1
		_CubemapMip ("CubmapMip", Range(1, 7)) = 0
		[Header(Emission)] [HDR] _EmiCol ("emission", Vector) = (1,1,1,1)
		_EmitInt ("emissionIntensity", Range(1, 10)) = 1
		_mask ("mask", 2D) = "white" {}
		_NormalScale ("normalScale", Range(0, 1.5)) = 1
		_Light ("LightPosition", Vector) = (0,0,0,0)
		_distor ("distorIntensity ", Range(0, 1)) = 0
		[Header(Transparency)] _OverallTransparency ("Overall Transparency", Range(0, 1)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}
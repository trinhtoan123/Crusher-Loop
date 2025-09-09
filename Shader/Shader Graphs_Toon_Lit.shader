Shader "Shader Graphs/Toon_Lit" {
	Properties {
		[NoScaleOffset] _MainTexture ("Main texture", 2D) = "white" {}
		_Light_Direction ("Light direction", Vector) = (0.5,0.5,0,1)
		_Tint ("Tint", Vector) = (1,1,1,1)
		[NoScaleOffset] _DetailAlbedo ("DetailAlbedo", 2D) = "white" {}
		[ToggleUI] _Albedo ("Albedo", Float) = 0
		_RampMin ("RampMin", Vector) = (0,0,0,1)
		_RampMax ("RampMax", Vector) = (1,1,1,1)
		Vector1_1CE4BCFA ("ShadowIntensity", Range(0, 1)) = 0
		[ToggleUI] _Shadows ("Shadows", Float) = 1
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_Occlusion ("Occlusion", Range(0, 1)) = 0
		[NoScaleOffset] _ToonSubShader_0c1a82ee69d7128092ae22416791fc25_Texture2DCB2794E1_106110312_Texture2D ("Texture2D", 2D) = "white" {}
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}
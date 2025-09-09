Shader "Shader Graphs/URP Dissolve/Dissolve_Metallic" {
	Properties {
		_BaseColor ("BaseColor", Vector) = (1,1,1,1)
		[NoScaleOffset] _BaseMap ("BaseMap", 2D) = "white" {}
		[NoScaleOffset] [Normal] _NormalMap ("NormalMap", 2D) = "bump" {}
		_NormalScale ("NormalScale", Float) = 1
		[NoScaleOffset] _R_Metallic_G_Occulsion_A_Smoothness ("R_Metallic,G_Occulsion,A_Smoothness", 2D) = "white" {}
		_Metallic ("Metallic", Range(0, 1)) = 0
		_OcclusionStrength ("OcclusionStrength", Range(0, 1)) = 1
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_Tiling ("Tiling", Vector) = (1,1,0,0)
		_Offest ("Offest", Vector) = (0,0,0,0)
		_Dissolve ("Dissolve", Range(0, 1)) = 0.5
		_NoiseScale ("NoiseScale", Float) = 50
		_NoiseUVSpeed ("NoiseUVSpeed", Vector) = (0,0,0,0)
		_EdgeWidth ("EdgeWidth", Range(0, 1)) = 0.05
		[HDR] _EdgeColor ("EdgeColor", Vector) = (0,3.890196,4,0)
		_EdgeColorIntensity ("EdgeColorIntensity", Float) = 1
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
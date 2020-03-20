
Shader "JKX1/Show/CharacterSkinLow"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_AmbientColor("AmbientColor", Color) = (0.09,0.09,0.09)
		_MainTex("Albedo", 2D) = "white" {}
	    _sat("sat",Range(-3, 3)) = 0
        //_Metallic("Metallic", Range(0.001, 0.999)) = 0.001
		_MetallicLow("MetallicLow", Range(0.001, 0.999)) = 0.001
		_Glossiness("Smoothness", Range(0.001, 0.999)) = 0.5
		_R_ME_G_SM_B_SKIN("R_ME_G_SM_B_SKIN", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_EmissionMap_Low("(RGB)Specular(A)Gloss_Low", 2D) = "black" {}
		_BRDFTex("BRDF Lookup (RGB)", 2D) = "gray" {}
		_MainTexLOD2("AlbedoLOD2", 2D) = "white" {}
		_AlbedoMultiLOD2("AlbedoMultiLOD2", Float) = 1

		[HideInInspector]_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
	    // Blending state
		[HideInInspector] _Mode("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" "PerformanceChecks" = "False" }
		LOD 400

		CGINCLUDE
		#define UNITY_SETUP_BRDF_INPUT MetallicSetup
		

		sampler2D _BRDFTex; half4 _BRDFTex_ST; 


		ENDCG
		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }

			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]

			CGPROGRAM
            #pragma target 3.0
			#pragma skip_variants LIGHTPROBE_SH LIGHTMAP_ON DYNAMICLIGHTMAP_ON VERTEXLIGHT_ON FOG_EXP FOG_EXP2
			#pragma multi_compile_instancing
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma vertex vertBase
			#pragma fragment fragBase
			#define _NORMALMAP 1
		    #define _SKINLOW 1
			#include "cgincs/TT_UnityStandardCoreForward.cginc"

			ENDCG
		}

		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vertShadowCaster
			#pragma fragment fragShadowCaster

			#include "cgincs/TT_UnityStandardShadow.cginc"

			ENDCG
		}
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" "PerformanceChecks" = "False" }
		LOD 200


		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
	{
		Name "FORWARD"
		Tags{ "LightMode" = "ForwardBase" }

		Blend[_SrcBlend][_DstBlend]
		ZWrite[_ZWrite]

		CGPROGRAM
#pragma target 3.0
#pragma skip_variants LIGHTPROBE_SH LIGHTMAP_ON DYNAMICLIGHTMAP_ON VERTEXLIGHT_ON FOG_EXP FOG_EXP2
#pragma multi_compile_fwdbase
#pragma multi_compile_fog
#pragma vertex vertBase
#pragma fragment fragBase
#define _SKINLOW 1
#include "cgincs/TT_UnityStandardCoreForward.cginc"

		ENDCG
	}
	}

//	SubShader
//	{
//		Tags{ "RenderType" = "Opaque" "PerformanceChecks" = "False" }
//		LOD 200
//
//		// ------------------------------------------------------------------
//		//  Base forward pass (directional light, emission, lightmaps, ...)
//		Pass
//	{
//		Name "FORWARD"
//		Tags{ "LightMode" = "ForwardBase" }
//
//		Blend[_SrcBlend][_DstBlend]
//		ZWrite[_ZWrite]
//
//		CGPROGRAM
//#pragma target 3.0
//#pragma skip_variants LIGHTPROBE_SH LIGHTMAP_ON DYNAMICLIGHTMAP_ON VERTEXLIGHT_ON FOG_EXP FOG_EXP2
//#pragma multi_compile_fwdbase
//#pragma multi_compile_fog
//#pragma vertex vertBase
//#pragma fragment fragBase
//#define _SKINLOWLOD2 1
//#include "TT_UnityStandardCoreForward.cginc"
//
//		ENDCG
//	}
//	}
//
	FallBack "JKX1/Fallback/UnlitTexture"
}

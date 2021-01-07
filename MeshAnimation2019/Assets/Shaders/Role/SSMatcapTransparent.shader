//--------------------------------------------------------------
// Desc: 角色shader(matcap)
// Author: yaoxin
// Date: 2019-08-20
// Copyright: xingchen
//---------------------------------------------------------------
Shader "StarStudio/Role/SSMatcap(Transparent)"
{
	Properties 
	{
        [Header(Cull)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
        
		[Space]_MainTex ("主贴图", 2D) = "white" {}
        [NoScaleOffset] _MatcapTex ("Matcap贴图", 2D) = "white" {}
        _MatcapIntensity("Matcap Intensity", Range(0, 10)) = 0
        
        _Transparency ("不透明度", Range(0, 1.0)) = 1.0   
        
        [Enum(Off, 0, On, 1)]
        _ZWrite("ZWrite", Float) = 1            // Off
	}

	CGINCLUDE
    
    #define TRANSPARENCY
    #define MATCAP

	ENDCG

	SubShader 
	{
		Tags 
		{ 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
		}
        
        Cull [_Cull]

		// 基本光照pass
		Pass
		{
			Tags 
			{ 
				"LightMode" = "ForwardBase"
			}
            
            Blend SrcAlpha OneMinusSrcAlpha            
            ZWrite [_ZWrite]

			CGPROGRAM

			#define FORWARD_BASE_PASS
			#pragma multi_compile_fog

			// 默认处理Linear雾效, 如果要关闭或者改变雾效, 修改下行
			#pragma skip_variants FOG_EXP FOG_EXP2

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "SSCharacter.cginc"
			
			ENDCG
		}

		//// shadow caster pass
		//Pass 
		//{
		//	Tags 
		//	{
		//		"LightMode" = "ShadowCaster"
		//	}

		//	CGPROGRAM

		//	#include "UnityCG.cginc"
  //          #include "SSCharacter.cginc"

		//	#pragma vertex vert_shadow_caster
		//	#pragma fragment frag_shadow_caster

		//	ENDCG
		//}
	}
}



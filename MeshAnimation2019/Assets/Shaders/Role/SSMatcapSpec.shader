//--------------------------------------------------------------
// Desc: 角色shader(matcap spec)
// Author: yaoxin
// Date: 2019-08-20
// Copyright: xingchen
//---------------------------------------------------------------
Shader "StarStudio/Role/SSMatcap(Spec)"
{
	Properties 
	{
        [Header(Cull)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
        
		[Space]_MainTex ("主贴图", 2D) = "white" {}
        [NoScaleOffset] _MatcapTex ("Matcap贴图", 2D) = "white" {}
        _MatcapIntensity("Matcap Intensity", Range(0, 10)) = 4
        
        [Header(Spec)]
        [NoScaleOffset] _MatcapSpecTex ("高光贴图", 2D) = "white" {}
        _SpecCol ("高光颜色", Color) = (1, 1, 1, 0.5)
        _SpecPower ("高光锐度", Range(0, 2)) = 0.4
        _MatcapSpecDir ("高光方向", Vector) = (1, 1, -1, 0)
	}

	CGINCLUDE
    
    #define MATCAP
    #define MATCAP_SPEC

	ENDCG

	SubShader 
	{
		Tags 
		{ 
            "Queue" = "AlphaTest+20"        // SSPlayerXray的渲染顺序为AlphaTest+10, 普通角色放在Xray角色之后渲染(不产生Xray遮挡)
			"RenderType" = "Opaque" 
		}
        
        Cull [_Cull]

		// 基本光照pass
		Pass
		{
			Tags 
			{ 
				"LightMode" = "ForwardBase"
			}

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



//--------------------------------------------------------------
// Desc: 角色shader(matcap)
// Author: yaoxin
// Date: 2019-08-20
// Copyright: xingchen
//---------------------------------------------------------------
Shader "StarStudio/Role/SSMatcap"
{
	Properties 
	{
        [Header(Cull)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
        
		[Space]_MainTex ("主贴图", 2D) = "white" {}
        [NoScaleOffset] _MatcapTex ("Matcap贴图", 2D) = "white" {}
        _MatcapIntensity("Matcap Intensity", Range(0, 10)) = 4
	}

	CGINCLUDE
    
    #define MATCAP

	ENDCG

	SubShader 
	{
		Tags 
		{ 
            "Queue" = "AlphaTest+20"        // SSPlayerXray的渲染顺序为AlphaTest+10, 普通角色放在Xray角色之后渲染(不产生Xray遮挡)
			"RenderType" = "Opaque" 
		}
        
        // 写入模板缓存第1位(解决透明Xray被遮挡的问题)
        Stencil 
        {
            Ref 1
            Comp Always
            Pass Replace
            WriteMask 1
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



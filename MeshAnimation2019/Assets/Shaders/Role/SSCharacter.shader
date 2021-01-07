//--------------------------------------------------------------
// Desc: 角色shader(法线贴图+高光贴图+自发光贴图+边缘光+漫反射Ramp+流光, 支持多光源, 点光源, 顶点光源, sh光照, 雾效, 阴影功能)
// Author: yaoxin
// Date: 2019-01-23
// Copyright: xingchen
//---------------------------------------------------------------
// ref(法线贴图):https://catlikecoding.com/unity/tutorials/rendering/part-6
// ref(PBR):https://zhuanlan.zhihu.com/p/34196915
Shader "StarStudio/Role/SSCharacter"
{
	Properties 
	{
        [Header(Cull)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
        
		[Space]_MainTex ("主贴图", 2D) = "white" {}
		[NoScaleOffset] _MaskTex ("遮罩贴图", 2D) = "white" {}
		[NoScaleOffset] _FlowRampTex ("流光动画/Ramp贴图(r:环境流光 g:流光1 b:流光2 a:ramp)", 2D) = "white" {}

		[HideInInspector]_DiffuseIntensity("贴图明暗调节", Range(0, 4)) = 2

		[Header(Specular)]
		_SpecCol ("基本高光颜色", Color) = (1, 1, 1, 0.5)
 		_SpecPower ("基本高光锐度", Range(0, 2)) = 0.4

 		[Header(Emissive)]
		_EmissiveCol ("自发光颜色", Color) = (1, 1, 1, 0.5)

 		[Header(Rim)]
		_RimColor("Rim Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_RimPower("Rim Power", Range(0.01, 10.0)) = 3.0
        _RimIntensity("Rim Intensity", Range(0, 100)) = 1
        [HideInInspector]_RimDir("Rim Dir", Range(0.5, 1)) = 0.75

		[Header(Ramp)]
		_RampDarkCol ("暗面颜色", Color) = (0, 0, 0, 1)
		_RampLightCol ("亮面颜色", Color) = (1, 1, 1, 1)
	}

	CGINCLUDE
    
    #define DIFFUSE
    #define SPECULAR
	#define SPECTEX
	#define EMISSIVETEX
	#define RIM
    #define RIM_DIR         // 单侧轮廓光
	#define RAMP

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

			// 如果要开启顶点光源和sh光照, 取消下行的注释
//			#pragma multi_compile _ VERTEXLIGHT_ON

			// 如果要开启接受阴影, 取消下行注释
//			#pragma multi_compile _ SHADOWS_SCREEN

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "SSCharacter.cginc"
			
			ENDCG
		}

		// 多光源pass
		Pass 
		{
			Tags 
			{
				"LightMode" = "ForwardAdd"
			}
            
			ZWrite Off
			Blend One One

			CGPROGRAM
            
            #define FORWARD_ADD_PASS
			#pragma multi_compile_fog
			#pragma multi_compile DIRECTIONAL POINT

			// 默认处理Linear雾效, 如果要关闭或者改变雾效, 修改下行
			#pragma skip_variants FOG_EXP FOG_EXP2

			// 如果要开启实时像素点光源, 注释掉下行
			#pragma skip_variants POINT

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
    
    CustomEditor "SSShaderGUI"
}



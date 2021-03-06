﻿//--------------------------------------------------------------
// Desc: 角色shader(法线贴图+高光贴图+自发光贴图+边缘光+漫反射Ramp+流光, 支持多光源, 点光源, 顶点光源, sh光照, 雾效, 阴影功能)
// Author: yaoxin
// Date: 2019-01-23
// Copyright: xingchen
//---------------------------------------------------------------
// ref(法线贴图):https://catlikecoding.com/unity/tutorials/rendering/part-6
// ref(PBR):https://zhuanlan.zhihu.com/p/34196915
Shader "StarStudio/Role/SSCharacter(CubemapXray)" 
{
    Properties 
    {
        [Header(Cull)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
        
        [Space]_MainTex ("主贴图", 2D) = "white" {}
        [NoScaleOffset] _MaskTex ("遮罩贴图", 2D) = "white" {}
        [NoScaleOffset] _FlowRampTex ("流光动画/Ramp贴图(r:环境流光 g:流光1 b:流光2 a:ramp)", 2D) = "white" {}

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
        
        [Header(Cubemap)]
        _CubeTex ("环境反射贴图", CUBE) = "" { }
        _CubeCol ("环境反射颜色", Color) = (1, 1, 1, 0.3)

        [Header(Xray)]
        _XrayCol ("透视颜色", Color) = (1, 1, 1, 1)
        _XrayPower("透视锐度", Range(0.01, 10.0)) = 2.0
        _XrayIntensity("透视强度", Range(0, 50.0)) = 10.0
    }

    CGINCLUDE
    
    #define DIFFUSE
    #define SPECULAR
    #define SPECTEX
    #define EMISSIVETEX
    #define RIM
    #define RIM_DIR         // 单侧轮廓光
    #define RAMP
    #define CUBEMAP

    ENDCG

    SubShader 
    {
        Tags 
        { 
            "Queue" = "AlphaTest+10"
            "RenderType" = "Opaque" 
        }
        
        Cull [_Cull]

        // 被遮挡部分的xray pass
        Pass
        {
            ZTest Greater
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM

            #pragma vertex vert_xray
            #pragma fragment frag_xray

            #include "UnityCG.cginc"
            #include "SSCharacter.cginc"

            ENDCG
        }

        // 基本光照pass
        Pass
        {
            Tags 
            { 
                "LightMode" = "ForwardBase"
            }
            
            // 非透明Xray的角色渲染写入模板缓存第1位(用来解决透明Xray被非透明Xray遮挡的渲染顺序问题)
            Stencil 
            {
                Ref 1
                Comp Always
                Pass Replace
                WriteMask 1
            }

            CGPROGRAM

            #define FORWARD_BASE_PASS
            #pragma multi_compile_fog

            // 默认处理Linear雾效, 如果要关闭或者改变雾效, 修改下行
            #pragma skip_variants FOG_EXP FOG_EXP2

            // 如果要开启顶点光源和sh光照, 取消下行的注释
            //#pragma multi_compile _ VERTEXLIGHT_ON

            // 如果要开启接受阴影, 取消下行注释
            //#pragma multi_compile _ SHADOWS_SCREEN

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0          // 双面VFACE需要

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
            #pragma target 3.0          // 双面VFACE需要

            #include "UnityCG.cginc"
            #include "SSCharacter.cginc"

            ENDCG
        }

        //// shadow caster pass
        //Pass 
        //{
        //    Tags 
        //    {
        //        "LightMode" = "ShadowCaster"
        //    }

        //    CGPROGRAM

        //    #include "UnityCG.cginc"
        //    #include "SSCharacter.cginc"

        //    #pragma vertex vert_shadow_caster
        //    #pragma fragment frag_shadow_caster
        //    #pragma target 3.0          // 双面VFACE需要

        //    ENDCG
        //}
    }
    
    CustomEditor "SSShaderGUI"
}



Shader "Mesh Animator/Universal Render Pipeline/Unlit"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (1, 1, 1, 1)
        _Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5

        // BlendMode
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("Src", Float) = 1.0
        [HideInInspector] _DstBlend("Dst", Float) = 0.0
        [HideInInspector] _ZWrite("ZWrite", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        [HideInInspector] _SampleGI("SampleGI", float) = 0.0 // needed from bakedlit
		
		// start MeshAnimator
		[PerRendererData] _AnimTimeInfo ("Animation Time Info", Vector) = (0.0, 0.0, 0.0, 0.0)
		[PerRendererData] _AnimTextures ("Animation Textures", 2DArray) = "" {}
		[PerRendererData] _AnimTextureIndex ("Animation Texture Index", float) = -1.0
		[PerRendererData] _AnimInfo ("Animation Info", Vector) = (0.0, 0.0, 0.0, 0.0)
		[PerRendererData] _AnimScalar ("Animation Scalar", Vector) = (1.0, 1.0, 1.0, 0.0)
		[PerRendererData] _CrossfadeAnimTextureIndex ("Crossfade Texture Index", float) = -1.0
		[PerRendererData] _CrossfadeAnimInfo ("Crossfade Animation Info", Vector) = (0.0, 0.0, 0.0, 0.0)
		[PerRendererData] _CrossfadeAnimScalar ("Crossfade Animation Scalar", Vector) = (1.0, 1.0, 1.0, 0.0)
		[PerRendererData] _CrossfadeStartTime ("Crossfade Start Time", float) = -1.0
		[PerRendererData] _CrossfadeEndTime ("Crossfade End Time", float) = -1.0
		// end MeshAnimator
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Blend [_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]
        Cull [_Cull]

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.5

            #pragma vertex MAUnlitVert
            #pragma fragment MAUnlitFrag
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
			
            #include "../MeshAnimator.hlsl"
            #include "MeshAnimator-URP-UnlitInput.hlsl"
            #include "MeshAnimator-URP-UnlitPass.hlsl"
            
            ENDHLSL
        }
        Pass
        {
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex MADepthOnlyVertex
            #pragma fragment MADepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
			
            #include "../MeshAnimator.hlsl"
            #include "MeshAnimator-URP-UnlitInput.hlsl"
            #include "MeshAnimator-URP-DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex MAUniversalVertexMeta
            #pragma fragment MAUniversalFragmentMetaUnlit
			
            #include "../MeshAnimator.hlsl"
            #include "MeshAnimator-URP-UnlitInput.hlsl"
            #include "MeshAnimator-URP-UnlitMetaPass.hlsl"

            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShader"
}

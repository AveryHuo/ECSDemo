#ifndef UNIVERSAL_LIT_META_PASS_INCLUDED
#define UNIVERSAL_LIT_META_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 uv0          : TEXCOORD0;
    float2 uv1          : TEXCOORD1;
    float2 uv2          : TEXCOORD2;
#ifdef _TANGENT_TO_WORLD
    float4 tangentOS     : TANGENT;
#endif
	uint vertexId		: SV_VertexID;
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
};

Varyings MAUniversalVertexMeta(Attributes input)
{
    Varyings output;

	float3 animatedPosition;
	float3 animatedNormal;

	ApplyMeshAnimationValues_float(
		input.positionOS.xyz,
		input.normalOS.xyz,
		UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTimeInfo), 
		_AnimTextures,
		UNITY_ACCESS_INSTANCED_PROP(Props, _AnimTextureIndex), 
		UNITY_ACCESS_INSTANCED_PROP(Props, _AnimInfo),
		UNITY_ACCESS_INSTANCED_PROP(Props, _AnimScalar), 
		UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeAnimTextureIndex), 
		UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeAnimInfo), 
		UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeAnimScalar), 
		UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeStartTime),  
		UNITY_ACCESS_INSTANCED_PROP(Props, _CrossfadeEndTime),  
		input.vertexId,
		sampler_AnimTextures,
		animatedPosition,
		animatedNormal);

	input.positionOS.xyz = animatedPosition;
	input.normalOS.xyz = animatedNormal;

    output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.uv2,
        unity_LightmapST, unity_DynamicLightmapST);
    output.uv = TRANSFORM_TEX(input.uv0, _BaseMap);
    return output;
}

half4 MAUniversalFragmentMeta(Varyings input) : SV_Target
{
    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

    BRDFData brdfData;
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

    MetaInput metaInput;
    metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
    metaInput.SpecularColor = surfaceData.specular;
    metaInput.Emission = surfaceData.emission;

    return MetaFragment(metaInput);
}


//LWRP -> Universal Backwards Compatibility
Varyings MALightweightVertexMeta(Attributes input)
{
    return MAUniversalVertexMeta(input);
}

half4 MALightweightFragmentMeta(Varyings input) : SV_Target
{
    return MAUniversalFragmentMeta(input);
}

#endif

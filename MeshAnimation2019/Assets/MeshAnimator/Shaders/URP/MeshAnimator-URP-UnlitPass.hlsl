#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

struct Attributes
{
    float4 positionOS       : POSITION;
    float2 uv               : TEXCOORD0;
	uint vertexId			: SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv        : TEXCOORD0;
    float fogCoord   : TEXCOORD1;
    float4 vertex    : SV_POSITION;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings MAUnlitVert(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	float3 animatedPosition;
	float3 animatedNormal;

	ApplyMeshAnimationValues_float(
		input.positionOS.xyz,
		float3(0,0,0),
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

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.vertex = vertexInput.positionCS;
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

    return output;
}

half4 MAUnlitFrag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half2 uv = input.uv;
    half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    half3 color = texColor.rgb * _BaseColor.rgb;
    half alpha = texColor.a * _BaseColor.a;
    AlphaDiscard(alpha, _Cutoff);

#ifdef _ALPHAPREMULTIPLY_ON
    color *= alpha;
#endif

    color = MixFog(color, input.fogCoord);

    return half4(color, alpha);
}
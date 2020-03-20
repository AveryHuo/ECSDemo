// Upgrade NOTE: replaced 'defined _NORMALMETALLICGLOSSMAP' with 'defined (_NORMALMETALLICGLOSSMAP)'

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_INPUT_INCLUDED
#define UNITY_STANDARD_INPUT_INCLUDED

// custom input
UNITY_DECLARE_TEXCUBE(_SpecCube);

#include "TT_UnityCG.cginc"
#include "TT_UnityStandardConfig.cginc"
#include "TT_UnityGlobalIllumination.cginc"
#include "TT_UnityStandardUtils.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP)
    #define _TANGENT_TO_WORLD 1
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
    #define// _DETAIL 1
#endif

#if UVSET
#define _UV1 1
#endif

//---------------------------------------
half4       _Color;
half4       _AmbientColor;
half        _Cutoff;
half        _sat;
sampler2D   _MainTex;
half4      _MainTex_ST;
half        _Specstr;
half        _SPstr;
half        _GLstr;
half        _Flatstr;
half        _AlbedoMultiLOD2;
sampler2D   _R_ME_G_SM_B_SKIN; half4 _R_ME_G_SM_B_SKIN_ST;
sampler2D   _MainTexLOD2; half4 _MainTexLOD2_ST;
sampler2D   _DetailAlbedoMap;
float4      _DetailAlbedoMap_ST;
sampler2D   _BumpMap;
//half        _BumpScale;

sampler2D   _DetailMask;
sampler2D   _DetailNormalMap;
half        _DetailNormalMapScale;

sampler2D   _SpecGlossMap;
sampler2D   _MetallicGlossMap;
half        _Metallic;
half        _MetallicLow;
float       _Glossiness;

sampler2D   _EmissionMap; half4 _EmissionMap_ST;
sampler2D   _EmissionMap_Low; half4 _EmissionMap_Low_ST;
half3       _Vector0;
half        _rimBias;
half        _rimScale;
half        _rimPower;
half4       _rimColor;
sampler2D   _maskTex; half4 _maskTex_ST;
//-------------------------------------------------------------------------------------
// Input functions

struct VertexInput
{
    float4 vertex   : POSITION;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
    float2 uv2      : TEXCOORD2;
#endif
#ifdef _TANGENT_TO_WORLD
    half4 tangent   : TANGENT;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 TexCoords(VertexInput v)
{
    float4 texcoord;
	#if _UV1
    texcoord.xy = TRANSFORM_TEX(v.uv1, _MainTex); 
    texcoord.zw = TRANSFORM_TEX(v.uv1, _MainTex);

    #else 
	texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex);
	texcoord.zw = TRANSFORM_TEX(v.uv0, _MainTex);
	#endif
    return texcoord;
}

half DetailMask(float2 uv)
{
    return tex2D (_DetailMask, uv).a;
}

half3 Albedo(float4 texcoords)
{
	#if _SKINLOWLOD2
    half3 albedo = _AlbedoMultiLOD2*tex2D (_MainTexLOD2, texcoords.xy).rgb;
	//float desaturateDot = dot(albedo, float3(0.299, 0.587, 0.114));

	#else
	half3 albedo = _Color.rgb * 2 * tex2D(_MainTex, texcoords.xy).rgb;
	float desaturateDot = dot(albedo, float3(0.299, 0.587, 0.114));
	#endif 
	//float3 desaturateVar = lerp(albedo, desaturateDot.xxx, _sat);
#if _SKINLOW
	float3 desaturateVar = lerp(albedo, desaturateDot.xxx, _sat+0.1+tex2D(_R_ME_G_SM_B_SKIN, texcoords.xy).r*0.3);
	albedo = desaturateVar*1.1- tex2D(_R_ME_G_SM_B_SKIN, texcoords.xy).r*_MetallicLow+ tex2D(_R_ME_G_SM_B_SKIN, texcoords.xy).g*(1-_Glossiness);

#elif _SKINLOWGRAY
	float3 desaturateVar = desaturateDot.xxx;
	albedo = desaturateVar;
#elif _SKINLOWGRAYLOD2
	
	float3 desaturateVar = desaturateDot.xxx;
	albedo = desaturateVar;
#elif _SKINLOWLOD2
	albedo = albedo;
#else
	float3 desaturateVar = lerp(albedo, desaturateDot.xxx, _sat);
	albedo = desaturateVar;
#endif 
	//clip(tex2D(_MainTex, texcoords.xy).a - _Cutoff);
#if _DETAIL
    #if (SHADER_TARGET < 30)
        // SM20: instruction count limitation
        // SM20: no detail mask
        half mask = 1;
    #else
        half mask = DetailMask(texcoords.xy);
    #endif
    half3 detailAlbedo = tex2D (_DetailAlbedoMap, texcoords.zw).rgb;
    #if _DETAIL_MULX2
        albedo *= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);
    #elif _DETAIL_MUL
        albedo *= LerpWhiteTo (detailAlbedo, mask);
    #elif _DETAIL_ADD
        albedo += detailAlbedo * mask;
    #elif _DETAIL_LERP
        albedo = lerp (albedo, detailAlbedo, mask);
    #endif
#endif
    return albedo;
}

half Alpha(float2 uv)
{
#if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
    return _Color.a;
#else
    return tex2D(_MainTex, uv).a * _Color.a;
#endif
}

//half Occlusion(float2 uv)
//{
//#if (SHADER_TARGET < 30)
//    // SM20: instruction count limitation
//    // SM20: simpler occlusion
//    return tex2D(_OcclusionMap, uv).g;
//#else
//    half occ = tex2D(_OcclusionMap, uv).g;
//    return LerpOneTo (occ, _OcclusionStrength);
//#endif
//}

half4 SpecularGloss(float2 uv)
{
    half4 sg;
#ifdef _SPECGLOSSMAP
    #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
        sg.rgb = tex2D(_SpecGlossMap, uv).rgb;
        sg.a = tex2D(_MainTex, uv).a;
    #else
        sg = tex2D(_SpecGlossMap, uv);
    #endif
    
#else
    sg.rgb = _SpecColor.rgb;
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        sg.a = tex2D(_MainTex, uv).a;
    #else
        sg.a = _Glossiness;
    #endif
#endif
    return sg;
}

half2 MetallicGloss(float2 uv)
{
    half2 mg;

//#ifdef _METALLICGLOSSMAP
//    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
//        mg.r = tex2D(_MetallicGlossMap, uv).r;
//        mg.g = tex2D(_MainTex, uv).a;
//    #else
        //mg = tex2D(_MetallicGlossMap, uv).ra;
//    #endif
//    
//#elif defined (_NORMALMETALLICGLOSSMAP)
//    mg = tex2DN(_Normal, uv.xy);
//    
//#elif defined (_ROUGHNESSAOMETALLICMAP)
//	mg = tex2D(_RoughnessAOMetallic, uv).br;
//	mg.g = 1 - mg.g;
//	
//#else
#ifdef _SKINLOWGRAY
	mg = half2(0,0);
#else
    mg.r = _Metallic*tex2D(_R_ME_G_SM_B_SKIN, uv).r;
    /*#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        mg.g = tex2D(_MainTex, uv).a;
    #else*/
        mg.g = _Glossiness*tex2D(_R_ME_G_SM_B_SKIN, uv).g;
#endif
   /* #endif*/
//#endif
    return mg;
}

half2 MetallicRough(float2 uv)
{
    half2 mg;
#ifdef _METALLICGLOSSMAP
    mg.r = tex2D(_MetallicGlossMap, uv).r;
#else
    mg.r = _Metallic;
#endif

#ifdef _SPECGLOSSMAP
    mg.g = 1.0f - tex2D(_SpecGlossMap, uv).r;
#else
    mg.g = 1.0f - _Glossiness;
#endif
    return mg;
}

//half3 Emission(float2 uv)
//{
//#ifndef _EMISSION
//    return 0;
//#else
//    return tex2D(_EmissionMap, uv).rgb;
//#endif
//}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords, out half3 normalTangent_flake)
{
	//half4 packedNormal = tex2D(_BumpMap, texcoords.xy);
	//half3 normalTangent = 0;
	//normalTangent.xy = packedNormal.ag * 2 - 1;
	// normalTangent.z = sqrt(1.0 - saturate(dot(normalTangent.xy, normalTangent.xy)));
	half3 normalTangent = UnpackNormal(tex2D (_BumpMap, texcoords.xy));    

//#if _DETAIL && defined(UNITY_ENABLE_DETAIL_NORMALMAP)
//    half mask = DetailMask(texcoords.xy);
//    half3 detailNormalTangent = UnpackScaleNormal(tex2D (_DetailNormalMap, texcoords.zw), _DetailNormalMapScale);
//    #if _DETAIL_LERP
//        normalTangent = lerp(
//            normalTangent,
//            detailNormalTangent,
//            mask);
//    #else
//        normalTangent = lerp(
//            normalTangent,
//            BlendNormals(normalTangent, detailNormalTangent),
//            mask);
//    #endif
//#endif

    normalTangent_flake = normalTangent;

//#if _FLAKENORMAL
//    // Apply scaled flake normal map
//	float2 scaledUV = texcoords.xy * _FlakesBumpMapScale;
//	half3 flakeNormal = UnpackNormal(tex2D (_FlakesBumpMap, scaledUV));
//
//	// Apply flake map strength
//	half3 scaledFlakeNormal = flakeNormal;
//	scaledFlakeNormal.xy *= _FlakesBumpStrength;
//	scaledFlakeNormal.z = 0; // Z set to 0 for better blending with other normal map.
//
//	// Blend regular normal map with flakes normal map
//	normalTangent_flake = normalize(normalTangent + scaledFlakeNormal);
//#endif

    return normalTangent;
}
#endif

//float4 Parallax (float4 texcoords, half3 viewDir)
//{
//#if !defined(_PARALLAXMAP) || (SHADER_TARGET < 30)
//    // Disable parallax on pre-SM3.0 shader target models
//    return texcoords;
//#else
//    half h = tex2D (_ParallaxMap, texcoords.xy).g;
//    float2 offset = ParallaxOffset1Step (h, _Parallax, viewDir);
//    return float4(texcoords.xy + offset, texcoords.zw + offset);
//#endif
//
//}

#endif  

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef SSCHARACTER_INCLUDED
#define SSCHARACTER_INCLUDED

#include "AutoLight.cginc"
#include "MeshAnimator.cginc"
sampler2D _MainTex;
float4 _MainTex_ST;

#if defined(DIFFUSE) || defined(SPECULAR) || defined(FLOW) || defined(RIM) || defined(CUBEMAP)
    #define NEED_NORMAL
#endif

#if defined(FLOW) || defined(STRAIGHT_SPEC) || defined(MATCAP) || defined(MATCAP_ENV)
    #define NEED_VIEW_NORMAL
#endif

#if defined(SPECULAR) || defined(MATCAP_SPEC)
    fixed4 _SpecCol;
    float _SpecPower;
#endif

#if defined(SPECTEX) || defined(EMISSIVETEX)
    sampler2D _MaskTex;
#endif

#if defined(NORMALTEX) || defined(MATCAP_NORMALTEX)
    sampler2D _NormalTex;
#endif

#if defined(FLOW) || defined(RAMP)
    sampler2D _FlowRampTex;
#endif

#if defined(RAMP)
    fixed4 _RampDarkCol;
    fixed4 _RampLightCol;
#endif

#if defined(EMISSIVETEX)
    fixed4 _EmissiveCol;
#endif

#if defined(STRAIGHT_SPEC)
    half _StraightSpec;
    half _StraightSpecDir;
    half _StraightSpecPower;
    half _StraightSpecIntensity;
#endif

#if defined(CUBEMAP)
    samplerCUBE _CubeTex;
    fixed4 _CubeCol;
    half _CubeSrc;
    half _CubeDest;
    half _CubeMode;
#endif

#if defined(MATCAP_ENV)
    sampler2D _MatcapEnvTex;
    fixed4 _MatcapEnvCol;
    half _MatcapEnvSrc;
    half _MatcapEnvDest;
    half _MatcapEnvMode;
#endif

#if defined(FLOW)
    samplerCUBE _FlowCubeTex;
    fixed4 _FlowCubeCol;
    half _FlowCubeSpeed;

    half _FlowSpecPower1;
    fixed4 _FlowSpecCol1;
    half _FlowSpecSpeed1;

    half _FlowSpecPower2;
    fixed4 _FlowSpecCol2;
    half _FlowSpecSpeed2;
#endif

#if defined(RIM)
    fixed4 _RimColor;
    half _RimPower;
    half _RimIntensity;
    half _RimDir;
#endif

#if defined(MATCAP)
    sampler2D _MatcapTex;
    half _MatcapIntensity;
#endif

#if defined(MATCAP_SPEC)
    sampler2D _MatcapSpecTex;
    half4 _MatcapSpecDir;
#endif

#if defined(TRANSPARENCY)
    half _Transparency;
#endif

uniform float4 _LightColor0;
static const float PI = 3.14159;
static const float PI2 = 3.14159 * 2;

// 得到一个颜色的亮度
float Luminosity(fixed4 col)
{
    return 0.299 * col.r + 0.587 * col.g + 0.114 * col.b;
}

struct vert_data 
{
    float4 vertex : POSITION;
    float4 texcoord : TEXCOORD;
    float3 normal : NORMAL;
    #if defined(NORMALTEX)
        float4 tangent : TANGENT;
    #endif
    uint vertexId : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    #if defined(NEED_NORMAL)
        half3 normal : TEXCOORD1;
        float3 worldPos : TEXCOORD2;
    #endif

    #if defined(NORMALTEX)
        half3 tangent : TEXCOORD3;
        half3 binormal : TEXCOORD4;
    #elif defined(MATCAP_NORMALTEX)
        half3 viewTangent : TEXCOORD3;
        half3 viewBinormal : TEXCOORD4;
    #endif

    #if defined(NEED_VIEW_NORMAL)
        half3 viewNormal : TEXCOORD5;
    #endif
    
    #if defined(RIM_DIR)
        half3 rimDir : TEXCOORD6;
    #endif

    UNITY_FOG_COORDS(7)

    #if defined(SHADOWS_SCREEN)
        SHADOW_COORDS(8)
    #endif
    
    #if defined(VERTEXLIGHT_ON)
        fixed3 vertexLightCol : TEXCOORD9;
    #endif

    UNITY_VERTEX_OUTPUT_STEREO
};


v2f vert(vert_data v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    v.vertex = ApplyMeshAnimation(v.vertex, v.vertexId);		
    v.normal = GetAnimatedMeshNormal(v.normal, v.vertexId);
    
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
    
    #if defined(NEED_NORMAL)
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    #endif

    #if defined(NORMALTEX)
        o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
        o.binormal = (v.tangent.w * unity_WorldTransformParams.w) * cross(o.normal, o.tangent);
    #elif defined(MATCAP_NORMALTEX)
        o.viewTangent = mul(UNITY_MATRIX_IT_MV, float4(v.tangent.xyz, 0)).xyz;
        o.viewBinormal = (v.tangent.w * unity_WorldTransformParams.w) * cross(o.viewNormal, o.viewTangent);
    #endif

    #if defined(NEED_VIEW_NORMAL)
        o.viewNormal = mul(UNITY_MATRIX_IT_MV, float4(v.normal, 0)).xyz;
        o.viewNormal = normalize(o.viewNormal);
    #endif
    
    #if defined(RIM_DIR)
        _RimDir = 0.5;      // 美术不需要调, 固定0.5的单侧轮廓光
        float rimAngle = PI2 * _RimDir;
        half3 viewRimDir = half3(cos(rimAngle), 0, sin(rimAngle));
        o.rimDir = mul((float3x3)transpose(UNITY_MATRIX_V), viewRimDir).xyz;
    #endif

    // 顶点光源
    #if defined(VERTEXLIGHT_ON)
        o.vertexLightCol = Shade4PointLights(
            unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
            unity_LightColor[0].rgb, unity_LightColor[1].rgb,
            unity_LightColor[2].rgb, unity_LightColor[3].rgb,
            unity_4LightAtten0, o.worldPos, o.normal
        );
    #endif

    UNITY_TRANSFER_FOG(o, o.pos);

    // 阴影
    #if defined(SHADOWS_SCREEN)
        TRANSFER_SHADOW(o);
    #endif

    return o;
}

half4 frag(v2f i, fixed facing : VFACE) : COLOR
{
    // 法线
    #if defined(NEED_NORMAL)
        // 如果是背面, 反转法线
        i.normal = (facing >= 0 ? 1 : -1) * i.normal;   // 比用sign效率高一些
        
        // 法线
        half3 originNormal = normalize(i.normal);
        half3 normal = originNormal;
        #if defined(NORMALTEX)
            half3 tangentNormal = UnpackNormal(tex2D(_NormalTex, i.uv));
            normal = normalize(
                tangentNormal.x * i.tangent +
                tangentNormal.y * i.binormal +
                tangentNormal.z * i.normal
            );
        #endif
    #endif
    
    #if defined(NEED_VIEW_NORMAL)
        i.viewNormal = (facing >= 0 ? 1 : -1) * i.viewNormal;
        
        #if defined(MATCAP_NORMALTEX)
        half3 tangentNormal = UnpackNormal(tex2D(_NormalTex, i.uv));
            i.viewNormal = normalize(
                tangentNormal.x * i.viewTangent +
                tangentNormal.y * i.viewBinormal +
                tangentNormal.z * i.viewNormal
            );
        #else
            i.viewNormal = normalize(i.viewNormal);
        #endif
        
        #if defined(FORWARD_BASE_PASS) && (defined(SPECULAR) || defined(FLOW))
            half3 flowViewNormal = normalize(half3(i.viewNormal.x, 0, -i.viewNormal.z));
        #endif
    #endif
    
    fixed4 texCol = tex2D(_MainTex, i.uv);

    // mask
    #if defined(SPECTEX) || defined(EMISSIVETEX)
        fixed3 maskCol = tex2D(_MaskTex, i.uv);
    #endif
    
    #if defined(SPECULAR) || defined(FLOW)
        half specMask = 1;
    #endif        
    
    #if defined(SPECULAR)
        half glossMask = 1;
    #endif

    #if defined(SPECTEX)
        specMask = maskCol.r;
        glossMask = maskCol.g;
    #endif

    #if defined(EMISSIVETEX)
        half emissiveMask = maskCol.b;
    #endif

    // lightDir, lightCol和attenuation
    #if defined(DIFFUSE) || defined(SPECULAR)
        #if defined(POINT)
            half3 lightDir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
        #else
            half3 lightDir = _WorldSpaceLightPos0.xyz;
        #endif

        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);

        fixed3 lightCol = _LightColor0.rgb * attenuation;
    #endif

    // indirect 
    #if defined(FORWARD_BASE_PASS)
        #if defined(VERTEXLIGHT_ON)
            fixed3 indirectCol = i.vertexLightCol;
            #if defined(FORWARD_BASE_PASS)
                fixed3 shCol = max(0, ShadeSH9(half4(i.normal, 1)));
                indirectCol += shCol;
            #endif
        #elif defined(MATCAP)
            fixed3 indirectCol = 0;
        #else
            fixed3 indirectCol = UNITY_LIGHTMODEL_AMBIENT.rgb;
        #endif
    #else 
        fixed3 indirectCol = 0;
    #endif

    indirectCol *= texCol.rgb;
    
    // 一些计算变量
    #if defined(SPECULAR) || defined(FLOW) || defined(RIM) || defined(CUBEMAP)
        half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
    #endif
    
    #if (defined(FLOW) || defined(CUBEMAP)) && defined(FORWARD_BASE_PASS)
        half3 reflDir = reflect(viewDir, normalize(originNormal));      // 不用法线贴图的法线
    #endif
        
    // matcap
    #if defined(FORWARD_BASE_PASS) && (defined(MATCAP) || defined(MATCAP_ENV))
        half2 matcapUV = i.viewNormal.xy * 0.5 + 0.5;
    #endif
    
    #if defined(MATCAP) && defined(FORWARD_BASE_PASS)
        fixed4 matcapCol = tex2D(_MatcapTex, matcapUV);
        matcapCol.rgb *= texCol.rgb * _MatcapIntensity;
        matcapCol.rgb = lerp(texCol.rgb, matcapCol.rgb, saturate(_MatcapIntensity));
        
        #if defined(MATCAP_SPEC)
            half specMask = tex2D(_MatcapSpecTex, i.uv).r;
            half3 matcapSpecDir = normalize(_MatcapSpecDir.xyz);
            half spec = pow(max(0, dot(matcapSpecDir, i.viewNormal)), exp2(_SpecPower * 10.0 + 1.0));
            fixed3 specCol = (spec * _SpecCol.a * 10 * specMask) * (texCol.rgb * _SpecCol.rgb);
            matcapCol.rgb += specCol.rgb;
        #endif
    #endif

    // diffuse
    #if defined(DIFFUSE)
        half lambert = max(0, dot(lightDir, normal));
        fixed3 kD = lambert;

        #if defined(RAMP) && defined(FORWARD_BASE_PASS)
            half3 ramp = tex2D(_FlowRampTex, float2(lambert, 0)).a;
            fixed3 rampCol = lerp(_RampDarkCol.rgb, _RampLightCol.rgb, ramp);
            fixed3 diffCol = rampCol.rgb * texCol.rgb * lightCol.rgb;
        #else
            fixed3 diffCol = kD * (texCol.rgb * lightCol.rgb);
        #endif
    #endif

    // specular
    #if defined(SPECULAR) && defined(FORWARD_BASE_PASS)
        half3 halfVector = normalize(lightDir + viewDir); 
        half gloss = (glossMask * _SpecPower);
        half specPow = exp2(gloss * 10.0 + 1.0);
        
        half spec = pow(max(0, dot(halfVector, normal)), specPow);
        
        #if defined(STRAIGHT_SPEC)
            half straightSpecAngle = PI2 * _StraightSpecDir;
            half specVDotN = sin(-straightSpecAngle) * flowViewNormal.x + cos(-straightSpecAngle) * flowViewNormal.z;
            half straightSpec = pow(max(0, specVDotN), _StraightSpecPower) * _StraightSpec * _StraightSpecIntensity;
            spec = max(straightSpec, spec);
        #endif
        
        fixed3 specCol = (spec * _SpecCol.a * 10 * specMask) * (texCol.rgb * _SpecCol.rgb * lightCol.rgb);
    #endif

    #if defined(FLOW) && defined(FORWARD_BASE_PASS)
        // 高光流光1
        half flowTexU = _Time.y * _FlowSpecSpeed1;
        half flowRotation = PI2 * tex2D(_FlowRampTex, float2(flowTexU % 1, 0)).g;        // 贴图设置为了clamp采样, 所以需要%1
        
        float flowVDotN = sin(-flowRotation) * flowViewNormal.x + cos(-flowRotation) * flowViewNormal.z;
        half flowSpec = pow(max(0, flowVDotN), max(0.01, _FlowSpecPower1));
        fixed3 flowSpecCol1 = (flowSpec * _FlowSpecCol1.a * 2) * _FlowSpecCol1.rgb;

        // 高光流光2
        flowTexU = _Time.y * _FlowSpecSpeed2;
        flowRotation = PI2 * tex2D(_FlowRampTex, float2(flowTexU % 1, 0)).b;
        
        flowVDotN = sin(-flowRotation) * flowViewNormal.x + cos(-flowRotation) * flowViewNormal.z;
        flowSpec = pow(max(0, flowVDotN), max(0.01, _FlowSpecPower2));
        fixed3 flowSpecCol2 = (flowSpec * _FlowSpecCol2.a * 2) * _FlowSpecCol2.rgb;

        // cube流光
        half cubeTexU = _Time.y * _FlowCubeSpeed;
        half cubeTexRotation = -PI2 * tex2D(_FlowRampTex, half2(cubeTexU % 1, 0)).r;

        half3 rotReflDir = half3(
        reflDir.x * cos(cubeTexRotation) + reflDir.z * sin(-cubeTexRotation),
        reflDir.y,
        reflDir.x * sin(cubeTexRotation) + reflDir.z * cos(cubeTexRotation));

        fixed3 flowCubeCol = (texCUBE(_FlowCubeTex, rotReflDir).rgb * _FlowCubeCol.rgb) * (_FlowCubeCol.a * 2);
    #endif
    
    #if defined(CUBEMAP) && defined(FORWARD_BASE_PASS)
        fixed3 cubeCol = (texCUBE(_CubeTex, reflDir).rgb * _CubeCol.rgb) * (_CubeCol.a * 2);
    #endif
    
    #if defined(MATCAP_ENV) && defined(FORWARD_BASE_PASS)
        fixed3 matcapEnvCol = tex2D(_MatcapEnvTex, matcapUV).rgb * _MatcapEnvCol.rgb * (_MatcapEnvCol.a * 2);;
    #endif

    // rim
    #if defined(RIM) && defined(FORWARD_BASE_PASS)
        half rim = pow(saturate(1 - dot(viewDir, originNormal)), _RimPower);
        fixed3 rimCol = (rim  * _RimIntensity * _RimColor.a * 8) * (texCol.rgb * _RimColor.rgb);
        
        #if defined(RIM_DIR)
            half rimDirFactor = saturate(1 - dot(i.rimDir, originNormal));
            rimDirFactor = pow(rimDirFactor, 8);
            
            rimCol *= rimDirFactor;
        #endif
    #else
        fixed3 rimCol = 0;
    #endif

    // emissive
    #if defined(EMISSIVETEX) && defined(FORWARD_BASE_PASS)
        fixed3 emissiveCol = (emissiveMask * _EmissiveCol.a * 4) * (texCol.rgb * _EmissiveCol.rgb);
    #endif

    // finalCol
    fixed4 finalCol = 1;
    finalCol.rgb = 0;
    
    #if defined(DIFFUSE)
        finalCol.rgb += diffCol.rgb;
    #endif
    
    #if defined(FORWARD_BASE_PASS)
        finalCol.rgb += indirectCol.rgb;
        
        #if defined(MATCAP)
            finalCol.rgb += matcapCol.rgb;
        #endif 
    
        #if defined(SPECULAR)
            finalCol.rgb += specCol.rgb;
        #endif

        #if defined(FLOW)
            finalCol.rgb += (flowCubeCol.rgb + flowSpecCol1.rgb + flowSpecCol2.rgb) * specMask;
        #endif

        #if defined(RIM)
            finalCol.rgb += rimCol.rgb;
        #endif

        #if defined(EMISSIVETEX)
            finalCol.rgb += emissiveCol.rgb;
        #endif
        
        #if defined(CUBEMAP)
            fixed3 cubeDestCol = lerp(cubeCol.rgb, finalCol.rgb * cubeCol.rgb, _CubeMode) * specMask;
            finalCol.rgb = _CubeSrc * finalCol.rgb + _CubeDest * cubeDestCol;
        #endif
        
        #if defined(MATCAP_ENV)
            fixed3 matcapEnvDestCol = lerp(matcapEnvCol.rgb, finalCol.rgb * matcapEnvCol.rgb, _MatcapEnvMode) * specMask;
            finalCol.rgb = _MatcapEnvSrc * finalCol.rgb + _MatcapEnvDest * matcapEnvDestCol;
        #endif
    #endif

    // 雾效
    #if defined(FORWARD_BASE_PASS)
        UNITY_APPLY_FOG_COLOR(i.fogCoord, finalCol, unity_FogColor);
    #else
        UNITY_APPLY_FOG_COLOR(i.fogCoord, finalCol, fixed4(0,0,0,0));
    #endif

    finalCol.a = texCol.a;
    
    #if defined(TRANSPARENCY)
        finalCol.a *= _Transparency;
    #endif
    return finalCol;
}

// shadow
struct vert_data_shadow_caster 
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};

float4 vert_shadow_caster(vert_data_shadow_caster v) : SV_POSITION 
{
    float4 pos = UnityClipSpaceShadowCasterPos(v.vertex.xyz, v.normal);
    return UnityApplyLinearShadowBias(pos);
}

half4 frag_shadow_caster() : SV_TARGET 
{
    return 0;
}

// xray
fixed4 _XrayCol;
half _XrayPower;
half _XrayIntensity;

struct vert_data_xray
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
};

struct v2f_xray
{
    float4 pos : SV_POSITION;
    half3 normal : normal;
    half3 worldPos : TEXCOORD0;
};

v2f_xray vert_xray(vert_data_xray v) 
{
    v2f_xray o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

    return o;
}

fixed4 frag_xray(v2f_xray i, fixed facing : VFACE) : SV_Target
{
    // 如果是背面, 反转法线
    i.normal = (facing >= 0 ? 1 : -1) * i.normal;   // 比用sign效率高一些
    
    half3 normal = normalize(i.normal);
    half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

    half rim = pow(1.0 - saturate(dot(viewDir, normal)), _XrayPower);
    fixed3 rimCol = rim * _XrayCol.rgb;
    
    fixed4 finalCol = 1;
    finalCol.rgb = rimCol;
    finalCol.a = _XrayCol.a;
    
    finalCol.rgb *= _XrayIntensity;

    return finalCol;
}

#endif
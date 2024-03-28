#ifndef FLATKIT_LIGHT_PASS_DR_INCLUDED
#define FLATKIT_LIGHT_PASS_DR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
// Check is needed because in Unity 2021 they use two different URP versions - 14 on desktop and 12 on mobile.
#if !VERSION_LOWER(13, 0)
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif
#endif
#include "Lighting_DR.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 staticLightmapUV    : TEXCOORD1;
    float2 dynamicLightmapUV    : TEXCOORD2;

#if defined(DR_VERTEX_COLORS_ON)
    float4 color        : COLOR;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;

    float3 positionWS               : TEXCOORD1;    // xyz: posWS

#ifdef _NORMALMAP
    float4 normalWS                   : TEXCOORD2;    // xyz: normal, w: viewDir.x
    float4 tangentWS                  : TEXCOORD3;    // xyz: tangent, w: viewDir.y
    float4 bitangentWS                : TEXCOORD4;    // xyz: bitangent, w: viewDir.z
#else
    float3  normalWS                  : TEXCOORD2;
    float3 viewDir                    : TEXCOORD3;
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half4 fogFactorAndVertexLight  : TEXCOORD5; // x: fogFactor, yzw: vertex light
#else
    half  fogFactor                 : TEXCOORD5;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
#endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);
    
#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV : TEXCOORD8; // Dynamic lightmap UVs
#endif

    float4 positionCS               : SV_POSITION;

#if defined(DR_VERTEX_COLORS_ON)
    float4 VertexColor              : COLOR;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

/// ---------------------------------------------------------------------------

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    inputData.positionWS = input.positionWS;

    #ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
#if VERSION_GREATER_EQUAL(12, 0)
    inputData.tangentToWorld = half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz);
    inputData.normalWS = TransformTangentToWorld(normalTS, inputData.tangentToWorld);
#else
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
    #endif
#else
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(inputData.positionWS);
    inputData.normalWS = input.normalWS;
    #endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
#else
#if VERSION_GREATER_EQUAL(12, 0)
    inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactor);
#endif
    inputData.vertexLighting = half3(0, 0, 0);
#endif

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
#endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #endif
}

Varyings StylizedPassVertex(Attributes input)
{
    #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
    #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
        CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(input.positionOS, input.normalOS, input.tangentOS)
    #else
        CURVEDWORLD_TRANSFORM_VERTEX(input.positionOS)
    #endif
    #endif

    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    const VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    
#if defined(_FOG_FRAGMENT)
    half fogFactor = 0;
#else
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
#endif

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.positionWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

    half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;

#ifdef _NORMALMAP
    output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
    output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDir = viewDirWS;
#endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif

#if UNITY_VERSION >= 202310
    OUTPUT_SH(vertexInput.positionWS, output.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.vertexSH);
#else
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
#else
    output.fogFactor = fogFactor;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

#if defined(DR_VERTEX_COLORS_ON)
    output.VertexColor = input.color;
#endif

    return output;
}

half4 StylizedPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    SurfaceData surfaceData;
    InitializeSimpleLitSurfaceData(input.uv, surfaceData);

#if defined(LOD_FADE_CROSSFADE) && !VERSION_LOWER(13, 0)
    LODFadeCrossFade(input.positionCS);
#endif

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    #if VERSION_GREATER_EQUAL(12, 0)
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);
    #endif

#if defined(DR_VERTEX_COLORS_ON)
    _BaseColor.rgb *= input.VertexColor.rgb;
#endif

    // Computes direct light contribution.
    half4 color = UniversalFragment_DSTRM(inputData, surfaceData.albedo * _BaseColor.rgb, surfaceData.emission, surfaceData.alpha);

    {
#if defined(_TEXTUREBLENDINGMODE_ADD)
        color.rgb += lerp(half3(0.0f, 0.0f, 0.0f), surfaceData.albedo + surfaceData.emission, _TextureImpact);
#else  // _TEXTUREBLENDINGMODE_MULTIPLY
        // This is the default blending mode for compatibility with the v.1 of the asset.
        color.rgb *= lerp(half3(1.0f, 1.0f, 1.0f), surfaceData.albedo + surfaceData.emission, _TextureImpact);
#endif
    }

#ifdef _DBUFFER
    ApplyDecalToBaseColor(input.positionCS, color.rgb);
#endif

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
#if UNITY_VERSION >= 202220
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));
#else
    color.a = OutputAlpha(color.a, _Surface);
#endif

    return color;
}

#endif // FLATKIT_LIGHT_PASS_DR_INCLUDED

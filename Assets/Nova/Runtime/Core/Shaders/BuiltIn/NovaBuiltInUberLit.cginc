#ifndef NOVA_BUILTIN_UBER_LIT_INCLUDED
#define NOVA_BUILTIN_UBER_LIT_INCLUDED

#include "NovaBuiltInUberUnlit.cginc"

half4 NovaResolveMinimumLitBaseColor(
    sampler2D baseMap,
    float2 uv,
    half4 tintColor,
    half4 vertexColor)
{
    half4 color = NOVA_SAMPLE_TEX2D(baseMap, uv);

    #if defined(_TINT_AREA_ALL) && defined(_TINT_COLOR_ENABLED)
    color *= tintColor;
    #endif

    color *= vertexColor;
    return color;
}

half4 NovaApplyMinimumLitForward(half4 baseColor, float3 normalWS, half cutoff)
{
    #if defined(_ALPHATEST_ENABLED)
    clip(baseColor.a - cutoff);
    #endif

    half3 litColor = NovaComputeMinimumLitColor(baseColor.rgb, normalWS);
    return half4(litColor, baseColor.a);
}

NOVA_DECLARE_TEX2D(_NormalMap);
NOVA_DECLARE_TEX2D_ARRAY(_NormalMap2DArray);
NOVA_DECLARE_TEX3D(_NormalMap3D);
NOVA_DECLARE_TEX2D(_SpecularMap);
NOVA_DECLARE_TEX2D_ARRAY(_SpecularMap2DArray);
NOVA_DECLARE_TEX3D(_SpecularMap3D);
NOVA_DECLARE_TEX2D(_MetallicMap);
NOVA_DECLARE_TEX2D_ARRAY(_MetallicMap2DArray);
NOVA_DECLARE_TEX3D(_MetallicMap3D);
NOVA_DECLARE_TEX2D(_SmoothnessMap);
NOVA_DECLARE_TEX2D_ARRAY(_SmoothnessMap2DArray);
NOVA_DECLARE_TEX3D(_SmoothnessMap3D);

half _NormalMapBumpScale;
half4 _SpecularColor;
half _SpecularMapChannelsX;
half _Metallic;
half _MetallicMapChannelsX;
half _Smoothness;
half _SmoothnessMapChannelsX;
half _BaseMapProgress;
half _BaseMapSliceCount;
half _BaseMapRowCount;
half _LitReceiveShadows;

struct NovaLitSurfaceData
{
    half3 albedo;
    half alpha;
    half metallic;
    half3 specularColor;
    half smoothness;
    half3 normalWS;
};

float3 NovaResolveLitTextureCoord(float2 uv, half progress, half sliceCount)
{
    return float3(uv, NovaBaseMapProgress(progress, sliceCount, _BaseMapRowCount, 0.0h));
}

#if defined(_BASE_MAP_MODE_2D_ARRAY)
#define NovaSampleLitScalarMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D_ARRAY(map2DArray, (litCoord).xy, (litCoord).z)
#define NovaSampleLitColorMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D_ARRAY(map2DArray, (litCoord).xy, (litCoord).z)
#define NovaSampleLitNormalMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D_ARRAY(map2DArray, (litCoord).xy, (litCoord).z)
#elif defined(_BASE_MAP_MODE_3D)
#define NovaSampleLitScalarMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX3D_LOD(map3D, float3((litCoord).xy, (litCoord).z), 0.0)
#define NovaSampleLitColorMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX3D_LOD(map3D, float3((litCoord).xy, (litCoord).z), 0.0)
#define NovaSampleLitNormalMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX3D_LOD(map3D, float3((litCoord).xy, (litCoord).z), 0.0)
#else
#define NovaSampleLitScalarMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D(map2D, (litCoord).xy)
#define NovaSampleLitColorMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D(map2D, (litCoord).xy)
#define NovaSampleLitNormalMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D(map2D, (litCoord).xy)
#endif

half NovaResolveMetallic(float3 litCoord)
{
    #if defined(_SPECULAR_SETUP)
    return 1.0h;
    #elif defined(_METALLIC_MAP_ENABLED)
    half4 metallic = NovaSampleLitScalarMap(_MetallicMap, _MetallicMap2DArray, _MetallicMap3D, litCoord);
    return saturate(NovaSelectChannel(metallic, _MetallicMapChannelsX) * _Metallic);
    #else
    return saturate(_Metallic);
    #endif
}

half3 NovaResolveSpecularColor(float3 litCoord)
{
    #if defined(_SPECULAR_SETUP)
    #if defined(_SPECULAR_MAP_ENABLED)
    half4 specular = NovaSampleLitColorMap(_SpecularMap, _SpecularMap2DArray, _SpecularMap3D, litCoord);
    return saturate(specular.rgb * _SpecularColor.rgb);
    #else
    return saturate(_SpecularColor.rgb);
    #endif
    #else
    return half3(0.0h, 0.0h, 0.0h);
    #endif
}

half NovaResolveSmoothness(float3 litCoord)
{
    #if defined(_SMOOTHNESS_MAP_ENABLED)
    half4 smoothness = NovaSampleLitScalarMap(_SmoothnessMap, _SmoothnessMap2DArray, _SmoothnessMap3D, litCoord);
    return saturate(NovaSelectChannel(smoothness, _SmoothnessMapChannelsX) * _Smoothness);
    #else
    return saturate(_Smoothness);
    #endif
}

half3 NovaResolveNormalMap(float3 litCoord)
{
    #if defined(_NORMAL_MAP_ENABLED)
    half4 packedNormal = NovaSampleLitNormalMap(_NormalMap, _NormalMap2DArray, _NormalMap3D, litCoord);
    half3 normalTS;
    normalTS.xy = packedNormal.xy * 2.0h - 1.0h;
    normalTS.xy *= _NormalMapBumpScale;
    normalTS.z = sqrt(saturate(1.0h - dot(normalTS.xy, normalTS.xy)));
    return NovaSafeNormalize(normalTS);
    #else
    return half3(0.0h, 0.0h, 1.0h);
    #endif
}

half3 NovaTangentToWorldNormal(half3 normalTS, float3 normalWS, float3 tangentWS, float3 bitangentWS)
{
    float3 normalizedNormalWS = NovaSafeNormalize(normalWS);

    #if defined(_NORMAL_MAP_ENABLED)
    float3 normalizedTangentWS = NovaSafeNormalize(tangentWS);
    float3 normalizedBitangentWS = NovaSafeNormalize(bitangentWS);
    return NovaSafeNormalize(
        normalTS.x * normalizedTangentWS +
        normalTS.y * normalizedBitangentWS +
        normalTS.z * normalizedNormalWS);
    #else
    return normalizedNormalWS;
    #endif
}

NovaLitSurfaceData NovaResolveLitSurfaceData(
    half4 baseColor,
    float3 litCoord,
    float3 normalWS,
    float3 tangentWS,
    float3 bitangentWS)
{
    NovaLitSurfaceData surfaceData;
    surfaceData.albedo = baseColor.rgb;
    surfaceData.alpha = baseColor.a;
    surfaceData.metallic = NovaResolveMetallic(litCoord);
    surfaceData.specularColor = NovaResolveSpecularColor(litCoord);
    surfaceData.smoothness = NovaResolveSmoothness(litCoord);
    surfaceData.normalWS = NovaTangentToWorldNormal(NovaResolveNormalMap(litCoord), normalWS, tangentWS, bitangentWS);
    return surfaceData;
}

half3 NovaComputeSpecularApproximation(NovaLitSurfaceData surfaceData, float3 lightDirectionWS, float3 viewDirectionWS)
{
    #if !defined(_SPECULAR_HIGHLIGHTS_ENABLED)
    return half3(0.0h, 0.0h, 0.0h);
    #else
    half ndotl = NovaComputeLambertDiffuse(surfaceData.normalWS, lightDirectionWS);
    half3 halfDirectionWS = NovaSafeNormalize(NovaSafeNormalize(lightDirectionWS) + NovaSafeNormalize(viewDirectionWS));
    half ndoth = saturate(dot(NovaSafeNormalize(surfaceData.normalWS), halfDirectionWS));
    half exponent = lerp(8.0h, 96.0h, surfaceData.smoothness);

    #if defined(_SPECULAR_SETUP)
    half3 specularColor = surfaceData.specularColor;
    half specularStrength = 1.0h;
    #else
    half3 specularColor = lerp(half3(0.04h, 0.04h, 0.04h), surfaceData.albedo, surfaceData.metallic);
    half specularStrength = lerp(0.25h, 1.0h, surfaceData.metallic);
    #endif

    half specularTerm = pow(ndoth, exponent) * surfaceData.smoothness * ndotl * specularStrength;
    return specularColor * specularTerm * NovaGetMainLightColor();
    #endif
}

half NovaLitReceiveShadowsEnabled()
{
    #if defined(_RECEIVE_SHADOWS_ENABLED)
    return step(0.5h, _LitReceiveShadows);
    #else
    return 0.0h;
    #endif
}

half4 NovaComputeLitForwardBaseColor(NovaLitSurfaceData surfaceData, float3 viewDirectionWS, half shadowAttenuation)
{
    half receiveShadow = NovaLitReceiveShadowsEnabled();
    half directShadow = NovaApplyReceiveShadow(shadowAttenuation, receiveShadow);
    half diffuse = NovaComputeLambertDiffuse(surfaceData.normalWS, NovaGetMainLightDirection());
    half3 direct = NovaGetMainLightColor() * diffuse * directShadow;
    half3 ambient = NovaComputeAmbientApproximation(surfaceData.normalWS);
    half3 diffuseColor = surfaceData.albedo * (direct + ambient);
    half3 specularColor = NovaComputeSpecularApproximation(surfaceData, NovaGetMainLightDirection(), viewDirectionWS) * directShadow;
    return half4(diffuseColor + specularColor, surfaceData.alpha);
}

half4 NovaComputeLitForwardBaseColor(NovaLitSurfaceData surfaceData, float3 viewDirectionWS)
{
    return NovaComputeLitForwardBaseColor(surfaceData, viewDirectionWS, 1.0h);
}

half4 NovaApplyLitForwardBase(
    half4 baseColor,
    float3 litCoord,
    float3 normalWS,
    float3 tangentWS,
    float3 bitangentWS,
    float3 viewDirectionWS,
    half cutoff,
    half shadowAttenuation)
{
    #if defined(_ALPHATEST_ENABLED)
    clip(baseColor.a - cutoff);
    #endif

    NovaLitSurfaceData surfaceData = NovaResolveLitSurfaceData(baseColor, litCoord, normalWS, tangentWS, bitangentWS);
    return NovaComputeLitForwardBaseColor(surfaceData, viewDirectionWS, shadowAttenuation);
}

#endif

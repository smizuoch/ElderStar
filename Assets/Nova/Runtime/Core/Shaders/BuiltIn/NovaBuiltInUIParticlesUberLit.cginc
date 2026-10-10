#ifndef NOVA_BUILTIN_UI_PARTICLES_UBER_LIT_INCLUDED
#define NOVA_BUILTIN_UI_PARTICLES_UBER_LIT_INCLUDED

#include "NovaBuiltInCore.cginc"
#include "NovaBuiltInTexture.cginc"
#include "NovaBuiltInLighting.cginc"
#include "NovaBuiltInShadow.cginc"
#include "NovaBuiltInUI.cginc"

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
half _Metallic;
half _MetallicMapChannelsX;
half _Smoothness;
half _SmoothnessMapChannelsX;
#ifndef NOVA_PARTICLE_FEATURES_INCLUDED
half _BaseMapProgress;
#endif
#ifndef NOVA_PARTICLE_FEATURES_INCLUDED
half _BaseMapSliceCount;
#endif
half _LitReceiveShadows;
half _EnvironmentReflections;

struct NovaUILitSurfaceData
{
    half3 albedo;
    half alpha;
    half metallic;
    half3 specularColor;
    half smoothness;
    half3 normalWS;
};

half4 NovaResolveUILitBaseColor(
    sampler2D baseMap,
    float2 uv,
    half4 tintColor,
    half4 vertexColor,
    float2 positionOS)
{
    half4 color = NOVA_SAMPLE_TEX2D(baseMap, uv);

    #if defined(_TINT_AREA_ALL) && defined(_TINT_COLOR_ENABLED)
    color *= tintColor;
    #endif

    color *= vertexColor;
    color.a *= NovaUIClipAlpha(positionOS);
    return color;
}

half NovaUIFlipBookProgress(half progress, half sliceCount)
{
    return floor(clamp(progress, 0.0h, 0.999h) * max(sliceCount, 1.0h));
}

half NovaUIFlipBookBlendingProgress(half progress, half sliceCount)
{
    half safeSliceCount = max(sliceCount, 1.0h);
    half offset = 1.0h / safeSliceCount * 0.5h;
    return lerp(offset, 1.0h - offset, saturate(progress));
}

float3 NovaResolveUILitTextureCoord(float2 uv)
{
    #if defined(_BASE_MAP_MODE_2D_ARRAY)
    return float3(uv, NovaUIFlipBookProgress(_BaseMapProgress, _BaseMapSliceCount));
    #elif defined(_BASE_MAP_MODE_3D)
    return float3(uv, NovaUIFlipBookBlendingProgress(_BaseMapProgress, _BaseMapSliceCount));
    #else
    return float3(uv, 0.0h);
    #endif
}

#if defined(_BASE_MAP_MODE_2D_ARRAY)
#define NovaSampleUILitScalarMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D_ARRAY(map2DArray, (litCoord).xy, (litCoord).z)
#define NovaSampleUILitColorMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D_ARRAY(map2DArray, (litCoord).xy, (litCoord).z)
#define NovaSampleUILitNormalMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D_ARRAY(map2DArray, (litCoord).xy, (litCoord).z)
#elif defined(_BASE_MAP_MODE_3D)
#define NovaSampleUILitScalarMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX3D_LOD(map3D, float3((litCoord).xy, (litCoord).z), 0.0)
#define NovaSampleUILitColorMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX3D_LOD(map3D, float3((litCoord).xy, (litCoord).z), 0.0)
#define NovaSampleUILitNormalMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX3D_LOD(map3D, float3((litCoord).xy, (litCoord).z), 0.0)
#else
#define NovaSampleUILitScalarMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D(map2D, (litCoord).xy)
#define NovaSampleUILitColorMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D(map2D, (litCoord).xy)
#define NovaSampleUILitNormalMap(map2D, map2DArray, map3D, litCoord) NOVA_SAMPLE_TEX2D(map2D, (litCoord).xy)
#endif

half NovaResolveUIMetallicMap(float3 litCoord)
{
    half4 metallic = NovaSampleUILitScalarMap(_MetallicMap, _MetallicMap2DArray, _MetallicMap3D, litCoord);
    return NovaSelectChannel(metallic, _MetallicMapChannelsX);
}

half3 NovaResolveUISpecularMap(float3 litCoord)
{
    half4 specular = NovaSampleUILitColorMap(_SpecularMap, _SpecularMap2DArray, _SpecularMap3D, litCoord);
    return specular.rgb;
}

half NovaResolveUISmoothnessMap(float3 litCoord)
{
    half4 smoothness = NovaSampleUILitScalarMap(_SmoothnessMap, _SmoothnessMap2DArray, _SmoothnessMap3D, litCoord);
    return NovaSelectChannel(smoothness, _SmoothnessMapChannelsX);
}

half NovaResolveUILitMetallic(float3 litCoord)
{
    #if defined(_SPECULAR_SETUP)
    return 1.0h;
    #elif defined(_METALLIC_MAP_ENABLED)
    return saturate(NovaResolveUIMetallicMap(litCoord) * _Metallic);
    #else
    return saturate(_Metallic);
    #endif
}

half3 NovaResolveUILitSpecularColor(float3 litCoord)
{
    #if defined(_SPECULAR_SETUP)
    #if defined(_SPECULAR_MAP_ENABLED)
    return saturate(NovaResolveUISpecularMap(litCoord) * _SpecularColor.rgb);
    #else
    return saturate(_SpecularColor.rgb);
    #endif
    #else
    return half3(0.0h, 0.0h, 0.0h);
    #endif
}

half NovaResolveUILitSmoothness(float3 litCoord)
{
    #if defined(_SMOOTHNESS_MAP_ENABLED)
    return saturate(NovaResolveUISmoothnessMap(litCoord) * _Smoothness);
    #else
    return saturate(_Smoothness);
    #endif
}

half3 NovaResolveUINormalMap(float3 litCoord)
{
    #if defined(_NORMAL_MAP_ENABLED)
    half4 packedNormal = NovaSampleUILitNormalMap(_NormalMap, _NormalMap2DArray, _NormalMap3D, litCoord);
    half3 normalTS;
    normalTS.xy = packedNormal.xy * 2.0h - 1.0h;
    normalTS.xy *= _NormalMapBumpScale;
    normalTS.z = sqrt(saturate(1.0h - dot(normalTS.xy, normalTS.xy)));
    return NovaSafeNormalize(normalTS);
    #else
    return half3(0.0h, 0.0h, 1.0h);
    #endif
}

half3 NovaUnpackUILitNormalMap2D(float2 uv)
{
    return NovaResolveUINormalMap(float3(uv, 0.0h));
}

half3 NovaUILitTangentToWorldNormal(half3 normalTS, float3 normalWS, float3 tangentWS, float3 bitangentWS)
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

NovaUILitSurfaceData NovaResolveUILitSurfaceData(
    half4 baseColor,
    float3 litCoord,
    float3 normalWS,
    float3 tangentWS,
    float3 bitangentWS)
{
    NovaUILitSurfaceData surfaceData;
    surfaceData.albedo = baseColor.rgb;
    surfaceData.alpha = baseColor.a;
    surfaceData.metallic = NovaResolveUILitMetallic(litCoord);
    surfaceData.specularColor = NovaResolveUILitSpecularColor(litCoord);
    surfaceData.smoothness = NovaResolveUILitSmoothness(litCoord);
    surfaceData.normalWS = NovaUILitTangentToWorldNormal(NovaResolveUINormalMap(litCoord), normalWS, tangentWS, bitangentWS);
    return surfaceData;
}

NovaUILitSurfaceData NovaResolveUILitSurfaceData(
    half4 baseColor, float2 uv, float3 normalWS, float3 tangentWS, float3 bitangentWS)
{
    return NovaResolveUILitSurfaceData(baseColor, NovaResolveUILitTextureCoord(uv),
        normalWS, tangentWS, bitangentWS);
}

half3 NovaComputeUILitSpecularApproximation(
    NovaUILitSurfaceData surfaceData,
    float3 lightDirectionWS,
    float3 viewDirectionWS)
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

half NovaUILitReceiveShadowsEnabled()
{
    #if defined(_RECEIVE_SHADOWS_ENABLED)
    return step(0.5h, _LitReceiveShadows);
    #else
    return 0.0h;
    #endif
}

half4 NovaComputeUILitForwardBaseColor(NovaUILitSurfaceData surfaceData, float3 viewDirectionWS, half shadowAttenuation)
{
    half receiveShadow = NovaUILitReceiveShadowsEnabled();
    half directShadow = NovaApplyReceiveShadow(shadowAttenuation, receiveShadow);
    half diffuse = NovaComputeLambertDiffuse(surfaceData.normalWS, NovaGetMainLightDirection());
    half3 direct = NovaGetMainLightColor() * diffuse * directShadow;
    half3 ambient = NovaComputeAmbientApproximation(surfaceData.normalWS);
    half3 diffuseColor = surfaceData.albedo * (direct + ambient);
    half3 specularColor = NovaComputeUILitSpecularApproximation(surfaceData, NovaGetMainLightDirection(), viewDirectionWS) * directShadow;
    half3 environmentReflection = half3(0.0h, 0.0h, 0.0h);
    #if defined(_ENVIRONMENT_REFLECTIONS_ENABLED)
    environmentReflection = NovaSampleEnvironmentReflection(
        NovaReflectViewDirection(viewDirectionWS, surfaceData.normalWS), surfaceData.smoothness)
        * NovaComputeEnvironmentReflectionStrength(surfaceData.albedo, surfaceData.metallic,
            surfaceData.specularColor, surfaceData.smoothness)
        * step(0.5h, _EnvironmentReflections);
    #endif
    return half4(diffuseColor + specularColor + environmentReflection, surfaceData.alpha);
}

half4 NovaComputeUILitForwardBaseColor(NovaUILitSurfaceData surfaceData, float3 viewDirectionWS)
{
    return NovaComputeUILitForwardBaseColor(surfaceData, viewDirectionWS, 1.0h);
}

#endif

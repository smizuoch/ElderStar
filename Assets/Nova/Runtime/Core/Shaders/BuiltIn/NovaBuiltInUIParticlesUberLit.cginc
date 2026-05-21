#ifndef NOVA_BUILTIN_UI_PARTICLES_UBER_LIT_INCLUDED
#define NOVA_BUILTIN_UI_PARTICLES_UBER_LIT_INCLUDED

#include "NovaBuiltInCore.cginc"
#include "NovaBuiltInTexture.cginc"
#include "NovaBuiltInLighting.cginc"
#include "NovaBuiltInUI.cginc"

NOVA_DECLARE_TEX2D(_NormalMap);
NOVA_DECLARE_TEX2D(_SpecularMap);
NOVA_DECLARE_TEX2D(_MetallicMap);
NOVA_DECLARE_TEX2D(_SmoothnessMap);

half _NormalMapBumpScale;
half4 _SpecularColor;
half _Metallic;
half _MetallicMapChannelsX;
half _Smoothness;
half _SmoothnessMapChannelsX;

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

half NovaResolveUILitMetallic(float2 uv)
{
    #if defined(_SPECULAR_SETUP)
    return 1.0h;
    #elif defined(_METALLIC_MAP_ENABLED)
    half4 metallic = NOVA_SAMPLE_TEX2D(_MetallicMap, uv);
    return saturate(NovaSelectChannel(metallic, _MetallicMapChannelsX) * _Metallic);
    #else
    return saturate(_Metallic);
    #endif
}

half3 NovaResolveUILitSpecularColor(float2 uv)
{
    #if defined(_SPECULAR_SETUP)
    #if defined(_SPECULAR_MAP_ENABLED)
    half4 specular = NOVA_SAMPLE_TEX2D(_SpecularMap, uv);
    return saturate(specular.rgb * _SpecularColor.rgb);
    #else
    return saturate(_SpecularColor.rgb);
    #endif
    #else
    return half3(0.0h, 0.0h, 0.0h);
    #endif
}

half NovaResolveUILitSmoothness(float2 uv)
{
    #if defined(_SMOOTHNESS_MAP_ENABLED)
    half4 smoothness = NOVA_SAMPLE_TEX2D(_SmoothnessMap, uv);
    return saturate(NovaSelectChannel(smoothness, _SmoothnessMapChannelsX) * _Smoothness);
    #else
    return saturate(_Smoothness);
    #endif
}

half3 NovaUnpackUILitNormalMap2D(float2 uv)
{
    #if defined(_NORMAL_MAP_ENABLED)
    half4 packedNormal = NOVA_SAMPLE_TEX2D(_NormalMap, uv);
    half3 normalTS;
    normalTS.xy = packedNormal.xy * 2.0h - 1.0h;
    normalTS.xy *= _NormalMapBumpScale;
    normalTS.z = sqrt(saturate(1.0h - dot(normalTS.xy, normalTS.xy)));
    return NovaSafeNormalize(normalTS);
    #else
    return half3(0.0h, 0.0h, 1.0h);
    #endif
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
    float2 uv,
    float3 normalWS,
    float3 tangentWS,
    float3 bitangentWS)
{
    NovaUILitSurfaceData surfaceData;
    surfaceData.albedo = baseColor.rgb;
    surfaceData.alpha = baseColor.a;
    surfaceData.metallic = NovaResolveUILitMetallic(uv);
    surfaceData.specularColor = NovaResolveUILitSpecularColor(uv);
    surfaceData.smoothness = NovaResolveUILitSmoothness(uv);
    surfaceData.normalWS = NovaUILitTangentToWorldNormal(NovaUnpackUILitNormalMap2D(uv), normalWS, tangentWS, bitangentWS);
    return surfaceData;
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

half4 NovaComputeUILitForwardBaseColor(NovaUILitSurfaceData surfaceData, float3 viewDirectionWS)
{
    half diffuse = NovaComputeLambertDiffuse(surfaceData.normalWS, NovaGetMainLightDirection());
    half3 direct = NovaGetMainLightColor() * diffuse;
    half3 ambient = NovaComputeAmbientApproximation(surfaceData.normalWS);
    half3 diffuseColor = surfaceData.albedo * saturate(direct + ambient);
    half3 specularColor = NovaComputeUILitSpecularApproximation(surfaceData, NovaGetMainLightDirection(), viewDirectionWS);
    return half4(diffuseColor + specularColor, surfaceData.alpha);
}

#endif

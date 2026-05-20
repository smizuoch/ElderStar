#ifndef NOVA_BUILTIN_UBER_LIT_INCLUDED
#define NOVA_BUILTIN_UBER_LIT_INCLUDED

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
NOVA_DECLARE_TEX2D(_SpecularMap);
NOVA_DECLARE_TEX2D(_MetallicMap);
NOVA_DECLARE_TEX2D(_SmoothnessMap);

half _NormalMapBumpScale;
half4 _SpecularColor;
half _SpecularMapChannelsX;
half _Metallic;
half _MetallicMapChannelsX;
half _Smoothness;
half _SmoothnessMapChannelsX;

struct NovaLitSurfaceData
{
    half3 albedo;
    half alpha;
    half metallic;
    half3 specularColor;
    half smoothness;
    half3 normalWS;
};

half NovaResolveMetallic(float2 uv)
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

half3 NovaResolveSpecularColor(float2 uv)
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

half NovaResolveSmoothness(float2 uv)
{
    #if defined(_SMOOTHNESS_MAP_ENABLED)
    half4 smoothness = NOVA_SAMPLE_TEX2D(_SmoothnessMap, uv);
    return saturate(NovaSelectChannel(smoothness, _SmoothnessMapChannelsX) * _Smoothness);
    #else
    return saturate(_Smoothness);
    #endif
}

half3 NovaUnpackNormalMap2D(float2 uv)
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
    float2 uv,
    float3 normalWS,
    float3 tangentWS,
    float3 bitangentWS)
{
    NovaLitSurfaceData surfaceData;
    surfaceData.albedo = baseColor.rgb;
    surfaceData.alpha = baseColor.a;
    surfaceData.metallic = NovaResolveMetallic(uv);
    surfaceData.specularColor = NovaResolveSpecularColor(uv);
    surfaceData.smoothness = NovaResolveSmoothness(uv);
    surfaceData.normalWS = NovaTangentToWorldNormal(NovaUnpackNormalMap2D(uv), normalWS, tangentWS, bitangentWS);
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

half4 NovaComputeLitForwardBaseColor(NovaLitSurfaceData surfaceData, float3 viewDirectionWS)
{
    half diffuse = NovaComputeLambertDiffuse(surfaceData.normalWS, NovaGetMainLightDirection());
    half3 direct = NovaGetMainLightColor() * diffuse;
    half3 ambient = NovaComputeAmbientApproximation(surfaceData.normalWS);
    half3 diffuseColor = surfaceData.albedo * (direct + ambient);
    half3 specularColor = NovaComputeSpecularApproximation(surfaceData, NovaGetMainLightDirection(), viewDirectionWS);
    return half4(diffuseColor + specularColor, surfaceData.alpha);
}

half4 NovaApplyLitForwardBase(
    half4 baseColor,
    float2 uv,
    float3 normalWS,
    float3 tangentWS,
    float3 bitangentWS,
    float3 viewDirectionWS,
    half cutoff)
{
    #if defined(_ALPHATEST_ENABLED)
    clip(baseColor.a - cutoff);
    #endif

    NovaLitSurfaceData surfaceData = NovaResolveLitSurfaceData(baseColor, uv, normalWS, tangentWS, bitangentWS);
    return NovaComputeLitForwardBaseColor(surfaceData, viewDirectionWS);
}

#endif

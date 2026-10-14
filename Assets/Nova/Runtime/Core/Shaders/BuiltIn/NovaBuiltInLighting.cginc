#ifndef NOVA_BUILTIN_LIGHTING_INCLUDED
#define NOVA_BUILTIN_LIGHTING_INCLUDED

#include "NovaBuiltInCore.cginc"
#include "Lighting.cginc"

struct NovaBuiltInMainLight
{
    half3 direction;
    half3 color;
    half attenuation;
};

struct NovaBuiltInSurfaceData
{
    half3 albedo;
    half3 specular;
    half metallic;
    half smoothness;
    half alpha;
};

half3 NovaGetMainLightDirection()
{
    return NovaSafeNormalize(_WorldSpaceLightPos0.xyz);
}

half3 NovaGetMainLightColor()
{
    return _LightColor0.rgb;
}

float3 NovaGetBuiltInAddLightDirection(float3 positionWS)
{
    return NovaSafeNormalize(UnityWorldSpaceLightDir(positionWS));
}

half3 NovaGetBuiltInAddLightColor()
{
    return _LightColor0.rgb;
}

half NovaComputeLambertDiffuse(float3 normalWS, float3 lightDirectionWS)
{
    return saturate(dot(NovaSafeNormalize(normalWS), NovaSafeNormalize(lightDirectionWS)));
}

half3 NovaComputeDiffuseAlbedo(half3 albedo, half metallic, half3 specularColor)
{
    #if defined(_SPECULAR_SETUP)
    half reflectivity = max(specularColor.r, max(specularColor.g, specularColor.b));
    return albedo * (1.0h - saturate(reflectivity));
    #else
    return albedo * (0.96h * (1.0h - saturate(metallic)));
    #endif
}

half3 NovaSpecularReflectance(half3 albedo, half metallic, half3 specularColor)
{
    #if defined(_SPECULAR_SETUP)
    return specularColor;
    #else
    return lerp(half3(0.04h, 0.04h, 0.04h), albedo, metallic);
    #endif
}

float NovaRoughness(half smoothness)
{
    float perceptualRoughness = 1.0 - saturate(smoothness);
    return max(perceptualRoughness * perceptualRoughness, 0.0078125);
}

half NovaGgxSpecular(float3 normalWS, float3 lightDirectionWS, float3 viewDirectionWS, half smoothness)
{
    // Same GGX distribution and joint visibility/Fresnel approximation used by
    // original NOVA's UniversalFragmentPBR, expressed without an SRP dependency.
    float3 lightDirection = NovaSafeNormalize(lightDirectionWS);
    float3 halfDirection = NovaSafeNormalize(lightDirection + NovaSafeNormalize(viewDirectionWS));
    float noH = saturate(dot(NovaSafeNormalize(normalWS), halfDirection));
    float loH = saturate(dot(lightDirection, halfDirection));
    float roughness = NovaRoughness(smoothness);
    float roughnessSquared = max(roughness * roughness, 0.00006103515625);
    float d = noH * noH * (roughnessSquared - 1.0) + 1.00001;
    return roughnessSquared / (d * d * max(0.1, loH * loH) * (roughness * 4.0 + 2.0));
}

NovaBuiltInMainLight NovaGetMainLight(float3 normalWS)
{
    NovaBuiltInMainLight lightData;
    lightData.direction = NovaGetMainLightDirection();
    lightData.color = NovaGetMainLightColor();
    lightData.attenuation = 1.0h;
    return lightData;
}

half3 NovaSampleAmbient(float3 normalWS)
{
    return ShadeSH9(half4(NovaSafeNormalize(normalWS), 1.0h));
}

half3 NovaComputeAmbientApproximation(float3 normalWS)
{
    half3 sh = NovaSampleAmbient(normalWS);
    return max(sh, UNITY_LIGHTMODEL_AMBIENT.rgb);
}

half3 NovaComputeAmbientApproximation(float3 normalWS, float3 positionWS)
{
    #if UNITY_LIGHT_PROBE_PROXY_VOLUME
    UNITY_BRANCH
    if (unity_ProbeVolumeParams.x == 1.0)
    {
        half4 normal = half4(NovaSafeNormalize(normalWS), 1);
        // Unity's proxy stores L0/L1 spatially; L2 remains the renderer's probe.
        // Replace L0/L1 instead of adding the ordinary SH result a second time.
        half3 ambient = max(0, SHEvalLinearL0L1_SampleProbeVolume(normal, positionWS) + SHEvalLinearL2(normal));
        #ifdef UNITY_COLORSPACE_GAMMA
        ambient = LinearToGammaSpace(ambient);
        #endif
        return ambient;
    }
    #endif
    return NovaComputeAmbientApproximation(normalWS);
}

half3 NovaComputeMinimumLitColor(half3 albedo, float3 normalWS)
{
    half diffuse = NovaComputeLambertDiffuse(normalWS, NovaGetMainLightDirection());
    half3 direct = NovaGetMainLightColor() * diffuse;
    half3 ambient = NovaComputeAmbientApproximation(normalWS);
    return albedo * saturate(direct + ambient);
}

float3 NovaReflectViewDirection(float3 viewDirectionWS, float3 normalWS)
{
    return NovaSafeNormalize(reflect(-NovaSafeNormalize(viewDirectionWS), NovaSafeNormalize(normalWS)));
}

half3 NovaSampleEnvironmentReflection(float3 reflectionDirectionWS, half smoothness)
{
    half perceptualRoughness = 1.0h - saturate(smoothness);
    half mip = perceptualRoughness * (1.7h - 0.7h * perceptualRoughness) * 6.0h;
    half4 encodedEnvironment = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, NovaSafeNormalize(reflectionDirectionWS), mip);
    return DecodeHDR(encodedEnvironment, unity_SpecCube0_HDR);
}

half3 NovaComputeEnvironmentReflectionStrength(
    half3 albedo,
    half metallic,
    half3 specularColor,
    half smoothness,
    half normalDotView)
{
    half3 reflectionColor = NovaSpecularReflectance(albedo, metallic, specularColor);
    #if defined(_SPECULAR_SETUP)
    half reflectivity = max(specularColor.r, max(specularColor.g, specularColor.b));
    #else
    half reflectivity = 0.04h + 0.96h * metallic;
    #endif
    float roughness = NovaRoughness(smoothness);
    half grazing = saturate(smoothness + reflectivity);
    half fresnel = 1.0h - saturate(normalDotView);
    fresnel *= fresnel;
    fresnel *= fresnel;
    return lerp(reflectionColor, grazing, fresnel) / (roughness * roughness + 1.0);
}

half3 NovaApplyEnvironmentReflection(half3 litColor, half3 environmentReflection)
{
    return litColor + environmentReflection;
}

#endif

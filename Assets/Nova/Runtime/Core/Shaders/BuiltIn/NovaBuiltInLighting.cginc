#ifndef NOVA_BUILTIN_LIGHTING_INCLUDED
#define NOVA_BUILTIN_LIGHTING_INCLUDED

#include "NovaBuiltInCore.cginc"
#include "Lighting.cginc"
#include "NovaBuiltInDebug.cginc"

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
    // ForwardBase supplies the normalized main direction, as consumed directly
    // by UnityStandardCore.MainLight. Punctual ForwardAdd still normalizes below.
    return _WorldSpaceLightPos0.xyz;
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
    // Surface resolvers provide a normalized world normal.
    return saturate(dot(normalWS, NovaSafeNormalize(lightDirectionWS)));
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
    float noH = saturate(dot(normalWS, halfDirection));
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

half3 NovaAmbientToActiveColorSpace(half3 irradiance)
{
    irradiance = max(0, irradiance);
    #ifdef UNITY_COLORSPACE_GAMMA
    // URP SampleSH uses piecewise sRGB, including its low-intensity linear segment.
    half3 high = 1.055h * pow(irradiance, 0.416666667h) - 0.055h;
    irradiance = irradiance <= 0.0031308h ? irradiance * 12.9232102h : high;
    #endif
    return irradiance;
}

#if defined(NOVA_VERTEX_GLOSSY_AMBIENT)
static half4 novaInterpolatedGlossyAmbient = 0;
#endif

half3 NovaSampleAmbientUnitNormal(float3 normalWS)
{
    half4 normal = half4(normalWS, 1);
    return NovaAmbientToActiveColorSpace(SHEvalLinearL0L1(normal) + SHEvalLinearL2(normal));
}

half3 NovaSampleAmbient(float3 normalWS)
{
    return NovaSampleAmbientUnitNormal(NovaSafeNormalize(normalWS));
}

half3 NovaComputeAmbientApproximation(float3 normalWS)
{
    // Original SampleSHPixel clamps negative irradiance, without a scene-color floor.
    return max(0, NovaSampleAmbient(normalWS));
}

half3 NovaComputeGlossyAmbient()
{
    // Original keeps DC glossy illumination even with reflection maps disabled.
    // Built-in's indirect-specular RGB can retain a previous skybox when switching
    // to Flat/Trilight. Recover DC from the current renderer SH instead. Packing
    // puts part of L2 into A.w, so remove it. The native alpha is reflection intensity.
    half3 dc = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w)
        + half3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0h;
    half3 ambient = max(0, dc) * unity_IndirectSpecColor.a;
    #ifdef UNITY_COLORSPACE_GAMMA
    // Original URP sets glossy DC through C# Color.gamma, whose HDR segment
    // differs from the shader's diffuse sRGB conversion above.
    ambient = half3(LinearToGammaSpaceExact(ambient.r), LinearToGammaSpaceExact(ambient.g), LinearToGammaSpaceExact(ambient.b));
    #endif
    return ambient;
}

half3 NovaSampleGlossyAmbient()
{
    #if defined(NOVA_VERTEX_GLOSSY_AMBIENT)
    return novaInterpolatedGlossyAmbient.rgb;
    #else
    return NovaComputeGlossyAmbient();
    #endif
}

half3 NovaComputeAmbientApproximation(float3 normalWS, float3 positionWS)
{
    half3 result = 0;
    #if UNITY_LIGHT_PROBE_PROXY_VOLUME
    UNITY_BRANCH
    if (unity_ProbeVolumeParams.x == 1.0)
    {
        half4 normal = half4(normalWS, 1);
        // Unity's proxy stores L0/L1 spatially; L2 remains the renderer's probe.
        // Replace L0/L1 instead of adding the ordinary SH result a second time.
        result = NovaAmbientToActiveColorSpace(SHEvalLinearL0L1_SampleProbeVolume(normal, positionWS) + SHEvalLinearL2(normal));
    }
    else
    #endif
    {
        result = NovaSampleAmbientUnitNormal(normalWS);
    }
    return result;
}

half3 NovaComputeMinimumLitColor(half3 albedo, float3 normalWS)
{
    normalWS = NovaSafeNormalize(normalWS);
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

half NovaEnvironmentNormalDotView(float3 unitNormalWS, float3 viewDirectionWS)
{
    #if !defined(_ENVIRONMENT_REFLECTIONS_ENABLED) && !defined(_SPECULAR_HIGHLIGHTS_ENABLED) && !defined(_TINT_AREA_RIM) && !defined(_TRANSPARENCY_BY_RIM)
    // Only NoV is needed: move the reciprocal length outside the dot product.
    return dot(unitNormalWS, viewDirectionWS) * rsqrt(max(dot(viewDirectionWS, viewDirectionWS), 1.0e-8));
    #else
    return dot(unitNormalWS, NovaSafeNormalize(viewDirectionWS));
    #endif
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
    #if defined(NOVA_VERTEX_GLOSSY_AMBIENT) && !defined(_SMOOTHNESS_MAP_ENABLED) && ((!defined(_SPECULAR_SETUP) && !defined(_METALLIC_MAP_ENABLED)) || (defined(_SPECULAR_SETUP) && !defined(_SPECULAR_MAP_ENABLED)))
    half grazing = novaInterpolatedGlossyAmbient.w;
    #else
    half grazing = saturate(smoothness + reflectivity);
    #endif
    half fresnel = 1.0h - saturate(normalDotView);
    fresnel *= fresnel;
    fresnel *= fresnel;
    #if defined(NOVA_VERTEX_GLOSSY_AMBIENT) && !defined(_SMOOTHNESS_MAP_ENABLED)
    // Uniform surface reduction is already multiplied into the vertex DC.
    return lerp(reflectionColor, grazing, fresnel);
    #else
    return lerp(reflectionColor, grazing, fresnel) / (roughness * roughness + 1.0);
    #endif
}

half3 NovaApplyEnvironmentReflection(half3 litColor, half3 environmentReflection)
{
    return litColor + environmentReflection;
}

#endif

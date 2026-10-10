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

half NovaNormalizedBlinnPhongLobe(half ndoth, half exponent)
{
    // Normalized Blinn-Phong distribution with the quarter microfacet factor.
    // This remains a Built-in approximation of URP's GGX distribution.
    return pow(ndoth, exponent) * ((exponent + 2.0h) / 25.13274123h);
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
    half mip = lerp(6.0h, 0.0h, saturate(smoothness));
    half4 encodedEnvironment = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, NovaSafeNormalize(reflectionDirectionWS), mip);
    return DecodeHDR(encodedEnvironment, unity_SpecCube0_HDR);
}

half3 NovaComputeEnvironmentReflectionStrength(
    half3 albedo,
    half metallic,
    half3 specularColor,
    half smoothness)
{
    half clampedSmoothness = saturate(smoothness);

    #if defined(_SPECULAR_SETUP)
    half3 reflectionColor = saturate(specularColor);
    #else
    half3 reflectionColor = lerp(half3(0.04h, 0.04h, 0.04h), albedo, saturate(metallic));
    #endif

    half workflowStrength = lerp(0.30h, 1.0h, saturate(metallic));
    return reflectionColor * workflowStrength * clampedSmoothness;
}

half3 NovaApplyEnvironmentReflection(half3 litColor, half3 environmentReflection)
{
    return litColor + environmentReflection;
}

#endif

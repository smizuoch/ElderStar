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

half NovaComputeLambertDiffuse(float3 normalWS, float3 lightDirectionWS)
{
    return saturate(dot(NovaSafeNormalize(normalWS), NovaSafeNormalize(lightDirectionWS)));
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

#endif

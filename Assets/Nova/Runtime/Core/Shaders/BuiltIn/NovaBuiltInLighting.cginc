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

NovaBuiltInMainLight NovaGetMainLight(float3 normalWS)
{
    NovaBuiltInMainLight lightData;
    lightData.direction = NovaSafeNormalize(_WorldSpaceLightPos0.xyz);
    lightData.color = _LightColor0.rgb;
    lightData.attenuation = 1.0h;
    return lightData;
}

half3 NovaSampleAmbient(float3 normalWS)
{
    return ShadeSH9(half4(NovaSafeNormalize(normalWS), 1.0h));
}

#endif

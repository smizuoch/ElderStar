#ifndef NOVA_BUILTIN_CANVAS_LIGHTING_INCLUDED
#define NOVA_BUILTIN_CANVAS_LIGHTING_INCLUDED
#if defined(NOVA_CANVAS_LIGHTING)
int _NovaCanvasLightCount;
float4 _NovaCanvasLightPositions[8];
half4 _NovaCanvasLightColors[8];
half4 _NovaCanvasLightDirections[8];
float4 _NovaCanvasLightAttenuation[8];

half3 NovaCanvasDirectLighting(NovaUILitSurfaceData surface, float3 positionWS, float3 viewDirectionWS)
{
    half3 result = 0;
    [loop] for (int index = 0; index < min(_NovaCanvasLightCount, 8); index++)
    {
        float3 vectorToLight = _NovaCanvasLightPositions[index].xyz - positionWS * _NovaCanvasLightPositions[index].w;
        float distanceSquared = max(dot(vectorToLight, vectorToLight), 0.0001);
        float3 direction = vectorToLight * rsqrt(distanceSquared);
        float4 parameters = _NovaCanvasLightAttenuation[index];
        half factor = distanceSquared * parameters.x;
        half smoothing = saturate(1.0h - factor * factor);
        float attenuation = smoothing * smoothing / distanceSquared;
        half angle = saturate(dot(_NovaCanvasLightDirections[index].xyz, direction) * parameters.y + parameters.z);
        attenuation *= angle * angle;
        half ndotl = NovaComputeLambertDiffuse(surface.normalWS, direction);
        half3 brdf = NovaComputeDiffuseAlbedo(surface.albedo, surface.metallic, surface.specularColor);
        #if defined(_SPECULAR_HIGHLIGHTS_ENABLED)
        brdf += NovaSpecularReflectance(surface.albedo, surface.metallic, surface.specularColor)
            * NovaGgxSpecular(surface.normalWS, direction, viewDirectionWS, surface.smoothness);
        #endif
        result += brdf * _NovaCanvasLightColors[index].rgb * (ndotl * attenuation);
    }
    return result;
}
#endif
#endif

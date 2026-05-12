#ifndef NOVA_BUILTIN_DEPTH_INCLUDED
#define NOVA_BUILTIN_DEPTH_INCLUDED

#include "NovaBuiltInCore.cginc"

UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;

float NovaSampleRawCameraDepth(float2 uv)
{
    return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
}

float NovaLinearEyeDepth(float rawDepth)
{
    return LinearEyeDepth(rawDepth);
}

float NovaSoftParticleFactor(float4 screenPosition, float intensity)
{
    float2 uv = NovaScreenUV(screenPosition);
    float sceneDepth = NovaLinearEyeDepth(NovaSampleRawCameraDepth(uv));
    float particleDepth = NovaLinearEyeDepth(screenPosition.z / max(screenPosition.w, 1.0e-6));
    return saturate((sceneDepth - particleDepth) / max(intensity, 1.0e-4));
}

float NovaDepthFadeFactor(float nearDistance, float farDistance, float width, float4 screenPosition)
{
    float particleDepth = NovaLinearEyeDepth(screenPosition.z / max(screenPosition.w, 1.0e-6));
    float distanceToRange = min(particleDepth - nearDistance, farDistance - particleDepth);
    return saturate(distanceToRange / max(width, 1.0e-4));
}

#endif

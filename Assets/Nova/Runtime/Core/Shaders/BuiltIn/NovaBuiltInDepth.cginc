#ifndef NOVA_BUILTIN_DEPTH_INCLUDED
#define NOVA_BUILTIN_DEPTH_INCLUDED

#include "NovaBuiltInCore.cginc"

UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;

float NovaSampleSceneDepth01(float2 uv)
{
    return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
}

float NovaSampleRawCameraDepth(float2 uv)
{
    return NovaSampleSceneDepth01(uv);
}

float NovaLinearEyeDepth(float rawDepth)
{
    if (unity_OrthoParams.w > 0.5)
    {
        float depth01 = rawDepth;
        #if defined(UNITY_REVERSED_Z)
        depth01 = 1.0 - depth01;
        #endif
        return lerp(_ProjectionParams.y, _ProjectionParams.z, saturate(depth01));
    }

    return LinearEyeDepth(rawDepth);
}

float NovaSampleSceneDepthEye(float2 uv)
{
    return NovaLinearEyeDepth(NovaSampleSceneDepth01(uv));
}

float NovaFragmentEyeDepth(float4 screenPosition)
{
    return screenPosition.z;
}

float NovaComputeSoftParticleFactor(float sceneDepth, float particleDepth, float intensity)
{
    return saturate((sceneDepth - particleDepth) / max(intensity, 1.0e-4));
}

float NovaSoftParticleFactor(float4 screenPosition, float intensity)
{
    float2 uv = NovaScreenUV(screenPosition);
    float sceneDepth = NovaSampleSceneDepthEye(uv);
    float particleDepth = NovaFragmentEyeDepth(screenPosition);
    return NovaComputeSoftParticleFactor(sceneDepth, particleDepth, intensity);
}

float NovaComputeDepthFadeFactor(float particleDepth, float nearDistance, float farDistance, float width)
{
    float distanceToRange = min(particleDepth - nearDistance, farDistance - particleDepth);
    return saturate(distanceToRange / max(width, 1.0e-4));
}

float NovaDepthFadeFactor(float nearDistance, float farDistance, float width, float4 screenPosition)
{
    return NovaComputeDepthFadeFactor(NovaFragmentEyeDepth(screenPosition), nearDistance, farDistance, width);
}

#endif

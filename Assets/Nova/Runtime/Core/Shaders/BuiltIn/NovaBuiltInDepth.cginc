#ifndef NOVA_BUILTIN_DEPTH_INCLUDED
#define NOVA_BUILTIN_DEPTH_INCLUDED

#include "NovaBuiltInCore.cginc"

#if UNITY_VERSION >= 600000 && defined(UNITY_SEPARATE_TEXTURE_SAMPLER)
// URP 17's deprecated sampler_CameraDepthTexture alias uses PointClamp.
// Preserve that filtering for original NOVA soft particles, including XR arrays.
    #if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
    Texture2DArray<float> _CameraDepthTexture;
    #else
    Texture2D<float> _CameraDepthTexture;
    #endif
SamplerState nova_point_clamp_sampler;
#else
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
#endif
float4 _CameraDepthTexture_TexelSize;

float NovaSampleSceneDepth01(float2 uv)
{
    #if UNITY_VERSION >= 600000 && defined(UNITY_SEPARATE_TEXTURE_SAMPLER)
        #if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
        return _CameraDepthTexture.SampleLevel(nova_point_clamp_sampler, float3(uv, unity_StereoEyeIndex), 0);
        #else
        return _CameraDepthTexture.SampleLevel(nova_point_clamp_sampler, uv, 0);
        #endif
    #else
    return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
    #endif
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

// Original NOVA linearizes both projected depths with _ZBufferParams, even
// for orthographic/oblique cameras. Keep physical depth helpers for occlusion.
float NovaSoftParticleFactor(float4 screenPosition, half intensity, float rawDepth)
{
    float sceneDepth = LinearEyeDepth(NovaSampleSceneDepth01(NovaScreenUV(screenPosition)));
    float particleDepth = LinearEyeDepth(rawDepth);
    half inverseIntensity = 1.0 / intensity;
    return saturate((sceneDepth - particleDepth) * inverseIntensity);
}

// Legacy generated source stores physical eye depth in screenPosition.z.
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
    // Original NOVA accepts signed widths, including values below 1e-4.
    return saturate(distanceToRange / width);
}

float NovaDepthFadeFactor(float nearDistance, float farDistance, float width, float rawDepth)
{
    // Match Particles.hlsl, including its perspective linearization on ortho
    // and oblique projections. SV_POSITION.z already contains clip z / w on D3D.
    return NovaComputeDepthFadeFactor(LinearEyeDepth(rawDepth), nearDistance, farDistance, width);
}

// Legacy generated Built-in shaders carried eye depth in screenPosition.z.
// Keep that source contract; newly generated shaders use the scalar overload.
float NovaDepthFadeFactor(float nearDistance, float farDistance, float width, float4 screenPosition)
{
    float distanceToRange = min(screenPosition.z - nearDistance, farDistance - screenPosition.z);
    return saturate(distanceToRange / max(width, 1.0e-4));
}

#endif

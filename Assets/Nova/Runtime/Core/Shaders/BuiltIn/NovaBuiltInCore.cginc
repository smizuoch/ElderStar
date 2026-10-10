#ifndef NOVA_BUILTIN_CORE_INCLUDED
#define NOVA_BUILTIN_CORE_INCLUDED

#include "UnityCG.cginc"
#include "NovaBuiltInParticleInstancing.cginc"

float3 NovaObjectToWorld(float3 positionOS)
{
    return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
}

float4 NovaObjectToClipPos(float4 positionOS)
{
    return UnityObjectToClipPos(positionOS);
}

float4 NovaObjectToClipPos(float3 positionOS)
{
    return UnityObjectToClipPos(float4(positionOS, 1.0));
}

float4 NovaWorldToClipPos(float3 positionWS)
{
    return mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
}

float3 NovaObjectToWorldNormal(float3 normalOS)
{
    return normalize(UnityObjectToWorldNormal(normalOS));
}

float3 NovaObjectToWorldTangent(float3 tangentOS)
{
    return normalize(mul((float3x3)unity_ObjectToWorld, tangentOS));
}

float3 NovaCreateBitangent(float3 normalWS, float3 tangentWS, float tangentSign)
{
    return cross(normalWS, tangentWS) * (tangentSign * unity_WorldTransformParams.w);
}

float3 NovaWorldSpaceViewDir(float3 positionWS)
{
    // Match the original pipeline's constant direction for orthographic cameras.
    return unity_OrthoParams.w > 0.5 ? UNITY_MATRIX_V[2].xyz : _WorldSpaceCameraPos.xyz - positionWS;
}

float3 NovaSafeNormalize(float3 value)
{
    return value * rsqrt(max(dot(value, value), 1.0e-8));
}

float2 NovaSafeNormalize(float2 value)
{
    return value * rsqrt(max(dot(value, value), 1.0e-8));
}

float4 NovaComputeScreenPos(float4 positionCS)
{
    return ComputeScreenPos(positionCS);
}

float2 NovaScreenUV(float4 screenPosition)
{
    return screenPosition.xy / max(screenPosition.w, 1.0e-6);
}

#endif

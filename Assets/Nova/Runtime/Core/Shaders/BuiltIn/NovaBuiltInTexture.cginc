#ifndef NOVA_BUILTIN_TEXTURE_INCLUDED
#define NOVA_BUILTIN_TEXTURE_INCLUDED

#include "NovaBuiltInCore.cginc"

#define NOVA_DECLARE_TEX2D(textureName) UNITY_DECLARE_TEX2D(textureName); float4 textureName##_ST
#define NOVA_DECLARE_TEX2D_NOST(textureName) UNITY_DECLARE_TEX2D(textureName)
#define NOVA_SAMPLE_TEX2D(textureName, uv) UNITY_SAMPLE_TEX2D(textureName, uv)
#if defined(UNITY_SEPARATE_TEXTURE_SAMPLER)
#define NOVA_SAMPLE_TEX2D_LOD(textureName, uv, lod) UNITY_SAMPLE_TEX2D_LOD(textureName, uv, lod)
#else
#define NOVA_SAMPLE_TEX2D_LOD(textureName, uv, lod) tex2Dlod(textureName, float4((uv), 0.0, (lod)))
#endif

#define NOVA_DECLARE_TEX2D_ARRAY(textureName) UNITY_DECLARE_TEX2DARRAY(textureName)
#define NOVA_SAMPLE_TEX2D_ARRAY(textureName, uv, slice) UNITY_SAMPLE_TEX2DARRAY(textureName, float3((uv), (slice)))

#define NOVA_DECLARE_TEX3D(textureName) UNITY_DECLARE_TEX3D(textureName); float4 textureName##_ST
#define NOVA_SAMPLE_TEX3D(textureName, uvw) UNITY_SAMPLE_TEX3D(textureName, uvw)
#define NOVA_SAMPLE_TEX3D_LOD(textureName, uvw, lod) UNITY_SAMPLE_TEX3D_LOD(textureName, uvw, lod)

// Original NOVA shares the selected base-map sampler with the Lit surface maps.
// Keep separate resource/sampler declarations so their importer wrap/filter settings
// cannot override the material's mirror sampler on modern graphics backends.
#if defined(_BASE_SAMPLER_STATE_POINT_MIRROR)
#define NOVA_BASE_SAMPLER sampler_point_mirror
#elif defined(_BASE_SAMPLER_STATE_LINEAR_MIRROR)
#define NOVA_BASE_SAMPLER sampler_linear_mirror
#elif defined(_BASE_SAMPLER_STATE_TRILINEAR_MIRROR)
#define NOVA_BASE_SAMPLER sampler_trilinear_mirror
#endif

#if defined(NOVA_BASE_SAMPLER) && defined(UNITY_SEPARATE_TEXTURE_SAMPLER)
SamplerState NOVA_BASE_SAMPLER;
#define NOVA_SAMPLE_BASE_TEX2D(tex, uv) tex.Sample(NOVA_BASE_SAMPLER, uv)
#define NOVA_SAMPLE_BASE_TEX2D_ARRAY(tex, uv, slice) tex.Sample(NOVA_BASE_SAMPLER, float3(uv, slice))
#define NOVA_SAMPLE_BASE_TEX3D_LOD(tex, uvw, lod) tex.SampleLevel(NOVA_BASE_SAMPLER, uvw, lod)
#elif defined(NOVA_BASE_SAMPLER)
// Coupled-sampler backends retain texture filtering; mirror the coordinates locally.
#define NOVA_SAMPLE_BASE_TEX2D(tex, uv) NOVA_SAMPLE_TEX2D(tex, NovaMirrorUV(uv))
#define NOVA_SAMPLE_BASE_TEX2D_ARRAY(tex, uv, slice) NOVA_SAMPLE_TEX2D_ARRAY(tex, NovaMirrorUV(uv), slice)
#define NOVA_SAMPLE_BASE_TEX3D_LOD(tex, uvw, lod) NOVA_SAMPLE_TEX3D_LOD(tex, float3(NovaMirrorUV((uvw).xy), (uvw).z), lod)
#else
#define NOVA_SAMPLE_BASE_TEX2D(tex, uv) NOVA_SAMPLE_TEX2D(tex, uv)
#define NOVA_SAMPLE_BASE_TEX2D_ARRAY(tex, uv, slice) NOVA_SAMPLE_TEX2D_ARRAY(tex, uv, slice)
#define NOVA_SAMPLE_BASE_TEX3D_LOD(tex, uvw, lod) NOVA_SAMPLE_TEX3D_LOD(tex, uvw, lod)
#endif

float2 NovaTransformUV(float2 uv, float4 scaleOffset)
{
    return uv * scaleOffset.xy + scaleOffset.zw;
}

float2 NovaMirrorUV(float2 uv)
{
    float2 repeated = frac(uv * 0.5) * 2.0;
    return lerp(repeated, 2.0 - repeated, step(1.0, repeated));
}

half NovaSelectChannel(half4 value, half channel)
{
    if (channel < 0.5h)
    {
        return value.r;
    }

    if (channel < 1.5h)
    {
        return value.g;
    }

    if (channel < 2.5h)
    {
        return value.b;
    }

    return value.a;
}

half2 NovaSelectChannels(half4 value, half2 channels)
{
    return half2(NovaSelectChannel(value, channels.x), NovaSelectChannel(value, channels.y));
}

half4 NovaSampleTexture2DMirror(sampler2D textureName, float2 uv)
{
    return tex2D(textureName, NovaMirrorUV(uv));
}

#endif

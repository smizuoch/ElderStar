#ifndef NOVA_BUILTIN_TEXTURE_INCLUDED
#define NOVA_BUILTIN_TEXTURE_INCLUDED

#include "NovaBuiltInCore.cginc"

#define NOVA_DECLARE_TEX2D(textureName) sampler2D textureName; float4 textureName##_ST
#define NOVA_DECLARE_TEX2D_NOST(textureName) sampler2D textureName
#define NOVA_SAMPLE_TEX2D(textureName, uv) tex2D(textureName, uv)
#define NOVA_SAMPLE_TEX2D_LOD(textureName, uv, lod) tex2Dlod(textureName, float4((uv), 0.0, (lod)))

#define NOVA_DECLARE_TEX2D_ARRAY(textureName) UNITY_DECLARE_TEX2DARRAY(textureName)
#define NOVA_SAMPLE_TEX2D_ARRAY(textureName, uv, slice) UNITY_SAMPLE_TEX2DARRAY(textureName, float3((uv), (slice)))

#define NOVA_DECLARE_TEX3D(textureName) sampler3D textureName; float4 textureName##_ST
#define NOVA_SAMPLE_TEX3D(textureName, uvw) tex3D(textureName, uvw)
#define NOVA_SAMPLE_TEX3D_LOD(textureName, uvw, lod) tex3Dlod(textureName, float4((uvw), (lod)))

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

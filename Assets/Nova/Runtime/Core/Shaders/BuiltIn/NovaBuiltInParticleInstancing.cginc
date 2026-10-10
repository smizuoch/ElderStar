#ifndef NOVA_BUILTIN_PARTICLE_INSTANCING_INCLUDED
#define NOVA_BUILTIN_PARTICLE_INSTANCING_INCLUDED

// Preserve NOVA's custom ParticleSystem instance layout. Matrix and packed-color
// decoding use Unity's Built-in implementation, with no SRP includes.
struct NovaParticleInstanceData
{
    float3x4 transform;
    uint color;
    float4 customCoord1;
    float4 customCoord2;
};
#define UNITY_PARTICLE_INSTANCE_DATA NovaParticleInstanceData
#define UNITY_PARTICLE_INSTANCE_DATA_NO_ANIM_FRAME
#include "UnityStandardParticleInstancing.cginc"

void NovaParticleCustomData(inout float4 coord1, inout float4 coord2)
{
    #if defined(UNITY_PARTICLE_INSTANCING_ENABLED)
    NovaParticleInstanceData data = unity_ParticleInstanceData[unity_InstanceID];
    coord1 = data.customCoord1;
    coord2 = data.customCoord2;
    #endif
}

#define NOVA_SETUP_PARTICLE(v) UNITY_SETUP_INSTANCE_ID(v); \
    vertInstancingColor(v.color); \
    NovaParticleCustomData(v.customCoord1, v.customCoord2);

#endif

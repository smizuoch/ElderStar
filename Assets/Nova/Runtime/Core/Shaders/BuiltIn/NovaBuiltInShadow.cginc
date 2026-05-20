#ifndef NOVA_BUILTIN_SHADOW_INCLUDED
#define NOVA_BUILTIN_SHADOW_INCLUDED

#include "UnityCG.cginc"
#include "AutoLight.cginc"

float4 NovaShadowCasterClipPos(float4 positionOS, float3 normalOS)
{
    return UnityApplyLinearShadowBias(UnityClipSpaceShadowCasterPos(positionOS, normalOS));
}

fixed4 NovaShadowCasterFragment()
{
    return 0;
}

#define NOVA_SHADOW_COORDS(index) SHADOW_COORDS(index)
#define NovaTransferShadowCoord(output, positionWS) TRANSFER_SHADOW_WPOS(output, positionWS)
#define NovaComputeShadowAttenuation(input, positionWS) SHADOW_ATTENUATION(input)

half NovaApplyReceiveShadow(half shadowAttenuation, half receiveShadows)
{
    return lerp(1.0h, saturate(shadowAttenuation), saturate(receiveShadows));
}

#endif

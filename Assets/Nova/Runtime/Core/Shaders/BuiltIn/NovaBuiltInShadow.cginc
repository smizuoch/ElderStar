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

#endif

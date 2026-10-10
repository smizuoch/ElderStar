#ifndef NOVA_BUILTIN_FOG_INCLUDED
#define NOVA_BUILTIN_FOG_INCLUDED

#include "UnityCG.cginc"

#define NOVA_FOG_COORDS(index) UNITY_FOG_COORDS(index)
#define NOVA_TRANSFER_FOG(output, positionCS) UNITY_TRANSFER_FOG(output, positionCS)
#define NOVA_APPLY_FOG(fogCoord, color) UNITY_APPLY_FOG(fogCoord, color)

void NovaApplyEyeFog(inout half4 color, float eyeDepth)
{
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    // Reuse world position instead of consuming another SM3.5 interpolator.
    // Eye depth also works for orthographic cameras, whose clip Z does not
    // have the perspective near..far range assumed by UNITY_CALC_FOG_FACTOR.
    float fogDepth = max(eyeDepth - _ProjectionParams.y, 0.0);
    UNITY_CALC_FOG_FACTOR_RAW(fogDepth);
    #if defined(UNITY_PASS_FORWARDADD)
    color.rgb *= saturate(unityFogFactor);
    #else
    color.rgb = lerp(unity_FogColor.rgb, color.rgb, saturate(unityFogFactor));
    #endif
    #endif
}

void NovaApplyWorldFog(inout half4 color, float3 positionWS)
{
    NovaApplyEyeFog(color, -mul(UNITY_MATRIX_V, float4(positionWS, 1.0)).z);
}

#endif

#ifndef NOVA_BUILTIN_FOG_INCLUDED
#define NOVA_BUILTIN_FOG_INCLUDED

#include "UnityCG.cginc"

#define NOVA_FOG_COORDS(index) UNITY_FOG_COORDS(index)
#define NOVA_TRANSFER_FOG(output, positionCS) UNITY_TRANSFER_FOG(output, positionCS)
#define NOVA_APPLY_FOG(fogCoord, color) UNITY_APPLY_FOG(fogCoord, color)

// Original NOVA explicitly uses the vertex fog factor (not URP's optional
// fragment-fog helper). Preserve its clip-space mapping and EXP coefficient.
float NovaVertexFog(float clipZ)
{
    float depth = UNITY_Z_0_FAR_FROM_CLIPSPACE(clipZ);
    #if defined(FOG_LINEAR)
    return saturate(depth * unity_FogParams.z + unity_FogParams.w);
    #elif defined(FOG_EXP) || defined(FOG_EXP2)
    return unity_FogParams.x * depth;
    #else
    return 0;
    #endif
}

void NovaApplyVertexFog(inout half4 color, float factor)
{
    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    #if defined(FOG_EXP)
    factor = exp2(-factor);
    #elif defined(FOG_EXP2)
    factor = exp2(-factor * factor);
    #endif
    #if defined(UNITY_PASS_FORWARDADD)
    color.rgb *= saturate(factor);
    #else
    color.rgb = lerp(unity_FogColor.rgb, color.rgb, saturate(factor));
    #endif
    #endif
}

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

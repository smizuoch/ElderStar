#ifndef NOVA_BUILTIN_DEBUG_INCLUDED
#define NOVA_BUILTIN_DEBUG_INCLUDED
#if defined(NOVA_DEBUG_DISPLAY)
int _NovaDebugMode;
static half3 novaDebugDirect = 0;
static half3 novaDebugIndirect = 0;

half4 NovaDebugColor(half4 albedo, float3 normalWS, float3 tangentWS, float eyeDepth,
    float fog, half3 direct, half3 indirect, half additionalCount)
{
    if (_NovaDebugMode == 1) return albedo;
    if (_NovaDebugMode == 2) return half4(normalize(normalWS) * .5 + .5, albedo.a);
    if (_NovaDebugMode == 3) return half4(normalize(tangentWS) * .5 + .5, albedo.a);
    if (_NovaDebugMode == 4) return half4(saturate(eyeDepth / _ProjectionParams.z).xxx, albedo.a);
    if (_NovaDebugMode == 5)
    {
        #if defined(FOG_EXP)
        fog = exp2(-fog);
        #elif defined(FOG_EXP2)
        fog = exp2(-fog * fog);
        #elif !defined(FOG_LINEAR)
        fog = 1;
        #endif
        return half4(saturate(fog).xxx, albedo.a);
    }
    if (_NovaDebugMode == 6) return half4(direct, albedo.a);
    if (_NovaDebugMode == 7) return half4(indirect, albedo.a);
    return half4((additionalCount / 8.0h).xxx, albedo.a);
}
#endif
#endif

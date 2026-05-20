#ifndef NOVA_BUILTIN_UBER_LIT_INCLUDED
#define NOVA_BUILTIN_UBER_LIT_INCLUDED

half4 NovaResolveMinimumLitBaseColor(
    sampler2D baseMap,
    float2 uv,
    half4 tintColor,
    half4 vertexColor)
{
    half4 color = NOVA_SAMPLE_TEX2D(baseMap, uv);

    #if defined(_TINT_AREA_ALL) && defined(_TINT_COLOR_ENABLED)
    color *= tintColor;
    #endif

    color *= vertexColor;
    return color;
}

half4 NovaApplyMinimumLitForward(half4 baseColor, float3 normalWS, half cutoff)
{
    #if defined(_ALPHATEST_ENABLED)
    clip(baseColor.a - cutoff);
    #endif

    half3 litColor = NovaComputeMinimumLitColor(baseColor.rgb, normalWS);
    return half4(litColor, baseColor.a);
}

#endif

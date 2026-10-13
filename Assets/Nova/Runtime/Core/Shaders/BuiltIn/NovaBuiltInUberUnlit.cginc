#ifndef NOVA_BUILTIN_UBER_UNLIT_INCLUDED
#define NOVA_BUILTIN_UBER_UNLIT_INCLUDED

half NovaGetCustomCoord(half coord, float4 customCoord1, float4 customCoord2)
{
    uint value = (uint)round(coord);
    if (value == 1u) return customCoord1.x;
    if (value == 11u) return customCoord1.y;
    if (value == 21u) return customCoord1.z;
    if (value == 31u) return customCoord1.w;
    if (value == 2u) return customCoord2.x;
    if (value == 12u) return customCoord2.y;
    if (value == 22u) return customCoord2.z;
    if (value == 32u) return customCoord2.w;
    return 0.0h;
}

half NovaFlipBookProgress(half progress, half sliceCount)
{
    return floor(clamp(progress, 0.0h, 0.999h) * max(sliceCount, 1.0h));
}

half NovaFlipBookBlendingProgress(half progress, half sliceCount)
{
    half safeSliceCount = max(sliceCount, 1.0h);
    half offset = 1.0h / safeSliceCount * 0.5h;
    return lerp(offset, 1.0h - offset, saturate(progress));
}

half NovaFlipBookProgressWithRandomRow(half progress, half sliceCount, half rowCount, half randomValue)
{
    half safeRowCount = max(rowCount, 1.0h);
    half framesPerRow = max(sliceCount, 1.0h) / safeRowCount;
    half selectedRow = clamp(floor(randomValue), 0.0h, safeRowCount - 1.0h);
    return selectedRow * framesPerRow + NovaFlipBookProgress(progress, framesPerRow);
}

half NovaFlipBookBlendingProgressWithRandomRow(half progress, half sliceCount, half rowCount, half randomValue)
{
    half safeSliceCount = max(sliceCount, 1.0h);
    half safeRowCount = max(rowCount, 1.0h);
    half framesPerRow = safeSliceCount / safeRowCount;
    half selectedRow = clamp(floor(randomValue), 0.0h, safeRowCount - 1.0h);
    // Custom Data can take progress outside 0..1. Original NOVA clamps only
    // the normalized frame index, after the selected-row offset is applied.
    half frameIndex = selectedRow * framesPerRow + progress * framesPerRow;
    half offset = 1.0h / safeSliceCount * 0.5h;
    return clamp((frameIndex + 0.5h) / safeSliceCount, offset, 1.0h - offset);
}

half NovaInverseLerpSafe(half a, half b, half value)
{
    return saturate((value - a) / max(b - a, 0.0001h));
}

float2 NovaApplyBaseMirror(float2 uv)
{
    #if defined(_BASE_SAMPLER_STATE_POINT_MIRROR) || defined(_BASE_SAMPLER_STATE_LINEAR_MIRROR) || defined(_BASE_SAMPLER_STATE_TRILINEAR_MIRROR)
    return NovaMirrorUV(uv);
    #else
    return uv;
    #endif
}

float2 NovaRotateUV(float2 uv, half angle, half2 offsets)
{
    half angleCos = cos(angle);
    half angleSin = sin(angle);
    half2x2 rotateMatrix = half2x2(angleCos, -angleSin, angleSin, angleCos);
    half2 uvOffsets = 0.5h - offsets;
    return mul(uv - uvOffsets, rotateMatrix) + uvOffsets;
}

half NovaBaseMapProgress(
    half progress,
    half sliceCount,
    half rowCount,
    half randomValue)
{
    #if defined(_BASE_MAP_MODE_2D_ARRAY)
        #if defined(_BASE_MAP_RANDOM_ROW_SELECTION_ENABLED)
        return NovaFlipBookProgressWithRandomRow(progress, sliceCount, rowCount, randomValue);
        #else
        return NovaFlipBookProgress(progress, sliceCount);
        #endif
    #elif defined(_BASE_MAP_MODE_3D)
        #if defined(_BASE_MAP_RANDOM_ROW_SELECTION_ENABLED)
        return NovaFlipBookBlendingProgressWithRandomRow(progress, sliceCount, rowCount, randomValue);
        #else
        return NovaFlipBookBlendingProgress(progress, sliceCount);
        #endif
    #else
    return 0.0h;
    #endif
}

half NovaTintMapProgress(
    half progress2DArray,
    half progress3D,
    half sliceCount,
    half rowCount,
    half randomValue)
{
    #if defined(_TINT_MAP_MODE_2D_ARRAY)
        #if defined(_TINT_MAP_RANDOM_ROW_SELECTION_ENABLED)
        return NovaFlipBookProgressWithRandomRow(progress2DArray, sliceCount, rowCount, randomValue);
        #else
        return NovaFlipBookProgress(progress2DArray, sliceCount);
        #endif
    #elif defined(_TINT_MAP_3D_ENABLED)
        #if defined(_TINT_MAP_RANDOM_ROW_SELECTION_ENABLED)
        return NovaFlipBookBlendingProgressWithRandomRow(progress3D, sliceCount, rowCount, randomValue);
        #else
        return NovaFlipBookBlendingProgress(progress3D, sliceCount);
        #endif
    #else
    return 0.0h;
    #endif
}

half NovaAlphaTransitionMapProgress(
    half progress,
    half sliceCount,
    half rowCount,
    half randomValue)
{
    #if defined(_ALPHA_TRANSITION_MAP_MODE_2D_ARRAY)
        #if defined(_ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED)
        return NovaFlipBookProgressWithRandomRow(progress, sliceCount, rowCount, randomValue);
        #else
        return NovaFlipBookProgress(progress, sliceCount);
        #endif
    #elif defined(_ALPHA_TRANSITION_MAP_MODE_3D)
        #if defined(_ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED)
        return NovaFlipBookBlendingProgressWithRandomRow(progress, sliceCount, rowCount, randomValue);
        #else
        return NovaFlipBookBlendingProgress(progress, sliceCount);
        #endif
    #else
    return 0.0h;
    #endif
}

half NovaAlphaTransitionSecondMapProgress(
    half progress,
    half sliceCount,
    half rowCount,
    half randomValue)
{
    #if defined(_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY)
        #if defined(_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_RANDOM_ROW_SELECTION_ENABLED)
        return NovaFlipBookProgressWithRandomRow(progress, sliceCount, rowCount, randomValue);
        #else
        return NovaFlipBookProgress(progress, sliceCount);
        #endif
    #elif defined(_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_3D)
        #if defined(_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_RANDOM_ROW_SELECTION_ENABLED)
        return NovaFlipBookBlendingProgressWithRandomRow(progress, sliceCount, rowCount, randomValue);
        #else
        return NovaFlipBookBlendingProgress(progress, sliceCount);
        #endif
    #else
    return 0.0h;
    #endif
}

half NovaEmissionMapProgress(
    half progress,
    half sliceCount,
    half rowCount,
    half randomValue)
{
    #if defined(_EMISSION_MAP_MODE_2D_ARRAY)
        #if defined(_EMISSION_MAP_RANDOM_ROW_SELECTION_ENABLED)
        return NovaFlipBookProgressWithRandomRow(progress, sliceCount, rowCount, randomValue);
        #else
        return NovaFlipBookProgress(progress, sliceCount);
        #endif
    #elif defined(_EMISSION_MAP_MODE_3D)
        #if defined(_EMISSION_MAP_RANDOM_ROW_SELECTION_ENABLED)
        return NovaFlipBookBlendingProgressWithRandomRow(progress, sliceCount, rowCount, randomValue);
        #else
        return NovaFlipBookBlendingProgress(progress, sliceCount);
        #endif
    #else
    return 0.0h;
    #endif
}

void NovaApplyBaseMapChannel(inout half4 color, half channel)
{
    #if defined(_BASE_MAP_CHANNEL_ENABLED)
    half selectedValue = NovaSelectChannel(color, channel - 1.0h);
    color.rgb = half3(selectedValue, selectedValue, selectedValue);
    #endif
}

void NovaApplyBaseMapTone(
    inout half4 color,
    half toneChannel,
    half4 shadowsColor,
    half4 midtonesColor,
    half4 highlightsColor,
    half shadows,
    half midtonesBalance,
    half highlights,
    half4 darktonesColor,
    half4 brightsColor,
    half darktonesBalance,
    half brightsBalance)
{
    #if defined(_BASE_MAP_TONE_MODE_TRITONE) || defined(_BASE_MAP_TONE_MODE_PENTONE)
    const half minRange = 0.01h;
    shadows = saturate(shadows);
    highlights = max(shadows + minRange, saturate(highlights));
    midtonesBalance = saturate(midtonesBalance);
    half midpoint = lerp(shadows, highlights, midtonesBalance);
    half luminance = NovaSelectChannel(color, toneChannel);

        #if defined(_BASE_MAP_TONE_MODE_TRITONE)
        half3 result = shadowsColor.rgb;
        result = lerp(result, midtonesColor.rgb, NovaInverseLerpSafe(shadows, midpoint, luminance));
        result = lerp(result, highlightsColor.rgb, NovaInverseLerpSafe(midpoint, highlights, luminance));
        color.rgb = result;
        #elif defined(_BASE_MAP_TONE_MODE_PENTONE)
        half brights = lerp(midpoint, highlights, saturate(brightsBalance));
        half darktones = lerp(shadows, midpoint, saturate(darktonesBalance));
        half3 result = shadowsColor.rgb;
        result = lerp(result, darktonesColor.rgb, NovaInverseLerpSafe(shadows, darktones, luminance));
        result = lerp(result, midtonesColor.rgb, NovaInverseLerpSafe(darktones, midpoint, luminance));
        result = lerp(result, brightsColor.rgb, NovaInverseLerpSafe(midpoint, brights, luminance));
        result = lerp(result, highlightsColor.rgb, NovaInverseLerpSafe(brights, highlights, luminance));
        color.rgb = result;
        #endif
    #endif
}

half2 NovaFlowMapUvOffset(half4 flowSrc, half channelsX, half channelsY, half intensity)
{
    half2 flow = half2(NovaSelectChannel(flowSrc, channelsX), NovaSelectChannel(flowSrc, channelsY));
    return (flow * 2.0h - 1.0h) * intensity;
}

half NovaLuminance(half3 color)
{
    return dot(color, half3(0.298912h, 0.586611h, 0.114478h));
}

half NovaRimValue(half rim, half progress, half sharpness, half inverse)
{
    if (inverse >= 0.5h)
    {
        rim = 1.0h - rim;
    }

    progress = min(1.0h, progress);
    half width = 1.0h - min(1.0h, sharpness);
    half start = lerp(-width, 1.0h, progress);
    half end = lerp(0.0h, 1.0h + width, progress);
    return smoothstep(start, end, rim);
}

void NovaApplyRimTransparency(inout half4 color, half rimDot, half progress, half sharpness, half inverse)
{
    #ifdef _TRANSPARENCY_BY_RIM
    color.a *= NovaRimValue(rimDot, progress, sharpness, inverse);
    #endif
}

void NovaApplyLuminanceTransparency(inout half4 color, half progress, half sharpness, half inverse)
{
    #ifdef _TRANSPARENCY_BY_LUMINANCE
    color.a *= NovaRimValue(NovaLuminance(color.rgb), progress, sharpness, inverse);
    #endif
}

void NovaApplyTintColor(inout half4 color, half4 tintColor, half blendRate)
{
    color *= lerp(half4(1.0h, 1.0h, 1.0h, 1.0h), tintColor, saturate(blendRate));
}

void NovaModulateAlphaTransitionProgress(inout half progress, half vertexAlpha)
{
    #ifdef _VERTEX_ALPHA_AS_TRANSITION_PROGRESS
    progress += 1.0h - vertexAlpha;
    #endif
    progress = min(1.0h, progress);
}

half NovaApplyAlphaTransitionProgress(half transitionAlpha, half transitionProgress, half dissolveSharpness)
{
    #ifdef _FADE_TRANSITION_ENABLED
    transitionProgress = (transitionProgress * 2.0h - 1.0h) * -1.0h;
    return saturate(transitionAlpha + transitionProgress);
    #elif defined(_DISSOLVE_TRANSITION_ENABLED)
    half dissolveWidth = lerp(0.5h, 0.0001h, saturate(dissolveSharpness));
    transitionProgress = lerp(-dissolveWidth, 1.0h + dissolveWidth, saturate(transitionProgress));
    return smoothstep(transitionProgress - dissolveWidth, transitionProgress + dissolveWidth, transitionAlpha);
    #else
    return 1.0h;
    #endif
}

half NovaAlphaTransitionFactor(half4 map, half channel, half transitionProgress, half dissolveSharpness)
{
    half transitionAlpha = NovaSelectChannel(map, channel);
    return NovaApplyAlphaTransitionProgress(transitionAlpha, transitionProgress, dissolveSharpness);
}

half NovaBlendAlphaTransitionFactors(half mainAlpha, half secondAlpha)
{
    #if defined(_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE)
    return (mainAlpha + secondAlpha) * 0.5h;
    #elif defined(_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY)
    return mainAlpha * secondAlpha;
    #else
    return mainAlpha;
    #endif
}

half2 NovaParallaxOffset(half height, half strength, half3 viewDirTS)
{
    half scale = lerp(0.0h, 0.1h, saturate(strength));
    half3 view = normalize(viewDirTS);
    view.z += 0.42h;
    half denominator = abs(view.z) < 0.0001h ? (view.z < 0 ? -0.0001h : 0.0001h) : view.z;
    return view.xy / denominator * (-height * scale);
}

#endif

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
    half frameIndex = selectedRow * framesPerRow + saturate(progress) * framesPerRow;
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

#endif

#ifndef NOVA_BUILTIN_BASE_UV_INCLUDED
#define NOVA_BUILTIN_BASE_UV_INCLUDED

#include "NovaBuiltInUberUnlit.cginc"

half _BaseMapRotation;
half _BaseMapRotationCoord;
float4 _BaseMapRotationOffsets;
half _BaseMapOffsetXCoord;
half _BaseMapOffsetYCoord;

float2 NovaBaseUV(float2 uv, float4 scaleOffset, float4 coord1, float4 coord2)
{
    // Original NOVA rotates around its pivot before tiling/offset, then adds Custom Data.
    if (_BaseMapRotation > 0.0h || _BaseMapRotationCoord > 0.0h)
    {
        half angle = _BaseMapRotation + NovaGetCustomCoord(_BaseMapRotationCoord, coord1, coord2);
        uv = NovaRotateUV(uv, angle * 6.2831853h, _BaseMapRotationOffsets.xy);
    }
    return NovaTransformUV(uv, scaleOffset) + float2(
        NovaGetCustomCoord(_BaseMapOffsetXCoord, coord1, coord2),
        NovaGetCustomCoord(_BaseMapOffsetYCoord, coord1, coord2));
}

#endif

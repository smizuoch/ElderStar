#ifndef NOVA_BUILTIN_UI_INCLUDED
#define NOVA_BUILTIN_UI_INCLUDED

#include "UnityCG.cginc"
#include "UnityUI.cginc"

float4 _ClipRect;

half NovaUIClipAlpha(float2 positionOS)
{
    #ifdef UNITY_UI_CLIP_RECT
    return UnityGet2DClipping(positionOS, _ClipRect);
    #else
    return 1.0h;
    #endif
}

#endif

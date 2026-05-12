#ifndef NOVA_BUILTIN_FOG_INCLUDED
#define NOVA_BUILTIN_FOG_INCLUDED

#include "UnityCG.cginc"

#define NOVA_FOG_COORDS(index) UNITY_FOG_COORDS(index)
#define NOVA_TRANSFER_FOG(output, positionCS) UNITY_TRANSFER_FOG(output, positionCS)
#define NOVA_APPLY_FOG(fogCoord, color) UNITY_APPLY_FOG(fogCoord, color)

#endif

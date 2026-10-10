#ifndef NOVA_BUILTIN_DISTORTION_INCLUDED
#define NOVA_BUILTIN_DISTORTION_INCLUDED

#include "NovaBuiltInCore.cginc"
#include "NovaBuiltInTexture.cginc"
#include "NovaBuiltInDepth.cginc"
#include "NovaBuiltInUberUnlit.cginc"

NOVA_DECLARE_TEX2D(_BaseMap);
NOVA_DECLARE_TEX2D(_FlowMap);
NOVA_DECLARE_TEX2D(_AlphaTransitionMap);
half _BaseMapOffsetXCoord, _BaseMapOffsetYCoord, _BaseMapChannelsX, _BaseMapChannelsY;
half _BaseMapRotation, _BaseMapRotationCoord, _BaseMapUnpackNormal;
float4 _BaseMapRotationOffsets;
half _DistortionIntensityMode, _DistortionIntensity, _DistortionIntensityCoord;
half _DistortionIntensityX, _DistortionIntensityXCoord, _DistortionIntensityY, _DistortionIntensityYCoord;
half _FlowMapOffsetXCoord, _FlowMapOffsetYCoord, _FlowMapChannelsX, _FlowMapChannelsY;
half _FlowIntensity, _FlowIntensityCoord, _FlowMapRotation, _FlowMapRotationCoord;
float4 _FlowMapRotationOffsets;
half _AlphaTransitionMapOffsetXCoord, _AlphaTransitionMapOffsetYCoord, _AlphaTransitionMapChannelsX;
half _AlphaTransitionProgress, _AlphaTransitionProgressCoord, _DissolveSharpness;
half _AlphaTransitionMapRotation, _AlphaTransitionMapRotationCoord;
float4 _AlphaTransitionMapRotationOffsets;
half _SoftParticlesIntensity, _DepthFadeNear, _DepthFadeFar, _DepthFadeWidth, _ZTest;

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float4 customCoord1 : TEXCOORD1;
    float4 customCoord2 : TEXCOORD2;
    fixed4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct v2f
{
    float4 pos : SV_POSITION;
    float2 baseUv : TEXCOORD0;
    float4 flowTransitionUv : TEXCOORD1;
    float4 screenPosition : TEXCOORD2;
    float4 customCoord1 : TEXCOORD3;
    float4 customCoord2 : TEXCOORD4;
    UNITY_VERTEX_OUTPUT_STEREO
};

float2 NovaDistortionUv(float2 uv, float4 st, half angle, half angleCoord, float2 pivot,
    half xCoord, half yCoord, float4 coord1, float4 coord2)
{
    if (angle > 0 || angleCoord > 0)
        uv = NovaRotateUV(uv, (angle + NovaGetCustomCoord(angleCoord, coord1, coord2)) * 6.2831853, pivot);
    return NovaTransformUV(uv, st) + float2(NovaGetCustomCoord(xCoord, coord1, coord2), NovaGetCustomCoord(yCoord, coord1, coord2));
}

v2f vert(appdata v)
{
    NOVA_SETUP_PARTICLE(v)
    v2f o = (v2f)0;
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.pos = NovaObjectToClipPos(v.vertex);
    o.screenPosition = NovaComputeScreenPos(o.pos);
    o.screenPosition.z = -UnityObjectToViewPos(v.vertex).z;
    o.customCoord1 = v.customCoord1;
    o.customCoord2 = v.customCoord2;
    o.baseUv = NovaDistortionUv(v.uv, _BaseMap_ST, _BaseMapRotation, _BaseMapRotationCoord, _BaseMapRotationOffsets.xy,
        _BaseMapOffsetXCoord, _BaseMapOffsetYCoord, v.customCoord1, v.customCoord2);
    o.flowTransitionUv.xy = NovaDistortionUv(v.uv, _FlowMap_ST, _FlowMapRotation, _FlowMapRotationCoord, _FlowMapRotationOffsets.xy,
        _FlowMapOffsetXCoord, _FlowMapOffsetYCoord, v.customCoord1, v.customCoord2);
    o.flowTransitionUv.zw = NovaDistortionUv(v.uv, _AlphaTransitionMap_ST, _AlphaTransitionMapRotation, _AlphaTransitionMapRotationCoord,
        _AlphaTransitionMapRotationOffsets.xy, _AlphaTransitionMapOffsetXCoord, _AlphaTransitionMapOffsetYCoord, v.customCoord1, v.customCoord2);
    return o;
}

void NovaDistortionDepthTest(float4 screenPosition)
{
    // Image effects have a resolved color target. Compare camera depth explicitly so
    // MSAA and non-MSAA cameras can share the same distortion buffer path.
    float sceneDepth = NovaSampleSceneDepthEye(NovaScreenUV(screenPosition));
    float difference = screenPosition.z - sceneDepth;
    if (_ZTest == 1) clip(-1);
    if (_ZTest == 2) clip(-difference - 0.0001);
    if (_ZTest == 3) clip(0.001 - abs(difference));
    if (_ZTest == 4) clip(0.001 - difference);
    if (_ZTest == 5) clip(difference - 0.0001);
    if (_ZTest == 6) clip(abs(difference) - 0.001);
    if (_ZTest == 7) clip(difference + 0.001);
}

half4 frag(v2f i) : SV_Target
{
    NovaDistortionDepthTest(i.screenPosition);
    #if defined(_FLOW_MAP_ENABLED) || defined(_FLOW_MAP_TARGET_BASE) || defined(_FLOW_MAP_TARGET_ALPHA_TRANSITION)
    half4 flowSource = NOVA_SAMPLE_TEX2D(_FlowMap, i.flowTransitionUv.xy);
    half2 flow = half2(NovaSelectChannel(flowSource, _FlowMapChannelsX), NovaSelectChannel(flowSource, _FlowMapChannelsY)) * 2 - 1;
    flow *= _FlowIntensity + NovaGetCustomCoord(_FlowIntensityCoord, i.customCoord1, i.customCoord2);
    #if defined(_FLOW_MAP_ENABLED) || defined(_FLOW_MAP_TARGET_BASE)
    i.baseUv += flow;
    #endif
    #if defined(_FLOW_MAP_TARGET_ALPHA_TRANSITION)
    i.flowTransitionUv.zw += flow;
    #endif
    #endif
    half4 source = NOVA_SAMPLE_BASE_TEX2D(_BaseMap, NovaApplyBaseMirror(i.baseUv));
    if (_BaseMapUnpackNormal > 0.5) source = half4((UnpackNormal(source) + 1) * 0.5, 1);
    half2 offset = half2(NovaSelectChannel(source, _BaseMapChannelsX), NovaSelectChannel(source, _BaseMapChannelsY)) * 2 - 1;
    half intensity = _DistortionIntensity + NovaGetCustomCoord(_DistortionIntensityCoord, i.customCoord1, i.customCoord2);
    half2 intensityXY = half2(_DistortionIntensityX + NovaGetCustomCoord(_DistortionIntensityXCoord, i.customCoord1, i.customCoord2),
        _DistortionIntensityY + NovaGetCustomCoord(_DistortionIntensityYCoord, i.customCoord1, i.customCoord2));
    offset *= 0.1 * (_DistortionIntensityMode > 0.5 ? intensityXY : half2(intensity, intensity));
    #if defined(_FADE_TRANSITION_ENABLED) || defined(_DISSOLVE_TRANSITION_ENABLED)
    offset *= NovaAlphaTransitionFactor(NOVA_SAMPLE_TEX2D(_AlphaTransitionMap, i.flowTransitionUv.zw), _AlphaTransitionMapChannelsX,
        _AlphaTransitionProgress + NovaGetCustomCoord(_AlphaTransitionProgressCoord, i.customCoord1, i.customCoord2), _DissolveSharpness);
    #endif
    #if defined(_SOFT_PARTICLES_ENABLED)
    offset *= NovaSoftParticleFactor(i.screenPosition, _SoftParticlesIntensity);
    #endif
    #if defined(_DEPTH_FADE_ENABLED)
    offset *= NovaDepthFadeFactor(_DepthFadeNear, _DepthFadeFar, _DepthFadeWidth, i.screenPosition);
    #endif
    return half4(offset, 0, 0);
}
#endif

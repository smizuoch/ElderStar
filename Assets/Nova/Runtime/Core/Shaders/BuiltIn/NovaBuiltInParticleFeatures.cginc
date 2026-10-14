#ifndef NOVA_PARTICLE_FEATURES_INCLUDED
#define NOVA_PARTICLE_FEATURES_INCLUDED

#if defined(NOVA_DEPTH_NORMALS)
// Match original ParticlesUberDepthNormalsCore, whose alpha path deliberately
// omits channel/tone/color correction and whose opaque flow uses the legacy flag.
#undef _BASE_MAP_CHANNEL_ENABLED
#undef _BASE_MAP_TONE_MODE_TRITONE
#undef _BASE_MAP_TONE_MODE_PENTONE
#undef _GREYSCALE_ENABLED
#undef _GRADIENT_MAP_ENABLED
#if !defined(_ALPHATEST_ENABLED)
#undef _FLOW_MAP_TARGET_BASE
#undef _FLOW_MAP_TARGET_TINT
#undef _FLOW_MAP_TARGET_EMISSION
#undef _FLOW_MAP_TARGET_ALPHA_TRANSITION
#endif
#if defined(_FLOW_MAP_ENABLED)
#define _FLOW_MAP_TARGET_BASE
#endif
#endif

#include "NovaBuiltInCore.cginc"
#include "NovaBuiltInFog.cginc"
#include "NovaBuiltInTexture.cginc"
#include "NovaBuiltInDepth.cginc"
#include "NovaBuiltInUI.cginc"
#include "NovaBuiltInBaseUV.cginc"

#if defined(NOVA_DEPTH_NORMALS)
float _NovaDepthNormalsUseSceneDepth;
#endif

NOVA_DECLARE_TEX2D(_BaseMap);
NOVA_DECLARE_TEX2D_ARRAY(_BaseMap2DArray);
float4 _BaseMap2DArray_ST;
NOVA_DECLARE_TEX3D(_BaseMap3D);
NOVA_DECLARE_TEX2D(_TintMap);
NOVA_DECLARE_TEX2D_ARRAY(_TintMap2DArray);
float4 _TintMap2DArray_ST;
NOVA_DECLARE_TEX3D(_TintMap3D);
NOVA_DECLARE_TEX2D(_FlowMap);
NOVA_DECLARE_TEX2D(_ParallaxMap);
NOVA_DECLARE_TEX2D_ARRAY(_ParallaxMap2DArray);
float4 _ParallaxMap2DArray_ST;
NOVA_DECLARE_TEX3D(_ParallaxMap3D);
NOVA_DECLARE_TEX2D(_GradientMap);
NOVA_DECLARE_TEX2D(_AlphaTransitionMap);
NOVA_DECLARE_TEX2D_ARRAY(_AlphaTransitionMap2DArray);
float4 _AlphaTransitionMap2DArray_ST;
NOVA_DECLARE_TEX3D(_AlphaTransitionMap3D);
NOVA_DECLARE_TEX2D(_AlphaTransitionMapSecondTexture);
NOVA_DECLARE_TEX2D_ARRAY(_AlphaTransitionMapSecondTexture2DArray);
float4 _AlphaTransitionMapSecondTexture2DArray_ST;
NOVA_DECLARE_TEX3D(_AlphaTransitionMapSecondTexture3D);
NOVA_DECLARE_TEX2D(_EmissionMap);
NOVA_DECLARE_TEX2D_ARRAY(_EmissionMap2DArray);
float4 _EmissionMap2DArray_ST;
NOVA_DECLARE_TEX3D(_EmissionMap3D);
NOVA_DECLARE_TEX2D(_EmissionColorRamp);
NOVA_DECLARE_TEX2D(_VertexDeformationMap);
fixed4 _TintColor;
fixed4 _EmissionColor;
fixed _Cutoff;
half _BaseMapProgress;
half _BaseMapProgressCoord;
half _BaseMapSliceCount;
half _BaseMapRandomRowCoord;
half _BaseMapRowCount;
half _BaseMapChannel;
half _BaseMapToneChannel;
half4 _BaseMapToneHighlightsColor;
half4 _BaseMapToneMidtonesColor;
half4 _BaseMapToneShadowsColor;
half _BaseMapToneHighlights;
half _BaseMapToneMidtones;
half _BaseMapToneShadows;
half _BaseMapToneHighlightsCoord;
half _BaseMapToneMidtonesCoord;
half _BaseMapToneShadowsCoord;
half4 _BaseMapToneBrightsColor;
half4 _BaseMapToneDarktonesColor;
half _BaseMapToneBrights;
half _BaseMapToneDarktones;
half _BaseMapToneBrightsCoord;
half _BaseMapToneDarktonesCoord;
half _TintMap3DProgress;
half _TintMap3DProgressCoord;
half _TintMapProgress;
half _TintMapProgressCoord;
half _TintMapSliceCount;
half _TintMapOffsetXCoord;
half _TintMapOffsetYCoord;
half _TintBlendRate;
half _TintBlendRateCoord;
half _TintRimProgress;
half _TintRimProgressCoord;
half _TintRimSharpness;
half _TintRimSharpnessCoord;
half _InverseTintRim;
half _TintMapRandomRowCoord;
half _TintMapRowCount;
half _TintMapRotation;
half _TintMapRotationCoord;
half4 _TintMapRotationOffsets;
half _FlowMapOffsetXCoord;
half _FlowMapOffsetYCoord;
half _FlowMapRotation;
half _FlowMapRotationCoord;
half4 _FlowMapRotationOffsets;
half _FlowMapChannelsX;
half _FlowMapChannelsY;
half _FlowIntensity;
half _FlowIntensityCoord;
half _ParallaxMapProgress;
half _ParallaxMapProgressCoord;
half _ParallaxMapOffsetXCoord;
half _ParallaxMapOffsetYCoord;
half _ParallaxMapChannel;
half _ParallaxMapSliceCount;
half _ParallaxStrength;
half _ParallaxMapRandomRowCoord;
half _ParallaxMapRowCount;
half _AlphaTransitionMapProgress;
half _AlphaTransitionMapProgressCoord;
half _AlphaTransitionMapOffsetXCoord;
half _AlphaTransitionMapOffsetYCoord;
half _AlphaTransitionMapRotation;
half _AlphaTransitionMapRotationCoord;
half4 _AlphaTransitionMapRotationOffsets;
half _AlphaTransitionMapChannelsX;
half _AlphaTransitionMapSliceCount;
half _AlphaTransitionProgress;
half _AlphaTransitionProgressCoord;
half _DissolveSharpness;
half _AlphaTransitionMapSecondTextureProgress;
half _AlphaTransitionMapSecondTextureProgressCoord;
half _AlphaTransitionMapSecondTextureOffsetXCoord;
half _AlphaTransitionMapSecondTextureOffsetYCoord;
half _AlphaTransitionMapSecondTextureRotation;
half _AlphaTransitionMapSecondTextureRotationCoord;
half4 _AlphaTransitionMapSecondTextureRotationOffsets;
half _AlphaTransitionMapSecondTextureChannelsX;
half _AlphaTransitionMapSecondTextureSliceCount;
half _AlphaTransitionProgressSecondTexture;
half _AlphaTransitionProgressCoordSecondTexture;
half _DissolveSharpnessSecondTexture;
half _AlphaTransitionMapRandomRowCoord;
half _AlphaTransitionMapRowCount;
half _AlphaTransitionMapSecondTextureRandomRowCoord;
half _AlphaTransitionMapSecondTextureRowCount;
half _EmissionMapProgress;
half _EmissionMapProgressCoord;
half _EmissionMapOffsetXCoord;
half _EmissionMapOffsetYCoord;
half _EmissionMapRotation;
half _EmissionMapRotationCoord;
half4 _EmissionMapRotationOffsets;
half _EmissionMapChannelsX;
half _EmissionMapSliceCount;
half _EmissionIntensity;
half _EmissionIntensityCoord;
half _KeepEdgeTransparency;
half _EmissionMapRandomRowCoord;
half _EmissionMapRowCount;
half _RimTransparencyProgress;
half _RimTransparencyProgressCoord;
half _RimTransparencySharpness;
half _RimTransparencySharpnessCoord;
half _InverseRimTransparency;
half _LuminanceTransparencyProgress;
half _LuminanceTransparencyProgressCoord;
half _LuminanceTransparencySharpness;
half _LuminanceTransparencySharpnessCoord;
half _InverseLuminanceTransparency;
half _SoftParticlesIntensity;
half _DepthFadeNear;
half _DepthFadeFar;
half _DepthFadeWidth;
half _VertexDeformationMapOffsetXCoord;
half _VertexDeformationMapOffsetYCoord;
half _VertexDeformationMapChannel;
float _VertexDeformationIntensity;
half _VertexDeformationIntensityCoord;
float _VertexDeformationBaseValue;

#include "NovaBuiltInLighting.cginc"
#include "NovaBuiltInShadow.cginc"
#if defined(NOVA_UI_LIT)
#include "NovaBuiltInUIParticlesUberLit.cginc"
#include "NovaBuiltInCanvasLighting.cginc"
#else
#include "NovaBuiltInUberLit.cginc"
#endif

struct appdata
{
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 uv : TEXCOORD0;
    float4 customCoord1 : TEXCOORD1;
    float4 customCoord2 : TEXCOORD2;
    fixed4 color : COLOR;
};

// Logical feature data. Pack before interpolation so the original vertex-space
// tangent basis and parallax direction fit stereo + shadowed SM3.5 ForwardAdd.
struct NovaFeatureData
{
    float4 pos;
    float4 baseUvAndProgress;
    float4 tintUvAndProgress;
    float4 flowUvAndPositionOS;
    float4 parallaxUvAndProgress;
    float4 customCoord1;
    float4 customCoord2;
    float3 positionWS;
    float3 tangentWS;
    float3 bitangentWS;
    float3 viewDirTS;
    float fog;
    float3 alphaTransitionUvAndProgress;
    float3 alphaTransitionSecondUvAndProgress;
    float3 emissionUvAndProgress;
    fixed4 color;
};

// A clipped UI, stereo and shadowed punctual light otherwise require 17 outputs.
// Reuse interpolated world position for the affine light transform in this variant.
#if defined(NOVA_UI_LIT) && defined(UNITY_UI_CLIP_RECT) && defined(UNITY_PASS_FORWARDADD) && (defined(POINT) || defined(POINT_COOKIE) || defined(SPOT) || defined(DIRECTIONAL_COOKIE))
#define NOVA_FRAGMENT_LIGHT_COORDS
#endif

struct v2f
{
    UNITY_VERTEX_OUTPUT_STEREO
    float4 pos : SV_POSITION;
    float4 baseTintUv : TEXCOORD0;
    float4 alphaUv : TEXCOORD1;
    float4 parallaxEmissionUv : TEXCOORD2;
    float4 progress : TEXCOORD3;
    float4 flowUvAndProgress : TEXCOORD4;
    float4 customCoord1 : TEXCOORD5;
    float4 customCoord2 : TEXCOORD6;
    float4 positionAndNormalX : TEXCOORD7;
    float4 tangentAndNormalY : TEXCOORD8;
    float4 bitangentAndNormalZ : TEXCOORD9;
    float4 viewAndFog : TEXCOORD10;
    #if defined(UNITY_PASS_FORWARDADD) && !defined(NOVA_FRAGMENT_LIGHT_COORDS)
    NOVA_LIGHTING_COORDS(11, 12)
    #else
    NOVA_SHADOW_COORDS(11)
    #endif
    fixed4 color : COLOR;
    #if defined(NOVA_UI_LIT) && defined(UNITY_UI_CLIP_RECT)
    float2 positionOS : TEXCOORD13;
    #endif
};

#if defined(NOVA_FRAGMENT_LIGHT_COORDS)
struct NovaFragmentLightData { NOVA_LIGHTING_COORDS(0, 1) };
half NovaFragmentLightAttenuation(v2f input, float3 positionWS)
{
    NovaFragmentLightData data = (NovaFragmentLightData)0;
    #if defined(POINT) || defined(POINT_COOKIE)
    data._LightCoord = mul(unity_WorldToLight, float4(positionWS, 1)).xyz;
    #elif defined(SPOT)
    data._LightCoord = mul(unity_WorldToLight, float4(positionWS, 1));
    #else
    data._LightCoord = mul(unity_WorldToLight, float4(positionWS, 1)).xy;
    #endif
    #if defined(SHADOWS_SCREEN) || defined(SHADOWS_CUBE) || (defined(SHADOWS_DEPTH) && defined(SPOT))
    data._ShadowCoord = input._ShadowCoord;
    #endif
    return LIGHT_ATTENUATION(data);
}
#undef NovaComputeBuiltInAddLightAttenuation
#define NovaComputeBuiltInAddLightAttenuation(input, positionWS) NovaFragmentLightAttenuation(input, positionWS)
#endif

v2f vert(appdata v)
{
    NOVA_SETUP_PARTICLE(v)
    NovaFeatureData o = (NovaFeatureData)0;
    #if defined(_VERTEX_DEFORMATION_ENABLED)
    float2 vertexDeformationUv = NovaTransformUV(v.uv, _VertexDeformationMap_ST);
    vertexDeformationUv.x += NovaGetCustomCoord(_VertexDeformationMapOffsetXCoord, v.customCoord1, v.customCoord2);
    vertexDeformationUv.y += NovaGetCustomCoord(_VertexDeformationMapOffsetYCoord, v.customCoord1, v.customCoord2);
    half4 vertexDeformation = NOVA_SAMPLE_TEX2D_LOD(_VertexDeformationMap, vertexDeformationUv, 0.0);
    float vertexDeformationIntensity = _VertexDeformationIntensity + NovaGetCustomCoord(_VertexDeformationIntensityCoord, v.customCoord1, v.customCoord2);
    float vertexDeformationAmount = (NovaSelectChannel(vertexDeformation, _VertexDeformationMapChannel) - _VertexDeformationBaseValue) * vertexDeformationIntensity;
    v.vertex.xyz += normalize(v.normal) * vertexDeformationAmount;
    #endif

    o.pos = NovaObjectToClipPos(v.vertex);
    float3 positionWS = NovaObjectToWorld(v.vertex.xyz);
    float3 vertexNormalWS = NovaObjectToWorldNormal(v.normal);
    o.baseUvAndProgress.w = vertexNormalWS.x;
    o.tintUvAndProgress.w = vertexNormalWS.y;
    o.parallaxUvAndProgress.w = vertexNormalWS.z;
    o.positionWS = positionWS;
    float3 tangentWS = NovaObjectToWorldTangent(v.tangent.xyz);
    float3 bitangentWS = NovaCreateBitangent(vertexNormalWS, tangentWS, v.tangent.w);
    o.tangentWS = tangentWS;
    // Original normal-map basis deliberately uses tangent.w without odd scale;
    // original parallax uses odd scale. Preserve the two distinct contracts.
    o.bitangentWS = cross(vertexNormalWS, tangentWS) * v.tangent.w;
    float3 viewDirWS = NovaWorldSpaceViewDir(positionWS);
    o.viewDirTS = float3(dot(viewDirWS, tangentWS), dot(viewDirWS, bitangentWS), dot(viewDirWS, vertexNormalWS));
    o.fog = NovaVertexFog(o.pos.z);
    half progress = _BaseMapProgress + NovaGetCustomCoord(_BaseMapProgressCoord, v.customCoord1, v.customCoord2);
    half randomValue = NovaGetCustomCoord(_BaseMapRandomRowCoord, v.customCoord1, v.customCoord2);
    o.baseUvAndProgress.z = NovaBaseMapProgress(progress, _BaseMapSliceCount, _BaseMapRowCount, randomValue);
    #if defined(_BASE_MAP_MODE_2D_ARRAY)
    o.baseUvAndProgress.xy = NovaBaseUV(v.uv, _BaseMap2DArray_ST, v.customCoord1, v.customCoord2);
    #elif defined(_BASE_MAP_MODE_3D)
    o.baseUvAndProgress.xy = NovaBaseUV(v.uv, _BaseMap3D_ST, v.customCoord1, v.customCoord2);
    #else
    o.baseUvAndProgress.xy = NovaBaseUV(v.uv, _BaseMap_ST, v.customCoord1, v.customCoord2);
    #endif
    o.tintUvAndProgress.xy = v.uv;
    #if defined(_TINT_MAP_ENABLED) || defined(_TINT_MAP_MODE_2D_ARRAY) || defined(_TINT_MAP_3D_ENABLED)
    if (_TintMapRotation > 0.0h || _TintMapRotationCoord > 0.0h)
    {
        half tintAngle = _TintMapRotation + NovaGetCustomCoord(_TintMapRotationCoord, v.customCoord1, v.customCoord2);
        o.tintUvAndProgress.xy = NovaRotateUV(o.tintUvAndProgress.xy, tintAngle * 6.2831853h, _TintMapRotationOffsets.xy);
    }
        #if defined(_TINT_MAP_MODE_2D_ARRAY)
        o.tintUvAndProgress.xy = NovaTransformUV(o.tintUvAndProgress.xy, _TintMap2DArray_ST);
        #elif defined(_TINT_MAP_3D_ENABLED)
        o.tintUvAndProgress.xy = NovaTransformUV(o.tintUvAndProgress.xy, _TintMap3D_ST);
        #else
        o.tintUvAndProgress.xy = NovaTransformUV(o.tintUvAndProgress.xy, _TintMap_ST);
        #endif
    o.tintUvAndProgress.xy.x += NovaGetCustomCoord(_TintMapOffsetXCoord, v.customCoord1, v.customCoord2);
    o.tintUvAndProgress.xy.y += NovaGetCustomCoord(_TintMapOffsetYCoord, v.customCoord1, v.customCoord2);
    #endif
    half tintProgress2DArray = _TintMapProgress + NovaGetCustomCoord(_TintMapProgressCoord, v.customCoord1, v.customCoord2);
    half tintProgress3D = _TintMap3DProgress + NovaGetCustomCoord(_TintMap3DProgressCoord, v.customCoord1, v.customCoord2);
    half tintRandomValue = NovaGetCustomCoord(_TintMapRandomRowCoord, v.customCoord1, v.customCoord2);
    o.tintUvAndProgress.z = NovaTintMapProgress(tintProgress2DArray, tintProgress3D, _TintMapSliceCount, _TintMapRowCount, tintRandomValue);
    o.flowUvAndPositionOS.xy = v.uv;
    #if defined(_FLOW_MAP_ENABLED) || defined(_FLOW_MAP_TARGET_BASE) || defined(_FLOW_MAP_TARGET_TINT) || defined(_FLOW_MAP_TARGET_EMISSION) || defined(_FLOW_MAP_TARGET_ALPHA_TRANSITION)
    if (_FlowMapRotation > 0.0h || _FlowMapRotationCoord > 0.0h)
    {
        half flowAngle = _FlowMapRotation + NovaGetCustomCoord(_FlowMapRotationCoord, v.customCoord1, v.customCoord2);
        o.flowUvAndPositionOS.xy = NovaRotateUV(o.flowUvAndPositionOS.xy, flowAngle * 6.2831853h, _FlowMapRotationOffsets.xy);
    }
    o.flowUvAndPositionOS.xy = NovaTransformUV(o.flowUvAndPositionOS.xy, _FlowMap_ST);
    o.flowUvAndPositionOS.xy.x += NovaGetCustomCoord(_FlowMapOffsetXCoord, v.customCoord1, v.customCoord2);
    o.flowUvAndPositionOS.xy.y += NovaGetCustomCoord(_FlowMapOffsetYCoord, v.customCoord1, v.customCoord2);
    #endif
    o.alphaTransitionUvAndProgress = float3(v.uv, 0.0h);
    o.alphaTransitionSecondUvAndProgress = float3(v.uv, 0.0h);
    #if defined(_FADE_TRANSITION_ENABLED) || defined(_DISSOLVE_TRANSITION_ENABLED)
    float2 alphaTransitionUv = v.uv;
    if (_AlphaTransitionMapRotation > 0.0h || _AlphaTransitionMapRotationCoord > 0.0h)
    {
        half alphaAngle = _AlphaTransitionMapRotation + NovaGetCustomCoord(_AlphaTransitionMapRotationCoord, v.customCoord1, v.customCoord2);
        alphaTransitionUv = NovaRotateUV(alphaTransitionUv, alphaAngle * 6.2831853h, _AlphaTransitionMapRotationOffsets.xy);
    }
        #if defined(_ALPHA_TRANSITION_MAP_MODE_2D_ARRAY)
        alphaTransitionUv = NovaTransformUV(alphaTransitionUv, _AlphaTransitionMap2DArray_ST);
        #elif defined(_ALPHA_TRANSITION_MAP_MODE_3D)
        alphaTransitionUv = NovaTransformUV(alphaTransitionUv, _AlphaTransitionMap3D_ST);
        #else
        alphaTransitionUv = NovaTransformUV(alphaTransitionUv, _AlphaTransitionMap_ST);
        #endif
    alphaTransitionUv.x += NovaGetCustomCoord(_AlphaTransitionMapOffsetXCoord, v.customCoord1, v.customCoord2);
    alphaTransitionUv.y += NovaGetCustomCoord(_AlphaTransitionMapOffsetYCoord, v.customCoord1, v.customCoord2);
    half alphaMapProgress = _AlphaTransitionMapProgress + NovaGetCustomCoord(_AlphaTransitionMapProgressCoord, v.customCoord1, v.customCoord2);
    half alphaRandomValue = NovaGetCustomCoord(_AlphaTransitionMapRandomRowCoord, v.customCoord1, v.customCoord2);
    o.alphaTransitionUvAndProgress = float3(
        alphaTransitionUv,
        NovaAlphaTransitionMapProgress(alphaMapProgress, _AlphaTransitionMapSliceCount, _AlphaTransitionMapRowCount, alphaRandomValue));

        #if defined(_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE) || defined(_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY)
        float2 alphaTransitionSecondUv = v.uv;
        if (_AlphaTransitionMapSecondTextureRotation > 0.0h || _AlphaTransitionMapSecondTextureRotationCoord > 0.0h)
        {
            half alphaSecondAngle = _AlphaTransitionMapSecondTextureRotation + NovaGetCustomCoord(_AlphaTransitionMapSecondTextureRotationCoord, v.customCoord1, v.customCoord2);
            alphaTransitionSecondUv = NovaRotateUV(alphaTransitionSecondUv, alphaSecondAngle * 6.2831853h, _AlphaTransitionMapSecondTextureRotationOffsets.xy);
        }
            #if defined(_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY)
            alphaTransitionSecondUv = NovaTransformUV(alphaTransitionSecondUv, _AlphaTransitionMapSecondTexture2DArray_ST);
            #elif defined(_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_3D)
            alphaTransitionSecondUv = NovaTransformUV(alphaTransitionSecondUv, _AlphaTransitionMapSecondTexture3D_ST);
            #else
            alphaTransitionSecondUv = NovaTransformUV(alphaTransitionSecondUv, _AlphaTransitionMapSecondTexture_ST);
            #endif
        alphaTransitionSecondUv.x += NovaGetCustomCoord(_AlphaTransitionMapSecondTextureOffsetXCoord, v.customCoord1, v.customCoord2);
        alphaTransitionSecondUv.y += NovaGetCustomCoord(_AlphaTransitionMapSecondTextureOffsetYCoord, v.customCoord1, v.customCoord2);
        half alphaSecondMapProgress = _AlphaTransitionMapSecondTextureProgress + NovaGetCustomCoord(_AlphaTransitionMapSecondTextureProgressCoord, v.customCoord1, v.customCoord2);
        half alphaSecondRandomValue = NovaGetCustomCoord(_AlphaTransitionMapSecondTextureRandomRowCoord, v.customCoord1, v.customCoord2);
        o.alphaTransitionSecondUvAndProgress = float3(
            alphaTransitionSecondUv,
            NovaAlphaTransitionSecondMapProgress(alphaSecondMapProgress, _AlphaTransitionMapSecondTextureSliceCount, _AlphaTransitionMapSecondTextureRowCount, alphaSecondRandomValue));
        #endif
    #endif
    o.emissionUvAndProgress = float3(v.uv, 0.0h);
    #if defined(_EMISSION_AREA_MAP)
    float2 emissionUv = v.uv;
    if (_EmissionMapRotation > 0.0h || _EmissionMapRotationCoord > 0.0h)
    {
        half emissionAngle = _EmissionMapRotation + NovaGetCustomCoord(_EmissionMapRotationCoord, v.customCoord1, v.customCoord2);
        emissionUv = NovaRotateUV(emissionUv, emissionAngle * 6.2831853h, _EmissionMapRotationOffsets.xy);
    }
        #if defined(_EMISSION_MAP_MODE_2D_ARRAY)
        emissionUv = NovaTransformUV(emissionUv, _EmissionMap2DArray_ST);
        #elif defined(_EMISSION_MAP_MODE_3D)
        emissionUv = NovaTransformUV(emissionUv, _EmissionMap3D_ST);
        #else
        emissionUv = NovaTransformUV(emissionUv, _EmissionMap_ST);
        #endif
    emissionUv.x += NovaGetCustomCoord(_EmissionMapOffsetXCoord, v.customCoord1, v.customCoord2);
    emissionUv.y += NovaGetCustomCoord(_EmissionMapOffsetYCoord, v.customCoord1, v.customCoord2);
    half emissionMapProgress = _EmissionMapProgress + NovaGetCustomCoord(_EmissionMapProgressCoord, v.customCoord1, v.customCoord2);
    half emissionRandomValue = NovaGetCustomCoord(_EmissionMapRandomRowCoord, v.customCoord1, v.customCoord2);
    o.emissionUvAndProgress = float3(
        emissionUv,
        NovaEmissionMapProgress(emissionMapProgress, _EmissionMapSliceCount, _EmissionMapRowCount, emissionRandomValue));
    #endif
    o.parallaxUvAndProgress.xyz = float3(v.uv, 0.0h);
    #if defined(_PARALLAX_MAP_TARGET_BASE) || defined(_PARALLAX_MAP_TARGET_TINT) || defined(_PARALLAX_MAP_TARGET_EMISSION)
        #if defined(_PARALLAX_MAP_MODE_2D_ARRAY)
        o.parallaxUvAndProgress.xy = NovaTransformUV(v.uv, _ParallaxMap2DArray_ST);
        #elif defined(_PARALLAX_MAP_MODE_3D)
        o.parallaxUvAndProgress.xy = NovaTransformUV(v.uv, _ParallaxMap3D_ST);
        #else
        o.parallaxUvAndProgress.xy = NovaTransformUV(v.uv, _ParallaxMap_ST);
        #endif
    o.parallaxUvAndProgress.x += NovaGetCustomCoord(_ParallaxMapOffsetXCoord, v.customCoord1, v.customCoord2);
    o.parallaxUvAndProgress.y += NovaGetCustomCoord(_ParallaxMapOffsetYCoord, v.customCoord1, v.customCoord2);
    half parallaxProgress = _ParallaxMapProgress + NovaGetCustomCoord(_ParallaxMapProgressCoord, v.customCoord1, v.customCoord2);
        #if defined(_PARALLAX_MAP_MODE_2D_ARRAY)
            #if defined(_PARALLAX_MAP_RANDOM_ROW_SELECTION_ENABLED)
            o.parallaxUvAndProgress.z = NovaFlipBookProgressWithRandomRow(parallaxProgress, _ParallaxMapSliceCount, _ParallaxMapRowCount, NovaGetCustomCoord(_ParallaxMapRandomRowCoord, v.customCoord1, v.customCoord2));
            #else
            o.parallaxUvAndProgress.z = NovaFlipBookProgress(parallaxProgress, _ParallaxMapSliceCount);
            #endif
        #elif defined(_PARALLAX_MAP_MODE_3D)
            #if defined(_PARALLAX_MAP_RANDOM_ROW_SELECTION_ENABLED)
            o.parallaxUvAndProgress.z = NovaFlipBookBlendingProgressWithRandomRow(parallaxProgress, _ParallaxMapSliceCount, _ParallaxMapRowCount, NovaGetCustomCoord(_ParallaxMapRandomRowCoord, v.customCoord1, v.customCoord2));
            #else
            o.parallaxUvAndProgress.z = NovaFlipBookBlendingProgress(parallaxProgress, _ParallaxMapSliceCount);
            #endif
        #endif
    #endif
    o.flowUvAndPositionOS.zw = v.vertex.xy;
    o.customCoord1 = v.customCoord1;
    o.customCoord2 = v.customCoord2;
    o.color = v.color;
    v2f packed = (v2f)0;
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(packed);
    packed.pos = o.pos;
    packed.baseTintUv = float4(o.baseUvAndProgress.xy, o.tintUvAndProgress.xy);
    packed.alphaUv = float4(o.alphaTransitionUvAndProgress.xy, o.alphaTransitionSecondUvAndProgress.xy);
    packed.parallaxEmissionUv = float4(o.parallaxUvAndProgress.xy, o.emissionUvAndProgress.xy);
    packed.progress = float4(o.baseUvAndProgress.z, o.tintUvAndProgress.z, o.alphaTransitionUvAndProgress.z, o.alphaTransitionSecondUvAndProgress.z);
    packed.flowUvAndProgress = float4(o.flowUvAndPositionOS.xy, o.parallaxUvAndProgress.z, o.emissionUvAndProgress.z);
    packed.customCoord1 = o.customCoord1;
    packed.customCoord2 = o.customCoord2;
    packed.positionAndNormalX = float4(o.positionWS, vertexNormalWS.x);
    packed.tangentAndNormalY = float4(o.tangentWS, vertexNormalWS.y);
    packed.bitangentAndNormalZ = float4(o.bitangentWS, vertexNormalWS.z);
    packed.viewAndFog = float4(o.viewDirTS, o.fog);
    packed.color = o.color;
    #if defined(NOVA_UI_LIT) && defined(UNITY_UI_CLIP_RECT)
    packed.positionOS = o.flowUvAndPositionOS.zw;
    #endif
    #if defined(UNITY_PASS_FORWARDADD) && !defined(NOVA_FRAGMENT_LIGHT_COORDS)
    NovaTransferBuiltInAddLightCoord(packed);
    #else
    NovaTransferShadowCoord(packed, positionWS);
    #endif
    return packed;
}

half4 SampleBaseMap(float2 uv, half progress)
{
    uv = NovaApplyBaseMirror(uv);
    #if defined(_BASE_MAP_MODE_2D_ARRAY)
    return NOVA_SAMPLE_BASE_TEX2D_ARRAY(_BaseMap2DArray, uv, progress);
    #elif defined(_BASE_MAP_MODE_3D)
    return NOVA_SAMPLE_BASE_TEX3D_LOD(_BaseMap3D, float3(uv, progress), 0.0);
    #else
    return NOVA_SAMPLE_BASE_TEX2D(_BaseMap, uv);
    #endif
}

half4 SampleTintMap(float2 uv, half progress)
{
    #if defined(_TINT_MAP_MODE_2D_ARRAY)
    return NOVA_SAMPLE_TEX2D_ARRAY(_TintMap2DArray, uv, progress);
    #elif defined(_TINT_MAP_3D_ENABLED)
    return NOVA_SAMPLE_TEX3D_LOD(_TintMap3D, float3(uv, progress), 0.0);
    #elif defined(_TINT_MAP_ENABLED)
    return NOVA_SAMPLE_TEX2D(_TintMap, uv);
    #else
    return half4(1.0h, 1.0h, 1.0h, 1.0h);
    #endif
}

half4 SampleAlphaTransitionMap(float2 uv, half progress)
{
    #if defined(_ALPHA_TRANSITION_MAP_MODE_2D_ARRAY)
    return NOVA_SAMPLE_TEX2D_ARRAY(_AlphaTransitionMap2DArray, uv, progress);
    #elif defined(_ALPHA_TRANSITION_MAP_MODE_3D)
    return NOVA_SAMPLE_TEX3D_LOD(_AlphaTransitionMap3D, float3(uv, progress), 0.0);
    #elif defined(_ALPHA_TRANSITION_MAP_MODE_2D)
    return NOVA_SAMPLE_TEX2D(_AlphaTransitionMap, uv);
    #else
    return half4(0.0h, 0.0h, 0.0h, 1.0h);
    #endif
}

half4 SampleAlphaTransitionSecondMap(float2 uv, half progress)
{
    #if defined(_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY)
    return NOVA_SAMPLE_TEX2D_ARRAY(_AlphaTransitionMapSecondTexture2DArray, uv, progress);
    #elif defined(_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_3D)
    return NOVA_SAMPLE_TEX3D_LOD(_AlphaTransitionMapSecondTexture3D, float3(uv, progress), 0.0);
    #elif defined(_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D)
    return NOVA_SAMPLE_TEX2D(_AlphaTransitionMapSecondTexture, uv);
    #else
    return half4(0.0h, 0.0h, 0.0h, 1.0h);
    #endif
}

half4 SampleParallaxMap(float2 uv, half progress)
{
    #if defined(_PARALLAX_MAP_MODE_2D_ARRAY)
    return NOVA_SAMPLE_TEX2D_ARRAY(_ParallaxMap2DArray, uv, progress);
    #elif defined(_PARALLAX_MAP_MODE_3D)
    return NOVA_SAMPLE_TEX3D_LOD(_ParallaxMap3D, float3(uv, progress), 0.0);
    #elif defined(_PARALLAX_MAP_MODE_2D)
    return NOVA_SAMPLE_TEX2D(_ParallaxMap, uv);
    #else
    return half4(0.0h, 0.0h, 0.0h, 1.0h);
    #endif
}

half4 SampleEmissionMap(float2 uv, half progress)
{
    #if defined(_EMISSION_MAP_MODE_2D_ARRAY)
    return NOVA_SAMPLE_TEX2D_ARRAY(_EmissionMap2DArray, uv, progress);
    #elif defined(_EMISSION_MAP_MODE_3D)
    return NOVA_SAMPLE_TEX3D_LOD(_EmissionMap3D, float3(uv, progress), 0.0);
    #elif defined(_EMISSION_MAP_MODE_2D)
    return NOVA_SAMPLE_TEX2D(_EmissionMap, uv);
    #else
    return half4(0.0h, 0.0h, 0.0h, 1.0h);
    #endif
}

void ApplyEmissionColor(inout half4 color, half4 emissionMap, half emissionIntensityParameter)
{
    const half texCompErrorMargin = 0.004h;
    half emissionIntensity = 0.0h;
    half emissionColorRampU = 0.0h;

    #if defined(_EMISSION_AREA_ALL)
    emissionIntensity = 1.0h;
    #elif defined(_EMISSION_AREA_MAP)
    half emissionMapValue = NovaSelectChannel(emissionMap, _EmissionMapChannelsX);
        #if defined(_EMISSION_COLOR_COLOR) || defined(_EMISSION_COLOR_BASECOLOR)
        emissionIntensity = emissionMapValue;
        #elif defined(_EMISSION_COLOR_MAP)
        emissionIntensity = step(texCompErrorMargin, emissionMapValue);
        emissionColorRampU = emissionMapValue;
        #endif
    #elif defined(_EMISSION_AREA_ALPHA)
    emissionIntensity = step(texCompErrorMargin, 1.0h - color.a);
    emissionColorRampU = color.a;
    color.a = _KeepEdgeTransparency >= 0.5h ? color.a : step(texCompErrorMargin, color.a);
    #endif

    half3 emissionColor = half3(0.0h, 0.0h, 0.0h);
    #if defined(_EMISSION_COLOR_COLOR)
    emissionColor = _EmissionColor.rgb;
    #elif defined(_EMISSION_COLOR_BASECOLOR)
    emissionColor = color.rgb;
    #elif defined(_EMISSION_COLOR_MAP)
    emissionColor = NOVA_SAMPLE_TEX2D(_EmissionColorRamp, half2(emissionColorRampU, 0.5h)).rgb;
    #endif

    color.rgb += emissionColor * emissionIntensity * emissionIntensityParameter;
}

half4 frag(v2f input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    NovaFeatureData i = (NovaFeatureData)0;
    i.baseUvAndProgress = float4(input.baseTintUv.xy, input.progress.x, input.positionAndNormalX.w);
    i.tintUvAndProgress = float4(input.baseTintUv.zw, input.progress.y, input.tangentAndNormalY.w);
    i.parallaxUvAndProgress = float4(input.parallaxEmissionUv.xy, input.flowUvAndProgress.z, input.bitangentAndNormalZ.w);
    i.alphaTransitionUvAndProgress = float3(input.alphaUv.xy, input.progress.z);
    i.alphaTransitionSecondUvAndProgress = float3(input.alphaUv.zw, input.progress.w);
    i.emissionUvAndProgress = float3(input.parallaxEmissionUv.zw, input.flowUvAndProgress.w);
    i.positionWS = input.positionAndNormalX.xyz;
    i.tangentWS = input.tangentAndNormalY.xyz;
    i.bitangentWS = input.bitangentAndNormalZ.xyz;
    i.viewDirTS = input.viewAndFog.xyz;
    i.fog = input.viewAndFog.w;
    i.flowUvAndPositionOS.xy = input.flowUvAndProgress.xy;
    #if defined(NOVA_UI_LIT) && defined(UNITY_UI_CLIP_RECT)
    i.flowUvAndPositionOS.zw = input.positionOS;
    #endif
    i.customCoord1 = input.customCoord1; i.customCoord2 = input.customCoord2;
    i.color = input.color;
    float3 interpolatedNormalWS = float3(i.baseUvAndProgress.w, i.tintUvAndProgress.w, i.parallaxUvAndProgress.w);
    #if defined(_SOFT_PARTICLES_ENABLED) || defined(_DEPTH_FADE_ENABLED) || defined(NOVA_DEPTH_NORMALS)
    // Reconstruct from the world position already carried for lighting. Keeping a
    // separate screen-position interpolator exceeds SM3.5's limit with cookie shadows.
    float4 screenPosition = NovaComputeScreenPos(NovaWorldToClipPos(i.positionWS));
    screenPosition.z = -mul(UNITY_MATRIX_V, float4(i.positionWS, 1)).z;
    #endif
    float3 particleViewDirWS = NovaWorldSpaceViewDir(i.positionWS);
    float3 particleBitangentWS = i.bitangentWS;
    float3 particleViewDirTS = i.viewDirTS;
    float2 baseUv = i.baseUvAndProgress.xy;
    float2 tintUv = i.tintUvAndProgress.xy;
    float2 alphaTransitionUv = i.alphaTransitionUvAndProgress.xy;
    float2 alphaTransitionSecondUv = i.alphaTransitionSecondUvAndProgress.xy;
    float2 emissionUv = i.emissionUvAndProgress.xy;

    #if defined(_FLOW_MAP_TARGET_BASE) || defined(_FLOW_MAP_TARGET_TINT) || defined(_FLOW_MAP_TARGET_EMISSION) || defined(_FLOW_MAP_TARGET_ALPHA_TRANSITION)
    half flowIntensity = _FlowIntensity + NovaGetCustomCoord(_FlowIntensityCoord, i.customCoord1, i.customCoord2);
    half2 flowOffset = NovaFlowMapUvOffset(NOVA_SAMPLE_TEX2D(_FlowMap, i.flowUvAndPositionOS.xy), _FlowMapChannelsX, _FlowMapChannelsY, flowIntensity);
        #if defined(_FLOW_MAP_TARGET_BASE)
        baseUv += flowOffset;
        #endif
        #if defined(_FLOW_MAP_TARGET_TINT)
        tintUv += flowOffset;
        #endif
        #if defined(_FLOW_MAP_TARGET_EMISSION)
        emissionUv += flowOffset;
        #endif
        #if defined(_FLOW_MAP_TARGET_ALPHA_TRANSITION)
        alphaTransitionUv += flowOffset;
        alphaTransitionSecondUv += flowOffset;
        #endif
    #endif

    #if defined(_PARALLAX_MAP_TARGET_BASE) || defined(_PARALLAX_MAP_TARGET_TINT) || defined(_PARALLAX_MAP_TARGET_EMISSION)
    half4 parallaxMap = SampleParallaxMap(i.parallaxUvAndProgress.xy, i.parallaxUvAndProgress.z);
    half parallaxHeight = NovaSelectChannel(parallaxMap, _ParallaxMapChannel);
    half2 parallaxOffset = NovaParallaxOffset(parallaxHeight, _ParallaxStrength, particleViewDirTS);
        #if defined(_PARALLAX_MAP_TARGET_BASE)
        baseUv += parallaxOffset;
        #endif
        #if defined(_PARALLAX_MAP_TARGET_TINT)
        tintUv += parallaxOffset;
        #endif
        #if defined(_PARALLAX_MAP_TARGET_EMISSION)
        emissionUv += parallaxOffset;
        #endif
    #endif

    half4 color = SampleBaseMap(baseUv, i.baseUvAndProgress.z);
    NovaApplyBaseMapChannel(color, _BaseMapChannel);
    NovaApplyBaseMapTone(
        color,
        _BaseMapToneChannel,
        _BaseMapToneShadowsColor,
        _BaseMapToneMidtonesColor,
        _BaseMapToneHighlightsColor,
        _BaseMapToneShadows + NovaGetCustomCoord(_BaseMapToneShadowsCoord, i.customCoord1, i.customCoord2),
        _BaseMapToneMidtones + NovaGetCustomCoord(_BaseMapToneMidtonesCoord, i.customCoord1, i.customCoord2),
        _BaseMapToneHighlights + NovaGetCustomCoord(_BaseMapToneHighlightsCoord, i.customCoord1, i.customCoord2),
        _BaseMapToneDarktonesColor,
        _BaseMapToneBrightsColor,
        _BaseMapToneDarktones + NovaGetCustomCoord(_BaseMapToneDarktonesCoord, i.customCoord1, i.customCoord2),
        _BaseMapToneBrights + NovaGetCustomCoord(_BaseMapToneBrightsCoord, i.customCoord1, i.customCoord2));

    #if defined(_TINT_AREA_ALL) || defined(_TINT_AREA_RIM)
    half tintBlendRate = _TintBlendRate + NovaGetCustomCoord(_TintBlendRateCoord, i.customCoord1, i.customCoord2);
        #if defined(_TINT_AREA_RIM)
        half3 normalWS = NovaSafeNormalize(interpolatedNormalWS);
        half3 viewDirWS = NovaSafeNormalize(particleViewDirWS);
        half rim = 1.0h - abs(dot(normalWS, viewDirWS));
        half tintRim = NovaRimValue(
            rim,
            _TintRimProgress + NovaGetCustomCoord(_TintRimProgressCoord, i.customCoord1, i.customCoord2),
            _TintRimSharpness + NovaGetCustomCoord(_TintRimSharpnessCoord, i.customCoord1, i.customCoord2),
            _InverseTintRim);
        tintBlendRate *= _TintBlendRate * tintRim;
        #endif
        #if defined(_TINT_COLOR_ENABLED)
        NovaApplyTintColor(color, _TintColor, tintBlendRate);
        #elif defined(_TINT_MAP_ENABLED) || defined(_TINT_MAP_MODE_2D_ARRAY) || defined(_TINT_MAP_3D_ENABLED)
        NovaApplyTintColor(color, SampleTintMap(tintUv, i.tintUvAndProgress.z), tintBlendRate);
        #endif
    #endif

    #if defined(_GREYSCALE_ENABLED)
    half luminance = NovaLuminance(color.rgb);
    color.rgb = half3(luminance, luminance, luminance);
    #elif defined(_GRADIENT_MAP_ENABLED)
    color.rgb = NOVA_SAMPLE_TEX2D(_GradientMap, half2(NovaLuminance(color.rgb), 0.5h)).rgb;
    #endif

    #if defined(_FADE_TRANSITION_ENABLED) || defined(_DISSOLVE_TRANSITION_ENABLED)
    half alphaTransitionProgress = _AlphaTransitionProgress + NovaGetCustomCoord(_AlphaTransitionProgressCoord, i.customCoord1, i.customCoord2);
    NovaModulateAlphaTransitionProgress(alphaTransitionProgress, i.color.a);
    half transitionAlpha = NovaAlphaTransitionFactor(
        SampleAlphaTransitionMap(alphaTransitionUv, i.alphaTransitionUvAndProgress.z),
        _AlphaTransitionMapChannelsX,
        alphaTransitionProgress,
        _DissolveSharpness);
        #if defined(_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE) || defined(_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY)
        half alphaTransitionProgressSecond = _AlphaTransitionProgressSecondTexture + NovaGetCustomCoord(_AlphaTransitionProgressCoordSecondTexture, i.customCoord1, i.customCoord2);
        NovaModulateAlphaTransitionProgress(alphaTransitionProgressSecond, i.color.a);
        half transitionAlphaSecond = NovaAlphaTransitionFactor(
            SampleAlphaTransitionSecondMap(alphaTransitionSecondUv, i.alphaTransitionSecondUvAndProgress.z),
            _AlphaTransitionMapSecondTextureChannelsX,
            alphaTransitionProgressSecond,
            _DissolveSharpnessSecondTexture);
        transitionAlpha = NovaBlendAlphaTransitionFactors(transitionAlpha, transitionAlphaSecond);
        #endif
    color.a *= transitionAlpha;
    #endif

    #if defined(_VERTEX_ALPHA_AS_TRANSITION_PROGRESS)
    color.rgb *= i.color.rgb;
    #else
    color *= i.color;
    #endif

    #if defined(_EMISSION_AREA_ALL) || defined(_EMISSION_AREA_MAP) || defined(_EMISSION_AREA_ALPHA)
    half emissionIntensity = _EmissionIntensity + NovaGetCustomCoord(_EmissionIntensityCoord, i.customCoord1, i.customCoord2);
    ApplyEmissionColor(color, SampleEmissionMap(emissionUv, i.emissionUvAndProgress.z), emissionIntensity);
    #endif

    #if defined(_TRANSPARENCY_BY_RIM)
    half3 transparencyNormalWS = NovaSafeNormalize(interpolatedNormalWS);
    half3 transparencyViewDirWS = NovaSafeNormalize(particleViewDirWS);
    half rimDot = abs(dot(transparencyNormalWS, transparencyViewDirWS));
    NovaApplyRimTransparency(
        color,
        rimDot,
        _RimTransparencyProgress + NovaGetCustomCoord(_RimTransparencyProgressCoord, i.customCoord1, i.customCoord2),
        _RimTransparencySharpness + NovaGetCustomCoord(_RimTransparencySharpnessCoord, i.customCoord1, i.customCoord2),
        _InverseRimTransparency);
    #endif

    #if defined(_TRANSPARENCY_BY_LUMINANCE)
    NovaApplyLuminanceTransparency(
        color,
        _LuminanceTransparencyProgress + NovaGetCustomCoord(_LuminanceTransparencyProgressCoord, i.customCoord1, i.customCoord2),
        _LuminanceTransparencySharpness + NovaGetCustomCoord(_LuminanceTransparencySharpnessCoord, i.customCoord1, i.customCoord2),
        _InverseLuminanceTransparency);
    #endif

    #if defined(_SOFT_PARTICLES_ENABLED)
    color.a *= NovaSoftParticleFactor(screenPosition, _SoftParticlesIntensity);
    #endif

    #if defined(_DEPTH_FADE_ENABLED)
    color.a *= NovaDepthFadeFactor(_DepthFadeNear, _DepthFadeFar, _DepthFadeWidth, screenPosition);
    #endif

    #if defined(NOVA_UI_LIT)
    color.a *= NovaUIClipAlpha(i.flowUvAndPositionOS.zw);
    #endif

    #ifdef _ALPHATEST_ENABLED
    clip(color.a - _Cutoff);
    #endif

    // Original fragLit lights the alpha-modulated albedo returned by fragUnlit.
    // Whiten the albedo before either light pass, rather than the final lit result.
    #if defined(_ALPHAMODULATE_ENABLED)
    color.rgb = lerp(half3(1.0, 1.0, 1.0), color.rgb, color.a);
    #endif

    #if defined(NOVA_DEPTH_NORMALS)
    UNITY_BRANCH
    if (_NovaDepthNormalsUseSceneDepth > 0.5)
    {
        float sceneDepth = NovaSampleSceneDepthEye(NovaScreenUV(screenPosition));
        clip(sceneDepth - screenPosition.z + max(0.0001, screenPosition.z * 0.00001));
    }
    // This explicit pass writes encoded world normals plus a validity mask. It is
    // opt-in and never replaces Unity's camera depth-normal texture.
    // Original depth vertices only compute flipbook progress for alpha-tested objects.
    #if defined(_ALPHATEST_ENABLED)
    float normalProgress = i.baseUvAndProgress.z;
    #else
    float normalProgress = 0;
    #endif
    float3 depthNormalWS = NovaTangentToWorldNormal(NovaResolveNormalMap(float3(baseUv, normalProgress)),
        interpolatedNormalWS, i.tangentWS, particleBitangentWS);
    return half4(depthNormalWS * 0.5 + 0.5, 1);
    #endif

    #if defined(NOVA_DEBUG_DISPLAY)
    half4 debugAlbedo = color;
    #endif

    #if defined(NOVA_UI_LIT)
    NovaUILitSurfaceData surfaceData = NovaResolveUILitSurfaceData(color,
        i.baseUvAndProgress.xyz, interpolatedNormalWS, i.tangentWS.xyz, particleBitangentWS);
    #if defined(UNITY_PASS_FORWARDADD)
    color = NovaComputeUILitForwardAddColor(surfaceData, i.positionWS, particleViewDirWS,
        NovaComputeBuiltInAddLightAttenuation(input, i.positionWS));
    #else
    color = NovaComputeUILitForwardBaseColor(surfaceData, particleViewDirWS,
        NovaComputeShadowAttenuation(input, i.positionWS), i.positionWS);
    #if defined(NOVA_CANVAS_LIGHTING)
    half3 canvasDirect = NovaCanvasDirectLighting(surfaceData, i.positionWS, particleViewDirWS);
    color.rgb += canvasDirect;
    #if defined(NOVA_DEBUG_DISPLAY)
    novaDebugDirect += canvasDirect;
    #endif
    #endif
    #endif
    #else
    NovaLitSurfaceData surfaceData = NovaResolveLitSurfaceData(color,
        i.baseUvAndProgress.xyz, interpolatedNormalWS, i.tangentWS.xyz, particleBitangentWS);
    #if defined(UNITY_PASS_FORWARDADD)
    color = NovaComputeLitForwardAddColor(surfaceData, i.positionWS, particleViewDirWS,
        NovaComputeBuiltInAddLightAttenuation(input, i.positionWS));
    #else
    color = NovaComputeLitForwardBaseColor(surfaceData, particleViewDirWS,
        NovaComputeShadowAttenuation(input, i.positionWS), i.positionWS);
    #endif
    #endif

    #if defined(NOVA_DEBUG_DISPLAY)
        #if defined(UNITY_PASS_FORWARDADD)
        if (_NovaDebugMode == 6) return color;
        if (_NovaDebugMode == 8)
            return half4((step(0.000001, NovaComputeBuiltInAddLightAttenuation(input, i.positionWS)) / 8.0h).xxx, 0);
        return 0;
        #else
        half count = 0;
        #if defined(NOVA_CANVAS_LIGHTING)
        count = _NovaCanvasLightCount;
        #endif
        return NovaDebugColor(debugAlbedo, surfaceData.normalWS, i.tangentWS,
            -mul(UNITY_MATRIX_V, float4(i.positionWS, 1)).z, i.fog, novaDebugDirect, novaDebugIndirect, count);
        #endif
    #endif
    NovaApplyVertexFog(color, i.fog);
    return color;
}

#endif

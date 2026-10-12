Shader "Nova/UIParticles/UberUnlit"
{
    Properties
        {
            // Render Settings
            _RenderType("Render Type", Float) = 2.0
            _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
            _TransparentBlendMode("Blend Mode", Float) = 0.0
            _Cull("Cull", Float) = 2.0
            _QueueOffset("Queue Offset", Float) = 0.0
            _VertexAlphaMode("Vertex Alpha Mode", Float) = 0.0
            _BlendSrc("Blend Src", Float) = 1.0
            _BlendDst("Blend Dst", Float) = 0.0
            _ZWrite("ZWrite", Float) = 0.0
            _ZWriteOverride("ZWrite Override", Float) = -1.0
            _ZTest("ZTest", Float) = 8.0
    
            _StencilComp ("Stencil Comparison", Float) = 8
            _Stencil ("Stencil ID", Float) = 0
            _StencilOp ("Stencil Operation", Float) = 0
            _StencilWriteMask ("Stencil Write Mask", Float) = 255
            _StencilReadMask ("Stencil Read Mask", Float) = 255
    
            _ColorMask ("Color Mask", Float) = 15
    
            // Base Map
            _BaseMapMode("Base Map Mode", Float) = 0.0
            [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
            _BaseMap2DArray("Base Map 2D Array", 2DArray) = "" {}
            _BaseMap3D("Base Map 3D", 3D) = "" {}
            _BaseMapProgress("Base Map Progress", Range(0.0, 1.0)) = 0.0
            _BaseMapProgressCoord("Base Map Progress Coord", Float) = 0.0
            _BaseMapSliceCount("Base Map Slice Count", Float) = 4.0
            _BaseMapRandomRowSelectionEnabled("Base Map Random Row Selection Enabled", Float) = 0.0
            _BaseMapRandomRowCoord("Base Map Random Row Coord", Float) = 0.0
            _BaseMapRowCount("Base Map Row Count", Float) = 1.0
            _BaseMapChannel("Base Map Channel", Float) = 0.0
            _BaseMapOffsetXCoord("Base Map Offset X Coord", Float) = 0.0
            _BaseMapOffsetYCoord("Base Map Offset Y Coord", Float) = 0.0
            _BaseMapRotation("Base Map Rotation", Range(0.0, 1.0)) = 0.0
            _BaseMapRotationCoord("Base Map Rotation Coord", Float) = 0.0
            _BaseMapRotationOffsets("Base Map Rotation Offsets", Vector) = (0.0, 0.0, 0.0, 0.0)
            _BaseMapMirrorSampling("Base Map Mirror Sampling", Float) = 0.0
            
            // Base Map Tri Tone
            // Base Map Tone Mode
            _BaseMapToneMode("Tone Mode", Float) = 0.0
            _BaseMapToneChannel("Tone Channel", Float) = 3.0
            
            // Base Map Tone - Common properties for TriTone and Pentone
            _BaseMapToneHighlightsColor("Highlights Color", Color) = (1,1,1,1)
            _BaseMapToneMidtonesColor("Midtones Color", Color) = (0.5,0.5,0.5,1)
            _BaseMapToneShadowsColor("Shadows Color", Color) = (0,0,0,1)
            _BaseMapToneHighlights("Highlights", Range(0.0, 1.0)) = 0.75
            _BaseMapToneMidtones("Midtones", Range(0.0, 1.0)) = 0.5
            _BaseMapToneShadows("Shadows", Range(0.0, 1.0)) = 0.25
            _BaseMapToneHighlightsCoord("Highlights Coord", Float) = 0.0
            _BaseMapToneMidtonesCoord("Midtones Coord", Float) = 0.0
            _BaseMapToneShadowsCoord("Shadows Coord", Float) = 0.0
            
            // Base Map Tone - Pentone exclusive properties
            _BaseMapToneBrightsColor("Brights Color", Color) = (0.75,0.75,0.75,1)
            _BaseMapToneDarktonesColor("Darktones Color", Color) = (0.25,0.25,0.25,1)
            _BaseMapToneBrights("Brights", Range(0, 1)) = 0.5
            _BaseMapToneDarktones("Darktones", Range(0, 1)) = 0.5
            _BaseMapToneBrightsCoord("Brights Coord", Float) = 0
            _BaseMapToneDarktonesCoord("Darktones Coord", Float) = 0
    
            // Tint Color
            _TintAreaMode("Tint Area Mode", Float) = 0.0
            _TintColorMode("Tint Color Mode", Float) = 0.0
            [HDR] _TintColor("Tint Color", Color) = (1, 1, 1, 1)
            _TintMap("Tint Map", 2D) = "white" {}
            _TintMap3D("Tint Map 3D", 3D) = "" {}
            _TintMap2DArray("Tint Map 2D Array", 2DArray) = "" {}
            _TintMap3DProgress("Tint Map 3D Progress", Range(0, 1)) = 0.0
            _TintMap3DProgressCoord("Tint Map 3D Progress Coord", Float) = 0.0
            _TintMapProgress("Tint Map Progress", Range(0, 1)) = 0.0
            _TintMapProgressCoord("Tint Map Progress Coord", Float) = 0.0
            _TintMapSliceCount("Tint Map Slice Count", Float) = 4.0
            _TintMapOffsetXCoord("Tint Map Offset X Coord", Float) = 0.0
            _TintMapOffsetYCoord("Tint Map Offset Y Coord", Float) = 0.0
            _TintBlendRate("Tint Blend Rate", Range(0.0, 1.0)) = 1.0
            _TintBlendRateCoord("Tint Blend Rate Coord", Float) = 0.0
            _TintRimProgress("Tint Rim Progress", Range(0.0, 1.0)) = 0.5
            _TintRimProgressCoord("Tint Rim Progress Coord", Float) = 0.0
            _TintRimSharpness("Tint Rim Sharpness", Range(0.0, 1.0)) = 0.5
            _TintRimSharpnessCoord("Tint Rim Sharpness Coord", Float) = 0.0
            _InverseTintRim("Inverse Tint Rim", Float) = 0.0
            _TintMapRandomRowSelectionEnabled("Tint Map Random Row Selection Enabled", Float) = 0.0
            _TintMapRandomRowCoord("Tint Map Random Row Coord", Float) = 0.0
            _TintMapRowCount("Tint Map Row Count", Float) = 1.0
    
            // Tint Map Rotation
            _TintMapRotation("Tint Map Rotation", Range(0.0, 1.0)) = 0.0
            _TintMapRotationCoord("Tint Map Rotation Coord", Float) = 0.0
            _TintMapRotationOffsets("Tint Map Rotation Offsets", Vector) = (0.0, 0.0, 0.0, 0.0)
    
            // Flow Map
            _FlowMap("Flow Map", 2D) = "grey" {}
            _FlowMapOffsetXCoord("Flow Map Offset X Coord", Float) = 0.0
            _FlowMapOffsetYCoord("Flow Map Offset Y Coord", Float) = 0.0
            _FlowMapRotation("Flow Map Rotation", Range(0.0, 1.0)) = 0.0
            _FlowMapRotationCoord("Flow Map Rotation Coord", Float) = 0.0
            _FlowMapRotationOffsets("Flow Map Rotation Offsets", Vector) = (0.0, 0.0, 0.0, 0.0)
            _FlowMapChannelsX("Flow Map Channes X", Float) = 0.0
            _FlowMapChannelsY("Flow Map Channes Y", Float) = 1.0
            _FlowIntensity("Flow Intensity", Float) = 1.0
            _FlowIntensityCoord("Flow Intensity Coord", Float) = 0.0
            _FlowMapTarget("Flow Map Target", Float) = 1.0
    
            // Parallax Map
            _ParallaxMapMode("Emission Map Mode", Float) = 0.0
            _ParallaxMap("Parallax Map", 2D) = "" {}
            _ParallaxMap2DArray("Parallax Map 2D Array", 2DArray) = "" {}
            _ParallaxMap3D("Parallax Map 3D", 3D) = "" {}
            _ParallaxMapProgress("Parallax Map Progress", Range(0.0, 1.0)) = 0.0
            _ParallaxMapProgressCoord("Parallax Map Progress Coord", Float) = 0.0
            _ParallaxMapOffsetXCoord("Parallax Map Offset X Coord", Float) = 0.0
            _ParallaxMapOffsetYCoord("Parallax Map Offset Y Coord", Float) = 0.0
            _ParallaxMapChannel("Parallax Map Channel", Float) = 0.0
            _ParallaxMapSliceCount("Parallax Map Slice Count", Float) = 4.0
            _ParallaxStrength("Parallax Strength", Range(0.0, 1.0)) = 0.3
            _ParallaxMapTarget("Parallax Map Target", Float) = 1.0
            _ParallaxMapRandomRowSelectionEnabled("Parallax Map Random Row Selection Enabled", Float) = 0.0
            _ParallaxMapRandomRowCoord("Parallax Map Random Row Coord", Float) = 0.0
            _ParallaxMapRowCount("Parallax Map Row Count", Float) = 1.0
    
            // Color Correction
            _ColorCorrectionMode("Alpha Transition Progress Coord", Float) = 0.0
            _GradientMap("Gradient Map", 2D) = "white" {}
    
            // Alpha Transition
            _AlphaTransitionMode("Alpha Transition Mode", Float) = 0.0
            _AlphaTransitionMapMode("Alpha Transition Map Mode", Float) = 0.0
            _AlphaTransitionMap("Alpha Transition Map", 2D) = "white" {}
            _AlphaTransitionMap2DArray("Alpha Transition Map 2D Array", 2DArray) = "" {}
            _AlphaTransitionMap3D("Alpha Transition Map 3D", 3D) = "" {}
            _AlphaTransitionMapProgress("Alpha　Transition Map Progress", Range(0.0, 1.0)) = 0.0
            _AlphaTransitionMapProgressCoord("Alpha　Transition Map Progress Coord", Float) = 0.0
            _AlphaTransitionMapOffsetXCoord("Alpha Transition Map Offset X Coord", Float) = 0.0
            _AlphaTransitionMapOffsetYCoord("Alpha Transition Map Offset Y Coord", Float) = 0.0
            _AlphaTransitionMapRotation("Alpha Transition Map Rotation", Range(0.0, 1.0)) = 0.0
            _AlphaTransitionMapRotationCoord("Alpha Transition Map Rotation Coord", Float) = 0.0
            _AlphaTransitionMapRotationOffsets("Alpha Transition Map Rotation Offsets", Vector) = (0.0, 0.0, 0.0, 0.0)
            _AlphaTransitionMapChannelsX("Alpha Transition Map Channes X", Float) = 0.0
            _AlphaTransitionMapSliceCount("Alpha Transition Map Slice Count", Float) = 4.0
            _AlphaTransitionProgress("Alpha Transition Progress", Range(0.0, 1.0)) = 0.0
            _AlphaTransitionProgressCoord("Alpha Transition Progress Coord", Float) = 0.0
            _DissolveSharpness("Dissolve Sharpness", Range(0.0, 1.0)) = 0.5
            _AlphaTransitionSecondTextureBlendMode("Alpha Transition Second Texture BlendMode", Float) = 0.0
            _AlphaTransitionMapSecondTextureMapMode("Alpha Transition Second Texture Map Mode", Float) = 0.0
            _AlphaTransitionMapSecondTexture("Alpha Transition Second Texture Map", 2D) = "white" {}
            _AlphaTransitionMapSecondTexture2DArray("Alpha Transition Second Texture Map 2D Array", 2DArray) = "" {}
            _AlphaTransitionMapSecondTexture3D("Alpha Transition Second Texture Map 3D", 3D) = "" {}
            _AlphaTransitionMapSecondTextureProgress("Alpha　Transition Map Second Texture  Progress", Range(0.0, 1.0)) = 0.0
            _AlphaTransitionMapSecondTextureProgressCoord("Alpha　Transition Map Second Texture  Progress Coord", Float) = 0.0
            _AlphaTransitionMapSecondTextureOffsetXCoord("Alpha Transition Second Texture Map Offset X Coord", Float) = 0.0
            _AlphaTransitionMapSecondTextureOffsetYCoord("Alpha Transition Second Texture Map Offset Y Coord", Float) = 0.0
            _AlphaTransitionMapSecondTextureRotation("Alpha Transition Map Second Texture Rotation", Range(0.0, 1.0)) = 0.0
            _AlphaTransitionMapSecondTextureRotationCoord("Alpha Transition Map Second Texture Rotation Coord", Float) = 0.0
            _AlphaTransitionMapSecondTextureRotationOffsets("Alpha Transition Map Second Texture Rotation Offsets", Vector) = (0.0, 0.0, 0.0, 0.0)
            _AlphaTransitionMapSecondTextureChannelsX("Alpha Transition Second Texture Map Channes X", Float) = 0.0
            _AlphaTransitionMapSecondTextureSliceCount("Alpha Transition Map Second Texture Slice Count", Float) = 4.0
            _AlphaTransitionProgressSecondTexture("Alpha Transition Second Texture Progress", Range(0.0, 1.0)) = 0.0
            _AlphaTransitionProgressCoordSecondTexture("Alpha Transition Second Texture Progress Coord", Float) = 0.0
            _DissolveSharpnessSecondTexture("Dissolve Sharpness", Range(0.0, 1.0)) = 0.5
            _AlphaTransitionMapRandomRowSelectionEnabled("Alpha Transition Map Random Row Selection Enabled", Float) = 0.0
            _AlphaTransitionMapRandomRowCoord("Alpha Transition Map Random Row Coord", Float) = 0.0
            _AlphaTransitionMapRowCount("Alpha Transition Map Row Count", Float) = 1.0
            _AlphaTransitionMapSecondTextureRandomRowSelectionEnabled("Alpha Transition Map Second Texture Random Row Selection Enabled", Float) = 0.0
            _AlphaTransitionMapSecondTextureRandomRowCoord("Alpha Transition Map Second Texture Random Row Coord", Float) = 0.0
            _AlphaTransitionMapSecondTextureRowCount("Alpha Transition Map Second Texture Row Count", Float) = 1.0
    
            // Emission
            _EmissionAreaType("Emission Area Type", Float) = 0.0
            _EmissionMapMode("Emission Map Mode", Float) = 0.0
            _EmissionMap("Emission Map", 2D) = "black" {}
            _EmissionMap2DArray("Emission Map 2D Array", 2DArray) = "" {}
            _EmissionMap3D("Emission Map 3D", 3D) = "" {}
            _EmissionMapProgress("Emission Map Progress", Range(0.0, 1.0)) = 0.0
            _EmissionMapProgressCoord("Emission Map Progress Coord", Float) = 0.0
            _EmissionMapOffsetXCoord("Emission Map Offset X Coord", Float) = 0.0
            _EmissionMapOffsetYCoord("Emission Map Offset Y Coord", Float) = 0.0
            _EmissionMapRotation("Emission Map Rotation", Range(0.0, 1.0)) = 0.0
            _EmissionMapRotationCoord("Emission Map Rotation Coord", Float) = 0.0
            _EmissionMapRotationOffsets("Emission Map Rotation Offsets", Vector) = (0.0, 0.0, 0.0, 0.0)
            _EmissionMapChannelsX("Emission Map Channels X", Float) = 0.0
            _EmissionMapSliceCount("Alpha Transition Map Slice Count", Float) = 4.0
            _EmissionColorType("Emission Color Type", Float) = 0.0
            [HDR]_EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
            _EmissionColorRamp("Emission Color Ramp", 2D) = "black" {}
            _EmissionIntensity("Emission Intensity", Float) = 1.0
            _EmissionIntensityCoord("Emission Intensity Coord", Float) = 0.0
            _KeepEdgeTransparency("Keep Edge Transparency", Float) = 1.0
            _EmissionMapRandomRowSelectionEnabled("Emission Map Random Row Selection Enabled", Float) = 0.0
            _EmissionMapRandomRowCoord("Emission Map Random Row Coord", Float) = 0.0
            _EmissionMapRowCount("Emission Map Row Count", Float) = 1.0
    
            // Transparency
            _RimTransparencyEnabled("Rim Transparency Enabled", Float) = 0.0
            _RimTransparencyProgress("Rim Transparency Progress", Range(0.0, 1.0)) = 0.5
            _RimTransparencyProgressCoord("Rim Transparency Progress Coord", Float) = 0.0
            _RimTransparencySharpness("Rim Transparency Sharpness", Range(0.0, 1.0)) = 0.5
            _RimTransparencySharpnessCoord("Rim Transparency Sharpness Coord", Float) = 0.0
            _InverseRimTransparency("Inverse Rim Transparency", Float) = 0.0
            _LuminanceTransparencyEnabled("Luminance Transparency Enabled", Float) = 0.0
            _LuminanceTransparencyProgress("Luminance Transparency Progress", Range(0.0, 1.0)) = 0.5
            _LuminanceTransparencyProgressCoord("Luminance Transparency Progress Coord", Float) = 0.0
            _LuminanceTransparencySharpness("Luminance Transparency Sharpness", Range(0.0, 1.0)) = 0.5
            _LuminanceTransparencySharpnessCoord("Luminance Transparency Sharpness Coord", Float) = 0.0
            _InverseLuminanceTransparency("Inverse Luminance Transparency", Float) = 0.0
            _SoftParticlesEnabled("Soft Particles Enabled", Float) = 0.0
            _SoftParticlesIntensity("Soft Particles Intensity", Float) = 1.0
            _DepthFadeEnabled("Depth Fade Enabled", Float) = 0.0
            _DepthFadeNear("Depth Fade Near", Float) = 1.0
            _DepthFadeFar("Depth Fade Far", Float) = 10.0
            _DepthFadeWidth("Depth Fade Width", Float) = 1.0
    
            // Vertex Deformation
            _VertexDeformationEnabled ("Vertex Deformation Enabled", Float) = 0
            _VertexDeformationMap ("Vertex Deformation Map", 2D) = "white" {}
            _VertexDeformationMapOffsetXCoord("VertexDeformation Map Offset X Coord", Float) = 0.0
            _VertexDeformationMapOffsetYCoord("VertexDeformation Map Offset Y Coord", Float) = 0.0
            _VertexDeformationMapChannel("VertexDeformation Map Channel", Float) = 0.0
            _VertexDeformationIntensity("VertexDeformation Intensity", Float) = 0.1
            _VertexDeformationIntensityCoord("VertexDeformation Intensity Coord", Float) = 0.0
            _VertexDeformationBaseValue("Vertex Deformation Base Value", Range(0.0, 1.0)) = 0
    
            // Shadow Caster
            _ShadowCasterEnabled("Shadow Caster", Float) = 0
            _ShadowCasterApplyVertexDeformation("Shadow Caster Vertex Deformation Enabled", Float) = 0
            _ShadowCasterAlphaTestEnabled("Shadow Caster Alpha Test Enabled", Float) = 0
            _ShadowCasterAlphaCutoff("Shadow Caster Alpha Test Cutoff", Range(0.0, 1.0)) = 0.5
            _ShadowCasterAlphaAffectedByTintColor("Shadow Caster Alpha Effect By Tint Color", Float) = 0
            _ShadowCasterAlphaAffectedByFlowMap("Shadow Caster Alpha Effect By Flow Map", Float) = 0
            _ShadowCasterAlphaAffectedByAlphaTransitionMap("Shadow Caster Alpha Effect By Alpha Transition Map", Float) = 0
            _ShadowCasterAlphaAffectedByTransparencyLuminance("Shadow Caster Alpha Effect By Transparency Luminance", Float) = 0
        }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "PerformanceChecks" = "False"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            Name "ForwardBase"
            Tags { "LightMode" = "ForwardBase" }
            Blend [_BlendSrc] [_BlendDst]
            ZWrite [_ZWrite]
            Cull [_Cull]
            Lighting Off
            ZTest [_ZTest]
            ColorMask [_ColorMask]

            CGPROGRAM
            #pragma vertex vert
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:vertInstancingSetup
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.5
            #pragma shader_feature_local_fragment _ DEBUG_DISPLAY
            #pragma shader_feature_local_fragment _VERTEX_ALPHA_AS_TRANSITION_PROGRESS
            #pragma shader_feature_local_fragment _ALPHAMODULATE_ENABLED
            #pragma shader_feature_local_fragment _ALPHATEST_ENABLED
            #pragma shader_feature_local _BASE_MAP_MODE_2D _BASE_MAP_MODE_2D_ARRAY _BASE_MAP_MODE_3D
            #pragma shader_feature_local_vertex _BASE_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local_fragment _BASE_MAP_CHANNEL_ENABLED
            #pragma shader_feature_local_fragment _ _BASE_SAMPLER_STATE_POINT_MIRROR _BASE_SAMPLER_STATE_LINEAR_MIRROR _BASE_SAMPLER_STATE_TRILINEAR_MIRROR
            #pragma shader_feature_local_fragment _ _BASE_MAP_TONE_MODE_TRITONE _BASE_MAP_TONE_MODE_PENTONE
            #pragma shader_feature_local _ _TINT_AREA_ALL _TINT_AREA_RIM
            #pragma shader_feature_local _ _TINT_COLOR_ENABLED _TINT_MAP_ENABLED _TINT_MAP_3D_ENABLED _TINT_MAP_MODE_2D_ARRAY
            #pragma shader_feature_local_vertex _TINT_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local _FLOW_MAP_ENABLED
            #pragma shader_feature_local _FLOW_MAP_TARGET_BASE
            #pragma shader_feature_local _FLOW_MAP_TARGET_TINT
            #pragma shader_feature_local _FLOW_MAP_TARGET_EMISSION
            #pragma shader_feature_local _FLOW_MAP_TARGET_ALPHA_TRANSITION
            #pragma shader_feature_local _PARALLAX_MAP_TARGET_BASE
            #pragma shader_feature_local _PARALLAX_MAP_TARGET_TINT
            #pragma shader_feature_local _PARALLAX_MAP_TARGET_EMISSION
            #pragma shader_feature_local _PARALLAX_MAP_MODE_2D _PARALLAX_MAP_MODE_2D_ARRAY _PARALLAX_MAP_MODE_3D
            #pragma shader_feature_local_vertex _PARALLAX_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local_fragment _ _GREYSCALE_ENABLED _GRADIENT_MAP_ENABLED
            #pragma shader_feature_local _ _FADE_TRANSITION_ENABLED _DISSOLVE_TRANSITION_ENABLED
            #pragma shader_feature_local _ALPHA_TRANSITION_MAP_MODE_2D _ALPHA_TRANSITION_MAP_MODE_2D_ARRAY _ALPHA_TRANSITION_MAP_MODE_3D
            #pragma shader_feature_local _ _ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE _ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY
            #pragma shader_feature_local _ _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_3D
            #pragma shader_feature_local_vertex _ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local_vertex _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local _ _EMISSION_AREA_ALL _EMISSION_AREA_MAP _EMISSION_AREA_ALPHA
            #pragma shader_feature_local _EMISSION_MAP_MODE_2D _EMISSION_MAP_MODE_2D_ARRAY _EMISSION_MAP_MODE_3D
            #pragma shader_feature_local_fragment _ _EMISSION_COLOR_COLOR _EMISSION_COLOR_BASECOLOR _EMISSION_COLOR_MAP
            #pragma shader_feature_local_vertex _EMISSION_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local _TRANSPARENCY_BY_LUMINANCE
            #pragma shader_feature_local _TRANSPARENCY_BY_RIM
            #pragma shader_feature_local _SOFT_PARTICLES_ENABLED
            #pragma shader_feature_local _DEPTH_FADE_ENABLED
            #pragma shader_feature_local_vertex _ _VERTEX_DEFORMATION_ENABLED
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "BuiltIn/NovaBuiltInCore.cginc"
            #include "BuiltIn/NovaBuiltInFog.cginc"
            #include "BuiltIn/NovaBuiltInTexture.cginc"
            #include "BuiltIn/NovaBuiltInDepth.cginc"
            #include "BuiltIn/NovaBuiltInUI.cginc"
            #include "BuiltIn/NovaBuiltInUberUnlit.cginc"

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

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                half baseMapProgress : TEXCOORD1;
                float2 tintUv : TEXCOORD2;
                half tintMapProgress : TEXCOORD3;
                float2 flowUv : TEXCOORD4;
                float3 parallaxUvAndProgress : TEXCOORD5;
                float2 positionOS : TEXCOORD6;
                float4 customCoord1 : TEXCOORD7;
                float4 customCoord2 : TEXCOORD8;
                float3 normalWS : TEXCOORD9;
                float3 viewDirWS : TEXCOORD10;
                float3 viewDirTS : TEXCOORD11;
                float3 alphaTransitionUvAndProgress : TEXCOORD12;
                float3 alphaTransitionSecondUvAndProgress : TEXCOORD13;
                float3 emissionUvAndProgress : TEXCOORD14;
                float4 screenPosition : TEXCOORD15;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                NOVA_SETUP_PARTICLE(v)
                v2f o;
                #if defined(_VERTEX_DEFORMATION_ENABLED)
                float2 vertexDeformationUv = NovaTransformUV(v.uv, _VertexDeformationMap_ST);
                vertexDeformationUv.x += NovaGetCustomCoord(_VertexDeformationMapOffsetXCoord, v.customCoord1, v.customCoord2);
                vertexDeformationUv.y += NovaGetCustomCoord(_VertexDeformationMapOffsetYCoord, v.customCoord1, v.customCoord2);
                half4 vertexDeformation = NOVA_SAMPLE_TEX2D_LOD(_VertexDeformationMap, vertexDeformationUv, 0.0);
                float vertexDeformationIntensity = _VertexDeformationIntensity + NovaGetCustomCoord(_VertexDeformationIntensityCoord, v.customCoord1, v.customCoord2);
                float vertexDeformationAmount = (NovaSelectChannel(vertexDeformation, _VertexDeformationMapChannel) - _VertexDeformationBaseValue) * vertexDeformationIntensity;
                v.vertex.xyz += normalize(v.normal) * vertexDeformationAmount;
                #endif

                o.vertex = NovaObjectToClipPos(v.vertex);
                o.screenPosition = NovaComputeScreenPos(o.vertex);
                o.screenPosition.z = -UnityObjectToViewPos(v.vertex).z;
                float3 positionWS = NovaObjectToWorld(v.vertex.xyz);
                o.normalWS = NovaObjectToWorldNormal(v.normal);
                o.viewDirWS = NovaWorldSpaceViewDir(positionWS);
                float3 tangentWS = NovaObjectToWorldTangent(v.tangent.xyz);
                float3 bitangentWS = NovaCreateBitangent(o.normalWS, tangentWS, v.tangent.w);
                o.viewDirTS = float3(dot(o.viewDirWS, tangentWS), dot(o.viewDirWS, bitangentWS), dot(o.viewDirWS, o.normalWS));
                half progress = _BaseMapProgress + NovaGetCustomCoord(_BaseMapProgressCoord, v.customCoord1, v.customCoord2);
                half randomValue = NovaGetCustomCoord(_BaseMapRandomRowCoord, v.customCoord1, v.customCoord2);
                o.baseMapProgress = NovaBaseMapProgress(progress, _BaseMapSliceCount, _BaseMapRowCount, randomValue);
                #if defined(_BASE_MAP_MODE_2D_ARRAY)
                o.uv = NovaTransformUV(v.uv, _BaseMap2DArray_ST);
                #elif defined(_BASE_MAP_MODE_3D)
                o.uv = NovaTransformUV(v.uv, _BaseMap3D_ST);
                #else
                o.uv = NovaTransformUV(v.uv, _BaseMap_ST);
                #endif
                o.tintUv = v.uv;
                #if defined(_TINT_MAP_ENABLED) || defined(_TINT_MAP_MODE_2D_ARRAY) || defined(_TINT_MAP_3D_ENABLED)
                if (_TintMapRotation > 0.0h || _TintMapRotationCoord > 0.0h)
                {
                    half tintAngle = _TintMapRotation + NovaGetCustomCoord(_TintMapRotationCoord, v.customCoord1, v.customCoord2);
                    o.tintUv = NovaRotateUV(o.tintUv, tintAngle * 6.2831853h, _TintMapRotationOffsets.xy);
                }
                    #if defined(_TINT_MAP_MODE_2D_ARRAY)
                    o.tintUv = NovaTransformUV(o.tintUv, _TintMap2DArray_ST);
                    #elif defined(_TINT_MAP_3D_ENABLED)
                    o.tintUv = NovaTransformUV(o.tintUv, _TintMap3D_ST);
                    #else
                    o.tintUv = NovaTransformUV(o.tintUv, _TintMap_ST);
                    #endif
                o.tintUv.x += NovaGetCustomCoord(_TintMapOffsetXCoord, v.customCoord1, v.customCoord2);
                o.tintUv.y += NovaGetCustomCoord(_TintMapOffsetYCoord, v.customCoord1, v.customCoord2);
                #endif
                half tintProgress2DArray = _TintMapProgress + NovaGetCustomCoord(_TintMapProgressCoord, v.customCoord1, v.customCoord2);
                half tintProgress3D = _TintMap3DProgress + NovaGetCustomCoord(_TintMap3DProgressCoord, v.customCoord1, v.customCoord2);
                half tintRandomValue = NovaGetCustomCoord(_TintMapRandomRowCoord, v.customCoord1, v.customCoord2);
                o.tintMapProgress = NovaTintMapProgress(tintProgress2DArray, tintProgress3D, _TintMapSliceCount, _TintMapRowCount, tintRandomValue);
                o.flowUv = v.uv;
                #if defined(_FLOW_MAP_ENABLED) || defined(_FLOW_MAP_TARGET_BASE) || defined(_FLOW_MAP_TARGET_TINT) || defined(_FLOW_MAP_TARGET_EMISSION) || defined(_FLOW_MAP_TARGET_ALPHA_TRANSITION)
                if (_FlowMapRotation > 0.0h || _FlowMapRotationCoord > 0.0h)
                {
                    half flowAngle = _FlowMapRotation + NovaGetCustomCoord(_FlowMapRotationCoord, v.customCoord1, v.customCoord2);
                    o.flowUv = NovaRotateUV(o.flowUv, flowAngle * 6.2831853h, _FlowMapRotationOffsets.xy);
                }
                o.flowUv = NovaTransformUV(o.flowUv, _FlowMap_ST);
                o.flowUv.x += NovaGetCustomCoord(_FlowMapOffsetXCoord, v.customCoord1, v.customCoord2);
                o.flowUv.y += NovaGetCustomCoord(_FlowMapOffsetYCoord, v.customCoord1, v.customCoord2);
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
                o.parallaxUvAndProgress = float3(v.uv, 0.0h);
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
                o.positionOS = v.vertex.xy;
                o.customCoord1 = v.customCoord1;
                o.customCoord2 = v.customCoord2;
                o.color = v.color;
                return o;
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

            fixed4 frag(v2f i) : SV_Target
            {
                float2 baseUv = i.uv;
                float2 tintUv = i.tintUv;
                float2 alphaTransitionUv = i.alphaTransitionUvAndProgress.xy;
                float2 alphaTransitionSecondUv = i.alphaTransitionSecondUvAndProgress.xy;
                float2 emissionUv = i.emissionUvAndProgress.xy;

                #if defined(_FLOW_MAP_TARGET_BASE) || defined(_FLOW_MAP_TARGET_TINT) || defined(_FLOW_MAP_TARGET_EMISSION) || defined(_FLOW_MAP_TARGET_ALPHA_TRANSITION)
                half flowIntensity = _FlowIntensity + NovaGetCustomCoord(_FlowIntensityCoord, i.customCoord1, i.customCoord2);
                half2 flowOffset = NovaFlowMapUvOffset(NOVA_SAMPLE_TEX2D(_FlowMap, i.flowUv), _FlowMapChannelsX, _FlowMapChannelsY, flowIntensity);
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
                half2 parallaxOffset = NovaParallaxOffset(parallaxHeight, _ParallaxStrength, i.viewDirTS);
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

                half4 color = SampleBaseMap(baseUv, i.baseMapProgress);
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
                    half3 normalWS = NovaSafeNormalize(i.normalWS);
                    half3 viewDirWS = NovaSafeNormalize(i.viewDirWS);
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
                    NovaApplyTintColor(color, SampleTintMap(tintUv, i.tintMapProgress), tintBlendRate);
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
                half3 transparencyNormalWS = NovaSafeNormalize(i.normalWS);
                half3 transparencyViewDirWS = NovaSafeNormalize(i.viewDirWS);
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
                color.a *= NovaSoftParticleFactor(i.screenPosition, _SoftParticlesIntensity);
                #endif

                #if defined(_DEPTH_FADE_ENABLED)
                color.a *= NovaDepthFadeFactor(_DepthFadeNear, _DepthFadeFar, _DepthFadeWidth, i.screenPosition);
                #endif

                color.a *= NovaUIClipAlpha(i.positionOS);

                #ifdef _ALPHATEST_ENABLED
                clip(color.a - _Cutoff);
                #endif

                #ifdef _ALPHAMODULATE_ENABLED
                color.rgb = lerp(fixed3(1.0, 1.0, 1.0), color.rgb, color.a);
                #endif

                NovaApplyEyeFog(color, i.screenPosition.z);
                return color;
            }
            ENDCG
        }
    }
    Fallback Off
    CustomEditor "Nova.Editor.Core.Scripts.UIParticlesUberUnlitGUI"
}

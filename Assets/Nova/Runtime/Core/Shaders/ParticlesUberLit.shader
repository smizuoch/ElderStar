Shader "Nova/Particles/UberLit"
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
            _ZWrite("ZWrite", Float) = 1.0
            _ZWriteOverride("ZWrite Override", Float) = -1.0
            _ZTest("ZTest", Float) = 4.0
            _LitWorkflowMode("Lit Workflow Mode",Float) = 0.0
            _LitReceiveShadows("Lit Receive Shadows", Float) = 0.0
            _SpecularHighlights("Specular Highlights", Float) = 0.0
            _EnvironmentReflections("Environment Reflections", Float) = 1.0
    
            // Surface Maps
            _NormalMap("Normal Map", 2D) = "" {}
            _NormalMap2DArray("Normal Map 2D Array", 2DArray) = "" {}
            _NormalMap3D("Normal Map 3D", 3D) = "" {}
            _NormalMapBumpScale("Normal Map Bump Scale", Float) = 1.0
            _SpecularMap("Specular Map", 2D) = "" {}
            _SpecularMap2DArray("Specular Map 2D Array", 2DArray) = "" {}
            _SpecularMap3D("Specluar Map 3D", 3D) = "" {}
            _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
            _SpecularMapChannelsX("Specular Map Channes X", Float) = 0.0
            _MetallicMap("Metallic Map", 2D) = "" {}
            _MetallicMap2DArray("Metallic Map 2D Array", 2DArray) = "" {}
            _MetallicMap3D("Metallic Map 3D", 3D) = "" {}
            [Gamma]_Metallic("Metallic", Range( 0.0, 1.0)) = 1.0
            _MetallicMapChannelsX("Metallic Map Channes X", Float) = 0.0
            _SmoothnessMap("Smoothness Map", 2D) = "" {}
            _SmoothnessMap2DArray("Smoothness Map 2D Array", 2DArray) = "" {}
            _SmoothnessMap3D("Smoothness Map 3D", 3D) = "" {}
            _Smoothness("Smoothness", Range( 0.0, 1.0)) = 1.0
            _SmoothnessMapChannelsX("Smoothness Map Channes X", Float) = 3.0
    
            // Base Map
            _BaseMapMode("Base Map Mode", Float) = 0.0
            [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
            _BaseMap2DArray("Base Map 2D Array", 2DArray) = "" {}
            _BaseMap3D("Base Map 3D", 3D) = "" {}
            _BaseMapProgress("Base Map Progress", Range(0.0, 1.0)) = 0.0
            _BaseMapProgressCoord("Base Map Progress Coord", Float) = 0.0
            _BaseMapSliceCount("Base Map Slice Count", Float) = 4.0
            _BaseMapOffsetXCoord("Base Map Offset X Coord", Float) = 0.0
            _BaseMapOffsetYCoord("Base Map Offset Y Coord", Float) = 0.0
            _BaseMapRotation("Base Map Rotation", Range(0.0, 1.0)) = 0.0
            _BaseMapRotationCoord("Base Map Rotation Coord", Float) = 0.0
            _BaseMapRotationOffsets("Base Map Rotation Offsets", Vector) = (0.0, 0.0, 0.0, 0.0)
            _BaseMapMirrorSampling("Base Map Mirror Sampling", Float) = 0.0
            
            // Base Map Random Row Selection
            _BaseMapRandomRowSelectionEnabled("Base Map Random Row Selection Enabled", Float) = 0.0
            _BaseMapRandomRowCoord("Base Map Random Row Coord", Float) = 0.0
            _BaseMapRowCount("Base Map Row Count", Float) = 1.0
            
            // Base Map Channel
            _BaseMapChannel("Base Map Channel", Float) = 0.0
            
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
            
            // Tint Map Random Row Selection
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
            _ParallaxMapSliceCount("Parallax Map Slice Count", Float) = 4.0
            _ParallaxMapChannel("Parallax Map Channel", Float) = 0.0
            _ParallaxStrength("Parallax Strength", Range(0.0, 1.0)) = 0.3
            _ParallaxMapTarget("Parallax Map Target", Float) = 1.0
            
            // Parallax Map Random Row Selection
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
            
            // Alpha Transition Random Row Selection
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
            
            // Emission Random Row Selection
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
            "NovaDepthNormals" = "True"
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "PerformanceChecks" = "False"
        }

        Pass
        {
            Name "ForwardBase"
            Tags { "LightMode" = "ForwardBase" }
            Blend [_BlendSrc] [_BlendDst]
            ZWrite [_ZWrite]
            Cull [_Cull]
            ZTest [_ZTest]

            CGPROGRAM
            #pragma vertex vert
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:vertInstancingSetup
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.5
            #pragma multi_compile_fwdbase
            #pragma shader_feature_local_fragment _ DEBUG_DISPLAY
            #pragma shader_feature_local_fragment EVALUATE_SH_MIXED
            #pragma shader_feature_local_fragment EVALUATE_SH_VERTEX
            #pragma shader_feature_local_fragment _VERTEX_ALPHA_AS_TRANSITION_PROGRESS
            #pragma shader_feature_local_fragment _ALPHAMODULATE_ENABLED
            #pragma shader_feature_local_fragment _ALPHATEST_ENABLED
            #pragma shader_feature_local_fragment _BASE_MAP_CHANNEL_ENABLED
            #pragma shader_feature_local _BASE_MAP_MODE_2D _BASE_MAP_MODE_2D_ARRAY _BASE_MAP_MODE_3D
            #pragma shader_feature_local_vertex _BASE_MAP_RANDOM_ROW_SELECTION_ENABLED
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
            #pragma shader_feature_local _SHADOW_CASTER_ALPHA_TEST_ENABLED
            #pragma shader_feature_local _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _NORMAL_MAP_ENABLED
            #pragma shader_feature_local_fragment _SPECULAR_MAP_ENABLED
            #pragma shader_feature_local_fragment _METALLIC_MAP_ENABLED
            #pragma shader_feature_local_fragment _SMOOTHNESS_MAP_ENABLED
            #pragma shader_feature_local_fragment _RECEIVE_SHADOWS_ENABLED
            #pragma shader_feature_local_fragment _SPECULAR_HIGHLIGHTS_ENABLED
            #pragma shader_feature_local_fragment _ENVIRONMENT_REFLECTIONS_ENABLED
            #pragma shader_feature_local_fragment _MAIN_LIGHT_SHADOWS
            #pragma shader_feature_local_fragment _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma shader_feature_local_fragment _SHADOWS_SOFT
            #pragma shader_feature_local_fragment _ADDITIONAL_LIGHTS
            #pragma shader_feature_local_fragment _ADDITIONAL_LIGHTS_VERTEX
            #pragma shader_feature_local_fragment _ADDITIONAL_LIGHT_SHADOWS
            #pragma shader_feature_local_fragment _FORWARD_PLUS
            #pragma shader_feature_local_fragment _CLUSTER_LIGHT_LOOP
            #include "BuiltIn/NovaBuiltInParticleFeatures.cginc"
            ENDCG
        }

        Pass
        {
            Name "ForwardAdd"
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            ZWrite Off
            Cull [_Cull]
            ZTest [_ZTest]

            CGPROGRAM
            #pragma vertex vert
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:vertInstancingSetup
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.5
            #pragma multi_compile_fwdadd_fullshadows
            #pragma shader_feature_local_fragment _ DEBUG_DISPLAY
            #pragma shader_feature_local_fragment EVALUATE_SH_MIXED
            #pragma shader_feature_local_fragment EVALUATE_SH_VERTEX
            #pragma shader_feature_local_fragment _VERTEX_ALPHA_AS_TRANSITION_PROGRESS
            #pragma shader_feature_local_fragment _ALPHAMODULATE_ENABLED
            #pragma shader_feature_local_fragment _ALPHATEST_ENABLED
            #pragma shader_feature_local_fragment _BASE_MAP_CHANNEL_ENABLED
            #pragma shader_feature_local _BASE_MAP_MODE_2D _BASE_MAP_MODE_2D_ARRAY _BASE_MAP_MODE_3D
            #pragma shader_feature_local_vertex _BASE_MAP_RANDOM_ROW_SELECTION_ENABLED
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
            #pragma shader_feature_local _SHADOW_CASTER_ALPHA_TEST_ENABLED
            #pragma shader_feature_local _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _NORMAL_MAP_ENABLED
            #pragma shader_feature_local_fragment _SPECULAR_MAP_ENABLED
            #pragma shader_feature_local_fragment _METALLIC_MAP_ENABLED
            #pragma shader_feature_local_fragment _SMOOTHNESS_MAP_ENABLED
            #pragma shader_feature_local_fragment _RECEIVE_SHADOWS_ENABLED
            #pragma shader_feature_local_fragment _SPECULAR_HIGHLIGHTS_ENABLED
            #pragma shader_feature_local_fragment _ENVIRONMENT_REFLECTIONS_ENABLED
            #pragma shader_feature_local_fragment _MAIN_LIGHT_SHADOWS
            #pragma shader_feature_local_fragment _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma shader_feature_local_fragment _SHADOWS_SOFT
            #pragma shader_feature_local_fragment _ADDITIONAL_LIGHTS
            #pragma shader_feature_local_fragment _ADDITIONAL_LIGHTS_VERTEX
            #pragma shader_feature_local_fragment _ADDITIONAL_LIGHT_SHADOWS
            #pragma shader_feature_local_fragment _FORWARD_PLUS
            #pragma shader_feature_local_fragment _CLUSTER_LIGHT_LOOP
            #include "BuiltIn/NovaBuiltInParticleFeatures.cginc"
            ENDCG
        }

        Pass
        {
            Name "SceneSelectionPass"
            Tags { "LightMode" = "SceneSelectionPass" }
            Cull [_Cull]
            ZWrite On
            ZTest [_ZTest]

            CGPROGRAM
            #pragma vertex vertSceneSelection
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:vertInstancingSetup
            #pragma fragment fragSceneHighlight
            #pragma target 3.5
            #pragma shader_feature_local_vertex _ _VERTEX_DEFORMATION_ENABLED
            #include "BuiltIn/NovaBuiltInCore.cginc"
            #include "BuiltIn/NovaBuiltInTexture.cginc"
            #include "BuiltIn/NovaBuiltInUberUnlit.cginc"

            NOVA_DECLARE_TEX2D(_VertexDeformationMap);
            float _VertexDeformationIntensity;
            float _VertexDeformationBaseValue;
            half _VertexDeformationMapOffsetXCoord;
            half _VertexDeformationMapOffsetYCoord;
            half _VertexDeformationMapChannel;
            half _VertexDeformationIntensityCoord;
            float _ObjectId;
            float _PassValue;

            struct NovaEditorAttributes
            {
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 customCoord1 : TEXCOORD1;
                float4 customCoord2 : TEXCOORD2;
            };

            struct NovaEditorVaryings
            {
                float4 vertex : SV_POSITION;
            };

            void NovaEditorApplyVertexDeformation(inout float4 vertex, float3 normal, float2 uv, float4 customCoord1, float4 customCoord2)
            {
                #if defined(_VERTEX_DEFORMATION_ENABLED)
                float2 vertexDeformationUv = NovaTransformUV(uv, _VertexDeformationMap_ST);
                vertexDeformationUv.x += NovaGetCustomCoord(_VertexDeformationMapOffsetXCoord, customCoord1, customCoord2);
                vertexDeformationUv.y += NovaGetCustomCoord(_VertexDeformationMapOffsetYCoord, customCoord1, customCoord2);
                half4 vertexDeformation = NOVA_SAMPLE_TEX2D_LOD(_VertexDeformationMap, vertexDeformationUv, 0.0);
                float vertexDeformationIntensity = _VertexDeformationIntensity + NovaGetCustomCoord(_VertexDeformationIntensityCoord, customCoord1, customCoord2);
                float vertexDeformationAmount = (NovaSelectChannel(vertexDeformation, _VertexDeformationMapChannel) - _VertexDeformationBaseValue) * vertexDeformationIntensity;
                vertex.xyz += normalize(normal) * vertexDeformationAmount;
                #endif
            }

            NovaEditorVaryings vertSceneSelection(NovaEditorAttributes v)
            {
                NOVA_SETUP_PARTICLE(v)
                NovaEditorVaryings o;
                NovaEditorApplyVertexDeformation(v.vertex, v.normal, v.uv, v.customCoord1, v.customCoord2);
                o.vertex = NovaObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 fragSceneHighlight(NovaEditorVaryings i) : SV_Target
            {
                return fixed4(_ObjectId, _PassValue, 1.0, 1.0);
            }
            ENDCG
        }

        Pass
        {
            Name "Picking"
            Tags { "LightMode" = "Picking" }
            Cull [_Cull]
            ZWrite On
            ZTest [_ZTest]

            CGPROGRAM
            #pragma vertex vertPicking
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:vertInstancingSetup
            #pragma fragment fragScenePicking
            #pragma target 3.5
            #pragma shader_feature_local_vertex _ _VERTEX_DEFORMATION_ENABLED
            #include "BuiltIn/NovaBuiltInCore.cginc"
            #include "BuiltIn/NovaBuiltInTexture.cginc"
            #include "BuiltIn/NovaBuiltInUberUnlit.cginc"

            NOVA_DECLARE_TEX2D(_VertexDeformationMap);
            float _VertexDeformationIntensity;
            float _VertexDeformationBaseValue;
            half _VertexDeformationMapOffsetXCoord;
            half _VertexDeformationMapOffsetYCoord;
            half _VertexDeformationMapChannel;
            half _VertexDeformationIntensityCoord;
            float4 _SelectionID;

            struct NovaPickingAttributes
            {
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 customCoord1 : TEXCOORD1;
                float4 customCoord2 : TEXCOORD2;
            };

            struct NovaPickingVaryings
            {
                float4 vertex : SV_POSITION;
            };

            void NovaPickingApplyVertexDeformation(inout float4 vertex, float3 normal, float2 uv, float4 customCoord1, float4 customCoord2)
            {
                #if defined(_VERTEX_DEFORMATION_ENABLED)
                float2 vertexDeformationUv = NovaTransformUV(uv, _VertexDeformationMap_ST);
                vertexDeformationUv.x += NovaGetCustomCoord(_VertexDeformationMapOffsetXCoord, customCoord1, customCoord2);
                vertexDeformationUv.y += NovaGetCustomCoord(_VertexDeformationMapOffsetYCoord, customCoord1, customCoord2);
                half4 vertexDeformation = NOVA_SAMPLE_TEX2D_LOD(_VertexDeformationMap, vertexDeformationUv, 0.0);
                float vertexDeformationIntensity = _VertexDeformationIntensity + NovaGetCustomCoord(_VertexDeformationIntensityCoord, customCoord1, customCoord2);
                float vertexDeformationAmount = (NovaSelectChannel(vertexDeformation, _VertexDeformationMapChannel) - _VertexDeformationBaseValue) * vertexDeformationIntensity;
                vertex.xyz += normalize(normal) * vertexDeformationAmount;
                #endif
            }

            NovaPickingVaryings vertPicking(NovaPickingAttributes v)
            {
                NOVA_SETUP_PARTICLE(v)
                NovaPickingVaryings o;
                NovaPickingApplyVertexDeformation(v.vertex, v.normal, v.uv, v.customCoord1, v.customCoord2);
                o.vertex = NovaObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 fragScenePicking(NovaPickingVaryings i) : SV_Target
            {
                return _SelectionID;
            }
            ENDCG
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            ZTest LEqual
            Cull [_Cull]
            ColorMask RGBA
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:vertInstancingSetup
            #pragma require 2darray
            #pragma shader_feature_local_fragment _VERTEX_ALPHA_AS_TRANSITION_PROGRESS
            #pragma shader_feature_local _ALPHATEST_ENABLED
            #pragma shader_feature_local _BASE_MAP_MODE_2D _BASE_MAP_MODE_2D_ARRAY _BASE_MAP_MODE_3D
            #pragma shader_feature_local_vertex _BASE_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local_fragment _BASE_MAP_CHANNEL_ENABLED
            #pragma shader_feature_local_fragment _ _BASE_SAMPLER_STATE_POINT_MIRROR _BASE_SAMPLER_STATE_LINEAR_MIRROR _BASE_SAMPLER_STATE_TRILINEAR_MIRROR
            #pragma shader_feature_local_fragment _ _BASE_MAP_TONE_MODE_TRITONE _BASE_MAP_TONE_MODE_PENTONE
            #pragma shader_feature_local _NORMAL_MAP_ENABLED
            #pragma shader_feature_local _ _TINT_AREA_ALL _TINT_AREA_RIM
            #pragma shader_feature_local _ _TINT_COLOR_ENABLED _TINT_MAP_ENABLED _TINT_MAP_3D_ENABLED _TINT_MAP_MODE_2D_ARRAY
            #pragma shader_feature_local_vertex _TINT_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local _FLOW_MAP_ENABLED // Obsolete, but retained for compatibility.
            #pragma shader_feature_local _FLOW_MAP_TARGET_BASE
            #pragma shader_feature_local _FLOW_MAP_TARGET_TINT
            #pragma shader_feature_local _FLOW_MAP_TARGET_EMISSION
            #pragma shader_feature_local _FLOW_MAP_TARGET_ALPHA_TRANSITION
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
            #pragma shader_feature_local_fragment _TRANSPARENCY_BY_LUMINANCE
            #pragma shader_feature_local _TRANSPARENCY_BY_RIM
            #pragma shader_feature_local _SOFT_PARTICLES_ENABLED
            #pragma shader_feature_local _DEPTH_FADE_ENABLED
            #pragma shader_feature_local_vertex _ _VERTEX_DEFORMATION_ENABLED
            #define NOVA_DEPTH_NORMALS
            #include "BuiltIn/NovaBuiltInParticleFeatures.cginc"
            ENDCG
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            CGPROGRAM
            #pragma vertex vertDepthOnly
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:vertInstancingSetup
            #pragma fragment fragDepthOnly
            #pragma target 3.5
            #pragma require 2darray
            #pragma shader_feature_local_fragment _ALPHATEST_ENABLED
            #pragma shader_feature_local_fragment _VERTEX_ALPHA_AS_TRANSITION_PROGRESS
            #pragma shader_feature_local _BASE_MAP_MODE_2D _BASE_MAP_MODE_2D_ARRAY _BASE_MAP_MODE_3D
            #pragma shader_feature_local_vertex _BASE_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local_fragment _BASE_MAP_CHANNEL_ENABLED
            #pragma shader_feature_local_fragment _ _BASE_SAMPLER_STATE_POINT_MIRROR _BASE_SAMPLER_STATE_LINEAR_MIRROR _BASE_SAMPLER_STATE_TRILINEAR_MIRROR
            #pragma shader_feature_local _ _TINT_AREA_ALL
            #pragma shader_feature_local _ _TINT_COLOR_ENABLED _TINT_MAP_ENABLED _TINT_MAP_3D_ENABLED _TINT_MAP_MODE_2D_ARRAY
            #pragma shader_feature_local_vertex _TINT_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local _FLOW_MAP_TARGET_BASE
            #pragma shader_feature_local _FLOW_MAP_TARGET_TINT
            #pragma shader_feature_local _FLOW_MAP_TARGET_ALPHA_TRANSITION
            #pragma shader_feature_local _ _FADE_TRANSITION_ENABLED _DISSOLVE_TRANSITION_ENABLED
            #pragma shader_feature_local _ALPHA_TRANSITION_MAP_MODE_2D _ALPHA_TRANSITION_MAP_MODE_2D_ARRAY _ALPHA_TRANSITION_MAP_MODE_3D
            #pragma shader_feature_local _ _ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE _ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY
            #pragma shader_feature_local _ _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_3D
            #pragma shader_feature_local_vertex _ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local_vertex _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local _TRANSPARENCY_BY_LUMINANCE
            #pragma shader_feature_local_vertex _ _VERTEX_DEFORMATION_ENABLED
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _NORMAL_MAP_ENABLED
            #pragma shader_feature_local_fragment _METALLIC_MAP_ENABLED
            #pragma shader_feature_local_fragment _SMOOTHNESS_MAP_ENABLED
            #include "BuiltIn/NovaBuiltInCore.cginc"
            #include "BuiltIn/NovaBuiltInTexture.cginc"
            #include "BuiltIn/NovaBuiltInUberUnlit.cginc"
            #include "BuiltIn/NovaBuiltInBaseUV.cginc"

            NOVA_DECLARE_TEX2D(_BaseMap);
            NOVA_DECLARE_TEX2D_ARRAY(_BaseMap2DArray);
            float4 _BaseMap2DArray_ST;
            NOVA_DECLARE_TEX3D(_BaseMap3D);
            NOVA_DECLARE_TEX2D(_TintMap);
            NOVA_DECLARE_TEX2D_ARRAY(_TintMap2DArray);
            float4 _TintMap2DArray_ST;
            NOVA_DECLARE_TEX3D(_TintMap3D);
            NOVA_DECLARE_TEX2D(_FlowMap);
            NOVA_DECLARE_TEX2D(_AlphaTransitionMap);
            NOVA_DECLARE_TEX2D_ARRAY(_AlphaTransitionMap2DArray);
            float4 _AlphaTransitionMap2DArray_ST;
            NOVA_DECLARE_TEX3D(_AlphaTransitionMap3D);
            NOVA_DECLARE_TEX2D(_AlphaTransitionMapSecondTexture);
            NOVA_DECLARE_TEX2D_ARRAY(_AlphaTransitionMapSecondTexture2DArray);
            float4 _AlphaTransitionMapSecondTexture2DArray_ST;
            NOVA_DECLARE_TEX3D(_AlphaTransitionMapSecondTexture3D);
            NOVA_DECLARE_TEX2D(_VertexDeformationMap);

            fixed4 _TintColor;
            fixed _Cutoff;
            half _BaseMapProgress;
            half _BaseMapProgressCoord;
            half _BaseMapSliceCount;
            half _BaseMapRandomRowCoord;
            half _BaseMapRowCount;
            half _TintMap3DProgress;
            half _TintMap3DProgressCoord;
            half _TintMapProgress;
            half _TintMapProgressCoord;
            half _TintMapSliceCount;
            half _TintMapOffsetXCoord;
            half _TintMapOffsetYCoord;
            half _TintBlendRate;
            half _TintBlendRateCoord;
            half _TintMapRandomRowCoord;
            half _TintMapRowCount;
            half _FlowMapOffsetXCoord;
            half _FlowMapOffsetYCoord;
            half _FlowMapChannelsX;
            half _FlowMapChannelsY;
            half _FlowIntensity;
            half _FlowIntensityCoord;
            half _AlphaTransitionMapProgress;
            half _AlphaTransitionMapProgressCoord;
            half _AlphaTransitionMapOffsetXCoord;
            half _AlphaTransitionMapOffsetYCoord;
            half _AlphaTransitionMapChannelsX;
            half _AlphaTransitionMapSliceCount;
            half _AlphaTransitionProgress;
            half _AlphaTransitionProgressCoord;
            half _DissolveSharpness;
            half _AlphaTransitionMapSecondTextureChannelsX;
            half _AlphaTransitionProgressSecondTexture;
            half _AlphaTransitionProgressCoordSecondTexture;
            half _DissolveSharpnessSecondTexture;
            half _LuminanceTransparencyProgress;
            half _LuminanceTransparencyProgressCoord;
            half _LuminanceTransparencySharpness;
            half _LuminanceTransparencySharpnessCoord;
            half _InverseLuminanceTransparency;
            half _VertexDeformationMapOffsetXCoord;
            half _VertexDeformationMapOffsetYCoord;
            half _VertexDeformationMapChannel;
            float _VertexDeformationIntensity;
            half _VertexDeformationIntensityCoord;
            float _VertexDeformationBaseValue;

            struct NovaDepthAttributes
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 customCoord1 : TEXCOORD1;
                float4 customCoord2 : TEXCOORD2;
                fixed4 color : COLOR;
            };

            struct NovaDepthVaryings
            {
                UNITY_VERTEX_OUTPUT_STEREO
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 customCoord1 : TEXCOORD1;
                float4 customCoord2 : TEXCOORD2;
                half baseProgress : TEXCOORD3;
                fixed4 color : COLOR;
            };

            void NovaDepthApplyVertexDeformation(inout float4 vertex, float3 normal, float2 uv, float4 customCoord1, float4 customCoord2)
            {
                #if defined(_VERTEX_DEFORMATION_ENABLED)
                float2 vertexDeformationUv = NovaTransformUV(uv, _VertexDeformationMap_ST);
                vertexDeformationUv.x += NovaGetCustomCoord(_VertexDeformationMapOffsetXCoord, customCoord1, customCoord2);
                vertexDeformationUv.y += NovaGetCustomCoord(_VertexDeformationMapOffsetYCoord, customCoord1, customCoord2);
                half4 vertexDeformation = NOVA_SAMPLE_TEX2D_LOD(_VertexDeformationMap, vertexDeformationUv, 0.0);
                float vertexDeformationIntensity = _VertexDeformationIntensity + NovaGetCustomCoord(_VertexDeformationIntensityCoord, customCoord1, customCoord2);
                float vertexDeformationAmount = (NovaSelectChannel(vertexDeformation, _VertexDeformationMapChannel) - _VertexDeformationBaseValue) * vertexDeformationIntensity;
                vertex.xyz += normalize(normal) * vertexDeformationAmount;
                #endif
            }

            NovaDepthVaryings vertDepthOnly(NovaDepthAttributes v)
            {
                NOVA_SETUP_PARTICLE(v)
                NovaDepthVaryings o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                NovaDepthApplyVertexDeformation(v.vertex, v.normal, v.uv, v.customCoord1, v.customCoord2);
                o.vertex = NovaObjectToClipPos(v.vertex);
                #if defined(_BASE_MAP_MODE_2D_ARRAY)
                o.uv = NovaBaseUV(v.uv, _BaseMap2DArray_ST, v.customCoord1, v.customCoord2);
                #elif defined(_BASE_MAP_MODE_3D)
                o.uv = NovaBaseUV(v.uv, _BaseMap3D_ST, v.customCoord1, v.customCoord2);
                #else
                o.uv = NovaBaseUV(v.uv, _BaseMap_ST, v.customCoord1, v.customCoord2);
                #endif
                o.baseProgress = NovaBaseMapProgress(
                    _BaseMapProgress + NovaGetCustomCoord(_BaseMapProgressCoord, v.customCoord1, v.customCoord2),
                    _BaseMapSliceCount, _BaseMapRowCount,
                    NovaGetCustomCoord(_BaseMapRandomRowCoord, v.customCoord1, v.customCoord2));
                o.customCoord1 = v.customCoord1;
                o.customCoord2 = v.customCoord2;
                o.color = v.color;
                return o;
            }

            half4 NovaDepthSampleBaseMap(float2 uv, half progress)
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

            half4 NovaDepthAlphaColor(NovaDepthVaryings i)
            {
                float2 baseUv = i.uv;
                #if defined(_FLOW_MAP_TARGET_BASE)
                half flowIntensity = _FlowIntensity + NovaGetCustomCoord(_FlowIntensityCoord, i.customCoord1, i.customCoord2);
                baseUv += NovaFlowMapUvOffset(NOVA_SAMPLE_TEX2D(_FlowMap, NovaTransformUV(i.uv, _FlowMap_ST)), _FlowMapChannelsX, _FlowMapChannelsY, flowIntensity);
                #endif

                half4 color = NovaDepthSampleBaseMap(baseUv, i.baseProgress);
                #if defined(_TINT_AREA_ALL) && defined(_TINT_COLOR_ENABLED)
                half tintBlendRate = _TintBlendRate + NovaGetCustomCoord(_TintBlendRateCoord, i.customCoord1, i.customCoord2);
                NovaApplyTintColor(color, _TintColor, tintBlendRate);
                #endif
                #if defined(_FADE_TRANSITION_ENABLED) || defined(_DISSOLVE_TRANSITION_ENABLED)
                half alphaTransitionProgress = _AlphaTransitionProgress + NovaGetCustomCoord(_AlphaTransitionProgressCoord, i.customCoord1, i.customCoord2);
                NovaModulateAlphaTransitionProgress(alphaTransitionProgress, i.color.a);
                color.a *= NovaAlphaTransitionFactor(NOVA_SAMPLE_TEX2D(_AlphaTransitionMap, NovaTransformUV(i.uv, _AlphaTransitionMap_ST)), _AlphaTransitionMapChannelsX, alphaTransitionProgress, _DissolveSharpness);
                #endif
                #if defined(_VERTEX_ALPHA_AS_TRANSITION_PROGRESS)
                color.rgb *= i.color.rgb;
                #else
                color *= i.color;
                #endif
                #if defined(_TRANSPARENCY_BY_LUMINANCE)
                NovaApplyLuminanceTransparency(color, _LuminanceTransparencyProgress + NovaGetCustomCoord(_LuminanceTransparencyProgressCoord, i.customCoord1, i.customCoord2), _LuminanceTransparencySharpness + NovaGetCustomCoord(_LuminanceTransparencySharpnessCoord, i.customCoord1, i.customCoord2), _InverseLuminanceTransparency);
                #endif
                return color;
            }

            fixed4 fragDepthOnly(NovaDepthVaryings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                #ifdef _ALPHATEST_ENABLED
                clip(NovaDepthAlphaColor(i).a - _Cutoff);
                #endif
                return 0;
            }
            ENDCG
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            CGPROGRAM
            #pragma vertex vertShadowCaster
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:vertInstancingSetup
            #pragma fragment fragShadowCaster
            #pragma target 3.5
            #pragma require 2darray
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma shader_feature_local_fragment _VERTEX_ALPHA_AS_TRANSITION_PROGRESS
            #pragma shader_feature_local _BASE_MAP_MODE_2D _BASE_MAP_MODE_2D_ARRAY _BASE_MAP_MODE_3D
            #pragma shader_feature_local_vertex _BASE_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local_fragment _BASE_MAP_CHANNEL_ENABLED
            #pragma shader_feature_local_fragment _ _BASE_SAMPLER_STATE_POINT_MIRROR _BASE_SAMPLER_STATE_LINEAR_MIRROR _BASE_SAMPLER_STATE_TRILINEAR_MIRROR
            #pragma shader_feature_local _ _TINT_AREA_ALL
            #pragma shader_feature_local _ _TINT_COLOR_ENABLED _TINT_MAP_ENABLED _TINT_MAP_3D_ENABLED _TINT_MAP_MODE_2D_ARRAY
            #pragma shader_feature_local_vertex _TINT_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local _FLOW_MAP_TARGET_BASE
            #pragma shader_feature_local _FLOW_MAP_TARGET_TINT
            #pragma shader_feature_local _FLOW_MAP_TARGET_ALPHA_TRANSITION
            #pragma shader_feature_local _ _FADE_TRANSITION_ENABLED _DISSOLVE_TRANSITION_ENABLED
            #pragma shader_feature_local _ALPHA_TRANSITION_MAP_MODE_2D _ALPHA_TRANSITION_MAP_MODE_2D_ARRAY _ALPHA_TRANSITION_MAP_MODE_3D
            #pragma shader_feature_local _ _ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE _ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY
            #pragma shader_feature_local _ _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_3D
            #pragma shader_feature_local_vertex _ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local_vertex _ALPHA_TRANSITION_MAP_SECOND_TEXTURE_RANDOM_ROW_SELECTION_ENABLED
            #pragma shader_feature_local _TRANSPARENCY_BY_LUMINANCE
            #pragma shader_feature_local_vertex _ _VERTEX_DEFORMATION_ENABLED
            #pragma shader_feature_local _SHADOW_CASTER_ALPHA_TEST_ENABLED
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _NORMAL_MAP_ENABLED
            #pragma shader_feature_local_fragment _METALLIC_MAP_ENABLED
            #pragma shader_feature_local_fragment _SMOOTHNESS_MAP_ENABLED
            #include "BuiltIn/NovaBuiltInCore.cginc"
            #include "BuiltIn/NovaBuiltInTexture.cginc"
            #include "BuiltIn/NovaBuiltInShadow.cginc"
            #include "BuiltIn/NovaBuiltInUberUnlit.cginc"
            #include "BuiltIn/NovaBuiltInBaseUV.cginc"

            NOVA_DECLARE_TEX2D(_BaseMap);
            NOVA_DECLARE_TEX2D_ARRAY(_BaseMap2DArray);
            float4 _BaseMap2DArray_ST;
            NOVA_DECLARE_TEX3D(_BaseMap3D);
            NOVA_DECLARE_TEX2D(_FlowMap);
            NOVA_DECLARE_TEX2D(_AlphaTransitionMap);
            NOVA_DECLARE_TEX2D_ARRAY(_AlphaTransitionMap2DArray);
            float4 _AlphaTransitionMap2DArray_ST;
            NOVA_DECLARE_TEX3D(_AlphaTransitionMap3D);
            NOVA_DECLARE_TEX2D(_VertexDeformationMap);

            fixed4 _TintColor;
            half _BaseMapProgress;
            half _BaseMapProgressCoord;
            half _BaseMapRandomRowCoord;
            half _BaseMapSliceCount;
            half _BaseMapRowCount;
            half _FlowMapChannelsX;
            half _FlowMapChannelsY;
            half _FlowIntensity;
            half _FlowIntensityCoord;
            half _AlphaTransitionMapChannelsX;
            half _AlphaTransitionProgress;
            half _AlphaTransitionProgressCoord;
            half _DissolveSharpness;
            half _LuminanceTransparencyProgress;
            half _LuminanceTransparencyProgressCoord;
            half _LuminanceTransparencySharpness;
            half _LuminanceTransparencySharpnessCoord;
            half _InverseLuminanceTransparency;
            half _VertexDeformationMapOffsetXCoord;
            half _VertexDeformationMapOffsetYCoord;
            half _VertexDeformationMapChannel;
            float _VertexDeformationIntensity;
            half _VertexDeformationIntensityCoord;
            float _VertexDeformationBaseValue;
            half _TintBlendRate;
            half _TintBlendRateCoord;
            half _ShadowCasterApplyVertexDeformation;
            half _ShadowCasterAlphaCutoff;
            half _ShadowCasterAlphaAffectedByTintColor;
            half _ShadowCasterAlphaAffectedByFlowMap;
            half _ShadowCasterAlphaAffectedByAlphaTransitionMap;
            half _ShadowCasterAlphaAffectedByTransparencyLuminance;

            struct NovaShadowAttributes
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 customCoord1 : TEXCOORD1;
                float4 customCoord2 : TEXCOORD2;
                fixed4 color : COLOR;
            };

            struct NovaShadowVaryings
            {
                UNITY_VERTEX_OUTPUT_STEREO
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
                float4 customCoord1 : TEXCOORD2;
                float4 customCoord2 : TEXCOORD3;
                half baseProgress : TEXCOORD4;
                fixed4 color : COLOR;
            };

            void NovaShadowApplyVertexDeformation(inout float4 vertex, float3 normal, float2 uv, float4 customCoord1, float4 customCoord2)
            {
                #if defined(_VERTEX_DEFORMATION_ENABLED)
                if (_ShadowCasterApplyVertexDeformation >= 0.5h)
                {
                    float2 vertexDeformationUv = NovaTransformUV(uv, _VertexDeformationMap_ST);
                    vertexDeformationUv.x += NovaGetCustomCoord(_VertexDeformationMapOffsetXCoord, customCoord1, customCoord2);
                    vertexDeformationUv.y += NovaGetCustomCoord(_VertexDeformationMapOffsetYCoord, customCoord1, customCoord2);
                    half4 vertexDeformation = NOVA_SAMPLE_TEX2D_LOD(_VertexDeformationMap, vertexDeformationUv, 0.0);
                    float vertexDeformationIntensity = _VertexDeformationIntensity + NovaGetCustomCoord(_VertexDeformationIntensityCoord, customCoord1, customCoord2);
                    float vertexDeformationAmount = (NovaSelectChannel(vertexDeformation, _VertexDeformationMapChannel) - _VertexDeformationBaseValue) * vertexDeformationIntensity;
                    vertex.xyz += normalize(normal) * vertexDeformationAmount;
                }
                #endif
            }

            NovaShadowVaryings vertShadowCaster(NovaShadowAttributes v)
            {
                NOVA_SETUP_PARTICLE(v)
                NovaShadowVaryings o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                NovaShadowApplyVertexDeformation(v.vertex, v.normal, v.uv, v.customCoord1, v.customCoord2);
                o.pos = NovaShadowCasterClipPos(v.vertex, v.normal);
                #if defined(_BASE_MAP_MODE_2D_ARRAY)
                o.uv = NovaBaseUV(v.uv, _BaseMap2DArray_ST, v.customCoord1, v.customCoord2);
                #elif defined(_BASE_MAP_MODE_3D)
                o.uv = NovaBaseUV(v.uv, _BaseMap3D_ST, v.customCoord1, v.customCoord2);
                #else
                o.uv = NovaBaseUV(v.uv, _BaseMap_ST, v.customCoord1, v.customCoord2);
                #endif
                o.baseProgress = NovaBaseMapProgress(
                    _BaseMapProgress + NovaGetCustomCoord(_BaseMapProgressCoord, v.customCoord1, v.customCoord2),
                    _BaseMapSliceCount, _BaseMapRowCount,
                    NovaGetCustomCoord(_BaseMapRandomRowCoord, v.customCoord1, v.customCoord2));
                o.customCoord1 = v.customCoord1;
                o.customCoord2 = v.customCoord2;
                o.color = v.color;
                return o;
            }

            half4 NovaShadowSampleBaseMap(float2 uv, half progress)
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

            half4 NovaShadowAlphaColor(NovaShadowVaryings i)
            {
                float2 baseUv = i.uv;
                #if defined(_FLOW_MAP_TARGET_BASE)
                if (_ShadowCasterAlphaAffectedByFlowMap >= 0.5h)
                {
                    half flowIntensity = _FlowIntensity + NovaGetCustomCoord(_FlowIntensityCoord, i.customCoord1, i.customCoord2);
                    baseUv += NovaFlowMapUvOffset(NOVA_SAMPLE_TEX2D(_FlowMap, NovaTransformUV(i.uv, _FlowMap_ST)), _FlowMapChannelsX, _FlowMapChannelsY, flowIntensity);
                }
                #endif

                half4 color = NovaShadowSampleBaseMap(baseUv, i.baseProgress);
                #if defined(_TINT_AREA_ALL) && defined(_TINT_COLOR_ENABLED)
                if (_ShadowCasterAlphaAffectedByTintColor >= 0.5h)
                {
                    half tintBlendRate = _TintBlendRate + NovaGetCustomCoord(_TintBlendRateCoord, i.customCoord1, i.customCoord2);
                    NovaApplyTintColor(color, _TintColor, tintBlendRate);
                }
                #endif
                #if defined(_FADE_TRANSITION_ENABLED) || defined(_DISSOLVE_TRANSITION_ENABLED)
                if (_ShadowCasterAlphaAffectedByAlphaTransitionMap >= 0.5h)
                {
                    half alphaTransitionProgress = _AlphaTransitionProgress + NovaGetCustomCoord(_AlphaTransitionProgressCoord, i.customCoord1, i.customCoord2);
                    NovaModulateAlphaTransitionProgress(alphaTransitionProgress, i.color.a);
                    color.a *= NovaAlphaTransitionFactor(NOVA_SAMPLE_TEX2D(_AlphaTransitionMap, NovaTransformUV(i.uv, _AlphaTransitionMap_ST)), _AlphaTransitionMapChannelsX, alphaTransitionProgress, _DissolveSharpness);
                }
                #endif
                #if defined(_VERTEX_ALPHA_AS_TRANSITION_PROGRESS)
                color.rgb *= i.color.rgb;
                #else
                color *= i.color;
                #endif
                #if defined(_TRANSPARENCY_BY_LUMINANCE)
                if (_ShadowCasterAlphaAffectedByTransparencyLuminance >= 0.5h)
                {
                    NovaApplyLuminanceTransparency(color, _LuminanceTransparencyProgress + NovaGetCustomCoord(_LuminanceTransparencyProgressCoord, i.customCoord1, i.customCoord2), _LuminanceTransparencySharpness + NovaGetCustomCoord(_LuminanceTransparencySharpnessCoord, i.customCoord1, i.customCoord2), _InverseLuminanceTransparency);
                }
                #endif
                return color;
            }

            fixed4 fragShadowCaster(NovaShadowVaryings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                #ifdef _SHADOW_CASTER_ALPHA_TEST_ENABLED
                clip(NovaShadowAlphaColor(i).a - _ShadowCasterAlphaCutoff);
                #endif
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    Fallback Off
    CustomEditor "Nova.Editor.Core.Scripts.ParticlesUberLitGUI"
}

Shader "Nova/Particles/Distortion"
{
    Properties
        {
            // Render Settings
            _Cull("Cull", Float) = 2.0
            _ZTest("ZTest", Float) = 4.0
    
            // Distortion
            _BaseMap("Base Map", 2D) = "grey" {}
            _BaseMapOffsetXCoord("Base Map Offset X Coord", Float) = 0.0
            _BaseMapOffsetYCoord("Base Map Offset Y Coord", Float) = 0.0
            _BaseMapChannelsX("Base Map Channels X", Float) = 0.0
            _BaseMapChannelsY("Base Map Channels Y", Float) = 1.0
            _BaseMapRotation("Base Map Rotation", Range(0.0, 1.0)) = 0.0
            _BaseMapRotationCoord("Base Map Rotation Coord", Float) = 0.0
            _BaseMapRotationOffsets("Base Map Rotation Offsets", Vector) = (0.0, 0.0, 0.0, 0.0)
            _BaseMapMirrorSampling("Base Map Mirror Sampling", Float) = 0
            _BaseMapUnpackNormal("Base Map Unpack Normal", Float) = 0
            _DistortionIntensityMode("Distortion Intensity Mode", Float) = 0.0
            [PowerSlider(3)]_DistortionIntensity("Distortion Intensity", Range(0, 1)) = 0.1
            _DistortionIntensityCoord("Distortion Intensity Coord", Float) = 0.0
            [PowerSlider(3)]_DistortionIntensityX("Distortion Intensity X", Range(0, 1)) = 0.1
            _DistortionIntensityXCoord("Distortion Intensity X Coord", Float) = 0.0
            [PowerSlider(3)]_DistortionIntensityY("Distortion Intensity Y", Range(0, 1)) = 0.1
            _DistortionIntensityYCoord("Distortion Intensity Y Coord", Float) = 0.0
    
            // Flow Map
            _FlowMap("Flow Map", 2D) = "grey" {}
            _FlowMapOffsetXCoord("Flow Map Offset X Coord", Float) = 0.0
            _FlowMapOffsetYCoord("Flow Map Offset Y Coord", Float) = 0.0
            _FlowMapChannelsX("Flow Map Channels X", Float) = 0.0
            _FlowMapChannelsY("Flow Map Channels Y", Float) = 1.0
            _FlowIntensity("Flow Intensity", Float) = 1.0
            _FlowIntensityCoord("Flow Intensity Coord", Float) = 0.0
            _FlowMapTarget("Flow Map Target", Float) = 1.0
            _FlowMapRotation("Flow Map Rotation", Range(0.0, 1.0)) = 0.0
            _FlowMapRotationCoord("Flow Map Rotation Coord", Float) = 0.0
            _FlowMapRotationOffsets("Flow Map Rotation Offsets", Vector) = (0.0, 0.0, 0.0, 0.0)
    
            // Alpha Transition
            _AlphaTransitionMode("Alpha Transition Mode", Float) = 0.0
            _AlphaTransitionMap("Alpha Transition Map", 2D) = "white" {}
            _AlphaTransitionMapOffsetXCoord("Alpha Transition Map Offset X Coord", Float) = 0.0
            _AlphaTransitionMapOffsetYCoord("Alpha Transition Map Offset Y Coord", Float) = 0.0
            _AlphaTransitionMapChannelsX("Alpha Transition Map Channels X", Float) = 0.0
            _AlphaTransitionProgress("Alpha Transition Progress", Range(0.0, 1.0)) = 0.0
            _AlphaTransitionProgressCoord("Alpha Transition Progress Coord", Float) = 0.0
            _DissolveSharpness("Dissolve Sharpness", Range(0.0, 1.0)) = 0.5
            _AlphaTransitionMapRotation("Alpha Transition Map Rotation", Range(0.0, 1.0)) = 0.0
            _AlphaTransitionMapRotationCoord("Alpha Transition Map Rotation Coord", Float) = 0.0
            _AlphaTransitionMapRotationOffsets("Alpha Transition Map Rotation Offsets", Vector) = (0.0, 0.0, 0.0, 0.0)
    
            // Transparency
            _SoftParticlesEnabled("Soft Particles Enabled", Float) = 0.0
            _SoftParticlesIntensity("Soft Particles Intensity", Float) = 1.0
            _DepthFadeEnabled("Depth Fade Enabled", Float) = 0.0
            _DepthFadeNear("Depth Fade Near", Float) = 1.0
            _DepthFadeFar("Depth Fade Far", Float) = 2.0
            _DepthFadeWidth("Depth Fade Width", Float) = 1.0
        }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _ _BASE_SAMPLER_STATE_LINEAR_MIRROR _BASE_SAMPLER_STATE_POINT_MIRROR _BASE_SAMPLER_STATE_TRILINEAR_MIRROR _DEPTH_FADE_ENABLED _DISSOLVE_TRANSITION_ENABLED _FADE_TRANSITION_ENABLED _FLOW_MAP_ENABLED _FLOW_MAP_TARGET_ALPHA_TRANSITION _FLOW_MAP_TARGET_BASE _SOFT_PARTICLES_ENABLED
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
    Fallback Off
    CustomEditor "Nova.Editor.Core.Scripts.ParticlesDistortionGUI"
}

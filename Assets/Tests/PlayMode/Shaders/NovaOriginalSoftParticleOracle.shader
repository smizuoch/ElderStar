// Oracle: NOVA 3.6.0 Particles.hlsl SoftParticle, commit 1c6b887.
// Original math Copyright 2025 CyberAgent, Inc., MIT (see Third Party Notices).
Shader "Hidden/NovaBuiltInTests/OriginalSoftParticleOracle"
{
    Properties { _Intensity("Intensity", Float) = 0.5 }
    SubShader { Pass {
        Cull Off ZWrite Off
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 3.5
        #include "UnityCG.cginc"
        half _Intensity;
        sampler2D _CameraDepthTexture;
        struct v2f { float4 pos : SV_POSITION; float4 projection : TEXCOORD0; };
        v2f vert(float4 vertex : POSITION)
        {
            v2f o; o.pos = UnityObjectToClipPos(vertex);
            o.projection = ComputeScreenPos(o.pos); return o;
        }
        float4 frag(v2f i) : SV_Target
        {
            float depth = LinearEyeDepth(i.projection.z / i.projection.w);
            float sceneDepth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.projection.xy / i.projection.w).r);
            half inverseIntensity = 1.0 / _Intensity;
            float factor = saturate((sceneDepth - depth) * inverseIntensity);
            return float4(factor, factor, factor, 1);
        }
        ENDCG
    } }
}

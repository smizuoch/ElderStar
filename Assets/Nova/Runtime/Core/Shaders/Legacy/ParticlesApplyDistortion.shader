Shader "Hidden/Nova/LegacyBuiltIn/ParticlesApplyDistortion"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        UsePass "Hidden/Nova/Particles/ApplyDistortion/APPLYDISTORTION"
    }
    Fallback Off
}

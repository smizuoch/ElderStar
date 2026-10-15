Shader "Hidden/NovaTests/OriginalApiLightMode"
{
    SubShader
    {
        Pass
        {
            Name "DifferentPassName"
            Tags { "LightMode" = "NovaCompatibilityOffsets" }
            Cull Off ZWrite Off ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 vert(float4 vertex : POSITION) : SV_POSITION { return UnityObjectToClipPos(vertex); }
            float4 frag() : SV_Target { return float4(.7, .5, 0, 1); }
            ENDCG
        }
    }
}

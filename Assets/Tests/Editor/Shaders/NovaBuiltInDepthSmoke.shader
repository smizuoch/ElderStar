Shader "Hidden/NovaBuiltIn/Tests/DepthSmoke"
{
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInCore.cginc"
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInDepth.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPosition : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = NovaObjectToClipPos(v.vertex);
                o.screenPosition = NovaComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = NovaScreenUV(i.screenPosition);
                float rawDepth = NovaSampleRawCameraDepth(uv);
                float eyeDepth = NovaLinearEyeDepth(rawDepth);
                float soft = NovaSoftParticleFactor(i.screenPosition, 1.0);
                float fade = NovaDepthFadeFactor(0.1, max(eyeDepth, 0.2), 1.0, i.screenPosition);
                return fixed4(soft, fade, saturate(eyeDepth * 0.01), 1);
            }
            ENDCG
        }
    }
    Fallback Off
}

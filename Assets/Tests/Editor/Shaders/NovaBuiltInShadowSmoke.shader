Shader "Hidden/NovaBuiltIn/Tests/ShadowSmoke"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInShadow.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = NovaShadowCasterClipPos(v.vertex, v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return NovaShadowCasterFragment();
            }
            ENDCG
        }
    }
    Fallback Off
}

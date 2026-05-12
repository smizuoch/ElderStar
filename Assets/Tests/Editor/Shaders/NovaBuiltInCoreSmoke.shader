Shader "Hidden/NovaBuiltIn/Tests/CoreSmoke"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInCore.cginc"
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInFog.cginc"
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInInstancing.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                NOVA_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                NOVA_FOG_COORDS(2)
                NOVA_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                v2f o;
                NOVA_SETUP_INSTANCE_ID(v);
                NOVA_TRANSFER_INSTANCE_ID(v, o);
                float3 positionWS = NovaObjectToWorld(v.vertex.xyz);
                float3 normalWS = NovaObjectToWorldNormal(v.normal);
                float3 tangentWS = NovaObjectToWorldTangent(v.tangent.xyz);
                o.vertex = NovaWorldToClipPos(positionWS);
                o.normalWS = NovaCreateBitangent(normalWS, tangentWS, v.tangent.w);
                o.viewDirWS = NovaWorldSpaceViewDir(positionWS);
                NOVA_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = fixed4(NovaSafeNormalize(i.normalWS) * 0.5 + 0.5, 1);
                NOVA_APPLY_FOG(i.fogCoord, color);
                return color;
            }
            ENDCG
        }
    }
    Fallback Off
}

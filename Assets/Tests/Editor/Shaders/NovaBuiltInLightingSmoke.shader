Shader "Hidden/NovaBuiltIn/Tests/LightingSmoke"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInCore.cginc"
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInLighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = NovaObjectToClipPos(v.vertex);
                o.normalWS = NovaObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                NovaBuiltInMainLight mainLight = NovaGetMainLight(i.normalWS);
                NovaBuiltInSurfaceData surfaceData;
                surfaceData.albedo = half3(0.5, 0.5, 0.5);
                surfaceData.specular = half3(0.04, 0.04, 0.04);
                surfaceData.metallic = 0;
                surfaceData.smoothness = 0.5;
                surfaceData.alpha = 1;
                half3 ambient = NovaSampleAmbient(i.normalWS);
                half ndotl = saturate(dot(NovaSafeNormalize(i.normalWS), mainLight.direction));
                return fixed4(surfaceData.albedo * (ambient + mainLight.color * ndotl), surfaceData.alpha);
            }
            ENDCG
        }
    }
    Fallback Off
}

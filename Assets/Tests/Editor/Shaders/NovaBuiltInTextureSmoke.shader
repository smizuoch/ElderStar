Shader "Hidden/NovaBuiltIn/Tests/TextureSmoke"
{
    Properties
    {
        _MainTex("Main Tex", 2D) = "white" {}
        _ArrayTex("Array Tex", 2DArray) = "" {}
        _VolumeTex("Volume Tex", 3D) = "" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma target 3.5
            #pragma require 2darray
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInCore.cginc"
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInTexture.cginc"

            NOVA_DECLARE_TEX2D(_MainTex);
            NOVA_DECLARE_TEX2D_ARRAY(_ArrayTex);
            NOVA_DECLARE_TEX3D(_VolumeTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = NovaObjectToClipPos(v.vertex);
                o.uv = NovaTransformUV(v.uv, _MainTex_ST);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 tex2dColor = NOVA_SAMPLE_TEX2D(_MainTex, NovaMirrorUV(i.uv));
                half4 texArrayColor = NOVA_SAMPLE_TEX2D_ARRAY(_ArrayTex, i.uv, 0);
                half4 tex3dColor = NOVA_SAMPLE_TEX3D(_VolumeTex, float3(i.uv, 0.5));
                half channel = NovaSelectChannel(tex2dColor + texArrayColor + tex3dColor, 0);
                return fixed4(channel.xxx, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}

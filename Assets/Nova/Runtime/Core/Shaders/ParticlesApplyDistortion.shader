Shader "Hidden/Nova/Particles/ApplyDistortion"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            Name "ApplyDistortion"
            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #include "BuiltIn/NovaBuiltInCore.cginc"
            UNITY_DECLARE_TEX2D(_MainTex);
            float _NovaDistortionForceAniso;
            #if defined(UNITY_SEPARATE_TEXTURE_SAMPLER)
            // URP's forced anisotropy is 9. Unity inline samplers support only
            // 2/4/8/16; 8 is the nearest supported approximation (see matrix).
            SamplerState nova_linear_clamp_aniso8_sampler;
            #endif
            sampler2D _ScreenSpaceUvTexture;
            v2f_img vert(appdata_img v)
            {
                v2f_img o;
                o.pos = NovaObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            half4 frag(v2f_img i) : SV_Target
            {
                float2 offsetUV = i.uv;
                // The command-buffer target uses a GPU render-texture projection;
                // Built-in's image-effect source uses blit UVs on top-origin APIs.
                #if UNITY_UV_STARTS_AT_TOP
                offsetUV.y = 1.0 - offsetUV.y;
                #endif
                float2 offset = tex2D(_ScreenSpaceUvTexture, offsetUV).rg * 2 - 1;
                float2 sourceUV = saturate(i.uv + offset);
                #if defined(UNITY_SEPARATE_TEXTURE_SAMPLER)
                if (_NovaDistortionForceAniso > 0.5)
                    return _MainTex.Sample(nova_linear_clamp_aniso8_sampler, sourceUV);
                #endif
                return UNITY_SAMPLE_TEX2D(_MainTex, sourceUV);
            }
            ENDCG
        }
    }
    Fallback Off
}

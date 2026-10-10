Shader "Hidden/Nova/Particles/ApplyDistortion"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "BuiltIn/NovaBuiltInCore.cginc"
            sampler2D _MainTex;
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
                return tex2D(_MainTex, saturate(i.uv + offset));
            }
            ENDCG
        }
    }
    Fallback Off
}

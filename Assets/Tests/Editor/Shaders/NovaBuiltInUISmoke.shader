Shader "Hidden/NovaBuiltIn/Tests/UISmoke"
{
    Properties
    {
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInCore.cginc"
            #include "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 positionOS : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = NovaObjectToClipPos(v.vertex);
                o.positionOS = v.vertex.xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1, 1, 1, NovaUIClipAlpha(i.positionOS));
            }
            ENDCG
        }
    }
    Fallback Off
}

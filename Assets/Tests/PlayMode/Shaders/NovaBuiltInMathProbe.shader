Shader "Hidden/NovaBuiltInTests/MathProbe"
{
    Properties { _Input("Input", Vector) = (0,0,0,0) _Mode("Mode", Float) = 0 }
    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"
            #include "../../../Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInTexture.cginc"
            #include "../../../Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInUberUnlit.cginc"
            #include "../../../Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInDepth.cginc"
            float4 _Input;
            float _Mode;
            float4 frag(v2f_img i) : SV_Target
            {
                if (_Mode < 0.5)
                    return float4(NovaFlipBookBlendingProgressWithRandomRow(_Input.x,_Input.y,_Input.z,_Input.w),0,0,1);
                if (_Mode < 1.5) return float4(NovaParallaxOffset(0.8,0.6,_Input.xyz),0,1);
                return float4(NovaSampleSceneDepth01(_Input.xy),0,0,1);
            }
            ENDCG
        }
    }
}

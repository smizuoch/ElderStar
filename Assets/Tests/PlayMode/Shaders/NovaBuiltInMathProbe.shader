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
                if (_Mode < 2.5) return float4(NovaSampleSceneDepth01(_Input.xy),0,0,1);
                if (_Mode > 3.5) return float4(NovaDepthFadeFactor(2,4,1,_Input),0,0,1);
                // Independent original ParticlesUber.hlsl GetRimValue oracle.
                // Copyright 2025 CyberAgent, Inc. (MIT), pinned NOVA 3.6.0.
                half rim = _Input.x, progress = _Input.y, sharpness = _Input.z;
                if (_Input.w >= 0.5) rim = 1.0 - rim;
                progress = min(1.0, progress);
                half width = 1.0 - min(1.0, sharpness);
                half start = lerp(-width, 1.0, progress);
                half end = lerp(0.0, 1.0 + width, progress);
                half original = smoothstep(start, end, rim);
                return float4(NovaRimValue(_Input.x, _Input.y, _Input.z, _Input.w), original, 0, 1);
            }
            ENDCG
        }
    }
}

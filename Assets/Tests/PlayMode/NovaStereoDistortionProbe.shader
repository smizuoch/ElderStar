// Bind an explicit two-eye capture for a headset-free GPU test of the production pass.
// A real camera's GrabPass is covered separately by the camera rendering tests.
Shader "Hidden/NovaTests/StereoDistortionProbe"
{
    Properties
    {
        _Cull("Cull", Float) = 0
        _ZTest("Depth test", Float) = 8
        _BaseMap("Offset", 2D) = "grey" {}
        _DistortionIntensity("Intensity", Float) = 0
        _NovaDistortionGrabTexture("Two eye capture", 2DArray) = "" {}
    }
    SubShader { UsePass "Nova/Particles/Distortion/AVATARDISTORTION" }
}

#ifndef NOVA_BUILTIN_DISTORTION_EDITOR_INCLUDED
#define NOVA_BUILTIN_DISTORTION_EDITOR_INCLUDED
#include "NovaBuiltInCore.cginc"

float _ObjectId, _PassValue;
float4 _SelectionID;
struct NovaDistortionEditorAttributes
{
    float4 vertex : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct NovaDistortionEditorVaryings
{
    float4 vertex : SV_POSITION;
};
NovaDistortionEditorVaryings vertEditor(NovaDistortionEditorAttributes v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    NovaDistortionEditorVaryings o;
    o.vertex = NovaObjectToClipPos(v.vertex);
    return o;
}
half4 fragSceneHighlight(NovaDistortionEditorVaryings i) : SV_Target
{
    return half4(_ObjectId, _PassValue, 1, 1);
}
half4 fragScenePicking(NovaDistortionEditorVaryings i) : SV_Target
{
    return _SelectionID;
}
#endif

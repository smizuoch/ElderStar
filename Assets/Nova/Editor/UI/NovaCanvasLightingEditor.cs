using UnityEditor;
using Nova.Runtime.UI;

namespace Nova.Editor.UI
{
    [CustomEditor(typeof(NovaCanvasLighting)), CanEditMultipleObjects]
    internal sealed class NovaCanvasLightingEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            foreach (var selected in targets)
            {
                var lighting = (NovaCanvasLighting)selected;
                EditorGUILayout.LabelField(lighting.name + " — Omitted lights", lighting.OmittedLightCount.ToString());
                if (lighting.OmittedLightCount > 0)
                    EditorGUILayout.HelpBox("The light limit omits some contributing lights. Increase Maximum Lights (up to 8), or narrow the light list.", MessageType.Info);
            }
            EditorGUILayout.HelpBox("Optional Unity Canvas lighting. Direct lighting comes from the selected list; additional light shadows and cookies are not sampled. Do not attach this component to VRChat avatars.", MessageType.Info);
            if (UnityEngine.GUILayout.Button("Collect Scene Lights"))
                foreach (var selected in targets)
                {
                    Undo.RecordObject(selected, "Collect NOVA Canvas lights");
                    ((NovaCanvasLighting)selected).CollectSceneLights(); EditorUtility.SetDirty(selected);
                }
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Editor.Core.Scripts
{
    public static class BuiltInLightingSettings
    {
        private const string Menu = "Tools/NOVA/Built-in/Match Original Lighting Settings";

        [MenuItem(Menu)]
        private static void ShowConfirmation()
        {
            if (EditorUtility.DisplayDialog("Match original NOVA lighting",
                "Use URP's light intensity and color temperature settings in this Built-in project? " +
                "This changes lighting for the entire project. Existing Built-in lights may need adjustment. " +
                "You can undo the change with Edit > Undo.", "Apply", "Cancel")) Apply();
        }

        [MenuItem(Menu, true)]
        private static bool IsBuiltIn() => GraphicsSettings.currentRenderPipeline == null;

        /// <summary>Persist the original URP light settings with editor Undo support.</summary>
        public static void Apply()
        {
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
            var settings = new SerializedObject(asset);
            Undo.SetCurrentGroupName("Match original NOVA lighting");
            settings.FindProperty("m_LightsUseLinearIntensity").boolValue = QualitySettings.activeColorSpace == ColorSpace.Linear;
            settings.FindProperty("m_LightsUseColorTemperature").boolValue = true;
            settings.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}

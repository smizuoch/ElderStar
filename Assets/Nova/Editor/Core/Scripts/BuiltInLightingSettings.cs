using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Editor.Core.Scripts
{
    public static class BuiltInLightingSettings
    {
        private const string Menu = "Tools/ElderStar/Built-in/Match Original Lighting Settings";

        private const string AutomaticMenu = "Tools/ElderStar/Built-in/Automatic Initial Lighting Setup";

        [InitializeOnLoadMethod]
        private static void ScheduleInitialSetup()
        {
            EditorApplication.delayCall += InitializeProject;
        }

        // Match the settings installed by the original pipeline, once per project/color space.
        // Persisting them in GraphicsSettings also makes standalone players inherit the same values.
        private static void InitializeProject()
        {
            if (!IsBuiltIn()) return;
            var state = BuiltInProjectSettings.instance;
            var colorSpace = (int)PlayerSettings.colorSpace;
            if (!state.automaticLightingSetup || state.initializedColorSpace == colorSpace) return;
            Apply();
            state.initializedColorSpace = colorSpace;
            state.Persist();
        }

        [MenuItem(AutomaticMenu)]
        private static void ToggleAutomaticSetup()
        {
            var state = BuiltInProjectSettings.instance;
            state.automaticLightingSetup = !state.automaticLightingSetup;
            state.initializedColorSpace = -1;
            state.Persist();
            InitializeProject();
        }

        [MenuItem(AutomaticMenu, true)]
        private static bool ValidateAutomaticSetup()
        {
            UnityEditor.Menu.SetChecked(AutomaticMenu, BuiltInProjectSettings.instance.automaticLightingSetup);
            return IsBuiltIn();
        }

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
            settings.FindProperty("m_LightsUseLinearIntensity").boolValue = PlayerSettings.colorSpace == ColorSpace.Linear;
            settings.FindProperty("m_LightsUseColorTemperature").boolValue = true;
            settings.ApplyModifiedProperties();
            // The serialized asset may already match while native state was changed by a pipeline.
            GraphicsSettings.lightsUseLinearIntensity = PlayerSettings.colorSpace == ColorSpace.Linear;
            GraphicsSettings.lightsUseColorTemperature = true;
            AssetDatabase.SaveAssetIfDirty(asset);
        }
    }
}

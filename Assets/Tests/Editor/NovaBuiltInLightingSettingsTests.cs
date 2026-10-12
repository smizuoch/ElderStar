using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInLightingSettingsTests
    {
        [Test]
        public void LightingSetupPersistsOnlyGraphicsSettingsAndLeavesOtherEditsUnsaved()
        {
            var path = "Assets/NovaLightingSaveScope-" + Guid.NewGuid().ToString("N") + ".mat";
            var material = new Material(Shader.Find("Nova/Particles/UberUnlit"));
            var linear = GraphicsSettings.lightsUseLinearIntensity;
            var temperature = GraphicsSettings.lightsUseColorTemperature;
            try
            {
                AssetDatabase.CreateAsset(material, path);
                var original = System.IO.File.ReadAllBytes(path);
                material.SetFloat("_Cutoff", 0.123f);
                EditorUtility.SetDirty(material);
                GraphicsSettings.lightsUseColorTemperature = false;
                Nova.Editor.Core.Scripts.BuiltInLightingSettings.Apply();
                Assert.That(System.IO.File.ReadAllBytes(path), Is.EqualTo(original), "Initializing lighting must not save unrelated user edits.");
                Assert.That(EditorUtility.IsDirty(material), Is.True);
                var saved = System.IO.File.ReadAllText("ProjectSettings/GraphicsSettings.asset");
                Assert.That(saved, Does.Contain("m_LightsUseColorTemperature: 1"));
                Assert.That(saved, Does.Contain("m_LightsUseLinearIntensity: " + (PlayerSettings.colorSpace == ColorSpace.Linear ? "1" : "0")));
            }
            finally
            {
                AssetDatabase.DeleteAsset(path);
                GraphicsSettings.lightsUseLinearIntensity = linear;
                GraphicsSettings.lightsUseColorTemperature = temperature;
                var settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
                Undo.ClearUndo(settings);
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FirstProjectInitializationMatchesOriginalOnlyWhenEnabled(bool enabled)
        {
            var assembly = typeof(Nova.Editor.Core.Scripts.BuiltInLightingSettings).Assembly;
            var stateType = assembly.GetType("Nova.Editor.Core.Scripts.BuiltInProjectSettings");
            Assert.That(stateType, Is.Not.Null, "A fresh project must receive the original lighting defaults automatically.");
            var state = (ScriptableObject)stateType.BaseType.GetProperty("instance").GetValue(null);
            var snapshot = JsonUtility.ToJson(state);
            var linear = GraphicsSettings.lightsUseLinearIntensity;
            var temperature = GraphicsSettings.lightsUseColorTemperature;
            var expected = QualitySettings.activeColorSpace == ColorSpace.Linear;
            try
            {
                JsonUtility.FromJsonOverwrite("{\"automaticLightingSetup\":" + enabled.ToString().ToLowerInvariant() +
                    ",\"initializedColorSpace\":-1}", state);
                GraphicsSettings.lightsUseLinearIntensity = !expected;
                GraphicsSettings.lightsUseColorTemperature = false;
                var initialize = typeof(Nova.Editor.Core.Scripts.BuiltInLightingSettings).GetMethod("InitializeProject",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                Assert.That(initialize, Is.Not.Null);
                initialize.Invoke(null, null);
                Assert.That(GraphicsSettings.lightsUseLinearIntensity, Is.EqualTo(enabled ? expected : !expected));
                Assert.That(GraphicsSettings.lightsUseColorTemperature, Is.EqualTo(enabled));
                // Domain reloads must not repeatedly overwrite a user's later lighting edits or Undo.
                GraphicsSettings.lightsUseLinearIntensity = !expected;
                GraphicsSettings.lightsUseColorTemperature = false;
                initialize.Invoke(null, null);
                Assert.That(GraphicsSettings.lightsUseLinearIntensity, Is.EqualTo(!expected));
                Assert.That(GraphicsSettings.lightsUseColorTemperature, Is.False);
            }
            finally
            {
                JsonUtility.FromJsonOverwrite(snapshot, state);
                stateType.GetMethod("Persist").Invoke(state, null);
                GraphicsSettings.lightsUseLinearIntensity = linear;
                GraphicsSettings.lightsUseColorTemperature = temperature;
                var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
                Undo.ClearUndo(asset);
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }

        [Test]
        public void OriginalLightingSettingsPersistAndSupportUndo()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Nova.Editor.Core.Scripts.BuiltInLightingSettings")).FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, "Original light intensity settings must remain available with Undo support.");
            var settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0];
            var savedLinear = GraphicsSettings.lightsUseLinearIntensity;
            var savedTemperature = GraphicsSettings.lightsUseColorTemperature;
            var expected = QualitySettings.activeColorSpace == ColorSpace.Linear;
            try
            {
                GraphicsSettings.lightsUseLinearIntensity = !expected;
                GraphicsSettings.lightsUseColorTemperature = false;
                Undo.IncrementCurrentGroup();
                type.GetMethod("Apply").Invoke(null, null);
                Assert.That(GraphicsSettings.lightsUseLinearIntensity, Is.EqualTo(expected));
                Assert.That(GraphicsSettings.lightsUseColorTemperature, Is.True);
                var serialized = new SerializedObject(settings);
                Assert.That(serialized.FindProperty("m_LightsUseLinearIntensity").boolValue, Is.EqualTo(expected));
                Assert.That(serialized.FindProperty("m_LightsUseColorTemperature").boolValue, Is.True);
                Undo.PerformUndo();
                Assert.That(GraphicsSettings.lightsUseLinearIntensity, Is.EqualTo(!expected));
                Assert.That(GraphicsSettings.lightsUseColorTemperature, Is.False);
                Undo.PerformRedo();
                Assert.That(GraphicsSettings.lightsUseLinearIntensity, Is.EqualTo(expected));
                Assert.That(GraphicsSettings.lightsUseColorTemperature, Is.True);
            }
            finally
            {
                Undo.ClearUndo(settings);
                GraphicsSettings.lightsUseLinearIntensity = savedLinear;
                GraphicsSettings.lightsUseColorTemperature = savedTemperature;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

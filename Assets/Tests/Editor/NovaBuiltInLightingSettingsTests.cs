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
        public void OriginalLightingSettingsPersistAndSupportUndo()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Nova.Editor.Core.Scripts.BuiltInLightingSettings")).FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, "Linear projects need an explicit migration action for URP light intensity settings.");
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

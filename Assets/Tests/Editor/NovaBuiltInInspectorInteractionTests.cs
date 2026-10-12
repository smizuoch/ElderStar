using System;
using System.Collections.Generic;
using System.Linq;
using Nova.Editor.Core.Scripts;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInInspectorInteractionTests
    {
        [TestCase("ParticlesUberUnlitGUI", "Nova/Particles/UberUnlit")]
        [TestCase("ParticlesUberLitGUI", "Nova/Particles/UberLit")]
        [TestCase("UIParticlesUberUnlitGUI", "Nova/UIParticles/UberUnlit")]
        [TestCase("UIParticlesUberLitGUI", "Nova/UIParticles/UberLit")]
        [TestCase("ParticlesDistortionGUI", "Nova/Particles/Distortion")]
        public void OriginalInspectorDrawsAndMultiEditSupportsUndoRedo(string guiName, string shaderName)
        {
            var first = new Material(Shader.Find(shaderName));
            var second = new Material(first);
            var editor = (MaterialEditor)UnityEditor.Editor.CreateEditor(new Object[] { first, second });
            var gui = (ShaderGUI)Activator.CreateInstance(typeof(ParticlesGUI).Assembly.GetType("Nova.Editor.Core.Scripts." + guiName));
            var window = ScriptableObject.CreateInstance<NovaParityInspectorWindow>();
            var prefs = new Dictionary<string, (bool exists, bool value)>();
            var texture = new Texture2D(2, 2);
            var array = new Texture2DArray(2, 2, 4, TextureFormat.RGBA32, false);
            var volume = new Texture3D(2, 2, 4, TextureFormat.RGBA32, false);
            try
            {
                foreach (var name in new[] { "RenderSettings", "BaseMap", "TintColor", "FlowMap", "ParallaxMap", "ColorCorrection",
                    "Transparency", "AlphaTransition", "Emission", "VertexDeformation", "ShadowCaster" })
                {
                    var key = "Nova.Editor.Core.Scripts.ParticlesUberCommonMaterialProperties." + name + "Foldout";
                    prefs[key] = (EditorPrefs.HasKey(key), EditorPrefs.GetBool(key));
                    EditorPrefs.SetBool(key, true);
                }
                window.position = new Rect(10, 10, 600, 900);
                window.Draw = () => gui.OnGUI(editor, MaterialEditor.GetMaterialProperties(editor.targets));
                window.Show();
                window.DrawNow();
                Assert.That(window.DrawCount, Is.GreaterThan(0), "The real OnGUI path must execute.");
                Assert.That(window.Error, Is.Null, window.Error?.ToString());

                for (var mode = 0; mode < 3; mode++)
                {
                    foreach (var material in new[] { first, second })
                    {
                        for (var i = 0; i < material.shader.GetPropertyCount(); i++)
                        {
                            var name = material.shader.GetPropertyName(i);
                            if (material.shader.GetPropertyType(i) == ShaderPropertyType.Texture)
                            {
                                var dimension = material.shader.GetPropertyTextureDimension(i);
                                material.SetTexture(name, dimension == TextureDimension.Tex2DArray ? array :
                                    dimension == TextureDimension.Tex3D ? (Texture)volume : texture);
                            }
                            else if (name.EndsWith("Enabled")) material.SetFloat(name, 1);
                            else if (name.EndsWith("SliceCount")) material.SetFloat(name, 4);
                            else if (name.EndsWith("RowCount")) material.SetFloat(name, 2);
                        }
                        foreach (var name in new[] { "_BaseMapMode", "_BaseMapToneMode", "_ParallaxMapMode", "_ColorCorrectionMode",
                            "_AlphaTransitionMapMode", "_AlphaTransitionMapSecondTextureMapMode", "_EmissionMapMode" })
                            if (material.HasProperty(name)) material.SetFloat(name, mode);
                        foreach (var setting in new Dictionary<string, float> { { "_TintColorMode", mode + 1 }, { "_TintAreaMode", 2 },
                            { "_AlphaTransitionMode", 2 }, { "_AlphaTransitionSecondTextureBlendMode", 1 }, { "_EmissionAreaType", mode + 1 },
                            { "_EmissionColorType", 2 }, { "_FlowMapTarget", 15 }, { "_ParallaxMapTarget", 7 }, { "_LitWorkflowMode", mode % 2 } })
                            if (material.HasProperty(setting.Key)) material.SetFloat(setting.Key, setting.Value);
                        gui.ValidateMaterial(material);
                    }
                    window.DrawNow();
                    Assert.That(window.Error, Is.Null, window.Error?.ToString());
                }

                var propertyName = shaderName.EndsWith("Distortion") ? "_DistortionIntensity" : "_RenderType";
                first.SetFloat(propertyName, 0);
                second.SetFloat(propertyName, 1);
                var property = MaterialEditor.GetMaterialProperties(editor.targets).Single(p => p.name == propertyName);
                Assert.That(property.hasMixedValue, Is.True);
                Undo.IncrementCurrentGroup();
                editor.RegisterPropertyChangeUndo("NOVA parity multi-edit");
                property.floatValue = 2;
                foreach (var material in new[] { first, second }) gui.ValidateMaterial(material);
                Undo.FlushUndoRecordObjects();
                Assert.That(first.GetFloat(propertyName), Is.EqualTo(2));
                Assert.That(second.GetFloat(propertyName), Is.EqualTo(2));
                window.DrawNow();
                Undo.PerformUndo();
                Assert.That(first.GetFloat(propertyName), Is.EqualTo(0));
                Assert.That(second.GetFloat(propertyName), Is.EqualTo(1));
                window.DrawNow();
                Undo.PerformRedo();
                Assert.That(first.GetFloat(propertyName), Is.EqualTo(2));
                Assert.That(second.GetFloat(propertyName), Is.EqualTo(2));
                window.DrawNow();
                Assert.That(window.Error, Is.Null, window.Error?.ToString());
            }
            finally
            {
                window.Close();
                foreach (var pref in prefs)
                    if (pref.Value.exists) EditorPrefs.SetBool(pref.Key, pref.Value.value); else EditorPrefs.DeleteKey(pref.Key);
                Undo.ClearUndo(first); Undo.ClearUndo(second);
                Object.DestroyImmediate(editor); Object.DestroyImmediate(first); Object.DestroyImmediate(second);
                Object.DestroyImmediate(texture); Object.DestroyImmediate(array); Object.DestroyImmediate(volume);
            }
        }

        [Test]
        public void FoldoutPreferencesSurviveInspectorRecreation()
        {
            const string key = "Nova.Editor.Core.Scripts.ParticlesUberCommonMaterialProperties.RenderSettingsFoldout";
            var existed = EditorPrefs.HasKey(key);
            var saved = EditorPrefs.GetBool(key);
            var material = new Material(Shader.Find("Nova/Particles/UberUnlit"));
            var editor = (MaterialEditor)UnityEditor.Editor.CreateEditor(material);
            var gui = (ShaderGUI)Activator.CreateInstance(typeof(ParticlesGUI).Assembly.GetType("Nova.Editor.Core.Scripts.ParticlesUberUnlitGUI"));
            var window = ScriptableObject.CreateInstance<NovaParityInspectorWindow>();
            try
            {
                var prefType = typeof(ParticlesGUI).Assembly.GetType("Nova.Editor.Foundation.Scripts.BoolEditorPrefsProperty");
                var value = prefType.GetProperty("Value");
                var pref = Activator.CreateInstance(prefType, key, true);
                value.SetValue(pref, false);
                Assert.That(EditorPrefs.GetBool(key), Is.False);
                window.position = new Rect(10, 10, 600, 900);
                window.Draw = () => gui.OnGUI(editor, MaterialEditor.GetMaterialProperties(editor.targets));
                window.Show();
                window.DrawNow();
                Assert.That(window.Error, Is.Null, window.Error?.ToString());
                Assert.That(EditorPrefs.GetBool(key), Is.False);
                var recreated = Activator.CreateInstance(prefType, key, true);
                Assert.That(value.GetValue(recreated), Is.EqualTo(false));
                value.SetValue(recreated, true);
                Assert.That(EditorPrefs.GetBool(key), Is.True);
            }
            finally
            {
                window.Close();
                if (existed) EditorPrefs.SetBool(key, saved); else EditorPrefs.DeleteKey(key);
                Object.DestroyImmediate(editor); Object.DestroyImmediate(material);
            }
        }
    }

    public sealed class NovaParityInspectorWindow : EditorWindow
    {
        public Action Draw;
        public Exception Error;
        public int DrawCount;
        public void DrawNow()
        {
            SendEvent(new Event { type = EventType.Layout });
            SendEvent(new Event { type = EventType.Repaint });
        }
        private void OnGUI()
        {
            if (Draw == null || Error != null) return;
            try
            {
                GUILayout.BeginArea(new Rect(0, 0, 590, 20000));
                Draw();
                GUILayout.EndArea();
                DrawCount++;
            }
            catch (Exception e) { Error = e; }
        }
    }
}

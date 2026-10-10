using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInOriginalEditorApiTests
    {
        private static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("Nova.Editor." + name)).FirstOrDefault(t => t != null);

        [TestCase("CustomCoord")]
        [TestCase("UICustomCoord")]
        [TestCase("RenderFace")]
        [TestCase("ColorChannels")]
        [TestCase("BaseMapChannel")]
        [TestCase("DistortionIntensityMode")]
        [TestCase("EmissionColorTypeForAllArea")]
        [TestCase("ZTest")]
        public void OriginalPublicEnumsRemainAvailable(string name)
        {
            var type = Find("Core.Scripts." + name);
            Assert.That(type, Is.Not.Null, name);
            Assert.That(type.IsPublic && type.IsEnum, Is.True, name);
        }

        [Test]
        public void PublicPropertyConstantsAndKeywordExtensionsRemainAvailable()
        {
            var properties = Find("Core.Scripts.MaterialPropertyNames");
            Assert.That(properties, Is.Not.Null);
            var names = properties.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string)).Select(f => (string)f.GetRawConstantValue()).ToArray();
            var materials = new[] { "Nova/Particles/UberUnlit", "Nova/Particles/UberLit", "Nova/Particles/Distortion",
                "Nova/UIParticles/UberUnlit", "Nova/UIParticles/UberLit" }.Select(n => new Material(Shader.Find(n))).ToArray();
            try
            {
                Assert.That(names.Length, Is.GreaterThan(200));
                foreach (var name in names) Assert.That(materials.Any(m => m.HasProperty(name)), Is.True, name);
            }
            finally { foreach (var material in materials) UnityEngine.Object.DestroyImmediate(material); }
            var keywords = Find("Core.Scripts.ShaderKeywords");
            Assert.That(keywords.GetField("SpecularHightlightsEnabled"), Is.Not.Null, "Preserve the original public spelling.");
            var mode = Find("Core.Scripts.BaseMapMode");
            var method = keywords.GetMethod("GetShaderKeyword", new[] { mode });
            Assert.That(method, Is.Not.Null);
            Assert.That(method.Invoke(null, new[] { Enum.ToObject(mode, 1) }), Is.EqualTo("_BASE_MAP_MODE_2D_ARRAY"));
        }

        [TestCase("ParticlesUberUnlitGUI", "Nova/Particles/UberUnlit")]
        [TestCase("ParticlesUberLitGUI", "Nova/Particles/UberLit")]
        [TestCase("UIParticlesUberUnlitGUI", "Nova/UIParticles/UberUnlit")]
        [TestCase("UIParticlesUberLitGUI", "Nova/UIParticles/UberLit")]
        [TestCase("ParticlesDistortionGUI", "Nova/Particles/Distortion")]
        public void CustomInspectorsBindAllPropertiesAndProvideOriginalLayout(string guiName, string shaderName)
        {
            var type = Find("Core.Scripts." + guiName);
            var gui = (ShaderGUI)Activator.CreateInstance(type);
            Assert.That(type.GetMethod("OnGUI").DeclaringType, Is.Not.EqualTo(typeof(ShaderGUI)));
            var setup = type.GetMethod("SetupProperties", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(setup, Is.Not.Null);
            var material = new Material(Shader.Find(shaderName));
            var editor = (MaterialEditor)UnityEditor.Editor.CreateEditor(material);
            try
            {
                var properties = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { material });
                var initialize = type.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(initialize, Is.Not.Null);
                initialize.Invoke(gui, new object[] { editor, properties });
                Assert.DoesNotThrow(() => setup.Invoke(gui, new object[] { properties }));
                var wrapper = Find("Core.Scripts.ParticlesGUI+Property");
                Assert.That(wrapper, Is.Not.Null);
                var property = Activator.CreateInstance(wrapper, new object[] { "_BaseMap" });
                wrapper.GetMethod("Setup").Invoke(property, new object[] { properties });
                Assert.That(((MaterialProperty)wrapper.GetProperty("Value").GetValue(property)).name, Is.EqualTo("_BaseMap"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
                UnityEngine.Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void OriginalCleanupAndRendererValidationToolsRemainCallable()
        {
            var cleanup = Find("Foundation.Scripts.UnusedReferencesRemover");
            var validation = Find("Core.Scripts.RendererErrorHandler");
            Assert.That(cleanup, Is.Not.Null);
            Assert.That(validation, Is.Not.Null);
            Assert.That(cleanup.GetMethod("RemoveUnusedReferences", new[] { typeof(Material) }), Is.Not.Null);
            Assert.That(validation.GetMethod("CheckErrorWithMaterial"), Is.Not.Null);
            Assert.That(validation.GetMethod("FixErrorWithMaterial"), Is.Not.Null);
        }

        [TestCase("Nova/Particles/UberUnlit")]
        [TestCase("Nova/Particles/UberLit")]
        public void FixNowRepairsActualParticleVertexStreams(string shaderName)
        {
            var material = new Material(Shader.Find(shaderName));
            var obj = new GameObject("NOVA vertex stream verification", typeof(ParticleSystem));
            try
            {
                material.SetFloat("_AlphaTransitionProgressCoord", 1);
                material.SetFloat("_AlphaTransitionMode", 2);
                var renderer = obj.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = material;
                renderer.SetActiveVertexStreams(new System.Collections.Generic.List<ParticleSystemVertexStream>
                    { ParticleSystemVertexStream.Position });
                var type = Find("Core.Scripts.RendererErrorHandler");
                Assert.That(type.GetMethod("CheckErrorWithMaterial").Invoke(null, new object[] { material }), Is.EqualTo(true));
                type.GetMethod("FixErrorWithMaterial").Invoke(null, new object[] { material });
                Assert.That(type.GetMethod("CheckErrorWithMaterial").Invoke(null, new object[] { material }), Is.EqualTo(false));
                var streams = new System.Collections.Generic.List<ParticleSystemVertexStream>();
                renderer.GetActiveVertexStreams(streams);
                Assert.That(streams, Does.Contain(ParticleSystemVertexStream.Custom1XYZW));
                Assert.That(streams, Does.Contain(ParticleSystemVertexStream.Custom2XYZW));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(obj);
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }
}

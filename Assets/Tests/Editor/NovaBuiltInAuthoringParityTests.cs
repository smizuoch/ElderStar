using System.IO;
using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Nova.Editor.Core.Scripts;
using UnityEngine;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInAuthoringParityTests
    {
        [Test]
        public void OriginalShaderReassignmentPreservesCommonValuesAndAppliesRenderState(
            [Values("Particles/UberUnlit", "Particles/UberLit", "UIParticles/UberUnlit", "UIParticles/UberLit")] string from,
            [Values("Particles/UberUnlit", "Particles/UberLit", "UIParticles/UberUnlit", "UIParticles/UberLit")] string to)
        {
            var oldShader = Shader.Find("Nova/" + from);
            var newShader = Shader.Find("Nova/" + to);
            var material = new Material(oldShader);
            var texture = new Texture2D(2, 2);
            try
            {
                material.SetTexture("_BaseMap", texture);
                material.SetColor("_TintColor", new Color(0.2f, 0.6f, 0.8f, 0.5f));
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 1);
                material.SetFloat("_QueueOffset", 13);
                var guiType = typeof(ParticlesGUI).Assembly.GetType("Nova.Editor.Core.Scripts." + to.Replace("/", "") + "GUI");
                var gui = (ShaderGUI)Activator.CreateInstance(guiType);
                gui.AssignNewShaderToMaterial(material, oldShader, newShader);
                Assert.That(material.shader, Is.SameAs(newShader));
                Assert.That(material.GetTexture("_BaseMap"), Is.SameAs(texture));
                Assert.That(material.GetColor("_TintColor"), Is.EqualTo(new Color(0.2f, 0.6f, 0.8f, 0.5f)));
                Assert.That(material.renderQueue, Is.EqualTo(3013));
                Assert.That(material.GetTag("RenderType", false), Is.EqualTo("Transparent"));
                Assert.That(material.GetFloat("_BlendSrc"), Is.EqualTo((float)UnityEngine.Rendering.BlendMode.SrcAlpha));
                Assert.That(material.GetFloat("_BlendDst"), Is.EqualTo((float)UnityEngine.Rendering.BlendMode.One));
                Assert.That(material.IsKeywordEnabled("_BASE_MAP_MODE_2D"), Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(material); UnityEngine.Object.DestroyImmediate(texture); }
        }

        [Test]
        public void EveryOriginalPublicAndProtectedEditorDeclarationRemainsAvailable()
        {
            var reference = JsonUtility.FromJson<NovaEditorContract.Snapshot>(File.ReadAllText(
                "Assets/Tests/Editor/Reference/EditorContract.json"));
            Assert.That(reference.entries.Length, Is.GreaterThan(300));
            Assert.That(reference.entries.Except(NovaEditorContract.Read()), Is.Empty,
                "Original method signatures, constants, enum values and property accessors must survive the port.");
        }

        [TestCase("tex_editor_icon_foldout_true")]
        [TestCase("tex_editor_icon_foldout_false")]
        public void OriginalInspectorFoldoutIconsAreIncluded(string resourceName)
        {
            var icon = Resources.Load<Texture2D>(resourceName);
            Assert.That(icon, Is.Not.Null, "The original Inspector draws this resource on every section header.");
            Assert.That(icon.width, Is.GreaterThan(0));
            Assert.That(icon.height, Is.GreaterThan(0));
        }
    }
}

using System;
using System.IO;
using System.Linq;
using Nova.Editor.Core.Scripts;
using Nova.Editor.Core.Scripts.Optimizer;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using RenderType = Nova.Editor.Core.Scripts.RenderType;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInOptimizerTests
    {
        private const string Folder = "Assets/NovaOptimizerVerification";
        private const string Outside = "Assets/NovaOptimizerOutside.mat";

        [OneTimeSetUp]
        public void Generate() { OptimizedShaderGenerator.Generate(Folder + "/Generated shaders"); }

        [OneTimeTearDown]
        public void Cleanup()
        {
            AssetDatabase.DeleteAsset(Folder);
            AssetDatabase.DeleteAsset(Outside);
        }

        [TestCase("UberUnlit")]
        [TestCase("UberLit")]
        public void EveryRenderTypeAndPassCombinationPreservesContractAndCompiles(string kind)
        {
            var original = Shader.Find("Nova/Particles/" + kind);
            for (var render = 0; render < 3; render++)
            for (var passes = 0; passes < 8; passes++)
            {
                var name = Name(kind, (RenderType)render, (OptionalShaderPass)passes);
                var shader = Shader.Find(name);
                Assert.That(shader, Is.Not.Null, name);
                Assert.That(ShaderUtil.GetPropertyCount(shader), Is.EqualTo(ShaderUtil.GetPropertyCount(original)), name);
                for (var property = 0; property < ShaderUtil.GetPropertyCount(original); property++)
                    Assert.That(ShaderUtil.GetPropertyName(shader, property), Is.EqualTo(ShaderUtil.GetPropertyName(original, property)), name);
                var material = new Material(shader);
                try
                {
                    Assert.That(material.FindPass("SceneSelectionPass"), Is.GreaterThanOrEqualTo(0), name);
                    Assert.That(material.FindPass("Picking"), Is.GreaterThanOrEqualTo(0), name);
                    Assert.That(material.FindPass("ShadowCaster") >= 0, Is.EqualTo((passes & 4) != 0), name);
                    Assert.That(material.FindPass("DepthOnly") >= 0, Is.EqualTo((passes & 1) != 0), name);
                    if (kind == "UberLit") Assert.That(material.FindPass("ForwardAdd"), Is.GreaterThanOrEqualTo(0), name);
                    material.EnableKeyword("_BASE_MAP_MODE_2D");
                    material.EnableKeyword("_ALPHATEST_ENABLED");
                    material.EnableKeyword("_VERTEX_DEFORMATION_ENABLED");
                    for (var pass = 0; pass < material.passCount; pass++)
                    {
                        ShaderUtil.CompilePass(material, pass, true);
                        Assert.That(ShaderUtil.IsPassCompiled(material, pass), Is.True, name);
                    }
                    var errors = ShaderUtil.GetShaderMessages(shader).Where(m => m.severity.ToString() == "Error").Select(m => m.message);
                    Assert.That(errors, Is.Empty, name);
                }
                finally { UnityEngine.Object.DestroyImmediate(material); }
            }
        }

        [Test]
        public void GeneratedIncludePathsRemainValidAfterMovingOutputFolder()
        {
            Assert.That(AssetDatabase.IsValidFolder(Folder + "/Generated shaders"), Is.True);
            var error = AssetDatabase.MoveAsset(Folder + "/Generated shaders", Folder + "/Moved");
            Assert.That(error, Is.Empty);
            try
            {
                var path = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:Shader", new[] { Folder + "/Moved" })[0]);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                var material = new Material(shader);
                try
                {
                    ShaderUtil.CompilePass(material, 0, true);
                    Assert.That(ShaderUtil.GetShaderMessages(shader).Where(m => m.severity.ToString() == "Error"), Is.Empty);
                }
                finally { UnityEngine.Object.DestroyImmediate(material); }
            }
            finally { Assert.That(AssetDatabase.MoveAsset(Folder + "/Moved", Folder + "/Generated shaders"), Is.Empty); }
        }

        [TestCase(RenderType.Opaque, OptionalShaderPass.ShadowCaster)]
        [TestCase(RenderType.Cutout, OptionalShaderPass.DepthOnly)]
        [TestCase(RenderType.Transparent, OptionalShaderPass.None)]
        public void ReplacementIsScopedAndPersistsPropertiesKeywordsAndQueue(RenderType renderType, OptionalShaderPass passes)
        {
            var path = Folder + "/Material.mat";
            var material = new Material(Shader.Find("Nova/Particles/UberUnlit"));
            material.SetFloat("_RenderType", (float)renderType);
            material.SetFloat("_QueueOffset", 3123 - (renderType == RenderType.Opaque ? 2000 : renderType == RenderType.Cutout ? 2450 : 3000));
            material.SetFloat("_TintAreaMode", 1);
            material.SetColor("_TintColor", new Color(0.2f, 0.7f, 0.4f, 0.6f));
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.CreateAsset(new Material(material), Outside);
            // Import invokes ShaderGUI.ValidateMaterial; apply caller overrides after it.
            material.EnableKeyword("_TINT_COLOR_ENABLED");
            material.renderQueue = 3123;
            try
            {
                OptimizedShaderReplacer.Replace(new OptimizedShaderReplacer.Settings
                {
                    TargetFolderPath = Folder,
                    OpaqueRequiredPasses = OptionalShaderPass.ShadowCaster,
                    CutoutRequiredPasses = OptionalShaderPass.DepthOnly,
                    TransparentRequiredPasses = OptionalShaderPass.None
                });
                Assert.That(material.shader.name, Is.EqualTo(Name("UberUnlit", renderType, passes)));
                Assert.That(material.renderQueue, Is.EqualTo(3123));
                Assert.That(material.IsKeywordEnabled("_TINT_COLOR_ENABLED"), Is.True);
                Assert.That(material.GetColor("_TintColor").g, Is.EqualTo(0.7f).Within(0.001));
                Assert.That(AssetDatabase.LoadAssetAtPath<Material>(Outside).shader.name, Is.EqualTo("Nova/Particles/UberUnlit"));
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                Assert.That(AssetDatabase.LoadAssetAtPath<Material>(path).shader.name, Is.EqualTo(Name("UberUnlit", renderType, passes)));
            }
            finally
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.DeleteAsset(Outside);
            }
        }

        [Test]
        public void InvalidOutputCannotOverwritePackageOrLeaveAssets()
        {
            Assert.Throws<ArgumentException>(() => OptimizedShaderGenerator.Generate("Packages/Unsafe"));
            Assert.Throws<ArgumentException>(() => OptimizedShaderGenerator.Generate("../Outside"));
        }

        private static string Name(string kind, RenderType renderType, OptionalShaderPass passes)
        {
            var names = new[] { OptionalShaderPass.DepthOnly, OptionalShaderPass.DepthNormals, OptionalShaderPass.ShadowCaster }
                .Where(p => (passes & p) != 0).Select(p => p.ToString()).ToArray();
            return "Hidden/Nova/Particles/" + kind + "(Optimized " + renderType + " " +
                (names.Length == 0 ? "None" : string.Join(" ", names)) + ")";
        }
    }
}

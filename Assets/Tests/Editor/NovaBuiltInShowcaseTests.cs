using System;
using System.IO;
using System.Linq;
using Nova.Editor.Core.Scripts;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInShowcaseTests
    {
        private const string Folder = "Assets/NovaShowcaseVerification";
        private const string ScenePath = Folder + "/Showcase.unity";

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.CreateFolder("Assets", "NovaShowcaseVerification");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), Folder + "/Previous.unity");
        }

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AssetDatabase.DeleteAsset(Folder);
        }

        [Test]
        public void PackageDeclaresAnImportableShowcaseSample()
        {
            var manifest = File.ReadAllText("Assets/Nova/package.json");
            Assert.That(manifest, Does.Contain("Samples~/BuiltInShowcase"));
            Assert.That(File.Exists("Assets/Nova/Samples~/BuiltInShowcase/NovaShowcase.unity"), Is.True);
            Assert.That(Directory.GetFiles("Assets/Nova/Samples~/BuiltInShowcase/NovaShowcase Assets", "*.mat").Length, Is.EqualTo(6));
        }

        [Test]
        public void UntitledSceneIsPreservedAndGetsAnActionableError()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var marker = new GameObject("Unsaved work");
            var error = Assert.Throws<InvalidOperationException>(() => BuiltInShowcase.Create(ScenePath));
            Assert.That(error.Message, Does.Contain("Save untitled"));
            Assert.That(marker, Is.Not.Null);
            Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(scene));
            Assert.That(File.Exists(ScenePath), Is.False);
        }

        [Test]
        public void GeneratedSceneHasWorkingMaterialsParticlesDepthAndDistortion()
        {
            var previous = SceneManager.GetActiveScene();
            BuiltInShowcase.Create(ScenePath);
            Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(previous), "Creating a sample must preserve the active scene.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            var target = new RenderTexture(480, 320, 24);
            var image = new Texture2D(480, 320, TextureFormat.RGB24, false);
            var oldTarget = RenderTexture.active;
            try
            {
                var roots = scene.GetRootGameObjects();
                foreach (var label in roots.SelectMany(r => r.GetComponentsInChildren<TextMesh>()))
                {
                    var bounds = label.GetComponent<Renderer>().bounds;
                    Assert.That(bounds.size.y, Is.LessThan(0.6f), label.text);
                    Assert.That(bounds.max.x, Is.LessThan(4.5f), label.text);
                }
                var camera = roots.SelectMany(r => r.GetComponentsInChildren<Camera>()).Single();
                Assert.That(camera.depthTextureMode & DepthTextureMode.Depth, Is.Not.Zero);
                Assert.That(camera.GetComponents<Component>().Any(c => c.GetType().Name == "ScreenSpaceDistortion"), Is.True);
                var systems = roots.SelectMany(r => r.GetComponentsInChildren<ParticleSystem>()).ToArray();
                Assert.That(systems.Length, Is.GreaterThanOrEqualTo(4));
                foreach (var system in systems) system.Simulate(2f, true, true, true);
                var materials = roots.SelectMany(r => r.GetComponentsInChildren<Renderer>()).SelectMany(r => r.sharedMaterials).ToArray();
                Assert.That(materials, Has.None.Null);
                foreach (var name in new[] { "Nova/Particles/UberUnlit", "Nova/Particles/UberLit", "Nova/UIParticles/UberUnlit", "Nova/UIParticles/UberLit", "Nova/Particles/Distortion" })
                    Assert.That(materials.Any(m => m.shader.name == name), Is.True, name);
                Assert.That(materials.All(m => AssetDatabase.Contains(m)), Is.True, "Scene material references must survive reload/build.");
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 480, 320), 0, 0);
                image.Apply();
                Directory.CreateDirectory("Logs");
                File.WriteAllBytes("Logs/NovaShowcase.png", image.EncodeToPNG());
                var colorful = image.GetPixels().Count(c => Mathf.Max(c.r, c.g, c.b) - Mathf.Min(c.r, c.g, c.b) > 0.15f);
                Assert.That(colorful, Is.GreaterThan(1500), "Sample must visibly render its colored effects.");
                camera.targetTexture = null;
            }
            finally
            {
                RenderTexture.active = oldTarget;
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(image);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void CreateRefusesToOverwriteExistingSceneOrAssets()
        {
            BuiltInShowcase.Create(ScenePath);
            var original = File.ReadAllBytes(ScenePath);
            Assert.Throws<ArgumentException>(() => BuiltInShowcase.Create(ScenePath));
            CollectionAssert.AreEqual(original, File.ReadAllBytes(ScenePath));
        }

        [TestCase("Packages/Showcase.unity")]
        [TestCase("Assets/../Showcase.unity")]
        [TestCase("Assets/Showcase.txt")]
        public void CreateRejectsInvalidOutput(string path)
        {
            Assert.Throws<ArgumentException>(() => BuiltInShowcase.Create(path));
        }
    }
}

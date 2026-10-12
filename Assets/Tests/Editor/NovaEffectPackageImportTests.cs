using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Nova.Tests.Editor
{
    public sealed class NovaEffectPackageImportTests
    {
        private const string Folder = "Assets/NovaEffectImportVerification";

        [Test]
        public void ChangingInputAfterPreviewIsRejectedBeforeImport()
        {
            var archive = "Temp/NovaEffectStale.unitypackage";
            Directory.CreateDirectory("Temp");
            File.Copy("Assets/Tests/Editor/Reference/EffectImportFixture.bytes", archive, true);
            var plan = Nova.Editor.Core.Scripts.BuiltInEffectPackageImporter.Preview(archive);
            File.AppendAllText(archive, "changed");
            Assert.Throws<InvalidOperationException>(() => Nova.Editor.Core.Scripts.BuiltInEffectPackageImporter.Import(plan, false));
            Assert.That(Directory.Exists(Folder), Is.False);
        }

        [UnityTest]
        public IEnumerator InteractiveImportShowsOnlyRetainedAssetsInUnityDialog()
        {
            var archive = "Temp/NovaEffectInteractive.unitypackage";
            Directory.CreateDirectory("Temp");
            File.Copy("Assets/Tests/Editor/Reference/EffectImportFixture.bytes", archive, true);
            Nova.Editor.Core.Scripts.BuiltInEffectPackageImporter.Import(
                Nova.Editor.Core.Scripts.BuiltInEffectPackageImporter.Preview(archive), true);
            EditorWindow window = null;
            try
            {
                for (var i = 0; i < 100 && window == null; i++)
                {
                    typeof(AssetDatabase).Assembly.GetType("UnityEditor.PackageUtility")
                        .GetMethod("TickPackageImport", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
                    yield return null;
                    window = Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(w => w.GetType().FullName == "UnityEditor.PackageImport");
                }
                Assert.That(window, Is.Not.Null, "The user must get Unity's normal import selection dialog.");
                var items = (Array)window.GetType().GetProperty("packageItems").GetValue(window);
                var paths = items.Cast<object>().Select(item => (string)item.GetType().GetField("exportedAssetPath").GetValue(item)).ToArray();
                Assert.That(paths, Does.Contain(Folder + "/Effect.mat"));
                Assert.That(paths.Any(path => path.Contains("/Bundled/")), Is.False);
                Assert.That(Directory.Exists(Folder), Is.False, "Opening the dialog must not import before the user clicks Import.");
            }
            finally { if (window != null) window.Close(); }
        }

        [Test]
        public void UnityImporterSkipsBundledNovaButKeepsUntouchedEffectAndUnrelatedFiles()
        {
            var type = typeof(Nova.Editor.Core.Scripts.BuiltInMaterialMigration).Assembly
                .GetType("Nova.Editor.Core.Scripts.BuiltInEffectPackageImporter");
            Assert.That(type, Is.Not.Null, "Effect packages need an import path that excludes bundled URP code.");
            var archive = "Temp/NovaEffectImportFixture.unitypackage";
            Directory.CreateDirectory("Temp");
            File.Copy("Assets/Tests/Editor/Reference/EffectImportFixture.bytes", archive, true);
            var before = File.ReadAllBytes(archive);
            var plan = type.GetMethod("Preview").Invoke(null, new object[] { archive });
            var paths = (string[])plan.GetType().GetProperty("AssetPaths").GetValue(plan);
            var excluded = (string[])plan.GetType().GetProperty("ExcludedNovaPaths").GetValue(plan);
            Assert.That(paths.Where(p => Path.HasExtension(p)).Count(), Is.EqualTo(2), string.Join("\n", paths));
            Assert.That(excluded, Is.EquivalentTo(new[] { Folder + "/Bundled/ParticlesUberUnlit.shader", Folder + "/Bundled/ScreenSpaceDistortion.cs" }));
            Assert.That(Directory.Exists(Folder), Is.False, "Preview may extract to Temp but cannot create project assets.");
            try
            {
                type.GetMethod("Import").Invoke(null, new[] { plan, (object)false });
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Assert.That(File.ReadAllBytes(archive), Is.EqualTo(before));
                Assert.That(File.ReadAllText(Folder + "/ParticlesUberUnlit.txt"), Is.EqualTo("Unrelated user asset.\n"));
                Assert.That(File.Exists(Folder + "/Bundled/ScreenSpaceDistortion.cs"), Is.False);
                Assert.That(File.Exists(Folder + "/Bundled/ParticlesUberUnlit.shader"), Is.False);
                var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/Effect.mat");
                Assert.That(material, Is.Not.Null);
                Assert.That(material.shader, Is.EqualTo(Shader.Find("Nova/Particles/UberUnlit")));
                Assert.That(material.renderQueue, Is.EqualTo(3017));
                Assert.That(File.ReadAllText(Folder + "/Effect.mat"), Is.EqualTo(File.ReadAllText("Assets/Tests/Editor/Reference/EffectImportMaterial.txt")));
            }
            finally { AssetDatabase.DeleteAsset(Folder); }
        }
    }
}

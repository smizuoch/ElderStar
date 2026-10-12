using System;
using System.IO;
using Nova.Editor.Core.Scripts;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInMaterialMigrationTests
    {
        private const string Folder = "Assets/NovaMigrationVerification";
        private const string Outside = "Assets/NovaMigrationOutside.mat";

        [SetUp]
        public void SetUp() { AssetDatabase.CreateFolder("Assets", "NovaMigrationVerification"); }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(Folder);
            AssetDatabase.DeleteAsset(Outside);
        }

        [TestCase("Nova/Particles/UberUnlit", "4651e33d77f6bedcc896127e121e2eb6")]
        [TestCase("Nova/Particles/UberLit", "30bca65b80428acc1b3b406943fb07ed")]
        [TestCase("Nova/UIParticles/UberUnlit", "776e3569165e363519e50f5106284c3c")]
        [TestCase("Nova/UIParticles/UberLit", "6e936eaf27061e8b3f787102769ecc27")]
        [TestCase("Nova/Particles/Distortion", "85397f9e441dca7ef1ef3b5d59ec92a7")]
        [TestCase("Hidden/Nova/Particles/ApplyDistortion", "7319141515ca58ce5781606eef21f5d5")]
        public void PreviewIsReadOnlyAndApplyPreservesMaterialDataWithBackup(string shaderName, string originalGuid)
        {
            var path = Folder + "/材質.mat";
            var builtinGuid = CreatePreviousPortMaterial(path, shaderName, originalGuid);
            var originalBytes = File.ReadAllBytes(path);
            var plan = BuiltInMaterialMigration.Preview(Folder);
            Assert.That(plan.Entries.Count, Is.EqualTo(1));
            Assert.That(plan.Entries[0].ShaderName, Is.EqualTo(shaderName));
            Assert.That(File.ReadAllBytes(path), Is.EqualTo(originalBytes), "Preview must not write.");

            var backup = BuiltInMaterialMigration.Apply(plan);
            Assert.That(File.ReadAllBytes(Path.Combine(backup, path)), Is.EqualTo(originalBytes));
            Assert.That(File.ReadAllText(path), Is.EqualTo(System.Text.Encoding.UTF8.GetString(originalBytes).TrimStart('\uFEFF').Replace(originalGuid, builtinGuid)));
            var migrated = AssetDatabase.LoadAssetAtPath<Material>(path);
            Assert.That(migrated.shader.name, Is.EqualTo(shaderName));
            if (migrated.HasProperty("_TintColor"))
                Assert.That(migrated.GetColor("_TintColor").g, Is.EqualTo(0.37f).Within(0.0001f));
            Assert.That(BuiltInMaterialMigration.Preview(Folder).Entries, Is.Empty, "Migration must be idempotent.");
        }

        [Test]
        public void MigrationPreservesBomLineEndingsAndUnrelatedGuidText()
        {
            const string originalGuid = "4651e33d77f6bedcc896127e121e2eb6";
            var path = Folder + "/Encoded.mat";
            var builtinGuid = CreatePreviousPortMaterial(path, "Nova/Particles/UberUnlit", originalGuid);
            var text = File.ReadAllText(path).Replace("\r\n", "\n").Replace("\n", "\r\n")
                + "\r\n# unrelated reference " + originalGuid + "\r\n";
            var encoding = new System.Text.UTF8Encoding(true);
            File.WriteAllText(path, text, encoding);
            var expected = text.Replace("guid: " + originalGuid + ", type: 3", "guid: " + builtinGuid + ", type: 3");
            BuiltInMaterialMigration.Apply(BuiltInMaterialMigration.Preview(Folder));
            CollectionAssert.AreEqual(encoding.GetPreamble().ConcatBytes(encoding.GetBytes(expected)), File.ReadAllBytes(path));
        }

        [Test]
        public void PreviewIsScopedAndReportsNonTextMaterials()
        {
            CreatePreviousPortMaterial(Outside, "Nova/Particles/UberUnlit", "4651e33d77f6bedcc896127e121e2eb6");
            var outsideBytes = File.ReadAllBytes(Outside);
            var current = new Material(Shader.Find("Nova/Particles/UberUnlit"));
            AssetDatabase.CreateAsset(current, Folder + "/AlreadyBuiltIn.mat");
            File.WriteAllBytes(Folder + "/Binary.mat", new byte[] { 0, 1, 2, 3 });
            var plan = BuiltInMaterialMigration.Preview(Folder);
            Assert.That(plan.Entries, Is.Empty);
            Assert.That(plan.SkippedFiles, Does.Contain(Folder + "/Binary.mat"));
            Assert.That(File.ReadAllBytes(Outside), Is.EqualTo(outsideBytes));
        }

        [TestCase("Packages")]
        [TestCase("Assets/../Packages")]
        [TestCase("Library")]
        public void PreviewRejectsFoldersOutsideAssets(string folder)
        {
            Assert.Throws<ArgumentException>(() => BuiltInMaterialMigration.Preview(folder));
        }

        [Test]
        public void ChangedMaterialInvalidatesTheWholePreviewBeforeAnyWrite()
        {
            CreatePreviousPortMaterial(Folder + "/A.mat", "Nova/Particles/UberUnlit", "4651e33d77f6bedcc896127e121e2eb6");
            CreatePreviousPortMaterial(Folder + "/B.mat", "Nova/Particles/UberLit", "30bca65b80428acc1b3b406943fb07ed");
            var plan = BuiltInMaterialMigration.Preview(Folder);
            var beforeA = File.ReadAllBytes(Folder + "/A.mat");
            File.AppendAllText(Folder + "/B.mat", "\n# changed after preview\n");
            Assert.Throws<InvalidOperationException>(() => BuiltInMaterialMigration.Apply(plan));
            Assert.That(File.ReadAllBytes(Folder + "/A.mat"), Is.EqualTo(beforeA));
        }

        private static string CreatePreviousPortMaterial(string path, string shaderName, string originalGuid)
        {
            var shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null);
            var builtinGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(shader));
            var material = new Material(shader) { name = "移行テスト" };
            if (material.HasProperty("_TintColor")) material.SetColor("_TintColor", new Color(0.2f, 0.37f, 0.8f, 0.6f));
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.SaveAssets();
            File.WriteAllText(path, File.ReadAllText(path).Replace(builtinGuid, originalGuid));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return builtinGuid;
        }
    }

    internal static class MigrationTestBytes
    {
        internal static byte[] ConcatBytes(this byte[] first, byte[] second)
        {
            var result = new byte[first.Length + second.Length];
            first.CopyTo(result, 0);
            second.CopyTo(result, first.Length);
            return result;
        }
    }
}

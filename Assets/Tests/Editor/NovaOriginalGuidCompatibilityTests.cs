using System.IO;
using System.Linq;
using Nova.Editor.Core.Scripts;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nova.Tests.Editor
{
    public sealed class NovaOriginalGuidCompatibilityTests
    {
        private const string Folder = "Assets/NovaOriginalGuidVerification";

        [TestCase("Nova/Particles/UberUnlit", "1bc5c6a70411243779ce486a98012125", "4651e33d77f6bedcc896127e121e2eb6")]
        [TestCase("Nova/Particles/UberLit", "65654f079f6f340b09f954eb6366ad5a", "30bca65b80428acc1b3b406943fb07ed")]
        [TestCase("Nova/UIParticles/UberUnlit", "72225468864e7e849bc3e630d1756259", "776e3569165e363519e50f5106284c3c")]
        [TestCase("Nova/UIParticles/UberLit", "5befadde7f2b6a9498dd225c31369d75", "6e936eaf27061e8b3f787102769ecc27")]
        [TestCase("Nova/Particles/Distortion", "f16a2dd74da8941d6a883c14b1f26710", "85397f9e441dca7ef1ef3b5d59ec92a7")]
        [TestCase("Hidden/Nova/Particles/ApplyDistortion", "4040547c0f97a42daaee2e90c4800bc5", "7319141515ca58ce5781606eef21f5d5")]
        public void OriginalGuidImportsWithoutRewritingAndPreviousPortGuidStillWorks(string name, string originalGuid, string previousGuid)
        {
            AssetDatabase.CreateFolder("Assets", "NovaOriginalGuidVerification");
            try
            {
                var shader = Shader.Find(name);
                Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(shader)), Is.EqualTo(originalGuid));
                var path = Folder + "/Untouched.mat";
                var text = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!21 &2100000\nMaterial:\n" +
                    "  serializedVersion: 8\n  m_Name: Untouched\n  m_Shader: {fileID: 4800000, guid: " + originalGuid + ", type: 3}\n" +
                    "  m_ValidKeywords: []\n  m_InvalidKeywords: []\n  m_CustomRenderQueue: 3011\n" +
                    "  m_SavedProperties:\n    serializedVersion: 3\n    m_TexEnvs: []\n    m_Ints: []\n" +
                    "    m_Floats: []\n    m_Colors: []\n";
                File.WriteAllText(path, text);
                var before = File.ReadAllBytes(path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                Assert.That(material.shader, Is.EqualTo(shader));
                Assert.That(material.renderQueue, Is.EqualTo(3011));
                Assert.That(BuiltInMaterialMigration.Preview(Folder).Entries, Is.Empty);
                Assert.That(File.ReadAllBytes(path), Is.EqualTo(before));

                var previous = AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(previousGuid));
                Assert.That(previous, Is.Not.Null, "Previously released material references must remain valid.");
                Assert.That(previous.GetPropertyCount(), Is.EqualTo(shader.GetPropertyCount()));
                for (var i = 0; i < shader.GetPropertyCount(); i++)
                    Assert.That(previous.GetPropertyName(i), Is.EqualTo(shader.GetPropertyName(i)));
                var legacyMaterial = new Material(previous);
                try
                {
                    Assert.That(legacyMaterial.passCount, Is.EqualTo(material.passCount));
                    for (var pass = 0; pass < legacyMaterial.passCount; pass++) ShaderUtil.CompilePass(legacyMaterial, pass, true);
                    Assert.That(ShaderUtil.GetShaderMessages(previous).Where(m => m.severity.ToString() == "Error"), Is.Empty);
                }
                finally { Object.DestroyImmediate(legacyMaterial); }
            }
            finally { AssetDatabase.DeleteAsset(Folder); }
        }
    }
}

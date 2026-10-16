using System;
using System.IO;
using System.Linq;
using Nova.Editor.Core.Scripts.Optimizer;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nova.Tests.PackageImport
{
    public sealed class NovaPackageIdentityMigrationTests
    {
        private const string Folder = "Assets/ElderStarIdentityVerificationSettled";
        private const string Generated = Folder + "/Generated";
        private const string Baseline = "Library/ElderStarIdentityBaselineSettled.json";
        [Serializable] private sealed class Entry { public string path, bytes, shaderGuid; }
        [Serializable] private sealed class Snapshot { public Entry[] materials; public string[] generatedPaths, generatedGuids; }

        // Run while the old-ID package is installed, before Client.AddAndRemove.
        public static void Prepare()
        {
            try
            {
                if (Directory.Exists(Folder)) throw new InvalidOperationException("Use a new identity fixture folder.");
                if (!UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Any(p => p.name == "jp.co.cyberagent.nova.builtin"))
                    throw new InvalidOperationException("Prepare must run on the old package ID.");
                if (Shader.Find("Nova/Particles/UberLit") == null) throw new InvalidOperationException("Install the old package first.");
                Directory.CreateDirectory(Folder);
                AssetDatabase.Refresh();
                OptimizedShaderGenerator.Generate(Generated);
                var generated = AssetDatabase.FindAssets("t:Shader", new[] { Generated }).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
                var shaders = new[] { "Nova/Particles/UberUnlit", "Nova/Particles/UberLit", "Nova/Particles/Distortion",
                    "Nova/UIParticles/UberUnlit", "Nova/UIParticles/UberLit", "Hidden/Nova/Particles/ApplyDistortion" }
                    .Select(Shader.Find).Concat(new[] { AssetDatabase.LoadAssetAtPath<Shader>(generated[0]) }).ToArray();
                var texture = new Texture2D(2, 2);
                texture.SetPixels(new[] { Color.red, Color.green, Color.blue, Color.white }); texture.Apply();
                AssetDatabase.CreateAsset(texture, Folder + "/RetainedTexture.asset");
                var paths = new string[shaders.Length];
                for (int i = 0; i < shaders.Length; i++)
                {
                    var material = new Material(shaders[i]);
                    if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
                    if (material.HasProperty("_TintColor")) material.SetColor("_TintColor", new Color(.17f, .32f, .63f, .81f));
                    if (material.HasProperty("_SoftParticlesIntensity")) material.SetFloat("_SoftParticlesIntensity", .37f);
                    // Author materials as the original Inspector does. A raw new Material
                    // otherwise receives deferred keyword/blend initialization on a later import.
                    var prefix = shaders[i].name.Contains("UIParticles") ? "UIParticles" : "Particles";
                    var guiName = shaders[i].name.Contains("UberLit") ? prefix + "UberLitGUI" :
                        shaders[i].name.Contains("UberUnlit") ? prefix + "UberUnlitGUI" :
                        shaders[i].name == "Nova/Particles/Distortion" ? "ParticlesDistortionGUI" : null;
                    if (guiName != null)
                        ((ShaderGUI)Activator.CreateInstance(typeof(OptimizedShaderGenerator).Assembly.GetType(
                            "Nova.Editor.Core.Scripts." + guiName), true)).ValidateMaterial(material);
                    paths[i] = Folder + "/Material" + i + ".mat";
                    AssetDatabase.CreateAsset(material, paths[i]);
                }
                AssetDatabase.SaveAssets();
                var snapshot = new Snapshot
                {
                    materials = paths.Select(path => new Entry { path = path, bytes = Convert.ToBase64String(File.ReadAllBytes(path)),
                        shaderGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<Material>(path).shader)) }).ToArray(),
                    generatedPaths = generated, generatedGuids = generated.Select(AssetDatabase.AssetPathToGUID).ToArray()
                };
                File.WriteAllText(Baseline, JsonUtility.ToJson(snapshot, true));
                Debug.Log("Prepared package identity baseline: " + paths.Length + " materials / " + generated.Length + " generated shaders.");
                EditorApplication.Exit(0);
            }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        public static void VerifyOldBaselineAfterReload()
        {
            try
            {
                Assert.That(UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Any(p => p.name == "jp.co.cyberagent.nova.builtin"), Is.True);
                var snapshot = JsonUtility.FromJson<Snapshot>(File.ReadAllText(Baseline));
                foreach (var entry in snapshot.materials) Assert.That(AssetDatabase.LoadAssetAtPath<Material>(entry.path), Is.Not.Null);
                AssetDatabase.SaveAssets();
                foreach (var entry in snapshot.materials)
                    Assert.That(File.ReadAllBytes(entry.path), Is.EqualTo(Convert.FromBase64String(entry.bytes)), entry.path);
                Debug.Log("Old-ID material baseline is stable after reload and SaveAssets.");
                EditorApplication.Exit(0);
            }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        [Test]
        public void ReplacementRetainsSerializedMaterialsAndRegenerationKeepsOptimizedGuids()
        {
            Assert.That(File.Exists(Baseline), Is.True, "Run Prepare with the previous package installed first.");
            var snapshot = JsonUtility.FromJson<Snapshot>(File.ReadAllText(Baseline));
            var packages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
            Assert.That(packages.Count(p => p.name == "me.smizuoch.elderstar"), Is.EqualTo(1));
            Assert.That(packages.Any(p => p.name == "jp.co.cyberagent.nova.builtin"), Is.False);
            // Regenerating in place updates includes while retaining .meta/GUIDs.
            OptimizedShaderGenerator.Generate(Generated);
            // Flush deferred validation before comparing, so a later SaveAssets cannot
            // hide changes until after this assertion has already passed.
            foreach (var entry in snapshot.materials) Assert.That(AssetDatabase.LoadAssetAtPath<Material>(entry.path), Is.Not.Null);
            AssetDatabase.SaveAssets();
            Assert.That(snapshot.generatedPaths.Select(AssetDatabase.AssetPathToGUID), Is.EqualTo(snapshot.generatedGuids));
            foreach (var path in snapshot.generatedPaths)
            {
                var source = File.ReadAllText(path);
                Assert.That(source, Does.Contain("Packages/me.smizuoch.elderstar/Runtime/Core/Shaders/BuiltIn/"));
                Assert.That(source, Does.Not.Contain("Packages/jp.co.cyberagent.nova.builtin/"));
            }
            foreach (var entry in snapshot.materials)
            {
                Assert.That(File.ReadAllBytes(entry.path), Is.EqualTo(Convert.FromBase64String(entry.bytes)), entry.path);
                var material = AssetDatabase.LoadAssetAtPath<Material>(entry.path);
                Assert.That(material.shader, Is.Not.Null, entry.path);
                Assert.That(material.shader.isSupported, Is.True, entry.path);
                Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material.shader)), Is.EqualTo(entry.shaderGuid));
                if (material.HasProperty("_BaseMap")) Assert.That(material.GetTexture("_BaseMap"), Is.Not.Null, entry.path);
            }
        }
    }
}

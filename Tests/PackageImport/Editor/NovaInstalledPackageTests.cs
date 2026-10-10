using System;
using System.IO;
using System.Linq;
using Nova.Editor.Core.Scripts.Optimizer;
using NUnit.Framework;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEngine;

namespace Nova.Tests.PackageImport
{
    public sealed class NovaInstalledPackageTests
    {
        [Test]
        public void ExternalPackageImportsWithoutUrpAndAllOriginalShaderNamesCompile()
        {
            var package = PackageInfo.FindForAssetPath("Packages/jp.co.cyberagent.nova.builtin/package.json");
            Assert.That(package, Is.Not.Null);
            Assert.That(package.resolvedPath, Does.Not.StartWith(Application.dataPath));
            TestContext.Progress.WriteLine("NOVA package source: " + package.source + " at " + package.resolvedPath);
            if (Environment.GetCommandLineArgs().Contains("-novaRequireCache"))
                Assert.That(package.resolvedPath.Replace('\\', '/'), Does.Contain("/Library/PackageCache/"));
            Assert.That(PackageInfo.GetAllRegisteredPackages().Any(p => p.name == "com.unity.render-pipelines.universal"), Is.False);
            foreach (var name in new[] { "Nova/Particles/UberUnlit", "Nova/Particles/UberLit", "Nova/Particles/Distortion",
                "Nova/UIParticles/UberUnlit", "Nova/UIParticles/UberLit", "Hidden/Nova/Particles/ApplyDistortion" })
            {
                var shader = Shader.Find(name);
                Assert.That(shader, Is.Not.Null, name);
                Assert.That(AssetDatabase.GetAssetPath(shader), Does.StartWith("Packages/jp.co.cyberagent.nova.builtin/"), name);
                var material = new Material(shader);
                try
                {
                    for (var pass = 0; pass < material.passCount; pass++) ShaderUtil.CompilePass(material, pass, true);
                    Assert.That(ShaderUtil.GetShaderMessages(shader).Where(m => m.severity.ToString() == "Error")
                        .Select(m => m.message), Is.Empty, name);
                }
                finally { UnityEngine.Object.DestroyImmediate(material); }
            }
        }

        [Test]
        public void OptimizerReadsInstalledPackageAndGeneratedIncludesCompile()
        {
            const string folder = "Assets/PackageGeneratedShaders";
            try
            {
                OptimizedShaderGenerator.Generate(folder);
                var paths = AssetDatabase.FindAssets("t:Shader", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
                Assert.That(paths.Length, Is.EqualTo(48));
                foreach (var path in paths)
                {
                    var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                    var material = new Material(shader);
                    try
                    {
                        for (var pass = 0; pass < material.passCount; pass++) ShaderUtil.CompilePass(material, pass, true);
                        Assert.That(ShaderUtil.GetShaderMessages(shader).Where(m => m.severity.ToString() == "Error")
                            .Select(m => m.message), Is.Empty, path);
                        Assert.That(File.ReadAllText(path), Does.Contain("Packages/jp.co.cyberagent.nova.builtin/Runtime/Core/Shaders/BuiltIn/"));
                    }
                    finally { UnityEngine.Object.DestroyImmediate(material); }
                }
            }
            finally { AssetDatabase.DeleteAsset(folder); }
        }
    }
}

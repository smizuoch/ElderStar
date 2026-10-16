using System;
using System.IO;
using System.Linq;
using Nova.Editor.Core.Scripts.Optimizer;
using NUnit.Framework;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public sealed class ElderStarDistributionTests
{
    private static readonly string[] Shaders = { "Nova/Particles/UberUnlit", "Nova/Particles/UberLit",
        "Nova/Particles/Distortion", "Nova/UIParticles/UberUnlit", "Nova/UIParticles/UberLit",
        "Hidden/Nova/Particles/ApplyDistortion" };
    private static string Root => AssetDatabase.GetAssetPath(Shader.Find(Shaders[0])).Split(new[] { "/Runtime/" }, StringSplitOptions.None)[0];

    [Test]
    public void AllOriginalShadersCompileWithoutUrp()
    {
        Assert.That(GraphicsSettings.currentRenderPipeline, Is.Null);
        Assert.That(PackageInfo.GetAllRegisteredPackages().Any(p => p.name == "com.unity.render-pipelines.universal"), Is.False);
        foreach (var name in Shaders)
        {
            var shader = Shader.Find(name);
            Assert.That(shader, Is.Not.Null, name);
            Assert.That(shader.isSupported, Is.True, name);
            Compile(shader);
        }
    }

    [Test]
    public void PackageIncludesIdentityOfflineGuideAndOriginalLicense()
    {
        var root = Root;
        Assert.That(File.ReadAllText(root + "/package.json"), Does.Contain("me.smizuoch.elderstar"));
        Assert.That(File.ReadAllText(root + "/LICENSE.md"), Does.Contain("Copyright 2021 CyberAgent"));
        var docs = root + (root.StartsWith("Assets/") ? "/Documentation" : "/Documentation~");
        Assert.That(File.Exists(docs + "/EditorAPI.md"), Is.True);
        Assert.That(File.Exists(docs + "/Images/barrier.gif"), Is.True);
        Assert.That(File.ReadAllText(root + "/README.md"), Does.Contain(".unitypackage"));
    }

    [Test]
    public void PackagedShowcaseReferencesResolveAndActuallyRender()
    {
        string path;
        if (Root.StartsWith("Assets/")) path = Root + "/Samples/BuiltInShowcase/NovaShowcase.unity";
        else
        {
            var info = PackageInfo.FindForAssetPath(Root + "/package.json");
            var sample = Sample.FindByPackage(info.name, info.version).Single();
            Assert.That(sample.Import(Sample.ImportOptions.OverridePreviousImports | Sample.ImportOptions.HideImportWindow), Is.True);
            path = sample.importPath + "/NovaShowcase.unity";
        }
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var target = new RenderTexture(960, 640, 24);
        var image = new Texture2D(960, 640, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            var roots = scene.GetRootGameObjects();
            var systems = roots.SelectMany(o => o.GetComponentsInChildren<ParticleSystem>()).ToArray();
            Assert.That(systems.Length, Is.EqualTo(5));
            foreach (var system in systems) system.Simulate(2, true, true, true);
            var materials = roots.SelectMany(o => o.GetComponentsInChildren<Renderer>()).SelectMany(r => r.sharedMaterials).ToArray();
            Assert.That(materials.All(m => m != null && m.shader != null && m.shader.isSupported), Is.True);
            var camera = roots.SelectMany(o => o.GetComponentsInChildren<Camera>()).Single();
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 960, 640), 0, 0);
            image.Apply();
            var colored = image.GetPixels().Count(c => Mathf.Max(c.r, c.g, c.b) - Mathf.Min(c.r, c.g, c.b) > 0.15f);
            TestContext.Progress.WriteLine("Rendered colored pixels: " + colored);
            Assert.That(colored, Is.GreaterThan(6000));
            Directory.CreateDirectory("Logs");
            File.WriteAllBytes("Logs/distribution-showcase.png", image.EncodeToPNG());
            foreach (var shader in materials.Select(m => m.shader).Distinct()) Compile(shader);
            camera.targetTexture = null;
        }
        finally
        {
            RenderTexture.active = previous;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(image);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
    }

    [Test]
    public void OptimizerIncludesResolveFromInstalledDistribution()
    {
        const string folder = "Assets/DistributionGenerated";
        try
        {
            OptimizedShaderGenerator.Generate(folder);
            var paths = AssetDatabase.FindAssets("t:Shader", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            Assert.That(paths.Length, Is.EqualTo(48));
            foreach (var path in paths)
            {
                Assert.That(File.ReadAllText(path), Does.Contain(Root + "/Runtime/Core/Shaders/BuiltIn/"));
                Compile(AssetDatabase.LoadAssetAtPath<Shader>(path));
            }
        }
        finally { AssetDatabase.DeleteAsset(folder); }
    }

    private static void Compile(Shader shader)
    {
        var material = new Material(shader);
        try
        {
            for (int pass = 0; pass < material.passCount; pass++) ShaderUtil.CompilePass(material, pass, true);
            Assert.That(ShaderUtil.GetShaderMessages(shader).Where(m => m.severity.ToString() == "Error").Select(m => m.message), Is.Empty, shader.name);
        }
        finally { Object.DestroyImmediate(material); }
    }
}

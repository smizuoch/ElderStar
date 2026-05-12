using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class NovaBuiltInCompatibilityLayerTests
{
    private const string RuntimeShaderRoot = "Assets/Nova/Runtime/Core/Shaders";
    private const string BuiltInIncludeRoot = "Assets/Nova/Runtime/Core/Shaders/BuiltIn";

    private static readonly string[] IncludeFiles =
    {
        "NovaBuiltInCore.cginc",
        "NovaBuiltInTexture.cginc",
        "NovaBuiltInDepth.cginc",
        "NovaBuiltInFog.cginc",
        "NovaBuiltInInstancing.cginc",
        "NovaBuiltInLighting.cginc",
        "NovaBuiltInShadow.cginc",
        "NovaBuiltInUI.cginc",
    };

    private static readonly string[] RuntimeShaderForbiddenTokens =
    {
        "Packages/com.unity.render-pipelines.universal",
        "UnityEngine.Rendering.Universal",
        "RenderPipeline\" = \"UniversalPipeline\"",
        "UniversalForward",
        "UniversalFragmentPBR",
        "DeclareDepthTexture.hlsl",
        "ProbeVolumeVariants.hlsl",
        "FoveatedRenderingKeywords.hlsl",
        "ScriptableRendererFeature",
        "ScriptableRenderPass",
        "RenderGraph",
    };

    private static readonly string[] SmokeShaderNames =
    {
        "Hidden/NovaBuiltIn/Tests/CoreSmoke",
        "Hidden/NovaBuiltIn/Tests/TextureSmoke",
        "Hidden/NovaBuiltIn/Tests/DepthSmoke",
        "Hidden/NovaBuiltIn/Tests/LightingSmoke",
        "Hidden/NovaBuiltIn/Tests/ShadowSmoke",
        "Hidden/NovaBuiltIn/Tests/UISmoke",
    };

    [Test]
    public void BuiltInCompatibilityIncludeFilesExist()
    {
        foreach (var fileName in IncludeFiles)
        {
            var path = Path.Combine(BuiltInIncludeRoot, fileName);
            Assert.That(File.Exists(path), Is.True, $"Missing Built-in include: {path}");
        }
    }

    [Test]
    public void BuiltInRuntimeShadersDoNotContainForbiddenPipelineTokens()
    {
        var shaderFiles = Directory.EnumerateFiles(RuntimeShaderRoot, "*", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".meta", StringComparison.Ordinal))
            .Where(path => path.EndsWith(".shader", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".cginc", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".hlsl", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.That(shaderFiles, Is.Not.Empty, "No runtime shader files were found to scan.");

        var violations = shaderFiles
            .SelectMany(path =>
            {
                var text = StripComments(File.ReadAllText(path));
                return RuntimeShaderForbiddenTokens
                    .Where(token => text.Contains(token, StringComparison.Ordinal))
                    .Select(token => $"{path}: {token}");
            })
            .ToArray();

        Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
    }

    [Test]
    public void TargetShaderSkeletonsIncludeLocalBuiltInCompatibilityLayer()
    {
        var targetShaderPaths = new[]
        {
            "Assets/Nova/Runtime/Core/Shaders/ParticlesApplyDistortion.shader",
            "Assets/Nova/Runtime/Core/Shaders/ParticlesDistortion.shader",
            "Assets/Nova/Runtime/Core/Shaders/ParticlesUberUnlit.shader",
            "Assets/Nova/Runtime/Core/Shaders/UIParticlesUberUnlit.shader",
            "Assets/Nova/Runtime/Core/Shaders/ParticlesUberLit.shader",
            "Assets/Nova/Runtime/Core/Shaders/UIParticlesUberLit.shader",
        };

        foreach (var path in targetShaderPaths)
        {
            var text = File.ReadAllText(path);
            Assert.That(text, Does.Contain("BuiltIn/NovaBuiltInCore.cginc"), path);
            Assert.That(text, Does.Contain("NovaObjectToClipPos"), path);
        }
    }

    [Test]
    public void CompatibilityLayerSmokeShadersHaveNoImportOrBlockingCompilerMessages()
    {
        foreach (var shaderName in SmokeShaderNames)
        {
            var shader = Shader.Find(shaderName);
            Assert.That(shader, Is.Not.Null, shaderName);

            var path = AssetDatabase.GetAssetPath(shader);
            Assert.That(path, Is.Not.Empty, $"Could not find asset path for {shaderName}");

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var blockingMessages = ShaderUtil.GetShaderMessages(shader)
                .Where(IsBlockingShaderMessage)
                .Select(message => $"{shaderName}: {message.severity}: {message.message} ({message.file}:{message.line})")
                .ToArray();

            Assert.That(blockingMessages, Is.Empty, string.Join(Environment.NewLine, blockingMessages));
        }
    }

    private static bool IsBlockingShaderMessage(ShaderMessage message)
    {
        var severity = message.severity.ToString();
        if (string.Equals(severity, "Error", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.Equals(severity, "Warning", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        TestContext.Out.WriteLine($"Shader warning: {message.message} ({message.file}:{message.line})");
        return Regex.IsMatch(
            message.message,
            "missing|include|unknown|macro|unsupported|texture|sampler|invalid|undeclared",
            RegexOptions.IgnoreCase);
    }

    private static string StripComments(string text)
    {
        text = Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        return Regex.Replace(text, @"//.*", string.Empty);
    }
}

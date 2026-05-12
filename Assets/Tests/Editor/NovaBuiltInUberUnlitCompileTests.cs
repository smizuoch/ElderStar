using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class NovaBuiltInUberUnlitCompileTests
{
    private const string ShaderName = "Nova/Particles/UberUnlit";
    private const string ShaderPath = "Assets/Nova/Runtime/Core/Shaders/ParticlesUberUnlit.shader";

    private static readonly string[] ForbiddenTokens =
    {
        "Packages/com.unity.render-pipelines.universal",
        "UnityEngine.Rendering.Universal",
        "UnityEngine.Rendering.RenderGraphModule",
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

    [Test]
    public void UberUnlitForwardPassUsesBuiltInRenderStateAndLocalHelpers()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Contain("\"LightMode\" = \"ForwardBase\""));
        Assert.That(shaderText, Does.Contain("Blend [_BlendSrc] [_BlendDst]"));
        Assert.That(shaderText, Does.Contain("ZWrite [_ZWrite]"));
        Assert.That(shaderText, Does.Contain("Cull [_Cull]"));
        Assert.That(shaderText, Does.Contain("ZTest [_ZTest]"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInCore.cginc"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInTexture.cginc"));
        Assert.That(shaderText, Does.Contain("NovaObjectToClipPos"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX2D(_BaseMap"));
        Assert.That(shaderText, Does.Contain("_TintColor"));
        Assert.That(shaderText, Does.Contain("_ALPHATEST_ENABLED"));
        Assert.That(shaderText, Does.Contain("clip("));
        Assert.That(shaderText, Does.Contain("CustomEditor \"Nova.Editor.Core.Scripts.ParticlesUberUnlitGUI\""));
    }

    [Test]
    public void UberUnlitRepresentativeKeywordStatesHaveNoBlockingCompilerMessages()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        AssetDatabase.ImportAsset(ShaderPath, ImportAssetOptions.ForceUpdate);

        foreach (var material in CreateRepresentativeMaterials(shader))
        {
            try
            {
                SetupUberUnlit(material);
                Shader.WarmupAllShaders();

                var blockingMessages = ShaderUtil.GetShaderMessages(shader)
                    .Where(IsBlockingShaderMessage)
                    .Select(message => $"{message.severity}: {message.message} ({message.file}:{message.line})")
                    .ToArray();

                Assert.That(blockingMessages, Is.Empty, string.Join(Environment.NewLine, blockingMessages));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }

    [Test]
    public void BuiltInSourceScanExcludesReferenceDirectoryAndContainsNoForbiddenPipelineTokens()
    {
        var scanRoots = new[]
        {
            "Assets/Nova/Runtime",
            "Assets/Nova/Editor",
        };

        var scannedFiles = scanRoots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            .Concat(new[] { "Assets/Nova/package.json", "Packages/manifest.json" })
            .Where(File.Exists)
            .Where(path => !path.EndsWith(".meta", StringComparison.Ordinal))
            .ToArray();

        Assert.That(scannedFiles, Is.Not.Empty);
        Assert.That(scannedFiles.Any(path => path.StartsWith("NovaShader/", StringComparison.Ordinal)), Is.False);

        var violations = scannedFiles
            .SelectMany(path =>
            {
                var text = StripComments(File.ReadAllText(path));
                return ForbiddenTokens
                    .Where(token => text.Contains(token, StringComparison.Ordinal))
                    .Select(token => $"{path}: {token}");
            })
            .ToArray();

        Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
    }

    private static Material[] CreateRepresentativeMaterials(Shader shader)
    {
        var defaultMaterial = new Material(shader);

        var baseMap2D = new Material(shader);
        baseMap2D.SetFloat("_BaseMapMode", 0);

        var alphaTest = new Material(shader);
        alphaTest.SetFloat("_RenderType", 1);
        alphaTest.SetFloat("_Cutoff", 0.5f);

        var alphaModulate = new Material(shader);
        alphaModulate.SetFloat("_RenderType", 2);
        alphaModulate.SetFloat("_TransparentBlendMode", 2);

        var tintColor = new Material(shader);
        tintColor.SetFloat("_TintAreaMode", 1);
        tintColor.SetFloat("_TintColorMode", 0);
        tintColor.SetColor("_TintColor", new Color(0.25f, 1f, 0.25f, 1f));

        var vertexColor = new Material(shader);
        vertexColor.SetFloat("_VertexAlphaMode", 0);

        return new[] { defaultMaterial, baseMap2D, alphaTest, alphaModulate, tintColor, vertexColor };
    }

    private static void SetupUberUnlit(Material material)
    {
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberUnlitMaterialPostProcessor", "SetupMaterialKeywords", material);
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberUnlitMaterialPostProcessor", "SetupMaterialBlendMode", material);
    }

    private static void InvokeStatic(string typeName, string methodName, Material material)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(typeName))
            .FirstOrDefault(found => found != null);
        Assert.That(type, Is.Not.Null, typeName);

        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        Assert.That(method, Is.Not.Null, $"{typeName}.{methodName}");
        method.Invoke(null, new object[] { material });
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
            "missing|include|unknown|macro|unsupported|texture|sampler|invalid|undeclared|variant|compile",
            RegexOptions.IgnoreCase);
    }

    private static string StripComments(string text)
    {
        text = Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        return Regex.Replace(text, @"//.*", string.Empty);
    }
}

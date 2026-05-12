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
                var ownedBaseMap = material.GetTexture("_BaseMap");
                if (ownedBaseMap != null && ownedBaseMap.name == "NovaBuiltInCompileTexture")
                {
                    UnityEngine.Object.DestroyImmediate(ownedBaseMap);
                }

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

        var tintMap = new Material(shader);
        tintMap.SetFloat("_TintAreaMode", 1);
        tintMap.SetFloat("_TintColorMode", 1);
        tintMap.SetTexture("_TintMap", CreateCompileTexture(FilterMode.Point));

        var tintMap2DArray = new Material(shader);
        tintMap2DArray.SetFloat("_TintAreaMode", 1);
        tintMap2DArray.SetFloat("_TintColorMode", 3);

        var tintMap3D = new Material(shader);
        tintMap3D.SetFloat("_TintAreaMode", 1);
        tintMap3D.SetFloat("_TintColorMode", 2);

        var tintMapRandomRow = new Material(shader);
        tintMapRandomRow.SetFloat("_TintAreaMode", 1);
        tintMapRandomRow.SetFloat("_TintColorMode", 3);
        tintMapRandomRow.SetFloat("_TintMapRandomRowSelectionEnabled", 1);
        tintMapRandomRow.SetFloat("_TintMapRandomRowCoord", 1);
        tintMapRandomRow.SetFloat("_TintMapRowCount", 2);

        var tintRim = new Material(shader);
        tintRim.SetFloat("_TintAreaMode", 2);
        tintRim.SetFloat("_TintColorMode", 0);
        tintRim.SetColor("_TintColor", new Color(0.25f, 1f, 0.25f, 1f));

        var flowBase = new Material(shader);
        flowBase.SetTexture("_FlowMap", CreateCompileTexture(FilterMode.Point));
        flowBase.SetFloat("_FlowMapTarget", 1);

        var flowTint = new Material(shader);
        flowTint.SetFloat("_TintAreaMode", 1);
        flowTint.SetFloat("_TintColorMode", 1);
        flowTint.SetTexture("_TintMap", CreateCompileTexture(FilterMode.Point));
        flowTint.SetTexture("_FlowMap", CreateCompileTexture(FilterMode.Point));
        flowTint.SetFloat("_FlowMapTarget", 2);

        var parallax2DBase = new Material(shader);
        parallax2DBase.SetTexture("_ParallaxMap", CreateCompileTexture(FilterMode.Point));
        parallax2DBase.SetFloat("_ParallaxMapMode", 0);
        parallax2DBase.SetFloat("_ParallaxMapTarget", 1);

        var parallax2DArrayTint = new Material(shader);
        parallax2DArrayTint.SetFloat("_ParallaxMapMode", 1);
        parallax2DArrayTint.SetFloat("_ParallaxMapTarget", 2);

        var parallax3D = new Material(shader);
        parallax3D.SetFloat("_ParallaxMapMode", 2);
        parallax3D.SetFloat("_ParallaxMapTarget", 1);

        var greyscale = new Material(shader);
        greyscale.SetFloat("_ColorCorrectionMode", 1);

        var gradientMap = new Material(shader);
        gradientMap.SetFloat("_ColorCorrectionMode", 2);
        gradientMap.SetTexture("_GradientMap", CreateCompileTexture(FilterMode.Bilinear));

        var vertexColor = new Material(shader);
        vertexColor.SetFloat("_VertexAlphaMode", 0);

        var baseMap2DArray = new Material(shader);
        baseMap2DArray.SetFloat("_BaseMapMode", 1);

        var baseMap3D = new Material(shader);
        baseMap3D.SetFloat("_BaseMapMode", 2);

        var channel = new Material(shader);
        channel.SetFloat("_BaseMapChannel", 2);

        var randomRow = new Material(shader);
        randomRow.SetFloat("_BaseMapMode", 1);
        randomRow.SetFloat("_BaseMapRandomRowSelectionEnabled", 1);
        randomRow.SetFloat("_BaseMapRandomRowCoord", 1);
        randomRow.SetFloat("_BaseMapRowCount", 2);

        var tritone = new Material(shader);
        tritone.SetFloat("_BaseMapToneMode", 1);

        var pentone = new Material(shader);
        pentone.SetFloat("_BaseMapToneMode", 2);

        var pointMirror = new Material(shader);
        pointMirror.SetFloat("_BaseMapMirrorSampling", 1);
        pointMirror.SetTexture("_BaseMap", CreateCompileTexture(FilterMode.Point));

        var linearMirror = new Material(shader);
        linearMirror.SetFloat("_BaseMapMirrorSampling", 1);
        linearMirror.SetTexture("_BaseMap", CreateCompileTexture(FilterMode.Bilinear));

        var trilinearMirror = new Material(shader);
        trilinearMirror.SetFloat("_BaseMapMirrorSampling", 1);
        trilinearMirror.SetTexture("_BaseMap", CreateCompileTexture(FilterMode.Trilinear));

        return new[]
        {
            defaultMaterial,
            baseMap2D,
            alphaTest,
            alphaModulate,
            tintColor,
            tintMap,
            tintMap2DArray,
            tintMap3D,
            tintMapRandomRow,
            tintRim,
            flowBase,
            flowTint,
            parallax2DBase,
            parallax2DArrayTint,
            parallax3D,
            greyscale,
            gradientMap,
            vertexColor,
            baseMap2DArray,
            baseMap3D,
            channel,
            randomRow,
            tritone,
            pentone,
            pointMirror,
            linearMirror,
            trilinearMirror
        };
    }

    private static Texture2D CreateCompileTexture(FilterMode filterMode)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.name = "NovaBuiltInCompileTexture";
        texture.filterMode = filterMode;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return texture;
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

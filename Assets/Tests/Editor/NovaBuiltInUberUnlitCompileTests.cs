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
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInDepth.cginc"));
        Assert.That(shaderText, Does.Contain("NovaObjectToClipPos"));
        Assert.That(shaderText, Does.Contain("NovaComputeScreenPos"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX2D(_BaseMap"));
        Assert.That(shaderText, Does.Contain("NovaSoftParticleFactor"));
        Assert.That(shaderText, Does.Contain("NovaDepthFadeFactor"));
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

        var fadeTransition = new Material(shader);
        fadeTransition.SetFloat("_AlphaTransitionMode", 1);
        fadeTransition.SetFloat("_AlphaTransitionMapMode", 0);
        fadeTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var dissolveTransition = new Material(shader);
        dissolveTransition.SetFloat("_AlphaTransitionMode", 2);
        dissolveTransition.SetFloat("_AlphaTransitionMapMode", 0);
        dissolveTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var alphaTransition2DArray = new Material(shader);
        alphaTransition2DArray.SetFloat("_AlphaTransitionMode", 2);
        alphaTransition2DArray.SetFloat("_AlphaTransitionMapMode", 1);
        alphaTransition2DArray.SetFloat("_AlphaTransitionMapSliceCount", 2);

        var alphaTransition3D = new Material(shader);
        alphaTransition3D.SetFloat("_AlphaTransitionMode", 2);
        alphaTransition3D.SetFloat("_AlphaTransitionMapMode", 2);
        alphaTransition3D.SetFloat("_AlphaTransitionMapSliceCount", 2);

        var alphaTransitionRandomRow = new Material(shader);
        alphaTransitionRandomRow.SetFloat("_AlphaTransitionMode", 2);
        alphaTransitionRandomRow.SetFloat("_AlphaTransitionMapMode", 1);
        alphaTransitionRandomRow.SetFloat("_AlphaTransitionMapRandomRowSelectionEnabled", 1);
        alphaTransitionRandomRow.SetFloat("_AlphaTransitionMapRandomRowCoord", 1);
        alphaTransitionRandomRow.SetFloat("_AlphaTransitionMapRowCount", 2);

        var secondAlphaTransitionAverage = new Material(shader);
        secondAlphaTransitionAverage.SetFloat("_AlphaTransitionMode", 2);
        secondAlphaTransitionAverage.SetFloat("_AlphaTransitionMapMode", 0);
        secondAlphaTransitionAverage.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        secondAlphaTransitionAverage.SetFloat("_AlphaTransitionSecondTextureBlendMode", 1);
        secondAlphaTransitionAverage.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 0);
        secondAlphaTransitionAverage.SetTexture("_AlphaTransitionMapSecondTexture", CreateCompileTexture(FilterMode.Point));

        var secondAlphaTransitionMultiply = new Material(shader);
        secondAlphaTransitionMultiply.SetFloat("_AlphaTransitionMode", 2);
        secondAlphaTransitionMultiply.SetFloat("_AlphaTransitionMapMode", 0);
        secondAlphaTransitionMultiply.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        secondAlphaTransitionMultiply.SetFloat("_AlphaTransitionSecondTextureBlendMode", 2);
        secondAlphaTransitionMultiply.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 0);
        secondAlphaTransitionMultiply.SetTexture("_AlphaTransitionMapSecondTexture", CreateCompileTexture(FilterMode.Point));

        var secondAlphaTransition2DArray = new Material(shader);
        secondAlphaTransition2DArray.SetFloat("_AlphaTransitionMode", 2);
        secondAlphaTransition2DArray.SetFloat("_AlphaTransitionMapMode", 0);
        secondAlphaTransition2DArray.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        secondAlphaTransition2DArray.SetFloat("_AlphaTransitionSecondTextureBlendMode", 1);
        secondAlphaTransition2DArray.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 1);
        secondAlphaTransition2DArray.SetFloat("_AlphaTransitionMapSecondTextureSliceCount", 2);

        var secondAlphaTransition3D = new Material(shader);
        secondAlphaTransition3D.SetFloat("_AlphaTransitionMode", 2);
        secondAlphaTransition3D.SetFloat("_AlphaTransitionMapMode", 0);
        secondAlphaTransition3D.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        secondAlphaTransition3D.SetFloat("_AlphaTransitionSecondTextureBlendMode", 1);
        secondAlphaTransition3D.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 2);
        secondAlphaTransition3D.SetFloat("_AlphaTransitionMapSecondTextureSliceCount", 2);

        var secondAlphaTransitionRandomRow = new Material(shader);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMode", 2);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMapMode", 0);
        secondAlphaTransitionRandomRow.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionSecondTextureBlendMode", 1);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 1);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMapSecondTextureRandomRowSelectionEnabled", 1);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMapSecondTextureRandomRowCoord", 1);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMapSecondTextureRowCount", 2);

        var vertexAlphaTransition = new Material(shader);
        vertexAlphaTransition.SetFloat("_VertexAlphaMode", 1);
        vertexAlphaTransition.SetFloat("_AlphaTransitionMode", 1);
        vertexAlphaTransition.SetFloat("_AlphaTransitionMapMode", 0);
        vertexAlphaTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var flowAlphaTransition = new Material(shader);
        flowAlphaTransition.SetTexture("_FlowMap", CreateCompileTexture(FilterMode.Point));
        flowAlphaTransition.SetFloat("_FlowMapTarget", 4);
        flowAlphaTransition.SetFloat("_AlphaTransitionMode", 2);
        flowAlphaTransition.SetFloat("_AlphaTransitionMapMode", 0);
        flowAlphaTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var emissionAreaAll = new Material(shader);
        emissionAreaAll.SetFloat("_EmissionAreaType", 1);
        emissionAreaAll.SetFloat("_EmissionColorType", 0);
        emissionAreaAll.SetColor("_EmissionColor", Color.green);

        var emissionAreaAlpha = new Material(shader);
        emissionAreaAlpha.SetFloat("_EmissionAreaType", 3);
        emissionAreaAlpha.SetFloat("_EmissionColorType", 0);
        emissionAreaAlpha.SetColor("_EmissionColor", Color.green);

        var emissionAreaMap = new Material(shader);
        emissionAreaMap.SetFloat("_EmissionAreaType", 2);
        emissionAreaMap.SetFloat("_EmissionColorType", 0);
        emissionAreaMap.SetFloat("_EmissionMapMode", 0);
        emissionAreaMap.SetTexture("_EmissionMap", CreateCompileTexture(FilterMode.Point));

        var emissionColorBase = new Material(shader);
        emissionColorBase.SetFloat("_EmissionAreaType", 1);
        emissionColorBase.SetFloat("_EmissionColorType", 1);

        var emissionColorRamp = new Material(shader);
        emissionColorRamp.SetFloat("_EmissionAreaType", 2);
        emissionColorRamp.SetFloat("_EmissionColorType", 2);
        emissionColorRamp.SetFloat("_EmissionMapMode", 0);
        emissionColorRamp.SetTexture("_EmissionMap", CreateCompileTexture(FilterMode.Point));
        emissionColorRamp.SetTexture("_EmissionColorRamp", CreateCompileTexture(FilterMode.Bilinear));

        var emissionMap2DArray = new Material(shader);
        emissionMap2DArray.SetFloat("_EmissionAreaType", 2);
        emissionMap2DArray.SetFloat("_EmissionColorType", 0);
        emissionMap2DArray.SetFloat("_EmissionMapMode", 1);
        emissionMap2DArray.SetFloat("_EmissionMapSliceCount", 2);

        var emissionMap3D = new Material(shader);
        emissionMap3D.SetFloat("_EmissionAreaType", 2);
        emissionMap3D.SetFloat("_EmissionColorType", 0);
        emissionMap3D.SetFloat("_EmissionMapMode", 2);
        emissionMap3D.SetFloat("_EmissionMapSliceCount", 2);

        var emissionMapRandomRow = new Material(shader);
        emissionMapRandomRow.SetFloat("_EmissionAreaType", 2);
        emissionMapRandomRow.SetFloat("_EmissionColorType", 0);
        emissionMapRandomRow.SetFloat("_EmissionMapMode", 1);
        emissionMapRandomRow.SetFloat("_EmissionMapRandomRowSelectionEnabled", 1);
        emissionMapRandomRow.SetFloat("_EmissionMapRandomRowCoord", 1);
        emissionMapRandomRow.SetFloat("_EmissionMapRowCount", 2);

        var flowEmission = new Material(shader);
        flowEmission.SetTexture("_FlowMap", CreateCompileTexture(FilterMode.Point));
        flowEmission.SetFloat("_FlowMapTarget", 8);
        flowEmission.SetFloat("_EmissionAreaType", 2);
        flowEmission.SetFloat("_EmissionColorType", 0);
        flowEmission.SetFloat("_EmissionMapMode", 0);
        flowEmission.SetTexture("_EmissionMap", CreateCompileTexture(FilterMode.Point));

        var rimTransparency = new Material(shader);
        rimTransparency.SetFloat("_RimTransparencyEnabled", 1);

        var luminanceTransparency = new Material(shader);
        luminanceTransparency.SetFloat("_LuminanceTransparencyEnabled", 1);

        var combinedTransparency = new Material(shader);
        combinedTransparency.SetFloat("_RimTransparencyEnabled", 1);
        combinedTransparency.SetFloat("_LuminanceTransparencyEnabled", 1);

        var softParticles = new Material(shader);
        softParticles.SetFloat("_RenderType", 2);
        softParticles.SetFloat("_SoftParticlesEnabled", 1);
        softParticles.SetFloat("_SoftParticlesIntensity", 1);

        var softParticlesAlphaTest = new Material(shader);
        softParticlesAlphaTest.SetFloat("_RenderType", 1);
        softParticlesAlphaTest.SetFloat("_SoftParticlesEnabled", 1);
        softParticlesAlphaTest.SetFloat("_SoftParticlesIntensity", 1);
        softParticlesAlphaTest.SetFloat("_Cutoff", 0.5f);

        var softParticlesAlphaTransition = new Material(shader);
        softParticlesAlphaTransition.SetFloat("_RenderType", 2);
        softParticlesAlphaTransition.SetFloat("_SoftParticlesEnabled", 1);
        softParticlesAlphaTransition.SetFloat("_SoftParticlesIntensity", 1);
        softParticlesAlphaTransition.SetFloat("_AlphaTransitionMode", 1);
        softParticlesAlphaTransition.SetFloat("_AlphaTransitionMapMode", 0);
        softParticlesAlphaTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var softParticlesEmission = new Material(shader);
        softParticlesEmission.SetFloat("_RenderType", 2);
        softParticlesEmission.SetFloat("_SoftParticlesEnabled", 1);
        softParticlesEmission.SetFloat("_SoftParticlesIntensity", 1);
        softParticlesEmission.SetFloat("_EmissionAreaType", 1);
        softParticlesEmission.SetFloat("_EmissionColorType", 0);
        softParticlesEmission.SetColor("_EmissionColor", Color.green);

        var softParticlesTransparency = new Material(shader);
        softParticlesTransparency.SetFloat("_RenderType", 2);
        softParticlesTransparency.SetFloat("_SoftParticlesEnabled", 1);
        softParticlesTransparency.SetFloat("_SoftParticlesIntensity", 1);
        softParticlesTransparency.SetFloat("_RimTransparencyEnabled", 1);

        var depthFade = new Material(shader);
        depthFade.SetFloat("_RenderType", 2);
        depthFade.SetFloat("_DepthFadeEnabled", 1);
        depthFade.SetFloat("_DepthFadeNear", 1.5f);
        depthFade.SetFloat("_DepthFadeFar", 3f);
        depthFade.SetFloat("_DepthFadeWidth", 0.5f);

        var depthFadeAlphaTransition = new Material(shader);
        depthFadeAlphaTransition.SetFloat("_RenderType", 2);
        depthFadeAlphaTransition.SetFloat("_DepthFadeEnabled", 1);
        depthFadeAlphaTransition.SetFloat("_DepthFadeNear", 1.5f);
        depthFadeAlphaTransition.SetFloat("_DepthFadeFar", 3f);
        depthFadeAlphaTransition.SetFloat("_DepthFadeWidth", 0.5f);
        depthFadeAlphaTransition.SetFloat("_AlphaTransitionMode", 1);
        depthFadeAlphaTransition.SetFloat("_AlphaTransitionMapMode", 0);
        depthFadeAlphaTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var depthFadeEmission = new Material(shader);
        depthFadeEmission.SetFloat("_RenderType", 2);
        depthFadeEmission.SetFloat("_DepthFadeEnabled", 1);
        depthFadeEmission.SetFloat("_DepthFadeNear", 1.5f);
        depthFadeEmission.SetFloat("_DepthFadeFar", 3f);
        depthFadeEmission.SetFloat("_DepthFadeWidth", 0.5f);
        depthFadeEmission.SetFloat("_EmissionAreaType", 1);
        depthFadeEmission.SetFloat("_EmissionColorType", 0);
        depthFadeEmission.SetColor("_EmissionColor", Color.green);

        var depthFadeTransparency = new Material(shader);
        depthFadeTransparency.SetFloat("_RenderType", 2);
        depthFadeTransparency.SetFloat("_DepthFadeEnabled", 1);
        depthFadeTransparency.SetFloat("_DepthFadeNear", 1.5f);
        depthFadeTransparency.SetFloat("_DepthFadeFar", 3f);
        depthFadeTransparency.SetFloat("_DepthFadeWidth", 0.5f);
        depthFadeTransparency.SetFloat("_LuminanceTransparencyEnabled", 1);

        var combinedDepthFeatures = new Material(shader);
        combinedDepthFeatures.SetFloat("_RenderType", 2);
        combinedDepthFeatures.SetFloat("_SoftParticlesEnabled", 1);
        combinedDepthFeatures.SetFloat("_SoftParticlesIntensity", 1);
        combinedDepthFeatures.SetFloat("_DepthFadeEnabled", 1);
        combinedDepthFeatures.SetFloat("_DepthFadeNear", 1.5f);
        combinedDepthFeatures.SetFloat("_DepthFadeFar", 3f);
        combinedDepthFeatures.SetFloat("_DepthFadeWidth", 0.5f);

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
            fadeTransition,
            dissolveTransition,
            alphaTransition2DArray,
            alphaTransition3D,
            alphaTransitionRandomRow,
            secondAlphaTransitionAverage,
            secondAlphaTransitionMultiply,
            secondAlphaTransition2DArray,
            secondAlphaTransition3D,
            secondAlphaTransitionRandomRow,
            vertexAlphaTransition,
            flowAlphaTransition,
            emissionAreaAll,
            emissionAreaAlpha,
            emissionAreaMap,
            emissionColorBase,
            emissionColorRamp,
            emissionMap2DArray,
            emissionMap3D,
            emissionMapRandomRow,
            flowEmission,
            rimTransparency,
            luminanceTransparency,
            combinedTransparency,
            softParticles,
            softParticlesAlphaTest,
            softParticlesAlphaTransition,
            softParticlesEmission,
            softParticlesTransparency,
            depthFade,
            depthFadeAlphaTransition,
            depthFadeEmission,
            depthFadeTransparency,
            combinedDepthFeatures,
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

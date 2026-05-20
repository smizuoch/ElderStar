using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberLitCompileTests
{
    private const string ShaderName = "Nova/Particles/UberLit";
    private const string ShaderPath = "Assets/Nova/Runtime/Core/Shaders/ParticlesUberLit.shader";
    private const string MatrixPath = "Documentation/BuiltInCompatibilityMatrix.md";

    private static readonly string[] LitProperties =
    {
        "_LitWorkflowMode",
        "_LitReceiveShadows",
        "_SpecularHighlights",
        "_EnvironmentReflections",
        "_NormalMap",
        "_NormalMap2DArray",
        "_NormalMap3D",
        "_NormalMapBumpScale",
        "_SpecularMap",
        "_SpecularMap2DArray",
        "_SpecularMap3D",
        "_SpecularColor",
        "_SpecularMapChannelsX",
        "_MetallicMap",
        "_MetallicMap2DArray",
        "_MetallicMap3D",
        "_Metallic",
        "_MetallicMapChannelsX",
        "_SmoothnessMap",
        "_SmoothnessMap2DArray",
        "_SmoothnessMap3D",
        "_Smoothness",
        "_SmoothnessMapChannelsX",
    };

    private static readonly string[] LitKeywords =
    {
        "EVALUATE_SH_MIXED",
        "EVALUATE_SH_VERTEX",
        "_ADDITIONAL_LIGHTS",
        "_ADDITIONAL_LIGHTS_VERTEX",
        "_ADDITIONAL_LIGHT_SHADOWS",
        "_CLUSTER_LIGHT_LOOP",
        "_ENVIRONMENT_REFLECTIONS_ENABLED",
        "_FORWARD_PLUS",
        "_MAIN_LIGHT_SHADOWS",
        "_MAIN_LIGHT_SHADOWS_CASCADE",
        "_METALLIC_MAP_ENABLED",
        "_NORMAL_MAP_ENABLED",
        "_RECEIVE_SHADOWS_ENABLED",
        "_SHADOWS_SOFT",
        "_SMOOTHNESS_MAP_ENABLED",
        "_SPECULAR_HIGHLIGHTS_ENABLED",
        "_SPECULAR_MAP_ENABLED",
        "_SPECULAR_SETUP",
    };

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
    public void UberLitLitPropertiesAndRepresentativeTypesArePreserved()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        try
        {
            var missing = LitProperties.Where(property => !material.HasProperty(property)).ToArray();
            Assert.That(missing, Is.Empty, string.Join(", ", missing));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(material);
        }

        AssertShaderPropertyTypes(
            shader,
            FloatLike("_LitWorkflowMode"),
            FloatLike("_LitReceiveShadows"),
            FloatLike("_SpecularHighlights"),
            FloatLike("_EnvironmentReflections"),
            Texture("_NormalMap"),
            Texture("_SpecularMap"),
            Color("_SpecularColor"),
            Texture("_MetallicMap"),
            FloatLike("_Metallic"),
            Texture("_SmoothnessMap"),
            FloatLike("_Smoothness"));
    }

    [Test]
    public void UberLitKeywordCompatibilitySurfaceIsDeclared()
    {
        var shaderText = File.ReadAllText(ShaderPath);
        var declarations = string.Join("\n", shaderText
            .Split('\n')
            .Where(line => line.Contains("#pragma", StringComparison.Ordinal) && line.Contains("shader_feature", StringComparison.Ordinal)));

        var missing = LitKeywords.Where(keyword => !declarations.Contains(keyword, StringComparison.Ordinal)).ToArray();
        Assert.That(missing, Is.Empty, string.Join(", ", missing));
    }

    [Test]
    public void UberLitMinimumForwardBasePassUsesBuiltInLitHelpersOnly()
    {
        var shaderText = File.ReadAllText(ShaderPath);
        var helperText = File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInUberLit.cginc");
        var lightingText = File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInLighting.cginc");
        var combinedText = shaderText + "\n" + helperText + "\n" + lightingText;

        Assert.That(shaderText, Does.Contain("\"LightMode\" = \"ForwardBase\""));
        Assert.That(shaderText, Does.Contain("Blend [_BlendSrc] [_BlendDst]"));
        Assert.That(shaderText, Does.Contain("ZWrite [_ZWrite]"));
        Assert.That(shaderText, Does.Contain("Cull [_Cull]"));
        Assert.That(shaderText, Does.Contain("ZTest [_ZTest]"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInCore.cginc"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInTexture.cginc"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInLighting.cginc"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInUberLit.cginc"));
        Assert.That(shaderText, Does.Contain("NovaObjectToClipPos"));
        Assert.That(shaderText, Does.Contain("NovaObjectToWorldNormal"));
        Assert.That(combinedText, Does.Contain("NOVA_SAMPLE_TEX2D(baseMap"));
        Assert.That(combinedText, Does.Contain("NovaComputeMinimumLitColor"));
        Assert.That(combinedText, Does.Contain("_TintColor"));
        Assert.That(combinedText, Does.Contain("_ALPHATEST_ENABLED"));
        Assert.That(combinedText, Does.Contain("clip("));
        Assert.That(shaderText, Does.Contain("CustomEditor \"Nova.Editor.Core.Scripts.ParticlesUberLitGUI\""));
    }

    [Test]
    public void UberLitPhase61PassTopologyIsForwardBaseOnlyAndDocumentsPendingPasses()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        try
        {
            var shaderText = File.ReadAllText(ShaderPath);
            var lightModes = Regex.Matches(shaderText, @"""LightMode""\s*=\s*""([^""]+)""")
                .Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .ToArray();
            var matrixText = File.ReadAllText(MatrixPath);

            Assert.That(lightModes, Is.EquivalentTo(new[] { "ForwardBase" }));
            Assert.That(material.FindPass("ForwardBase"), Is.GreaterThanOrEqualTo(0));
            Assert.That(material.FindPass("ShadowCaster"), Is.LessThan(0));
            Assert.That(material.FindPass("DepthOnly"), Is.LessThan(0));
            Assert.That(material.FindPass("SceneSelectionPass"), Is.LessThan(0));
            Assert.That(material.FindPass("Picking"), Is.LessThan(0));
            Assert.That(shaderText, Does.Not.Contain("UniversalForward"));
            Assert.That(shaderText, Does.Not.Contain("RenderPipeline\" = \"UniversalPipeline\""));
            Assert.That(matrixText, Does.Contain("Phase 6.1"));
            Assert.That(matrixText, Does.Contain("Lit ShadowCaster"));
            Assert.That(matrixText, Does.Contain("Lit DepthOnly"));
            Assert.That(matrixText, Does.Contain("Lit SceneSelectionPass"));
            Assert.That(matrixText, Does.Contain("Lit Picking"));
            Assert.That(matrixText, Does.Contain("not yet implemented"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(material);
        }
    }

    [Test]
    public void UberLitRepresentativeKeywordStatesHaveNoBlockingCompilerMessages()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        AssetDatabase.ImportAsset(ShaderPath, ImportAssetOptions.ForceUpdate);

        foreach (var compileCase in CreateCompileCases(shader))
        {
            TestContext.Out.WriteLine(compileCase.Name);
            try
            {
                SetupUberLit(compileCase.Material);
                foreach (var keyword in compileCase.AdditionalKeywords)
                {
                    compileCase.Material.EnableKeyword(keyword);
                }

                Shader.WarmupAllShaders();

                var blockingMessages = ShaderUtil.GetShaderMessages(shader)
                    .Where(IsBlockingShaderMessage)
                    .Select(message => $"{compileCase.Name}: {message.severity}: {message.message} ({message.file}:{message.line})")
                    .ToArray();

                Assert.That(blockingMessages, Is.Empty, string.Join(Environment.NewLine, blockingMessages));
            }
            finally
            {
                DestroyMaterialAndOwnedTextures(compileCase.Material);
            }
        }
    }

    [Test]
    public void Phase61DoesNotPromoteUiLitDistortionOrOptimizerRuntime()
    {
        var uiLitText = File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/UIParticlesUberLit.shader");
        var distortionText = File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/ParticlesDistortion.shader");
        var matrixText = File.ReadAllText(MatrixPath);

        Assert.That(uiLitText, Does.Not.Contain("NovaBuiltInUberLit.cginc"));
        Assert.That(uiLitText, Does.Not.Contain("NovaComputeMinimumLitColor"));
        Assert.That(distortionText, Does.Not.Contain("DistortedUvBuffer"));
        Assert.That(matrixText, Does.Contain("UI Lit remains Pending"));
        Assert.That(matrixText, Does.Contain("Distortion runtime remains Pending"));
        Assert.That(matrixText, Does.Contain("optimizer/replacer runtime behavior remains Pending"));
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

    private static CompileCase[] CreateCompileCases(Shader shader)
    {
        return new[]
        {
            new CompileCase("default Lit material", new Material(shader)),
            new CompileCase("_ALPHATEST_ENABLED", WithFloat(shader, "_RenderType", 1)),
            new CompileCase("_SPECULAR_SETUP", WithFloat(shader, "_LitWorkflowMode", 0)),
            new CompileCase("_NORMAL_MAP_ENABLED", WithTexture(shader, "_NormalMap")),
            new CompileCase("_SPECULAR_MAP_ENABLED", WithTexture(shader, "_SpecularMap")),
            new CompileCase("_METALLIC_MAP_ENABLED", WithTexture(shader, "_MetallicMap")),
            new CompileCase("_SMOOTHNESS_MAP_ENABLED", WithTexture(shader, "_SmoothnessMap")),
            new CompileCase("_RECEIVE_SHADOWS_ENABLED", WithFloat(shader, "_LitReceiveShadows", 1)),
            new CompileCase("_SPECULAR_HIGHLIGHTS_ENABLED", WithFloat(shader, "_SpecularHighlights", 1)),
            new CompileCase("_ENVIRONMENT_REFLECTIONS_ENABLED", WithFloat(shader, "_EnvironmentReflections", 1)),
            new CompileCase("_MAIN_LIGHT_SHADOWS", new Material(shader), "_MAIN_LIGHT_SHADOWS"),
            new CompileCase("_MAIN_LIGHT_SHADOWS_CASCADE", new Material(shader), "_MAIN_LIGHT_SHADOWS_CASCADE"),
            new CompileCase("_SHADOWS_SOFT", new Material(shader), "_SHADOWS_SOFT"),
            new CompileCase("_ADDITIONAL_LIGHTS", new Material(shader), "_ADDITIONAL_LIGHTS"),
            new CompileCase("_ADDITIONAL_LIGHTS_VERTEX", new Material(shader), "_ADDITIONAL_LIGHTS_VERTEX"),
            new CompileCase("_ADDITIONAL_LIGHT_SHADOWS", new Material(shader), "_ADDITIONAL_LIGHT_SHADOWS"),
            new CompileCase("_FORWARD_PLUS", new Material(shader), "_FORWARD_PLUS"),
            new CompileCase("_CLUSTER_LIGHT_LOOP", new Material(shader), "_CLUSTER_LIGHT_LOOP"),
            new CompileCase(
                "representative Lit keyword combination",
                WithTexture(WithTexture(WithFloat(WithFloat(shader, "_RenderType", 1), "_SpecularHighlights", 1), "_NormalMap"), "_SmoothnessMap"),
                "_MAIN_LIGHT_SHADOWS",
                "_SHADOWS_SOFT"),
        };
    }

    private static Material WithFloat(Shader shader, string property, float value)
    {
        return WithFloat(new Material(shader), property, value);
    }

    private static Material WithFloat(Material material, string property, float value)
    {
        material.SetFloat(property, value);
        return material;
    }

    private static Material WithTexture(Shader shader, string property)
    {
        return WithTexture(new Material(shader), property);
    }

    private static Material WithTexture(Material material, string property)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUberLitCompileTexture",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(Enumerable.Repeat(UnityEngine.Color.white, 4).ToArray());
        texture.Apply();
        material.SetTexture(property, texture);
        return material;
    }

    private static void AssertShaderPropertyTypes(Shader shader, params ExpectedPropertyType[] expected)
    {
        foreach (var property in expected)
        {
            var index = shader.FindPropertyIndex(property.Name);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), property.Name);
            var actual = shader.GetPropertyType(index);
            if (property.Type == ShaderPropertyType.Float)
            {
                Assert.That(actual, Is.EqualTo(ShaderPropertyType.Float).Or.EqualTo(ShaderPropertyType.Range), property.Name);
            }
            else
            {
                Assert.That(actual, Is.EqualTo(property.Type), property.Name);
            }
        }
    }

    private static ExpectedPropertyType Texture(string name)
    {
        return new ExpectedPropertyType(name, ShaderPropertyType.Texture);
    }

    private static ExpectedPropertyType Color(string name)
    {
        return new ExpectedPropertyType(name, ShaderPropertyType.Color);
    }

    private static ExpectedPropertyType FloatLike(string name)
    {
        return new ExpectedPropertyType(name, ShaderPropertyType.Float);
    }

    private static void SetupUberLit(Material material)
    {
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor", "SetupMaterialKeywords", material);
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor", "SetupMaterialBlendMode", material);
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

        var text = message.message ?? string.Empty;
        return text.Contains("undeclared identifier", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Couldn't open include file", StringComparison.OrdinalIgnoreCase)
            || text.Contains("invalid subscript", StringComparison.OrdinalIgnoreCase)
            || text.Contains("failed to compile", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Shader error", StringComparison.OrdinalIgnoreCase);
    }

    private static void DestroyMaterialAndOwnedTextures(Material material)
    {
        var textures = new[]
            {
                "_NormalMap",
                "_NormalMap2DArray",
                "_NormalMap3D",
                "_SpecularMap",
                "_SpecularMap2DArray",
                "_SpecularMap3D",
                "_MetallicMap",
                "_MetallicMap2DArray",
                "_MetallicMap3D",
                "_SmoothnessMap",
                "_SmoothnessMap2DArray",
                "_SmoothnessMap3D",
            }
            .Where(material.HasProperty)
            .Select(material.GetTexture)
            .Where(texture => texture != null && texture.name == "NovaBuiltInUberLitCompileTexture")
            .Distinct()
            .ToArray();

        foreach (var texture in textures)
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }

        UnityEngine.Object.DestroyImmediate(material);
    }

    private static string StripComments(string text)
    {
        text = Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        text = Regex.Replace(text, @"//.*", string.Empty);
        return text;
    }

    private readonly struct ExpectedPropertyType
    {
        public ExpectedPropertyType(string name, ShaderPropertyType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public ShaderPropertyType Type { get; }
    }

    private sealed class CompileCase
    {
        public CompileCase(string name, Material material, params string[] additionalKeywords)
        {
            Name = name;
            Material = material;
            AdditionalKeywords = additionalKeywords;
        }

        public string Name { get; }
        public Material Material { get; }
        public string[] AdditionalKeywords { get; }
    }
}

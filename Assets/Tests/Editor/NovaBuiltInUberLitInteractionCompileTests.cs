using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberLitInteractionCompileTests
{
    private const string ShaderName = "Nova/Particles/UberLit";
    private const string ShaderPath = "Assets/Nova/Runtime/Core/Shaders/ParticlesUberLit.shader";
    private const string MatrixPath = "Documentation/BuiltInVerificationHistory.md";

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
    public void UberLitComplexInteractionKeywordStatesHaveNoBlockingCompilerMessages()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        AssetDatabase.ImportAsset(ShaderPath, ImportAssetOptions.ForceUpdate);
        AssertCompileCasesHaveNoBlockingMessages(new[]
        {
            new CompileCase(
                "normal 2D array + metallic 2D array + smoothness 2D array",
                WithTexture2DArray(
                    WithTexture2DArray(
                        WithTexture2DArray(WithBaseMapMode(shader, 1f), "_NormalMap2DArray"),
                        "_MetallicMap2DArray"),
                    "_SmoothnessMap2DArray")),
            new CompileCase(
                "normal 3D + specular 3D + smoothness 3D",
                WithTexture3D(
                    WithTexture3D(
                        WithTexture3D(WithBaseMapMode(WithFloat(shader, "_LitWorkflowMode", 0f), 2f), "_NormalMap3D"),
                        "_SpecularMap3D"),
                    "_SmoothnessMap3D")),
            new CompileCase(
                "specular workflow + environment reflections + normal map",
                WithTexture(WithFloat(WithFloat(shader, "_LitWorkflowMode", 0f), "_EnvironmentReflections", 1f), "_NormalMap")),
            new CompileCase(
                "metallic workflow + environment reflections + receive shadows",
                WithFloat(WithFloat(WithFloat(shader, "_LitWorkflowMode", 1f), "_EnvironmentReflections", 1f), "_LitReceiveShadows", 1f),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "ForwardAdd + metallic + smoothness + normal map",
                WithTexture(WithTexture(WithTexture(shader, "_MetallicMap"), "_SmoothnessMap"), "_NormalMap"),
                "_ADDITIONAL_LIGHTS"),
            new CompileCase(
                "ForwardAdd + specular workflow + environment reflections",
                WithFloat(WithFloat(WithFloat(shader, "_LitWorkflowMode", 0f), "_EnvironmentReflections", 1f), "_SpecularHighlights", 1f),
                "_ADDITIONAL_LIGHTS"),
            new CompileCase(
                "receive shadows + alpha test + normal map",
                WithTexture(WithFloat(WithFloat(shader, "_LitReceiveShadows", 1f), "_RenderType", 1f), "_NormalMap"),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "ShadowCaster + alpha test + tint shadow influence",
                WithFloat(
                    WithFloat(
                        WithFloat(
                            WithFloat(shader, "_ShadowCasterEnabled", 1f),
                            "_ShadowCasterAlphaTestEnabled",
                            1f),
                        "_ShadowCasterAlphaAffectedByTintColor",
                        1f),
                    "_TintAreaMode",
                    1f)),
            new CompileCase(
                "ShadowCaster + flow + alpha transition + luminance shadow alpha influence",
                WithTexture(
                    WithTexture(
                        WithFloat(
                            WithFloat(
                                WithFloat(
                                    WithFloat(
                                        WithFloat(shader, "_ShadowCasterEnabled", 1f),
                                        "_ShadowCasterAlphaTestEnabled",
                                        1f),
                                    "_ShadowCasterAlphaAffectedByFlowMap",
                                    1f),
                                "_ShadowCasterAlphaAffectedByAlphaTransitionMap",
                                1f),
                            "_ShadowCasterAlphaAffectedByTransparencyLuminance",
                            1f),
                        "_FlowMap"),
                    "_AlphaTransitionMap"),
                "_FLOW_MAP_TARGET_BASE",
                "_DISSOLVE_TRANSITION_ENABLED",
                "_TRANSPARENCY_BY_LUMINANCE"),
            new CompileCase(
                "DepthOnly + alpha test + representative lit surface keywords",
                WithTexture(
                    WithTexture(
                        WithFloat(WithFloat(shader, "_RenderType", 1f), "_LitWorkflowMode", 0f),
                        "_NormalMap"),
                    "_SmoothnessMap")),
            new CompileCase(
                "SceneSelectionPass/Picking + representative lit keywords",
                WithTexture(WithTexture(WithFloat(shader, "_LitWorkflowMode", 0f), "_NormalMap"), "_SmoothnessMap"),
                "_SPECULAR_SETUP"),
            new CompileCase(
                "major implemented Lit features except mutually exclusive texture modes",
                WithTexture(
                    WithTexture(
                        WithTexture(
                            WithFloat(
                                WithFloat(
                                    WithFloat(
                                        WithFloat(
                                            WithFloat(shader, "_LitWorkflowMode", 0f),
                                            "_EnvironmentReflections",
                                            1f),
                                        "_LitReceiveShadows",
                                        1f),
                                    "_SpecularHighlights",
                                    1f),
                                "_RenderType",
                                1f),
                            "_NormalMap"),
                        "_SpecularMap"),
                    "_SmoothnessMap"),
                "_ADDITIONAL_LIGHTS",
                "_MAIN_LIGHT_SHADOWS",
                "_SHADOWS_SOFT"),
            new CompileCase("compatibility-only _FORWARD_PLUS", new Material(shader), "_FORWARD_PLUS"),
            new CompileCase("compatibility-only _CLUSTER_LIGHT_LOOP", new Material(shader), "_CLUSTER_LIGHT_LOOP"),
            new CompileCase("additional-light vertex compile state", new Material(shader), "_ADDITIONAL_LIGHTS_VERTEX"),
            new CompileCase("additional-light shadow compile state", new Material(shader), "_ADDITIONAL_LIGHTS", "_ADDITIONAL_LIGHT_SHADOWS"),
        });
    }

    [Test]
    public void UberLitPhase66APassTopologyAndUnsupportedItemsAreDocumented()
    {
        var shaderText = File.ReadAllText(ShaderPath);
        var matrixText = File.ReadAllText(MatrixPath);
        var lightModes = Regex.Matches(shaderText, @"""LightMode""\s*=\s*""([^""]+)""")
            .Cast<Match>()
            .Select(match => match.Groups[1].Value)
            .ToArray();

        Assert.That(lightModes, Is.EquivalentTo(new[] { "ForwardBase", "ForwardAdd", "ShadowCaster", "DepthOnly", "SceneSelectionPass", "Picking" }));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"DepthNormals\""));
        Assert.That(shaderText, Does.Not.Contain("UniversalForward"));
        Assert.That(shaderText, Does.Not.Contain("RenderPipeline\" = \"UniversalPipeline\""));
        Assert.That(shaderText, Does.Not.Contain("Packages/com.unity.render-pipelines.universal"));

        foreach (var required in new[]
                 {
                     "Phase 6.6A UberLit Consolidation Status",
                     "Implemented and smoke-tested",
                     "Implemented but approximate",
                     "Preserved but not fully parity-tested",
                     "Unsupported or intentionally not implemented",
                     "no Built-in DepthNormals implementation",
                     "no URP DepthNormals prepass parity",
                     "no full URP PBR parity",
                     "no Forward+ runtime",
                     "no cluster light loop runtime",
                     "no probe volume runtime",
                     "no foveated rendering runtime",
                     "no golden visual parity",
                     "golden image baselines not added",
                 })
        {
            Assert.That(matrixText, Does.Contain(required), required);
        }
    }

    [Test]
    public void UberLitMaterialSetupStillRepresentsImplementedLitFeatureKeywords()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        try
        {
            material.SetFloat("_LitWorkflowMode", 0f);
            material.SetFloat("_LitReceiveShadows", 1f);
            material.SetFloat("_SpecularHighlights", 1f);
            material.SetFloat("_EnvironmentReflections", 1f);
            material.SetFloat("_BaseMapMode", 0f);
            material.SetTexture("_NormalMap", CreateTexture2D(Color.white));
            material.SetTexture("_SpecularMap", CreateTexture2D(Color.white));
            material.SetTexture("_MetallicMap", CreateTexture2D(Color.white));
            material.SetTexture("_SmoothnessMap", CreateTexture2D(Color.white));

            SetupUberLit(material);

            Assert.That(material.IsKeywordEnabled("_SPECULAR_SETUP"), Is.True);
            Assert.That(material.IsKeywordEnabled("_RECEIVE_SHADOWS_ENABLED"), Is.True);
            Assert.That(material.IsKeywordEnabled("_SPECULAR_HIGHLIGHTS_ENABLED"), Is.True);
            Assert.That(material.IsKeywordEnabled("_ENVIRONMENT_REFLECTIONS_ENABLED"), Is.True);
            Assert.That(material.IsKeywordEnabled("_NORMAL_MAP_ENABLED"), Is.True);
            Assert.That(material.IsKeywordEnabled("_SPECULAR_MAP_ENABLED"), Is.True);
            Assert.That(material.IsKeywordEnabled("_METALLIC_MAP_ENABLED"), Is.True);
            Assert.That(material.IsKeywordEnabled("_SMOOTHNESS_MAP_ENABLED"), Is.True);

            material.SetFloat("_LitWorkflowMode", 1f);
            material.SetFloat("_LitReceiveShadows", 0f);
            material.SetFloat("_EnvironmentReflections", 0f);
            SetupUberLit(material);

            Assert.That(material.IsKeywordEnabled("_SPECULAR_SETUP"), Is.False);
            Assert.That(material.IsKeywordEnabled("_RECEIVE_SHADOWS_ENABLED"), Is.False);
            Assert.That(material.IsKeywordEnabled("_ENVIRONMENT_REFLECTIONS_ENABLED"), Is.False);
        }
        finally
        {
            DestroyMaterialAndOwnedTextures(material);
        }
    }

    [Test]
    public void Phase66AScopeGuardStillDoesNotPromoteUiLitFullParityDistortionOrOptimizerRuntime()
    {
        var uiLitText = File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/UIParticlesUberLit.shader");
        var distortionText = File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/ParticlesDistortion.shader");
        var editorText = File.ReadAllText("Assets/Nova/Editor/Core/Scripts/OptimizedShaderGenerator.cs");
        var matrixText = File.ReadAllText(MatrixPath);

        Assert.That(uiLitText, Does.Not.Contain("NovaComputeLitForwardBaseColor"));
        Assert.That(uiLitText, Does.Contain("\"LightMode\" = \"ForwardAdd\""));
        Assert.That(uiLitText, Does.Not.Contain("\"LightMode\" = \"ShadowCaster\""));
        Assert.That(distortionText, Does.Contain("DistortedUvBuffer"));
        Assert.That(editorText, Does.Contain("StripOptionalPasses"));
        Assert.That(matrixText, Does.Contain("Phase 7.1 UI UberLit Minimum Forward Status"));
        Assert.That(matrixText, Does.Contain("Distortion runtime remains Pending"));
        Assert.That(matrixText, Does.Contain("optimizer/replacer runtime behavior remains Pending"));
    }

    [Test]
    public void BuiltInSourceScanForPhase66AExcludesReferenceDirectoryAndContainsNoForbiddenPipelineTokens()
    {
        var scannedFiles = new[]
            {
                "Assets/Nova/Runtime",
                "Assets/Nova/Editor",
            }
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

    private static void AssertCompileCasesHaveNoBlockingMessages(CompileCase[] compileCases)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        foreach (var compileCase in compileCases)
        {
            TestContext.Out.WriteLine(compileCase.Name);
            try
            {
                SetupUberLit(compileCase.Material);
                foreach (var keyword in compileCase.AdditionalKeywords)
                {
                    compileCase.Material.EnableKeyword(keyword);
                }

                for (var pass = 0; pass < compileCase.Material.passCount; pass++)
                {
                    ShaderUtil.CompilePass(compileCase.Material, pass, true);
                    Assert.That(ShaderUtil.IsPassCompiled(compileCase.Material, pass), Is.True, "Pass " + pass);
                }

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
        var texture = CreateTexture2D(Color.white);
        texture.name = "NovaBuiltInUberLitInteractionCompileTexture";
        material.SetTexture(property, texture);
        return material;
    }

    private static Material WithTexture2DArray(Material material, string property)
    {
        var texture = new Texture2DArray(2, 2, 2, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUberLitInteractionCompileTexture",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        var pixels = Enumerable.Repeat(Color.white, 4).ToArray();
        texture.SetPixels(pixels, 0);
        texture.SetPixels(pixels, 1);
        texture.Apply();
        material.SetTexture(property, texture);
        return material;
    }

    private static Material WithTexture3D(Material material, string property)
    {
        var texture = new Texture3D(2, 2, 2, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUberLitInteractionCompileTexture",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(Enumerable.Repeat(Color.white, 8).ToArray());
        texture.Apply();
        material.SetTexture(property, texture);
        return material;
    }

    private static Material WithBaseMapMode(Shader shader, float mode)
    {
        return WithBaseMapMode(new Material(shader), mode);
    }

    private static Material WithBaseMapMode(Material material, float mode)
    {
        material.SetFloat("_BaseMapMode", mode);
        material.SetFloat("_BaseMapSliceCount", 2f);
        material.SetFloat("_BaseMapProgress", mode > 1.5f ? 1f : 0.75f);
        return material;
    }

    private static Texture2D CreateTexture2D(Color color)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(Enumerable.Repeat(color, 4).ToArray());
        texture.Apply();
        return texture;
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
            || text.Contains("Shader error", StringComparison.OrdinalIgnoreCase)
            || text.Contains("UniversalFragmentPBR", StringComparison.OrdinalIgnoreCase)
            || text.Contains("ForwardAdd", StringComparison.OrdinalIgnoreCase);
    }

    private static void DestroyMaterialAndOwnedTextures(Material material)
    {
        var properties = new[]
        {
            "_BaseMap",
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
            "_FlowMap",
            "_AlphaTransitionMap",
        };

        foreach (var texture in properties
                     .Where(material.HasProperty)
                     .Select(material.GetTexture)
                     .Where(texture => texture != null)
                     .Distinct())
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

    private sealed class CompileCase
    {
        public CompileCase(string name, Material material, params string[] additionalKeywords)
        {
            Name = name;
            Material = material;
            AdditionalKeywords = additionalKeywords ?? Array.Empty<string>();
        }

        public string Name { get; }
        public Material Material { get; }
        public string[] AdditionalKeywords { get; }
    }
}

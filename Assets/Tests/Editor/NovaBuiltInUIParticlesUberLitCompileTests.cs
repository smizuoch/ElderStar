using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUIParticlesUberLitCompileTests
{
    private const string ShaderName = "Nova/UIParticles/UberLit";
    private const string ShaderPath = "Assets/Nova/Runtime/Core/Shaders/UIParticlesUberLit.shader";
    private const string MatrixPath = "Documentation/BuiltInVerificationHistory.md";

    [Test]
    public void CompatibilityKeywordsUseMaterialLocalFragmentVariantsLikeParticleLit()
    {
        var source = File.ReadAllText(ShaderPath);
        foreach (var keyword in new[] { "_MAIN_LIGHT_SHADOWS", "_MAIN_LIGHT_SHADOWS_CASCADE", "_SHADOWS_SOFT",
            "_ADDITIONAL_LIGHTS", "_ADDITIONAL_LIGHTS_VERTEX", "_ADDITIONAL_LIGHT_SHADOWS", "_FORWARD_PLUS", "_CLUSTER_LIGHT_LOOP" })
        {
            Assert.That(Regex.Matches(source, @"(?m)^\s*#pragma shader_feature_local_fragment " + keyword + @"\s*$").Count,
                Is.EqualTo(2), keyword + " must remain declared in both passes without multiplying unused URP/vertex variants.");
        }
    }

    [TestCase("Nova/Particles/UberLit", "SPOT", "SHADOWS_DEPTH")]
    [TestCase("Nova/UIParticles/UberLit", "SPOT", "SHADOWS_DEPTH")]
    [TestCase("Nova/Particles/UberLit", "POINT_COOKIE", "SHADOWS_CUBE")]
    [TestCase("Nova/UIParticles/UberLit", "POINT_COOKIE", "SHADOWS_CUBE")]
    public void StereoShadowedAdditionalLightsFitVertexOutputBudget(string shaderName, string light, string shadows)
    {
        var material = new Material(Shader.Find(shaderName)) { enableInstancing = true };
        try
        {
            foreach (var keyword in new[] { light, shadows, "STEREO_INSTANCING_ON", "FOG_EXP2",
                "_BASE_MAP_MODE_2D", "_ALPHA_TRANSITION_MAP_MODE_2D", "_EMISSION_MAP_MODE_2D", "_PARALLAX_MAP_MODE_2D" })
                material.EnableKeyword(keyword);
            ShaderUtil.CompilePass(material, material.FindPass("ForwardAdd"), true);
            Assert.That(ShaderUtil.GetShaderMessages(material.shader)
                .Where(m => m.severity.ToString() == "Error").Select(m => m.message), Is.Empty);
        }
        finally { UnityEngine.Object.DestroyImmediate(material); }
    }

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

    private static readonly string[] UiProperties =
    {
        "_StencilComp",
        "_Stencil",
        "_StencilOp",
        "_StencilWriteMask",
        "_StencilReadMask",
        "_ColorMask",
    };

    private static readonly string[] CompileCompatibilityKeywords =
    {
        "UNITY_UI_CLIP_RECT",
        "_SPECULAR_SETUP",
        "_NORMAL_MAP_ENABLED",
        "_SPECULAR_MAP_ENABLED",
        "_METALLIC_MAP_ENABLED",
        "_SMOOTHNESS_MAP_ENABLED",
        "_RECEIVE_SHADOWS_ENABLED",
        "_SPECULAR_HIGHLIGHTS_ENABLED",
        "_ENVIRONMENT_REFLECTIONS_ENABLED",
        "_MAIN_LIGHT_SHADOWS",
        "_MAIN_LIGHT_SHADOWS_CASCADE",
        "_SHADOWS_SOFT",
        "_ADDITIONAL_LIGHTS",
        "_ADDITIONAL_LIGHTS_VERTEX",
        "_ADDITIONAL_LIGHT_SHADOWS",
        "_FORWARD_PLUS",
        "_CLUSTER_LIGHT_LOOP",
    };

    private static readonly string[] ForbiddenUiKeywords =
    {
        "_CASTING_PUNCTUAL_LIGHT_SHADOW",
        "_SHADOW_CASTER_ALPHA_TEST_ENABLED",
        "EVALUATE_SH_MIXED",
        "EVALUATE_SH_VERTEX",
    };

    [Test]
    public void UIParticlesUberLitPropertiesAndRepresentativeTypesArePreserved()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        try
        {
            var missing = LitProperties.Concat(UiProperties)
                .Where(property => !material.HasProperty(property))
                .ToArray();
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
            Texture("_NormalMap2DArray"),
            Texture("_NormalMap3D"),
            Texture("_SpecularMap"),
            Texture("_SpecularMap2DArray"),
            Texture("_SpecularMap3D"),
            Color("_SpecularColor"),
            Texture("_MetallicMap"),
            Texture("_MetallicMap2DArray"),
            Texture("_MetallicMap3D"),
            FloatLike("_Metallic"),
            Texture("_SmoothnessMap"),
            Texture("_SmoothnessMap2DArray"),
            Texture("_SmoothnessMap3D"),
            FloatLike("_Smoothness"),
            FloatLike("_StencilComp"),
            FloatLike("_Stencil"),
            FloatLike("_ColorMask"));
    }

    [Test]
    public void UIParticlesUberLitMinimumForwardPassUsesBuiltInUiLitHelpersOnly()
    {
        var shaderText = File.ReadAllText(ShaderPath) + File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInParticleFeatures.cginc");
        var combinedText = shaderText + "\n"
            + File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInLighting.cginc")
            + "\n"
            + File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInUIParticlesUberLit.cginc");

        Assert.That(shaderText, Does.Contain("\"LightMode\" = \"ForwardBase\""));
        Assert.That(shaderText, Does.Contain("Blend [_BlendSrc] [_BlendDst]"));
        Assert.That(shaderText, Does.Contain("ZWrite [_ZWrite]"));
        Assert.That(shaderText, Does.Contain("Cull [_Cull]"));
        Assert.That(shaderText, Does.Contain("ZTest [_ZTest]"));
        Assert.That(shaderText, Does.Contain("ColorMask [_ColorMask]"));
        Assert.That(shaderText, Does.Contain("NovaBuiltInCore.cginc"));
        Assert.That(shaderText, Does.Contain("NovaBuiltInTexture.cginc"));
        Assert.That(shaderText, Does.Contain("NovaBuiltInLighting.cginc"));
        Assert.That(shaderText, Does.Contain("NovaBuiltInUI.cginc"));
        Assert.That(shaderText, Does.Contain("NovaObjectToClipPos"));
        Assert.That(shaderText, Does.Contain("NovaObjectToWorldNormal"));
        Assert.That(combinedText, Does.Contain("tex2D(baseMap"));
        Assert.That(shaderText, Does.Contain("_TintColor"));
        Assert.That(shaderText, Does.Contain("i.color"));
        Assert.That(combinedText, Does.Contain("NovaUIClipAlpha"));
        Assert.That(combinedText, Does.Contain("NovaComputeMinimumLitColor"));
        Assert.That(combinedText, Does.Contain("NovaComputeAmbientApproximation"));
        Assert.That(shaderText, Does.Contain("NovaBuiltInShadow.cginc"));
        Assert.That(shaderText, Does.Contain("NovaTransferShadowCoord"));
        Assert.That(shaderText, Does.Contain("NovaComputeShadowAttenuation"));
        Assert.That(combinedText, Does.Contain("NovaUILitReceiveShadowsEnabled"));
        Assert.That(combinedText, Does.Contain("NovaApplyReceiveShadow"));
        Assert.That(combinedText, Does.Contain("ShadeSH9"));
        Assert.That(shaderText, Does.Contain("_ALPHATEST_ENABLED"));
        Assert.That(shaderText, Does.Contain("clip("));
        Assert.That(shaderText, Does.Contain("CustomEditor \"Nova.Editor.Core.Scripts.UIParticlesUberLitGUI\""));
    }

    [Test]
    public void UIParticlesUberLitDeclaresUiStencilAndColorMaskState()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Contain("Stencil"));
        Assert.That(shaderText, Does.Contain("Ref [_Stencil]"));
        Assert.That(shaderText, Does.Contain("Comp [_StencilComp]"));
        Assert.That(shaderText, Does.Contain("Pass [_StencilOp]"));
        Assert.That(shaderText, Does.Contain("ReadMask [_StencilReadMask]"));
        Assert.That(shaderText, Does.Contain("WriteMask [_StencilWriteMask]"));
        Assert.That(shaderText, Does.Contain("ColorMask [_ColorMask]"));
    }

    [Test]
    public void UIParticlesUberLitPassTopologySupportsBuiltInForwardLighting()
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

            Assert.That(lightModes, Is.EquivalentTo(new[] { "ForwardBase", "ForwardAdd" }));
            Assert.That(material.FindPass("ForwardBase"), Is.GreaterThanOrEqualTo(0));
            Assert.That(material.FindPass("ForwardAdd"), Is.GreaterThanOrEqualTo(0));
            Assert.That(material.FindPass("ShadowCaster"), Is.LessThan(0));
            Assert.That(material.FindPass("DepthOnly"), Is.LessThan(0));
            Assert.That(material.FindPass("DepthNormals"), Is.LessThan(0));
            Assert.That(material.FindPass("SceneSelectionPass"), Is.LessThan(0));
            Assert.That(material.FindPass("Picking"), Is.LessThan(0));
            Assert.That(shaderText, Does.Not.Contain("UniversalForward"));
            Assert.That(shaderText, Does.Not.Contain("RenderPipeline\" = \"UniversalPipeline\""));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(material);
        }
    }

    [Test]
    public void UIParticlesUberLitKeywordSurfaceIncludesLitCompatibilityAndUiExclusions()
    {
        var declarations = DeclaredKeywords();

        var missing = CompileCompatibilityKeywords
            .Where(keyword => !declarations.Contains(keyword))
            .ToArray();
        var forbidden = ForbiddenUiKeywords
            .Where(keyword => declarations.Contains(keyword))
            .ToArray();

        Assert.That(missing, Is.Empty, string.Join(", ", missing));
        Assert.That(forbidden, Is.Empty, string.Join(", ", forbidden));
    }

    [Test]
    public void UIParticlesUberLitMaterialSetupPreservesRenderLitAndUiSurface()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        try
        {
            material.SetFloat("_StencilComp", 3);
            material.SetFloat("_Stencil", 7);
            material.SetFloat("_StencilOp", 2);
            material.SetFloat("_StencilWriteMask", 127);
            material.SetFloat("_StencilReadMask", 63);
            material.SetFloat("_ColorMask", 11);
            material.EnableKeyword("UNITY_UI_CLIP_RECT");
            material.SetFloat("_RenderType", 2);
            material.SetFloat("_TransparentBlendMode", 1);
            material.SetFloat("_Cull", (float)CullMode.Front);
            material.SetFloat("_QueueOffset", 5);
            material.SetFloat("_BlendSrc", (float)BlendMode.One);
            material.SetFloat("_BlendDst", (float)BlendMode.Zero);
            material.SetFloat("_ZWriteOverride", -1);
            material.SetFloat("_ZTest", (float)CompareFunction.Always);
            material.SetFloat("_LitWorkflowMode", 0);
            material.SetFloat("_LitReceiveShadows", 1);
            material.SetFloat("_SpecularHighlights", 1);
            material.SetFloat("_EnvironmentReflections", 1);

            SetupUberLit(material);

            Assert.That(material.renderQueue, Is.EqualTo((int)RenderQueue.Transparent + 5));
            Assert.That(material.GetTag("RenderType", false), Is.EqualTo("Transparent"));
            Assert.That(material.GetFloat("_BlendSrc"), Is.EqualTo((float)BlendMode.SrcAlpha));
            Assert.That(material.GetFloat("_BlendDst"), Is.EqualTo((float)BlendMode.One));
            Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(0));
            Assert.That(material.GetFloat("_ZTest"), Is.EqualTo((float)CompareFunction.Always));
            Assert.That(material.GetFloat("_StencilComp"), Is.EqualTo(3));
            Assert.That(material.GetFloat("_Stencil"), Is.EqualTo(7));
            Assert.That(material.GetFloat("_StencilOp"), Is.EqualTo(2));
            Assert.That(material.GetFloat("_StencilWriteMask"), Is.EqualTo(127));
            Assert.That(material.GetFloat("_StencilReadMask"), Is.EqualTo(63));
            Assert.That(material.GetFloat("_ColorMask"), Is.EqualTo(11));
            Assert.That(material.IsKeywordEnabled("UNITY_UI_CLIP_RECT"), Is.True);
            Assert.That(material.IsKeywordEnabled("_SPECULAR_SETUP"), Is.True);
            Assert.That(material.IsKeywordEnabled("_RECEIVE_SHADOWS_ENABLED"), Is.True);
            Assert.That(material.IsKeywordEnabled("_SPECULAR_HIGHLIGHTS_ENABLED"), Is.True);
            Assert.That(material.IsKeywordEnabled("_ENVIRONMENT_REFLECTIONS_ENABLED"), Is.True);
            Assert.That(material.IsKeywordEnabled("_SHADOW_CASTER_ALPHA_TEST_ENABLED"), Is.False);
            Assert.That(material.GetShaderPassEnabled("ShadowCaster"), Is.False);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(material);
        }
    }

    [Test]
    public void UIParticlesUberLitRepresentativeKeywordStatesHaveNoBlockingCompilerMessages()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        AssetDatabase.ImportAsset(ShaderPath, ImportAssetOptions.ForceUpdate);
        AssertCompileCasesHaveNoBlockingMessages(CreateCompileCases(shader));
    }

    [Test]
    public void UIParticlesUberLitSurfaceApproximationUsesUiScopedHelpers()
    {
        var shaderText = File.ReadAllText(ShaderPath) + File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInParticleFeatures.cginc");
        var helperPath = "Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInUIParticlesUberLit.cginc";
        Assert.That(File.Exists(helperPath), Is.True, helperPath);

        var helperText = File.ReadAllText(helperPath);
        var combinedText = shaderText + "\n" + helperText;

        Assert.That(shaderText, Does.Contain("NovaBuiltInUIParticlesUberLit.cginc"));
        Assert.That(shaderText, Does.Contain("NovaResolveUILitSurfaceData"));
        Assert.That(shaderText, Does.Contain("NovaComputeUILitForwardBaseColor"));
        Assert.That(combinedText, Does.Contain("NovaUIClipAlpha"));
        Assert.That(shaderText, Does.Contain("float4 tangent : TANGENT"));
        Assert.That(combinedText, Does.Contain("NovaUILitSurfaceData"));
        Assert.That(combinedText, Does.Contain("NovaResolveUILitMetallic"));
        Assert.That(combinedText, Does.Contain("NovaResolveUILitSpecularColor"));
        Assert.That(combinedText, Does.Contain("NovaResolveUILitSmoothness"));
        Assert.That(combinedText, Does.Contain("NovaUnpackUILitNormalMap2D"));
        Assert.That(combinedText, Does.Contain("NovaComputeUILitSpecularApproximation"));
        Assert.That(combinedText, Does.Contain("_NormalMapBumpScale"));
        Assert.That(combinedText, Does.Not.Contain("UniversalFragmentPBR"));
        Assert.That(shaderText, Does.Contain("#define NOVA_UI_LIT 1"));
        Assert.That(combinedText, Does.Contain("NovaSampleEnvironmentReflection"));
    }

    [Test]
    public void UIParticlesUberLitDoesNotIncludeUrpShaderLibrariesOrNonUiPassKeywords()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Not.Contain("Packages/com.unity.render-pipelines.universal"));
        Assert.That(shaderText, Does.Not.Contain("Core.hlsl"));
        Assert.That(shaderText, Does.Not.Contain("Lighting.hlsl"));
        Assert.That(shaderText, Does.Not.Contain("Shadows.hlsl"));
        Assert.That(shaderText, Does.Not.Contain("ProbeVolumeVariants.hlsl"));
        Assert.That(shaderText, Does.Not.Contain("FoveatedRenderingKeywords.hlsl"));
        Assert.That(shaderText, Does.Not.Contain("_CASTING_PUNCTUAL_LIGHT_SHADOW"));
        Assert.That(shaderText, Does.Not.Contain("_SHADOW_CASTER_ALPHA_TEST_ENABLED"));
        Assert.That(shaderText, Does.Not.Contain("EVALUATE_SH_MIXED"));
        Assert.That(shaderText, Does.Not.Contain("EVALUATE_SH_VERTEX"));
    }

    [Test]
    public void UIParticlesUberLitPhase72AStatusIsDocumented()
    {
        var matrixText = File.ReadAllText(MatrixPath);

        Assert.That(matrixText, Does.Contain("Phase 7.2A UI UberLit Surface Approximation Status"));
        Assert.That(matrixText, Does.Contain("UI metallic workflow"));
        Assert.That(matrixText, Does.Contain("UI specular workflow"));
        Assert.That(matrixText, Does.Contain("UI normal map 2D"));
        Assert.That(matrixText, Does.Contain("UI smoothness map 2D"));
        Assert.That(matrixText, Does.Contain("basic specular approximation"));
        Assert.That(matrixText, Does.Contain("full Canvas `Mask` / `RectMask2D` parity"));
        Assert.That(matrixText, Does.Contain("2D array / 3D normal/specular/metallic/smoothness maps"));
        Assert.That(matrixText, Does.Contain("Forward+"));
    }

    [Test]
    public void UIParticlesUberLitPhase72BStatusIsDocumented()
    {
        var matrixText = File.ReadAllText(MatrixPath);

        Assert.That(matrixText, Does.Contain("Phase 7.2B UI UberLit Texture Mode Status"));
        Assert.That(matrixText, Does.Contain("UI normal map 2D array"));
        Assert.That(matrixText, Does.Contain("UI normal map 3D"));
        Assert.That(matrixText, Does.Contain("UI metallic map 2D array"));
        Assert.That(matrixText, Does.Contain("UI metallic map 3D"));
        Assert.That(matrixText, Does.Contain("UI specular map 2D array"));
        Assert.That(matrixText, Does.Contain("UI specular map 3D"));
        Assert.That(matrixText, Does.Contain("UI smoothness map 2D array"));
        Assert.That(matrixText, Does.Contain("UI smoothness map 3D"));
        Assert.That(matrixText, Does.Contain("Phase 7.3A"));
    }

    [Test]
    public void UIParticlesUberLitPhase73AReceiveShadowStatusIsDocumented()
    {
        var matrixText = File.ReadAllText(MatrixPath);

        Assert.That(matrixText, Does.Contain("Phase 7.3A UI UberLit Receive Shadow Status"));
        Assert.That(matrixText, Does.Contain("UI receive shadows runtime"));
        Assert.That(matrixText, Does.Contain("_LitReceiveShadows"));
        Assert.That(matrixText, Does.Contain("_RECEIVE_SHADOWS_ENABLED"));
        Assert.That(matrixText, Does.Contain("Built-in `AutoLight.cginc`"));
        Assert.That(matrixText, Does.Contain("Ambient remains unshadowed"));
        Assert.That(matrixText, Does.Contain("UI Lit still has no `ForwardAdd`, `ShadowCaster`, `DepthOnly`, `DepthNormals`, `SceneSelectionPass`, or `Picking`"));
        Assert.That(matrixText, Does.Contain("UI environment reflections"));
        Assert.That(matrixText, Does.Contain("The next recommended phase is Phase 7.3B: `Nova/UIParticles/UberLit` environment reflections / reflection probe approximation."));
    }

    private static CompileCase[] CreateCompileCases(Shader shader)
    {
        return new[]
        {
            new CompileCase("default UI Lit material", new Material(shader)),
            new CompileCase("base map 2D mode", WithFloat(shader, "_BaseMapMode", 0)),
            new CompileCase("_ALPHATEST_ENABLED", WithFloat(shader, "_RenderType", 1)),
            new CompileCase("tint color", WithFloat(WithFloat(shader, "_TintAreaMode", 1), "_TintColorMode", 0)),
            new CompileCase("UNITY_UI_CLIP_RECT", WithClipRect(new Material(shader))),
            new CompileCase("stencil property source check", WithStencil(new Material(shader))),
            new CompileCase("_SPECULAR_SETUP", WithFloat(shader, "_LitWorkflowMode", 0)),
            new CompileCase("_NORMAL_MAP_ENABLED", WithTexture(shader, "_NormalMap")),
            new CompileCase("_SPECULAR_MAP_ENABLED", WithTexture(WithFloat(shader, "_LitWorkflowMode", 0), "_SpecularMap")),
            new CompileCase("_METALLIC_MAP_ENABLED", WithTexture(shader, "_MetallicMap")),
            new CompileCase("_SMOOTHNESS_MAP_ENABLED", WithTexture(shader, "_SmoothnessMap")),
            new CompileCase("UI normal map 2D array", WithTexture2DArray(WithLitTextureMode(shader, 1, 0.75f, 2), "_NormalMap2DArray")),
            new CompileCase("UI normal map 3D", WithTexture3D(WithLitTextureMode(shader, 2, 1, 2), "_NormalMap3D")),
            new CompileCase("UI metallic map 2D array", WithTexture2DArray(WithLitTextureMode(shader, 1, 0.75f, 2), "_MetallicMap2DArray")),
            new CompileCase("UI metallic map 3D", WithTexture3D(WithLitTextureMode(shader, 2, 1, 2), "_MetallicMap3D")),
            new CompileCase("UI specular map 2D array", WithTexture2DArray(WithFloat(WithLitTextureMode(shader, 1, 0.75f, 2), "_LitWorkflowMode", 0), "_SpecularMap2DArray")),
            new CompileCase("UI specular map 3D", WithTexture3D(WithFloat(WithLitTextureMode(shader, 2, 1, 2), "_LitWorkflowMode", 0), "_SpecularMap3D")),
            new CompileCase("UI smoothness map 2D array", WithTexture2DArray(WithLitTextureMode(shader, 1, 0.75f, 2), "_SmoothnessMap2DArray")),
            new CompileCase("UI smoothness map 3D", WithTexture3D(WithLitTextureMode(shader, 2, 1, 2), "_SmoothnessMap3D")),
            new CompileCase("_RECEIVE_SHADOWS_ENABLED", WithFloat(shader, "_LitReceiveShadows", 1)),
            new CompileCase("_SPECULAR_HIGHLIGHTS_ENABLED", WithFloat(shader, "_SpecularHighlights", 1)),
            new CompileCase("_ENVIRONMENT_REFLECTIONS_ENABLED", WithFloat(shader, "_EnvironmentReflections", 1)),
            new CompileCase("_MAIN_LIGHT_SHADOWS", new Material(shader), "_MAIN_LIGHT_SHADOWS"),
            new CompileCase("_MAIN_LIGHT_SHADOWS_CASCADE", new Material(shader), "_MAIN_LIGHT_SHADOWS_CASCADE"),
            new CompileCase("_SHADOWS_SOFT", new Material(shader), "_SHADOWS_SOFT"),
            new CompileCase(
                "_RECEIVE_SHADOWS_ENABLED + _MAIN_LIGHT_SHADOWS",
                WithFloat(shader, "_LitReceiveShadows", 1),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "_RECEIVE_SHADOWS_ENABLED + _MAIN_LIGHT_SHADOWS_CASCADE",
                WithFloat(shader, "_LitReceiveShadows", 1),
                "_MAIN_LIGHT_SHADOWS_CASCADE"),
            new CompileCase(
                "_RECEIVE_SHADOWS_ENABLED + _SHADOWS_SOFT",
                WithFloat(shader, "_LitReceiveShadows", 1),
                "_SHADOWS_SOFT"),
            new CompileCase(
                "receive shadows + metallic workflow",
                WithFloat(WithFloat(shader, "_LitReceiveShadows", 1), "_LitWorkflowMode", 1),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "receive shadows + specular workflow",
                WithFloat(WithFloat(shader, "_LitReceiveShadows", 1), "_LitWorkflowMode", 0),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "receive shadows + normal map",
                WithTexture(WithFloat(shader, "_LitReceiveShadows", 1), "_NormalMap"),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "receive shadows + smoothness map",
                WithTexture(WithFloat(shader, "_LitReceiveShadows", 1), "_SmoothnessMap"),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "receive shadows + alpha test",
                WithFloat(WithFloat(shader, "_LitReceiveShadows", 1), "_RenderType", 1),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "receive shadows + UI clip rect",
                WithClipRect(WithFloat(shader, "_LitReceiveShadows", 1)),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "receive shadows + UI Lit 2D array maps",
                WithTexture2DArray(WithTexture2DArray(WithFloat(WithLitTextureMode(shader, 1, 0.75f, 2), "_LitReceiveShadows", 1), "_NormalMap2DArray"), "_MetallicMap2DArray"),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "receive shadows + UI Lit 3D maps",
                WithTexture3D(WithTexture3D(WithFloat(WithLitTextureMode(shader, 2, 1, 2), "_LitReceiveShadows", 1), "_NormalMap3D"), "_SmoothnessMap3D"),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase(
                "receive shadows + stencil properties",
                WithStencil(WithFloat(shader, "_LitReceiveShadows", 1)),
                "_MAIN_LIGHT_SHADOWS"),
            new CompileCase("_ADDITIONAL_LIGHTS", new Material(shader), "_ADDITIONAL_LIGHTS"),
            new CompileCase("_ADDITIONAL_LIGHTS_VERTEX", new Material(shader), "_ADDITIONAL_LIGHTS_VERTEX"),
            new CompileCase("_ADDITIONAL_LIGHT_SHADOWS", new Material(shader), "_ADDITIONAL_LIGHT_SHADOWS"),
            new CompileCase("_FORWARD_PLUS", new Material(shader), "_FORWARD_PLUS"),
            new CompileCase("_CLUSTER_LIGHT_LOOP", new Material(shader), "_CLUSTER_LIGHT_LOOP"),
            new CompileCase("_NORMAL_MAP_ENABLED + _SPECULAR_SETUP", WithTexture(WithFloat(shader, "_LitWorkflowMode", 0), "_NormalMap")),
            new CompileCase("_METALLIC_MAP_ENABLED + _SMOOTHNESS_MAP_ENABLED", WithTexture(WithTexture(WithFloat(shader, "_LitWorkflowMode", 1), "_MetallicMap"), "_SmoothnessMap")),
            new CompileCase("_SPECULAR_MAP_ENABLED + _SMOOTHNESS_MAP_ENABLED", WithTexture(WithTexture(WithFloat(shader, "_LitWorkflowMode", 0), "_SpecularMap"), "_SmoothnessMap")),
            new CompileCase("_NORMAL_MAP_ENABLED + _SMOOTHNESS_MAP_ENABLED", WithTexture(WithTexture(shader, "_NormalMap"), "_SmoothnessMap")),
            new CompileCase(
                "UI normal 2D array + specular workflow",
                WithTexture2DArray(WithFloat(WithLitTextureMode(shader, 1, 0.75f, 2), "_LitWorkflowMode", 0), "_NormalMap2DArray")),
            new CompileCase(
                "UI normal 3D + specular workflow",
                WithTexture3D(WithFloat(WithLitTextureMode(shader, 2, 1, 2), "_LitWorkflowMode", 0), "_NormalMap3D")),
            new CompileCase(
                "UI metallic 2D array + smoothness 2D array",
                WithTexture2DArray(WithTexture2DArray(WithFloat(WithLitTextureMode(shader, 1, 0.75f, 2), "_LitWorkflowMode", 1), "_MetallicMap2DArray"), "_SmoothnessMap2DArray")),
            new CompileCase(
                "UI metallic 3D + smoothness 3D",
                WithTexture3D(WithTexture3D(WithFloat(WithLitTextureMode(shader, 2, 1, 2), "_LitWorkflowMode", 1), "_MetallicMap3D"), "_SmoothnessMap3D")),
            new CompileCase(
                "UI specular 2D array + smoothness 2D array",
                WithTexture2DArray(WithTexture2DArray(WithFloat(WithLitTextureMode(shader, 1, 0.75f, 2), "_LitWorkflowMode", 0), "_SpecularMap2DArray"), "_SmoothnessMap2DArray")),
            new CompileCase(
                "UI specular 3D + smoothness 3D",
                WithTexture3D(WithTexture3D(WithFloat(WithLitTextureMode(shader, 2, 1, 2), "_LitWorkflowMode", 0), "_SpecularMap3D"), "_SmoothnessMap3D")),
            new CompileCase(
                "all Phase 7.2B texture mode keywords",
                WithTexture2DArray(WithTexture2DArray(WithTexture2DArray(WithFloat(WithFloat(WithLitTextureMode(shader, 1, 0.75f, 2), "_LitWorkflowMode", 0), "_SpecularHighlights", 1), "_NormalMap2DArray"), "_SpecularMap2DArray"), "_SmoothnessMap2DArray")),
            new CompileCase(
                "Phase 7.2B with UNITY_UI_CLIP_RECT",
                WithClipRect(WithTexture2DArray(WithTexture2DArray(WithFloat(WithLitTextureMode(shader, 1, 0.75f, 2), "_LitWorkflowMode", 1), "_NormalMap2DArray"), "_MetallicMap2DArray"))),
            new CompileCase(
                "Phase 7.2B with alpha test",
                WithTexture3D(WithFloat(WithFloat(WithLitTextureMode(shader, 2, 1, 2), "_RenderType", 1), "_LitWorkflowMode", 0), "_SpecularMap3D")),
            new CompileCase(
                "Phase 7.2B with stencil properties",
                WithStencil(WithTexture3D(WithTexture3D(WithFloat(WithLitTextureMode(shader, 2, 1, 2), "_LitWorkflowMode", 1), "_NormalMap3D"), "_SmoothnessMap3D"))),
            new CompileCase(
                "all Phase 7.2A surface keywords",
                WithTexture(WithTexture(WithTexture(WithFloat(WithFloat(shader, "_LitWorkflowMode", 0), "_SpecularHighlights", 1), "_NormalMap"), "_SpecularMap"), "_SmoothnessMap")),
            new CompileCase(
                "Phase 7.2A with UNITY_UI_CLIP_RECT",
                WithClipRect(WithTexture(WithTexture(WithFloat(WithFloat(shader, "_LitWorkflowMode", 0), "_SpecularHighlights", 1), "_NormalMap"), "_SmoothnessMap"))),
            new CompileCase(
                "Phase 7.2A with alpha test",
                WithTexture(WithTexture(WithFloat(WithFloat(WithFloat(shader, "_RenderType", 1), "_LitWorkflowMode", 0), "_SpecularHighlights", 1), "_SpecularMap"), "_SmoothnessMap")),
            new CompileCase(
                "Phase 7.2A with stencil properties",
                WithStencil(WithTexture(WithTexture(WithFloat(WithFloat(shader, "_LitWorkflowMode", 1), "_SpecularHighlights", 1), "_MetallicMap"), "_NormalMap"))),
            new CompileCase(
                "representative UI Lit keyword combination",
                WithClipRect(WithTexture(WithFloat(WithFloat(WithFloat(shader, "_RenderType", 1), "_TintAreaMode", 1), "_TintColorMode", 0), "_NormalMap")),
                "_MAIN_LIGHT_SHADOWS",
                "_SHADOWS_SOFT",
                "_ADDITIONAL_LIGHTS",
                "_FORWARD_PLUS"),
        };
    }

    private static HashSet<string> DeclaredKeywords()
    {
        var declarations = new HashSet<string>(StringComparer.Ordinal);
        foreach (var rawLine in File.ReadAllText(ShaderPath).Split('\n'))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith("#pragma", StringComparison.Ordinal) || !line.Contains("shader_feature", StringComparison.Ordinal) && !line.Contains("multi_compile", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var token in line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (token == "_" || token.StartsWith("#", StringComparison.Ordinal) || token.Contains("shader_feature") || token.Contains("multi_compile"))
                {
                    continue;
                }

                declarations.Add(token);
            }
        }

        return declarations;
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
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUIParticlesUberLitCompileTexture",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(Enumerable.Repeat(UnityEngine.Color.white, 4).ToArray());
        texture.Apply();
        material.SetTexture(property, texture);
        return material;
    }

    private static Material WithTexture2DArray(Material material, string property)
    {
        var texture = new Texture2DArray(2, 2, 2, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUIParticlesUberLitCompileTexture",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(Enumerable.Repeat(UnityEngine.Color.white, 4).ToArray(), 0);
        texture.SetPixels(Enumerable.Repeat(UnityEngine.Color.white, 4).ToArray(), 1);
        texture.Apply();
        material.SetTexture(property, texture);
        return material;
    }

    private static Material WithTexture3D(Material material, string property)
    {
        var texture = new Texture3D(2, 2, 2, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUIParticlesUberLitCompileTexture",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(Enumerable.Repeat(UnityEngine.Color.white, 8).ToArray());
        texture.Apply();
        material.SetTexture(property, texture);
        return material;
    }

    private static Material WithLitTextureMode(Shader shader, float baseMapMode, float progress, float sliceCount)
    {
        return WithLitTextureMode(new Material(shader), baseMapMode, progress, sliceCount);
    }

    private static Material WithLitTextureMode(Material material, float baseMapMode, float progress, float sliceCount)
    {
        material.SetFloat("_BaseMapMode", baseMapMode);
        material.SetFloat("_BaseMapProgress", progress);
        material.SetFloat("_BaseMapSliceCount", sliceCount);
        return material;
    }

    private static Material WithClipRect(Material material)
    {
        material.EnableKeyword("UNITY_UI_CLIP_RECT");
        material.SetVector("_ClipRect", new Vector4(-0.25f, -1f, 1f, 1f));
        return material;
    }

    private static Material WithStencil(Material material)
    {
        material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
        material.SetFloat("_Stencil", 1);
        material.SetFloat("_StencilOp", (float)StencilOp.Keep);
        material.SetFloat("_StencilReadMask", 255);
        material.SetFloat("_StencilWriteMask", 255);
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
            || text.Contains("Shader error", StringComparison.OrdinalIgnoreCase)
            || text.Contains("invalid pass", StringComparison.OrdinalIgnoreCase);
    }

    private static void DestroyMaterialAndOwnedTextures(Material material)
    {
        var textures = material.GetTexturePropertyNames()
            .Select(material.GetTexture)
            .Where(texture => texture != null && texture.name == "NovaBuiltInUIParticlesUberLitCompileTexture")
            .Distinct()
            .ToArray();

        foreach (var texture in textures)
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }

        UnityEngine.Object.DestroyImmediate(material);
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

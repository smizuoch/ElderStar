using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class NovaBuiltInUIParticlesUberUnlitInteractionCompileTests
{
    private const string ShaderName = "Nova/UIParticles/UberUnlit";
    private const string ShaderPath = "Assets/Nova/Runtime/Core/Shaders/UIParticlesUberUnlit.shader";
    private const string MatrixPath = "Documentation/BuiltInVerificationHistory.md";

    [Test]
    public void UIParticlesUberUnlitComplexInteractionKeywordSetsHaveNoBlockingCompilerMessages()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        AssetDatabase.ImportAsset(ShaderPath, ImportAssetOptions.ForceUpdate);

        foreach (var compileCase in CreateComplexCompileCases())
        {
            TestContext.Out.WriteLine(compileCase.Name);
            var ownedObjects = new List<UnityEngine.Object>();
            var material = new Material(shader);
            try
            {
                compileCase.Configure(material, ownedObjects);
                SetupUberUnlit(material);
                for (var pass = 0; pass < material.passCount; pass++)
                {
                    ShaderUtil.CompilePass(material, pass, true);
                    Assert.That(ShaderUtil.IsPassCompiled(material, pass), Is.True, "Pass " + pass);
                }

                foreach (var expectedKeyword in compileCase.ExpectedKeywords)
                {
                    Assert.That(material.IsKeywordEnabled(expectedKeyword), Is.True, $"{compileCase.Name}: {expectedKeyword}");
                }

                foreach (var expectedDisabledKeyword in compileCase.ExpectedDisabledKeywords)
                {
                    Assert.That(material.IsKeywordEnabled(expectedDisabledKeyword), Is.False, $"{compileCase.Name}: {expectedDisabledKeyword}");
                }

                var blockingMessages = ShaderUtil.GetShaderMessages(shader)
                    .Where(IsBlockingShaderMessage)
                    .Select(message => $"{message.severity}: {message.message} ({message.file}:{message.line})")
                    .ToArray();

                Assert.That(blockingMessages, Is.Empty, $"{compileCase.Name}{Environment.NewLine}{string.Join(Environment.NewLine, blockingMessages)}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
                foreach (var ownedObject in ownedObjects)
                {
                    UnityEngine.Object.DestroyImmediate(ownedObject);
                }
            }
        }
    }

    [Test]
    public void UIParticlesUberUnlitPhase56APassTopologyRemainsUiOnly()
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

            Assert.That(lightModes, Is.EquivalentTo(new[] { "ForwardBase" }));
            Assert.That(material.FindPass("ShadowCaster"), Is.EqualTo(-1));
            Assert.That(material.FindPass("DepthOnly"), Is.EqualTo(-1));
            Assert.That(material.FindPass("DepthNormals"), Is.EqualTo(-1));
            Assert.That(material.FindPass("SceneSelectionPass"), Is.EqualTo(-1));
            Assert.That(material.FindPass("Picking"), Is.EqualTo(-1));

            var keywordDeclarations = string.Join("\n", shaderText
                .Split('\n')
                .Where(line => line.Contains("#pragma", StringComparison.Ordinal)));
            Assert.That(keywordDeclarations, Does.Contain("UNITY_UI_CLIP_RECT"));
            Assert.That(keywordDeclarations, Does.Not.Contain("_CASTING_PUNCTUAL_LIGHT_SHADOW"));
            Assert.That(keywordDeclarations, Does.Not.Contain("_SHADOW_CASTER_ALPHA_TEST_ENABLED"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(material);
        }
    }

    [Test]
    public void UIParticlesUberUnlitMaterialEditorApiSurvivesComplexSetup()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);
        var material = new Material(shader);

        try
        {
            material.SetFloat("_RenderType", 2);
            material.SetFloat("_TransparentBlendMode", 0);
            material.SetFloat("_Cull", 0);
            material.SetFloat("_ZWrite", 0);
            material.SetFloat("_ZTest", 8);
            material.SetFloat("_StencilComp", 3);
            material.SetFloat("_Stencil", 7);
            material.SetFloat("_StencilOp", 2);
            material.SetFloat("_StencilReadMask", 127);
            material.SetFloat("_StencilWriteMask", 63);
            material.SetFloat("_ColorMask", 3);
            material.SetFloat("_TintAreaMode", 1);
            material.SetFloat("_TintColorMode", 0);
            material.SetFloat("_EmissionAreaType", 1);
            material.SetFloat("_EmissionColorType", 0);
            material.EnableKeyword("UNITY_UI_CLIP_RECT");

            SetupUberUnlit(material);

            Assert.That(material.GetFloat("_StencilComp"), Is.EqualTo(3));
            Assert.That(material.GetFloat("_Stencil"), Is.EqualTo(7));
            Assert.That(material.GetFloat("_StencilOp"), Is.EqualTo(2));
            Assert.That(material.GetFloat("_StencilReadMask"), Is.EqualTo(127));
            Assert.That(material.GetFloat("_StencilWriteMask"), Is.EqualTo(63));
            Assert.That(material.GetFloat("_ColorMask"), Is.EqualTo(3));
            Assert.That(material.IsKeywordEnabled("_TINT_AREA_ALL"), Is.True);
            Assert.That(material.IsKeywordEnabled("_TINT_COLOR_ENABLED"), Is.True);
            Assert.That(material.IsKeywordEnabled("_EMISSION_AREA_ALL"), Is.True);
            Assert.That(material.IsKeywordEnabled("_EMISSION_COLOR_COLOR"), Is.True);
            Assert.That(material.IsKeywordEnabled("UNITY_UI_CLIP_RECT"), Is.True);
            Assert.That(File.ReadAllText(ShaderPath), Does.Contain("CustomEditor \"Nova.Editor.Core.Scripts.UIParticlesUberUnlitGUI\""));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(material);
        }
    }

    [Test]
    public void UIParticlesUberUnlitPhase56AStatusAuditDocumentsScopeAndLimitations()
    {
        var matrixText = File.ReadAllText(MatrixPath);

        Assert.That(matrixText, Does.Contain("Phase 5.6A UI UberUnlit Consolidation Status Audit"));
        Assert.That(matrixText, Does.Contain("Implemented and smoke-tested"));
        Assert.That(matrixText, Does.Contain("Implemented but approximate"));
        Assert.That(matrixText, Does.Contain("Preserved but not fully parity-tested"));
        Assert.That(matrixText, Does.Contain("Intentionally not implemented for UI UberUnlit"));
        Assert.That(matrixText, Does.Contain("base map 2D array"));
        Assert.That(matrixText, Does.Contain("flow target alpha transition"));
        Assert.That(matrixText, Does.Contain("Vertex deformation in UI ForwardBase"));
        Assert.That(matrixText, Does.Contain("non-UI ShadowCaster keyword exclusion"));
        Assert.That(matrixText, Does.Contain("manual mirror sampler approximation"));
        Assert.That(matrixText, Does.Contain("Built-in camera depth texture approximation"));
        Assert.That(matrixText, Does.Contain("controlled mesh-based UI shader smoke harness"));
        Assert.That(matrixText, Does.Contain("Screen Space Overlay depth relationship limitation"));
        Assert.That(matrixText, Does.Contain("full Canvas `Mask` / `RectMask2D` parity"));
        Assert.That(matrixText, Does.Contain("full Particle System Custom Vertex Streams parity"));
        Assert.That(matrixText, Does.Contain("golden image visual parity"));
        Assert.That(matrixText, Does.Contain("UI ShadowCaster pass"));
        Assert.That(matrixText, Does.Contain("UI DepthOnly pass"));
        Assert.That(matrixText, Does.Contain("UI DepthNormals pass"));
        Assert.That(matrixText, Does.Contain("UI SceneSelectionPass"));
        Assert.That(matrixText, Does.Contain("UI Picking pass"));
        Assert.That(matrixText, Does.Not.Contain("full UI UberUnlit visual parity complete"));
        Assert.That(matrixText, Does.Not.Contain("final visual regression complete"));
    }

    [Test]
    public void Phase56ADoesNotPromoteLitDistortionOrOptimizerRuntimeScope()
    {
        var shaderFiles = new[]
        {
            "Assets/Nova/Runtime/Core/Shaders/UIParticlesUberLit.shader",
            "Assets/Nova/Runtime/Core/Shaders/ParticlesDistortion.shader",
        };

        foreach (var shaderFile in shaderFiles)
        {
            var text = File.ReadAllText(shaderFile);
            Assert.That(text.Contains(@"""LightMode"" = ""ForwardAdd"""), Is.EqualTo(shaderFile.Contains("UIParticlesUberLit")), shaderFile);
            Assert.That(text, Does.Not.Contain(@"""LightMode"" = ""ShadowCaster"""), shaderFile);
            Assert.That(text, Does.Not.Contain(@"""LightMode"" = ""DepthOnly"""), shaderFile);
            Assert.That(text, Does.Not.Contain(@"""LightMode"" = ""DepthNormals"""), shaderFile);
            Assert.That(text.Contains(@"""LightMode"" = ""DistortedUvBuffer"""), Is.EqualTo(shaderFile.Contains("ParticlesDistortion")), shaderFile);
        }

        var litText = File.ReadAllText("Assets/Nova/Runtime/Core/Shaders/ParticlesUberLit.shader");
        Assert.That(litText, Does.Contain(@"""LightMode"" = ""ForwardAdd"""));
        Assert.That(litText, Does.Contain(@"""LightMode"" = ""ShadowCaster"""));
        Assert.That(litText, Does.Contain(@"""LightMode"" = ""DepthOnly"""));
        Assert.That(litText, Does.Contain(@"""LightMode"" = ""DepthNormals"""));
        Assert.That(litText, Does.Not.Contain(@"""LightMode"" = ""DistortedUvBuffer"""));
    }

    private static IEnumerable<CompileCase> CreateComplexCompileCases()
    {
        yield return new CompileCase(
            "base map 2D array + tint map 2D array + flow + clip rect",
            (material, owned) =>
            {
                var baseArray = CreateTexture2DArray();
                var tintArray = CreateTexture2DArray();
                var flow = CreateTexture2D(Color.gray);
                owned.AddRange(new UnityEngine.Object[] { baseArray, tintArray, flow });
                material.SetFloat("_BaseMapMode", 1);
                material.SetFloat("_BaseMapSliceCount", 2);
                material.SetTexture("_BaseMap2DArray", baseArray);
                material.SetFloat("_TintAreaMode", 1);
                material.SetFloat("_TintColorMode", 3);
                material.SetFloat("_TintMapSliceCount", 2);
                material.SetTexture("_TintMap2DArray", tintArray);
                material.SetTexture("_FlowMap", flow);
                material.SetFloat("_FlowMapTarget", 3);
                material.EnableKeyword("UNITY_UI_CLIP_RECT");
            },
            "_BASE_MAP_MODE_2D_ARRAY",
            "_TINT_AREA_ALL",
            "_TINT_MAP_MODE_2D_ARRAY",
            "_FLOW_MAP_TARGET_BASE",
            "_FLOW_MAP_TARGET_TINT");

        yield return new CompileCase(
            "base map 3D + alpha transition 3D + emission 3D",
            (material, owned) =>
            {
                var base3D = CreateTexture3D();
                var alpha3D = CreateTexture3D();
                var emission3D = CreateTexture3D();
                owned.AddRange(new UnityEngine.Object[] { base3D, alpha3D, emission3D });
                material.SetFloat("_BaseMapMode", 2);
                material.SetFloat("_BaseMapSliceCount", 2);
                material.SetTexture("_BaseMap3D", base3D);
                material.SetFloat("_AlphaTransitionMode", 2);
                material.SetFloat("_AlphaTransitionMapMode", 2);
                material.SetFloat("_AlphaTransitionMapSliceCount", 2);
                material.SetTexture("_AlphaTransitionMap3D", alpha3D);
                material.SetFloat("_EmissionAreaType", 2);
                material.SetFloat("_EmissionColorType", 0);
                material.SetFloat("_EmissionMapMode", 2);
                material.SetFloat("_EmissionMapSliceCount", 2);
                material.SetTexture("_EmissionMap3D", emission3D);
            },
            "_BASE_MAP_MODE_3D",
            "_DISSOLVE_TRANSITION_ENABLED",
            "_ALPHA_TRANSITION_MAP_MODE_3D",
            "_EMISSION_AREA_MAP",
            "_EMISSION_COLOR_COLOR",
            "_EMISSION_MAP_MODE_3D");

        yield return new CompileCase(
            "dissolve + second alpha transition + flow target alpha transition",
            (material, owned) =>
            {
                var alpha = CreateTexture2D(Color.white);
                var second = CreateTexture2D(Color.white);
                var flow = CreateTexture2D(Color.gray);
                owned.AddRange(new UnityEngine.Object[] { alpha, second, flow });
                material.SetFloat("_AlphaTransitionMode", 2);
                material.SetFloat("_AlphaTransitionMapMode", 0);
                material.SetTexture("_AlphaTransitionMap", alpha);
                material.SetFloat("_AlphaTransitionSecondTextureBlendMode", 2);
                material.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 0);
                material.SetTexture("_AlphaTransitionMapSecondTexture", second);
                material.SetTexture("_FlowMap", flow);
                material.SetFloat("_FlowMapTarget", 4);
            },
            "_DISSOLVE_TRANSITION_ENABLED",
            "_ALPHA_TRANSITION_MAP_MODE_2D",
            "_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY",
            "_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D",
            "_FLOW_MAP_TARGET_ALPHA_TRANSITION");

        yield return new CompileCase(
            "emission + keep edge transparency + luminance transparency",
            (material, owned) =>
            {
                material.SetFloat("_EmissionAreaType", 3);
                material.SetFloat("_EmissionColorType", 0);
                material.SetFloat("_KeepEdgeTransparency", 1);
                material.SetFloat("_LuminanceTransparencyEnabled", 1);
            },
            "_EMISSION_AREA_ALPHA",
            "_EMISSION_COLOR_COLOR",
            "_TRANSPARENCY_BY_LUMINANCE");

        yield return new CompileCase(
            "soft particles + depth fade + alpha transition + emission",
            (material, owned) =>
            {
                var alpha = CreateTexture2D(Color.white);
                owned.Add(alpha);
                material.SetFloat("_SoftParticlesEnabled", 1);
                material.SetFloat("_DepthFadeEnabled", 1);
                material.SetFloat("_AlphaTransitionMode", 1);
                material.SetFloat("_AlphaTransitionMapMode", 0);
                material.SetTexture("_AlphaTransitionMap", alpha);
                material.SetFloat("_EmissionAreaType", 1);
                material.SetFloat("_EmissionColorType", 0);
            },
            "_SOFT_PARTICLES_ENABLED",
            "_DEPTH_FADE_ENABLED",
            "_FADE_TRANSITION_ENABLED",
            "_ALPHA_TRANSITION_MAP_MODE_2D",
            "_EMISSION_AREA_ALL",
            "_EMISSION_COLOR_COLOR");

        yield return new CompileCase(
            "vertex deformation + soft particles + clip rect",
            (material, owned) =>
            {
                var deformation = CreateTexture2D(Color.white);
                owned.Add(deformation);
                material.SetTexture("_VertexDeformationMap", deformation);
                material.SetFloat("_VertexDeformationIntensity", 0.25f);
                material.SetFloat("_SoftParticlesEnabled", 1);
                material.EnableKeyword("UNITY_UI_CLIP_RECT");
            },
            "_VERTEX_DEFORMATION_ENABLED",
            "_SOFT_PARTICLES_ENABLED");

        yield return new CompileCase(
            "vertex deformation + alpha transition + emission + rect clip",
            (material, owned) =>
            {
                var deformation = CreateTexture2D(Color.white);
                var alpha = CreateTexture2D(Color.white);
                owned.AddRange(new UnityEngine.Object[] { deformation, alpha });
                material.SetTexture("_VertexDeformationMap", deformation);
                material.SetFloat("_AlphaTransitionMode", 2);
                material.SetFloat("_AlphaTransitionMapMode", 0);
                material.SetTexture("_AlphaTransitionMap", alpha);
                material.SetFloat("_EmissionAreaType", 1);
                material.SetFloat("_EmissionColorType", 0);
                material.EnableKeyword("UNITY_UI_CLIP_RECT");
            },
            "_VERTEX_DEFORMATION_ENABLED",
            "_DISSOLVE_TRANSITION_ENABLED",
            "_ALPHA_TRANSITION_MAP_MODE_2D",
            "_EMISSION_AREA_ALL",
            "_EMISSION_COLOR_COLOR");

        yield return new CompileCase(
            "tint rim + rim transparency + luminance transparency",
            (material, owned) =>
            {
                material.SetFloat("_TintAreaMode", 2);
                material.SetFloat("_TintColorMode", 0);
                material.SetFloat("_RimTransparencyEnabled", 1);
                material.SetFloat("_LuminanceTransparencyEnabled", 1);
            },
            "_TINT_AREA_RIM",
            "_TINT_COLOR_ENABLED",
            "_TRANSPARENCY_BY_RIM",
            "_TRANSPARENCY_BY_LUMINANCE");

        yield return new CompileCase(
            "stencil property state + alpha test + emission",
            (material, owned) =>
            {
                material.SetFloat("_StencilComp", 3);
                material.SetFloat("_Stencil", 1);
                material.SetFloat("_StencilOp", 2);
                material.SetFloat("_StencilReadMask", 255);
                material.SetFloat("_StencilWriteMask", 255);
                material.SetFloat("_RenderType", 1);
                material.SetFloat("_Cutoff", 0.5f);
                material.SetFloat("_EmissionAreaType", 1);
                material.SetFloat("_EmissionColorType", 0);
            },
            "_ALPHATEST_ENABLED",
            "_EMISSION_AREA_ALL",
            "_EMISSION_COLOR_COLOR");

        yield return new CompileCase(
            "dense UI non-lit implemented feature set",
            (material, owned) =>
            {
                var baseArray = CreateTexture2DArray();
                var tintArray = CreateTexture2DArray();
                var alpha = CreateTexture2D(Color.white);
                var second = CreateTexture2D(Color.white);
                var emission = CreateTexture2D(Color.white);
                var ramp = CreateTexture2D(Color.white);
                var flow = CreateTexture2D(Color.gray);
                var deformation = CreateTexture2D(Color.white);
                owned.AddRange(new UnityEngine.Object[] { baseArray, tintArray, alpha, second, emission, ramp, flow, deformation });
                material.SetFloat("_BaseMapMode", 1);
                material.SetTexture("_BaseMap2DArray", baseArray);
                material.SetFloat("_BaseMapRandomRowSelectionEnabled", 1);
                material.SetFloat("_BaseMapToneMode", 1);
                material.SetFloat("_TintAreaMode", 1);
                material.SetFloat("_TintColorMode", 3);
                material.SetTexture("_TintMap2DArray", tintArray);
                material.SetFloat("_TintMapRandomRowSelectionEnabled", 1);
                material.SetTexture("_FlowMap", flow);
                material.SetFloat("_FlowMapTarget", 15);
                material.SetFloat("_ColorCorrectionMode", 1);
                material.SetFloat("_AlphaTransitionMode", 2);
                material.SetFloat("_AlphaTransitionMapMode", 0);
                material.SetTexture("_AlphaTransitionMap", alpha);
                material.SetFloat("_AlphaTransitionSecondTextureBlendMode", 1);
                material.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 0);
                material.SetTexture("_AlphaTransitionMapSecondTexture", second);
                material.SetFloat("_EmissionAreaType", 2);
                material.SetFloat("_EmissionColorType", 2);
                material.SetFloat("_EmissionMapMode", 0);
                material.SetTexture("_EmissionMap", emission);
                material.SetTexture("_EmissionColorRamp", ramp);
                material.SetFloat("_RimTransparencyEnabled", 1);
                material.SetFloat("_LuminanceTransparencyEnabled", 1);
                material.SetFloat("_SoftParticlesEnabled", 1);
                material.SetFloat("_DepthFadeEnabled", 1);
                material.SetTexture("_VertexDeformationMap", deformation);
                material.EnableKeyword("UNITY_UI_CLIP_RECT");
            },
            new[]
            {
                "_BASE_MAP_MODE_2D_ARRAY",
                "_BASE_MAP_RANDOM_ROW_SELECTION_ENABLED",
                "_BASE_MAP_TONE_MODE_TRITONE",
                "_TINT_AREA_ALL",
                "_TINT_MAP_MODE_2D_ARRAY",
                "_TINT_MAP_RANDOM_ROW_SELECTION_ENABLED",
                "_FLOW_MAP_TARGET_BASE",
                "_FLOW_MAP_TARGET_TINT",
                "_FLOW_MAP_TARGET_ALPHA_TRANSITION",
                "_FLOW_MAP_TARGET_EMISSION",
                "_GREYSCALE_ENABLED",
                "_DISSOLVE_TRANSITION_ENABLED",
                "_ALPHA_TRANSITION_MAP_MODE_2D",
                "_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE",
                "_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D",
                "_EMISSION_AREA_MAP",
                "_EMISSION_COLOR_MAP",
                "_EMISSION_MAP_MODE_2D",
                "_TRANSPARENCY_BY_RIM",
                "_TRANSPARENCY_BY_LUMINANCE",
                "_SOFT_PARTICLES_ENABLED",
                "_DEPTH_FADE_ENABLED",
                "_VERTEX_DEFORMATION_ENABLED",
            },
            new[] { "_CASTING_PUNCTUAL_LIGHT_SHADOW", "_SHADOW_CASTER_ALPHA_TEST_ENABLED", "_BASE_MAP_MODE_3D", "_GRADIENT_MAP_ENABLED", "_FADE_TRANSITION_ENABLED" });
    }

    private static Texture2D CreateTexture2D(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static Texture2DArray CreateTexture2DArray()
    {
        var texture = new Texture2DArray(1, 1, 2, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(new[] { Color.white }, 0);
        texture.SetPixels(new[] { Color.gray }, 1);
        texture.Apply();
        return texture;
    }

    private static Texture3D CreateTexture3D()
    {
        var texture = new Texture3D(1, 1, 2, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(new[] { Color.white, Color.gray });
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

    private readonly struct CompileCase
    {
        public CompileCase(string name, Action<Material, List<UnityEngine.Object>> configure, params string[] expectedKeywords)
            : this(name, configure, expectedKeywords, Array.Empty<string>())
        {
        }

        public CompileCase(
            string name,
            Action<Material, List<UnityEngine.Object>> configure,
            IReadOnlyList<string> expectedKeywords,
            IReadOnlyList<string> expectedDisabledKeywords)
        {
            Name = name;
            Configure = configure;
            ExpectedKeywords = expectedKeywords;
            ExpectedDisabledKeywords = expectedDisabledKeywords;
        }

        public string Name { get; }
        public Action<Material, List<UnityEngine.Object>> Configure { get; }
        public IReadOnlyList<string> ExpectedKeywords { get; }
        public IReadOnlyList<string> ExpectedDisabledKeywords { get; }
    }
}

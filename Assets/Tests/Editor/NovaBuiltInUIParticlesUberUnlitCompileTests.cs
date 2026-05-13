using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class NovaBuiltInUIParticlesUberUnlitCompileTests
{
    private const string ShaderName = "Nova/UIParticles/UberUnlit";
    private const string ShaderPath = "Assets/Nova/Runtime/Core/Shaders/UIParticlesUberUnlit.shader";

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
    public void UIParticlesUberUnlitForwardPassUsesBuiltInUiSurface()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Contain("\"LightMode\" = \"ForwardBase\""));
        Assert.That(shaderText, Does.Contain("Blend [_BlendSrc] [_BlendDst]"));
        Assert.That(shaderText, Does.Contain("ZWrite [_ZWrite]"));
        Assert.That(shaderText, Does.Contain("Cull [_Cull]"));
        Assert.That(shaderText, Does.Contain("ZTest [_ZTest]"));
        Assert.That(shaderText, Does.Contain("ColorMask [_ColorMask]"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInCore.cginc"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInTexture.cginc"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInUI.cginc"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInUberUnlit.cginc"));
        Assert.That(shaderText, Does.Contain("NovaObjectToClipPos"));
        Assert.That(shaderText, Does.Contain("SampleBaseMap"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX2D(_BaseMap"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX2D_ARRAY(_BaseMap2DArray"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX3D_LOD(_BaseMap3D"));
        Assert.That(shaderText, Does.Contain("NovaApplyBaseMapChannel"));
        Assert.That(shaderText, Does.Contain("NovaApplyBaseMapTone"));
        Assert.That(shaderText, Does.Contain("NovaApplyTintColor"));
        Assert.That(shaderText, Does.Contain("NovaUIClipAlpha"));
        Assert.That(shaderText, Does.Contain("_ALPHATEST_ENABLED"));
        Assert.That(shaderText, Does.Contain("clip("));
        Assert.That(shaderText, Does.Contain("CustomEditor \"Nova.Editor.Core.Scripts.UIParticlesUberUnlitGUI\""));
    }

    [Test]
    public void UIParticlesUberUnlitDeclaresUiStencilAndColorMaskState()
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
    public void UIParticlesUberUnlitPhase51PassTopologyStaysUiOnly()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(CountOccurrences(shaderText, "\n        Pass\n        {"), Is.EqualTo(1));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"ShadowCaster\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"DepthOnly\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"DepthNormals\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"SceneSelectionPass\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"Picking\""));
        var keywordDeclarations = string.Join("\n", shaderText
            .Split('\n')
            .Where(line => line.Contains("#pragma", StringComparison.Ordinal) && line.Contains("shader_feature", StringComparison.Ordinal)));
        Assert.That(keywordDeclarations, Does.Not.Contain("_CASTING_PUNCTUAL_LIGHT_SHADOW"));
        Assert.That(keywordDeclarations, Does.Not.Contain("_SHADOW_CASTER_ALPHA_TEST_ENABLED"));
        Assert.That(shaderText, Does.Contain("UNITY_UI_CLIP_RECT"));
    }

    [Test]
    public void UIParticlesUberUnlitPhase52ABaseTextureKeywordsStayUiScoped()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Contain("_BASE_MAP_MODE_2D_ARRAY"));
        Assert.That(shaderText, Does.Contain("_BASE_MAP_MODE_3D"));
        Assert.That(shaderText, Does.Contain("_BASE_MAP_CHANNEL_ENABLED"));
        Assert.That(shaderText, Does.Contain("_BASE_MAP_RANDOM_ROW_SELECTION_ENABLED"));
        Assert.That(shaderText, Does.Contain("_BASE_MAP_TONE_MODE_TRITONE"));
        Assert.That(shaderText, Does.Contain("_BASE_MAP_TONE_MODE_PENTONE"));
        Assert.That(shaderText, Does.Contain("_BASE_SAMPLER_STATE_POINT_MIRROR"));
        Assert.That(shaderText, Does.Contain("_BASE_SAMPLER_STATE_LINEAR_MIRROR"));
        Assert.That(shaderText, Does.Contain("_BASE_SAMPLER_STATE_TRILINEAR_MIRROR"));
        Assert.That(shaderText, Does.Contain("NovaApplyBaseMirror"));
        Assert.That(shaderText, Does.Contain("NovaBaseMapProgress"));
    }

    [Test]
    public void UIParticlesUberUnlitRepresentativeKeywordStatesHaveNoBlockingCompilerMessages()
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
                foreach (var texture in material.GetTexturePropertyNames()
                    .Select(material.GetTexture)
                    .Where(texture => texture != null && texture.name.StartsWith("NovaBuiltInUiCompile", StringComparison.Ordinal))
                    .Distinct())
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }

                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }

    [Test]
    public void UIParticlesUberUnlitSourceContainsNoForbiddenPipelineTokens()
    {
        var shaderText = StripComments(File.ReadAllText(ShaderPath));
        var violations = ForbiddenTokens
            .Where(token => shaderText.Contains(token, StringComparison.Ordinal))
            .ToArray();

        Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
    }

    private static Material[] CreateRepresentativeMaterials(Shader shader)
    {
        var defaultMaterial = new Material(shader);

        var baseMap2D = new Material(shader);
        baseMap2D.SetFloat("_BaseMapMode", 0);
        baseMap2D.SetTexture("_BaseMap", CreateCompileTexture());

        var baseMap2DArray = new Material(shader);
        baseMap2DArray.SetFloat("_BaseMapMode", 1);
        baseMap2DArray.SetFloat("_BaseMapSliceCount", 2);
        baseMap2DArray.SetTexture("_BaseMap2DArray", CreateCompileTexture2DArray());

        var baseMap3D = new Material(shader);
        baseMap3D.SetFloat("_BaseMapMode", 2);
        baseMap3D.SetFloat("_BaseMapSliceCount", 2);
        baseMap3D.SetTexture("_BaseMap3D", CreateCompileTexture3D());

        var channel = new Material(shader);
        channel.SetFloat("_BaseMapChannel", 2);
        channel.SetTexture("_BaseMap", CreateCompileTexture());

        var randomRow = new Material(shader);
        randomRow.SetFloat("_BaseMapMode", 1);
        randomRow.SetFloat("_BaseMapSliceCount", 4);
        randomRow.SetFloat("_BaseMapRandomRowSelectionEnabled", 1);
        randomRow.SetFloat("_BaseMapRandomRowCoord", 1);
        randomRow.SetFloat("_BaseMapRowCount", 2);
        randomRow.SetTexture("_BaseMap2DArray", CreateCompileTexture2DArray(4));

        var tritone = new Material(shader);
        tritone.SetFloat("_BaseMapToneMode", 1);
        tritone.SetTexture("_BaseMap", CreateCompileTexture());

        var pentone = new Material(shader);
        pentone.SetFloat("_BaseMapToneMode", 2);
        pentone.SetTexture("_BaseMap", CreateCompileTexture());

        var pointMirror = new Material(shader);
        pointMirror.SetFloat("_BaseMapMirrorSampling", 1);
        pointMirror.SetTexture("_BaseMap", CreateCompileTexture(FilterMode.Point));

        var linearMirror = new Material(shader);
        linearMirror.SetFloat("_BaseMapMirrorSampling", 1);
        linearMirror.SetTexture("_BaseMap", CreateCompileTexture(FilterMode.Bilinear));

        var trilinearMirror = new Material(shader);
        trilinearMirror.SetFloat("_BaseMapMirrorSampling", 1);
        trilinearMirror.SetTexture("_BaseMap", CreateCompileTexture(FilterMode.Trilinear));

        var baseFeatureClipRect = new Material(shader);
        baseFeatureClipRect.SetFloat("_BaseMapMode", 1);
        baseFeatureClipRect.SetFloat("_BaseMapSliceCount", 2);
        baseFeatureClipRect.SetTexture("_BaseMap2DArray", CreateCompileTexture2DArray());
        baseFeatureClipRect.EnableKeyword("UNITY_UI_CLIP_RECT");
        baseFeatureClipRect.SetVector("_ClipRect", new Vector4(-0.25f, -1f, 1f, 1f));

        var baseFeatureAlphaTest = new Material(shader);
        baseFeatureAlphaTest.SetFloat("_RenderType", 1);
        baseFeatureAlphaTest.SetFloat("_Cutoff", 0.5f);
        baseFeatureAlphaTest.SetFloat("_BaseMapToneMode", 1);
        baseFeatureAlphaTest.SetTexture("_BaseMap", CreateCompileTexture());

        var tintColor = new Material(shader);
        tintColor.SetFloat("_TintAreaMode", 1);
        tintColor.SetFloat("_TintColorMode", 0);
        tintColor.SetColor("_TintColor", new Color(0.25f, 1f, 0.25f, 1f));

        var alphaTest = new Material(shader);
        alphaTest.SetFloat("_RenderType", 1);
        alphaTest.SetFloat("_Cutoff", 0.5f);

        var clipRect = new Material(shader);
        clipRect.EnableKeyword("UNITY_UI_CLIP_RECT");
        clipRect.SetVector("_ClipRect", new Vector4(-0.25f, -1f, 1f, 1f));

        var stencil = new Material(shader);
        stencil.SetFloat("_StencilComp", 3);
        stencil.SetFloat("_Stencil", 1);
        stencil.SetFloat("_StencilOp", 2);
        stencil.SetFloat("_StencilReadMask", 255);
        stencil.SetFloat("_StencilWriteMask", 255);
        stencil.SetFloat("_ColorMask", 15);

        return new[]
        {
            defaultMaterial,
            baseMap2D,
            baseMap2DArray,
            baseMap3D,
            channel,
            randomRow,
            tritone,
            pentone,
            pointMirror,
            linearMirror,
            trilinearMirror,
            baseFeatureClipRect,
            baseFeatureAlphaTest,
            tintColor,
            alphaTest,
            clipRect,
            stencil
        };
    }

    private static Texture2D CreateCompileTexture(FilterMode filterMode = FilterMode.Point)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUiCompileTexture",
            filterMode = filterMode,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return texture;
    }

    private static Texture2DArray CreateCompileTexture2DArray(int depth = 2)
    {
        var texture = new Texture2DArray(1, 1, depth, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUiCompileTexture2DArray",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        for (var slice = 0; slice < depth; slice++)
        {
            texture.SetPixels(new[] { Color.white }, slice);
        }

        texture.Apply();
        return texture;
    }

    private static Texture3D CreateCompileTexture3D()
    {
        var texture = new Texture3D(1, 1, 2, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUiCompileTexture3D",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(new[] { Color.white, Color.white });
        texture.Apply();
        return texture;
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

    private static string StripComments(string text)
    {
        return Regex.Replace(
            text,
            @"/\*.*?\*/|//.*?$",
            string.Empty,
            RegexOptions.Singleline | RegexOptions.Multiline);
    }

    private static int CountOccurrences(string text, string token)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += token.Length;
        }

        return count;
    }
}

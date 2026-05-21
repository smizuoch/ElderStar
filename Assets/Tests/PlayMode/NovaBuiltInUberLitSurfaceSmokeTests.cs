using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberLitSurfaceSmokeTests
{
    private const string LitShaderName = "Nova/Particles/UberLit";
    private const int RenderSize = 128;
    private const int TextureSize = 8;

    [Test]
    public void UberLitMetallicAffectsSpecularResponse()
    {
        var low = RenderLit(material => ConfigureMetallic(material, 0f, 0.85f));
        var high = RenderLit(material => ConfigureMetallic(material, 1f, 0.85f));

        Assert.That(ColorDistance(high.Center, low.Center), Is.GreaterThan(0.08f), $"low {low.Center}, high {high.Center}");
        Assert.That(Luminance(high.Center), Is.GreaterThan(Luminance(low.Center) + 0.03f), $"low {low.Center}, high {high.Center}");
    }

    [Test]
    public void UberLitMetallicMapAffectsSpecularResponse()
    {
        var sample = RenderLit(material =>
        {
            ConfigureMetallic(material, 1f, 0.85f);
            material.SetTexture("_MetallicMap", CreateSplitTexture(new Color(0f, 0f, 0f, 1f), new Color(1f, 1f, 1f, 1f)));
            material.SetFloat("_MetallicMapChannelsX", 0f);
        });

        Assert.That(Luminance(sample.Right), Is.GreaterThan(Luminance(sample.Left) + 0.04f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitMetallicMapChannelSelectionAffectsOutput()
    {
        var redChannel = RenderLit(material =>
        {
            ConfigureMetallic(material, 1f, 0.85f);
            material.SetTexture("_MetallicMap", CreateSolidTexture(new Color(1f, 0f, 0f, 1f)));
            material.SetFloat("_MetallicMapChannelsX", 0f);
        });
        var greenChannel = RenderLit(material =>
        {
            ConfigureMetallic(material, 1f, 0.85f);
            material.SetTexture("_MetallicMap", CreateSolidTexture(new Color(1f, 0f, 0f, 1f)));
            material.SetFloat("_MetallicMapChannelsX", 1f);
        });

        Assert.That(ColorDistance(redChannel.Center, greenChannel.Center), Is.GreaterThan(0.08f), $"red {redChannel.Center}, green {greenChannel.Center}");
    }

    [Test]
    public void UberLitMetallicMap2DArrayAffectsSpecularResponse()
    {
        var sample = RenderLit(material =>
        {
            ConfigureMetallic(material, 1f, 0.85f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_MetallicMap2DArray", CreateSplitTextureArray(new Color(0f, 0f, 0f, 1f), new Color(1f, 1f, 1f, 1f)));
            material.SetFloat("_MetallicMapChannelsX", 0f);
        });

        Assert.That(Luminance(sample.Right), Is.GreaterThan(Luminance(sample.Left) + 0.04f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitMetallicMap2DArrayChannelSelectionAffectsOutput()
    {
        var redChannel = RenderLit(material =>
        {
            ConfigureMetallic(material, 1f, 0.85f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_MetallicMap2DArray", CreateTextureArray(new Color(1f, 0f, 0f, 1f), new Color(1f, 0f, 0f, 1f)));
            material.SetFloat("_MetallicMapChannelsX", 0f);
        });
        var greenChannel = RenderLit(material =>
        {
            ConfigureMetallic(material, 1f, 0.85f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_MetallicMap2DArray", CreateTextureArray(new Color(1f, 0f, 0f, 1f), new Color(1f, 0f, 0f, 1f)));
            material.SetFloat("_MetallicMapChannelsX", 1f);
        });

        Assert.That(ColorDistance(redChannel.Center, greenChannel.Center), Is.GreaterThan(0.08f), $"red {redChannel.Center}, green {greenChannel.Center}");
    }

    [Test]
    public void UberLitMetallicMap3DAffectsSpecularResponse()
    {
        var sample = RenderLit(material =>
        {
            ConfigureMetallic(material, 1f, 0.85f);
            ConfigureLitTextureMode(material, 2f, 1f, 2f);
            material.SetTexture("_MetallicMap3D", CreateSplitTexture3D(new Color(0f, 0f, 0f, 1f), new Color(1f, 1f, 1f, 1f)));
            material.SetFloat("_MetallicMapChannelsX", 0f);
        });

        Assert.That(Luminance(sample.Right), Is.GreaterThan(Luminance(sample.Left) + 0.04f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitSpecularWorkflowUsesSpecularColor()
    {
        var red = RenderLit(material => ConfigureSpecular(material, new Color(1f, 0.05f, 0.05f, 1f), 0.85f));
        var blue = RenderLit(material => ConfigureSpecular(material, new Color(0.05f, 0.05f, 1f, 1f), 0.85f));

        Assert.That(red.Center.r, Is.GreaterThan(red.Center.b + 0.08f), red.Center.ToString());
        Assert.That(blue.Center.b, Is.GreaterThan(blue.Center.r + 0.08f), blue.Center.ToString());
    }

    [Test]
    public void UberLitSpecularMapAffectsSpecularResponse()
    {
        var sample = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.85f);
            material.SetTexture("_SpecularMap", CreateSplitTexture(new Color(0f, 0f, 0f, 1f), new Color(1f, 1f, 1f, 1f)));
        });

        Assert.That(Luminance(sample.Right), Is.GreaterThan(Luminance(sample.Left) + 0.04f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitSpecularMap2DArrayAffectsSpecularResponse()
    {
        var sample = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.85f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_SpecularMap2DArray", CreateSplitTextureArray(new Color(0f, 0f, 0f, 1f), new Color(1f, 1f, 1f, 1f)));
        });

        Assert.That(Luminance(sample.Right), Is.GreaterThan(Luminance(sample.Left) + 0.04f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitSpecularMap2DArrayPreservesSpecularColorOrTinting()
    {
        var red = RenderLit(material =>
        {
            ConfigureSpecular(material, new Color(1f, 0.05f, 0.05f, 1f), 0.85f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_SpecularMap2DArray", CreateTextureArray(Color.white, Color.white));
        });
        var blue = RenderLit(material =>
        {
            ConfigureSpecular(material, new Color(0.05f, 0.05f, 1f, 1f), 0.85f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_SpecularMap2DArray", CreateTextureArray(Color.white, Color.white));
        });

        Assert.That(red.Center.r, Is.GreaterThan(red.Center.b + 0.08f), red.Center.ToString());
        Assert.That(blue.Center.b, Is.GreaterThan(blue.Center.r + 0.08f), blue.Center.ToString());
    }

    [Test]
    public void UberLitSpecularMap3DAffectsSpecularResponse()
    {
        var sample = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.85f);
            ConfigureLitTextureMode(material, 2f, 1f, 2f);
            material.SetTexture("_SpecularMap3D", CreateSplitTexture3D(new Color(0f, 0f, 0f, 1f), new Color(1f, 1f, 1f, 1f)));
        });

        Assert.That(Luminance(sample.Right), Is.GreaterThan(Luminance(sample.Left) + 0.04f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitSpecularHighlightsToggleAffectsOutput()
    {
        var enabled = RenderLit(material => ConfigureSpecular(material, Color.white, 0.85f));
        var disabled = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.85f);
            material.SetFloat("_SpecularHighlights", 0f);
        });

        Assert.That(Luminance(enabled.Center), Is.GreaterThan(Luminance(disabled.Center) + 0.04f), $"enabled {enabled.Center}, disabled {disabled.Center}");
    }

    [Test]
    public void UberLitSmoothnessAffectsSpecularShapeOrIntensity()
    {
        var low = RenderLit(material => ConfigureSpecular(material, Color.white, 0.15f));
        var high = RenderLit(material => ConfigureSpecular(material, Color.white, 0.95f));

        Assert.That(ColorDistance(high.Center, low.Center), Is.GreaterThan(0.04f), $"low {low.Center}, high {high.Center}");
    }

    [Test]
    public void UberLitSmoothnessMapAffectsSpecularResponse()
    {
        var sample = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 1f);
            material.SetTexture("_SmoothnessMap", CreateSplitTexture(new Color(0.05f, 0.05f, 0.05f, 0.05f), new Color(1f, 1f, 1f, 1f)));
            material.SetFloat("_SmoothnessMapChannelsX", 0f);
        });

        Assert.That(ColorDistance(sample.Right, sample.Left), Is.GreaterThan(0.04f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitSmoothnessMap2DArrayAffectsSpecularResponse()
    {
        var sample = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 1f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_SmoothnessMap2DArray", CreateSplitTextureArray(new Color(0.05f, 0.05f, 0.05f, 0.05f), new Color(1f, 1f, 1f, 1f)));
            material.SetFloat("_SmoothnessMapChannelsX", 0f);
        });

        Assert.That(ColorDistance(sample.Right, sample.Left), Is.GreaterThan(0.04f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitSmoothnessMap2DArrayChannelSelectionAffectsOutput()
    {
        var redChannel = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 1f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_SmoothnessMap2DArray", CreateTextureArray(new Color(1f, 0.05f, 0.05f, 0.05f), new Color(1f, 0.05f, 0.05f, 0.05f)));
            material.SetFloat("_SmoothnessMapChannelsX", 0f);
        });
        var greenChannel = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 1f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_SmoothnessMap2DArray", CreateTextureArray(new Color(1f, 0.05f, 0.05f, 0.05f), new Color(1f, 0.05f, 0.05f, 0.05f)));
            material.SetFloat("_SmoothnessMapChannelsX", 1f);
        });

        Assert.That(ColorDistance(redChannel.Center, greenChannel.Center), Is.GreaterThan(0.04f), $"red {redChannel.Center}, green {greenChannel.Center}");
    }

    [Test]
    public void UberLitSmoothnessMap3DAffectsSpecularResponse()
    {
        var sample = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 1f);
            ConfigureLitTextureMode(material, 2f, 1f, 2f);
            material.SetTexture("_SmoothnessMap3D", CreateSplitTexture3D(new Color(0.05f, 0.05f, 0.05f, 0.05f), new Color(1f, 1f, 1f, 1f)));
            material.SetFloat("_SmoothnessMapChannelsX", 0f);
        });

        Assert.That(ColorDistance(sample.Right, sample.Left), Is.GreaterThan(0.04f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitNormalMapAffectsLighting()
    {
        var flat = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.8f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(0.5f, 0.5f, 1f, 1f)));
        }, LightCase.Angled(Color.white, 2.4f));
        var tilted = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.8f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
        }, LightCase.Angled(Color.white, 2.4f));

        Assert.That(ColorDistance(tilted.Center, flat.Center), Is.GreaterThan(0.05f), $"flat {flat.Center}, tilted {tilted.Center}");
    }

    [Test]
    public void UberLitNormalMap2DArrayAffectsLighting()
    {
        var flat = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.8f);
            ConfigureLitTextureMode(material, 1f, 0.25f, 2f);
            material.SetTexture("_NormalMap2DArray", CreateTextureArray(new Color(0.5f, 0.5f, 1f, 1f), new Color(1f, 0.5f, 0.5f, 1f)));
        }, LightCase.Angled(Color.white, 2.4f));
        var tilted = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.8f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_NormalMap2DArray", CreateTextureArray(new Color(0.5f, 0.5f, 1f, 1f), new Color(1f, 0.5f, 0.5f, 1f)));
        }, LightCase.Angled(Color.white, 2.4f));

        Assert.That(ColorDistance(tilted.Center, flat.Center), Is.GreaterThan(0.05f), $"flat {flat.Center}, tilted {tilted.Center}");
    }

    [Test]
    public void UberLitNormalMap3DAffectsLighting()
    {
        var flat = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.8f);
            ConfigureLitTextureMode(material, 2f, 0f, 2f);
            material.SetTexture("_NormalMap3D", CreateTexture3D(new Color(0.5f, 0.5f, 1f, 1f), new Color(1f, 0.5f, 0.5f, 1f)));
        }, LightCase.Angled(Color.white, 2.4f));
        var tilted = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.8f);
            ConfigureLitTextureMode(material, 2f, 1f, 2f);
            material.SetTexture("_NormalMap3D", CreateTexture3D(new Color(0.5f, 0.5f, 1f, 1f), new Color(1f, 0.5f, 0.5f, 1f)));
        }, LightCase.Angled(Color.white, 2.4f));

        Assert.That(ColorDistance(tilted.Center, flat.Center), Is.GreaterThan(0.05f), $"flat {flat.Center}, tilted {tilted.Center}");
    }

    [Test]
    public void UberLitNormalMapBumpScaleAffectsLighting()
    {
        var low = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.8f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
            material.SetFloat("_NormalMapBumpScale", 0.15f);
        }, LightCase.Angled(Color.white, 2.4f));
        var high = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.8f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
            material.SetFloat("_NormalMapBumpScale", 1f);
        }, LightCase.Angled(Color.white, 2.4f));

        Assert.That(ColorDistance(high.Center, low.Center), Is.GreaterThan(0.04f), $"low {low.Center}, high {high.Center}");
    }

    [Test]
    public void UberLitNormalMapWorksWithSpecularWorkflow()
    {
        var flat = RenderLit(material =>
        {
            ConfigureSpecular(material, new Color(0.2f, 0.8f, 1f, 1f), 0.85f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(0.5f, 0.5f, 1f, 1f)));
        }, LightCase.Angled(Color.white, 2.4f));
        var tilted = RenderLit(material =>
        {
            ConfigureSpecular(material, new Color(0.2f, 0.8f, 1f, 1f), 0.85f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
        }, LightCase.Angled(Color.white, 2.4f));

        Assert.That(ColorDistance(tilted.Center, flat.Center), Is.GreaterThan(0.05f), $"flat {flat.Center}, tilted {tilted.Center}");
    }

    [Test]
    public void UberLitSurfaceFeaturesPreserveAlphaTest()
    {
        var background = new Color(0.05f, 0.10f, 0.85f, 1f);
        var sample = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.9f);
            material.SetFloat("_RenderType", 1f);
            material.SetFloat("_Cutoff", 0.5f);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0.05f, 0.05f, 0.2f)));
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
        }, background: background);

        Assert.That(sample.Center.b, Is.GreaterThan(sample.Center.r + 0.25f), sample.Center.ToString());
    }

    [Test]
    public void UberLitSurfaceFeaturesPreserveTintAndVertexColor()
    {
        var sample = RenderLit(
            material =>
            {
                ConfigureMetallic(material, 0.4f, 0.7f);
                material.SetFloat("_TintAreaMode", 1f);
                material.SetFloat("_TintColorMode", 0f);
                material.SetColor("_TintColor", new Color(0.1f, 1f, 0.1f, 1f));
            },
            mesh: CreateQuad(new Color(1f, 0.55f, 1f, 1f)));

        Assert.That(sample.Center.g, Is.GreaterThan(sample.Center.r + 0.10f), sample.Center.ToString());
        Assert.That(sample.Center.g, Is.GreaterThan(sample.Center.b + 0.04f), sample.Center.ToString());
    }

    [Test]
    public void UberLitTextureArraySurfaceStackAffectsLighting()
    {
        var baseline = RenderLit(material =>
        {
            ConfigureMetallic(material, 0f, 0.25f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(0.5f, 0.5f, 1f, 1f)));
        }, LightCase.Angled(Color.white, 2.4f));
        var stacked = RenderLit(material =>
        {
            ConfigureMetallic(material, 1f, 1f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetTexture("_NormalMap2DArray", CreateTextureArray(new Color(0.5f, 0.5f, 1f, 1f), new Color(1f, 0.5f, 0.5f, 1f)));
            material.SetTexture("_MetallicMap2DArray", CreateTextureArray(Color.black, Color.white));
            material.SetTexture("_SmoothnessMap2DArray", CreateTextureArray(Color.black, Color.white));
            material.SetFloat("_MetallicMapChannelsX", 0f);
            material.SetFloat("_SmoothnessMapChannelsX", 0f);
        }, LightCase.Angled(Color.white, 2.4f));

        Assert.That(ColorDistance(stacked.Center, baseline.Center), Is.GreaterThan(0.08f), $"baseline {baseline.Center}, stacked {stacked.Center}");
    }

    [Test]
    public void UberLitTexture3DSurfaceStackAffectsLighting()
    {
        var baseline = RenderLit(material =>
        {
            ConfigureMetallic(material, 0f, 0.25f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(0.5f, 0.5f, 1f, 1f)));
        }, LightCase.Angled(Color.white, 2.4f));
        var stacked = RenderLit(material =>
        {
            ConfigureMetallic(material, 1f, 1f);
            ConfigureLitTextureMode(material, 2f, 1f, 2f);
            material.SetTexture("_NormalMap3D", CreateTexture3D(new Color(0.5f, 0.5f, 1f, 1f), new Color(1f, 0.5f, 0.5f, 1f)));
            material.SetTexture("_MetallicMap3D", CreateTexture3D(Color.black, Color.white));
            material.SetTexture("_SmoothnessMap3D", CreateTexture3D(Color.black, Color.white));
            material.SetFloat("_MetallicMapChannelsX", 0f);
            material.SetFloat("_SmoothnessMapChannelsX", 0f);
        }, LightCase.Angled(Color.white, 2.4f));

        Assert.That(ColorDistance(stacked.Center, baseline.Center), Is.GreaterThan(0.08f), $"baseline {baseline.Center}, stacked {stacked.Center}");
    }

    [Test]
    public void UberLitTextureModesPreserveAlphaTestTintAndVertexColor()
    {
        var background = new Color(0.05f, 0.10f, 0.85f, 1f);
        var clipped = RenderLit(material =>
        {
            ConfigureSpecular(material, Color.white, 0.9f);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetFloat("_RenderType", 1f);
            material.SetFloat("_Cutoff", 0.5f);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0.05f, 0.05f, 0.2f)));
            material.SetTexture("_NormalMap2DArray", CreateTextureArray(new Color(0.5f, 0.5f, 1f, 1f), new Color(1f, 0.5f, 0.5f, 1f)));
        }, background: background);
        var tinted = RenderLit(
            material =>
            {
                ConfigureMetallic(material, 0.4f, 0.7f);
                ConfigureLitTextureMode(material, 2f, 1f, 2f);
                material.SetTexture("_NormalMap3D", CreateTexture3D(new Color(0.5f, 0.5f, 1f, 1f), new Color(0.5f, 0.5f, 1f, 1f)));
                material.SetFloat("_TintAreaMode", 1f);
                material.SetFloat("_TintColorMode", 0f);
                material.SetColor("_TintColor", new Color(0.1f, 1f, 0.1f, 1f));
            },
            mesh: CreateQuad(new Color(1f, 0.55f, 1f, 1f)));

        Assert.That(clipped.Center.b, Is.GreaterThan(clipped.Center.r + 0.25f), clipped.Center.ToString());
        Assert.That(tinted.Center.g, Is.GreaterThan(tinted.Center.r + 0.10f), tinted.Center.ToString());
        Assert.That(tinted.Center.g, Is.GreaterThan(tinted.Center.b + 0.04f), tinted.Center.ToString());
    }

    private static void ConfigureMetallic(Material material, float metallic, float smoothness)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.28f, 0.28f, 0.28f, 1f)));
        material.SetFloat("_LitWorkflowMode", 1f);
        material.SetFloat("_SpecularHighlights", 1f);
        material.SetFloat("_EnvironmentReflections", 0f);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
    }

    private static void ConfigureLitTextureMode(Material material, float baseMapMode, float progress, float sliceCount)
    {
        material.SetFloat("_BaseMapMode", baseMapMode);
        material.SetFloat("_BaseMapProgress", progress);
        material.SetFloat("_BaseMapSliceCount", sliceCount);
    }

    private static void ConfigureSpecular(Material material, Color specularColor, float smoothness)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.28f, 0.28f, 0.28f, 1f)));
        material.SetFloat("_LitWorkflowMode", 0f);
        material.SetFloat("_SpecularHighlights", 1f);
        material.SetFloat("_EnvironmentReflections", 0f);
        material.SetColor("_SpecularColor", specularColor);
        material.SetFloat("_Smoothness", smoothness);
    }

    private static RenderSample RenderLit(
        Action<Material> configureMaterial,
        LightCase? lightCase = null,
        Color? ambient = null,
        Mesh mesh = null,
        Color? background = null)
    {
        var shader = Shader.Find(LitShaderName);
        Assert.That(shader, Is.Not.Null, LitShaderName);

        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        var previousAmbientIntensity = RenderSettings.ambientIntensity;
        var material = new Material(shader);
        var actualMesh = mesh ?? CreateQuad(Color.white);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitSurfaceCamera");
        var meshObject = new GameObject("NovaBuiltInUberLitSurfaceMesh");
        var lightObject = new GameObject("NovaBuiltInUberLitSurfaceLight");

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient ?? Color.black;
            RenderSettings.ambientIntensity = 1f;

            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            material.SetFloat("_RenderType", 0f);
            configureMaterial(material);
            SetupUberLit(material);

            meshObject.AddComponent<MeshFilter>().sharedMesh = actualMesh;
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            var selectedLight = lightCase ?? LightCase.Front(Color.white, 2.2f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = selectedLight.Color;
            light.intensity = selectedLight.Intensity;
            lightObject.transform.rotation = selectedLight.Rotation;

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 1.25f;
            camera.transform.position = new Vector3(0f, 0f, -3f);
            camera.transform.rotation = Quaternion.identity;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background ?? new Color(0.01f, 0.01f, 0.01f, 1f);
            camera.targetTexture = target;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.Render();

            var previousActive = RenderTexture.active;
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, RenderSize, RenderSize), 0, 0);
            readback.Apply();
            RenderTexture.active = previousActive;

            return new RenderSample(
                AverageRegion(readback, 0.40f, 0.60f, 0.40f, 0.60f),
                AverageRegion(readback, 0.20f, 0.40f, 0.40f, 0.60f),
                AverageRegion(readback, 0.60f, 0.80f, 0.40f, 0.60f));
        }
        finally
        {
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            RenderSettings.ambientIntensity = previousAmbientIntensity;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(meshObject);
            UnityEngine.Object.DestroyImmediate(lightObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(actualMesh);
            DestroyMaterialAndOwnedTextures(material);
        }
    }

    private static Color AverageRegion(Texture2D texture, float minX, float maxX, float minY, float maxY)
    {
        var x0 = Mathf.FloorToInt(minX * texture.width);
        var x1 = Mathf.CeilToInt(maxX * texture.width);
        var y0 = Mathf.FloorToInt(minY * texture.height);
        var y1 = Mathf.CeilToInt(maxY * texture.height);
        var sum = Color.clear;
        var count = 0;

        for (var y = y0; y < y1; y++)
        {
            for (var x = x0; x < x1; x++)
            {
                sum += texture.GetPixel(x, y);
                count++;
            }
        }

        return sum / Mathf.Max(count, 1);
    }

    private static Texture2D CreateSolidTexture(Color color)
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(Enumerable.Repeat(color, TextureSize * TextureSize).ToArray());
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateSplitTexture(Color left, Color right)
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };

        for (var y = 0; y < TextureSize; y++)
        {
            for (var x = 0; x < TextureSize; x++)
            {
                texture.SetPixel(x, y, x < TextureSize / 2 ? left : right);
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture2DArray CreateTextureArray(params Color[] slices)
    {
        var texture = new Texture2DArray(TextureSize, TextureSize, slices.Length, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };

        for (var slice = 0; slice < slices.Length; slice++)
        {
            texture.SetPixels(Enumerable.Repeat(slices[slice], TextureSize * TextureSize).ToArray(), slice);
        }

        texture.Apply();
        return texture;
    }

    private static Texture2DArray CreateSplitTextureArray(Color left, Color right)
    {
        var texture = new Texture2DArray(TextureSize, TextureSize, 2, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };

        for (var slice = 0; slice < 2; slice++)
        {
            var pixels = new Color[TextureSize * TextureSize];
            for (var y = 0; y < TextureSize; y++)
            {
                for (var x = 0; x < TextureSize; x++)
                {
                    pixels[y * TextureSize + x] = x < TextureSize / 2 ? left : right;
                }
            }

            texture.SetPixels(pixels, slice);
        }

        texture.Apply();
        return texture;
    }

    private static Texture3D CreateTexture3D(params Color[] slices)
    {
        var texture = new Texture3D(TextureSize, TextureSize, slices.Length, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        var pixels = new Color[TextureSize * TextureSize * slices.Length];

        for (var z = 0; z < slices.Length; z++)
        {
            for (var y = 0; y < TextureSize; y++)
            {
                for (var x = 0; x < TextureSize; x++)
                {
                    pixels[z * TextureSize * TextureSize + y * TextureSize + x] = slices[z];
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private static Texture3D CreateSplitTexture3D(Color left, Color right)
    {
        var texture = new Texture3D(TextureSize, TextureSize, 2, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        var pixels = new Color[TextureSize * TextureSize * 2];

        for (var z = 0; z < 2; z++)
        {
            for (var y = 0; y < TextureSize; y++)
            {
                for (var x = 0; x < TextureSize; x++)
                {
                    pixels[z * TextureSize * TextureSize + y * TextureSize + x] = x < TextureSize / 2 ? left : right;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private static Mesh CreateQuad(Color vertexColor)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberLitSurfaceQuad" };
        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
        };
        mesh.colors = Enumerable.Repeat(vertexColor, 4).ToArray();
        mesh.normals = Enumerable.Repeat(Vector3.back, 4).ToArray();
        mesh.tangents = Enumerable.Repeat(new Vector4(1f, 0f, 0f, 1f), 4).ToArray();
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateBounds();
        return mesh;
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

    private static float Luminance(Color color)
    {
        return color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
    }

    private static float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
    }

    private static void DestroyMaterialAndOwnedTextures(Material material)
    {
        foreach (var property in new[]
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
                 })
        {
            if (!material.HasProperty(property))
            {
                continue;
            }

            var texture = material.GetTexture(property);
            if (texture != null)
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        UnityEngine.Object.DestroyImmediate(material);
    }

    private readonly struct RenderSample
    {
        public RenderSample(Color center, Color left, Color right)
        {
            Center = center;
            Left = left;
            Right = right;
        }

        public Color Center { get; }
        public Color Left { get; }
        public Color Right { get; }
    }

    private readonly struct LightCase
    {
        private LightCase(Quaternion rotation, Color color, float intensity)
        {
            Rotation = rotation;
            Color = color;
            Intensity = intensity;
        }

        public Quaternion Rotation { get; }
        public Color Color { get; }
        public float Intensity { get; }

        public static LightCase Front(Color color, float intensity)
        {
            return new LightCase(Quaternion.LookRotation(Vector3.forward), color, intensity);
        }

        public static LightCase Angled(Color color, float intensity)
        {
            return new LightCase(Quaternion.LookRotation(new Vector3(-0.45f, 0f, 0.89f)), color, intensity);
        }
    }
}

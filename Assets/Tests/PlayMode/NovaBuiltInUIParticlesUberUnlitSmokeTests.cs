using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUIParticlesUberUnlitSmokeTests
{
    private const string ShaderName = "Nova/UIParticles/UberUnlit";
    private const int TextureSize = 4;
    private const int RenderSize = 64;

    [Test]
    public void UIParticlesUberUnlitRendersBaseMap2D()
    {
        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.08f, 0.05f, 1f)));
            material.SetFloat("_BaseMapMode", 0);
        });

        Assert.That(color.r, Is.GreaterThan(0.45f), color.ToString());
        Assert.That(color.r, Is.GreaterThan(color.g + 0.15f), color.ToString());
        Assert.That(color.r, Is.GreaterThan(color.b + 0.15f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitTintAffectsOutput()
    {
        var untinted = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
            material.SetFloat("_BaseMapMode", 0);
        });
        var tinted = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_TintAreaMode", 1);
            material.SetFloat("_TintColorMode", 0);
            material.SetColor("_TintColor", new Color(0.15f, 0.9f, 0.15f, 1f));
        });

        Assert.That(tinted.g, Is.GreaterThan(tinted.r + 0.2f), tinted.ToString());
        Assert.That(tinted.g, Is.GreaterThan(tinted.b + 0.2f), tinted.ToString());
        Assert.That(tinted.r, Is.LessThan(untinted.r - 0.15f), $"Untinted {untinted}, tinted {tinted}");
    }

    [Test]
    public void UIParticlesUberUnlitVertexColorAffectsOutput()
    {
        var color = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                material.SetFloat("_BaseMapMode", 0);
            },
            mesh: CreateQuad(new Color(0.15f, 0.9f, 0.15f, 1f)));

        Assert.That(color.g, Is.GreaterThan(color.r + 0.2f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.2f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitVertexAlphaAffectsOutput()
    {
        var background = new Color(0.05f, 0.1f, 0.8f, 1f);
        var opaque = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0.05f, 0.05f, 1f)));
                material.SetFloat("_BaseMapMode", 0);
            },
            mesh: CreateQuad(Color.white),
            background: background);
        var transparent = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0.05f, 0.05f, 1f)));
                material.SetFloat("_BaseMapMode", 0);
            },
            mesh: CreateQuad(new Color(1f, 1f, 1f, 0.2f)),
            background: background);

        Assert.That(opaque.r, Is.GreaterThan(transparent.r + 0.2f), $"Opaque {opaque}, transparent {transparent}");
        Assert.That(transparent.b, Is.GreaterThan(opaque.b + 0.2f), $"Opaque {opaque}, transparent {transparent}");
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTestClipsBelowCutoff()
    {
        var background = new Color(0.05f, 0.1f, 0.8f, 1f);
        var color = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0.05f, 0.05f, 0.25f)));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_RenderType", 1);
                material.SetFloat("_Cutoff", 0.5f);
            },
            background: background);

        Assert.That(color.b, Is.GreaterThan(color.r + 0.25f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.g + 0.25f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitRendersBaseMap2DArray()
    {
        var texture = CreateTexture2DArray(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.9f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.9f, 1f));

        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap2DArray", texture);
            material.SetFloat("_BaseMapMode", 1);
            material.SetFloat("_BaseMapProgress", 0.45f);
            material.SetFloat("_BaseMapSliceCount", texture.depth);
        });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.2f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.2f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitRendersBaseMap3D()
    {
        var texture = CreateTexture3D(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.9f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.9f, 1f));

        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap3D", texture);
            material.SetFloat("_BaseMapMode", 2);
            material.SetFloat("_BaseMapProgress", 0.45f);
            material.SetFloat("_BaseMapSliceCount", texture.depth);
        });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.12f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.12f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitBaseMapChannelExtractionAffectsOutput()
    {
        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.82f, 0.18f, 1f)));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_BaseMapChannel", 2);
        });

        Assert.That(Mathf.Abs(color.r - color.g), Is.LessThan(0.08f), color.ToString());
        Assert.That(Mathf.Abs(color.g - color.b), Is.LessThan(0.08f), color.ToString());
        Assert.That(color.r, Is.GreaterThan(0.45f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitBaseMapRandomRowSelectionAffectsOutput()
    {
        var texture = CreateTexture2DArray(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.9f, 0.08f, 0.05f, 1f),
            new Color(0.05f, 0.82f, 0.05f, 1f),
            new Color(0.05f, 0.9f, 0.05f, 1f));

        var color = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap2DArray", texture);
                material.SetFloat("_BaseMapMode", 1);
                material.SetFloat("_BaseMapProgress", 0.1f);
                material.SetFloat("_BaseMapSliceCount", texture.depth);
                material.SetFloat("_BaseMapRandomRowSelectionEnabled", 1);
                material.SetFloat("_BaseMapRandomRowCoord", 1);
                material.SetFloat("_BaseMapRowCount", 2);
            },
            mesh: CreateQuad(Color.white, customCoord1: new Vector4(1.2f, 0f, 0f, 0f)));

        Assert.That(color.g, Is.GreaterThan(color.r + 0.2f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.2f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitTritoneAffectsOutput()
    {
        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f)));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_BaseMapToneMode", 1);
            material.SetFloat("_BaseMapToneChannel", 0);
            material.SetColor("_BaseMapToneShadowsColor", new Color(0.9f, 0.05f, 0.05f, 1f));
            material.SetColor("_BaseMapToneMidtonesColor", new Color(0.05f, 0.9f, 0.05f, 1f));
            material.SetColor("_BaseMapToneHighlightsColor", new Color(0.05f, 0.05f, 0.9f, 1f));
            material.SetFloat("_BaseMapToneShadows", 0.2f);
            material.SetFloat("_BaseMapToneMidtones", 0.5f);
            material.SetFloat("_BaseMapToneHighlights", 0.8f);
        });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.15f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitPentoneAffectsOutput()
    {
        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.65f, 0.65f, 0.65f, 1f)));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_BaseMapToneMode", 2);
            material.SetFloat("_BaseMapToneChannel", 0);
            material.SetColor("_BaseMapToneShadowsColor", new Color(0.9f, 0.05f, 0.05f, 1f));
            material.SetColor("_BaseMapToneDarktonesColor", new Color(0.9f, 0.9f, 0.05f, 1f));
            material.SetColor("_BaseMapToneMidtonesColor", new Color(0.05f, 0.9f, 0.05f, 1f));
            material.SetColor("_BaseMapToneBrightsColor", new Color(0.05f, 0.85f, 0.85f, 1f));
            material.SetColor("_BaseMapToneHighlightsColor", new Color(0.05f, 0.05f, 0.9f, 1f));
            material.SetFloat("_BaseMapToneShadows", 0.1f);
            material.SetFloat("_BaseMapToneMidtones", 0.5f);
            material.SetFloat("_BaseMapToneHighlights", 0.9f);
            material.SetFloat("_BaseMapToneDarktones", 0.5f);
            material.SetFloat("_BaseMapToneBrights", 0.5f);
        });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.1f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.r + 0.1f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitTintMapAffectsOutput()
    {
        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_TintAreaMode", 1);
            material.SetFloat("_TintColorMode", 1);
            material.SetTexture("_TintMap", CreateSolidTexture(new Color(0.08f, 0.9f, 0.08f, 1f)));
            material.SetFloat("_TintBlendRate", 1);
        });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.2f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.2f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitTintMap2DArrayAffectsOutput()
    {
        var tintTexture = CreateTexture2DArray(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.85f, 1f));

        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_TintAreaMode", 1);
            material.SetFloat("_TintColorMode", 3);
            material.SetTexture("_TintMap2DArray", tintTexture);
            material.SetFloat("_TintMapProgress", 0.45f);
            material.SetFloat("_TintMapSliceCount", tintTexture.depth);
            material.SetFloat("_TintBlendRate", 1);
        });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.15f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitTintMap3DAffectsOutput()
    {
        var tintTexture = CreateTexture3D(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.85f, 1f));

        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_TintAreaMode", 1);
            material.SetFloat("_TintColorMode", 2);
            material.SetTexture("_TintMap3D", tintTexture);
            material.SetFloat("_TintMap3DProgress", 0.45f);
            material.SetFloat("_TintMapSliceCount", tintTexture.depth);
            material.SetFloat("_TintBlendRate", 1);
        });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.12f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.12f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitTintMapRandomRowSelectionAffectsOutput()
    {
        var tintTexture = CreateTexture2DArray(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.9f, 0.08f, 0.05f, 1f),
            new Color(0.05f, 0.82f, 0.05f, 1f),
            new Color(0.05f, 0.9f, 0.05f, 1f));

        var color = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_TintAreaMode", 1);
                material.SetFloat("_TintColorMode", 3);
                material.SetTexture("_TintMap2DArray", tintTexture);
                material.SetFloat("_TintMapProgress", 0.1f);
                material.SetFloat("_TintMapSliceCount", tintTexture.depth);
                material.SetFloat("_TintMapRandomRowSelectionEnabled", 1);
                material.SetFloat("_TintMapRandomRowCoord", 1);
                material.SetFloat("_TintMapRowCount", 2);
                material.SetFloat("_TintBlendRate", 1);
            },
            mesh: CreateQuad(Color.white, customCoord1: new Vector4(1.2f, 0f, 0f, 0f)));

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.15f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitTintRimAffectsOutput()
    {
        var color = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_TintAreaMode", 2);
                material.SetFloat("_TintColorMode", 0);
                material.SetColor("_TintColor", new Color(0.08f, 0.9f, 0.08f, 1f));
                material.SetFloat("_TintBlendRate", 1);
                material.SetFloat("_TintRimProgress", 0.5f);
                material.SetFloat("_TintRimSharpness", 1f);
                material.SetFloat("_InverseTintRim", 1f);
            },
            mesh: CreateQuad(Color.white, normal: Vector3.back));

        Assert.That(color.g, Is.GreaterThan(color.r + 0.2f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.2f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitFlowMapOffsetsBaseSampling()
    {
        var baseTexture = CreateHorizontalSplitTexture(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            rightColorStartX: TextureSize - 1);
        var flowTexture = CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f));

        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", baseTexture);
            material.SetFloat("_BaseMapMode", 0);
            material.SetTexture("_FlowMap", flowTexture);
            material.SetFloat("_FlowMapTarget", 1);
            material.SetFloat("_FlowMapChannelsX", 0);
            material.SetFloat("_FlowMapChannelsY", 1);
            material.SetFloat("_FlowIntensity", 0.4f);
        });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitFlowMapOffsetsTintSampling()
    {
        var tintTexture = CreateHorizontalSplitTexture(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            rightColorStartX: TextureSize - 1);
        var flowTexture = CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f));

        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_TintAreaMode", 1);
            material.SetFloat("_TintColorMode", 1);
            material.SetTexture("_TintMap", tintTexture);
            material.SetFloat("_TintBlendRate", 1);
            material.SetTexture("_FlowMap", flowTexture);
            material.SetFloat("_FlowMapTarget", 2);
            material.SetFloat("_FlowMapChannelsX", 0);
            material.SetFloat("_FlowMapChannelsY", 1);
            material.SetFloat("_FlowIntensity", 0.4f);
        });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitParallax2DOffsetsBaseSampling()
    {
        var baseTexture = CreateHorizontalSplitTexture(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            rightColorStartX: TextureSize / 2);
        var parallaxTexture = CreateSolidTexture(Color.white);

        var color = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", baseTexture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetTexture("_ParallaxMap", parallaxTexture);
                material.SetFloat("_ParallaxMapMode", 0);
                material.SetFloat("_ParallaxMapTarget", 1);
                material.SetFloat("_ParallaxMapChannel", 0);
                material.SetFloat("_ParallaxStrength", 1f);
            },
            mesh: CreateQuad(Color.white, skewForParallax: true));

        Assert.That(color.g, Is.GreaterThan(color.r + 0.1f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitGreyscaleAffectsOutput()
    {
        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.05f, 0.05f, 1f)));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_ColorCorrectionMode", 1);
        });

        Assert.That(Mathf.Abs(color.r - color.g), Is.LessThan(0.08f), color.ToString());
        Assert.That(Mathf.Abs(color.g - color.b), Is.LessThan(0.08f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitGradientMapAffectsOutput()
    {
        var gradientTexture = CreateHorizontalSplitTexture(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.85f, 1f),
            rightColorStartX: TextureSize / 2);

        var color = RenderUiUberUnlit(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.05f, 0.85f, 0.05f, 1f)));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_ColorCorrectionMode", 2);
            material.SetTexture("_GradientMap", gradientTexture);
        });

        Assert.That(color.b, Is.GreaterThan(color.r + 0.15f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.g + 0.05f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitStencilMaskClipsOutput()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateStencilMask(new Rect(-0.5f, -0.5f, 0.5f, 1f)),
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.08f, 0.05f, 1f)));
                    material.SetFloat("_BaseMapMode", 0);
                    material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                    material.SetFloat("_Stencil", 1);
                    material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                    material.SetFloat("_StencilReadMask", 255);
                    material.SetFloat("_StencilWriteMask", 255);
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[0].r, Is.GreaterThan(samples[0].b + 0.2f), $"Inside {samples[0]}, outside {samples[1]}");
        Assert.That(samples[1].b, Is.GreaterThan(samples[1].r + 0.02f), $"Inside {samples[0]}, outside {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitRectClipClipsOutput()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.08f, 0.05f, 1f)));
                    material.SetFloat("_BaseMapMode", 0);
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-0.05f, -1f, 1f, 1f));
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[1].r, Is.GreaterThan(samples[1].b + 0.2f), $"Clipped {samples[0]}, visible {samples[1]}");
        Assert.That(samples[0].b, Is.GreaterThan(samples[0].r + 0.02f), $"Clipped {samples[0]}, visible {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitBaseTextureFeaturesRespectRectClip()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var texture = CreateTexture2DArray(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.9f, 0.05f, 1f));
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap2DArray", texture);
                    material.SetFloat("_BaseMapMode", 1);
                    material.SetFloat("_BaseMapProgress", 0.75f);
                    material.SetFloat("_BaseMapSliceCount", texture.depth);
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-0.05f, -1f, 1f, 1f));
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[1].g, Is.GreaterThan(samples[1].r + 0.2f), $"Clipped {samples[0]}, visible {samples[1]}");
        Assert.That(samples[0].b, Is.GreaterThan(samples[0].g + 0.02f), $"Clipped {samples[0]}, visible {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitBaseTextureFeaturesPreserveStencilOrMaskBehavior()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateStencilMask(new Rect(-0.5f, -0.5f, 0.5f, 1f)),
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f)));
                    material.SetFloat("_BaseMapMode", 0);
                    material.SetFloat("_BaseMapToneMode", 1);
                    material.SetFloat("_BaseMapToneChannel", 0);
                    material.SetColor("_BaseMapToneShadowsColor", new Color(0.9f, 0.05f, 0.05f, 1f));
                    material.SetColor("_BaseMapToneMidtonesColor", new Color(0.05f, 0.9f, 0.05f, 1f));
                    material.SetColor("_BaseMapToneHighlightsColor", new Color(0.05f, 0.05f, 0.9f, 1f));
                    material.SetFloat("_BaseMapToneShadows", 0.2f);
                    material.SetFloat("_BaseMapToneMidtones", 0.5f);
                    material.SetFloat("_BaseMapToneHighlights", 0.8f);
                    material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                    material.SetFloat("_Stencil", 1);
                    material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                    material.SetFloat("_StencilReadMask", 255);
                    material.SetFloat("_StencilWriteMask", 255);
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[0].g, Is.GreaterThan(samples[0].r + 0.15f), $"Inside {samples[0]}, outside {samples[1]}");
        Assert.That(samples[1].b, Is.GreaterThan(samples[1].g + 0.02f), $"Inside {samples[0]}, outside {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitTintFlowFeaturesRespectRectClip()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var tintTexture = CreateHorizontalSplitTexture(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            rightColorStartX: TextureSize - 1);
        var flowTexture = CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f));
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                    material.SetFloat("_BaseMapMode", 0);
                    material.SetFloat("_TintAreaMode", 1);
                    material.SetFloat("_TintColorMode", 1);
                    material.SetTexture("_TintMap", tintTexture);
                    material.SetFloat("_TintBlendRate", 1);
                    material.SetTexture("_FlowMap", flowTexture);
                    material.SetFloat("_FlowMapTarget", 2);
                    material.SetFloat("_FlowMapChannelsX", 0);
                    material.SetFloat("_FlowMapChannelsY", 1);
                    material.SetFloat("_FlowIntensity", 0.4f);
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-0.05f, -1f, 1f, 1f));
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[1].g, Is.GreaterThan(samples[1].r + 0.15f), $"Clipped {samples[0]}, visible {samples[1]}");
        Assert.That(samples[0].b, Is.GreaterThan(samples[0].g + 0.02f), $"Clipped {samples[0]}, visible {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitTintFlowFeaturesPreserveStencilOrMaskBehavior()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateStencilMask(new Rect(-0.5f, -0.5f, 0.5f, 1f)),
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.05f, 0.85f, 0.05f, 1f)));
                    material.SetFloat("_BaseMapMode", 0);
                    material.SetFloat("_ColorCorrectionMode", 2);
                    material.SetTexture("_GradientMap", CreateHorizontalSplitTexture(
                        new Color(0.85f, 0.05f, 0.05f, 1f),
                        new Color(0.05f, 0.05f, 0.85f, 1f),
                        rightColorStartX: TextureSize / 2));
                    material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                    material.SetFloat("_Stencil", 1);
                    material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                    material.SetFloat("_StencilReadMask", 255);
                    material.SetFloat("_StencilWriteMask", 255);
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[0].b, Is.GreaterThan(samples[0].r + 0.15f), $"Inside {samples[0]}, outside {samples[1]}");
        Assert.That(samples[1].b, Is.GreaterThan(samples[1].r + 0.02f), $"Inside {samples[0]}, outside {samples[1]}");
    }

    private static Color RenderUiUberUnlit(
        Action<Material> configureMaterial,
        Mesh mesh = null,
        Color? background = null)
    {
        return RenderUiSceneSamples(
            new[] { CreateUiDraw(configureMaterial, mesh) },
            new[] { new Vector2(0.5f, 0.5f) },
            background ?? new Color(0.01f, 0.01f, 0.01f, 1f))[0];
    }

    private static UiDraw CreateUiDraw(Action<Material> configureMaterial, Mesh mesh = null)
    {
        return new UiDraw(configureMaterial, mesh ?? CreateQuad(Color.white));
    }

    private static UiDraw CreateStencilMask(Rect rect)
    {
        return new UiDraw(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_StencilComp", (float)CompareFunction.Always);
                material.SetFloat("_Stencil", 1);
                material.SetFloat("_StencilOp", (float)StencilOp.Replace);
                material.SetFloat("_StencilReadMask", 255);
                material.SetFloat("_StencilWriteMask", 255);
                material.SetFloat("_ColorMask", 0);
            },
            CreateQuad(Color.white, rect),
            renderQueue: 2999);
    }

    private static Color[] RenderUiSceneSamples(
        IReadOnlyList<UiDraw> draws,
        IReadOnlyList<Vector2> normalizedSamplePositions,
        Color background)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUIParticlesUberUnlitCamera");
        var ownedObjects = new List<UnityEngine.Object> { target, readback, cameraObject };
        var previous = RenderTexture.active;

        try
        {
            foreach (var draw in draws)
            {
                var material = new Material(shader);
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetFloat("_Cull", (float)CullMode.Off);
                material.SetFloat("_ZTest", (float)CompareFunction.Always);
                draw.ConfigureMaterial(material);
                SetupUberUnlit(material);
                if (draw.RenderQueue.HasValue)
                {
                    material.renderQueue = draw.RenderQueue.Value;
                }

                var gameObject = new GameObject("NovaBuiltInUIParticlesUberUnlitDraw");
                var filter = gameObject.AddComponent<MeshFilter>();
                filter.sharedMesh = draw.Mesh;
                var renderer = gameObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                ownedObjects.Add(material);
                ownedObjects.Add(draw.Mesh);
                ownedObjects.Add(gameObject);
                ownedObjects.AddRange(material.GetTexturePropertyNames()
                    .Select(material.GetTexture)
                    .Where(texture => texture != null));
            }

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.orthographic = true;
            camera.orthographicSize = 0.75f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 10f;
            camera.transform.position = new Vector3(0f, 0f, -2f);
            camera.targetTexture = target;
            camera.Render();

            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, RenderSize, RenderSize), 0, 0);
            readback.Apply();

            return normalizedSamplePositions
                .Select(position =>
                {
                    var x = Mathf.Clamp(Mathf.RoundToInt(position.x * (RenderSize - 1)), 0, RenderSize - 1);
                    var y = Mathf.Clamp(Mathf.RoundToInt(position.y * (RenderSize - 1)), 0, RenderSize - 1);
                    return readback.GetPixel(x, y);
                })
                .ToArray();
        }
        finally
        {
            RenderTexture.active = previous;
            var camera = cameraObject.GetComponent<Camera>();
            if (camera != null)
            {
                camera.targetTexture = null;
            }

            foreach (var ownedObject in ownedObjects.Where(ownedObject => ownedObject != null).Distinct())
            {
                UnityEngine.Object.DestroyImmediate(ownedObject);
            }
        }
    }

    private static Mesh CreateQuad(Color vertexColor, Vector4? customCoord1 = null, bool skewForParallax = false, Vector3? normal = null)
    {
        return CreateQuad(vertexColor, new Rect(-0.5f, -0.5f, 1f, 1f), customCoord1, skewForParallax, normal);
    }

    private static Mesh CreateQuad(Color vertexColor, Rect rect, Vector4? customCoord1 = null, bool skewForParallax = false, Vector3? normal = null)
    {
        var mesh = new Mesh { name = "NovaBuiltInUIParticlesUberUnlitQuad" };
        mesh.vertices = new[]
        {
            new Vector3(rect.xMin, rect.yMin, skewForParallax ? -0.2f : 0f),
            new Vector3(rect.xMax, rect.yMin, skewForParallax ? 0.2f : 0f),
            new Vector3(rect.xMax, rect.yMax, skewForParallax ? 0.2f : 0f),
            new Vector3(rect.xMin, rect.yMax, skewForParallax ? -0.2f : 0f),
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
        };
        mesh.colors = Enumerable.Repeat(vertexColor, 4).ToArray();
        mesh.SetUVs(1, Enumerable.Repeat(customCoord1 ?? Vector4.zero, 4).ToList());
        mesh.normals = Enumerable.Repeat((normal ?? Vector3.back).normalized, 4).ToArray();
        var tangent = skewForParallax ? new Vector4(0.45f, 0f, 0.89f, 1f) : new Vector4(1f, 0f, 0f, 1f);
        mesh.tangents = Enumerable.Repeat(tangent, 4).ToArray();
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Texture2D CreateHorizontalSplitTexture(Color leftColor, Color rightColor, int rightColorStartX)
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
                texture.SetPixel(x, y, x >= rightColorStartX ? rightColor : leftColor);
            }
        }

        texture.Apply();
        return texture;
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

    private static Texture2DArray CreateTexture2DArray(params Color[] sliceColors)
    {
        var texture = new Texture2DArray(TextureSize, TextureSize, sliceColors.Length, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };

        for (var slice = 0; slice < sliceColors.Length; slice++)
        {
            texture.SetPixels(Enumerable.Repeat(sliceColors[slice], TextureSize * TextureSize).ToArray(), slice);
        }

        texture.Apply();
        return texture;
    }

    private static Texture3D CreateTexture3D(params Color[] depthColors)
    {
        var texture = new Texture3D(TextureSize, TextureSize, depthColors.Length, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        var pixels = new Color[TextureSize * TextureSize * depthColors.Length];
        for (var z = 0; z < depthColors.Length; z++)
        {
            for (var y = 0; y < TextureSize; y++)
            {
                for (var x = 0; x < TextureSize; x++)
                {
                    pixels[x + TextureSize * (y + TextureSize * z)] = depthColors[z];
                }
            }
        }

        texture.SetPixels(pixels);
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

    private sealed class UiDraw
    {
        public UiDraw(Action<Material> configureMaterial, Mesh mesh, int? renderQueue = null)
        {
            ConfigureMaterial = configureMaterial;
            Mesh = mesh;
            RenderQueue = renderQueue;
        }

        public Action<Material> ConfigureMaterial { get; }
        public Mesh Mesh { get; }
        public int? RenderQueue { get; }
    }
}

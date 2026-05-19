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
    public void UIParticlesUberUnlitFadeAlphaTransitionAffectsOutput()
    {
        var visible = RenderUiAlphaTransition(CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f)), progress: 0f, mode: 1);
        var faded = RenderUiAlphaTransition(CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f)), progress: 1f, mode: 1);

        Assert.That(visible.r, Is.GreaterThan(faded.r + 0.25f), $"Visible {visible}, faded {faded}");
    }

    [Test]
    public void UIParticlesUberUnlitDissolveAlphaTransitionClipsOrAttenuatesOutput()
    {
        var attenuated = RenderUiAlphaTransition(CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 1f)), progress: 0.5f, mode: 2);
        var retained = RenderUiAlphaTransition(CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)), progress: 0.5f, mode: 2);

        Assert.That(retained.r, Is.GreaterThan(attenuated.r + 0.25f), $"Retained {retained}, attenuated {attenuated}");
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTransitionMap2DAffectsOutput()
    {
        var low = RenderUiAlphaTransition(CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 1f)), progress: 0.5f, mode: 2);
        var high = RenderUiAlphaTransition(CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)), progress: 0.5f, mode: 2);

        Assert.That(high.r, Is.GreaterThan(low.r + 0.25f), $"High {high}, low {low}");
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTransitionMap2DArrayAffectsOutput()
    {
        var lowTexture = CreateTexture2DArray(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.1f, 0.1f, 0.1f, 1f));
        var highTexture = CreateTexture2DArray(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.9f, 0.9f, 0.9f, 1f),
            new Color(0.1f, 0.1f, 0.1f, 1f));

        var low = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 1, m =>
            {
                m.SetTexture("_AlphaTransitionMap2DArray", lowTexture);
                m.SetFloat("_AlphaTransitionMapProgress", 0.45f);
                m.SetFloat("_AlphaTransitionMapSliceCount", lowTexture.depth);
            }),
            background: Color.black);
        var high = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 1, m =>
            {
                m.SetTexture("_AlphaTransitionMap2DArray", highTexture);
                m.SetFloat("_AlphaTransitionMapProgress", 0.45f);
                m.SetFloat("_AlphaTransitionMapSliceCount", highTexture.depth);
            }),
            background: Color.black);

        Assert.That(high.r, Is.GreaterThan(low.r + 0.25f), $"High {high}, low {low}");
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTransitionMap3DAffectsOutput()
    {
        var lowTexture = CreateTexture3D(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.1f, 0.1f, 0.1f, 1f));
        var highTexture = CreateTexture3D(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.9f, 0.9f, 0.9f, 1f),
            new Color(0.1f, 0.1f, 0.1f, 1f));

        var low = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 2, m =>
            {
                m.SetTexture("_AlphaTransitionMap3D", lowTexture);
                m.SetFloat("_AlphaTransitionMapProgress", 0.45f);
                m.SetFloat("_AlphaTransitionMapSliceCount", lowTexture.depth);
            }),
            background: Color.black);
        var high = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 2, m =>
            {
                m.SetTexture("_AlphaTransitionMap3D", highTexture);
                m.SetFloat("_AlphaTransitionMapProgress", 0.45f);
                m.SetFloat("_AlphaTransitionMapSliceCount", highTexture.depth);
            }),
            background: Color.black);

        Assert.That(high.r, Is.GreaterThan(low.r + 0.25f), $"High {high}, low {low}");
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTransitionRandomRowSelectionAffectsOutput()
    {
        var lowRowTexture = CreateTexture2DArray(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.15f, 0.15f, 0.15f, 1f),
            new Color(0.85f, 0.85f, 0.85f, 1f),
            new Color(0.9f, 0.9f, 0.9f, 1f));
        var highRowTexture = CreateTexture2DArray(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.15f, 0.15f, 0.15f, 1f),
            new Color(0.85f, 0.85f, 0.85f, 1f),
            new Color(0.9f, 0.9f, 0.9f, 1f));

        var lowRow = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 1, m =>
            {
                m.SetTexture("_AlphaTransitionMap2DArray", lowRowTexture);
                m.SetFloat("_AlphaTransitionMapProgress", 0.1f);
                m.SetFloat("_AlphaTransitionMapSliceCount", lowRowTexture.depth);
                m.SetFloat("_AlphaTransitionMapRandomRowSelectionEnabled", 1);
                m.SetFloat("_AlphaTransitionMapRandomRowCoord", 1);
                m.SetFloat("_AlphaTransitionMapRowCount", 2);
            }),
            mesh: CreateQuad(Color.white, customCoord1: new Vector4(0.2f, 0f, 0f, 0f)),
            background: Color.black);
        var highRow = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 1, m =>
            {
                m.SetTexture("_AlphaTransitionMap2DArray", highRowTexture);
                m.SetFloat("_AlphaTransitionMapProgress", 0.1f);
                m.SetFloat("_AlphaTransitionMapSliceCount", highRowTexture.depth);
                m.SetFloat("_AlphaTransitionMapRandomRowSelectionEnabled", 1);
                m.SetFloat("_AlphaTransitionMapRandomRowCoord", 1);
                m.SetFloat("_AlphaTransitionMapRowCount", 2);
            }),
            mesh: CreateQuad(Color.white, customCoord1: new Vector4(1.2f, 0f, 0f, 0f)),
            background: Color.black);

        Assert.That(highRow.r, Is.GreaterThan(lowRow.r + 0.25f), $"High row {highRow}, low row {lowRow}");
    }

    [Test]
    public void UIParticlesUberUnlitSecondAlphaTransitionTexture2DAffectsOutput()
    {
        var noSecond = RenderUiAlphaTransition(CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)), progress: 0.5f, mode: 2);
        var withSecond = RenderUiSecondAlphaTransition(
            CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)),
            CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 1f)),
            secondBlendMode: 1);

        Assert.That(noSecond.r, Is.GreaterThan(withSecond.r + 0.18f), $"No second {noSecond}, with second {withSecond}");
    }

    [Test]
    public void UIParticlesUberUnlitSecondAlphaTransitionTextureAverageAffectsOutput()
    {
        var noSecond = RenderUiAlphaTransition(CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)), progress: 0.5f, mode: 2);
        var average = RenderUiSecondAlphaTransition(
            CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)),
            CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 1f)),
            secondBlendMode: 1);

        Assert.That(noSecond.r, Is.GreaterThan(average.r + 0.18f), $"No second {noSecond}, average {average}");
    }

    [Test]
    public void UIParticlesUberUnlitSecondAlphaTransitionTextureMultiplyAffectsOutput()
    {
        var multiply = RenderUiSecondAlphaTransition(
            CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)),
            CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 1f)),
            secondBlendMode: 2);

        Assert.That(multiply.r, Is.LessThan(0.2f), multiply.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitVertexAlphaCanDriveTransitionProgress()
    {
        var color = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 1, 0f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f)));
                m.SetFloat("_VertexAlphaMode", 1);
            }),
            mesh: CreateQuad(new Color(1f, 1f, 1f, 0.2f)),
            background: Color.black);

        Assert.That(color.r, Is.LessThan(0.1f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitFlowMapOffsetsAlphaTransitionSampling()
    {
        var transitionTexture = CreateHorizontalSplitTexture(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.9f, 0.9f, 0.9f, 1f),
            rightColorStartX: TextureSize - 1);
        var flowTexture = CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f));

        var noFlow = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", transitionTexture);
            }),
            background: Color.black);
        var flow = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", CreateHorizontalSplitTexture(
                    new Color(0.1f, 0.1f, 0.1f, 1f),
                    new Color(0.9f, 0.9f, 0.9f, 1f),
                    rightColorStartX: TextureSize - 1));
                m.SetTexture("_FlowMap", flowTexture);
                m.SetFloat("_FlowMapTarget", 4);
                m.SetFloat("_FlowMapChannelsX", 0);
                m.SetFloat("_FlowMapChannelsY", 1);
                m.SetFloat("_FlowIntensity", 0.4f);
            }),
            background: Color.black);

        Assert.That(flow.r, Is.GreaterThan(noFlow.r + 0.25f), $"No flow {noFlow}, flow {flow}");
    }

    [Test]
    public void UIParticlesUberUnlitEmissionAreaAllAffectsOutput()
    {
        var disabled = RenderUiEmissionAreaAll(intensity: 0f);
        var enabled = RenderUiEmissionAreaAll(intensity: 1.2f);

        Assert.That(enabled.g, Is.GreaterThan(disabled.g + 0.20f), $"Disabled {disabled}, enabled {enabled}");
        Assert.That(enabled.g, Is.GreaterThan(enabled.r + 0.12f), enabled.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionAreaAlphaUsesAlpha()
    {
        var lowAlpha = RenderUiEmissionAreaAlpha(baseAlpha: 0.15f, keepEdgeTransparency: false);
        var highAlpha = RenderUiEmissionAreaAlpha(baseAlpha: 1f, keepEdgeTransparency: false);

        Assert.That(lowAlpha.g, Is.GreaterThan(highAlpha.g + 0.20f), $"Low {lowAlpha}, high {highAlpha}");
    }

    [Test]
    public void UIParticlesUberUnlitEmissionAreaMapMasksEmission()
    {
        var emissionMap = CreateHorizontalSplitTexture(
            new Color(0f, 0f, 0f, 1f),
            new Color(1f, 0f, 0f, 1f),
            rightColorStartX: TextureSize / 2);

        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material => ConfigureUiEmission(material, 2, 0, 0, 1f, m =>
                {
                    m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.08f, 0.08f, 1f)));
                    m.SetTexture("_EmissionMap", emissionMap);
                    m.SetFloat("_EmissionMapChannelsX", 0);
                    m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                })),
            },
            new[] { new Vector2(0.25f, 0.5f), new Vector2(0.75f, 0.5f) },
            Color.black);

        Assert.That(samples[1].g, Is.GreaterThan(samples[0].g + 0.20f), $"Masked {samples[0]}, unmasked {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitEmissionMap2DAffectsOutput()
    {
        var emissionMap = CreateSolidTexture(new Color(0f, 0.9f, 0f, 1f));
        var color = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 2, 0, 0, 1f, m =>
            {
                m.SetTexture("_EmissionMap", emissionMap);
                m.SetFloat("_EmissionMapChannelsX", 1);
                m.SetColor("_EmissionColor", new Color(0.9f, 0f, 0f, 1f));
            }),
            background: Color.black);

        Assert.That(color.r, Is.GreaterThan(color.g + 0.12f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionMap2DArrayAffectsOutput()
    {
        var emissionMap = CreateTexture2DArray(
            new Color(0f, 0f, 0f, 1f),
            new Color(1f, 0f, 0f, 1f),
            new Color(0f, 0f, 0f, 1f));

        var color = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 2, 0, 1, 1f, m =>
            {
                m.SetTexture("_EmissionMap2DArray", emissionMap);
                m.SetFloat("_EmissionMapProgress", 0.45f);
                m.SetFloat("_EmissionMapSliceCount", emissionMap.depth);
                m.SetFloat("_EmissionMapChannelsX", 0);
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }),
            background: Color.black);

        Assert.That(color.g, Is.GreaterThan(color.r + 0.12f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionMap3DAffectsOutput()
    {
        var emissionMap = CreateTexture3D(
            new Color(0f, 0f, 0f, 1f),
            new Color(1f, 0f, 0f, 1f),
            new Color(0f, 0f, 0f, 1f));

        var color = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 2, 0, 2, 1f, m =>
            {
                m.SetTexture("_EmissionMap3D", emissionMap);
                m.SetFloat("_EmissionMapProgress", 0.45f);
                m.SetFloat("_EmissionMapSliceCount", emissionMap.depth);
                m.SetFloat("_EmissionMapChannelsX", 0);
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }),
            background: Color.black);

        Assert.That(color.g, Is.GreaterThan(color.r + 0.10f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionMapRandomRowSelectionAffectsOutput()
    {
        var emissionMap = CreateTexture2DArray(
            new Color(0f, 0f, 0f, 1f),
            new Color(0.1f, 0f, 0f, 1f),
            new Color(1f, 0f, 0f, 1f),
            new Color(0.9f, 0f, 0f, 1f));

        var color = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 2, 0, 1, 1f, m =>
            {
                m.SetTexture("_EmissionMap2DArray", emissionMap);
                m.SetFloat("_EmissionMapProgress", 0.1f);
                m.SetFloat("_EmissionMapSliceCount", emissionMap.depth);
                m.SetFloat("_EmissionMapRandomRowSelectionEnabled", 1);
                m.SetFloat("_EmissionMapRandomRowCoord", 1);
                m.SetFloat("_EmissionMapRowCount", 2);
                m.SetFloat("_EmissionMapChannelsX", 0);
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }),
            mesh: CreateQuad(Color.white, customCoord1: new Vector4(1.2f, 0f, 0f, 0f)),
            background: Color.black);

        Assert.That(color.g, Is.GreaterThan(color.r + 0.10f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionColorFromBaseColorAffectsOutput()
    {
        var color = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 1, 1, 0, 1f, m =>
            {
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.55f, 0.08f, 0.08f, 1f)));
            }),
            background: Color.black);

        Assert.That(color.r, Is.GreaterThan(color.g + 0.18f), color.ToString());
        Assert.That(color.r, Is.GreaterThan(color.b + 0.18f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionColorFromExplicitColorAffectsOutput()
    {
        var color = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 1, 0, 0, 1f, m =>
            {
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }),
            background: Color.black);

        Assert.That(color.g, Is.GreaterThan(color.r + 0.18f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.18f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionColorMapOrRampAffectsOutput()
    {
        var emissionMap = CreateSolidTexture(new Color(0.8f, 0f, 0f, 1f));
        var ramp = CreateHorizontalSplitTexture(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.9f, 1f),
            rightColorStartX: TextureSize / 2);

        var color = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 2, 2, 0, 1f, m =>
            {
                m.SetTexture("_EmissionMap", emissionMap);
                m.SetTexture("_EmissionColorRamp", ramp);
                m.SetFloat("_EmissionMapChannelsX", 0);
            }),
            background: Color.black);

        Assert.That(color.b, Is.GreaterThan(color.r + 0.12f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionIntensityAffectsOutput()
    {
        var low = RenderUiEmissionAreaAll(intensity: 0.15f);
        var high = RenderUiEmissionAreaAll(intensity: 1.2f);

        Assert.That(high.g, Is.GreaterThan(low.g + 0.20f), $"Low {low}, high {high}");
    }

    [Test]
    public void UIParticlesUberUnlitKeepEdgeTransparencyPreservesTransparentEdges()
    {
        var keep = RenderUiEmissionAreaAlpha(baseAlpha: 0.2f, keepEdgeTransparency: true, renderTransparent: true);
        var harden = RenderUiEmissionAreaAlpha(baseAlpha: 0.2f, keepEdgeTransparency: false, renderTransparent: true);

        Assert.That(harden.g, Is.GreaterThan(keep.g + 0.18f), $"Keep {keep}, harden {harden}");
    }

    [Test]
    public void UIParticlesUberUnlitFlowMapOffsetsEmissionSampling()
    {
        var emissionMap = CreateHorizontalSplitTexture(
            new Color(0f, 0f, 0f, 1f),
            new Color(1f, 0f, 0f, 1f),
            rightColorStartX: TextureSize - 1);
        var flowTexture = CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f));

        var color = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 2, 0, 0, 1f, m =>
            {
                m.SetTexture("_EmissionMap", emissionMap);
                m.SetFloat("_EmissionMapChannelsX", 0);
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                m.SetTexture("_FlowMap", flowTexture);
                m.SetFloat("_FlowMapTarget", 8);
                m.SetFloat("_FlowMapChannelsX", 0);
                m.SetFloat("_FlowMapChannelsY", 1);
                m.SetFloat("_FlowIntensity", 0.4f);
            }),
            background: Color.black);

        Assert.That(color.g, Is.GreaterThan(color.r + 0.12f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitRimTransparencyAffectsOutput()
    {
        var disabled = RenderUiRimTransparency(false, inverse: false, progress: 0.5f, sharpness: 0.5f, Vector3.right);
        var enabled = RenderUiRimTransparency(true, inverse: false, progress: 0.5f, sharpness: 0.5f, Vector3.right);

        Assert.That(disabled.r, Is.GreaterThan(enabled.r + 0.25f), $"Disabled {disabled}, enabled {enabled}");
    }

    [Test]
    public void UIParticlesUberUnlitInverseRimTransparencyInvertsEffect()
    {
        var normal = RenderUiRimTransparency(true, inverse: false, progress: 0.5f, sharpness: 0.5f, Vector3.right);
        var inverse = RenderUiRimTransparency(true, inverse: true, progress: 0.5f, sharpness: 0.5f, Vector3.right);

        Assert.That(inverse.r, Is.GreaterThan(normal.r + 0.25f), $"Normal {normal}, inverse {inverse}");
    }

    [Test]
    public void UIParticlesUberUnlitRimTransparencyProgressAffectsOutput()
    {
        var normal = new Vector3(0.866f, 0f, -0.5f).normalized;
        var lowProgress = RenderUiRimTransparency(true, inverse: false, progress: 0.2f, sharpness: 0.5f, normal);
        var highProgress = RenderUiRimTransparency(true, inverse: false, progress: 0.8f, sharpness: 0.5f, normal);

        Assert.That(lowProgress.r, Is.GreaterThan(highProgress.r + 0.20f), $"Low {lowProgress}, high {highProgress}");
    }

    [Test]
    public void UIParticlesUberUnlitRimTransparencySharpnessAffectsFalloff()
    {
        var normal = new Vector3(0.848f, 0f, -0.53f).normalized;
        var soft = RenderUiRimTransparency(true, inverse: false, progress: 0.5f, sharpness: 0f, normal);
        var sharp = RenderUiRimTransparency(true, inverse: false, progress: 0.5f, sharpness: 0.95f, normal);

        Assert.That(sharp.r, Is.GreaterThan(soft.r + 0.08f), $"Soft {soft}, sharp {sharp}");
    }

    [Test]
    public void UIParticlesUberUnlitLuminanceTransparencyAffectsOutput()
    {
        var disabled = RenderUiLuminanceTransparencySamples(enabled: false, inverse: false, progress: 0.5f, sharpness: 0.5f);
        var enabled = RenderUiLuminanceTransparencySamples(enabled: true, inverse: false, progress: 0.5f, sharpness: 0.5f);

        Assert.That(disabled[0].r, Is.GreaterThan(enabled[0].r + 0.12f), $"Disabled dark {disabled[0]}, enabled dark {enabled[0]}");
        Assert.That(enabled[1].r, Is.GreaterThan(enabled[0].r + 0.30f), $"Dark {enabled[0]}, bright {enabled[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitInverseLuminanceTransparencyInvertsEffect()
    {
        var samples = RenderUiLuminanceTransparencySamples(enabled: true, inverse: true, progress: 0.5f, sharpness: 0.5f);

        Assert.That(samples[0].r, Is.GreaterThan(samples[1].r + 0.12f), $"Dark {samples[0]}, bright {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitLuminanceTransparencyProgressAffectsOutput()
    {
        var lowProgress = RenderUiLuminanceTransparency(new Color(0.5f, 0.5f, 0.5f, 1f), inverse: false, progress: 0.2f, sharpness: 0.5f);
        var highProgress = RenderUiLuminanceTransparency(new Color(0.5f, 0.5f, 0.5f, 1f), inverse: false, progress: 0.8f, sharpness: 0.5f);

        Assert.That(lowProgress.r, Is.GreaterThan(highProgress.r + 0.20f), $"Low {lowProgress}, high {highProgress}");
    }

    [Test]
    public void UIParticlesUberUnlitLuminanceTransparencySharpnessAffectsOutput()
    {
        var soft = RenderUiLuminanceTransparency(new Color(0.53f, 0.53f, 0.53f, 1f), inverse: false, progress: 0.5f, sharpness: 0f);
        var sharp = RenderUiLuminanceTransparency(new Color(0.53f, 0.53f, 0.53f, 1f), inverse: false, progress: 0.5f, sharpness: 0.95f);

        Assert.That(sharp.r, Is.GreaterThan(soft.r + 0.08f), $"Soft {soft}, sharp {sharp}");
    }

    [Test]
    public void UIParticlesUberUnlitRimAndLuminanceTransparencyCanCombine()
    {
        var normal = new Vector3(0.866f, 0f, -0.5f).normalized;
        var disabled = RenderUiCombinedTransparency(rim: false, luminance: false, normal);
        var rimOnly = RenderUiCombinedTransparency(rim: true, luminance: false, normal);
        var luminanceOnly = RenderUiCombinedTransparency(rim: false, luminance: true, normal);
        var combined = RenderUiCombinedTransparency(rim: true, luminance: true, normal);

        Assert.That(disabled.r, Is.GreaterThan(combined.r + 0.15f), $"Disabled {disabled}, combined {combined}");
        Assert.That(rimOnly.r, Is.GreaterThan(combined.r + 0.08f), $"Rim {rimOnly}, combined {combined}");
        Assert.That(luminanceOnly.r, Is.GreaterThan(combined.r + 0.08f), $"Luminance {luminanceOnly}, combined {combined}");
    }

    [Test]
    public void UIParticlesUberUnlitTransparencyFeaturesRespectRectClip()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                    material.SetFloat("_BaseMapMode", 0);
                    material.SetFloat("_LuminanceTransparencyEnabled", 1f);
                    material.SetFloat("_LuminanceTransparencyProgress", 0.1f);
                    material.SetFloat("_LuminanceTransparencySharpness", 0f);
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-0.05f, -1f, 1f, 1f));
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[1].r, Is.GreaterThan(samples[0].r + 0.5f), $"Clipped {samples[0]}, visible {samples[1]}");
        Assert.That(samples[0].b, Is.GreaterThan(samples[0].r + 0.02f), $"Clipped {samples[0]}, visible {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitTransparencyFeaturesPreserveStencilOrMaskBehavior()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateStencilMask(new Rect(-0.5f, -0.5f, 0.5f, 1f)),
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                    material.SetFloat("_BaseMapMode", 0);
                    material.SetFloat("_LuminanceTransparencyEnabled", 1f);
                    material.SetFloat("_LuminanceTransparencyProgress", 0.1f);
                    material.SetFloat("_LuminanceTransparencySharpness", 0f);
                    material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                    material.SetFloat("_Stencil", 1);
                    material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                    material.SetFloat("_StencilReadMask", 255);
                    material.SetFloat("_StencilWriteMask", 255);
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[0].r, Is.GreaterThan(samples[1].r + 0.5f), $"Inside {samples[0]}, outside {samples[1]}");
        Assert.That(samples[1].b, Is.GreaterThan(samples[1].r + 0.02f), $"Inside {samples[0]}, outside {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitTransparencyPreservesEmissionAndAlphaTransition()
    {
        var faded = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 1, 0, 0, 1f, m =>
            {
                ConfigureUiAlphaTransition(m, 1, 1f, 0, alpha =>
                {
                    alpha.SetTexture("_AlphaTransitionMap", CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f)));
                });
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.08f, 0.08f, 1f)));
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                m.SetFloat("_LuminanceTransparencyEnabled", 1f);
                m.SetFloat("_LuminanceTransparencyProgress", 0.2f);
                m.SetFloat("_LuminanceTransparencySharpness", 0f);
            }),
            background: Color.black);
        var visible = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 1, 0, 0, 1f, m =>
            {
                ConfigureUiAlphaTransition(m, 1, 0f, 0, alpha =>
                {
                    alpha.SetTexture("_AlphaTransitionMap", CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f)));
                });
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.08f, 0.08f, 1f)));
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                m.SetFloat("_LuminanceTransparencyEnabled", 1f);
                m.SetFloat("_LuminanceTransparencyProgress", 0.2f);
                m.SetFloat("_LuminanceTransparencySharpness", 0f);
            }),
            background: Color.black);

        Assert.That(visible.g, Is.GreaterThan(faded.g + 0.25f), $"Visible {visible}, faded {faded}");
        Assert.That(visible.g, Is.GreaterThan(visible.r + 0.15f), visible.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitTransparencyPreservesVertexColorAlphaAndAlphaTest()
    {
        var color = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_LuminanceTransparencyEnabled", 1f);
                material.SetFloat("_LuminanceTransparencyProgress", 0.1f);
                material.SetFloat("_LuminanceTransparencySharpness", 0f);
            },
            mesh: CreateQuad(new Color(0.1f, 0.9f, 0.1f, 0.5f)),
            background: Color.black);

        Assert.That(color.g, Is.GreaterThan(color.r + 0.18f), color.ToString());
        Assert.That(color.g, Is.InRange(0.25f, 0.75f), color.ToString());

        var background = new Color(0.05f, 0.1f, 0.8f, 1f);
        var clipped = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 1f, 1f, 0.45f)));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_RenderType", 1);
                material.SetFloat("_Cutoff", 0.5f);
                material.SetFloat("_LuminanceTransparencyEnabled", 1f);
                material.SetFloat("_LuminanceTransparencyProgress", 0.1f);
                material.SetFloat("_LuminanceTransparencySharpness", 0f);
            },
            background: background);

        Assert.That(clipped.b, Is.GreaterThan(clipped.r + 0.25f), clipped.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitSoftParticlesRequireCameraDepthTexture()
    {
        var result = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        });

        Assert.That(result.CameraDepthTextureMode.HasFlag(DepthTextureMode.Depth), Is.True);
        Assert.That(result.Color.r + result.Color.g + result.Color.b, Is.GreaterThan(0.05f), result.Color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitSoftParticlesFadeNearSceneDepth()
    {
        var disabled = RenderUiDepthScene(material => ConfigureUiTransparentDepthMaterial(material), opaqueZ: 0.08f).Color;
        var enabled = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        }, opaqueZ: 0.08f).Color;

        Assert.That(disabled.r, Is.GreaterThan(enabled.r + 0.25f), $"Disabled {disabled}, enabled {enabled}");
        Assert.That(enabled.b, Is.GreaterThan(disabled.b + 0.15f), $"Disabled {disabled}, enabled {enabled}");
    }

    [Test]
    public void UIParticlesUberUnlitSoftParticlesIntensityAffectsOutput()
    {
        var low = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 0.05f);
        }, opaqueZ: 0.25f).Color;
        var high = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        }, opaqueZ: 0.25f).Color;

        Assert.That(low.r, Is.GreaterThan(high.r + 0.20f), $"Low {low}, high {high}");
        Assert.That(high.b, Is.GreaterThan(low.b + 0.10f), $"Low {low}, high {high}");
    }

    [Test]
    public void UIParticlesUberUnlitSoftParticlesDoNotAffectWhenDepthFar()
    {
        var disabled = RenderUiDepthScene(material => ConfigureUiTransparentDepthMaterial(material), opaqueZ: 4f).Color;
        var enabled = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        }, opaqueZ: 4f).Color;

        Assert.That(Mathf.Abs(disabled.r - enabled.r), Is.LessThan(0.08f), $"Disabled {disabled}, enabled {enabled}");
        Assert.That(Mathf.Abs(disabled.b - enabled.b), Is.LessThan(0.08f), $"Disabled {disabled}, enabled {enabled}");
    }

    [Test]
    public void UIParticlesUberUnlitDepthFadeAffectsOutput()
    {
        var disabled = RenderUiDepthScene(material => ConfigureUiTransparentDepthMaterial(material)).Color;
        var enabled = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            ConfigureUiDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
        }).Color;

        Assert.That(disabled.r, Is.GreaterThan(enabled.r + 0.25f), $"Disabled {disabled}, enabled {enabled}");
    }

    [Test]
    public void UIParticlesUberUnlitDepthFadeNearFarAffectOutput()
    {
        var wideRange = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            ConfigureUiDepthFade(material, near: 0.5f, far: 5f, width: 0.5f);
        }).Color;
        var nearEdge = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            ConfigureUiDepthFade(material, near: 1.9f, far: 5f, width: 0.5f);
        }).Color;

        Assert.That(wideRange.r, Is.GreaterThan(nearEdge.r + 0.20f), $"Wide {wideRange}, near edge {nearEdge}");
    }

    [Test]
    public void UIParticlesUberUnlitDepthFadeWidthAffectsFalloff()
    {
        var narrow = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            ConfigureUiDepthFade(material, near: 1.9f, far: 3f, width: 0.05f);
        }).Color;
        var wide = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            ConfigureUiDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
        }).Color;

        Assert.That(narrow.r, Is.GreaterThan(wide.r + 0.20f), $"Narrow {narrow}, wide {wide}");
    }

    [Test]
    public void UIParticlesUberUnlitDepthFadeDoesNotBreakBaseAlpha()
    {
        var disabled = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0f, 0f, 0.5f)));
        }).Color;
        var enabled = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0f, 0f, 0.5f)));
            ConfigureUiDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
        }).Color;

        Assert.That(disabled.r, Is.GreaterThan(enabled.r + 0.08f), $"Disabled {disabled}, enabled {enabled}");
        Assert.That(enabled.r, Is.LessThan(0.8f), enabled.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitSoftParticlesAndDepthFadeCanCombine()
    {
        var disabled = RenderUiDepthScene(material => ConfigureUiTransparentDepthMaterial(material), opaqueZ: 0.25f).Color;
        var softOnly = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        }, opaqueZ: 0.25f).Color;
        var depthOnly = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            ConfigureUiDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
        }, opaqueZ: 0.25f).Color;
        var combined = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
            ConfigureUiDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
        }, opaqueZ: 0.25f).Color;

        Assert.That(disabled.r, Is.GreaterThan(combined.r + 0.30f), $"Disabled {disabled}, combined {combined}");
        Assert.That(softOnly.r, Is.GreaterThan(combined.r + 0.08f), $"Soft {softOnly}, combined {combined}");
        Assert.That(depthOnly.r, Is.GreaterThan(combined.r + 0.08f), $"Depth {depthOnly}, combined {combined}");
    }

    [Test]
    public void UIParticlesUberUnlitDepthFeaturesPreserveAlphaTransition()
    {
        var visible = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            ConfigureUiAlphaTransition(material, 1, 0f, 0, alpha => alpha.SetTexture("_AlphaTransitionMap", CreateSolidTexture(Color.white)));
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 0.05f);
        }, opaqueZ: 4f).Color;
        var faded = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            ConfigureUiAlphaTransition(material, 1, 1f, 0, alpha => alpha.SetTexture("_AlphaTransitionMap", CreateSolidTexture(Color.white)));
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 0.05f);
        }, opaqueZ: 4f).Color;

        Assert.That(visible.r, Is.GreaterThan(faded.r + 0.25f), $"Visible {visible}, faded {faded}");
    }

    [Test]
    public void UIParticlesUberUnlitDepthFeaturesPreserveEmissionAndTransparency()
    {
        var withoutDepth = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            ConfigureUiEmissionAndTransparency(material);
        }, opaqueZ: 0.25f).Color;
        var withDepth = RenderUiDepthScene(material =>
        {
            ConfigureUiTransparentDepthMaterial(material);
            ConfigureUiEmissionAndTransparency(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        }, opaqueZ: 0.25f).Color;

        Assert.That(withoutDepth.g, Is.GreaterThan(0.10f), withoutDepth.ToString());
        Assert.That(withDepth.g, Is.GreaterThan(0.03f), withDepth.ToString());
        Assert.That(withoutDepth.g, Is.GreaterThan(withDepth.g + 0.05f), $"Without {withoutDepth}, with {withDepth}");
    }

    [Test]
    public void UIParticlesUberUnlitDepthFeaturesRespectRectClip()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                    material.SetFloat("_BaseMapMode", 0);
                    ConfigureUiDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-0.05f, -1f, 1f, 1f));
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[1].r, Is.GreaterThan(samples[0].r + 0.10f), $"Clipped {samples[0]}, visible {samples[1]}");
        Assert.That(samples[0].b, Is.GreaterThan(samples[0].r + 0.02f), $"Clipped {samples[0]}, visible {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitDepthFeaturesPreserveStencilOrMaskBehavior()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateStencilMask(new Rect(-0.5f, -0.5f, 0.5f, 1f)),
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                    material.SetFloat("_BaseMapMode", 0);
                    ConfigureUiDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
                    material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                    material.SetFloat("_Stencil", 1);
                    material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                    material.SetFloat("_StencilReadMask", 255);
                    material.SetFloat("_StencilWriteMask", 255);
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[0].r, Is.GreaterThan(samples[1].r + 0.10f), $"Inside {samples[0]}, outside {samples[1]}");
        Assert.That(samples[1].b, Is.GreaterThan(samples[1].r + 0.02f), $"Inside {samples[0]}, outside {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitDepthFeaturesPreserveColorMask()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var color = RenderUiUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                material.SetFloat("_BaseMapMode", 0);
                ConfigureUiDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
                material.SetFloat("_ColorMask", 0);
            },
            background: background);

        Assert.That(color.r, Is.LessThan(0.06f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.r + 0.02f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionFeaturesPreserveVertexColorAndAlpha()
    {
        var color = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 1, 0, 0, 0.6f, m =>
            {
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.35f, 0.35f, 0.35f, 1f)));
                m.SetColor("_EmissionColor", new Color(0f, 0.5f, 0f, 1f));
            }),
            mesh: CreateQuad(new Color(0.1f, 0.9f, 0.1f, 0.5f)),
            background: Color.black);

        Assert.That(color.g, Is.GreaterThan(color.r + 0.18f), color.ToString());
        Assert.That(color.g, Is.InRange(0.25f, 0.85f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionFeaturesPreserveAlphaTest()
    {
        var background = new Color(0.05f, 0.1f, 0.8f, 1f);
        var color = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 1, 0, 0, 1f, m =>
            {
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 0.2f)));
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                m.SetFloat("_RenderType", 1);
                m.SetFloat("_Cutoff", 0.5f);
            }),
            background: background);

        Assert.That(color.b, Is.GreaterThan(color.r + 0.25f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.g + 0.25f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitEmissionFeaturesRespectRectClip()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    ConfigureUiEmission(material, 2, 0, 0, 1f, m =>
                    {
                        m.SetTexture("_EmissionMap", CreateSolidTexture(new Color(1f, 0f, 0f, 1f)));
                        m.SetFloat("_EmissionMapChannelsX", 0);
                        m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                    });
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-0.05f, -1f, 1f, 1f));
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[1].g, Is.GreaterThan(samples[0].g + 0.45f), $"Clipped {samples[0]}, visible {samples[1]}");
        Assert.That(samples[0].b, Is.GreaterThan(samples[0].g + 0.02f), $"Clipped {samples[0]}, visible {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitEmissionFeaturesPreserveStencilOrMaskBehavior()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateStencilMask(new Rect(-0.5f, -0.5f, 0.5f, 1f)),
                CreateUiDraw(material =>
                {
                    ConfigureUiEmission(material, 1, 0, 0, 1f, m =>
                    {
                        m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                    });
                    material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                    material.SetFloat("_Stencil", 1);
                    material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                    material.SetFloat("_StencilReadMask", 255);
                    material.SetFloat("_StencilWriteMask", 255);
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[0].g, Is.GreaterThan(samples[1].g + 0.45f), $"Inside {samples[0]}, outside {samples[1]}");
        Assert.That(samples[1].b, Is.GreaterThan(samples[1].g + 0.02f), $"Inside {samples[0]}, outside {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitEmissionPreservesAlphaTransition()
    {
        var faded = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 1, 0, 0, 1f, m =>
            {
                ConfigureUiAlphaTransition(m, 1, 1f, 0, alpha =>
                {
                    alpha.SetTexture("_AlphaTransitionMap", CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f)));
                });
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }),
            background: Color.black);
        var visible = RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 1, 0, 0, 1f, m =>
            {
                ConfigureUiAlphaTransition(m, 1, 0f, 0, alpha =>
                {
                    alpha.SetTexture("_AlphaTransitionMap", CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f)));
                });
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }),
            background: Color.black);

        Assert.That(visible.g, Is.GreaterThan(faded.g + 0.25f), $"Visible {visible}, faded {faded}");
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTransitionFeaturesPreserveVertexColorAndAlpha()
    {
        var color = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)));
            }),
            mesh: CreateQuad(new Color(0.1f, 0.9f, 0.1f, 0.5f)),
            background: Color.black);

        Assert.That(color.g, Is.GreaterThan(color.r + 0.18f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.18f), color.ToString());
        Assert.That(color.g, Is.InRange(0.25f, 0.65f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTransitionFeaturesPreserveAlphaTest()
    {
        var background = new Color(0.05f, 0.1f, 0.8f, 1f);
        var color = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 1f)));
                m.SetFloat("_RenderType", 1);
                m.SetFloat("_Cutoff", 0.5f);
            }),
            background: background);

        Assert.That(color.b, Is.GreaterThan(color.r + 0.25f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.g + 0.25f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTransitionFeaturesPreserveColorMask()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var color = RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)));
                m.SetFloat("_ColorMask", 0);
            }),
            background: background);

        Assert.That(color.r, Is.LessThan(0.06f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.r + 0.02f), color.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTransitionFeaturesRespectRectClip()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    ConfigureUiAlphaTransition(material, 2, 0.5f, 0, m =>
                    {
                        m.SetTexture("_AlphaTransitionMap", CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)));
                    });
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-0.05f, -1f, 1f, 1f));
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[1].r, Is.GreaterThan(samples[0].r + 0.5f), $"Clipped {samples[0]}, visible {samples[1]}");
        Assert.That(samples[0].r, Is.LessThan(0.1f), $"Clipped {samples[0]}, visible {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTransitionFeaturesPreserveStencilOrMaskBehavior()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateStencilMask(new Rect(-0.5f, -0.5f, 0.5f, 1f)),
                CreateUiDraw(material =>
                {
                    ConfigureUiAlphaTransition(material, 2, 0.5f, 0, m =>
                    {
                        m.SetTexture("_AlphaTransitionMap", CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f)));
                    });
                    material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                    material.SetFloat("_Stencil", 1);
                    material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                    material.SetFloat("_StencilReadMask", 255);
                    material.SetFloat("_StencilWriteMask", 255);
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[0].r, Is.GreaterThan(samples[1].r + 0.5f), $"Inside {samples[0]}, outside {samples[1]}");
        Assert.That(samples[1].r, Is.LessThan(0.1f), $"Inside {samples[0]}, outside {samples[1]}");
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

    private static Color RenderUiAlphaTransition(Texture transitionTexture, float progress, int mode)
    {
        return RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, mode, progress, 0, m => m.SetTexture("_AlphaTransitionMap", transitionTexture)),
            background: Color.black);
    }

    private static Color RenderUiSecondAlphaTransition(Texture mainTexture, Texture secondTexture, int secondBlendMode)
    {
        return RenderUiUberUnlit(
            material => ConfigureUiAlphaTransition(material, 2, 0.5f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", mainTexture);
                m.SetFloat("_AlphaTransitionSecondTextureBlendMode", secondBlendMode);
                m.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 0);
                m.SetTexture("_AlphaTransitionMapSecondTexture", secondTexture);
                m.SetFloat("_AlphaTransitionProgressSecondTexture", 0.5f);
            }),
            background: Color.black);
    }

    private static void ConfigureUiAlphaTransition(Material material, int mode, float progress, int mapMode, Action<Material> configureMap)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
        material.SetFloat("_BaseMapMode", 0);
        material.SetFloat("_RenderType", 2);
        material.SetFloat("_TransparentBlendMode", 0);
        material.SetFloat("_AlphaTransitionMode", mode);
        material.SetFloat("_AlphaTransitionMapMode", mapMode);
        material.SetFloat("_AlphaTransitionProgress", progress);
        material.SetFloat("_DissolveSharpness", 1f);
        material.SetFloat("_DissolveSharpnessSecondTexture", 1f);
        material.SetColor("_TintColor", Color.white);
        configureMap(material);
    }

    private static Color RenderUiEmissionAreaAll(float intensity)
    {
        return RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 1, 0, 0, intensity, m =>
            {
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.08f, 0.08f, 1f)));
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }),
            background: Color.black);
    }

    private static Color RenderUiEmissionAreaAlpha(float baseAlpha, bool keepEdgeTransparency, bool renderTransparent = false)
    {
        return RenderUiUberUnlit(
            material => ConfigureUiEmission(material, 3, 0, 0, 1f, m =>
            {
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.08f, 0.08f, baseAlpha)));
                m.SetFloat("_RenderType", renderTransparent ? 2 : 0);
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                m.SetFloat("_KeepEdgeTransparency", keepEdgeTransparency ? 1f : 0f);
            }),
            background: Color.black);
    }

    private static Color RenderUiRimTransparency(bool enabled, bool inverse, float progress, float sharpness, Vector3 normal)
    {
        return RenderUiUberUnlit(
            material =>
            {
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_RimTransparencyEnabled", enabled ? 1f : 0f);
                material.SetFloat("_InverseRimTransparency", inverse ? 1f : 0f);
                material.SetFloat("_RimTransparencyProgress", progress);
                material.SetFloat("_RimTransparencySharpness", sharpness);
            },
            mesh: CreateQuad(Color.white, normal: normal),
            background: Color.black);
    }

    private static Color[] RenderUiLuminanceTransparencySamples(bool enabled, bool inverse, float progress, float sharpness)
    {
        var texture = CreateHorizontalSplitTexture(
            new Color(0.35f, 0.35f, 0.35f, 1f),
            Color.white,
            rightColorStartX: TextureSize / 2);

        return RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    material.SetFloat("_RenderType", 2);
                    material.SetFloat("_TransparentBlendMode", 0);
                    material.SetTexture("_BaseMap", texture);
                    material.SetFloat("_BaseMapMode", 0);
                    material.SetFloat("_LuminanceTransparencyEnabled", enabled ? 1f : 0f);
                    material.SetFloat("_InverseLuminanceTransparency", inverse ? 1f : 0f);
                    material.SetFloat("_LuminanceTransparencyProgress", progress);
                    material.SetFloat("_LuminanceTransparencySharpness", sharpness);
                }),
            },
            new[] { new Vector2(0.25f, 0.5f), new Vector2(0.75f, 0.5f) },
            Color.black);
    }

    private static Color RenderUiLuminanceTransparency(Color baseColor, bool inverse, float progress, float sharpness)
    {
        return RenderUiUberUnlit(
            material =>
            {
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetTexture("_BaseMap", CreateSolidTexture(baseColor));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_LuminanceTransparencyEnabled", 1f);
                material.SetFloat("_InverseLuminanceTransparency", inverse ? 1f : 0f);
                material.SetFloat("_LuminanceTransparencyProgress", progress);
                material.SetFloat("_LuminanceTransparencySharpness", sharpness);
            },
            background: Color.black);
    }

    private static Color RenderUiCombinedTransparency(bool rim, bool luminance, Vector3 normal)
    {
        return RenderUiUberUnlit(
            material =>
            {
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f)));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_RimTransparencyEnabled", rim ? 1f : 0f);
                material.SetFloat("_RimTransparencyProgress", 0.5f);
                material.SetFloat("_RimTransparencySharpness", 0f);
                material.SetFloat("_LuminanceTransparencyEnabled", luminance ? 1f : 0f);
                material.SetFloat("_LuminanceTransparencyProgress", 0.5f);
                material.SetFloat("_LuminanceTransparencySharpness", 0f);
            },
            mesh: CreateQuad(Color.white, normal: normal),
            background: Color.black);
    }

    private static void ConfigureUiTransparentDepthMaterial(Material material)
    {
        material.SetFloat("_RenderType", 2);
        material.SetFloat("_TransparentBlendMode", 0);
        material.SetFloat("_Cull", (float)CullMode.Off);
        material.SetFloat("_ZTest", (float)CompareFunction.Always);
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0f, 0f, 1f)));
        material.SetFloat("_BaseMapMode", 0);
    }

    private static void ConfigureUiDepthFade(Material material, float near, float far, float width)
    {
        material.SetFloat("_DepthFadeEnabled", 1f);
        material.SetFloat("_DepthFadeNear", near);
        material.SetFloat("_DepthFadeFar", far);
        material.SetFloat("_DepthFadeWidth", width);
    }

    private static void ConfigureUiEmissionAndTransparency(Material material)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.25f, 0.25f, 0.25f, 1f)));
        material.SetFloat("_EmissionAreaType", 1);
        material.SetFloat("_EmissionColorType", 0);
        material.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
        material.SetFloat("_EmissionIntensity", 1f);
        material.SetFloat("_LuminanceTransparencyEnabled", 1f);
        material.SetFloat("_LuminanceTransparencyProgress", 0.1f);
        material.SetFloat("_LuminanceTransparencySharpness", 0f);
    }

    private static void ConfigureUiEmission(Material material, int areaType, int colorType, int mapMode, float intensity, Action<Material> configure)
    {
        material.SetFloat("_RenderType", 2);
        material.SetFloat("_TransparentBlendMode", 0);
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.08f, 0.08f, 1f)));
        material.SetFloat("_BaseMapMode", 0);
        material.SetFloat("_EmissionAreaType", areaType);
        material.SetFloat("_EmissionColorType", colorType);
        material.SetFloat("_EmissionMapMode", mapMode);
        material.SetFloat("_EmissionIntensity", intensity);
        material.SetFloat("_KeepEdgeTransparency", 1f);
        configure(material);
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

    private static DepthRenderResult RenderUiDepthScene(Action<Material> configureMaterial, float opaqueZ = 0.08f)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var particleMesh = CreateQuad(Color.white);
        var opaqueMesh = CreateQuad(Color.white);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUIParticlesUberUnlitDepthCamera");
        var particleObject = new GameObject("NovaBuiltInUIParticlesUberUnlitDepthParticle");
        var opaqueObject = new GameObject("NovaBuiltInUIParticlesUberUnlitDepthOpaque");
        var ownedObjects = new List<UnityEngine.Object>();
        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        var previous = RenderTexture.active;

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;

            configureMaterial(material);
            SetupUberUnlit(material);

            var particleFilter = particleObject.AddComponent<MeshFilter>();
            particleFilter.sharedMesh = particleMesh;
            var particleRenderer = particleObject.AddComponent<MeshRenderer>();
            particleRenderer.sharedMaterial = material;

            opaqueObject.transform.position = new Vector3(0f, 0f, opaqueZ);
            var opaqueFilter = opaqueObject.AddComponent<MeshFilter>();
            opaqueFilter.sharedMesh = opaqueMesh;
            var opaqueRenderer = opaqueObject.AddComponent<MeshRenderer>();
            opaqueRenderer.sharedMaterial = CreateOpaqueMaterial(new Color(0f, 0f, 1f, 1f));
            ownedObjects.Add(opaqueRenderer.sharedMaterial);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 0.75f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;
            camera.depthTextureMode |= DepthTextureMode.Depth;
            camera.transform.position = new Vector3(0f, 0f, -2f);
            camera.targetTexture = target;
            camera.Render();

            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, RenderSize, RenderSize), 0, 0);
            readback.Apply();

            var color = readback.GetPixel(RenderSize / 2, RenderSize / 2);
            return new DepthRenderResult(color, camera.depthTextureMode);
        }
        finally
        {
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(particleObject);
            UnityEngine.Object.DestroyImmediate(opaqueObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(particleMesh);
            UnityEngine.Object.DestroyImmediate(opaqueMesh);
            UnityEngine.Object.DestroyImmediate(material);
            foreach (var ownedObject in ownedObjects.Where(ownedObject => ownedObject != null).Distinct())
            {
                UnityEngine.Object.DestroyImmediate(ownedObject);
            }
        }
    }

    private static Material CreateOpaqueMaterial(Color color)
    {
        var shader = Shader.Find("Standard");
        Assert.That(shader, Is.Not.Null, "Standard");
        var material = new Material(shader);
        material.SetColor("_Color", color);
        material.renderQueue = (int)RenderQueue.Geometry;
        return material;
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

    private readonly struct DepthRenderResult
    {
        public DepthRenderResult(Color color, DepthTextureMode cameraDepthTextureMode)
        {
            Color = color;
            CameraDepthTextureMode = cameraDepthTextureMode;
        }

        public Color Color { get; }

        public DepthTextureMode CameraDepthTextureMode { get; }
    }
}

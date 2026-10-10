using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Nova.Tests.PlayMode
{
[TestFixture("Nova/Particles/UberUnlit")]
[TestFixture("Nova/UIParticles/UberUnlit")]
[TestFixture("Nova/Particles/UberLit")]
[TestFixture("Nova/UIParticles/UberLit")]
public sealed class NovaBuiltInUberUnlitRenderingSmokeTests
{
    private readonly string ShaderName;

    public NovaBuiltInUberUnlitRenderingSmokeTests(string shaderName)
    {
        ShaderName = shaderName;
    }
    private const int TextureSize = 4;
    private const int RenderSize = 64;

    [UnityTest]
    public IEnumerator UberShadersApplySceneFogLinear() => AssertSceneFog(FogMode.Linear);

    [UnityTest]
    public IEnumerator UberShadersApplySceneFogExponential() => AssertSceneFog(FogMode.Exponential);

    [UnityTest]
    public IEnumerator UberShadersApplySceneFogExponentialSquared() => AssertSceneFog(FogMode.ExponentialSquared);

    [UnityTest]
    public IEnumerator UberShadersApplySceneFogPerspectiveLinear() => AssertSceneFog(FogMode.Linear, false);

    [UnityTest]
    public IEnumerator UberShadersApplySceneFogPerspectiveExponential() => AssertSceneFog(FogMode.Exponential, false);

    [UnityTest]
    public IEnumerator UberShadersApplySceneFogPerspectiveExponentialSquared() => AssertSceneFog(FogMode.ExponentialSquared, false);

    private IEnumerator AssertSceneFog(FogMode mode, bool orthographic = true)
    {
        Color Render()
        {
            var texture = CreateSolidTexture(Color.white);
            return RenderUberUnlit(material => material.SetTexture("_BaseMap", texture),
                ownedObjects: new[] { texture }, orthographic: orthographic);
        }
        var oldEnabled = RenderSettings.fog;
        var oldMode = RenderSettings.fogMode;
        var oldColor = RenderSettings.fogColor;
        var oldDensity = RenderSettings.fogDensity;
        var oldStart = RenderSettings.fogStartDistance;
        var oldEnd = RenderSettings.fogEndDistance;
        try
        {
            RenderSettings.fog = false;
            yield return null; // Unity publishes fog globals at the frame boundary.
            var clear = Render();
            RenderSettings.fog = true;
            RenderSettings.fogMode = mode;
            RenderSettings.fogColor = Color.blue;
            RenderSettings.fogDensity = 3f;
            RenderSettings.fogStartDistance = 0f;
            RenderSettings.fogEndDistance = 0.5f;
            yield return null;
            var fogged = Render();
            Assert.That(clear.r, Is.GreaterThan(0.5f), clear.ToString());
            Assert.That(fogged.r, Is.LessThan(0.08f), fogged.ToString());
            Assert.That(fogged.b, Is.GreaterThan(0.7f), fogged.ToString());
        }
        finally
        {
            RenderSettings.fog = oldEnabled;
            RenderSettings.fogMode = oldMode;
            RenderSettings.fogColor = oldColor;
            RenderSettings.fogDensity = oldDensity;
            RenderSettings.fogStartDistance = oldStart;
            RenderSettings.fogEndDistance = oldEnd;
        }
    }

    [Test]
    public void UberUnlitRendersBaseMap2D()
    {
        var color = RenderUberUnlit(new Color(0.95f, 0.08f, 0.05f, 1f), Color.white, Color.white, tintEnabled: false);

        Assert.That(color.r, Is.GreaterThan(0.45f), color.ToString());
        Assert.That(color.r, Is.GreaterThan(color.g + 0.15f), color.ToString());
        Assert.That(color.r, Is.GreaterThan(color.b + 0.15f), color.ToString());
    }

    [Test]
    public void UberUnlitTintAffectsOutput()
    {
        var untinted = RenderUberUnlit(Color.white, Color.white, Color.white, tintEnabled: false);
        var tinted = RenderUberUnlit(Color.white, new Color(0.2f, 1f, 0.2f, 1f), Color.white, tintEnabled: true);

        Assert.That(tinted.g, Is.GreaterThan(tinted.r + 0.2f), tinted.ToString());
        Assert.That(tinted.g, Is.GreaterThan(tinted.b + 0.2f), tinted.ToString());
        Assert.That(tinted.r, Is.LessThan(untinted.r - 0.15f), $"Untinted {untinted}, tinted {tinted}");
    }

    [Test]
    public void UberUnlitAlphaTestClipsBelowCutoff()
    {
        var background = new Color(0.05f, 0.1f, 0.85f, 1f);
        var color = RenderUberUnlit(
            new Color(1f, 0.05f, 0.05f, 0.25f),
            Color.white,
            Color.white,
            tintEnabled: false,
            alphaTestEnabled: true,
            background: background);

        Assert.That(color.b, Is.GreaterThan(color.r + 0.25f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.g + 0.25f), color.ToString());
    }

    [Test]
    public void UberUnlitVertexColorAffectsOutput()
    {
        var color = RenderUberUnlit(Color.white, Color.white, new Color(0.2f, 1f, 0.2f, 1f), tintEnabled: false);

        Assert.That(color.g, Is.GreaterThan(color.r + 0.2f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.2f), color.ToString());
    }

    [Test]
    public void UberUnlitRendersBaseMap2DArray()
    {
        var texture = CreateTexture2DArray(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.9f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.9f, 1f));

        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap2DArray", texture);
                material.SetFloat("_BaseMapMode", 1);
                material.SetFloat("_BaseMapProgress", 0.45f);
                material.SetFloat("_BaseMapSliceCount", texture.depth);
            },
            ownedObjects: new UnityEngine.Object[] { texture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.2f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.2f), color.ToString());
    }

    [Test]
    public void UberUnlitRendersBaseMap3D()
    {
        var texture = CreateTexture3D(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.9f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.9f, 1f));

        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap3D", texture);
                material.SetFloat("_BaseMapMode", 2);
                material.SetFloat("_BaseMapProgress", 0.45f);
                material.SetFloat("_BaseMapSliceCount", texture.depth);
            },
            ownedObjects: new UnityEngine.Object[] { texture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.12f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.12f), color.ToString());
    }

    [Test]
    public void UberUnlitBaseMapChannelExtractionAffectsOutput()
    {
        var texture = CreateSolidTexture(new Color(0.08f, 0.82f, 0.18f, 1f));
        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", texture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_BaseMapChannel", 2);
            },
            ownedObjects: new UnityEngine.Object[] { texture });

        Assert.That(Mathf.Abs(color.r - color.g), Is.LessThan(0.08f), color.ToString());
        Assert.That(Mathf.Abs(color.g - color.b), Is.LessThan(0.08f), color.ToString());
        Assert.That(color.r, Is.GreaterThan(0.45f), color.ToString());
    }

    [Test]
    public void UberUnlitBaseMapRandomRowSelectionAffectsOutput()
    {
        var texture = CreateTexture2DArray(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.9f, 0.08f, 0.05f, 1f),
            new Color(0.05f, 0.82f, 0.05f, 1f),
            new Color(0.05f, 0.9f, 0.05f, 1f));

        var color = RenderUberUnlit(
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
            mesh: CreateQuad(Color.white, customCoord1: new Vector4(1.2f, 0f, 0f, 0f)),
            ownedObjects: new UnityEngine.Object[] { texture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.2f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.2f), color.ToString());
    }

    [Test]
    public void UberUnlitTritoneAffectsOutput()
    {
        var texture = CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f));
        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", texture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_BaseMapToneMode", 1);
                material.SetFloat("_BaseMapToneChannel", 0);
                material.SetColor("_BaseMapToneShadowsColor", new Color(0.9f, 0.05f, 0.05f, 1f));
                material.SetColor("_BaseMapToneMidtonesColor", new Color(0.05f, 0.9f, 0.05f, 1f));
                material.SetColor("_BaseMapToneHighlightsColor", new Color(0.05f, 0.05f, 0.9f, 1f));
                material.SetFloat("_BaseMapToneShadows", 0.2f);
                material.SetFloat("_BaseMapToneMidtones", 0.5f);
                material.SetFloat("_BaseMapToneHighlights", 0.8f);
            },
            ownedObjects: new UnityEngine.Object[] { texture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.15f), color.ToString());
    }

    [Test]
    public void UberUnlitPentoneAffectsOutput()
    {
        var texture = CreateSolidTexture(new Color(0.65f, 0.65f, 0.65f, 1f));
        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", texture);
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
            },
            ownedObjects: new UnityEngine.Object[] { texture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.1f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.r + 0.1f), color.ToString());
    }

    [Test]
    public void UberUnlitTintMapAffectsOutput()
    {
        var baseTexture = CreateSolidTexture(Color.white);
        var tintTexture = CreateSolidTexture(new Color(0.08f, 0.9f, 0.08f, 1f));
        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", baseTexture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_TintAreaMode", 1);
                material.SetFloat("_TintColorMode", 1);
                material.SetTexture("_TintMap", tintTexture);
                material.SetFloat("_TintBlendRate", 1);
            },
            ownedObjects: new UnityEngine.Object[] { baseTexture, tintTexture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.2f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.2f), color.ToString());
    }

    [Test]
    public void UberUnlitTintMap2DArrayAffectsOutput()
    {
        var baseTexture = CreateSolidTexture(Color.white);
        var tintTexture = CreateTexture2DArray(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.85f, 1f));

        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", baseTexture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_TintAreaMode", 1);
                material.SetFloat("_TintColorMode", 3);
                material.SetTexture("_TintMap2DArray", tintTexture);
                material.SetFloat("_TintMapProgress", 0.45f);
                material.SetFloat("_TintMapSliceCount", tintTexture.depth);
                material.SetFloat("_TintBlendRate", 1);
            },
            ownedObjects: new UnityEngine.Object[] { baseTexture, tintTexture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.15f), color.ToString());
    }

    [Test]
    public void UberUnlitTintMap3DAffectsOutput()
    {
        var baseTexture = CreateSolidTexture(Color.white);
        var tintTexture = CreateTexture3D(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.85f, 1f));

        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", baseTexture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_TintAreaMode", 1);
                material.SetFloat("_TintColorMode", 2);
                material.SetTexture("_TintMap3D", tintTexture);
                material.SetFloat("_TintMap3DProgress", 0.45f);
                material.SetFloat("_TintMapSliceCount", tintTexture.depth);
                material.SetFloat("_TintBlendRate", 1);
            },
            ownedObjects: new UnityEngine.Object[] { baseTexture, tintTexture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.12f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.12f), color.ToString());
    }

    [Test]
    public void UberUnlitTintMapRandomRowSelectionAffectsOutput()
    {
        var baseTexture = CreateSolidTexture(Color.white);
        var tintTexture = CreateTexture2DArray(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.9f, 0.08f, 0.05f, 1f),
            new Color(0.05f, 0.82f, 0.05f, 1f),
            new Color(0.05f, 0.9f, 0.05f, 1f));

        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", baseTexture);
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
            mesh: CreateQuad(Color.white, customCoord1: new Vector4(1.2f, 0f, 0f, 0f)),
            ownedObjects: new UnityEngine.Object[] { baseTexture, tintTexture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.15f), color.ToString());
    }

    [Test]
    public void UberUnlitTintRimAffectsOutput()
    {
        var baseTexture = CreateSolidTexture(Color.white);
        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", baseTexture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_TintAreaMode", 2);
                material.SetFloat("_TintColorMode", 0);
                material.SetColor("_TintColor", new Color(0.08f, 0.9f, 0.08f, 1f));
                material.SetFloat("_TintBlendRate", 1);
                material.SetFloat("_TintRimProgress", 0.5f);
                material.SetFloat("_TintRimSharpness", 1f);
                material.SetFloat("_InverseTintRim", 1f);
            },
            ownedObjects: new UnityEngine.Object[] { baseTexture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.2f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.2f), color.ToString());
    }

    [Test]
    public void UberUnlitFlowMapOffsetsBaseSampling()
    {
        var baseTexture = CreateHorizontalSplitTexture(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            rightColorStartX: TextureSize - 1);
        var flowTexture = CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f));

        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", baseTexture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetTexture("_FlowMap", flowTexture);
                material.SetFloat("_FlowMapTarget", 1);
                material.SetFloat("_FlowMapChannelsX", 0);
                material.SetFloat("_FlowMapChannelsY", 1);
                material.SetFloat("_FlowIntensity", 0.4f);
            },
            ownedObjects: new UnityEngine.Object[] { baseTexture, flowTexture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
    }

    [Test]
    public void UberUnlitFlowMapOffsetsTintSampling()
    {
        var baseTexture = CreateSolidTexture(Color.white);
        var tintTexture = CreateHorizontalSplitTexture(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            rightColorStartX: TextureSize - 1);
        var flowTexture = CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f));

        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", baseTexture);
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
            },
            ownedObjects: new UnityEngine.Object[] { baseTexture, tintTexture, flowTexture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
    }

    [Test]
    public void UberUnlitParallax2DOffsetsBaseSampling()
    {
        var baseTexture = CreateHorizontalSplitTexture(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.85f, 0.05f, 1f),
            rightColorStartX: TextureSize / 2);
        var parallaxTexture = CreateSolidTexture(new Color(1f, 1f, 1f, 1f));

        var color = RenderUberUnlit(
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
            mesh: CreateQuad(Color.white, skewForParallax: true),
            ownedObjects: new UnityEngine.Object[] { baseTexture, parallaxTexture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.1f), color.ToString());
    }

    [TestCase(0, 1)]
    [TestCase(1, 1)]
    [TestCase(2, 1)]
    [TestCase(0, 2)]
    [TestCase(1, 2)]
    [TestCase(2, 2)]
    [TestCase(0, 4)]
    [TestCase(1, 4)]
    [TestCase(2, 4)]
    public void ParallaxMovesEveryTextureModeAndTarget(int mode, int target)
    {
        Color Render(float strength)
        {
            var pattern = CreateHorizontalSplitTexture(Color.black, Color.white, TextureSize / 2);
            var solid = CreateSolidTexture(target == 4 ? Color.black : Color.white);
            Texture height = mode == 0 ? (Texture)CreateSolidTexture(Color.white)
                : mode == 1 ? CreateTexture2DArray(Color.white, Color.white)
                : (Texture)CreateTexture3D(Color.white, Color.white);
            return RenderUberUnlitSamples(material =>
            {
                material.SetTexture("_BaseMap", target == 1 ? pattern : solid);
                material.SetFloat("_BaseMapMode", 0f);
                if (target == 2)
                {
                    material.SetFloat("_TintAreaMode", 1f);
                    material.SetFloat("_TintColorMode", 1f);
                    material.SetTexture("_TintMap", pattern);
                    material.SetFloat("_TintBlendRate", 1f);
                }
                if (target == 4)
                {
                    material.SetFloat("_EmissionAreaType", 2f);
                    material.SetFloat("_EmissionColorType", 0f);
                    material.SetColor("_EmissionColor", Color.white);
                    material.SetTexture("_EmissionMap", pattern);
                    material.SetFloat("_EmissionMapChannelsX", 0f);
                    material.SetFloat("_EmissionIntensity", 1f);
                }
                material.SetTexture(mode == 0 ? "_ParallaxMap" : mode == 1 ? "_ParallaxMap2DArray" : "_ParallaxMap3D", height);
                material.SetFloat("_ParallaxMapMode", mode);
                material.SetFloat("_ParallaxMapSliceCount", 2f);
                material.SetFloat("_ParallaxMapTarget", target);
                material.SetFloat("_ParallaxMapChannel", 0f);
                material.SetFloat("_ParallaxStrength", strength);
            }, new[] { new Vector2(0.48f, 0.5f) },
                mesh: CreateQuad(Color.white, skewForParallax: true),
                ownedObjects: new UnityEngine.Object[] { pattern, solid, height })[0];
        }
        var disabled = Render(0f);
        var enabled = Render(1f);
        Assert.That(enabled.r, Is.GreaterThan(disabled.r + 0.25f),
            $"mode {mode}, target {target}: off {disabled}, on {enabled}");
    }

    [Test]
    public void UberUnlitGreyscaleAffectsOutput()
    {
        var texture = CreateSolidTexture(new Color(0.9f, 0.05f, 0.05f, 1f));
        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", texture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_ColorCorrectionMode", 1);
            },
            ownedObjects: new UnityEngine.Object[] { texture });

        Assert.That(Mathf.Abs(color.r - color.g), Is.LessThan(0.08f), color.ToString());
        Assert.That(Mathf.Abs(color.g - color.b), Is.LessThan(0.08f), color.ToString());
    }

    [Test]
    public void UberUnlitGradientMapAffectsOutput()
    {
        var baseTexture = CreateSolidTexture(new Color(0.05f, 0.85f, 0.05f, 1f));
        var gradientTexture = CreateHorizontalSplitTexture(
            new Color(0.85f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.85f, 1f),
            rightColorStartX: TextureSize / 2);

        var color = RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", baseTexture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_ColorCorrectionMode", 2);
                material.SetTexture("_GradientMap", gradientTexture);
            },
            ownedObjects: new UnityEngine.Object[] { baseTexture, gradientTexture });

        Assert.That(color.b, Is.GreaterThan(color.r + 0.15f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.g + 0.05f), color.ToString());
    }

    [Test]
    public void UberUnlitFadeAlphaTransitionAffectsOutput()
    {
        var visibleTexture = CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f));
        var fadedTexture = CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f));
        var visible = RenderAlphaTransition(visibleTexture, progress: 0f, mode: 1);
        var faded = RenderAlphaTransition(fadedTexture, progress: 1f, mode: 1);

        Assert.That(visible.r, Is.GreaterThan(faded.r + 0.25f), $"Visible {visible}, faded {faded}");
    }

    [Test]
    public void UberUnlitDissolveAlphaTransitionClipsOrAttenuatesOutput()
    {
        var lowTransition = CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 1f));
        var highTransition = CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f));
        var attenuated = RenderAlphaTransition(lowTransition, progress: 0.5f, mode: 2);
        var retained = RenderAlphaTransition(highTransition, progress: 0.5f, mode: 2);

        Assert.That(retained.r, Is.GreaterThan(attenuated.r + 0.25f), $"Retained {retained}, attenuated {attenuated}");
    }

    [Test]
    public void UberUnlitAlphaTransitionMap2DAffectsOutput()
    {
        var lowTransition = CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 1f));
        var highTransition = CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f));
        var low = RenderAlphaTransition(lowTransition, progress: 0.5f, mode: 2);
        var high = RenderAlphaTransition(highTransition, progress: 0.5f, mode: 2);

        Assert.That(high.r, Is.GreaterThan(low.r + 0.25f), $"High {high}, low {low}");
    }

    [Test]
    public void UberUnlitAlphaTransitionMap2DArrayAffectsOutput()
    {
        var texture = CreateTexture2DArray(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.9f, 0.9f, 0.9f, 1f),
            new Color(0.1f, 0.1f, 0.1f, 1f));

        var color = RenderUberUnlit(
            material => ConfigureAlphaTransition(material, 2, 0.5f, 1, m =>
            {
                m.SetTexture("_AlphaTransitionMap2DArray", texture);
                m.SetFloat("_AlphaTransitionMapProgress", 0.45f);
                m.SetFloat("_AlphaTransitionMapSliceCount", texture.depth);
            }),
            background: Color.black,
            ownedObjects: new UnityEngine.Object[] { texture });

        Assert.That(color.r, Is.GreaterThan(0.35f), color.ToString());
    }

    [Test]
    public void UberUnlitAlphaTransitionMap3DAffectsOutput()
    {
        var texture = CreateTexture3D(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.9f, 0.9f, 0.9f, 1f),
            new Color(0.1f, 0.1f, 0.1f, 1f));

        var color = RenderUberUnlit(
            material => ConfigureAlphaTransition(material, 2, 0.5f, 2, m =>
            {
                m.SetTexture("_AlphaTransitionMap3D", texture);
                m.SetFloat("_AlphaTransitionMapProgress", 0.45f);
                m.SetFloat("_AlphaTransitionMapSliceCount", texture.depth);
            }),
            background: Color.black,
            ownedObjects: new UnityEngine.Object[] { texture });

        Assert.That(color.r, Is.GreaterThan(0.30f), color.ToString());
    }

    [Test]
    public void UberUnlitAlphaTransitionRandomRowSelectionAffectsOutput()
    {
        var texture = CreateTexture2DArray(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.15f, 0.15f, 0.15f, 1f),
            new Color(0.85f, 0.85f, 0.85f, 1f),
            new Color(0.9f, 0.9f, 0.9f, 1f));

        var color = RenderUberUnlit(
            material => ConfigureAlphaTransition(material, 2, 0.5f, 1, m =>
            {
                m.SetTexture("_AlphaTransitionMap2DArray", texture);
                m.SetFloat("_AlphaTransitionMapProgress", 0.1f);
                m.SetFloat("_AlphaTransitionMapSliceCount", texture.depth);
                m.SetFloat("_AlphaTransitionMapRandomRowSelectionEnabled", 1);
                m.SetFloat("_AlphaTransitionMapRandomRowCoord", 1);
                m.SetFloat("_AlphaTransitionMapRowCount", 2);
            }),
            mesh: CreateQuad(Color.white, customCoord1: new Vector4(1.2f, 0f, 0f, 0f)),
            background: Color.black,
            ownedObjects: new UnityEngine.Object[] { texture });

        Assert.That(color.r, Is.GreaterThan(0.30f), color.ToString());
    }

    [Test]
    public void UberUnlitSecondAlphaTransitionTextureAverageAffectsOutput()
    {
        var mainTextureWithoutSecond = CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f));
        var mainTextureWithSecond = CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f));
        var secondTexture = CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 1f));
        var noSecond = RenderAlphaTransition(mainTextureWithoutSecond, progress: 0.5f, mode: 2);
        var average = RenderSecondAlphaTransition(mainTextureWithSecond, secondTexture, secondBlendMode: 1);

        Assert.That(noSecond.r, Is.GreaterThan(average.r + 0.18f), $"No second {noSecond}, average {average}");
    }

    [TestCase(1, false, 0)]
    [TestCase(2, false, 0)]
    [TestCase(1, true, 0)]
    [TestCase(2, true, 0)]
    [TestCase(1, false, 1)]
    [TestCase(2, false, 1)]
    [TestCase(1, true, 1)]
    [TestCase(2, true, 1)]
    [TestCase(1, false, 2)]
    [TestCase(2, false, 2)]
    [TestCase(1, true, 2)]
    [TestCase(2, true, 2)]
    public void TransitionArrayAndVolumeSelectFramesAndRows(int mapMode, bool randomRow, int secondBlend)
    {
        Color Render(bool high)
        {
            var lowColor = new Color(0.1f, 0.1f, 0.1f, 1);
            var highColor = new Color(0.9f, 0.9f, 0.9f, 1);
            Texture texture = mapMode == 1
                ? (Texture)CreateTexture2DArray(lowColor, lowColor, highColor, highColor)
                : CreateTexture3D(lowColor, lowColor, highColor, highColor);
            var second = secondBlend > 0;
            var prefix = second ? "_AlphaTransitionMapSecondTexture" : "_AlphaTransitionMap";
            return RenderUberUnlit(material => ConfigureAlphaTransition(material, 2, 0.5f, second ? 0 : mapMode, m =>
            {
                m.SetTexture("_BaseMap", Texture2D.whiteTexture);
                if (second) m.SetTexture("_AlphaTransitionMap", Texture2D.whiteTexture);
                m.SetFloat("_AlphaTransitionSecondTextureBlendMode", secondBlend);
                m.SetFloat("_AlphaTransitionMapSecondTextureMapMode", mapMode);
                m.SetFloat("_AlphaTransitionProgressSecondTexture", 0.5f);
                m.SetTexture(prefix + (mapMode == 1 ? "2DArray" : "3D"), texture);
                m.SetFloat(prefix + "Progress", randomRow ? 0.1f : high ? 0.7f : 0.1f);
                m.SetFloat(prefix + "SliceCount", 4);
                m.SetFloat(prefix + "RandomRowSelectionEnabled", randomRow ? 1 : 0);
                m.SetFloat(prefix + "RandomRowCoord", 1);
                m.SetFloat(prefix + "RowCount", 2);
            }), mesh: CreateQuad(Color.white, customCoord1: new Vector4(high ? 1.2f : 0, 0, 0, 0)),
                background: Color.black, ownedObjects: new[] { texture });
        }
        var low = Render(false);
        var high = Render(true);
        Assert.That(high.r, Is.GreaterThan(low.r + 0.2f), $"low {low}, high {high}");
        if (secondBlend != 1) Assert.That(low.r, Is.LessThan(0.04f), low.ToString());
    }

    [Test]
    public void UberUnlitSecondAlphaTransitionTextureMultiplyAffectsOutput()
    {
        var mainTexture = CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f));
        var secondTexture = CreateSolidTexture(new Color(0.1f, 0.1f, 0.1f, 1f));
        var multiply = RenderSecondAlphaTransition(mainTexture, secondTexture, secondBlendMode: 2);

        Assert.That(multiply.r, Is.LessThan(0.2f), multiply.ToString());
    }

    [Test]
    public void UberUnlitVertexAlphaCanDriveTransitionProgress()
    {
        var transitionTexture = CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f));
        var opaqueVertex = RenderUberUnlit(
            material => ConfigureAlphaTransition(material, 1, 0f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", transitionTexture);
                m.SetFloat("_VertexAlphaMode", 1);
            }),
            mesh: CreateQuad(Color.white),
            background: Color.black,
            ownedObjects: new UnityEngine.Object[] { transitionTexture });

        var transparentVertexTexture = CreateSolidTexture(new Color(0.5f, 0.5f, 0.5f, 1f));
        var transparentVertex = RenderUberUnlit(
            material => ConfigureAlphaTransition(material, 1, 0f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", transparentVertexTexture);
                m.SetFloat("_VertexAlphaMode", 1);
            }),
            mesh: CreateQuad(new Color(1f, 1f, 1f, 0f)),
            background: Color.black,
            ownedObjects: new UnityEngine.Object[] { transparentVertexTexture });

        Assert.That(opaqueVertex.r, Is.GreaterThan(transparentVertex.r + 0.25f), $"Opaque {opaqueVertex}, transparent {transparentVertex}");
    }

    [Test]
    public void UberUnlitFlowMapOffsetsAlphaTransitionSampling()
    {
        var transitionTexture = CreateHorizontalSplitTexture(
            new Color(0.1f, 0.1f, 0.1f, 1f),
            new Color(0.9f, 0.9f, 0.9f, 1f),
            rightColorStartX: TextureSize - 1);
        var flowTexture = CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f));

        var color = RenderUberUnlit(
            material => ConfigureAlphaTransition(material, 2, 0.5f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", transitionTexture);
                m.SetTexture("_FlowMap", flowTexture);
                m.SetFloat("_FlowMapTarget", 4);
                m.SetFloat("_FlowMapChannelsX", 0);
                m.SetFloat("_FlowMapChannelsY", 1);
                m.SetFloat("_FlowIntensity", 0.4f);
            }),
            background: Color.black,
            ownedObjects: new UnityEngine.Object[] { transitionTexture, flowTexture });

        Assert.That(color.r, Is.GreaterThan(0.30f), color.ToString());
    }

    [Test]
    public void UberUnlitEmissionAreaAllAffectsOutput()
    {
        var disabled = RenderEmissionAreaAll(intensity: 0f);
        var enabled = RenderEmissionAreaAll(intensity: 1.2f);

        Assert.That(enabled.g, Is.GreaterThan(disabled.g + 0.20f), $"Disabled {disabled}, enabled {enabled}");
        Assert.That(enabled.g, Is.GreaterThan(enabled.r + 0.12f), enabled.ToString());
    }

    [Test]
    public void UberUnlitEmissionAreaAlphaUsesAlpha()
    {
        var lowAlpha = RenderEmissionAreaAlpha(baseAlpha: 0.15f, keepEdgeTransparency: false);
        var highAlpha = RenderEmissionAreaAlpha(baseAlpha: 1f, keepEdgeTransparency: false);

        Assert.That(lowAlpha.g, Is.GreaterThan(highAlpha.g + 0.20f), $"Low {lowAlpha}, high {highAlpha}");
    }

    [Test]
    public void UberUnlitEmissionAreaMapMasksEmission()
    {
        var emissionMap = CreateHorizontalSplitTexture(
            new Color(0f, 0f, 0f, 1f),
            new Color(1f, 0f, 0f, 1f),
            rightColorStartX: TextureSize / 2);

        var samples = RenderUberUnlitSamples(
            material => ConfigureEmission(material, 2, 0, 0, 1f, m =>
            {
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.08f, 0.08f, 1f)));
                m.SetTexture("_EmissionMap", emissionMap);
                m.SetFloat("_EmissionMapChannelsX", 0);
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }),
            new[] { new Vector2(0.25f, 0.5f), new Vector2(0.75f, 0.5f) },
            ownedObjects: new UnityEngine.Object[] { emissionMap });

        Assert.That(samples[1].g, Is.GreaterThan(samples[0].g + 0.20f), $"Masked {samples[0]}, unmasked {samples[1]}");
    }

    [Test]
    public void UberUnlitEmissionMap2DAffectsOutput()
    {
        var emissionMap = CreateSolidTexture(new Color(0f, 0.9f, 0f, 1f));
        var color = RenderUberUnlit(
            material => ConfigureEmission(material, 2, 0, 0, 1f, m =>
            {
                m.SetTexture("_EmissionMap", emissionMap);
                m.SetFloat("_EmissionMapChannelsX", 1);
                m.SetColor("_EmissionColor", new Color(0.9f, 0f, 0f, 1f));
            }),
            ownedObjects: new UnityEngine.Object[] { emissionMap });

        Assert.That(color.r, Is.GreaterThan(color.g + 0.12f), color.ToString());
    }

    [Test]
    public void UberUnlitEmissionMap2DArrayAffectsOutput()
    {
        var emissionMap = CreateTexture2DArray(
            new Color(0f, 0f, 0f, 1f),
            new Color(1f, 0f, 0f, 1f),
            new Color(0f, 0f, 0f, 1f));

        var color = RenderUberUnlit(
            material => ConfigureEmission(material, 2, 0, 1, 1f, m =>
            {
                m.SetTexture("_EmissionMap2DArray", emissionMap);
                m.SetFloat("_EmissionMapProgress", 0.45f);
                m.SetFloat("_EmissionMapSliceCount", emissionMap.depth);
                m.SetFloat("_EmissionMapChannelsX", 0);
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }),
            ownedObjects: new UnityEngine.Object[] { emissionMap });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.12f), color.ToString());
    }

    [Test]
    public void UberUnlitEmissionMap3DAffectsOutput()
    {
        var emissionMap = CreateTexture3D(
            new Color(0f, 0f, 0f, 1f),
            new Color(1f, 0f, 0f, 1f),
            new Color(0f, 0f, 0f, 1f));

        var color = RenderUberUnlit(
            material => ConfigureEmission(material, 2, 0, 2, 1f, m =>
            {
                m.SetTexture("_EmissionMap3D", emissionMap);
                m.SetFloat("_EmissionMapProgress", 0.45f);
                m.SetFloat("_EmissionMapSliceCount", emissionMap.depth);
                m.SetFloat("_EmissionMapChannelsX", 0);
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }),
            ownedObjects: new UnityEngine.Object[] { emissionMap });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.10f), color.ToString());
    }

    [Test]
    public void UberUnlitEmissionMapRandomRowSelectionAffectsOutput()
    {
        var emissionMap = CreateTexture2DArray(
            new Color(0f, 0f, 0f, 1f),
            new Color(0.1f, 0f, 0f, 1f),
            new Color(1f, 0f, 0f, 1f),
            new Color(0.9f, 0f, 0f, 1f));

        var color = RenderUberUnlit(
            material => ConfigureEmission(material, 2, 0, 1, 1f, m =>
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
            ownedObjects: new UnityEngine.Object[] { emissionMap });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.10f), color.ToString());
    }

    [Test]
    public void UberUnlitEmissionColorFromBaseColorAffectsOutput()
    {
        var color = RenderUberUnlit(
            material => ConfigureEmission(material, 1, 1, 0, 1f, m =>
            {
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.55f, 0.08f, 0.08f, 1f)));
            }));

        Assert.That(color.r, Is.GreaterThan(color.g + 0.18f), color.ToString());
        Assert.That(color.r, Is.GreaterThan(color.b + 0.18f), color.ToString());
    }

    [Test]
    public void UberUnlitEmissionColorFromExplicitColorAffectsOutput()
    {
        var color = RenderUberUnlit(
            material => ConfigureEmission(material, 1, 0, 0, 1f, m =>
            {
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }));

        Assert.That(color.g, Is.GreaterThan(color.r + 0.18f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.18f), color.ToString());
    }

    [Test]
    public void UberUnlitEmissionColorMapOrRampAffectsOutput()
    {
        var emissionMap = CreateSolidTexture(new Color(0.8f, 0f, 0f, 1f));
        var ramp = CreateHorizontalSplitTexture(
            new Color(0.9f, 0.05f, 0.05f, 1f),
            new Color(0.05f, 0.05f, 0.9f, 1f),
            rightColorStartX: TextureSize / 2);

        var color = RenderUberUnlit(
            material => ConfigureEmission(material, 2, 2, 0, 1f, m =>
            {
                m.SetTexture("_EmissionMap", emissionMap);
                m.SetTexture("_EmissionColorRamp", ramp);
                m.SetFloat("_EmissionMapChannelsX", 0);
            }),
            ownedObjects: new UnityEngine.Object[] { emissionMap, ramp });

        Assert.That(color.b, Is.GreaterThan(color.r + 0.12f), color.ToString());
    }

    [Test]
    public void UberUnlitEmissionIntensityAffectsOutput()
    {
        var low = RenderEmissionAreaAll(intensity: 0.15f);
        var high = RenderEmissionAreaAll(intensity: 1.2f);

        Assert.That(high.g, Is.GreaterThan(low.g + 0.20f), $"Low {low}, high {high}");
    }

    [Test]
    public void UberUnlitKeepEdgeTransparencyPreservesTransparentEdges()
    {
        var keep = RenderEmissionAreaAlpha(baseAlpha: 0.2f, keepEdgeTransparency: true, renderTransparent: true);
        var harden = RenderEmissionAreaAlpha(baseAlpha: 0.2f, keepEdgeTransparency: false, renderTransparent: true);

        Assert.That(harden.g, Is.GreaterThan(keep.g + 0.18f), $"Keep {keep}, harden {harden}");
    }

    [Test]
    public void UberUnlitFlowMapOffsetsEmissionSampling()
    {
        var emissionMap = CreateHorizontalSplitTexture(
            new Color(0f, 0f, 0f, 1f),
            new Color(1f, 0f, 0f, 1f),
            rightColorStartX: TextureSize - 1);
        var flowTexture = CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f));

        var color = RenderUberUnlit(
            material => ConfigureEmission(material, 2, 0, 0, 1f, m =>
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
            ownedObjects: new UnityEngine.Object[] { emissionMap, flowTexture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.12f), color.ToString());
    }

    [Test]
    public void UberUnlitRimTransparencyAffectsOutput()
    {
        var disabled = RenderRimTransparency(false, inverse: false, progress: 0.5f, sharpness: 0.5f, Vector3.right);
        var enabled = RenderRimTransparency(true, inverse: false, progress: 0.5f, sharpness: 0.5f, Vector3.right);

        Assert.That(disabled.r, Is.GreaterThan(enabled.r + 0.25f), $"Disabled {disabled}, enabled {enabled}");
    }

    [Test]
    public void UberUnlitInverseRimTransparencyInvertsEffect()
    {
        var normal = RenderRimTransparency(true, inverse: false, progress: 0.5f, sharpness: 0.5f, Vector3.right);
        var inverse = RenderRimTransparency(true, inverse: true, progress: 0.5f, sharpness: 0.5f, Vector3.right);

        Assert.That(inverse.r, Is.GreaterThan(normal.r + 0.25f), $"Normal {normal}, inverse {inverse}");
    }

    [Test]
    public void UberUnlitRimTransparencyProgressAffectsOutput()
    {
        var normal = new Vector3(0.866f, 0f, -0.5f).normalized;
        var lowProgress = RenderRimTransparency(true, inverse: false, progress: 0.2f, sharpness: 0.5f, normal);
        var highProgress = RenderRimTransparency(true, inverse: false, progress: 0.8f, sharpness: 0.5f, normal);

        Assert.That(lowProgress.r, Is.GreaterThan(highProgress.r + 0.20f), $"Low {lowProgress}, high {highProgress}");
    }

    [Test]
    public void UberUnlitRimTransparencySharpnessAffectsFalloff()
    {
        var normal = new Vector3(0.848f, 0f, -0.53f).normalized;
        var soft = RenderRimTransparency(true, inverse: false, progress: 0.5f, sharpness: 0f, normal);
        var sharp = RenderRimTransparency(true, inverse: false, progress: 0.5f, sharpness: 0.95f, normal);

        Assert.That(sharp.r, Is.GreaterThan(soft.r + 0.10f), $"Soft {soft}, sharp {sharp}");
    }

    [Test]
    public void UberUnlitLuminanceTransparencyAffectsOutput()
    {
        var samples = RenderLuminanceTransparencySamples(inverse: false, progress: 0.5f, sharpness: 0.5f);

        Assert.That(samples[1].r, Is.GreaterThan(samples[0].r + 0.30f), $"Dark {samples[0]}, bright {samples[1]}");
    }

    [Test]
    public void UberUnlitInverseLuminanceTransparencyInvertsEffect()
    {
        var samples = RenderLuminanceTransparencySamples(inverse: true, progress: 0.5f, sharpness: 0.5f);

        Assert.That(samples[0].r, Is.GreaterThan(samples[1].r + 0.12f), $"Dark {samples[0]}, bright {samples[1]}");
    }

    [Test]
    public void UberUnlitLuminanceTransparencyProgressAffectsOutput()
    {
        var lowProgress = RenderLuminanceTransparency(new Color(0.5f, 0.5f, 0.5f, 1f), inverse: false, progress: 0.2f, sharpness: 0.5f);
        var highProgress = RenderLuminanceTransparency(new Color(0.5f, 0.5f, 0.5f, 1f), inverse: false, progress: 0.8f, sharpness: 0.5f);

        Assert.That(lowProgress.r, Is.GreaterThan(highProgress.r + 0.20f), $"Low {lowProgress}, high {highProgress}");
    }

    [Test]
    public void UberUnlitLuminanceTransparencySharpnessAffectsOutput()
    {
        var soft = RenderLuminanceTransparency(new Color(0.53f, 0.53f, 0.53f, 1f), inverse: false, progress: 0.5f, sharpness: 0f);
        var sharp = RenderLuminanceTransparency(new Color(0.53f, 0.53f, 0.53f, 1f), inverse: false, progress: 0.5f, sharpness: 0.95f);

        Assert.That(sharp.r, Is.GreaterThan(soft.r + 0.08f), $"Soft {soft}, sharp {sharp}");
    }

    [Test]
    public void UberUnlitRimAndLuminanceTransparencyCanCombine()
    {
        var normal = new Vector3(0.866f, 0f, -0.5f).normalized;
        var disabled = RenderCombinedTransparency(rim: false, luminance: false, normal);
        var rimOnly = RenderCombinedTransparency(rim: true, luminance: false, normal);
        var luminanceOnly = RenderCombinedTransparency(rim: false, luminance: true, normal);
        var combined = RenderCombinedTransparency(rim: true, luminance: true, normal);

        Assert.That(disabled.r, Is.GreaterThan(combined.r + 0.15f), $"Disabled {disabled}, combined {combined}");
        Assert.That(rimOnly.r, Is.GreaterThan(combined.r + 0.08f), $"Rim {rimOnly}, combined {combined}");
        Assert.That(luminanceOnly.r, Is.GreaterThan(combined.r + 0.08f), $"Luminance {luminanceOnly}, combined {combined}");
    }

    [Test]
    public void UberUnlitTransparencyKeepsBaseTintAlphaTransitionAndEmissionUsable()
    {
        var transitionTexture = CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f));
        var color = RenderUberUnlit(
            material =>
            {
                ConfigureAlphaTransition(material, 2, 0.45f, 0, m =>
                {
                    m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.35f, 0.35f, 0.35f, 1f)));
                    m.SetTexture("_AlphaTransitionMap", transitionTexture);
                    m.SetFloat("_TintAreaMode", 1);
                    m.SetFloat("_TintColorMode", 0);
                    m.SetColor("_TintColor", new Color(0.2f, 0.9f, 0.2f, 1f));
                    m.SetFloat("_EmissionAreaType", 1);
                    m.SetFloat("_EmissionColorType", 0);
                    m.SetColor("_EmissionColor", new Color(0f, 0.4f, 0f, 1f));
                    m.SetFloat("_EmissionIntensity", 0.8f);
                    m.SetFloat("_LuminanceTransparencyEnabled", 1f);
                    m.SetFloat("_LuminanceTransparencyProgress", 0.2f);
                    m.SetFloat("_LuminanceTransparencySharpness", 0.5f);
                });
            },
            background: Color.black,
            ownedObjects: new UnityEngine.Object[] { transitionTexture });

        Assert.That(color.g, Is.GreaterThan(color.r + 0.15f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(color.b + 0.15f), color.ToString());
        Assert.That(color.g, Is.GreaterThan(0.18f), color.ToString());
    }

    private Color RenderUberUnlit(
        Color baseMapColor,
        Color tintColor,
        Color vertexColor,
        bool tintEnabled,
        bool alphaTestEnabled = false,
        Color? background = null)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var texture = CreateSolidTexture(baseMapColor);
        return RenderUberUnlit(
            material =>
            {
                material.SetTexture("_BaseMap", texture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_RenderType", alphaTestEnabled ? 1 : 0);
                material.SetFloat("_Cutoff", 0.5f);
                material.SetFloat("_TintAreaMode", tintEnabled ? 1 : 0);
                material.SetFloat("_TintColorMode", 0);
                material.SetColor("_TintColor", tintColor);
            },
            CreateQuad(vertexColor),
            background,
            new UnityEngine.Object[] { texture });
    }

    private Color RenderAlphaTransition(Texture transitionTexture, float progress, int mode)
    {
        return RenderUberUnlit(
            material => ConfigureAlphaTransition(material, mode, progress, 0, m => m.SetTexture("_AlphaTransitionMap", transitionTexture)),
            background: Color.black,
            ownedObjects: new UnityEngine.Object[] { transitionTexture });
    }

    private Color RenderSecondAlphaTransition(Texture mainTexture, Texture secondTexture, int secondBlendMode)
    {
        return RenderUberUnlit(
            material => ConfigureAlphaTransition(material, 2, 0.5f, 0, m =>
            {
                m.SetTexture("_AlphaTransitionMap", mainTexture);
                m.SetFloat("_AlphaTransitionSecondTextureBlendMode", secondBlendMode);
                m.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 0);
                m.SetTexture("_AlphaTransitionMapSecondTexture", secondTexture);
                m.SetFloat("_AlphaTransitionProgressSecondTexture", 0.5f);
            }),
            background: Color.black,
            ownedObjects: new[] { mainTexture, secondTexture });
    }

    private static void ConfigureAlphaTransition(Material material, int mode, float progress, int mapMode, Action<Material> configureMap)
    {
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

    private Color RenderEmissionAreaAll(float intensity)
    {
        return RenderUberUnlit(
            material => ConfigureEmission(material, 1, 0, 0, intensity, m =>
            {
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.08f, 0.08f, 1f)));
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            }));
    }

    private Color RenderEmissionAreaAlpha(float baseAlpha, bool keepEdgeTransparency, bool renderTransparent = false)
    {
        return RenderUberUnlit(
            material => ConfigureEmission(material, 3, 0, 0, 1f, m =>
            {
                m.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.08f, 0.08f, baseAlpha)));
                m.SetFloat("_RenderType", renderTransparent ? 2 : 0);
                m.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                m.SetFloat("_KeepEdgeTransparency", keepEdgeTransparency ? 1f : 0f);
            }),
            background: Color.black);
    }

    private static void ConfigureEmission(Material material, int areaType, int colorType, int mapMode, float intensity, Action<Material> configure)
    {
        material.SetFloat("_RenderType", 0);
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.08f, 0.08f, 0.08f, 1f)));
        material.SetFloat("_BaseMapMode", 0);
        material.SetFloat("_EmissionAreaType", areaType);
        material.SetFloat("_EmissionColorType", colorType);
        material.SetFloat("_EmissionMapMode", mapMode);
        material.SetFloat("_EmissionIntensity", intensity);
        material.SetFloat("_KeepEdgeTransparency", 1f);
        configure(material);
    }

    private Color RenderUberUnlit(
        Action<Material> configureMaterial,
        Mesh mesh = null,
        Color? background = null,
        IEnumerable<UnityEngine.Object> ownedObjects = null,
        bool orthographic = true)
    {
        return RenderUberUnlitSamples(configureMaterial, new[] { new Vector2(0.5f, 0.5f) }, mesh, background, ownedObjects, orthographic)[0];
    }

    private Color[] RenderUberUnlitSamples(
        Action<Material> configureMaterial,
        IReadOnlyList<Vector2> normalizedSamplePositions,
        Mesh mesh = null,
        Color? background = null,
        IEnumerable<UnityEngine.Object> ownedObjects = null,
        bool orthographic = true)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);

        NovaLitTestTextures.UseDiffuseDefaults(material);
        var actualMesh = mesh ?? CreateQuad(Color.white);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberUnlitSmokeCamera");
        var quadObject = new GameObject("NovaBuiltInUberUnlitSmokeQuad");
        var objectsToDestroy = ownedObjects?.ToList() ?? new List<UnityEngine.Object>();
        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            material.SetFloat("_RenderType", 0);
            configureMaterial(material);
            SetupUberUnlit(material);

            var filter = quadObject.AddComponent<MeshFilter>();
            filter.sharedMesh = actualMesh;
            var renderer = quadObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background ?? new Color(0.01f, 0.01f, 0.01f, 1f);
            camera.orthographic = orthographic;
            camera.orthographicSize = 0.75f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 10f;
            camera.transform.position = new Vector3(0f, 0f, -2f);
            camera.targetTexture = target;
            camera.Render();

            var previous = RenderTexture.active;
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, RenderSize, RenderSize), 0, 0);
            readback.Apply();
            RenderTexture.active = previous;

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
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(quadObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(actualMesh);
            UnityEngine.Object.DestroyImmediate(material);
            foreach (var ownedObject in objectsToDestroy)
            {
                UnityEngine.Object.DestroyImmediate(ownedObject);
            }
        }
    }

    private Color RenderRimTransparency(bool enabled, bool inverse, float progress, float sharpness, Vector3 normal)
    {
        return RenderUberUnlit(
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

    private Color[] RenderLuminanceTransparencySamples(bool inverse, float progress, float sharpness)
    {
        var texture = CreateHorizontalSplitTexture(
            new Color(0.35f, 0.35f, 0.35f, 1f),
            Color.white,
            rightColorStartX: TextureSize / 2);

        return RenderUberUnlitSamples(
            material =>
            {
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetTexture("_BaseMap", texture);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_LuminanceTransparencyEnabled", 1f);
                material.SetFloat("_InverseLuminanceTransparency", inverse ? 1f : 0f);
                material.SetFloat("_LuminanceTransparencyProgress", progress);
                material.SetFloat("_LuminanceTransparencySharpness", sharpness);
            },
            new[] { new Vector2(0.25f, 0.5f), new Vector2(0.75f, 0.5f) },
            background: Color.black,
            ownedObjects: new UnityEngine.Object[] { texture });
    }

    private Color RenderLuminanceTransparency(Color baseColor, bool inverse, float progress, float sharpness)
    {
        return RenderUberUnlit(
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

    private Color RenderCombinedTransparency(bool rim, bool luminance, Vector3 normal)
    {
        return RenderUberUnlit(
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

    private static Texture2D CreateSolidTexture(Color color)
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        var pixels = Enumerable.Repeat(color, TextureSize * TextureSize).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateHorizontalSplitTexture(Color leftColor, Color rightColor, int rightColorStartX)
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
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

    private static Texture2DArray CreateTexture2DArray(params Color[] sliceColors)
    {
        var texture = new Texture2DArray(TextureSize, TextureSize, sliceColors.Length, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        for (var slice = 0; slice < sliceColors.Length; slice++)
        {
            var pixels = Enumerable.Repeat(sliceColors[slice], TextureSize * TextureSize).ToArray();
            texture.SetPixels(pixels, slice);
        }

        texture.Apply();
        return texture;
    }

    private static Texture3D CreateTexture3D(params Color[] depthColors)
    {
        var texture = new Texture3D(TextureSize, TextureSize, depthColors.Length, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
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

    private static Mesh CreateQuad(Color vertexColor, Vector4? customCoord1 = null, Vector4? customCoord2 = null, bool skewForParallax = false, Vector3? normal = null)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberUnlitSmokeQuad" };
        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, skewForParallax ? -0.2f : 0f),
            new Vector3(0.5f, -0.5f, skewForParallax ? 0.2f : 0f),
            new Vector3(0.5f, 0.5f, skewForParallax ? 0.2f : 0f),
            new Vector3(-0.5f, 0.5f, skewForParallax ? -0.2f : 0f),
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
        mesh.SetUVs(2, Enumerable.Repeat(customCoord2 ?? Vector4.zero, 4).ToList());
        mesh.normals = Enumerable.Repeat((normal ?? Vector3.back).normalized, 4).ToArray();
        var tangent = skewForParallax ? new Vector4(0.45f, 0f, 0.89f, 1f) : new Vector4(1f, 0f, 0f, 1f);
        mesh.tangents = Enumerable.Repeat(tangent, 4).ToArray();
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void SetupUberUnlit(Material material)
    {
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberUnlitMaterialPostProcessor", "SetupMaterialKeywords", material);
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberUnlitMaterialPostProcessor", "SetupMaterialBlendMode", material);
        if (material.HasProperty("_EnvironmentReflections"))
        {
            material.SetFloat("_EnvironmentReflections", 0f);
            material.SetFloat("_SpecularHighlights", 0f);
            InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor", "SetupMaterialKeywords", material);
        }
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
}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberUnlitRenderingSmokeTests
{
    private const string ShaderName = "Nova/Particles/UberUnlit";
    private const int TextureSize = 4;
    private const int RenderSize = 64;

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

    private static Color RenderUberUnlit(
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

    private static Color RenderUberUnlit(
        Action<Material> configureMaterial,
        Mesh mesh = null,
        Color? background = null,
        IEnumerable<UnityEngine.Object> ownedObjects = null)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var actualMesh = mesh ?? CreateQuad(Color.white);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberUnlitSmokeCamera");
        var quadObject = new GameObject("NovaBuiltInUberUnlitSmokeQuad");
        var objectsToDestroy = ownedObjects?.ToList() ?? new List<UnityEngine.Object>();

        try
        {
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
            camera.orthographic = true;
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

            return readback.GetPixel(RenderSize / 2, RenderSize / 2);
        }
        finally
        {
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

    private static Mesh CreateQuad(Color vertexColor, Vector4? customCoord1 = null, Vector4? customCoord2 = null, bool skewForParallax = false)
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
        mesh.normals = Enumerable.Repeat(Vector3.back, 4).ToArray();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUIParticlesUberUnlitInteractionSmokeTests
{
    private const string ShaderName = "Nova/UIParticles/UberUnlit";
    private const int RenderSize = 96;
    private const int TextureSize = 4;

    [Test]
    public void UIParticlesUberUnlitComplexTextureTintFlowColorCorrectionInteraction()
    {
        var background = new Color(0.02f, 0.03f, 0.08f, 1f);
        var simple = RenderUiColor(material =>
        {
            material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
            material.SetFloat("_BaseMapMode", 0);
        }, background: background);

        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    material.SetFloat("_BaseMapMode", 2);
                    material.SetFloat("_BaseMapProgress", 0.45f);
                    material.SetFloat("_BaseMapSliceCount", 3f);
                    material.SetTexture("_BaseMap3D", CreateTexture3D(Color.white, Color.white, Color.white));
                    material.SetFloat("_TintAreaMode", 1);
                    material.SetFloat("_TintColorMode", 1);
                    material.SetFloat("_TintBlendRate", 1f);
                    material.SetTexture("_TintMap", CreateHorizontalSplitTexture(new Color(0.9f, 0.05f, 0.05f, 1f), new Color(0.05f, 0.9f, 0.05f, 1f), TextureSize - 1));
                    material.SetTexture("_FlowMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
                    material.SetFloat("_FlowMapTarget", 2f);
                    material.SetFloat("_FlowMapChannelsX", 0f);
                    material.SetFloat("_FlowMapChannelsY", 1f);
                    material.SetFloat("_FlowIntensity", 0.4f);
                    material.SetFloat("_ColorCorrectionMode", 2f);
                    material.SetTexture("_GradientMap", CreateHorizontalSplitTexture(new Color(0.9f, 0.05f, 0.05f, 1f), new Color(0.05f, 0.05f, 0.9f, 1f), TextureSize / 2));
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-0.05f, -1f, 1f, 1f));
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[0].b, Is.GreaterThan(samples[0].r + 0.02f), $"clipped {samples[0]}, visible {samples[1]}");
        Assert.That(samples[1].b, Is.GreaterThan(samples[1].r + 0.08f), $"simple {simple}, visible {samples[1]}");
        Assert.That(ColorDistance(simple, samples[1]), Is.GreaterThan(0.25f), $"simple {simple}, visible {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitAlphaTransitionEmissionTransparencyInteraction()
    {
        var background = new Color(0.02f, 0.04f, 0.75f, 1f);
        var visible = RenderAlphaEmissionTransparency(progress: 0f, background);
        var transparent = RenderAlphaEmissionTransparency(progress: 1f, background);

        Assert.That(visible.g, Is.GreaterThan(transparent.g + 0.12f), $"visible {visible}, transparent {transparent}");
        Assert.That(transparent.b, Is.GreaterThan(visible.b + 0.15f), $"visible {visible}, transparent {transparent}");
        Assert.That(transparent.g, Is.LessThan(0.14f), transparent.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitDepthFeaturesWithEmissionAndTransparencyInteraction()
    {
        var withoutDepth = RenderDepthScene(material => ConfigureDepthEmissionTransparency(material, useDepthFeatures: false)).Center;
        var withDepth = RenderDepthScene(material => ConfigureDepthEmissionTransparency(material, useDepthFeatures: true)).Center;

        Assert.That(withoutDepth.g, Is.GreaterThan(withDepth.g + 0.05f), $"without {withoutDepth}, with {withDepth}");
        Assert.That(withDepth.g, Is.GreaterThan(0.02f), withDepth.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitVertexDeformationWithDepthAndAlphaInteraction()
    {
        var undeformed = RenderInteractionStats(material => ConfigureDepthAlphaDeformation(material, deform: false, cutoff: 0.2f));
        var deformed = RenderInteractionStats(material => ConfigureDepthAlphaDeformation(material, deform: true, cutoff: 0.2f));
        var clipped = RenderInteractionStats(material => ConfigureDepthAlphaDeformation(material, deform: true, cutoff: 0.9f));

        Assert.That(deformed.CenterX, Is.GreaterThan(undeformed.CenterX + 8), $"undeformed {undeformed}, deformed {deformed}");
        Assert.That(clipped.LitPixels, Is.LessThan(deformed.LitPixels * 0.35f), $"deformed {deformed}, clipped {clipped}");
    }

    [Test]
    public void UIParticlesUberUnlitStencilClipWithComplexFeatureStack()
    {
        var background = new Color(0.02f, 0.03f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateStencilMask(new Rect(-0.6f, -0.5f, 0.7f, 1f)),
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.25f, 0.04f, 0.04f, 1f)));
                    material.SetFloat("_BaseMapMode", 0);
                    material.SetFloat("_AlphaTransitionMode", 1);
                    material.SetFloat("_AlphaTransitionMapMode", 0);
                    material.SetFloat("_AlphaTransitionProgress", 0.15f);
                    material.SetTexture("_AlphaTransitionMap", CreateSolidTexture(Color.white));
                    material.SetFloat("_EmissionAreaType", 1);
                    material.SetFloat("_EmissionColorType", 0);
                    material.SetColor("_EmissionColor", new Color(0.8f, 0f, 0f, 1f));
                    material.SetFloat("_EmissionIntensity", 1f);
                    material.SetTexture("_VertexDeformationMap", CreateSolidTexture(Color.white));
                    material.SetFloat("_VertexDeformationIntensity", 0.18f);
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-1f, -1f, 0.2f, 1f));
                    material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                    material.SetFloat("_Stencil", 1);
                    material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                    material.SetFloat("_StencilReadMask", 255);
                    material.SetFloat("_StencilWriteMask", 255);
                }, CreateQuad(Color.white, normal: Vector3.right)),
            },
            new[] { new Vector2(0.35f, 0.5f), new Vector2(0.6f, 0.5f), new Vector2(0.82f, 0.5f) },
            background);

        Assert.That(samples[0].r, Is.GreaterThan(samples[0].b + 0.20f), $"inside {samples[0]}");
        Assert.That(samples[1].b, Is.GreaterThan(samples[1].r + 0.02f), $"clip edge {samples[1]}");
        Assert.That(samples[2].b, Is.GreaterThan(samples[2].r + 0.02f), $"stencil outside {samples[2]}");
    }

    [Test]
    public void UIParticlesUberUnlitColorMaskWithTransparencyAndEmission()
    {
        var background = new Color(0.02f, 0.04f, 0.75f, 1f);
        var visible = RenderColorMaskInteraction(colorMask: 15, background);
        var masked = RenderColorMaskInteraction(colorMask: 0, background);

        Assert.That(visible.g, Is.GreaterThan(masked.g + 0.15f), $"visible {visible}, masked {masked}");
        Assert.That(masked.b, Is.GreaterThan(masked.g + 0.30f), masked.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitVertexAlphaWithAlphaTransitionAndEmission()
    {
        var fullAlpha = RenderVertexAlphaTransitionEmission(new Color(1f, 1f, 1f, 1f));
        var transitionAlpha = RenderVertexAlphaTransitionEmission(new Color(1f, 1f, 1f, 0.2f));

        Assert.That(transitionAlpha.g, Is.GreaterThan(fullAlpha.g + 0.10f), $"full {fullAlpha}, transition {transitionAlpha}");
        Assert.That(fullAlpha.a, Is.GreaterThan(transitionAlpha.a + 0.10f), $"full {fullAlpha}, transition {transitionAlpha}");
        Assert.That(transitionAlpha.g, Is.GreaterThan(transitionAlpha.r + 0.20f), transitionAlpha.ToString());
    }

    private static Color RenderAlphaEmissionTransparency(float progress, Color background)
    {
        return RenderUiColor(
            material =>
            {
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.2f, 0.2f, 0.2f, 1f)));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_AlphaTransitionMode", 1);
                material.SetFloat("_AlphaTransitionMapMode", 0);
                material.SetFloat("_AlphaTransitionProgress", progress);
                material.SetTexture("_AlphaTransitionMap", CreateSolidTexture(Color.white));
                material.SetFloat("_EmissionAreaType", 3);
                material.SetFloat("_EmissionColorType", 0);
                material.SetColor("_EmissionColor", new Color(0f, 0.95f, 0f, 1f));
                material.SetFloat("_EmissionIntensity", 1.3f);
                material.SetFloat("_KeepEdgeTransparency", 1f);
                material.SetFloat("_LuminanceTransparencyEnabled", 1f);
                material.SetFloat("_LuminanceTransparencyProgress", 0.05f);
                material.SetFloat("_LuminanceTransparencySharpness", 0f);
            },
            background: background);
    }

    private static void ConfigureDepthEmissionTransparency(Material material, bool useDepthFeatures)
    {
        material.SetFloat("_RenderType", 2);
        material.SetFloat("_TransparentBlendMode", 0);
        material.SetFloat("_Cull", (float)CullMode.Off);
        material.SetFloat("_ZTest", (float)CompareFunction.Always);
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.18f, 0.18f, 0.18f, 1f)));
        material.SetFloat("_BaseMapMode", 0);
        material.SetFloat("_EmissionAreaType", 1);
        material.SetFloat("_EmissionColorType", 0);
        material.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
        material.SetFloat("_EmissionIntensity", 0.8f);
        material.SetFloat("_LuminanceTransparencyEnabled", 1f);
        material.SetFloat("_LuminanceTransparencyProgress", 0.08f);
        material.SetFloat("_LuminanceTransparencySharpness", 0f);
        material.EnableKeyword("UNITY_UI_CLIP_RECT");
        material.SetVector("_ClipRect", new Vector4(-1f, -1f, 1f, 1f));
        if (useDepthFeatures)
        {
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
            material.SetFloat("_DepthFadeEnabled", 1f);
            material.SetFloat("_DepthFadeNear", 1.8f);
            material.SetFloat("_DepthFadeFar", 3f);
            material.SetFloat("_DepthFadeWidth", 0.5f);
        }
    }

    private static void ConfigureDepthAlphaDeformation(Material material, bool deform, float cutoff)
    {
        material.SetFloat("_RenderType", 1);
        material.SetFloat("_Cutoff", cutoff);
        material.SetFloat("_DepthFadeEnabled", 1f);
        material.SetFloat("_DepthFadeNear", 0.5f);
        material.SetFloat("_DepthFadeFar", 5f);
        material.SetFloat("_DepthFadeWidth", 0.25f);
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.05f, 0.05f, 0.55f)));
        material.SetFloat("_BaseMapMode", 0);
        if (deform)
        {
            material.SetTexture("_VertexDeformationMap", CreateSolidTexture(Color.white));
            material.SetFloat("_VertexDeformationIntensity", 0.42f);
            material.SetFloat("_VertexDeformationBaseValue", 0f);
            material.SetFloat("_VertexDeformationMapChannel", 0f);
        }
    }

    private static Color RenderColorMaskInteraction(float colorMask, Color background)
    {
        return RenderUiColor(
            material =>
            {
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.18f, 0.18f, 0.18f, 1f)));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_EmissionAreaType", 1);
                material.SetFloat("_EmissionColorType", 0);
                material.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                material.SetFloat("_EmissionIntensity", 1f);
                material.SetFloat("_LuminanceTransparencyEnabled", 1f);
                material.SetFloat("_LuminanceTransparencyProgress", 0.05f);
                material.SetFloat("_LuminanceTransparencySharpness", 0f);
                material.SetFloat("_ColorMask", colorMask);
            },
            background: background);
    }

    private static Color RenderVertexAlphaTransitionEmission(Color vertexColor)
    {
        return RenderUiColor(
            material =>
            {
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.2f, 0.2f, 0.2f, 1f)));
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_VertexAlphaMode", 1f);
                material.SetFloat("_AlphaTransitionMode", 1f);
                material.SetFloat("_AlphaTransitionMapMode", 0f);
                material.SetFloat("_AlphaTransitionProgress", 0.05f);
                material.SetTexture("_AlphaTransitionMap", CreateSolidTexture(Color.white));
                material.SetFloat("_EmissionAreaType", 3f);
                material.SetFloat("_EmissionColorType", 0f);
                material.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
                material.SetFloat("_EmissionIntensity", 1f);
                material.SetFloat("_KeepEdgeTransparency", 1f);
            },
            mesh: CreateQuad(vertexColor),
            background: new Color(0.02f, 0.04f, 0.2f, 1f));
    }

    private static Color RenderUiColor(Action<Material> configureMaterial, Mesh mesh = null, Color? background = null)
    {
        return RenderUiSceneSamples(
            new[] { CreateUiDraw(configureMaterial, mesh) },
            new[] { new Vector2(0.5f, 0.5f) },
            background ?? Color.black)[0];
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
        var cameraObject = new GameObject("NovaBuiltInUiInteractionCamera");
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

                var gameObject = new GameObject("NovaBuiltInUiInteractionDraw");
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

    private static DepthResult RenderDepthScene(Action<Material> configureMaterial)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var particleMesh = CreateQuad(Color.white);
        var opaqueMesh = CreateQuad(Color.white);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUiInteractionDepthCamera");
        var particleObject = new GameObject("NovaBuiltInUiInteractionDepthParticle");
        var opaqueObject = new GameObject("NovaBuiltInUiInteractionDepthOpaque");
        var previous = RenderTexture.active;

        try
        {
            configureMaterial(material);
            SetupUberUnlit(material);

            particleObject.AddComponent<MeshFilter>().sharedMesh = particleMesh;
            particleObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            opaqueObject.transform.position = new Vector3(0f, 0f, 0.08f);
            opaqueObject.AddComponent<MeshFilter>().sharedMesh = opaqueMesh;
            opaqueObject.AddComponent<MeshRenderer>().sharedMaterial = CreateOpaqueMaterial(new Color(0f, 0f, 1f, 1f));

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

            return new DepthResult(readback.GetPixel(RenderSize / 2, RenderSize / 2), camera.depthTextureMode);
        }
        finally
        {
            RenderTexture.active = previous;
            var camera = cameraObject.GetComponent<Camera>();
            if (camera != null)
            {
                camera.targetTexture = null;
            }

            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(particleObject.GetComponent<MeshRenderer>()?.sharedMaterial);
            UnityEngine.Object.DestroyImmediate(opaqueObject.GetComponent<MeshRenderer>()?.sharedMaterial);
            UnityEngine.Object.DestroyImmediate(particleObject);
            UnityEngine.Object.DestroyImmediate(opaqueObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(particleMesh);
            UnityEngine.Object.DestroyImmediate(opaqueMesh);
            UnityEngine.Object.DestroyImmediate(material);
        }
    }

    private static RenderStats RenderInteractionStats(Action<Material> configureMaterial)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var mesh = CreateQuad(Color.white, normal: Vector3.right);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUiInteractionStatsCamera");
        var meshObject = new GameObject("NovaBuiltInUiInteractionStatsMesh");
        var previous = RenderTexture.active;

        try
        {
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            configureMaterial(material);
            SetupUberUnlit(material);

            meshObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

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

            return RenderStats.From(readback);
        }
        finally
        {
            RenderTexture.active = previous;
            var camera = cameraObject.GetComponent<Camera>();
            if (camera != null)
            {
                camera.targetTexture = null;
            }

            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(meshObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(mesh);
            UnityEngine.Object.DestroyImmediate(material);
        }
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

    private static Mesh CreateQuad(Color vertexColor, Vector4? customCoord1 = null, Vector3? normal = null)
    {
        return CreateQuad(vertexColor, new Rect(-0.5f, -0.5f, 1f, 1f), customCoord1, normal);
    }

    private static Mesh CreateQuad(Color vertexColor, Rect rect, Vector4? customCoord1 = null, Vector3? normal = null)
    {
        var mesh = new Mesh { name = "NovaBuiltInUiInteractionQuad" };
        mesh.vertices = new[]
        {
            new Vector3(rect.xMin, rect.yMin, 0f),
            new Vector3(rect.xMax, rect.yMin, 0f),
            new Vector3(rect.xMax, rect.yMax, 0f),
            new Vector3(rect.xMin, rect.yMax, 0f),
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
        mesh.SetUVs(2, Enumerable.Repeat(Vector4.zero, 4).ToList());
        mesh.normals = Enumerable.Repeat((normal ?? Vector3.back).normalized, 4).ToArray();
        mesh.tangents = Enumerable.Repeat(new Vector4(1f, 0f, 0f, 1f), 4).ToArray();
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateBounds();
        return mesh;
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

    private static Material CreateOpaqueMaterial(Color color)
    {
        var shader = Shader.Find("Standard");
        Assert.That(shader, Is.Not.Null, "Standard");
        var material = new Material(shader);
        material.SetColor("_Color", color);
        material.renderQueue = (int)RenderQueue.Geometry;
        return material;
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

    private static float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) + Mathf.Abs(a.a - b.a);
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

    private readonly struct DepthResult
    {
        public DepthResult(Color center, DepthTextureMode depthTextureMode)
        {
            Center = center;
            DepthTextureMode = depthTextureMode;
        }

        public Color Center { get; }
        public DepthTextureMode DepthTextureMode { get; }
    }

    private readonly struct RenderStats
    {
        private RenderStats(int litPixels, float centerX)
        {
            LitPixels = litPixels;
            CenterX = centerX;
        }

        public int LitPixels { get; }
        public float CenterX { get; }

        public static RenderStats From(Texture2D texture)
        {
            var litPixels = 0;
            var weightedX = 0f;
            var weight = 0f;

            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var color = texture.GetPixel(x, y);
                    var brightness = color.r + color.g + color.b;
                    if (brightness <= 0.08f)
                    {
                        continue;
                    }

                    litPixels++;
                    weightedX += x * brightness;
                    weight += brightness;
                }
            }

            return new RenderStats(litPixels, weight <= 0f ? 0f : weightedX / weight);
        }

        public override string ToString()
        {
            return $"LitPixels={LitPixels}, CenterX={CenterX}";
        }
    }
}

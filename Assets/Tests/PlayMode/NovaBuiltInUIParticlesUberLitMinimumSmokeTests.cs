using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUIParticlesUberLitMinimumSmokeTests
{
    private const string LitShaderName = "Nova/UIParticles/UberLit";
    private const string UnlitShaderName = "Nova/UIParticles/UberUnlit";
    private const int RenderSize = 96;
    private const int TextureSize = 4;

    [Test]
    public void UIParticlesUberLitRendersWithDirectionalLight()
    {
        var color = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(Color.white, 1.6f),
            Color.black);

        Assert.That(Luminance(color), Is.GreaterThan(0.18f), color.ToString());
    }

    [Test]
    public void UIParticlesUberLitRespondsToLightDirection()
    {
        var front = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(Color.white, 1.8f),
            Color.black);
        var back = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Back(Color.white, 1.8f),
            Color.black);

        Assert.That(Luminance(front), Is.GreaterThan(Luminance(back) + 0.12f), $"front {front}, back {back}");
    }

    [Test]
    public void UIParticlesUberLitRespondsToLightColor()
    {
        var red = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(1f, 0.05f, 0.05f, 1f), 1.8f),
            Color.black);
        var blue = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(0.05f, 0.05f, 1f, 1f), 1.8f),
            Color.black);

        Assert.That(red.r, Is.GreaterThan(red.b + 0.12f), red.ToString());
        Assert.That(blue.b, Is.GreaterThan(blue.r + 0.12f), blue.ToString());
    }

    [Test]
    public void UIParticlesUberLitAmbientApproximationAffectsOutput()
    {
        var darkAmbient = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Back(Color.white, 0f),
            Color.black);
        var brightAmbient = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Back(Color.white, 0f),
            new Color(0.35f, 0.35f, 0.35f, 1f));

        Assert.That(Luminance(brightAmbient), Is.GreaterThan(Luminance(darkAmbient) + 0.08f), $"dark {darkAmbient}, bright {brightAmbient}");
    }

    [Test]
    public void UIParticlesUberLitBaseMap2DAffectsLitAlbedo()
    {
        var color = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.05f, 0.05f, 1f))),
            LightCase.Front(Color.white, 1.8f),
            Color.black);

        Assert.That(color.r, Is.GreaterThan(color.g + 0.12f), color.ToString());
        Assert.That(color.r, Is.GreaterThan(color.b + 0.12f), color.ToString());
    }

    [Test]
    public void UIParticlesUberLitTintAndVertexColorAffectLitAlbedo()
    {
        var neutral = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(Color.white, 1.8f),
            Color.black);
        var tinted = RenderUiLit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                material.SetFloat("_TintAreaMode", 1);
                material.SetFloat("_TintColorMode", 0);
                material.SetColor("_TintColor", new Color(0.15f, 1f, 0.15f, 1f));
            },
            LightCase.Front(Color.white, 1.8f),
            Color.black,
            CreateQuad(new Color(1f, 0.65f, 1f, 1f)));

        Assert.That(tinted.g, Is.GreaterThan(tinted.r + 0.10f), tinted.ToString());
        Assert.That(ColorDistance(neutral, tinted), Is.GreaterThan(0.12f), $"neutral {neutral}, tinted {tinted}");
    }

    [Test]
    public void UIParticlesUberLitVertexAlphaAffectsOutput()
    {
        var background = new Color(0.05f, 0.10f, 0.75f, 1f);
        var opaque = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0.05f, 0.05f, 1f))),
            LightCase.Front(Color.white, 1.8f),
            Color.black,
            CreateQuad(Color.white),
            background);
        var transparent = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0.05f, 0.05f, 1f))),
            LightCase.Front(Color.white, 1.8f),
            Color.black,
            CreateQuad(new Color(1f, 1f, 1f, 0.2f)),
            background);

        Assert.That(opaque.r, Is.GreaterThan(transparent.r + 0.10f), $"opaque {opaque}, transparent {transparent}");
        Assert.That(transparent.b, Is.GreaterThan(opaque.b + 0.10f), $"opaque {opaque}, transparent {transparent}");
    }

    [Test]
    public void UIParticlesUberLitAlphaTestClipsBelowCutoff()
    {
        var background = new Color(0.05f, 0.10f, 0.80f, 1f);
        var color = RenderUiLit(
            material =>
            {
                material.SetFloat("_RenderType", 1);
                material.SetFloat("_Cutoff", 0.5f);
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0.05f, 0.05f, 0.25f)));
            },
            LightCase.Front(Color.white, 1.8f),
            Color.black,
            background: background);

        Assert.That(color.b, Is.GreaterThan(color.r + 0.20f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.g + 0.20f), color.ToString());
    }

    [Test]
    public void UIParticlesUberLitStencilMaskClipsOutput()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateStencilMask(new Rect(-0.5f, -0.5f, 0.5f, 1f)),
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.08f, 0.05f, 1f)));
                    material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                    material.SetFloat("_Stencil", 1);
                    material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                    material.SetFloat("_StencilReadMask", 255);
                    material.SetFloat("_StencilWriteMask", 255);
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background,
            LightCase.Front(Color.white, 1.8f),
            Color.black);

        Assert.That(samples[0].r, Is.GreaterThan(samples[0].b + 0.10f), $"inside {samples[0]}, outside {samples[1]}");
        Assert.That(samples[1].b, Is.GreaterThan(samples[1].r + 0.02f), $"inside {samples[0]}, outside {samples[1]}");
    }

    [Test]
    public void UIParticlesUberLitRectClipClipsOutput()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.08f, 0.05f, 1f)));
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-0.05f, -1f, 1f, 1f));
                }),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background,
            LightCase.Front(Color.white, 1.8f),
            Color.black);

        Assert.That(samples[1].r, Is.GreaterThan(samples[1].b + 0.10f), $"clipped {samples[0]}, visible {samples[1]}");
        Assert.That(samples[0].b, Is.GreaterThan(samples[0].r + 0.02f), $"clipped {samples[0]}, visible {samples[1]}");
    }

    [Test]
    public void UIParticlesUberLitMinimumPathDiffersFromUIParticlesUberUnlit()
    {
        var litRed = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(1f, 0.05f, 0.05f, 1f), 1.8f),
            Color.black);
        var litBlue = RenderUiLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(0.05f, 0.05f, 1f, 1f), 1.8f),
            Color.black);
        var unlitRed = RenderUiUnlit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(1f, 0.05f, 0.05f, 1f), 1.8f),
            Color.black);
        var unlitBlue = RenderUiUnlit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(0.05f, 0.05f, 1f, 1f), 1.8f),
            Color.black);

        Assert.That(ColorDistance(litRed, litBlue), Is.GreaterThan(0.18f), $"lit red {litRed}, lit blue {litBlue}");
        Assert.That(ColorDistance(unlitRed, unlitBlue), Is.LessThan(0.08f), $"unlit red {unlitRed}, unlit blue {unlitBlue}");
    }

    private static Color RenderUiLit(
        Action<Material> configureMaterial,
        LightCase lightCase,
        Color ambient,
        Mesh mesh = null,
        Color? background = null)
    {
        return RenderUiSceneSamples(
            new[] { CreateUiDraw(configureMaterial, mesh ?? CreateQuad(Color.white)) },
            new[] { new Vector2(0.5f, 0.5f) },
            background ?? new Color(0.01f, 0.01f, 0.01f, 1f),
            lightCase,
            ambient,
            LitShaderName,
            SetupUberLit)[0];
    }

    private static Color RenderUiUnlit(Action<Material> configureMaterial, LightCase lightCase, Color ambient)
    {
        return RenderUiSceneSamples(
            new[] { CreateUiDraw(configureMaterial, CreateQuad(Color.white)) },
            new[] { new Vector2(0.5f, 0.5f) },
            new Color(0.01f, 0.01f, 0.01f, 1f),
            lightCase,
            ambient,
            UnlitShaderName,
            SetupUberUnlit)[0];
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
        Color background,
        LightCase lightCase,
        Color ambient,
        string shaderName = LitShaderName,
        Action<Material> setupMaterial = null)
    {
        var shader = Shader.Find(shaderName);
        Assert.That(shader, Is.Not.Null, shaderName);

        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        var previousAmbientIntensity = RenderSettings.ambientIntensity;
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUIParticlesUberLitCamera");
        var lightObject = new GameObject("NovaBuiltInUIParticlesUberLitLight");
        var ownedObjects = new List<UnityEngine.Object> { target, readback, cameraObject, lightObject };
        var previous = RenderTexture.active;

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient;
            RenderSettings.ambientIntensity = 1f;

            foreach (var draw in draws)
            {
                var material = new Material(shader);
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetFloat("_Cull", (float)CullMode.Off);
                material.SetFloat("_ZTest", (float)CompareFunction.Always);
                if (material.HasProperty("_EnvironmentReflections"))
                {
                    material.SetFloat("_EnvironmentReflections", 0f);
                }

                draw.ConfigureMaterial(material);
                (setupMaterial ?? SetupUberLit)(material);
                if (draw.RenderQueue.HasValue)
                {
                    material.renderQueue = draw.RenderQueue.Value;
                }

                var gameObject = new GameObject("NovaBuiltInUIParticlesUberLitDraw");
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

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = lightCase.Color;
            light.intensity = lightCase.Intensity;
            lightObject.transform.rotation = lightCase.Rotation;

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.orthographic = true;
            camera.orthographicSize = 0.75f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 10f;
            camera.transform.position = new Vector3(0f, 0f, -2f);
            camera.targetTexture = target;
            camera.allowHDR = false;
            camera.allowMSAA = false;
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
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            RenderSettings.ambientIntensity = previousAmbientIntensity;
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

    private static Mesh CreateQuad(Color vertexColor)
    {
        return CreateQuad(vertexColor, new Rect(-0.5f, -0.5f, 1f, 1f));
    }

    private static Mesh CreateQuad(Color vertexColor, Rect rect)
    {
        var mesh = new Mesh { name = "NovaBuiltInUIParticlesUberLitQuad" };
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
        mesh.SetUVs(1, Enumerable.Repeat(Vector4.zero, 4).ToList());
        mesh.SetUVs(2, Enumerable.Repeat(Vector4.zero, 4).ToList());
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

    private static float Luminance(Color color)
    {
        return color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
    }

    private static float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
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

        public static LightCase Back(Color color, float intensity)
        {
            return new LightCase(Quaternion.LookRotation(Vector3.back), color, intensity);
        }
    }
}

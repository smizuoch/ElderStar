using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberLitMinimumSmokeTests
{
    private const string LitShaderName = "Nova/Particles/UberLit";
    private const string UnlitShaderName = "Nova/Particles/UberUnlit";
    private const int RenderSize = 96;
    private const int TextureSize = 4;

    [Test]
    public void UberLitRendersWithDirectionalLight()
    {
        var color = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(Color.white, 1.6f),
            Color.black);

        Assert.That(color.r + color.g + color.b, Is.GreaterThan(0.55f), color.ToString());
    }

    [Test]
    public void UberLitRespondsToLightDirection()
    {
        var front = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(Color.white, 1.8f),
            Color.black);
        var back = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Back(Color.white, 1.8f),
            Color.black);

        Assert.That(Luminance(front), Is.GreaterThan(Luminance(back) + 0.20f), $"front {front}, back {back}");
    }

    [Test]
    public void UberLitRespondsToLightColor()
    {
        var red = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(1f, 0.05f, 0.05f, 1f), 1.8f),
            Color.black);
        var blue = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(0.05f, 0.05f, 1f, 1f), 1.8f),
            Color.black);

        Assert.That(red.r, Is.GreaterThan(red.b + 0.20f), red.ToString());
        Assert.That(blue.b, Is.GreaterThan(blue.r + 0.20f), blue.ToString());
    }

    [Test]
    public void UberLitAmbientApproximationAffectsBackOrUnlitSide()
    {
        var darkAmbient = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Back(Color.white, 0f),
            Color.black);
        var brightAmbient = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Back(Color.white, 0f),
            new Color(0.35f, 0.35f, 0.35f, 1f));

        Assert.That(Luminance(brightAmbient), Is.GreaterThan(Luminance(darkAmbient) + 0.12f), $"dark {darkAmbient}, bright {brightAmbient}");
    }

    [Test]
    public void UberLitBaseMap2DAffectsLitAlbedo()
    {
        var color = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.05f, 0.05f, 1f))),
            LightCase.Front(Color.white, 1.8f),
            Color.black);

        Assert.That(color.r, Is.GreaterThan(color.g + 0.18f), color.ToString());
        Assert.That(color.r, Is.GreaterThan(color.b + 0.18f), color.ToString());
    }

    [Test]
    public void UberLitTintAndVertexColorAffectLitAlbedo()
    {
        var neutral = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(Color.white, 1.8f),
            Color.black);
        var tinted = RenderLit(
            material =>
            {
                material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
                material.SetFloat("_TintAreaMode", 1);
                material.SetFloat("_TintColorMode", 0);
                material.SetColor("_TintColor", new Color(0.15f, 1f, 0.15f, 1f));
            },
            LightCase.Front(Color.white, 1.8f),
            Color.black,
            CreateQuad(new Color(1f, 0.6f, 1f, 1f)));

        Assert.That(tinted.g, Is.GreaterThan(tinted.r + 0.18f), tinted.ToString());
        Assert.That(tinted.g, Is.GreaterThan(tinted.b + 0.08f), tinted.ToString());
        Assert.That(ColorDistance(neutral, tinted), Is.GreaterThan(0.20f), $"neutral {neutral}, tinted {tinted}");
    }

    [Test]
    public void UberLitAlphaTestClipsBelowCutoff()
    {
        var background = new Color(0.05f, 0.10f, 0.85f, 1f);
        var color = RenderLit(
            material =>
            {
                material.SetFloat("_RenderType", 1);
                material.SetFloat("_Cutoff", 0.5f);
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0.05f, 0.05f, 0.25f)));
            },
            LightCase.Front(Color.white, 1.8f),
            Color.black,
            background: background);

        Assert.That(color.b, Is.GreaterThan(color.r + 0.25f), color.ToString());
        Assert.That(color.b, Is.GreaterThan(color.g + 0.25f), color.ToString());
    }

    [Test]
    public void UberLitMinimumLitPathDiffersFromUberUnlit()
    {
        var litRed = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(1f, 0.05f, 0.05f, 1f), 1.8f),
            Color.black);
        var litBlue = RenderLit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(0.05f, 0.05f, 1f, 1f), 1.8f),
            Color.black);
        var unlitRed = RenderUnlit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(1f, 0.05f, 0.05f, 1f), 1.8f),
            Color.black);
        var unlitBlue = RenderUnlit(
            material => material.SetTexture("_BaseMap", CreateSolidTexture(Color.white)),
            LightCase.Front(new Color(0.05f, 0.05f, 1f, 1f), 1.8f),
            Color.black);

        Assert.That(ColorDistance(litRed, litBlue), Is.GreaterThan(0.30f), $"lit red {litRed}, lit blue {litBlue}");
        Assert.That(ColorDistance(unlitRed, unlitBlue), Is.LessThan(0.08f), $"unlit red {unlitRed}, unlit blue {unlitBlue}");
    }

    private static Color RenderLit(Action<Material> configureMaterial, LightCase lightCase, Color ambient, Mesh mesh = null, Color? background = null)
    {
        return RenderShader(LitShaderName, configureMaterial, SetupUberLit, lightCase, ambient, mesh, background);
    }

    private static Color RenderUnlit(Action<Material> configureMaterial, LightCase lightCase, Color ambient, Mesh mesh = null, Color? background = null)
    {
        return RenderShader(UnlitShaderName, configureMaterial, SetupUberUnlit, lightCase, ambient, mesh, background);
    }

    private static Color RenderShader(
        string shaderName,
        Action<Material> configureMaterial,
        Action<Material> setupMaterial,
        LightCase lightCase,
        Color ambient,
        Mesh mesh = null,
        Color? background = null)
    {
        var shader = Shader.Find(shaderName);
        Assert.That(shader, Is.Not.Null, shaderName);

        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        var previousAmbientIntensity = RenderSettings.ambientIntensity;
        var material = new Material(shader);
        NovaLitTestTextures.UseDiffuseDefaults(material);
        var actualMesh = mesh ?? CreateQuad(Color.white);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitSmokeCamera");
        var meshObject = new GameObject("NovaBuiltInUberLitSmokeMesh");
        var lightObject = new GameObject("NovaBuiltInUberLitSmokeLight");

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient;
            RenderSettings.ambientIntensity = 1f;

            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            material.SetFloat("_RenderType", 0);
            if (material.HasProperty("_EnvironmentReflections"))
            {
                material.SetFloat("_EnvironmentReflections", 0f);
            }
            configureMaterial(material);
            setupMaterial(material);

            meshObject.AddComponent<MeshFilter>().sharedMesh = actualMesh;
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = lightCase.Color;
            light.intensity = lightCase.Intensity;
            lightObject.transform.rotation = lightCase.Rotation;

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

            return AverageRegion(readback, 0.35f, 0.65f, 0.35f, 0.65f);
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
            UnityEngine.Object.DestroyImmediate(material);
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

    private static Mesh CreateQuad(Color vertexColor)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberLitSmokeQuad" };
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

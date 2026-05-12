using System;
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
        var material = new Material(shader);
        var mesh = CreateQuad(vertexColor);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberUnlitSmokeCamera");
        var quadObject = new GameObject("NovaBuiltInUberUnlitSmokeQuad");

        try
        {
            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_RenderType", alphaTestEnabled ? 1 : 0);
            material.SetFloat("_Cutoff", 0.5f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            material.SetFloat("_TintAreaMode", tintEnabled ? 1 : 0);
            material.SetFloat("_TintColorMode", 0);
            material.SetColor("_TintColor", tintColor);
            SetupUberUnlit(material);

            var filter = quadObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
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
            UnityEngine.Object.DestroyImmediate(mesh);
            UnityEngine.Object.DestroyImmediate(material);
            UnityEngine.Object.DestroyImmediate(texture);
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

    private static Mesh CreateQuad(Color vertexColor)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberUnlitSmokeQuad" };
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

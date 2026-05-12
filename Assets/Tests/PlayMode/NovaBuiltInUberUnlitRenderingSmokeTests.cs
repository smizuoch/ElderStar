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

    private static Mesh CreateQuad(Color vertexColor, Vector4? customCoord1 = null, Vector4? customCoord2 = null)
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
        mesh.SetUVs(1, Enumerable.Repeat(customCoord1 ?? Vector4.zero, 4).ToList());
        mesh.SetUVs(2, Enumerable.Repeat(customCoord2 ?? Vector4.zero, 4).ToList());
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

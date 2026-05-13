using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberUnlitVertexDeformationSmokeTests
{
    private const string ShaderName = "Nova/Particles/UberUnlit";
    private const int RenderSize = 64;
    private const int TextureSize = 4;

    [Test]
    public void UberUnlitVertexDeformationMovesGeometry()
    {
        var disabled = RenderVertexDeformationStats(null, 0.35f, 0f, 0);
        var enabled = RenderVertexDeformationStats(CreateSolidTexture(new Color(1f, 0f, 0f, 1f)), 0.35f, 0f, 0);

        Assert.That(enabled.MinX, Is.GreaterThan(disabled.MinX + 8), $"disabled {disabled}, enabled {enabled}");
        Assert.That(enabled.LitPixels, Is.GreaterThan(400), enabled.ToString());
    }

    [Test]
    public void UberUnlitVertexDeformationIntensityAffectsMotion()
    {
        var map = CreateSolidTexture(new Color(1f, 0f, 0f, 1f));

        var low = RenderVertexDeformationStats(map, 0.12f, 0f, 0, destroyTexture: false);
        var high = RenderVertexDeformationStats(map, 0.36f, 0f, 0);

        Assert.That(high.MinX, Is.GreaterThan(low.MinX + 8), $"low {low}, high {high}");
    }

    [Test]
    public void UberUnlitVertexDeformationBaseValueAffectsMotion()
    {
        var map = CreateSolidTexture(new Color(0.5f, 0f, 0f, 1f));

        var positive = RenderVertexDeformationStats(map, 0.4f, 0f, 0, destroyTexture: false);
        var negative = RenderVertexDeformationStats(map, 0.4f, 1f, 0);

        Assert.That(positive.CenterX, Is.GreaterThan(negative.CenterX + 12), $"positive {positive}, negative {negative}");
    }

    [Test]
    public void UberUnlitVertexDeformationChannelSelectionAffectsMotion()
    {
        var map = CreateSolidTexture(new Color(0f, 1f, 0f, 1f));

        var redChannel = RenderVertexDeformationStats(map, 0.35f, 0f, 0, destroyTexture: false);
        var greenChannel = RenderVertexDeformationStats(map, 0.35f, 0f, 1);

        Assert.That(greenChannel.MinX, Is.GreaterThan(redChannel.MinX + 8), $"red {redChannel}, green {greenChannel}");
    }

    [Test]
    public void UberUnlitVertexDeformationWithOffsetCoordinatesAffectsSampling()
    {
        var map = CreateHorizontalSplitTexture(new Color(0f, 0f, 0f, 1f), new Color(1f, 0f, 0f, 1f));

        var withoutOffset = RenderVertexDeformationStats(map, 0.35f, 0f, 0, destroyTexture: false);
        var withOffset = RenderVertexDeformationStats(
            map,
            0.35f,
            0f,
            0,
            material => material.SetFloat("_VertexDeformationMapOffsetXCoord", 1f),
            customCoord1: new Vector4(0.75f, 0f, 0f, 0f));

        Assert.That(withOffset.MinX, Is.GreaterThan(withoutOffset.MinX + 8), $"without {withoutOffset}, with {withOffset}");
    }

    [Test]
    public void UberUnlitVertexDeformationPreservesForwardColorFeatures()
    {
        var map = CreateSolidTexture(new Color(1f, 0f, 0f, 1f));
        var stats = RenderVertexDeformationStats(
            map,
            0.35f,
            0f,
            0,
            material =>
            {
                material.SetFloat("_EmissionAreaType", 1f);
                material.SetFloat("_EmissionColorType", 0f);
                material.SetColor("_EmissionColor", new Color(0f, 0.8f, 0f, 1f));
                material.SetFloat("_EmissionIntensity", 1f);
            },
            baseColor: new Color(0.15f, 0.15f, 0.15f, 1f));

        Assert.That(stats.MinX, Is.GreaterThan(20), stats.ToString());
        Assert.That(stats.AverageColor.g, Is.GreaterThan(stats.AverageColor.r + 0.2f), stats.ToString());
    }

    private static RenderStats RenderVertexDeformationStats(
        Texture2D deformationMap,
        float intensity,
        float baseValue,
        int channel,
        Action<Material> configureMaterial = null,
        Vector4? customCoord1 = null,
        Color? baseColor = null,
        bool destroyTexture = true)
    {
        var ownedObjects = new List<UnityEngine.Object>();
        if (deformationMap != null && destroyTexture)
        {
            ownedObjects.Add(deformationMap);
        }

        var baseMap = CreateSolidTexture(baseColor ?? new Color(0.9f, 0.05f, 0.05f, 1f));
        ownedObjects.Add(baseMap);

        return RenderToStats(
            material =>
            {
                material.SetTexture("_BaseMap", baseMap);
                material.SetFloat("_BaseMapMode", 0f);
                material.SetFloat("_RenderType", 0f);
                material.SetFloat("_VertexDeformationIntensity", intensity);
                material.SetFloat("_VertexDeformationBaseValue", baseValue);
                material.SetFloat("_VertexDeformationMapChannel", channel);
                if (deformationMap != null)
                {
                    material.SetTexture("_VertexDeformationMap", deformationMap);
                }

                configureMaterial?.Invoke(material);
            },
            CreateQuad(Vector3.right, customCoord1 ?? Vector4.zero),
            ownedObjects);
    }

    private static RenderStats RenderToStats(
        Action<Material> configureMaterial,
        Mesh mesh,
        IEnumerable<UnityEngine.Object> ownedObjects)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInVertexDeformationSmokeCamera");
        var meshObject = new GameObject("NovaBuiltInVertexDeformationSmokeMesh");
        var objectsToDestroy = ownedObjects.ToList();

        try
        {
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            configureMaterial(material);
            SetupUberUnlit(material);

            var filter = meshObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = meshObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
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

            return RenderStats.From(readback);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(meshObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(mesh);
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
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(Enumerable.Repeat(color, TextureSize * TextureSize).ToArray());
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateHorizontalSplitTexture(Color leftColor, Color rightColor)
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        for (var y = 0; y < TextureSize; y++)
        {
            for (var x = 0; x < TextureSize; x++)
            {
                texture.SetPixel(x, y, x < TextureSize / 2 ? leftColor : rightColor);
            }
        }

        texture.Apply();
        return texture;
    }

    private static Mesh CreateQuad(Vector3 normal, Vector4 customCoord1)
    {
        var mesh = new Mesh { name = "NovaBuiltInVertexDeformationSmokeQuad" };
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
        mesh.colors = Enumerable.Repeat(Color.white, 4).ToArray();
        mesh.SetUVs(1, Enumerable.Repeat(customCoord1, 4).ToList());
        mesh.SetUVs(2, Enumerable.Repeat(Vector4.zero, 4).ToList());
        mesh.normals = Enumerable.Repeat(normal.normalized, 4).ToArray();
        mesh.tangents = Enumerable.Repeat(new Vector4(1f, 0f, 0f, 1f), 4).ToArray();
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

    private readonly struct RenderStats
    {
        private RenderStats(int minX, int maxX, int litPixels, Color averageColor)
        {
            MinX = minX;
            MaxX = maxX;
            LitPixels = litPixels;
            AverageColor = averageColor;
        }

        public int MinX { get; }

        public int MaxX { get; }

        public int LitPixels { get; }

        public float CenterX => (MinX + MaxX) * 0.5f;

        public Color AverageColor { get; }

        public static RenderStats From(Texture2D texture)
        {
            var minX = texture.width;
            var maxX = -1;
            var litPixels = 0;
            var colorSum = Color.black;

            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var color = texture.GetPixel(x, y);
                    if (color.r + color.g + color.b <= 0.18f)
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    litPixels++;
                    colorSum += color;
                }
            }

            Assert.That(litPixels, Is.GreaterThan(0), "Rendered mesh should produce non-background pixels.");
            return new RenderStats(minX, maxX, litPixels, colorSum / litPixels);
        }

        public override string ToString()
        {
            return $"MinX={MinX}, MaxX={MaxX}, CenterX={CenterX}, LitPixels={LitPixels}, AverageColor={AverageColor}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUIParticlesUberUnlitVertexDeformationSmokeTests
{
    private const string ShaderName = "Nova/UIParticles/UberUnlit";
    private const int RenderSize = 64;
    private const int TextureSize = 4;

    [Test]
    public void UIParticlesUberUnlitVertexDeformationMovesGeometry()
    {
        var disabled = RenderVertexDeformationStats(null, 0.35f, 0f, 0);
        var enabled = RenderVertexDeformationStats(CreateSolidTexture(new Color(1f, 0f, 0f, 1f)), 0.35f, 0f, 0);

        Assert.That(enabled.MinX, Is.GreaterThan(disabled.MinX + 8), $"disabled {disabled}, enabled {enabled}");
        Assert.That(enabled.LitPixels, Is.GreaterThan(400), enabled.ToString());
    }

    [Test]
    public void UIParticlesUberUnlitVertexDeformationIntensityAffectsMotion()
    {
        var map = CreateSolidTexture(new Color(1f, 0f, 0f, 1f));

        var low = RenderVertexDeformationStats(map, 0.12f, 0f, 0, destroyTexture: false);
        var high = RenderVertexDeformationStats(map, 0.36f, 0f, 0);

        Assert.That(high.MinX, Is.GreaterThan(low.MinX + 8), $"low {low}, high {high}");
    }

    [Test]
    public void UIParticlesUberUnlitVertexDeformationBaseValueAffectsMotion()
    {
        var map = CreateSolidTexture(new Color(0.5f, 0f, 0f, 1f));

        var positive = RenderVertexDeformationStats(map, 0.4f, 0f, 0, destroyTexture: false);
        var negative = RenderVertexDeformationStats(map, 0.4f, 1f, 0);

        Assert.That(positive.CenterX, Is.GreaterThan(negative.CenterX + 12), $"positive {positive}, negative {negative}");
    }

    [Test]
    public void UIParticlesUberUnlitVertexDeformationChannelSelectionAffectsMotion()
    {
        var map = CreateSolidTexture(new Color(0f, 1f, 0f, 1f));

        var redChannel = RenderVertexDeformationStats(map, 0.35f, 0f, 0, destroyTexture: false);
        var greenChannel = RenderVertexDeformationStats(map, 0.35f, 0f, 1);

        Assert.That(greenChannel.MinX, Is.GreaterThan(redChannel.MinX + 8), $"red {redChannel}, green {greenChannel}");
    }

    [Test]
    public void UIParticlesUberUnlitVertexDeformationWithOffsetCoordinatesAffectsSampling()
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
    public void UIParticlesUberUnlitVertexDeformationPreservesColorAndAlphaFeatures()
    {
        var map = CreateSolidTexture(new Color(1f, 0f, 0f, 1f));
        var alphaMap = CreateSolidTexture(new Color(0.9f, 0.9f, 0.9f, 1f));
        try
        {
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
                    material.SetFloat("_AlphaTransitionMode", 1f);
                    material.SetFloat("_AlphaTransitionMapMode", 0f);
                    material.SetFloat("_AlphaTransitionProgress", 0.25f);
                    material.SetTexture("_AlphaTransitionMap", alphaMap);
                },
                meshColor: new Color(1f, 1f, 1f, 0.75f),
                baseColor: new Color(0.15f, 0.15f, 0.15f, 1f));

            Assert.That(stats.MinX, Is.GreaterThan(20), stats.ToString());
            Assert.That(stats.AverageColor.g, Is.GreaterThan(stats.AverageColor.r + 0.15f), stats.ToString());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(alphaMap);
        }
    }

    [Test]
    public void UIParticlesUberUnlitVertexDeformationRespectsRectClip()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.05f, 0.05f, 1f)));
                    material.SetFloat("_BaseMapMode", 0);
                    ConfigureVertexDeformationMaterial(material, CreateSolidTexture(new Color(1f, 0f, 0f, 1f)), 0.2f, 0f, 0);
                    material.EnableKeyword("UNITY_UI_CLIP_RECT");
                    material.SetVector("_ClipRect", new Vector4(-0.05f, -1f, 1f, 1f));
                }, CreateQuad(Color.white, normal: Vector3.right)),
            },
            new[] { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[1].r, Is.GreaterThan(samples[1].b + 0.2f), $"clipped {samples[0]}, visible {samples[1]}");
        Assert.That(samples[0].b, Is.GreaterThan(samples[0].r + 0.02f), $"clipped {samples[0]}, visible {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitVertexDeformationPreservesStencilOrMaskBehavior()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateStencilMask(new Rect(-0.6f, -0.5f, 0.7f, 1f)),
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.05f, 0.05f, 1f)));
                    material.SetFloat("_BaseMapMode", 0);
                    ConfigureVertexDeformationMaterial(material, CreateSolidTexture(new Color(1f, 0f, 0f, 1f)), 0.2f, 0f, 0);
                    material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                    material.SetFloat("_Stencil", 1);
                    material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                    material.SetFloat("_StencilReadMask", 255);
                    material.SetFloat("_StencilWriteMask", 255);
                }, CreateQuad(Color.white, normal: Vector3.right)),
            },
            new[] { new Vector2(0.35f, 0.5f), new Vector2(0.75f, 0.5f) },
            background);

        Assert.That(samples[0].r, Is.GreaterThan(samples[0].b + 0.2f), $"inside {samples[0]}, outside {samples[1]}");
        Assert.That(samples[1].b, Is.GreaterThan(samples[1].r + 0.02f), $"inside {samples[0]}, outside {samples[1]}");
    }

    [Test]
    public void UIParticlesUberUnlitVertexDeformationPreservesColorMask()
    {
        var background = new Color(0.03f, 0.04f, 0.08f, 1f);
        var samples = RenderUiSceneSamples(
            new[]
            {
                CreateUiDraw(material =>
                {
                    material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.05f, 0.05f, 1f)));
                    material.SetFloat("_BaseMapMode", 0);
                    ConfigureVertexDeformationMaterial(material, CreateSolidTexture(new Color(1f, 0f, 0f, 1f)), 0.35f, 0f, 0);
                    material.SetFloat("_ColorMask", 0);
                }, CreateQuad(Color.white, normal: Vector3.right)),
            },
            new[] { new Vector2(0.7f, 0.5f) },
            background);

        Assert.That(samples[0].b, Is.GreaterThan(samples[0].r + 0.02f), samples[0].ToString());
    }

    [Test]
    public void UIParticlesUberUnlitVertexDeformationPreservesDepthFeatureInteraction()
    {
        var map = CreateSolidTexture(new Color(1f, 0f, 0f, 1f));
        try
        {
            var withoutDepthFade = RenderDepthSceneStats(material =>
            {
                ConfigureVertexDeformationMaterial(material, map, 0.25f, 0f, 0);
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
            });
            var withDepthFade = RenderDepthSceneStats(material =>
            {
                ConfigureVertexDeformationMaterial(material, map, 0.25f, 0f, 0);
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetFloat("_DepthFadeEnabled", 1f);
                material.SetFloat("_DepthFadeNear", 1.9f);
                material.SetFloat("_DepthFadeFar", 3f);
                material.SetFloat("_DepthFadeWidth", 0.5f);
            });

            Assert.That(withoutDepthFade.MaxX, Is.GreaterThan(58), withoutDepthFade.ToString());
            Assert.That(withDepthFade.AverageColor.r, Is.LessThan(withoutDepthFade.AverageColor.r - 0.1f), $"without {withoutDepthFade}, with {withDepthFade}");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(map);
        }
    }

    private static RenderStats RenderVertexDeformationStats(
        Texture2D deformationMap,
        float intensity,
        float baseValue,
        int channel,
        Action<Material> configureMaterial = null,
        Vector4? customCoord1 = null,
        Color? meshColor = null,
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
                ConfigureVertexDeformationMaterial(material, deformationMap, intensity, baseValue, channel);
                configureMaterial?.Invoke(material);
            },
            CreateQuad(meshColor ?? Color.white, customCoord1, Vector3.right),
            ownedObjects);
    }

    private static RenderStats RenderDepthSceneStats(Action<Material> configureMaterial)
    {
        return RenderToStats(
            configureMaterial,
            CreateQuad(Color.white, normal: Vector3.right),
            Array.Empty<UnityEngine.Object>(),
            addOpaqueDepthPlane: true);
    }

    private static RenderStats RenderToStats(
        Action<Material> configureMaterial,
        Mesh mesh,
        IEnumerable<UnityEngine.Object> ownedObjects,
        bool addOpaqueDepthPlane = false)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUiVertexDeformationSmokeCamera");
        var meshObject = new GameObject("NovaBuiltInUiVertexDeformationSmokeMesh");
        var objectsToDestroy = ownedObjects.ToList();
        var previous = RenderTexture.active;

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

            GameObject opaqueObject = null;
            Material opaqueMaterial = null;
            Mesh opaqueMesh = null;
            if (addOpaqueDepthPlane)
            {
                opaqueObject = new GameObject("NovaBuiltInUiVertexDeformationOpaqueDepth");
                opaqueObject.transform.position = new Vector3(0f, 0f, 0.08f);
                opaqueMesh = CreateQuad(Color.white);
                var opaqueFilter = opaqueObject.AddComponent<MeshFilter>();
                opaqueFilter.sharedMesh = opaqueMesh;
                var opaqueRenderer = opaqueObject.AddComponent<MeshRenderer>();
                opaqueMaterial = CreateOpaqueMaterial(new Color(0f, 0f, 1f, 1f));
                opaqueRenderer.sharedMaterial = opaqueMaterial;
                objectsToDestroy.Add(opaqueObject);
                objectsToDestroy.Add(opaqueMesh);
                objectsToDestroy.Add(opaqueMaterial);
            }

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

    private static Color[] RenderUiSceneSamples(
        IReadOnlyList<UiDraw> draws,
        IReadOnlyList<Vector2> normalizedSamplePositions,
        Color background)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUiVertexDeformationSceneCamera");
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

                var gameObject = new GameObject("NovaBuiltInUiVertexDeformationSceneDraw");
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

    private static void ConfigureVertexDeformationMaterial(Material material, Texture2D deformationMap, float intensity, float baseValue, int channel)
    {
        material.SetFloat("_VertexDeformationIntensity", intensity);
        material.SetFloat("_VertexDeformationBaseValue", baseValue);
        material.SetFloat("_VertexDeformationMapChannel", channel);
        if (deformationMap != null)
        {
            material.SetTexture("_VertexDeformationMap", deformationMap);
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
        var mesh = new Mesh { name = "NovaBuiltInUiVertexDeformationQuad" };
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

    private static Texture2D CreateHorizontalSplitTexture(Color leftColor, Color rightColor)
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
                texture.SetPixel(x, y, x < TextureSize / 2 ? leftColor : rightColor);
            }
        }

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

    private readonly struct RenderStats
    {
        private RenderStats(int litPixels, int minX, int maxX, float centerX, Color averageColor)
        {
            LitPixels = litPixels;
            MinX = minX;
            MaxX = maxX;
            CenterX = centerX;
            AverageColor = averageColor;
        }

        public int LitPixels { get; }
        public int MinX { get; }
        public int MaxX { get; }
        public float CenterX { get; }
        public Color AverageColor { get; }

        public static RenderStats From(Texture2D texture)
        {
            var litPixels = 0;
            var minX = texture.width;
            var maxX = -1;
            var weightedX = 0f;
            var weight = 0f;
            var colorSum = Color.black;

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
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    weightedX += x * brightness;
                    weight += brightness;
                    colorSum += color;
                }
            }

            var averageColor = litPixels == 0 ? Color.black : colorSum / litPixels;
            var centerX = weight <= 0f ? 0f : weightedX / weight;
            return new RenderStats(litPixels, minX, maxX, centerX, averageColor);
        }

        public override string ToString()
        {
            return $"LitPixels={LitPixels}, MinX={MinX}, MaxX={MaxX}, CenterX={CenterX}, AverageColor={AverageColor}";
        }
    }
}

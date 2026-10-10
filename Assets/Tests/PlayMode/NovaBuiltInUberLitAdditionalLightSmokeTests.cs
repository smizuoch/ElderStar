using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.PlayMode
{
[TestFixture("Nova/Particles/UberLit")]
[TestFixture("Nova/UIParticles/UberLit")]
public sealed class NovaBuiltInUberLitAdditionalLightSmokeTests
{
    private readonly string LitShaderName;
    public NovaBuiltInUberLitAdditionalLightSmokeTests(string shaderName)
    {
        LitShaderName = shaderName;
    }
    private const int RenderSize = 144;
    private const int TextureSize = 8;

    [Test]
    public void UberLitAdditionalLightsAffectOutput()
    {
        var without = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), additionalLight: null);
        var with = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Point(Color.white));

        Assert.That(Luminance(with.Center), Is.GreaterThan(Luminance(without.Center) + 0.05f), $"with {with.Center}, without {without.Center}");
    }

    [Test]
    public void UberLitAdditionalLightsKeywordControlsContribution()
    {
        var enabled = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Point(Color.white));
        var disabled = RenderAdditional(
            material => ConfigureMetallic(material, 0f, 0.35f),
            AdditionalLightCase.Point(Color.white),
            configureAfterSetup: material => material.DisableKeyword("_ADDITIONAL_LIGHTS"));

        Assert.That(Luminance(enabled.Center), Is.GreaterThan(Luminance(disabled.Center) + 0.05f), $"enabled {enabled.Center}, disabled {disabled.Center}");
    }

    [Test]
    public void UberLitAdditionalPointLightColorAffectsOutput()
    {
        var red = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Point(new Color(1f, 0.05f, 0.05f, 1f)));
        var blue = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Point(new Color(0.05f, 0.05f, 1f, 1f)));

        Assert.That(red.Center.r, Is.GreaterThan(red.Center.b + 0.08f), red.Center.ToString());
        Assert.That(blue.Center.b, Is.GreaterThan(blue.Center.r + 0.08f), blue.Center.ToString());
    }

    [Test]
    public void UberLitAdditionalPointLightPositionAffectsOutput()
    {
        var front = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Point(Color.white, new Vector3(0f, 0f, -1.2f), 5f, 4.5f));
        var behind = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Point(Color.white, new Vector3(0f, 0f, 1.2f), 5f, 4.5f));

        Assert.That(Luminance(front.Center), Is.GreaterThan(Luminance(behind.Center) + 0.05f), $"front {front.Center}, behind {behind.Center}");
    }

    [Test]
    public void UberLitAdditionalPointLightRangeAffectsOutput()
    {
        var inRange = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Point(Color.white, new Vector3(0f, 0f, -1.2f), 5f, 4.5f));
        var outOfRange = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Point(Color.white, new Vector3(0f, 0f, -1.2f), 5f, 0.3f));

        Assert.That(Luminance(inRange.Center), Is.GreaterThan(Luminance(outOfRange.Center) + 0.05f), $"in range {inRange.Center}, out {outOfRange.Center}");
    }

    [Test]
    public void UberLitAdditionalSpotLightAffectsOutput()
    {
        var point = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), additionalLight: null);
        var spot = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Spot(Color.white, Quaternion.LookRotation(Vector3.forward)));

        Assert.That(Luminance(spot.Center), Is.GreaterThan(Luminance(point.Center) + 0.04f), $"spot {spot.Center}, none {point.Center}");
    }

    [Test]
    public void UberLitAdditionalSpotLightDirectionAffectsOutput()
    {
        var toward = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Spot(Color.white, Quaternion.LookRotation(Vector3.forward)));
        var away = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.35f), AdditionalLightCase.Spot(Color.white, Quaternion.LookRotation(Vector3.left)));

        Assert.That(Luminance(toward.Center), Is.GreaterThan(Luminance(away.Center) + 0.04f), $"toward {toward.Center}, away {away.Center}");
    }

    [Test]
    public void UberLitAdditionalLightsWorkWithMetallicWorkflow()
    {
        var low = RenderAdditional(material => ConfigureMetallic(material, 0f, 0.9f));
        var high = RenderAdditional(material => ConfigureMetallic(material, 1f, 0.9f));

        Assert.That(ColorDistance(high.Center, low.Center), Is.GreaterThan(0.04f), $"low {low.Center}, high {high.Center}");
    }

    [Test]
    public void UberLitAdditionalLightsWorkWithSpecularWorkflow()
    {
        var red = RenderAdditional(material => ConfigureSpecular(material, new Color(1f, 0.05f, 0.05f, 1f), 0.9f));
        var blue = RenderAdditional(material => ConfigureSpecular(material, new Color(0.05f, 0.05f, 1f, 1f), 0.9f));

        Assert.That(red.Center.r, Is.GreaterThan(red.Center.b + 0.04f), red.Center.ToString());
        Assert.That(blue.Center.b, Is.GreaterThan(blue.Center.r + 0.04f), blue.Center.ToString());
    }

    [Test]
    public void UberLitAdditionalLightsWorkWithSmoothness()
    {
        var low = RenderAdditional(material => ConfigureSpecular(material, Color.white, 0.15f));
        var high = RenderAdditional(material => ConfigureSpecular(material, Color.white, 0.95f));

        Assert.That(ColorDistance(high.Center, low.Center), Is.GreaterThan(0.03f), $"low {low.Center}, high {high.Center}");
    }

    [Test]
    public void UberLitAdditionalLightsWorkWithNormalMap()
    {
        var flat = RenderAdditional(material =>
        {
            ConfigureMetallic(material, 0f, 0.35f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(0.5f, 0.5f, 1f, 1f)));
        }, AdditionalLightCase.Point(Color.white, new Vector3(1.2f, 0f, -1.2f), 5f, 4.5f));
        var tilted = RenderAdditional(material =>
        {
            ConfigureMetallic(material, 0f, 0.35f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
        }, AdditionalLightCase.Point(Color.white, new Vector3(1.2f, 0f, -1.2f), 5f, 4.5f));

        Assert.That(ColorDistance(tilted.Center, flat.Center), Is.GreaterThan(0.04f), $"flat {flat.Center}, tilted {tilted.Center}");
    }

    [Test]
    public void UberLitAdditionalLightsCoexistWithEnvironmentReflections()
    {
        var reflectionOnly = RenderAdditional(material => ConfigureReflections(material), additionalLight: null, ambient: new Color(0.02f, 0.02f, 0.02f, 1f));
        var both = RenderAdditional(material => ConfigureReflections(material), AdditionalLightCase.Point(Color.white), ambient: new Color(0.02f, 0.02f, 0.02f, 1f));

        Assert.That(Luminance(reflectionOnly.Center), Is.GreaterThan(0.02f), reflectionOnly.Center.ToString());
        Assert.That(Luminance(both.Center), Is.GreaterThan(Luminance(reflectionOnly.Center) + 0.04f), $"both {both.Center}, reflection {reflectionOnly.Center}");
    }

    [Test]
    public void UberLitForwardAddDoesNotAddAmbientOrReflectionTwice()
    {
        var ambientReflection = RenderAdditional(
            material => ConfigureReflections(material),
            additionalLight: null,
            ambient: new Color(0.30f, 0.30f, 0.30f, 1f));
        var withAdditional = RenderAdditional(
            material => ConfigureReflections(material),
            AdditionalLightCase.Point(new Color(1f, 0.05f, 0.05f, 1f), new Vector3(0f, 0f, -1.2f), 2.4f, 4.5f),
            ambient: new Color(0.30f, 0.30f, 0.30f, 1f));

        var delta = withAdditional.Center - ambientReflection.Center;
        Assert.That(Luminance(delta), Is.GreaterThan(0.02f), $"with {withAdditional.Center}, base {ambientReflection.Center}, delta {delta}");
        Assert.That(delta.r, Is.GreaterThan(delta.b + 0.05f), $"with {withAdditional.Center}, base {ambientReflection.Center}, delta {delta}");
    }

    [Test]
    public void UberLitAdditionalLightsPreserveAlphaTest()
    {
        var background = new Color(0.03f, 0.07f, 0.75f, 1f);
        var sample = RenderAdditional(
            material =>
            {
                ConfigureMetallic(material, 0f, 0.35f);
                material.SetFloat("_RenderType", 1f);
                material.SetFloat("_Cutoff", 0.5f);
                material.SetTexture("_BaseMap", CreateSplitTexture(new Color(1f, 1f, 1f, 0.15f), Color.white));
            },
            AdditionalLightCase.Point(Color.white),
            background: background);

        Assert.That(sample.Left.b, Is.GreaterThan(sample.Left.r + 0.20f), sample.Left.ToString());
        Assert.That(Luminance(sample.Right), Is.GreaterThan(Luminance(sample.Left) + 0.05f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitAdditionalLightsPreserveTintAndVertexColor()
    {
        var sample = RenderAdditional(
            material =>
            {
                ConfigureMetallic(material, 0f, 0.35f);
                material.SetFloat("_TintAreaMode", 1f);
                material.SetFloat("_TintColorMode", 0f);
                material.SetColor("_TintColor", new Color(0.1f, 1f, 0.1f, 1f));
            },
            AdditionalLightCase.Point(Color.white),
            mesh: CreateQuad(new Color(1f, 0.55f, 1f, 1f)));

        Assert.That(sample.Center.g, Is.GreaterThan(sample.Center.r + 0.08f), sample.Center.ToString());
        Assert.That(sample.Center.g, Is.GreaterThan(sample.Center.b + 0.03f), sample.Center.ToString());
    }

    private static void ConfigureMetallic(Material material, float metallic, float smoothness)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.30f, 0.30f, 0.30f, 1f)));
        material.SetFloat("_LitWorkflowMode", 1f);
        material.SetFloat("_SpecularHighlights", 1f);
        material.SetFloat("_EnvironmentReflections", 0f);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
    }

    private static void ConfigureSpecular(Material material, Color specularColor, float smoothness)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.30f, 0.30f, 0.30f, 1f)));
        material.SetFloat("_LitWorkflowMode", 0f);
        material.SetFloat("_SpecularHighlights", 1f);
        material.SetFloat("_EnvironmentReflections", 0f);
        material.SetColor("_SpecularColor", specularColor);
        material.SetFloat("_Smoothness", smoothness);
    }

    private static void ConfigureReflections(Material material)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.24f, 0.24f, 0.24f, 1f)));
        material.SetFloat("_LitWorkflowMode", 1f);
        material.SetFloat("_SpecularHighlights", 0f);
        material.SetFloat("_EnvironmentReflections", 1f);
        // Keep diffuse and reflection energy so both lighting paths are observable.
        material.SetFloat("_Metallic", 0.5f);
        material.SetFloat("_Smoothness", 0.9f);
    }

    private RenderSample RenderAdditional(
        Action<Material> configureMaterial,
        AdditionalLightCase? additionalLight = null,
        Action<Material> configureAfterSetup = null,
        Color? ambient = null,
        Color? background = null,
        Mesh mesh = null)
    {
        var shader = Shader.Find(LitShaderName);
        Assert.That(shader, Is.Not.Null, LitShaderName);

        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        var previousAmbientIntensity = RenderSettings.ambientIntensity;
        var previousDefaultReflectionMode = RenderSettings.defaultReflectionMode;
        var previousCustomReflection = previousDefaultReflectionMode == DefaultReflectionMode.Custom ? RenderSettings.customReflection : null;
        var previousReflectionIntensity = RenderSettings.reflectionIntensity;
        var previousPixelLightCount = QualitySettings.pixelLightCount;
        var previousActive = RenderTexture.active;

        var material = new Material(shader);

        NovaLitTestTextures.UseDiffuseDefaults(material);
        var actualMesh = mesh ?? CreateQuad(Color.white);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitAdditionalCamera");
        var meshObject = new GameObject("NovaBuiltInUberLitAdditionalMesh");
        var mainLightObject = new GameObject("NovaBuiltInUberLitAdditionalMainLight");
        var additionalLightObject = additionalLight.HasValue ? new GameObject("NovaBuiltInUberLitAdditionalLight") : null;
        var cubemap = CreateReflectionCubemap();

        try
        {
            QualitySettings.pixelLightCount = 8;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient ?? Color.black;
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflection = cubemap;
            RenderSettings.reflectionIntensity = 1f;

            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            material.SetFloat("_RenderType", 0f);
            configureMaterial(material);
            SetupUberLit(material);
            material.EnableKeyword("_ADDITIONAL_LIGHTS");
            configureAfterSetup?.Invoke(material);

            meshObject.AddComponent<MeshFilter>().sharedMesh = actualMesh;
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            var mainLight = mainLightObject.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.color = Color.white;
            mainLight.intensity = 0.18f;
            mainLight.renderMode = LightRenderMode.ForcePixel;
            mainLightObject.transform.rotation = Quaternion.LookRotation(Vector3.forward);

            if (additionalLight.HasValue)
            {
                additionalLight.Value.Apply(additionalLightObject);
            }

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

            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, RenderSize, RenderSize), 0, 0);
            readback.Apply();
            RenderTexture.active = previousActive;

            return new RenderSample(
                AverageRegion(readback, 0.40f, 0.60f, 0.40f, 0.60f),
                AverageRegion(readback, 0.20f, 0.40f, 0.40f, 0.60f),
                AverageRegion(readback, 0.60f, 0.80f, 0.40f, 0.60f));
        }
        finally
        {
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            RenderSettings.ambientIntensity = previousAmbientIntensity;
            RenderSettings.defaultReflectionMode = previousDefaultReflectionMode;
            RenderSettings.customReflection = previousCustomReflection;
            RenderSettings.reflectionIntensity = previousReflectionIntensity;
            QualitySettings.pixelLightCount = previousPixelLightCount;
            RenderTexture.active = previousActive;

            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(meshObject);
            UnityEngine.Object.DestroyImmediate(mainLightObject);
            if (additionalLightObject != null)
            {
                UnityEngine.Object.DestroyImmediate(additionalLightObject);
            }

            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(actualMesh);
            UnityEngine.Object.DestroyImmediate(cubemap);
            DestroyMaterialAndOwnedTextures(material);
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

    private static Texture2D CreateSplitTexture(Color left, Color right)
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
                texture.SetPixel(x, y, x < TextureSize / 2 ? left : right);
            }
        }

        texture.Apply();
        return texture;
    }

    private static Cubemap CreateReflectionCubemap()
    {
        var cubemap = new Cubemap(16, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };
        foreach (CubemapFace face in Enum.GetValues(typeof(CubemapFace)))
        {
            if (face == CubemapFace.Unknown)
            {
                continue;
            }

            cubemap.SetPixels(Enumerable.Repeat(new Color(0.35f, 0.42f, 0.55f, 1f), 16 * 16).ToArray(), face);
        }

        cubemap.Apply();
        return cubemap;
    }

    private static Mesh CreateQuad(Color vertexColor)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberLitAdditionalQuad" };
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

    private static void DestroyMaterialAndOwnedTextures(Material material)
    {
        foreach (var property in new[] { "_BaseMap", "_NormalMap" })
        {
            if (!material.HasProperty(property))
            {
                continue;
            }

            var texture = material.GetTexture(property);
            if (texture != null)
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        UnityEngine.Object.DestroyImmediate(material);
    }

    private readonly struct RenderSample
    {
        public RenderSample(Color center, Color left, Color right)
        {
            Center = center;
            Left = left;
            Right = right;
        }

        public Color Center { get; }
        public Color Left { get; }
        public Color Right { get; }
    }

    private readonly struct AdditionalLightCase
    {
        private AdditionalLightCase(LightType type, Color color, Vector3 position, Quaternion rotation, float intensity, float range, float spotAngle)
        {
            Type = type;
            Color = color;
            Position = position;
            Rotation = rotation;
            Intensity = intensity;
            Range = range;
            SpotAngle = spotAngle;
        }

        private LightType Type { get; }
        private Color Color { get; }
        private Vector3 Position { get; }
        private Quaternion Rotation { get; }
        private float Intensity { get; }
        private float Range { get; }
        private float SpotAngle { get; }

        public static AdditionalLightCase Point(Color color)
        {
            return Point(color, new Vector3(0f, 0f, -1.2f), 5f, 4.5f);
        }

        public static AdditionalLightCase Point(Color color, Vector3 position, float intensity, float range)
        {
            return new AdditionalLightCase(LightType.Point, color, position, Quaternion.identity, intensity, range, 30f);
        }

        public static AdditionalLightCase Spot(Color color, Quaternion rotation)
        {
            return new AdditionalLightCase(LightType.Spot, color, new Vector3(0f, 0f, -1.2f), rotation, 7f, 5f, 38f);
        }

        public void Apply(GameObject lightObject)
        {
            lightObject.transform.position = Position;
            lightObject.transform.rotation = Rotation;
            var light = lightObject.AddComponent<Light>();
            light.type = Type;
            light.color = Color;
            light.intensity = Intensity;
            light.range = Range;
            light.spotAngle = SpotAngle;
            light.renderMode = LightRenderMode.ForcePixel;
            light.shadows = LightShadows.None;
        }
    }
}
}

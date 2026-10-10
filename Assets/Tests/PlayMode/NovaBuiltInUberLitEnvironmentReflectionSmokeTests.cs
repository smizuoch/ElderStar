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
public sealed class NovaBuiltInUberLitEnvironmentReflectionSmokeTests
{
    private readonly string LitShaderName;

    public NovaBuiltInUberLitEnvironmentReflectionSmokeTests(string shaderName)
    {
        LitShaderName = shaderName;
    }
    private const int RenderSize = 128;
    private const int TextureSize = 8;

    [Test]
    public void UberLitEnvironmentReflectionsAffectOutput()
    {
        var disabled = RenderEnvironment(material => ConfigureReflections(material, enabled: false));
        var enabled = RenderEnvironment(material => ConfigureReflections(material, enabled: true));

        Assert.That(Luminance(enabled.Center), Is.GreaterThan(Luminance(disabled.Center) + 0.05f), $"enabled {enabled.Center}, disabled {disabled.Center}");
        Assert.That(ColorDistance(enabled.Center, disabled.Center), Is.GreaterThan(0.08f), $"enabled {enabled.Center}, disabled {disabled.Center}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsCanBeDisabled()
    {
        var enabled = RenderEnvironment(material => ConfigureReflections(material, enabled: true));
        var disabled = RenderEnvironment(material => ConfigureReflections(material, enabled: false));

        Assert.That(Luminance(disabled.Center), Is.LessThan(Luminance(enabled.Center) * 0.55f), $"enabled {enabled.Center}, disabled {disabled.Center}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsKeywordControlsContribution()
    {
        var enabled = RenderEnvironment(material => ConfigureReflections(material, enabled: true));
        var keywordDisabled = RenderEnvironment(
            material => ConfigureReflections(material, enabled: true),
            material => material.DisableKeyword("_ENVIRONMENT_REFLECTIONS_ENABLED"));

        Assert.That(Luminance(keywordDisabled.Center), Is.LessThan(Luminance(enabled.Center) * 0.55f), $"enabled {enabled.Center}, keyword disabled {keywordDisabled.Center}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsRespondToSmoothness()
    {
        var low = RenderEnvironment(material => ConfigureReflections(material, smoothness: 0.05f));
        var high = RenderEnvironment(material => ConfigureReflections(material, smoothness: 1f));

        Assert.That(Luminance(high.Center), Is.GreaterThan(Luminance(low.Center) + 0.04f), $"low {low.Center}, high {high.Center}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsRespondToSmoothnessMap()
    {
        var sample = RenderEnvironment(material =>
        {
            ConfigureReflections(material, smoothness: 1f);
            material.SetTexture("_SmoothnessMap", CreateSplitTexture(new Color(0.05f, 0.05f, 0.05f, 0.05f), Color.white));
            material.SetFloat("_SmoothnessMapChannelsX", 0f);
        });

        Assert.That(ColorDistance(sample.Right, sample.Left), Is.GreaterThan(0.08f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsRespondToMetallic()
    {
        var low = RenderEnvironment(material => ConfigureReflections(material, metallic: 0f));
        var high = RenderEnvironment(material => ConfigureReflections(material, metallic: 1f));

        Assert.That(Luminance(high.Center), Is.GreaterThan(Luminance(low.Center) + 0.04f), $"low {low.Center}, high {high.Center}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsRespondToMetallicMap()
    {
        var sample = RenderEnvironment(material =>
        {
            ConfigureReflections(material, metallic: 1f);
            material.SetTexture("_MetallicMap", CreateSplitTexture(Color.black, Color.white));
            material.SetFloat("_MetallicMapChannelsX", 0f);
        });

        Assert.That(ColorDistance(sample.Right, sample.Left), Is.GreaterThan(0.08f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsUseSpecularColorInSpecularWorkflow()
    {
        var red = RenderEnvironment(material => ConfigureSpecularReflections(material, new Color(1f, 0.04f, 0.04f, 1f)));
        var blue = RenderEnvironment(material => ConfigureSpecularReflections(material, new Color(0.04f, 0.04f, 1f, 1f)));

        Assert.That(red.Center.r, Is.GreaterThan(red.Center.b + 0.06f), red.Center.ToString());
        Assert.That(blue.Center.b, Is.GreaterThan(blue.Center.r + 0.06f), blue.Center.ToString());
    }

    [Test]
    public void UberLitEnvironmentReflectionsRespondToSpecularMap()
    {
        var sample = RenderEnvironment(material =>
        {
            ConfigureSpecularReflections(material, Color.white);
            material.SetTexture("_SpecularMap", CreateSplitTexture(Color.black, Color.white));
        });

        Assert.That(Luminance(sample.Right), Is.GreaterThan(Luminance(sample.Left) + 0.04f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsRespondToNormalMap()
    {
        var flat = RenderEnvironment(material =>
        {
            ConfigureReflections(material, smoothness: 1f, metallic: 1f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(0.5f, 0.5f, 1f, 1f)));
        });
        var tilted = RenderEnvironment(material =>
        {
            ConfigureReflections(material, smoothness: 1f, metallic: 1f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
        });

        Assert.That(ColorDistance(tilted.Center, flat.Center), Is.GreaterThan(0.05f), $"flat {flat.Center}, tilted {tilted.Center}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsRespondToNormalBumpScale()
    {
        var low = RenderEnvironment(material =>
        {
            ConfigureReflections(material, smoothness: 1f, metallic: 1f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
            material.SetFloat("_NormalMapBumpScale", 0.1f);
        });
        var high = RenderEnvironment(material =>
        {
            ConfigureReflections(material, smoothness: 1f, metallic: 1f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
            material.SetFloat("_NormalMapBumpScale", 1f);
        });

        Assert.That(ColorDistance(high.Center, low.Center), Is.GreaterThan(0.04f), $"low {low.Center}, high {high.Center}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsCoexistWithReceiveShadows()
    {
        var sample = RenderReceiveShadowEnvironment(material =>
        {
            ConfigureReflections(material, smoothness: 0.9f, metallic: 0.8f);
            material.SetFloat("_LitReceiveShadows", 1f);
        });

        Assert.That(ColorDistance(sample.Lit, sample.Shadow), Is.GreaterThan(0.08f), sample.ToString());
        Assert.That(Mathf.Min(Luminance(sample.Lit), Luminance(sample.Shadow)), Is.GreaterThan(0.03f), sample.ToString());
    }

    [Test]
    public void UberLitEnvironmentReflectionsPreserveAmbientApproximation()
    {
        var ambientOnly = RenderEnvironment(
            material => ConfigureReflections(material, enabled: false),
            ambient: new Color(0.18f, 0.18f, 0.18f, 1f));
        var ambientAndReflection = RenderEnvironment(
            material => ConfigureReflections(material, enabled: true),
            ambient: new Color(0.18f, 0.18f, 0.18f, 1f));

        Assert.That(Luminance(ambientOnly.Center), Is.GreaterThan(0.02f), ambientOnly.Center.ToString());
        Assert.That(Luminance(ambientAndReflection.Center), Is.GreaterThan(Luminance(ambientOnly.Center) + 0.04f), $"ambient {ambientOnly.Center}, reflected {ambientAndReflection.Center}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsPreserveAlphaTest()
    {
        var background = new Color(0.02f, 0.06f, 0.75f, 1f);
        var sample = RenderEnvironment(
            material =>
            {
                ConfigureReflections(material, smoothness: 1f, metallic: 1f);
                material.SetFloat("_RenderType", 1f);
                material.SetFloat("_Cutoff", 0.5f);
                material.SetTexture("_BaseMap", CreateSplitTexture(new Color(1f, 1f, 1f, 0.15f), Color.white));
            },
            background: background);

        Assert.That(sample.Left.b, Is.GreaterThan(sample.Left.r + 0.25f), sample.Left.ToString());
        Assert.That(Luminance(sample.Right), Is.GreaterThan(Luminance(sample.Left) + 0.05f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitEnvironmentReflectionsPreserveTintAndVertexColor()
    {
        var tinted = RenderEnvironment(
            material =>
            {
                ConfigureReflections(material, smoothness: 0.65f, metallic: 0.25f);
                material.SetFloat("_TintAreaMode", 1f);
                material.SetFloat("_TintColorMode", 0f);
                material.SetColor("_TintColor", new Color(0.1f, 1f, 0.1f, 1f));
            },
            ambient: new Color(0.18f, 0.18f, 0.18f, 1f),
            lightIntensity: 0.8f,
            mesh: CreateQuad(new Color(1f, 0.55f, 1f, 1f)));
        var reflectionOnly = RenderEnvironment(material => ConfigureReflections(material, smoothness: 0.65f, metallic: 0.25f));

        Assert.That(tinted.Center.g, Is.GreaterThan(tinted.Center.r + 0.04f), tinted.Center.ToString());
        Assert.That(Luminance(tinted.Center), Is.GreaterThan(Luminance(reflectionOnly.Center) + 0.02f), $"tinted {tinted.Center}, reflection {reflectionOnly.Center}");
    }

    private static void ConfigureReflections(Material material, bool enabled = true, float smoothness = 0.9f, float metallic = 1f)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.22f, 0.22f, 0.22f, 1f)));
        material.SetFloat("_LitWorkflowMode", 1f);
        material.SetFloat("_EnvironmentReflections", enabled ? 1f : 0f);
        material.SetFloat("_SpecularHighlights", 0f);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
    }

    private static void ConfigureSpecularReflections(Material material, Color specularColor)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.22f, 0.22f, 0.22f, 1f)));
        material.SetFloat("_LitWorkflowMode", 0f);
        material.SetFloat("_EnvironmentReflections", 1f);
        material.SetFloat("_SpecularHighlights", 0f);
        material.SetColor("_SpecularColor", specularColor);
        material.SetFloat("_Smoothness", 0.9f);
    }

    private RenderSample RenderEnvironment(
        Action<Material> configureMaterial,
        Action<Material> configureAfterSetup = null,
        Color? ambient = null,
        Color? background = null,
        float lightIntensity = 0f,
        Mesh mesh = null)
    {
        var shader = Shader.Find(LitShaderName);
        Assert.That(shader, Is.Not.Null, LitShaderName);

        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        var previousAmbientIntensity = RenderSettings.ambientIntensity;
        var previousDefaultReflectionMode = RenderSettings.defaultReflectionMode;
        var previousCustomReflection = previousDefaultReflectionMode == DefaultReflectionMode.Custom
            ? RenderSettings.customReflection
            : null;
        var previousReflectionIntensity = RenderSettings.reflectionIntensity;
        var previousActive = RenderTexture.active;

        var material = new Material(shader);
        var actualMesh = mesh ?? CreateQuad(Color.white);
        var cubemap = CreateReflectionCubemap();
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitReflectionCamera");
        var meshObject = new GameObject("NovaBuiltInUberLitReflectionMesh");
        var lightObject = new GameObject("NovaBuiltInUberLitReflectionLight");

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient ?? Color.black;
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflection = cubemap;
            RenderSettings.reflectionIntensity = 1f;
            DynamicGI.UpdateEnvironment();

            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            material.SetFloat("_RenderType", 0f);
            configureMaterial(material);
            SetupUberLit(material);
            configureAfterSetup?.Invoke(material);

            meshObject.AddComponent<MeshFilter>().sharedMesh = actualMesh;
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = lightIntensity;
            lightObject.transform.rotation = Quaternion.LookRotation(Vector3.forward);

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
            if (previousDefaultReflectionMode == DefaultReflectionMode.Custom)
            {
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                RenderSettings.customReflection = previousCustomReflection;
            }
            else
            {
                RenderSettings.defaultReflectionMode = previousDefaultReflectionMode;
            }
            RenderSettings.reflectionIntensity = previousReflectionIntensity;
            DynamicGI.UpdateEnvironment();
            RenderTexture.active = previousActive;

            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(meshObject);
            UnityEngine.Object.DestroyImmediate(lightObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(actualMesh);
            UnityEngine.Object.DestroyImmediate(cubemap);
            DestroyMaterialAndOwnedTextures(material);
        }
    }

    private ShadowSample RenderReceiveShadowEnvironment(Action<Material> configureMaterial)
    {
        var shader = Shader.Find(LitShaderName);
        Assert.That(shader, Is.Not.Null, LitShaderName);

        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        var previousAmbientIntensity = RenderSettings.ambientIntensity;
        var previousDefaultReflectionMode = RenderSettings.defaultReflectionMode;
        var previousCustomReflection = previousDefaultReflectionMode == DefaultReflectionMode.Custom
            ? RenderSettings.customReflection
            : null;
        var previousReflectionIntensity = RenderSettings.reflectionIntensity;
        var previousShadows = QualitySettings.shadows;
        var previousShadowDistance = QualitySettings.shadowDistance;
        var previousShadowCascades = QualitySettings.shadowCascades;
        var previousShadowProjection = QualitySettings.shadowProjection;
        var previousShadowResolution = QualitySettings.shadowResolution;
        var previousActive = RenderTexture.active;

        var material = new Material(shader);
        var casterMaterial = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
        var receiverMesh = CreateReceiverPlane(Color.white);
        var cubemap = CreateReflectionCubemap();
        var target = new RenderTexture(160, 160, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(160, 160, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitReflectionShadowCamera");
        var receiverObject = new GameObject("NovaBuiltInUberLitReflectionShadowReceiver");
        var depthProxyObject = new GameObject("NovaBuiltInUberLitReflectionShadowDepthProxy");
        var casterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var lightObject = new GameObject("NovaBuiltInUberLitReflectionShadowLight");

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.03f, 0.03f, 0.03f, 1f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflection = cubemap;
            RenderSettings.reflectionIntensity = 1f;
            DynamicGI.UpdateEnvironment();
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 40f;
            QualitySettings.shadowCascades = 0;
            QualitySettings.shadowProjection = ShadowProjection.StableFit;
            QualitySettings.shadowResolution = ShadowResolution.High;

            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            material.SetFloat("_RenderType", 0f);
            configureMaterial(material);
            SetupUberLit(material);

            receiverObject.AddComponent<MeshFilter>().sharedMesh = receiverMesh;
            var receiverRenderer = receiverObject.AddComponent<MeshRenderer>();
            receiverRenderer.sharedMaterial = material;
            receiverRenderer.receiveShadows = true;
            receiverRenderer.shadowCastingMode = ShadowCastingMode.Off;

            depthProxyObject.AddComponent<MeshFilter>().sharedMesh = receiverMesh;
            depthProxyObject.transform.position = new Vector3(0f, -0.01f, 0f);
            var depthProxyRenderer = depthProxyObject.AddComponent<MeshRenderer>();
            depthProxyRenderer.sharedMaterial = casterMaterial;
            depthProxyRenderer.shadowCastingMode = ShadowCastingMode.Off;
            depthProxyRenderer.receiveShadows = false;

            casterObject.transform.position = new Vector3(0.4f, 0.55f, 0f);
            casterObject.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            var casterRenderer = casterObject.GetComponent<MeshRenderer>();
            casterRenderer.sharedMaterial = casterMaterial;
            casterRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            casterRenderer.receiveShadows = false;

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1.65f;
            light.shadows = LightShadows.Hard;
            light.shadowStrength = 0.9f;
            light.shadowBias = 0.005f;
            light.shadowNormalBias = 0.05f;
            lightObject.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.5f;
            camera.transform.position = new Vector3(0f, 6f, 0f);
            camera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.01f, 0.01f, 0.01f, 1f);
            camera.depthTextureMode = DepthTextureMode.Depth;
            camera.targetTexture = target;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.Render();

            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, 160, 160), 0, 0);
            readback.Apply();
            RenderTexture.active = previousActive;

            return new ShadowSample(
                AverageRegion(readback, 0.55f, 0.63f, 0.46f, 0.54f),
                AverageRegion(readback, 0.18f, 0.28f, 0.46f, 0.54f));
        }
        finally
        {
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            RenderSettings.ambientIntensity = previousAmbientIntensity;
            if (previousDefaultReflectionMode == DefaultReflectionMode.Custom)
            {
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                RenderSettings.customReflection = previousCustomReflection;
            }
            else
            {
                RenderSettings.defaultReflectionMode = previousDefaultReflectionMode;
            }
            RenderSettings.reflectionIntensity = previousReflectionIntensity;
            DynamicGI.UpdateEnvironment();
            QualitySettings.shadows = previousShadows;
            QualitySettings.shadowDistance = previousShadowDistance;
            QualitySettings.shadowCascades = previousShadowCascades;
            QualitySettings.shadowProjection = previousShadowProjection;
            QualitySettings.shadowResolution = previousShadowResolution;
            RenderTexture.active = previousActive;

            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(receiverObject);
            UnityEngine.Object.DestroyImmediate(depthProxyObject);
            UnityEngine.Object.DestroyImmediate(casterObject);
            UnityEngine.Object.DestroyImmediate(lightObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(receiverMesh);
            UnityEngine.Object.DestroyImmediate(cubemap);
            DestroyMaterialAndOwnedTextures(material);
            UnityEngine.Object.DestroyImmediate(casterMaterial);
        }
    }

    private static Cubemap CreateReflectionCubemap()
    {
        var cubemap = new Cubemap(16, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Trilinear,
            wrapMode = TextureWrapMode.Clamp,
        };

        SetFace(cubemap, CubemapFace.PositiveX, new Color(1f, 0.1f, 0.1f, 1f));
        SetFace(cubemap, CubemapFace.NegativeX, new Color(0.1f, 1f, 0.1f, 1f));
        SetFace(cubemap, CubemapFace.PositiveY, new Color(0.1f, 0.1f, 1f, 1f));
        SetFace(cubemap, CubemapFace.NegativeY, new Color(1f, 1f, 0.1f, 1f));
        SetFace(cubemap, CubemapFace.PositiveZ, new Color(0.1f, 1f, 1f, 1f));
        SetFace(cubemap, CubemapFace.NegativeZ, new Color(1f, 0.1f, 1f, 1f));
        cubemap.Apply();
        return cubemap;
    }

    private static void SetFace(Cubemap cubemap, CubemapFace face, Color color)
    {
        cubemap.SetPixels(Enumerable.Repeat(color, 16 * 16).ToArray(), face);
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

    private static Mesh CreateQuad(Color vertexColor)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberLitReflectionQuad" };
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

    private static Mesh CreateReceiverPlane(Color vertexColor)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberLitReflectionShadowReceiverPlane" };
        mesh.vertices = new[]
        {
            new Vector3(-2.25f, 0f, -2.25f),
            new Vector3(2.25f, 0f, -2.25f),
            new Vector3(2.25f, 0f, 2.25f),
            new Vector3(-2.25f, 0f, 2.25f),
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
        };
        mesh.colors = Enumerable.Repeat(vertexColor, 4).ToArray();
        mesh.normals = Enumerable.Repeat(Vector3.up, 4).ToArray();
        mesh.tangents = Enumerable.Repeat(new Vector4(1f, 0f, 0f, 1f), 4).ToArray();
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateBounds();
        return mesh;
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
        foreach (var property in new[]
                 {
                     "_BaseMap",
                     "_NormalMap",
                     "_SpecularMap",
                     "_MetallicMap",
                     "_SmoothnessMap",
                 })
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

    private readonly struct ShadowSample
    {
        public ShadowSample(Color lit, Color shadow)
        {
            Lit = lit;
            Shadow = shadow;
        }

        public Color Lit { get; }
        public Color Shadow { get; }

        public override string ToString()
        {
            return $"lit {Lit}, shadow {Shadow}";
        }
    }
}
}

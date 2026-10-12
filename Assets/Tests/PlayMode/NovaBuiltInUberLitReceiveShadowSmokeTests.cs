using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberLitReceiveShadowSmokeTests
{
    private const string LitShaderName = "Nova/Particles/UberLit";
    private const int RenderSize = 160;
    private const int TextureSize = 8;

    [Test]
    public void UberLitReceiveShadowsDarkensShadowedRegion()
    {
        var sample = RenderReceiveShadowScene(material => ConfigureReceiveShadows(material, true));

        AssertReceivesShadow(sample);
    }

    [Test]
    public void UberLitReceiveShadowsCanBeDisabled()
    {
        var enabled = RenderReceiveShadowScene(material => ConfigureReceiveShadows(material, true));
        var disabled = RenderReceiveShadowScene(material => ConfigureReceiveShadows(material, false));

        AssertReceivesShadow(enabled);
        Assert.That(
            ShadowDelta(disabled),
            Is.LessThan(ShadowDelta(enabled) * 0.45f),
            $"enabled {enabled}, disabled {disabled}");
        Assert.That(
            Luminance(disabled.Shadow),
            Is.GreaterThan(Luminance(enabled.Shadow) + 0.06f),
            $"enabled {enabled}, disabled {disabled}");
    }

    [Test]
    public void UberLitReceiveShadowsKeywordControlsAttenuation()
    {
        var enabled = RenderReceiveShadowScene(material => ConfigureReceiveShadows(material, true));
        var keywordDisabled = RenderReceiveShadowScene(
            material => ConfigureReceiveShadows(material, true),
            configureAfterSetup: material => material.DisableKeyword("_RECEIVE_SHADOWS_ENABLED"));

        AssertReceivesShadow(enabled);
        Assert.That(
            ShadowDelta(keywordDisabled),
            Is.LessThan(ShadowDelta(enabled) * 0.45f),
            $"enabled {enabled}, keyword disabled {keywordDisabled}");
    }

    [Test]
    public void UberLitReceiveShadowsWorksWithMetallicWorkflow()
    {
        var sample = RenderReceiveShadowScene(material =>
        {
            ConfigureReceiveShadows(material, true);
            material.SetFloat("_LitWorkflowMode", 1f);
            material.SetFloat("_SpecularHighlights", 1f);
            material.SetFloat("_Metallic", 0.8f);
            material.SetFloat("_Smoothness", 0.75f);
        });

        AssertReceivesShadow(sample);
    }

    [Test]
    public void UberLitReceiveShadowsWorksWithSpecularWorkflow()
    {
        var sample = RenderReceiveShadowScene(material =>
        {
            ConfigureReceiveShadows(material, true);
            material.SetFloat("_LitWorkflowMode", 0f);
            material.SetFloat("_SpecularHighlights", 1f);
            material.SetColor("_SpecularColor", new Color(0.9f, 0.9f, 1f, 1f));
            material.SetFloat("_Smoothness", 0.75f);
        });

        AssertReceivesShadow(sample);
    }

    [Test]
    public void UberLitReceiveShadowsWorksWithNormalMap()
    {
        var sample = RenderReceiveShadowScene(material =>
        {
            ConfigureReceiveShadows(material, true);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(0.5f, 0.5f, 1f, 1f)));
            material.SetFloat("_NormalMapBumpScale", 0.8f);
        });

        AssertReceivesShadow(sample);
    }

    [Test]
    public void UberLitReceiveShadowsWorksWithAlphaTest()
    {
        var background = new Color(0.02f, 0.05f, 0.7f, 1f);
        var sample = RenderReceiveShadowScene(
            material =>
            {
                ConfigureReceiveShadows(material, true);
                material.SetFloat("_RenderType", 1f);
                material.SetFloat("_Cutoff", 0.5f);
                material.SetTexture(
                    "_BaseMap",
                    CreateSplitTexture(
                        new Color(1f, 1f, 1f, 0.2f),
                        new Color(1f, 1f, 1f, 1f)));
            },
            background: background);

        Assert.That(Luminance(sample.RightLit), Is.GreaterThan(Luminance(sample.Shadow) + 0.08f), sample.ToString());
        Assert.That(sample.Clipped.b, Is.GreaterThan(sample.Clipped.r + 0.25f), sample.ToString());
    }

    [Test]
    public void UberLitReceiveShadowsPreserveTintAndVertexColor()
    {
        var sample = RenderReceiveShadowScene(
            material =>
            {
                ConfigureReceiveShadows(material, true);
                material.SetFloat("_TintAreaMode", 1f);
                material.SetFloat("_TintColorMode", 0f);
                material.SetColor("_TintColor", new Color(0.15f, 1f, 0.15f, 1f));
            },
            mesh: CreateReceiverPlane(new Color(1f, 0.55f, 1f, 1f)));

        AssertReceivesShadow(sample);
        Assert.That(sample.Shadow.g, Is.GreaterThan(sample.Shadow.r + 0.03f), sample.ToString());
        Assert.That(sample.Lit.g, Is.GreaterThan(sample.Lit.r + 0.10f), sample.ToString());
    }

    [Test]
    public void UberLitReceiveShadowsPreserveAmbientContribution()
    {
        var sample = RenderReceiveShadowScene(
            material => ConfigureReceiveShadows(material, true),
            ambient: new Color(0.22f, 0.22f, 0.22f, 1f));

        AssertReceivesShadow(sample);
        Assert.That(Luminance(sample.Shadow), Is.GreaterThan(0.05f), sample.ToString());
    }

    [Test]
    public void UberLitAdditionalLightsCoexistWithReceiveShadows()
    {
        var withoutAdditional = RenderReceiveShadowScene(material => ConfigureReceiveShadows(material, true));
        var withAdditional = RenderReceiveShadowScene(
            material => ConfigureReceiveShadows(material, true),
            configureAfterSetup: material => material.EnableKeyword("_ADDITIONAL_LIGHTS"),
            additionalPointLight: true);

        AssertReceivesShadow(withAdditional);
        Assert.That(
            Luminance(withAdditional.Shadow),
            Is.GreaterThan(Luminance(withoutAdditional.Shadow) + 0.05f),
            $"with {withAdditional}, without {withoutAdditional}");
    }

    private static void ConfigureReceiveShadows(Material material, bool enabled)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.82f, 0.82f, 0.82f, 1f)));
        material.SetFloat("_LitReceiveShadows", enabled ? 1f : 0f);
    }

    private static RenderSample RenderReceiveShadowScene(
        Action<Material> configureMaterial,
        Color? ambient = null,
        Mesh mesh = null,
        Color? background = null,
        Action<Material> configureAfterSetup = null,
        bool additionalPointLight = false)
    {
        var shader = Shader.Find(LitShaderName);
        Assert.That(shader, Is.Not.Null, LitShaderName);

        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        var previousAmbientIntensity = RenderSettings.ambientIntensity;
        var previousShadows = QualitySettings.shadows;
        var previousShadowDistance = QualitySettings.shadowDistance;
        var previousShadowCascades = QualitySettings.shadowCascades;
        var previousShadowProjection = QualitySettings.shadowProjection;
        var previousShadowResolution = QualitySettings.shadowResolution;
        var previousActive = RenderTexture.active;

        var material = new Material(shader);

        NovaLitTestTextures.UseDiffuseDefaults(material);
        var casterMaterial = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
        var receiverMesh = mesh ?? CreateReceiverPlane(Color.white);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGBFloat);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBAFloat, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitShadowCamera");
        var receiverObject = new GameObject("NovaBuiltInUberLitShadowReceiver");
        var depthProxyObject = new GameObject("NovaBuiltInUberLitShadowDepthProxy");
        var casterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var lightObject = new GameObject("NovaBuiltInUberLitShadowLight");
        var additionalLightObject = additionalPointLight ? new GameObject("NovaBuiltInUberLitShadowAdditionalLight") : null;
        var actualBackground = background ?? new Color(0.01f, 0.01f, 0.01f, 1f);

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient ?? new Color(0.04f, 0.04f, 0.04f, 1f);
            RenderSettings.ambientIntensity = 1f;
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
            configureAfterSetup?.Invoke(material);
            casterMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry - 10;
            if (casterMaterial.HasProperty("_Color"))
            {
                casterMaterial.SetColor("_Color", actualBackground);
            }

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

            casterObject.name = "NovaBuiltInUberLitHiddenShadowCaster";
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

            if (additionalLightObject != null)
            {
                additionalLightObject.transform.position = new Vector3(0.2f, 2f, 0f);
                var additionalLight = additionalLightObject.AddComponent<Light>();
                additionalLight.type = LightType.Point;
                additionalLight.color = new Color(1f, 0.92f, 0.82f, 1f);
                additionalLight.intensity = 2.2f;
                additionalLight.range = 5f;
                additionalLight.renderMode = LightRenderMode.ForcePixel;
                additionalLight.shadows = LightShadows.None;
            }

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.5f;
            camera.transform.position = new Vector3(0f, 6f, 0f);
            camera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = actualBackground;
            camera.depthTextureMode = DepthTextureMode.Depth;
            camera.targetTexture = target;
            camera.allowHDR = true;
            camera.allowMSAA = false;
            camera.Render();

            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, RenderSize, RenderSize), 0, 0);
            readback.Apply();
            RenderTexture.active = previousActive;

            return new RenderSample(
                AverageRegion(readback, 0.55f, 0.63f, 0.46f, 0.54f),
                AverageRegion(readback, 0.18f, 0.28f, 0.46f, 0.54f),
                AverageRegion(readback, 0.76f, 0.86f, 0.46f, 0.54f),
                AverageRegion(readback, 0.18f, 0.28f, 0.46f, 0.54f));
        }
        finally
        {
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            RenderSettings.ambientIntensity = previousAmbientIntensity;
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
            if (additionalLightObject != null)
            {
                UnityEngine.Object.DestroyImmediate(additionalLightObject);
            }
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(receiverMesh);
            DestroyMaterialAndOwnedTextures(material);
            UnityEngine.Object.DestroyImmediate(casterMaterial);
        }
    }

    private static void AssertReceivesShadow(RenderSample sample)
    {
        Assert.That(Luminance(sample.Lit), Is.GreaterThan(Luminance(sample.Shadow) + 0.08f), sample.ToString());
    }

    private static float ShadowDelta(RenderSample sample)
    {
        return Luminance(sample.Lit) - Luminance(sample.Shadow);
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

    private static Mesh CreateReceiverPlane(Color vertexColor)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberLitShadowReceiverPlane" };
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
        public RenderSample(Color shadow, Color lit, Color rightLit, Color clipped)
        {
            Shadow = shadow;
            Lit = lit;
            RightLit = rightLit;
            Clipped = clipped;
        }

        public Color Shadow { get; }
        public Color Lit { get; }
        public Color RightLit { get; }
        public Color Clipped { get; }

        public override string ToString()
        {
            return $"shadow {Shadow}, lit {Lit}, rightLit {RightLit}, clipped {Clipped}";
        }
    }
}

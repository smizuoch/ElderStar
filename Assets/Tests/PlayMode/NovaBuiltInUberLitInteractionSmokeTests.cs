using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberLitInteractionSmokeTests
{
    private const string LitShaderName = "Nova/Particles/UberLit";
    private const int RenderSize = 144;
    private const int ShadowRenderSize = 160;
    private const int TextureSize = 8;

    [Test]
    public void UberLitComplexSurfaceLightingInteraction()
    {
        var simple = RenderForward(material => ConfigureMetallicSurface(material, 0f, 0.25f));
        var complex = RenderForward(
            material =>
            {
                ConfigureSpecularSurface(material, new Color(0.25f, 0.8f, 1f, 1f), 1f);
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.35f, 0.25f, 0.25f, 1f)));
                material.SetTexture("_NormalMap", CreateSolidTexture(new Color(0.7f, 0.5f, 1f, 1f)));
                material.SetTexture("_SmoothnessMap", CreateSplitTexture(new Color(0.05f, 0.05f, 0.05f, 0.05f), Color.white));
                material.SetFloat("_SmoothnessMapChannelsX", 0f);
                material.SetFloat("_TintAreaMode", 1f);
                material.SetFloat("_TintColorMode", 0f);
                material.SetColor("_TintColor", new Color(0.35f, 1f, 0.45f, 1f));
            },
            mesh: CreateQuad(new Color(1f, 0.6f, 1f, 1f)),
            light: LightCase.Angled(Color.white, 2.4f));

        Assert.That(Luminance(complex.Center), Is.GreaterThan(0.02f), complex.Center.ToString());
        Assert.That(ColorDistance(complex.Center, simple.Center), Is.GreaterThan(0.06f), $"simple {simple.Center}, complex {complex.Center}");
    }

    [Test]
    public void UberLitReceiveShadowEnvironmentReflectionInteraction()
    {
        var shadowedReflection = RenderShadowReceiver(material =>
        {
            ConfigureReflectionSurface(material, true);
            material.SetFloat("_LitReceiveShadows", 1f);
        });
        var shadowedNoReflection = RenderShadowReceiver(material =>
        {
            ConfigureReflectionSurface(material, false);
            material.SetFloat("_LitReceiveShadows", 1f);
        });

        AssertReceivesShadow(shadowedReflection);
        Assert.That(Luminance(shadowedReflection.Shadow), Is.GreaterThan(Luminance(shadowedNoReflection.Shadow) + 0.015f), $"reflection {shadowedReflection}, no reflection {shadowedNoReflection}");
    }

    [Test]
    public void UberLitAdditionalLightEnvironmentReflectionInteraction()
    {
        var reflectionOnly = RenderForward(material => ConfigureReflectionSurface(material, true), mainLightIntensity: 0.15f);
        var withAdditional = RenderForward(
            material => ConfigureReflectionSurface(material, true),
            AdditionalLightCase.Point(new Color(1f, 0.08f, 0.08f, 1f)),
            mainLightIntensity: 0.15f);

        var delta = withAdditional.Center - reflectionOnly.Center;
        Assert.That(Luminance(reflectionOnly.Center), Is.GreaterThan(0.02f), reflectionOnly.Center.ToString());
        Assert.That(Luminance(delta), Is.GreaterThan(0.015f), $"reflection {reflectionOnly.Center}, with additional {withAdditional.Center}, delta {delta}");
        Assert.That(delta.r, Is.GreaterThan(delta.b + 0.03f), $"reflection {reflectionOnly.Center}, with additional {withAdditional.Center}, delta {delta}");
    }

    [Test]
    public void UberLitAdditionalLightReceiveShadowInteraction()
    {
        var withoutAdditional = RenderShadowReceiver(material => ConfigureReceiveShadowSurface(material));
        var withAdditional = RenderShadowReceiver(
            material => ConfigureReceiveShadowSurface(material),
            additionalPointLight: true);

        AssertReceivesShadow(withAdditional);
        Assert.That(Luminance(withAdditional.Shadow), Is.GreaterThan(Luminance(withoutAdditional.Shadow) + 0.035f), $"with {withAdditional}, without {withoutAdditional}");
    }

    [Test]
    public void UberLitShadowCasterWithLitSurfaceInteraction()
    {
        var disabled = RenderNovaShadowCasterStats(material =>
        {
            ConfigureShadowCasterMaterial(material);
            material.SetFloat("_ShadowCasterEnabled", 0f);
        });
        var complex = RenderNovaShadowCasterStats(material =>
        {
            ConfigureShadowCasterMaterial(material);
            material.SetFloat("_LitWorkflowMode", 0f);
            material.SetFloat("_SpecularHighlights", 1f);
            material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
            material.SetTexture("_SpecularMap", CreateSolidTexture(Color.white));
            material.SetTexture("_SmoothnessMap", CreateSolidTexture(Color.white));
        });

        Assert.That(complex.ShadowPixels, Is.GreaterThan(disabled.ShadowPixels + 120), $"disabled {disabled}, complex {complex}");
        Assert.That(complex.AverageLuminance, Is.LessThan(disabled.AverageLuminance - 0.015f), $"disabled {disabled}, complex {complex}");
    }

    [Test]
    public void UberLitDepthOnlyWithAlphaAndSurfaceInteraction()
    {
        var lowCutoff = RenderDepthProbe(material =>
        {
            ConfigureDepthSurface(material);
            material.SetFloat("_Cutoff", 0.1f);
        });
        var highCutoff = RenderDepthProbe(material =>
        {
            ConfigureDepthSurface(material);
            material.SetFloat("_Cutoff", 0.9f);
        });

        Assert.That(lowCutoff.g, Is.GreaterThan(highCutoff.g + 0.5f), $"low {lowCutoff}, high {highCutoff}");
    }

    [Test]
    public void UberLitForwardBaseForwardAddAlphaTintInteraction()
    {
        var background = new Color(0.02f, 0.07f, 0.72f, 1f);
        var sample = RenderForward(
            material =>
            {
                ConfigureMetallicSurface(material, 0f, 0.35f);
                material.SetFloat("_RenderType", 1f);
                material.SetFloat("_Cutoff", 0.5f);
                material.SetTexture("_BaseMap", CreateSplitTexture(new Color(1f, 1f, 1f, 0.15f), Color.white));
                material.SetFloat("_TintAreaMode", 1f);
                material.SetFloat("_TintColorMode", 0f);
                material.SetColor("_TintColor", new Color(0.12f, 1f, 0.12f, 1f));
            },
            AdditionalLightCase.Point(Color.white),
            mesh: CreateQuad(new Color(1f, 0.55f, 1f, 1f)),
            background: background);

        Assert.That(sample.Left.b, Is.GreaterThan(sample.Left.r + 0.20f), sample.Left.ToString());
        Assert.That(sample.Right.g, Is.GreaterThan(sample.Right.r + 0.06f), sample.Right.ToString());
        Assert.That(Luminance(sample.Right), Is.GreaterThan(Luminance(sample.Left) + 0.05f), $"left {sample.Left}, right {sample.Right}");
    }

    [Test]
    public void UberLitAllImplementedLightingFeaturesStayDistinguishable()
    {
        var allFeatures = RenderShadowReceiver(material => ConfigureAllLightingFeatures(material, reflection: true, receiveShadows: true), additionalPointLight: true);
        var withoutAdditional = RenderShadowReceiver(material => ConfigureAllLightingFeatures(material, reflection: true, receiveShadows: true));
        var withoutReflection = RenderShadowReceiver(
            material => ConfigureAllLightingFeatures(material, reflection: false, receiveShadows: true),
            additionalPointLight: true);
        var withoutReceiveShadows = RenderShadowReceiver(
            material => ConfigureAllLightingFeatures(material, reflection: true, receiveShadows: false),
            additionalPointLight: true);

        Assert.That(ShadowDelta(allFeatures), Is.GreaterThan(0.02f), allFeatures.ToString());
        Assert.That(Luminance(allFeatures.Lit), Is.GreaterThan(Luminance(withoutAdditional.Lit) + 0.02f), $"all {allFeatures}, without additional {withoutAdditional}");
        Assert.That(Luminance(allFeatures.Shadow), Is.GreaterThan(Luminance(withoutReflection.Shadow) + 0.015f), $"all {allFeatures}, without reflection {withoutReflection}");
        Assert.That(ShadowDelta(withoutReceiveShadows), Is.LessThan(ShadowDelta(allFeatures) * 0.55f), $"all {allFeatures}, without shadows {withoutReceiveShadows}");
    }

    private static void ConfigureAllLightingFeatures(Material material, bool reflection, bool receiveShadows)
    {
        ConfigureReflectionSurface(material, reflection);
        material.SetFloat("_LitReceiveShadows", receiveShadows ? 1f : 0f);
        material.SetFloat("_SpecularHighlights", 1f);
        material.SetTexture("_NormalMap", CreateSolidTexture(new Color(0.7f, 0.5f, 1f, 1f)));
        material.SetTexture("_SmoothnessMap", CreateSolidTexture(Color.white));
    }

    private static void ConfigureMetallicSurface(Material material, float metallic, float smoothness)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.30f, 0.30f, 0.30f, 1f)));
        material.SetFloat("_LitWorkflowMode", 1f);
        material.SetFloat("_SpecularHighlights", 1f);
        material.SetFloat("_EnvironmentReflections", 0f);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
    }

    private static void ConfigureSpecularSurface(Material material, Color specularColor, float smoothness)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.30f, 0.30f, 0.30f, 1f)));
        material.SetFloat("_LitWorkflowMode", 0f);
        material.SetFloat("_SpecularHighlights", 1f);
        material.SetFloat("_EnvironmentReflections", 0f);
        material.SetColor("_SpecularColor", specularColor);
        material.SetFloat("_Smoothness", smoothness);
    }

    private static void ConfigureReflectionSurface(Material material, bool enabled)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.24f, 0.24f, 0.24f, 1f)));
        material.SetFloat("_LitWorkflowMode", 1f);
        material.SetFloat("_SpecularHighlights", 0f);
        material.SetFloat("_EnvironmentReflections", enabled ? 1f : 0f);
        material.SetFloat("_Metallic", 1f);
        material.SetFloat("_Smoothness", 0.9f);
    }

    private static void ConfigureReceiveShadowSurface(Material material)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.82f, 0.82f, 0.82f, 1f)));
        material.SetFloat("_LitReceiveShadows", 1f);
        material.SetFloat("_LitWorkflowMode", 1f);
        material.SetFloat("_Metallic", 0.2f);
        material.SetFloat("_Smoothness", 0.55f);
    }

    private static void ConfigureShadowCasterMaterial(Material material)
    {
        material.SetFloat("_Cull", (float)CullMode.Back);
        material.SetFloat("_RenderType", 1f);
        material.SetFloat("_Cutoff", 0.5f);
        material.SetFloat("_ShadowCasterEnabled", 1f);
        material.SetFloat("_ShadowCasterAlphaTestEnabled", 1f);
        material.SetFloat("_ShadowCasterAlphaCutoff", 0.5f);
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 1f, 1f, 0.8f)));
        material.SetFloat("_Metallic", 0.4f);
        material.SetFloat("_Smoothness", 0.8f);
    }

    private static void ConfigureDepthSurface(Material material)
    {
        material.SetFloat("_RenderType", 1f);
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 1f, 1f, 0.45f)));
        material.SetFloat("_LitWorkflowMode", 0f);
        material.SetFloat("_SpecularHighlights", 1f);
        material.SetTexture("_NormalMap", CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f)));
        material.SetTexture("_SmoothnessMap", CreateSolidTexture(Color.white));
    }

    private static RenderSample RenderForward(
        Action<Material> configureMaterial,
        AdditionalLightCase? additionalLight = null,
        Mesh mesh = null,
        Color? background = null,
        float mainLightIntensity = 1.2f,
        LightCase? light = null)
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
        var actualMesh = mesh ?? CreateQuad(Color.white);
        var cubemap = CreateReflectionCubemap();
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitInteractionForwardCamera");
        var meshObject = new GameObject("NovaBuiltInUberLitInteractionForwardMesh");
        var mainLightObject = new GameObject("NovaBuiltInUberLitInteractionForwardMainLight");
        var additionalLightObject = additionalLight.HasValue ? new GameObject("NovaBuiltInUberLitInteractionForwardAdditionalLight") : null;

        try
        {
            QualitySettings.pixelLightCount = 8;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;
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
            if (additionalLight.HasValue)
            {
                material.EnableKeyword("_ADDITIONAL_LIGHTS");
            }

            meshObject.AddComponent<MeshFilter>().sharedMesh = actualMesh;
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            var selectedLight = light ?? LightCase.Front(Color.white, mainLightIntensity);
            var mainLight = mainLightObject.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.color = selectedLight.Color;
            mainLight.intensity = selectedLight.Intensity;
            mainLight.renderMode = LightRenderMode.ForcePixel;
            mainLightObject.transform.rotation = selectedLight.Rotation;

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
            if (previousDefaultReflectionMode == DefaultReflectionMode.Custom)
            {
                RenderSettings.customReflection = previousCustomReflection;
            }
            RenderSettings.reflectionIntensity = previousReflectionIntensity;
            DynamicGI.UpdateEnvironment();
            QualitySettings.pixelLightCount = previousPixelLightCount;
            RenderTexture.active = previousActive;

            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(meshObject);
            UnityEngine.Object.DestroyImmediate(mainLightObject);
            UnityEngine.Object.DestroyImmediate(additionalLightObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(actualMesh);
            UnityEngine.Object.DestroyImmediate(cubemap);
            DestroyMaterialAndOwnedTextures(material);
        }
    }

    private static ShadowSample RenderShadowReceiver(Action<Material> configureMaterial, bool additionalPointLight = false)
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
        var target = new RenderTexture(ShadowRenderSize, ShadowRenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(ShadowRenderSize, ShadowRenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitInteractionShadowCamera");
        var receiverObject = new GameObject("NovaBuiltInUberLitInteractionShadowReceiver");
        var depthProxyObject = new GameObject("NovaBuiltInUberLitInteractionShadowDepthProxy");
        var casterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var lightObject = new GameObject("NovaBuiltInUberLitInteractionShadowLight");
        var additionalLightObject = additionalPointLight ? new GameObject("NovaBuiltInUberLitInteractionShadowAdditionalLight") : null;

        try
        {
            QualitySettings.pixelLightCount = 8;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 40f;
            QualitySettings.shadowCascades = 0;
            QualitySettings.shadowProjection = ShadowProjection.StableFit;
            QualitySettings.shadowResolution = ShadowResolution.High;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.035f, 0.035f, 0.035f, 1f);
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
            if (additionalPointLight)
            {
                material.EnableKeyword("_ADDITIONAL_LIGHTS");
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

            if (additionalPointLight)
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
            camera.backgroundColor = new Color(0.01f, 0.01f, 0.01f, 1f);
            camera.depthTextureMode = DepthTextureMode.Depth;
            camera.targetTexture = target;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.Render();

            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, ShadowRenderSize, ShadowRenderSize), 0, 0);
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
            RenderSettings.defaultReflectionMode = previousDefaultReflectionMode;
            if (previousDefaultReflectionMode == DefaultReflectionMode.Custom)
            {
                RenderSettings.customReflection = previousCustomReflection;
            }
            RenderSettings.reflectionIntensity = previousReflectionIntensity;
            DynamicGI.UpdateEnvironment();
            QualitySettings.pixelLightCount = previousPixelLightCount;
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
            UnityEngine.Object.DestroyImmediate(additionalLightObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(receiverMesh);
            UnityEngine.Object.DestroyImmediate(cubemap);
            DestroyMaterialAndOwnedTextures(material);
            UnityEngine.Object.DestroyImmediate(casterMaterial);
        }
    }

    private static ShadowStats RenderNovaShadowCasterStats(Action<Material> configureMaterial)
    {
        var shader = Shader.Find(LitShaderName);
        Assert.That(shader, Is.Not.Null, LitShaderName);

        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        var previousShadowDistance = QualitySettings.shadowDistance;
        var previousActive = RenderTexture.active;

        var material = new Material(shader);
        var planeMaterial = CreatePlaneMaterial();
        var target = new RenderTexture(128, 128, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitInteractionCasterCamera");
        var lightObject = new GameObject("NovaBuiltInUberLitInteractionCasterLight");
        var planeObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        var casterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.22f, 0.22f, 1f);
            QualitySettings.shadowDistance = 30f;

            configureMaterial(material);
            SetupUberLit(material);

            planeObject.transform.position = Vector3.zero;
            planeObject.transform.localScale = new Vector3(2.5f, 1f, 2.5f);
            planeObject.GetComponent<MeshRenderer>().sharedMaterial = planeMaterial;
            planeObject.GetComponent<MeshRenderer>().receiveShadows = true;

            casterObject.transform.position = new Vector3(0f, 0.65f, 0f);
            casterObject.transform.localScale = new Vector3(0.75f, 1.1f, 0.75f);
            var casterRenderer = casterObject.GetComponent<MeshRenderer>();
            casterRenderer.sharedMaterial = material;
            casterRenderer.shadowCastingMode = material.GetFloat("_ShadowCasterEnabled") > 0.5f
                ? ShadowCastingMode.ShadowsOnly
                : ShadowCastingMode.Off;
            casterRenderer.receiveShadows = false;

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Hard;
            light.shadowStrength = 1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 1.7f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;
            camera.transform.position = new Vector3(0f, 3.1f, -2.6f);
            camera.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
            camera.targetTexture = target;
            camera.Render();

            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            readback.Apply();
            RenderTexture.active = previousActive;

            return ShadowStats.From(readback);
        }
        finally
        {
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            QualitySettings.shadowDistance = previousShadowDistance;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(lightObject);
            UnityEngine.Object.DestroyImmediate(planeObject);
            UnityEngine.Object.DestroyImmediate(casterObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            DestroyMaterialAndOwnedTextures(material);
            UnityEngine.Object.DestroyImmediate(planeMaterial);
        }
    }

    private static Color RenderDepthProbe(Action<Material> configureDepthMaterial)
    {
        var shader = Shader.Find(LitShaderName);
        var probeShader = Shader.Find("Hidden/NovaBuiltIn/Tests/DepthProbe");
        Assert.That(shader, Is.Not.Null, LitShaderName);
        Assert.That(probeShader, Is.Not.Null, "Hidden/NovaBuiltIn/Tests/DepthProbe");

        var previousActive = RenderTexture.active;
        var depthMaterial = new Material(shader);
        var probeMaterial = new Material(probeShader);
        var mesh = CreateQuad(Color.white);
        var target = new RenderTexture(64, 64, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitInteractionDepthProbeCamera");
        var commandBuffer = new CommandBuffer { name = "NovaBuiltInUberLitInteractionDepthProbe" };

        try
        {
            var depthPass = depthMaterial.FindPass("DepthOnly");
            Assert.That(depthPass, Is.GreaterThanOrEqualTo(0), "DepthOnly pass should exist.");

            depthMaterial.SetFloat("_Cull", (float)CullMode.Off);
            depthMaterial.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            configureDepthMaterial(depthMaterial);
            SetupUberLit(depthMaterial);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 0.75f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;
            camera.transform.position = new Vector3(0f, 0f, -2f);
            camera.targetTexture = target;

            commandBuffer.DrawMesh(mesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), depthMaterial, 0, depthPass);
            commandBuffer.DrawMesh(mesh, Matrix4x4.TRS(new Vector3(0f, 0f, 0.2f), Quaternion.identity, Vector3.one), probeMaterial);
            camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            camera.Render();

            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, 64, 64), 0, 0);
            readback.Apply();
            RenderTexture.active = previousActive;
            return readback.GetPixel(32, 32);
        }
        finally
        {
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(mesh);
            DestroyMaterialAndOwnedTextures(depthMaterial);
            UnityEngine.Object.DestroyImmediate(probeMaterial);
            commandBuffer.Release();
        }
    }

    private static void AssertReceivesShadow(ShadowSample sample)
    {
        Assert.That(Luminance(sample.Lit), Is.GreaterThan(Luminance(sample.Shadow) + 0.07f), sample.ToString());
    }

    private static float ShadowDelta(ShadowSample sample)
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

    private static Cubemap CreateReflectionCubemap()
    {
        var cubemap = new Cubemap(16, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };

        SetFace(cubemap, CubemapFace.PositiveX, new Color(0.9f, 0.1f, 0.1f, 1f));
        SetFace(cubemap, CubemapFace.NegativeX, new Color(0.1f, 0.9f, 0.1f, 1f));
        SetFace(cubemap, CubemapFace.PositiveY, new Color(0.1f, 0.1f, 0.9f, 1f));
        SetFace(cubemap, CubemapFace.NegativeY, new Color(0.9f, 0.9f, 0.1f, 1f));
        SetFace(cubemap, CubemapFace.PositiveZ, new Color(0.1f, 0.9f, 0.9f, 1f));
        SetFace(cubemap, CubemapFace.NegativeZ, new Color(0.9f, 0.1f, 0.9f, 1f));
        cubemap.Apply();
        return cubemap;
    }

    private static void SetFace(Cubemap cubemap, CubemapFace face, Color color)
    {
        cubemap.SetPixels(Enumerable.Repeat(color, 16 * 16).ToArray(), face);
    }

    private static Mesh CreateQuad(Color vertexColor)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberLitInteractionQuad" };
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
        var mesh = new Mesh { name = "NovaBuiltInUberLitInteractionShadowReceiverPlane" };
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

    private static Material CreatePlaneMaterial()
    {
        var shader = Shader.Find("Standard");
        Assert.That(shader, Is.Not.Null, "Standard");
        var material = new Material(shader);
        material.SetColor("_Color", new Color(0.76f, 0.76f, 0.76f, 1f));
        return material;
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
        var properties = new[]
        {
            "_BaseMap",
            "_NormalMap",
            "_SpecularMap",
            "_MetallicMap",
            "_SmoothnessMap",
        };

        foreach (var texture in properties
                     .Where(material.HasProperty)
                     .Select(material.GetTexture)
                     .Where(texture => texture != null)
                     .Distinct())
        {
            UnityEngine.Object.DestroyImmediate(texture);
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
        public ShadowSample(Color shadow, Color lit)
        {
            Shadow = shadow;
            Lit = lit;
        }

        public Color Lit { get; }
        public Color Shadow { get; }

        public override string ToString()
        {
            return $"lit {Lit}, shadow {Shadow}";
        }
    }

    private readonly struct ShadowStats
    {
        private ShadowStats(float averageLuminance, int shadowPixels)
        {
            AverageLuminance = averageLuminance;
            ShadowPixels = shadowPixels;
        }

        public float AverageLuminance { get; }
        public int ShadowPixels { get; }

        public static ShadowStats From(Texture2D texture)
        {
            var luminanceSum = 0f;
            var sampleCount = 0;
            var shadowPixels = 0;

            for (var y = texture.height / 4; y < texture.height * 3 / 4; y++)
            {
                for (var x = texture.width / 4; x < texture.width * 3 / 4; x++)
                {
                    var luminance = Luminance(texture.GetPixel(x, y));
                    luminanceSum += luminance;
                    sampleCount++;
                    if (luminance < 0.48f)
                    {
                        shadowPixels++;
                    }
                }
            }

            return new ShadowStats(luminanceSum / sampleCount, shadowPixels);
        }

        public override string ToString()
        {
            return $"AverageLuminance={AverageLuminance}, ShadowPixels={ShadowPixels}";
        }
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

        public static LightCase Angled(Color color, float intensity)
        {
            return new LightCase(Quaternion.LookRotation(new Vector3(0.45f, 0.15f, 1f).normalized), color, intensity);
        }
    }

    private readonly struct AdditionalLightCase
    {
        private AdditionalLightCase(Color color, Vector3 position, float intensity, float range)
        {
            Color = color;
            Position = position;
            Intensity = intensity;
            Range = range;
        }

        private Color Color { get; }
        private Vector3 Position { get; }
        private float Intensity { get; }
        private float Range { get; }

        public static AdditionalLightCase Point(Color color)
        {
            return new AdditionalLightCase(color, new Vector3(0f, 0f, -1.2f), 4.2f, 4.5f);
        }

        public void Apply(GameObject lightObject)
        {
            lightObject.transform.position = Position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color;
            light.intensity = Intensity;
            light.range = Range;
            light.renderMode = LightRenderMode.ForcePixel;
            light.shadows = LightShadows.None;
        }
    }
}

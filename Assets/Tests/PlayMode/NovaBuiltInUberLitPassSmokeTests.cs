using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberLitPassSmokeTests
{
    private const string ShaderName = "Nova/Particles/UberLit";
    private const int RenderSize = 128;
    private const int TextureSize = 4;

    [Test]
    public void UberLitShadowCasterCastsShadowWhenEnabled()
    {
        var disabled = RenderShadowStats(material => material.SetFloat("_ShadowCasterEnabled", 0));
        var enabled = RenderShadowStats(material => material.SetFloat("_ShadowCasterEnabled", 1));

        Assert.That(enabled.AverageLuminance, Is.LessThan(disabled.AverageLuminance - 0.015f), $"disabled {disabled}, enabled {enabled}");
    }

    [Test]
    public void UberLitShadowCasterDisabledReducesShadow()
    {
        var enabled = RenderShadowStats(material => material.SetFloat("_ShadowCasterEnabled", 1));
        var disabled = RenderShadowStats(material => material.SetFloat("_ShadowCasterEnabled", 0));

        Assert.That(disabled.ShadowPixels, Is.LessThan(enabled.ShadowPixels * 0.65f), $"enabled {enabled}, disabled {disabled}");
    }

    [Test]
    public void UberLitShadowCasterAlphaCutoffAffectsShadow()
    {
        var lowCutoff = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.1f);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 1f, 1f, 0.45f)));
        });
        var highCutoff = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.9f);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 1f, 1f, 0.45f)));
        });

        Assert.That(highCutoff.ShadowPixels, Is.LessThan(lowCutoff.ShadowPixels * 0.65f), $"low {lowCutoff}, high {highCutoff}");
    }

    [Test]
    public void UberLitShadowCasterAlphaTestDisabledIgnoresCutoff()
    {
        var alphaTestDisabled = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 0);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.95f);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 1f, 1f, 0.2f)));
        });
        var alphaTestEnabled = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.95f);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 1f, 1f, 0.2f)));
        });

        Assert.That(alphaTestDisabled.ShadowPixels, Is.GreaterThan(alphaTestEnabled.ShadowPixels + 80), $"disabled {alphaTestDisabled}, enabled {alphaTestEnabled}");
    }

    [Test]
    public void UberLitShadowCasterAlphaCanBeAffectedByTintColor()
    {
        var unaffected = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.5f);
            material.SetFloat("_TintAreaMode", 1);
            material.SetFloat("_TintColorMode", 0);
            material.SetColor("_TintColor", new Color(1f, 1f, 1f, 0.1f));
            material.SetFloat("_ShadowCasterAlphaAffectedByTintColor", 0);
        });
        var affected = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.5f);
            material.SetFloat("_TintAreaMode", 1);
            material.SetFloat("_TintColorMode", 0);
            material.SetColor("_TintColor", new Color(1f, 1f, 1f, 0.1f));
            material.SetFloat("_ShadowCasterAlphaAffectedByTintColor", 1);
        });

        Assert.That(affected.ShadowPixels, Is.LessThan(unaffected.ShadowPixels * 0.65f), $"unaffected {unaffected}, affected {affected}");
    }

    [Test]
    public void UberLitDepthOnlyPassWritesDepth()
    {
        var withoutDepth = RenderDepthOnlyProbe(null);
        var withDepth = RenderDepthOnlyProbe(material =>
        {
            material.SetFloat("_RenderType", 0);
            material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
        });

        Assert.That(withDepth.g, Is.GreaterThan(withoutDepth.g + 0.5f), $"without {withoutDepth}, with {withDepth}");
    }

    [Test]
    public void UberLitDepthOnlyAlphaTestAffectsDepth()
    {
        var lowCutoff = RenderDepthOnlyProbe(material =>
        {
            material.SetFloat("_RenderType", 1);
            material.SetFloat("_Cutoff", 0.1f);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 1f, 1f, 0.45f)));
        });
        var highCutoff = RenderDepthOnlyProbe(material =>
        {
            material.SetFloat("_RenderType", 1);
            material.SetFloat("_Cutoff", 0.9f);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 1f, 1f, 0.45f)));
        });

        Assert.That(lowCutoff.g, Is.GreaterThan(highCutoff.g + 0.5f), $"low {lowCutoff}, high {highCutoff}");
    }

    [Test]
    public void UberLitShadowAndDepthPassesDoNotBreakForwardBaseAndForwardAdd()
    {
        var withoutAdditional = RenderForwardSample(null);
        var withAdditional = RenderForwardSample(light =>
        {
            light.type = LightType.Point;
            light.color = Color.white;
            light.intensity = 4f;
            light.range = 5f;
        });

        Assert.That(Luminance(withAdditional), Is.GreaterThan(Luminance(withoutAdditional) + 0.04f), $"without {withoutAdditional}, with {withAdditional}");
    }

    private static ShadowStats RenderShadowStats(Action<Material> configureMaterial)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var planeMaterial = CreatePlaneMaterial();
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitShadowSmokeCamera");
        var lightObject = new GameObject("NovaBuiltInUberLitShadowSmokeLight");
        var planeObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        var casterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        var previousShadowDistance = QualitySettings.shadowDistance;

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.22f, 0.22f, 1f);
            QualitySettings.shadowDistance = 30f;

            material.SetFloat("_Cull", (float)CullMode.Back);
            material.SetFloat("_RenderType", 0);
            material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
            material.SetFloat("_LitWorkflowMode", 1f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.25f);
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

            ReadRenderTexture(target, readback);
            return ShadowStats.From(readback);
        }
        finally
        {
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            QualitySettings.shadowDistance = previousShadowDistance;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(lightObject);
            UnityEngine.Object.DestroyImmediate(planeObject);
            UnityEngine.Object.DestroyImmediate(casterObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(material);
            UnityEngine.Object.DestroyImmediate(planeMaterial);
        }
    }

    private static Color RenderDepthOnlyProbe(Action<Material> configureDepthMaterial)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);
        var probeShader = Shader.Find("Hidden/NovaBuiltIn/Tests/DepthProbe");
        Assert.That(probeShader, Is.Not.Null, "Hidden/NovaBuiltIn/Tests/DepthProbe");

        var depthMaterial = new Material(shader);
        var probeMaterial = new Material(probeShader);
        var mesh = CreateQuad(Color.white);
        var target = new RenderTexture(64, 64, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitDepthOnlyProbeCamera");
        var commandBuffer = new CommandBuffer { name = "NovaBuiltInUberLitDepthOnlyProbe" };

        try
        {
            var depthPass = depthMaterial.FindPass("DepthOnly");
            Assert.That(depthPass, Is.GreaterThanOrEqualTo(0), "DepthOnly pass should exist.");

            depthMaterial.SetFloat("_Cull", (float)CullMode.Off);
            depthMaterial.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            configureDepthMaterial?.Invoke(depthMaterial);
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

            if (configureDepthMaterial != null)
            {
                commandBuffer.DrawMesh(mesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), depthMaterial, 0, depthPass);
            }

            commandBuffer.DrawMesh(mesh, Matrix4x4.TRS(new Vector3(0f, 0f, 0.2f), Quaternion.identity, Vector3.one), probeMaterial);
            camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            camera.Render();

            ReadRenderTexture(target, readback);
            return readback.GetPixel(32, 32);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(mesh);
            UnityEngine.Object.DestroyImmediate(depthMaterial);
            UnityEngine.Object.DestroyImmediate(probeMaterial);
            commandBuffer.Release();
        }
    }

    private static Color RenderForwardSample(Action<Light> configureAdditionalLight)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var mesh = CreateQuad(Color.white);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberLitForwardCamera");
        var meshObject = new GameObject("NovaBuiltInUberLitForwardMesh");
        var mainLightObject = new GameObject("NovaBuiltInUberLitForwardMainLight");
        var additionalLightObject = configureAdditionalLight == null ? null : new GameObject("NovaBuiltInUberLitForwardAdditionalLight");
        var previousPixelLightCount = QualitySettings.pixelLightCount;
        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;

        try
        {
            QualitySettings.pixelLightCount = 8;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;

            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.35f, 0.35f, 0.35f, 1f)));
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_LitWorkflowMode", 1f);
            material.SetFloat("_SpecularHighlights", 1f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.35f);
            SetupUberLit(material);
            material.EnableKeyword("_ADDITIONAL_LIGHTS");

            meshObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            var mainLight = mainLightObject.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.color = new Color(0.18f, 0.18f, 0.18f, 1f);
            mainLight.intensity = 1f;
            mainLightObject.transform.rotation = Quaternion.LookRotation(Vector3.forward);

            if (additionalLightObject != null)
            {
                additionalLightObject.transform.position = new Vector3(0f, 0f, -1.2f);
                var additionalLight = additionalLightObject.AddComponent<Light>();
                configureAdditionalLight(additionalLight);
            }

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 0.75f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;
            camera.transform.position = new Vector3(0f, 0f, -2f);
            camera.targetTexture = target;
            camera.Render();

            ReadRenderTexture(target, readback);
            return readback.GetPixel(RenderSize / 2, RenderSize / 2);
        }
        finally
        {
            QualitySettings.pixelLightCount = previousPixelLightCount;
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(meshObject);
            UnityEngine.Object.DestroyImmediate(mainLightObject);
            UnityEngine.Object.DestroyImmediate(additionalLightObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(mesh);
            UnityEngine.Object.DestroyImmediate(material);
        }
    }

    private static Material CreatePlaneMaterial()
    {
        var shader = Shader.Find("Standard");
        Assert.That(shader, Is.Not.Null, "Standard");
        var material = new Material(shader);
        material.SetColor("_Color", new Color(0.76f, 0.76f, 0.76f, 1f));
        return material;
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

    private static Mesh CreateQuad(Color color)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberLitPassSmokeQuad" };
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
        mesh.colors = Enumerable.Repeat(color, 4).ToArray();
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

    private static void ReadRenderTexture(RenderTexture target, Texture2D readback)
    {
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        readback.ReadPixels(new Rect(0, 0, readback.width, readback.height), 0, 0);
        readback.Apply();
        RenderTexture.active = previous;
    }

    private static float Luminance(Color color)
    {
        return color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
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
                    var color = texture.GetPixel(x, y);
                    var luminance = Luminance(color);
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberUnlitShadowSmokeTests
{
    private const string ShaderName = "Nova/Particles/UberUnlit";
    private const int RenderSize = 128;
    private const int TextureSize = 4;

    [Test]
    public void UberUnlitShadowCasterCastsShadowWhenEnabled()
    {
        var disabled = RenderShadowStats(material => material.SetFloat("_ShadowCasterEnabled", 0));
        var enabled = RenderShadowStats(material => material.SetFloat("_ShadowCasterEnabled", 1));

        Assert.That(enabled.AverageLuminance, Is.LessThan(disabled.AverageLuminance - 0.015f), $"disabled {disabled}, enabled {enabled}");
    }

    [Test]
    public void UberUnlitShadowCasterDisabledReducesShadow()
    {
        var enabled = RenderShadowStats(material => material.SetFloat("_ShadowCasterEnabled", 1));
        var disabled = RenderShadowStats(material => material.SetFloat("_ShadowCasterEnabled", 0));

        Assert.That(disabled.ShadowPixels, Is.LessThan(enabled.ShadowPixels * 0.65f), $"enabled {enabled}, disabled {disabled}");
    }

    [Test]
    public void UberUnlitShadowCasterAlphaCutoffAffectsShadow()
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
    public void UberUnlitShadowCasterAlphaTestDisabledIgnoresCutoff()
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
    public void UberUnlitShadowCasterCanApplyVertexDeformation()
    {
        var withoutShadowDeformation = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            ConfigureVertexDeformation(material);
            material.SetFloat("_ShadowCasterApplyVertexDeformation", 0);
        });
        var withShadowDeformation = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            ConfigureVertexDeformation(material);
            material.SetFloat("_ShadowCasterApplyVertexDeformation", 1);
        });

        Assert.That(Mathf.Abs(withShadowDeformation.ShadowPixels - withoutShadowDeformation.ShadowPixels), Is.GreaterThan(60), $"without {withoutShadowDeformation}, with {withShadowDeformation}");
    }

    [Test]
    public void UberUnlitForwardDeformationDoesNotForceShadowDeformationWhenDisabled()
    {
        var noDeformation = RenderShadowStats(material => material.SetFloat("_ShadowCasterEnabled", 1));
        var shadowDeformationDisabled = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            ConfigureVertexDeformation(material);
            material.SetFloat("_ShadowCasterApplyVertexDeformation", 0);
        });

        Assert.That(Mathf.Abs(noDeformation.ShadowPixels - shadowDeformationDisabled.ShadowPixels), Is.LessThan(80), $"none {noDeformation}, disabled {shadowDeformationDisabled}");
    }

    [Test]
    public void UberUnlitShadowCasterAlphaCanBeAffectedByTintColor()
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
    public void UberUnlitShadowCasterAlphaCanBeAffectedByAlphaTransition()
    {
        var unaffected = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.5f);
            material.SetFloat("_AlphaTransitionMode", 1);
            material.SetFloat("_AlphaTransitionMapMode", 0);
            material.SetFloat("_AlphaTransitionProgress", 1f);
            material.SetTexture("_AlphaTransitionMap", CreateSolidTexture(Color.white));
            material.SetFloat("_ShadowCasterAlphaAffectedByAlphaTransitionMap", 0);
        });
        var affected = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.5f);
            material.SetFloat("_AlphaTransitionMode", 1);
            material.SetFloat("_AlphaTransitionMapMode", 0);
            material.SetFloat("_AlphaTransitionProgress", 1f);
            material.SetTexture("_AlphaTransitionMap", CreateSolidTexture(Color.white));
            material.SetFloat("_ShadowCasterAlphaAffectedByAlphaTransitionMap", 1);
        });

        Assert.That(affected.ShadowPixels, Is.LessThan(unaffected.ShadowPixels * 0.65f), $"unaffected {unaffected}, affected {affected}");
    }

    [Test]
    public void UberUnlitShadowCasterAlphaCanBeAffectedByLuminanceTransparency()
    {
        var unaffected = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.5f);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0f, 0f, 0f, 1f)));
            material.SetFloat("_LuminanceTransparencyEnabled", 1);
            material.SetFloat("_LuminanceTransparencyProgress", 0.7f);
            material.SetFloat("_LuminanceTransparencySharpness", 0.9f);
            material.SetFloat("_ShadowCasterAlphaAffectedByTransparencyLuminance", 0);
        });
        var affected = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.5f);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0f, 0f, 0f, 1f)));
            material.SetFloat("_LuminanceTransparencyEnabled", 1);
            material.SetFloat("_LuminanceTransparencyProgress", 0.7f);
            material.SetFloat("_LuminanceTransparencySharpness", 0.9f);
            material.SetFloat("_ShadowCasterAlphaAffectedByTransparencyLuminance", 1);
        });

        Assert.That(affected.ShadowPixels, Is.LessThan(unaffected.ShadowPixels * 0.65f), $"unaffected {unaffected}, affected {affected}");
    }

    private static void ConfigureVertexDeformation(Material material)
    {
        material.SetTexture("_VertexDeformationMap", CreateSolidTexture(Color.white));
        material.SetFloat("_VertexDeformationIntensity", 0.45f);
        material.SetFloat("_VertexDeformationBaseValue", 0f);
        material.SetFloat("_VertexDeformationMapChannel", 0f);
    }

    private static ShadowStats RenderShadowStats(Action<Material> configureMaterial)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var planeMaterial = CreatePlaneMaterial();
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInShadowSmokeCamera");
        var lightObject = new GameObject("NovaBuiltInShadowSmokeLight");
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
            configureMaterial(material);
            SetupUberUnlit(material);

            planeObject.name = "NovaBuiltInShadowSmokeReceiver";
            planeObject.transform.position = Vector3.zero;
            planeObject.transform.localScale = new Vector3(2.5f, 1f, 2.5f);
            planeObject.GetComponent<MeshRenderer>().sharedMaterial = planeMaterial;
            planeObject.GetComponent<MeshRenderer>().receiveShadows = true;

            casterObject.name = "NovaBuiltInShadowSmokeCaster";
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

            var previous = RenderTexture.active;
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, RenderSize, RenderSize), 0, 0);
            readback.Apply();
            RenderTexture.active = previous;

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
                    var luminance = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
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

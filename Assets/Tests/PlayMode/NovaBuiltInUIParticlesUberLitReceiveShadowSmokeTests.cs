using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUIParticlesUberLitReceiveShadowSmokeTests
{
    private const string LitShaderName = "Nova/UIParticles/UberLit";
    private const int RenderSize = 160;
    private const int TextureSize = 8;

    [Test]
    public void UIParticlesUberLitReceiveShadowsDarkensShadowedRegion()
    {
        var sample = RenderReceiveShadowScene(material => ConfigureReceiveShadows(material, true));

        AssertReceivesShadow(sample);
    }

    [Test]
    public void UIParticlesUberLitReceiveShadowsCanBeDisabled()
    {
        var enabled = RenderReceiveShadowScene(material => ConfigureReceiveShadows(material, true));
        var disabled = RenderReceiveShadowScene(material => ConfigureReceiveShadows(material, false));

        AssertReceivesShadow(enabled);
        Assert.That(Luminance(disabled.Shadow), Is.GreaterThan(Luminance(enabled.Shadow) + 0.015f), $"enabled {enabled}, disabled {disabled}");
    }

    [Test]
    public void UIParticlesUberLitReceiveShadowsKeywordControlsAttenuation()
    {
        var enabled = RenderReceiveShadowScene(material => ConfigureReceiveShadows(material, true));
        var keywordDisabled = RenderReceiveShadowScene(
            material => ConfigureReceiveShadows(material, true),
            configureAfterSetup: material => material.DisableKeyword("_RECEIVE_SHADOWS_ENABLED"));

        AssertReceivesShadow(enabled);
        Assert.That(Luminance(keywordDisabled.Shadow), Is.GreaterThan(Luminance(enabled.Shadow) + 0.015f), $"enabled {enabled}, keyword disabled {keywordDisabled}");
    }

    [Test]
    public void UIParticlesUberLitReceiveShadowsWorkWithMetallicWorkflow()
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
    public void UIParticlesUberLitReceiveShadowsWorkWithSpecularWorkflow()
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
    public void UIParticlesUberLitReceiveShadowsWorkWithNormalMap()
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
    public void UIParticlesUberLitReceiveShadowsWorkWithTextureArrayOr3DMaps()
    {
        var sample = RenderReceiveShadowScene(material =>
        {
            ConfigureReceiveShadows(material, true);
            ConfigureLitTextureMode(material, 1f, 0.75f, 2f);
            material.SetFloat("_LitWorkflowMode", 1f);
            material.SetFloat("_SpecularHighlights", 1f);
            material.SetTexture("_NormalMap2DArray", CreateTextureArray(new Color(0.5f, 0.5f, 1f, 1f), new Color(0.85f, 0.5f, 1f, 1f)));
            material.SetTexture("_MetallicMap2DArray", CreateTextureArray(Color.black, Color.white));
            material.SetTexture("_SmoothnessMap2DArray", CreateTextureArray(Color.black, Color.white));
        });

        AssertReceivesShadow(sample);
    }

    [Test]
    public void UIParticlesUberLitReceiveShadowsWorkWithAlphaTest()
    {
        var background = new Color(0.02f, 0.05f, 0.75f, 1f);
        var sample = RenderReceiveShadowScene(
            material =>
            {
                ConfigureReceiveShadows(material, true);
                material.SetFloat("_RenderType", 1f);
                material.SetFloat("_Cutoff", 0.5f);
                material.SetTexture("_BaseMap", CreateSplitTexture(new Color(1f, 1f, 1f, 0.2f), new Color(1f, 1f, 1f, 1f)));
            },
            background: background);

        Assert.That(Luminance(sample.Lit), Is.GreaterThan(Luminance(sample.Shadow) + 0.015f), sample.ToString());
        Assert.That(Luminance(sample.Shadow), Is.GreaterThan(Luminance(sample.Clipped) + 0.05f), sample.ToString());
        Assert.That(sample.Clipped.b, Is.GreaterThan(sample.Clipped.r + 0.20f), sample.ToString());
    }

    [Test]
    public void UIParticlesUberLitReceiveShadowsPreserveTintVertexColorAndVertexAlpha()
    {
        var sample = RenderReceiveShadowScene(
            material =>
            {
                ConfigureReceiveShadows(material, true);
                material.SetFloat("_TintAreaMode", 1f);
                material.SetFloat("_TintColorMode", 0f);
                material.SetColor("_TintColor", new Color(0.15f, 1f, 0.15f, 1f));
            },
            mesh: CreateReceiverQuad(new Color(1f, 0.65f, 1f, 0.72f)),
            background: new Color(0.02f, 0.04f, 0.14f, 1f));

        AssertReceivesShadow(sample);
        Assert.That(sample.Shadow.g, Is.GreaterThan(sample.Shadow.r + 0.02f), sample.ToString());
        Assert.That(sample.Lit.g, Is.GreaterThan(sample.Lit.r + 0.08f), sample.ToString());
        Assert.That(sample.Lit.a, Is.LessThan(0.85f), sample.ToString());
    }

    [Test]
    public void UIParticlesUberLitReceiveShadowsRespectRectClip()
    {
        var background = new Color(0.02f, 0.05f, 0.75f, 1f);
        var sample = RenderReceiveShadowScene(
            material =>
            {
                ConfigureReceiveShadows(material, true);
                material.EnableKeyword("UNITY_UI_CLIP_RECT");
                material.SetVector("_ClipRect", new Vector4(-0.2f, -2f, 2f, 2f));
            },
            background: background);

        AssertReceivesShadow(sample);
        Assert.That(sample.Clipped.a, Is.LessThan(0.05f), sample.ToString());
    }

    [Test]
    public void UIParticlesUberLitReceiveShadowsPreserveStencilOrMaskBehavior()
    {
        var background = new Color(0.02f, 0.05f, 0.75f, 1f);
        var sample = RenderReceiveShadowScene(
            material =>
            {
                ConfigureReceiveShadows(material, true);
                material.SetFloat("_StencilComp", (float)CompareFunction.Equal);
                material.SetFloat("_Stencil", 1f);
                material.SetFloat("_StencilOp", (float)StencilOp.Keep);
                material.SetFloat("_StencilReadMask", 255f);
                material.SetFloat("_StencilWriteMask", 255f);
            },
            includeStencilMask: true,
            background: background);

        AssertReceivesShadow(sample);
        Assert.That(sample.RightLit.b, Is.GreaterThan(sample.RightLit.r + 0.20f), sample.ToString());
    }

    [Test]
    public void UIParticlesUberLitReceiveShadowsPreserveColorMask()
    {
        var background = new Color(0.08f, 0.02f, 0.08f, 1f);
        var sample = RenderReceiveShadowScene(
            material =>
            {
                ConfigureReceiveShadows(material, true);
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.1f, 0.1f, 1f, 1f)));
                material.SetFloat("_ColorMask", 2f);
            },
            background: background);

        Assert.That(sample.Lit.b, Is.GreaterThan(sample.Shadow.b + 0.008f), sample.ToString());
        Assert.That(Mathf.Abs(sample.Lit.r - background.r), Is.LessThan(0.03f), sample.ToString());
        Assert.That(Mathf.Abs(sample.Lit.g - background.g), Is.LessThan(0.03f), sample.ToString());
    }

    [Test]
    public void UIParticlesUberLitReceiveShadowsPreserveAmbientContribution()
    {
        var sample = RenderReceiveShadowScene(
            material => ConfigureReceiveShadows(material, true),
            ambient: new Color(0.20f, 0.20f, 0.20f, 1f));

        AssertReceivesShadow(sample);
        Assert.That(Luminance(sample.Shadow), Is.GreaterThan(0.05f), sample.ToString());
    }

    private static void ConfigureReceiveShadows(Material material, bool enabled)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.82f, 0.82f, 0.82f, 1f)));
        material.SetFloat("_LitReceiveShadows", enabled ? 1f : 0f);
        material.SetFloat("_EnvironmentReflections", 0f);
    }

    private static RenderSample RenderReceiveShadowScene(
        Action<Material> configureMaterial,
        Color? ambient = null,
        Mesh mesh = null,
        Color? background = null,
        Action<Material> configureAfterSetup = null,
        bool includeStencilMask = false)
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
        var maskMaterial = includeStencilMask ? new Material(shader) : null;
        var casterMaterial = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
        var receiverMesh = mesh ?? CreateReceiverQuad(Color.white);
        var maskMesh = includeStencilMask ? CreateReceiverQuad(Color.white, new Rect(-0.2f, -1.7f, 1.9f, 3.4f)) : null;
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUIParticlesUberLitShadowCamera");
        var receiverObject = new GameObject("NovaBuiltInUIParticlesUberLitShadowReceiver");
        var maskObject = includeStencilMask ? new GameObject("NovaBuiltInUIParticlesUberLitStencilMask") : null;
        var casterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var lightObject = new GameObject("NovaBuiltInUIParticlesUberLitShadowLight");
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

            if (maskMaterial != null)
            {
                maskMaterial.SetFloat("_RenderType", 0f);
                maskMaterial.SetFloat("_Cull", (float)CullMode.Off);
                maskMaterial.SetFloat("_ZTest", (float)CompareFunction.Always);
                maskMaterial.SetFloat("_StencilComp", (float)CompareFunction.Always);
                maskMaterial.SetFloat("_Stencil", 1f);
                maskMaterial.SetFloat("_StencilOp", (float)StencilOp.Replace);
                maskMaterial.SetFloat("_StencilReadMask", 255f);
                maskMaterial.SetFloat("_StencilWriteMask", 255f);
                maskMaterial.SetFloat("_ColorMask", 0f);
                SetupUberLit(maskMaterial);
                maskMaterial.renderQueue = (int)RenderQueue.Geometry - 20;

                maskObject.AddComponent<MeshFilter>().sharedMesh = maskMesh;
                var maskRenderer = maskObject.AddComponent<MeshRenderer>();
                maskRenderer.sharedMaterial = maskMaterial;
                maskRenderer.shadowCastingMode = ShadowCastingMode.Off;
                maskRenderer.receiveShadows = false;
            }

            if (casterMaterial.HasProperty("_Color"))
            {
                casterMaterial.SetColor("_Color", actualBackground);
            }

            receiverObject.AddComponent<MeshFilter>().sharedMesh = receiverMesh;
            var receiverRenderer = receiverObject.AddComponent<MeshRenderer>();
            receiverRenderer.sharedMaterial = material;
            receiverRenderer.receiveShadows = true;
            receiverRenderer.shadowCastingMode = ShadowCastingMode.Off;

            casterObject.name = "NovaBuiltInUIParticlesUberLitHiddenShadowCaster";
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
            camera.backgroundColor = actualBackground;
            camera.depthTextureMode = DepthTextureMode.Depth;
            camera.targetTexture = target;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.Render();

            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, RenderSize, RenderSize), 0, 0);
            readback.Apply();
            RenderTexture.active = previousActive;

            return new RenderSample(
                AverageRegion(readback, 0.76f, 0.86f, 0.46f, 0.54f),
                AverageRegion(readback, 0.54f, 0.64f, 0.46f, 0.54f),
                AverageRegion(readback, 0.18f, 0.28f, 0.46f, 0.54f),
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
            if (maskObject != null)
            {
                UnityEngine.Object.DestroyImmediate(maskObject);
            }

            UnityEngine.Object.DestroyImmediate(casterObject);
            UnityEngine.Object.DestroyImmediate(lightObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(receiverMesh);
            if (maskMesh != null)
            {
                UnityEngine.Object.DestroyImmediate(maskMesh);
            }

            DestroyMaterialAndOwnedTextures(material);
            if (maskMaterial != null)
            {
                DestroyMaterialAndOwnedTextures(maskMaterial);
            }

            UnityEngine.Object.DestroyImmediate(casterMaterial);
        }
    }

    private static void AssertReceivesShadow(RenderSample sample)
    {
        Assert.That(Luminance(sample.Lit), Is.GreaterThan(Luminance(sample.Shadow) + 0.008f), sample.ToString());
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

    private static Texture2DArray CreateTextureArray(params Color[] slices)
    {
        var texture = new Texture2DArray(TextureSize, TextureSize, slices.Length, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };

        for (var slice = 0; slice < slices.Length; slice++)
        {
            texture.SetPixels(Enumerable.Repeat(slices[slice], TextureSize * TextureSize).ToArray(), slice);
        }

        texture.Apply();
        return texture;
    }

    private static void ConfigureLitTextureMode(Material material, float baseMapMode, float progress, float sliceCount)
    {
        material.SetFloat("_BaseMapMode", baseMapMode);
        material.SetFloat("_BaseMapProgress", progress);
        material.SetFloat("_BaseMapSliceCount", sliceCount);
    }

    private static Mesh CreateReceiverQuad(Color vertexColor)
    {
        return CreateReceiverQuad(vertexColor, new Rect(-1.7f, -1.7f, 3.4f, 3.4f));
    }

    private static Mesh CreateReceiverQuad(Color vertexColor, Rect rect)
    {
        var mesh = new Mesh { name = "NovaBuiltInUIParticlesUberLitShadowReceiverQuad" };
        mesh.vertices = new[]
        {
            new Vector3(rect.xMin, 0f, rect.yMin),
            new Vector3(rect.xMax, 0f, rect.yMin),
            new Vector3(rect.xMax, 0f, rect.yMax),
            new Vector3(rect.xMin, 0f, rect.yMax),
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
        };
        mesh.colors = Enumerable.Repeat(vertexColor, 4).ToArray();
        mesh.SetUVs(1, Enumerable.Repeat(Vector4.zero, 4).ToList());
        mesh.SetUVs(2, Enumerable.Repeat(Vector4.zero, 4).ToList());
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
        var textures = material.GetTexturePropertyNames()
            .Select(material.GetTexture)
            .Where(texture => texture != null)
            .Distinct()
            .ToArray();

        foreach (var texture in textures)
        {
            UnityEngine.Object.DestroyImmediate(texture);
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

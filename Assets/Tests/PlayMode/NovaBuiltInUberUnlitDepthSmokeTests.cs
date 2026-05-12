using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberUnlitDepthSmokeTests
{
    private const string ShaderName = "Nova/Particles/UberUnlit";
    private const int TextureSize = 4;
    private const int RenderSize = 64;

    [Test]
    public void UberUnlitSoftParticlesRequireCameraDepthTexture()
    {
        var result = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        });

        Assert.That(result.CameraDepthTextureMode.HasFlag(DepthTextureMode.Depth), Is.True);
        Assert.That(result.Color.r + result.Color.g + result.Color.b, Is.GreaterThan(0.05f), result.Color.ToString());
    }

    [Test]
    public void UberUnlitSoftParticlesFadeNearSceneDepth()
    {
        var disabled = RenderDepthScene(material => ConfigureTransparentDepthMaterial(material), opaqueZ: 0.08f).Color;
        var enabled = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        }, opaqueZ: 0.08f).Color;

        Assert.That(disabled.r, Is.GreaterThan(enabled.r + 0.25f), $"Disabled {disabled}, enabled {enabled}");
        Assert.That(enabled.b, Is.GreaterThan(disabled.b + 0.15f), $"Disabled {disabled}, enabled {enabled}");
    }

    [Test]
    public void UberUnlitSoftParticlesIntensityAffectsOutput()
    {
        var low = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 0.05f);
        }, opaqueZ: 0.25f).Color;
        var high = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        }, opaqueZ: 0.25f).Color;

        Assert.That(low.r, Is.GreaterThan(high.r + 0.20f), $"Low {low}, high {high}");
        Assert.That(high.b, Is.GreaterThan(low.b + 0.10f), $"Low {low}, high {high}");
    }

    [Test]
    public void UberUnlitSoftParticlesDoNotAffectWhenDepthFar()
    {
        var disabled = RenderDepthScene(material => ConfigureTransparentDepthMaterial(material), opaqueZ: 4f).Color;
        var enabled = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        }, opaqueZ: 4f).Color;

        Assert.That(Mathf.Abs(disabled.r - enabled.r), Is.LessThan(0.08f), $"Disabled {disabled}, enabled {enabled}");
        Assert.That(Mathf.Abs(disabled.b - enabled.b), Is.LessThan(0.08f), $"Disabled {disabled}, enabled {enabled}");
    }

    [Test]
    public void UberUnlitDepthFadeAffectsOutput()
    {
        var disabled = RenderDepthScene(material => ConfigureTransparentDepthMaterial(material)).Color;
        var enabled = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            ConfigureDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
        }).Color;

        Assert.That(disabled.r, Is.GreaterThan(enabled.r + 0.25f), $"Disabled {disabled}, enabled {enabled}");
    }

    [Test]
    public void UberUnlitDepthFadeNearFarAffectOutput()
    {
        var wideRange = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            ConfigureDepthFade(material, near: 0.5f, far: 5f, width: 0.5f);
        }).Color;
        var nearEdge = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            ConfigureDepthFade(material, near: 1.9f, far: 5f, width: 0.5f);
        }).Color;

        Assert.That(wideRange.r, Is.GreaterThan(nearEdge.r + 0.20f), $"Wide {wideRange}, near edge {nearEdge}");
    }

    [Test]
    public void UberUnlitDepthFadeWidthAffectsFalloff()
    {
        var narrow = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            ConfigureDepthFade(material, near: 1.9f, far: 3f, width: 0.05f);
        }).Color;
        var wide = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            ConfigureDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
        }).Color;

        Assert.That(narrow.r, Is.GreaterThan(wide.r + 0.20f), $"Narrow {narrow}, wide {wide}");
    }

    [Test]
    public void UberUnlitDepthFadeDoesNotBreakBaseAlpha()
    {
        var disabled = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0f, 0f, 0.5f)));
        }).Color;
        var enabled = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0f, 0f, 0.5f)));
            ConfigureDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
        }).Color;

        Assert.That(disabled.r, Is.GreaterThan(enabled.r + 0.08f), $"Disabled {disabled}, enabled {enabled}");
        Assert.That(enabled.r, Is.LessThan(0.8f), enabled.ToString());
    }

    [Test]
    public void UberUnlitSoftParticlesAndDepthFadeCanCombine()
    {
        var disabled = RenderDepthScene(material => ConfigureTransparentDepthMaterial(material), opaqueZ: 0.25f).Color;
        var softOnly = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        }, opaqueZ: 0.25f).Color;
        var depthOnly = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            ConfigureDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
        }, opaqueZ: 0.25f).Color;
        var combined = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
            ConfigureDepthFade(material, near: 1.9f, far: 3f, width: 0.5f);
        }, opaqueZ: 0.25f).Color;

        Assert.That(disabled.r, Is.GreaterThan(combined.r + 0.30f), $"Disabled {disabled}, combined {combined}");
        Assert.That(softOnly.r, Is.GreaterThan(combined.r + 0.08f), $"Soft {softOnly}, combined {combined}");
        Assert.That(depthOnly.r, Is.GreaterThan(combined.r + 0.08f), $"Depth {depthOnly}, combined {combined}");
    }

    [Test]
    public void UberUnlitDepthFeaturesPreserveAlphaTransition()
    {
        var visible = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            ConfigureAlphaTransition(material, progress: 0f);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 0.05f);
        }, opaqueZ: 4f).Color;
        var faded = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            ConfigureAlphaTransition(material, progress: 1f);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 0.05f);
        }, opaqueZ: 4f).Color;

        Assert.That(visible.r, Is.GreaterThan(faded.r + 0.25f), $"Visible {visible}, faded {faded}");
    }

    [Test]
    public void UberUnlitDepthFeaturesPreserveEmissionAndTransparency()
    {
        var withoutDepth = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            ConfigureEmissionAndTransparency(material);
        }, opaqueZ: 0.25f).Color;
        var withDepth = RenderDepthScene(material =>
        {
            ConfigureTransparentDepthMaterial(material);
            ConfigureEmissionAndTransparency(material);
            material.SetFloat("_SoftParticlesEnabled", 1f);
            material.SetFloat("_SoftParticlesIntensity", 1f);
        }, opaqueZ: 0.25f).Color;

        Assert.That(withoutDepth.g, Is.GreaterThan(0.10f), withoutDepth.ToString());
        Assert.That(withDepth.g, Is.GreaterThan(0.03f), withDepth.ToString());
        Assert.That(withoutDepth.g, Is.GreaterThan(withDepth.g + 0.05f), $"Without {withoutDepth}, with {withDepth}");
    }

    private static void ConfigureTransparentDepthMaterial(Material material)
    {
        material.SetFloat("_RenderType", 2);
        material.SetFloat("_TransparentBlendMode", 0);
        material.SetFloat("_Cull", (float)CullMode.Off);
        material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 0f, 0f, 1f)));
        material.SetFloat("_BaseMapMode", 0);
    }

    private static void ConfigureDepthFade(Material material, float near, float far, float width)
    {
        material.SetFloat("_DepthFadeEnabled", 1f);
        material.SetFloat("_DepthFadeNear", near);
        material.SetFloat("_DepthFadeFar", far);
        material.SetFloat("_DepthFadeWidth", width);
    }

    private static void ConfigureAlphaTransition(Material material, float progress)
    {
        material.SetFloat("_AlphaTransitionMode", 1);
        material.SetFloat("_AlphaTransitionMapMode", 0);
        material.SetFloat("_AlphaTransitionProgress", progress);
        material.SetTexture("_AlphaTransitionMap", CreateSolidTexture(Color.white));
    }

    private static void ConfigureEmissionAndTransparency(Material material)
    {
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.25f, 0.25f, 0.25f, 1f)));
        material.SetFloat("_EmissionAreaType", 1);
        material.SetFloat("_EmissionColorType", 0);
        material.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
        material.SetFloat("_EmissionIntensity", 1f);
        material.SetFloat("_LuminanceTransparencyEnabled", 1f);
        material.SetFloat("_LuminanceTransparencyProgress", 0.1f);
        material.SetFloat("_LuminanceTransparencySharpness", 0f);
    }

    private static DepthRenderResult RenderDepthScene(Action<Material> configureMaterial, float opaqueZ = 0.08f)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var particleMesh = CreateQuad("NovaBuiltInDepthParticleQuad");
        var opaqueMesh = CreateQuad("NovaBuiltInDepthOpaqueQuad");
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberUnlitDepthCamera");
        var particleObject = new GameObject("NovaBuiltInUberUnlitDepthParticle");
        var opaqueObject = new GameObject("NovaBuiltInUberUnlitDepthOpaque");
        var ownedObjects = new List<UnityEngine.Object>();
        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;

            configureMaterial(material);
            SetupUberUnlit(material);

            var particleFilter = particleObject.AddComponent<MeshFilter>();
            particleFilter.sharedMesh = particleMesh;
            var particleRenderer = particleObject.AddComponent<MeshRenderer>();
            particleRenderer.sharedMaterial = material;

            opaqueObject.transform.position = new Vector3(0f, 0f, opaqueZ);
            var opaqueFilter = opaqueObject.AddComponent<MeshFilter>();
            opaqueFilter.sharedMesh = opaqueMesh;
            var opaqueRenderer = opaqueObject.AddComponent<MeshRenderer>();
            opaqueRenderer.sharedMaterial = CreateOpaqueMaterial(new Color(0f, 0f, 1f, 1f));
            ownedObjects.Add(opaqueRenderer.sharedMaterial);

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

            var previous = RenderTexture.active;
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, RenderSize, RenderSize), 0, 0);
            readback.Apply();
            RenderTexture.active = previous;

            var color = readback.GetPixel(RenderSize / 2, RenderSize / 2);
            return new DepthRenderResult(color, camera.depthTextureMode);
        }
        finally
        {
            RenderSettings.ambientMode = previousAmbientMode;
            RenderSettings.ambientLight = previousAmbientLight;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(particleObject);
            UnityEngine.Object.DestroyImmediate(opaqueObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(particleMesh);
            UnityEngine.Object.DestroyImmediate(opaqueMesh);
            UnityEngine.Object.DestroyImmediate(material);
            foreach (var ownedObject in ownedObjects)
            {
                UnityEngine.Object.DestroyImmediate(ownedObject);
            }
        }
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

    private static Texture2D CreateSolidTexture(Color color)
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(Enumerable.Repeat(color, TextureSize * TextureSize).ToArray());
        texture.Apply();
        return texture;
    }

    private static Mesh CreateQuad(string name)
    {
        var mesh = new Mesh { name = name };
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
        mesh.SetUVs(1, Enumerable.Repeat(Vector4.zero, 4).ToList());
        mesh.SetUVs(2, Enumerable.Repeat(Vector4.zero, 4).ToList());
        mesh.normals = Enumerable.Repeat(Vector3.back, 4).ToArray();
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

    private readonly struct DepthRenderResult
    {
        public DepthRenderResult(Color color, DepthTextureMode cameraDepthTextureMode)
        {
            Color = color;
            CameraDepthTextureMode = cameraDepthTextureMode;
        }

        public Color Color { get; }

        public DepthTextureMode CameraDepthTextureMode { get; }
    }
}

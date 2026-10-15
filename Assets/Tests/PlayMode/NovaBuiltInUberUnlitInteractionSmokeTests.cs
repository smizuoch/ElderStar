using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInUberUnlitInteractionSmokeTests
{
    private const string ShaderName = "Nova/Particles/UberUnlit";
    private const int RenderSize = 96;
    private const int TextureSize = 4;

    [Test]
    public void UberUnlitComplexTextureTintFlowColorCorrectionInteraction()
    {
        var simple = RenderColor(material =>
        {
            material.SetFloat("_BaseMapMode", 0);
            material.SetTexture("_BaseMap", CreateSolidTexture(Color.white));
        });

        var complex = RenderColor(material =>
        {
            var baseMap = CreateTexture3D(Color.white, Color.white, Color.white);
            var tintMap = CreateHorizontalSplitTexture(new Color(0.9f, 0.05f, 0.05f, 1f), new Color(0.05f, 0.9f, 0.05f, 1f), TextureSize - 1);
            var flowMap = CreateSolidTexture(new Color(1f, 0.5f, 0.5f, 1f));
            var gradientMap = CreateHorizontalSplitTexture(new Color(0.9f, 0.05f, 0.05f, 1f), new Color(0.05f, 0.05f, 0.9f, 1f), TextureSize / 2);
            material.SetTexture("_BaseMap3D", baseMap);
            material.SetFloat("_BaseMapMode", 2);
            material.SetFloat("_BaseMapProgress", 0.45f);
            material.SetFloat("_BaseMapSliceCount", baseMap.depth);
            material.SetFloat("_TintAreaMode", 1);
            material.SetFloat("_TintColorMode", 1);
            material.SetTexture("_TintMap", tintMap);
            material.SetFloat("_TintBlendRate", 1);
            material.SetTexture("_FlowMap", flowMap);
            material.SetFloat("_FlowMapTarget", 2);
            material.SetFloat("_FlowMapChannelsX", 0);
            material.SetFloat("_FlowMapChannelsY", 1);
            material.SetFloat("_FlowIntensity", 0.4f);
            material.SetFloat("_ColorCorrectionMode", 2);
            material.SetTexture("_GradientMap", gradientMap);
        });

        Assert.That(complex.b, Is.GreaterThan(0.18f), complex.ToString());
        Assert.That(complex.b, Is.GreaterThan(complex.r + 0.08f), complex.ToString());
        Assert.That(ColorDistance(simple, complex), Is.GreaterThan(0.25f), $"simple {simple}, complex {complex}");
    }

    [Test]
    public void UberUnlitAlphaTransitionEmissionTransparencyInteraction()
    {
        var background = new Color(0.02f, 0.04f, 0.75f, 1f);
        var visible = RenderAlphaEmissionTransparency(progress: 0f, background);
        var transparent = RenderAlphaEmissionTransparency(progress: 1f, background);

        Assert.That(visible.r + visible.g, Is.GreaterThan(transparent.r + transparent.g + 0.10f), $"visible {visible}, transparent {transparent}");
        Assert.That(transparent.b, Is.GreaterThan(visible.b + 0.20f), $"visible {visible}, transparent {transparent}");
        Assert.That(transparent.g, Is.LessThan(0.12f), transparent.ToString());
    }

    [Test]
    public void UberUnlitDepthFeaturesWithEmissionAndTransparencyInteraction()
    {
        var withoutDepth = RenderDepthInteraction(useDepthFeatures: false);
        var withDepth = RenderDepthInteraction(useDepthFeatures: true);

        Assert.That(withoutDepth.g, Is.GreaterThan(withDepth.g + 0.05f), $"without {withoutDepth}, with {withDepth}");
        Assert.That(withDepth.g, Is.GreaterThan(0.025f), withDepth.ToString());
        Assert.That(withDepth.b, Is.GreaterThan(0.10f), withDepth.ToString());
    }

    [Test]
    public void UberUnlitVertexDeformationWithDepthAndAlphaInteraction()
    {
        var undeformed = RenderForwardStats(material => ConfigureDepthAlphaDeformationInteraction(material, deform: false, cutoff: 0.2f));
        var deformed = RenderForwardStats(material => ConfigureDepthAlphaDeformationInteraction(material, deform: true, cutoff: 0.2f));
        var clipped = RenderForwardStats(material => ConfigureDepthAlphaDeformationInteraction(material, deform: true, cutoff: 0.9f));

        Assert.That(deformed.CenterX, Is.GreaterThan(undeformed.CenterX + 8), $"undeformed {undeformed}, deformed {deformed}");
        Assert.That(clipped.LitPixels, Is.LessThan(deformed.LitPixels * 0.35f), $"deformed {deformed}, clipped {clipped}");
    }

    [Test]
    public void UberUnlitShadowCasterWithAlphaTransitionAndDeformationInteraction()
    {
        var baseline = RenderShadowStats(material => material.SetFloat("_ShadowCasterEnabled", 1));
        var deformed = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            ConfigureShadowDeformation(material);
        });
        var faded = RenderShadowStats(material =>
        {
            material.SetFloat("_ShadowCasterEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
            material.SetFloat("_ShadowCasterAlphaCutoff", 0.5f);
            material.SetFloat("_ShadowCasterAlphaAffectedByAlphaTransitionMap", 1);
            ConfigureShadowDeformation(material);
            ConfigureFadeAlphaTransition(material, progress: 1f);
        });

        Assert.That(Mathf.Abs(deformed.ShadowPixels - baseline.ShadowPixels), Is.GreaterThan(50), $"baseline {baseline}, deformed {deformed}");
        Assert.That(faded.ShadowPixels, Is.LessThan(deformed.ShadowPixels * 0.65f), $"deformed {deformed}, faded {faded}");
    }

    [Test]
    public void UberUnlitDepthOnlyWithAlphaAndDeformationInteraction()
    {
        var undeformed = RenderDepthOnlyStats(deform: false, cutoff: 0.1f);
        var deformed = RenderDepthOnlyStats(deform: true, cutoff: 0.1f);
        var clipped = RenderDepthOnlyStats(deform: true, cutoff: 0.9f);

        Assert.That(undeformed.LeftGreen, Is.GreaterThan(deformed.LeftGreen + 0.45f), $"undeformed {undeformed}, deformed {deformed}");
        Assert.That(deformed.RightGreen, Is.GreaterThan(0.45f), deformed.ToString());
        Assert.That(clipped.RightGreen, Is.LessThan(deformed.RightGreen - 0.45f), $"deformed {deformed}, clipped {clipped}");
    }

    private static Color RenderAlphaEmissionTransparency(float progress, Color background)
    {
        return RenderColor(
            material =>
            {
                material.SetFloat("_RenderType", 2);
                material.SetFloat("_TransparentBlendMode", 0);
                material.SetFloat("_BaseMapMode", 0);
                material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.3f, 0.3f, 0.3f, 1f)));
                ConfigureFadeAlphaTransition(material, progress);
                material.SetFloat("_EmissionAreaType", 3);
                material.SetFloat("_EmissionColorType", 0);
                material.SetColor("_EmissionColor", new Color(0f, 0.95f, 0f, 1f));
                material.SetFloat("_EmissionIntensity", 1.2f);
                material.SetFloat("_KeepEdgeTransparency", 1f);
                material.SetFloat("_LuminanceTransparencyEnabled", 1f);
                material.SetFloat("_LuminanceTransparencyProgress", 0.05f);
                material.SetFloat("_LuminanceTransparencySharpness", 0f);
            },
            background: background);
    }

    private static Color RenderDepthInteraction(bool useDepthFeatures)
    {
        return RenderDepthScene(material =>
        {
            material.SetFloat("_RenderType", 2);
            material.SetFloat("_TransparentBlendMode", 0);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.18f, 0.18f, 0.18f, 1f)));
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_EmissionAreaType", 1);
            material.SetFloat("_EmissionColorType", 0);
            material.SetColor("_EmissionColor", new Color(0f, 0.9f, 0f, 1f));
            material.SetFloat("_EmissionIntensity", 0.8f);
            material.SetFloat("_LuminanceTransparencyEnabled", 1);
            material.SetFloat("_LuminanceTransparencyProgress", 0.08f);
            material.SetFloat("_LuminanceTransparencySharpness", 0f);
            if (useDepthFeatures)
            {
                material.SetFloat("_SoftParticlesEnabled", 1);
                material.SetFloat("_SoftParticlesIntensity", 1);
                material.SetFloat("_DepthFadeEnabled", 1);
                material.SetFloat("_DepthFadeNear", 1.8f);
                material.SetFloat("_DepthFadeFar", 3f);
                material.SetFloat("_DepthFadeWidth", 0.5f);
            }
        });
    }

    private static void ConfigureDepthAlphaDeformationInteraction(Material material, bool deform, float cutoff)
    {
        material.SetFloat("_RenderType", 1);
        material.SetFloat("_Cutoff", cutoff);
        material.SetFloat("_DepthFadeEnabled", 1);
        material.SetFloat("_DepthFadeNear", 0.5f);
        material.SetFloat("_DepthFadeFar", 5f);
        material.SetFloat("_DepthFadeWidth", 0.25f);
        material.SetTexture("_BaseMap", CreateSolidTexture(new Color(0.9f, 0.05f, 0.05f, 0.55f)));
        material.SetFloat("_BaseMapMode", 0);
        if (deform)
        {
            material.SetTexture("_VertexDeformationMap", CreateSolidTexture(Color.white));
            material.SetFloat("_VertexDeformationIntensity", 0.42f);
            material.SetFloat("_VertexDeformationBaseValue", 0f);
            material.SetFloat("_VertexDeformationMapChannel", 0f);
        }
    }

    private static void ConfigureShadowDeformation(Material material)
    {
        material.SetTexture("_VertexDeformationMap", CreateSolidTexture(Color.white));
        material.SetFloat("_VertexDeformationIntensity", 0.42f);
        material.SetFloat("_VertexDeformationBaseValue", 0f);
        material.SetFloat("_VertexDeformationMapChannel", 0f);
        material.SetFloat("_ShadowCasterApplyVertexDeformation", 1);
    }

    private static void ConfigureFadeAlphaTransition(Material material, float progress)
    {
        material.SetFloat("_AlphaTransitionMode", 1);
        material.SetFloat("_AlphaTransitionMapMode", 0);
        material.SetFloat("_AlphaTransitionProgress", progress);
        material.SetTexture("_AlphaTransitionMap", CreateSolidTexture(Color.white));
    }

    private static Color RenderColor(Action<Material> configureMaterial, Mesh mesh = null, Color? background = null)
    {
        var samples = RenderSamples(configureMaterial, new[] { new Vector2(0.5f, 0.5f) }, mesh, background);
        return samples[0];
    }

    private static Color[] RenderSamples(Action<Material> configureMaterial, IReadOnlyList<Vector2> samplePositions, Mesh mesh = null, Color? background = null)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var actualMesh = mesh ?? CreateQuad(Vector3.back);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberUnlitInteractionCamera");
        var meshObject = new GameObject("NovaBuiltInUberUnlitInteractionMesh");

        try
        {
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            material.SetFloat("_RenderType", 0);
            configureMaterial(material);
            SetupUberUnlit(material);

            meshObject.AddComponent<MeshFilter>().sharedMesh = actualMesh;
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            var camera = cameraObject.AddComponent<Camera>();
            ConfigureCamera(camera, target, background ?? new Color(0.01f, 0.01f, 0.01f, 1f));
            NovaDepthFadeTestCamera.UseEyeDistanceRanges(camera, new[] { material });
            camera.Render();
            Readback(target, readback);

            return samplePositions.Select(position => Sample(readback, position)).ToArray();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(meshObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(actualMesh);
            UnityEngine.Object.DestroyImmediate(material);
        }
    }

    private static Color RenderDepthScene(Action<Material> configureMaterial)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var particleMesh = CreateQuad(Vector3.back);
        var opaqueMesh = CreateQuad(Vector3.back);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberUnlitInteractionDepthCamera");
        var particleObject = new GameObject("NovaBuiltInUberUnlitInteractionDepthParticle");
        var opaqueObject = new GameObject("NovaBuiltInUberUnlitInteractionDepthOpaque");
        var previousAmbientMode = RenderSettings.ambientMode;
        var previousAmbientLight = RenderSettings.ambientLight;
        Material opaqueMaterial = null;

        try
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;

            configureMaterial(material);
            SetupUberUnlit(material);

            particleObject.AddComponent<MeshFilter>().sharedMesh = particleMesh;
            particleObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            opaqueObject.transform.position = new Vector3(0f, 0f, 0.08f);
            opaqueObject.AddComponent<MeshFilter>().sharedMesh = opaqueMesh;
            opaqueMaterial = CreateStandardMaterial(new Color(0f, 0f, 1f, 1f));
            opaqueObject.AddComponent<MeshRenderer>().sharedMaterial = opaqueMaterial;

            var camera = cameraObject.AddComponent<Camera>();
            ConfigureCamera(camera, target, Color.black);
            camera.depthTextureMode |= DepthTextureMode.Depth;
            NovaDepthFadeTestCamera.UseEyeDistanceRanges(camera, new[] { material });
            camera.Render();
            Readback(target, readback);

            return readback.GetPixel(RenderSize / 2, RenderSize / 2);
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
            UnityEngine.Object.DestroyImmediate(opaqueMaterial);
        }
    }

    private static RenderStats RenderForwardStats(Action<Material> configureMaterial)
    {
        var mesh = CreateQuad(Vector3.right);
        var readback = RenderTextureToTexture(material =>
        {
            configureMaterial(material);
        }, mesh);
        try
        {
            return RenderStats.From(readback);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(readback);
        }
    }

    private static Texture2D RenderTextureToTexture(Action<Material> configureMaterial, Mesh mesh)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberUnlitInteractionStatsCamera");
        var meshObject = new GameObject("NovaBuiltInUberUnlitInteractionStatsMesh");

        try
        {
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            configureMaterial(material);
            SetupUberUnlit(material);

            meshObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;

            var camera = cameraObject.AddComponent<Camera>();
            ConfigureCamera(camera, target, Color.black);
            NovaDepthFadeTestCamera.UseEyeDistanceRanges(camera, new[] { material });
            camera.Render();
            Readback(target, readback);
            return readback;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(meshObject);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(mesh);
            UnityEngine.Object.DestroyImmediate(material);
        }
    }

    private static ShadowStats RenderShadowStats(Action<Material> configureMaterial)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        var material = new Material(shader);
        var planeMaterial = CreateStandardMaterial(new Color(0.76f, 0.76f, 0.76f, 1f));
        var target = new RenderTexture(128, 128, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberUnlitInteractionShadowCamera");
        var lightObject = new GameObject("NovaBuiltInUberUnlitInteractionShadowLight");
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
            Readback(target, readback);

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

    private static DepthOnlyStats RenderDepthOnlyStats(bool deform, float cutoff)
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);
        var probeShader = Shader.Find("Hidden/NovaBuiltIn/Tests/DepthProbe");
        Assert.That(probeShader, Is.Not.Null, "Hidden/NovaBuiltIn/Tests/DepthProbe");

        var depthMaterial = new Material(shader);
        var probeMaterial = new Material(probeShader);
        var mesh = CreateQuad(Vector3.right);
        var probeMesh = CreateQuad(Vector3.back);
        var target = new RenderTexture(RenderSize, RenderSize, 24, RenderTextureFormat.ARGB32);
        var readback = new Texture2D(RenderSize, RenderSize, TextureFormat.RGBA32, false);
        var cameraObject = new GameObject("NovaBuiltInUberUnlitInteractionDepthOnlyCamera");
        var commandBuffer = new CommandBuffer { name = "NovaBuiltInUberUnlitInteractionDepthOnly" };

        try
        {
            var depthPass = depthMaterial.FindPass("DepthOnly");
            Assert.That(depthPass, Is.GreaterThanOrEqualTo(0), "DepthOnly pass should exist.");

            depthMaterial.SetFloat("_Cull", (float)CullMode.Off);
            depthMaterial.SetFloat("_RenderType", 1);
            depthMaterial.SetFloat("_Cutoff", cutoff);
            depthMaterial.SetTexture("_BaseMap", CreateSolidTexture(new Color(1f, 1f, 1f, 0.4f)));
            if (deform)
            {
                depthMaterial.SetTexture("_VertexDeformationMap", CreateSolidTexture(Color.white));
                depthMaterial.SetFloat("_VertexDeformationIntensity", 0.45f);
                depthMaterial.SetFloat("_VertexDeformationBaseValue", 0f);
                depthMaterial.SetFloat("_VertexDeformationMapChannel", 0f);
            }

            SetupUberUnlit(depthMaterial);

            var camera = cameraObject.AddComponent<Camera>();
            ConfigureCamera(camera, target, Color.black);
            commandBuffer.DrawMesh(mesh, Matrix4x4.identity, depthMaterial, 0, depthPass);
            commandBuffer.DrawMesh(probeMesh, Matrix4x4.TRS(new Vector3(0f, 0f, 0.2f), Quaternion.identity, Vector3.one), probeMaterial);
            camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
            camera.Render();
            Readback(target, readback);

            return new DepthOnlyStats(
                Sample(readback, new Vector2(0.18f, 0.5f)).g,
                Sample(readback, new Vector2(0.82f, 0.5f)).g);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(readback);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(mesh);
            UnityEngine.Object.DestroyImmediate(probeMesh);
            UnityEngine.Object.DestroyImmediate(depthMaterial);
            UnityEngine.Object.DestroyImmediate(probeMaterial);
            commandBuffer.Release();
        }
    }

    private static Mesh CreateQuad(Vector3 normal)
    {
        var mesh = new Mesh { name = "NovaBuiltInUberUnlitInteractionQuad" };
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
        mesh.normals = Enumerable.Repeat(normal.normalized, 4).ToArray();
        mesh.tangents = Enumerable.Repeat(new Vector4(1f, 0f, 0f, 1f), 4).ToArray();
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateBounds();
        return mesh;
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

    private static Texture2D CreateHorizontalSplitTexture(Color leftColor, Color rightColor, int rightColorStartX)
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        for (var y = 0; y < TextureSize; y++)
        {
            for (var x = 0; x < TextureSize; x++)
            {
                texture.SetPixel(x, y, x >= rightColorStartX ? rightColor : leftColor);
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture3D CreateTexture3D(params Color[] depthColors)
    {
        var texture = new Texture3D(TextureSize, TextureSize, depthColors.Length, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[TextureSize * TextureSize * depthColors.Length];
        for (var z = 0; z < depthColors.Length; z++)
        {
            for (var y = 0; y < TextureSize; y++)
            {
                for (var x = 0; x < TextureSize; x++)
                {
                    pixels[x + TextureSize * (y + TextureSize * z)] = depthColors[z];
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private static Material CreateStandardMaterial(Color color)
    {
        var shader = Shader.Find("Standard");
        Assert.That(shader, Is.Not.Null, "Standard");
        var material = new Material(shader);
        material.SetColor("_Color", color);
        return material;
    }

    private static void ConfigureCamera(Camera camera, RenderTexture target, Color background)
    {
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = background;
        camera.orthographic = true;
        camera.orthographicSize = 0.75f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 10f;
        camera.transform.position = new Vector3(0f, 0f, -2f);
        camera.targetTexture = target;
    }

    private static void Readback(RenderTexture target, Texture2D readback)
    {
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        readback.ReadPixels(new Rect(0, 0, readback.width, readback.height), 0, 0);
        readback.Apply();
        RenderTexture.active = previous;
    }

    private static Color Sample(Texture2D texture, Vector2 normalizedPosition)
    {
        var x = Mathf.Clamp(Mathf.RoundToInt(normalizedPosition.x * (texture.width - 1)), 0, texture.width - 1);
        var y = Mathf.Clamp(Mathf.RoundToInt(normalizedPosition.y * (texture.height - 1)), 0, texture.height - 1);
        return texture.GetPixel(x, y);
    }

    private static float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
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
        private RenderStats(int minX, int maxX, int litPixels)
        {
            MinX = minX;
            MaxX = maxX;
            LitPixels = litPixels;
        }

        public int MinX { get; }

        public int MaxX { get; }

        public int LitPixels { get; }

        public float CenterX => (MinX + MaxX) * 0.5f;

        public static RenderStats From(Texture2D texture)
        {
            var minX = texture.width;
            var maxX = -1;
            var litPixels = 0;
            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var color = texture.GetPixel(x, y);
                    if (color.r + color.g + color.b <= 0.15f)
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    litPixels++;
                }
            }

            if (litPixels == 0)
            {
                return new RenderStats(texture.width, -1, 0);
            }

            return new RenderStats(minX, maxX, litPixels);
        }

        public override string ToString()
        {
            return $"MinX={MinX}, MaxX={MaxX}, CenterX={CenterX}, LitPixels={LitPixels}";
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

    private readonly struct DepthOnlyStats
    {
        public DepthOnlyStats(float leftGreen, float rightGreen)
        {
            LeftGreen = leftGreen;
            RightGreen = rightGreen;
        }

        public float LeftGreen { get; }

        public float RightGreen { get; }

        public override string ToString()
        {
            return $"LeftGreen={LeftGreen}, RightGreen={RightGreen}";
        }
    }
}

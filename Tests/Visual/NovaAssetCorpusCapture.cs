// Isolated verification harness; never included in the distributable package.
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
#if NOVA_VISUAL_REFERENCE
using UnityEngine.Rendering.Universal;
#endif

public sealed class NovaAssetCorpusCapture
{
    private const int Size = 256;
    private Camera camera;
    private RenderTexture target;
    private Texture2D readback;
    private string output;
    private readonly List<string> frames = new List<string>();
    private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();

    [UnityTest]
    public IEnumerator CaptureLicensedAssetCorpus()
    {
        var args = Environment.GetCommandLineArgs();
        output = args[Array.IndexOf(args, "-novaVisualOutput") + 1];
        Directory.CreateDirectory(output);
        RenderSettings.fog = false;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.reflectionIntensity = 0;
        QualitySettings.antiAliasing = 0;
#if NOVA_VISUAL_REFERENCE
        var data = Own(ScriptableObject.CreateInstance<UniversalRendererData>());
#if !UNITY_6000_0_OR_NEWER
        // URP 14 creates shader resources in its asset-creation menu, not CreateInstance.
        typeof(UniversalRendererData).GetMethod("ReloadAllNullProperties", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(data, null);
#endif
        var featureType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Runtime.Core.Scripts.ScreenSpaceDistortion")).First(t => t != null);
        var distortion = (ScriptableRendererFeature)Own(ScriptableObject.CreateInstance(featureType));
        data.rendererFeatures.Add(distortion);
        var pipeline = Own(UniversalRenderPipelineAsset.Create(data));
        pipeline.msaaSampleCount = 1;
        pipeline.supportsHDR = false;
        pipeline.supportsCameraDepthTexture = true;
        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
#else
        GraphicsSettings.defaultRenderPipeline = null;
        QualitySettings.renderPipeline = null;
        // No manual Apply call: exercise automatic initial settings.
        Assert.That(GraphicsSettings.lightsUseLinearIntensity, Is.EqualTo(QualitySettings.activeColorSpace == ColorSpace.Linear));
        Assert.That(GraphicsSettings.lightsUseColorTemperature, Is.True);
#endif
        var go = Own(new GameObject("Corpus camera", typeof(Camera)));
        camera = go.GetComponent<Camera>();
        camera.enabled = true;
        camera.orthographic = true;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 500;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.11f, 0.19f, 1);
        camera.allowHDR = false;
        camera.allowMSAA = false;
        camera.depthTextureMode = DepthTextureMode.Depth;
#if NOVA_VISUAL_REFERENCE
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
#else
        var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Runtime.Core.Scripts.ScreenSpaceDistortion")).First(t => t != null);
        go.AddComponent(type);
#endif
        target = Own(new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32));
        camera.targetTexture = target;
        readback = Own(new Texture2D(Size, Size, TextureFormat.RGBA32, false));
        var light = Own(new GameObject("Corpus light", typeof(Light))).GetComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1, 0.8f, 0.6f);
        light.intensity = 0.8f;
        light.transform.rotation = Quaternion.Euler(25, 20, 0);
        RenderSettings.sun = light;
        try
        {
            yield return null;
            yield return null;
            if (!args.Contains("-novaOnlyPrefabs")) yield return CaptureTexturesAndDefaults();
            yield return CapturePrefabs();
            File.WriteAllLines(Path.Combine(output, "inventory.txt"), frames.OrderBy(s => s));
            File.WriteAllText(Path.Combine(output, "capture.txt"), Application.unityVersion + "\n" +
                QualitySettings.activeColorSpace + "\n" + SystemInfo.graphicsDeviceType + "\n" + frames.Count + " frames\n");
        }
        finally
        {
            if (camera != null) camera.targetTexture = null;
            foreach (var item in owned.AsEnumerable().Reverse()) if (item != null) UnityEngine.Object.DestroyImmediate(item);
        }
    }

    private IEnumerator CaptureTexturesAndDefaults()
    {
        // Original default Lit is metallic. A brighter backdrop exposes low-alpha
        // textures even when their default light response is black in Linear space.
        var effectBackground = camera.backgroundColor;
        camera.backgroundColor = new Color(0.3f, 0.4f, 0.5f, 1);
        var quad = Own(new GameObject("Corpus quad", typeof(MeshFilter), typeof(MeshRenderer)));
        var mesh = Own(new Mesh
        {
            vertices = new[] { new Vector3(-0.5f,-0.5f,0), new Vector3(0.5f,-0.5f,0), new Vector3(0.5f,0.5f,0), new Vector3(-0.5f,0.5f,0) },
            uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up },
            normals = Enumerable.Repeat(Vector3.back, 4).ToArray(),
            tangents = Enumerable.Repeat(new Vector4(1,0,0,1), 4).ToArray(),
            colors = Enumerable.Repeat(Color.white, 4).ToArray(),
            triangles = new[] { 0,2,1,0,3,2 }
        });
        mesh.RecalculateBounds();
        quad.GetComponent<MeshFilter>().sharedMesh = mesh;
        camera.orthographicSize = 0.65f;
        camera.transform.SetPositionAndRotation(new Vector3(0,0,-2), Quaternion.identity);
        var textures = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Corpus/Kenney" })
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(s => s).ToArray();
        Assert.That(textures.Length, Is.EqualTo(96), "Pinned CC0 pack inventory changed.");
        foreach (var shaderName in new[] { "Particles/UberUnlit", "Particles/UberLit", "UIParticles/UberUnlit", "UIParticles/UberLit" })
        {
            var shader = Shader.Find("Nova/" + shaderName);
            Assert.That(shader, Is.Not.Null);
            var material = Own(new Material(shader));
            quad.GetComponent<MeshRenderer>().sharedMaterial = material;
            var name = shaderName.Replace('/', '_');
            WriteState(material, name + "-fresh");
            yield return Capture(name + "-fresh");
            var guiName = shaderName.Replace("/", "") + "GUI";
            var guiType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Editor.Core.Scripts." + guiName)).First(t => t != null);
            var gui = (ShaderGUI)Activator.CreateInstance(guiType);
            gui.ValidateMaterial(material);
            WriteState(material, name + "-validated");
            yield return Capture(name + "-validated");
            material.SetFloat("_RenderType", 2);
            material.SetFloat("_TransparentBlendMode", 0);
            gui.ValidateMaterial(material);
            foreach (var path in textures)
            {
                material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                yield return Capture(name + "-kenney-" + Path.GetFileNameWithoutExtension(path));
            }
            CheckShader(material);
        }
        quad.SetActive(false);
        camera.backgroundColor = effectBackground;
    }

    private IEnumerator CapturePrefabs()
    {
        var paths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Samples/Prefabs" })
            .Select(AssetDatabase.GUIDToAssetPath).Where(p => Path.GetFileName(p).StartsWith("pfb_eff_")).OrderBy(p => p).ToArray();
        Assert.That(paths.Length, Is.EqualTo(13), "Pinned MIT sample inventory changed.");
        foreach (var path in paths)
        {
            if (Environment.GetCommandLineArgs().Contains("-novaOnlyHealSort") && Path.GetFileNameWithoutExtension(path) != "pfb_eff_heal02") continue;
            if (Environment.GetCommandLineArgs().Contains("-novaSkipUnity6FireCrash") && Path.GetFileNameWithoutExtension(path) == "pfb_eff_fire")
            {
                File.WriteAllText(Path.Combine(output, "engine-crash-skip.txt"), "pfb_eff_fire: both original and port crash in ParticleSystemRenderer::PrepareForRender on 6000.3.23f1. See report.\n");
                continue;
            }
            var effect = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            try
            {
                yield return null;
                var systems = effect.GetComponentsInChildren<ParticleSystem>(true);
                Assert.That(systems, Is.Not.Empty, path);
                if (Environment.GetCommandLineArgs().Contains("-novaExplicitParticleOrder"))
                {
                    // Diagnostic fallback only; never serialize back to the source prefab.
                    var renderers = effect.GetComponentsInChildren<ParticleSystemRenderer>(true);
                    for (var i = 0; i < renderers.Length; i++) renderers[i].sortingOrder = i;
                    File.WriteAllText(Path.Combine(output, "explicit-order.txt"), "Runtime renderer sortingOrder set to hierarchy index in both pipelines. This is a fallback experiment, not unchanged-asset evidence.\n");
                }
                for (var i = 0; i < systems.Length; i++)
                {
                    systems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                    systems[i].useAutoRandomSeed = false;
                    systems[i].randomSeed = (uint)(23456 + i);
                }
                foreach (var material in effect.GetComponentsInChildren<Renderer>(true).Where(r => r.enabled)
                    .SelectMany(RenderedMaterials).Distinct())
                {
                    Assert.That(material, Is.Not.Null, path);
                    Assert.That(material.shader.name, Does.StartWith("Nova/"), path);
                    CheckShader(material);
                }
                foreach (var time in new[] { 0.15f, 0.5f, 1.2f })
                {
                    var state = new StringBuilder();
                    foreach (var system in systems)
                    {
                        system.Simulate(time, false, true, true);
                        system.Pause(false);
                        var particles = new ParticleSystem.Particle[system.particleCount];
                        system.GetParticles(particles);
                        state.AppendLine(system.name + ":" + particles.Length);
                        foreach (var p in particles) state.AppendLine(p.position.ToString("R") + "|" + p.velocity.ToString("R") + "|" + p.remainingLifetime.ToString("R") + "|" + p.randomSeed);
                    }
                    yield return null; // Let Unity finalize mesh/trail particle data before capture.
                    var name = Path.GetFileNameWithoutExtension(path) + "-" + Mathf.RoundToInt(time * 100).ToString("000");
                    File.WriteAllText(Path.Combine(output, name + "-state.txt"), state.ToString());
                    // Fixed stage framing: conservative ParticleSystem renderer bounds can be
                    // hundreds of times larger than the visible effect (especially trails).
                    var focus = effect.transform.position + Vector3.up * 0.6f;
                    camera.orthographicSize = 3;
                    foreach (var angle in new[] { 0, 35, 80 })
                    {
                        camera.transform.position = focus + Quaternion.Euler(15, angle, 0) * Vector3.back * 8;
                        camera.transform.LookAt(focus);
                        camera.ResetWorldToCameraMatrix();
                        camera.ResetProjectionMatrix();
                        yield return null;
                        yield return Capture(name + "-" + angle.ToString("00"));
                    }
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(effect); }
        }
    }

    private static IEnumerable<Material> RenderedMaterials(Renderer renderer)
    {
        if (!renderer.gameObject.activeInHierarchy) yield break;
        if (renderer is ParticleSystemRenderer particleRenderer)
        {
            if (particleRenderer.renderMode != ParticleSystemRenderMode.None) yield return particleRenderer.sharedMaterial;
            if (particleRenderer.GetComponent<ParticleSystem>().trails.enabled) yield return particleRenderer.trailMaterial;
        }
        else foreach (var material in renderer.sharedMaterials) yield return material;
    }

    private void WriteState(Material m, string name)
    {
        var state = new StringBuilder();
        state.AppendLine("queue=" + m.renderQueue + ";type=" + m.GetTag("RenderType", false));
        state.AppendLine(string.Join(",", m.shaderKeywords.Where(k => k != "_ADDITIONAL_LIGHTS").OrderBy(k => k)));
        for (var i = 0; i < m.shader.GetPropertyCount(); i++)
        {
            var property = m.shader.GetPropertyName(i);
            var type = m.shader.GetPropertyType(i);
            var value = type == ShaderPropertyType.Texture ? m.GetTexture(property)?.name ?? "null" :
                type == ShaderPropertyType.Color ? m.GetColor(property).ToString("R") :
                type == ShaderPropertyType.Vector ? m.GetVector(property).ToString("R") : m.GetFloat(property).ToString("R");
            state.AppendLine(property + "=" + value);
        }
        File.WriteAllText(Path.Combine(output, name + "-state.txt"), state.ToString());
    }

    private void CheckShader(Material m)
    {
        Assert.That(m.shader.isSupported, Is.True, m.name);
        for (var pass = 0; pass < m.passCount; pass++) ShaderUtil.CompilePass(m, pass, true);
        Assert.That(ShaderUtil.GetShaderMessages(m.shader).Where(s => s.severity.ToString() == "Error"), Is.Empty, m.name);
    }

    private IEnumerator Capture(string name)
    {
        // Capture the normal game-camera render. Render-request paths do not reproduce
        // the original distortion feature's viewport/RTHandle behavior in Unity 6.
        yield return null;
        yield return null;
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        readback.ReadPixels(new Rect(0,0,Size,Size),0,0);
        readback.Apply();
        RenderTexture.active = previous;
        var pixels = readback.GetPixels32();
        using (var stream = File.Create(Path.Combine(output, name + ".ppm")))
        {
            var header = Encoding.ASCII.GetBytes("P6\n" + Size + " " + Size + "\n255\n");
            stream.Write(header, 0, header.Length);
            for (var y = Size - 1; y >= 0; y--)
            for (var x = 0; x < Size; x++)
            {
                var c = pixels[y * Size + x]; stream.WriteByte(c.r); stream.WriteByte(c.g); stream.WriteByte(c.b);
            }
        }
        frames.Add(name + ".ppm");
    }

    private T Own<T>(T item) where T : UnityEngine.Object { owned.Add(item); return item; }
}
#endif

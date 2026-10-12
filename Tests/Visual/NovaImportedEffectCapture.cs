// Verification harness only: import the user's package into isolated projects before running.
#if UNITY_EDITOR
using System;
using System.Collections;
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

public sealed class NovaImportedEffectCapture
{
    private const int Size = 384;

    [UnityTest]
    public IEnumerator CaptureImportedEffectFrames()
    {
        var args = Environment.GetCommandLineArgs();
        var output = args[Array.IndexOf(args, "-novaVisualOutput") + 1];
        var prefabPath = args[Array.IndexOf(args, "-novaEffectPrefab") + 1];
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Assert.That(prefab, Is.Not.Null);
        Directory.CreateDirectory(output);
        RenderSettings.fog = false;
        QualitySettings.antiAliasing = 0;
#if NOVA_VISUAL_REFERENCE
        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        var pipeline = UniversalRenderPipelineAsset.Create(rendererData);
        pipeline.msaaSampleCount = 1;
        pipeline.supportsHDR = false;
        QualitySettings.renderPipeline = pipeline;
        GraphicsSettings.defaultRenderPipeline = pipeline;
#else
        QualitySettings.renderPipeline = null;
        GraphicsSettings.defaultRenderPipeline = null;
#endif
        var effect = UnityEngine.Object.Instantiate(prefab);
        var systems = effect.GetComponentsInChildren<ParticleSystem>(true);
        Assert.That(systems.Length, Is.EqualTo(6), "Fire001 fixture changed; review the input package.");
        for (var i = 0; i < systems.Length; i++)
        {
            systems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            systems[i].useAutoRandomSeed = false;
            systems[i].randomSeed = (uint)(12345 + i);
        }
        var cameraObject = new GameObject("Effect comparison camera", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 2.5f;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 100;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.11f, 0.19f, 1);
        camera.allowHDR = false;
        camera.allowMSAA = false;
        var target = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = target;
        var image = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        try
        {
            foreach (var material in effect.GetComponentsInChildren<Renderer>(true).Where(r => r.enabled).SelectMany(r => r.sharedMaterials).Distinct())
            {
                Assert.That(material, Is.Not.Null);
                Assert.That(material.shader.name, Is.EqualTo("Nova/Particles/UberUnlit"));
                for (var pass = 0; pass < material.passCount; pass++) ShaderUtil.CompilePass(material, pass, true);
                Assert.That(ShaderUtil.GetShaderMessages(material.shader).Where(m => m.severity.ToString() == "Error"), Is.Empty);
            }
            yield return null;
            yield return null;
            foreach (var time in new[] { 0.25f, 0.75f, 1.5f, 3f, 5f, 8f })
            {
                var state = new StringBuilder();
                foreach (var system in systems)
                {
                    system.Simulate(time, false, true, true);
                    system.Pause(false);
                    var particles = new ParticleSystem.Particle[system.particleCount];
                    system.GetParticles(particles);
                    state.AppendLine(system.name + ":" + particles.Length);
                    foreach (var particle in particles)
                        state.AppendLine(particle.position.ToString("R") + "|" + particle.velocity.ToString("R") + "|" + particle.remainingLifetime.ToString("R") + "|" + particle.randomSeed);
                }
                var stamp = Mathf.RoundToInt(time * 100).ToString("0000");
                File.WriteAllText(Path.Combine(output, stamp + "-particles.txt"), state.ToString());
                foreach (var angle in new[] { 0, 35, 80 })
                {
                    var focus = effect.transform.position + Vector3.up * 1.2f;
                    camera.transform.position = focus + Quaternion.Euler(0, angle, 0) * new Vector3(0, 0.5f, -8);
                    camera.transform.LookAt(focus);
#if NOVA_VISUAL_REFERENCE
                    RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
#else
                    camera.Render();
#endif
                    var previous = RenderTexture.active;
                    RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
                    image.Apply();
                    RenderTexture.active = previous;
                    var pixels = image.GetPixels32();
                    Assert.That(pixels.Count(c => c.r > 80 && c.r > c.b * 1.1f), Is.GreaterThan(100), "No visible fire.");
                    using (var stream = File.Create(Path.Combine(output, stamp + "-" + angle.ToString("00") + ".ppm")))
                    {
                        var header = Encoding.ASCII.GetBytes("P6\n" + Size + " " + Size + "\n255\n");
                        stream.Write(header, 0, header.Length);
                        for (var y = Size - 1; y >= 0; y--)
                        for (var x = 0; x < Size; x++)
                        {
                            var c = pixels[y * Size + x]; stream.WriteByte(c.r); stream.WriteByte(c.g); stream.WriteByte(c.b);
                        }
                    }
                }
            }
            File.WriteAllText(Path.Combine(output, "capture.txt"), Application.unityVersion + "\n" + SystemInfo.graphicsDeviceType + "\n" + QualitySettings.activeColorSpace + "\n18 frames\n");
        }
        finally
        {
            foreach (var item in new UnityEngine.Object[] { effect, cameraObject, target, image }) UnityEngine.Object.DestroyImmediate(item);
        }
    }
}
#endif

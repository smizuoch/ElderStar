// Differential fixture: define NOVA_VISUAL_REFERENCE only in the original URP project.
using System;
using System.Collections;
using System.IO;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
#if NOVA_VISUAL_REFERENCE
using UnityEngine.Rendering.Universal;
#endif
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaAvatarOverlapCapture
    {
        [UnityTest]
        public IEnumerator CaptureAvatarOverlap()
        {
            foreach (var mirrored in new[] { false, true })
            foreach (var mode in new[] { "zero", "fade", "dissolve", "depth fade" })
                yield return CaptureCase(mode, mirrored);
        }

        private IEnumerator CaptureCase(string mode, bool mirrored)
        {
            var args = Environment.GetCommandLineArgs();
            var output = args[Array.IndexOf(args, "-novaVisualOutput") + 1];
            Directory.CreateDirectory(output);
            var previousPipeline = GraphicsSettings.defaultRenderPipeline;
            var previousQuality = QualitySettings.renderPipeline;
#if NOVA_VISUAL_REFERENCE
            var data = ScriptableObject.CreateInstance<UniversalRendererData>();
#if !UNITY_6000_0_OR_NEWER
            typeof(UniversalRendererData).GetMethod("ReloadAllNullProperties", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(data, null);
#endif
            var featureType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Runtime.Core.Scripts.ScreenSpaceDistortion")).First(t => t != null);
            var feature = (ScriptableRendererFeature)ScriptableObject.CreateInstance(featureType);
            data.rendererFeatures.Add(feature);
            var pipeline = UniversalRenderPipelineAsset.Create(data);
            pipeline.msaaSampleCount = 1; pipeline.supportsHDR = false;
            GraphicsSettings.defaultRenderPipeline = pipeline; QualitySettings.renderPipeline = pipeline;
#else
            GraphicsSettings.defaultRenderPipeline = null; QualitySettings.renderPipeline = null;
#endif
            var cameraObject = new GameObject("Overlap camera", typeof(Camera));
            var background = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var underlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var foreground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var backgroundMaterial = new Material(Shader.Find("Nova/Particles/UberUnlit"));
            var underlayMaterial = new Material(Shader.Find("Nova/Particles/Distortion"));
            var foregroundMaterial = new Material(underlayMaterial);
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var offset = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            var target = new RenderTexture(128, 128, 24);
            var cpu = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            var previousTarget = RenderTexture.active;
            var previousCulling = GL.invertCulling;
            var previousActive = Shader.GetGlobalFloat("_NovaCameraDistortionActive");
            var previousMirror = Shader.GetGlobalFloat("_VRChatMirrorMode");
            try
            {
                for (var y = 0; y < 64; y++)
                for (var x = 0; x < 64; x++)
                    texture.SetPixel(x, y, new Color(x / 63f, y / 63f, ((x / 4 + y / 4) % 2), 1));
                texture.Apply();
                offset.SetPixels(Enumerable.Repeat(new Color(1, .5f, 0, 1), 4).ToArray());
                offset.Apply();
                backgroundMaterial.SetTexture("_BaseMap", texture);
                backgroundMaterial.SetFloat("_RenderType", 0);
                backgroundMaterial.SetFloat("_Cull", 0);
                Setup("ParticlesUberUnlitMaterialPostProcessor", backgroundMaterial);
                background.GetComponent<Renderer>().sharedMaterial = backgroundMaterial;
                background.transform.position = new Vector3(0, 0, 1);
                background.transform.localScale = Vector3.one * 2;
                foreach (var material in new[] { underlayMaterial, foregroundMaterial })
                {
                    material.SetFloat("_Cull", 0);
                    material.SetFloat("_DistortionIntensity", .6f);
                    material.SetTexture("_BaseMap", offset);
                }
                if (mode == "zero") foregroundMaterial.SetFloat("_DistortionIntensity", 0);
                if (mode == "fade" || mode == "dissolve")
                {
                    foregroundMaterial.SetTexture("_AlphaTransitionMap", Texture2D.whiteTexture);
                    foregroundMaterial.SetFloat("_AlphaTransitionMode", mode == "fade" ? 1 : 2);
                    foregroundMaterial.SetFloat("_AlphaTransitionProgress", 1);
                    foregroundMaterial.SetFloat("_DissolveSharpness", 1);
                }
                if (mode == "depth fade")
                {
                    foregroundMaterial.SetFloat("_DepthFadeEnabled", 1);
                    foregroundMaterial.SetFloat("_DepthFadeNear", 20);
                    foregroundMaterial.SetFloat("_DepthFadeFar", 30);
                }
                Setup("ParticlesDistortionMaterialPostProcessor", underlayMaterial);
                Setup("ParticlesDistortionMaterialPostProcessor", foregroundMaterial);
                underlayMaterial.renderQueue = 3100;
                foregroundMaterial.renderQueue = 3101;
                underlay.GetComponent<Renderer>().sharedMaterial = underlayMaterial;
                foreground.GetComponent<Renderer>().sharedMaterial = foregroundMaterial;
                underlay.transform.position = new Vector3(0, 0, .2f);
                var camera = cameraObject.GetComponent<Camera>();
                camera.transform.position = new Vector3(0, 0, -2);
                camera.orthographic = true;
                camera.orthographicSize = 1;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
#if NOVA_VISUAL_REFERENCE
                camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
#endif
                if (mirrored)
                {
                    camera.worldToCameraMatrix = Matrix4x4.Scale(new Vector3(-1, 1, 1)) * camera.worldToCameraMatrix;
                    GL.invertCulling = !previousCulling;
                }
                Shader.SetGlobalFloat("_NovaCameraDistortionActive", 0);
                Shader.SetGlobalFloat("_VRChatMirrorMode", mirrored ? 2 : 0);
                Color32[] Render()
                {

                    RenderTexture.active = target;
                    cpu.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
                    cpu.Apply();
                    return cpu.GetPixels32();
                }
                foreground.SetActive(false);
                underlay.SetActive(false);
                yield return null; yield return null;
                var empty = Render();
                underlay.SetActive(true);
                yield return null; yield return null;
                var expected = Render();
                Save(output, mode, mirrored, "underlay", expected);
                Assert.That(DifferentPixels(empty, expected), Is.GreaterThan(500), "Visible underlay is required.");
                foreground.SetActive(true);
                yield return null; yield return null;
                var actual = Render();
                Save(output, mode, mirrored, "overlap", actual);
                Assert.That(DifferentPixels(expected, actual), Is.EqualTo(0),
                    "A zero-offset foreground must not erase another distortion already on screen.");
            }
            finally
            {
                GraphicsSettings.defaultRenderPipeline = previousPipeline; QualitySettings.renderPipeline = previousQuality;
#if NOVA_VISUAL_REFERENCE
                UnityEngine.Object.DestroyImmediate(pipeline); UnityEngine.Object.DestroyImmediate(feature); UnityEngine.Object.DestroyImmediate(data);
#endif
                Shader.SetGlobalFloat("_NovaCameraDistortionActive", previousActive);
                Shader.SetGlobalFloat("_VRChatMirrorMode", previousMirror);
                GL.invertCulling = previousCulling;
                RenderTexture.active = previousTarget;
                foreach (var item in new UnityEngine.Object[] { cameraObject, background, underlay, foreground,
                    backgroundMaterial, underlayMaterial, foregroundMaterial, texture, offset, target, cpu })
                    UnityEngine.Object.DestroyImmediate(item);
            }
        }

        private static void Save(string output, string mode, bool mirror, string suffix, Color32[] pixels)
        {
            using (var stream = File.Create(Path.Combine(output, mode.Replace(" ", "-") + "-" + mirror + "-" + suffix + ".ppm")))
            {
                var header = System.Text.Encoding.ASCII.GetBytes("P6\n128 128\n255\n"); stream.Write(header, 0, header.Length);
                for (var y = 127; y >= 0; y--) for (var x = 0; x < 128; x++)
                { var color = pixels[y * 128 + x]; stream.WriteByte(color.r); stream.WriteByte(color.g); stream.WriteByte(color.b); }
            }
        }

        private static int DifferentPixels(Color32[] a, Color32[] b) => a.Zip(b,
            (x, y) => Math.Max(Math.Abs(x.r - y.r), Math.Max(Math.Abs(x.g - y.g), Math.Abs(x.b - y.b)))).Count(x => x > 1);

        private static void Setup(string name, Material material)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Nova.Editor.Core.Scripts." + name)).First(t => t != null);
            type.GetMethod("SetupMaterialKeywords").Invoke(null, new object[] { material });
            type.GetMethod("SetupMaterialBlendMode")?.Invoke(null, new object[] { material });
        }
    }
}

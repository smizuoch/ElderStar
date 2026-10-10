#if UNITY_EDITOR
using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaBuiltInOptimizedRenderingTests
    {
        private const string Folder = "Assets/NovaOptimizedRenderingVerification";

        [OneTimeSetUp]
        public void Generate()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Nova.Editor.Core.Scripts.Optimizer.OptimizedShaderGenerator")).First(t => t != null);
            type.GetMethod("Generate").Invoke(null, new object[] { Folder });
        }

        [OneTimeTearDown]
        public void Cleanup() { UnityEditor.AssetDatabase.DeleteAsset(Folder); }

        [TestCase("UberUnlit", "Opaque", 0)]
        [TestCase("UberUnlit", "Cutout", 1)]
        [TestCase("UberUnlit", "Transparent", 2)]
        [TestCase("UberLit", "Opaque", 0)]
        [TestCase("UberLit", "Cutout", 1)]
        [TestCase("UberLit", "Transparent", 2)]
        public void OptimizedForwardRenderingMatchesOriginal(string kind, string renderName, int renderType)
        {
            var material = new Material(Shader.Find("Nova/Particles/" + kind));
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var cameraObject = new GameObject("Optimized shader camera", typeof(Camera));
            var target = new RenderTexture(64, 64, 24);
            var image = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var albedo = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            var ambientMode = RenderSettings.ambientMode;
            var ambient = RenderSettings.ambientLight;
            var active = RenderTexture.active;
            try
            {
                albedo.SetPixels(new[] { new Color(0.4f, 0.8f, 0.2f, 0.1f), new Color(0.4f, 0.8f, 0.2f, 0.9f),
                    new Color(0.4f, 0.8f, 0.2f, 0.1f), new Color(0.4f, 0.8f, 0.2f, 0.9f) });
                albedo.filterMode = FilterMode.Point;
                albedo.Apply();
                material.SetTexture("_BaseMap", albedo);
                material.SetFloat("_RenderType", renderType);
                material.SetFloat("_Cull", 0);
                material.SetFloat("_TintAreaMode", 1);
                material.SetColor("_TintColor", new Color(0.8f, 0.9f, 0.7f, 1));
                if (kind == "UberLit")
                {
                    material.SetFloat("_EnvironmentReflections", 0);
                    material.SetFloat("_SpecularHighlights", 0);
                }
                var processor = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("Nova.Editor.Core.Scripts.Particles" + kind + "MaterialPostProcessor")).First(t => t != null);
                processor.GetMethod("SetupMaterialKeywords").Invoke(null, new object[] { material });
                processor.GetMethod("SetupMaterialBlendMode").Invoke(null, new object[] { material });
                quad.GetComponent<Renderer>().sharedMaterial = material;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = Color.white;
                var camera = cameraObject.GetComponent<Camera>();
                camera.transform.position = new Vector3(0, 0, -3);
                camera.orthographic = true;
                camera.orthographicSize = 0.6f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.03f, 0.04f, 0.05f, 1);
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                var original = Capture(camera, image);
                foreach (var passSet in new[] { "None", "DepthOnly DepthNormals ShadowCaster" })
                {
                    var shader = Shader.Find("Hidden/Nova/Particles/" + kind + "(Optimized " + renderName + " " + passSet + ")");
                    Assert.That(shader, Is.Not.Null);
                    var queue = material.renderQueue;
                    material.shader = shader;
                    material.renderQueue = queue;
                    var optimized = Capture(camera, image);
                    Assert.That(optimized[32 * 64 + 45].g, Is.GreaterThan(0.15f), "A blank frame cannot establish parity.");
                    var difference = original.Zip(optimized, (a, b) => Vector4.Distance(a, b)).Max();
                    Assert.That(difference, Is.LessThan(0.01f), kind + " " + renderName + " " + passSet);
                }
            }
            finally
            {
                RenderTexture.active = active;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientLight = ambient;
                foreach (var obj in new UnityEngine.Object[] { material, quad, cameraObject, target, image, albedo })
                    UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        private static Color[] Capture(Camera camera, Texture2D image)
        {
            camera.Render();
            RenderTexture.active = camera.targetTexture;
            image.ReadPixels(new Rect(0, 0, 64, 64), 0, 0);
            image.Apply();
            return image.GetPixels();
        }
    }
}
#endif

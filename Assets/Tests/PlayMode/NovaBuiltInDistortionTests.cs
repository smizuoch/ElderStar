using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaBuiltInDistortionTests
    {
        [TestCase("enabled", true)]
        [TestCase("zero", false)]
        [TestCase("disabled renderer", false)]
        [TestCase("camera layer", false)]
        [TestCase("dissolved", false)]
        [TestCase("occluded", false)]
        [TestCase("vertical", true)]
        [TestCase("negative", true)]
        [TestCase("xy zero", false)]
        [TestCase("soft particles", false)]
        [TestCase("depth fade", false)]
        [TestCase("cull front", false)]
        public void CameraDistortionRespectsMaterialAndVisibility(string scenario, bool shouldDistort)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Nova.Runtime.Core.Scripts.ScreenSpaceDistortion"))
                .FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, "The Built-in camera component is missing.");
            var cameraObject = new GameObject("Distortion test camera", typeof(Camera));
            var background = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var particle = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var backgroundMaterial = new Material(Shader.Find("Nova/Particles/UberUnlit"));
            var distortionMaterial = new Material(Shader.Find("Nova/Particles/Distortion"));
            var gradient = new Texture2D(64, 4, TextureFormat.RGBA32, false);
            var offset = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            var target = new RenderTexture(128, 128, 24);
            var readback = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            var previous = RenderTexture.active;
            try
            {
                for (var y = 0; y < 4; y++)
                for (var x = 0; x < 64; x++) gradient.SetPixel(x, y, new Color(x / 63f, y / 3f, 0, 1));
                gradient.Apply();
                var offsetColor = scenario == "vertical" ? new Color(0.5f, 1, 0, 1)
                    : scenario == "negative" ? new Color(0, 0.5f, 0, 1) : new Color(1, 0.5f, 0, 1);
                offset.SetPixels(Enumerable.Repeat(offsetColor, 4).ToArray());
                offset.Apply();
                backgroundMaterial.SetTexture("_BaseMap", gradient);
                backgroundMaterial.SetFloat("_RenderType", 0);
                backgroundMaterial.SetFloat("_Cull", 0);
                backgroundMaterial.SetFloat("_ShadowCasterEnabled", 1);
                Setup("ParticlesUberUnlitMaterialPostProcessor", backgroundMaterial);
                background.transform.position = new Vector3(0, 0, 1);
                background.transform.localScale = new Vector3(2, 2, 1);
                background.GetComponent<Renderer>().sharedMaterial = backgroundMaterial;
                distortionMaterial.SetTexture("_BaseMap", offset);
                distortionMaterial.SetFloat("_Cull", 0);
                distortionMaterial.SetFloat("_DistortionIntensity", scenario == "zero" ? 0 : 1);
                if (scenario == "xy zero")
                {
                    distortionMaterial.SetFloat("_DistortionIntensityMode", 1);
                    distortionMaterial.SetFloat("_DistortionIntensityX", 0);
                    distortionMaterial.SetFloat("_DistortionIntensityY", 0);
                }
                if (scenario == "soft particles")
                {
                    distortionMaterial.SetFloat("_SoftParticlesEnabled", 1);
                    distortionMaterial.SetFloat("_SoftParticlesIntensity", 1000);
                }
                if (scenario == "depth fade")
                {
                    distortionMaterial.SetFloat("_DepthFadeEnabled", 1);
                    distortionMaterial.SetFloat("_DepthFadeNear", 20);
                    distortionMaterial.SetFloat("_DepthFadeFar", 30);
                }
                if (scenario == "cull front") distortionMaterial.SetFloat("_Cull", 1);
                if (scenario == "dissolved")
                {
                    distortionMaterial.SetTexture("_AlphaTransitionMap", Texture2D.whiteTexture);
                    distortionMaterial.SetFloat("_AlphaTransitionMode", 2);
                    distortionMaterial.SetFloat("_AlphaTransitionProgress", 1);
                    distortionMaterial.SetFloat("_DissolveSharpness", 1);
                }
                Setup("ParticlesDistortionMaterialPostProcessor", distortionMaterial);
                particle.GetComponent<Renderer>().sharedMaterial = distortionMaterial;
                particle.GetComponent<Renderer>().enabled = scenario != "disabled renderer";
                particle.layer = 8;
                if (scenario == "occluded") particle.transform.position = new Vector3(0, 0, 2);
                var camera = cameraObject.GetComponent<Camera>();
                camera.transform.position = new Vector3(0, 0, -3);
                camera.orthographic = true;
                camera.orthographicSize = 1;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 10;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                if (scenario == "camera layer") camera.cullingMask &= ~(1 << 8);
                var effect = (Behaviour)cameraObject.AddComponent(type);
                effect.enabled = false;
                var baseline = Read(camera, target, readback);
                effect.enabled = true;
                var distorted = Read(camera, target, readback);
                if (scenario == "negative") Assert.That(distorted.r, Is.LessThan(baseline.r - 0.08f), $"before {baseline}, after {distorted}");
                else if (scenario == "vertical") Assert.That(distorted.g, Is.GreaterThan(baseline.g + 0.08f), $"before {baseline}, after {distorted}");
                else if (shouldDistort) Assert.That(distorted.r, Is.GreaterThan(baseline.r + 0.08f), $"before {baseline}, after {distorted}");
                else Assert.That(Vector4.Distance(distorted, baseline), Is.LessThan(0.025f), $"before {baseline}, after {distorted}");
                effect.enabled = false;
                var restored = Read(camera, target, readback);
                Assert.That(Mathf.Abs(restored.r - baseline.r), Is.LessThan(0.015f), "Disabling must restore the original frame.");
            }
            finally
            {
                RenderTexture.active = previous;
                foreach (var obj in new UnityEngine.Object[] { cameraObject, background, particle, backgroundMaterial,
                    distortionMaterial, gradient, offset, target, readback }) UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        private static Color Read(Camera camera, RenderTexture target, Texture2D readback)
        {
            camera.Render();
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            readback.Apply();
            return readback.GetPixel(64, 64);
        }

        private static void Setup(string name, Material material)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Editor.Core.Scripts." + name)).First(t => t != null);
            type.GetMethod("SetupMaterialKeywords").Invoke(null, new object[] { material });
            type.GetMethod("SetupMaterialBlendMode")?.Invoke(null, new object[] { material });
        }
    }
}

using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.PlayMode
{
    [TestFixture("Nova/Particles/Distortion")]
    [TestFixture("Nova/Particles/UberUnlit")]
    [TestFixture("Nova/UIParticles/UberUnlit")]
    [TestFixture("Nova/Particles/UberLit")]
    [TestFixture("Nova/UIParticles/UberLit")]
    public sealed class NovaOriginalDepthFadeTests
    {
        private readonly string shaderName;
        public NovaOriginalDepthFadeTests(string name) { shaderName = name; }

        [TestCase("perspective", .1f, 5f, .5f)]
        [TestCase("orthographic", .1f, 5f, .5f)]
        [TestCase("oblique", .1f, 5f, .5f)]
        [TestCase("mirror", .1f, 5f, .5f)]
        [TestCase("negative-scale", .1f, 5f, .5f)]
        [TestCase("negative-width", 1f, 2f, -.5f)]
        [TestCase("thin-width", 2.999995f, 4f, .00001f)]
        public void FadeMatchesOriginalProjectedDepth(string view, float near, float far, float width)
        {
            var cameraObject = new GameObject("Depth fade camera", typeof(Camera));
            var surface = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var lightObject = new GameObject("Depth fade light", typeof(Light));
            var material = new Material(Shader.Find(shaderName));
            var oracle = new Material(Shader.Find("Hidden/NovaBuiltInTests/OriginalDepthFadeOracle"));
            var target = new RenderTexture(64, 64, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var cpu = new Texture2D(64, 64, TextureFormat.RGBAFloat, false, true);
            var oldTarget = RenderTexture.active; var oldAmbient = RenderSettings.ambientLight;
            var oldAmbientMode = RenderSettings.ambientMode; var oldInvert = GL.invertCulling;
            try
            {
                RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = Color.black;
                var light = lightObject.GetComponent<Light>(); light.type = LightType.Directional; light.intensity = .5f;
                var camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
                camera.transform.position = new Vector3(0, 0, -3); camera.nearClipPlane = .1f; camera.farClipPlane = 10;
                camera.fieldOfView = 40; camera.orthographic = view == "orthographic"; camera.orthographicSize = 1;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.clear;
                camera.allowHDR = true; camera.allowMSAA = false; camera.targetTexture = target;
                if (view == "oblique") camera.projectionMatrix = camera.CalculateObliqueMatrix(new Vector4(.1f,.2f,-1,-2));
                if (view == "mirror") { camera.worldToCameraMatrix = Matrix4x4.Scale(new Vector3(-1,1,1)) * camera.worldToCameraMatrix; GL.invertCulling = !oldInvert; }
                if (view == "negative-scale") surface.transform.localScale = new Vector3(-1,1,1);
                material.SetFloat("_Cull", 0); material.SetFloat("_RenderType", 2); material.SetFloat("_TransparentBlendMode", 0);
                material.SetTexture("_BaseMap", Texture2D.whiteTexture);
                if (material.HasProperty("_DistortionIntensity")) { material.SetFloat("_DistortionIntensity", 2); material.SetFloat("_ZTest", 8); }
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0);
                if (material.HasProperty("_LitWorkflowMode")) material.SetFloat("_LitWorkflowMode", 1);
                if (material.HasProperty("_SpecularHighlights")) material.SetFloat("_SpecularHighlights", 0);
                if (material.HasProperty("_EnvironmentReflections")) material.SetFloat("_EnvironmentReflections", 0);
                material.SetFloat("_DepthFadeNear", near); material.SetFloat("_DepthFadeFar", far); material.SetFloat("_DepthFadeWidth", width);
                oracle.SetFloat("_Near", near); oracle.SetFloat("_Far", far); oracle.SetFloat("_Width", width);
                var renderer = surface.GetComponent<Renderer>();
                Color[] Render(Material m)
                {
                    renderer.sharedMaterial = m;
                    if (m == material && shaderName.EndsWith("Distortion"))
                    {
                        renderer.enabled = false;
                        using (var commands = new CommandBuffer())
                        {
                            var pass = material.FindPass("DistortedUvBuffer"); Assert.That(pass, Is.GreaterThanOrEqualTo(0));
                            commands.DrawMesh(surface.GetComponent<MeshFilter>().sharedMesh, surface.transform.localToWorldMatrix, material, 0, pass);
                            camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commands);
                            try { camera.Render(); }
                            finally { camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, commands); renderer.enabled = true; }
                        }
                    }
                    else camera.Render();
                    RenderTexture.active = target;
                    cpu.ReadPixels(new Rect(0,0,64,64),0,0); cpu.Apply(); return cpu.GetPixels();
                }
                Setup(material); var unfaded = Render(material); var expected = Render(oracle);
                material.SetFloat("_DepthFadeEnabled", 1); Setup(material); var actual = Render(material);
                var witnesses = 0; var maxError = 0f;
                for (var i = 0; i < actual.Length; i++)
                {
                    if (unfaded[i].r < .1f || expected[i].a < .99f) continue;
                    witnesses++;
                    maxError = Mathf.Max(maxError, Mathf.Abs(actual[i].r / unfaded[i].r - expected[i].r));
                }
                Assert.That(witnesses, Is.GreaterThan(100), "Visible lit/geometry witness");
                Assert.That(maxError, Is.LessThan(.005f), "Original clip-depth/width formula must control actual surface alpha.");
            }
            finally
            {
                RenderTexture.active = oldTarget; RenderSettings.ambientLight = oldAmbient;
                RenderSettings.ambientMode = oldAmbientMode; GL.invertCulling = oldInvert;
                foreach (var item in new UnityEngine.Object[] { cameraObject, surface, lightObject, material, oracle, target, cpu }) UnityEngine.Object.DestroyImmediate(item);
            }
        }

        private static void Setup(Material material)
        {
            var name = material.shader.name.EndsWith("Distortion") ? "ParticlesDistortionMaterialPostProcessor" : material.shader.name.EndsWith("UberLit") ? "ParticlesUberLitMaterialPostProcessor" : "ParticlesUberUnlitMaterialPostProcessor";
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Editor.Core.Scripts." + name)).First(t => t != null);
            type.GetMethod("SetupMaterialKeywords").Invoke(null,new object[] {material});
            type.GetMethod("SetupMaterialBlendMode")?.Invoke(null,new object[] {material});
        }
    }
}

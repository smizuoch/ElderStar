using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaBuiltInParticleSystemTests
    {
        [TestCase("Nova/Particles/UberUnlit", false)]
        [TestCase("Nova/Particles/UberLit", false)]
        [TestCase("Nova/UIParticles/UberUnlit", false)]
        [TestCase("Nova/UIParticles/UberLit", false)]
        [TestCase("Nova/Particles/UberUnlit", true)]
        [TestCase("Nova/Particles/UberLit", true)]
        [TestCase("Nova/UIParticles/UberUnlit", true)]
        [TestCase("Nova/UIParticles/UberLit", true)]
        public void ParticlesUseVertexColorAndCustomTransitionProgress(string shaderName, bool instanced)
        {
            var cameraObject = new GameObject("Particle test camera", typeof(Camera));
            var particleObject = new GameObject("NOVA particles", typeof(ParticleSystem));
            var material = new Material(Shader.Find(shaderName));
            var target = new RenderTexture(128, 128, 24);
            var image = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            var mesh = new Mesh();
            var ambientMode = RenderSettings.ambientMode;
            var ambient = RenderSettings.ambientLight;
            var active = RenderTexture.active;
            try
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = Color.white;
                var camera = cameraObject.GetComponent<Camera>();
                camera.transform.position = new Vector3(0, 0, -3);
                camera.orthographic = true;
                camera.orthographicSize = 1;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                material.SetTexture("_BaseMap", Texture2D.whiteTexture);
                material.SetTexture("_AlphaTransitionMap", Texture2D.whiteTexture);
                material.SetFloat("_AlphaTransitionMode", 2);
                material.SetFloat("_AlphaTransitionProgressCoord", 1);
                material.SetFloat("_DissolveSharpness", 1);
                material.SetFloat("_Cull", 0);
                material.SetFloat("_RenderType", 2);
                if (shaderName.EndsWith("/UberLit"))
                {
                    material.SetFloat("_EnvironmentReflections", 0);
                    material.SetFloat("_SpecularHighlights", 0);
                }
                Setup(material, shaderName.EndsWith("/UberLit"));
                var system = particleObject.GetComponent<ParticleSystem>();
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = system.main;
                main.playOnAwake = false;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.startSpeed = 0;
                main.startSize = 1;
                main.startLifetime = 100;
                var emission = system.emission;
                emission.enabled = false;
                var renderer = particleObject.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = material;
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                if (instanced)
                {
                    mesh.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f),
                        new Vector3(0.5f, 0.5f), new Vector3(-0.5f, 0.5f) };
                    mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
                    mesh.normals = Enumerable.Repeat(Vector3.back, 4).ToArray();
                    mesh.tangents = Enumerable.Repeat(new Vector4(1, 0, 0, 1), 4).ToArray();
                    mesh.colors = Enumerable.Repeat(Color.white, 4).ToArray();
                    mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
                    renderer.renderMode = ParticleSystemRenderMode.Mesh;
                    renderer.mesh = mesh;
                    renderer.enableGPUInstancing = true;
                    material.enableInstancing = true;
                    Assert.That(renderer.supportsMeshInstancing, Is.True);
#if UNITY_EDITOR
                    Assert.That(UnityEditor.ShaderUtil.HasProceduralInstancing(material.shader), Is.True);
#endif
                }
                renderer.SetActiveVertexStreams(new List<ParticleSystemVertexStream>
                {
                    ParticleSystemVertexStream.Position, ParticleSystemVertexStream.Normal,
                    ParticleSystemVertexStream.Color, ParticleSystemVertexStream.UV, ParticleSystemVertexStream.UV2,
                    ParticleSystemVertexStream.Custom1XYZW, ParticleSystemVertexStream.Custom2XYZW, ParticleSystemVertexStream.Tangent
                });
                system.Emit(new ParticleSystem.EmitParams
                {
                    position = new Vector3(-0.4f, 0, 0), velocity = Vector3.zero, startLifetime = 100,
                    startSize = 0.65f, startColor = new Color(0.8f, 0.35f, 0.1f, 1)
                }, 1);
                system.Emit(new ParticleSystem.EmitParams
                {
                    position = new Vector3(0.4f, 0, 0), velocity = Vector3.zero, startLifetime = 100,
                    startSize = 0.65f, startColor = Color.blue
                }, 1);
                system.Simulate(0.01f, true, false);
                system.SetCustomParticleData(new List<Vector4> { Vector4.zero, Vector4.one }, ParticleSystemCustomData.Custom1);
                var visible = Capture(camera, image);
                Assert.That(visible.r, Is.GreaterThan(0.2f), visible.ToString());
                Assert.That(visible.r, Is.GreaterThan(visible.b * 2), "Particle vertex color must reach the shader.");
                Assert.That(image.GetPixel(90, 64).maxColorComponent, Is.LessThan(0.04f));
                system.SetCustomParticleData(new List<Vector4> { Vector4.one, Vector4.zero }, ParticleSystemCustomData.Custom1);
                var dissolved = Capture(camera, image);
                Assert.That(dissolved.maxColorComponent, Is.LessThan(0.04f), "Custom1.x must drive the material transition.");
                var second = image.GetPixel(90, 64);
                Assert.That(second.b, Is.GreaterThan(0.2f), "The second instance needs its own transform, color and custom coordinates.");
                Assert.That(second.b, Is.GreaterThan(second.r * 2));
                material.SetFloat("_AlphaTransitionProgressCoord", 2);
                system.SetCustomParticleData(new List<Vector4> { Vector4.zero, Vector4.one }, ParticleSystemCustomData.Custom2);
                Assert.That(Capture(camera, image).r, Is.GreaterThan(0.2f), "Custom2.x must independently control the first particle.");
                Assert.That(image.GetPixel(90, 64).maxColorComponent, Is.LessThan(0.04f));
            }
            finally
            {
                RenderTexture.active = active;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientLight = ambient;
                foreach (var obj in new UnityEngine.Object[] { cameraObject, particleObject, material, target, image, mesh })
                    UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        private static Color Capture(Camera camera, Texture2D image)
        {
            camera.Render();
            RenderTexture.active = camera.targetTexture;
            image.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            image.Apply();
            return image.GetPixel(38, 64);
        }

        private static void Setup(Material material, bool lit)
        {
            var name = lit ? "ParticlesUberLitMaterialPostProcessor" : "ParticlesUberUnlitMaterialPostProcessor";
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Editor.Core.Scripts." + name)).First(t => t != null);
            type.GetMethod("SetupMaterialKeywords").Invoke(null, new object[] { material });
            type.GetMethod("SetupMaterialBlendMode").Invoke(null, new object[] { material });
        }
    }
}

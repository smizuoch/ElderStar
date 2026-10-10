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
        [TestCase("Nova/Particles/UberUnlit")]
        [TestCase("Nova/Particles/UberLit")]
        [TestCase("Nova/UIParticles/UberUnlit")]
        [TestCase("Nova/UIParticles/UberLit")]
        public void BillboardParticlesUseVertexColorAndCustomTransitionProgress(string shaderName)
        {
            var cameraObject = new GameObject("Particle test camera", typeof(Camera));
            var particleObject = new GameObject("NOVA particles", typeof(ParticleSystem));
            var material = new Material(Shader.Find(shaderName));
            var target = new RenderTexture(128, 128, 24);
            var image = new Texture2D(128, 128, TextureFormat.RGBA32, false);
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
                renderer.SetActiveVertexStreams(new List<ParticleSystemVertexStream>
                {
                    ParticleSystemVertexStream.Position, ParticleSystemVertexStream.Normal,
                    ParticleSystemVertexStream.Color, ParticleSystemVertexStream.UV, ParticleSystemVertexStream.UV2,
                    ParticleSystemVertexStream.Custom1XYZW, ParticleSystemVertexStream.Custom2XYZW, ParticleSystemVertexStream.Tangent
                });
                system.Emit(new ParticleSystem.EmitParams
                {
                    position = Vector3.zero, velocity = Vector3.zero, startLifetime = 100,
                    startSize = 1, startColor = new Color(0.8f, 0.35f, 0.1f, 1)
                }, 1);
                system.Simulate(0.01f, true, false);
                system.SetCustomParticleData(new List<Vector4> { Vector4.zero }, ParticleSystemCustomData.Custom1);
                var visible = Capture(camera, image);
                Assert.That(visible.r, Is.GreaterThan(0.2f), visible.ToString());
                Assert.That(visible.r, Is.GreaterThan(visible.b * 2), "Particle vertex color must reach the shader.");
                system.SetCustomParticleData(new List<Vector4> { Vector4.one }, ParticleSystemCustomData.Custom1);
                var dissolved = Capture(camera, image);
                Assert.That(dissolved.maxColorComponent, Is.LessThan(0.04f), "Custom1.x must drive the material transition.");
            }
            finally
            {
                RenderTexture.active = active;
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientLight = ambient;
                foreach (var obj in new UnityEngine.Object[] { cameraObject, particleObject, material, target, image })
                    UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        private static Color Capture(Camera camera, Texture2D image)
        {
            camera.Render();
            RenderTexture.active = camera.targetTexture;
            image.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            image.Apply();
            return image.GetPixel(64, 64);
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

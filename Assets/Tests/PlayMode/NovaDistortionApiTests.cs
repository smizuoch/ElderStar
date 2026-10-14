using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaDistortionApiTests
    {
        [Test]
        public void DeferredCommandsKeepTheirOwnTexturesAndRenderSignedOffsets()
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Nova.Runtime.Core.Scripts.BuiltInDistortionPass")).FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, "The Built-in API must execute real command buffers.");
            var cameraObject = new GameObject("Distortion API camera", typeof(Camera));
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var material = new Material(Shader.Find("Nova/Particles/Distortion"));
            var gradient = new Texture2D(64, 2, TextureFormat.RGBAFloat, false, true);
            var offset = new Texture2D(1, 1, TextureFormat.RGBAFloat, false, true);
            var source = new RenderTexture(64, 64, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var offsets = new RenderTexture(64, 64, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            var neutral = new RenderTexture(64, 64, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            var result = new RenderTexture(64, 64, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var identity = new RenderTexture(64, 64, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var read = new Texture2D(64, 64, TextureFormat.RGBAFloat, false, true);
            var previous = RenderTexture.active;
            var pass = (IDisposable)Activator.CreateInstance(type);
            try
            {
                for (int y = 0; y < 2; y++) for (int x = 0; x < 64; x++)
                    gradient.SetPixel(x, y, new Color(x / 63f, 0, 0, 1));
                gradient.Apply(); Graphics.Blit(gradient, source);
                offset.SetPixel(0, 0, new Color(1, .5f, 0, 1)); offset.Apply();
                material.SetTexture("_BaseMap", offset); material.SetFloat("_Cull", 0);
                material.SetFloat("_DistortionIntensity", 1f); material.EnableKeyword("_BASE_MAP_MODE_2D");
                quad.GetComponent<Renderer>().sharedMaterial = material;
                var camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
                camera.transform.position = new Vector3(0, 0, -3); camera.orthographic = true; camera.orthographicSize = 1;
                using (var first = new CommandBuffer()) using (var second = new CommandBuffer())
                {
                    type.GetMethod("RecordOffsets").Invoke(pass, new object[] { first, camera, offsets, new[] { quad.GetComponent<Renderer>() } });
                    type.GetMethod("RecordComposite").Invoke(pass, new object[] { first, source, result, offsets });
                    type.GetMethod("RecordOffsets").Invoke(pass, new object[] { second, camera, neutral, Array.Empty<Renderer>() });
                    type.GetMethod("RecordComposite").Invoke(pass, new object[] { second, source, identity, neutral });
                    // Record both before executing: mutable material texture bindings must not leak.
                    Graphics.ExecuteCommandBuffer(second); Graphics.ExecuteCommandBuffer(first);
                }
                RenderTexture.active = identity; read.ReadPixels(new Rect(0, 0, 64, 64), 0, 0); read.Apply();
                float baseline = read.GetPixel(32, 32).r;
                Assert.That(baseline, Is.EqualTo(.5f).Within(.03f), "Zero offsets must preserve the source.");
                RenderTexture.active = result; read.ReadPixels(new Rect(0, 0, 64, 64), 0, 0); read.Apply();
                Assert.That(read.GetPixel(32, 32).r, Is.GreaterThan(baseline + .1f), "An explicitly registered renderer must distort.");
                Assert.That(read.GetPixel(4, 32).r, Is.EqualTo(4 / 63f).Within(.03f), "Uncovered pixels remain unchanged.");
            }
            finally
            {
                pass.Dispose(); RenderTexture.active = previous;
                foreach (var owned in new UnityEngine.Object[] { cameraObject, quad, material, gradient, offset, source, offsets, neutral, result, identity, read })
                    UnityEngine.Object.DestroyImmediate(owned);
            }
        }
    }
}

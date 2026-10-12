using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaBuiltInStereoTests
    {
        [TestCase("Nova/Particles/UberUnlit", false)]
        [TestCase("Nova/UIParticles/UberUnlit", false)]
        [TestCase("Nova/Particles/UberLit", false)]
        [TestCase("Nova/UIParticles/UberLit", false)]
        [TestCase("Nova/Particles/UberUnlit", true)]
        [TestCase("Nova/UIParticles/UberUnlit", true)]
        [TestCase("Nova/Particles/UberLit", true)]
        [TestCase("Nova/UIParticles/UberLit", true)]
        public void InstancedStereoWritesBothEyeSlices(string shaderName, bool softParticles)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var mesh = Object.Instantiate(quad.GetComponent<MeshFilter>().sharedMesh);
            mesh.colors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();
            var material = new Material(Shader.Find(shaderName)) { enableInstancing = true };
            var target = new RenderTexture(32, 32, 0) { dimension = TextureDimension.Tex2DArray, volumeDepth = 2 };
            var read = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var previous = RenderTexture.active;
            var stereoGlobals = new ComputeBuffer(68, 16, ComputeBufferType.Constant);
            var depth = new Texture2DArray(4, 4, 2, TextureFormat.RFloat, false, true);
            try
            {
                target.Create();
                material.SetFloat("_Cull", 0);
                material.SetFloat("_ZTest", 8);
                material.SetFloat("_BlendSrc", 1);
                material.SetFloat("_BlendDst", 0);
                material.SetTexture("_BaseMap", Texture2D.whiteTexture);
                material.EnableKeyword("_BASE_MAP_MODE_2D");
                material.EnableKeyword("STEREO_INSTANCING_ON");
                if (softParticles)
                {
                    depth.SetPixels(Enumerable.Repeat(Color.white, 16).ToArray(), 0);
                    depth.SetPixels(Enumerable.Repeat(Color.black, 16).ToArray(), 1); depth.Apply();
                    material.SetTexture("_CameraDepthTexture", depth);
                    material.SetFloat("_SoftParticlesIntensity", 10);
                    material.EnableKeyword("_SOFT_PARTICLES_ENABLED");
                }
                // Unity's built-in stereo constants normally come from the XR device.
                // Supply identity eye matrices so this GPU regression needs no headset.
                var constants = new Vector4[68];
                for (var m = 0; m < 16; m++)
                    for (var row = 0; row < 4; row++) constants[m * 4 + row] = Matrix4x4.identity.GetRow(row);
                constants[66] = constants[67] = new Vector4(1, 1, 0, 0);
                stereoGlobals.SetData(constants);
                using (var cmd = new CommandBuffer())
                {
                    cmd.SetRenderTarget(target, 0, CubemapFace.Unknown, -1);
                    cmd.ClearRenderTarget(false, true, Color.clear);
                    cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                    foreach (var name in new[] { "unity_StereoMatrixVP", "unity_StereoMatrixV", "unity_StereoMatrixP" })
                        cmd.SetGlobalMatrixArray(name, new[] { Matrix4x4.identity, Matrix4x4.identity });
                    cmd.EnableShaderKeyword("STEREO_INSTANCING_ON");
                    cmd.SetGlobalVector("_ZBufferParams", new Vector4(999, 1, 9.99f, .01f));
                    cmd.SetGlobalConstantBuffer(stereoGlobals, Shader.PropertyToID("UnityStereoGlobals"), 0, 68 * 16);
                    cmd.DrawMeshInstanced(mesh, 0, material,
                        material.FindPass("ForwardBase"), new[] { Matrix4x4.identity, Matrix4x4.identity }, 2);
                    cmd.DisableShaderKeyword("STEREO_INSTANCING_ON");
                    Graphics.ExecuteCommandBuffer(cmd);
                }
                for (var eye = 0; eye < 2; eye++)
                {
                    Graphics.SetRenderTarget(target, 0, CubemapFace.Unknown, eye);
                    read.ReadPixels(new Rect(0, 0, 32, 32), 0, 0);
                    read.Apply();
                    if (softParticles && eye == 0)
                        Assert.That(read.GetPixels().Max(c => c.a), Is.LessThan(.05f), "Near geometry fades only the left eye.");
                    else Assert.That(read.GetPixels().Count(c => c.a > 0.9f), Is.GreaterThan(100),
                        shaderName + " eye " + eye + " must receive the particle, including its fragment alpha.");
                }
            }
            finally
            {
                Shader.DisableKeyword("STEREO_INSTANCING_ON");
                Shader.SetGlobalConstantBuffer("UnityStereoGlobals", (ComputeBuffer)null, 0, 0);
                stereoGlobals.Dispose();
                RenderTexture.active = previous;
                foreach (var obj in new Object[] { quad, mesh, material, target, read, depth }) Object.DestroyImmediate(obj);
            }
        }
    }
}

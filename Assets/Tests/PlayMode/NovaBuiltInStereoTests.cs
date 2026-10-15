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
        [TestCase("Nova/Particles/Distortion", false)]
        [TestCase("Nova/Particles/UberUnlit", true)]
        [TestCase("Nova/UIParticles/UberUnlit", true)]
        [TestCase("Nova/Particles/UberLit", true)]
        [TestCase("Nova/UIParticles/UberLit", true)]
        [TestCase("Nova/Particles/UberUnlit", false, "DepthNormals")]
        [TestCase("Nova/Particles/UberLit", false, "DepthNormals")]
        [TestCase("Nova/Particles/UberUnlit", false, "DepthOnly")]
        [TestCase("Nova/Particles/UberLit", false, "DepthOnly")]
        [TestCase("Nova/Particles/UberUnlit", false, "ShadowCaster")]
        [TestCase("Nova/Particles/UberLit", false, "ShadowCaster")]
        public void InstancedStereoWritesBothEyeSlices(string shaderName, bool softParticles, string depthPass = null)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var mesh = Object.Instantiate(quad.GetComponent<MeshFilter>().sharedMesh);
            mesh.colors = Enumerable.Repeat(Color.white, mesh.vertexCount).ToArray();
            if (shaderName.EndsWith("/Distortion")) mesh.vertices = mesh.vertices.Select(v => v + Vector3.forward * .5f).ToArray();
            var material = new Material(Shader.Find(shaderName.EndsWith("/Distortion")
                ? "Hidden/NovaTests/StereoDistortionProbe" : shaderName)) { enableInstancing = true };
            var target = new RenderTexture(32, 32, 24) { dimension = TextureDimension.Tex2DArray, volumeDepth = 2 };
            var read = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var previous = RenderTexture.active;
            var stereoGlobals = new ComputeBuffer(68, 16, ComputeBufferType.Constant);
            var depth = new Texture2DArray(4, 4, 2, TextureFormat.RFloat, false, true);
            var grab = new Texture2DArray(4, 4, 2, TextureFormat.RGBA32, false, true);
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
                var distortion = shaderName.EndsWith("/Distortion");
                if (distortion)
                {
                    grab.SetPixels(Enumerable.Repeat(Color.red, 16).ToArray(), 0);
                    grab.SetPixels(Enumerable.Repeat(Color.green, 16).ToArray(), 1); grab.Apply();
                    material.SetTexture("_NovaDistortionGrabTexture", grab);
                    // Exercise a visible distortion sample: zero offsets intentionally discard.
                    material.SetFloat("_DistortionIntensity", .1f);
                }
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
                #if UNITY_EDITOR
                var compilePass = material.FindPass(distortion ? "AvatarDistortion" : "ForwardBase");
                UnityEditor.ShaderUtil.CompilePass(material, compilePass, true);
                Assert.That(UnityEditor.ShaderUtil.GetShaderMessages(material.shader).Where(m => m.severity.ToString() == "Error").Select(m => m.message), Is.Empty);
                #endif
                using (var cmd = new CommandBuffer())
                {
                    cmd.SetRenderTarget(target, 0, CubemapFace.Unknown, -1);
                    cmd.ClearRenderTarget(true, true, Color.clear);
                    cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                    foreach (var name in new[] { "unity_StereoMatrixVP", "unity_StereoMatrixV", "unity_StereoMatrixP" })
                        cmd.SetGlobalMatrixArray(name, new[] { Matrix4x4.identity, Matrix4x4.identity });
                    cmd.EnableShaderKeyword("STEREO_INSTANCING_ON");
                    cmd.SetSinglePassStereo(SinglePassStereoMode.Instancing);
                    cmd.SetGlobalVector("_ZBufferParams", new Vector4(999, 1, 9.99f, .01f));
                    cmd.SetGlobalConstantBuffer(stereoGlobals, Shader.PropertyToID("UnityStereoGlobals"), 0, 68 * 16);
                    // With identity projection, z=0 is the far plane in reversed Z.
                    // Place soft particles between the near/far depth slices.
                    var drawMatrix = softParticles ? Matrix4x4.Translate(new Vector3(0,0,.5f)) : Matrix4x4.identity;
                    if (depthPass != null)
                    {
                        var near = Matrix4x4.Translate(new Vector3(0, 0, SystemInfo.usesReversedZBuffer ? .75f : .25f));
                        cmd.DrawMeshInstanced(mesh, 0, material, material.FindPass(depthPass), new[] { near, near }, 2);
                        material.SetFloat("_ZTest", 4);
                        drawMatrix = Matrix4x4.Translate(new Vector3(0, 0, SystemInfo.usesReversedZBuffer ? .25f : .75f));
                    }
                    cmd.DrawMeshInstanced(mesh, 0, material,
                        material.FindPass(distortion ? "AvatarDistortion" : "ForwardBase"), new[] { drawMatrix, drawMatrix }, 2);
                    cmd.DisableShaderKeyword("STEREO_INSTANCING_ON");
                    cmd.SetSinglePassStereo(SinglePassStereoMode.None);
                    Graphics.ExecuteCommandBuffer(cmd);
                }
                for (var eye = 0; eye < 2; eye++)
                {
                    Graphics.SetRenderTarget(target, 0, CubemapFace.Unknown, eye);
                    read.ReadPixels(new Rect(0, 0, 32, 32), 0, 0);
                    read.Apply();
                    if (distortion)
                        Assert.That(Vector4.Distance(read.GetPixel(16, 16), eye == 0 ? Color.red : Color.green), Is.LessThan(.01f),
                            "Eye " + eye + " sampled " + read.GetPixel(16, 16) + "; camera compositor flag=" + Shader.GetGlobalFloat("_NovaCameraDistortionActive"));
                    if (depthPass == "DepthNormals")
                    {
                        var normal = read.GetPixel(16,16);
                        Assert.That(normal.a, Is.GreaterThan(.95f), "Normal coverage in eye " + eye);
                        Assert.That(normal.r, Is.EqualTo(.5f).Within(.01f));
                        Assert.That(normal.g, Is.EqualTo(.5f).Within(.01f));
                        Assert.That(normal.b, Is.LessThan(.01f), "The encoded normal, not the occluded white color pass, must remain.");
                    }
                    else if (depthPass != null)
                        Assert.That(read.GetPixel(16, 16).a, Is.LessThan(.05f), depthPass + " must occlude the later geometry in eye " + eye);
                    else if (softParticles && eye == 0)
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
                foreach (var obj in new Object[] { quad, mesh, material, target, read, depth, grab }) Object.DestroyImmediate(obj);
            }
        }
    }
}

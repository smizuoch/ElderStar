using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaBuiltInDistortionEditorPassTests
    {
        [TestCase("SceneSelectionPass")]
        [TestCase("Picking")]
        public void DistortionEditorPassWritesSelectionIdentifier(string passName)
        {
            var material = new Material(Shader.Find("Nova/Particles/Distortion"));
            var mesh = new Mesh();
            var target = new RenderTexture(32, 32, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var image = new Texture2D(32, 32, TextureFormat.RGBA32, false, true);
            var active = RenderTexture.active;
            try
            {
                var pass = material.FindPass(passName);
                Assert.That(pass, Is.GreaterThanOrEqualTo(0), passName);
                mesh.vertices = new[] { new Vector3(-1, -1), new Vector3(1, -1), new Vector3(1, 1), new Vector3(-1, 1) };
                mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
                material.SetFloat("_ObjectId", 0.25f);
                material.SetFloat("_PassValue", 0.75f);
                material.SetVector("_SelectionID", new Vector4(0.7f, 0.2f, 0.4f, 1));
                target.Create();
                using (var commands = new CommandBuffer())
                {
                    commands.SetRenderTarget(target);
                    commands.ClearRenderTarget(true, true, Color.black);
                    commands.SetViewProjectionMatrices(Matrix4x4.identity,
                        GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-1, 1, -1, 1, 0.1f, 10), true));
                    commands.DrawMesh(mesh, Matrix4x4.Translate(new Vector3(0, 0, -1)), material, 0, pass);
                    Graphics.ExecuteCommandBuffer(commands);
                }
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 32, 32), 0, 0);
                image.Apply();
                var expected = passName == "Picking" ? new Color(0.7f, 0.2f, 0.4f, 1) : new Color(0.25f, 0.75f, 1, 1);
                Assert.That(Vector4.Distance(image.GetPixel(16, 16), expected), Is.LessThan(0.02f));
            }
            finally
            {
                RenderTexture.active = active;
                foreach (var obj in new Object[] { material, mesh, target, image }) Object.DestroyImmediate(obj);
            }
        }
    }
}

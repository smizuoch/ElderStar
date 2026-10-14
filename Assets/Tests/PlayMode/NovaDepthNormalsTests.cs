using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaDepthNormalsTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void ArrayNormalProgressMatchesOriginalDepthPass(bool alphaTest)
        {
            var material = new Material(Shader.Find("Nova/Particles/UberLit"));
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var array = new Texture2DArray(1,1,2,TextureFormat.RGBAFloat,false,true);
            var target = new RenderTexture(32,32,24,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
            var read = new Texture2D(32,32,TextureFormat.RGBAFloat,false,true);
            var previous = RenderTexture.active;
            try
            {
                array.SetPixels(new[] { new Color(.5f,.5f,1,1) },0);
                array.SetPixels(new[] { new Color(1,.5f,1,1) },1); array.Apply();
                material.SetTexture("_NormalMap2DArray",array);
                material.SetFloat("_BaseMapSliceCount",2);material.SetFloat("_BaseMapProgress",1);
                material.SetFloat("_Cull",0);material.SetFloat("_Cutoff",0);
                material.EnableKeyword("_BASE_MAP_MODE_2D_ARRAY"); material.EnableKeyword("_NORMAL_MAP_ENABLED");
                if (alphaTest) material.EnableKeyword("_ALPHATEST_ENABLED");
                using (var command = new CommandBuffer())
                {
                    command.SetRenderTarget(target);command.ClearRenderTarget(true,true,Color.clear);
                    command.SetViewProjectionMatrices(Matrix4x4.identity,Matrix4x4.identity);
                    command.DrawMesh(quad.GetComponent<MeshFilter>().sharedMesh,Matrix4x4.identity,material,0,material.FindPass("DepthNormals"));
                    Graphics.ExecuteCommandBuffer(command);
                }
                RenderTexture.active=target;read.ReadPixels(new Rect(0,0,32,32),0,0);read.Apply();
                var sample=read.GetPixel(16,16);
                Assert.That(sample.a,Is.GreaterThan(.99f));
                // Original InitializeVertexOutputDrawDepth leaves progress zero for opaque objects.
                Assert.That(sample.r,Is.EqualTo(alphaTest ? 1 : .5f).Within(.01f));
                Assert.That(sample.b,Is.EqualTo(alphaTest ? .5f : 0).Within(.01f));
            }
            finally
            {
                RenderTexture.active=previous;
                foreach(var owned in new UnityEngine.Object[]{quad,material,array,target,read})UnityEngine.Object.DestroyImmediate(owned);
            }
        }

        [UnityTest]
        public IEnumerator OptionalCameraBufferRespectsWorldOcclusionAndReleasesOnDisable()
        {
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Nova.Runtime.Core.Scripts.NovaDepthNormals")).FirstOrDefault(t=>t!=null);
            Assert.That(type,Is.Not.Null,"The optional camera buffer must have an executable implementation.");
            var cameraObject=new GameObject("NOVA normals camera",typeof(Camera));
            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);
            var material=new Material(Shader.Find("Nova/Particles/UberUnlit"));
            var wallMaterial=new Material(Shader.Find("Standard"));
            var target=new RenderTexture(64,64,24);
            var read=new Texture2D(64,64,TextureFormat.RGBAFloat,false,true);var previous=RenderTexture.active;
            try
            {
                var camera=cameraObject.GetComponent<Camera>(); camera.enabled=false;
                camera.transform.position=new Vector3(0,0,-3);camera.orthographic=true;camera.orthographicSize=1;camera.targetTexture=target;
                material.SetFloat("_RenderType",0);material.SetFloat("_Cull",0);
                quad.GetComponent<Renderer>().sharedMaterial=material;
                wall.transform.position=new Vector3(-.25f,0,-1);wall.transform.localScale=new Vector3(.5f,2,.2f);
                wall.GetComponent<Renderer>().sharedMaterial=wallMaterial;
                var helper=(Behaviour)cameraObject.AddComponent(type);
                type.GetMethod("RegisterRenderer").Invoke(helper,new object[]{quad.GetComponent<Renderer>()});
                yield return null;camera.Render();
                var normals=(RenderTexture)type.GetProperty("Texture").GetValue(helper);
                Assert.That(normals,Is.Not.Null);Assert.That(normals.IsCreated(),Is.True);
                RenderTexture.active=normals;read.ReadPixels(new Rect(0,0,64,64),0,0);read.Apply();
                Assert.That(read.GetPixel(24,32).a,Is.LessThan(.01f),"Opaque world geometry must occlude NOVA normals.");
                Assert.That(read.GetPixel(40,32).a,Is.GreaterThan(.99f),"Unoccluded NOVA normal must remain valid.");
                RenderTexture.active=previous;
                yield return null;camera.Render();
                Assert.That(type.GetProperty("Texture").GetValue(helper),Is.SameAs(normals),"Repeated explicit renders reuse the allocation.");
                camera.enabled=true;yield return null;
                camera.enabled=false;yield return null;
                Assert.That(type.GetProperty("Texture").GetValue(helper),Is.Null,"Disabling an automatic camera releases its buffer.");
                camera.Render();
                helper.enabled=false;yield return null;
                Assert.That(type.GetProperty("Texture").GetValue(helper),Is.Null);
                Assert.That(normals==null || !normals.IsCreated(),Is.True);
                Assert.That(camera.GetCommandBuffers(CameraEvent.AfterForwardOpaque).Any(c=>c.name=="NOVA Depth Normals"),Is.False);
            }
            finally
            {
                RenderTexture.active=previous;
                foreach(var owned in new UnityEngine.Object[]{cameraObject,quad,wall,material,wallMaterial,target,read}) UnityEngine.Object.DestroyImmediate(owned);
            }
        }

        [TestCase("Nova/Particles/UberUnlit", false)]
        [TestCase("Nova/Particles/UberUnlit", true)]
        [TestCase("Nova/Particles/UberLit", false)]
        [TestCase("Nova/Particles/UberLit", true)]
        public void ExplicitPassWritesNormalAndValidityAndRespectsAlphaClip(string shaderName, bool clipped)
        {
            var material = new Material(Shader.Find(shaderName));
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var cameraObject = new GameObject("Depth normal test camera", typeof(Camera));
            var target = new RenderTexture(32,32,24,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
            var read = new Texture2D(32,32,TextureFormat.RGBAFloat,false,true);
            var texture = new Texture2D(1,1); texture.SetPixel(0,0,new Color(1,1,1,clipped ? 0 : 1)); texture.Apply();
            var previous = RenderTexture.active;
            try
            {
                material.SetTexture("_BaseMap",texture); material.SetFloat("_RenderType",1);
                material.SetFloat("_Cutoff",.5f); material.SetFloat("_Cull",0);
                var processor = AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Nova.Editor.Core.Scripts.ParticlesUber" + (shaderName.EndsWith("UberLit") ? "Lit" : "Unlit") + "MaterialPostProcessor")).First(t=>t!=null);
                processor.GetMethod("SetupMaterialKeywords").Invoke(null,new object[] { material });
                processor.GetMethod("SetupMaterialBlendMode").Invoke(null,new object[] { material });
                Assert.That(material.IsKeywordEnabled("_ALPHATEST_ENABLED"),Is.True);
                var pass = material.FindPass("DepthNormals");
                Assert.That(pass,Is.GreaterThanOrEqualTo(0),"An executable pass is required, not only an optimizer enum value.");
                var camera = cameraObject.GetComponent<Camera>(); camera.enabled=false;
                camera.transform.position = new Vector3(0,0,-2); camera.orthographic=true; camera.orthographicSize=1;
                using (var command = new CommandBuffer())
                {
                    command.SetRenderTarget(target); command.ClearRenderTarget(true,true,Color.clear);
                    command.SetViewProjectionMatrices(camera.worldToCameraMatrix,GL.GetGPUProjectionMatrix(camera.projectionMatrix,true));
                    command.DrawMesh(quad.GetComponent<MeshFilter>().sharedMesh,Matrix4x4.identity,material,0,pass);
                    Graphics.ExecuteCommandBuffer(command);
                }
                RenderTexture.active=target; read.ReadPixels(new Rect(0,0,32,32),0,0); read.Apply();
                var sample=read.GetPixel(16,16);
                Assert.That(sample.a,Is.EqualTo(clipped ? 0 : 1).Within(.01f),"Coverage must distinguish clipped/empty pixels.");
                if (!clipped)
                {
                    Assert.That(sample.r,Is.EqualTo(.5f).Within(.01f));
                    Assert.That(sample.g,Is.EqualTo(.5f).Within(.01f));
                    Assert.That(sample.b,Is.EqualTo(0).Within(.01f));
                }
            }
            finally
            {
                RenderTexture.active=previous;
                foreach(var owned in new UnityEngine.Object[] {quad,cameraObject,material,target,read,texture}) UnityEngine.Object.DestroyImmediate(owned);
            }
        }
    }
}

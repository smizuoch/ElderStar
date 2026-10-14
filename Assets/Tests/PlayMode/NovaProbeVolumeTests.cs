using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaProbeVolumeTests
    {
        [TestCase("Nova/Particles/UberLit",false,true)]
        [TestCase("Nova/Particles/UberLit",true,true)]
        [TestCase("Nova/Particles/UberLit",false,false)]
        [TestCase("Nova/Particles/UberLit",true,false)]
        [TestCase("Nova/UIParticles/UberLit",false,true)]
        [TestCase("Nova/UIParticles/UberLit",true,true)]
        [TestCase("Nova/UIParticles/UberLit",false,false)]
        [TestCase("Nova/UIParticles/UberLit",true,false)]
        public void ProbeVolumeChangesIndirectLightAcrossOneRenderer(string shaderName, bool localSpace, bool enabled)
        {
            var material = new Material(Shader.Find(shaderName));
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var texture = new Texture3D(32,2,2,TextureFormat.RGBAFloat,false) { filterMode=FilterMode.Bilinear, wrapMode=TextureWrapMode.Clamp };
            var pixels = new Color[128];
            for (int z=0;z<2;z++) for (int y=0;y<2;y++) for(int x=0;x<32;x++)
            {
                float t=(x%8)/7f;
                pixels[x+32*(y+2*z)]=new Color(0,0,0,x<8 ? Mathf.Lerp(.8f,.05f,t) : x<16 ? Mathf.Lerp(.05f,.8f,t) : 0);
            }
            texture.SetPixels(pixels); texture.Apply();
            var constants=new ComputeBuffer(7,16,ComputeBufferType.Constant);
            var target=new RenderTexture(64,32,24,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
            var read=new Texture2D(64,32,TextureFormat.RGBAFloat,false,true);
            var previous=RenderTexture.active;
            var previousLight=Shader.GetGlobalVector("_LightColor0");
            var previousGlossy=Shader.GetGlobalVector("unity_IndirectSpecColor");
            var propertyBlock=new MaterialPropertyBlock();
            var sh=new SphericalHarmonicsL2(); sh.AddAmbientLight(new Color(0,0,2));
            propertyBlock.CopySHCoefficientArraysFrom(new[]{sh});
            try
            {
                material.SetFloat("_LitWorkflowMode",1); material.SetFloat("_Metallic",0);
                material.SetFloat("_SpecularHighlights",0); material.SetFloat("_EnvironmentReflections",0);
                material.SetFloat("_RenderType",0); material.SetFloat("_Cull",0);
                var processor=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor")).First(t=>t!=null);
                foreach(var method in new[]{"SetupMaterialKeywords","SetupMaterialBlendMode"}) processor.GetMethod(method).Invoke(null,new object[]{material});
                material.EnableKeyword("LIGHTPROBE_SH");
                propertyBlock.SetTexture("unity_ProbeVolumeSH",texture);
                var data=new Vector4[7]; data[0]=new Vector4(enabled ? 1 : 0,localSpace ? 1 : 0,1f/32,0);
                for(int row=0;row<4;row++) data[row+1]=(localSpace ? Matrix4x4.Scale(new Vector3(-1,1,1)) : Matrix4x4.identity).GetRow(row);
                data[5]=Vector3.one; data[6]=new Vector4(-.5f,-.5f,-.5f,0); constants.SetData(data);
                using(var command=new CommandBuffer())
                {
                    command.SetRenderTarget(target); command.ClearRenderTarget(true,true,Color.clear);
                    command.SetViewProjectionMatrices(Matrix4x4.identity,Matrix4x4.identity);
                    command.SetGlobalVector("_LightColor0",Vector4.zero);
                    // This assertion isolates diffuse proxy replacement. Original
                    // NOVA keeps glossy DC with environment reflection maps disabled.
                    command.SetGlobalVector("unity_IndirectSpecColor",Vector4.zero);
                    command.SetGlobalConstantBuffer(constants,Shader.PropertyToID("UnityProbeVolume"),0,7*16);
                    command.DrawMesh(quad.GetComponent<MeshFilter>().sharedMesh,Matrix4x4.identity,material,0,material.FindPass("ForwardBase"),propertyBlock);
                    Graphics.ExecuteCommandBuffer(command);
                }
                RenderTexture.active=target;read.ReadPixels(new Rect(0,0,64,32),0,0);read.Apply();
                var left=read.GetPixel(20,16);var right=read.GetPixel(43,16);
                Assert.That(left.a,Is.GreaterThan(.9f)); Assert.That(right.a,Is.GreaterThan(.9f));
                if (enabled)
                {
                    if (localSpace) { var swap=left;left=right;right=swap; }
                    Assert.That(left.r,Is.GreaterThan(right.r+.3f),"Spatial red probe gradient: "+left+" / "+right);
                    Assert.That(right.g,Is.GreaterThan(left.g+.3f),"Spatial green probe gradient: "+left+" / "+right);
                    Assert.That(left.b,Is.LessThan(.01f),"Ordinary blue SH must not be added to proxy L0/L1.");
                }
                else
                {
                    Assert.That(left.b,Is.GreaterThan(.5f),"Disabled proxy must use the ordinary SH.");
                    Assert.That(Vector4.Distance(left,right),Is.LessThan(.01f));
                }
            }
            finally
            {
                RenderTexture.active=previous;
                Shader.SetGlobalConstantBuffer("UnityProbeVolume",(ComputeBuffer)null,0,0);
                Shader.SetGlobalVector("_LightColor0",previousLight);
                Shader.SetGlobalVector("unity_IndirectSpecColor",previousGlossy);
                constants.Dispose();
                foreach(var owned in new UnityEngine.Object[]{quad,material,texture,target,read}) UnityEngine.Object.DestroyImmediate(owned);
            }
        }
    }
}

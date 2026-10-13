using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
public sealed class NovaSamplerSharingResearch
{
 [Test]
 public void ExportSamplerProgram()
 {
   var data=UnityEditor.ShaderUtil.GetShaderData(Shader.Find("Hidden/NovaResearch/SamplerSharing"));
   var compiled=data.GetSubshader(0).GetPass(0).CompileVariant(UnityEditor.Rendering.ShaderType.Fragment,
     new string[0],UnityEditor.Rendering.ShaderCompilerPlatform.D3D,UnityEditor.BuildTarget.StandaloneWindows64,true);
   Assert.That(compiled.Success,Is.True,string.Join(";",compiled.Messages.Select(m=>m.message)));
   var args=Environment.GetCommandLineArgs();var index=Array.IndexOf(args,"-novaShaderBinary");
   Assert.That(index,Is.GreaterThanOrEqualTo(0));
   System.IO.File.WriteAllBytes(args[index+1],compiled.ShaderData);
 }
 [TestCase(2,false)] [TestCase(9,false)] [TestCase(16,false)]
 [TestCase(2,true)] [TestCase(9,true)] [TestCase(16,true)]
 public void BorrowedSamplerMatchesTextureSetting(int level,bool useRenderTexture)
 {
   var old=QualitySettings.anisotropicFiltering;
   var active=RenderTexture.active;
   var tex=new Texture2D(512,512,TextureFormat.RGBA32,false,true);
   var donor=new Texture2D(2,2,TextureFormat.RGBA32,false,true);
   var sourceRT=new RenderTexture(512,512,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
   var target=new RenderTexture(64,64,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
   var cpu=new Texture2D(64,64,TextureFormat.RGBA32,false,true);
   var material=new Material(Shader.Find("Hidden/NovaResearch/SamplerSharing"));
   try {
     Assert.That(material.shader.isSupported,Is.True);
     QualitySettings.anisotropicFiltering=AnisotropicFiltering.Enable;
     var r=new System.Random(20261013);
     tex.SetPixels32(Enumerable.Range(0,512*512).Select(_=>new Color32((byte)r.Next(256),(byte)r.Next(256),(byte)r.Next(256),255)).ToArray());tex.Apply();
     tex.wrapMode=sourceRT.wrapMode=donor.wrapMode=TextureWrapMode.Clamp;
     tex.filterMode=sourceRT.filterMode=donor.filterMode=FilterMode.Bilinear;
     tex.anisoLevel=sourceRT.anisoLevel=donor.anisoLevel=level;donor.Apply();
     Graphics.Blit(tex,sourceRT);
     Texture source=useRenderTexture ? (Texture)sourceRT : tex;
     material.SetTexture("_MainTex",source);material.SetTexture("_SamplerDonor",donor);
     Color32[] Render(int mode) {
       material.SetFloat("_Mode",mode);Graphics.Blit(source,target,material);
       RenderTexture.active=target;cpu.ReadPixels(new Rect(0,0,64,64),0,0);cpu.Apply();return cpu.GetPixels32();
     }
     int Difference(Color32[] a,Color32[] b)=>a.Zip(b,(x,y)=>Math.Max(Math.Abs(x.r-y.r),Math.Max(Math.Abs(x.g-y.g),Math.Abs(x.b-y.b)))).Max();
     var max=0;var inlineMax=0;var control=0;
     foreach(var footprint in new[]{new Vector4(1,0,0,.05f),new Vector4(.7f,.7f,-.04f,.04f),new Vector4(3,0,0,.15f)}) {
       material.SetVector("_Footprint",footprint);donor.anisoLevel=level;
       var a=Render(0);var b=Render(1);var c=Render(2);
       max=Math.Max(max,Difference(a,b));inlineMax=Math.Max(inlineMax,Difference(a,c));
       donor.anisoLevel=1;control=Math.Max(control,Difference(a,Render(1)));
     }
     Debug.Log("NOVA_SAMPLER_RESEARCH unity="+Application.unityVersion+" device="+SystemInfo.graphicsDeviceName+" api="+SystemInfo.graphicsDeviceType+" level="+level+" renderTexture="+useRenderTexture+" sharedMax="+max+" inline8Max="+inlineMax+" controlMax="+control);
     Assert.That(UnityEditor.ShaderUtil.GetShaderMessages(material.shader).Where(m=>m.severity.ToString()=="Error"),Is.Empty);
     Assert.That(control,Is.GreaterThan(0),"The comparison must actually respond to sampler settings.");
     Assert.That(max,Is.EqualTo(0),"Borrowed texture sampler must reproduce the source sampler bit-for-bit.");
   } finally {
     QualitySettings.anisotropicFiltering=old;RenderTexture.active=active;
     foreach(var item in new UnityEngine.Object[]{material,tex,donor,sourceRT,target,cpu})UnityEngine.Object.DestroyImmediate(item);
   }
 }
}
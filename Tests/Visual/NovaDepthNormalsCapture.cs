// Identical explicit-pass fixture; define NOVA_VISUAL_REFERENCE only in the original project.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaDepthNormalsCapture
{
    [Test]
    public void CaptureDepthNormals()
    {
        var args=Environment.GetCommandLineArgs();
        var output=args[Array.IndexOf(args,"-novaVisualOutput")+1];Directory.CreateDirectory(output);
        var owned=new List<UnityEngine.Object>();var previous=RenderTexture.active;
        var cameraObject=new GameObject("Depth normal comparison",typeof(Camera));owned.Add(cameraObject);
        var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);owned.Add(quad);
        var mesh=UnityEngine.Object.Instantiate(quad.GetComponent<MeshFilter>().sharedMesh);owned.Add(mesh);
        mesh.colors=Enumerable.Repeat(Color.white,4).ToArray();
        mesh.normals=new[]{new Vector3(.3f,.2f,-1).normalized,new Vector3(-.3f,.2f,-1).normalized,new Vector3(.3f,-.2f,-1).normalized,new Vector3(-.3f,-.2f,-1).normalized};
        mesh.SetUVs(1,Enumerable.Repeat(new Vector4(.6f,.15f,0,0),4).ToList());
        var target=new RenderTexture(64,64,24,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);owned.Add(target);
        var read=new Texture2D(64,64,TextureFormat.RGBAFloat,false,true);owned.Add(read);
        var png=new Texture2D(64,64,TextureFormat.RGBA32,false,true);owned.Add(png);
        var normal=new Texture2D(16,1,TextureFormat.RGBAFloat,false,true){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Repeat};owned.Add(normal);
        var albedo=new Texture2D(16,1,TextureFormat.RGBAFloat,false,true){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Repeat};owned.Add(albedo);
        var flow=new Texture2D(1,1,TextureFormat.RGBAFloat,false,true);owned.Add(flow);
        var array=new Texture2DArray(16,1,2,TextureFormat.RGBAFloat,false,true){filterMode=FilterMode.Point};owned.Add(array);
        for(int x=0;x<16;x++){normal.SetPixel(x,0,new Color(x<8 ? .5f : .8f,.5f,1,1));albedo.SetPixel(x,0,new Color(1,0,0,x<4 ? .2f : 1));}
        normal.Apply();albedo.Apply();flow.SetPixel(0,0,new Color(1,.5f,0,1));flow.Apply();
        array.SetPixels(Enumerable.Repeat(new Color(.5f,.5f,1,1),16).ToArray(),0);
        array.SetPixels(Enumerable.Repeat(new Color(.8f,.5f,1,1),16).ToArray(),1);array.Apply();
        try
        {
            var camera=cameraObject.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=.8f;
            camera.transform.position=new Vector3(0,0,-3);camera.nearClipPlane=.1f;camera.farClipPlane=10;
            foreach(var kind in new[]{"UberUnlit","UberLit"}) foreach(bool clip in new[]{false,true}) foreach(bool negative in new[]{false,true})
            foreach(var feature in new[]{"plain","normal","array","legacy-flow","target-flow","channel-luminance","deform","custom-alpha"})
            {
                var material=new Material(Shader.Find("Nova/Particles/"+kind));owned.Add(material);
                material.SetFloat("_Cull",0);material.SetFloat("_Cutoff",.5f);material.SetFloat("_RenderType",clip ? 1 : 0);
                material.SetTexture("_BaseMap",albedo);material.EnableKeyword("_BASE_MAP_MODE_2D");
                if(clip)material.EnableKeyword("_ALPHATEST_ENABLED");
                if(feature=="normal" || feature=="array" || feature.EndsWith("flow"))
                {
                    material.SetTexture("_NormalMap",normal);material.EnableKeyword("_NORMAL_MAP_ENABLED");
                }
                if(feature=="array")
                {
                    material.DisableKeyword("_BASE_MAP_MODE_2D");material.EnableKeyword("_BASE_MAP_MODE_2D_ARRAY");
                    material.SetTexture("_NormalMap2DArray",array);material.SetTexture("_BaseMap2DArray",array);
                    material.SetFloat("_BaseMapSliceCount",2);material.SetFloat("_BaseMapProgress",1);
                }
                if(feature.EndsWith("flow"))
                {
                    material.SetTexture("_FlowMap",flow);material.SetFloat("_FlowIntensity",.4f);
                    material.EnableKeyword(feature=="legacy-flow" ? "_FLOW_MAP_ENABLED" : "_FLOW_MAP_TARGET_BASE");
                }
                if(feature=="channel-luminance")
                {
                    material.SetFloat("_BaseMapChannel",1);material.EnableKeyword("_BASE_MAP_CHANNEL_ENABLED");
                    material.SetFloat("_LuminanceTransparencyProgress",.5f);material.SetFloat("_LuminanceTransparencySharpness",1);
                    material.EnableKeyword("_TRANSPARENCY_BY_LUMINANCE");
                }
                if(feature=="deform")
                {
                    material.SetTexture("_VertexDeformationMap",Texture2D.whiteTexture);material.SetFloat("_VertexDeformationIntensity",.2f);
                    material.SetFloat("_VertexDeformationIntensityCoord",1);material.EnableKeyword("_VERTEX_DEFORMATION_ENABLED");
                }
                if(feature=="custom-alpha")
                {
                    material.SetFloat("_BaseMapOffsetXCoord",1);material.SetFloat("_BaseMapOffsetYCoord",2);
                    material.SetTexture("_AlphaTransitionMap",albedo);material.SetFloat("_AlphaTransitionProgressCoord",1);
                    material.EnableKeyword("_ALPHA_TRANSITION_MAP_MODE_2D");material.EnableKeyword("_FADE_TRANSITION_ENABLED");
                }
                int pass=-1;
                for(int p=0;p<material.passCount;p++)if(string.Equals(material.shader.FindPassTagValue(p,new ShaderTagId("LightMode")).name,"DepthNormals",StringComparison.OrdinalIgnoreCase)){pass=p;break;}
                Assert.That(pass,Is.GreaterThanOrEqualTo(0));
                #if UNITY_EDITOR
                UnityEditor.ShaderUtil.CompilePass(material,pass,true);
                Assert.That(UnityEditor.ShaderUtil.GetShaderMessages(material.shader).Where(m=>m.severity.ToString()=="Error").Select(m=>m.message),Is.Empty);
                #endif
                using(var command=new CommandBuffer())
                {
                    command.SetRenderTarget(target);command.ClearRenderTarget(true,true,Color.clear);
                    command.SetViewProjectionMatrices(camera.worldToCameraMatrix,GL.GetGPUProjectionMatrix(camera.projectionMatrix,true));
                    command.SetGlobalVector("_WorldSpaceCameraPos",camera.transform.position);
                    command.DrawMesh(mesh,Matrix4x4.Scale(new Vector3(negative ? -1.1f : 1.1f,.8f,1.4f)),material,0,pass);
                    Graphics.ExecuteCommandBuffer(command);
                }
                RenderTexture.active=target;read.ReadPixels(new Rect(0,0,64,64),0,0);read.Apply();var pixels=read.GetPixels();
                #if NOVA_VISUAL_REFERENCE
                for(int i=0;i<pixels.Length;i++)
                {
                    var c=pixels[i];bool valid=new Vector3(c.r,c.g,c.b).sqrMagnitude>.01f;
                    pixels[i]=valid ? new Color(c.r*.5f+.5f,c.g*.5f+.5f,c.b*.5f+.5f,1) : Color.clear;
                }
                #endif
                if(feature=="plain" && !clip)Assert.That(pixels.Count(c=>c.a>.5f),Is.GreaterThan(100));
                png.SetPixels(pixels);png.Apply();
                using(var stream=File.Create(Path.Combine(output,$"{kind}-{clip}-{negative}-{feature}.ppm")))
                {
                    var header=System.Text.Encoding.ASCII.GetBytes("P6\n64 64\n255\n");stream.Write(header,0,header.Length);
                    foreach(var c in png.GetPixels32()){stream.WriteByte(c.r);stream.WriteByte(c.g);stream.WriteByte(c.b);}
                }
            }
        }
        finally{RenderTexture.active=previous;foreach(var item in owned)UnityEngine.Object.DestroyImmediate(item);}
    }
}

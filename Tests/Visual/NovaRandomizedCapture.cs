// Isolated differential test. Prefix only the reference copy with NOVA_VISUAL_REFERENCE.
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
#if NOVA_VISUAL_REFERENCE
using UnityEngine.Rendering.Universal;
#endif

public sealed class NovaRandomizedCapture
{
    private const int Size = 128;
    private const int Seed = 20261013;
    private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
    private Texture2D[] textures;
    private Texture2DArray array;
    private Texture3D volume;
    private Texture2D field;
    private Camera camera;
    private Mesh mesh;
    private MeshRenderer renderer;
    private RenderTexture target;
    private Texture2D readback;
    private string output;

    [Serializable] private sealed class State { public int seed; public string shader; public string[] keywords; public string[] properties; }

    [UnityTest]
    public IEnumerator CaptureRandomizedMaterialFrames()
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-novaVisualOutput");
        Assert.That(index, Is.GreaterThanOrEqualTo(0));
        output = args[index + 1];
        Directory.CreateDirectory(output);
        Assert.That(Directory.GetFiles(output), Is.Empty, "Use a fresh directory; stale captures cannot count as evidence.");
        QualitySettings.antiAliasing = 0;
        QualitySettings.anisotropicFiltering = args.Contains("-novaDisableAnisotropy")
            ? AnisotropicFiltering.Disable : AnisotropicFiltering.ForceEnable;
        RenderSettings.fog = false;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.reflectionIntensity = 0;
        try
        {
#if NOVA_VISUAL_REFERENCE
            var data = Own(ScriptableObject.CreateInstance<UniversalRendererData>());
            // Both pipelines must expose opaque depth while drawing transparent particles.
            data.copyDepthMode = CopyDepthMode.AfterOpaques;
#if !UNITY_6000_0_OR_NEWER
            typeof(UniversalRendererData).GetMethod("ReloadAllNullProperties", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(data, null);
#endif
            var featureType = FindType("Nova.Runtime.Core.Scripts.ScreenSpaceDistortion");
            data.rendererFeatures.Add((ScriptableRendererFeature)Own(ScriptableObject.CreateInstance(featureType)));
            var pipeline = Own(UniversalRenderPipelineAsset.Create(data));
            pipeline.msaaSampleCount = 1; pipeline.supportsHDR = false; pipeline.supportsCameraDepthTexture = true;
            GraphicsSettings.defaultRenderPipeline = pipeline; QualitySettings.renderPipeline = pipeline;
#else
            GraphicsSettings.defaultRenderPipeline = null; QualitySettings.renderPipeline = null;
            FindType("Nova.Editor.Core.Scripts.BuiltInLightingSettings").GetMethod("Apply").Invoke(null, null);
#endif
            camera = Own(new GameObject("Random comparison camera", typeof(Camera))).GetComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -2);
            camera.orthographic = false; camera.fieldOfView = 40;
            camera.nearClipPlane = 0.1f; camera.farClipPlane = 10;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.13f,0.19f,0.27f,1);
            camera.allowHDR = false; camera.allowMSAA = false;
            camera.depthTextureMode = DepthTextureMode.Depth;
#if NOVA_VISUAL_REFERENCE
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            camera.GetUniversalAdditionalCameraData().requiresDepthTexture = true;
#else
            camera.gameObject.AddComponent(FindType("Nova.Runtime.Core.Scripts.ScreenSpaceDistortion"));
#endif
            target = Own(new RenderTexture(Size,Size,24,RenderTextureFormat.ARGB32)); camera.targetTexture = target;
            readback = Own(new Texture2D(Size,Size,TextureFormat.RGBA32,false));
            var light = Own(new GameObject("Random comparison light", typeof(Light))).GetComponent<Light>();
            light.type = LightType.Directional; light.color = Color.white; light.intensity = 0.7f;
            RenderSettings.sun = light;
            var quad = Own(new GameObject("Random comparison quad",typeof(MeshFilter),typeof(MeshRenderer)));
            mesh = Own(new Mesh {
                vertices = new[] { new Vector3(-0.5f,-0.5f,0),new Vector3(0.5f,-0.5f,0),new Vector3(0.5f,0.5f,0),new Vector3(-0.5f,0.5f,0) },
                uv = new[] { Vector2.zero,Vector2.right,Vector2.one,Vector2.up },
                normals = Enumerable.Repeat(Vector3.back,4).ToArray(),
                tangents = Enumerable.Repeat(new Vector4(1,0,0,1),4).ToArray(),
                triangles = new[] {0,2,1,0,3,2}
            });
            mesh.SetUVs(1, Enumerable.Repeat(new Vector4(0.13f,0.27f,0.41f,0.63f),4).ToList());
            mesh.SetUVs(2, Enumerable.Repeat(new Vector4(0.21f,0.34f,0.56f,0.78f),4).ToList());
            mesh.SetUVs(3, Enumerable.Repeat(new Vector4(0.21f,0.34f,0.56f,0.78f),4).ToList());
            mesh.RecalculateBounds(); quad.GetComponent<MeshFilter>().sharedMesh = mesh;
            renderer = quad.GetComponent<MeshRenderer>();
            LoadTextures();
            // A textured opaque backdrop makes distortion and soft intersections observable.
            var backdrop = Own(UnityEngine.Object.Instantiate(quad)); backdrop.name = "Backdrop";
            backdrop.transform.position = new Vector3(0,0,0.35f); backdrop.transform.localScale = Vector3.one * 2;
            var backgroundMaterial = Own(new Material(Shader.Find("Nova/Particles/UberUnlit")));
            backgroundMaterial.SetTexture("_BaseMap",field); Set(backgroundMaterial,"_RenderType",0); Set(backgroundMaterial,"_Cull",0);
            Set(backgroundMaterial,"_ShadowCasterEnabled",1); Setup(backgroundMaterial);
            backdrop.GetComponent<MeshRenderer>().sharedMaterial = backgroundMaterial;
            yield return null; yield return null;
            var names = new[] {"Particles/UberUnlit","UIParticles/UberUnlit","Particles/UberLit","UIParticles/UberLit","Particles/Distortion"};
            var captured=0;
            foreach (var name in names)
            for (var sample = 0; sample < (name.EndsWith("Distortion") ? 32 : 96); sample++)
            {
                // Diagnostic subsets never satisfy the full-inventory comparison gate.
                if (args.Contains("-novaDepthDiagnostic") && !name.EndsWith("Distortion") && sample / 8 != 9) continue;
                if (args.Contains("-novaRawDistortion") && !name.EndsWith("Distortion")) continue;
                var m = new Material(Shader.Find("Nova/" + name));
                try
                {
                    Configure(m, sample);
                    if (args.Contains("-novaNoSoftParticles")) Set(m,"_SoftParticlesEnabled",0);
                    if (args.Contains("-novaNoDepthFade")) Set(m,"_DepthFadeEnabled",0);
                    Setup(m);
                    mesh.colors = Enumerable.Repeat(sample == 0 ? Color.white : new Color(0.7f,0.85f,0.6f,0.8f),4).ToArray();
                    renderer.sharedMaterial = m;
                    backdrop.SetActive(sample != 0 || name.EndsWith("Distortion"));
                    var id = name.Replace('/','_') + "__" + sample.ToString("000");
                    SaveState(m, sample, id);
                    yield return null; yield return null;
                    SaveFrame(id);
                    if (sample == 4 && name.EndsWith("Distortion"))
                    {
                        var enabled = readback.GetPixels32();
                        renderer.enabled = false;
                        yield return null; yield return null;
                        var disabled = ReadPixels();
                        Assert.That(enabled.Zip(disabled,(a,b)=>Math.Abs(a.r-b.r)+Math.Abs(a.g-b.g)+Math.Abs(a.b-b.b)).Count(d=>d>3),
                            Is.GreaterThan(100),"Distortion must visibly change the background.");
                        renderer.enabled = true;
                    }
                    // A background-only image cannot prove that the depth path ran.
                    // This witness must visibly respond to soft particles in both pipelines.
                    if (sample == 73 && name == "Particles/UberUnlit" && !args.Contains("-novaNoSoftParticles"))
                    {
                        var enabled = readback.GetPixels32();
                        Set(m,"_SoftParticlesEnabled",0); Setup(m);
                        yield return null; yield return null;
                        var disabled = ReadPixels();
                        Assert.That(enabled.Zip(disabled,(a,b)=>Math.Abs(a.r-b.r)+Math.Abs(a.g-b.g)+Math.Abs(a.b-b.b)).Count(d=>d>3),
                            Is.GreaterThan(100),"Soft particles must change visible pixels; verify depth copy occurs before transparents.");
                        Set(m,"_SoftParticlesEnabled",1); Setup(m);
                    }
                    if (args.Contains("-novaReadDepth")) SaveDepth(id);
                    captured++;
                    if (args.Contains("-novaRawDistortion")) SaveRawDistortion(m,id);
                    Assert.That(m.shader.isSupported, Is.True, id);
                    Assert.That(ShaderUtil.GetShaderMessages(m.shader).Where(s => s.severity.ToString() == "Error"),Is.Empty,id);
                }
                finally { renderer.sharedMaterial = null; UnityEngine.Object.DestroyImmediate(m); }
            }
            File.WriteAllText(Path.Combine(output,"capture.txt"), Application.unityVersion + "\n" +
                SystemInfo.graphicsDeviceType + "\n" + QualitySettings.activeColorSpace + "\nseed=" + Seed + "\n" + captured + " frames\nanisotropy=" + QualitySettings.anisotropicFiltering + "\n");
        }
        finally
        {
            if (camera != null) camera.targetTexture = null;
            foreach (var item in owned.AsEnumerable().Reverse()) if (item != null) UnityEngine.Object.DestroyImmediate(item);
        }
    }

    private void LoadTextures()
    {
        textures = AssetDatabase.FindAssets("t:Texture2D", new[] {"Assets/Corpus/Kenney"})
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p,StringComparer.Ordinal)
            .Select(AssetDatabase.LoadAssetAtPath<Texture2D>).ToArray();
        Assert.That(textures.Length,Is.EqualTo(96));
        field = Own(new Texture2D(32,32,TextureFormat.RGBA32,false,true) { name = "seeded-field", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Repeat });
        for (var y=0;y<32;y++) for (var x=0;x<32;x++) field.SetPixel(x,y,new Color((x+0.5f)/32,(y+0.5f)/32,((x/4+y/4)%2)*0.7f+0.15f,0.25f+0.7f*x/31));
        field.Apply();
        array = Own(new Texture2DArray(64,64,4,TextureFormat.RGBA32,false) { name="CC0-array",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Repeat });
        volume = Own(new Texture3D(64,64,4,TextureFormat.RGBA32,false) { name="CC0-volume",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Repeat });
        var pixels = new List<Color>();
        var rt = RenderTexture.GetTemporary(64,64,0,RenderTextureFormat.ARGB32);
        var cpu = Own(new Texture2D(64,64,TextureFormat.RGBA32,false));
        var previous = RenderTexture.active;
        try
        {
            for (var slice=0;slice<4;slice++)
            {
                Graphics.Blit(textures[new System.Random(Seed+slice).Next(textures.Length)],rt);
                RenderTexture.active=rt; cpu.ReadPixels(new Rect(0,0,64,64),0,0); cpu.Apply();
                var colors=cpu.GetPixels(); array.SetPixels(colors,slice); pixels.AddRange(colors);
            }
        }
        finally { RenderTexture.active=previous; RenderTexture.ReleaseTemporary(rt); }
        array.Apply(); volume.SetPixels(pixels.ToArray()); volume.Apply();
    }

    private void Configure(Material m, int sample)
    {
        var r = new System.Random(Seed+sample);
        var profile = sample / 8;
        var distortion = m.shader.name.EndsWith("Distortion");
        float V(float min=0,float max=1) => min+(max-min)*(float)r.NextDouble();
        var coords = new[] {0,1,11,21,31,2,12,22,32};
        for (var i=0;i<m.shader.GetPropertyCount();i++)
        {
            var p=m.shader.GetPropertyName(i); var type=m.shader.GetPropertyType(i);
            if (type == ShaderPropertyType.Texture)
            {
                var dimension=m.shader.GetPropertyTextureDimension(i);
                Texture t=dimension==TextureDimension.Tex2DArray ? array : dimension==TextureDimension.Tex3D ? (Texture)volume : textures[r.Next(textures.Length)];
                if (p.Contains("NormalMap")) t=null; // enabled separately in the surface profile
                m.SetTexture(p,t);
                m.SetTextureScale(p,new Vector2(V(0.65f,1.5f),V(0.65f,1.5f)));
                m.SetTextureOffset(p,new Vector2(V(-0.3f,0.3f),V(-0.3f,0.3f)));
            }
            else if (type == ShaderPropertyType.Color) m.SetColor(p,new Color(V(0.1f,0.85f),V(0.1f,0.85f),V(0.1f,0.85f),V(0.5f,1)));
            else if (type == ShaderPropertyType.Vector && p.EndsWith("RotationOffsets")) m.SetVector(p,new Vector4(V(-0.3f,0.3f),V(-0.3f,0.3f),0,0));
            else if (p.EndsWith("Coord") || p == "_AlphaTransitionProgressCoordSecondTexture") Set(m,p,coords[r.Next(coords.Length)]);
            else if (p.EndsWith("ChannelsX") || p.EndsWith("ChannelsY") || p == "_ParallaxMapChannel" || p == "_VertexDeformationMapChannel") Set(m,p,r.Next(4));
            else if (p.EndsWith("SliceCount")) Set(m,p,4);
            else if (p.EndsWith("RowCount")) Set(m,p,2);
            else if (p.EndsWith("RandomRowSelectionEnabled")) Set(m,p,r.Next(2));
            else if (type == ShaderPropertyType.Range) Set(m,p,V(0.1f,0.8f));
        }
        Set(m,"_Cull",0); Set(m,"_RenderType",2); Set(m,"_TransparentBlendMode",sample%3);
        Set(m,"_BaseMapMode",sample%3); Set(m,"_BaseMapMirrorSampling",sample%2);
        Set(m,"_Metallic",0); Set(m,"_LitWorkflowMode",1); Set(m,"_Smoothness",0);
        Set(m,"_EnvironmentReflections",0); Set(m,"_SpecularHighlights",0);
        foreach (var p in new[] {"_SpecularMap","_SpecularMap2DArray","_SpecularMap3D","_MetallicMap","_MetallicMap2DArray","_MetallicMap3D","_SmoothnessMap","_SmoothnessMap2DArray","_SmoothnessMap3D"}) if(m.HasProperty(p)) m.SetTexture(p,null);
        Set(m,"_FlowMapTarget",0); Set(m,"_ParallaxMapTarget",0);
        Set(m,"_BaseMapToneMode",profile==1 ? 1+sample%2 : 0);
        Set(m,"_BaseMapToneChannel",sample%4); Set(m,"_BaseMapChannel",profile==0 ? sample%5 : 0);
        Set(m,"_TintAreaMode",profile==2 || profile>=10 ? 1+sample%2 : 0);
        Set(m,"_TintColorMode",sample%4); Set(m,"_InverseTintRim",sample%2);
        Set(m,"_AlphaTransitionMode",profile==3 || profile>=10 ? 1+sample%2 : 0);
        Set(m,"_AlphaTransitionMapMode",sample%3); Set(m,"_AlphaTransitionSecondTextureBlendMode",sample%3);
        Set(m,"_AlphaTransitionMapSecondTextureMapMode",(sample+1)%3);
        Set(m,"_VertexAlphaMode",sample%2);
        if (profile==4 || profile>=10) { Set(m,"_FlowMapTarget",1+sample%15); Set(m,"_FlowIntensity",V(0.03f,0.2f)); m.SetTexture("_FlowMap",field); }
        if (profile==5 || profile>=10) { Set(m,"_ParallaxMapTarget",1+sample%7); Set(m,"_ParallaxMapMode",sample%3); Set(m,"_ParallaxStrength",V(0.05f,0.3f)); }
        Set(m,"_EmissionAreaType",profile==6 || profile>=10 ? 1+sample%3 : 0);
        Set(m,"_EmissionColorType",sample%3); Set(m,"_EmissionMapMode",sample%3); Set(m,"_EmissionIntensity",V(0.05f,0.4f));
        Set(m,"_KeepEdgeTransparency",sample%2); Set(m,"_ColorCorrectionMode",profile==7 || profile==11 ? 1+sample%2 : 0);
        Set(m,"_RimTransparencyEnabled",profile==8 ? 1 : 0); Set(m,"_LuminanceTransparencyEnabled",profile==8 ? 1 : 0);
        Set(m,"_InverseRimTransparency",sample%2); Set(m,"_InverseLuminanceTransparency",sample%2);
        Set(m,"_DepthFadeEnabled",profile==9 ? 1 : 0); Set(m,"_SoftParticlesEnabled",profile==9 ? 1 : 0);
        Set(m,"_DepthFadeNear",V(0,1.9f)); Set(m,"_DepthFadeFar",3); Set(m,"_DepthFadeWidth",V(0.2f,1)); Set(m,"_SoftParticlesIntensity",V(0.1f,1));
        Set(m,"_VertexDeformationEnabled",profile==9 || profile==11 ? 1 : 0); Set(m,"_VertexDeformationIntensity",V(0.03f,0.15f));
        if (distortion)
        {
            m.SetTexture("_BaseMap",field); m.SetTexture("_FlowMap",field); m.SetTexture("_AlphaTransitionMap",field);
            Set(m,"_BaseMapUnpackNormal",sample%2); Set(m,"_DistortionIntensityMode",sample%2);
            Set(m,"_DistortionIntensity",V(0.01f,0.06f)); Set(m,"_DistortionIntensityX",V(-0.04f,0.04f)); Set(m,"_DistortionIntensityY",V(-0.04f,0.04f));
            Set(m,"_FlowMapTarget",sample%4); Set(m,"_FlowIntensity",V(0.01f,0.15f));
            Set(m,"_AlphaTransitionMode",sample%3); Set(m,"_SoftParticlesEnabled",sample%2);
        }
        if (sample==0 && !distortion)
        {
            m.SetTexture("_BaseMap",field); m.SetTextureScale("_BaseMap",Vector2.one); m.SetTextureOffset("_BaseMap",Vector2.zero);
            Set(m,"_RenderType",0); Set(m,"_BaseMapMode",0); Set(m,"_BaseMapRotation",0); Set(m,"_BaseMapRotationCoord",0);
        }
    }

    private static void Set(Material m,string name,float value) { if(m.HasProperty(name)) m.SetFloat(name,value); }
    private static Type FindType(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
    private static void Setup(Material m)
    {
        var guiName=m.shader.name.Substring(5).Replace("/","")+"GUI";
        ((ShaderGUI)Activator.CreateInstance(FindType("Nova.Editor.Core.Scripts."+guiName))).ValidateMaterial(m);
    }
    private void SaveState(Material m,int sample,string id)
    {
        var properties=new List<string>();
        for(var i=0;i<m.shader.GetPropertyCount();i++)
        {
            var p=m.shader.GetPropertyName(i); var t=m.shader.GetPropertyType(i);
            string value;
            if(t==ShaderPropertyType.Texture)
            {
                var texture=m.GetTexture(p);
                value=(texture == null ? "null" : texture.name)+"|"+m.GetTextureScale(p).ToString("R")+"|"+m.GetTextureOffset(p).ToString("R");
            }
            else if(t==ShaderPropertyType.Color) value=m.GetColor(p).ToString("R");
            else if(t==ShaderPropertyType.Vector) value=m.GetVector(p).ToString("R");
            else value=m.GetFloat(p).ToString("R",CultureInfo.InvariantCulture);
            properties.Add(p+"="+value);
        }
        File.WriteAllText(Path.Combine(output,id+".json"),JsonUtility.ToJson(new State {
            seed=Seed+sample,shader=m.shader.name,properties=properties.ToArray(),
            keywords=m.shaderKeywords.Where(k=>k!="_ADDITIONAL_LIGHTS").OrderBy(k=>k,StringComparer.Ordinal).ToArray()
        },true));
    }
    private void SaveFrame(string id)
    {
        var colors=ReadPixels();
        using(var stream=File.Create(Path.Combine(output,id+".ppm")))
        {
            var header=Encoding.ASCII.GetBytes("P6\n128 128\n255\n"); stream.Write(header,0,header.Length);
            for(var y=Size-1;y>=0;y--) for(var x=0;x<Size;x++) {var c=colors[y*Size+x];stream.WriteByte(c.r);stream.WriteByte(c.g);stream.WriteByte(c.b);}
        }
    }
    private Color32[] ReadPixels()
    {
        var previous=RenderTexture.active; RenderTexture.active=target;
        readback.ReadPixels(new Rect(0,0,Size,Size),0,0); readback.Apply(); RenderTexture.active=previous;
        return readback.GetPixels32();
    }
    private void SaveRawDistortion(Material m,string id)
    {
        // Diagnostic: isolate offset math from the compositor and camera depth scheduling.
        m.DisableKeyword("_SOFT_PARTICLES_ENABLED");m.DisableKeyword("_DEPTH_FADE_ENABLED");m.SetFloat("_ZTest",8);
        var rt=new RenderTexture(Size,Size,0,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear);
        var cpu=new Texture2D(Size,Size,TextureFormat.RGBAFloat,false,true);var previous=RenderTexture.active;
        try
        {
            using(var cmd=new CommandBuffer())
            {
                cmd.SetRenderTarget(rt);cmd.ClearRenderTarget(false,true,Color.gray);
                cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix,GL.GetGPUProjectionMatrix(camera.projectionMatrix,true));
#if NOVA_VISUAL_REFERENCE
                var pass=0;
#else
                var pass=m.FindPass("DistortedUvBuffer");Assert.That(pass,Is.GreaterThanOrEqualTo(0));
#endif
                cmd.DrawMesh(mesh,Matrix4x4.identity,m,0,pass);Graphics.ExecuteCommandBuffer(cmd);
            }
            RenderTexture.active=rt;cpu.ReadPixels(new Rect(0,0,Size,Size),0,0);cpu.Apply();
            using(var writer=new BinaryWriter(File.Create(Path.Combine(output,id+".rgfloat"))))
                foreach(var p in cpu.GetPixels()){writer.Write(p.r);writer.Write(p.g);}
        }
        finally {RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(cpu);UnityEngine.Object.DestroyImmediate(rt);}
    }
    private void SaveDepth(string id)
    {
        var depth=Shader.GetGlobalTexture("_CameraDepthTexture");Assert.That(depth,Is.Not.Null);
        var rt=new RenderTexture(Size,Size,0,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
        var cpu=new Texture2D(Size,Size,TextureFormat.RGBAFloat,false,true);var previous=RenderTexture.active;
        try
        {
            Graphics.Blit(depth,rt);RenderTexture.active=rt;cpu.ReadPixels(new Rect(0,0,Size,Size),0,0);cpu.Apply();
            using(var writer=new BinaryWriter(File.Create(Path.Combine(output,id+".depthfloat"))))
                foreach(var p in cpu.GetPixels())writer.Write(p.r);
        }
        finally {RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(cpu);UnityEngine.Object.DestroyImmediate(rt);}
    }
    private T Own<T>(T item) where T:UnityEngine.Object { owned.Add(item);return item; }
}
#endif

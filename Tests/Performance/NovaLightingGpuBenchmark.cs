using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Nova.Runtime.Core.Scripts;
using Nova.Runtime.UI;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// Disposable standalone measurement fixture. Never included in the NOVA package or avatar.
public sealed class NovaLightingGpuBenchmark : MonoBehaviour
{
    public Material[] baselineMaterials, candidateMaterials;
    [Serializable] public sealed class Scenario { public string name; public int material, layers=1, lights; public bool mirror,fog,clip; public string helper; }
    public Scenario[] scenarios;
    [Serializable] public sealed class Block
    {
        public string scenario,variant; public bool optional; public int repeat,width=1920,height=1080;
        public double[] gpuMs,cpuMs; public long draws,materialBytes,renderTextureBytes,probeBytes;
    }
    [Serializable] public sealed class Report
    {
        public string unity,device,api,timingMethod,error;
        public bool completed,gpuTimingAvailable;
        public Block[] blocks;
    }
    [DllImport("NovaGpuTimer")] static extern IntPtr NovaGetTimerEvent();
    [DllImport("NovaGpuTimer")] static extern int NovaPopGpuTime(out int block,out double milliseconds);
    readonly List<Block> blocks=new List<Block>();
    readonly List<double> gpu=new List<double>(),cpu=new List<double>();
    readonly List<long> draws=new List<long>();
    readonly List<Renderer> meshes=new List<Renderer>();
    readonly List<Light> lights=new List<Light>();
    CommandBuffer begin,end,probeCommands;
    ProfilerRecorder drawCounter;
    Camera cameraMain,cameraMirror;
    NovaDepthNormals depthNormals;
    NovaAmbientLighting ambientMain,ambientMirror;
    GameObject canvasObject;RawImage graphic;NovaCanvasLighting canvasLighting;
    RenderTexture mainTarget,mirrorTarget;
    ComputeBuffer probeConstants;Texture3D probeTexture;MaterialPropertyBlock probeProperties;
    Mesh mesh;
    int blockIndex,frames;float started;bool finished;
    IntPtr timer;string output;Report report;

    void Start()
    {
        var args=Environment.GetCommandLineArgs();output=args[Array.IndexOf(args,"-novaPerformanceOutput")+1];
        report=new Report{unity=Application.unityVersion,device=SystemInfo.graphicsDeviceName,api=SystemInfo.graphicsDeviceType.ToString(),
            timingMethod="D3D11 timestamp/disjoint around offscreen rendering (no Present); CPU stopwatch around command submission; ProfilerRecorder Draw Calls Count; active native resource sizes"};
        try
        {
            QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=0;QualitySettings.pixelLightCount=8;
            Application.targetFrameRate=-1;Application.runInBackground=true;
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.1f,.1f,.1f);
            begin=new CommandBuffer();end=new CommandBuffer();probeCommands=new CommandBuffer();
            timer=NovaGetTimerEvent();end.IssuePluginEventAndData(timer,1,IntPtr.Zero);
            drawCounter=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);
            mainTarget=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32);
            mirrorTarget=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32);
            cameraMain=CreateCamera("Main measurement",mainTarget);
            cameraMirror=CreateCamera("Reflected measurement",mirrorTarget);
            cameraMirror.worldToCameraMatrix=Matrix4x4.Scale(new Vector3(-1,1,1))*cameraMirror.worldToCameraMatrix;
            ambientMain=cameraMain.gameObject.AddComponent<NovaAmbientLighting>();ambientMain.enabled=false;
            ambientMirror=cameraMirror.gameObject.AddComponent<NovaAmbientLighting>();ambientMirror.enabled=false;
            depthNormals=cameraMain.gameObject.AddComponent<NovaDepthNormals>();depthNormals.enabled=false;
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Quad);
            mesh=Instantiate(primitive.GetComponent<MeshFilter>().sharedMesh);Destroy(primitive);
            mesh.normals=new[]{new Vector3(.3f,.2f,-1).normalized,new Vector3(-.3f,.2f,-1).normalized,new Vector3(.3f,-.2f,-1).normalized,new Vector3(-.3f,-.2f,-1).normalized};
            mesh.colors=Enumerable.Repeat(Color.white,4).ToArray();
            for(int i=0;i<8;i++)
            {
                var item=new GameObject("Layer "+i,typeof(MeshFilter),typeof(MeshRenderer));
                item.GetComponent<MeshFilter>().sharedMesh=mesh;item.transform.localScale=new Vector3(5,2.8f,1.4f);
                item.transform.position=new Vector3(0,0,i*.003f);
                var renderer=item.GetComponent<Renderer>();renderer.shadowCastingMode=ShadowCastingMode.Off;meshes.Add(renderer);
                depthNormals.RegisterRenderer(renderer);
            }
            for(int i=0;i<8;i++)
            {
                var light=new GameObject("Light "+i,typeof(Light)).GetComponent<Light>();lights.Add(light);
                light.type=i==0 ? LightType.Directional : LightType.Point;light.intensity=.3f;light.range=5;
                light.transform.position=new Vector3((i%3-1)*.5f,0,-1);light.shadows=LightShadows.None;light.renderMode=LightRenderMode.ForcePixel;
            }
            canvasObject=new GameObject("Measured Canvas",typeof(RectTransform),typeof(Canvas));
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=cameraMain;
            canvasObject.GetComponent<RectTransform>().sizeDelta=new Vector2(5,2.8f);
            var child=new GameObject("Measured Graphic",typeof(RectTransform),typeof(RawImage));child.transform.SetParent(canvasObject.transform,false);
            graphic=child.GetComponent<RawImage>();graphic.rectTransform.sizeDelta=new Vector2(5,2.8f);
            canvasLighting=canvasObject.AddComponent<NovaCanvasLighting>();foreach(var light in lights)canvasLighting.RegisterLight(light);
            canvasLighting.enabled=false;canvasObject.SetActive(false);
            probeConstants=new ComputeBuffer(7,16,ComputeBufferType.Constant);
            probeTexture=new Texture3D(32,2,2,TextureFormat.RGBAHalf,false);probeTexture.SetPixels(Enumerable.Repeat(new Color(0,0,0,.15f),128).ToArray());probeTexture.Apply();
            probeProperties=new MaterialPropertyBlock();probeProperties.SetTexture("unity_ProbeVolumeSH",probeTexture);
            var ordinaryProbe=new SphericalHarmonicsL2();ordinaryProbe.AddAmbientLight(new Color(.15f,.15f,.15f));
            probeProperties.CopySHCoefficientArraysFrom(new[]{ordinaryProbe});
            BeginBlock();
        }
        catch(Exception error){Finish(error.ToString());}
    }

    Camera CreateCamera(string name,RenderTexture target)
    {
        var camera=new GameObject(name,typeof(Camera)).GetComponent<Camera>();camera.enabled=false;
        camera.transform.position=new Vector3(0,0,-3);camera.fieldOfView=55;camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=Color.black;camera.targetTexture=target;camera.allowHDR=false;camera.allowMSAA=false;
        return camera;
    }
    bool Candidate => blockIndex%4==1 || blockIndex%4==2;
    Scenario Current => scenarios[blockIndex/12];
    Material ActiveMaterial => Candidate ? candidateMaterials[Current.material] : baselineMaterials[Current.material];

    void BeginBlock()
    {
        var current=Current;frames=0;gpu.Clear();cpu.Clear();draws.Clear();started=Time.realtimeSinceStartup;
        for(int i=0;i<meshes.Count;i++){meshes[i].enabled=(string.IsNullOrEmpty(current.helper) || current.helper=="ambient") && i<current.layers || current.helper=="depth" && i<current.layers;meshes[i].sharedMaterial=ActiveMaterial;}
        for(int i=0;i<lights.Count;i++)lights[i].enabled=i<Math.Max(1,current.lights);
        if(current.clip)ActiveMaterial.EnableKeyword("UNITY_UI_CLIP_RECT");else ActiveMaterial.DisableKeyword("UNITY_UI_CLIP_RECT");
        ActiveMaterial.SetVector("_ClipRect",new Vector4(-10,-10,10,10));
        RenderSettings.fog=current.fog;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.12f;
        canvasObject.SetActive(current.helper=="canvas");graphic.material=ActiveMaterial;
        canvasLighting.MaximumLights=current.lights;canvasLighting.enabled=current.helper=="canvas" && Candidate;
        depthNormals.enabled=current.helper=="depth" && Candidate;
        ambientMain.enabled=ambientMirror.enabled=current.helper=="ambient" && Candidate;
        if(current.helper=="probe")
        {
            var data=new Vector4[7];data[0]=new Vector4(Candidate ? 1 : 0,0,1f/32,0);
            for(int row=0;row<4;row++)data[row+1]=Matrix4x4.identity.GetRow(row);
            data[5]=Vector3.one;data[6]=new Vector4(-.5f,-.5f,-.5f,0);probeConstants.SetData(data);
        }
    }
    void Update()
    {
        if(finished || report==null)return;
        try
        {
            frames++;
            if(Time.realtimeSinceStartup-started>90)throw new Exception("No complete GPU block within 90 seconds.");
            begin.Clear();begin.IssuePluginEventAndData(timer,0,new IntPtr(frames>64 ? blockIndex : -1));
            long tick=Stopwatch.GetTimestamp();Graphics.ExecuteCommandBuffer(begin);
            if(Current.helper=="probe")
            {
                probeCommands.Clear();probeCommands.SetRenderTarget(mainTarget);probeCommands.ClearRenderTarget(true,true,Color.black);
                probeCommands.SetViewProjectionMatrices(cameraMain.worldToCameraMatrix,GL.GetGPUProjectionMatrix(cameraMain.projectionMatrix,true));
                probeCommands.SetGlobalVector("_LightColor0",Vector4.zero);
                probeCommands.SetGlobalConstantBuffer(probeConstants,Shader.PropertyToID("UnityProbeVolume"),0,112);
                probeCommands.DrawMesh(mesh,Matrix4x4.Scale(new Vector3(5,2.8f,1)),ActiveMaterial,0,ActiveMaterial.FindPass("ForwardBase"),probeProperties);
                Graphics.ExecuteCommandBuffer(probeCommands);
            }
            else
            {
                if(Current.helper=="canvas")Canvas.ForceUpdateCanvases();
                if(Current.mirror)cameraMirror.Render();cameraMain.Render();
            }
            Graphics.ExecuteCommandBuffer(end);
            double milliseconds=(Stopwatch.GetTimestamp()-tick)*1000.0/Stopwatch.Frequency;
            if(frames>64 && cpu.Count<128){cpu.Add(milliseconds);draws.Add(drawCounter.LastValue);}
            while(NovaPopGpuTime(out int block,out double time)!=0)
                if(block==blockIndex && time>0 && gpu.Count<128){gpu.Add(time);report.gpuTimingAvailable=true;}
            if(frames==1 && blockIndex%12<2)SaveWitness();
            if(gpu.Count>=128 && cpu.Count>=128)EndBlock();
        }
        catch(Exception error){Finish(error.ToString());}
    }
    void EndBlock()
    {
        long drawMedian=draws.OrderBy(x=>x).ElementAt(draws.Count/2);
        if(drawMedian==0)throw new Exception("Draw-call counter unavailable; do not substitute an estimate.");
        long materialBytes=Profiler.GetRuntimeMemorySizeLong(ActiveMaterial);
        if(canvasLighting.enabled)materialBytes+=Profiler.GetRuntimeMemorySizeLong(graphic.materialForRendering);
        blocks.Add(new Block{scenario=Current.name,optional=!string.IsNullOrEmpty(Current.helper),variant=Candidate ? "candidate" : "baseline",repeat=blockIndex%12/4,
            gpuMs=gpu.ToArray(),cpuMs=cpu.ToArray(),draws=drawMedian,materialBytes=materialBytes,
            renderTextureBytes=Profiler.GetRuntimeMemorySizeLong(mainTarget)+(Current.mirror ? Profiler.GetRuntimeMemorySizeLong(mirrorTarget) : 0)
                +(depthNormals.Texture!=null ? Profiler.GetRuntimeMemorySizeLong(depthNormals.Texture) : 0),
            probeBytes=Current.helper=="probe" && Candidate ? Profiler.GetRuntimeMemorySizeLong(probeTexture)+112 : 0});
        report.blocks=blocks.ToArray();File.WriteAllText(output,JsonUtility.ToJson(report,true));
        blockIndex++;if(blockIndex==scenarios.Length*12)Finish(null);else BeginBlock();
    }
    void SaveWitness()
    {
        var previous=RenderTexture.active;var image=new Texture2D(1920,1080,TextureFormat.RGBA32,false);
        try
        {
            RenderTexture.active=mainTarget;image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();
            var pixels=image.GetPixels32();if(pixels.Count(c=>c.r>4 || c.g>4 || c.b>4)<100)throw new Exception("Invisible benchmark geometry: "+Current.name);
            using(var stream=File.Create(output+".block-"+blockIndex+".ppm"))
            {
                var header=System.Text.Encoding.ASCII.GetBytes("P6\n1920 1080\n255\n");stream.Write(header,0,header.Length);
                foreach(var c in pixels){stream.WriteByte(c.r);stream.WriteByte(c.g);stream.WriteByte(c.b);}
            }
        }
        finally{RenderTexture.active=previous;Destroy(image);}
    }
    void Finish(string error)
    {
        finished=true;report.error=error;report.completed=error==null;report.blocks=blocks.ToArray();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));File.WriteAllText(output,JsonUtility.ToJson(report,true));
        Debug.Log("NOVA_LIGHTING_BENCHMARK completed="+report.completed+" blocks="+blocks.Count+" error="+error);Application.Quit(error==null ? 0 : 1);
    }
    void OnDestroy(){drawCounter.Dispose();begin?.Dispose();end?.Dispose();probeCommands?.Dispose();probeConstants?.Dispose();}
}

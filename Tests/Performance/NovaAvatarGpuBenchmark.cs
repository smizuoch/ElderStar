using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

// Standalone measurement fixture only. Never shipped on an avatar.
public sealed class NovaAvatarGpuBenchmark : MonoBehaviour
{
    public Shader baselineShader;
    public Shader candidateShader;
    public Shader backgroundShader;
    [Serializable] public sealed class Block
    {
        public string scenario, variant;
        public int repeat, active, zero, width, height;
        public bool mirror;
        public double[] gpuMs, cpuMs;
    }
    [Serializable] public sealed class Report
    {
        public string unity, device, api, sourceCommit, timingMethod;
        public bool completed, gpuTimingAvailable;
        public string error;
        public Block[] blocks;
    }
    readonly List<Renderer> renderers = new List<Renderer>();
    readonly List<Material> materials = new List<Material>();
    readonly List<Block> blocks = new List<Block>();
    readonly List<double> gpu = new List<double>(), cpu = new List<double>();
    readonly FrameTiming[] latest = new FrameTiming[1];
    readonly int[] activeCounts = { 1, 8, 8, 32, 8 };
    readonly int[] zeroCounts = { 0, 0, 32, 0, 16 };
    readonly string[] names = { "one", "eight-active", "eight-plus-32-zero", "32-active", "mirror-eight-plus-16-zero" };
    Camera mirrorCamera, mainCamera;
    bool offscreen;
    IntPtr timerEvent;
    readonly CommandBuffer beginTimer = new CommandBuffer(), endTimer = new CommandBuffer();
    [DllImport("NovaGpuTimer")] static extern IntPtr NovaGetTimerEvent();
    [DllImport("NovaGpuTimer")] static extern int NovaPopGpuTime(out int block, out double milliseconds);
    int blockIndex, elapsedFrames;
    ulong previousTimestamp;
    float blockStart;
    string output;
    Report report;
    bool finished;

    void Start()
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-novaPerformanceOutput");
        output = index >= 0 ? args[index + 1] : Path.Combine(Application.persistentDataPath, "nova-gpu.json");
        offscreen = args.Contains("-novaOffscreen");
        report = new Report { unity = Application.unityVersion, device = SystemInfo.graphicsDeviceName,
            api = SystemInfo.graphicsDeviceType.ToString(), sourceCommit = "8064c6a baseline; candidate copied at build",
            timingMethod = offscreen ? "D3D11 timestamp/disjoint around offscreen Camera.Render; no Present" : "FrameTimingManager" };
        try
        {
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            Application.targetFrameRate = -1;
            Application.runInBackground = true;
            if (!offscreen) Screen.SetResolution(1920, 1080, false);
            RenderSettings.fog = false;
            Shader.SetGlobalFloat("_NovaCameraDistortionActive", 0);
            if (!baselineShader || !candidateShader || !backgroundShader) throw new Exception("Missing benchmark shader.");
            if (!baselineShader.isSupported || !candidateShader.isSupported) throw new Exception("Unsupported benchmark shader.");
            var camera = new GameObject("Benchmark camera", typeof(Camera)).GetComponent<Camera>();
            mainCamera = camera;
            camera.transform.position = new Vector3(0, 0, -2);
            camera.orthographic = true; camera.orthographicSize = 1;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            camera.allowHDR = false; camera.allowMSAA = false;
            var mirrorObject = Instantiate(camera.gameObject); mirrorObject.name = "Reflected-camera workload";
            mirrorCamera = mirrorObject.GetComponent<Camera>();
            mirrorCamera.depth = -1;
            mirrorCamera.worldToCameraMatrix = Matrix4x4.Scale(new Vector3(-1, 1, 1)) * mirrorCamera.worldToCameraMatrix;
            mirrorCamera.targetTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            if (offscreen)
            {
                timerEvent = NovaGetTimerEvent();
                endTimer.IssuePluginEventAndData(timerEvent, 1, IntPtr.Zero);
                camera.targetTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
                camera.enabled = false; mirrorCamera.enabled = false;
            }
            var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            for (var y = 0; y < 256; y++)
            for (var x = 0; x < 256; x++) texture.SetPixel(x, y, new Color(x / 255f, y / 255f, ((x / 8 + y / 8) % 2), 1));
            texture.Apply();
            var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = "Patterned background"; backdrop.transform.position = Vector3.forward;
            backdrop.transform.localScale = new Vector3(4, 2.25f, 1);
            var background = new Material(backgroundShader);
            background.SetTexture("_BaseMap", texture); background.SetFloat("_Cull", 0);
            background.SetFloat("_BlendSrc", 1); background.SetFloat("_BlendDst", 0);
            background.SetFloat("_ZWrite", 1); background.renderQueue = 2000;
            backdrop.GetComponent<Renderer>().sharedMaterial = background;
            var offset = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            offset.SetPixels(Enumerable.Repeat(new Color(.8f, .6f, 0, 1), 4).ToArray()); offset.Apply();
            for (var i = 0; i < 40; i++)
            {
                var item = GameObject.CreatePrimitive(PrimitiveType.Quad);
                item.name = "Overdraw layer " + i;
                item.transform.position = new Vector3(0, 0, .5f - i * .001f);
                item.transform.localScale = new Vector3(3.2f, 1.8f, 1);
                var material = new Material(candidateShader);
                material.SetTexture("_BaseMap", offset); material.SetFloat("_Cull", 0);
                var renderer = item.GetComponent<Renderer>(); renderer.sharedMaterial = material;
                renderers.Add(renderer); materials.Add(material);
            }
            BeginBlock();
        }
        catch (Exception error) { Finish(error.ToString()); }
    }

    void BeginBlock()
    {
        // Three ABBA repetitions per scenario, limiting drift from temperature/order.
        var scenario = blockIndex / 12;
        var candidate = blockIndex % 4 == 1 || blockIndex % 4 == 2;
        for (var i = 0; i < renderers.Count; i++)
        {
            renderers[i].enabled = i < activeCounts[scenario] + zeroCounts[scenario];
            materials[i].shader = candidate ? candidateShader : baselineShader;
            materials[i].SetFloat("_DistortionIntensity", i < activeCounts[scenario] ? .3f : 0);
            materials[i].renderQueue = 3100 + i;
        }
        mirrorCamera.enabled = !offscreen && scenario == 4;
        elapsedFrames = 0; gpu.Clear(); cpu.Clear(); previousTimestamp = 0;
        blockStart = Time.realtimeSinceStartup;
    }

    void Update()
    {
        if (finished || report == null) return;
        elapsedFrames++;
        if (Time.realtimeSinceStartup - blockStart > 60) { Finish("No complete block within 60s; GPU timing unavailable or workload stalled."); return; }
        if (offscreen)
        {
            beginTimer.Clear();
            beginTimer.IssuePluginEventAndData(timerEvent, 0, new IntPtr(elapsedFrames > 64 ? blockIndex : -1));
            Graphics.ExecuteCommandBuffer(beginTimer);
            if (blockIndex / 12 == 4) mirrorCamera.Render();
            mainCamera.Render();
            Graphics.ExecuteCommandBuffer(endTimer);
            if (elapsedFrames == 1 && blockIndex % 12 < 2) SaveWitness();
            while (NovaPopGpuTime(out var block, out var milliseconds) != 0)
                if (block == blockIndex && milliseconds > 0 && gpu.Count < 128) { gpu.Add(milliseconds); report.gpuTimingAvailable = true; }
            if (gpu.Count >= 128) EndBlock();
            return;
        }
        FrameTimingManager.CaptureFrameTimings();
        if (elapsedFrames <= 64) return;
        if (FrameTimingManager.GetLatestTimings(1, latest) == 0) return;
        var timing = latest[0];
        if (timing.frameStartTimestamp == previousTimestamp || timing.gpuFrameTime <= 0 || double.IsNaN(timing.gpuFrameTime)) return;
        previousTimestamp = timing.frameStartTimestamp;
        report.gpuTimingAvailable = true;
        gpu.Add(timing.gpuFrameTime); cpu.Add(timing.cpuFrameTime);
        if (gpu.Count < 128) return;
        EndBlock();
    }

    void SaveWitness()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
        var previous = RenderTexture.active;
        var image = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
        try
        {
            RenderTexture.active = mainCamera.targetTexture;
            image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); image.Apply();
            using (var stream = File.Create(output + ".block-" + blockIndex + ".ppm"))
            {
                var header = System.Text.Encoding.ASCII.GetBytes("P6\n1920 1080\n255\n"); stream.Write(header, 0, header.Length);
                var pixels = image.GetPixels32();
                for (var y = 1079; y >= 0; y--) for (var x = 0; x < 1920; x++)
                { var c = pixels[y * 1920 + x]; stream.WriteByte(c.r); stream.WriteByte(c.g); stream.WriteByte(c.b); }
            }
        }
        finally { RenderTexture.active = previous; Destroy(image); }
    }

    void EndBlock()
    {
        var scenario = blockIndex / 12;
        blocks.Add(new Block { scenario = names[scenario], variant = blockIndex % 4 == 1 || blockIndex % 4 == 2 ? "candidate" : "baseline",
            repeat = (blockIndex % 12) / 4, active = activeCounts[scenario], zero = zeroCounts[scenario], mirror = scenario == 4,
            width = offscreen ? 1920 : Screen.width, height = offscreen ? 1080 : Screen.height, gpuMs = gpu.ToArray(), cpuMs = cpu.ToArray() });
        report.blocks = blocks.ToArray();
        File.WriteAllText(output, JsonUtility.ToJson(report, true));
        blockIndex++;
        if (blockIndex == 60) Finish(null); else BeginBlock();
    }

    void Finish(string error)
    {
        finished = true; report.error = error; report.completed = error == null;
        report.blocks = blocks.ToArray();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
        File.WriteAllText(output, JsonUtility.ToJson(report, true));
        Debug.Log("NOVA_GPU_BENCHMARK completed=" + report.completed + " blocks=" + blocks.Count + " error=" + error);
        Application.Quit(error == null ? 0 : 1);
    }
}

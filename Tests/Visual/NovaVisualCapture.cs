// Copied into isolated PlayMode validation projects. The reference copy is prefixed
// with #define NOVA_VISUAL_REFERENCE; the distributable package never includes URP.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
#if NOVA_VISUAL_REFERENCE
using UnityEngine.Rendering.Universal;
#endif

public sealed class NovaVisualCapture
{
    private const int Size = 128;
    private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();

    [UnityTest]
    public IEnumerator CaptureOriginalFeatureFrames()
    {
        var args = Environment.GetCommandLineArgs();
        var argIndex = Array.IndexOf(args, "-novaVisualOutput");
        Assert.That(argIndex, Is.GreaterThanOrEqualTo(0));
        var output = args[argIndex + 1];
        var pbr = Array.IndexOf(args, "-novaPbr") >= 0;
        Directory.CreateDirectory(output);
        QualitySettings.antiAliasing = 0;
        RenderSettings.fog = false;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.15f);
        RenderSettings.reflectionIntensity = 0f;
#if NOVA_VISUAL_REFERENCE
        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        var pipeline = UniversalRenderPipelineAsset.Create(rendererData);
        pipeline.msaaSampleCount = 1;
        pipeline.supportsHDR = false;
        pipeline.supportsCameraDepthTexture = true;
        QualitySettings.renderPipeline = pipeline;
        GraphicsSettings.defaultRenderPipeline = pipeline;
#else
        QualitySettings.renderPipeline = null;
        GraphicsSettings.defaultRenderPipeline = null;
#endif
        var cameraObject = new GameObject("Visual comparison camera", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -2);
        camera.orthographic = true;
        camera.orthographicSize = 0.75f;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 10f;
        camera.allowHDR = false;
        camera.allowMSAA = false;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.11f, 0.19f, 1f);
        var target = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = target;
        var lightObject = new GameObject("Visual comparison light", typeof(Light));
        var light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1f;
        light.transform.rotation = Quaternion.identity;
        RenderSettings.sun = light;
        var meshObject = new GameObject("Visual comparison quad", typeof(MeshFilter), typeof(MeshRenderer));
        var mesh = new Mesh
        {
            vertices = new[] { new Vector3(-0.5f,-0.5f,0), new Vector3(0.5f,-0.5f,0), new Vector3(0.5f,0.5f,0), new Vector3(-0.5f,0.5f,0) },
            uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up },
            normals = Enumerable.Repeat(Vector3.back, 4).ToArray(),
            tangents = Enumerable.Repeat(new Vector4(1,0,0,1), 4).ToArray(),
            colors = Enumerable.Repeat(Color.white, 4).ToArray(),
            triangles = new[] { 0,2,1,0,3,2 },
        };
        mesh.RecalculateBounds();
        meshObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        var readback = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        var names = new[] { "Nova/Particles/UberUnlit", "Nova/UIParticles/UberUnlit", "Nova/Particles/UberLit", "Nova/UIParticles/UberLit" };
        var cases = new[] { "base", "vertex-color", "tint", "channel", "mirror", "tritone", "pentone", "greyscale", "gradient", "fade", "dissolve", "second-transition", "flow", "parallax", "emission", "luminance", "rim", "vertex-deformation", "array", "volume", "alpha-blend", "additive", "multiply", "cutout" };
        Cubemap environment = null;
        if (pbr)
        {
            names = names.Where(name => name.EndsWith("UberLit")).ToArray();
            cases = (from kind in new[] { "direct", "reflection" }
                     from workflow in new[] { 0, 1 }
                     from smoothness in new[] { 0, 1, 2 }
                     from angle in new[] { 0, 30, 60 }
                     select $"{kind}-{workflow}-{smoothness}-{angle}").ToArray();
            RenderSettings.ambientLight = Color.black;
            environment = new Cubemap(64, TextureFormat.RGBA32, true) { filterMode = FilterMode.Trilinear };
            for (var mip = 0; mip < 7; mip++)
            for (var face = 0; face < 6; face++)
                environment.SetPixels(Enumerable.Repeat(new Color(1f - mip / 8f, 0.2f + mip / 8f, 0.4f, 1),
                    (64 >> mip) * (64 >> mip)).ToArray(), (CubemapFace)face, mip);
            environment.Apply(false);
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflection = environment;
            RenderSettings.reflectionIntensity = 1;
            DynamicGI.UpdateEnvironment();
        }
        yield return null;
        yield return null;
        foreach (var name in names)
        foreach (var feature in cases)
        {
            var shader = Shader.Find(name);
            Assert.That(shader, Is.Not.Null, name);
            var material = new Material(shader);
            owned.Add(material);
            material.SetFloat("_Cull", 0);
            material.SetFloat("_RenderType", 0);
            material.SetFloat("_BaseMapMode", 0);
            material.SetFloat("_Metallic", 0);
            material.SetFloat("_LitWorkflowMode", 1); // Metallic workflow, dielectric surface.
            material.SetFloat("_Smoothness", 0);
            material.SetFloat("_EnvironmentReflections", 0);
            material.SetFloat("_SpecularHighlights", 0);
            material.SetTexture("_BaseMap", Texture((x,y) => new Color((x + 1f)/16f, (y + 1f)/16f, 0.25f, 0.8f)));
            mesh.colors = Enumerable.Repeat(feature == "vertex-color" ? new Color(0.3f,0.8f,0.5f,0.6f) : Color.white, 4).ToArray();
            mesh.tangents = Enumerable.Repeat(feature == "parallax" ? new Vector4(0.45f,0,0.89f,1) : new Vector4(1,0,0,1),4).ToArray();
            Configure(material, feature);
            if (pbr)
            {
                var parts = feature.Split('-');
                var reflection = parts[0] == "reflection";
                var workflow = int.Parse(parts[1]);
                var smoothness = new[] { 0f, 0.3f, 0.7f }[int.Parse(parts[2])];
                var angle = int.Parse(parts[3]);
                light.intensity = reflection ? 0f : smoothness > 0.5f ? 0.015f : 0.6f;
                light.transform.rotation = Quaternion.Euler(0, reflection ? 0 : angle, 0);
                mesh.normals = Enumerable.Repeat(Quaternion.Euler(0, reflection ? angle : 0, 0) * Vector3.back, 4).ToArray();
                material.SetTexture("_BaseMap", Texture((x,y) => Color.white));
                material.SetFloat("_LitWorkflowMode", workflow);
                material.SetFloat("_Metallic", 0.75f);
                material.SetColor("_SpecularColor", new Color(0.5f, 0.3f, 0.7f, 1));
                material.SetFloat("_Smoothness", smoothness);
                material.SetFloat("_EnvironmentReflections", reflection ? 1 : 0);
                material.SetFloat("_SpecularHighlights", reflection ? 0 : 1);
            }
            Setup(material, name.Contains("UberLit"));
            meshObject.GetComponent<MeshRenderer>().sharedMaterial = material;
            yield return null;
#if NOVA_VISUAL_REFERENCE
            RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
#else
            camera.Render();
#endif
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0,0,Size,Size),0,0);
            readback.Apply();
            RenderTexture.active = previous;
            var id = name.Replace("Nova/", "").Replace("/", "_") + "__" + feature;
            WritePpm(Path.Combine(output, id + ".ppm"), readback.GetPixels32());
            foreach (var item in owned) UnityEngine.Object.DestroyImmediate(item);
            owned.Clear();
        }
        File.WriteAllText(Path.Combine(output, "capture.txt"), Application.unityVersion + "\n" + SystemInfo.graphicsDeviceType + "\n" + QualitySettings.activeColorSpace + "\n" + names.Length * cases.Length + " frames\n");
        UnityEngine.Object.DestroyImmediate(cameraObject);
        UnityEngine.Object.DestroyImmediate(lightObject);
        UnityEngine.Object.DestroyImmediate(meshObject);
        UnityEngine.Object.DestroyImmediate(mesh);
        UnityEngine.Object.DestroyImmediate(readback);
        UnityEngine.Object.DestroyImmediate(target);
        if (environment != null) UnityEngine.Object.DestroyImmediate(environment);
    }

    private void Configure(Material m, string feature)
    {
        switch (feature)
        {
            case "tint": m.SetFloat("_TintAreaMode",1); m.SetFloat("_TintColorMode",0); m.SetColor("_TintColor",new Color(0.25f,0.8f,0.4f,0.5f)); m.SetFloat("_TintBlendRate",0.6f); break;
            case "channel": m.SetFloat("_BaseMapChannel",1); break;
            case "mirror": m.SetFloat("_BaseMapMirrorSampling",1); m.SetTextureScale("_BaseMap",new Vector2(2,1)); m.SetTextureOffset("_BaseMap",new Vector2(0.5f,0)); break;
            case "tritone": m.SetFloat("_BaseMapToneMode",1); m.SetFloat("_BaseMapToneChannel",0); break;
            case "pentone": m.SetFloat("_BaseMapToneMode",2); m.SetFloat("_BaseMapToneChannel",0); break;
            case "greyscale": m.SetFloat("_ColorCorrectionMode",1); break;
            case "gradient": m.SetFloat("_ColorCorrectionMode",2); m.SetTexture("_GradientMap",Texture((x,y)=>new Color(x/15f,0,1-x/15f,1))); break;
            case "fade": case "dissolve": case "second-transition":
                m.SetFloat("_AlphaTransitionMode",feature == "fade" ? 1 : 2);
                m.SetFloat("_RenderType",2); m.SetTexture("_AlphaTransitionMap",Texture((x,y)=>new Color(x/15f,x/15f,x/15f,x/15f)));
                m.SetFloat("_AlphaTransitionProgress",0.4f); m.SetFloat("_DissolveSharpness",0.75f);
                if (feature == "second-transition") { m.SetFloat("_AlphaTransitionSecondTextureBlendMode",2); m.SetTexture("_AlphaTransitionMapSecondTexture",Texture((x,y)=>new Color(y/15f,y/15f,y/15f,y/15f))); m.SetFloat("_AlphaTransitionProgressSecondTexture",0.2f); }
                break;
            case "flow": m.SetTexture("_FlowMap",Texture((x,y)=>new Color(0.75f,0.25f,0,1))); m.SetFloat("_FlowMapTarget",1); m.SetFloat("_FlowIntensity",0.3f); break;
            case "parallax": m.SetTexture("_ParallaxMap",Texture((x,y)=>Color.white)); m.SetFloat("_ParallaxMapTarget",1); m.SetFloat("_ParallaxStrength",1); break;
            case "emission": m.SetFloat("_EmissionAreaType",2); m.SetTexture("_EmissionMap",Texture((x,y)=>new Color(x/15f,0,0,1))); m.SetColor("_EmissionColor",new Color(0.2f,0.4f,1,1)); m.SetFloat("_EmissionIntensity",0.7f); break;
            case "luminance": m.SetFloat("_RenderType",2); m.SetFloat("_LuminanceTransparencyEnabled",1); m.SetFloat("_LuminanceTransparencyProgress",0.5f); break;
            case "rim": m.SetFloat("_RenderType",2); m.SetFloat("_RimTransparencyEnabled",1); m.SetFloat("_RimTransparencyProgress",0.6f); break;
            case "vertex-deformation": m.SetFloat("_VertexDeformationEnabled",1); m.SetTexture("_VertexDeformationMap",Texture((x,y)=>Color.white)); m.SetFloat("_VertexDeformationIntensity",0.3f); break;
            case "array":
                var array = new Texture2DArray(16,16,2,TextureFormat.RGBA32,false); owned.Add(array); array.filterMode=FilterMode.Point;
                array.SetPixels(Enumerable.Repeat(Color.red,256).ToArray(),0); array.SetPixels(Enumerable.Repeat(Color.green,256).ToArray(),1); array.Apply();
                m.SetTexture("_BaseMap2DArray",array); m.SetFloat("_BaseMapMode",1); m.SetFloat("_BaseMapSliceCount",2); m.SetFloat("_BaseMapProgress",0.75f); break;
            case "volume":
                var volume = new Texture3D(16,16,2,TextureFormat.RGBA32,false); owned.Add(volume); volume.filterMode=FilterMode.Bilinear;
                volume.SetPixels(Enumerable.Repeat(Color.red,256).Concat(Enumerable.Repeat(Color.blue,256)).ToArray()); volume.Apply();
                m.SetTexture("_BaseMap3D",volume); m.SetFloat("_BaseMapMode",2); m.SetFloat("_BaseMapSliceCount",2); m.SetFloat("_BaseMapProgress",0.3f); break;
            case "alpha-blend": m.SetFloat("_RenderType",2); m.SetFloat("_TransparentBlendMode",0); break;
            case "additive": m.SetFloat("_RenderType",2); m.SetFloat("_TransparentBlendMode",1); break;
            case "multiply": m.SetFloat("_RenderType",2); m.SetFloat("_TransparentBlendMode",2); break;
            case "cutout": m.SetFloat("_RenderType",1); m.SetFloat("_Cutoff",0.5f); m.SetTexture("_BaseMap",Texture((x,y)=>new Color(0.4f,0.8f,0.2f,x/15f))); break;
        }
    }

    private Texture2D Texture(Func<int,int,Color> pixel)
    {
        var texture = new Texture2D(16,16,TextureFormat.RGBA32,false) { filterMode=FilterMode.Point, wrapMode=TextureWrapMode.Repeat };
        for (var y=0;y<16;y++) for (var x=0;x<16;x++) texture.SetPixel(x,y,pixel(x,y));
        texture.Apply(); owned.Add(texture); return texture;
    }

    private static void Setup(Material material, bool lit)
    {
        var name = "Nova.Editor.Core.Scripts.ParticlesUber" + (lit ? "Lit" : "Unlit") + "MaterialPostProcessor";
        var type = AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).FirstOrDefault(t=>t!=null);
        Assert.That(type,Is.Not.Null,name);
        foreach (var method in new[] { "SetupMaterialKeywords", "SetupMaterialBlendMode" })
            type.GetMethod(method,BindingFlags.Public|BindingFlags.Static).Invoke(null,new object[]{material});
    }

    private static void WritePpm(string path, Color32[] colors)
    {
        using (var stream = File.Create(path))
        {
            var header = Encoding.ASCII.GetBytes("P6\n"+Size+" "+Size+"\n255\n"); stream.Write(header,0,header.Length);
            for (var y=Size-1;y>=0;y--) for (var x=0;x<Size;x++)
            { var c=colors[y*Size+x]; stream.WriteByte(c.r); stream.WriteByte(c.g); stream.WriteByte(c.b); }
        }
    }
}

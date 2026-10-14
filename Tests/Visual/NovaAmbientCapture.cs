// Identical original/port fixture. Define NOVA_VISUAL_REFERENCE only in the original project.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
#if NOVA_VISUAL_REFERENCE
using UnityEngine.Rendering.Universal;
#endif

public sealed class NovaAmbientCapture
{
    [UnityTest]
    public IEnumerator CaptureAmbient()
    {
        var args = Environment.GetCommandLineArgs();
        var output = Path.Combine(args[Array.IndexOf(args, "-novaVisualOutput") + 1], "ambient");
        Directory.CreateDirectory(output);
        var owned = new List<UnityEngine.Object>();
        var oldPipeline = GraphicsSettings.defaultRenderPipeline;
        var oldQuality = QualitySettings.renderPipeline;
        var oldMode = RenderSettings.ambientMode;
        var oldAmbient = RenderSettings.ambientLight;
        var oldSky = RenderSettings.ambientSkyColor; var oldEquator = RenderSettings.ambientEquatorColor;
        var oldGround = RenderSettings.ambientGroundColor; var oldProbe = RenderSettings.ambientProbe;
        var oldIntensity = RenderSettings.reflectionIntensity;
        var oldFog = RenderSettings.fog;
        var oldTarget = RenderTexture.active;
        var oldLinear = GraphicsSettings.lightsUseLinearIntensity;
        var state = new List<string>();
        try
        {
            GraphicsSettings.lightsUseLinearIntensity = QualitySettings.activeColorSpace == ColorSpace.Linear;
#if NOVA_VISUAL_REFERENCE
            var data = ScriptableObject.CreateInstance<UniversalRendererData>(); owned.Add(data);
#if !UNITY_6000_0_OR_NEWER
            typeof(UniversalRendererData).GetMethod("ReloadAllNullProperties", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(data, null);
#endif
            var pipeline = UniversalRenderPipelineAsset.Create(data); owned.Add(pipeline);
            pipeline.msaaSampleCount = 1; pipeline.supportsHDR = false;
            GraphicsSettings.defaultRenderPipeline = pipeline; QualitySettings.renderPipeline = pipeline;
#else
            GraphicsSettings.defaultRenderPipeline = null; QualitySettings.renderPipeline = null;
#endif
            RenderSettings.fog = false;
            var cameraObject = new GameObject("Ambient capture", typeof(Camera)); owned.Add(cameraObject);
            var camera = cameraObject.GetComponent<Camera>();
            camera.transform.position = new Vector3(0, 0, -3);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            camera.allowHDR = false; camera.allowMSAA = false; camera.cullingMask = 1 << 29;
            var target = new RenderTexture(128,128,24); owned.Add(target); camera.targetTexture = target;
            var read = new Texture2D(128,128,TextureFormat.RGBA32,false); owned.Add(read);
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); owned.Add(sphere);
            sphere.layer = 29; sphere.transform.localScale = Vector3.one * 2;
            var renderer = sphere.GetComponent<Renderer>();
            foreach (var shader in new[] { "Nova/Particles/UberLit", "Nova/UIParticles/UberLit" })
            foreach (var ambient in new[] { "black", "flat", "colored", "gradient", "sky-probe" })
            foreach (var intensity in new[] { 0f, 1f })
            foreach (var surface in new[] { "diffuse", "dielectric", "metal" })
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = ambient == "black" ? Color.black : ambient == "flat" ? new Color(.15f,.15f,.15f) : new Color(.06f,.22f,.45f);
                if (ambient == "gradient")
                {
                    RenderSettings.ambientMode = AmbientMode.Trilight;
                    RenderSettings.ambientSkyColor = new Color(.06f,.22f,.45f);
                    RenderSettings.ambientEquatorColor = new Color(.15f,.09f,.04f);
                    RenderSettings.ambientGroundColor = new Color(.02f,.08f,.01f);
                }
                if (ambient == "sky-probe")
                {
                    RenderSettings.ambientMode = AmbientMode.Skybox;
                    var skyProbe = new SphericalHarmonicsL2();
                    skyProbe.AddAmbientLight(new Color(.03f,.06f,.02f));
                    skyProbe.AddDirectionalLight(new Vector3(.3f,.8f,-.5f).normalized,new Color(.12f,.04f,.02f),1);
                    RenderSettings.ambientProbe = skyProbe;
                }
                RenderSettings.reflectionIntensity = intensity;
                var material = new Material(Shader.Find(shader));
                try
                {
                    material.SetFloat("_RenderType",0); material.SetFloat("_Cull",0);
                    material.SetFloat("_LitWorkflowMode",surface == "diffuse" ? 0 : 1);
                    material.SetColor("_SpecularColor",Color.black);
                    material.SetFloat("_Metallic",surface == "metal" ? 1 : 0);
                    material.SetFloat("_Smoothness",.6f);
                    material.SetFloat("_SpecularHighlights",0);
                    material.SetFloat("_EnvironmentReflections",0);
                    material.SetFloat("_LitReceiveShadows",0);
                    var processor = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor")).First(t => t != null);
                    foreach (var method in new[] { "SetupMaterialKeywords", "SetupMaterialBlendMode" }) processor.GetMethod(method).Invoke(null,new object[]{material});
                    renderer.sharedMaterial = material;
                    yield return null; yield return null;
                    camera.Render(); RenderTexture.active = target;
                    read.ReadPixels(new Rect(0,0,128,128),0,0); read.Apply();
                    var name = shader.Replace('/','-') + "_" + ambient + "_" + intensity + "_" + surface;
                    using (var stream = File.Create(Path.Combine(output,name + ".ppm")))
                    {
                        var header = System.Text.Encoding.ASCII.GetBytes("P6\n128 128\n255\n"); stream.Write(header,0,header.Length);
                        var pixels = read.GetPixels32();
                        for (int y=127;y>=0;y--) for(int x=0;x<128;x++)
                        { var p=pixels[y*128+x]; stream.WriteByte(p.r); stream.WriteByte(p.g); stream.WriteByte(p.b); }
                    }
                    var sh = RenderSettings.ambientProbe;
                    state.Add(name + " L0=" + new Vector3(sh[0,0],sh[1,0],sh[2,0]).ToString("R") + " glossy=" + Shader.GetGlobalVector("_GlossyEnvironmentColor").ToString("R") + " indirect=" + Shader.GetGlobalVector("unity_IndirectSpecColor").ToString("R") + " center=" + read.GetPixel(64,64).ToString("R") + " sky=" + Shader.GetGlobalVector("unity_AmbientSky").ToString("R") + " equator=" + Shader.GetGlobalVector("unity_AmbientEquator").ToString("R") + " ground=" + Shader.GetGlobalVector("unity_AmbientGround").ToString("R") + " ar=" + Shader.GetGlobalVector("unity_SHAr").ToString("R"));
                }
                finally { UnityEngine.Object.DestroyImmediate(material); }
            }
            File.WriteAllLines(Path.Combine(output,"state.txt"),state);
            File.WriteAllText(Path.Combine(output,"metadata.txt"),Application.unityVersion + "\n" + QualitySettings.activeColorSpace + "\n" + SystemInfo.graphicsDeviceType + "\n");
        }
        finally
        {
            RenderTexture.active = oldTarget;
            GraphicsSettings.defaultRenderPipeline = oldPipeline; QualitySettings.renderPipeline = oldQuality;
            GraphicsSettings.lightsUseLinearIntensity = oldLinear;
            RenderSettings.ambientMode = oldMode; RenderSettings.ambientLight = oldAmbient;
            RenderSettings.ambientSkyColor=oldSky; RenderSettings.ambientEquatorColor=oldEquator;
            RenderSettings.ambientGroundColor=oldGround; RenderSettings.ambientProbe=oldProbe;
            RenderSettings.reflectionIntensity = oldIntensity; RenderSettings.fog = oldFog;
            foreach (var item in owned.OfType<GameObject>()) UnityEngine.Object.DestroyImmediate(item);
            foreach (var item in owned.AsEnumerable().Reverse()) if (item != null) UnityEngine.Object.DestroyImmediate(item);
        }
    }
}

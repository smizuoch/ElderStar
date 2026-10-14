// Run the identical fixture in isolated original/port projects; define NOVA_VISUAL_REFERENCE only for original.
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

public sealed class NovaInterpolationCapture
{
    [UnityTest]
    public IEnumerator CaptureInterpolation()
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-novaVisualOutput");
        Assert.That(index, Is.GreaterThanOrEqualTo(0));
        var output = args[index + 1]; Directory.CreateDirectory(output);
        var owned = new List<UnityEngine.Object>();
        var oldPipeline = GraphicsSettings.defaultRenderPipeline;
        var oldQuality = QualitySettings.renderPipeline;
        var oldAmbient = RenderSettings.ambientLight; var oldAmbientMode = RenderSettings.ambientMode;
        var oldFog = RenderSettings.fog; var oldFogMode = RenderSettings.fogMode;
        var oldFogColor = RenderSettings.fogColor; var oldFogDensity = RenderSettings.fogDensity;
        var oldFogStart = RenderSettings.fogStartDistance; var oldFogEnd = RenderSettings.fogEndDistance;
        var oldLinear = GraphicsSettings.lightsUseLinearIntensity;
        var oldTemperature = GraphicsSettings.lightsUseColorTemperature;
        var oldTarget = RenderTexture.active;
        var oldInvert = GL.invertCulling;
        try
        {
            GraphicsSettings.lightsUseLinearIntensity = QualitySettings.activeColorSpace == ColorSpace.Linear; GraphicsSettings.lightsUseColorTemperature = true;
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
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = Environment.GetCommandLineArgs().Contains("-novaBlackAmbient") ? Color.black : new Color(.15f,.15f,.15f);
            var camObject = new GameObject("Interpolation camera", typeof(Camera)); owned.Add(camObject);
            var camera = camObject.GetComponent<Camera>(); camera.transform.position = new Vector3(0, 0, -3);
            camera.nearClipPlane = .3f; camera.farClipPlane = 20; camera.fieldOfView = 55;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            camera.allowHDR = false; camera.allowMSAA = false;
            var target = new RenderTexture(128, 128, 24); owned.Add(target); camera.targetTexture = target;
            var cpu = new Texture2D(128, 128, TextureFormat.RGBA32, false); owned.Add(cpu);
            var lightObject = new GameObject("Interpolation key light", typeof(Light)); owned.Add(lightObject);
            var light = lightObject.GetComponent<Light>(); light.type = LightType.Directional;
            light.intensity = .8f; light.shadows = LightShadows.None;
            light.transform.rotation = Quaternion.Euler(20, -30, 0);
            var effect = new GameObject("Coarse curved mesh", typeof(MeshFilter), typeof(MeshRenderer)); owned.Add(effect);
            var mesh = CreateMesh(); owned.Add(mesh); effect.GetComponent<MeshFilter>().sharedMesh = mesh;
            var baseMap = new Texture2D(32, 32, TextureFormat.RGBA32, false, true); owned.Add(baseMap);
            baseMap.wrapMode = TextureWrapMode.Repeat; baseMap.filterMode = FilterMode.Bilinear;
            for (var y = 0; y < 32; y++) for (var x = 0; x < 32; x++)
                baseMap.SetPixel(x, y, new Color(.1f + .8f * x / 31, .1f + .8f * y / 31, .35f, 1));
            baseMap.Apply();
            var normal = new Texture2D(2, 2, TextureFormat.RGBA32, false, true); owned.Add(normal);
            normal.SetPixels(Enumerable.Repeat(new Color(.68f, .78f, .9f, 1), 4).ToArray()); normal.Apply();
            var controls = new Dictionary<string, Color32[]>();
            foreach (var shader in new[] { "Nova/Particles/UberUnlit", "Nova/UIParticles/UberUnlit", "Nova/Particles/UberLit", "Nova/UIParticles/UberLit" })
            foreach (var feature in new[] { "control", "rim", "parallax", "normal", "fog-linear", "fog-exp", "fog-exp2", "combined" })
            foreach (var view in new[] { "perspective", "orthographic", "mirror", "negative-scale", "near-clip", "oblique" })
            {
                var material = new Material(Shader.Find(shader));
                try
                {
                    material.SetTexture("_BaseMap", baseMap); material.SetFloat("_Cull", 0);
                    material.SetFloat("_RenderType", 0); material.SetFloat("_Metallic", 0);
                    if (material.HasProperty("_LitWorkflowMode")) material.SetFloat("_LitWorkflowMode", 1);
                    if (material.HasProperty("_SpecularHighlights")) material.SetFloat("_SpecularHighlights", 0);
                    if (material.HasProperty("_EnvironmentReflections")) material.SetFloat("_EnvironmentReflections", 0);
                    if (material.HasProperty("_LitReceiveShadows")) material.SetFloat("_LitReceiveShadows", 0);
                    if (feature == "rim" || feature == "combined")
                    {
                        material.SetFloat("_TintAreaMode", 2); material.SetFloat("_TintColorMode", 0);
                        material.SetColor("_TintColor", Color.green); material.SetFloat("_TintBlendRate", 1);
                        material.SetFloat("_TintRimProgress", .4f); material.SetFloat("_TintRimSharpness", .6f);
                    }
                    if (feature == "parallax" || feature == "combined")
                    {
                        material.SetTexture("_ParallaxMap", baseMap); material.SetFloat("_ParallaxMapTarget", 1);
                        material.SetFloat("_ParallaxStrength", .7f);
                    }
                    if ((feature == "normal" || feature == "combined") && material.HasProperty("_NormalMap"))
                    {
                        material.SetTexture("_NormalMap", normal); material.SetFloat("_NormalMapBumpScale", 1);
                    }
                    var name = shader.EndsWith("UberUnlit") ? "ParticlesUberUnlitMaterialPostProcessor" : "ParticlesUberLitMaterialPostProcessor";
                    var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Editor.Core.Scripts." + name)).First(t => t != null);
                    type.GetMethod("SetupMaterialKeywords").Invoke(null, new object[] { material });
                    type.GetMethod("SetupMaterialBlendMode")?.Invoke(null, new object[] { material });
                    effect.GetComponent<Renderer>().sharedMaterial = material;
                    effect.transform.localScale = new Vector3(view == "negative-scale" ? -1.1f : 1.1f, .8f, 1.4f);
                    camera.orthographic = view == "orthographic"; camera.orthographicSize = 1.4f;
                    camera.nearClipPlane = view == "near-clip" ? 2.7f : .3f;
                    camera.ResetWorldToCameraMatrix(); camera.ResetProjectionMatrix();
                    if (view == "oblique") camera.projectionMatrix = camera.CalculateObliqueMatrix(new Vector4(.15f,.2f,-1,-2));
                    if (view == "mirror") camera.worldToCameraMatrix = camera.worldToCameraMatrix * Matrix4x4.Scale(new Vector3(-1, 1, 1));
                    GL.invertCulling = view == "mirror";
                    RenderSettings.fog = feature.StartsWith("fog-") || feature == "combined";
                    RenderSettings.fogMode = feature == "fog-exp" ? FogMode.Exponential : feature == "fog-exp2" ? FogMode.ExponentialSquared : FogMode.Linear;
                    RenderSettings.fogStartDistance = 1.5f; RenderSettings.fogEndDistance = 3.5f;
                    RenderSettings.fogDensity = .35f; RenderSettings.fogColor = new Color(.1f, .2f, .35f);
                    yield return null; yield return null;
                    camera.Render(); RenderTexture.active = target;
                    cpu.ReadPixels(new Rect(0, 0, 128, 128), 0, 0); cpu.Apply();
                    var pixels = cpu.GetPixels32();
                    Assert.That(pixels.Count(p => p.r + p.g + p.b > 20), Is.GreaterThan(500), shader + " " + feature + " " + view + " visible geometry witness");
                    var key = shader + view;
                    if (feature == "control") controls[key] = pixels;
                    else if ((feature != "normal" || shader.EndsWith("UberLit")) && !((view == "orthographic" || view == "near-clip" || view == "oblique") && feature.StartsWith("fog-")))
                        Assert.That(pixels.Zip(controls[key], (a,b) => Math.Abs(a.r-b.r)+Math.Abs(a.g-b.g)+Math.Abs(a.b-b.b)).Count(d => d > 3), Is.GreaterThan(32), feature + " must visibly affect the image");
                    Save(Path.Combine(output, shader.Replace('/', '-') + "_" + feature + "_" + view + ".ppm"), pixels);
                }
                finally { UnityEngine.Object.DestroyImmediate(material); }
            }
        }
        finally
        {
            RenderTexture.active = oldTarget; GL.invertCulling = oldInvert;
            GraphicsSettings.defaultRenderPipeline = oldPipeline; QualitySettings.renderPipeline = oldQuality;
            GraphicsSettings.lightsUseLinearIntensity = oldLinear; GraphicsSettings.lightsUseColorTemperature = oldTemperature;
            RenderSettings.ambientLight = oldAmbient; RenderSettings.ambientMode = oldAmbientMode;
            RenderSettings.fog = oldFog; RenderSettings.fogMode = oldFogMode; RenderSettings.fogColor = oldFogColor;
            RenderSettings.fogDensity = oldFogDensity; RenderSettings.fogStartDistance = oldFogStart; RenderSettings.fogEndDistance = oldFogEnd;
            foreach (var item in owned.OfType<GameObject>()) UnityEngine.Object.DestroyImmediate(item);
            foreach (var item in owned.AsEnumerable().Reverse()) if (item != null) UnityEngine.Object.DestroyImmediate(item);
        }
    }

    private static Mesh CreateMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = new[] { new Vector3(-1,-1,-.8f), new Vector3(1,-1,.6f), new Vector3(-1,1,.6f), new Vector3(1,1,-.3f) };
        mesh.normals = new[] { new Vector3(-.8f,-.1f,-1).normalized, new Vector3(.6f,-.2f,-1).normalized,
            new Vector3(-.2f,.7f,-1).normalized, new Vector3(.8f,.4f,-1).normalized };
        mesh.tangents = mesh.normals.Select(n => { var t = Vector3.Cross(n, Vector3.up).normalized; return new Vector4(t.x,t.y,t.z,1); }).ToArray();
        mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
        mesh.colors = Enumerable.Repeat(Color.white,4).ToArray(); mesh.triangles = new[] { 0,2,1,2,3,1 };
        mesh.RecalculateBounds(); return mesh;
    }

    private static void Save(string path, Color32[] pixels)
    {
        using (var stream = File.Create(path))
        {
            var header = System.Text.Encoding.ASCII.GetBytes("P6\n128 128\n255\n"); stream.Write(header,0,header.Length);
            for (var y = 127; y >= 0; y--) for (var x = 0; x < 128; x++)
            { var p = pixels[y*128+x]; stream.WriteByte(p.r); stream.WriteByte(p.g); stream.WriteByte(p.b); }
        }
    }
}

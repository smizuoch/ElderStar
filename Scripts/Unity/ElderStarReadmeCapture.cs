// Documentation capture harness. Copy into an isolated Built-in project's Assets/Editor.
// Runs real ElderStar Inspector code and real camera rendering; never ships in the package.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Nova.Editor.Core.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public sealed class ElderStarReadmeCapture : EditorWindow
{
    private static string output;
    private static ElderStarReadmeCapture window;
    private static double next;
    private static int index;
    private static bool ready;
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static string[] Sections = { "RenderSettings", "VertexDeformation", "BaseMap", "TintColor", "FlowMap", "ParallaxMap", "ColorCorrection", "AlphaTransition", "Emission", "Transparency", "ShadowCaster", "LitSettings", "SurfaceMaps", "Distortion", "DistortionFlow", "DistortionAlpha", "DistortionTransparency", "CanvasLighting" };
    private readonly Dictionary<string, (bool exists, bool value)> prefs = new Dictionary<string, (bool, bool)>();
    private Material material;
    private MaterialEditor materialEditor;
    private ShaderGUI shaderGUI;
    private UnityEditor.Editor componentEditor;
    private GameObject canvas;
    private Exception error;
    private float contentHeight;

    public static void Capture()
    {
        var args = Environment.GetCommandLineArgs();
        output = args[Array.IndexOf(args, "-elderstarCaptureOutput") + 1];
        index = 0;
        ready = false;
        var subset = Array.IndexOf(args, "-elderstarSections");
        if (subset >= 0) Sections = args[subset + 1].Split(',');
        Directory.CreateDirectory(output);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GraphicsSettings.defaultRenderPipeline = null;
        QualitySettings.renderPipeline = null;
        window = CreateInstance<ElderStarReadmeCapture>();
        window.titleContent = new GUIContent("ElderStar documentation capture");
        window.position = new Rect(30, 30, 640, 900);
        window.ShowUtility();
        window.Focus();
        window.Setup(Sections[0]);
        next = EditorApplication.timeSinceStartup + 3;
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < next) { window.Repaint(); return; }
        try
        {
            if (window.error != null) throw window.error;
            // The native capture client saves the bounded window and acknowledges
            // each section. Hidden GUI buffers are not reliable on Windows.
            if (!ready)
            {
                window.position = new Rect(window.position.x, window.position.y, 640, Mathf.Ceil(window.contentHeight) + 4);
                window.titleContent = new GUIContent("ElderStar - " + Sections[index]);
                File.WriteAllText(Path.Combine(output, "current.txt"), Sections[index]);
                ready = true;
                next = EditorApplication.timeSinceStartup + 1;
                return;
            }
            if (!File.Exists(Path.Combine(output, Sections[index] + ".ack"))) return;
            index++;
            ready = false;
            if (index < Sections.Length)
            {
                window.Setup(Sections[index]);
                next = EditorApplication.timeSinceStartup + 1;
                return;
            }
            EditorApplication.update -= Tick;
            window.Close();
            CaptureEffects();
            File.WriteAllText(Path.Combine(output, "environment.txt"), Application.unityVersion + "\n" + SystemInfo.graphicsDeviceType + "\n" + QualitySettings.activeColorSpace + "\n");
            Debug.Log("ELDERSTAR_README_CAPTURE_COMPLETE=" + output);
            EditorApplication.Exit(0);
        }
        catch (Exception e) { EditorApplication.update -= Tick; if (window != null) window.Close(); Debug.LogException(e); EditorApplication.Exit(1); }
    }

    private void Setup(string section)
    {
        if (materialEditor != null) DestroyImmediate(materialEditor);
        if (material != null) DestroyImmediate(material);
        if (componentEditor != null) DestroyImmediate(componentEditor);
        if (canvas != null) DestroyImmediate(canvas);
        if (section == "CanvasLighting")
        {
            canvas = new GameObject("Canvas", typeof(Canvas));
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Runtime.UI.NovaCanvasLighting")).FirstOrDefault(t => t != null);
            if (type == null) type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).First(t => t.Name == "NovaCanvasLighting");
            componentEditor = UnityEditor.Editor.CreateEditor(canvas.AddComponent(type));
            return;
        }
        var distortion = section.StartsWith("Distortion");
        var lit = section == "LitSettings" || section == "SurfaceMaps";
        var shader = Shader.Find("Nova/Particles/" + (distortion ? "Distortion" : lit ? "UberLit" : "UberUnlit"));
        material = new Material(shader) { name = "ElderStar " + section };
        foreach (var property in new[] { "_ParallaxMapEnabled", "_VertexDeformationEnabled", "_SoftParticlesEnabled", "_DepthFadeEnabled", "_ShadowCasterEnabled" })
            if (material.HasProperty(property)) material.SetFloat(property, 1);
        foreach (var setting in new Dictionary<string, float> { { "_RenderType", 2 }, { "_TintAreaMode", 2 }, { "_TintColorMode", 0 }, { "_AlphaTransitionMode", 2 }, { "_EmissionAreaType", 1 }, { "_EmissionColorType", 0 }, { "_ColorCorrectionMode", 2 }, { "_FlowMapTarget", 15 }, { "_ParallaxMapTarget", 7 }, { "_LitWorkflowMode", 1 } })
            if (material.HasProperty(setting.Key)) material.SetFloat(setting.Key, setting.Value);
        materialEditor = (MaterialEditor)UnityEditor.Editor.CreateEditor(material);
        var guiType = typeof(ParticlesGUI).Assembly.GetType("Nova.Editor.Core.Scripts.Particles" + (distortion ? "Distortion" : lit ? "UberLit" : "UberUnlit") + "GUI");
        shaderGUI = (ShaderGUI)Activator.CreateInstance(guiType, true);
        shaderGUI.ValidateMaterial(material);
        var properties = MaterialEditor.GetMaterialProperties(materialEditor.targets);
        Method(guiType, "Initialize").Invoke(shaderGUI, new object[] { materialEditor, properties });
        Method(guiType, "SetupProperties").Invoke(shaderGUI, new object[] { properties });
        if (!distortion)
        {
            var common = Field(guiType, "_commonGUI").GetValue(shaderGUI);
            var commonProps = Field(guiType, "_commonMaterialProperties").GetValue(shaderGUI);
            common.GetType().GetMethod("Setup").Invoke(common, new[] { (object)materialEditor, commonProps });
            if (lit) Field(guiType, "_editor").SetValue(shaderGUI, materialEditor);
        }
        foreach (var key in Sections.Where(s => !s.StartsWith("Distortion")).Select(s => "Nova.Editor.Core.Scripts.ParticlesUberCommonMaterialProperties." + s + "Foldout"))
        {
            if (!prefs.ContainsKey(key)) prefs[key] = (EditorPrefs.HasKey(key), EditorPrefs.GetBool(key));
            EditorPrefs.SetBool(key, true);
        }
    }

    private void OnGUI()
    {
        if (error != null) return;
        try
        {
            GUILayout.BeginArea(new Rect(8, 8, 624, 880));
            if (componentEditor != null) componentEditor.OnInspectorGUI();
            else if (shaderGUI != null)
            {
                var section = Sections[index];
                var type = shaderGUI.GetType();
                EditorGUIUtility.labelWidth = 180;
                if (section.StartsWith("Distortion"))
                {
                    var method = section == "Distortion" ? "DrawDistortionProperties" : section == "DistortionFlow" ? "DrawFlowMapProperties" : section == "DistortionAlpha" ? "DrawAlphaTransitionProperties" : "DrawTransparencyProperties";
                    EditorGUILayout.LabelField(section == "Distortion" ? "Distortion" : section.Replace("Distortion", "Distortion / "), EditorStyles.boldLabel);
                    Method(type, method).Invoke(shaderGUI, new object[] { materialEditor, MaterialEditor.GetMaterialProperties(materialEditor.targets) });
                }
                else if (section == "SurfaceMaps")
                {
                    EditorGUILayout.LabelField("Surface Maps", EditorStyles.boldLabel);
                    Method(type, "InternalDrawSurfaceMapsProperties").Invoke(shaderGUI, null);
                }
                else
                {
                    var common = Field(type, "_commonGUI").GetValue(shaderGUI);
                    if (section == "RenderSettings" || section == "LitSettings")
                    {
                        Action extra = section == "LitSettings" ? (Action)(() => Method(type, "InternalRenderSettingsProperties").Invoke(shaderGUI, null)) : null;
                        common.GetType().GetMethod("DrawRenderSettingsProperties").Invoke(common, new object[] { extra });
                    }
                    else common.GetType().GetMethod("Draw" + section + "Properties").Invoke(common, null);
                }
            }
            GUILayout.Space(8);
            if (Event.current.type == EventType.Repaint) contentHeight = GUILayoutUtility.GetLastRect().yMax + 16;
            GUILayout.EndArea();
        }
        catch (Exception e) { error = e; }
    }

    private void OnDisable()
    {
        foreach (var pref in prefs) if (pref.Value.exists) EditorPrefs.SetBool(pref.Key, pref.Value.value); else EditorPrefs.DeleteKey(pref.Key);
        if (materialEditor != null) DestroyImmediate(materialEditor);
        if (material != null) DestroyImmediate(material);
        if (componentEditor != null) DestroyImmediate(componentEditor);
        if (canvas != null) DestroyImmediate(canvas);
    }

    private static MethodInfo Method(Type type, string name)
    {
        while (type != null) { var m = type.GetMethod(name, Private | BindingFlags.DeclaredOnly); if (m != null) return m; type = type.BaseType; }
        throw new MissingMethodException(name);
    }
    private static FieldInfo Field(Type type, string name)
    {
        while (type != null) { var f = type.GetField(name, Private | BindingFlags.DeclaredOnly); if (f != null) return f; type = type.BaseType; }
        throw new MissingFieldException(name);
    }
    private static int Save(RenderTexture target, string path)
    {
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
        var pixels = image.GetPixels32();
        var visible = pixels.Count(p => Math.Max(p.r, Math.Max(p.g, p.b)) > 100);
        File.WriteAllBytes(path, image.EncodeToPNG());
        DestroyImmediate(image);
        RenderTexture.active = previous;
        return visible;
    }

    public static void CaptureEffectsOnly()
    {
        try
        {
            var args = Environment.GetCommandLineArgs();
            output = args[Array.IndexOf(args, "-elderstarCaptureOutput") + 1];
            Directory.CreateDirectory(output);
            CaptureEffects();
            Debug.Log("ELDERSTAR_EFFECT_CAPTURE_COMPLETE=" + output);
            EditorApplication.Exit(0);
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }

    private static void CaptureEffects()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.fog = false;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.12f, .14f, .2f);
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.customReflection = AssetDatabase.LoadAssetAtPath<Cubemap>("Assets/Samples/Materials/SampleLit/ReflectionProbe-0.exr");
        RenderSettings.reflectionIntensity = 1;
        var light = new GameObject("Key", typeof(Light)).GetComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1;
        light.transform.rotation = Quaternion.Euler(35, -30, 0);
        var camera = new GameObject("Documentation camera", typeof(Camera)).GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.025f, .035f, .065f);
        camera.orthographic = true; camera.orthographicSize = 3;
        camera.allowHDR = false; camera.allowMSAA = false;
        camera.depthTextureMode = DepthTextureMode.Depth;
        camera.nearClipPlane = .01f; camera.farClipPlane = 100;
        var target = new RenderTexture(960, 600, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = target;
        var samples = new[] { "pfb_eff_barrier01", "pfb_eff_ice02", "pfb_eff_slash01", "pfb_eff_heal01" };
        foreach (var sample in samples)
        {
            var effect = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/Prefabs/" + sample + ".prefab"));
            var systems = effect.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++) { systems[i].useAutoRandomSeed = false; systems[i].randomSeed = (uint)(23456 + i); }
            var focus = sample == "pfb_eff_ice02" ? new Vector3(-1.1f, 1, 0) : sample == "pfb_eff_slash01" ? Vector3.zero : new Vector3(0, 1, 0);
            var elevation = sample == "pfb_eff_slash01" ? 60 : 15;
            camera.transform.position = focus + Quaternion.Euler(elevation, 15, 0) * Vector3.back * 8;
            camera.transform.LookAt(focus);
            Directory.CreateDirectory(Path.Combine(output, sample));
            int maxVisible = 0;
            for (int frame = 0; frame < 48; frame++)
            {
                var time = .1f + frame / 24f;
                foreach (var ps in systems) { ps.Simulate(time, false, true, true); ps.Pause(false); }
                camera.Render();
                maxVisible = Math.Max(maxVisible, Save(target, Path.Combine(output, sample, frame.ToString("000") + ".png")));
            }
            if (maxVisible < 800) throw new InvalidOperationException("No clearly visible effect: " + sample);
            File.AppendAllText(Path.Combine(output, "visible-pixels.tsv"), sample + "\t" + maxVisible + "\n");
            foreach (var shader in effect.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m != null).Select(m => m.shader).Distinct())
                if (ShaderUtil.GetShaderMessages(shader).Any(m => m.severity.ToString() == "Error")) throw new InvalidOperationException("Shader errors: " + shader.name);
            DestroyImmediate(effect);
        }
        DestroyImmediate(target);
    }
}

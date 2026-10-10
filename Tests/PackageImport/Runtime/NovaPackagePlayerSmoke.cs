using System;
using System.Collections;
using System.IO;
using Nova.Runtime.Core.Scripts;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaPackagePlayerSmoke : MonoBehaviour
{
    private IEnumerator Start()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.white;
        yield return null;
        yield return null;
        var reportPath = Argument("-novaReport", Path.Combine(Application.persistentDataPath, "nova-player-smoke.txt"));
        try
        {
            Run();
            File.WriteAllText(reportPath, "PASS: " + Application.unityVersion +
                "\nSix original shaders found. Four Uber color renders passed. Distortion offset and disable passed.\n");
            Debug.Log("NOVA PLAYER SMOKE PASSED");
            Application.Quit(0);
        }
        catch (Exception exception)
        {
            File.WriteAllText(reportPath, "FAIL: " + exception);
            Debug.LogException(exception);
            Application.Quit(1);
        }
    }

    private static void Run()
    {
        if (GraphicsSettings.currentRenderPipeline != null) throw new Exception("This validation requires Built-in.");
        foreach (var name in new[] { "Nova/Particles/UberUnlit", "Nova/Particles/UberLit", "Nova/UIParticles/UberUnlit",
            "Nova/UIParticles/UberLit", "Nova/Particles/Distortion", "Hidden/Nova/Particles/ApplyDistortion" })
            if (Shader.Find(name) == null) throw new Exception("Shader missing from player: " + name);
        var camera = new GameObject("Verification camera", typeof(Camera)).GetComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -3);
        camera.orthographic = true;
        camera.orthographicSize = 1;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.allowHDR = false;
        camera.allowMSAA = false;
        camera.targetTexture = new RenderTexture(128, 128, 24);
        var image = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        foreach (var materialName in new[] { "ParticlesUberUnlit", "ParticlesUberLit", "UIParticlesUberUnlit", "UIParticlesUberLit" })
        {
            quad.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("NovaPackageSmoke/" + materialName);
            var color = Capture(camera, image);
            if (color.r < 0.15f || color.g < 0.08f || color.r < color.b * 2)
                throw new Exception(materialName + " produced unexpected color: " + color);
        }
        var background = new Material(Resources.Load<Material>("NovaPackageSmoke/ParticlesUberUnlit"));
        var gradient = new Texture2D(64, 2, TextureFormat.RGBA32, false);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 64; x++) gradient.SetPixel(x, y, new Color(x / 63f, 0.2f, 0.05f, 1));
        gradient.Apply();
        background.SetTexture("_BaseMap", gradient);
        background.SetColor("_TintColor", Color.white);
        quad.GetComponent<Renderer>().sharedMaterial = background;
        quad.transform.position = new Vector3(0, 0, 1);
        quad.transform.localScale = new Vector3(2, 2, 1);
        var distortion = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var distortionMaterial = new Material(Resources.Load<Material>("NovaPackageSmoke/Distortion"));
        var offset = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        offset.SetPixels(new[] { new Color(1, 0.5f, 0, 1), new Color(1, 0.5f, 0, 1),
            new Color(1, 0.5f, 0, 1), new Color(1, 0.5f, 0, 1) });
        offset.Apply();
        distortionMaterial.SetTexture("_BaseMap", offset);
        distortion.GetComponent<Renderer>().sharedMaterial = distortionMaterial;
        var original = Capture(camera, image);
        var effect = camera.gameObject.AddComponent<ScreenSpaceDistortion>();
        var warped = Capture(camera, image);
        if (warped.r < original.r + 0.06f) throw new Exception("Distortion failed: " + original + " -> " + warped);
        effect.enabled = false;
        if (Mathf.Abs(Capture(camera, image).r - original.r) > 0.02f) throw new Exception("Disabling distortion failed.");
    }

    private static Color Capture(Camera camera, Texture2D image)
    {
        camera.Render();
        RenderTexture.active = camera.targetTexture;
        image.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
        image.Apply();
        return image.GetPixel(64, 64);
    }

    private static string Argument(string name, string fallback)
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
    }
}

using System;
using System.Collections;
using System.IO;
using Nova.Runtime.Core.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public sealed class NovaPackagePlayerSmoke : MonoBehaviour
{
    private IEnumerator Start()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-novaShowcase") >= 0)
        {
            SceneManager.LoadScene("NovaShowcase");
            yield break;
        }
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.white;
        yield return null;
        yield return null;
        var reportPath = Argument("-novaReport", Path.Combine(Application.persistentDataPath, "nova-player-smoke.txt"));
        if (!TryStage(Run, reportPath)) yield break;
        DontDestroyOnLoad(gameObject);
        if (!TryStage(() => SceneManager.LoadScene("NovaShowcase"), reportPath)) yield break;
        yield return null;
        yield return null;
        if (!TryStage(() => VerifyShowcase(reportPath), reportPath)) yield break;
        File.WriteAllText(reportPath, "PASS: " + Application.unityVersion +
            "\nSix original shaders found. Four Uber color renders passed. Distortion offset and disable passed.\n" +
            "Packaged showcase loaded, five particle systems simulated and its rendered image passed.\n");
        Debug.Log("NOVA PLAYER SMOKE PASSED");
        Application.Quit(0);
    }

    private static bool TryStage(Action validate, string reportPath)
    {
        try
        {
            validate();
            return true;
        }
        catch (Exception exception)
        {
            File.WriteAllText(reportPath, "FAIL: " + exception);
            Debug.LogException(exception);
            Application.Quit(1);
            return false;
        }
    }

    private static void VerifyShowcase(string reportPath)
    {
        var systems = FindObjectsOfType<ParticleSystem>();
        if (systems.Length != 5) throw new Exception("Packaged showcase particle systems are missing.");
        foreach (var system in systems) system.Simulate(2, true, true, true);
        var camera = Camera.main;
        if (camera == null || camera.GetComponent<ScreenSpaceDistortion>() == null)
            throw new Exception("Packaged showcase camera/distortion is missing.");
        var target = new RenderTexture(960, 640, 24);
        var image = new Texture2D(960, 640, TextureFormat.RGB24, false);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        image.ReadPixels(new Rect(0, 0, 960, 640), 0, 0);
        image.Apply();
        var colored = 0;
        foreach (var color in image.GetPixels())
            if (Mathf.Max(color.r, color.g, color.b) - Mathf.Min(color.r, color.g, color.b) > 0.15f) colored++;
        if (colored < 6000) throw new Exception("Packaged showcase produced too few colored pixels: " + colored);
        // PPM avoids adding the optional ImageConversion module to the minimal test project.
        var header = System.Text.Encoding.ASCII.GetBytes("P6\n960 640\n255\n");
        var pixels = image.GetPixels32();
        var ppm = new byte[header.Length + pixels.Length * 3];
        header.CopyTo(ppm, 0);
        var offset = header.Length;
        for (var y = 639; y >= 0; y--)
        for (var x = 0; x < 960; x++)
        {
            var color = pixels[y * 960 + x];
            ppm[offset++] = color.r;
            ppm[offset++] = color.g;
            ppm[offset++] = color.b;
        }
        File.WriteAllBytes(reportPath + ".ppm", ppm);
        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(target);
        Destroy(image);
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

// Copy into Assets/Editor of an isolated validation project.
using System;
using System.IO;
using System.Linq;
using Nova.Editor.Core.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NovaShowcaseExport
{
    public static void Export()
    {
        try
        {
            var folder = AssetDatabase.GenerateUniqueAssetPath("Assets/NovaBuiltInSampleExport");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), folder + "/ExportContext.unity");
            var path = folder + "/NovaShowcase.unity";
            BuiltInShowcase.Create(path);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            foreach (var system in roots.SelectMany(r => r.GetComponentsInChildren<ParticleSystem>())) system.Simulate(2, true, true, true);
            var camera = roots.SelectMany(r => r.GetComponentsInChildren<Camera>()).Single();
            var target = new RenderTexture(1440, 960, 24);
            var image = new Texture2D(1440, 960, TextureFormat.RGB24, false);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1440, 960), 0, 0);
            image.Apply();
            Directory.CreateDirectory("Logs");
            File.WriteAllBytes("Logs/NovaShowcase.png", image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(image);
            Debug.Log("NOVA_SAMPLE_EXPORT=" + Path.GetFullPath(folder));
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }
}

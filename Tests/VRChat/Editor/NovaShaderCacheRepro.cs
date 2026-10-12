// Diagnostic fixture only. Never shipped in the NOVA package.
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class NovaShaderCacheRepro
{
    const string MaterialPath = "Assets/NovaShaderCacheRepro.mat";
    [Serializable] public class Result
    {
        public bool pristine, clearOnly, reimport;
        public string error;
    }

    public static void Run()
    {
        var result = new Result();
        const string root = "Exports/NovaVRCValidation/CacheRepro";
        Directory.CreateDirectory(root);
        var shader = Shader.Find("Nova/Particles/Distortion");
        var path = AssetDatabase.GetAssetPath(shader);
        var created = false;
        try
        {
            if (File.Exists(MaterialPath)) throw new InvalidOperationException("Diagnostic material already exists.");
            AssetDatabase.CreateAsset(new Material(shader), MaterialPath);
            created = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            result.pristine = BuildAndLoad(root + "/pristine", path);
            ShaderUtil.ClearCachedData(shader);
            result.clearOnly = BuildAndLoad(root + "/clear-only", path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            result.reimport = BuildAndLoad(root + "/reimport", path);
        }
        catch (Exception e) { result.error = e.ToString(); }
        finally
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (created) AssetDatabase.DeleteAsset(MaterialPath);
            File.WriteAllText(root + "/result.json", JsonUtility.ToJson(result, true));
            EditorApplication.Exit(string.IsNullOrEmpty(result.error) && result.pristine && !result.clearOnly && result.reimport ? 0 : 1);
        }
    }

    static bool BuildAndLoad(string output, string shader)
    {
        Directory.CreateDirectory(output);
        var map = new[] { new AssetBundleBuild { assetBundleName = "nova-cache-repro",
            assetNames = new[] { shader, MaterialPath } } };
        var manifest = BuildPipeline.BuildAssetBundles(output, map, BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildTarget.StandaloneWindows64);
        if (!manifest) throw new InvalidOperationException("Diagnostic bundle build failed.");
        var bundle = AssetBundle.LoadFromFile(output + "/nova-cache-repro");
        if (!bundle) throw new InvalidOperationException("Diagnostic bundle load failed.");
        try { return bundle.LoadAllAssets<Shader>().Single(s => s.name == "Nova/Particles/Distortion").isSupported; }
        finally { bundle.Unload(true); }
    }
}

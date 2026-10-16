// Copy to Assets/Editor in an isolated packaging project. Never ship this script.
using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class ElderStarDistribution
{
    private static PackRequest request;
    private static double deadline;

    public static void Export()
    {
        try
        {
            var output = Argument("-elderstarOutput");
            Directory.CreateDirectory(output);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/Nova/package.json"));
            if (manifest.name != "me.smizuoch.elderstar") throw new Exception("Unexpected package identity");
            // Import can rewrite legacy .cginc and minimal .meta files. Preserve
            // reviewed production bytes in the exported artifact, retaining the
            // same paths/GUIDs already registered by Unity. No dependency export.
            AssetDatabase.StartAssetEditing();
            try
            {
                var source = Argument("-elderstarUpmSource");
                foreach (var section in new[] { "Runtime", "Editor" })
                foreach (var file in Directory.GetFiles(Path.Combine(source, section), "*", SearchOption.AllDirectories))
                {
                    var relative = file.Substring(source.Length).TrimStart('/', '\\');
                    File.Copy(file, Path.Combine("Assets/Nova", relative), true);
                }
                AssetDatabase.ExportPackage("Assets/Nova", Path.Combine(output, "ElderStar-" + manifest.version + ".unitypackage"), ExportPackageOptions.Recurse);
            }
            finally { AssetDatabase.StopAssetEditing(); }
            request = Client.Pack(Argument("-elderstarUpmSource"), output);
            deadline = EditorApplication.timeSinceStartup + 300;
            EditorApplication.update += Poll;
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static void Poll()
    {
        if (!request.IsCompleted && EditorApplication.timeSinceStartup < deadline) return;
        EditorApplication.update -= Poll;
        if (!request.IsCompleted || request.Status != StatusCode.Success)
        {
            Debug.LogError(request.IsCompleted ? request.Error.message : "Package export timed out");
            EditorApplication.Exit(1);
            return;
        }
        Debug.Log("ELDERSTAR_DISTRIBUTION_EXPORTED=" + request.Result.tarballPath);
        EditorApplication.Exit(0);
    }

    private static string Argument(string name)
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, name);
        if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("Missing " + name);
        return args[index + 1];
    }

    [Serializable] private sealed class Manifest { public string name; public string version; }
}

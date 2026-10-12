// Copy into Assets/Editor of an isolated validation project.
using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class NovaPackageBootstrap
{
    private static PackRequest pack;
    private static AddRequest add;
    private static double deadline;
    private const string Pending = "NovaPackageBootstrap.PendingSource";

    [InitializeOnLoadMethod]
    private static void ResumeAfterReload()
    {
        var source = SessionState.GetString(Pending, "");
        if (!string.IsNullOrEmpty(source)) EditorApplication.delayCall += () => Add(source);
    }

    private static void Add(string source)
    {
        SessionState.SetString(Pending, source);
        add = Client.Add(source);
        BeginPolling();
    }

    public static void Install()
    {
        Add("file:" + Argument("-novaPackageSource"));
    }

    public static void PackAndInstall()
    {
        pack = Client.Pack(Argument("-novaPackageSource"), Argument("-novaPackageTarballs"));
        BeginPolling();
    }

    private static void BeginPolling()
    {
        deadline = EditorApplication.timeSinceStartup + 300;
        EditorApplication.update += Poll;
    }

    private static void Poll()
    {
        if (EditorApplication.timeSinceStartup > deadline) { Finish(false, "Package operation timed out."); return; }
        if (pack != null)
        {
            if (!pack.IsCompleted) return;
            if (pack.Status != StatusCode.Success) { Finish(false, pack.Error.message); return; }
            Debug.Log("NOVA package archive: " + pack.Result.tarballPath);
            SessionState.SetString(Pending, "file:" + pack.Result.tarballPath);
            add = Client.Add("file:" + pack.Result.tarballPath);
            pack = null;
        }
        if (add == null || !add.IsCompleted) return;
        Finish(add.Status == StatusCode.Success,
            add.Status == StatusCode.Success ? "Installed " + add.Result.packageId : add.Error.message);
    }

    private static void Finish(bool success, string message)
    {
        EditorApplication.update -= Poll;
        SessionState.EraseString(Pending);
        if (success) Debug.Log(message); else Debug.LogError(message);
        EditorApplication.Exit(success ? 0 : 1);
    }

    private static string Argument(string name)
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, name);
        if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("Missing argument " + name);
        return args[index + 1];
    }
}

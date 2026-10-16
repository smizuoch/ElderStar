// Test fixture only: add test dependencies and optionally the actual UPM tarball.
using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class ElderStarDistributionBootstrap
{
    private const string Pending = "ElderStar.DistributionBootstrap.Pending";
    private static AddAndRemoveRequest request;
    private static double deadline;

    [InitializeOnLoadMethod]
    private static void Resume()
    {
        if (SessionState.GetBool(Pending, false)) EditorApplication.delayCall += Install;
    }

    public static void Install()
    {
        SessionState.SetBool(Pending, true);
        var packages = new System.Collections.Generic.List<string> { "com.unity.test-framework@1.1.33",
#if UNITY_6000_0_OR_NEWER
            "com.unity.ugui@2.0.0"
#else
            "com.unity.ugui@1.0.0"
#endif
        };
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-elderstarTarball");
        if (index >= 0) packages.Add("file:" + args[index + 1]);
        request = Client.AddAndRemove(packages.ToArray());
        deadline = EditorApplication.timeSinceStartup + 300;
        EditorApplication.update -= Poll;
        EditorApplication.update += Poll;
    }

    private static void Poll()
    {
        if (!request.IsCompleted && EditorApplication.timeSinceStartup < deadline) return;
        EditorApplication.update -= Poll;
        SessionState.EraseBool(Pending);
        if (!request.IsCompleted || request.Status != StatusCode.Success)
        {
            Debug.LogError(request.IsCompleted ? request.Error.message : "Distribution fixture timed out");
            EditorApplication.Exit(1);
            return;
        }
        Debug.Log("ELDERSTAR_DISTRIBUTION_FIXTURE_READY");
        EditorApplication.Exit(0);
    }
}

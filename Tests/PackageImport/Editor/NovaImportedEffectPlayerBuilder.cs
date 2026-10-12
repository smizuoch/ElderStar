using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NovaImportedEffectPlayerBuilder
{
    public static void Build()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/N2Studio/Prefabs/Fire001.prefab");
        if (prefab == null) throw new InvalidOperationException("Import the unchanged Fire001 package first.");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var effect = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        var camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
        camera.transform.position = new Vector3(0, 1.7f, -8);
        camera.transform.LookAt(new Vector3(0, 1.2f, 0));
        camera.orthographic = true;
        camera.orthographicSize = 2.5f;
        camera.allowHDR = false;
        camera.allowMSAA = false;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.11f, 0.19f, 1);
        var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("NovaImportedEffectPlayerSmoke")).First(t => t != null);
        var smoke = new GameObject("Fire001 acceptance").AddComponent(type);
        type.GetField("Effect").SetValue(smoke, effect);
        type.GetField("View").SetValue(smoke, camera);
        const string path = "Assets/Fire001PlayerAcceptance.unity";
        EditorSceneManager.SaveScene(scene, path);
        Directory.CreateDirectory("Build/Fire001");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { path },
            locationPathName = "Build/Fire001/Fire001Acceptance.exe", target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development });
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}

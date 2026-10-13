using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NovaAvatarGpuBenchmarkBuilder
{
    public static void Build()
    {
        var args = Environment.GetCommandLineArgs();
        var index = Array.IndexOf(args, "-novaBenchmarkPlayer");
        if (index < 0) throw new ArgumentException("Missing -novaBenchmarkPlayer");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var runner = new GameObject("Benchmark controller").AddComponent<NovaAvatarGpuBenchmark>();
        runner.baselineShader = Shader.Find("Hidden/NovaBenchmark/BaselineDistortion");
        runner.candidateShader = Shader.Find("Nova/Particles/Distortion");
        runner.backgroundShader = Shader.Find("Nova/Particles/UberUnlit");
        if (!runner.baselineShader || !runner.candidateShader) throw new Exception("Missing benchmark shaders");
        const string path = "Assets/NovaGpuBenchmark.unity";
        EditorSceneManager.SaveScene(scene, path);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        PlayerSettings.runInBackground = true;
        PlayerSettings.enableFrameTimingStats = true;
        PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        Directory.CreateDirectory(Path.GetDirectoryName(args[index + 1]));
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { path },
            target = BuildTarget.StandaloneWindows64, locationPathName = args[index + 1], options = BuildOptions.None });
        if (result.summary.result != BuildResult.Succeeded) throw new Exception("Benchmark player build failed: " + result.summary.result);
    }
}

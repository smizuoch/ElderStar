using System;
using System.IO;
using Nova.Editor.Core.Scripts;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NovaPackagePlayerBuilder
{
    public static void Build()
    {
        try
        {
            const string resources = "Assets/Resources/NovaPackageSmoke";
            Directory.CreateDirectory(resources);
            AssetDatabase.Refresh();
            foreach (var pair in new[]
            {
                new[] { "ParticlesUberUnlit", "Nova/Particles/UberUnlit" },
                new[] { "ParticlesUberLit", "Nova/Particles/UberLit" },
                new[] { "UIParticlesUberUnlit", "Nova/UIParticles/UberUnlit" },
                new[] { "UIParticlesUberLit", "Nova/UIParticles/UberLit" },
                new[] { "Distortion", "Nova/Particles/Distortion" }
            })
            {
                var path = resources + "/" + pair[0] + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(Shader.Find(pair[1]));
                    AssetDatabase.CreateAsset(material, path);
                }
                material.SetFloat("_Cull", 0);
                if (pair[0] == "Distortion")
                {
                    material.SetFloat("_DistortionIntensity", 1);
                    ParticlesDistortionMaterialPostProcessor.SetupMaterialKeywords(material);
                }
                else
                {
                    material.SetFloat("_RenderType", 0);
                    material.SetFloat("_TintAreaMode", 1);
                    material.SetColor("_TintColor", new Color(0.6f, 0.3f, 0.05f, 1));
                    if (pair[0].EndsWith("UberLit"))
                    {
                        material.SetFloat("_LitWorkflowMode", 1);
                        material.SetFloat("_Metallic", 0);
                        material.SetFloat("_EnvironmentReflections", 0);
                        material.SetFloat("_SpecularHighlights", 0);
                        ParticlesUberLitMaterialPostProcessor.SetupMaterialKeywords(material);
                    }
                    else ParticlesUberUnlitMaterialPostProcessor.SetupMaterialKeywords(material);
                    ParticlesUberUnlitMaterialPostProcessor.SetupMaterialBlendMode(material);
                }
                EditorUtility.SetDirty(material);
            }
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Runtime smoke component is in the default runtime assembly.
            var type = AppDomain.CurrentDomain.GetAssemblies().SelectType("NovaPackagePlayerSmoke");
            new GameObject("NOVA player verification").AddComponent(type);
            const string scenePath = "Assets/NovaPackageSmoke.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.runInBackground = true;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { scenePath },
                locationPathName = "Build/NovaPackageSmoke.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static Type SelectType(this System.Reflection.Assembly[] assemblies, string name)
    {
        foreach (var assembly in assemblies)
        {
            var type = assembly.GetType(name);
            if (type != null) return type;
        }
        throw new InvalidOperationException("Missing runtime player verification component.");
    }
}

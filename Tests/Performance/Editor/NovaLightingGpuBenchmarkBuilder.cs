using System;
using System.Collections.Generic;
using System.IO;
using Nova.Editor.Core.Scripts;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class NovaLightingGpuBenchmarkBuilder
{
    public static void Build()
    {
        var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-novaBenchmarkPlayer");
        if(index<0)throw new ArgumentException("Missing -novaBenchmarkPlayer");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var runner=new GameObject("NOVA lighting measurements").AddComponent<NovaLightingGpuBenchmark>();
        Directory.CreateDirectory("Assets/NovaBenchmark/Materials");
        var texture=new Texture2D(32,32,TextureFormat.RGBA32,false,true);
        for(int y=0;y<32;y++)for(int x=0;x<32;x++)texture.SetPixel(x,y,new Color(.1f+.8f*x/31,.1f+.8f*y/31,.4f,1));
        texture.Apply();AssetDatabase.CreateAsset(texture,"Assets/NovaBenchmark/Pattern.asset");
        var normal=new Texture2D(1,1,TextureFormat.RGBA32,false,true);normal.SetPixel(0,0,new Color(.68f,.7f,.9f,1));normal.Apply();
        AssetDatabase.CreateAsset(normal,"Assets/NovaBenchmark/Normal.asset");
        runner.baselineMaterials=new Material[8];runner.candidateMaterials=new Material[8];
        var kinds=new[]{"Particles/UberUnlit","UIParticles/UberUnlit","Particles/UberLit","UIParticles/UberLit"};
        var scenarios=new List<NovaLightingGpuBenchmark.Scenario>();
        for(int kind=0;kind<4;kind++)for(int combined=0;combined<2;combined++)
        {
            int materialIndex=kind*2+combined;
            for(int variant=0;variant<2;variant++)
            {
                var shader=Shader.Find((variant==0 ? "Hidden/NovaLightingBaseline/" : "Nova/")+kinds[kind]);
                if(shader==null)throw new Exception("Missing benchmark shader "+kinds[kind]);
                var material=new Material(shader);material.SetTexture("_BaseMap",texture);material.SetFloat("_Cull",0);
                material.SetFloat("_RenderType",2);material.SetFloat("_ZWriteOverride",0);material.SetFloat("_LitWorkflowMode",1);
                if(kind>=2){material.SetFloat("_Metallic",0);material.SetTexture("_NormalMap",combined==1 ? normal : null);material.SetFloat("_EnvironmentReflections",0);material.SetFloat("_SpecularHighlights",combined);}
                if(combined==1)
                {
                    material.SetTexture("_ParallaxMap",texture);material.SetFloat("_ParallaxMapMode",0);material.SetFloat("_ParallaxMapTarget",1);material.SetFloat("_ParallaxStrength",.03f);
                    material.SetFloat("_TintAreaMode",2);material.SetFloat("_TintColorMode",0);material.SetColor("_TintColor",new Color(.5f,.7f,.8f,1));
                }
                if(kind>=2){ParticlesUberLitMaterialPostProcessor.SetupMaterialKeywords(material);ParticlesUberLitMaterialPostProcessor.SetupMaterialBlendMode(material);}
                else{ParticlesUberUnlitMaterialPostProcessor.SetupMaterialKeywords(material);ParticlesUberUnlitMaterialPostProcessor.SetupMaterialBlendMode(material);}
                if(kind>=2)material.EnableKeyword("LIGHTPROBE_SH");
                material.SetFloat("_BlendSrc",1);material.SetFloat("_BlendDst",1);material.SetFloat("_ZWrite",0);material.SetFloat("_ZTest",4);material.renderQueue=3000;
                if(combined==1)
                {
                    material.EnableKeyword("_PARALLAX_MAP_MODE_2D");material.EnableKeyword("_PARALLAX_MAP_TARGET_BASE");
                    material.EnableKeyword("_TINT_AREA_RIM");material.EnableKeyword("_TINT_COLOR_ENABLED");
                    if(kind>=2)material.EnableKeyword("_NORMAL_MAP_ENABLED");
                }
                AssetDatabase.CreateAsset(material,$"Assets/NovaBenchmark/Materials/{materialIndex}-{variant}.mat");
                (variant==0 ? runner.baselineMaterials : runner.candidateMaterials)[materialIndex]=material;
            }
            foreach(bool mirror in new[]{false,true})scenarios.Add(new NovaLightingGpuBenchmark.Scenario{
                name=kinds[kind]+(combined==0 ? "-default" : "-combined")+(mirror ? "-overlap-mirror" : "-single"),material=materialIndex,layers=mirror ? 8 : 1,mirror=mirror,fog=combined==1});
        }
        foreach(bool mirror in new[]{false,true})scenarios.Add(new NovaLightingGpuBenchmark.Scenario{
            name="UI-Lit-clipped-eight-lights"+(mirror ? "-overlap-mirror" : "-single"),material=7,layers=mirror ? 8 : 1,mirror=mirror,clip=true,lights=8,fog=true});
        // Optional workloads compare one candidate shader with its helper disabled/enabled.
        foreach(int count in new[]{0,4,8})scenarios.Add(new NovaLightingGpuBenchmark.Scenario{name="canvas-"+count,helper="canvas",material=6,lights=count});
        scenarios.Add(new NovaLightingGpuBenchmark.Scenario{name="depth-normals-eight",helper="depth",material=5,layers=8,fog=true});
        scenarios.Add(new NovaLightingGpuBenchmark.Scenario{name="lppv",helper="probe",material=4});
        // For optional scenarios select candidate materials for both sides; baseline source
        // remains used only for the eighteen ordinary-path comparisons.
        for(int i=0;i<scenarios.Count;i++)if(!string.IsNullOrEmpty(scenarios[i].helper))
        {
            int old=scenarios[i].material,newIndex=runner.candidateMaterials.Length;
            Array.Resize(ref runner.candidateMaterials,newIndex+1);Array.Resize(ref runner.baselineMaterials,newIndex+1);
            runner.candidateMaterials[newIndex]=runner.baselineMaterials[newIndex]=runner.candidateMaterials[old];scenarios[i].material=newIndex;
        }
        runner.scenarios=scenarios.ToArray();
        const string path="Assets/NovaLightingBenchmark.unity";EditorSceneManager.SaveScene(scene,path);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.Mono2x);
        PlayerSettings.runInBackground=true;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
        Directory.CreateDirectory(Path.GetDirectoryName(args[index+1]));
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{path},target=BuildTarget.StandaloneWindows64,locationPathName=args[index+1],options=BuildOptions.Development});
        if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Measurement build failed: "+result.summary.result);
    }
}

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.Editor
{
    public sealed class NovaShaderDiagnosticsTests
    {
        [TestCase("Nova/DiagnosticStripTest")]
        [TestCase("Hidden/Nova/DiagnosticStripTest")]
        public void AvatarAssetBundlesExcludeDiagnosticVariants(string shaderName)
        {
            string id=Guid.NewGuid().ToString("N");
            string asset="Assets/NovaDiagnosticStripTest-"+id+".shader";
            string output="Temp/NovaDiagnosticStripTest-"+id;
            try
            {
                File.WriteAllText(asset,"Shader \""+shaderName+"\" { SubShader { Pass { CGPROGRAM\n#pragma vertex vert\n#pragma fragment frag\n#pragma multi_compile _ NOVA_DEBUG_DISPLAY\nfloat4 vert(float4 v:POSITION):SV_POSITION{return v;}\nfloat4 frag():SV_Target{return 1;}\nENDCG\n} } }");
                AssetDatabase.ImportAsset(asset,ImportAssetOptions.ForceSynchronousImport);
                Directory.CreateDirectory(output);NovaDiagnosticVariantObserver.ShaderName=shaderName;
                NovaDiagnosticVariantObserver.SawNormal=false;NovaDiagnosticVariantObserver.SawDiagnostic=false;
                var result=BuildPipeline.BuildAssetBundles(output,new[]{new AssetBundleBuild{assetBundleName="diagnostic-check",assetNames=new[]{asset}}},BuildAssetBundleOptions.ForceRebuildAssetBundle,BuildTarget.StandaloneWindows64);
                Assert.That(result,Is.Not.Null);
                Assert.That(NovaDiagnosticVariantObserver.SawNormal,Is.True,"The build must retain the ordinary variant.");
                Assert.That(NovaDiagnosticVariantObserver.SawDiagnostic,Is.False,"Editor diagnostics must not be shipped in avatar asset bundles, including Hidden/Nova optimized shaders.");
            }
            finally
            {
                NovaDiagnosticVariantObserver.ShaderName=null;AssetDatabase.DeleteAsset(asset);
                if(Directory.Exists(output))Directory.Delete(output,true);
            }
        }
        private static IEnumerable Views()
        {
            foreach (var shader in new[] { "Nova/Particles/UberUnlit", "Nova/Particles/UberLit", "Nova/UIParticles/UberUnlit", "Nova/UIParticles/UberLit" })
            foreach (var view in new[] { "Albedo", "Normals", "Tangents", "Depth", "Fog", "DirectLight", "IndirectLight", "AdditionalLightCount" })
                yield return new TestCaseData(shader, view);
        }

        [TestCaseSource(nameof(Views))]
        public void EditorViewsRenderAndRestoreGlobalState(string shaderName, string view)
        {
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Nova.Editor.Core.Scripts.NovaShaderDiagnostics")).FirstOrDefault(t=>t!=null);
            Assert.That(type,Is.Not.Null);
            var method=type.GetMethod("Render");Assert.That(method,Is.Not.Null);
            var cameraObject=new GameObject("Diagnostic camera",typeof(Camera));
            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);
            var material=new Material(Shader.Find(shaderName));
            var target=new RenderTexture(32,32,24,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
            var read=new Texture2D(32,32,TextureFormat.RGBAFloat,false,true);var previous=RenderTexture.active;
            bool previousKeyword=Shader.IsKeywordEnabled("NOVA_DEBUG_DISPLAY");
            int previousMode=Shader.GetGlobalInt("_NovaDebugMode");
            bool previousFog=RenderSettings.fog; RenderSettings.fog=false;
            try
            {
                Shader.DisableKeyword("NOVA_DEBUG_DISPLAY");
                material.SetFloat("_Cull",0);material.EnableKeyword("_BASE_MAP_MODE_2D");
                material.DisableKeyword("_ENVIRONMENT_REFLECTIONS_ENABLED");
                quad.GetComponent<Renderer>().sharedMaterial=material;
                quad.layer=31;
                var originalKeywords=material.shaderKeywords.OrderBy(k=>k).ToArray();
                var camera=cameraObject.GetComponent<Camera>();camera.enabled=false;
                camera.cullingMask=1<<31;camera.farClipPlane=10;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
                camera.transform.position=new Vector3(0,0,-2);camera.orthographic=true;camera.orthographicSize=1;camera.targetTexture=target;
                var mode=Enum.Parse(method.GetParameters()[1].ParameterType,view);
                method.Invoke(null,new object[]{camera,mode});
                RenderTexture.active=target;read.ReadPixels(new Rect(0,0,32,32),0,0);read.Apply();var sample=read.GetPixel(16,16);
                Assert.That(float.IsNaN(sample.r) || float.IsInfinity(sample.r), Is.False, view);
                if (view == "Normals") AssertColor(sample, new Color(.5f,.5f,0));
                if (view == "Tangents") AssertColor(sample, new Color(1,.5f,.5f));
                if (view == "Depth") AssertColor(sample, new Color(.2f,.2f,.2f));
                if (view == "Albedo" || view == "Fog") AssertColor(sample, Color.white);
                if (view == "AdditionalLightCount" || (shaderName.EndsWith("Unlit") && (view == "DirectLight" || view == "IndirectLight"))) AssertColor(sample, Color.black);
                Assert.That(Shader.GetGlobalInt("_NovaDebugMode"),Is.EqualTo(previousMode));
                Assert.That(Shader.IsKeywordEnabled("NOVA_DEBUG_DISPLAY"),Is.False,"A diagnostic render must restore the normal path.");
                CollectionAssert.AreEqual(originalKeywords,material.shaderKeywords.OrderBy(k=>k));
            }
            finally
            {
                if(previousKeyword)Shader.EnableKeyword("NOVA_DEBUG_DISPLAY");else Shader.DisableKeyword("NOVA_DEBUG_DISPLAY");
                RenderSettings.fog=previousFog;Shader.SetGlobalInt("_NovaDebugMode",previousMode);
                RenderTexture.active=previous;
                foreach(var owned in new UnityEngine.Object[]{quad,cameraObject,material,target,read}) UnityEngine.Object.DestroyImmediate(owned);
            }
        }
        private static void AssertColor(Color actual, Color expected)
        {
            Assert.That(actual.r,Is.EqualTo(expected.r).Within(.01f));
            Assert.That(actual.g,Is.EqualTo(expected.g).Within(.01f));
            Assert.That(actual.b,Is.EqualTo(expected.b).Within(.01f));
        }
    }

    public sealed class NovaDiagnosticVariantObserver : IPreprocessShaders
    {
        internal static string ShaderName;
        internal static bool SawNormal,SawDiagnostic;
        public int callbackOrder=>10000;
        public void OnProcessShader(Shader shader,ShaderSnippetData snippet,IList<ShaderCompilerData> data)
        {
            if(shader.name!=ShaderName)return;
            var keyword=new ShaderKeyword("NOVA_DEBUG_DISPLAY");
            foreach(var variant in data)
                if(variant.shaderKeywordSet.IsEnabled(keyword))SawDiagnostic=true;else SawNormal=true;
        }
    }
}

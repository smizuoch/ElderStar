using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaAmbientLightingTests
    {
        [TestCase("Nova/Particles/UberLit",0f)]
        [TestCase("Nova/Particles/UberLit",.35f)]
        [TestCase("Nova/Particles/UberLit",1f)]
        [TestCase("Nova/UIParticles/UberLit",0f)]
        [TestCase("Nova/UIParticles/UberLit",.35f)]
        [TestCase("Nova/UIParticles/UberLit",1f)]
        public void DisabledReflectionsRetainOriginalGlossyAmbient(string shaderName, float intensity)
        {
            RenderAmbient(shaderName,intensity,false);
        }

        [TestCase("Nova/Particles/UberLit")]
        [TestCase("Nova/UIParticles/UberLit")]
        public void LowIntensityProbePreservesSrgbLinearSegment(string shaderName)
        {
            RenderAmbient(shaderName,0,true,true);
        }

        [TestCase("Nova/Particles/UberLit")]
        [TestCase("Nova/UIParticles/UberLit")]
        public void GlossyAmbientMatchesOriginalHdrColorConversion(string shaderName)
        {
            RenderAmbient(shaderName,2,false,false,true);
        }

        [TestCase("Nova/Particles/UberLit")]
        [TestCase("Nova/UIParticles/UberLit")]
        public void CustomProbeCanBeDarkerThanSceneAmbient(string shaderName)
        {
            RenderAmbient(shaderName,0,true);
        }

        private static void RenderAmbient(string shaderName,float intensity,bool customProbe,bool darkProbe=false,bool hdr=false)
        {
            var material = new Material(Shader.Find(shaderName));
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var cameraObject = new GameObject("Ambient regression",typeof(Camera));
            var target = new RenderTexture(32,32,24,RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
            var read = new Texture2D(32,32,TextureFormat.RGBAFloat,false,true);
            var oldMode=RenderSettings.ambientMode; var oldAmbient=RenderSettings.ambientLight;
            var oldReflection=RenderSettings.reflectionIntensity; var oldTarget=RenderTexture.active;
            var oldFog=RenderSettings.fog;
            try
            {
                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=customProbe ? Color.red : hdr ? new Color(1.5f,1.2f,.45f) : new Color(.15f,.22f,.45f);
                RenderSettings.reflectionIntensity=intensity; RenderSettings.fog=false;
                quad.layer=29;
                var camera=cameraObject.GetComponent<Camera>(); camera.enabled=false;
                camera.transform.position=new Vector3(0,0,-2);camera.orthographic=true;camera.orthographicSize=.6f;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
                camera.cullingMask=1<<29;camera.targetTexture=target;camera.allowHDR=true;
                material.SetFloat("_RenderType",0);material.SetFloat("_Cull",0);
                material.SetFloat("_LitWorkflowMode",customProbe ? 0 : 1);material.SetColor("_SpecularColor",Color.black);
                material.SetFloat("_Metallic",1);
                material.SetFloat("_Smoothness",customProbe ? 0 : .6f);material.SetFloat("_EnvironmentReflections",0);
                material.SetFloat("_SpecularHighlights",0);material.SetFloat("_LitReceiveShadows",0);
                var processor=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor")).First(t=>t!=null);
                foreach(var method in new[]{"SetupMaterialKeywords","SetupMaterialBlendMode"})processor.GetMethod(method).Invoke(null,new object[]{material});
                var renderer=quad.GetComponent<Renderer>();renderer.sharedMaterial=material;
                var sh=new SphericalHarmonicsL2();var expected=darkProbe ? new Color(.0001f,.002f,.01f) : new Color(.02f,.05f,.01f);
                if(customProbe)
                {
                    renderer.lightProbeUsage=LightProbeUsage.CustomProvided;
                    sh.AddAmbientLight(expected);
                    var block=new MaterialPropertyBlock();block.CopySHCoefficientArraysFrom(new[]{sh});renderer.SetPropertyBlock(block);
                }
                camera.Render();RenderTexture.active=target;read.ReadPixels(new Rect(0,0,32,32),0,0);read.Apply();
                if(!customProbe)
                {
                    sh=RenderSettings.ambientProbe;
                    expected=new Color(sh[0,0],sh[1,0],sh[2,0])*intensity;
                }
                if(QualitySettings.activeColorSpace==ColorSpace.Gamma)expected=expected.gamma;
                if(!customProbe)expected*=1f/(1f+Mathf.Pow(.4f,4));
                var actual=read.GetPixel(16,16);
                for(int channel=0;channel<3;channel++)Assert.That(actual[channel],Is.EqualTo(expected[channel]).Within(darkProbe ? .0001f : .003f),customProbe ? "Scene ambient must not raise the supplied probe: "+actual : "Original glossy ambient must remain with reflection maps disabled: "+actual);
            }
            finally
            {
                RenderTexture.active=oldTarget;RenderSettings.ambientMode=oldMode;RenderSettings.ambientLight=oldAmbient;
                RenderSettings.reflectionIntensity=oldReflection;RenderSettings.fog=oldFog;
                foreach(var item in new UnityEngine.Object[]{quad,cameraObject,material,target,read})UnityEngine.Object.DestroyImmediate(item);
            }
        }
    }
}

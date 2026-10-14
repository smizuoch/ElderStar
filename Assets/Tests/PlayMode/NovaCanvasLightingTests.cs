using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaCanvasLightingTests
    {
        [UnityTest]
        public IEnumerator SeparateCanvasesKeepIndependentLightsWhenGraphicsMove()
        {
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Nova.Runtime.UI.NovaCanvasLighting")).First(t=>t!=null);
            var imageType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEngine.UI.RawImage")).First(t=>t!=null);
            var roots=new GameObject[2];var lights=new GameObject[2];var graphics=new Component[2];var helpers=new Behaviour[2];
            var material=new Material(Shader.Find("Nova/UIParticles/UberLit"));
            try
            {
                for(int i=0;i<2;i++)
                {
                    roots[i]=new GameObject("Independent Canvas "+i,typeof(RectTransform),typeof(Canvas));
                    roots[i].GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
                    var child=new GameObject("Shared material Graphic",typeof(RectTransform));child.transform.SetParent(roots[i].transform,false);
                    graphics[i]=child.AddComponent(imageType);imageType.GetProperty("material").SetValue(graphics[i],material);
                    helpers[i]=(Behaviour)roots[i].AddComponent(type);
                    lights[i]=new GameObject("Local light "+i,typeof(Light));var light=lights[i].GetComponent<Light>();
                    light.type=LightType.Directional;light.color=i==0 ? Color.red : Color.green;
                    type.GetMethod("RegisterLight").Invoke(helpers[i],new object[]{light});
                }
                yield return null;Canvas.ForceUpdateCanvases();
                var a=(Material)imageType.GetProperty("materialForRendering").GetValue(graphics[0]);
                var b=(Material)imageType.GetProperty("materialForRendering").GetValue(graphics[1]);
                Assert.That(a,Is.Not.SameAs(b));Assert.That(a.GetVectorArray("_NovaCanvasLightColors")[0].x,Is.GreaterThan(.9f));
                Assert.That(b.GetVectorArray("_NovaCanvasLightColors")[0].y,Is.GreaterThan(.9f));
                graphics[0].transform.SetParent(roots[1].transform,false);
                Canvas.ForceUpdateCanvases();yield return null;Canvas.ForceUpdateCanvases();
                a=(Material)imageType.GetProperty("materialForRendering").GetValue(graphics[0]);
                Assert.That(a,Is.Not.SameAs(material));Assert.That(a.GetVectorArray("_NovaCanvasLightColors")[0].y,Is.GreaterThan(.9f));
                helpers[0].enabled=false;yield return null;Canvas.ForceUpdateCanvases();
                Assert.That(a==null,Is.False,"Disabling the old Canvas must not destroy the new owner's drawing material.");
                lights[1].GetComponent<Light>().cullingMask=0;Canvas.ForceUpdateCanvases();
                Assert.That(a.GetInt("_NovaCanvasLightCount"),Is.Zero,"Layer filtering updates without collecting the scene again.");
                Assert.That(material.IsKeywordEnabled("NOVA_CANVAS_LIGHTING"),Is.False);
            }
            finally
            {
                foreach(var root in roots)if(root!=null)UnityEngine.Object.DestroyImmediate(root);
                foreach(var light in lights)if(light!=null)UnityEngine.Object.DestroyImmediate(light);
                UnityEngine.Object.DestroyImmediate(material);
            }
        }

        [UnityTest]
        public IEnumerator LightLimitsMaterialRegenerationAndCleanupAcrossCanvasModes(
            [Values(0,1,4,8,10)] int lightCount,
            [Values(4,8)] int limit,
            [Values(RenderMode.WorldSpace,RenderMode.ScreenSpaceCamera,RenderMode.ScreenSpaceOverlay)] RenderMode mode)
        {
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Nova.Runtime.UI.NovaCanvasLighting")).First(t=>t!=null);
            var imageType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEngine.UI.RawImage")).First(t=>t!=null);
            var root=new GameObject("Canvas ownership test",typeof(RectTransform),typeof(Canvas));
            var material=new Material(Shader.Find("Nova/UIParticles/UberLit"));
            var replacement=new Material(material);
            var objects=new System.Collections.Generic.List<GameObject>();
            try
            {
                root.GetComponent<Canvas>().renderMode=mode;
                if(mode==RenderMode.ScreenSpaceCamera)
                {
                    var cameraObject=new GameObject("Canvas camera",typeof(Camera));objects.Add(cameraObject);
                    var camera=cameraObject.GetComponent<Camera>();camera.enabled=false;
                    root.GetComponent<Canvas>().worldCamera=camera;
                }
                var child=new GameObject("Graphic",typeof(RectTransform));child.transform.SetParent(root.transform,false);
                var graphic=child.AddComponent(imageType);imageType.GetProperty("material").SetValue(graphic,material);
                var helper=(Behaviour)root.AddComponent(type);
                Assert.That(type.GetProperty("MaximumLights").GetValue(helper),Is.EqualTo(4));
                type.GetProperty("MaximumLights").SetValue(helper,limit);
                for(int index=0;index<lightCount;index++)
                {
                    var item=new GameObject("Light "+index,typeof(Light));objects.Add(item);
                    var light=item.GetComponent<Light>();light.type=LightType.Directional;light.intensity=index+1;
                    type.GetMethod("RegisterLight").Invoke(helper,new object[]{light});
                }
                yield return null;Canvas.ForceUpdateCanvases();
                var drawing=(Material)imageType.GetProperty("materialForRendering").GetValue(graphic);
                Assert.That(drawing.GetInt("_NovaCanvasLightCount"),Is.EqualTo(Math.Min(lightCount,limit)));
                Assert.That(type.GetProperty("OmittedLightCount").GetValue(helper),Is.EqualTo(Math.Max(0,lightCount-limit)));
                if(lightCount>0) Assert.That(drawing.GetVectorArray("_NovaCanvasLightColors")[0].x,Is.EqualTo(lightCount).Within(.01f),"Strongest contributor must be selected first.");
                replacement.SetColor("_TintColor",Color.cyan);
                imageType.GetProperty("material").SetValue(graphic,replacement);
                Canvas.ForceUpdateCanvases();
                var regenerated=(Material)imageType.GetProperty("materialForRendering").GetValue(graphic);
                Assert.That(regenerated.GetColor("_TintColor"),Is.EqualTo(Color.cyan));
                Assert.That(material.IsKeywordEnabled("NOVA_CANVAS_LIGHTING"),Is.False);
                Assert.That(replacement.IsKeywordEnabled("NOVA_CANVAS_LIGHTING"),Is.False);
                helper.enabled=false;yield return null;Canvas.ForceUpdateCanvases();
                Assert.That(imageType.GetProperty("materialForRendering").GetValue(graphic),Is.SameAs(replacement));
                Assert.That(drawing==null,Is.True,"Owned drawing material must be released on disable.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                foreach(var item in objects) UnityEngine.Object.DestroyImmediate(item);
                UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(replacement);
            }
        }

        [UnityTest]
        public IEnumerator RegisteredLightIlluminatesCanvasWithoutChangingSourceMaterial(
            [Values(false, true)] bool brightSpotFacingAway,
            [Values(LightType.Point, LightType.Directional, LightType.Spot)] LightType lightType,
            [Values(false, true)] bool screenSpaceCamera)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Runtime.UI.NovaCanvasLighting")).FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, "Optional Canvas lighting integration must be available when uGUI is installed.");
            var imageType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("UnityEngine.UI.RawImage")).First(t => t != null);
            var canvasObject = new GameObject("NOVA lit canvas", typeof(RectTransform), typeof(Canvas));
            var cameraObject = new GameObject("NOVA UI lighting camera", typeof(Camera));
            var lightObject = new GameObject("NOVA UI point", typeof(Light));
            var awayObject = new GameObject("NOVA excluded spot", typeof(Light));
            var material = new Material(Shader.Find("Nova/UIParticles/UberLit"));
            var target = new RenderTexture(64,64,24); var cpu = new Texture2D(64,64,TextureFormat.RGBA32,false);
            var previous = RenderTexture.active; var previousAmbient = RenderSettings.ambientLight; var previousMode = RenderSettings.ambientMode;
            try
            {
                RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = Color.black;
                var camera = cameraObject.GetComponent<Camera>(); camera.transform.position = new Vector3(0,0,-3);
                camera.orthographic = true; camera.orthographicSize = 1; camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black; camera.targetTexture = target;
                var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = screenSpaceCamera ? RenderMode.ScreenSpaceCamera : RenderMode.WorldSpace; canvas.worldCamera = camera;
                canvas.planeDistance = 3;
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
                canvasObject.GetComponent<RectTransform>().sizeDelta = Vector2.one * 2;
                var graphic = new GameObject("NOVA graphic", typeof(RectTransform)); graphic.transform.SetParent(canvasObject.transform,false);
                graphic.GetComponent<RectTransform>().sizeDelta = Vector2.one * (screenSpaceCamera ? 32 : 1);
                material.SetFloat("_Cull",0); material.SetFloat("_SpecularHighlights",0); material.SetFloat("_EnvironmentReflections",0);
                material.SetFloat("_Metallic",0); material.SetFloat("_LitWorkflowMode",1); material.SetFloat("_RenderType",1);
                var setup = AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor")).First(t=>t!=null);
                setup.GetMethod("SetupMaterialKeywords").Invoke(null,new object[] { material });
                setup.GetMethod("SetupMaterialBlendMode").Invoke(null,new object[] { material });
                var ui = graphic.AddComponent(imageType); imageType.GetProperty("material").SetValue(ui,material);
                var originalKeywords = material.shaderKeywords.OrderBy(k => k).ToArray();
                var bridge = canvasObject.AddComponent(type);
                var light = lightObject.GetComponent<Light>(); light.type = lightType; light.range = 4; light.intensity = 1; light.color = Color.red;
                light.transform.position = new Vector3(0,0,-1);
                type.GetMethod("RegisterLight").Invoke(bridge,new object[] { light });
                var away = awayObject.GetComponent<Light>(); away.enabled = brightSpotFacingAway;
                away.type = LightType.Spot; away.intensity = 100; away.range = 4; away.spotAngle = 20;
                away.transform.position = new Vector3(0,0,-1); away.transform.rotation = Quaternion.Euler(0,180,0);
                type.GetProperty("MaximumLights").SetValue(bridge,1);
                if (brightSpotFacingAway) type.GetMethod("RegisterLight").Invoke(bridge,new object[] { away });
                yield return null; Canvas.ForceUpdateCanvases(); camera.Render();
                var drawing = (Material)imageType.GetProperty("materialForRendering").GetValue(ui);
                Assert.That(drawing, Is.Not.SameAs(material), "The test must exercise the modifier's owned material.");
                Assert.That(drawing.IsKeywordEnabled("NOVA_CANVAS_LIGHTING"), Is.True);
                Assert.That(drawing.GetInt("_NovaCanvasLightCount"), Is.EqualTo(1));
                Assert.That(drawing.GetVectorArray("_NovaCanvasLightColors")[0].y, Is.LessThan(.01f),
                    "A brighter spotlight pointing away must not displace the contributing red light.");
                RenderTexture.active = target; cpu.ReadPixels(new Rect(0,0,64,64),0,0); cpu.Apply(); var lit = cpu.GetPixel(32,32);
                Assert.That(lit.r, Is.GreaterThan(.2f), lit.ToString());
                Assert.That(lit.r, Is.GreaterThan(lit.g + .15f), "The registered point light must contribute its color.");
                type.GetMethod("UnregisterLight").Invoke(bridge,new object[] { light });
                light.enabled = false;
                yield return null; Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = target; cpu.ReadPixels(new Rect(0,0,64,64),0,0); cpu.Apply();
                Assert.That(cpu.GetPixel(32,32).r, Is.LessThan(lit.r * .5f), "Unregistered light must stop contributing.");
                CollectionAssert.AreEqual(originalKeywords, material.shaderKeywords.OrderBy(k=>k));
                Assert.That(imageType.GetProperty("material").GetValue(ui), Is.SameAs(material));
            }
            finally
            {
                RenderTexture.active = previous; RenderSettings.ambientLight = previousAmbient; RenderSettings.ambientMode = previousMode;
                UnityEngine.Object.DestroyImmediate(canvasObject); UnityEngine.Object.DestroyImmediate(cameraObject); UnityEngine.Object.DestroyImmediate(lightObject);
                UnityEngine.Object.DestroyImmediate(awayObject);
                UnityEngine.Object.DestroyImmediate(material); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(cpu);
            }
        }
    }
}

using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Nova.Tests.PlayMode
{
    [TestFixture("Nova/UIParticles/UberUnlit")]
    [TestFixture("Nova/UIParticles/UberLit")]
    public sealed class NovaBuiltInCanvasMaskTests
    {
        private readonly string shaderName;
        public NovaBuiltInCanvasMaskTests(string shaderName) { this.shaderName = shaderName; }

        [UnityTest]
        public IEnumerator CanvasMaskClipsActualGraphic() { return RenderMask(true); }

        [UnityTest]
        public IEnumerator CanvasRectMask2DClipsActualGraphic() { return RenderMask(false); }

        private IEnumerator RenderMask(bool stencil)
        {
            var imageType = FindUiType("RawImage");
            var maskType = FindUiType(stencil ? "Mask" : "RectMask2D");
            var material = new Material(Shader.Find(shaderName));
            NovaLitTestTextures.UseDiffuseDefaults(material);
            var texture = new Texture2D(2, 2);
            texture.SetPixels(Enumerable.Repeat(Color.green, 4).ToArray());
            texture.Apply();
            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_Cull", 0);
            material.SetInt("_BlendSrc", (int)BlendMode.SrcAlpha);
            material.SetInt("_BlendDst", (int)BlendMode.OneMinusSrcAlpha);
            // Match an authored transparent material after the original Inspector's setup.
            // Raw new Material(shader) uses queue 2000 in NOVA; its defaults have separate EditMode coverage.
            material.renderQueue = (int)RenderQueue.Transparent;
            material.EnableKeyword("_BASE_MAP_MODE_2D");
            var previousAmbient = RenderSettings.ambientLight;
            var previousAmbientMode = RenderSettings.ambientMode;
            var previousActive = RenderTexture.active;
            var target = new RenderTexture(96, 96, 24);
            var pixels = new Texture2D(96, 96, TextureFormat.RGBA32, false);
            var cameraObject = new GameObject("Canvas mask test camera", typeof(Camera));
            var canvasObject = new GameObject("Canvas mask test", typeof(RectTransform), typeof(Canvas));
            try
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = Color.white;
                var camera = cameraObject.GetComponent<Camera>();
                camera.transform.position = new Vector3(0, 0, -3);
                camera.orthographic = true;
                camera.orthographicSize = 1.5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.targetTexture = target;
                var canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = camera;
                canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
                canvasObject.GetComponent<RectTransform>().sizeDelta = new Vector2(3, 3);
                var maskObject = new GameObject("Mask", typeof(RectTransform));
                maskObject.transform.SetParent(canvasObject.transform, false);
                maskObject.GetComponent<RectTransform>().sizeDelta = Vector2.one;
                if (stencil) maskObject.AddComponent(imageType);
                var child = new GameObject("NOVA graphic", typeof(RectTransform));
                child.transform.SetParent(maskObject.transform, false);
                child.GetComponent<RectTransform>().sizeDelta = Vector2.one * 2;
                var graphic = child.AddComponent(imageType);
                imageType.GetProperty("material").SetValue(graphic, material);
                yield return null;
                Canvas.ForceUpdateCanvases();
                Read(camera, target, pixels);
                Assert.That(pixels.GetPixel(72, 48).g, Is.GreaterThan(0.4f), "Unmasked control must cover the outside sample.");

                var mask = maskObject.AddComponent(maskType);
                if (stencil) maskType.GetProperty("showMaskGraphic").SetValue(mask, false);
                yield return null;
                Canvas.ForceUpdateCanvases();
                Read(camera, target, pixels);
                Assert.That(pixels.GetPixel(48, 48).g, Is.GreaterThan(0.4f), "Mask must preserve the inside pixel.");
                Assert.That(pixels.GetPixel(72, 48).g, Is.LessThan(0.08f), "Mask must reject the outside pixel.");
            }
            finally
            {
                RenderSettings.ambientLight = previousAmbient;
                RenderSettings.ambientMode = previousAmbientMode;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(canvasObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(material);
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(pixels);
            }
        }

        private static Type FindUiType(string name)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("UnityEngine.UI." + name)).FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, "The test project must install com.unity.ugui for real Canvas mask coverage.");
            return type;
        }

        private static void Read(Camera camera, RenderTexture target, Texture2D pixels)
        {
            camera.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 96, 96), 0, 0);
            pixels.Apply();
        }
    }
}

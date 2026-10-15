using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaNestedDistortionCameraTests
    {
        private const string Active = "_NovaCameraDistortionActive";

        [TestCase(CameraType.Game, false)]
        [TestCase(CameraType.Game, true)]
        [TestCase(CameraType.Reflection, false)]
        [TestCase(CameraType.Reflection, true)]
        [TestCase(CameraType.Preview, false)]
        [TestCase(CameraType.Preview, true)]
        public void NestedCameraMatchesItsStandaloneFrame(CameraType childType, bool childCompositor)
        {
            using (var scene = new Scene())
            {
                scene.Child.cameraType = childType;
                if (childCompositor) scene.AddEffect(scene.Child);
                var standalone = scene.Render(scene.Child);
                scene.Surface.SetActive(false);
                var empty = scene.Render(scene.Child);
                scene.Surface.SetActive(true);
                Assert.That(DifferentPixels(standalone, empty), Is.GreaterThan(200), "Visible distortion witness");
                scene.AddEffect(scene.Parent);
                var parentStandalone = scene.Render(scene.Parent);
                Color32[] nested = null;
                float parentBefore = -1, parentAfter = -1;
                Camera.CameraCallback renderChild = camera =>
                {
                    if (camera != scene.Parent) return;
                    parentBefore = Shader.GetGlobalFloat(Active);
                    nested = scene.Render(scene.Child);
                    parentAfter = Shader.GetGlobalFloat(Active);
                };
                Camera.onPreRender += renderChild;
                try
                {
                    var parentNested = scene.Render(scene.Parent);
                    Assert.That(nested, Is.Not.Null);
                    Assert.That(DifferentPixels(standalone, nested), Is.Zero, "Nested camera must retain its own shader/compositor path.");
                    Assert.That(DifferentPixels(parentStandalone, parentNested), Is.Zero, "Returning from the child must restore the parent's path.");
                    Assert.That(parentBefore, Is.EqualTo(1));
                    Assert.That(parentAfter, Is.EqualTo(1));
                    Assert.That(Shader.GetGlobalFloat(Active), Is.Zero);
                }
                finally { Camera.onPreRender -= renderChild; }
            }
        }

        [Test]
        public void DisablingLastCompositorInsideChildRestoresExternalState()
        {
            using (var scene = new Scene())
            {
                var effect = scene.AddEffect(scene.Parent);
                Shader.SetGlobalFloat(Active, .375f);
                float duringChild = -1, afterChild = -1;
                Camera.CameraCallback renderChild = camera =>
                {
                    if (camera == scene.Parent)
                    {
                        scene.Child.Render();
                        afterChild = Shader.GetGlobalFloat(Active);
                    }
                    if (camera == scene.Child)
                    {
                        effect.enabled = false;
                        duringChild = Shader.GetGlobalFloat(Active);
                    }
                };
                Camera.onPreRender += renderChild;
                try
                {
                    scene.Parent.Render();
                    Assert.That(duringChild, Is.Zero);
                    Assert.That(afterChild, Is.Zero, "A disabled parent must resume shader-only rendering.");
                    Assert.That(Shader.GetGlobalFloat(Active), Is.EqualTo(.375f));
                    // After the final owner/render ends, callbacks must be detached.
                    float withoutOwner = -1;
                    Camera.CameraCallback observe = camera => { if (camera == scene.Child) withoutOwner = Shader.GetGlobalFloat(Active); };
                    Camera.onPreRender += observe;
                    try { scene.Child.Render(); }
                    finally { Camera.onPreRender -= observe; }
                    Assert.That(withoutOwner, Is.EqualTo(.375f));
                }
                finally { Camera.onPreRender -= renderChild; }
            }
        }

        private static int DifferentPixels(Color32[] a, Color32[] b) => a.Zip(b,
            (x, y) => Math.Max(Math.Abs(x.r - y.r), Math.Max(Math.Abs(x.g - y.g), Math.Abs(x.b - y.b)))).Count(x => x > 1);

        private sealed class Scene : IDisposable
        {
            private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
            private readonly RenderTexture previousTarget = RenderTexture.active;
            private readonly float previousActive = Shader.GetGlobalFloat(Active);
            private readonly Texture2D cpu;
            public readonly Camera Parent, Child;
            public readonly GameObject Surface;
            public Scene()
            {
                Shader.SetGlobalFloat(Active, 0);
                Parent = Camera("Parent"); Child = Camera("Child");
                cpu = Own(new Texture2D(128, 128, TextureFormat.RGBA32, false));
                var background = Own(GameObject.CreatePrimitive(PrimitiveType.Quad));
                background.transform.position = new Vector3(0, 0, 1);
                background.transform.localScale = new Vector3(2, 2, 1);
                var gradient = Own(new Texture2D(64, 4, TextureFormat.RGBA32, false));
                for (var y = 0; y < 4; y++) for (var x = 0; x < 64; x++)
                    gradient.SetPixel(x, y, new Color(x / 63f, y / 3f, 0, 1));
                gradient.Apply();
                var backgroundMaterial = Material("UberUnlit", gradient);
                backgroundMaterial.SetFloat("_RenderType", 0);
                Setup("ParticlesUberUnlitMaterialPostProcessor", backgroundMaterial);
                background.GetComponent<Renderer>().sharedMaterial = backgroundMaterial;
                var offset = Own(new Texture2D(2, 2, TextureFormat.RGBA32, false, true));
                offset.SetPixels(Enumerable.Repeat(new Color(1, .5f, 0, 1), 4).ToArray()); offset.Apply();
                var distortion = Material("Distortion", offset);
                distortion.SetFloat("_DistortionIntensity", .6f);
                Setup("ParticlesDistortionMaterialPostProcessor", distortion);
                Surface = Own(GameObject.CreatePrimitive(PrimitiveType.Quad));
                Surface.GetComponent<Renderer>().sharedMaterial = distortion;
            }
            private T Own<T>(T item) where T : UnityEngine.Object { owned.Add(item); return item; }
            private Camera Camera(string name)
            {
                var camera = Own(new GameObject(name, typeof(Camera))).GetComponent<Camera>();
                camera.enabled = false; camera.transform.position = new Vector3(0, 0, -3);
                camera.orthographic = true; camera.orthographicSize = 1;
                camera.nearClipPlane = .1f; camera.farClipPlane = 10;
                camera.allowHDR = false; camera.allowMSAA = false;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
                camera.targetTexture = Own(new RenderTexture(128, 128, 24));
                return camera;
            }
            private Material Material(string name, Texture texture)
            {
                var material = Own(new Material(Shader.Find("Nova/Particles/" + name)));
                material.SetFloat("_Cull", 0); material.SetTexture("_BaseMap", texture); return material;
            }
            public Behaviour AddEffect(Camera camera)
            {
                var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Runtime.Core.Scripts.ScreenSpaceDistortion")).First(t => t != null);
                return (Behaviour)camera.gameObject.AddComponent(type);
            }
            public Color32[] Render(Camera camera)
            {
                camera.Render(); RenderTexture.active = camera.targetTexture;
                cpu.ReadPixels(new Rect(0, 0, 128, 128), 0, 0); cpu.Apply(); return cpu.GetPixels32();
            }
            public void Dispose()
            {
                RenderTexture.active = previousTarget;
                foreach (var item in owned.OfType<GameObject>()) UnityEngine.Object.DestroyImmediate(item);
                foreach (var item in owned.AsEnumerable().Reverse()) if (item != null) UnityEngine.Object.DestroyImmediate(item);
                Shader.SetGlobalFloat(Active, previousActive);
            }
            private static void Setup(string name, Material material)
            {
                var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Editor.Core.Scripts." + name)).First(t => t != null);
                type.GetMethod("SetupMaterialKeywords").Invoke(null, new object[] { material });
                type.GetMethod("SetupMaterialBlendMode")?.Invoke(null, new object[] { material });
            }
        }
    }
}

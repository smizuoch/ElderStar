using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaOriginalDistortionApiTests
    {
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private readonly List<IDisposable> passes = new List<IDisposable>();
        private Camera camera;
        private RenderTexture source, shifted, neutral, output;
        private RenderTexture previous;

        private static Type Find(string name)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Nova.Runtime.Core.Scripts." + name)).FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, name + " must provide executable Built-in compatibility.");
            return type;
        }

        private object Make(string name, params object[] args)
        {
            var pass = Activator.CreateInstance(Find(name), args);
            passes.Add((IDisposable)pass);
            return pass;
        }

        private static void Call(object pass, string method, params object[] args)
        {
            try { pass.GetType().GetMethod(method).Invoke(pass, args); }
            catch (TargetInvocationException error) { throw error.InnerException; }
        }

        private T Own<T>(T value) where T : UnityEngine.Object { owned.Add(value); return value; }
        private RenderTexture Target() => Own(new RenderTexture(64, 64, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear));

        [SetUp]
        public void SetUp()
        {
            previous = RenderTexture.active;
            camera = Own(new GameObject("Original distortion API camera", typeof(Camera))).GetComponent<Camera>();
            camera.enabled = false; camera.transform.position = new Vector3(0, 0, -3);
            camera.orthographic = true; camera.orthographicSize = 1;
            source = Target(); shifted = Target(); neutral = Target(); output = Target();
            var gradient = Own(new Texture2D(64, 2, TextureFormat.RGBAFloat, false, true));
            for (int y = 0; y < 2; y++) for (int x = 0; x < 64; x++) gradient.SetPixel(x, y, new Color(x / 63f, 0, 0, 1));
            gradient.Apply(); Graphics.Blit(gradient, source);
            Clear(shifted, new Color(.7f, .5f, 0, 1)); Clear(neutral, Color.gray);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var pass in passes) pass.Dispose(); passes.Clear();
            RenderTexture.active = previous;
            foreach (var value in owned) UnityEngine.Object.DestroyImmediate(value);
            owned.Clear();
        }

        private static void Clear(RenderTexture target, Color color)
        {
            var old = RenderTexture.active; RenderTexture.active = target;
            GL.Clear(false, true, color); RenderTexture.active = old;
        }

        private float Center(RenderTexture target)
        {
            var read = Own(new Texture2D(64, 64, TextureFormat.RGBAFloat, false, true));
            RenderTexture.active = target; read.ReadPixels(new Rect(0, 0, 64, 64), 0, 0); read.Apply();
            return read.GetPixel(32, 32).r;
        }

        private object Apply(bool sceneView = true) => Make("ApplyDistortionPass", sceneView, Shader.Find("Hidden/Nova/Particles/ApplyDistortion"));

        [Test]
        public void OriginalConstructorsAndSetupSignaturesRemainCallable()
        {
            Assert.That(Find("ApplyDistortionPass").GetConstructor(new[] { typeof(bool), typeof(Shader) }), Is.Not.Null);
            Assert.That(Find("ApplyDistortionPass").GetMethod("Setup", new[] { typeof(RenderTargetIdentifier) }), Is.Not.Null);
            Assert.That(Find("DistortedUvBufferPass").GetConstructor(new[] { typeof(string) }), Is.Not.Null);
        }

        [Test]
        public void ContextResetClearsTheBindingWithoutReleasingCallerTexture()
        {
            var type = Find("DistortionContextItem");
            Assert.That(type.GetMethod("Reset").IsVirtual, Is.True, "Original Reset can be overridden by a caller's derived context.");
            var context = Activator.CreateInstance(type);
            var field = type.GetField("DistortedUvTexture");
            Assert.That(field.FieldType, Is.EqualTo(typeof(RenderTexture)), "Use a real Built-in texture, not an emulated pipeline handle.");
            field.SetValue(context, shifted);
            var apply = Apply();
            Call(apply, "Setup", new RenderTargetIdentifier((RenderTexture)field.GetValue(context)));
            using (var commands = new CommandBuffer())
            {
                Call(apply, "Execute", commands, camera, source, output);
                Call(context, "Reset");
                Assert.That(field.GetValue(context), Is.Null);
                Assert.That(shifted.IsCreated(), Is.True);
                Graphics.ExecuteCommandBuffer(commands);
            }
            Assert.That(Center(output), Is.GreaterThan(.6f));
            Call(context, "Reset");
        }

        [Test]
        public void SetupCapturesItsTargetWhenCommandsAreRecorded()
        {
            var pass = Apply(); var identity = Target();
            using (var a = new CommandBuffer()) using (var b = new CommandBuffer())
            {
                Call(pass, "Setup", new RenderTargetIdentifier(shifted));
                Call(pass, "Execute", a, camera, source, output);
                Call(pass, "Setup", new RenderTargetIdentifier(neutral));
                Call(pass, "Execute", b, camera, source, identity);
                Graphics.ExecuteCommandBuffer(b); Graphics.ExecuteCommandBuffer(a);
            }
            Assert.That(Center(identity), Is.EqualTo(.5f).Within(.03f));
            Assert.That(Center(output), Is.GreaterThan(Center(identity) + .1f));
        }

        [Test]
        public void OriginalSetupSupportsCommandBufferTemporaryTargetIdentifiers()
        {
            var pass = Apply(); int id = Shader.PropertyToID("_NovaOriginalApiTestOffsets");
            using (var commands = new CommandBuffer())
            {
                commands.GetTemporaryRT(id, 64, 64, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                commands.SetRenderTarget(id); commands.ClearRenderTarget(false, true, new Color(.7f, .5f, 0, 1));
                Call(pass, "Setup", new RenderTargetIdentifier(id));
                Call(pass, "Execute", commands, camera, source, output);
                commands.ReleaseTemporaryRT(id); Graphics.ExecuteCommandBuffer(commands);
            }
            Assert.That(Center(output), Is.GreaterThan(.6f));
        }

        [TestCase(false, .5f)]
        [TestCase(true, .9f)] // Encoded .7 represents signed UV offset .7 * 2 - 1 = .4.
        public void SceneViewConstructorOptionControlsActualCompositing(bool enabled, float expected)
        {
            var pass = Apply(enabled); camera.cameraType = CameraType.SceneView;
            Call(pass, "Setup", new RenderTargetIdentifier(shifted));
            using (var commands = new CommandBuffer())
            {
                Call(pass, "Execute", commands, camera, source, output); Graphics.ExecuteCommandBuffer(commands);
            }
            Assert.That(Center(output), Is.EqualTo(expected).Within(.04f));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void LightModeSelectionUsesTheTagRatherThanThePassName(bool enabled)
        {
            var pass = Make("DistortedUvBufferPass", "NovaCompatibilityOffsets");
            var quad = Own(GameObject.CreatePrimitive(PrimitiveType.Quad));
            var material = Own(new Material(Shader.Find("Hidden/NovaTests/OriginalApiLightMode")));
            material.SetShaderPassEnabled("NovaCompatibilityOffsets", enabled);
            quad.GetComponent<Renderer>().sharedMaterial = material;
            Call(pass, "Setup", shifted);
            using (var commands = new CommandBuffer())
            {
                Call(pass, "Execute", commands, camera, new[] { quad.GetComponent<Renderer>() });
                Graphics.ExecuteCommandBuffer(commands);
            }
            Assert.That(Center(shifted), Is.EqualTo(enabled ? .7f : .5f).Within(.02f));
        }

        [Test]
        public void MissingSetupAndAliasedTargetsFailBeforeRecording()
        {
            var apply = Apply(); var offsets = Make("DistortedUvBufferPass", "DistortedUvBuffer");
            using (var commands = new CommandBuffer())
            {
                Assert.Throws<InvalidOperationException>(() => Call(apply, "Execute", commands, camera, source, output));
                Assert.Throws<InvalidOperationException>(() => Call(offsets, "Execute", commands, camera, Array.Empty<Renderer>()));
                Call(apply, "Setup", new RenderTargetIdentifier(output));
                Assert.Throws<ArgumentException>(() => Call(apply, "Execute", commands, camera, source, output));
                Assert.That(commands.sizeInBytes, Is.Zero);
            }
        }

        [TestCase(CameraType.Reflection)]
        [TestCase(CameraType.Preview)]
        public void UnsupportedCameraKindsFailExplicitly(CameraType kind)
        {
            var pass = Apply(); camera.cameraType = kind;
            Call(pass, "Setup", new RenderTargetIdentifier(shifted));
            using (var commands = new CommandBuffer())
                Assert.Throws<NotSupportedException>(() => Call(pass, "Execute", commands, camera, source, output));
        }

        [Test]
        public void DisposedPassesRejectUseWithoutDestroyingCallerTextures()
        {
            var apply = Apply(); var offsets = Make("DistortedUvBufferPass", "DistortedUvBuffer");
            Call(offsets, "Setup", shifted);
            ((IDisposable)apply).Dispose(); ((IDisposable)offsets).Dispose();
            Assert.Throws<ObjectDisposedException>(() => Call(apply, "Setup", new RenderTargetIdentifier(shifted)));
            Assert.Throws<ObjectDisposedException>(() => Call(offsets, "Setup", shifted));
            Assert.That(shifted.IsCreated(), Is.True);
        }

        [UnityTest]
        public IEnumerator ApplyPassReleasesItsOwnedMaterial()
        {
            var shader = Shader.Find("Hidden/Nova/Particles/ApplyDistortion");
            int Count() => Resources.FindObjectsOfTypeAll<Material>().Count(m => m.shader == shader);
            int before = Count(); var pass = Apply();
            Assert.That(Count(), Is.EqualTo(before + 1));
            ((IDisposable)pass).Dispose(); yield return null;
            Assert.That(Count(), Is.EqualTo(before));
        }

        [Test]
        public void ExistingFacadeCommandBudgetIsRecorded()
        {
            var pass = Make("BuiltInDistortionPass");
            using (var commands = new CommandBuffer())
            {
                Call(pass, "RecordOffsets", commands, camera, neutral, Array.Empty<Renderer>());
                Call(pass, "RecordComposite", commands, source, output, neutral);
                TestContext.WriteLine("ExistingFacadeCommandBytes=" + commands.sizeInBytes);
                Graphics.ExecuteCommandBuffer(commands);
            }
            Assert.That(Center(output), Is.EqualTo(.5f).Within(.03f));
        }
    }
}

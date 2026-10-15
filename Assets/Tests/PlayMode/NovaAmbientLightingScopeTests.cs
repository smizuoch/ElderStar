using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Nova.Tests.PlayMode
{
    public sealed class NovaAmbientLightingScopeTests
    {
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void NestedCamerasRestoreAndReleaseAmbientState(bool bindChild, bool disableParent)
        {
            const string key = "_NovaGlobalGlossyAmbient";
            var saved = Shader.GetGlobalVector(key);
            var external = new Vector4(.1f,.2f,.3f,.4f);
            var parentObject = new GameObject("Ambient parent",typeof(Camera));
            var childObject = new GameObject("Ambient child",typeof(Camera));
            var parent = parentObject.GetComponent<Camera>(); var child = childObject.GetComponent<Camera>();
            var target = new RenderTexture(8,8,24); parent.targetTexture=target; child.targetTexture=target;
            parent.enabled=false; child.enabled=false; parent.cullingMask=0; child.cullingMask=0;
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Nova.Runtime.Core.Scripts.NovaAmbientLighting")).First(t=>t!=null);
            var effect = (Behaviour)parentObject.AddComponent(type);
            if(bindChild) childObject.AddComponent(type);
            Vector4 before=default, inside=default, after=default;
            Camera.CameraCallback callback = camera =>
            {
                if(camera==parent)
                {
                    before=Shader.GetGlobalVector(key); child.Render(); after=Shader.GetGlobalVector(key);
                }
                else if(camera==child)
                {
                    inside=Shader.GetGlobalVector(key);
                    if(disableParent) effect.enabled=false;
                }
            };
            try
            {
                Shader.SetGlobalVector(key,external); Camera.onPreRender+=callback;
                parent.Render();
                Assert.That(before.w,Is.EqualTo(1));
                Assert.That(inside.w,Is.EqualTo(bindChild ? 1 : 0));
                Assert.That(after,Is.EqualTo(disableParent ? Vector4.zero : before));
                Assert.That(Shader.GetGlobalVector(key),Is.EqualTo(external));
                Camera.onPreRender-=callback;
                UnityEngine.Object.DestroyImmediate(parentObject);
                var childEffect=child.GetComponent(type); if(childEffect) UnityEngine.Object.DestroyImmediate(childEffect);
                Vector4 unbound=default;
                Camera.CameraCallback observe = c=>unbound=Shader.GetGlobalVector(key);
                Camera.onPreRender+=observe;
                try { child.Render(); } finally { Camera.onPreRender-=observe; }
                Assert.That(unbound,Is.EqualTo(external),"Last owner removal must detach callbacks.");
            }
            finally
            {
                Camera.onPreRender-=callback;
                if(parentObject) UnityEngine.Object.DestroyImmediate(parentObject);
                UnityEngine.Object.DestroyImmediate(childObject); UnityEngine.Object.DestroyImmediate(target);
                Shader.SetGlobalVector(key,saved);
            }
        }
    }
}

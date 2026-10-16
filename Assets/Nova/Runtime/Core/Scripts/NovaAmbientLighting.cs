using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Runtime.Core.Scripts
{
    /// <summary>Optional Built-in camera binding for original NOVA's global glossy
    /// ambient probe. Local diffuse probes remain supplied by the renderer.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    [AddComponentMenu("ElderStar/Global Ambient Lighting")]
    public sealed class NovaAmbientLighting : MonoBehaviour
    {
        private static readonly int GlobalAmbient = Shader.PropertyToID("_NovaGlobalGlossyAmbient");
        private static readonly Dictionary<Camera, NovaAmbientLighting> Owners = new Dictionary<Camera, NovaAmbientLighting>();
        private static readonly List<Scope> Scopes = new List<Scope>(4);
        private static bool subscribed;
        private Camera targetCamera;
        private struct Scope { public Camera Camera; public Vector4 Previous; }

        private void OnEnable()
        {
            targetCamera = GetComponent<Camera>();
            Owners[targetCamera] = this;
            if (subscribed) return;
            Camera.onPreRender += BeginCamera;
            Camera.onPostRender += EndCamera;
            subscribed = true;
        }

        private void OnDisable()
        {
            if (targetCamera != null) Owners.Remove(targetCamera);
            if (Scopes.Count > 0 && Scopes[Scopes.Count - 1].Camera == targetCamera)
                Shader.SetGlobalVector(GlobalAmbient, Vector4.zero);
            UnsubscribeIfIdle();
        }

        private static bool IsBound(Camera camera) => GraphicsSettings.currentRenderPipeline == null
            && Owners.TryGetValue(camera, out var owner) && owner != null && owner.isActiveAndEnabled;

        private static void BeginCamera(Camera camera)
        {
            Scopes.Add(new Scope { Camera = camera, Previous = Shader.GetGlobalVector(GlobalAmbient) });
            var value = Vector4.zero;
            if (IsBound(camera))
            {
                // Same DC coefficient, intensity and C# Color.gamma as URP's
                // _GlossyEnvironmentColor. This is independent of renderer SH.
                var probe = RenderSettings.ambientProbe;
                var color = new Color(probe[0, 0], probe[1, 0], probe[2, 0]) * RenderSettings.reflectionIntensity;
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma) color = color.gamma;
                value = new Vector4(color.r, color.g, color.b, 1);
            }
            Shader.SetGlobalVector(GlobalAmbient, value);
        }

        private static void EndCamera(Camera camera)
        {
            if (Scopes.Count == 0 || Scopes[Scopes.Count - 1].Camera != camera) return;
            var scope = Scopes[Scopes.Count - 1];
            Scopes.RemoveAt(Scopes.Count - 1);
            // A parent disabled during a nested render must not be reactivated.
            var value = Scopes.Count == 0 || IsBound(Scopes[Scopes.Count - 1].Camera) ? scope.Previous : Vector4.zero;
            Shader.SetGlobalVector(GlobalAmbient, value);
            UnsubscribeIfIdle();
        }

        private static void UnsubscribeIfIdle()
        {
            if (!subscribed || Owners.Count != 0 || Scopes.Count != 0) return;
            Camera.onPreRender -= BeginCamera;
            Camera.onPostRender -= EndCamera;
            subscribed = false;
        }
    }
}

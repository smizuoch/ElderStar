using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Runtime.Core.Scripts
{
    /// <summary>Built-in camera image effect replacing NOVA's URP renderer feature.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    [AddComponentMenu("ElderStar/Screen Space Distortion")]
    [ImageEffectAllowedInSceneView]
    public sealed class ScreenSpaceDistortion : MonoBehaviour
    {
        [SerializeField] private bool _applyToSceneView = true;
        [SerializeField, HideInInspector] private Shader _applyDistortionShader;
        private BuiltInDistortionPass distortionPass;
        private Camera targetCamera;
        private static readonly int CameraActive = Shader.PropertyToID("_NovaCameraDistortionActive");
        private static readonly Dictionary<Camera, ScreenSpaceDistortion> Owners = new Dictionary<Camera, ScreenSpaceDistortion>();
        private static readonly List<CameraScope> Scopes = new List<CameraScope>(4);
        private static bool subscribed;

        private struct CameraScope
        {
            public Camera Camera;
            public float PreviousActive;
        }

        public void Create()
        {
            targetCamera = GetComponent<Camera>();
            targetCamera.depthTextureMode |= DepthTextureMode.Depth;
            if (distortionPass != null) return;
            if (_applyDistortionShader == null)
                _applyDistortionShader = Shader.Find("Hidden/Nova/Particles/ApplyDistortion");
            if (_applyDistortionShader != null)
                distortionPass = new BuiltInDistortionPass(_applyDistortionShader);
        }

        private void OnEnable()
        {
            Create();
            Owners[targetCamera] = this;
            if (subscribed) return;
            Camera.onPreCull += BeginCamera;
            Camera.onPostRender += EndCamera;
            subscribed = true;
        }

        private void OnDisable()
        {
            if (targetCamera != null) Owners.Remove(targetCamera);
            if (Scopes.Count > 0) SetCameraActive(Scopes[Scopes.Count - 1].Camera);
            UnsubscribeIfIdle();
            distortionPass?.Dispose();
            distortionPass = null;
            // Leave the shared camera depth flag set: other effects may now depend on it.
        }

        private bool CanComposite => distortionPass != null && GraphicsSettings.currentRenderPipeline == null
            && !targetCamera.stereoEnabled && targetCamera.cameraType != CameraType.Preview
            && targetCamera.cameraType != CameraType.Reflection
            && (_applyToSceneView || targetCamera.cameraType != CameraType.SceneView);

        private static void BeginCamera(Camera camera)
        {
            // Reflection/preview cameras often have no NOVA component. Scope every
            // camera while any compositor exists so they cannot inherit its flag.
            Scopes.Add(new CameraScope { Camera = camera, PreviousActive = Shader.GetGlobalFloat(CameraActive) });
            SetCameraActive(camera);
        }

        private static void EndCamera(Camera camera)
        {
            if (Scopes.Count == 0 || Scopes[Scopes.Count - 1].Camera != camera) return;
            var scope = Scopes[Scopes.Count - 1];
            Scopes.RemoveAt(Scopes.Count - 1);
            if (Scopes.Count == 0) Shader.SetGlobalFloat(CameraActive, scope.PreviousActive);
            else SetCameraActive(Scopes[Scopes.Count - 1].Camera);
            UnsubscribeIfIdle();
        }

        private static void SetCameraActive(Camera camera)
        {
            Shader.SetGlobalFloat(CameraActive, Owners.TryGetValue(camera, out var owner)
                && owner != null && owner.isActiveAndEnabled && owner.CanComposite ? 1 : 0);
        }

        private static void UnsubscribeIfIdle()
        {
            // Disabling the last owner inside a nested render must still unwind
            // both camera scopes and restore the caller's external shader state.
            if (!subscribed || Owners.Count != 0 || Scopes.Count != 0) return;
            Camera.onPreCull -= BeginCamera;
            Camera.onPostRender -= EndCamera;
            subscribed = false;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!CanComposite)
            {
                Graphics.Blit(source, destination);
                return;
            }

            var format = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf)
                ? RenderTextureFormat.RGHalf : RenderTextureFormat.ARGBHalf;
            var offsets = RenderTexture.GetTemporary(source.width, source.height, 0, format, RenderTextureReadWrite.Linear);
            offsets.filterMode = FilterMode.Bilinear;
            offsets.wrapMode = TextureWrapMode.Clamp;
            try
            {
                using (var commands = new CommandBuffer { name = "NOVA Built-in Distortion" })
                {
                    distortionPass.RecordOffsets(commands, targetCamera, offsets, FindObjectsOfType<Renderer>());
                    distortionPass.RecordComposite(commands, source, destination, offsets);
                    Graphics.ExecuteCommandBuffer(commands);
                }
            }
            finally
            {
                RenderTexture.ReleaseTemporary(offsets);
            }
        }
    }
}

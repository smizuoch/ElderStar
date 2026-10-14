using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Runtime.Core.Scripts
{
    /// <summary>Built-in camera image effect replacing NOVA's URP renderer feature.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    [AddComponentMenu("NOVA/Screen Space Distortion")]
    [ImageEffectAllowedInSceneView]
    public sealed class ScreenSpaceDistortion : MonoBehaviour
    {
        [SerializeField] private bool _applyToSceneView = true;
        [SerializeField, HideInInspector] private Shader _applyDistortionShader;
        private BuiltInDistortionPass distortionPass;
        private Camera targetCamera;
        private static readonly int CameraActive = Shader.PropertyToID("_NovaCameraDistortionActive");
        private float previousCameraActive;
        private bool rendering;

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

        private void OnEnable() { Create(); }

        private void OnDisable()
        {
            OnPostRender();
            distortionPass?.Dispose();
            distortionPass = null;
            // Leave the shared camera depth flag set: other effects may now depend on it.
        }

        private bool CanComposite => distortionPass != null && GraphicsSettings.currentRenderPipeline == null
            && !targetCamera.stereoEnabled && targetCamera.cameraType != CameraType.Preview
            && targetCamera.cameraType != CameraType.Reflection
            && (_applyToSceneView || targetCamera.cameraType != CameraType.SceneView);

        private void OnPreCull()
        {
            previousCameraActive = Shader.GetGlobalFloat(CameraActive);
            Shader.SetGlobalFloat(CameraActive, CanComposite ? 1 : 0);
            rendering = true;
        }

        private void OnPostRender()
        {
            if (!rendering) return;
            Shader.SetGlobalFloat(CameraActive, previousCameraActive);
            rendering = false;
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

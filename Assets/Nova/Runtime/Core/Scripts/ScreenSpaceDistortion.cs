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
        private Material applyMaterial;
        private Camera targetCamera;
        private static readonly int UvTexture = Shader.PropertyToID("_ScreenSpaceUvTexture");

        public void Create()
        {
            targetCamera = GetComponent<Camera>();
            targetCamera.depthTextureMode |= DepthTextureMode.Depth;
            if (applyMaterial != null) return;
            if (_applyDistortionShader == null)
                _applyDistortionShader = Shader.Find("Hidden/Nova/Particles/ApplyDistortion");
            if (_applyDistortionShader != null)
                applyMaterial = new Material(_applyDistortionShader) { hideFlags = HideFlags.HideAndDontSave };
        }

        private void OnEnable() { Create(); }

        private void OnDisable()
        {
            if (applyMaterial == null) return;
            if (Application.isPlaying) Destroy(applyMaterial);
            else DestroyImmediate(applyMaterial);
            applyMaterial = null;
            // Leave the shared camera depth flag set: other effects may now depend on it.
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (applyMaterial == null || GraphicsSettings.currentRenderPipeline != null || targetCamera.stereoEnabled
                || targetCamera.cameraType == CameraType.Preview || targetCamera.cameraType == CameraType.Reflection
                || (!_applyToSceneView && targetCamera.cameraType == CameraType.SceneView))
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
                    commands.SetRenderTarget(offsets);
                    commands.ClearRenderTarget(false, true, Color.gray);
                    commands.SetViewProjectionMatrices(targetCamera.worldToCameraMatrix,
                        GL.GetGPUProjectionMatrix(targetCamera.projectionMatrix, true));
                    commands.SetInvertCulling(SystemInfo.graphicsUVStartsAtTop ^ GL.invertCulling);
                    var planes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
                    foreach (var renderer in FindObjectsOfType<Renderer>())
                    {
                        if (!renderer.enabled || renderer.forceRenderingOff || !renderer.gameObject.activeInHierarchy
                            || renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly
                            || (targetCamera.cullingMask & (1 << renderer.gameObject.layer)) == 0
                            || !GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) continue;
                        var materials = renderer.sharedMaterials;
                        for (var submesh = 0; submesh < materials.Length; submesh++)
                        {
                            var material = materials[submesh];
                            if (material == null) continue;
                            var pass = material.FindPass("DistortedUvBuffer");
                            if (pass >= 0 && material.GetShaderPassEnabled("DistortedUvBuffer"))
                                commands.DrawRenderer(renderer, material, submesh, pass);
                        }
                    }
                    commands.SetInvertCulling(GL.invertCulling);
                    Graphics.ExecuteCommandBuffer(commands);
                }
                applyMaterial.SetTexture(UvTexture, offsets);
                // The original hidden shader has no ShaderLab Properties block.
                // Bind the source explicitly without adding a public material property.
                applyMaterial.SetTexture("_MainTex", source);
                Graphics.Blit(source, destination, applyMaterial);
            }
            finally
            {
                applyMaterial.SetTexture(UvTexture, null);
                applyMaterial.SetTexture("_MainTex", null);
                RenderTexture.ReleaseTemporary(offsets);
            }
        }
    }
}

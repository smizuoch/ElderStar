using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Runtime.Core.Scripts
{
    /// <summary>Optional NOVA-only world normals (RGB encoded 0..1) and coverage (A).
    /// Does not replace the camera's depth or depth-normal texture.</summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    [AddComponentMenu("ElderStar/Depth Normals")]
    public sealed class NovaDepthNormals : MonoBehaviour
    {
        [SerializeField] private List<Renderer> renderers = new List<Renderer>();
        private readonly List<Material> materials = new List<Material>();
        private readonly Plane[] planes = new Plane[6];
        private Camera targetCamera;
        private bool cameraWasEnabled;
        private CommandBuffer commands;
        private RenderTexture normals;
        private static readonly int SceneDepth = Shader.PropertyToID("_NovaDepthNormalsUseSceneDepth");
        public RenderTexture Texture => normals;

        public void RegisterRenderer(Renderer renderer) { if (renderer != null && !renderers.Contains(renderer)) renderers.Add(renderer); }
        public void UnregisterRenderer(Renderer renderer) { renderers.Remove(renderer); }
        [ContextMenu("Collect Scene Renderers")]
        public void CollectRenderers()
        {
            renderers.Clear();
            foreach (var renderer in FindObjectsOfType<Renderer>(true))
            {
                renderer.GetSharedMaterials(materials);
                foreach (var material in materials)
                    if (material != null && HasDepthNormalsTag(material))
                    { RegisterRenderer(renderer); break; }
            }
        }
        private void Reset() { CollectRenderers(); }
        private void OnEnable()
        {
            targetCamera = GetComponent<Camera>();
            cameraWasEnabled = targetCamera.enabled;
            targetCamera.depthTextureMode |= DepthTextureMode.Depth;
            if (renderers.Count == 0) CollectRenderers();
        }
        private void LateUpdate()
        {
            if (targetCamera == null) return;
            // A disabled camera can still be rendered explicitly. Reuse its buffer;
            // release when automatic rendering transitions from enabled to disabled.
            if (cameraWasEnabled && !targetCamera.enabled) Release();
            cameraWasEnabled = targetCamera.enabled;
        }
        private void OnPreCull()
        {
            if (GraphicsSettings.currentRenderPipeline != null) { Release(); return; }
            int width = Mathf.Max(1, targetCamera.pixelWidth), height = Mathf.Max(1, targetCamera.pixelHeight);
            var dimension = TextureDimension.Tex2D;
            int slices = 1;
            #if NOVA_VR_MODULE
            if (targetCamera.stereoEnabled && UnityEngine.XR.XRSettings.enabled)
            {
                var eye = UnityEngine.XR.XRSettings.eyeTextureDesc;
                width = eye.width; height = eye.height; dimension = eye.dimension; slices = eye.volumeDepth;
            }
            #endif
            if (normals == null || normals.width != width || normals.height != height || normals.dimension != dimension || normals.volumeDepth != slices)
            {
                Release();
                normals = new RenderTexture(new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGBHalf, 24)
                { dimension = dimension, volumeDepth = slices, msaaSamples = 1, sRGB = false })
                { name = "NOVA Depth Normals", hideFlags = HideFlags.HideAndDontSave, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                normals.Create();
            }
            if (commands == null)
            {
                commands = new CommandBuffer { name = "NOVA Depth Normals" };
                targetCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commands);
            }
            commands.Clear();
            commands.SetRenderTarget(normals, 0, CubemapFace.Unknown, dimension == TextureDimension.Tex2DArray ? -1 : 0);
            commands.ClearRenderTarget(true, true, Color.clear);
            commands.SetGlobalFloat(SceneDepth, 1);
            GeometryUtility.CalculateFrustumPlanes(targetCamera, planes);
            foreach (var renderer in renderers)
            {
                if (renderer == null || !renderer.enabled || renderer.forceRenderingOff || !renderer.gameObject.activeInHierarchy
                    || renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly
                    || (targetCamera.cullingMask & (1 << renderer.gameObject.layer)) == 0
                    || !GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) continue;
                renderer.GetSharedMaterials(materials);
                for (int submesh = 0; submesh < materials.Count; submesh++)
                {
                    var material = materials[submesh];
                    if (material == null || !HasDepthNormalsTag(material)) continue;
                    int pass = material.FindPass("DepthNormals");
                    if (pass >= 0 && material.GetShaderPassEnabled("DepthNormals")) commands.DrawRenderer(renderer, material, submesh, pass);
                }
            }
            commands.SetGlobalFloat(SceneDepth, 0);
            commands.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        }
        private void OnDisable() { Release(); }
        private void OnDestroy() { Release(); }
        private static bool HasDepthNormalsTag(Material material) =>
            string.Equals(material.GetTag("NovaDepthNormals", false), "True", System.StringComparison.OrdinalIgnoreCase);
        private void Release()
        {
            if (commands != null)
            {
                if (targetCamera != null) targetCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, commands);
                commands.Dispose(); commands = null;
            }
            if (normals != null)
            {
                normals.Release();
                if (Application.isPlaying) Destroy(normals); else DestroyImmediate(normals);
                normals = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Runtime.Core.Scripts
{
    /// <summary>
    /// Records the two mono Built-in distortion stages into a caller-owned command buffer.
    /// Keep this instance and all input textures/renderers alive until execution completes.
    /// RecordOffsets changes the render target and view/projection state; the caller owns
    /// their restoration and supplies the camera depth texture for depth-aware materials.
    /// XR/reflection cameras should use the shader-only distortion path instead.
    /// </summary>
    public sealed class BuiltInDistortionPass : IDisposable
    {
        private readonly Plane[] planes = new Plane[6];
        private readonly List<Material> materials = new List<Material>();
        private Material applyMaterial;
        private static readonly int UvTexture = Shader.PropertyToID("_ScreenSpaceUvTexture");
        private static readonly int MainTexture = Shader.PropertyToID("_MainTex");
        private static readonly int ForceAniso = Shader.PropertyToID("_NovaDistortionForceAniso");

        public BuiltInDistortionPass() : this(Shader.Find("Hidden/Nova/Particles/ApplyDistortion")) { }

        public BuiltInDistortionPass(Shader shader)
        {
            if (shader == null) throw new ArgumentNullException(nameof(shader));
            applyMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }

        /// <summary>Clear to neutral offsets and draw the explicit renderer list; no scene search.</summary>
        public void RecordOffsets(CommandBuffer commands, Camera camera, RenderTexture offsets, IReadOnlyList<Renderer> renderers)
        {
            Validate(commands);
            if (camera == null) throw new ArgumentNullException(nameof(camera));
            if (offsets == null) throw new ArgumentNullException(nameof(offsets));
            if (renderers == null) throw new ArgumentNullException(nameof(renderers));
            if (camera.stereoEnabled || camera.cameraType == CameraType.Reflection || GraphicsSettings.currentRenderPipeline != null)
                throw new NotSupportedException("This pass records ordinary mono Built-in cameras. Use the shader-only path for XR/reflection.");
            if (offsets.dimension != TextureDimension.Tex2D)
                throw new ArgumentException("The offset target must be a mono 2D render texture.", nameof(offsets));

            commands.SetRenderTarget(offsets);
            commands.ClearRenderTarget(false, true, Color.gray);
            commands.SetViewProjectionMatrices(camera.worldToCameraMatrix, GL.GetGPUProjectionMatrix(camera.projectionMatrix, true));
            commands.SetInvertCulling(SystemInfo.graphicsUVStartsAtTop ^ GL.invertCulling);
            GeometryUtility.CalculateFrustumPlanes(camera, planes);
            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || !renderer.enabled || renderer.forceRenderingOff || !renderer.gameObject.activeInHierarchy
                    || renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly
                    || (camera.cullingMask & (1 << renderer.gameObject.layer)) == 0
                    || !GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) continue;
                renderer.GetSharedMaterials(materials);
                for (int submesh = 0; submesh < materials.Count; submesh++)
                {
                    var material = materials[submesh];
                    if (material == null) continue;
                    int pass = material.FindPass("DistortedUvBuffer");
                    if (pass >= 0 && material.GetShaderPassEnabled("DistortedUvBuffer"))
                        commands.DrawRenderer(renderer, material, submesh, pass);
                }
            }
            commands.SetInvertCulling(GL.invertCulling);
        }

        /// <summary>
        /// Apply signed offsets to source. Texture bindings are recorded as commands, not
        /// mutable material state, so independently recorded cameras can share this object.
        /// Global texture/anisotropy bindings are restored to their values at record time.
        /// </summary>
        public void RecordComposite(CommandBuffer commands, RenderTexture source, RenderTexture destination, RenderTexture offsets)
        {
            Validate(commands);
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (offsets == null) throw new ArgumentNullException(nameof(offsets));
            if (source == destination || offsets == destination)
                throw new ArgumentException("Compositing cannot read and write the same texture.", nameof(destination));
            var previousSource = Shader.GetGlobalTexture(MainTexture);
            var previousOffsets = Shader.GetGlobalTexture(UvTexture);
            float previousAniso = Shader.GetGlobalFloat(ForceAniso);
            commands.SetGlobalTexture(MainTexture, source);
            commands.SetGlobalTexture(UvTexture, offsets);
            commands.SetGlobalFloat(ForceAniso, QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable ? 1 : 0);
            commands.Blit(source, destination, applyMaterial);
            commands.SetGlobalTexture(MainTexture, previousSource);
            commands.SetGlobalTexture(UvTexture, previousOffsets);
            commands.SetGlobalFloat(ForceAniso, previousAniso);
        }

        private void Validate(CommandBuffer commands)
        {
            if (applyMaterial == null) throw new ObjectDisposedException(nameof(BuiltInDistortionPass));
            if (commands == null) throw new ArgumentNullException(nameof(commands));
        }

        public void Dispose()
        {
            if (applyMaterial == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(applyMaterial);
            else UnityEngine.Object.DestroyImmediate(applyMaterial);
            applyMaterial = null;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Runtime.Core.Scripts
{
    /// <summary>
    /// Built-in signed-offset stage. The constructor retains the original LightMode meaning,
    /// including passes whose Name differs from the tag. Resources belong to the caller.
    /// </summary>
    public class DistortedUvBufferPass : IDisposable
    {
        private readonly string lightMode;
        private readonly ShaderTagId lightModeValue;
        private static readonly ShaderTagId LightModeTag = new ShaderTagId("LightMode");
        private readonly Plane[] planes = new Plane[6];
        private readonly List<Material> materials = new List<Material>();
        private RenderTexture target;
        private bool disposed;

        public DistortedUvBufferPass(string lightMode)
        {
            if (string.IsNullOrEmpty(lightMode)) throw new ArgumentException("A LightMode tag is required.", nameof(lightMode));
            this.lightMode = lightMode;
            lightModeValue = new ShaderTagId(lightMode);
        }

        /// <summary>Built-in target binding; the caller owns allocation and release.</summary>
        public void Setup(RenderTexture renderTarget)
        {
            ThrowIfDisposed();
            ValidateTarget(renderTarget);
            target = renderTarget;
        }

        /// <summary>Clear neutral offsets and record visible renderers into the configured target.</summary>
        public void Execute(CommandBuffer commands, Camera camera, IReadOnlyList<Renderer> renderers)
        {
            ThrowIfDisposed();
            if (target == null) throw new InvalidOperationException("Call Setup with a live target before Execute.");
            RecordOffsets(commands, camera, target, renderers);
        }

        internal void RecordOffsets(CommandBuffer commands, Camera camera, RenderTexture offsets, IReadOnlyList<Renderer> renderers)
        {
            ThrowIfDisposed();
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            ValidateCamera(camera);
            ValidateTarget(offsets);
            if (renderers == null) throw new ArgumentNullException(nameof(renderers));
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
                    if (material == null || !material.GetShaderPassEnabled(lightMode)) continue;
                    for (int pass = 0; pass < material.passCount; pass++)
                    {
                        if (material.shader.FindPassTagValue(pass, LightModeTag) != lightModeValue) continue;
                        commands.DrawRenderer(renderer, material, submesh, pass);
                        break;
                    }
                }
            }
            commands.SetInvertCulling(GL.invertCulling);
        }

        internal static void ValidateCamera(Camera camera)
        {
            if (camera == null) throw new ArgumentNullException(nameof(camera));
            if (camera.stereoEnabled || camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview
                || GraphicsSettings.currentRenderPipeline != null)
                throw new NotSupportedException("This pass records ordinary mono Built-in cameras. Use the shader-only path for XR/reflection/preview.");
        }

        private static void ValidateTarget(RenderTexture target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (target.dimension != TextureDimension.Tex2D)
                throw new ArgumentException("The offset target must be a mono 2D render texture.", nameof(target));
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(DistortedUvBufferPass));
        }

        public void Dispose()
        {
            disposed = true;
            target = null;
            materials.Clear();
        }
    }
}

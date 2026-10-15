using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Runtime.Core.Scripts
{
    /// <summary>
    /// Built-in distortion composite preserving the original constructor and Setup signature.
    /// Keep this object and supplied targets alive until recorded commands complete.
    /// </summary>
    public class ApplyDistortionPass : IDisposable
    {
        private readonly bool applyToSceneView;
        private Material material;
        private RenderTargetIdentifier offsets;
        private bool configured;
        private static readonly int UvTexture = Shader.PropertyToID("_ScreenSpaceUvTexture");
        private static readonly int MainTexture = Shader.PropertyToID("_MainTex");
        private static readonly int ForceAniso = Shader.PropertyToID("_NovaDistortionForceAniso");

        public ApplyDistortionPass(bool applyToSceneView, Shader shader)
        {
            if (shader == null) throw new ArgumentNullException(nameof(shader));
            this.applyToSceneView = applyToSceneView;
            material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }

        /// <summary>Bind a caller-owned texture or a command-buffer temporary target.</summary>
        public void Setup(RenderTargetIdentifier distortedUvBufferIdentifier)
        {
            ThrowIfDisposed();
            offsets = distortedUvBufferIdentifier;
            configured = true;
        }

        /// <summary>
        /// Record the composite. Excluded scene-view cameras copy the source unchanged.
        /// This does not submit commands or release caller-owned resources.
        /// </summary>
        public void Execute(CommandBuffer commands, Camera camera, RenderTexture source, RenderTexture destination)
        {
            ThrowIfDisposed();
            if (!configured) throw new InvalidOperationException("Call Setup before Execute.");
            DistortedUvBufferPass.ValidateCamera(camera);
            ValidateTargets(commands, source, destination, offsets);
            if (!applyToSceneView && camera.cameraType == CameraType.SceneView)
            {
                commands.Blit(source, destination);
                return;
            }
            RecordComposite(commands, source, destination, offsets);
        }

        internal void RecordComposite(CommandBuffer commands, RenderTexture source, RenderTexture destination, RenderTargetIdentifier offsetTarget)
        {
            ThrowIfDisposed();
            ValidateTargets(commands, source, destination, offsetTarget);
            var previousSource = Shader.GetGlobalTexture(MainTexture);
            var previousOffsets = Shader.GetGlobalTexture(UvTexture);
            float previousAniso = Shader.GetGlobalFloat(ForceAniso);
            commands.SetGlobalTexture(MainTexture, source);
            commands.SetGlobalTexture(UvTexture, offsetTarget);
            commands.SetGlobalFloat(ForceAniso, QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable ? 1 : 0);
            commands.Blit(source, destination, material);
            commands.SetGlobalTexture(MainTexture, previousSource);
            commands.SetGlobalTexture(UvTexture, previousOffsets);
            commands.SetGlobalFloat(ForceAniso, previousAniso);
        }

        private static void ValidateTargets(CommandBuffer commands, RenderTexture source, RenderTexture destination, RenderTargetIdentifier offsetTarget)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source == destination || (destination != null && offsetTarget.Equals(new RenderTargetIdentifier(destination))))
                throw new ArgumentException("Compositing cannot read and write the same texture.", nameof(destination));
        }

        private void ThrowIfDisposed()
        {
            if (material == null) throw new ObjectDisposedException(nameof(ApplyDistortionPass));
        }

        public void Dispose()
        {
            if (material == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(material);
            else UnityEngine.Object.DestroyImmediate(material);
            material = null;
        }
    }
}

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
        private readonly DistortedUvBufferPass offsetsPass;
        private readonly ApplyDistortionPass applyPass;

        public BuiltInDistortionPass() : this(Shader.Find("Hidden/Nova/Particles/ApplyDistortion")) { }

        public BuiltInDistortionPass(Shader shader)
        {
            applyPass = new ApplyDistortionPass(true, shader);
            offsetsPass = new DistortedUvBufferPass("DistortedUvBuffer");
        }

        /// <summary>Clear to neutral offsets and draw the explicit renderer list; no scene search.</summary>
        public void RecordOffsets(CommandBuffer commands, Camera camera, RenderTexture offsets, IReadOnlyList<Renderer> renderers)
        {
            offsetsPass.RecordOffsets(commands, camera, offsets, renderers);
        }

        /// <summary>
        /// Apply signed offsets to source. Texture bindings are recorded as commands, not
        /// mutable material state, so independently recorded cameras can share this object.
        /// Global texture/anisotropy bindings are restored to their values at record time.
        /// </summary>
        public void RecordComposite(CommandBuffer commands, RenderTexture source, RenderTexture destination, RenderTexture offsets)
        {
            if (offsets == null) throw new ArgumentNullException(nameof(offsets));
            applyPass.RecordComposite(commands, source, destination, new RenderTargetIdentifier(offsets));
        }

        public void Dispose()
        {
            offsetsPass.Dispose();
            applyPass.Dispose();
        }
    }
}

using UnityEngine;

namespace Nova.Runtime.Core.Scripts
{
    /// <summary>
    /// Per-camera Built-in distortion bindings. The caller owns the actual texture and
    /// its lifetime. Reset clears the binding without releasing caller-owned GPU resources.
    /// This is a plain data object, not an engine scheduling context.
    /// </summary>
    public class DistortionContextItem
    {
        public RenderTexture DistortedUvTexture;

        public virtual void Reset()
        {
            DistortedUvTexture = null;
        }
    }
}

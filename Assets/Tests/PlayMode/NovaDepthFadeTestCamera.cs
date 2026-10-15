using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal static class NovaDepthFadeTestCamera
{
    // These legacy assertions use physical eye-distance ranges. Original NOVA's
    // projection-depth linearization represents those ranges only in perspective.
    // Keep framing at the z=0 test surface; orthographic/oblique behavior has its
    // own independent original-math image oracles in NovaOriginalDepthFadeTests
    // and NovaOriginalSoftParticleTests.
    public static void UseEyeDistanceRanges(Camera camera, IEnumerable<Material> materials)
    {
        if (!materials.Any(m => (m.HasProperty("_DepthFadeEnabled") && m.GetFloat("_DepthFadeEnabled") > .5f)
            || (m.HasProperty("_SoftParticlesEnabled") && m.GetFloat("_SoftParticlesEnabled") > .5f))) return;
        var distance = -camera.worldToCameraMatrix.MultiplyPoint(Vector3.zero).z;
        camera.fieldOfView = 2 * Mathf.Atan(camera.orthographicSize / distance) * Mathf.Rad2Deg;
        camera.orthographic = false;
    }
}

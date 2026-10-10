using System.Linq;
using UnityEngine;

internal static class NovaLitTestTextures
{
    // Common color/mask/shadow fixtures need a diffuse surface. NOVA's original
    // default is white specular, which has zero diffuse after energy conservation.
    internal static void UseDiffuseDefaults(Material material)
    {
        if (!material.HasProperty("_LitWorkflowMode")) return;
        material.SetFloat("_LitWorkflowMode", 1f);
        material.SetFloat("_Metallic", 0f);
    }

    // Surface-map tests vary _BaseMapMode. Keep their albedo input identical across modes
    // now that the forward path correctly samples the matching base-map texture too.
    internal static void MatchBaseMapMode(Material material)
    {
        var mode = material.GetFloat("_BaseMapMode");
        var source = material.GetTexture("_BaseMap") as Texture2D;
        if (mode < 0.5f || source == null) return;
        var depth = Mathf.Max(1, Mathf.RoundToInt(material.GetFloat("_BaseMapSliceCount")));
        if (mode < 1.5f && material.GetTexture("_BaseMap2DArray") == null)
        {
            var texture = new Texture2DArray(source.width, source.height, depth, TextureFormat.RGBA32, false);
            texture.filterMode = source.filterMode;
            texture.wrapMode = source.wrapMode;
            for (var slice = 0; slice < depth; slice++) texture.SetPixels(source.GetPixels(), slice);
            texture.Apply();
            material.SetTexture("_BaseMap2DArray", texture);
        }
        else if (mode >= 1.5f && material.GetTexture("_BaseMap3D") == null)
        {
            var texture = new Texture3D(source.width, source.height, depth, TextureFormat.RGBA32, false);
            texture.filterMode = source.filterMode;
            texture.wrapMode = source.wrapMode;
            texture.SetPixels(Enumerable.Range(0, depth).SelectMany(_ => source.GetPixels()).ToArray());
            texture.Apply();
            material.SetTexture("_BaseMap3D", texture);
        }
    }
}

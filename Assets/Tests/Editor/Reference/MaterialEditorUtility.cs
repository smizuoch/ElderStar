using UnityEngine;

namespace Nova.Tests.Reference
{
    // Original NOVA 3.6.0 SetKeyword implementation; the GUI utility itself is internal.
    internal static class MaterialEditorUtility
    {
        public static void SetKeyword(Material material, string keyword, bool state)
        {
            if (state) material.EnableKeyword(keyword);
            else material.DisableKeyword(keyword);
        }
    }
}

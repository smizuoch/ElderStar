using System;
using Nova.Editor.Core.Scripts.Optimizer.Internal;
using UnityEditor;
using UnityEngine;

namespace Nova.Editor.Core.Scripts.Optimizer
{
    public static class OptimizedShaderReplacer
    {
        public static void Replace(Settings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var folder = settings.TargetFolderPath;
            if (!string.IsNullOrEmpty(folder) && (!AssetDatabase.IsValidFolder(folder) ||
                (folder != "Assets" && !folder.StartsWith("Assets/", StringComparison.Ordinal))))
                throw new ArgumentException("Choose a material folder under Assets.", nameof(settings));
            var guids = AssetDatabase.FindAssets("t:Material", new[] { string.IsNullOrEmpty(folder) ? "Assets" : folder });
            foreach (var guid in guids)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (material == null || material.shader == null ||
                    (material.shader.name != "Nova/Particles/UberLit" && material.shader.name != "Nova/Particles/UberUnlit")) continue;
                var render = (RenderType)material.GetFloat("_RenderType");
                if (render < RenderType.Opaque || render >= RenderType.Num) continue;
                var passes = render == RenderType.Opaque ? settings.OpaqueRequiredPasses
                    : render == RenderType.Cutout ? settings.CutoutRequiredPasses : settings.TransparentRequiredPasses;
                var name = OptimizedShaderUtility.GetOptimizedShaderName(material.shader.name, render, passes);
                var shader = Shader.Find(name);
                if (shader == null)
                {
                    Debug.LogWarning("Could not find optimized shader: " + name + ". Run OptimizedShaderGenerator.Generate() first.", material);
                    continue;
                }
                Undo.RecordObject(material, "Replace NOVA shader");
                var queue = material.renderQueue;
                var keywords = material.shaderKeywords;
                material.shader = shader;
                material.shaderKeywords = keywords;
                material.renderQueue = queue;
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssetIfDirty(material);
            }
        }

        public class Settings
        {
            public OptionalShaderPass OpaqueRequiredPasses { get; set; }
            public OptionalShaderPass TransparentRequiredPasses { get; set; }
            public OptionalShaderPass CutoutRequiredPasses { get; set; }
            public string TargetFolderPath { get; set; }
        }
    }
}

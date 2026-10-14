using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Nova.Editor.Core.Scripts.Optimizer.Internal;
using UnityEditor;
using UnityEngine;

namespace Nova.Editor.Core.Scripts.Optimizer
{
    public static class OptimizedShaderGenerator
    {
        // Original NOVA opaque depth/shadow keyword reduction. Keep deformation and pass-specific controls.
        private static readonly Regex OpaqueUnusedKeywordRegex = new(
            @"(?:" +
            @"_VERTEX_ALPHA_AS_TRANSITION_PROGRESS\b|" +
            @"_ALPHAMODULATE_ENABLED\b|" +
            @"_ALPHATEST_ENABLED\b|" +
            @"_BASE_MAP_MODE_2D\b|" +
            @"_BASE_MAP_MODE_2D_ARRAY\b|" +
            @"_BASE_MAP_MODE_3D\b|" +
            @"_BASE_MAP_CHANNEL_ENABLED\b|" +
            @"_BASE_MAP_TONE_MODE_TRITONE\b|" +
            @"_BASE_MAP_TONE_MODE_PENTONE\b|" +
            @"_BASE_SAMPLER_STATE_POINT_MIRROR\b|" +
            @"_BASE_SAMPLER_STATE_LINEAR_MIRROR\b|" +
            @"_BASE_SAMPLER_STATE_TRILINEAR_MIRROR\b|" +
            @"_TINT_AREA_ALL\b|" +
            @"_TINT_AREA_RIM\b|" +
            @"_TINT_COLOR_ENABLED\b|" +
            @"_TINT_MAP_ENABLED\b|" +
            @"_TINT_MAP_3D_ENABLED\b|" +
            @"_TINT_MAP_MODE_2D_ARRAY\b|" +
            @"_FLOW_MAP_ENABLED\b|" +
            @"_FLOW_MAP_TARGET_BASE\b|" +
            @"_FLOW_MAP_TARGET_TINT\b|" +
            @"_FLOW_MAP_TARGET_EMISSION\b|" +
            @"_FLOW_MAP_TARGET_ALPHA_TRANSITION\b|" +
            @"_PARALLAX_MAP_TARGET_BASE\b|" +
            @"_PARALLAX_MAP_TARGET_TINT\b|" +
            @"_PARALLAX_MAP_TARGET_EMISSION\b|" +
            @"_PARALLAX_MAP_MODE_2D\b|" +
            @"_PARALLAX_MAP_MODE_2D_ARRAY\b|" +
            @"_PARALLAX_MAP_MODE_3D\b|" +
            @"_GREYSCALE_ENABLED\b|" +
            @"_GRADIENT_MAP_ENABLED\b|" +
            @"_FADE_TRANSITION_ENABLED\b|" +
            @"_DISSOLVE_TRANSITION_ENABLED\b|" +
            @"_ALPHA_TRANSITION_MAP_MODE_2D\b|" +
            @"_ALPHA_TRANSITION_MAP_MODE_2D_ARRAY\b|" +
            @"_ALPHA_TRANSITION_MAP_MODE_3D\b|" +
            @"_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE\b|" +
            @"_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY\b|" +
            @"_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D\b|" +
            @"_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY\b|" +
            @"_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_3D\b|" +
            @"_EMISSION_AREA_ALL\b|" +
            @"_EMISSION_AREA_MAP\b|" +
            @"_EMISSION_AREA_ALPHA\b|" +
            @"_EMISSION_MAP_MODE_2D\b|" +
            @"_EMISSION_MAP_MODE_2D_ARRAY\b|" +
            @"_EMISSION_MAP_MODE_3D\b|" +
            @"_EMISSION_COLOR_COLOR\b|" +
            @"_EMISSION_COLOR_BASECOLOR\b|" +
            @"_EMISSION_COLOR_MAP\b|" +
            @"_TRANSPARENCY_BY_LUMINANCE\b|" +
            @"_TRANSPARENCY_BY_RIM\b|" +
            @"_SOFT_PARTICLES_ENABLED\b|" +
            @"_DEPTH_FADE_ENABLED\b" +
            @")\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static void Generate(string outputFolderPath)
        {
            if (string.IsNullOrWhiteSpace(outputFolderPath))
                throw new ArgumentException("Choose an output folder under Assets.", nameof(outputFolderPath));
            var output = Path.GetFullPath(outputFolderPath).Replace('\\', '/');
            var assets = Path.GetFullPath(Application.dataPath).Replace('\\', '/');
            if (!output.Equals(assets, StringComparison.OrdinalIgnoreCase) &&
                !output.StartsWith(assets + "/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Generated shaders must be saved under Assets.", nameof(outputFolderPath));
            var outputAssetPath = "Assets" + output.Substring(assets.Length);
            Directory.CreateDirectory(output);
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var baseName in new[] { "Nova/Particles/UberUnlit", "Nova/Particles/UberLit" })
                {
                    var shader = Shader.Find(baseName);
                    if (shader == null) throw new InvalidOperationException("Missing source shader: " + baseName);
                    var sourcePath = AssetDatabase.GetAssetPath(shader);
                    var source = File.ReadAllText(sourcePath);
                    var sourceFolder = Path.GetDirectoryName(sourcePath).Replace('\\', '/');
                    // Project-relative include paths also survive moving the generated folder.
                    source = Regex.Replace(source, @"#include\s+""([^""]+)""", match =>
                    {
                        var include = match.Groups[1].Value;
                        var localPath = sourceFolder + "/" + include;
                        return File.Exists(localPath) ? "#include \"" + localPath + "\"" : match.Value;
                    });
                    for (var render = RenderType.Opaque; render < RenderType.Num; render++)
                    for (var mask = 0; mask < 8; mask++)
                    {
                        var name = OptimizedShaderUtility.GetOptimizedShaderName(baseName, render, (OptionalShaderPass)mask);
                        var generated = source.Replace("Shader \"" + baseName + "\"", "Shader \"" + name + "\"");
                        generated = StripOptionalPasses(generated, (OptionalShaderPass)mask, render);
                        var fileName = Path.GetFileNameWithoutExtension(sourcePath) + name.Substring(("Hidden/" + baseName).Length) + ".shader";
                        File.WriteAllText(outputAssetPath + "/" + fileName, generated, new UTF8Encoding(false));
                    }
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static string StripOptionalPasses(string source, OptionalShaderPass required, RenderType renderType)
        {
            var starts = Regex.Matches(source, @"(?m)^\s*Pass\s*\{");
            var result = new StringBuilder(source);
            for (var index = starts.Count - 1; index >= 0; index--)
            {
                var match = starts[index];
                var opening = source.IndexOf('{', match.Index);
                var depth = 1;
                var end = opening + 1;
                while (end < source.Length && depth > 0)
                {
                    if (source[end] == '{') depth++;
                    if (source[end] == '}') depth--;
                    end++;
                }
                if (depth != 0) throw new InvalidDataException("Unbalanced shader pass.");
                var pass = source.Substring(match.Index, end - match.Index);
                var lightMode = Regex.Match(pass, @"""LightMode""\s*=\s*""([^""]+)""").Groups[1].Value;
                var optional = Enum.TryParse(lightMode, out OptionalShaderPass flag) && flag != OptionalShaderPass.None;
                if (optional && (required & flag) == 0)
                    result.Remove(match.Index, end - match.Index);
                // Opaque DepthNormals still samples normal maps, their selected texture
                // dimension, UVs and filtering. Color-depth pruning would break them.
                else if (optional && renderType == RenderType.Opaque && flag != OptionalShaderPass.DepthNormals)
                {
                    var optimized = Regex.Replace(pass, @"(?m)^[ \t]*#pragma[ \t]+shader_feature[^\r\n]*(?:\r?\n|$)",
                        line => OpaqueUnusedKeywordRegex.IsMatch(line.Value) ? "" : line.Value);
                    result.Remove(match.Index, end - match.Index).Insert(match.Index, optimized);
                }
            }
            return result.ToString();
        }
    }
}

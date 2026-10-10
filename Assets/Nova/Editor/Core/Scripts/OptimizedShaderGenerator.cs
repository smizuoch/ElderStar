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
        public static void Generate(string outputFolderPath)
        {
            if (string.IsNullOrWhiteSpace(outputFolderPath))
                throw new ArgumentException("Choose an output folder under Assets.", nameof(outputFolderPath));
            var output = Path.GetFullPath(outputFolderPath).Replace('\\', '/');
            var assets = Path.GetFullPath(Application.dataPath).Replace('\\', '/');
            if (!output.StartsWith(assets + "/", StringComparison.OrdinalIgnoreCase))
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
                        generated = StripOptionalPasses(generated, (OptionalShaderPass)mask);
                        var fileName = Path.GetFileNameWithoutExtension(sourcePath) + name.Substring(("Hidden/" + baseName).Length) + ".shader";
                        File.WriteAllText(outputAssetPath + "/" + fileName, generated, new UTF8Encoding(false));
                    }
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static string StripOptionalPasses(string source, OptionalShaderPass required)
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
                if (Enum.TryParse(lightMode, out OptionalShaderPass flag) && flag != OptionalShaderPass.None && (required & flag) == 0)
                    result.Remove(match.Index, end - match.Index);
            }
            return result.ToString();
        }
    }
}

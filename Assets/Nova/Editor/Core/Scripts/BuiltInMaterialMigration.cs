using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Nova.Editor.Core.Scripts
{
    public static class BuiltInMaterialMigration
    {
        public sealed class Entry
        {
            public string AssetPath { get; internal set; }
            public string ShaderName { get; internal set; }
            internal byte[] OriginalBytes;
            internal byte[] MigratedBytes;
            internal string BuiltInGuid;
        }

        public sealed class Plan
        {
            public IReadOnlyList<Entry> Entries { get; internal set; } = Array.Empty<Entry>();
            public IReadOnlyList<string> SkippedFiles { get; internal set; } = Array.Empty<string>();
        }

        private static readonly string[,] Shaders =
        {
            { "Nova/Particles/UberUnlit", "1bc5c6a70411243779ce486a98012125", "4651e33d77f6bedcc896127e121e2eb6" },
            { "Nova/Particles/UberLit", "65654f079f6f340b09f954eb6366ad5a", "30bca65b80428acc1b3b406943fb07ed" },
            { "Nova/UIParticles/UberUnlit", "72225468864e7e849bc3e630d1756259", "776e3569165e363519e50f5106284c3c" },
            { "Nova/UIParticles/UberLit", "5befadde7f2b6a9498dd225c31369d75", "6e936eaf27061e8b3f787102769ecc27" },
            { "Nova/Particles/Distortion", "f16a2dd74da8941d6a883c14b1f26710", "85397f9e441dca7ef1ef3b5d59ec92a7" },
            { "Hidden/Nova/Particles/ApplyDistortion", "4040547c0f97a42daaee2e90c4800bc5", "7319141515ca58ce5781606eef21f5d5" }
        };
        private static readonly Regex ShaderReference = new Regex(
            @"(?m)^\s*m_Shader:\s*\{fileID:\s*4800000,\s*guid:\s*(?<guid>[a-fA-F0-9]{32}),\s*type:\s*3\s*\}");

        /// <summary>Inspect text .mat assets under Assets without modifying them.</summary>
        public static Plan Preview(string folder)
        {
            folder = ValidateFolder(folder);
            var entries = new List<Entry>();
            var skipped = new List<string>();
            foreach (var path in MaterialFiles(folder).OrderBy(p => p, StringComparer.Ordinal))
            {
                var bytes = File.ReadAllBytes(path);
                var text = Encoding.UTF8.GetString(bytes);
                if (!text.TrimStart('\uFEFF').StartsWith("%YAML", StringComparison.Ordinal))
                {
                    skipped.Add(path);
                    continue;
                }
                var match = ShaderReference.Match(text);
                if (!match.Success) continue;
                for (var i = 0; i < Shaders.GetLength(0); i++)
                {
                    if (!string.Equals(match.Groups["guid"].Value, Shaders[i, 1], StringComparison.OrdinalIgnoreCase)) continue;
                    var replacement = (byte[])bytes.Clone();
                    // Replace the ASCII GUID in the original bytes; retain BOM, line endings and all other data.
                    Encoding.ASCII.GetBytes(Shaders[i, 2]).CopyTo(replacement,
                        Encoding.UTF8.GetByteCount(text.Substring(0, match.Groups["guid"].Index)));
                    entries.Add(new Entry { AssetPath = path, ShaderName = Shaders[i, 0], BuiltInGuid = Shaders[i, 2],
                        OriginalBytes = bytes, MigratedBytes = replacement });
                    break;
                }
            }
            return new Plan { Entries = entries.AsReadOnly(), SkippedFiles = skipped.AsReadOnly() };
        }

        /// <summary>Apply a reviewed plan. Return the backup folder; abort stale plans before any write.</summary>
        public static string Apply(Plan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (plan.Entries.Count == 0) return string.Empty;
            foreach (var entry in plan.Entries)
            {
                ValidateFolder(Path.GetDirectoryName(entry.AssetPath));
                if (!File.Exists(entry.AssetPath) || !File.ReadAllBytes(entry.AssetPath).SequenceEqual(entry.OriginalBytes))
                    throw new InvalidOperationException("Material changed after preview; preview again: " + entry.AssetPath);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(entry.BuiltInGuid));
                if (shader == null || shader.name != entry.ShaderName)
                    throw new InvalidOperationException("Install the Built-in NOVA shader before migration: " + entry.ShaderName);
            }
            var backup = Path.GetFullPath("Library/NovaBuiltInMigration/" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")
                + "-" + Guid.NewGuid().ToString("N"));
            foreach (var entry in plan.Entries)
            {
                var destination = Path.Combine(backup, entry.AssetPath);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.WriteAllBytes(destination, entry.OriginalBytes);
            }
            var written = new List<Entry>();
            try
            {
                foreach (var entry in plan.Entries)
                {
                    written.Add(entry);
                    File.WriteAllBytes(entry.AssetPath, entry.MigratedBytes);
                }
                foreach (var entry in plan.Entries) AssetDatabase.ImportAsset(entry.AssetPath, ImportAssetOptions.ForceUpdate);
            }
            catch
            {
                foreach (var entry in written) File.WriteAllBytes(entry.AssetPath, entry.OriginalBytes);
                foreach (var entry in written) AssetDatabase.ImportAsset(entry.AssetPath, ImportAssetOptions.ForceUpdate);
                throw;
            }
            return backup;
        }

        private static string ValidateFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder)) throw new ArgumentException("Choose a folder under Assets.", nameof(folder));
            folder = folder.Replace('\\', '/').TrimEnd('/');
            if ((folder != "Assets" && !folder.StartsWith("Assets/", StringComparison.Ordinal))
                || folder.Split('/').Any(part => part == ".." || part == ".") || !AssetDatabase.IsValidFolder(folder))
                throw new ArgumentException("Choose an existing folder under Assets.", nameof(folder));
            var current = folder;
            while (!string.IsNullOrEmpty(current))
            {
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new ArgumentException("Migration does not follow linked folders.", nameof(folder));
                current = Path.GetDirectoryName(current);
            }
            return folder;
        }

        private static IEnumerable<string> MaterialFiles(string folder)
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*.mat"))
                if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) == 0) yield return file.Replace('\\', '/');
            foreach (var child in Directory.EnumerateDirectories(folder))
                if ((File.GetAttributes(child) & FileAttributes.ReparsePoint) == 0)
                    foreach (var file in MaterialFiles(child)) yield return file;
        }
    }

    public sealed class BuiltInMaterialMigrationWindow : EditorWindow
    {
        private DefaultAsset folder;
        private BuiltInMaterialMigration.Plan plan;
        private Vector2 scroll;
        private string message;

        [MenuItem("Tools/NOVA/Built-in/Material Migration")]
        public static void Open() { GetWindow<BuiltInMaterialMigrationWindow>("NOVA Migration"); }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Migrate original NOVA 3.6 material shader references after installing the Built-in port. " +
                "Preview lists text .mat files only. Backups are saved under Library/NovaBuiltInMigration. " +
                "Generated optimized shaders require manual reassignment and regeneration.", MessageType.Info);
            EditorGUI.BeginChangeCheck();
            folder = (DefaultAsset)EditorGUILayout.ObjectField("Material folder", folder, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck()) plan = null;
            if (GUILayout.Button("Preview"))
            {
                plan = null;
                try { plan = BuiltInMaterialMigration.Preview(AssetDatabase.GetAssetPath(folder)); message = null; }
                catch (Exception error) { message = error.Message; }
            }
            if (plan != null)
            {
                EditorGUILayout.LabelField(plan.Entries.Count + " materials to migrate");
                scroll = EditorGUILayout.BeginScrollView(scroll);
                foreach (var entry in plan.Entries) EditorGUILayout.LabelField(entry.AssetPath, entry.ShaderName);
                foreach (var path in plan.SkippedFiles) EditorGUILayout.LabelField("Skipped non-text material", path);
                EditorGUILayout.EndScrollView();
                using (new EditorGUI.DisabledScope(plan.Entries.Count == 0))
                {
                    if (GUILayout.Button("Apply preview and create backups"))
                    {
                        try { message = "Backup: " + BuiltInMaterialMigration.Apply(plan); plan = null; }
                        catch (Exception error) { message = error.Message; }
                    }
                }
            }
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, MessageType.Info);
        }
    }
}

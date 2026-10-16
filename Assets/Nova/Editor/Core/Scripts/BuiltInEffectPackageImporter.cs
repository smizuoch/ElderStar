using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace Nova.Editor.Core.Scripts
{
    /// <summary>Import untouched effects using Unity's importer, omitting bundled original NOVA implementation assets.</summary>
    public static class BuiltInEffectPackageImporter
    {
        public sealed class Plan
        {
            public string[] AssetPaths { get; internal set; }
            public string[] ExcludedNovaPaths { get; internal set; }
            internal string PackagePath;
            internal byte[] PackageHash;
            internal string ImportPath;
        }

        [MenuItem("Assets/Import Package/ElderStar Effect...", false, 21)]
        [MenuItem("Tools/ElderStar/Built-in/Import Effect Package...")]
        public static void Open()
        {
            var path = EditorUtility.OpenFilePanel("Import ElderStar effect", "", "unitypackage");
            if (string.IsNullOrEmpty(path)) return;
            try { Import(Preview(path), true); }
            catch (Exception error) { Debug.LogException(error); }
        }

        public static Plan Preview(string packagePath)
        {
            if (string.IsNullOrEmpty(packagePath)) throw new ArgumentException("Select a unitypackage.", nameof(packagePath));
            packagePath = Path.GetFullPath(packagePath);
            var hash = Hash(packagePath);
            var catalog = Resources.Load<TextAsset>("NovaOriginalAssetGuids");
            if (catalog == null) throw new InvalidOperationException("NOVA original-asset catalog is missing. Reinstall the Built-in package.");
            var known = new HashSet<string>(catalog.text.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"))
                .Select(l => l.Split('\t')[0].Trim()), StringComparer.OrdinalIgnoreCase);
            var paths = new List<string>();
            var excluded = new List<string>();
            var folder = Path.GetFullPath("Temp/NovaEffectImports/" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            var importPath = Path.Combine(folder, Path.GetFileNameWithoutExtension(packagePath) + ".unitypackage");
            // Filter dependency entries in a temporary archive. Copy every retained TAR header and
            // payload verbatim: Unity still performs the actual asset import and conflict handling.
            using (var input = new GZipStream(File.OpenRead(packagePath), CompressionMode.Decompress))
            using (var output = new GZipStream(File.Create(importPath), System.IO.Compression.CompressionLevel.Optimal))
            {
                var header = new byte[512];
                var buffer = new byte[16384];
                while (ReadBlock(input, header))
                {
                    if (header.All(value => value == 0)) break;
                    var name = Field(header, 0, 100);
                    if (!Regex.IsMatch(name, @"^[a-fA-F0-9]{32}/?(?:[a-zA-Z0-9_.-]+)?$")
                        || (header[156] != 0 && header[156] != (byte)'0' && header[156] != (byte)'5'))
                        throw new InvalidDataException("Unsupported unitypackage entry: " + name);
                    var checksum = Octal(header, 148, 8);
                    long sum = 0;
                    for (var i = 0; i < 512; i++) sum += i >= 148 && i < 156 ? 32 : header[i];
                    if (sum != checksum) throw new InvalidDataException("Invalid unitypackage checksum.");
                    var size = Octal(header, 124, 12);
                    if (size > 2147483647) throw new InvalidDataException("Package entry exceeds 2 GB.");
                    var skip = known.Contains(name.Substring(0, 32));
                    var pathname = name.EndsWith("/pathname", StringComparison.Ordinal);
                    if (pathname && size > 32768) throw new InvalidDataException("Invalid package pathname length.");
                    if (!skip) output.Write(header, 0, header.Length);
                    using (var pathBytes = pathname ? new MemoryStream() : null)
                    {
                        var remaining = ((size + 511) / 512) * 512;
                        long offset = 0;
                        while (remaining > 0)
                        {
                            var count = input.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
                            if (count == 0) throw new EndOfStreamException("Truncated unitypackage.");
                            if (!skip) output.Write(buffer, 0, count);
                            if (pathname && offset < size) pathBytes.Write(buffer, 0, (int)Math.Min(count, size - offset));
                            remaining -= count;
                            offset += count;
                        }
                        if (pathname)
                        {
                            var path = Encoding.UTF8.GetString(pathBytes.ToArray()).TrimEnd('\0', '\r', '\n');
                            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.Contains('\\')
                                || path.Split('/').Any(part => part == ".." || part == "." || part.Contains(':')))
                                throw new InvalidDataException("Effect assets must be under Assets: " + path);
                            (skip ? excluded : paths).Add(path);
                        }
                    }
                }
                output.Write(new byte[1024], 0, 1024);
            }
            if (paths.Count + excluded.Count == 0) throw new InvalidDataException("No effect assets in this unitypackage.");
            return new Plan { PackagePath = packagePath, PackageHash = hash, ImportPath = importPath,
                AssetPaths = paths.ToArray(), ExcludedNovaPaths = excluded.ToArray() };
        }

        private static bool ReadBlock(Stream stream, byte[] buffer)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                var count = stream.Read(buffer, offset, buffer.Length - offset);
                if (count == 0)
                {
                    if (offset == 0) return false;
                    throw new EndOfStreamException("Truncated unitypackage header.");
                }
                offset += count;
            }
            return true;
        }

        private static string Field(byte[] data, int offset, int length) => Encoding.ASCII.GetString(data, offset, length).TrimEnd('\0', ' ');
        private static long Octal(byte[] data, int offset, int length) => Convert.ToInt64(Field(data, offset, length).Trim(), 8);

        public static void Import(Plan plan, bool interactive = true)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (plan.ImportPath == null || !Hash(plan.PackagePath).SequenceEqual(plan.PackageHash))
                throw new InvalidOperationException("Package changed after preview; select the package again.");
            // Original GUIDs resolve straight to the Built-in shaders. No material setup, migration,
            // reserialization or prefab modification is performed here.
            if (plan.ExcludedNovaPaths.Length > 0)
                Debug.Log("NOVA: using the installed Built-in implementation; omitted " + plan.ExcludedNovaPaths.Length +
                    " bundled original NOVA assets. Effect materials, prefabs and textures are imported unchanged.");
            if (interactive)
                AssetDatabase.ImportPackage(plan.ImportPath, true);
            else
            {
                // Synchronous entry point used by batch acceptance. Interactive use above is public API.
                var method = typeof(AssetDatabase).GetMethod("ImportPackageImmediately", BindingFlags.NonPublic | BindingFlags.Static);
                if (method == null) throw new NotSupportedException("Use the interactive effect importer in this Unity version.");
                if (!(bool)method.Invoke(null, new object[] { plan.ImportPath }))
                    throw new InvalidOperationException("Unity could not import the effect package.");
            }
        }

        private static byte[] Hash(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var algorithm = SHA256.Create()) return algorithm.ComputeHash(stream);
        }

    }
}

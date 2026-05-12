using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class NovaBuiltInPhase1Tests
{
    private const string PackageRoot = "Assets/Nova";
    private const string RuntimeRoot = "Assets/Nova/Runtime";
    private const string PortingSpecPath = "Documentation/BuiltInPortingSpec.md";

    private static readonly string[] OriginalShaderNames =
    {
        "Hidden/Nova/Particles/ApplyDistortion",
        "Nova/Particles/Distortion",
        "Nova/Particles/UberUnlit",
        "Nova/UIParticles/UberUnlit",
        "Nova/Particles/UberLit",
        "Nova/UIParticles/UberLit",
    };

    private static readonly IReadOnlyDictionary<string, string> OriginalCustomEditors =
        new Dictionary<string, string>
        {
            { "Nova/Particles/Distortion", "Nova.Editor.Core.Scripts.ParticlesDistortionGUI" },
            { "Nova/Particles/UberUnlit", "Nova.Editor.Core.Scripts.ParticlesUberUnlitGUI" },
            { "Nova/UIParticles/UberUnlit", "Nova.Editor.Core.Scripts.UIParticlesUberUnlitGUI" },
            { "Nova/Particles/UberLit", "Nova.Editor.Core.Scripts.ParticlesUberLitGUI" },
            { "Nova/UIParticles/UberLit", "Nova.Editor.Core.Scripts.UIParticlesUberLitGUI" },
        };

    [Test]
    public void BuiltInPackageDoesNotReferenceUrpRuntime()
    {
        Assert.That(Directory.Exists(PackageRoot), Is.True, $"Missing Built-in package root: {PackageRoot}");
        Assert.That(Directory.Exists(RuntimeRoot), Is.True, $"Missing Built-in runtime root: {RuntimeRoot}");

        var scannedFiles = Directory.EnumerateFiles(RuntimeRoot, "*", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(PackageRoot, "*.asmdef", SearchOption.AllDirectories))
            .Concat(new[] { Path.Combine(PackageRoot, "package.json") })
            .Where(File.Exists)
            .Where(path => !path.EndsWith(".meta", StringComparison.Ordinal))
            .Distinct()
            .ToArray();

        Assert.That(scannedFiles, Is.Not.Empty, "No Built-in runtime/package files were found to scan.");

        var forbiddenPatterns = new[]
        {
            "com.unity.render-pipelines.universal",
            "Unity.RenderPipelines.Universal",
            "UnityEngine.Rendering.Universal",
            "UnityEngine.Rendering.RenderGraphModule",
            "RenderGraph",
        };

        var violations = scannedFiles
            .SelectMany(path =>
            {
                var text = File.ReadAllText(path);
                return forbiddenPatterns
                    .Where(pattern => text.Contains(pattern, StringComparison.Ordinal))
                    .Select(pattern => $"{path}: {pattern}");
            })
            .ToArray();

        Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
    }

    [Test]
    public void OriginalShaderNamesAreDiscoverable()
    {
        foreach (var shaderName in OriginalShaderNames)
        {
            Assert.That(Shader.Find(shaderName), Is.Not.Null, shaderName);
        }
    }

    [Test]
    public void OriginalShaderPropertiesArePreserved()
    {
        foreach (var entry in ExpectedPropertiesByShader())
        {
            var shader = Shader.Find(entry.Key);
            Assert.That(shader, Is.Not.Null, entry.Key);

            var material = new Material(shader);
            try
            {
                var missing = entry.Value.Where(propertyName => !material.HasProperty(propertyName)).ToArray();
                Assert.That(missing, Is.Empty, $"{entry.Key} is missing material properties: {string.Join(", ", missing)}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }

    [Test]
    public void OriginalCustomEditorsArePreserved()
    {
        foreach (var entry in OriginalCustomEditors)
        {
            var editorType = FindType(entry.Value);
            Assert.That(editorType, Is.Not.Null, entry.Value);
            Assert.That(typeof(ShaderGUI).IsAssignableFrom(editorType), Is.True, $"{entry.Value} must remain a ShaderGUI.");

            var shader = Shader.Find(entry.Key);
            Assert.That(shader, Is.Not.Null, entry.Key);

            var path = AssetDatabase.GetAssetPath(shader);
            Assert.That(path, Is.Not.Empty, $"Could not find asset path for {entry.Key}");

            var shaderText = File.ReadAllText(path);
            Assert.That(shaderText, Does.Contain($"CustomEditor \"{entry.Value}\""), entry.Key);
        }
    }

    private static IReadOnlyDictionary<string, string[]> ExpectedPropertiesByShader()
    {
        var spec = File.ReadAllText(PortingSpecPath);
        var distortion = ExtractPropertyNames(spec, "Distortion properties");
        var uberUnlit = ExtractPropertyNames(spec, "UberUnlit base properties");
        var litAdditions = ExtractPropertyNames(spec, "Lit property additions");
        var uiAdditions = ExtractPropertyNames(spec, "UI property additions");

        return new Dictionary<string, string[]>
        {
            { "Hidden/Nova/Particles/ApplyDistortion", Array.Empty<string>() },
            { "Nova/Particles/Distortion", distortion },
            { "Nova/Particles/UberUnlit", uberUnlit },
            { "Nova/UIParticles/UberUnlit", Merge(uberUnlit, uiAdditions) },
            { "Nova/Particles/UberLit", Merge(uberUnlit, litAdditions) },
            { "Nova/UIParticles/UberLit", Merge(uberUnlit, litAdditions, uiAdditions) },
        };
    }

    private static string[] ExtractPropertyNames(string spec, string heading)
    {
        var headingMarker = $"#### {heading}";
        var start = spec.IndexOf(headingMarker, StringComparison.Ordinal);
        Assert.That(start, Is.GreaterThanOrEqualTo(0), $"Missing spec heading: {heading}");

        var nextHeading = spec.IndexOf("\n#### ", start + headingMarker.Length, StringComparison.Ordinal);
        if (nextHeading < 0)
        {
            nextHeading = spec.IndexOf("\n### ", start + headingMarker.Length, StringComparison.Ordinal);
        }

        var section = nextHeading < 0 ? spec.Substring(start) : spec.Substring(start, nextHeading - start);
        return Regex.Matches(section, @"_[A-Za-z][A-Za-z0-9_]*")
            .Select(match => match.Value)
            .Distinct()
            .ToArray();
    }

    private static string[] Merge(params string[][] propertySets)
    {
        return propertySets.SelectMany(properties => properties).Distinct().ToArray();
    }

    private static Type FindType(string fullName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(fullName, false))
            .FirstOrDefault(type => type != null);
    }
}

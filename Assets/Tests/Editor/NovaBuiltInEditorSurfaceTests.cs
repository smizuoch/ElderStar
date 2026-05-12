using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class NovaBuiltInEditorSurfaceTests
{
    private static readonly string[] GuiTypeNames =
    {
        "Nova.Editor.Core.Scripts.ParticlesDistortionGUI",
        "Nova.Editor.Core.Scripts.ParticlesUberUnlitGUI",
        "Nova.Editor.Core.Scripts.UIParticlesUberUnlitGUI",
        "Nova.Editor.Core.Scripts.ParticlesUberLitGUI",
        "Nova.Editor.Core.Scripts.UIParticlesUberLitGUI",
    };

    [Test]
    public void CustomEditorClassesExposeMaterialSetupSurfaceWithoutUrpReferences()
    {
        foreach (var typeName in GuiTypeNames)
        {
            var type = FindType(typeName);
            Assert.That(type, Is.Not.Null, typeName);
            Assert.That(typeof(ShaderGUI).IsAssignableFrom(type), Is.True, $"{typeName} must remain a ShaderGUI.");
            Assert.That(type.GetMethod("ValidateMaterial", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null, $"{typeName}.ValidateMaterial");
            Assert.That(type.GetMethod("AssignNewShaderToMaterial", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null, $"{typeName}.AssignNewShaderToMaterial");

            var assemblyReferences = type.Assembly.GetReferencedAssemblies().Select(reference => reference.Name).ToArray();
            Assert.That(assemblyReferences.Any(reference => reference.Contains("Universal", StringComparison.OrdinalIgnoreCase)), Is.False, typeName);
            Assert.That(assemblyReferences.Any(reference => reference.Contains("RenderGraph", StringComparison.OrdinalIgnoreCase)), Is.False, typeName);
        }
    }

    [Test]
    public void MaterialPostProcessorsPreserveOriginalPublicSetupMethods()
    {
        AssertPublicSetupMethod("Nova.Editor.Core.Scripts.ParticlesDistortionMaterialPostProcessor", "SetupMaterialKeywords");
        AssertPublicSetupMethod("Nova.Editor.Core.Scripts.ParticlesUberUnlitMaterialPostProcessor", "SetupMaterialKeywords");
        AssertPublicSetupMethod("Nova.Editor.Core.Scripts.ParticlesUberUnlitMaterialPostProcessor", "SetupMaterialBlendMode");
        AssertPublicSetupMethod("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor", "SetupMaterialKeywords");
        AssertPublicSetupMethod("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor", "SetupMaterialBlendMode");
    }

    [Test]
    public void OptimizerPublicApiSurfaceIsDiscoverable()
    {
        var optionalPassType = FindType("Nova.Editor.Core.Scripts.Optimizer.OptionalShaderPass");
        Assert.That(optionalPassType, Is.Not.Null);
        Assert.That(optionalPassType.IsEnum, Is.True);
        Assert.That(Enum.GetNames(optionalPassType), Is.EquivalentTo(new[] { "None", "DepthOnly", "DepthNormals", "ShadowCaster" }));

        var generator = FindType("Nova.Editor.Core.Scripts.Optimizer.OptimizedShaderGenerator");
        Assert.That(generator, Is.Not.Null);
        Assert.That(generator.GetMethod("Generate", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null), Is.Not.Null);

        var replacer = FindType("Nova.Editor.Core.Scripts.Optimizer.OptimizedShaderReplacer");
        Assert.That(replacer, Is.Not.Null);
        Assert.That(replacer.GetMethod("Replace", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);

        var settings = FindType("Nova.Editor.Core.Scripts.Optimizer.OptimizedShaderReplacer+Settings");
        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.GetProperty("OpaqueRequiredPasses"), Is.Not.Null);
        Assert.That(settings.GetProperty("TransparentRequiredPasses"), Is.Not.Null);
        Assert.That(settings.GetProperty("CutoutRequiredPasses"), Is.Not.Null);
        Assert.That(settings.GetProperty("TargetFolderPath"), Is.Not.Null);
    }

    private static void AssertPublicSetupMethod(string typeName, string methodName)
    {
        var type = FindType(typeName);
        Assert.That(type, Is.Not.Null, typeName);
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Material) }, null);
        Assert.That(method, Is.Not.Null, $"{typeName}.{methodName}(Material)");
    }

    private static Type FindType(string fullName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(fullName))
            .FirstOrDefault(type => type != null);
    }
}

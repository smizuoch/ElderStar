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
    public void ElderStarMenusRetainAllCommandsAndMatchingValidators()
    {
        var menus = TypeCache.GetMethodsWithAttribute<MenuItem>()
            .Where(method => method.DeclaringType.Assembly.GetName().Name == "Nova.Editor")
            .SelectMany(method => method.GetCustomAttributes<MenuItem>()).ToArray();
        var commands = menus.Where(menu => !menu.validate).Select(menu => menu.menuItem).ToArray();
        Assert.That(commands, Is.EquivalentTo(new[]
        {
            "Tools/ElderStar/RemoveUnusedReferences",
            "Tools/ElderStar/Built-in/Create Showcase Scene",
            "Assets/Import Package/ElderStar Effect...",
            "Tools/ElderStar/Built-in/Import Effect Package...",
            "Tools/ElderStar/Built-in/Material Migration",
            "Tools/ElderStar/Built-in/Match Original Lighting Settings",
            "Tools/ElderStar/Built-in/Automatic Initial Lighting Setup",
            "Tools/ElderStar/Rendering Diagnostics",
        }));
        Assert.That(menus.Where(menu => menu.validate).Select(menu => menu.menuItem), Is.EquivalentTo(new[]
        {
            "Tools/ElderStar/Built-in/Match Original Lighting Settings",
            "Tools/ElderStar/Built-in/Automatic Initial Lighting Setup",
        }));
    }

    [TestCase("Nova.Runtime.Core.Scripts.ScreenSpaceDistortion", "ElderStar/Screen Space Distortion")]
    [TestCase("Nova.Runtime.Core.Scripts.NovaDepthNormals", "ElderStar/Depth Normals")]
    [TestCase("Nova.Runtime.Core.Scripts.NovaAmbientLighting", "ElderStar/Global Ambient Lighting")]
    [TestCase("Nova.Runtime.UI.NovaCanvasLighting", "ElderStar/Canvas Lighting")]
    public void ComponentMenusUseElderStarWithoutRenamingTypes(string typeName, string menu)
    {
        var type = FindType(typeName);
        Assert.That(type, Is.Not.Null, typeName);
        Assert.That(type.GetCustomAttribute<AddComponentMenu>().componentMenu, Is.EqualTo(menu));
    }

    [TestCase("Tools/ElderStar/Built-in/Material Migration", "Nova.Editor.Core.Scripts.BuiltInMaterialMigrationWindow", "ElderStar Migration")]
    [TestCase("Tools/ElderStar/Rendering Diagnostics", "Nova.Editor.Core.Scripts.NovaShaderDiagnosticsWindow", "ElderStar Diagnostics")]
    public void ElderStarMenuOpensOriginalWindowType(string menu, string typeName, string title)
    {
        var type = FindType(typeName);
        Assert.That(type, Is.Not.Null, typeName);
        try
        {
            Assert.That(EditorApplication.ExecuteMenuItem(menu), Is.True, menu);
            var window = Resources.FindObjectsOfTypeAll(type).OfType<EditorWindow>().Single();
            Assert.That(window.titleContent.text, Is.EqualTo(title));
        }
        finally
        {
            foreach (var window in Resources.FindObjectsOfTypeAll(type).OfType<EditorWindow>()) window.Close();
        }
    }

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

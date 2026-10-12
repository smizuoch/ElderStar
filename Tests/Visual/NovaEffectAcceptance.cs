// Editor-only entry point for isolated external-package acceptance, not included in the package.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

public static class NovaEffectAcceptance
{
    public static void Import()
    {
        var args = Environment.GetCommandLineArgs();
        var package = args[Array.IndexOf(args, "-novaEffectPackage") + 1];
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("Nova.Editor.Core.Scripts.BuiltInEffectPackageImporter")).First(t => t != null);
        var plan = type.GetMethod("Preview").Invoke(null, new object[] { package });
        var paths = (string[])plan.GetType().GetProperty("AssetPaths").GetValue(plan);
        var excluded = (string[])plan.GetType().GetProperty("ExcludedNovaPaths").GetValue(plan);
        File.WriteAllLines("import-plan.txt", paths.Concat(new[] { "EXCLUDED NOVA IMPLEMENTATION:" }).Concat(excluded));
        type.GetMethod("Import").Invoke(null, new[] { plan, (object)false });
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/N2Studio/Prefabs/Fire001.prefab") == null)
            throw new InvalidOperationException("Effect import failed.");
        EditorApplication.Exit(0);
    }
}

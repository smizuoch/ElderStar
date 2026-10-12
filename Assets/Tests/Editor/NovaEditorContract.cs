using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nova.Tests.Editor
{
    // Also copied unchanged into an isolated original-NOVA project to capture the independent contract.
    public static class NovaEditorContract
    {
        [Serializable] public sealed class Snapshot { public string[] entries; }
        private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static string[] Read()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "Nova.Editor");
            var entries = new List<string>();
            foreach (var type in assembly.GetExportedTypes())
            {
                var prefix = type.FullName + " ";
                entries.Add(prefix + "type : " + TypeName(type.BaseType));
                foreach (var ctor in type.GetConstructors(Members).Where(Visible))
                    entries.Add(prefix + "constructor " + Signature(ctor));
                foreach (var method in type.GetMethods(Members).Where(Visible))
                    entries.Add(prefix + "method " + TypeName(method.ReturnType) + " " + Signature(method));
                foreach (var field in type.GetFields(Members).Where(f => f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly))
                    entries.Add(prefix + "field " + (field.IsStatic ? "static " : "") + TypeName(field.FieldType) + " " + field.Name +
                        (field.IsLiteral ? " = " + Convert.ToString(field.GetRawConstantValue(), CultureInfo.InvariantCulture) : ""));
                foreach (var property in type.GetProperties(Members).Where(p => p.GetAccessors(true).Any(Visible)))
                    entries.Add(prefix + "property " + TypeName(property.PropertyType) + " " + property.Name);
            }
            return entries.OrderBy(e => e, StringComparer.Ordinal).ToArray();
        }

        private static bool Visible(MethodBase m) => m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly;
        private static string TypeName(Type t) => t == null ? "none" : t.FullName ?? t.Name;
        private static string Signature(MethodBase m) => (m.IsPublic ? "public " : "protected ") +
            (m.IsStatic ? "static " : "") + m.Name + "(" + string.Join(",", m.GetParameters().Select(p =>
                TypeName(p.ParameterType) + (p.IsOptional ? "=" + Convert.ToString(p.DefaultValue, CultureInfo.InvariantCulture) : ""))) + ")";

        public static void Export()
        {
            var args = Environment.GetCommandLineArgs();
            var path = args[Array.IndexOf(args, "-novaContractOutput") + 1];
            File.WriteAllText(path, JsonUtility.ToJson(new Snapshot { entries = Read() }, true));
            Debug.Log("NOVA editor contract exported: " + Read().Length);
            EditorApplication.Exit(0);
        }
    }
}

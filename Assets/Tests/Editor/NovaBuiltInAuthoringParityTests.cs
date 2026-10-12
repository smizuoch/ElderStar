using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInAuthoringParityTests
    {
        [Test]
        public void EveryOriginalPublicAndProtectedEditorDeclarationRemainsAvailable()
        {
            var reference = JsonUtility.FromJson<NovaEditorContract.Snapshot>(File.ReadAllText(
                "Assets/Tests/Editor/Reference/EditorContract.json"));
            Assert.That(reference.entries.Length, Is.GreaterThan(300));
            Assert.That(reference.entries.Except(NovaEditorContract.Read()), Is.Empty,
                "Original method signatures, constants, enum values and property accessors must survive the port.");
        }

        [TestCase("tex_editor_icon_foldout_true")]
        [TestCase("tex_editor_icon_foldout_false")]
        public void OriginalInspectorFoldoutIconsAreIncluded(string resourceName)
        {
            var icon = Resources.Load<Texture2D>(resourceName);
            Assert.That(icon, Is.Not.Null, "The original Inspector draws this resource on every section header.");
            Assert.That(icon.width, Is.GreaterThan(0));
            Assert.That(icon.height, Is.GreaterThan(0));
        }
    }
}

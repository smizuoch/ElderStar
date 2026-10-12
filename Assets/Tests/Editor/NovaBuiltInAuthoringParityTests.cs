using NUnit.Framework;
using UnityEngine;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInAuthoringParityTests
    {
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

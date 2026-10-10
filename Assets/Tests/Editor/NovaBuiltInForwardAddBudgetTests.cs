using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInForwardAddBudgetTests
    {
        [TestCase("Nova/Particles/UberLit")]
        [TestCase("Nova/UIParticles/UberLit")]
        public void DirectionalCookieWithScreenShadowsFitsShaderModel35(string name)
        {
            var shader = Shader.Find(name);
            var material = new Material(shader);
            try
            {
                foreach (var keyword in new[] { "DIRECTIONAL_COOKIE", "SHADOWS_SCREEN",
                    "_ALPHA_TRANSITION_MAP_MODE_2D", "_BASE_MAP_MODE_2D", "_EMISSION_MAP_MODE_2D", "_PARALLAX_MAP_MODE_2D" })
                    material.EnableKeyword(keyword);
                ShaderUtil.CompilePass(material, material.FindPass("ForwardAdd"), true);
                Assert.That(ShaderUtil.GetShaderMessages(shader).Where(m => m.severity.ToString() == "Error")
                    .Select(m => m.message), Is.Empty, name);
            }
            finally { Object.DestroyImmediate(material); }
        }
    }
}

using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nova.Tests.Editor
{
    public sealed class NovaBuiltInForwardAddBudgetTests
    {
        [TestCase("Nova/Particles/UberLit", "")]
        [TestCase("Nova/UIParticles/UberLit", "")]
        [TestCase("Nova/Particles/UberLit", "FOG_LINEAR")]
        [TestCase("Nova/UIParticles/UberLit", "FOG_LINEAR")]
        [TestCase("Nova/Particles/UberLit", "FOG_EXP2")]
        [TestCase("Nova/UIParticles/UberLit", "FOG_EXP2")]
        public void DirectionalCookieWithScreenShadowsFitsShaderModel35(string name, string fog)
        {
            var shader = Shader.Find(name);
            var material = new Material(shader);
            try
            {
                if (fog.Length > 0) material.EnableKeyword(fog);
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

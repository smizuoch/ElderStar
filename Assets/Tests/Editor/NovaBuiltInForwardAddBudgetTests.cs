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
        public void AmbientFallbackHasNoUninitializedValueDiagnostic(string name)
        {
            var shader = Shader.Find(name);
            var material = new Material(shader);
            try
            {
                foreach (var keyword in new[] { "DIRECTIONAL", "_BASE_MAP_MODE_2D", "_ALPHA_TRANSITION_MAP_MODE_2D", "_EMISSION_MAP_MODE_2D", "_PARALLAX_MAP_MODE_2D" })
                    material.EnableKeyword(keyword);
                ShaderUtil.CompilePass(material, material.FindPass("ForwardBase"), true);
                Assert.That(ShaderUtil.GetShaderMessages(shader).Where(m => m.message.Contains("uninitialized"))
                    .Select(m => m.message), Is.Empty, name);
            }
            finally { Object.DestroyImmediate(material); }
        }

        [TestCase("Nova/Particles/UberLit", "DIRECTIONAL_COOKIE", "SHADOWS_SCREEN")]
        [TestCase("Nova/UIParticles/UberLit", "DIRECTIONAL_COOKIE", "SHADOWS_SCREEN")]
        [TestCase("Nova/Particles/UberLit", "POINT", "SHADOWS_CUBE")]
        [TestCase("Nova/UIParticles/UberLit", "POINT", "SHADOWS_CUBE")]
        [TestCase("Nova/Particles/UberLit", "POINT_COOKIE", "SHADOWS_CUBE")]
        [TestCase("Nova/UIParticles/UberLit", "POINT_COOKIE", "SHADOWS_CUBE")]
        [TestCase("Nova/Particles/UberLit", "SPOT", "SHADOWS_DEPTH")]
        [TestCase("Nova/UIParticles/UberLit", "SPOT", "SHADOWS_DEPTH")]
        public void AllInterpolatedFeaturesFitStereoShadowedForwardAdd(string name, string light, string shadow)
        {
            var shader = Shader.Find(name);
            var material = new Material(shader);
            try
            {
                foreach (var keyword in new[] { light, shadow, "STEREO_INSTANCING_ON", "FOG_LINEAR",
                    "UNITY_UI_CLIP_RECT", "_NORMAL_MAP_ENABLED", "_ADDITIONAL_LIGHTS", "_RECEIVE_SHADOWS_ENABLED",
                    "_BASE_MAP_MODE_2D_ARRAY", "_TINT_MAP_MODE_2D_ARRAY", "_TINT_AREA_RIM", "_FLOW_MAP_ENABLED",
                    "_FLOW_MAP_TARGET_BASE", "_PARALLAX_MAP_MODE_2D_ARRAY", "_PARALLAX_MAP_TARGET_BASE",
                    "_EMISSION_MAP_MODE_2D_ARRAY", "_EMISSION_AREA_MAP", "_EMISSION_COLOR_COLOR",
                    "_ALPHA_TRANSITION_MAP_MODE_2D_ARRAY", "_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY",
                    "_FADE_TRANSITION_ENABLED", "_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE", "_SOFT_PARTICLES_ENABLED",
                    "_TRANSPARENCY_BY_RIM", "_DEPTH_FADE_ENABLED" }) material.EnableKeyword(keyword);
                ShaderUtil.CompilePass(material, material.FindPass("ForwardAdd"), true);
                Assert.That(ShaderUtil.GetShaderMessages(shader).Where(m => m.severity.ToString() == "Error")
                    .Select(m => m.message), Is.Empty, name);
            }
            finally { Object.DestroyImmediate(material); }
        }

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

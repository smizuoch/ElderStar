using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class NovaBuiltInKeywordTests
{
    private static readonly string[] DistortionKeywords =
    {
        "_BASE_SAMPLER_STATE_LINEAR_MIRROR",
        "_BASE_SAMPLER_STATE_POINT_MIRROR",
        "_BASE_SAMPLER_STATE_TRILINEAR_MIRROR",
        "_DEPTH_FADE_ENABLED",
        "_DISSOLVE_TRANSITION_ENABLED",
        "_FADE_TRANSITION_ENABLED",
        "_FLOW_MAP_ENABLED",
        "_FLOW_MAP_TARGET_ALPHA_TRANSITION",
        "_FLOW_MAP_TARGET_BASE",
        "_SOFT_PARTICLES_ENABLED",
    };

    private static readonly string[] UberUnlitBaseKeywords =
    {
        "DEBUG_DISPLAY",
        "_ALPHAMODULATE_ENABLED",
        "_ALPHATEST_ENABLED",
        "_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE",
        "_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY",
        "_ALPHA_TRANSITION_MAP_MODE_2D",
        "_ALPHA_TRANSITION_MAP_MODE_2D_ARRAY",
        "_ALPHA_TRANSITION_MAP_MODE_3D",
        "_ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED",
        "_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D",
        "_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY",
        "_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_3D",
        "_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_RANDOM_ROW_SELECTION_ENABLED",
        "_BASE_MAP_CHANNEL_ENABLED",
        "_BASE_MAP_MODE_2D",
        "_BASE_MAP_MODE_2D_ARRAY",
        "_BASE_MAP_MODE_3D",
        "_BASE_MAP_RANDOM_ROW_SELECTION_ENABLED",
        "_BASE_MAP_TONE_MODE_PENTONE",
        "_BASE_MAP_TONE_MODE_TRITONE",
        "_BASE_SAMPLER_STATE_LINEAR_MIRROR",
        "_BASE_SAMPLER_STATE_POINT_MIRROR",
        "_BASE_SAMPLER_STATE_TRILINEAR_MIRROR",
        "_CASTING_PUNCTUAL_LIGHT_SHADOW",
        "_DEPTH_FADE_ENABLED",
        "_DISSOLVE_TRANSITION_ENABLED",
        "_EMISSION_AREA_ALL",
        "_EMISSION_AREA_ALPHA",
        "_EMISSION_AREA_MAP",
        "_EMISSION_COLOR_BASECOLOR",
        "_EMISSION_COLOR_COLOR",
        "_EMISSION_COLOR_MAP",
        "_EMISSION_MAP_MODE_2D",
        "_EMISSION_MAP_MODE_2D_ARRAY",
        "_EMISSION_MAP_MODE_3D",
        "_EMISSION_MAP_RANDOM_ROW_SELECTION_ENABLED",
        "_FADE_TRANSITION_ENABLED",
        "_FLOW_MAP_ENABLED",
        "_FLOW_MAP_TARGET_ALPHA_TRANSITION",
        "_FLOW_MAP_TARGET_BASE",
        "_FLOW_MAP_TARGET_EMISSION",
        "_FLOW_MAP_TARGET_TINT",
        "_GRADIENT_MAP_ENABLED",
        "_GREYSCALE_ENABLED",
        "_PARALLAX_MAP_MODE_2D",
        "_PARALLAX_MAP_MODE_2D_ARRAY",
        "_PARALLAX_MAP_MODE_3D",
        "_PARALLAX_MAP_RANDOM_ROW_SELECTION_ENABLED",
        "_PARALLAX_MAP_TARGET_BASE",
        "_PARALLAX_MAP_TARGET_EMISSION",
        "_PARALLAX_MAP_TARGET_TINT",
        "_SHADOW_CASTER_ALPHA_TEST_ENABLED",
        "_SOFT_PARTICLES_ENABLED",
        "_TINT_AREA_ALL",
        "_TINT_AREA_RIM",
        "_TINT_COLOR_ENABLED",
        "_TINT_MAP_3D_ENABLED",
        "_TINT_MAP_ENABLED",
        "_TINT_MAP_MODE_2D_ARRAY",
        "_TINT_MAP_RANDOM_ROW_SELECTION_ENABLED",
        "_TRANSPARENCY_BY_LUMINANCE",
        "_TRANSPARENCY_BY_RIM",
        "_VERTEX_ALPHA_AS_TRANSITION_PROGRESS",
        "_VERTEX_DEFORMATION_ENABLED",
    };

    private static readonly string[] LitKeywords =
    {
        "EVALUATE_SH_MIXED",
        "EVALUATE_SH_VERTEX",
        "_ADDITIONAL_LIGHTS",
        "_ADDITIONAL_LIGHTS_VERTEX",
        "_ADDITIONAL_LIGHT_SHADOWS",
        "_CLUSTER_LIGHT_LOOP",
        "_ENVIRONMENT_REFLECTIONS_ENABLED",
        "_FORWARD_PLUS",
        "_MAIN_LIGHT_SHADOWS",
        "_MAIN_LIGHT_SHADOWS_CASCADE",
        "_METALLIC_MAP_ENABLED",
        "_NORMAL_MAP_ENABLED",
        "_RECEIVE_SHADOWS_ENABLED",
        "_SHADOWS_SOFT",
        "_SMOOTHNESS_MAP_ENABLED",
        "_SPECULAR_HIGHLIGHTS_ENABLED",
        "_SPECULAR_MAP_ENABLED",
        "_SPECULAR_SETUP",
    };

    [Test]
    public void TargetShadersDeclareExpectedKeywordInventory()
    {
        AssertShaderDeclaresKeywords("Nova/Particles/Distortion", DistortionKeywords);
        AssertShaderDeclaresKeywords("Nova/Particles/UberUnlit", UberUnlitBaseKeywords);
        AssertShaderDeclaresKeywords("Nova/UIParticles/UberUnlit", UberUnlitBaseKeywords.Except(NonUiShadowKeywords()).Append("UNITY_UI_CLIP_RECT").ToArray());
        AssertShaderDeclaresKeywords("Nova/Particles/UberLit", UberUnlitBaseKeywords.Concat(LitKeywords).ToArray());
        AssertShaderDeclaresKeywords(
            "Nova/UIParticles/UberLit",
            UberUnlitBaseKeywords.Except(NonUiShadowKeywords()).Concat(LitKeywords.Except(new[] { "EVALUATE_SH_MIXED", "EVALUATE_SH_VERTEX" })).Append("UNITY_UI_CLIP_RECT").ToArray());
    }

    [Test]
    public void UiShadersDoNotDeclareNonUiOnlyKeywordInventory()
    {
        AssertShaderDoesNotDeclareKeywords("Nova/UIParticles/UberUnlit", NonUiShadowKeywords());
        AssertShaderDoesNotDeclareKeywords("Nova/UIParticles/UberLit", NonUiShadowKeywords().Concat(new[] { "EVALUATE_SH_MIXED", "EVALUATE_SH_VERTEX" }).ToArray());
    }

    private static string[] NonUiShadowKeywords()
    {
        return new[]
        {
            "_CASTING_PUNCTUAL_LIGHT_SHADOW",
            "_SHADOW_CASTER_ALPHA_TEST_ENABLED",
        };
    }

    private static void AssertShaderDeclaresKeywords(string shaderName, IReadOnlyCollection<string> expectedKeywords)
    {
        var declarations = DeclaredKeywords(shaderName);
        var missing = expectedKeywords.Where(keyword => !declarations.Contains(keyword)).ToArray();
        Assert.That(missing, Is.Empty, $"{shaderName} is missing keyword declarations: {string.Join(", ", missing)}");
    }

    private static void AssertShaderDoesNotDeclareKeywords(string shaderName, IReadOnlyCollection<string> unexpectedKeywords)
    {
        var declarations = DeclaredKeywords(shaderName);
        var present = unexpectedKeywords.Where(declarations.Contains).ToArray();
        Assert.That(present, Is.Empty, $"{shaderName} should not declare non-UI keywords: {string.Join(", ", present)}");
    }

    private static HashSet<string> DeclaredKeywords(string shaderName)
    {
        var shader = Shader.Find(shaderName);
        Assert.That(shader, Is.Not.Null, shaderName);

        var path = AssetDatabase.GetAssetPath(shader);
        Assert.That(path, Is.Not.Empty, shaderName);

        var text = File.ReadAllText(path);
        var declarations = new HashSet<string>(StringComparer.Ordinal);
        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith("#pragma", StringComparison.Ordinal) || !line.Contains("shader_feature", StringComparison.Ordinal) && !line.Contains("multi_compile", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var token in line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (token == "_" || token.StartsWith("#", StringComparison.Ordinal) || token.Contains("shader_feature") || token.Contains("multi_compile"))
                {
                    continue;
                }

                declarations.Add(token);
            }
        }

        return declarations;
    }
}

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NovaBuiltInMaterialSetupTests
{
    [Test]
    public void UberUnlitSetupAppliesRenderQueueTagsBlendAndDepthState()
    {
        using (var scope = NewMaterialScope("Nova/Particles/UberUnlit"))
        {
            var material = scope.Material;
            material.SetFloat("_RenderType", 0);
            material.SetFloat("_QueueOffset", 7);
            material.SetFloat("_TransparentBlendMode", 2);
            material.SetFloat("_ZWriteOverride", -1);
            material.SetFloat("_Cull", (float)CullMode.Front);
            material.SetFloat("_ZTest", (float)CompareFunction.Greater);
            SetupUberUnlit(material);

            Assert.That(material.renderQueue, Is.EqualTo((int)RenderQueue.Geometry + 7));
            Assert.That(material.GetTag("RenderType", false), Is.EqualTo("Opaque"));
            Assert.That(material.GetFloat("_BlendSrc"), Is.EqualTo((float)BlendMode.One));
            Assert.That(material.GetFloat("_BlendDst"), Is.EqualTo((float)BlendMode.Zero));
            Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(1));
            Assert.That(material.GetFloat("_Cull"), Is.EqualTo((float)CullMode.Front));
            Assert.That(material.GetFloat("_ZTest"), Is.EqualTo((float)CompareFunction.Greater));
            Assert.That(material.IsKeywordEnabled("_ALPHAMODULATE_ENABLED"), Is.False);
        }

        using (var scope = NewMaterialScope("Nova/Particles/UberUnlit"))
        {
            var material = scope.Material;
            material.SetFloat("_RenderType", 1);
            material.SetFloat("_QueueOffset", -3);
            material.SetFloat("_ZWriteOverride", 0);
            SetupUberUnlit(material);

            Assert.That(material.renderQueue, Is.EqualTo((int)RenderQueue.AlphaTest - 3));
            Assert.That(material.GetTag("RenderType", false), Is.EqualTo("TransparentCutout"));
            Assert.That(material.GetFloat("_BlendSrc"), Is.EqualTo((float)BlendMode.One));
            Assert.That(material.GetFloat("_BlendDst"), Is.EqualTo((float)BlendMode.Zero));
            Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(0));
            Assert.That(material.IsKeywordEnabled("_ALPHATEST_ENABLED"), Is.True);
        }

        using (var scope = NewMaterialScope("Nova/Particles/UberUnlit"))
        {
            var material = scope.Material;
            material.SetFloat("_RenderType", 2);
            material.SetFloat("_TransparentBlendMode", 1);
            material.SetFloat("_QueueOffset", 11);
            material.SetFloat("_BlendSrc", (float)BlendMode.One);
            material.SetFloat("_BlendDst", (float)BlendMode.Zero);
            material.SetFloat("_ZWriteOverride", 1);
            SetupUberUnlit(material);

            Assert.That(material.renderQueue, Is.EqualTo((int)RenderQueue.Transparent + 11));
            Assert.That(material.GetTag("RenderType", false), Is.EqualTo("Transparent"));
            Assert.That(material.GetFloat("_BlendSrc"), Is.EqualTo((float)BlendMode.SrcAlpha));
            Assert.That(material.GetFloat("_BlendDst"), Is.EqualTo((float)BlendMode.One));
            Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(1));
        }
    }

    [Test]
    public void UberUnlitFeatureKeywordSetupMatchesOriginalRepresentativeRules()
    {
        using (var scope = NewMaterialScope("Nova/Particles/UberUnlit"))
        {
            var material = scope.Material;
            var texture = MakeTexture(FilterMode.Trilinear);
            var secondTexture = MakeTexture(FilterMode.Bilinear);
            try
            {
                material.SetFloat("_RenderType", 1);
                material.SetFloat("_TransparentBlendMode", 2);
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_BaseMapMirrorSampling", 1);
                material.SetTexture("_BaseMap", texture);
                material.SetFloat("_BaseMapChannel", 1);
                material.SetFloat("_BaseMapToneMode", 1);
                material.SetFloat("_TintAreaMode", 2);
                material.SetFloat("_TintColorMode", 1);
                material.SetFloat("_FlowMapTarget", 15);
                material.SetTexture("_FlowMap", texture);
                material.SetFloat("_ParallaxMapMode", 0);
                material.SetFloat("_ParallaxMapTarget", 7);
                material.SetTexture("_ParallaxMap", texture);
                material.SetFloat("_ColorCorrectionMode", 1);
                material.SetFloat("_AlphaTransitionMode", 1);
                material.SetFloat("_AlphaTransitionMapMode", 0);
                material.SetTexture("_AlphaTransitionMap", texture);
                material.SetFloat("_AlphaTransitionSecondTextureBlendMode", 1);
                material.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 0);
                material.SetTexture("_AlphaTransitionMapSecondTexture", secondTexture);
                material.SetFloat("_EmissionAreaType", 1);
                material.SetFloat("_EmissionColorType", 1);
                material.SetFloat("_EmissionMapMode", 0);
                material.SetFloat("_RimTransparencyEnabled", 1);
                material.SetFloat("_LuminanceTransparencyEnabled", 1);
                material.SetFloat("_SoftParticlesEnabled", 1);
                material.SetFloat("_DepthFadeEnabled", 1);
                material.SetTexture("_VertexDeformationMap", texture);
                material.SetFloat("_ShadowCasterEnabled", 1);
                material.SetFloat("_ShadowCasterAlphaTestEnabled", 1);
                SetupUberUnlit(material);

                AssertKeywordState(
                    material,
                    enabled: new[]
                    {
                        "_ALPHATEST_ENABLED",
                        "_BASE_MAP_MODE_2D",
                        "_BASE_MAP_CHANNEL_ENABLED",
                        "_BASE_MAP_TONE_MODE_TRITONE",
                        "_BASE_SAMPLER_STATE_TRILINEAR_MIRROR",
                        "_TINT_MAP_ENABLED",
                        "_TINT_AREA_RIM",
                        "_FLOW_MAP_TARGET_BASE",
                        "_FLOW_MAP_TARGET_TINT",
                        "_FLOW_MAP_TARGET_ALPHA_TRANSITION",
                        "_FLOW_MAP_TARGET_EMISSION",
                        "_PARALLAX_MAP_MODE_2D",
                        "_PARALLAX_MAP_TARGET_BASE",
                        "_PARALLAX_MAP_TARGET_TINT",
                        "_PARALLAX_MAP_TARGET_EMISSION",
                        "_GREYSCALE_ENABLED",
                        "_FADE_TRANSITION_ENABLED",
                        "_ALPHA_TRANSITION_MAP_MODE_2D",
                        "_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE",
                        "_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D",
                        "_EMISSION_AREA_ALL",
                        "_EMISSION_COLOR_BASECOLOR",
                        "_EMISSION_MAP_MODE_2D",
                        "_TRANSPARENCY_BY_RIM",
                        "_TRANSPARENCY_BY_LUMINANCE",
                        "_SOFT_PARTICLES_ENABLED",
                        "_DEPTH_FADE_ENABLED",
                        "_VERTEX_DEFORMATION_ENABLED",
                        "_SHADOW_CASTER_ALPHA_TEST_ENABLED",
                    },
                    disabled: new[]
                    {
                        "_BASE_MAP_MODE_2D_ARRAY",
                        "_BASE_MAP_MODE_3D",
                        "_BASE_MAP_TONE_MODE_PENTONE",
                        "_TINT_COLOR_ENABLED",
                        "_TINT_MAP_3D_ENABLED",
                        "_TINT_MAP_MODE_2D_ARRAY",
                        "_ALPHAMODULATE_ENABLED",
                        "_GRADIENT_MAP_ENABLED",
                        "_DISSOLVE_TRANSITION_ENABLED",
                        "_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY",
                    });
                Assert.That(material.GetShaderPassEnabled("ShadowCaster"), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(secondTexture);
            }
        }

        using (var scope = NewMaterialScope("Nova/Particles/UberUnlit"))
        {
            var material = scope.Material;
            material.SetFloat("_RenderType", 2);
            material.SetFloat("_TransparentBlendMode", 2);
            SetupUberUnlit(material);

            AssertKeywordState(
                material,
                enabled: new[] { "_ALPHAMODULATE_ENABLED" },
                disabled: new[] { "_ALPHATEST_ENABLED" });
        }

        using (var scope = NewMaterialScope("Nova/Particles/UberUnlit"))
        {
            var material = scope.Material;
            var texture = MakeTexture(FilterMode.Point);
            var textureArray = new Texture2DArray(1, 1, 1, TextureFormat.RGBA32, false);
            var texture3D = new Texture3D(1, 1, 1, TextureFormat.RGBA32, false);
            try
            {
                material.SetFloat("_BaseMapMode", 1);
                material.SetTexture("_BaseMap2DArray", textureArray);
                material.SetFloat("_BaseMapRandomRowSelectionEnabled", 1);
                material.SetFloat("_TintColorMode", 3);
                material.SetTexture("_TintMap2DArray", textureArray);
                material.SetFloat("_TintMapRandomRowSelectionEnabled", 1);
                material.SetFloat("_ParallaxMapMode", 2);
                material.SetTexture("_ParallaxMap3D", texture3D);
                material.SetFloat("_ParallaxMapRandomRowSelectionEnabled", 1);
                material.SetFloat("_ColorCorrectionMode", 2);
                material.SetFloat("_AlphaTransitionMode", 2);
                material.SetFloat("_AlphaTransitionMapMode", 2);
                material.SetTexture("_AlphaTransitionMap3D", texture3D);
                material.SetFloat("_AlphaTransitionMapRandomRowSelectionEnabled", 1);
                material.SetFloat("_AlphaTransitionSecondTextureBlendMode", 2);
                material.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 1);
                material.SetTexture("_AlphaTransitionMapSecondTexture2DArray", textureArray);
                material.SetFloat("_AlphaTransitionMapSecondTextureRandomRowSelectionEnabled", 1);
                material.SetFloat("_EmissionAreaType", 2);
                material.SetFloat("_EmissionColorType", 2);
                material.SetFloat("_EmissionMapMode", 2);
                material.SetFloat("_EmissionMapRandomRowSelectionEnabled", 1);
                material.SetFloat("_BaseMapToneMode", 2);
                material.SetFloat("_BaseMapMirrorSampling", 1);
                material.SetTexture("_BaseMap", texture);
                SetupUberUnlit(material);

                AssertKeywordState(
                    material,
                    enabled: new[]
                    {
                        "_BASE_MAP_MODE_2D_ARRAY",
                        "_BASE_MAP_RANDOM_ROW_SELECTION_ENABLED",
                        "_BASE_MAP_TONE_MODE_PENTONE",
                        "_TINT_MAP_MODE_2D_ARRAY",
                        "_TINT_MAP_RANDOM_ROW_SELECTION_ENABLED",
                        "_PARALLAX_MAP_MODE_3D",
                        "_PARALLAX_MAP_RANDOM_ROW_SELECTION_ENABLED",
                        "_GRADIENT_MAP_ENABLED",
                        "_DISSOLVE_TRANSITION_ENABLED",
                        "_ALPHA_TRANSITION_MAP_MODE_3D",
                        "_ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED",
                        "_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY",
                        "_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY",
                        "_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_RANDOM_ROW_SELECTION_ENABLED",
                        "_EMISSION_AREA_MAP",
                        "_EMISSION_COLOR_MAP",
                        "_EMISSION_MAP_MODE_3D",
                        "_EMISSION_MAP_RANDOM_ROW_SELECTION_ENABLED",
                    },
                    disabled: new[]
                    {
                        "_BASE_MAP_MODE_2D",
                        "_BASE_MAP_MODE_3D",
                        "_BASE_MAP_TONE_MODE_TRITONE",
                        "_GREYSCALE_ENABLED",
                        "_FADE_TRANSITION_ENABLED",
                        "_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE",
                        "_EMISSION_AREA_ALL",
                        "_EMISSION_AREA_ALPHA",
                        "_EMISSION_COLOR_BASECOLOR",
                    });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(textureArray);
                UnityEngine.Object.DestroyImmediate(texture3D);
            }
        }
    }

    [Test]
    public void DistortionSetupTogglesRepresentativeKeywords()
    {
        using (var scope = NewMaterialScope("Nova/Particles/Distortion"))
        {
            var material = scope.Material;
            var texture = MakeTexture(FilterMode.Point);
            try
            {
                material.SetFloat("_BaseMapMirrorSampling", 1);
                material.SetTexture("_BaseMap", texture);
                material.SetTexture("_FlowMap", texture);
                material.SetFloat("_FlowMapTarget", 3);
                material.SetTexture("_AlphaTransitionMap", texture);
                material.SetFloat("_AlphaTransitionMode", 2);
                material.SetFloat("_SoftParticlesEnabled", 1);
                material.SetFloat("_DepthFadeEnabled", 1);
                SetupDistortion(material);

                AssertKeywordState(
                    material,
                    enabled: new[]
                    {
                        "_BASE_SAMPLER_STATE_POINT_MIRROR",
                        "_FLOW_MAP_TARGET_BASE",
                        "_FLOW_MAP_TARGET_ALPHA_TRANSITION",
                        "_DISSOLVE_TRANSITION_ENABLED",
                        "_SOFT_PARTICLES_ENABLED",
                        "_DEPTH_FADE_ENABLED",
                    },
                    disabled: new[]
                    {
                        "_BASE_SAMPLER_STATE_LINEAR_MIRROR",
                        "_BASE_SAMPLER_STATE_TRILINEAR_MIRROR",
                        "_FLOW_MAP_ENABLED",
                        "_FADE_TRANSITION_ENABLED",
                    });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }

    [Test]
    public void UberLitSetupTogglesRepresentativeLitKeywords()
    {
        using (var scope = NewMaterialScope("Nova/Particles/UberLit"))
        {
            var material = scope.Material;
            var texture = MakeTexture(FilterMode.Bilinear);
            try
            {
                material.SetFloat("_BaseMapMode", 0);
                material.SetFloat("_LitWorkflowMode", 0);
                material.SetFloat("_LitReceiveShadows", 1);
                material.SetFloat("_SpecularHighlights", 1);
                material.SetFloat("_EnvironmentReflections", 1);
                material.SetTexture("_NormalMap", texture);
                material.SetTexture("_SpecularMap", texture);
                material.SetTexture("_MetallicMap", texture);
                material.SetTexture("_SmoothnessMap", texture);
                SetupUberLit(material);

                AssertKeywordState(
                    material,
                    enabled: new[]
                    {
                        "_SPECULAR_SETUP",
                        "_RECEIVE_SHADOWS_ENABLED",
                        "_SPECULAR_HIGHLIGHTS_ENABLED",
                        "_ENVIRONMENT_REFLECTIONS_ENABLED",
                        "_NORMAL_MAP_ENABLED",
                        "_SPECULAR_MAP_ENABLED",
                        "_METALLIC_MAP_ENABLED",
                        "_SMOOTHNESS_MAP_ENABLED",
                    },
                    disabled: Array.Empty<string>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }

    [Test]
    public void UiMaterialSetupPreservesStencilAndClipSurface()
    {
        using (var scope = NewMaterialScope("Nova/UIParticles/UberUnlit"))
        {
            var material = scope.Material;
            material.SetFloat("_StencilComp", 3);
            material.SetFloat("_Stencil", 7);
            material.SetFloat("_StencilOp", 2);
            material.SetFloat("_StencilWriteMask", 127);
            material.SetFloat("_StencilReadMask", 63);
            material.SetFloat("_ColorMask", 11);
            material.EnableKeyword("UNITY_UI_CLIP_RECT");
            material.SetFloat("_RenderType", 0);
            material.SetFloat("_ZWriteOverride", -1);
            SetupUberUnlit(material);

            Assert.That(material.GetFloat("_StencilComp"), Is.EqualTo(3));
            Assert.That(material.GetFloat("_Stencil"), Is.EqualTo(7));
            Assert.That(material.GetFloat("_StencilOp"), Is.EqualTo(2));
            Assert.That(material.GetFloat("_StencilWriteMask"), Is.EqualTo(127));
            Assert.That(material.GetFloat("_StencilReadMask"), Is.EqualTo(63));
            Assert.That(material.GetFloat("_ColorMask"), Is.EqualTo(11));
            Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(0));
            Assert.That(material.IsKeywordEnabled("UNITY_UI_CLIP_RECT"), Is.True);
            Assert.That(material.IsKeywordEnabled("_SHADOW_CASTER_ALPHA_TEST_ENABLED"), Is.False);
            Assert.That(material.GetShaderPassEnabled("ShadowCaster"), Is.False);
        }
    }

    private static MaterialScope NewMaterialScope(string shaderName)
    {
        var shader = Shader.Find(shaderName);
        Assert.That(shader, Is.Not.Null, shaderName);
        return new MaterialScope(new Material(shader));
    }

    private static Texture2D MakeTexture(FilterMode filterMode)
    {
        var texture = new Texture2D(1, 1);
        texture.filterMode = filterMode;
        return texture;
    }

    private static void SetupDistortion(Material material)
    {
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesDistortionMaterialPostProcessor", "SetupMaterialKeywords", material);
    }

    private static void SetupUberUnlit(Material material)
    {
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberUnlitMaterialPostProcessor", "SetupMaterialKeywords", material);
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberUnlitMaterialPostProcessor", "SetupMaterialBlendMode", material);
    }

    private static void SetupUberLit(Material material)
    {
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor", "SetupMaterialKeywords", material);
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberLitMaterialPostProcessor", "SetupMaterialBlendMode", material);
    }

    private static void InvokeStatic(string typeName, string methodName, Material material)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(typeName))
            .FirstOrDefault(found => found != null);
        Assert.That(type, Is.Not.Null, typeName);

        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        Assert.That(method, Is.Not.Null, $"{typeName}.{methodName}");
        method.Invoke(null, new object[] { material });
    }

    private static void AssertKeywordState(Material material, string[] enabled, string[] disabled)
    {
        foreach (var keyword in enabled)
        {
            Assert.That(material.IsKeywordEnabled(keyword), Is.True, $"{keyword} should be enabled.");
        }

        foreach (var keyword in disabled)
        {
            Assert.That(material.IsKeywordEnabled(keyword), Is.False, $"{keyword} should be disabled.");
        }
    }

    private readonly struct MaterialScope : IDisposable
    {
        public MaterialScope(Material material)
        {
            Material = material;
        }

        public Material Material { get; }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(Material);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class NovaBuiltInUIParticlesUberUnlitCompileTests
{
    private const string ShaderName = "Nova/UIParticles/UberUnlit";
    private const string ShaderPath = "Assets/Nova/Runtime/Core/Shaders/UIParticlesUberUnlit.shader";

    private static readonly string[] ForbiddenTokens =
    {
        "Packages/com.unity.render-pipelines.universal",
        "UnityEngine.Rendering.Universal",
        "UnityEngine.Rendering.RenderGraphModule",
        "RenderPipeline\" = \"UniversalPipeline\"",
        "UniversalForward",
        "UniversalFragmentPBR",
        "DeclareDepthTexture.hlsl",
        "ProbeVolumeVariants.hlsl",
        "FoveatedRenderingKeywords.hlsl",
        "ScriptableRendererFeature",
        "ScriptableRenderPass",
        "RenderGraph",
    };

    [Test]
    public void UIParticlesUberUnlitForwardPassUsesBuiltInUiSurface()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Contain("\"LightMode\" = \"ForwardBase\""));
        Assert.That(shaderText, Does.Contain("Blend [_BlendSrc] [_BlendDst]"));
        Assert.That(shaderText, Does.Contain("ZWrite [_ZWrite]"));
        Assert.That(shaderText, Does.Contain("Cull [_Cull]"));
        Assert.That(shaderText, Does.Contain("ZTest [_ZTest]"));
        Assert.That(shaderText, Does.Contain("ColorMask [_ColorMask]"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInCore.cginc"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInTexture.cginc"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInUI.cginc"));
        Assert.That(shaderText, Does.Contain("BuiltIn/NovaBuiltInUberUnlit.cginc"));
        Assert.That(shaderText, Does.Contain("NovaObjectToClipPos"));
        Assert.That(shaderText, Does.Contain("SampleBaseMap"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX2D(_BaseMap"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX2D_ARRAY(_BaseMap2DArray"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX3D_LOD(_BaseMap3D"));
        Assert.That(shaderText, Does.Contain("NovaApplyBaseMapChannel"));
        Assert.That(shaderText, Does.Contain("NovaApplyBaseMapTone"));
        Assert.That(shaderText, Does.Contain("NovaApplyTintColor"));
        Assert.That(shaderText, Does.Contain("SampleTintMap"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX2D(_TintMap"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX2D_ARRAY(_TintMap2DArray"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX3D_LOD(_TintMap3D"));
        Assert.That(shaderText, Does.Contain("NovaFlowMapUvOffset"));
        Assert.That(shaderText, Does.Contain("SampleParallaxMap"));
        Assert.That(shaderText, Does.Contain("NovaParallaxOffset"));
        Assert.That(shaderText, Does.Contain("NovaLuminance"));
        Assert.That(shaderText, Does.Contain("NOVA_SAMPLE_TEX2D(_GradientMap"));
        Assert.That(shaderText, Does.Contain("NOVA_DECLARE_TEX2D(_AlphaTransitionMap"));
        Assert.That(shaderText, Does.Contain("NOVA_DECLARE_TEX2D_ARRAY(_AlphaTransitionMap2DArray"));
        Assert.That(shaderText, Does.Contain("NOVA_DECLARE_TEX3D(_AlphaTransitionMap3D"));
        Assert.That(shaderText, Does.Contain("NOVA_DECLARE_TEX2D(_AlphaTransitionMapSecondTexture"));
        Assert.That(shaderText, Does.Contain("NOVA_DECLARE_TEX2D_ARRAY(_AlphaTransitionMapSecondTexture2DArray"));
        Assert.That(shaderText, Does.Contain("NOVA_DECLARE_TEX3D(_AlphaTransitionMapSecondTexture3D"));
        Assert.That(shaderText, Does.Contain("SampleAlphaTransitionMap"));
        Assert.That(shaderText, Does.Contain("SampleAlphaTransitionSecondMap"));
        Assert.That(shaderText, Does.Contain("NovaAlphaTransitionMapProgress"));
        Assert.That(shaderText, Does.Contain("NovaAlphaTransitionSecondMapProgress"));
        Assert.That(shaderText, Does.Contain("NovaAlphaTransitionFactor"));
        Assert.That(shaderText, Does.Contain("NovaBlendAlphaTransitionFactors"));
        Assert.That(shaderText, Does.Contain("NovaModulateAlphaTransitionProgress"));
        Assert.That(shaderText, Does.Contain("NOVA_DECLARE_TEX2D(_EmissionMap"));
        Assert.That(shaderText, Does.Contain("NOVA_DECLARE_TEX2D_ARRAY(_EmissionMap2DArray"));
        Assert.That(shaderText, Does.Contain("NOVA_DECLARE_TEX3D(_EmissionMap3D"));
        Assert.That(shaderText, Does.Contain("NOVA_DECLARE_TEX2D(_EmissionColorRamp"));
        Assert.That(shaderText, Does.Contain("SampleEmissionMap"));
        Assert.That(shaderText, Does.Contain("NovaEmissionMapProgress"));
        Assert.That(shaderText, Does.Contain("ApplyEmissionColor"));
        Assert.That(shaderText, Does.Contain("NovaUIClipAlpha"));
        Assert.That(shaderText, Does.Contain("_ALPHATEST_ENABLED"));
        Assert.That(shaderText, Does.Contain("clip("));
        Assert.That(shaderText, Does.Contain("CustomEditor \"Nova.Editor.Core.Scripts.UIParticlesUberUnlitGUI\""));
    }

    [Test]
    public void UIParticlesUberUnlitDeclaresUiStencilAndColorMaskState()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Contain("Stencil"));
        Assert.That(shaderText, Does.Contain("Ref [_Stencil]"));
        Assert.That(shaderText, Does.Contain("Comp [_StencilComp]"));
        Assert.That(shaderText, Does.Contain("Pass [_StencilOp]"));
        Assert.That(shaderText, Does.Contain("ReadMask [_StencilReadMask]"));
        Assert.That(shaderText, Does.Contain("WriteMask [_StencilWriteMask]"));
        Assert.That(shaderText, Does.Contain("ColorMask [_ColorMask]"));
    }

    [Test]
    public void UIParticlesUberUnlitPhase51PassTopologyStaysUiOnly()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(CountOccurrences(shaderText, "\n        Pass\n        {"), Is.EqualTo(1));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"ShadowCaster\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"DepthOnly\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"DepthNormals\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"SceneSelectionPass\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"Picking\""));
        var keywordDeclarations = string.Join("\n", shaderText
            .Split('\n')
            .Where(line => line.Contains("#pragma", StringComparison.Ordinal) && line.Contains("shader_feature", StringComparison.Ordinal)));
        Assert.That(keywordDeclarations, Does.Not.Contain("_CASTING_PUNCTUAL_LIGHT_SHADOW"));
        Assert.That(keywordDeclarations, Does.Not.Contain("_SHADOW_CASTER_ALPHA_TEST_ENABLED"));
        Assert.That(shaderText, Does.Contain("UNITY_UI_CLIP_RECT"));
    }

    [Test]
    public void UIParticlesUberUnlitPhase52ABaseTextureKeywordsStayUiScoped()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Contain("_BASE_MAP_MODE_2D_ARRAY"));
        Assert.That(shaderText, Does.Contain("_BASE_MAP_MODE_3D"));
        Assert.That(shaderText, Does.Contain("_BASE_MAP_CHANNEL_ENABLED"));
        Assert.That(shaderText, Does.Contain("_BASE_MAP_RANDOM_ROW_SELECTION_ENABLED"));
        Assert.That(shaderText, Does.Contain("_BASE_MAP_TONE_MODE_TRITONE"));
        Assert.That(shaderText, Does.Contain("_BASE_MAP_TONE_MODE_PENTONE"));
        Assert.That(shaderText, Does.Contain("_BASE_SAMPLER_STATE_POINT_MIRROR"));
        Assert.That(shaderText, Does.Contain("_BASE_SAMPLER_STATE_LINEAR_MIRROR"));
        Assert.That(shaderText, Does.Contain("_BASE_SAMPLER_STATE_TRILINEAR_MIRROR"));
        Assert.That(shaderText, Does.Contain("NovaApplyBaseMirror"));
        Assert.That(shaderText, Does.Contain("NovaBaseMapProgress"));
    }

    [Test]
    public void UIParticlesUberUnlitPhase52BTintFlowParallaxColorKeywordsStayUiScoped()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Contain("_TINT_COLOR_ENABLED"));
        Assert.That(shaderText, Does.Contain("_TINT_MAP_ENABLED"));
        Assert.That(shaderText, Does.Contain("_TINT_MAP_MODE_2D_ARRAY"));
        Assert.That(shaderText, Does.Contain("_TINT_MAP_3D_ENABLED"));
        Assert.That(shaderText, Does.Contain("_TINT_MAP_RANDOM_ROW_SELECTION_ENABLED"));
        Assert.That(shaderText, Does.Contain("_TINT_AREA_ALL"));
        Assert.That(shaderText, Does.Contain("_TINT_AREA_RIM"));
        Assert.That(shaderText, Does.Contain("_FLOW_MAP_ENABLED"));
        Assert.That(shaderText, Does.Contain("_FLOW_MAP_TARGET_BASE"));
        Assert.That(shaderText, Does.Contain("_FLOW_MAP_TARGET_TINT"));
        Assert.That(shaderText, Does.Contain("_PARALLAX_MAP_MODE_2D"));
        Assert.That(shaderText, Does.Contain("_PARALLAX_MAP_MODE_2D_ARRAY"));
        Assert.That(shaderText, Does.Contain("_PARALLAX_MAP_MODE_3D"));
        Assert.That(shaderText, Does.Contain("_PARALLAX_MAP_TARGET_BASE"));
        Assert.That(shaderText, Does.Contain("_PARALLAX_MAP_TARGET_TINT"));
        Assert.That(shaderText, Does.Contain("_GREYSCALE_ENABLED"));
        Assert.That(shaderText, Does.Contain("_GRADIENT_MAP_ENABLED"));
        Assert.That(shaderText, Does.Contain("UNITY_UI_CLIP_RECT"));
        Assert.That(shaderText, Does.Not.Contain("_CASTING_PUNCTUAL_LIGHT_SHADOW"));
        Assert.That(shaderText, Does.Not.Contain("_SHADOW_CASTER_ALPHA_TEST_ENABLED"));
    }

    [Test]
    public void UIParticlesUberUnlitPhase53AAlphaTransitionKeywordsStayUiScoped()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Contain("_FADE_TRANSITION_ENABLED"));
        Assert.That(shaderText, Does.Contain("_DISSOLVE_TRANSITION_ENABLED"));
        Assert.That(shaderText, Does.Contain("_ALPHA_TRANSITION_MAP_MODE_2D"));
        Assert.That(shaderText, Does.Contain("_ALPHA_TRANSITION_MAP_MODE_2D_ARRAY"));
        Assert.That(shaderText, Does.Contain("_ALPHA_TRANSITION_MAP_MODE_3D"));
        Assert.That(shaderText, Does.Contain("_ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED"));
        Assert.That(shaderText, Does.Contain("_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D"));
        Assert.That(shaderText, Does.Contain("_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY"));
        Assert.That(shaderText, Does.Contain("_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_3D"));
        Assert.That(shaderText, Does.Contain("_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_RANDOM_ROW_SELECTION_ENABLED"));
        Assert.That(shaderText, Does.Contain("_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE"));
        Assert.That(shaderText, Does.Contain("_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY"));
        Assert.That(shaderText, Does.Contain("_VERTEX_ALPHA_AS_TRANSITION_PROGRESS"));
        Assert.That(shaderText, Does.Contain("_FLOW_MAP_TARGET_ALPHA_TRANSITION"));
        Assert.That(shaderText, Does.Contain("UNITY_UI_CLIP_RECT"));
        Assert.That(shaderText, Does.Not.Contain("_CASTING_PUNCTUAL_LIGHT_SHADOW"));
        Assert.That(shaderText, Does.Not.Contain("_SHADOW_CASTER_ALPHA_TEST_ENABLED"));
    }

    [Test]
    public void UIParticlesUberUnlitPhase53BEmissionKeywordsStayUiScoped()
    {
        var shaderText = File.ReadAllText(ShaderPath);

        Assert.That(shaderText, Does.Contain("_EMISSION_AREA_ALL"));
        Assert.That(shaderText, Does.Contain("_EMISSION_AREA_ALPHA"));
        Assert.That(shaderText, Does.Contain("_EMISSION_AREA_MAP"));
        Assert.That(shaderText, Does.Contain("_EMISSION_COLOR_BASECOLOR"));
        Assert.That(shaderText, Does.Contain("_EMISSION_COLOR_COLOR"));
        Assert.That(shaderText, Does.Contain("_EMISSION_COLOR_MAP"));
        Assert.That(shaderText, Does.Contain("_EMISSION_MAP_MODE_2D"));
        Assert.That(shaderText, Does.Contain("_EMISSION_MAP_MODE_2D_ARRAY"));
        Assert.That(shaderText, Does.Contain("_EMISSION_MAP_MODE_3D"));
        Assert.That(shaderText, Does.Contain("_EMISSION_MAP_RANDOM_ROW_SELECTION_ENABLED"));
        Assert.That(shaderText, Does.Contain("_FLOW_MAP_TARGET_EMISSION"));
        Assert.That(shaderText, Does.Contain("UNITY_UI_CLIP_RECT"));
        Assert.That(shaderText, Does.Not.Contain("_CASTING_PUNCTUAL_LIGHT_SHADOW"));
        Assert.That(shaderText, Does.Not.Contain("_SHADOW_CASTER_ALPHA_TEST_ENABLED"));
        Assert.That(CountOccurrences(shaderText, "\n        Pass\n        {"), Is.EqualTo(1));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"ShadowCaster\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"DepthOnly\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"DepthNormals\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"SceneSelectionPass\""));
        Assert.That(shaderText, Does.Not.Contain("\"LightMode\" = \"Picking\""));
    }

    [Test]
    public void UIParticlesUberUnlitRepresentativeKeywordStatesHaveNoBlockingCompilerMessages()
    {
        var shader = Shader.Find(ShaderName);
        Assert.That(shader, Is.Not.Null, ShaderName);

        AssetDatabase.ImportAsset(ShaderPath, ImportAssetOptions.ForceUpdate);

        foreach (var material in CreateRepresentativeMaterials(shader))
        {
            try
            {
                SetupUberUnlit(material);
                Shader.WarmupAllShaders();

                var blockingMessages = ShaderUtil.GetShaderMessages(shader)
                    .Where(IsBlockingShaderMessage)
                    .Select(message => $"{message.severity}: {message.message} ({message.file}:{message.line})")
                    .ToArray();

                Assert.That(blockingMessages, Is.Empty, string.Join(Environment.NewLine, blockingMessages));
            }
            finally
            {
                foreach (var texture in material.GetTexturePropertyNames()
                    .Select(material.GetTexture)
                    .Where(texture => texture != null && texture.name.StartsWith("NovaBuiltInUiCompile", StringComparison.Ordinal))
                    .Distinct())
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }

                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }

    [Test]
    public void UIParticlesUberUnlitSourceContainsNoForbiddenPipelineTokens()
    {
        var shaderText = StripComments(File.ReadAllText(ShaderPath));
        var violations = ForbiddenTokens
            .Where(token => shaderText.Contains(token, StringComparison.Ordinal))
            .ToArray();

        Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
    }

    private static Material[] CreateRepresentativeMaterials(Shader shader)
    {
        var defaultMaterial = new Material(shader);

        var baseMap2D = new Material(shader);
        baseMap2D.SetFloat("_BaseMapMode", 0);
        baseMap2D.SetTexture("_BaseMap", CreateCompileTexture());

        var baseMap2DArray = new Material(shader);
        baseMap2DArray.SetFloat("_BaseMapMode", 1);
        baseMap2DArray.SetFloat("_BaseMapSliceCount", 2);
        baseMap2DArray.SetTexture("_BaseMap2DArray", CreateCompileTexture2DArray());

        var baseMap3D = new Material(shader);
        baseMap3D.SetFloat("_BaseMapMode", 2);
        baseMap3D.SetFloat("_BaseMapSliceCount", 2);
        baseMap3D.SetTexture("_BaseMap3D", CreateCompileTexture3D());

        var channel = new Material(shader);
        channel.SetFloat("_BaseMapChannel", 2);
        channel.SetTexture("_BaseMap", CreateCompileTexture());

        var randomRow = new Material(shader);
        randomRow.SetFloat("_BaseMapMode", 1);
        randomRow.SetFloat("_BaseMapSliceCount", 4);
        randomRow.SetFloat("_BaseMapRandomRowSelectionEnabled", 1);
        randomRow.SetFloat("_BaseMapRandomRowCoord", 1);
        randomRow.SetFloat("_BaseMapRowCount", 2);
        randomRow.SetTexture("_BaseMap2DArray", CreateCompileTexture2DArray(4));

        var tritone = new Material(shader);
        tritone.SetFloat("_BaseMapToneMode", 1);
        tritone.SetTexture("_BaseMap", CreateCompileTexture());

        var pentone = new Material(shader);
        pentone.SetFloat("_BaseMapToneMode", 2);
        pentone.SetTexture("_BaseMap", CreateCompileTexture());

        var pointMirror = new Material(shader);
        pointMirror.SetFloat("_BaseMapMirrorSampling", 1);
        pointMirror.SetTexture("_BaseMap", CreateCompileTexture(FilterMode.Point));

        var linearMirror = new Material(shader);
        linearMirror.SetFloat("_BaseMapMirrorSampling", 1);
        linearMirror.SetTexture("_BaseMap", CreateCompileTexture(FilterMode.Bilinear));

        var trilinearMirror = new Material(shader);
        trilinearMirror.SetFloat("_BaseMapMirrorSampling", 1);
        trilinearMirror.SetTexture("_BaseMap", CreateCompileTexture(FilterMode.Trilinear));

        var baseFeatureClipRect = new Material(shader);
        baseFeatureClipRect.SetFloat("_BaseMapMode", 1);
        baseFeatureClipRect.SetFloat("_BaseMapSliceCount", 2);
        baseFeatureClipRect.SetTexture("_BaseMap2DArray", CreateCompileTexture2DArray());
        baseFeatureClipRect.EnableKeyword("UNITY_UI_CLIP_RECT");
        baseFeatureClipRect.SetVector("_ClipRect", new Vector4(-0.25f, -1f, 1f, 1f));

        var baseFeatureAlphaTest = new Material(shader);
        baseFeatureAlphaTest.SetFloat("_RenderType", 1);
        baseFeatureAlphaTest.SetFloat("_Cutoff", 0.5f);
        baseFeatureAlphaTest.SetFloat("_BaseMapToneMode", 1);
        baseFeatureAlphaTest.SetTexture("_BaseMap", CreateCompileTexture());

        var tintColor = new Material(shader);
        tintColor.SetFloat("_TintAreaMode", 1);
        tintColor.SetFloat("_TintColorMode", 0);
        tintColor.SetColor("_TintColor", new Color(0.25f, 1f, 0.25f, 1f));

        var tintMap = new Material(shader);
        tintMap.SetFloat("_TintAreaMode", 1);
        tintMap.SetFloat("_TintColorMode", 1);
        tintMap.SetTexture("_TintMap", CreateCompileTexture());

        var tintMap2DArray = new Material(shader);
        tintMap2DArray.SetFloat("_TintAreaMode", 1);
        tintMap2DArray.SetFloat("_TintColorMode", 3);
        tintMap2DArray.SetFloat("_TintMapSliceCount", 2);
        tintMap2DArray.SetTexture("_TintMap2DArray", CreateCompileTexture2DArray());

        var tintMap3D = new Material(shader);
        tintMap3D.SetFloat("_TintAreaMode", 1);
        tintMap3D.SetFloat("_TintColorMode", 2);
        tintMap3D.SetFloat("_TintMapSliceCount", 2);
        tintMap3D.SetTexture("_TintMap3D", CreateCompileTexture3D());

        var tintMapRandomRow = new Material(shader);
        tintMapRandomRow.SetFloat("_TintAreaMode", 1);
        tintMapRandomRow.SetFloat("_TintColorMode", 3);
        tintMapRandomRow.SetFloat("_TintMapSliceCount", 4);
        tintMapRandomRow.SetFloat("_TintMapRandomRowSelectionEnabled", 1);
        tintMapRandomRow.SetFloat("_TintMapRandomRowCoord", 1);
        tintMapRandomRow.SetFloat("_TintMapRowCount", 2);
        tintMapRandomRow.SetTexture("_TintMap2DArray", CreateCompileTexture2DArray(4));

        var tintRim = new Material(shader);
        tintRim.SetFloat("_TintAreaMode", 2);
        tintRim.SetFloat("_TintColorMode", 0);
        tintRim.SetColor("_TintColor", new Color(0.25f, 1f, 0.25f, 1f));

        var flowBase = new Material(shader);
        flowBase.SetTexture("_FlowMap", CreateCompileTexture());
        flowBase.SetFloat("_FlowMapTarget", 1);

        var flowTint = new Material(shader);
        flowTint.SetFloat("_TintAreaMode", 1);
        flowTint.SetFloat("_TintColorMode", 1);
        flowTint.SetTexture("_TintMap", CreateCompileTexture());
        flowTint.SetTexture("_FlowMap", CreateCompileTexture());
        flowTint.SetFloat("_FlowMapTarget", 2);

        var parallax2DBase = new Material(shader);
        parallax2DBase.SetTexture("_ParallaxMap", CreateCompileTexture());
        parallax2DBase.SetFloat("_ParallaxMapMode", 0);
        parallax2DBase.SetFloat("_ParallaxMapTarget", 1);

        var parallax2DArrayTint = new Material(shader);
        parallax2DArrayTint.SetFloat("_ParallaxMapMode", 1);
        parallax2DArrayTint.SetFloat("_ParallaxMapTarget", 2);
        parallax2DArrayTint.SetFloat("_ParallaxMapSliceCount", 2);
        parallax2DArrayTint.SetTexture("_ParallaxMap2DArray", CreateCompileTexture2DArray());

        var parallax3D = new Material(shader);
        parallax3D.SetFloat("_ParallaxMapMode", 2);
        parallax3D.SetFloat("_ParallaxMapTarget", 1);
        parallax3D.SetFloat("_ParallaxMapSliceCount", 2);
        parallax3D.SetTexture("_ParallaxMap3D", CreateCompileTexture3D());

        var greyscale = new Material(shader);
        greyscale.SetFloat("_ColorCorrectionMode", 1);

        var gradientMap = new Material(shader);
        gradientMap.SetFloat("_ColorCorrectionMode", 2);
        gradientMap.SetTexture("_GradientMap", CreateCompileTexture(FilterMode.Bilinear));

        var tintFlowClipRect = new Material(shader);
        tintFlowClipRect.SetFloat("_TintAreaMode", 1);
        tintFlowClipRect.SetFloat("_TintColorMode", 1);
        tintFlowClipRect.SetTexture("_TintMap", CreateCompileTexture());
        tintFlowClipRect.SetTexture("_FlowMap", CreateCompileTexture());
        tintFlowClipRect.SetFloat("_FlowMapTarget", 2);
        tintFlowClipRect.EnableKeyword("UNITY_UI_CLIP_RECT");
        tintFlowClipRect.SetVector("_ClipRect", new Vector4(-0.25f, -1f, 1f, 1f));

        var gradientAlphaTest = new Material(shader);
        gradientAlphaTest.SetFloat("_RenderType", 1);
        gradientAlphaTest.SetFloat("_Cutoff", 0.5f);
        gradientAlphaTest.SetFloat("_ColorCorrectionMode", 2);
        gradientAlphaTest.SetTexture("_GradientMap", CreateCompileTexture(FilterMode.Bilinear));

        var fadeTransition = new Material(shader);
        fadeTransition.SetFloat("_AlphaTransitionMode", 1);
        fadeTransition.SetFloat("_AlphaTransitionMapMode", 0);
        fadeTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var dissolveTransition = new Material(shader);
        dissolveTransition.SetFloat("_AlphaTransitionMode", 2);
        dissolveTransition.SetFloat("_AlphaTransitionMapMode", 0);
        dissolveTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var alphaTransition2DArray = new Material(shader);
        alphaTransition2DArray.SetFloat("_AlphaTransitionMode", 2);
        alphaTransition2DArray.SetFloat("_AlphaTransitionMapMode", 1);
        alphaTransition2DArray.SetFloat("_AlphaTransitionMapSliceCount", 2);
        alphaTransition2DArray.SetTexture("_AlphaTransitionMap2DArray", CreateCompileTexture2DArray());

        var alphaTransition3D = new Material(shader);
        alphaTransition3D.SetFloat("_AlphaTransitionMode", 2);
        alphaTransition3D.SetFloat("_AlphaTransitionMapMode", 2);
        alphaTransition3D.SetFloat("_AlphaTransitionMapSliceCount", 2);
        alphaTransition3D.SetTexture("_AlphaTransitionMap3D", CreateCompileTexture3D());

        var alphaTransitionRandomRow = new Material(shader);
        alphaTransitionRandomRow.SetFloat("_AlphaTransitionMode", 2);
        alphaTransitionRandomRow.SetFloat("_AlphaTransitionMapMode", 1);
        alphaTransitionRandomRow.SetFloat("_AlphaTransitionMapRandomRowSelectionEnabled", 1);
        alphaTransitionRandomRow.SetFloat("_AlphaTransitionMapRandomRowCoord", 1);
        alphaTransitionRandomRow.SetFloat("_AlphaTransitionMapRowCount", 2);
        alphaTransitionRandomRow.SetTexture("_AlphaTransitionMap2DArray", CreateCompileTexture2DArray(4));

        var secondAlphaTransitionAverage = new Material(shader);
        secondAlphaTransitionAverage.SetFloat("_AlphaTransitionMode", 2);
        secondAlphaTransitionAverage.SetFloat("_AlphaTransitionMapMode", 0);
        secondAlphaTransitionAverage.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        secondAlphaTransitionAverage.SetFloat("_AlphaTransitionSecondTextureBlendMode", 1);
        secondAlphaTransitionAverage.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 0);
        secondAlphaTransitionAverage.SetTexture("_AlphaTransitionMapSecondTexture", CreateCompileTexture(FilterMode.Point));

        var secondAlphaTransitionMultiply = new Material(shader);
        secondAlphaTransitionMultiply.SetFloat("_AlphaTransitionMode", 2);
        secondAlphaTransitionMultiply.SetFloat("_AlphaTransitionMapMode", 0);
        secondAlphaTransitionMultiply.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        secondAlphaTransitionMultiply.SetFloat("_AlphaTransitionSecondTextureBlendMode", 2);
        secondAlphaTransitionMultiply.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 0);
        secondAlphaTransitionMultiply.SetTexture("_AlphaTransitionMapSecondTexture", CreateCompileTexture(FilterMode.Point));

        var secondAlphaTransition2DArray = new Material(shader);
        secondAlphaTransition2DArray.SetFloat("_AlphaTransitionMode", 2);
        secondAlphaTransition2DArray.SetFloat("_AlphaTransitionMapMode", 0);
        secondAlphaTransition2DArray.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        secondAlphaTransition2DArray.SetFloat("_AlphaTransitionSecondTextureBlendMode", 1);
        secondAlphaTransition2DArray.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 1);
        secondAlphaTransition2DArray.SetFloat("_AlphaTransitionMapSecondTextureSliceCount", 2);
        secondAlphaTransition2DArray.SetTexture("_AlphaTransitionMapSecondTexture2DArray", CreateCompileTexture2DArray());

        var secondAlphaTransition3D = new Material(shader);
        secondAlphaTransition3D.SetFloat("_AlphaTransitionMode", 2);
        secondAlphaTransition3D.SetFloat("_AlphaTransitionMapMode", 0);
        secondAlphaTransition3D.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        secondAlphaTransition3D.SetFloat("_AlphaTransitionSecondTextureBlendMode", 1);
        secondAlphaTransition3D.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 2);
        secondAlphaTransition3D.SetFloat("_AlphaTransitionMapSecondTextureSliceCount", 2);
        secondAlphaTransition3D.SetTexture("_AlphaTransitionMapSecondTexture3D", CreateCompileTexture3D());

        var secondAlphaTransitionRandomRow = new Material(shader);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMode", 2);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMapMode", 0);
        secondAlphaTransitionRandomRow.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionSecondTextureBlendMode", 1);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMapSecondTextureMapMode", 1);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMapSecondTextureRandomRowSelectionEnabled", 1);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMapSecondTextureRandomRowCoord", 1);
        secondAlphaTransitionRandomRow.SetFloat("_AlphaTransitionMapSecondTextureRowCount", 2);
        secondAlphaTransitionRandomRow.SetTexture("_AlphaTransitionMapSecondTexture2DArray", CreateCompileTexture2DArray(4));

        var vertexAlphaTransition = new Material(shader);
        vertexAlphaTransition.SetFloat("_VertexAlphaMode", 1);
        vertexAlphaTransition.SetFloat("_AlphaTransitionMode", 1);
        vertexAlphaTransition.SetFloat("_AlphaTransitionMapMode", 0);
        vertexAlphaTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var flowAlphaTransition = new Material(shader);
        flowAlphaTransition.SetTexture("_FlowMap", CreateCompileTexture(FilterMode.Point));
        flowAlphaTransition.SetFloat("_FlowMapTarget", 4);
        flowAlphaTransition.SetFloat("_AlphaTransitionMode", 2);
        flowAlphaTransition.SetFloat("_AlphaTransitionMapMode", 0);
        flowAlphaTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var emissionAreaAll = new Material(shader);
        emissionAreaAll.SetFloat("_EmissionAreaType", 1);
        emissionAreaAll.SetFloat("_EmissionColorType", 0);
        emissionAreaAll.SetColor("_EmissionColor", new Color(0f, 1f, 0f, 1f));

        var emissionAreaAlpha = new Material(shader);
        emissionAreaAlpha.SetFloat("_EmissionAreaType", 3);
        emissionAreaAlpha.SetFloat("_EmissionColorType", 0);
        emissionAreaAlpha.SetColor("_EmissionColor", new Color(0f, 1f, 0f, 1f));

        var emissionAreaMap = new Material(shader);
        emissionAreaMap.SetFloat("_EmissionAreaType", 2);
        emissionAreaMap.SetFloat("_EmissionColorType", 0);
        emissionAreaMap.SetFloat("_EmissionMapMode", 0);
        emissionAreaMap.SetTexture("_EmissionMap", CreateCompileTexture(FilterMode.Point));

        var emissionColorBase = new Material(shader);
        emissionColorBase.SetFloat("_EmissionAreaType", 1);
        emissionColorBase.SetFloat("_EmissionColorType", 1);

        var emissionColorRamp = new Material(shader);
        emissionColorRamp.SetFloat("_EmissionAreaType", 2);
        emissionColorRamp.SetFloat("_EmissionColorType", 2);
        emissionColorRamp.SetFloat("_EmissionMapMode", 0);
        emissionColorRamp.SetTexture("_EmissionMap", CreateCompileTexture(FilterMode.Point));
        emissionColorRamp.SetTexture("_EmissionColorRamp", CreateCompileTexture(FilterMode.Point));

        var emissionMap2DArray = new Material(shader);
        emissionMap2DArray.SetFloat("_EmissionAreaType", 2);
        emissionMap2DArray.SetFloat("_EmissionColorType", 0);
        emissionMap2DArray.SetFloat("_EmissionMapMode", 1);
        emissionMap2DArray.SetFloat("_EmissionMapSliceCount", 2);
        emissionMap2DArray.SetTexture("_EmissionMap2DArray", CreateCompileTexture2DArray());

        var emissionMap3D = new Material(shader);
        emissionMap3D.SetFloat("_EmissionAreaType", 2);
        emissionMap3D.SetFloat("_EmissionColorType", 0);
        emissionMap3D.SetFloat("_EmissionMapMode", 2);
        emissionMap3D.SetFloat("_EmissionMapSliceCount", 2);
        emissionMap3D.SetTexture("_EmissionMap3D", CreateCompileTexture3D());

        var emissionMapRandomRow = new Material(shader);
        emissionMapRandomRow.SetFloat("_EmissionAreaType", 2);
        emissionMapRandomRow.SetFloat("_EmissionColorType", 0);
        emissionMapRandomRow.SetFloat("_EmissionMapMode", 1);
        emissionMapRandomRow.SetFloat("_EmissionMapRandomRowSelectionEnabled", 1);
        emissionMapRandomRow.SetFloat("_EmissionMapRandomRowCoord", 1);
        emissionMapRandomRow.SetFloat("_EmissionMapRowCount", 2);
        emissionMapRandomRow.SetFloat("_EmissionMapSliceCount", 4);
        emissionMapRandomRow.SetTexture("_EmissionMap2DArray", CreateCompileTexture2DArray(4));

        var flowEmission = new Material(shader);
        flowEmission.SetFloat("_EmissionAreaType", 2);
        flowEmission.SetFloat("_EmissionColorType", 0);
        flowEmission.SetFloat("_EmissionMapMode", 0);
        flowEmission.SetTexture("_EmissionMap", CreateCompileTexture(FilterMode.Point));
        flowEmission.SetTexture("_FlowMap", CreateCompileTexture(FilterMode.Point));
        flowEmission.SetFloat("_FlowMapTarget", 8);

        var emissionClipRect = new Material(shader);
        emissionClipRect.SetFloat("_EmissionAreaType", 2);
        emissionClipRect.SetFloat("_EmissionColorType", 0);
        emissionClipRect.SetFloat("_EmissionMapMode", 0);
        emissionClipRect.SetTexture("_EmissionMap", CreateCompileTexture(FilterMode.Point));
        emissionClipRect.EnableKeyword("UNITY_UI_CLIP_RECT");
        emissionClipRect.SetVector("_ClipRect", new Vector4(-0.25f, -1f, 1f, 1f));

        var emissionAlphaTest = new Material(shader);
        emissionAlphaTest.SetFloat("_RenderType", 1);
        emissionAlphaTest.SetFloat("_Cutoff", 0.5f);
        emissionAlphaTest.SetFloat("_EmissionAreaType", 1);
        emissionAlphaTest.SetFloat("_EmissionColorType", 0);

        var emissionAlphaTransition = new Material(shader);
        emissionAlphaTransition.SetFloat("_AlphaTransitionMode", 2);
        emissionAlphaTransition.SetFloat("_AlphaTransitionMapMode", 0);
        emissionAlphaTransition.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        emissionAlphaTransition.SetFloat("_EmissionAreaType", 1);
        emissionAlphaTransition.SetFloat("_EmissionColorType", 0);

        var alphaTransitionClipRect = new Material(shader);
        alphaTransitionClipRect.SetFloat("_AlphaTransitionMode", 2);
        alphaTransitionClipRect.SetFloat("_AlphaTransitionMapMode", 0);
        alphaTransitionClipRect.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));
        alphaTransitionClipRect.EnableKeyword("UNITY_UI_CLIP_RECT");
        alphaTransitionClipRect.SetVector("_ClipRect", new Vector4(-0.25f, -1f, 1f, 1f));

        var alphaTransitionAlphaTest = new Material(shader);
        alphaTransitionAlphaTest.SetFloat("_RenderType", 1);
        alphaTransitionAlphaTest.SetFloat("_Cutoff", 0.5f);
        alphaTransitionAlphaTest.SetFloat("_AlphaTransitionMode", 2);
        alphaTransitionAlphaTest.SetFloat("_AlphaTransitionMapMode", 0);
        alphaTransitionAlphaTest.SetTexture("_AlphaTransitionMap", CreateCompileTexture(FilterMode.Point));

        var alphaTest = new Material(shader);
        alphaTest.SetFloat("_RenderType", 1);
        alphaTest.SetFloat("_Cutoff", 0.5f);

        var clipRect = new Material(shader);
        clipRect.EnableKeyword("UNITY_UI_CLIP_RECT");
        clipRect.SetVector("_ClipRect", new Vector4(-0.25f, -1f, 1f, 1f));

        var stencil = new Material(shader);
        stencil.SetFloat("_StencilComp", 3);
        stencil.SetFloat("_Stencil", 1);
        stencil.SetFloat("_StencilOp", 2);
        stencil.SetFloat("_StencilReadMask", 255);
        stencil.SetFloat("_StencilWriteMask", 255);
        stencil.SetFloat("_ColorMask", 15);

        return new[]
        {
            defaultMaterial,
            baseMap2D,
            baseMap2DArray,
            baseMap3D,
            channel,
            randomRow,
            tritone,
            pentone,
            pointMirror,
            linearMirror,
            trilinearMirror,
            baseFeatureClipRect,
            baseFeatureAlphaTest,
            tintColor,
            tintMap,
            tintMap2DArray,
            tintMap3D,
            tintMapRandomRow,
            tintRim,
            flowBase,
            flowTint,
            parallax2DBase,
            parallax2DArrayTint,
            parallax3D,
            greyscale,
            gradientMap,
            tintFlowClipRect,
            gradientAlphaTest,
            fadeTransition,
            dissolveTransition,
            alphaTransition2DArray,
            alphaTransition3D,
            alphaTransitionRandomRow,
            secondAlphaTransitionAverage,
            secondAlphaTransitionMultiply,
            secondAlphaTransition2DArray,
            secondAlphaTransition3D,
            secondAlphaTransitionRandomRow,
            vertexAlphaTransition,
            flowAlphaTransition,
            emissionAreaAll,
            emissionAreaAlpha,
            emissionAreaMap,
            emissionColorBase,
            emissionColorRamp,
            emissionMap2DArray,
            emissionMap3D,
            emissionMapRandomRow,
            flowEmission,
            emissionClipRect,
            emissionAlphaTest,
            emissionAlphaTransition,
            alphaTransitionClipRect,
            alphaTransitionAlphaTest,
            alphaTest,
            clipRect,
            stencil
        };
    }

    private static Texture2D CreateCompileTexture(FilterMode filterMode = FilterMode.Point)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUiCompileTexture",
            filterMode = filterMode,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return texture;
    }

    private static Texture2DArray CreateCompileTexture2DArray(int depth = 2)
    {
        var texture = new Texture2DArray(1, 1, depth, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUiCompileTexture2DArray",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        for (var slice = 0; slice < depth; slice++)
        {
            texture.SetPixels(new[] { Color.white }, slice);
        }

        texture.Apply();
        return texture;
    }

    private static Texture3D CreateCompileTexture3D()
    {
        var texture = new Texture3D(1, 1, 2, TextureFormat.RGBA32, false)
        {
            name = "NovaBuiltInUiCompileTexture3D",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.SetPixels(new[] { Color.white, Color.white });
        texture.Apply();
        return texture;
    }

    private static bool IsBlockingShaderMessage(ShaderMessage message)
    {
        var severity = message.severity.ToString();
        if (string.Equals(severity, "Error", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.Equals(severity, "Warning", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        TestContext.Out.WriteLine($"Shader warning: {message.message} ({message.file}:{message.line})");
        return Regex.IsMatch(
            message.message,
            "missing|include|unknown|macro|unsupported|texture|sampler|invalid|undeclared|variant|compile",
            RegexOptions.IgnoreCase);
    }

    private static void SetupUberUnlit(Material material)
    {
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberUnlitMaterialPostProcessor", "SetupMaterialKeywords", material);
        InvokeStatic("Nova.Editor.Core.Scripts.ParticlesUberUnlitMaterialPostProcessor", "SetupMaterialBlendMode", material);
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

    private static string StripComments(string text)
    {
        return Regex.Replace(
            text,
            @"/\*.*?\*/|//.*?$",
            string.Empty,
            RegexOptions.Singleline | RegexOptions.Multiline);
    }

    private static int CountOccurrences(string text, string token)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += token.Length;
        }

        return count;
    }
}

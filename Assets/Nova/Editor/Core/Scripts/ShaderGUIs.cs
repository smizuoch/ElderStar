using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Editor.Core.Scripts
{
    using static MaterialSetupUtility;

    public static class ParticlesDistortionMaterialPostProcessor
    {
        public static void SetupMaterialKeywords(Material material)
        {
            SetMirrorKeyword(material, "_BaseMapMirrorSampling", "_BaseMap");
            SetKeyword(material, ShaderKeywords.FlowMapEnabled, false);

            var hasFlowMap = material.GetTexture("_FlowMap") != null;
            var flowTarget = (FlowMapTargetDistortion)GetFloat(material, "_FlowMapTarget");
            SetKeyword(material, ShaderKeywords.FlowMapTargetBase, hasFlowMap && (flowTarget & FlowMapTargetDistortion.BaseMap) != 0);
            SetKeyword(material, ShaderKeywords.FlowMapTargetAlphaTransition, hasFlowMap && (flowTarget & FlowMapTargetDistortion.AlphaTransitionMap) != 0);

            var hasAlphaTransition = material.GetTexture("_AlphaTransitionMap") != null;
            var alphaMode = (AlphaTransitionMode)GetFloat(material, "_AlphaTransitionMode");
            SetKeyword(material, ShaderKeywords.FadeTransitionEnabled, hasAlphaTransition && alphaMode == AlphaTransitionMode.Fade);
            SetKeyword(material, ShaderKeywords.DissolveTransitionEnabled, hasAlphaTransition && alphaMode == AlphaTransitionMode.Dissolve);
            SetKeyword(material, ShaderKeywords.SoftParticlesEnabled, GetFloat(material, "_SoftParticlesEnabled") > 0.5f);
            SetKeyword(material, ShaderKeywords.DepthFadeEnabled, GetFloat(material, "_DepthFadeEnabled") > 0.5f);
        }
    }

    public static class ParticlesUberUnlitMaterialPostProcessor
    {
        public static void SetupMaterialKeywords(Material material)
        {
            material.doubleSidedGI = GetFloat(material, "_Cull") != (float)CullMode.Back;
            SetKeyword(material, ShaderKeywords.VertexAlphaAsTransitionProgress, GetFloat(material, "_VertexAlphaMode") == (float)VertexAlphaMode.TransitionProgress);
            SetupBaseMapKeywords(material);
            SetupTintAndCorrectionKeywords(material);
            SetupFlowKeywords(material);
            SetupParallaxKeywords(material);
            SetupAlphaTransitionKeywords(material);
            SetupEmissionKeywords(material);
            SetupTransparencyKeywords(material);
            SetKeyword(material, ShaderKeywords.VertexDeformationEnabled, material.GetTexture("_VertexDeformationMap") != null);

            var isUiShader = IsUiShader(material);
            var shadowCasterEnabled = !isUiShader && GetFloat(material, "_ShadowCasterEnabled") > 0.5f;
            material.SetShaderPassEnabled("ShadowCaster", shadowCasterEnabled);
            SetKeyword(material, ShaderKeywords.ShadowCasterAlphaTestEnabled, shadowCasterEnabled && GetFloat(material, "_ShadowCasterAlphaTestEnabled") > 0.5f);
        }

        public static void SetupMaterialBlendMode(Material material)
        {
            var renderType = (RenderType)GetFloat(material, "_RenderType");
            var queueOffset = (int)GetFloat(material, "_QueueOffset");
            var alphaClip = renderType == RenderType.Cutout;
            SetKeyword(material, ShaderKeywords.AlphaTestEnabled, alphaClip);

            var uiShader = IsUiShader(material);
            var defaultOpaqueZWrite = uiShader ? 0 : 1;
            var zWriteOverride = (ZWriteOverride)GetFloat(material, "_ZWriteOverride");

            int ZWrite(int defaultValue)
            {
                return zWriteOverride == ZWriteOverride.Auto ? defaultValue : (int)zWriteOverride;
            }

            switch (renderType)
            {
                case RenderType.Opaque:
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry + queueOffset;
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.SetInt("_BlendSrc", (int)BlendMode.One);
                    material.SetInt("_BlendDst", (int)BlendMode.Zero);
                    material.SetInt("_ZWrite", ZWrite(defaultOpaqueZWrite));
                    SetKeyword(material, ShaderKeywords.AlphaModulateEnabled, false);
                    break;
                case RenderType.Cutout:
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest + queueOffset;
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_BlendSrc", (int)BlendMode.One);
                    material.SetInt("_BlendDst", (int)BlendMode.Zero);
                    material.SetInt("_ZWrite", ZWrite(defaultOpaqueZWrite));
                    SetKeyword(material, ShaderKeywords.AlphaModulateEnabled, false);
                    break;
                case RenderType.Transparent:
                    SetupTransparentBlend(material);
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + queueOffset;
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_ZWrite", ZWrite(0));
                    break;
            }
        }

        private static void SetupTransparentBlend(Material material)
        {
            switch ((TransparentBlendMode)GetFloat(material, "_TransparentBlendMode"))
            {
                case TransparentBlendMode.Alpha:
                    material.SetInt("_BlendSrc", (int)BlendMode.SrcAlpha);
                    material.SetInt("_BlendDst", (int)BlendMode.OneMinusSrcAlpha);
                    SetKeyword(material, ShaderKeywords.AlphaModulateEnabled, false);
                    break;
                case TransparentBlendMode.Additive:
                    material.SetInt("_BlendSrc", (int)BlendMode.SrcAlpha);
                    material.SetInt("_BlendDst", (int)BlendMode.One);
                    SetKeyword(material, ShaderKeywords.AlphaModulateEnabled, false);
                    break;
                case TransparentBlendMode.Multiply:
                    material.SetInt("_BlendSrc", (int)BlendMode.DstColor);
                    material.SetInt("_BlendDst", (int)BlendMode.Zero);
                    SetKeyword(material, ShaderKeywords.AlphaModulateEnabled, true);
                    break;
            }
        }

        private static void SetupBaseMapKeywords(Material material)
        {
            var mode = (BaseMapMode)GetFloat(material, "_BaseMapMode");
            SetKeyword(material, ShaderKeywords.BaseMapMode2D, mode == BaseMapMode.SingleTexture);
            SetKeyword(material, ShaderKeywords.BaseMapMode2DArray, mode == BaseMapMode.FlipBook);
            SetKeyword(material, ShaderKeywords.BaseMapMode3D, mode == BaseMapMode.FlipBookBlending);
            SetMirrorKeyword(material, "_BaseMapMirrorSampling", mode == BaseMapMode.SingleTexture ? "_BaseMap" : mode == BaseMapMode.FlipBook ? "_BaseMap2DArray" : "_BaseMap3D");
            SetKeyword(material, ShaderKeywords.BaseMapRandomRowSelectionEnabled, IsFlipBook(mode) && GetFloat(material, "_BaseMapRandomRowSelectionEnabled") > 0.5f);
            SetKeyword(material, ShaderKeywords.BaseMapChannelEnabled, GetFloat(material, "_BaseMapChannel") > 0.5f);

            var toneMode = (BaseMapToneMode)GetFloat(material, "_BaseMapToneMode");
            SetKeyword(material, ShaderKeywords.BaseMapToneModeTriTone, toneMode == BaseMapToneMode.Tritone);
            SetKeyword(material, ShaderKeywords.BaseMapToneModePentone, toneMode == BaseMapToneMode.Pentone);
        }

        private static void SetupTintAndCorrectionKeywords(Material material)
        {
            var tintAreaMode = (TintAreaMode)GetFloat(material, "_TintAreaMode");
            SetKeyword(material, ShaderKeywords.TintAreaAll, tintAreaMode == TintAreaMode.All);
            SetKeyword(material, ShaderKeywords.TintAreaRim, tintAreaMode == TintAreaMode.Rim);

            var tintColorMode = (TintColorMode)GetFloat(material, "_TintColorMode");
            SetKeyword(material, ShaderKeywords.TintColorEnabled, tintColorMode == TintColorMode.SingleColor);
            SetKeyword(material, ShaderKeywords.TintMapEnabled, tintColorMode == TintColorMode.SingleTexture);
            SetKeyword(material, ShaderKeywords.TintMap3DEnabled, tintColorMode == TintColorMode.FlipBookBlending);
            SetKeyword(material, ShaderKeywords.TintMapMode2DArray, tintColorMode == TintColorMode.FlipBook);
            SetKeyword(material, ShaderKeywords.TintMapRandomRowSelectionEnabled, (tintColorMode == TintColorMode.FlipBook || tintColorMode == TintColorMode.FlipBookBlending) && GetFloat(material, "_TintMapRandomRowSelectionEnabled") > 0.5f);

            var colorCorrectionMode = (ColorCorrectionMode)GetFloat(material, "_ColorCorrectionMode");
            SetKeyword(material, ShaderKeywords.GreyscaleEnabled, colorCorrectionMode == ColorCorrectionMode.Greyscale);
            SetKeyword(material, ShaderKeywords.GradientMapEnabled, colorCorrectionMode == ColorCorrectionMode.GradientMap);
        }

        private static void SetupFlowKeywords(Material material)
        {
            SetKeyword(material, ShaderKeywords.FlowMapEnabled, false);
            var hasFlowMap = material.GetTexture("_FlowMap") != null;
            var target = (FlowMapTarget)GetFloat(material, "_FlowMapTarget");
            SetKeyword(material, ShaderKeywords.FlowMapTargetBase, hasFlowMap && (target & FlowMapTarget.BaseMap) != 0);
            SetKeyword(material, ShaderKeywords.FlowMapTargetTint, hasFlowMap && (target & FlowMapTarget.TintMap) != 0);
            SetKeyword(material, ShaderKeywords.FlowMapTargetAlphaTransition, hasFlowMap && (target & FlowMapTarget.AlphaTransitionMap) != 0);
            SetKeyword(material, ShaderKeywords.FlowMapTargetEmission, hasFlowMap && (target & FlowMapTarget.EmissionMap) != 0);
        }

        private static void SetupParallaxKeywords(Material material)
        {
            var mode = (ParallaxMapMode)GetFloat(material, "_ParallaxMapMode");
            SetKeyword(material, ShaderKeywords.ParallaxMapMode2D, mode == ParallaxMapMode.SingleTexture);
            SetKeyword(material, ShaderKeywords.ParallaxMapMode2DArray, mode == ParallaxMapMode.FlipBook);
            SetKeyword(material, ShaderKeywords.ParallaxMapMode3D, mode == ParallaxMapMode.FlipBookBlending);

            var hasMap = material.GetTexture(mode == ParallaxMapMode.SingleTexture ? "_ParallaxMap" : mode == ParallaxMapMode.FlipBook ? "_ParallaxMap2DArray" : "_ParallaxMap3D") != null;
            var target = (ParallaxMapTarget)GetFloat(material, "_ParallaxMapTarget");
            SetKeyword(material, ShaderKeywords.ParallaxMapTargetBase, hasMap && (target & ParallaxMapTarget.BaseMap) != 0);
            SetKeyword(material, ShaderKeywords.ParallaxMapTargetTint, hasMap && (target & ParallaxMapTarget.TintMap) != 0);
            SetKeyword(material, ShaderKeywords.ParallaxMapTargetEmission, hasMap && (target & ParallaxMapTarget.EmissionMap) != 0);
            SetKeyword(material, ShaderKeywords.ParallaxMapRandomRowSelectionEnabled, IsFlipBook(mode) && GetFloat(material, "_ParallaxMapRandomRowSelectionEnabled") > 0.5f);
        }

        private static void SetupAlphaTransitionKeywords(Material material)
        {
            var hasMap = material.GetTexture("_AlphaTransitionMap") != null || material.GetTexture("_AlphaTransitionMap2DArray") != null || material.GetTexture("_AlphaTransitionMap3D") != null;
            var alphaMode = (AlphaTransitionMode)GetFloat(material, "_AlphaTransitionMode");
            SetKeyword(material, ShaderKeywords.FadeTransitionEnabled, hasMap && alphaMode == AlphaTransitionMode.Fade);
            SetKeyword(material, ShaderKeywords.DissolveTransitionEnabled, hasMap && alphaMode == AlphaTransitionMode.Dissolve);

            var mapMode = (AlphaTransitionMapMode)GetFloat(material, "_AlphaTransitionMapMode");
            SetKeyword(material, ShaderKeywords.AlphaTransitionMapMode2D, mapMode == AlphaTransitionMapMode.SingleTexture);
            SetKeyword(material, ShaderKeywords.AlphaTransitionMapMode2DArray, mapMode == AlphaTransitionMapMode.FlipBook);
            SetKeyword(material, ShaderKeywords.AlphaTransitionMapMode3D, mapMode == AlphaTransitionMapMode.FlipBookBlending);
            SetKeyword(material, ShaderKeywords.AlphaTransitionMapRandomRowSelectionEnabled, IsFlipBook(mapMode) && GetFloat(material, "_AlphaTransitionMapRandomRowSelectionEnabled") > 0.5f);

            var secondBlend = (AlphaTransitionBlendMode)GetFloat(material, "_AlphaTransitionSecondTextureBlendMode");
            var hasSecond = material.GetTexture("_AlphaTransitionMapSecondTexture") != null || material.GetTexture("_AlphaTransitionMapSecondTexture2DArray") != null || material.GetTexture("_AlphaTransitionMapSecondTexture3D") != null;
            var secondActive = hasSecond && secondBlend != AlphaTransitionBlendMode.None;
            SetKeyword(material, ShaderKeywords.AlphaTransitionBlendSecondTexAverage, hasSecond && secondBlend == AlphaTransitionBlendMode.Average);
            SetKeyword(material, ShaderKeywords.AlphaTransitionBlendSecondTexMultiply, hasSecond && secondBlend == AlphaTransitionBlendMode.Multiply);

            var secondMode = (AlphaTransitionMapMode)GetFloat(material, "_AlphaTransitionMapSecondTextureMapMode");
            SetKeyword(material, ShaderKeywords.AlphaTransitionMapSecondTextureMode2D, secondActive && secondMode == AlphaTransitionMapMode.SingleTexture);
            SetKeyword(material, ShaderKeywords.AlphaTransitionMapSecondTextureMode2DArray, secondActive && secondMode == AlphaTransitionMapMode.FlipBook);
            SetKeyword(material, ShaderKeywords.AlphaTransitionMapSecondTextureMode3D, secondActive && secondMode == AlphaTransitionMapMode.FlipBookBlending);
            SetKeyword(material, ShaderKeywords.AlphaTransitionMapSecondTextureRandomRowSelectionEnabled, secondActive && IsFlipBook(secondMode) && GetFloat(material, "_AlphaTransitionMapSecondTextureRandomRowSelectionEnabled") > 0.5f);
        }

        private static void SetupEmissionKeywords(Material material)
        {
            var area = (EmissionAreaType)GetFloat(material, "_EmissionAreaType");
            SetKeyword(material, ShaderKeywords.EmissionAreaAll, area == EmissionAreaType.All);
            SetKeyword(material, ShaderKeywords.EmissionAreaMap, area == EmissionAreaType.ByTexture);
            SetKeyword(material, ShaderKeywords.EmissionAreaAlpha, area == EmissionAreaType.Edge);

            var color = (EmissionColorType)GetFloat(material, "_EmissionColorType");
            SetKeyword(material, ShaderKeywords.EmissionColorColor, color == EmissionColorType.Color);
            SetKeyword(material, ShaderKeywords.EmissionColorBaseColor, color == EmissionColorType.BaseColor);
            SetKeyword(material, ShaderKeywords.EmissionColorMap, color == EmissionColorType.GradiantMap);

            var mode = (EmissionMapMode)GetFloat(material, "_EmissionMapMode");
            SetKeyword(material, ShaderKeywords.EmissionMapMode2D, mode == EmissionMapMode.SingleTexture);
            SetKeyword(material, ShaderKeywords.EmissionMapMode2DArray, mode == EmissionMapMode.FlipBook);
            SetKeyword(material, ShaderKeywords.EmissionMapMode3D, mode == EmissionMapMode.FlipBookBlending);
            SetKeyword(material, ShaderKeywords.EmissionMapRandomRowSelectionEnabled, IsFlipBook(mode) && GetFloat(material, "_EmissionMapRandomRowSelectionEnabled") > 0.5f);
        }

        private static void SetupTransparencyKeywords(Material material)
        {
            SetKeyword(material, ShaderKeywords.TransparencyByRim, GetFloat(material, "_RimTransparencyEnabled") > 0.5f);
            SetKeyword(material, ShaderKeywords.TransparencyByLuminance, GetFloat(material, "_LuminanceTransparencyEnabled") > 0.5f);
            SetKeyword(material, ShaderKeywords.SoftParticlesEnabled, GetFloat(material, "_SoftParticlesEnabled") > 0.5f);
            SetKeyword(material, ShaderKeywords.DepthFadeEnabled, GetFloat(material, "_DepthFadeEnabled") > 0.5f);
        }
    }

    public static class ParticlesUberLitMaterialPostProcessor
    {
        public static void SetupMaterialKeywords(Material material)
        {
            ParticlesUberUnlitMaterialPostProcessor.SetupMaterialKeywords(material);
            // Built-in schedules ForwardAdd but does not supply URP's lighting keyword.
            SetKeyword(material, "_ADDITIONAL_LIGHTS", true);
            SetKeyword(material, ShaderKeywords.ReceiveShadowsEnabled, GetFloat(material, "_LitReceiveShadows") > 0.5f);
            SetKeyword(material, ShaderKeywords.SpecularHighlightsEnabled, GetFloat(material, "_SpecularHighlights") > 0.5f);
            SetKeyword(material, ShaderKeywords.EnvironmentReflectionsEnabled, GetFloat(material, "_EnvironmentReflections") > 0.5f);
            SetKeyword(material, ShaderKeywords.SpecularSetup, (LitWorkflowMode)GetFloat(material, "_LitWorkflowMode") == LitWorkflowMode.Specular);

            var mode = (BaseMapMode)GetFloat(material, "_BaseMapMode");
            SetKeyword(material, ShaderKeywords.NormalMapEnabled, HasSurfaceMap(material, mode, "_NormalMap", "_NormalMap2DArray", "_NormalMap3D"));
            SetKeyword(material, ShaderKeywords.MetallicMapEnabled, HasSurfaceMap(material, mode, "_MetallicMap", "_MetallicMap2DArray", "_MetallicMap3D"));
            SetKeyword(material, ShaderKeywords.SmoothnessMapEnabled, HasSurfaceMap(material, mode, "_SmoothnessMap", "_SmoothnessMap2DArray", "_SmoothnessMap3D"));
            SetKeyword(material, ShaderKeywords.SpecularMapEnabled, HasSurfaceMap(material, mode, "_SpecularMap", "_SpecularMap2DArray", "_SpecularMap3D"));
        }

        public static void SetupMaterialBlendMode(Material material)
        {
            ParticlesUberUnlitMaterialPostProcessor.SetupMaterialBlendMode(material);
        }
    }

    internal static class MaterialSetupUtility
    {
        public static float GetFloat(Material material, string propertyName)
        {
            return material.HasProperty(propertyName) ? material.GetFloat(propertyName) : 0f;
        }

        public static void SetKeyword(Material material, string keyword, bool state)
        {
            if (state)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }

        public static void SetMirrorKeyword(Material material, string enabledPropertyName, string texturePropertyName)
        {
            SetKeyword(material, ShaderKeywords.BaseSamplerStatePointMirror, false);
            SetKeyword(material, ShaderKeywords.BaseSamplerStateLinearMirror, false);
            SetKeyword(material, ShaderKeywords.BaseSamplerStateTrilinearMirror, false);
            if (GetFloat(material, enabledPropertyName) < 0.5f || material.GetTexture(texturePropertyName) == null)
            {
                return;
            }

            switch (material.GetTexture(texturePropertyName).filterMode)
            {
                case FilterMode.Point:
                    SetKeyword(material, ShaderKeywords.BaseSamplerStatePointMirror, true);
                    break;
                case FilterMode.Bilinear:
                    SetKeyword(material, ShaderKeywords.BaseSamplerStateLinearMirror, true);
                    break;
                case FilterMode.Trilinear:
                    SetKeyword(material, ShaderKeywords.BaseSamplerStateTrilinearMirror, true);
                    break;
            }
        }

        public static bool HasSurfaceMap(Material material, BaseMapMode mode, string map2D, string map2DArray, string map3D)
        {
            var propertyName = mode == BaseMapMode.SingleTexture ? map2D : mode == BaseMapMode.FlipBook ? map2DArray : map3D;
            return material.GetTexture(propertyName) != null;
        }

        public static bool IsFlipBook<T>(T mode) where T : Enum
        {
            var value = Convert.ToInt32(mode);
            return value == 1 || value == 2;
        }

        public static bool IsUiShader(Material material)
        {
            return material.shader != null && material.shader.name.StartsWith("Nova/UIParticles/", StringComparison.Ordinal);
        }
    }
}

namespace Nova.Editor.Core.Scripts.Optimizer
{
    [Flags]
    public enum OptionalShaderPass
    {
        None = 0,
        DepthOnly = 1,
        DepthNormals = 1 << 1,
        ShadowCaster = 1 << 2,
    }

}

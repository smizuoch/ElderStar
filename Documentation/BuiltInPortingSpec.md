# Built-in Porting Spec

Phase 0 scope: audit only. No runtime code is implemented by this document.

Original source inspected:

- `NovaShader/Assets/Nova/package.json`
- `NovaShader/Assets/Nova/Runtime/Nova.asmdef`
- `NovaShader/Assets/Nova/Editor/Nova.Editor.asmdef`
- `NovaShader/Assets/Nova/Runtime/Core/Scripts/*.cs`
- `NovaShader/Assets/Nova/Runtime/Core/Shaders/*.{shader,hlsl}`
- `NovaShader/Assets/Nova/Editor/Core/Scripts/*.cs`

The original package is `jp.co.cyberagent.nova` version `3.6.0` for Unity `2022.3`. Its package manifest declares no package dependency, but the runtime assembly references two external GUID assemblies and the runtime code/shaders directly use URP and RenderGraph APIs. The Built-in port must not keep those runtime URP references.

## Current TODO

Current TODO: Authoring parity follow-up (2026-10-12): restore missing Inspector foldout icons with failing tests, then audit original UI/API/declarations, material state transitions and optimizer behavior. The 2026-10-10 release evidence remains historical; rerun acceptance after these changes.

Authoring parity worklist:

- [ ] Restore and verify original Inspector resources and foldout interactions.
- [ ] Compare every shader property declaration and original editor API/UI contract.
- [ ] Differential-test material setup and shader reassignment, including legacy texture modes and multiple selection/Undo.
- [ ] Restore practical optimizer behavior omitted by the initial port; verify resulting shaders and rendering.
- [ ] Re-audit remaining compatibility differences, implement practical improvements, and document irreducible pipeline differences precisely.
- [ ] Run both Unity suites and package/player verification; refresh delivery artifacts and evidence.

Completion worklist (2026-10-10):

- [x] UI UberLit environment reflections and shared particle features (all four shaders).
- [x] UI UberLit additional lights and Canvas masks.
- [x] Built-in distortion runtime and rendering/cleanup tests.
- [x] Optimized shader generator/replacer output compilation and rendering.
- [x] Complete original editor APIs/inspectors and actual billboard ParticleSystem/vertex-stream coverage.
- [x] Particle mesh instancing and distortion particle rendering.
- [x] Distortion editor selection and picking.
- [x] Repeat isolated UPM installation, package-cache include resolution and standalone player/sample validation on the final release tarball.
- [x] Samples, migration utilities, installation and camera setup documentation.
- [x] Audit imported normal-map packing and Lit surface-map UV behavior against the original.
- [x] Review all original properties/keywords/features against implementation; resolve every pending matrix entry with evidence and documented fallbacks.
- [x] Full EditMode and graphics-enabled PlayMode on installed Unity 2022.3 and Unity 6; compiler diagnostics.
- [x] Final visual regression with explicit thresholds; preserve all existing golden images and baselines.

## Risk-Ordered TODO

1. Package skeleton with no URP runtime dependency.
   - Dependency: none.
   - Risk: all later work is blocked if the package cannot import in a Built-in-only Unity project.
   - Output: package files, runtime/editor asmdefs, empty or fake shader assets discoverable by `Shader.Find`.
2. Shader API preservation tests.
   - Dependency: package skeleton.
   - Risk: losing material property names or custom editor bindings silently breaks existing materials.
   - Output: EditMode tests for shader names, property inventories, custom editors, and forbidden URP references.
3. Built-in shader compatibility layer.
   - Dependency: shader API tests.
   - Risk: URP HLSL helpers provide transforms, depth sampling, fog, lighting structs, instancing, and shadow helpers that Built-in does differently.
   - Output: local includes replacing URP `Core.hlsl`, `DeclareDepthTexture.hlsl`, `Unlit.hlsl`, `Lighting.hlsl`, `Shadows.hlsl`, and URP texture declarations where practical.
4. Particles UberUnlit forward path.
   - Dependency: compatibility layer.
   - Risk: largest non-lit feature surface with alpha transitions, flow, parallax, color correction, transparency, soft particles, depth fade, vertex deformation, and shadow caster variants.
   - Output: Built-in shader `Nova/Particles/UberUnlit` with all original properties and keywords.
5. UI Particles UberUnlit.
   - Dependency: Particles UberUnlit.
   - Risk: stencil and UI clip behavior must remain compatible with Unity UI.
   - Output: Built-in shader `Nova/UIParticles/UberUnlit` with UI stencil properties and `UNITY_UI_CLIP_RECT`.
6. Particles UberLit.
   - Dependency: UberUnlit and compatibility layer.
   - Risk: `UniversalFragmentPBR`, URP light loops, shadow macros, reflection controls, and baked/global illumination have no exact Built-in equivalent.
   - Output: closest Built-in forward PBR approximation with documented differences.
7. UI Particles UberLit.
   - Dependency: Particles UberLit and UI UberUnlit.
   - Risk: lit UI particles combine the highest shader feature count with UI clipping and Built-in lighting approximation.
8. Distortion.
   - Dependency: render path decision for Built-in.
   - Risk: original uses `ScriptableRendererFeature`, `ScriptableRenderPass`, RenderGraph, custom `DistortedUvBuffer` LightMode, and full-screen blit.
   - Output: documented Built-in approximation, likely camera command buffer or camera event path, or Unsupported if no robust package-level hook is accepted.
9. Optimized Shader Generator and Replacer.
   - Dependency: shader names and pass topology stabilized.
   - Risk: optimizer currently strips URP optional passes by LightMode and rewrites include paths; Built-in pass names and fallback names must be updated without changing public API.
10. Samples and migration utilities.
    - Dependency: target shaders stable.
    - Risk: sample materials can hide missing properties or wrong render queues.
11. Final shader diagnostics, PlayMode, and visual regression.
    - Dependency: all shader/runtime paths.
    - Risk: compile success is not enough; visuals and pass participation need coverage.

## URP-Specific Dependencies

### C# runtime dependencies

These files cannot be copied into the Built-in runtime unchanged:

- `Runtime/Core/Scripts/ScreenSpaceDistortion.cs`
  - Uses `UnityEngine.Rendering.Universal`, `ScriptableRendererFeature`, `ScriptableRenderer`, `RenderingData`, `UniversalRenderPipelineDebugDisplaySettings`, `RenderGraphSettings`, `RTHandle`, `RenderingUtils.ReAllocateIfNeeded`, camera target descriptors, and `renderer.EnqueuePass`.
- `Runtime/Core/Scripts/DistortedUvBufferPass.cs`
  - Uses `UnityEngine.Rendering.Universal`, `ScriptableRenderPass`, `RenderingData`, `RenderPassEvent`, `RTHandle`, `ConfigureTarget`, `ConfigureClear`, `CreateDrawingSettings`, URP renderer camera depth target, and URP-style renderer lists.
- `Runtime/Core/Scripts/ApplyDistortionPass.cs`
  - Uses `UnityEngine.Rendering.Universal`, `ScriptableRenderPass`, `RenderingData`, `RenderPassEvent`, `CoreUtils.CreateEngineMaterial`, camera color target handles, and `Blit(cmd, ref renderingData, material)`.
- `Runtime/Core/Scripts/DistortedUvBufferPass_RenderGraph.cs`
  - Uses `UnityEngine.Rendering.RenderGraphModule`, `UnityEngine.Rendering.Universal`, `RenderGraph`, `UniversalRenderingData`, `UniversalCameraData`, `UniversalLightData`, `UniversalResourceData`, `TextureHandle`, `RendererListHandle`, `RasterGraphContext`, and `RenderingUtils.CreateDrawingSettings`.
- `Runtime/Core/Scripts/ApplyDistortionPass_RenderGraph.cs`
  - Uses `UnityEngine.Rendering.RenderGraphModule`, `UnityEngine.Rendering.Universal`, `RenderGraph`, `UniversalCameraData`, `UniversalResourceData`, `TextureHandle`, `RasterGraphContext`, and `Blitter.BlitTexture`.
- `Runtime/Core/Scripts/DistortionContextItem_RenderGraph.cs`
  - Uses `UnityEngine.Rendering.RenderGraphModule.ContextItem` and `TextureHandle`.
- `Runtime/Nova.asmdef`
  - References two external GUID assemblies. They are not defined in the audited package and must be resolved or removed in the Built-in package. The runtime source confirms the effective dependency is URP/RenderGraph.

Editor files mentioning `DepthOnly`, `DepthNormals`, or `ShadowCaster` are not automatically URP-only because Built-in supports some of those pass names, but the optimizer behavior must be retested against the Built-in pass topology.

### Shader and HLSL dependencies

- `Runtime/Core/Shaders/Particles.hlsl`
  - Includes `Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl`.
- `Runtime/Core/Shaders/ParticlesApplyDistortion.shader`
  - Has SubShader tag `"RenderPipeline" = "UniversalPipeline"`.
  - Includes URP `Core.hlsl`.
  - Includes SRP Core `Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl`.
- `Runtime/Core/Shaders/ParticlesDistortion.shader`
  - Has SubShader tag `"RenderPipeline" = "UniversalPipeline"`.
  - Uses LightMode `DistortedUvBuffer`, consumed by the URP renderer feature.
- `Runtime/Core/Shaders/ParticlesDistortionForward.hlsl`
  - Includes URP `Core.hlsl`.
- `Runtime/Core/Shaders/ParticlesDistortionEditor.hlsl`
  - Includes URP `Core.hlsl`.
- `Runtime/Core/Shaders/ParticlesUberDepthNormalsCore.hlsl`
  - Includes URP `Core.hlsl`.
  - Implements URP-style DepthOnly and DepthNormals behavior.
- `Runtime/Core/Shaders/ParticlesUberLit.shader`
  - Has SubShader tag `"RenderPipeline" = "UniversalPipeline"`.
  - Uses LightMode `UniversalForward`, `DepthNormals`, `DepthOnly`, and `ShadowCaster`.
  - Includes URP `ProbeVolumeVariants.hlsl`.
  - Includes SRP Core `FoveatedRenderingKeywords.hlsl`.
- `Runtime/Core/Shaders/ParticlesUberLitForward.hlsl`
  - Includes URP `Core.hlsl` and `Lighting.hlsl`.
  - Calls `UniversalFragmentPBR`.
- `Runtime/Core/Shaders/ParticlesUberShadowCaster.hlsl`
  - Includes URP `Core.hlsl` and `Shadows.hlsl`.
  - Relies on URP shadow caster constants set by `UnityEngine.Rendering.Universal.ShadowUtils`.
- `Runtime/Core/Shaders/ParticlesUberUnlit.shader`
  - Has SubShader tag `"RenderPipeline" = "UniversalPipeline"`.
  - Uses LightMode `UniversalForward`, `DepthNormals`, `DepthOnly`, and `ShadowCaster`.
- `Runtime/Core/Shaders/ParticlesUberUnlitForward.hlsl`
  - Includes URP `Core.hlsl` and `Unlit.hlsl`.
- `Runtime/Core/Shaders/ParticlesUberUnlitEditor.hlsl`
  - Includes URP `Core.hlsl`.
- `Runtime/Core/Shaders/UIParticlesUberLit.shader`
  - Has SubShader tag `"RenderPipeline" = "UniversalPipeline"`.
  - Uses LightMode `UniversalForward`.
- `Runtime/Core/Shaders/UIParticlesUberUnlit.shader`
  - Has SubShader tag `"RenderPipeline" = "UniversalPipeline"`.
  - Uses LightMode `UniversalForward`.

## Shader Inventory

### Shader names, custom editors, and passes

| Shader | File | Custom editor | Passes and LightMode tags |
| --- | --- | --- | --- |
| `Hidden/Nova/Particles/ApplyDistortion` | `ParticlesApplyDistortion.shader` | none | Pass 1: name none, LightMode none |
| `Nova/Particles/Distortion` | `ParticlesDistortion.shader` | `Nova.Editor.Core.Scripts.ParticlesDistortionGUI` | Pass 1: LightMode `DistortedUvBuffer`; Pass 2: LightMode `SceneSelectionPass`; Pass 3: LightMode `Picking` |
| `Nova/Particles/UberUnlit` | `ParticlesUberUnlit.shader` | `Nova.Editor.Core.Scripts.ParticlesUberUnlitGUI` | Pass 1: LightMode `UniversalForward`; Pass 2: LightMode `SceneSelectionPass`; Pass 3: LightMode `Picking`; Pass 4: LightMode `DepthNormals`; Pass 5: LightMode `DepthOnly`; Pass 6: name `ShadowCaster`, LightMode `ShadowCaster` |
| `Nova/UIParticles/UberUnlit` | `UIParticlesUberUnlit.shader` | `Nova.Editor.Core.Scripts.UIParticlesUberUnlitGUI` | Pass 1: LightMode `UniversalForward` |
| `Nova/Particles/UberLit` | `ParticlesUberLit.shader` | `Nova.Editor.Core.Scripts.ParticlesUberLitGUI` | Pass 1: LightMode `UniversalForward`; Pass 2: LightMode `SceneSelectionPass`; Pass 3: LightMode `Picking`; Pass 4: LightMode `DepthNormals`; Pass 5: LightMode `DepthOnly`; Pass 6: name `ShadowCaster`, LightMode `ShadowCaster` |
| `Nova/UIParticles/UberLit` | `UIParticlesUberLit.shader` | `Nova.Editor.Core.Scripts.UIParticlesUberLitGUI` | Pass 1: LightMode `UniversalForward` |

Built-in pass mapping proposal:

- `UniversalForward` -> `ForwardBase` for the primary Built-in forward pass. Add `ForwardAdd` only after lit tests require per-pixel additional light parity.
- `DepthOnly` -> Built-in compatible depth-only pass, likely `LightMode` `DepthOnly` or untagged depending on diagnostics.
- `DepthNormals` -> Approximate or omit until a Built-in consumer is defined; Built-in has no URP depth normals prepass contract.
- `ShadowCaster` -> Built-in `ShadowCaster`.
- `SceneSelectionPass` and `Picking` -> keep editor-only passes if they compile without URP includes, otherwise document as approximate editor tooling.
- `DistortedUvBuffer` -> custom Built-in command-buffer draw path if distortion is implemented; not consumed by Built-in automatically.

### Property inventories

Property inventories are expressed as reusable sets to avoid hiding repetition. The per-shader mapping below defines the complete property list for each shader.

#### Distortion properties

`Nova/Particles/Distortion` has 47 properties:

`_Cull`, `_ZTest`, `_BaseMap`, `_BaseMapOffsetXCoord`, `_BaseMapOffsetYCoord`, `_BaseMapChannelsX`, `_BaseMapChannelsY`, `_BaseMapRotation`, `_BaseMapRotationCoord`, `_BaseMapRotationOffsets`, `_BaseMapMirrorSampling`, `_BaseMapUnpackNormal`, `_DistortionIntensityMode`, `_DistortionIntensity`, `_DistortionIntensityCoord`, `_DistortionIntensityX`, `_DistortionIntensityXCoord`, `_DistortionIntensityY`, `_DistortionIntensityYCoord`, `_FlowMap`, `_FlowMapOffsetXCoord`, `_FlowMapOffsetYCoord`, `_FlowMapChannelsX`, `_FlowMapChannelsY`, `_FlowIntensity`, `_FlowIntensityCoord`, `_FlowMapTarget`, `_FlowMapRotation`, `_FlowMapRotationCoord`, `_FlowMapRotationOffsets`, `_AlphaTransitionMode`, `_AlphaTransitionMap`, `_AlphaTransitionMapOffsetXCoord`, `_AlphaTransitionMapOffsetYCoord`, `_AlphaTransitionMapChannelsX`, `_AlphaTransitionProgress`, `_AlphaTransitionProgressCoord`, `_DissolveSharpness`, `_AlphaTransitionMapRotation`, `_AlphaTransitionMapRotationCoord`, `_AlphaTransitionMapRotationOffsets`, `_SoftParticlesEnabled`, `_SoftParticlesIntensity`, `_DepthFadeEnabled`, `_DepthFadeNear`, `_DepthFadeFar`, `_DepthFadeWidth`.

#### UberUnlit base properties

`Nova/Particles/UberUnlit` has 196 properties:

`_RenderType`, `_Cutoff`, `_TransparentBlendMode`, `_Cull`, `_QueueOffset`, `_VertexAlphaMode`, `_BlendSrc`, `_BlendDst`, `_ZWrite`, `_ZWriteOverride`, `_ZTest`, `_BaseMapMode`, `_BaseMap`, `_BaseMap2DArray`, `_BaseMap3D`, `_BaseMapProgress`, `_BaseMapProgressCoord`, `_BaseMapSliceCount`, `_BaseMapOffsetXCoord`, `_BaseMapOffsetYCoord`, `_BaseMapRotation`, `_BaseMapRotationCoord`, `_BaseMapRotationOffsets`, `_BaseMapMirrorSampling`, `_BaseMapRandomRowSelectionEnabled`, `_BaseMapRandomRowCoord`, `_BaseMapRowCount`, `_BaseMapChannel`, `_BaseMapToneMode`, `_BaseMapToneChannel`, `_BaseMapToneHighlightsColor`, `_BaseMapToneMidtonesColor`, `_BaseMapToneShadowsColor`, `_BaseMapToneHighlights`, `_BaseMapToneMidtones`, `_BaseMapToneShadows`, `_BaseMapToneHighlightsCoord`, `_BaseMapToneMidtonesCoord`, `_BaseMapToneShadowsCoord`, `_BaseMapToneBrightsColor`, `_BaseMapToneDarktonesColor`, `_BaseMapToneBrights`, `_BaseMapToneDarktones`, `_BaseMapToneBrightsCoord`, `_BaseMapToneDarktonesCoord`, `_TintAreaMode`, `_TintColorMode`, `_TintColor`, `_TintMap`, `_TintMap3D`, `_TintMap2DArray`, `_TintMap3DProgress`, `_TintMap3DProgressCoord`, `_TintMapProgress`, `_TintMapProgressCoord`, `_TintMapSliceCount`, `_TintMapOffsetXCoord`, `_TintMapOffsetYCoord`, `_TintBlendRate`, `_TintBlendRateCoord`, `_TintRimProgress`, `_TintRimProgressCoord`, `_TintRimSharpness`, `_TintRimSharpnessCoord`, `_InverseTintRim`, `_TintMapRandomRowSelectionEnabled`, `_TintMapRandomRowCoord`, `_TintMapRowCount`, `_TintMapRotation`, `_TintMapRotationCoord`, `_TintMapRotationOffsets`, `_FlowMap`, `_FlowMapOffsetXCoord`, `_FlowMapOffsetYCoord`, `_FlowMapRotation`, `_FlowMapRotationCoord`, `_FlowMapRotationOffsets`, `_FlowMapChannelsX`, `_FlowMapChannelsY`, `_FlowIntensity`, `_FlowIntensityCoord`, `_FlowMapTarget`, `_ParallaxMapMode`, `_ParallaxMap`, `_ParallaxMap2DArray`, `_ParallaxMap3D`, `_ParallaxMapProgress`, `_ParallaxMapProgressCoord`, `_ParallaxMapOffsetXCoord`, `_ParallaxMapOffsetYCoord`, `_ParallaxMapChannel`, `_ParallaxMapSliceCount`, `_ParallaxStrength`, `_ParallaxMapTarget`, `_ParallaxMapRandomRowSelectionEnabled`, `_ParallaxMapRandomRowCoord`, `_ParallaxMapRowCount`, `_ColorCorrectionMode`, `_GradientMap`, `_AlphaTransitionMode`, `_AlphaTransitionMapMode`, `_AlphaTransitionMap`, `_AlphaTransitionMap2DArray`, `_AlphaTransitionMap3D`, `_AlphaTransitionMapProgress`, `_AlphaTransitionMapProgressCoord`, `_AlphaTransitionMapOffsetXCoord`, `_AlphaTransitionMapOffsetYCoord`, `_AlphaTransitionMapRotation`, `_AlphaTransitionMapRotationCoord`, `_AlphaTransitionMapRotationOffsets`, `_AlphaTransitionMapChannelsX`, `_AlphaTransitionMapSliceCount`, `_AlphaTransitionProgress`, `_AlphaTransitionProgressCoord`, `_DissolveSharpness`, `_AlphaTransitionSecondTextureBlendMode`, `_AlphaTransitionMapSecondTextureMapMode`, `_AlphaTransitionMapSecondTexture`, `_AlphaTransitionMapSecondTexture2DArray`, `_AlphaTransitionMapSecondTexture3D`, `_AlphaTransitionMapSecondTextureProgress`, `_AlphaTransitionMapSecondTextureProgressCoord`, `_AlphaTransitionMapSecondTextureOffsetXCoord`, `_AlphaTransitionMapSecondTextureOffsetYCoord`, `_AlphaTransitionMapSecondTextureRotation`, `_AlphaTransitionMapSecondTextureRotationCoord`, `_AlphaTransitionMapSecondTextureRotationOffsets`, `_AlphaTransitionMapSecondTextureChannelsX`, `_AlphaTransitionMapSecondTextureSliceCount`, `_AlphaTransitionProgressSecondTexture`, `_AlphaTransitionProgressCoordSecondTexture`, `_DissolveSharpnessSecondTexture`, `_AlphaTransitionMapRandomRowSelectionEnabled`, `_AlphaTransitionMapRandomRowCoord`, `_AlphaTransitionMapRowCount`, `_AlphaTransitionMapSecondTextureRandomRowSelectionEnabled`, `_AlphaTransitionMapSecondTextureRandomRowCoord`, `_AlphaTransitionMapSecondTextureRowCount`, `_EmissionAreaType`, `_EmissionMapMode`, `_EmissionMap`, `_EmissionMap2DArray`, `_EmissionMap3D`, `_EmissionMapProgress`, `_EmissionMapProgressCoord`, `_EmissionMapOffsetXCoord`, `_EmissionMapOffsetYCoord`, `_EmissionMapRotation`, `_EmissionMapRotationCoord`, `_EmissionMapRotationOffsets`, `_EmissionMapChannelsX`, `_EmissionMapSliceCount`, `_EmissionColorType`, `_EmissionColor`, `_EmissionColorRamp`, `_EmissionIntensity`, `_EmissionIntensityCoord`, `_KeepEdgeTransparency`, `_EmissionMapRandomRowSelectionEnabled`, `_EmissionMapRandomRowCoord`, `_EmissionMapRowCount`, `_RimTransparencyEnabled`, `_RimTransparencyProgress`, `_RimTransparencyProgressCoord`, `_RimTransparencySharpness`, `_RimTransparencySharpnessCoord`, `_InverseRimTransparency`, `_LuminanceTransparencyEnabled`, `_LuminanceTransparencyProgress`, `_LuminanceTransparencyProgressCoord`, `_LuminanceTransparencySharpness`, `_LuminanceTransparencySharpnessCoord`, `_InverseLuminanceTransparency`, `_SoftParticlesEnabled`, `_SoftParticlesIntensity`, `_DepthFadeEnabled`, `_DepthFadeNear`, `_DepthFadeFar`, `_DepthFadeWidth`, `_VertexDeformationEnabled`, `_VertexDeformationMap`, `_VertexDeformationMapOffsetXCoord`, `_VertexDeformationMapOffsetYCoord`, `_VertexDeformationMapChannel`, `_VertexDeformationIntensity`, `_VertexDeformationIntensityCoord`, `_VertexDeformationBaseValue`, `_ShadowCasterEnabled`, `_ShadowCasterApplyVertexDeformation`, `_ShadowCasterAlphaTestEnabled`, `_ShadowCasterAlphaCutoff`, `_ShadowCasterAlphaAffectedByTintColor`, `_ShadowCasterAlphaAffectedByFlowMap`, `_ShadowCasterAlphaAffectedByAlphaTransitionMap`, `_ShadowCasterAlphaAffectedByTransparencyLuminance`.

#### Lit property additions

`Nova/Particles/UberLit` and `Nova/UIParticles/UberLit` add these 23 properties to the UberUnlit base set:

`_LitWorkflowMode`, `_LitReceiveShadows`, `_SpecularHighlights`, `_EnvironmentReflections`, `_NormalMap`, `_NormalMap2DArray`, `_NormalMap3D`, `_NormalMapBumpScale`, `_SpecularMap`, `_SpecularMap2DArray`, `_SpecularMap3D`, `_SpecularColor`, `_SpecularMapChannelsX`, `_MetallicMap`, `_MetallicMap2DArray`, `_MetallicMap3D`, `_Metallic`, `_MetallicMapChannelsX`, `_SmoothnessMap`, `_SmoothnessMap2DArray`, `_SmoothnessMap3D`, `_Smoothness`, `_SmoothnessMapChannelsX`.

#### UI property additions

`Nova/UIParticles/UberUnlit` and `Nova/UIParticles/UberLit` add these 6 UI/stencil properties:

`_StencilComp`, `_Stencil`, `_StencilOp`, `_StencilWriteMask`, `_StencilReadMask`, `_ColorMask`.

#### Per-shader property mapping

- `Hidden/Nova/Particles/ApplyDistortion`: no properties.
- `Nova/Particles/Distortion`: Distortion properties.
- `Nova/Particles/UberUnlit`: UberUnlit base properties.
- `Nova/UIParticles/UberUnlit`: UberUnlit base properties plus UI property additions.
- `Nova/Particles/UberLit`: UberUnlit base properties plus Lit property additions.
- `Nova/UIParticles/UberLit`: UberUnlit base properties plus Lit property additions plus UI property additions.

### Keyword inventories

#### Distortion keywords

`Nova/Particles/Distortion` uses:

`_BASE_SAMPLER_STATE_LINEAR_MIRROR`, `_BASE_SAMPLER_STATE_POINT_MIRROR`, `_BASE_SAMPLER_STATE_TRILINEAR_MIRROR`, `_DEPTH_FADE_ENABLED`, `_DISSOLVE_TRANSITION_ENABLED`, `_FADE_TRANSITION_ENABLED`, `_FLOW_MAP_ENABLED`, `_FLOW_MAP_TARGET_ALPHA_TRANSITION`, `_FLOW_MAP_TARGET_BASE`, `_SOFT_PARTICLES_ENABLED`.

#### UberUnlit base keywords

`Nova/Particles/UberUnlit` uses:

`DEBUG_DISPLAY`, `_ALPHAMODULATE_ENABLED`, `_ALPHATEST_ENABLED`, `_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE`, `_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY`, `_ALPHA_TRANSITION_MAP_MODE_2D`, `_ALPHA_TRANSITION_MAP_MODE_2D_ARRAY`, `_ALPHA_TRANSITION_MAP_MODE_3D`, `_ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED`, `_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D`, `_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D_ARRAY`, `_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_3D`, `_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_RANDOM_ROW_SELECTION_ENABLED`, `_BASE_MAP_CHANNEL_ENABLED`, `_BASE_MAP_MODE_2D`, `_BASE_MAP_MODE_2D_ARRAY`, `_BASE_MAP_MODE_3D`, `_BASE_MAP_RANDOM_ROW_SELECTION_ENABLED`, `_BASE_MAP_TONE_MODE_PENTONE`, `_BASE_MAP_TONE_MODE_TRITONE`, `_BASE_SAMPLER_STATE_LINEAR_MIRROR`, `_BASE_SAMPLER_STATE_POINT_MIRROR`, `_BASE_SAMPLER_STATE_TRILINEAR_MIRROR`, `_CASTING_PUNCTUAL_LIGHT_SHADOW`, `_DEPTH_FADE_ENABLED`, `_DISSOLVE_TRANSITION_ENABLED`, `_EMISSION_AREA_ALL`, `_EMISSION_AREA_ALPHA`, `_EMISSION_AREA_MAP`, `_EMISSION_COLOR_BASECOLOR`, `_EMISSION_COLOR_COLOR`, `_EMISSION_COLOR_MAP`, `_EMISSION_MAP_MODE_2D`, `_EMISSION_MAP_MODE_2D_ARRAY`, `_EMISSION_MAP_MODE_3D`, `_EMISSION_MAP_RANDOM_ROW_SELECTION_ENABLED`, `_FADE_TRANSITION_ENABLED`, `_FLOW_MAP_ENABLED`, `_FLOW_MAP_TARGET_ALPHA_TRANSITION`, `_FLOW_MAP_TARGET_BASE`, `_FLOW_MAP_TARGET_EMISSION`, `_FLOW_MAP_TARGET_TINT`, `_GRADIENT_MAP_ENABLED`, `_GREYSCALE_ENABLED`, `_PARALLAX_MAP_MODE_2D`, `_PARALLAX_MAP_MODE_2D_ARRAY`, `_PARALLAX_MAP_MODE_3D`, `_PARALLAX_MAP_RANDOM_ROW_SELECTION_ENABLED`, `_PARALLAX_MAP_TARGET_BASE`, `_PARALLAX_MAP_TARGET_EMISSION`, `_PARALLAX_MAP_TARGET_TINT`, `_SHADOW_CASTER_ALPHA_TEST_ENABLED`, `_SOFT_PARTICLES_ENABLED`, `_TINT_AREA_ALL`, `_TINT_AREA_RIM`, `_TINT_COLOR_ENABLED`, `_TINT_MAP_3D_ENABLED`, `_TINT_MAP_ENABLED`, `_TINT_MAP_MODE_2D_ARRAY`, `_TINT_MAP_RANDOM_ROW_SELECTION_ENABLED`, `_TRANSPARENCY_BY_LUMINANCE`, `_TRANSPARENCY_BY_RIM`, `_VERTEX_ALPHA_AS_TRANSITION_PROGRESS`, `_VERTEX_DEFORMATION_ENABLED`.

#### Lit keyword additions

`Nova/Particles/UberLit` adds these 18 keywords to the UberUnlit base:

`EVALUATE_SH_MIXED`, `EVALUATE_SH_VERTEX`, `_ADDITIONAL_LIGHTS`, `_ADDITIONAL_LIGHTS_VERTEX`, `_ADDITIONAL_LIGHT_SHADOWS`, `_CLUSTER_LIGHT_LOOP`, `_ENVIRONMENT_REFLECTIONS_ENABLED`, `_FORWARD_PLUS`, `_MAIN_LIGHT_SHADOWS`, `_MAIN_LIGHT_SHADOWS_CASCADE`, `_METALLIC_MAP_ENABLED`, `_NORMAL_MAP_ENABLED`, `_RECEIVE_SHADOWS_ENABLED`, `_SHADOWS_SOFT`, `_SMOOTHNESS_MAP_ENABLED`, `_SPECULAR_HIGHLIGHTS_ENABLED`, `_SPECULAR_MAP_ENABLED`, `_SPECULAR_SETUP`.

#### UI keyword differences

UI shaders add `UNITY_UI_CLIP_RECT`.

UI shaders do not include the non-UI shadow caster-only keywords `_CASTING_PUNCTUAL_LIGHT_SHADOW` and `_SHADOW_CASTER_ALPHA_TEST_ENABLED` because they have no ShadowCaster pass.

`Nova/UIParticles/UberLit` also lacks `EVALUATE_SH_MIXED` and `EVALUATE_SH_VERTEX` compared with non-UI UberLit.

## Proposed Test Strategy

### EditMode tests

- Package import/no-URP dependency:
  - Assert runtime asmdefs do not reference `com.unity.render-pipelines.universal`, URP assembly names, or known URP GUIDs.
  - Scan package source for forbidden runtime namespaces: `UnityEngine.Rendering.Universal` and `UnityEngine.Rendering.RenderGraphModule`.
- Shader discovery:
  - Assert `Shader.Find` succeeds for all six original shader names.
- Shader API preservation:
  - For every shader, assert all required material property names exist.
  - Assert custom editor class names are present in shader text.
  - Assert material post processors set the same public keywords as the original editor code.
- Optimizer API:
  - Assert optimized shader names generated by `OptimizedShaderUtility` remain stable.
  - Assert optional pass stripping handles Built-in pass names without deleting required passes.

### Shader compile diagnostics

- Import each target shader in a Built-in-only test project.
- Query `ShaderUtil` compiler messages in EditMode.
- Treat warnings that indicate missing includes, unknown macros, unsupported texture types, or invalid pass tags as failing until explicitly documented.
- Compile smoke permutations:
  - Base/default material.
  - Alpha test.
  - 2D array and 3D texture mode keywords.
  - Flow, parallax, alpha transition, emission, transparency, depth fade, vertex deformation.
  - Lit metallic and specular workflows.
  - ShadowCaster enabled and alpha-tested.

### PlayMode tests

- Non-visual behavior:
  - Render queue, blend, cull, z-write, z-test, and shader pass enablement after changing material properties through custom editors/post processors.
  - UI stencil and `UNITY_UI_CLIP_RECT` clipping in a Canvas.
  - Soft particle/depth fade smoke scene with camera depth texture enabled where Built-in supports it.
- Rendering smoke:
  - Draw particle systems using each shader and assert non-empty frame output.
  - Test Built-in shadow casting for UberUnlit and UberLit when `_ShadowCasterEnabled` is enabled.
  - Test distortion only after the Built-in runtime path is chosen.

### Visual regression

- Use golden images from controlled sample scenes only.
- Do not update baselines silently.
- Compare against original URP renderings where exact parity is expected; compare against documented Built-in baselines where the compatibility matrix says Approximate.
- Required scenes:
  - UberUnlit default/additive/alpha-test.
  - Base map 2D, 2D array, 3D.
  - Flow/parallax/alpha transition/emission/transparency.
  - UberLit metallic/specular/normal/shadow receive.
  - UI clipping and stencil.
  - Distortion before/after full-screen application.

## Phase 1 TDD Plan

### First failing tests to write

1. `BuiltInPackageDoesNotReferenceUrpRuntime`
   - Fails while runtime asmdef/source still reference URP or RenderGraph.
2. `OriginalShaderNamesAreDiscoverable`
   - Fails until the Built-in package contains shader assets for all original shader names.
3. `OriginalShaderPropertiesArePreserved`
   - Fails until fake shader assets define the original property names.
4. `OriginalCustomEditorsArePreserved`
   - Fails until shader `CustomEditor` declarations and editor classes are present.

### Smallest fake implementation for Phase 1 green

- Add package/asmdef skeleton with no URP references.
- Add compile-minimal Built-in shaders for all six original shader names.
- Define all audited properties with compatible property names and basic defaults.
- Preserve `CustomEditor` declarations and editor class stubs or copied editor APIs if they compile without URP.
- Use a single simple color output pass for visible shaders and no actual feature implementation yet.
- Mark these shaders as T0/T1 only; do not claim feature completion.

### Triangulation needed after fake implementation

- Add a second shader API test that checks property counts and a sample of property types/defaults, then refine fake property declarations.
- Add material post-processor tests for keyword toggling from real material property combinations, then port editor logic.
- Add the first compile diagnostics permutation for `_ALPHATEST_ENABLED`, then implement the smallest alpha clip behavior.
- Add the first visual smoke test for UberUnlit base color, then replace fake color output with real base-map/tint behavior.
- Add a lit material test for metallic/specular workflow before porting any Built-in PBR approximation.

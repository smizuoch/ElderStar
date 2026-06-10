# Built-in Compatibility Matrix

Status values:

- Exact: expected to match original behavior in Built-in.
- Approximate: expected to work with a documented Built-in-specific difference.
- Unsupported: not planned unless a later design changes the porting approach.
- Not Applicable: original behavior is URP infrastructure that does not apply directly to Built-in.
- Pending: not implemented or verified yet.

Phase 0 records expected compatibility only. Implementation status is Pending for every runtime feature.

## Package and Editor Surface

| Feature | Original dependency | Built-in target | Expected status | Reason / fallback |
| --- | --- | --- | --- | --- |
| Package imports without URP | Runtime asmdef references external GUIDs; runtime source uses URP and RenderGraph | Remove runtime URP and RenderGraph references | Exact | Required for package goal. Failing import blocks all phases. |
| Public material property names | Shader `Properties` blocks and `MaterialPropertyNames.cs` | Preserve names | Exact | Existing materials depend on exact names. |
| Shader names | Six original shader names | Preserve names | Exact | `Shader.Find` compatibility is required. |
| Custom editor API | `Nova.Editor.Core.Scripts.*GUI` classes | Preserve public editor class names | Exact | Material inspectors and `CustomEditor` declarations depend on names. |
| Material post processors | Editor C# keyword/setup logic | Preserve behavior where shader features exist | Exact or Approximate | Exact for property/keyword setup; Approximate where the target rendering feature is approximate. |
| Optimized shader generator | URP pass names and source include rewriting | Retarget to Built-in pass topology | Approximate | Optimized output names can be preserved, but generated shader body and optional passes may differ. |

## Shaders

| Shader | Original passes | Built-in target | Expected status | Reason / fallback |
| --- | --- | --- | --- | --- |
| `Hidden/Nova/Particles/ApplyDistortion` | One full-screen URP/SRP Core blit pass | Built-in full-screen blit shader if distortion runtime is implemented | Approximate | Original includes URP `Core.hlsl` and SRP Core `Blit.hlsl`; Built-in blit uniforms and sampling differ. |
| `Nova/Particles/Distortion` | `DistortedUvBuffer`, `SceneSelectionPass`, `Picking` | Custom Built-in distortion buffer draw pass or documented fallback | Approximate | Built-in has no `ScriptableRendererFeature`; custom LightMode is not consumed automatically. |
| `Nova/Particles/UberUnlit` | `UniversalForward`, `SceneSelectionPass`, `Picking`, `DepthNormals`, `DepthOnly`, `ShadowCaster` | Built-in forward, editor selection/picking, depth, and shadow caster passes | Approximate | Unlit color features should be close; URP debug display, depth normals, depth texture helpers, and fog semantics differ. |
| `Nova/UIParticles/UberUnlit` | `UniversalForward` with UI stencil/clip | Built-in UI-compatible forward pass | Approximate | Stencil and clipping should be preserved; URP forward/debug/depth helpers differ. |
| `Nova/Particles/UberLit` | `UniversalForward`, `SceneSelectionPass`, `Picking`, `DepthNormals`, `DepthOnly`, `ShadowCaster` | Built-in forward lit approximation plus shadow caster | Approximate | `UniversalFragmentPBR`, URP light loops, Forward+, probe volumes, and URP shadow data have no exact Built-in equivalent. |
| `Nova/UIParticles/UberLit` | `UniversalForward` with UI stencil/clip | Built-in UI-compatible lit approximation | Approximate | Same lit limitations as UberLit plus UI clipping/stencil constraints. |

## Built-in Shader Compatibility Layer

Phase 3 adds local Built-in include scaffolding under `Assets/Nova/Runtime/Core/Shaders/BuiltIn/`. These files are verified for import/compile smoke only. They intentionally do not mark particle rendering, lit rendering, UI rendering, distortion runtime, optimized shader generation, or visual parity complete.

| Feature | Include surface | Implementation status | Runtime status | Notes |
| --- | --- | --- | --- | --- |
| Transform helpers | `NovaBuiltInCore.cginc` | Verified | Pending | Provides object/world/clip transforms, normal/tangent conversion, bitangent generation, view direction, safe normalize, and screen-position helpers through Built-in matrices and `UnityCG.cginc`. Feature shaders have not yet ported their full vertex data paths. |
| Texture helpers | `NovaBuiltInTexture.cginc` | Verified | Pending | Provides compile-safe 2D, 2D array, 3D, UV transform, channel selection, and mirror-UV helper surface. Mirror sampler visual parity and flip-book feature behavior remain for later phases. |
| Depth and screen helpers | `NovaBuiltInDepth.cginc` | Verified | Pending | Provides camera depth declaration/sampling, linear eye depth, soft-particle factor, depth-fade factor, and screen UV helpers. Cameras must provide a Built-in camera depth texture for runtime depth-dependent behavior. Final soft-particle and depth-fade behavior remains for feature phases. |
| Fog helpers | `NovaBuiltInFog.cginc` | Verified | Pending | Wraps Built-in fog macros for future shader passes. No target shader has real fog integration yet. |
| Instancing helpers | `NovaBuiltInInstancing.cginc` | Verified | Pending | Wraps Built-in instance ID setup/transfer macros only. Mesh/particle instancing feature behavior remains unimplemented. |
| Shadow caster helpers | `NovaBuiltInShadow.cginc` | Verified | Pending | Provides a compile-safe Built-in ShadowCaster clip-position helper. Bias and per-light behavior are Built-in approximations and not parity with the original shadow constants. |
| Lighting scaffolding | `NovaBuiltInLighting.cginc` | Verified | Approximate | Provides main-light direction/color access, ambient sampling, Lambert diffuse, and Phase 6.2A surface inputs for `Nova/Particles/UberLit`. Full UberLit PBR parity remains pending and is not claimed. |
| UI helpers | `NovaBuiltInUI.cginc` | Verified | Pending | Provides local access to Built-in UI clipping support while preserving stencil-compatible properties in target shaders. Full UI shader behavior remains later phases. |

## Common Shader Features

| Feature | Original dependency | Built-in target | Expected status | Reason / fallback |
| --- | --- | --- | --- | --- |
| Render settings: render type, blend, cull, queue, z-write, z-test | Shader properties and editor post processors | Built-in material render state | Exact | Built-in supports equivalent render states. |
| Vertex alpha as transition progress | Shader keyword/property logic | Same vertex color interpretation | Exact | Pipeline-independent math. |
| Alpha modulate and alpha test | Shader keywords and clip | Same clip/blend math | Exact | Pipeline-independent once includes are replaced. |
| Base map 2D | Texture sampling | Same sampler behavior where supported | Exact | Built-in shader code can implement directly. |
| Base map 2D array | `#pragma require 2darray` | Same if target platform supports arrays | Approximate | Platform support and Built-in shader target diagnostics must be verified. |
| Base map 3D | 3D texture sampling | Same if target platform supports 3D textures | Approximate | Platform support and shader model must be verified. |
| Mirror sampler state keywords | URP sampler definitions | Local sampler definitions | Approximate | Built-in sampler syntax differs by backend; visual parity requires testing. |
| Base map random row selection | Shader math | Same math | Exact | Pipeline-independent. |
| Base map channel extraction | Shader math | Same math | Exact | Pipeline-independent. |
| Tritone and pentone tone mapping | Shader math | Same math | Exact | Pipeline-independent. |
| Tint color, tint map, tint rim | Shader math | Same math | Exact | Pipeline-independent. |
| Flow map | Shader math and texture sampling | Same math | Exact | Pipeline-independent after texture helpers exist. |
| Parallax map | URP-inspired parallax comment, shader math | Same or simplified math | Approximate | Original references URP parallax behavior; Built-in view direction/tangent helpers differ. |
| Greyscale and gradient map | Shader math | Same math | Exact | Pipeline-independent. |
| Fade and dissolve alpha transition | Shader math | Same math | Exact | Pipeline-independent. |
| Second alpha transition texture | Shader math and sampling | Same if texture modes compile | Approximate | 2D array/3D texture support needs diagnostics. |
| Emission area, map, color source, intensity | Shader math | Same math | Exact | Pipeline-independent. |
| Rim and luminance transparency | Shader math plus view direction | Same or close view-vector math | Approximate | View direction helper differs between URP and Built-in. |
| Soft particles | URP `DeclareDepthTexture.hlsl` and depth sampling | Built-in camera depth texture sampling | Approximate | Built-in depth texture setup and macros differ; camera settings may be required. |
| Depth fade | URP depth sampling | Built-in camera depth texture sampling | Approximate | Same limitation as soft particles. |
| Vertex deformation | Shader math in vertex stage | Same math | Exact | Pipeline-independent after transform helpers exist. |
| Scene selection and picking | URP/editor passes with URP includes | Built-in-compatible editor passes | Approximate | Unity editor picking pass behavior differs by pipeline. |
| ShadowCaster pass | URP `Shadows.hlsl` and URP shadow constants | Built-in `ShadowCaster` pass | Approximate | Built-in supports shadow caster passes, but bias and punctual shadow constants differ. |
| DepthOnly pass | URP LightMode `DepthOnly` | Built-in depth-only pass | Approximate | Built-in consumers and tags differ. |
| DepthNormals pass | URP depth normals prepass | Optional Built-in approximation | Unsupported or Approximate | Built-in has no URP depth normals contract; implement only if tests demonstrate a consumer. |
| Debug display | URP `DEBUG_DISPLAY` | No Built-in equivalent | Unsupported | Preserve keyword if needed for material compatibility, but no URP debug rendering path. |

## Lit Features

| Feature | Original dependency | Built-in target | Expected status | Reason / fallback |
| --- | --- | --- | --- | --- |
| Metallic workflow | URP `UniversalFragmentPBR` surface data | Built-in PBR approximation | Approximate | Built-in Standard lighting model differs from URP. |
| Specular workflow | URP `_SPECULAR_SETUP` path | Built-in specular approximation | Approximate | Built-in has different BRDF and reflection handling. |
| Normal map 2D/2D array/3D | URP tangent/input data | Built-in tangent-space normal sampling | Approximate | Transform helpers and texture mode support need diagnostics. |
| Smoothness map | URP PBR surface data | Built-in smoothness input | Approximate | BRDF response differs. |
| Receive shadows | URP shadow macros and light data | Built-in shadow attenuation | Approximate | Built-in forward shadowing differs and may require pass variants. |
| Main light and additional lights | URP light loop and Forward+ keywords | Built-in `ForwardBase` and optional `ForwardAdd` | Approximate | Forward+ and URP additional-light keywords are not available. |
| Environment reflections | URP environment reflection controls | Built-in reflection probes | Approximate | Reflection probe sampling and controls differ. |
| Probe volumes | URP `ProbeVolumeVariants.hlsl` | No direct Built-in equivalent | Unsupported | Preserve material-facing controls only if harmless; document missing probe-volume behavior. |
| Foveated rendering keywords | SRP Core include | No Built-in equivalent | Unsupported | Remove dependency; no Built-in fallback planned. |

## UI Features

| Feature | Original dependency | Built-in target | Expected status | Reason / fallback |
| --- | --- | --- | --- | --- |
| Stencil properties | `_StencilComp`, `_Stencil`, `_StencilOp`, `_StencilWriteMask`, `_StencilReadMask`, `_ColorMask` | Preserve exact property names and stencil state | Exact | Unity UI uses the same material property contract. |
| Rect clipping | `UNITY_UI_CLIP_RECT` | Built-in UI clipping macros or local equivalent | Approximate | Macro support differs by included UI shader headers. |
| UI UberUnlit | URP forward pass | Built-in UI forward pass | Approximate | Common unlit limitations apply. |
| UI UberLit | URP forward lit pass | Built-in UI lit approximation | Approximate | Common lit limitations apply. |

## Distortion

| Feature | Original dependency | Built-in target | Expected status | Reason / fallback |
| --- | --- | --- | --- | --- |
| `ScreenSpaceDistortion` renderer feature | `ScriptableRendererFeature` | Built-in camera command buffer, camera component, or unsupported runtime feature | Approximate | Built-in lacks renderer features; a package-level hook must be designed. |
| Distorted UV buffer | `ScriptableRenderPass` rendering `DistortedUvBuffer` LightMode into RTHandle | CommandBuffer draw into temporary render texture | Approximate | Sorting, filtering, camera depth, and render target allocation differ. |
| Apply distortion | URP pre-postprocess pass and RenderGraph path | Built-in full-screen blit at a camera event | Approximate | Built-in postprocessing order and scene view handling differ. |
| RenderGraph implementation | URP RenderGraph APIs | Not applicable | Not Applicable | Built-in Render Pipeline has no URP RenderGraph path. |
| Render compatibility mode branch | URP version-specific compatibility API | Not applicable | Not Applicable | Built-in does not expose this mode. |

## Optimizer and Replacer

| Feature | Original dependency | Built-in target | Expected status | Reason / fallback |
| --- | --- | --- | --- | --- |
| Optional pass enum | `DepthOnly`, `DepthNormals`, `ShadowCaster` | Preserve enum API | Exact | Public editor API should remain stable. |
| Optional pass stripping | Regex against URP LightMode values | Retarget to Built-in pass names | Approximate | Pass set may differ, especially `DepthNormals`. |
| Include path rewriting | Original shader include layout | Retarget to Built-in include layout | Approximate | Built-in compatibility includes will have different paths. |
| Shader replacement | `Shader.Find` optimized shader names | Preserve optimized name scheme | Exact | Existing editor workflows depend on names. |

## Phase 1 Test Intent

First failing tests:

1. Built-in package has no runtime URP/RenderGraph references.
2. All original shader names resolve with `Shader.Find`.
3. All audited material properties exist on their target shaders.
4. All original custom editor declarations resolve to editor classes.

Smallest fake to pass:

- Minimal package and asmdefs.
- Minimal Built-in shaders with original names, original property declarations, preserved custom editor strings, and one simple compileable pass.
- No feature-complete rendering claims.

Triangulation points:

- Add property type/default assertions after property-name tests pass.
- Add keyword toggle tests before porting editor post processors.
- Add shader compile permutations before replacing fake shader bodies.
- Add first visual smoke for UberUnlit before porting lit or distortion.

## Phase 1.5 Verification Status

Verified on May 12, 2026 with Windows Unity launched from WSL through `Scripts/run-all-unity-editmode-tests.sh`.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| WSL-to-Windows EditMode runner | Passed | Passed | Scripts stage a temporary Windows-side project mirror because Windows Unity cannot open the WSL case-sensitive filesystem as a project. Result XML and editor logs are written under `Logs/Unity/<version>/`. |
| Package/editor/API surface tests | Passed | Passed | Existing Phase 1 checks remain green for no runtime URP references, shader discovery, property names, and custom editor declarations. |
| Shader property count and representative type tests | Passed | Passed | Counts match the Phase 0 inventory for all six skeleton shaders; representative texture, color, float, and range-compatible properties are verified. |
| Custom editor type loadability | Passed | Passed | The five original `Nova.Editor.Core.Scripts.*GUI` type names compile and load as `ShaderGUI` stubs. |
| Shader import/compiler diagnostics | Passed | Passed | All six fake skeleton shaders import with no blocking compiler diagnostics. This is a Phase 1.5 skeleton check only, not runtime rendering completion. |
| Forbidden runtime URP/RenderGraph scan | Passed | Passed | Runtime source and runtime asmdef scan found no `UnityEngine.Rendering.Universal`, `UnityEngine.Rendering.RenderGraphModule`, `com.unity.render-pipelines.universal`, `Unity.RenderPipelines.Universal`, or runtime asmdef `GUID:` references. |

Phase 1.5 does not mark any runtime shader feature complete. It verifies only the package/editor/API surface, shader discovery, property inventory, custom editor type loadability, no runtime URP/RenderGraph references, and import/compiler diagnostics for the fake Built-in shader skeleton.

## Phase 2 Verification Status

Phase 2 preserves shader/material/editor API behavior before replacing the fake shader bodies. It does not implement Built-in rendering, visual parity, distortion runtime, UberUnlit runtime features, Lit runtime features, or UI rendering behavior.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Shader keyword inventory | Passed | Passed | Target shaders declare the audited keyword surface. UI shaders declare `UNITY_UI_CLIP_RECT` and intentionally omit non-UI ShadowCaster-only keywords; UI UberLit also omits `EVALUATE_SH_MIXED` and `EVALUATE_SH_VERTEX`. |
| Material render state setup | Passed | Passed | Editor-side setup preserves/applies `_RenderType`, `_TransparentBlendMode`, `_Cull`, `_QueueOffset`, `_BlendSrc`, `_BlendDst`, `_ZWrite`, `_ZWriteOverride`, and `_ZTest` for representative opaque, cutout, transparent, additive, and multiply cases. |
| Feature keyword setup | Passed | Passed | Representative unlit, lit, distortion, and UI material properties toggle the preserved material keywords through the same post-processor path used by the `ShaderGUI` classes. Runtime shader math remains Pending. |
| Custom editor surface | Passed | Passed | Original `Nova.Editor.Core.Scripts.*GUI` class names remain loadable as `ShaderGUI` types and route validation/assignment through material setup. |
| Material post-processors | Passed | Passed | Public setup entry points are preserved for distortion, UberUnlit, and UberLit. UI shaders reuse the Uber setup path while preserving stencil and `_ColorMask` properties. |
| Optimizer/replacer public API surface | Passed | Passed | `OptionalShaderPass`, `OptimizedShaderGenerator.Generate`, `OptimizedShaderReplacer.Replace`, and replacer settings are discoverable only as API-preservation stubs. Generation/replacement behavior remains Pending for the optimizer phase. |
| Shader import/compiler diagnostics | Passed | Passed | Fake shader bodies still compile after keyword declaration additions. This is not runtime feature completion. |

Phase 2 compatibility annotations:

- Keyword preservation is Exact for the audited public surface, including URP-only compatibility keywords that currently have no Built-in runtime effect.
- Material setup and post-processor keyword behavior are Exact for the representative editor rules covered by EditMode tests.
- Optimizer and replacer behavior is still Pending except for public API discovery.
- Runtime rendering rows above remain Pending/Approximate/Unsupported as previously documented; no visual baseline or runtime shader feature is complete in Phase 2.

## Phase 3 Verification Status

Phase 3 creates a local Built-in shader compatibility include layer and compile smoke coverage for that layer. It does not implement real UberUnlit, UI UberUnlit, UberLit, UI UberLit, distortion runtime, CommandBuffer distortion, optimized shader generation behavior, PlayMode behavior, visual regression, or golden images.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Local Built-in include files | Passed | Passed | Core, texture, depth, fog, instancing, lighting, shadow, and UI include files exist under the runtime shader directory and contain no runtime dependency on the original pipeline package. |
| Include smoke shader diagnostics | Passed | Passed | Hidden test shaders import the local includes and query shader compiler diagnostics for blocking include, macro, sampler, texture, and syntax failures. |
| Target shader skeleton integration | Passed | Passed | Six preserved shader names include the local Built-in core layer and use `NovaObjectToClipPos`; fake color output remains intentionally unchanged. |
| Forbidden runtime shader token scan | Passed | Passed | Static scan is scoped to `Assets/Nova/Runtime/Core/Shaders` and excludes the untracked original reference directory. |
| Material/editor API regression | Passed | Passed | Phase 1.5 and Phase 2 property, keyword, custom editor, material setup, and optimizer/replacer API discovery tests remain green. |

Phase 3 compatibility annotations:

- Include/API scaffolding is implemented and compile-verified; runtime visual features remain Pending or Approximate as documented above.
- Depth helpers require a camera depth texture when future feature passes consume soft particles or depth fade.
- Texture array and 3D texture helpers are smoke-compiled, but platform-specific visual behavior and feature math are still unverified.
- Shadow and lighting helpers are intentionally minimal Built-in surfaces for later passes, not original lighting or shadow parity.

## Phase 4.1 Verification Status

Phase 4.1 begins `Nova/Particles/UberUnlit` only. It replaces the fake forward color output with a minimal Built-in `ForwardBase` unlit path. This phase does not complete the full UberUnlit feature set, UI shaders, Lit shaders, distortion, optimizer/replacer behavior, visual regression, or golden images.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| `Nova/Particles/UberUnlit` forward pass | Passed | Passed | Primary pass uses Built-in `ForwardBase`, local Built-in includes, object-to-clip transform, UV0, vertex color, `_BaseMap` 2D sampling, material blend/depth/cull/z-test properties, and preserved `CustomEditor`. |
| Base map 2D smoke | Passed | Passed | PlayMode render-to-texture smoke verifies a solid `_BaseMap` 2D texture affects the center pixel. 2D array and 3D base-map modes remain pending. |
| Tint color smoke | Passed | Passed | PlayMode smoke uses the material setup path to enable `_TINT_AREA_ALL` and `_TINT_COLOR_ENABLED`, then verifies `_TintColor` changes output. Tint maps, tint rim, tint flip-book modes, and tint flow/parallax targets remain pending. |
| Vertex color smoke | Passed | Passed | PlayMode smoke verifies mesh vertex color multiplies the minimal forward output. `_VERTEX_ALPHA_AS_TRANSITION_PROGRESS` is preserved as a compile surface only in this phase. |
| Alpha test smoke | Passed | Passed | PlayMode smoke verifies `_ALPHATEST_ENABLED` clips below `_Cutoff`. Fade/dissolve alpha transitions and second transition textures remain pending. |
| Render state regression | Passed | Passed | Phase 2 material setup tests remain green for `_RenderType`, `_TransparentBlendMode`, `_Cull`, `_QueueOffset`, `_BlendSrc`, `_BlendDst`, `_ZWrite`, `_ZWriteOverride`, and `_ZTest`. |
| Shader compiler diagnostics | Passed | Passed | Representative default, base 2D, alpha test, alpha modulate, tint color, and vertex color material states have no blocking shader compiler diagnostics. |
| Forbidden URP/RenderGraph scan | Passed | Passed | Static validation scans `Assets/Nova/Runtime`, `Assets/Nova/Editor`, `Assets/Nova/package.json`, and `Packages/manifest.json`; it excludes the untracked `NovaShader/` reference directory. |

Phase 4.1 compatibility annotations:

- `Nova/Particles/UberUnlit` base 2D forward rendering is now implemented for the minimal unlit path only.
- The implemented tint path is limited to `_TINT_AREA_ALL` with `_TINT_COLOR_ENABLED`, matching the original material setup combination for single tint color. Rim tint and tint texture modes are still Pending.
- Vertex color multiplication is implemented for the default path. Vertex alpha as transition progress remains a preserved keyword surface until alpha transition support is ported.
- Alpha clipping is implemented for `_ALPHATEST_ENABLED` and `_Cutoff`; alpha transition, dissolve, luminance/rim transparency, soft particles, and depth fade remain Pending.
- The following `Nova/Particles/UberUnlit` features remain Pending: base map 2D array, base map 3D, random row selection, channel extraction, tritone/pentone, flow map, parallax map, greyscale/gradient map, fade/dissolve alpha transition, second alpha transition texture, emission, rim transparency, luminance transparency, soft particles, depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, and Picking.

## Phase 4.2A Verification Status

Phase 4.2A expands only `Nova/Particles/UberUnlit` base texture handling on the Phase 4.1 Built-in forward path. It does not implement flow map, parallax map, tint map expansion, alpha transitions, emission, transparency, soft particles, depth fade, vertex deformation, additional passes, UI shaders, Lit shaders, distortion runtime, optimizer behavior, visual regression, or golden images.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Base map 2D array | Passed | Passed | `_BASE_MAP_MODE_2D_ARRAY` samples `_BaseMap2DArray` using original-compatible flip-book progress and `_BaseMapSliceCount`. PlayMode smoke renders a deterministic solid-color slice. |
| Base map 3D | Passed | Passed | `_BASE_MAP_MODE_3D` samples `_BaseMap3D` using original-compatible flip-book blending progress. PlayMode smoke renders a deterministic 3D layer. |
| Base map channel extraction | Passed | Passed | `_BASE_MAP_CHANNEL_ENABLED` follows the original `BaseMapChannel` rule: `0` preserves RGB, `1/2/3` select red/green/blue and write that value to RGB while preserving alpha. |
| Base map random row selection | Passed | Passed | `_BASE_MAP_RANDOM_ROW_SELECTION_ENABLED` implements the original row/frame math for 2D array and 3D modes. The smoke test uses mesh `TEXCOORD1.x` as `CustomCoord.Coord1X`; full particle Custom Vertex Streams parity remains unclaimed. |
| Tritone mode | Passed | Passed | `_BASE_MAP_TONE_MODE_TRITONE` applies the original shadow/midtone/highlight interpolation and `_BaseMapToneChannel` selection. |
| Pentone mode | Passed | Passed | `_BASE_MAP_TONE_MODE_PENTONE` applies the original shadow/darktone/midtone/bright/highlight interpolation and `_BaseMapToneChannel` selection. |
| Mirror sampler keywords | Passed | Passed | Existing mirror keywords are preserved. Phase 4.2A uses manual mirrored UVs for the base map instead of relying on backend-specific Built-in sampler-state syntax, so sampler filtering parity remains Approximate. |
| Shader compiler diagnostics | Passed | Passed | Compile coverage now includes 2D array, 3D, channel, random row, tritone, pentone, and point/linear/trilinear mirror keyword states. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UberUnlitRendersBaseMap2DArray`, `UberUnlitRendersBaseMap3D`, `UberUnlitBaseMapChannelExtractionAffectsOutput`, `UberUnlitBaseMapRandomRowSelectionAffectsOutput`, `UberUnlitTritoneAffectsOutput`, and `UberUnlitPentoneAffectsOutput`. |
| Forbidden URP/RenderGraph scan | Passed | Passed | Static validation continues to scan Built-in runtime/editor/package files and exclude the untracked `NovaShader/` reference directory. |

Phase 4.2A compatibility annotations:

- Base map 2D array, base map 3D, channel extraction, random row shader math, tritone, and pentone are implemented for `Nova/Particles/UberUnlit` Built-in `ForwardBase`.
- 2D array and 3D support remain Approximate at the platform level because texture array and 3D texture availability depends on the target backend, but the tested Unity editor paths pass.
- Mirror sampling is a compatibility surface only in this phase. The Built-in port manually mirrors UVs when the preserved mirror keywords are enabled; exact sampler-state filtering and wrap parity remains for later visual validation.
- Random row selection is shader-side compatible with the original custom-coordinate numeric layout. The test uses mesh UV channels to provide deterministic coordinates and does not claim full Particle System Custom Vertex Streams parity.
- The following `Nova/Particles/UberUnlit` features remain Pending: flow map, parallax map, tint map expansion, tint rim expansion, greyscale/gradient map, fade/dissolve alpha transition, second alpha transition texture, emission, rim transparency, luminance transparency, soft particles, depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, and Picking.

## Phase 4.2B Verification Status

Phase 4.2B expands only `Nova/Particles/UberUnlit` texture/tint/color-correction features on the Built-in `ForwardBase` path. It does not implement alpha transition, dissolve transition, second alpha transition texture, emission, rim transparency, luminance transparency, soft particles, depth fade, vertex deformation, additional passes, UI shaders, Lit shaders, distortion runtime, optimizer behavior, visual regression, or golden images.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Tint map 2D | Passed | Passed | `_TINT_MAP_ENABLED` samples `_TintMap` and applies original-style tint blending with `_TintBlendRate`. |
| Tint map 2D array | Passed | Passed | `_TINT_MAP_MODE_2D_ARRAY` samples `_TintMap2DArray` with `_TintMapProgress`, `_TintMapSliceCount`, and the same flip-book frame helper used for base maps. |
| Tint map 3D | Passed | Passed | `_TINT_MAP_3D_ENABLED` samples `_TintMap3D` using `_TintMap3DProgress` and flip-book blending progress. |
| Tint map random row selection | Passed | Passed | `_TINT_MAP_RANDOM_ROW_SELECTION_ENABLED` uses `_TintMapRowCount` and `_TintMapRandomRowCoord`. The smoke test supplies deterministic mesh `TEXCOORD1.x`; full Particle System Custom Vertex Streams parity remains unclaimed. |
| Tint rim | Passed | Passed | `_TINT_AREA_RIM` uses Built-in world normal and view direction helpers with `_TintRimProgress`, `_TintRimSharpness`, and `_InverseTintRim`. View-vector math is an approximation of the original URP helper path. |
| Flow map target base | Passed | Passed | `_FLOW_MAP_TARGET_BASE` samples `_FlowMap`, selects `_FlowMapChannelsX/Y`, applies `_FlowIntensity`, and offsets base map UVs. |
| Flow map target tint | Passed | Passed | `_FLOW_MAP_TARGET_TINT` offsets tint map UVs when tint map sampling is active. Emission and alpha-transition flow targets remain pending because those feature groups are not implemented in this phase. |
| Parallax map 2D target base | Passed | Passed | `_PARALLAX_MAP_MODE_2D` with `_PARALLAX_MAP_TARGET_BASE` samples `_ParallaxMap`, selects `_ParallaxMapChannel`, and applies a Built-in tangent-space one-step offset. This is Approximate because the Built-in path uses local tangent/view helpers and the smoke uses a deterministic test tangent rather than validating full particle tangent stream parity. |
| Greyscale | Passed | Passed | `_GREYSCALE_ENABLED` applies the original luminance coefficients to RGB. |
| Gradient map | Passed | Passed | `_GRADIENT_MAP_ENABLED` samples `_GradientMap` at luminance on the U axis. |
| Shader compiler diagnostics | Passed | Passed | Compile coverage includes tint color/map/2D array/3D/random row/rim, flow base/tint, parallax 2D/2D array/3D target base/tint, greyscale, and gradient keywords. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UberUnlitTintMapAffectsOutput`, `UberUnlitTintMap2DArrayAffectsOutput`, `UberUnlitTintMap3DAffectsOutput`, `UberUnlitTintMapRandomRowSelectionAffectsOutput`, `UberUnlitTintRimAffectsOutput`, `UberUnlitFlowMapOffsetsBaseSampling`, `UberUnlitFlowMapOffsetsTintSampling`, `UberUnlitParallax2DOffsetsBaseSampling`, `UberUnlitGreyscaleAffectsOutput`, and `UberUnlitGradientMapAffectsOutput`. |
| Forbidden URP/RenderGraph scan | Passed | Passed | Static validation scans Built-in runtime/editor/package files and excludes the untracked `NovaShader/` reference directory. |

Phase 4.2B compatibility annotations:

- Tint map 2D, tint map 2D array, tint map 3D, tint map random row selection, tint rim, flow target base, flow target tint, greyscale, and gradient map are implemented for `Nova/Particles/UberUnlit` Built-in `ForwardBase`.
- Tint rim is Approximate at the view-helper level because Built-in normal/view setup differs from URP. The feature has deterministic PlayMode smoke coverage but not final visual parity coverage.
- Parallax 2D target base is Approximate. It uses a one-step tangent-space offset compatible with Built-in helpers; 2D array and 3D parallax runtime behavior are preserved as compile/keyword compatibility only and remain Pending for runtime completion.
- Flow targets for emission and alpha transition remain Pending because emission and alpha transition are explicitly out of Phase 4.2B scope.
- Random row selection tests use mesh UV channels to represent custom coordinates and do not claim full Particle System Custom Vertex Streams parity.
- The following `Nova/Particles/UberUnlit` features remain Pending: fade/dissolve alpha transition, second alpha transition texture, emission, rim transparency, luminance transparency, soft particles, depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, and Picking.

## Phase 4.3A Verification Status

Phase 4.3A expands only `Nova/Particles/UberUnlit` alpha transition behavior on the Built-in `ForwardBase` path. It does not implement emission, rim transparency, luminance transparency, soft particles, depth fade, vertex deformation, additional passes, UI shaders, Lit shaders, distortion runtime, optimizer behavior, visual regression, or golden images.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Fade alpha transition | Passed | Passed | `_FADE_TRANSITION_ENABLED` applies original-compatible fade progress math using `_AlphaTransitionProgress`, custom progress coordinates, and optional vertex alpha progress modulation. |
| Dissolve alpha transition | Passed | Passed | `_DISSOLVE_TRANSITION_ENABLED` samples the alpha transition map signal and applies `_DissolveSharpness` through the original smoothstep-width formula. |
| Alpha transition map 2D | Passed | Passed | `_ALPHA_TRANSITION_MAP_MODE_2D` samples `_AlphaTransitionMap`, applies channel selection, offset, rotation, and transition progress where the Built-in helper surface supports them. |
| Alpha transition map 2D array | Passed | Passed | `_ALPHA_TRANSITION_MAP_MODE_2D_ARRAY` samples `_AlphaTransitionMap2DArray` using `_AlphaTransitionMapProgress`, `_AlphaTransitionMapSliceCount`, and flip-book frame selection. |
| Alpha transition map 3D | Passed | Passed | `_ALPHA_TRANSITION_MAP_MODE_3D` samples `_AlphaTransitionMap3D` using flip-book blending progress. Texture 3D availability remains platform-dependent. |
| Alpha transition random row selection | Passed | Passed | `_ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED` uses `_AlphaTransitionMapRowCount` and `_AlphaTransitionMapRandomRowCoord`. The smoke test supplies deterministic mesh `TEXCOORD1.x`; full Particle System Custom Vertex Streams parity remains unclaimed. |
| Second alpha transition texture 2D | Passed | Passed | `_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D` samples `_AlphaTransitionMapSecondTexture` and combines it with the primary transition map. |
| Second texture average blend | Passed | Passed | `_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE` averages the primary and second transition alpha factors. |
| Second texture multiply blend | Passed | Passed | `_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY` multiplies the primary and second transition alpha factors. |
| Second alpha transition texture 2D array / 3D | Pending | Pending | Keyword and property compatibility are preserved and compile-covered, but deterministic runtime smokes were not stable enough for this phase. Runtime completion remains pending rather than over-claimed. |
| Second alpha transition random row selection | Pending | Pending | Keyword and property compatibility are preserved and compile-covered. Runtime row-selection behavior for the second texture remains pending with the second 2D array/3D runtime paths. |
| Vertex alpha as transition progress | Passed | Passed | `_VERTEX_ALPHA_AS_TRANSITION_PROGRESS` follows the original rule: vertex alpha adds inverse progress for alpha transitions while vertex RGB still multiplies color and vertex alpha is not double-applied to final alpha. |
| Flow map target alpha transition | Passed | Passed | `_FLOW_MAP_TARGET_ALPHA_TRANSITION` offsets alpha transition UVs using the existing Built-in flow map logic without changing base or tint UVs for this target. |
| Shader compiler diagnostics | Passed | Passed | Compile coverage includes fade, dissolve, alpha transition 2D/2D array/3D/random row, second texture 2D/2D array/3D/random row keywords, average/multiply blend keywords, vertex-alpha progress, and flow target alpha transition. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UberUnlitFadeAlphaTransitionAffectsOutput`, `UberUnlitDissolveAlphaTransitionClipsOrAttenuatesOutput`, `UberUnlitAlphaTransitionMap2DAffectsOutput`, `UberUnlitAlphaTransitionMap2DArrayAffectsOutput`, `UberUnlitAlphaTransitionMap3DAffectsOutput`, `UberUnlitAlphaTransitionRandomRowSelectionAffectsOutput`, `UberUnlitSecondAlphaTransitionTextureAverageAffectsOutput`, `UberUnlitSecondAlphaTransitionTextureMultiplyAffectsOutput`, `UberUnlitVertexAlphaCanDriveTransitionProgress`, and `UberUnlitFlowMapOffsetsAlphaTransitionSampling`. |
| Forbidden URP/RenderGraph scan | Passed | Passed | Static validation scans Built-in runtime/editor/package files and excludes the untracked `NovaShader/` reference directory. |

Phase 4.3A compatibility annotations:

- Fade transition, dissolve transition, alpha transition map 2D, alpha transition map 2D array, alpha transition map 3D, alpha transition random row selection, second alpha transition texture 2D, second texture average/multiply blending, vertex alpha transition progress, and flow target alpha transition are implemented for `Nova/Particles/UberUnlit` Built-in `ForwardBase`.
- 2D array and 3D transition-map support remain Approximate at the platform level because texture array and 3D texture availability depends on the target backend, but the tested Unity editor paths pass for the primary transition map.
- Second alpha transition texture support is runtime-verified only for 2D in Phase 4.3A. Second texture 2D array, second texture 3D, and second texture random row selection keep keyword/property/compile compatibility and remain Pending for deterministic runtime completion.
- Random row selection tests use mesh UV channels to represent custom coordinates and do not claim full Particle System Custom Vertex Streams parity.
- The following `Nova/Particles/UberUnlit` features remain Pending: emission, rim transparency, luminance transparency, soft particles, depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, and Picking.

## Phase 4.3B Verification Status

Phase 4.3B expands only `Nova/Particles/UberUnlit` emission behavior on the Built-in `ForwardBase` path. It does not implement rim transparency, luminance transparency, soft particles, depth fade, vertex deformation, additional passes, UI shaders, Lit shaders, distortion runtime, optimizer behavior, visual regression, or golden images.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Emission area all | Passed | Passed | `_EMISSION_AREA_ALL` adds emission across the rendered surface using `_EmissionIntensity` and the selected emission color source. |
| Emission area alpha | Passed | Passed | `_EMISSION_AREA_ALPHA` follows the original edge rule: emission is driven by `1 - alpha`, with texture-compression margin handling. |
| Emission area map | Passed | Passed | `_EMISSION_AREA_MAP` samples the emission map signal and preserves `_EmissionMapChannelsX` channel selection. |
| Emission map 2D | Passed | Passed | `_EMISSION_MAP_MODE_2D` samples `_EmissionMap` with offset, rotation, and channel selection through the Built-in helper path. |
| Emission map 2D array | Passed | Passed | `_EMISSION_MAP_MODE_2D_ARRAY` samples `_EmissionMap2DArray` using `_EmissionMapProgress`, `_EmissionMapSliceCount`, and flip-book frame selection. |
| Emission map 3D | Passed | Passed | `_EMISSION_MAP_MODE_3D` samples `_EmissionMap3D` using flip-book blending progress. Texture 3D availability remains platform-dependent. |
| Emission map random row selection | Passed | Passed | `_EMISSION_MAP_RANDOM_ROW_SELECTION_ENABLED` uses `_EmissionMapRowCount` and `_EmissionMapRandomRowCoord`. The smoke test supplies deterministic mesh `TEXCOORD1.x`; full Particle System Custom Vertex Streams parity remains unclaimed. |
| Emission color from base color | Passed | Passed | `_EMISSION_COLOR_BASECOLOR` derives emission from the current post-base/tint/color-correction RGB, matching the original emission call order. |
| Emission color from explicit color | Passed | Passed | `_EMISSION_COLOR_COLOR` uses `_EmissionColor.rgb` and `_EmissionIntensity`. |
| Emission color map/ramp | Passed | Passed | `_EMISSION_COLOR_MAP` samples `_EmissionColorRamp` at the emission-map or alpha-derived signal, matching the original ramp semantics for covered area modes. |
| Emission intensity | Passed | Passed | `_EmissionIntensity` and `_EmissionIntensityCoord` scale the final emission contribution. |
| Keep edge transparency | Passed | Passed | `_KeepEdgeTransparency` preserves alpha for edge emission when enabled and hardens nonzero alpha when disabled, matching the original shader rule. |
| Flow map target emission | Passed | Passed | `_FLOW_MAP_TARGET_EMISSION` offsets only emission-map UVs with the existing flow-map offset path. |
| Shader compiler diagnostics | Passed | Passed | Compile coverage includes emission area all/alpha/map, emission color color/base/ramp, emission map 2D/2D array/3D/random row, and flow target emission keywords. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UberUnlitEmissionAreaAllAffectsOutput`, `UberUnlitEmissionAreaAlphaUsesAlpha`, `UberUnlitEmissionAreaMapMasksEmission`, `UberUnlitEmissionMap2DAffectsOutput`, `UberUnlitEmissionMap2DArrayAffectsOutput`, `UberUnlitEmissionMap3DAffectsOutput`, `UberUnlitEmissionMapRandomRowSelectionAffectsOutput`, `UberUnlitEmissionColorFromBaseColorAffectsOutput`, `UberUnlitEmissionColorFromExplicitColorAffectsOutput`, `UberUnlitEmissionColorMapOrRampAffectsOutput`, `UberUnlitEmissionIntensityAffectsOutput`, `UberUnlitKeepEdgeTransparencyPreservesTransparentEdges`, and `UberUnlitFlowMapOffsetsEmissionSampling`. |
| Forbidden URP/RenderGraph scan | Passed | Passed | Static validation scans Built-in runtime/editor/package files and excludes the untracked `NovaShader/` reference directory. |

Phase 4.3B compatibility annotations:

- Emission area all, emission area alpha, emission area map, emission map 2D, emission map 2D array, emission map 3D, emission map random row selection, emission color from base color, emission color from `_EmissionColor`, emission color ramp, emission intensity, keep-edge transparency, and flow target emission are implemented for `Nova/Particles/UberUnlit` Built-in `ForwardBase`.
- 2D array and 3D emission-map support remain Approximate at the platform level because texture array and 3D texture availability depends on the target backend, but the tested Unity editor paths pass.
- Emission color from base color is applied after base/tint/color-correction and vertex color, matching the original `fragUnlit` call order before alpha clip and alpha modulation.
- Random row selection tests use mesh UV channels to represent custom coordinates and do not claim full Particle System Custom Vertex Streams parity.
- The following `Nova/Particles/UberUnlit` features remain Pending: rim transparency, luminance transparency, soft particles, depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, and Picking.

## Phase 4.4A Verification Status

Phase 4.4A expands only `Nova/Particles/UberUnlit` rim and luminance transparency behavior on the Built-in `ForwardBase` path. It does not implement soft particles, depth fade, vertex deformation, additional passes, UI shaders, Lit shaders, distortion runtime, optimizer behavior, visual regression, or golden images.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Rim transparency | Passed | Passed | `_TRANSPARENCY_BY_RIM` multiplies final alpha using the original progress/sharpness shaping after emission and before alpha test/modulation. The rim input uses the Built-in world normal/view direction approximation. |
| Inverse rim transparency | Passed | Passed | `_InverseRimTransparency` flips the rim input before shaping, matching the original helper semantics. |
| Rim transparency progress and sharpness | Passed | Passed | `_RimTransparencyProgress`, `_RimTransparencyProgressCoord`, `_RimTransparencySharpness`, and `_RimTransparencySharpnessCoord` are applied through the same shaping helper used by the original shader. |
| Luminance transparency | Passed | Passed | `_TRANSPARENCY_BY_LUMINANCE` derives luminance from the current RGB after base, tint, color correction, vertex color, and emission, then multiplies final alpha. |
| Inverse luminance transparency | Passed | Passed | `_InverseLuminanceTransparency` flips luminance before shaping, matching the original helper semantics. |
| Luminance transparency progress and sharpness | Passed | Passed | `_LuminanceTransparencyProgress`, `_LuminanceTransparencyProgressCoord`, `_LuminanceTransparencySharpness`, and `_LuminanceTransparencySharpnessCoord` are applied through the same shaping helper as rim transparency. |
| Combined rim and luminance transparency | Passed | Passed | When both transparency keywords are enabled, both alpha factors are applied multiplicatively in original-compatible order. |
| Interaction with previous features | Passed | Passed | Existing base map, tint, alpha transition, and emission smokes remain green; an added smoke combines luminance transparency with base/tint/alpha-transition/emission output. |
| Shader compiler diagnostics | Passed | Passed | Compile coverage includes `_TRANSPARENCY_BY_RIM`, `_TRANSPARENCY_BY_LUMINANCE`, and both keywords enabled together. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UberUnlitRimTransparencyAffectsOutput`, `UberUnlitInverseRimTransparencyInvertsEffect`, `UberUnlitRimTransparencyProgressAffectsOutput`, `UberUnlitRimTransparencySharpnessAffectsFalloff`, `UberUnlitLuminanceTransparencyAffectsOutput`, `UberUnlitInverseLuminanceTransparencyInvertsEffect`, `UberUnlitLuminanceTransparencyProgressAffectsOutput`, `UberUnlitLuminanceTransparencySharpnessAffectsOutput`, and `UberUnlitRimAndLuminanceTransparencyCanCombine`. |
| Forbidden URP/RenderGraph scan | Passed | Passed | Static validation scans Built-in runtime/editor/package files and excludes the untracked `NovaShader/` reference directory. |

Phase 4.4A compatibility annotations:

- Rim transparency, inverse rim transparency, rim progress/sharpness, luminance transparency, inverse luminance transparency, luminance progress/sharpness, and combined rim plus luminance transparency are implemented for `Nova/Particles/UberUnlit` Built-in `ForwardBase`.
- Rim transparency is Approximate because Built-in world normal/view direction interpolation differs from the original URP helper path. The implementation preserves the original alpha-factor math but does not claim pixel-perfect rim parity.
- Luminance transparency is implemented with the original call order: after base/tint/color-correction, vertex color, and emission, and before alpha test and alpha modulation.
- Combined rim plus luminance transparency applies both alpha multipliers; it does not replace alpha transition or keep-edge transparency behavior.
- The following `Nova/Particles/UberUnlit` features remain Pending: soft particles, depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, and Picking.

## Phase 4.4B Verification Status

Phase 4.4B expands only `Nova/Particles/UberUnlit` soft particles and depth fade on the Built-in `ForwardBase` path. It does not implement vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, Picking, UI UberUnlit, Lit, Distortion runtime, optimizer/replacer runtime behavior, visual regression, or golden images.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Soft particles | Passed | Passed | `_SOFT_PARTICLES_ENABLED` samples Built-in `_CameraDepthTexture`, compares scene eye depth with fragment eye depth, and multiplies final alpha using the original `(sceneDepth - particleDepth) / _SoftParticlesIntensity` factor. |
| Soft particles intensity | Passed | Passed | `_SoftParticlesIntensity` controls the attenuation denominator. Higher intensity produces stronger fade near scene depth; very small values are clamped defensively to avoid division by zero. |
| Built-in camera depth texture requirement | Passed | Passed | Depth-dependent PlayMode smokes explicitly set `camera.depthTextureMode |= DepthTextureMode.Depth` and include a depth-writing opaque `Standard` surface behind the transparent particle surface for deterministic camera depth generation. |
| Depth fade | Passed | Passed | `_DEPTH_FADE_ENABLED` multiplies final alpha by the original view-depth range factor based on fragment eye depth. The original shader does not sample scene depth for this factor; the Built-in path keeps that semantic while sharing the local depth helper data path. |
| Depth fade near/far/width | Passed | Passed | `_DepthFadeNear`, `_DepthFadeFar`, and `_DepthFadeWidth` shape the range and falloff using the original `min(depth - near, far - depth) / width` rule. |
| Combined soft particles and depth fade | Passed | Passed | When both keywords are active, both alpha factors are applied multiplicatively after rim/luminance transparency and before alpha test/modulation. |
| Interaction with alpha transition, emission, and transparency | Passed | Passed | Added smokes verify alpha transition remains effective with a depth feature, and emission/luminance transparency output remains non-empty while soft particles attenuate it. |
| Shader compiler diagnostics | Passed | Passed | Compile coverage includes `_SOFT_PARTICLES_ENABLED`, `_DEPTH_FADE_ENABLED`, both together, soft particles with alpha test/alpha transition/emission/transparency, and depth fade with alpha transition/emission/transparency. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UberUnlitSoftParticlesRequireCameraDepthTexture`, `UberUnlitSoftParticlesFadeNearSceneDepth`, `UberUnlitSoftParticlesIntensityAffectsOutput`, `UberUnlitSoftParticlesDoNotAffectWhenDepthFar`, `UberUnlitDepthFadeAffectsOutput`, `UberUnlitDepthFadeNearFarAffectOutput`, `UberUnlitDepthFadeWidthAffectsFalloff`, `UberUnlitDepthFadeDoesNotBreakBaseAlpha`, `UberUnlitSoftParticlesAndDepthFadeCanCombine`, `UberUnlitDepthFeaturesPreserveAlphaTransition`, and `UberUnlitDepthFeaturesPreserveEmissionAndTransparency`. |
| Forbidden URP/RenderGraph scan | Passed | Passed | Static validation scans Built-in runtime/editor/package files and excludes the untracked `NovaShader/` reference directory. |

Phase 4.4B compatibility annotations:

- Soft particles, soft particles intensity, depth fade, depth fade near/far/width, and combined soft particles plus depth fade are implemented for `Nova/Particles/UberUnlit` Built-in `ForwardBase`.
- Soft particles are Approximate because Built-in camera depth texture generation differs from URP. Cameras using this feature must enable depth texture generation, and a depth-writing opaque surface must participate in the camera depth texture for the fade to have scene depth input.
- The Built-in depth helper now handles orthographic camera depth conversion explicitly because Built-in raw depth texture values differ from the original URP sampling path. Final pixel parity remains a visual-regression task.
- Depth fade preserves the original view-depth semantics rather than inventing a scene-depth intersection fade; `_CameraDepthTexture` is still required for soft particles and is enabled in the shared depth-feature test scenes.
- Combined depth features multiply their alpha factors conservatively and do not replace alpha transition, emission keep-edge transparency, rim transparency, luminance transparency, alpha test, or alpha modulation behavior.
- The following `Nova/Particles/UberUnlit` features remain Pending: vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, and Picking.

## Phase 4.5A Verification Status

Phase 4.5A expands only `Nova/Particles/UberUnlit` vertex deformation on the Built-in `ForwardBase` path. It does not implement ShadowCaster deformation, `_ShadowCasterApplyVertexDeformation`, ShadowCaster alpha behavior, DepthOnly, DepthNormals, SceneSelectionPass, Picking, UI UberUnlit, Lit, Distortion runtime, optimizer/replacer runtime behavior, visual regression, or golden images.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Vertex deformation enabled path | Passed | Passed | `_VERTEX_DEFORMATION_ENABLED` is applied in the primary Built-in `ForwardBase` vertex stage before clip-space, screen-position, world-position, normal/view, and fragment UV data are produced. |
| Vertex deformation map sampling | Passed | Passed | `_VertexDeformationMap` is sampled in the vertex shader with LOD 0 through the local Built-in `tex2Dlod` helper macro. The existing shader target 3.5 is sufficient for the tested editor backends. |
| Vertex deformation channel selection | Passed | Passed | `_VertexDeformationMapChannel` selects the sampled RGBA channel using the same 0/1/2/3 mapping as the shared Built-in channel helper; PlayMode compares red and green channel motion. |
| Vertex deformation intensity | Passed | Passed | `_VertexDeformationIntensity` scales the map-derived displacement, and zero or lower intensity produces less silhouette movement. |
| Vertex deformation base value | Passed | Passed | `_VertexDeformationBaseValue` is subtracted from the selected map channel before intensity multiplication, allowing positive and negative movement around the base value. |
| Vertex deformation offset coordinates | Passed | Passed | `_VertexDeformationMapOffsetXCoord` and `_VertexDeformationMapOffsetYCoord` use the existing Built-in custom-coordinate helper. The smoke test covers `TEXCOORD1.x`; full Particle System Custom Vertex Streams parity remains pending. |
| Deformation direction | Passed | Passed | Vertices are displaced along normalized object-space normals, matching the original shader semantics. |
| Interaction with previous ForwardBase features | Passed | Passed | Existing base map, tint, alpha test, base map 2D array/3D, tone mapping, flow, parallax, greyscale/gradient, alpha transition, emission, rim/luminance transparency, soft particles, and depth fade tests remain green; an added smoke verifies emission remains visible while deformation moves geometry. |
| Shader compiler diagnostics | Passed | Passed | Compile coverage includes `_VERTEX_DEFORMATION_ENABLED`, vertex deformation plus alpha test, base map 2D array, alpha transition, emission, and depth fade. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UberUnlitVertexDeformationMovesGeometry`, `UberUnlitVertexDeformationIntensityAffectsMotion`, `UberUnlitVertexDeformationBaseValueAffectsMotion`, `UberUnlitVertexDeformationChannelSelectionAffectsMotion`, `UberUnlitVertexDeformationWithOffsetCoordinatesAffectsSampling`, and `UberUnlitVertexDeformationPreservesForwardColorFeatures`. |
| Forbidden URP/RenderGraph scan | Passed | Passed | Static validation scans Built-in runtime/editor/package files and excludes the untracked `NovaShader/` reference directory. |

Phase 4.5A compatibility annotations:

- Vertex deformation, deformation map sampling, channel selection, intensity, base value, and shader-side map offset coordinates are implemented for `Nova/Particles/UberUnlit` Built-in `ForwardBase`.
- Vertex deformation uses the original semantics: sample `_VertexDeformationMap` at transformed UV plus custom-coordinate offsets, subtract `_VertexDeformationBaseValue`, multiply by `_VertexDeformationIntensity` plus its custom coordinate, and offset object-space position along `normalize(normalOS)`.
- Vertex texture sampling uses `tex2Dlod` through the local Built-in texture helper surface. No shader target increase was required beyond the existing target 3.5 in the tested Unity editor paths.
- Offset coordinate support is shader-side compatible with the existing custom-coordinate numeric layout. The deterministic test uses mesh `TEXCOORD1.x`; full Particle System Custom Vertex Streams authoring and renderer stream validation parity remains pending.
- This phase is ForwardBase-only. Deformed geometry is not yet reflected in ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking behavior, and `_ShadowCasterApplyVertexDeformation` remains only a preserved material property/API surface.
- The following `Nova/Particles/UberUnlit` features remain Pending: ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, and Picking.

## Phase 4.5B Verification Status

Phase 4.5B expands only `Nova/Particles/UberUnlit` pass topology beyond the primary `ForwardBase` path. It does not implement UI UberUnlit, Lit, Distortion runtime, optimizer/replacer runtime behavior, golden image visual regression, full Particle System Custom Vertex Streams parity, or full URP DepthNormals parity.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| ShadowCaster pass topology | Passed | Passed | Adds Built-in `Name "ShadowCaster"` / `LightMode` `ShadowCaster`, local Built-in shadow helpers, `ZWrite On`, `ZTest LEqual`, `ColorMask 0`, and material `_Cull`. Built-in shadow bias and punctual shadow behavior are Approximate rather than URP parity. |
| `_ShadowCasterEnabled` | Passed | Passed | Preserved as the public material property and routed through the existing editor setup to `material.SetShaderPassEnabled("ShadowCaster", ...)`. Built-in cannot remove a pass from the shader solely from a material float without material/editor setup, so renderer shadow participation still depends on Unity renderer shadow settings plus the material setup call. |
| ShadowCaster alpha test | Passed | Passed | `_ShadowCasterAlphaTestEnabled` enables `_SHADOW_CASTER_ALPHA_TEST_ENABLED`; `_ShadowCasterAlphaCutoff` clips the shadow alpha path independently from the visible `_Cutoff`. |
| ShadowCaster vertex deformation | Passed | Passed | `_ShadowCasterApplyVertexDeformation` gates the same deformation-map sampling, channel, base-value, intensity, and custom-coordinate offset path used by the Phase 4.5A forward pass. |
| ShadowCaster alpha influence toggles | Passed | Passed | Tint color, flow-map UV offset, alpha-transition map, and luminance transparency toggles are implemented in the shadow alpha path where their corresponding feature keywords are active. Runtime smoke coverage targets tint, alpha transition, and luminance; flow-map influence remains compile/source covered because robust real-shadow UV-offset validation is deferred. |
| DepthOnly pass | Passed | Passed | Adds a Built-in-compatible `Name "DepthOnly"` / `LightMode` `DepthOnly` pass with depth write, optional alpha clip, alpha-transition/luminance alpha influence, and vertex deformation. Built-in camera depth texture consumption remains Approximate because Built-in commonly uses `ShadowCaster` for camera depth generation. |
| SceneSelectionPass | Passed | Passed | Adds a compile-compatible editor pass preserving `LightMode` `SceneSelectionPass` and returning Unity editor object/pass IDs. Runtime editor selection visual parity is not claimed. |
| Picking | Passed | Passed | Adds a compile-compatible editor pass preserving `LightMode` `Picking` and returning `_SelectionID`. Runtime editor picking parity is not claimed. |
| DepthNormals decision | Passed | Passed | No DepthNormals pass is added in Phase 4.5B. Built-in has no URP DepthNormals prepass contract, and no clear Built-in consumer/test exists for a minimal feature-complete pass at this stage. Status remains Unsupported/Pending until a later phase defines a Built-in consumer. |

Phase 4.5B compatibility annotations:

- `Nova/Particles/UberUnlit` now has Built-in pass declarations for `ForwardBase`, `SceneSelectionPass`, `Picking`, `DepthOnly`, and `ShadowCaster`.
- ShadowCaster is Approximate: it uses Built-in shadow caster bias helpers instead of URP `Shadows.hlsl` and URP shadow constants, and `_CASTING_PUNCTUAL_LIGHT_SHADOW` is preserved only as a compatibility keyword surface.
- `_ShadowCasterEnabled` is implemented through the original material setup API and Unity pass enablement. Materials that bypass setup may still have the shader pass available until setup is applied.
- DepthOnly is Approximate: the pass is compile-safe and can write depth when explicitly drawn, but Built-in camera depth generation does not provide the same URP DepthOnly prepass contract.
- SceneSelectionPass and Picking are compile-compatible editor passes only; no runtime visual parity claim is made.
- DepthNormals remains unimplemented in Phase 4.5B because Built-in has no URP DepthNormals prepass contract.
- The following remain out of scope: UI UberUnlit, Lit, Distortion runtime, optimizer/replacer runtime behavior, golden image visual regression, final visual parity, and full Particle System Custom Vertex Streams parity.

## Phase 4.6A UberUnlit Status Audit

Phase 4.6A consolidates the existing `Nova/Particles/UberUnlit` Built-in port after Phase 4.5B. It adds interaction smoke coverage, representative complex compile permutation coverage, pass-topology/documentation regressions, and explicit status documentation. It does not port `Nova/UIParticles/UberUnlit`, any Lit shader, Distortion runtime, optimizer/replacer runtime behavior, final visual regression, or golden image baselines. This audit does not claim full UberUnlit visual parity.

### Implemented and smoke-tested

- ForwardBase base map 2D.
- base map 2D array.
- base map 3D.
- base map channel extraction.
- random row selection.
- tritone.
- pentone.
- tint color.
- tint map 2D / 2D array / 3D.
- tint random row.
- tint rim.
- flow target base.
- flow target tint.
- flow target alpha transition.
- flow target emission.
- parallax 2D target base approximation.
- greyscale.
- gradient map.
- fade alpha transition.
- dissolve alpha transition.
- alpha transition map 2D / 2D array / 3D.
- primary alpha transition random row.
- second alpha transition 2D average / multiply.
- vertex alpha transition progress.
- emission area all / alpha / map.
- emission map 2D / 2D array / 3D.
- emission map random row.
- emission color base / explicit / ramp.
- emission intensity.
- keep edge transparency.
- rim transparency.
- luminance transparency.
- soft particles.
- depth fade.
- vertex deformation in ForwardBase.
- ShadowCaster.
- DepthOnly.
- SceneSelectionPass compile compatibility.
- Picking compile compatibility.

Phase 4.6A interaction smoke coverage adds deterministic, non-golden combinations for complex texture/tint/flow/color-correction, alpha-transition/emission/transparency, depth features with emission/transparency, vertex deformation with alpha/depth state, ShadowCaster alpha-transition/deformation, and DepthOnly alpha/deformation behavior.

Phase 4.6A complex compile permutation coverage represents:

- base map 2D array + tint map 2D array + flow.
- base map 3D + alpha transition 3D + emission 3D.
- dissolve + second alpha transition + flow target alpha transition.
- emission + keep edge transparency + luminance transparency.
- soft particles + depth fade + alpha transition + emission.
- vertex deformation + soft particles + ShadowCaster.
- ShadowCaster + alpha test + alpha transition + luminance transparency.
- DepthOnly + alpha test + vertex deformation.
- SceneSelectionPass / Picking with common keyword set.
- one dense implemented non-lit keyword set excluding mutually exclusive modes.

### Implemented but approximate

- manual mirror sampler approximation: Built-in uses local mirror UV math rather than URP sampler-state parity.
- parallax 2D approximation: the Built-in path uses a one-step tangent/view helper approximation.
- rim transparency view-vector approximation: Built-in world normal/view interpolation differs from the original URP helper path.
- soft particles Built-in camera depth texture approximation: cameras must provide `_CameraDepthTexture`, and Built-in depth generation differs from URP.
- depth fade Built-in camera depth texture approximation: the shared Built-in depth helper path is validated, but final depth visual parity remains pending.
- Built-in ShadowCaster bias/punctual shadow approximation: Built-in `UnityClipSpaceShadowCasterPos` and bias behavior replace URP shadow caster constants.
- DepthOnly Built-in consumption approximation: the pass can be explicitly drawn and compile safely, but Built-in does not provide the same URP DepthOnly prepass contract.
- editor SceneSelection/Picking compile-compatible only: SceneSelectionPass and Picking are compile-compatible editor passes only; no runtime visual parity claim is made.

### Preserved but not fully parity-tested

- full Particle System Custom Vertex Streams parity.
- flow-map shadow alpha influence real-shadow UV-offset smoke.
- second alpha transition 2D array / 3D / random-row runtime behavior.
- editor visual scene selection / picking parity.
- golden image visual parity.
- final visual regression remains pending.

### Unsupported or intentionally not implemented

- URP debug display.
- URP DepthNormals prepass parity. DepthNormals remains intentionally unimplemented because Built-in has no URP DepthNormals prepass contract and no Built-in consumer has been defined for this package phase.

Phase 4.6A compatibility annotations:

- `Nova/Particles/UberUnlit` remains the only runtime shader family advanced by this phase.
- ShadowCaster is Approximate for Built-in shadow bias, punctual shadow behavior, and material/pass enablement differences.
- DepthOnly is Approximate for Built-in consumption differences.
- SceneSelectionPass and Picking remain compile-compatible / approximate, not visual parity complete.
- Full Particle System Custom Vertex Streams parity remains pending; deterministic tests supply mesh UV streams directly.
- Visual regression is still pending and golden image baselines were not added or updated.
- UI UberUnlit is still the next phase and is not complete.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| EditMode suite | Passed, 32/32 | Passed, 32/32 | Adds `NovaBuiltInUberUnlitInteractionCompileTests` for pass topology, documentation/status audit, scope guard, and complex compile permutations. |
| PlayMode suite | Passed, 87/87 | Passed, 87/87 | Adds `NovaBuiltInUberUnlitInteractionSmokeTests` for six broad non-golden feature interaction smokes. |
| Shader compiler diagnostics | Passed | Passed | Existing diagnostics plus Phase 4.6A representative complex keyword states report no blocking shader compiler messages. |
| Forbidden URP/RenderGraph scan | Passed | Passed | Static validation scans `Assets/Nova/Runtime`, `Assets/Nova/Editor`, `Assets/Nova/package.json`, and `Packages/manifest.json`; the untracked `NovaShader/` reference directory is excluded. |

## Phase 5.1 UI UberUnlit Minimum Forward Status

Phase 5.1 begins `Nova/UIParticles/UberUnlit` only. It replaces the Phase 1/2 UI skeleton output with a minimum Built-in UI-compatible `ForwardBase` pass. This phase does not implement full UI UberUnlit feature parity, UI Lit, non-UI Lit, Distortion runtime, optimizer/replacer runtime behavior, final visual regression, or golden image baselines.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Minimum UI forward pass | Passed | Passed | One Built-in `ForwardBase` pass samples `_BaseMap` in 2D mode, applies `_TintColor` for `_TINT_AREA_ALL` + `_TINT_COLOR_ENABLED`, multiplies vertex color/alpha, preserves material alpha, supports `_ALPHATEST_ENABLED` + `_Cutoff`, and uses local Built-in includes only. |
| Base map 2D UI smoke | Passed | Passed | Non-golden PlayMode smoke renders a deterministic solid `_BaseMap` color through `Nova/UIParticles/UberUnlit`. UI 2D array and 3D base map modes remain pending. |
| Tint / vertex color / vertex alpha | Passed | Passed | PlayMode smokes verify tint changes the dominant channel, UI vertex color multiplies output, and vertex alpha blends toward the known background. Tint maps, tint rim, and vertex alpha as transition progress remain pending. |
| Alpha test | Passed | Passed | PlayMode smoke verifies `_ALPHATEST_ENABLED` clips below `_Cutoff` and reveals the background. Alpha transition and dissolve remain pending for UI. |
| UI stencil properties and state | Passed | Passed | `_StencilComp`, `_Stencil`, `_StencilOp`, `_StencilWriteMask`, `_StencilReadMask`, and `_ColorMask` remain material properties; the shader has a stencil block referencing those properties; a controlled stencil mask smoke hides/reveals the UI particle quad. |
| `_ColorMask` | Passed | Passed | Source and PlayMode stencil-mask coverage verify `_ColorMask` is wired to `ColorMask [_ColorMask]`, including a no-color stencil writer material. |
| `UNITY_UI_CLIP_RECT` / rect clip | Passed | Passed | `UNITY_UI_CLIP_RECT` remains declared as a local multi-compile keyword. The shader uses the local Built-in UI helper around `UnityGet2DClipping`, and a deterministic rect-clip smoke verifies outside-clip output is hidden. |
| Canvas / Mask / RectMask2D coverage | Approximate | Approximate | Phase 5.1 uses controlled mesh-based UI shader smokes instead of a uGUI `Canvas`/`Mask`/`RectMask2D` scene because the package manifest does not depend on `com.unity.ugui`. Full Canvas masking parity remains pending. |
| UI pass topology | Passed | Passed | `Nova/UIParticles/UberUnlit` remains a single-pass UI shader. Phase 5.1 does not add ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking. |
| Non-UI ShadowCaster keyword exclusion | Passed | Passed | UI shader keyword declarations still include `UNITY_UI_CLIP_RECT` and intentionally exclude `_CASTING_PUNCTUAL_LIGHT_SHADOW` and `_SHADOW_CASTER_ALPHA_TEST_ENABLED`. ShadowCaster material properties remain preserved as public API surface. |
| Material/editor API regression | Passed | Passed | Existing material setup continues to preserve `_RenderType`, `_TransparentBlendMode`, `_Cull`, `_QueueOffset`, `_BlendSrc`, `_BlendDst`, `_ZWrite`, `_ZWriteOverride`, `_ZTest`, UI stencil properties, `_ColorMask`, and `UNITY_UI_CLIP_RECT`. |
| Shader compiler diagnostics | Passed | Passed | New EditMode diagnostics cover default UI import, base-map 2D, tint color, alpha test, `UNITY_UI_CLIP_RECT`, and stencil property states with no blocking compiler messages. |

Phase 5.1 compatibility annotations:

- `Nova/UIParticles/UberUnlit` now has a real minimum Built-in UI forward path, but only for the base-map 2D + tint color + vertex color/alpha + alpha test subset.
- Stencil and `_ColorMask` are implemented through the same material property contract Unity UI uses. The runtime smoke validates a controlled stencil setup, not full uGUI Mask component behavior.
- Rect clipping is Approximate: it uses Built-in `UnityUI.cginc` clipping through `NovaBuiltInUI.cginc` and object-space clip positions. The smoke validates deterministic mesh clipping; Canvas `RectMask2D` behavior remains pending.
- Keyword declarations were split into independent UI-compatible groups so base map, tint, alpha test, and `UNITY_UI_CLIP_RECT` can be enabled together. Non-UI ShadowCaster-only keywords remain excluded from UI shader declarations.
- UI pass topology intentionally stays one primary forward pass. No ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking scope was added in Phase 5.1.
- Remaining UI UberUnlit gaps: full feature parity, base map 2D array / 3D, tone/channel/random row, flow, parallax, tint map/rim expansion, greyscale/gradient map, alpha transition, emission, rim/luminance transparency, soft particles/depth fade, vertex deformation, full Canvas Mask/RectMask2D parity, visual regression, and golden image parity.

## Phase 5.2A UI UberUnlit Base Texture and Tone Status

Phase 5.2A expands only `Nova/UIParticles/UberUnlit` base texture and tone behavior on top of the Phase 5.1 single-pass Built-in UI forward path. It does not implement UI tint map expansion, tint rim, flow map, parallax map, greyscale/gradient map, alpha transition, emission, rim/luminance transparency, soft particles/depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, Picking, Lit, Distortion runtime, optimizer/replacer runtime behavior, final visual regression, or golden image baselines.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| UI base map 2D array | Passed | Passed | `_BASE_MAP_MODE_2D_ARRAY` samples `_BaseMap2DArray` using the same Built-in flip-book progress helper already proven for `Nova/Particles/UberUnlit`; PlayMode smoke renders a deterministic solid-color slice. |
| UI base map 3D | Passed | Passed | `_BASE_MAP_MODE_3D` samples `_BaseMap3D` using the shared flip-book blending progress helper; PlayMode smoke renders a deterministic 3D layer. |
| UI base map channel extraction | Passed | Passed | `_BASE_MAP_CHANNEL_ENABLED` uses `_BaseMapChannel` through the shared base-map channel helper; PlayMode smoke verifies selected-channel grayscale output while preserving alpha behavior. |
| UI base map random row selection | Passed | Passed | `_BASE_MAP_RANDOM_ROW_SELECTION_ENABLED` uses `_BaseMapRowCount` and `_BaseMapRandomRowCoord` through mesh `TEXCOORD1` in the controlled UI smoke harness. Full Particle System Custom Vertex Streams parity remains unclaimed. |
| UI tritone mode | Passed | Passed | `_BASE_MAP_TONE_MODE_TRITONE` uses `_BaseMapToneChannel` and shadow/midtone/highlight properties through the shared tone helper; PlayMode smoke verifies tone colors affect output. |
| UI pentone mode | Passed | Passed | `_BASE_MAP_TONE_MODE_PENTONE` uses the shared shadow/darktone/midtone/bright/highlight interpolation helper; PlayMode smoke verifies pentone colors affect output. |
| UI mirror sampler compatibility surface | Passed | Passed | `_BASE_SAMPLER_STATE_POINT_MIRROR`, `_BASE_SAMPLER_STATE_LINEAR_MIRROR`, and `_BASE_SAMPLER_STATE_TRILINEAR_MIRROR` remain declared and use the shared manual mirror-UV approximation. Exact backend sampler-state filtering/wrap parity remains approximate. |
| UI stencil / rect clip regression | Passed | Passed | New PlayMode smokes verify a base texture feature still respects `UNITY_UI_CLIP_RECT` and a tone feature still respects controlled stencil masking. Canvas `Mask` / `RectMask2D` parity remains pending. |
| UI pass topology | Passed | Passed | `Nova/UIParticles/UberUnlit` remains one `ForwardBase` UI pass with stencil and `ColorMask [_ColorMask]`; no ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking pass was added. |
| Non-UI keyword exclusion | Passed | Passed | UI shader declarations still include `UNITY_UI_CLIP_RECT` and intentionally exclude `_CASTING_PUNCTUAL_LIGHT_SHADOW` and `_SHADOW_CASTER_ALPHA_TEST_ENABLED`. |
| Shader compiler diagnostics | Passed | Passed | EditMode compile coverage now includes 2D array, 3D, channel extraction, random row, tritone, pentone, mirror sampler keywords, base texture feature plus `UNITY_UI_CLIP_RECT`, and base texture feature plus alpha test. |

Phase 5.2A compatibility annotations:

- `Nova/UIParticles/UberUnlit` now reuses the shared Built-in UberUnlit base texture helpers for 2D array, 3D, random-row progress, channel extraction, tritone, pentone, and manual mirror UV behavior.
- 2D array and 3D support remain Approximate at the platform level because texture array and 3D texture support depend on the target backend, although the tested Unity editor paths pass deterministic PlayMode smoke coverage.
- Mirror sampling remains Approximate: the Built-in port manually mirrors UVs when the preserved mirror keywords are enabled rather than relying on URP sampler-state definitions.
- Random row selection is validated through mesh UV streams in the UI smoke harness. Full Particle System Custom Vertex Streams parity is still pending.
- Stencil, `_ColorMask`, rect clipping, vertex color, vertex alpha, alpha test, and the single-pass UI topology are preserved by regression coverage.
- Remaining UI UberUnlit gaps after Phase 5.2A: full feature parity, tint map/rim expansion, flow map, parallax map, greyscale/gradient map, alpha transition, emission, rim/luminance transparency, soft particles/depth fade, vertex deformation, full Canvas Mask/RectMask2D parity, visual regression, and golden image parity.

## Phase 5.2B UI UberUnlit Tint, Flow, Parallax, and Color Status

Phase 5.2B expands only `Nova/UIParticles/UberUnlit` tint map, tint rim, flow map, parallax 2D base-target approximation, greyscale, and gradient-map behavior on top of the Phase 5.2A single-pass Built-in UI forward path. It does not implement UI alpha transition, dissolve transition, second alpha transition texture, emission, rim/luminance transparency, soft particles/depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, Picking, Lit, Distortion runtime, optimizer/replacer runtime behavior, final visual regression, or golden image baselines.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| UI tint map 2D | Passed | Passed | `_TINT_MAP_ENABLED` samples `_TintMap` and applies the shared tint blend helper with `_TintBlendRate` while preserving vertex color, vertex alpha, UI clip, stencil, and `_ColorMask` behavior. |
| UI tint map 2D array | Passed | Passed | `_TINT_MAP_MODE_2D_ARRAY` samples `_TintMap2DArray` using `_TintMapProgress`, `_TintMapSliceCount`, and the shared flip-book progress helper. |
| UI tint map 3D | Passed | Passed | `_TINT_MAP_3D_ENABLED` samples `_TintMap3D` using `_TintMap3DProgress` and the shared flip-book blending progress helper. |
| UI tint random row selection | Passed | Passed | `_TINT_MAP_RANDOM_ROW_SELECTION_ENABLED` uses `_TintMapRowCount` and `_TintMapRandomRowCoord`; the smoke supplies deterministic mesh `TEXCOORD1.x`, so full Particle System Custom Vertex Streams parity remains unclaimed. |
| UI tint rim | Passed | Passed | `_TINT_AREA_RIM` uses Built-in world normal and view direction helpers with `_TintRimProgress`, `_TintRimSharpness`, and `_InverseTintRim`. This is Approximate because UI orthographic/view-vector behavior differs from the URP helper path. |
| UI flow map target base | Passed | Passed | `_FLOW_MAP_TARGET_BASE` samples `_FlowMap`, selects `_FlowMapChannelsX/Y`, applies `_FlowIntensity`, and offsets base-map UVs. |
| UI flow map target tint | Passed | Passed | `_FLOW_MAP_TARGET_TINT` offsets tint-map UVs when tint map sampling is active and leaves base-map sampling unchanged. Alpha-transition and emission flow targets remain pending because those feature groups are out of scope. |
| UI parallax map 2D target base | Passed | Passed | `_PARALLAX_MAP_MODE_2D` with `_PARALLAX_MAP_TARGET_BASE` samples `_ParallaxMap`, selects `_ParallaxMapChannel`, and applies the shared one-step Built-in tangent-space offset. This is Approximate and validated only through a controlled UI mesh/tangent smoke. |
| UI parallax target tint | Pending | Pending | `_PARALLAX_MAP_TARGET_TINT` remains a compile/keyword compatibility surface only in Phase 5.2B; runtime tint-target offset is deferred until a stable narrow UI smoke is added. |
| UI greyscale | Passed | Passed | `_GREYSCALE_ENABLED` applies the shared luminance conversion to RGB while preserving alpha, clip, and stencil behavior. |
| UI gradient map | Passed | Passed | `_GRADIENT_MAP_ENABLED` samples `_GradientMap` by luminance on the U axis while preserving UI alpha, clip, and stencil behavior. |
| UI stencil / rect clip regression | Passed | Passed | New PlayMode smokes verify tint+flow still respects `UNITY_UI_CLIP_RECT` and gradient-map output still respects controlled stencil masking. Canvas `Mask` / `RectMask2D` parity remains pending. |
| UI pass topology | Passed | Passed | `Nova/UIParticles/UberUnlit` remains one `ForwardBase` UI pass with stencil and `ColorMask [_ColorMask]`; no ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking pass was added. |
| Non-UI keyword exclusion | Passed | Passed | UI shader declarations still include `UNITY_UI_CLIP_RECT` and intentionally exclude `_CASTING_PUNCTUAL_LIGHT_SHADOW` and `_SHADOW_CASTER_ALPHA_TEST_ENABLED`. |
| Shader compiler diagnostics | Passed | Passed | EditMode compile coverage now includes tint color/map/2D array/3D/random row/rim, flow base/tint targets, parallax 2D/2D array/3D modes and base/tint targets, greyscale, gradient map, `UNITY_UI_CLIP_RECT` combinations, alpha test combinations, and stencil property states. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UIParticlesUberUnlitTintMapAffectsOutput`, `UIParticlesUberUnlitTintMap2DArrayAffectsOutput`, `UIParticlesUberUnlitTintMap3DAffectsOutput`, `UIParticlesUberUnlitTintMapRandomRowSelectionAffectsOutput`, `UIParticlesUberUnlitTintRimAffectsOutput`, `UIParticlesUberUnlitFlowMapOffsetsBaseSampling`, `UIParticlesUberUnlitFlowMapOffsetsTintSampling`, `UIParticlesUberUnlitParallax2DOffsetsBaseSampling`, `UIParticlesUberUnlitGreyscaleAffectsOutput`, `UIParticlesUberUnlitGradientMapAffectsOutput`, `UIParticlesUberUnlitTintFlowFeaturesRespectRectClip`, and `UIParticlesUberUnlitTintFlowFeaturesPreserveStencilOrMaskBehavior`. |

Phase 5.2B compatibility annotations:

- `Nova/UIParticles/UberUnlit` now reuses the shared Built-in UberUnlit helpers for tint-map progress, random-row selection, tint blending, rim factor, flow UV offset, parallax 2D offset, luminance, and gradient-map sampling where those helpers are safe for the UI forward path.
- Parallax 2D target base is Approximate. It uses a Built-in tangent/view-direction approximation and controlled mesh tangents in tests; no pixel-perfect URP parity or full UI/particle tangent-stream parity is claimed.
- Parallax 2D array and 3D runtime behavior, and parallax target tint runtime behavior, remain Pending. Their keyword/property surfaces are compile-covered only.
- Flow targets for alpha transition and emission remain Pending because UI alpha transition and UI emission are explicitly out of Phase 5.2B scope.
- Stencil, `_ColorMask`, rect clipping, vertex color, vertex alpha, alpha test, and the single-pass UI topology are preserved by regression coverage.
- Full Canvas `Mask` / `RectMask2D` parity is still pending; Phase 5.2B continues to use the controlled mesh-based UI shader smoke harness without adding `com.unity.ugui`.
- Remaining UI UberUnlit gaps after Phase 5.2B: full feature parity, alpha transition, dissolve transition, second alpha transition texture, emission, rim/luminance transparency, soft particles/depth fade, vertex deformation, parallax 2D array/3D runtime behavior, parallax tint target runtime behavior, full Canvas Mask/RectMask2D parity, visual regression, and golden image parity.

## Phase 5.3A UI UberUnlit Alpha Transition Status

Phase 5.3A expands only `Nova/UIParticles/UberUnlit` alpha transition behavior on top of the Phase 5.2B single-pass Built-in UI forward path. It does not implement UI emission, rim/luminance transparency, soft particles/depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, Picking, Lit, Distortion runtime, optimizer/replacer runtime behavior, visual regression, or golden image baselines.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| UI fade alpha transition | Passed | Passed | `_FADE_TRANSITION_ENABLED` applies the shared alpha transition progress math with `_AlphaTransitionProgress`, custom progress coordinates, and optional vertex-alpha progress modulation while preserving UI blend, clip, stencil, and vertex color behavior. |
| UI dissolve alpha transition | Passed | Passed | `_DISSOLVE_TRANSITION_ENABLED` samples the alpha transition map signal and applies `_DissolveSharpness` through the shared smoothstep-width formula. |
| UI alpha transition map 2D | Passed | Passed | `_ALPHA_TRANSITION_MAP_MODE_2D` samples `_AlphaTransitionMap` with channel, offset, rotation, and progress support through the shared helper path. |
| UI alpha transition map 2D array | Passed | Passed | `_ALPHA_TRANSITION_MAP_MODE_2D_ARRAY` samples `_AlphaTransitionMap2DArray` using `_AlphaTransitionMapProgress`, `_AlphaTransitionMapSliceCount`, and flip-book frame selection. Texture array support remains platform-dependent. |
| UI alpha transition map 3D | Passed | Passed | `_ALPHA_TRANSITION_MAP_MODE_3D` samples `_AlphaTransitionMap3D` with flip-book blending progress. Texture 3D support remains platform-dependent. |
| UI alpha transition random row selection | Passed | Passed | `_ALPHA_TRANSITION_MAP_RANDOM_ROW_SELECTION_ENABLED` uses `_AlphaTransitionMapRowCount` and `_AlphaTransitionMapRandomRowCoord`; the smoke supplies deterministic mesh `TEXCOORD1.x`, not full Particle System Custom Vertex Streams parity. |
| UI second alpha transition texture 2D | Passed | Passed | `_ALPHA_TRANSITION_MAP_SECOND_TEXTURE_MODE_2D` samples `_AlphaTransitionMapSecondTexture` and combines it with the primary transition factor. |
| UI second texture average blend | Passed | Passed | `_ALPHA_TRANSITION_BLEND_SECOND_TEX_AVERAGE` averages the primary and second transition factors. |
| UI second texture multiply blend | Passed | Passed | `_ALPHA_TRANSITION_BLEND_SECOND_TEX_MULTIPLY` multiplies the primary and second transition factors. |
| UI second alpha transition texture 2D array / 3D | Pending | Pending | Keyword, property, and compile compatibility are preserved. Runtime smoke coverage remains pending, matching the Phase 4.3A non-UI scope for second texture array/3D modes. |
| UI second alpha transition random row selection | Pending | Pending | Keyword, property, and compile compatibility are preserved. Runtime row-selection smoke coverage for the second texture remains pending. |
| UI vertex alpha as transition progress | Passed | Passed | `_VERTEX_ALPHA_AS_TRANSITION_PROGRESS` uses UI vertex alpha to modulate transition progress and avoids also multiplying final alpha by vertex alpha in that mode. Default vertex alpha final-alpha behavior remains preserved when the keyword is disabled. |
| UI flow map target alpha transition | Passed | Passed | `_FLOW_MAP_TARGET_ALPHA_TRANSITION` offsets primary and second alpha transition UVs with the existing UI flow-map path without changing base or tint sampling for this target. |
| UI stencil / ColorMask / rect clip regressions | Passed | Passed | Alpha-transition smokes verify rect clip, controlled stencil masking, `_ColorMask`, vertex color, vertex alpha, and alpha test behavior remain intact. Canvas `Mask` / `RectMask2D` parity is still pending because the package still does not add `com.unity.ugui`. |
| UI pass topology | Passed | Passed | `Nova/UIParticles/UberUnlit` remains one `ForwardBase` UI pass with stencil and `ColorMask [_ColorMask]`; no ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking pass was added. |
| Non-UI keyword exclusion | Passed | Passed | UI shader declarations still include `UNITY_UI_CLIP_RECT` and intentionally exclude `_CASTING_PUNCTUAL_LIGHT_SHADOW` and `_SHADOW_CASTER_ALPHA_TEST_ENABLED`. |
| Shader compiler diagnostics | Passed | Passed | EditMode compile coverage includes fade, dissolve, alpha transition 2D/2D array/3D/random row, second texture 2D/2D array/3D/random row keywords, average/multiply blend keywords, vertex-alpha progress, flow target alpha transition, `UNITY_UI_CLIP_RECT`, alpha test, and stencil property states. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UIParticlesUberUnlitFadeAlphaTransitionAffectsOutput`, `UIParticlesUberUnlitDissolveAlphaTransitionClipsOrAttenuatesOutput`, `UIParticlesUberUnlitAlphaTransitionMap2DAffectsOutput`, `UIParticlesUberUnlitAlphaTransitionMap2DArrayAffectsOutput`, `UIParticlesUberUnlitAlphaTransitionMap3DAffectsOutput`, `UIParticlesUberUnlitAlphaTransitionRandomRowSelectionAffectsOutput`, `UIParticlesUberUnlitSecondAlphaTransitionTexture2DAffectsOutput`, `UIParticlesUberUnlitSecondAlphaTransitionTextureAverageAffectsOutput`, `UIParticlesUberUnlitSecondAlphaTransitionTextureMultiplyAffectsOutput`, `UIParticlesUberUnlitVertexAlphaCanDriveTransitionProgress`, `UIParticlesUberUnlitFlowMapOffsetsAlphaTransitionSampling`, and alpha-transition UI regression smokes for rect clip, stencil, ColorMask, vertex color/alpha, and alpha test. |

Phase 5.3A compatibility annotations:

- `Nova/UIParticles/UberUnlit` now reuses the shared Built-in UberUnlit alpha transition helpers for fade, dissolve, primary transition map sampling, random-row progress, second texture 2D blending, vertex-alpha transition progress, and flow target alpha transition.
- Primary alpha transition 2D array and 3D modes are runtime smoke-tested but remain Approximate at the platform level because texture array and 3D texture support depends on the target backend.
- Second alpha transition texture runtime behavior is verified only for 2D with average and multiply blending in Phase 5.3A. Second texture 2D array, 3D, and random-row runtime behavior remain Pending with compile/keyword/property compatibility preserved.
- Random row selection tests use mesh UV streams to represent custom coordinates and do not claim full Particle System Custom Vertex Streams parity.
- Stencil, `_ColorMask`, rect clipping, vertex color, vertex alpha, alpha test, and the single-pass UI topology remain covered by deterministic non-golden PlayMode smokes.
- Full Canvas `Mask` / `RectMask2D` parity remains pending; Phase 5.3A continues to use the controlled mesh-based UI shader smoke harness without adding `com.unity.ugui`.
- Remaining UI UberUnlit gaps after Phase 5.3A: full feature parity, emission, rim/luminance transparency, soft particles/depth fade, vertex deformation, parallax 2D array/3D runtime behavior, parallax tint target runtime behavior, second alpha transition 2D array/3D/random-row runtime behavior, full Canvas Mask/RectMask2D parity, visual regression, and golden image parity.

## Phase 5.3B UI UberUnlit Emission Status

Phase 5.3B expands only `Nova/UIParticles/UberUnlit` emission behavior on top of the Phase 5.3A single-pass Built-in UI forward path. It does not implement UI rim/luminance transparency, soft particles/depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, Picking, Lit, Distortion runtime, optimizer/replacer runtime behavior, visual regression, or golden image baselines.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| UI emission area all | Passed | Passed | `_EMISSION_AREA_ALL` adds emission across the rendered UI surface using `_EmissionIntensity` and the selected emission color source while preserving UI vertex color, alpha, clip, stencil, and blend state behavior. |
| UI emission area alpha | Passed | Passed | `_EMISSION_AREA_ALPHA` follows the original edge rule: emission is driven by `1 - alpha`, with texture-compression margin handling and `_KeepEdgeTransparency` support. |
| UI emission area map | Passed | Passed | `_EMISSION_AREA_MAP` samples the emission map signal and preserves `_EmissionMapChannelsX` channel extraction. |
| UI emission map 2D | Passed | Passed | `_EMISSION_MAP_MODE_2D` samples `_EmissionMap` with offset, rotation, and channel selection through the shared Built-in helper path. |
| UI emission map 2D array | Passed | Passed | `_EMISSION_MAP_MODE_2D_ARRAY` samples `_EmissionMap2DArray` using `_EmissionMapProgress`, `_EmissionMapSliceCount`, and flip-book frame selection. Texture array support remains platform-dependent. |
| UI emission map 3D | Passed | Passed | `_EMISSION_MAP_MODE_3D` samples `_EmissionMap3D` using flip-book blending progress. Texture 3D support remains platform-dependent. |
| UI emission map random row selection | Passed | Passed | `_EMISSION_MAP_RANDOM_ROW_SELECTION_ENABLED` uses `_EmissionMapRowCount` and `_EmissionMapRandomRowCoord`; the smoke supplies deterministic mesh `TEXCOORD1.x`, not full Particle System Custom Vertex Streams parity. |
| UI emission color from base color | Passed | Passed | `_EMISSION_COLOR_BASECOLOR` derives emission from the current post-base/tint/color-correction and vertex-color RGB, matching the original emission call order. |
| UI emission color from explicit color | Passed | Passed | `_EMISSION_COLOR_COLOR` uses `_EmissionColor.rgb` and `_EmissionIntensity`. |
| UI emission color map/ramp | Passed | Passed | `_EMISSION_COLOR_MAP` samples `_EmissionColorRamp` at the emission-map or alpha-derived signal, matching the original ramp semantics for covered area modes. |
| UI emission intensity | Passed | Passed | `_EmissionIntensity` and `_EmissionIntensityCoord` scale the final emission contribution. |
| UI keep edge transparency | Passed | Passed | `_KeepEdgeTransparency` preserves alpha for edge emission when enabled and hardens nonzero alpha when disabled, matching the original shader rule. |
| UI flow map target emission | Passed | Passed | `_FLOW_MAP_TARGET_EMISSION` offsets only emission-map UVs with the existing UI flow-map offset path and does not affect base, tint, or alpha-transition sampling for this target. |
| UI stencil / ColorMask / rect clip regressions | Passed | Passed | Emission smokes verify rect clip, controlled stencil masking, vertex color, vertex alpha, alpha test, and alpha transition behavior remain intact. Existing `_ColorMask` coverage remains green. Canvas `Mask` / `RectMask2D` parity is still pending because the package still does not add `com.unity.ugui`. |
| UI alpha transition interaction | Passed | Passed | Emission remains visible when alpha transition permits output and is attenuated when the existing fade transition reduces alpha. |
| UI pass topology | Passed | Passed | `Nova/UIParticles/UberUnlit` remains one `ForwardBase` UI pass with stencil and `ColorMask [_ColorMask]`; no ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking pass was added. |
| Non-UI keyword exclusion | Passed | Passed | UI shader declarations still include `UNITY_UI_CLIP_RECT` and intentionally exclude `_CASTING_PUNCTUAL_LIGHT_SHADOW` and `_SHADOW_CASTER_ALPHA_TEST_ENABLED`. |
| Shader compiler diagnostics | Passed | Passed | EditMode compile coverage includes emission area all/alpha/map, emission color color/base/ramp, emission map 2D/2D array/3D/random row, flow target emission, `UNITY_UI_CLIP_RECT`, alpha test, alpha transition, and stencil property states. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UIParticlesUberUnlitEmissionAreaAllAffectsOutput`, `UIParticlesUberUnlitEmissionAreaAlphaUsesAlpha`, `UIParticlesUberUnlitEmissionAreaMapMasksEmission`, `UIParticlesUberUnlitEmissionMap2DAffectsOutput`, `UIParticlesUberUnlitEmissionMap2DArrayAffectsOutput`, `UIParticlesUberUnlitEmissionMap3DAffectsOutput`, `UIParticlesUberUnlitEmissionMapRandomRowSelectionAffectsOutput`, `UIParticlesUberUnlitEmissionColorFromBaseColorAffectsOutput`, `UIParticlesUberUnlitEmissionColorFromExplicitColorAffectsOutput`, `UIParticlesUberUnlitEmissionColorMapOrRampAffectsOutput`, `UIParticlesUberUnlitEmissionIntensityAffectsOutput`, `UIParticlesUberUnlitKeepEdgeTransparencyPreservesTransparentEdges`, `UIParticlesUberUnlitFlowMapOffsetsEmissionSampling`, and emission UI regression smokes for rect clip, stencil, vertex color/alpha, alpha test, and alpha transition. |

Phase 5.3B compatibility annotations:

- `Nova/UIParticles/UberUnlit` now reuses the shared Built-in UberUnlit emission progress, map sampling, color source, intensity, keep-edge transparency, and flow target emission semantics while keeping the single UI forward pass.
- UI emission 2D array and 3D modes are runtime smoke-tested but remain Approximate at the platform level because texture array and 3D texture support depends on the target backend.
- Random row selection tests use mesh UV streams to represent custom coordinates and do not claim full Particle System Custom Vertex Streams parity.
- Stencil, `_ColorMask`, rect clipping, vertex color, vertex alpha, alpha test, alpha transition interaction, and the single-pass UI topology remain covered by deterministic non-golden PlayMode smokes.
- Full Canvas `Mask` / `RectMask2D` parity remains pending; Phase 5.3B continues to use the controlled mesh-based UI shader smoke harness without adding `com.unity.ugui`.
- Remaining UI UberUnlit gaps after Phase 5.3B: full feature parity, rim/luminance transparency, soft particles/depth fade, vertex deformation, parallax 2D array/3D runtime behavior, parallax tint target runtime behavior, second alpha transition 2D array/3D/random-row runtime behavior, full Canvas Mask/RectMask2D parity, visual regression, and golden image parity.

## Phase 5.4A UI UberUnlit Transparency Status

Phase 5.4A expands only `Nova/UIParticles/UberUnlit` rim and luminance transparency behavior on top of the Phase 5.3B single-pass Built-in UI forward path. It does not implement UI soft particles, depth fade, vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, Picking, Lit, Distortion runtime, optimizer/replacer runtime behavior, visual regression, or golden image baselines.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| UI rim transparency | Passed | Passed | `_TRANSPARENCY_BY_RIM` multiplies final alpha using the shared progress/sharpness shaping after emission and before UI rect clipping and alpha test. The rim input uses the same Built-in world normal/view-direction approximation already used for `Nova/Particles/UberUnlit`. |
| UI inverse rim transparency | Passed | Passed | `_InverseRimTransparency` flips the rim input before shaping, matching the shared helper semantics. |
| UI rim progress and sharpness | Passed | Passed | `_RimTransparencyProgress`, `_RimTransparencyProgressCoord`, `_RimTransparencySharpness`, and `_RimTransparencySharpnessCoord` are routed through the shared shaping helper and custom-coordinate path. |
| UI luminance transparency | Passed | Passed | `_TRANSPARENCY_BY_LUMINANCE` derives luminance from current RGB after base, tint, color correction, vertex color, and emission, then multiplies final alpha. |
| UI inverse luminance transparency | Passed | Passed | `_InverseLuminanceTransparency` flips the luminance input before shaping. |
| UI luminance progress and sharpness | Passed | Passed | `_LuminanceTransparencyProgress`, `_LuminanceTransparencyProgressCoord`, `_LuminanceTransparencySharpness`, and `_LuminanceTransparencySharpnessCoord` use the same shaping helper as rim transparency. |
| UI combined rim and luminance transparency | Passed | Passed | When both transparency keywords are enabled, both alpha multipliers are applied in the same conservative multiplicative order used by the non-UI Built-in UberUnlit path. |
| UI stencil / ColorMask / rect clip regressions | Passed | Passed | Transparency smokes verify rect clipping, controlled stencil masking, vertex color, vertex alpha, and alpha test remain intact. Existing `_ColorMask` coverage remains green. Canvas `Mask` / `RectMask2D` parity is still pending because the package still does not add `com.unity.ugui`. |
| UI emission / alpha transition interaction | Passed | Passed | A combined smoke verifies luminance transparency does not clobber existing emission or fade alpha transition behavior. |
| UI pass topology | Passed | Passed | `Nova/UIParticles/UberUnlit` remains one `ForwardBase` UI pass with stencil and `ColorMask [_ColorMask]`; no ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking pass was added. |
| Non-UI keyword exclusion | Passed | Passed | UI shader declarations still include `UNITY_UI_CLIP_RECT` and intentionally exclude `_CASTING_PUNCTUAL_LIGHT_SHADOW` and `_SHADOW_CASTER_ALPHA_TEST_ENABLED`. |
| Shader compiler diagnostics | Passed | Passed | EditMode compile coverage includes `_TRANSPARENCY_BY_RIM`, `_TRANSPARENCY_BY_LUMINANCE`, both together, combinations with `UNITY_UI_CLIP_RECT`, alpha test, alpha transition, emission, and stencil property state. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UIParticlesUberUnlitRimTransparencyAffectsOutput`, `UIParticlesUberUnlitInverseRimTransparencyInvertsEffect`, `UIParticlesUberUnlitRimTransparencyProgressAffectsOutput`, `UIParticlesUberUnlitRimTransparencySharpnessAffectsFalloff`, `UIParticlesUberUnlitLuminanceTransparencyAffectsOutput`, `UIParticlesUberUnlitInverseLuminanceTransparencyInvertsEffect`, `UIParticlesUberUnlitLuminanceTransparencyProgressAffectsOutput`, `UIParticlesUberUnlitLuminanceTransparencySharpnessAffectsOutput`, `UIParticlesUberUnlitRimAndLuminanceTransparencyCanCombine`, transparency UI regression smokes for rect clip, stencil, vertex color/alpha, alpha test, and emission plus alpha transition. |

Phase 5.4A compatibility annotations:

- `Nova/UIParticles/UberUnlit` now reuses the shared Built-in UberUnlit rim transparency and luminance transparency helpers while preserving the single UI forward pass.
- UI rim transparency is Approximate because Built-in world normal/view-direction interpolation differs from the original URP helper path. The alpha-factor math is shared with the already verified non-UI Built-in path, but pixel-perfect URP rim parity is not claimed.
- Luminance transparency follows the original call order for this port: after base/tint/color-correction, vertex color, and emission, and before UI rect clipping, alpha test, and alpha modulation.
- Combined rim plus luminance transparency applies both alpha multipliers and does not replace alpha transition, emission keep-edge transparency, vertex alpha, stencil, rect clipping, or `_ColorMask` behavior.
- Full Canvas `Mask` / `RectMask2D` parity remains pending; Phase 5.4A continues to use the controlled mesh-based UI shader smoke harness without adding `com.unity.ugui`.
- Remaining UI UberUnlit gaps after Phase 5.4A: full feature parity, soft particles/depth fade, vertex deformation, parallax 2D array/3D runtime behavior, parallax tint target runtime behavior, second alpha transition 2D array/3D/random-row runtime behavior, full Canvas Mask/RectMask2D parity, visual regression, and golden image parity.

## Phase 5.4B UI UberUnlit Depth Features Status

Phase 5.4B expands only `Nova/UIParticles/UberUnlit` soft particles and depth fade behavior on top of the Phase 5.4A single-pass Built-in UI forward path. It does not implement UI vertex deformation, ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, Picking, Lit, Distortion runtime, optimizer/replacer runtime behavior, visual regression, or golden image baselines.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| UI soft particles | Passed | Passed | `_SOFT_PARTICLES_ENABLED` samples Built-in `_CameraDepthTexture`, compares scene eye depth with the UI-compatible fragment eye depth, and multiplies final alpha using the same `(sceneDepth - particleDepth) / _SoftParticlesIntensity` factor already verified for `Nova/Particles/UberUnlit`. |
| UI soft particles intensity | Passed | Passed | `_SoftParticlesIntensity` controls the attenuation denominator. Higher intensity produces stronger fade near scene depth; very small values are clamped by the shared Built-in helper to avoid division by zero. |
| Built-in camera depth texture requirement | Passed | Passed | UI soft-particle PlayMode smokes explicitly set `camera.depthTextureMode |= DepthTextureMode.Depth` and include a depth-writing opaque `Standard` surface behind the UI-compatible particle surface for deterministic camera depth generation. |
| UI depth fade | Passed | Passed | `_DEPTH_FADE_ENABLED` multiplies final alpha by the original view-depth range factor based on fragment eye depth. This preserves the non-UI Built-in Phase 4.4B semantic; it is not a scene-depth intersection fade. |
| UI depth fade near/far/width | Passed | Passed | `_DepthFadeNear`, `_DepthFadeFar`, and `_DepthFadeWidth` shape the range and falloff with the shared `min(depth - near, far - depth) / width` rule. |
| UI combined soft particles and depth fade | Passed | Passed | When both keywords are active, the UI shader applies both alpha factors multiplicatively after rim/luminance transparency and before UI rect clipping, alpha test, and alpha modulation. |
| UI previous-feature interactions | Passed | Passed | Added smokes verify alpha transition remains effective with a depth feature, and emission/luminance transparency output remains non-empty while soft particles attenuate it. Existing base map, tint, flow, alpha transition, emission, keep-edge transparency, rim/luminance transparency, vertex color, vertex alpha, and alpha test coverage remains green. |
| UI stencil / rect clip regressions | Passed | Passed | Depth-feature smokes verify `UNITY_UI_CLIP_RECT` and controlled stencil masking remain intact while depth fade is active. |
| UI ColorMask regression | Passed | Passed | A depth-feature smoke verifies `_ColorMask = 0` still suppresses color writes. |
| UI pass topology | Passed | Passed | `Nova/UIParticles/UberUnlit` remains one `ForwardBase` UI pass with stencil and `ColorMask [_ColorMask]`; no ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking pass was added. |
| Non-UI keyword exclusion | Passed | Passed | UI shader declarations still include `UNITY_UI_CLIP_RECT` and intentionally exclude `_CASTING_PUNCTUAL_LIGHT_SHADOW` and `_SHADOW_CASTER_ALPHA_TEST_ENABLED`. |
| Shader compiler diagnostics | Passed | Passed | EditMode compile coverage includes `_SOFT_PARTICLES_ENABLED`, `_DEPTH_FADE_ENABLED`, both together, combinations with `UNITY_UI_CLIP_RECT`, alpha test, alpha transition, emission, rim/luminance transparency, and stencil property state. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UIParticlesUberUnlitSoftParticlesRequireCameraDepthTexture`, `UIParticlesUberUnlitSoftParticlesFadeNearSceneDepth`, `UIParticlesUberUnlitSoftParticlesIntensityAffectsOutput`, `UIParticlesUberUnlitSoftParticlesDoNotAffectWhenDepthFar`, `UIParticlesUberUnlitDepthFadeAffectsOutput`, `UIParticlesUberUnlitDepthFadeNearFarAffectOutput`, `UIParticlesUberUnlitDepthFadeWidthAffectsFalloff`, `UIParticlesUberUnlitDepthFadeDoesNotBreakBaseAlpha`, `UIParticlesUberUnlitSoftParticlesAndDepthFadeCanCombine`, `UIParticlesUberUnlitDepthFeaturesPreserveAlphaTransition`, `UIParticlesUberUnlitDepthFeaturesPreserveEmissionAndTransparency`, `UIParticlesUberUnlitDepthFeaturesRespectRectClip`, `UIParticlesUberUnlitDepthFeaturesPreserveStencilOrMaskBehavior`, and `UIParticlesUberUnlitDepthFeaturesPreserveColorMask`. |

Phase 5.4B compatibility annotations:

- `Nova/UIParticles/UberUnlit` now reuses the local Built-in depth helper and the same soft-particle/depth-fade alpha attenuation used by the non-UI Built-in UberUnlit path while preserving the single UI forward pass.
- UI soft particles are Approximate because Built-in camera depth texture generation and sampling differ from URP. Cameras using this feature must enable depth texture generation, and a depth-writing opaque surface must participate in the camera depth texture for scene-depth attenuation to have input.
- Screen Space Overlay style UI has no meaningful scene-depth relationship in this package without additional runtime infrastructure. Phase 5.4B tests use controlled mesh-based UI-compatible rendering instead of full Canvas `Mask` / `RectMask2D` parity.
- Depth fade preserves the original view-depth range semantics; `_CameraDepthTexture` is required for UI soft particles, while the depth-fade factor is derived from fragment eye depth.
- Combined depth features multiply their alpha factors conservatively and do not replace UI alpha test, fade/dissolve alpha transition, emission keep-edge transparency, rim transparency, luminance transparency, vertex alpha, rect clipping, stencil, or `_ColorMask` behavior.
- Full Canvas `Mask` / `RectMask2D` parity remains pending; Phase 5.4B continues to use the controlled mesh-based UI shader smoke harness without adding `com.unity.ugui`.
- Remaining UI UberUnlit gaps after Phase 5.4B: full feature parity, vertex deformation, parallax 2D array/3D runtime behavior, parallax tint target runtime behavior, second alpha transition 2D array/3D/random-row runtime behavior, full Canvas Mask/RectMask2D parity, visual regression, and golden image parity.

## Phase 5.5A UI UberUnlit Vertex Deformation Status

Phase 5.5A expands only `Nova/UIParticles/UberUnlit` vertex deformation in the primary Built-in UI-compatible `ForwardBase` pass. It does not add UI ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, Picking, Lit, Distortion runtime, optimizer/replacer runtime behavior, full Particle System Custom Vertex Streams parity, full Canvas `Mask` / `RectMask2D` parity, visual regression, or golden image baselines.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| UI vertex deformation enabled path | Passed | Passed | `_VERTEX_DEFORMATION_ENABLED` now runs in the UI `ForwardBase` vertex stage before `NovaObjectToClipPos`, matching the non-UI Phase 4.5A direction by moving object-space vertices along `normalize(v.normal)`. |
| UI vertex deformation map sampling | Passed | Passed | The UI vertex path declares `_VertexDeformationMap` and samples it with `NOVA_SAMPLE_TEX2D_LOD` so the Built-in vertex stage can use a deterministic LOD. The existing `#pragma target 3.5` remains sufficient in the tested editor backends; no broader target increase was added. |
| UI channel selection | Passed | Passed | `_VertexDeformationMapChannel` uses the shared `NovaSelectChannel` mapping. PlayMode smokes compare red and green channel selections. |
| UI intensity | Passed | Passed | `_VertexDeformationIntensity` and `_VertexDeformationIntensityCoord` use the same additive custom-coordinate rule as the non-UI path. PlayMode smokes verify higher intensity produces larger motion. |
| UI base value | Passed | Passed | `_VertexDeformationBaseValue` is subtracted from the sampled channel before intensity multiplication. PlayMode smokes verify changing the base value reverses or reduces motion predictably. |
| UI offset coordinates | Passed | Passed | `_VertexDeformationMapOffsetXCoord` and `_VertexDeformationMapOffsetYCoord` use `NovaGetCustomCoord` against `TEXCOORD1/2`, matching the shader-side custom-coordinate contract. Full Particle System Custom Vertex Streams parity remains pending. |
| UI previous-feature interactions | Passed | Passed | Vertex deformation smokes cover base-map output plus emission and alpha-transition interaction, depth fade interaction, vertex color/alpha participation, alpha test through representative compile coverage, and existing UI feature suites remain green. |
| UI stencil / rect clip regressions | Passed | Passed | PlayMode smokes verify `UNITY_UI_CLIP_RECT` and controlled stencil masking remain effective while vertex deformation is active. |
| UI ColorMask regression | Passed | Passed | A PlayMode smoke verifies `_ColorMask = 0` still suppresses color writes while vertex deformation is active. |
| UI depth feature interaction | Passed | Passed | A PlayMode smoke verifies vertex deformation still moves the surface while depth fade attenuates output in the controlled depth-texture scene. Soft-particle/depth-fade compile permutations are also covered. |
| UI pass topology | Passed | Passed | `Nova/UIParticles/UberUnlit` remains one `ForwardBase` UI pass with stencil and `ColorMask [_ColorMask]`; no ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking pass was added. |
| Non-UI keyword exclusion | Passed | Passed | UI shader declarations still include `UNITY_UI_CLIP_RECT` and `_VERTEX_DEFORMATION_ENABLED` while excluding `_CASTING_PUNCTUAL_LIGHT_SHADOW` and `_SHADOW_CASTER_ALPHA_TEST_ENABLED`. |
| Shader compiler diagnostics | Passed | Passed | EditMode compile coverage includes `_VERTEX_DEFORMATION_ENABLED`, with `UNITY_UI_CLIP_RECT`, alpha test, alpha transition, emission, rim/luminance transparency, soft-particles/depth-fade state, and stencil property state. |
| Rendering smokes | Passed | Passed | PlayMode coverage adds `UIParticlesUberUnlitVertexDeformationMovesGeometry`, `UIParticlesUberUnlitVertexDeformationIntensityAffectsMotion`, `UIParticlesUberUnlitVertexDeformationBaseValueAffectsMotion`, `UIParticlesUberUnlitVertexDeformationChannelSelectionAffectsMotion`, `UIParticlesUberUnlitVertexDeformationWithOffsetCoordinatesAffectsSampling`, `UIParticlesUberUnlitVertexDeformationPreservesColorAndAlphaFeatures`, `UIParticlesUberUnlitVertexDeformationRespectsRectClip`, `UIParticlesUberUnlitVertexDeformationPreservesStencilOrMaskBehavior`, `UIParticlesUberUnlitVertexDeformationPreservesColorMask`, and `UIParticlesUberUnlitVertexDeformationPreservesDepthFeatureInteraction`. |

Phase 5.5A compatibility annotations:

- `Nova/UIParticles/UberUnlit` now uses the same shader-side vertex deformation semantics already verified for `Nova/Particles/UberUnlit`: transformed UV sampling, custom-coordinate X/Y offsets, selected channel minus base value, intensity plus custom-coordinate intensity, and object-space normal displacement before clip-space transform.
- The implementation is ForwardBase-only for UI. It does not claim shadow/depth/editor-pass consistency because `Nova/UIParticles/UberUnlit` intentionally has no UI ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking pass.
- Vertex texture sampling is Approximate at the platform level because backend support follows Built-in shader target/platform constraints. The tested Unity 2022.3 and 6000 editor paths compile and render with the existing `#pragma target 3.5`; no new target requirement was needed.
- Offset-coordinate support is shader-side only in this phase. Tests provide deterministic mesh UV custom coordinates, but full Particle System Custom Vertex Streams parity remains pending.
- Full Canvas `Mask` / `RectMask2D` parity remains pending; Phase 5.5A continues to use the controlled mesh-based UI shader smoke harness without adding `com.unity.ugui`.
- Remaining UI UberUnlit gaps after Phase 5.5A: full feature interaction consolidation, parallax 2D array/3D runtime behavior, parallax tint target runtime behavior, second alpha transition 2D array/3D/random-row runtime behavior, full Particle System Custom Vertex Streams parity, full Canvas Mask/RectMask2D parity, visual regression, and golden image parity.

## Phase 5.6A UI UberUnlit Consolidation Status Audit

Phase 5.6A consolidates the `Nova/UIParticles/UberUnlit` Built-in port after Phase 5.5A. It adds high-value interaction smoke coverage, representative complex compile permutation coverage, pass-topology/static regressions, and an explicit status audit before moving to Lit. It does not add a new major runtime feature, does not add UI ShadowCaster/depth/editor passes, does not add `com.unity.ugui`, does not start final visual regression, and does not add or update golden image baselines. This audit does not claim full UI UberUnlit visual parity.

### Implemented and smoke-tested

- UI minimum forward rendering through a single Built-in `ForwardBase` pass.
- Base map 2D, base map 2D array, base map 3D, base map channel extraction, and base map random row selection.
- Tritone and pentone base-map tone mapping.
- Tint color, tint map 2D, tint map 2D array, tint map 3D, tint random row, and tint rim.
- Flow target base, flow target tint, flow target alpha transition, and flow target emission.
- Parallax 2D target base approximation.
- Greyscale and gradient map color correction.
- Fade alpha transition and dissolve alpha transition.
- Alpha transition map 2D, alpha transition map 2D array, alpha transition map 3D, and primary alpha transition random row.
- Second alpha transition 2D average and second alpha transition 2D multiply.
- Vertex alpha transition progress.
- Emission area all, emission area alpha, emission area map, emission map 2D, emission map 2D array, emission map 3D, emission map random row, emission color base, emission color explicit, emission color ramp, emission intensity, and keep edge transparency.
- Rim transparency and luminance transparency.
- Soft particles and depth fade.
- Vertex deformation in UI ForwardBase, including deformation map sampling, channel selection, intensity, base value, and offset coordinate path.
- UI stencil properties, `_ColorMask`, `UNITY_UI_CLIP_RECT` / local rect clip behavior, one-pass UI topology, and non-UI ShadowCaster keyword exclusion.

### Implemented but approximate

- Manual mirror sampler approximation uses explicit mirrored UVs instead of exact backend sampler-state parity.
- Parallax 2D approximation uses the shared Built-in tangent/view-vector helper path and is only runtime-smoked for target base.
- Tint rim / rim transparency view-vector approximation uses Built-in world normal and view direction helpers rather than the original URP helper path.
- Soft particles Built-in camera depth texture approximation depends on `_CameraDepthTexture` generation by the Built-in camera.
- Depth fade Built-in camera depth texture approximation is tested in the same controlled depth scene, while the depth-fade factor itself follows the original view-depth range semantics.
- Texture array / 3D texture platform dependence remains because support depends on the target backend and shader model.
- Controlled mesh-based UI shader smoke harness is used instead of full Canvas parity.
- Screen Space Overlay depth relationship limitation remains: overlay-style UI has no meaningful scene-depth relationship in this package without additional runtime infrastructure.

### Preserved but not fully parity-tested

- Full Canvas `Mask` / `RectMask2D` parity remains pending.
- Full Particle System Custom Vertex Streams parity remains pending; tests provide deterministic mesh UV custom coordinates.
- Second alpha transition 2D array / 3D / random-row runtime behavior remains pending beyond compile coverage.
- Parallax target tint / 2D array / 3D runtime behavior remains pending beyond compile coverage.
- Full UI material inspector visual parity remains pending.
- Golden image visual parity remains pending.
- Final visual regression remains pending.

### Intentionally not implemented for UI UberUnlit

- UI ShadowCaster pass.
- UI DepthOnly pass.
- UI DepthNormals pass.
- UI SceneSelectionPass.
- UI Picking pass.
- Non-UI ShadowCaster-only keywords, including `_CASTING_PUNCTUAL_LIGHT_SHADOW` and `_SHADOW_CASTER_ALPHA_TEST_ENABLED`.
- URP debug display path.

Phase 5.6A interaction smoke coverage adds deterministic, non-golden combinations for complex texture/tint/flow/color-correction with rect clip, alpha-transition/emission/transparency, depth features with emission/transparency and rect clip, vertex deformation with depth and alpha behavior, stencil plus rect clip with a complex feature stack, `_ColorMask` with transparency/emission, and vertex alpha as alpha-transition progress with emission keep-edge transparency.

Phase 5.6A complex compile permutation coverage represents:

- base map 2D array + tint map 2D array + flow + `UNITY_UI_CLIP_RECT`
- base map 3D + alpha transition 3D + emission 3D
- dissolve + second alpha transition + flow target alpha transition
- emission + keep edge transparency + luminance transparency
- soft particles + depth fade + alpha transition + emission
- vertex deformation + soft particles + `UNITY_UI_CLIP_RECT`
- vertex deformation + alpha transition + emission + rect clip
- tint rim + rim transparency + luminance transparency
- stencil property state + alpha test + emission
- all major currently implemented UI non-lit feature keywords except mutually exclusive modes

Phase 5.6A topology and material/editor regressions verify that `Nova/UIParticles/UberUnlit` still declares exactly one UI forward pass, still declares no ShadowCaster / DepthOnly / DepthNormals / SceneSelectionPass / Picking pass, still excludes non-UI ShadowCaster-only keywords, still includes `UNITY_UI_CLIP_RECT`, still preserves stencil and `_ColorMask` properties through material setup, and still declares `CustomEditor "Nova.Editor.Core.Scripts.UIParticlesUberUnlitGUI"`.

Phase 5.6A compatibility annotations:

- `Nova/UIParticles/UberUnlit` is a stable Built-in approximation for the implemented non-lit UI feature set, but not a claim of full UI UberUnlit visual parity.
- One-pass UI topology is intentional. The original UI shader had only a UI forward pass, and the Built-in port does not add UI ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, or Picking passes.
- Full Canvas `Mask` / `RectMask2D` parity remains pending; controlled stencil and rect-clip paths are smoke-tested without adding `com.unity.ugui`.
- Full Particle System Custom Vertex Streams parity remains pending; shader-side custom-coordinate semantics are covered through deterministic mesh UV channels.
- Built-in depth texture behavior for UI depth features remains Approximate. Tests use a controlled camera depth texture setup, and Screen Space Overlay depth behavior remains a documented limitation.
- Texture array, 3D texture, vertex texture sampling, manual mirror sampling, parallax, and rim/view-vector behavior remain Approximate until final target-platform and visual validation.
- Visual regression remains pending and golden image baselines were not added or updated.
- Lit is the next major shader family. `Nova/Particles/UberLit`, `Nova/UIParticles/UberLit`, Distortion runtime, and Optimized Shader Generator/Replacer runtime behavior remain outside Phase 5.6A scope.

## Phase 6.1 UberLit Minimum ForwardBase Status

Phase 6.1 begins `Nova/Particles/UberLit` only. It replaces the Phase 1/2 fake vertex-color output with a minimum Built-in `ForwardBase` lit approximation. This phase does not attempt full PBR parity, does not add `ForwardAdd`, does not add receive-shadow runtime behavior, does not add Lit ShadowCaster / Lit DepthOnly / Lit DepthNormals / Lit SceneSelectionPass / Lit Picking, does not port `Nova/UIParticles/UberLit`, does not port Distortion, does not implement optimizer/replacer runtime behavior, and does not add or update golden images.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Minimum `ForwardBase` lit pass | Passed | Passed | `Nova/Particles/UberLit` now has one named Built-in `ForwardBase` pass using local Built-in includes, object-to-clip transforms, world normal interpolation, UV0, vertex color, and material render-state properties. |
| Main directional light approximation | Passed | Passed | The Phase 6.1 path uses `_WorldSpaceLightPos0` and `_LightColor0` through local `NovaGetMainLightDirection`, `NovaGetMainLightColor`, `NovaComputeLambertDiffuse`, and `NovaComputeMinimumLitColor` helpers. This is Lambert diffuse only and is not URP `UniversalFragmentPBR` parity. |
| Ambient / SH approximation | Passed | Passed | `NovaComputeAmbientApproximation` samples Built-in SH through `ShadeSH9` and falls back against `UNITY_LIGHTMODEL_AMBIENT`. This is a Built-in approximation of the original baked/global illumination path. |
| Base map 2D lit albedo | Passed | Passed | The minimum Lit path samples `_BaseMap` as a 2D texture and multiplies it into the lit albedo. Base map 2D array and 3D runtime behavior remain pending for Lit. |
| Tint / vertex color lit albedo | Passed | Passed | `_TINT_AREA_ALL` plus `_TINT_COLOR_ENABLED` multiplies `_TintColor`; vertex color also multiplies the minimum lit albedo. Tint maps, tint rim, flow, parallax, color correction, emission, transparency, soft particles, depth fade, and vertex deformation remain pending for Lit. |
| Alpha test and alpha output | Passed | Passed | `_ALPHATEST_ENABLED` clips against `_Cutoff`, and the fragment output preserves the resolved base alpha. Other alpha-transition paths remain pending for Lit runtime behavior. |
| Lit property preservation | Passed | Passed | The 23 audited Lit property additions remain present on `Nova/Particles/UberLit`, with representative texture, color, float, and range-compatible property types covered by EditMode tests. |
| Lit keyword compile compatibility | Passed | Passed | Lit keywords remain declared for compatibility, including URP-only surfaces such as `_FORWARD_PLUS` and `_CLUSTER_LIGHT_LOOP`. Many keywords are compile-compatible only in Phase 6.1 and do not imply runtime behavior. |
| Shader compiler diagnostics | Passed | Passed | EditMode compile coverage includes the default material, alpha test, specular setup, normal/specular/metallic/smoothness map keywords, receive shadows, specular highlights, environment reflections, main light shadow keywords, additional light keywords, Forward+, cluster light loop, and one representative combination. |
| Lit rendering smokes | Passed | Passed | PlayMode coverage verifies non-empty directional-light rendering, light direction response, light color response, ambient contribution, base map albedo, tint plus vertex color, alpha test clipping, and a meaningful difference from `Nova/Particles/UberUnlit`. |
| Pass topology | Passed | Passed | `Nova/Particles/UberLit` is intentionally `ForwardBase` only in Phase 6.1. Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, and Lit Picking are not yet implemented. |
| Scope guard | Passed | Passed | UI Lit remains Pending, Distortion runtime remains Pending, and optimizer/replacer runtime behavior remains Pending. |

Phase 6.1 compatibility annotations:

- `Nova/Particles/UberLit` minimum `ForwardBase` runtime behavior is Approximate. It is a Built-in Lambert diffuse plus ambient/SH path, not a port of URP `UniversalFragmentPBR`.
- Main directional light behavior is Approximate and limited to Built-in forward base main light uniforms. URP additional light loops, Forward+, cluster light loop, and URP shadow data are preserved only as keyword compatibility surfaces in this phase.
- Ambient / SH behavior is Approximate. The Built-in path uses `ShadeSH9` with a `UNITY_LIGHTMODEL_AMBIENT` fallback instead of URP baked GI, probe-volume, and APV behavior.
- Base map 2D lit albedo, `_TINT_AREA_ALL` / `_TINT_COLOR_ENABLED`, vertex color multiplication, alpha clipping, and alpha output preservation are implemented and smoke-tested for the minimum Lit path.
- Lit property preservation is Exact for the audited public shader property surface. Material/editor keyword setup remains covered as an API regression surface.
- Lit keyword declarations are preserved for compile compatibility, including `EVALUATE_SH_MIXED`, `EVALUATE_SH_VERTEX`, `_ADDITIONAL_LIGHTS`, `_ADDITIONAL_LIGHTS_VERTEX`, `_ADDITIONAL_LIGHT_SHADOWS`, `_CLUSTER_LIGHT_LOOP`, `_ENVIRONMENT_REFLECTIONS_ENABLED`, `_FORWARD_PLUS`, `_MAIN_LIGHT_SHADOWS`, `_MAIN_LIGHT_SHADOWS_CASCADE`, `_METALLIC_MAP_ENABLED`, `_NORMAL_MAP_ENABLED`, `_RECEIVE_SHADOWS_ENABLED`, `_SHADOWS_SOFT`, `_SMOOTHNESS_MAP_ENABLED`, `_SPECULAR_HIGHLIGHTS_ENABLED`, `_SPECULAR_MAP_ENABLED`, and `_SPECULAR_SETUP`.
- Unsupported URP-specific runtime behavior in this phase: Forward+, cluster light loop, URP additional light loop, probe volumes, foveated rendering, and URP `UniversalFragmentPBR`.
- Intentionally pending for later Lit phases: metallic workflow runtime, specular workflow runtime, normal map runtime, smoothness map runtime, receive shadows runtime, environment reflections runtime, `ForwardAdd`, Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, Lit Picking, full visual parity, and golden image visual regression.
- `Nova/UIParticles/UberLit`, Distortion runtime, and optimizer/replacer runtime behavior remain outside Phase 6.1 scope and are not promoted by this phase.

## Phase 6.2A UberLit Surface Specular Status

Phase 6.2A expands only the primary `Nova/Particles/UberLit` Built-in `ForwardBase` path. It adds a local Built-in approximation for metallic/specular workflow inputs, 2D metallic/specular/smoothness/normal maps, smoothness, bump scale, and basic specular highlights. It does not attempt URP `UniversalFragmentPBR` parity, does not add receive shadows, environment reflections, additional lights, `ForwardAdd`, Lit ShadowCaster / DepthOnly / DepthNormals / SceneSelectionPass / Picking, `Nova/UIParticles/UberLit`, Distortion runtime, optimizer/replacer runtime behavior, golden image visual regression, 2D array Lit surface maps, or 3D Lit surface maps.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Metallic workflow | Passed | Passed | `_LitWorkflowMode` / `_SPECULAR_SETUP` semantics follow the original editor setup: workflow mode `1` is metallic, mode `0` is specular. Metallic mode uses `_Metallic` and feeds a Built-in specular approximation; diffuse/base color remains the Phase 6.1 albedo path. |
| Metallic map 2D and channel | Passed | Passed | `_METALLIC_MAP_ENABLED` samples `_MetallicMap` in 2D mode, selects `_MetallicMapChannelsX`, multiplies by `_Metallic`, and affects the approximation. `_MetallicMap2DArray` and `_MetallicMap3D` remain runtime-pending. |
| Specular workflow and color | Passed | Passed | `_SPECULAR_SETUP` switches to specular workflow and uses `_SpecularColor` for the highlight approximation. In metallic workflow, specular color is approximated from dielectric 0.04 to albedo by metallic value. |
| Specular map 2D | Passed | Passed | `_SPECULAR_MAP_ENABLED` samples `_SpecularMap` in 2D mode and multiplies RGB by `_SpecularColor`, matching the original RGB map semantics. `_SpecularMapChannelsX` remains preserved; the original runtime path does not use it for RGB specular sampling. `_SpecularMap2DArray` and `_SpecularMap3D` remain runtime-pending. |
| Specular highlights toggle | Passed | Passed | `_SPECULAR_HIGHLIGHTS_ENABLED` gates the local specular contribution. `_SpecularHighlights` remains the material/editor property that drives the keyword through the preserved setup path. |
| Smoothness scalar and map 2D | Passed | Passed | `_Smoothness` shapes and scales the Blinn-Phong-style highlight. `_SMOOTHNESS_MAP_ENABLED` samples `_SmoothnessMap`, selects `_SmoothnessMapChannelsX`, and multiplies by `_Smoothness`. `_SmoothnessMap2DArray` and `_SmoothnessMap3D` remain runtime-pending. |
| Normal map 2D and bump scale | Passed | Passed | `_NORMAL_MAP_ENABLED` samples `_NormalMap`, unpacks a tangent-space 2D normal, applies `_NormalMapBumpScale`, and transforms it through Built-in tangent/bitangent/normal interpolation. `_NormalMap2DArray` and `_NormalMap3D` remain runtime-pending. |
| Basic specular approximation | Passed | Passed | The local model is a Built-in Blinn-Phong-style approximation using main light direction, view direction, smoothness-shaped exponent, and workflow-specific specular color/strength. It is not energy-equivalent to URP and does not claim PBR parity. |
| Lit feature interactions | Passed | Passed | Phase 6.2A smoke tests verify the new surface features preserve alpha test, tint color, and vertex color behavior from the Phase 6.1 minimum albedo/alpha path. |
| Lit keyword compile compatibility | Passed | Passed | Compile diagnostics cover `_SPECULAR_SETUP`, `_NORMAL_MAP_ENABLED`, `_SPECULAR_MAP_ENABLED`, `_METALLIC_MAP_ENABLED`, `_SMOOTHNESS_MAP_ENABLED`, `_SPECULAR_HIGHLIGHTS_ENABLED`, representative Phase 6.2A combinations, and compatibility-only URP keywords for shadows/additional lights/Forward+/cluster light loop. |
| Pass topology | Passed | Passed | `Nova/Particles/UberLit` remains a single primary Built-in `ForwardBase` runtime pass. Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, and Lit Picking are not yet implemented. |
| Scope guard | Passed | Passed | UI Lit remains Pending, Distortion runtime remains Pending, and optimizer/replacer runtime behavior remains Pending. No golden images were added or updated. |

Phase 6.2A compatibility annotations:

- Metallic workflow, metallic map 2D, metallic map channel selection, specular workflow, specular color, specular map 2D, smoothness scalar, smoothness map 2D, smoothness map channel selection, normal map 2D, bump scale, and basic specular highlights are implemented for `Nova/Particles/UberLit` `ForwardBase` only.
- The Lit BRDF is Approximate. It uses a local Built-in Blinn-Phong-style term rather than URP `UniversalFragmentPBR`, URP BRDF data, URP reflection probes, URP shadows, or URP light loops.
- Specular map channel status is property-preserved. The original Lit forward helper samples specular map RGB directly, so Phase 6.2A keeps RGB semantics instead of inventing `_SpecularMapChannelsX` runtime channel extraction.
- Runtime-pending Lit surface texture modes: `_NormalMap2DArray`, `_NormalMap3D`, `_SpecularMap2DArray`, `_SpecularMap3D`, `_MetallicMap2DArray`, `_MetallicMap3D`, `_SmoothnessMap2DArray`, and `_SmoothnessMap3D`.
- Runtime-pending lighting and topology features: `_LitReceiveShadows`, `_EnvironmentReflections`, additional lights, `ForwardAdd`, Forward+, cluster light loop, probe volumes, foveated rendering, Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, Lit Picking, full visual parity, and golden image visual regression.
- `Nova/UIParticles/UberLit`, Distortion runtime, and optimizer/replacer runtime behavior remain outside Phase 6.2A scope and are not promoted by this phase.

## Phase 6.2B UberLit Texture Mode Status

Phase 6.2B expands only the primary `Nova/Particles/UberLit` Built-in `ForwardBase` surface approximation. It adds runtime sampling for Lit-specific normal, metallic, specular, and smoothness maps in 2D array and 3D texture modes. It does not attempt URP `UniversalFragmentPBR` parity, receive shadows, environment reflections, additional lights, `ForwardAdd`, Forward+, cluster light loop, probe volumes, foveated rendering, Lit ShadowCaster / DepthOnly / DepthNormals / SceneSelectionPass / Picking, `Nova/UIParticles/UberLit`, Distortion runtime, optimizer/replacer runtime behavior, golden image visual regression, or full visual parity.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Lit texture-mode resolver | Passed | Passed | Lit surface maps follow the original material setup rule: `_BaseMapMode` selects the source texture family. `SingleTexture` samples 2D maps, `FlipBook` samples 2D array maps, and `FlipBookBlending` samples 3D maps. No new Lit-specific progress, slice, or mode properties were added. |
| Normal map 2D array | Passed | Passed | `_NORMAL_MAP_ENABLED` samples `_NormalMap2DArray` when `_BASE_MAP_MODE_2D_ARRAY` is active, unpacks tangent-space normals, applies `_NormalMapBumpScale`, and affects diffuse/specular lighting through the existing Built-in tangent basis. |
| Normal map 3D | Passed | Passed | `_NORMAL_MAP_ENABLED` samples `_NormalMap3D` when `_BASE_MAP_MODE_3D` is active. This is Approximate because 3D texture sampling and tangent-space normal precision are backend-dependent. |
| Metallic map 2D array and channel | Passed | Passed | `_METALLIC_MAP_ENABLED` samples `_MetallicMap2DArray`, selects `_MetallicMapChannelsX`, multiplies by `_Metallic`, and affects the local specular approximation. |
| Metallic map 3D and channel | Passed | Passed | `_METALLIC_MAP_ENABLED` samples `_MetallicMap3D`, preserves `_MetallicMapChannelsX` selection, and affects the local specular approximation. |
| Specular map 2D array | Passed | Passed | `_SPECULAR_MAP_ENABLED` samples `_SpecularMap2DArray` in specular workflow and multiplies RGB by `_SpecularColor`. `_SpecularMapChannelsX` remains property-preserved; the original Lit forward path uses RGB specular map semantics. |
| Specular map 3D | Passed | Passed | `_SPECULAR_MAP_ENABLED` samples `_SpecularMap3D` in specular workflow and preserves `_SpecularColor` tinting. |
| Smoothness map 2D array and channel | Passed | Passed | `_SMOOTHNESS_MAP_ENABLED` samples `_SmoothnessMap2DArray`, selects `_SmoothnessMapChannelsX`, multiplies by `_Smoothness`, and affects the specular shape/intensity. |
| Smoothness map 3D and channel | Passed | Passed | `_SMOOTHNESS_MAP_ENABLED` samples `_SmoothnessMap3D`, preserves `_SmoothnessMapChannelsX` selection, and affects the specular shape/intensity. |
| Lit surface interaction smokes | Passed | Passed | PlayMode coverage verifies normal/metallic/smoothness 2D array stack behavior, normal/metallic/smoothness 3D stack behavior, and preservation of alpha test, base map 2D, tint, vertex color, metallic/specular workflow, smoothness, and normal map 2D regressions. |
| Shader compiler diagnostics | Passed | Passed | EditMode compile coverage includes normal/metallic/specular/smoothness 2D array and 3D cases, normal array/3D plus specular workflow, metallic plus smoothness array/3D, specular plus smoothness array/3D, one representative Phase 6.2B combination, and compatibility-only URP lighting keywords. |
| Texture array / 3D platform support | Passed | Passed | The shader remains at `#pragma target 3.5`, matching the existing texture array requirement. 2D array and 3D support remain Approximate because target backend support can vary outside the tested editor paths. |
| Pass topology | Passed | Passed | `Nova/Particles/UberLit` remains a single primary Built-in `ForwardBase` runtime pass. Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, and Lit Picking remain not implemented. |
| Scope guard | Passed | Passed | UI Lit remains Pending, Distortion runtime remains Pending, optimizer/replacer runtime behavior remains Pending, and no golden images were added or updated. |

Phase 6.2B compatibility annotations:

- Normal map 2D array, normal map 3D, metallic map 2D array, metallic map 3D, specular map 2D array, specular map 3D, smoothness map 2D array, and smoothness map 3D are implemented for `Nova/Particles/UberLit` `ForwardBase` only.
- Lit map texture selection is intentionally coupled to `_BaseMapMode`, matching the original editor/material post-processor. `_BaseMapProgress` and `_BaseMapSliceCount` drive the 2D array slice or 3D depth coordinate; no new public material properties were introduced.
- Metallic and smoothness channel selection is implemented for 2D, 2D array, and 3D maps through `_MetallicMapChannelsX` and `_SmoothnessMapChannelsX`.
- Specular map semantics remain RGB-based for 2D, 2D array, and 3D maps. `_SpecularMapChannelsX` is still preserved as a public property but is not used by the original Lit forward RGB specular sampling path.
- The Lit BRDF remains Approximate. Phase 6.2B expands texture sources feeding the Phase 6.2A Blinn-Phong-style main-light approximation; it does not claim URP PBR parity.
- Texture array and 3D texture behavior is Approximate at the platform level. Unity 2022.3.22f1 and Unity 6000.0.74f1 editor smoke coverage passes, but target backend support can still vary outside these editor paths.
- Shader target / pragma status: no broad target increase was added; the shader remains at `#pragma target 3.5`.
- Property preservation remains Exact for the audited public Lit map properties. Material/editor setup still controls Lit map keywords by checking the texture property that corresponds to `_BaseMapMode`.
- Lit keyword compile compatibility remains compile-only for `_RECEIVE_SHADOWS_ENABLED`, `_ENVIRONMENT_REFLECTIONS_ENABLED`, `_MAIN_LIGHT_SHADOWS`, `_MAIN_LIGHT_SHADOWS_CASCADE`, `_SHADOWS_SOFT`, `_ADDITIONAL_LIGHTS`, `_ADDITIONAL_LIGHTS_VERTEX`, `_ADDITIONAL_LIGHT_SHADOWS`, `_FORWARD_PLUS`, and `_CLUSTER_LIGHT_LOOP`; Phase 6.2B does not claim runtime behavior for those keywords.
- Runtime-pending lighting and topology features: receive shadows, environment reflections, additional lights / `ForwardAdd`, Forward+, cluster light loop, probe volumes, foveated rendering, Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, Lit Picking, full visual parity, and golden image visual regression.
- `Nova/UIParticles/UberLit`, Distortion runtime, and optimizer/replacer runtime behavior remain outside Phase 6.2B scope and are not promoted by this phase.

## Phase 6.3A UberLit Receive Shadows Status

Phase 6.3A expands only the primary `Nova/Particles/UberLit` Built-in `ForwardBase` approximation. It adds Built-in main directional receive-shadow attenuation for other shadow-casting objects. It does not add Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, Lit Picking, `ForwardAdd`, additional lights, environment reflections, Forward+, cluster light loop, probe volumes, foveated rendering, `Nova/UIParticles/UberLit`, Distortion runtime, optimizer/replacer runtime behavior, golden image visual regression, or URP shadow parity.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Receive shadows runtime | Passed | Passed | `_RECEIVE_SHADOWS_ENABLED` and `_LitReceiveShadows > 0.5` apply Built-in `AutoLight.cginc` ForwardBase shadow attenuation to direct diffuse/specular lighting. Ambient remains unshadowed. |
| `_LitReceiveShadows` | Passed | Passed | Original editor/material semantics are preserved: the material setup path enables `_RECEIVE_SHADOWS_ENABLED` when `_LitReceiveShadows` is greater than `0.5`. Runtime also checks the property to avoid stale keyword-only behavior. |
| `_RECEIVE_SHADOWS_ENABLED` | Passed | Passed | Disabling the keyword bypasses receive-shadow attenuation even when `_LitReceiveShadows` remains enabled. |
| Main directional shadow attenuation | Passed | Passed | Built-in `#pragma multi_compile_fwdbase`, local `NovaTransferShadowCoord`, and local `NovaComputeShadowAttenuation` wrappers use Built-in ForwardBase shadow variants. This is not URP main-light shadow parity. |
| Soft shadow compatibility | Compile-verified | Passed | `_SHADOWS_SOFT` remains compile-compatible. Runtime softness is controlled by Built-in light and quality settings, not by URP keyword semantics alone. No separate soft-shadow PlayMode assertion is claimed in this phase. |
| Cascade compatibility | Compile-verified | Passed | `_MAIN_LIGHT_SHADOWS_CASCADE` remains compile-compatible. Runtime cascade selection is controlled by Built-in quality settings and screen-space shadow resolve behavior, not URP cascade data parity. |
| Metallic/specular interaction | Passed | Passed | Receive-shadow smoke tests cover metallic workflow and specular workflow while preserving the Phase 6.2A/6.2B surface approximation. |
| Normal map interaction | Passed | Passed | Receive-shadow smoke tests cover `_NORMAL_MAP_ENABLED` with direct lighting and shadow attenuation active. |
| Alpha test interaction | Passed | Passed | Receive-shadow smoke tests cover `_ALPHATEST_ENABLED`; visible pixels still receive shadows and clipped pixels remain at the known background/proxy color. |
| Tint / vertex color interaction | Passed | Passed | Receive-shadow smoke tests verify tinted and vertex-colored albedo remains tinted while shadowed regions darken. |
| Ambient under shadows | Passed | Passed | Ambient is deliberately preserved while direct diffuse/specular lighting is shadowed. This is a Built-in approximation, not URP baked/global illumination parity. |
| Shader compiler diagnostics | Passed | Passed | Compile coverage includes `_RECEIVE_SHADOWS_ENABLED`, `_MAIN_LIGHT_SHADOWS`, `_MAIN_LIGHT_SHADOWS_CASCADE`, `_SHADOWS_SOFT`, receive shadows plus those keywords, receive shadows plus metallic/specular/normal/smoothness/alpha-test states, and representative 2D array / 3D Lit map states. |
| Pass topology | Passed | Passed | `Nova/Particles/UberLit` remains a single primary Built-in `ForwardBase` runtime pass. Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, and Lit Picking remain not implemented. |
| Scope guard | Passed | Passed | UI Lit remains Pending, Distortion runtime remains Pending, optimizer/replacer runtime behavior remains Pending, and no golden images were added or updated. |

Phase 6.3A compatibility annotations:

- Receive shadows are Approximate. The Built-in port uses Built-in ForwardBase shadow variants and `AutoLight.cginc` wrappers; it does not reproduce URP shadow bias, URP screen-space shadow data, URP cascade data, URP punctual shadows, or `UniversalFragmentPBR` lighting behavior.
- Built-in receive-shadow runtime is limited to the current primary `ForwardBase` approximation for `Nova/Particles/UberLit`. No `ForwardAdd` or additional-light shadow behavior is implemented.
- `_LitReceiveShadows` and `_RECEIVE_SHADOWS_ENABLED` are runtime-active for direct diffuse/specular attenuation. Ambient remains unshadowed by design for this phase.
- Built-in soft shadow and cascade behavior are controlled by `Light` and `QualitySettings`; `_SHADOWS_SOFT` and `_MAIN_LIGHT_SHADOWS_CASCADE` are preserved as compile-compatible URP-style keyword surfaces, not exact runtime controls.
- Receive-shadow PlayMode smokes use a Built-in depth proxy to exercise screen-space directional shadows without adding a Nova Lit ShadowCaster or DepthOnly pass. This keeps Phase 6.3A scoped to receiving shadows from other shadow-casting objects and does not make `Nova/Particles/UberLit` cast shadows.
- Metallic workflow, specular workflow, normal map, smoothness, base map 2D, tint, vertex color, and alpha test continue to use the Phase 6.2A/6.2B Built-in surface approximation under receive shadows.
- Unsupported or pending URP-specific behavior remains: URP shadow bias parity, URP punctual shadow parity, URP additional light shadows, Forward+, cluster light loop, probe volumes, foveated rendering, Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, Lit Picking, environment reflections, full PBR parity, full visual parity, and golden image visual regression.
- `Nova/UIParticles/UberLit`, Distortion runtime, and optimizer/replacer runtime behavior remain outside Phase 6.3A scope and are not promoted by this phase.

## Phase 6.3B UberLit Environment Reflections Status

Phase 6.3B expands only the primary `Nova/Particles/UberLit` Built-in `ForwardBase` approximation. It adds a local Built-in environment reflection approximation using Built-in global reflection/skybox cubemap data. It does not add additional lights, `ForwardAdd`, Forward+, cluster light loop, probe volumes, foveated rendering, Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, Lit Picking, `Nova/UIParticles/UberLit`, Distortion runtime, optimizer/replacer runtime behavior, golden image visual regression, URP reflection-probe parity, or URP PBR parity.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Environment reflections runtime | Passed | Passed | `_ENVIRONMENT_REFLECTIONS_ENABLED` and `_EnvironmentReflections > 0.5` add a visible Built-in reflection contribution in the primary `ForwardBase` path. |
| `_EnvironmentReflections` | Passed | Passed | Original editor/material semantics are preserved: the material setup path enables `_ENVIRONMENT_REFLECTIONS_ENABLED` when `_EnvironmentReflections` is greater than `0.5`. Runtime also checks the property so stale keyword-only state cannot force reflections on. |
| `_ENVIRONMENT_REFLECTIONS_ENABLED` | Passed | Passed | Disabling the keyword bypasses environment reflection contribution even when `_EnvironmentReflections` remains enabled. |
| Built-in reflection probe / skybox approximation | Passed | Passed | The port samples `unity_SpecCube0` through local Nova-prefixed helpers and `DecodeHDR`. This uses Built-in global reflection data and is only an approximation of the original URP environment reflection path. |
| Reflection probe blending / capture | Approximate | Approximate | Direct URP reflection probe blending parity and probe capture behavior are not implemented or claimed. Tests use deterministic Built-in `RenderSettings.customReflection` input rather than relying on probe capture timing. |
| Box projection | Unsupported | Unsupported | Built-in box projection parity is not implemented in Phase 6.3B. The reflection direction is a conservative view-direction/world-normal reflection vector. |
| Smoothness interaction | Passed | Passed | `_Smoothness` and `_SMOOTHNESS_MAP_ENABLED` affect reflection strength and cubemap mip selection in a stable Built-in direction. This is not URP roughness/BRDF parity. |
| Metallic workflow interaction | Passed | Passed | Metallic workflow uses `_Metallic` / `_MetallicMap` to increase and tint reflection response using the local Phase 6.2A/6.2B surface data approximation. |
| Specular workflow interaction | Passed | Passed | `_SPECULAR_SETUP` uses `_SpecularColor` / `_SpecularMap` to color and scale the reflection contribution. |
| Normal map interaction | Passed | Passed | `_NORMAL_MAP_ENABLED` and `_NormalMapBumpScale` affect world normal and therefore reflection direction. Existing 2D, 2D array, and 3D Lit normal map compile coverage remains in place. |
| Receive-shadow interaction | Passed | Passed | Receive shadows continue to attenuate direct diffuse/specular lighting. Environment reflection remains unshadowed in this approximation, matching the documented Built-in separation between direct shadow attenuation and global reflection. |
| Ambient interaction | Passed | Passed | Ambient approximation remains separate from reflection contribution; PlayMode smokes verify ambient-only output and additional reflection output are distinguishable. |
| Alpha test interaction | Passed | Passed | `_ALPHATEST_ENABLED` still clips pixels before the reflection contribution is returned. |
| Tint / vertex color interaction | Passed | Passed | Tint and vertex color continue to affect base albedo while reflection contribution remains additive and stable. |
| Shader compiler diagnostics | Passed | Passed | Compile coverage includes `_ENVIRONMENT_REFLECTIONS_ENABLED`, specular workflow, smoothness/metallic/specular/normal maps, receive shadows, alpha test, representative 2D array / 3D Lit map states, and compatibility-only URP lighting keywords. |
| Pass topology | Passed | Passed | `Nova/Particles/UberLit` remains a single primary Built-in `ForwardBase` runtime pass. Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, and Lit Picking remain not implemented. |
| Scope guard | Passed | Passed | UI Lit remains Pending, Distortion runtime remains Pending, optimizer/replacer runtime behavior remains Pending, additional lights/ForwardAdd remain Pending, and no golden images were added or updated. |

Phase 6.3B compatibility annotations:

- Environment reflections are Approximate. The Built-in port samples Built-in `unity_SpecCube0` with local Nova-prefixed helpers; it does not copy URP BRDF, URP reflection probe code, `UniversalFragmentPBR`, probe volumes, or foveated rendering includes.
- `_EnvironmentReflections` and `_ENVIRONMENT_REFLECTIONS_ENABLED` are runtime-active for the primary `ForwardBase` reflection contribution.
- Smoothness controls both reflection strength and a conservative cubemap mip approximation. This gives stable behavior for scalar and map-driven smoothness but is not URP roughness parity.
- Metallic workflow uses the local albedo/metallic approximation for reflection color and strength. Specular workflow uses `_SpecularColor` and `_SpecularMap` directly for reflection color and strength.
- Normal maps affect reflection direction through the same tangent-to-world normal path used by the Phase 6.2A/6.2B direct-light approximation.
- Receive shadows attenuate only direct diffuse/specular lighting. The Phase 6.3B environment reflection term remains unshadowed; this is documented behavior for the Built-in approximation, not URP parity.
- Built-in reflection probe blending, probe capture timing, box projection, URP PBR BRDF parity, URP reflection probe parity, and full visual parity remain unsupported or pending.
- Unsupported or pending URP-specific behavior remains: probe volumes, Forward+, cluster light loop, additional lights, `ForwardAdd`, foveated rendering, Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, Lit Picking, full PBR parity, full visual parity, and golden image visual regression.
- `Nova/UIParticles/UberLit`, Distortion runtime, and optimizer/replacer runtime behavior remain outside Phase 6.3B scope and are not promoted by this phase.

## Phase 6.4A UberLit Additional Lights / ForwardAdd Status

Phase 6.4A expands only `Nova/Particles/UberLit`. It adds a Built-in `ForwardAdd` pass that approximates URP additional-light contribution using Built-in per-light forward-add globals and attenuation. It does not implement URP Forward+, a clustered light loop, probe volumes, foveated rendering, Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, Lit Picking, `Nova/UIParticles/UberLit`, Distortion runtime, optimizer/replacer runtime behavior, golden image visual regression, URP additional-light-loop parity, or URP PBR parity.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| ForwardAdd pass topology | Passed | Passed | `Nova/Particles/UberLit` now has one Built-in `ForwardBase` pass and one Built-in `ForwardAdd` pass. Lit ShadowCaster, DepthOnly, DepthNormals, SceneSelectionPass, and Picking remain not implemented. |
| Additional lights runtime | Passed | Passed | `_ADDITIONAL_LIGHTS` gates the Built-in `ForwardAdd` contribution. This is Approximate and uses Built-in pass scheduling rather than URP's additional-light loop. |
| Additional point lights | Passed | Passed | Point-light color, position, and range attenuation are covered by non-golden PlayMode smoke tests. The falloff follows Built-in forward-add attenuation, not URP. |
| Additional spot lights | Passed | Passed | Spot-light cone/direction and color are covered by smoke tests. The result is Approximate. |
| Surface interaction | Passed | Passed | Additional-light diffuse/specular uses the Phase 6.2A/6.2B local metallic, specular, smoothness, and normal-map approximation. It does not call `UniversalFragmentPBR`. |
| Receive-shadow interaction | Passed | Passed | Existing receive-shadow behavior remains in `ForwardBase`; an additional point light remains distinguishable in the receive-shadow smoke. Additional-light shadows are not claimed as URP parity. |
| Environment-reflection interaction | Passed | Passed | Environment reflections remain `ForwardBase` only. `ForwardAdd` does not add ambient or reflection terms again. |
| Alpha, tint, and vertex color regressions | Passed | Passed | Additional-light smokes cover alpha test, tint color, and mesh vertex color. Transparent blend behavior remains a Built-in additive-pass approximation. |
| `_ADDITIONAL_LIGHTS_VERTEX` | Compile-compatible | Compile-compatible | Preserved as a keyword surface and compile diagnostic. Runtime vertex additional-light behavior is not implemented in Phase 6.4A. |
| `_ADDITIONAL_LIGHT_SHADOWS` | Compile-compatible / Built-in controlled | Compile-compatible / Built-in controlled | Preserved as a keyword surface and compile diagnostic. Built-in additional-light shadows, where available, are controlled by Built-in light/shadow variants and quality settings rather than URP-style keyword semantics. No additional-light shadow parity is claimed. |
| `_FORWARD_PLUS` / `_CLUSTER_LIGHT_LOOP` | Unsupported runtime | Unsupported runtime | Keywords remain compile-compatible only. No Forward+ or clustered loop is implemented. |

Phase 6.4A compatibility annotations:

- Built-in additional lights / `ForwardAdd` are Approximate. The pass uses Built-in `ForwardAdd` scheduling, additive blending, `_WorldSpaceLightPos0`, `_LightColor0`, and Built-in attenuation macros. It is not URP Forward+ and not URP additional-light-loop parity.
- `_ADDITIONAL_LIGHTS` is runtime-active for the new `ForwardAdd` approximation. Materials without this keyword receive no deliberate Nova additional-light contribution, even though Built-in still schedules the pass.
- `_ADDITIONAL_LIGHTS_VERTEX`, `_ADDITIONAL_LIGHT_SHADOWS`, `_FORWARD_PLUS`, and `_CLUSTER_LIGHT_LOOP` remain compatibility keywords. Phase 6.4A does not implement vertex additional lights, URP additional-light shadow parity, Forward+, or a cluster light loop.
- Additional-light contribution uses albedo, alpha, tint, vertex color, normal map, metallic/specular workflow, smoothness, and specular-highlight controls from the existing Built-in Lit surface approximation. It intentionally does not add ambient or environment reflection in the `ForwardAdd` pass.
- Receive shadows remain a `ForwardBase` main-light approximation. Additional-light shadows are not marked complete; Built-in full-shadow variants may compile, but runtime behavior depends on Built-in light and quality settings and is not a URP `_ADDITIONAL_LIGHT_SHADOWS` equivalent.
- Unsupported or pending URP-specific behavior remains: URP additional-light-loop parity, Forward+, cluster light loop, probe volumes, foveated rendering, Lit ShadowCaster, Lit DepthOnly, Lit DepthNormals, Lit SceneSelectionPass, Lit Picking, full PBR parity, full visual parity, and golden image visual regression.
- `Nova/UIParticles/UberLit`, Distortion runtime, and optimizer/replacer runtime behavior remain outside Phase 6.4A scope and are not promoted by this phase.

## Phase 6.5A UberLit Pass Topology Status

Phase 6.5A expands only `Nova/Particles/UberLit` pass topology. It adds Built-in-compatible `ShadowCaster`, `DepthOnly`, `SceneSelectionPass`, and `Picking` passes while preserving the Phase 6.4A `ForwardBase` and `ForwardAdd` lighting behavior. It does not add new Lit surface features, UI Lit, Distortion runtime, optimizer/replacer runtime behavior, golden image visual regression, URP ShadowCaster parity, URP DepthOnly parity, or URP DepthNormals parity.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Lit pass topology | Passed | Passed | `Nova/Particles/UberLit` declares `ForwardBase`, `ForwardAdd`, `ShadowCaster`, `DepthOnly`, `SceneSelectionPass`, and `Picking`. It deliberately does not declare `DepthNormals`. |
| Lit ShadowCaster | Passed | Passed | A Built-in `ShadowCaster` pass uses local Built-in helpers and participates in Built-in shadow casting. Built-in ShadowCaster bias/punctual shadow behavior is Approximate and not URP shadow bias or URP punctual shadow parity. |
| `_ShadowCasterEnabled` | Passed | Passed | The preserved material setup path enables/disables the `ShadowCaster` pass with `Material.SetShaderPassEnabled`. Runtime smoke coverage also uses `Renderer.shadowCastingMode` as the Built-in fallback path when comparing disabled casting, matching the previously documented UberUnlit pattern. |
| ShadowCaster alpha test | Passed | Passed | `_ShadowCasterAlphaTestEnabled` enables `_SHADOW_CASTER_ALPHA_TEST_ENABLED`, and `_ShadowCasterAlphaCutoff` clips shadow caster fragments against the shadow alpha source. This is separate from visible forward `_Cutoff` unless forward alpha-test keywords are also enabled. |
| Shadow alpha affected by tint color | Passed | Passed | `_ShadowCasterAlphaAffectedByTintColor` is runtime-active for `_TINT_AREA_ALL` plus `_TINT_COLOR_ENABLED` and is covered by non-golden shadow smoke. |
| Shadow alpha affected by flow map | Compile-compatible | Compile-compatible | `_ShadowCasterAlphaAffectedByFlowMap` is implemented for the Built-in shadow alpha source when flow targets the base map. Runtime visual validation remains pending because Phase 6.5A keeps the stable PlayMode coverage to the tint alpha toggle. |
| Shadow alpha affected by alpha transition map | Compile-compatible | Compile-compatible | `_ShadowCasterAlphaAffectedByAlphaTransitionMap` is implemented for the Built-in shadow alpha source with fade/dissolve transition keywords. Runtime visual validation remains pending. |
| Shadow alpha affected by transparency luminance | Compile-compatible | Compile-compatible | `_ShadowCasterAlphaAffectedByTransparencyLuminance` is implemented for the Built-in shadow alpha source with `_TRANSPARENCY_BY_LUMINANCE`. Runtime visual validation remains pending. |
| `_ShadowCasterApplyVertexDeformation` | Compile-compatible / pending runtime | Compile-compatible / pending runtime | The property and keyword surface are preserved, and the shadow pass can apply the existing narrow vertex deformation path when `_VERTEX_DEFORMATION_ENABLED` is active. Full Lit ForwardBase vertex deformation parity remains pending, so no full Lit deformation parity is claimed. |
| Lit DepthOnly | Passed | Passed | A Built-in-compatible `DepthOnly` pass writes depth in explicit command-buffer smoke coverage and respects `_ALPHATEST_ENABLED` / `_Cutoff`. Built-in camera consumption of a custom `DepthOnly` pass remains Approximate. |
| SceneSelectionPass | Compile-compatible | Compile-compatible | The editor pass is declared and compiles with local Built-in helpers. Editor picking/selection visual parity is not claimed. |
| Picking | Compile-compatible | Compile-compatible | The editor picking pass is declared and compiles with local Built-in helpers. Stable editor picking visual parity is not claimed. |
| Lit DepthNormals | Unsupported / documented | Unsupported / documented | Phase 6.5A intentionally omits a `DepthNormals` pass because Built-in has no URP DepthNormals prepass contract and no tested Built-in consumer for this port stage. |
| Shader compiler diagnostics | Passed | Passed | Diagnostics cover ShadowCaster default, shadow alpha test, shadow alpha influence toggles, representative Lit surface keywords, DepthOnly default, DepthOnly alpha/transition/Lit keyword states, SceneSelectionPass, Picking, and existing ForwardBase/ForwardAdd cases. |
| Rendering smokes | Passed | Passed | Non-golden PlayMode smokes cover enabled/disabled shadow casting, shadow alpha cutoff, shadow alpha-test disabled behavior, tint-driven shadow alpha, explicit DepthOnly depth writes, DepthOnly alpha clipping, and a ForwardBase/ForwardAdd regression. |
| Scope guard | Passed | Passed | `Nova/UIParticles/UberLit`, Distortion runtime, optimizer/replacer runtime behavior, and golden images remain untouched. |

Phase 6.5A compatibility annotations:

- Lit ShadowCaster is Approximate. The Built-in port uses Built-in shadow caster macros and local wrappers, not URP `Shadows.hlsl`, URP `ShadowUtils`, URP shadow constants, or URP normal-bias parity.
- `_ShadowCasterEnabled` is preserved as the public material compatibility surface. Built-in cannot make every material property independently remove a pass at render time without material setup, so the editor/material setup path calls `SetShaderPassEnabled`, and runtime smoke comparisons use `Renderer.shadowCastingMode` as the documented fallback.
- ShadowCaster alpha testing uses `_ShadowCasterAlphaCutoff` and the shadow-specific keyword. It does not silently substitute visible forward `_Cutoff` as the shadow cutoff.
- The tint color shadow alpha toggle has stable runtime smoke coverage. Flow map, alpha transition, and luminance transparency shadow alpha toggles are implemented and compile-covered, but runtime visual validation remains pending until a broader Lit feature interaction phase.
- `_ShadowCasterApplyVertexDeformation` is preserved and compile-compatible, but full Lit vertex deformation parity remains pending because Phase 6.5A does not implement broad Lit ForwardBase vertex deformation.
- Lit DepthOnly is Approximate. Explicit command-buffer smoke proves the pass can write depth and alpha-clip, but Built-in camera depth consumption differs from URP DepthOnly prepass behavior.
- SceneSelectionPass and Picking are compile-compatible editor passes only. The phase does not claim editor selection or picking visual parity.
- Lit DepthNormals is Unsupported for Phase 6.5A. Built-in has no URP DepthNormals prepass contract, so the port does not add a fake feature-complete `DepthNormals` pass without a clear consumer and stable test.
- `Nova/Particles/UberLit` still does not claim full PBR parity, URP Forward+ parity, URP additional-light-loop parity, URP shadow parity, final visual parity, or golden image completion.
- The next planned Lit phase is Phase 6.6A: `Nova/Particles/UberLit` consolidation and feature interaction regression.

## Phase 6.6A UberLit Consolidation Status

Phase 6.6A consolidates the current `Nova/Particles/UberLit` Built-in implementation after Phase 6.5A. It adds high-value interaction smoke coverage, complex compile permutation coverage, pass topology/status regressions, material/editor setup checks, and an implementation status audit. It does not port `Nova/UIParticles/UberLit`, Distortion, Optimized Shader Generator/Replacer runtime behavior, URP PBR parity, Forward+, cluster light loops, probe volumes, foveated rendering, URP DepthNormals parity, final visual regression, or golden image baselines.

### Implemented and smoke-tested

- Minimum `ForwardBase` lit path.
- Main directional light Lambert approximation.
- Ambient / SH approximation.
- Base map 2D lit albedo.
- Tint and vertex color lit albedo.
- Alpha test.
- Metallic workflow.
- Metallic map 2D / 2D array / 3D.
- Metallic map channel selection.
- Specular workflow.
- Specular color.
- Specular map 2D / 2D array / 3D.
- Smoothness scalar.
- Smoothness map 2D / 2D array / 3D.
- Smoothness channel selection.
- Normal map 2D / 2D array / 3D.
- Normal bump scale.
- Basic Blinn-Phong-style specular approximation.
- Receive shadows.
- `_LitReceiveShadows`.
- `_RECEIVE_SHADOWS_ENABLED`.
- Environment reflections.
- `_EnvironmentReflections`.
- `_ENVIRONMENT_REFLECTIONS_ENABLED`.
- Additional lights via `ForwardAdd`.
- Point light contribution.
- Spot light contribution.
- `_ADDITIONAL_LIGHTS`.
- ShadowCaster.
- ShadowCaster alpha test.
- DepthOnly.
- SceneSelectionPass compile compatibility.
- Picking compile compatibility.
- Phase 6.6A interaction smoke coverage for complex surface lighting, receive shadows plus environment reflections, additional lights plus environment reflections, additional lights plus receive shadows, ShadowCaster plus Lit surface keywords, DepthOnly plus alpha and Lit surface keywords, ForwardBase/ForwardAdd alpha/tint/vertex color, and a combined main/additional/reflection/shadow feature scene.
- Phase 6.6A complex compile permutation coverage for representative 2D array, 3D, ForwardAdd, receive-shadow, environment-reflection, ShadowCaster, DepthOnly, SceneSelectionPass/Picking, major Lit keyword, compatibility-only URP keyword, and additional-light keyword states.
- Phase 6.6A pass topology regression verifies `ForwardBase`, `ForwardAdd`, `ShadowCaster`, `DepthOnly`, `SceneSelectionPass`, and `Picking` are declared, and `DepthNormals`, `UniversalForward`, UniversalPipeline tags, and URP shader includes are not declared.
- Phase 6.6A material/editor API regression verifies the `CustomEditor "Nova.Editor.Core.Scripts.ParticlesUberLitGUI"` declaration remains intact and material setup still toggles the already-implemented Lit feature keywords.

### Implemented but approximate

- Built-in Lambert / Blinn-Phong-style lighting approximation.
- Built-in BRDF approximation, not URP PBR parity.
- Built-in receive-shadow approximation.
- Built-in soft/cascade shadow behavior controlled by Light/QualitySettings.
- Built-in environment reflection via `unity_SpecCube0` / skybox approximation.
- No URP reflection probe blending parity.
- No box projection parity.
- Built-in `ForwardAdd` approximation.
- Additional light shadows compile-compatible only unless runtime-tested.
- Built-in shadow caster bias / punctual behavior approximation.
- DepthOnly consumption approximation.
- SceneSelection/Picking compile-compatible editor passes only.
- Environment reflections remain unshadowed by the Built-in approximation; receive shadows attenuate direct lighting only.

### Preserved but not fully parity-tested

- URP-style additional light shadow parity.
- `_ADDITIONAL_LIGHTS_VERTEX` runtime behavior.
- Flow-map shadow alpha real-shadow runtime validation.
- Alpha-transition shadow alpha runtime validation.
- Luminance shadow alpha runtime validation.
- Full Particle System Custom Vertex Streams parity.
- Editor scene selection / picking visual parity.
- Full inspector visual parity.
- Golden image visual parity.
- Final visual regression.

### Unsupported or intentionally not implemented

- URP `UniversalFragmentPBR` parity.
- no full URP PBR parity.
- Forward+ runtime.
- no Forward+ runtime.
- Cluster light loop runtime.
- no cluster light loop runtime.
- Probe volumes.
- no probe volume runtime.
- Foveated rendering.
- no foveated rendering runtime.
- URP DepthNormals prepass parity.
- no URP DepthNormals prepass parity.
- `DepthNormals` pass for Built-in in the current design.
- no Built-in DepthNormals implementation.
- URP debug display path.
- no golden visual parity.
- golden image baselines not added.

Phase 6.6A compatibility annotations:

- `Nova/Particles/UberLit` is a stable Built-in approximation with broader non-golden interaction coverage, not a full UberLit completion claim.
- Interaction smokes are deliberately broad and relative. They verify that implemented feature groups remain distinguishable together without adding golden images or final visual baselines.
- Complex compile coverage is representative, not exhaustive. It is intended to catch include, keyword, pass, lighting, shadow, reflection, and `ForwardAdd` regressions without creating a combinatorial explosion.
- Additional-light shadow parity remains pending. `_ADDITIONAL_LIGHT_SHADOWS` is preserved and compile-covered, but URP-style runtime semantics are not claimed.
- `DepthNormals` remains intentionally omitted because Built-in has no URP DepthNormals prepass contract and no stable Built-in consumer has been defined for this package phase.
- UI Lit remains Pending. `Nova/UIParticles/UberLit` stays a skeleton/API surface and is the next major shader family.
- Distortion runtime remains Pending.
- Optimizer/replacer runtime behavior remains Pending.
- Final visual regression remains Pending, golden image baselines were not added, and no existing golden images were updated.
- The next recommended phase is Phase 7.1: `Nova/UIParticles/UberLit` skeleton-to-minimum Built-in UI forward lit approximation.

## Phase 7.1 UI UberLit Minimum Forward Status

Phase 7.1 begins the `Nova/UIParticles/UberLit` runtime port by replacing the Phase 1/2 fake output with a minimum UI-compatible Built-in forward lit approximation. It does not mark full UI UberLit parity complete.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| Minimum UI ForwardBase lit | Passed | Passed | `Nova/UIParticles/UberLit` has one primary `ForwardBase` pass using local Built-in includes. The pass samples `_BaseMap` in 2D mode, applies `_TintColor` for the all-area tint path, multiplies vertex color and alpha, applies UI rect clip alpha, applies alpha test, and writes lit alpha-preserving color. |
| Main directional light approximation | Passed | Passed | Main directional light color and direction affect output through the shared Built-in Lambert helper. This is Approximate and does not call `UniversalFragmentPBR`. |
| Ambient / SH approximation | Passed | Passed | The UI Lit path uses the shared `ShadeSH9` plus `UNITY_LIGHTMODEL_AMBIENT` fallback helper. This is a Built-in approximation, not URP baked/global illumination parity. |
| Base map 2D lit albedo | Passed | Passed | `_BaseMap` 2D changes lit albedo. 2D array and 3D base map runtime remain pending for UI Lit. |
| Tint / vertex color lit albedo | Passed | Passed | `_TINT_AREA_ALL` plus `_TINT_COLOR_ENABLED` and mesh vertex color affect lit albedo. Full UI UberUnlit tint map, tint rim, flow, tone, parallax, and color-correction behavior is not claimed for UI Lit in this phase. |
| Vertex alpha | Passed | Passed | Vertex alpha is preserved through the final alpha and transparent blending. `_VERTEX_ALPHA_AS_TRANSITION_PROGRESS` is preserved as a keyword surface but transition-progress runtime is not implemented for UI Lit in this phase. |
| Alpha test | Passed | Passed | `_ALPHATEST_ENABLED` clips with `_Cutoff` after base alpha, vertex alpha, and rect clip alpha are resolved. |
| UI stencil | Passed | Passed | The pass declares a stencil block using `_StencilComp`, `_Stencil`, `_StencilOp`, `_StencilWriteMask`, and `_StencilReadMask`; controlled mesh-based UI smoke verifies hide/reveal behavior. |
| `_ColorMask` | Passed | Passed | The pass uses `ColorMask [_ColorMask]`; material setup preserves the property. |
| `UNITY_UI_CLIP_RECT` / rect clip | Passed | Passed | The pass preserves `UNITY_UI_CLIP_RECT` and uses local `NovaUIClipAlpha` / `UnityGet2DClipping` against object-space UI-compatible positions. Controlled mesh-based rect clip smoke verifies clipped output. |
| UI pass topology | Passed | Passed | UI Lit remains a one-pass shader for Phase 7.1. It has no `ForwardAdd`, `ShadowCaster`, `DepthOnly`, `DepthNormals`, `SceneSelectionPass`, or `Picking`. |
| Lit property preservation | Passed | Passed | Lit workflow, receive-shadow, specular, environment-reflection, normal, specular-map, metallic, and smoothness properties remain present with representative property types. |
| Lit keyword compile compatibility | Passed | Passed | Representative UI Lit keywords compile, including `_SPECULAR_SETUP`, `_NORMAL_MAP_ENABLED`, `_SPECULAR_MAP_ENABLED`, `_METALLIC_MAP_ENABLED`, `_SMOOTHNESS_MAP_ENABLED`, `_RECEIVE_SHADOWS_ENABLED`, `_SPECULAR_HIGHLIGHTS_ENABLED`, `_ENVIRONMENT_REFLECTIONS_ENABLED`, `_MAIN_LIGHT_SHADOWS`, `_MAIN_LIGHT_SHADOWS_CASCADE`, `_SHADOWS_SOFT`, `_ADDITIONAL_LIGHTS`, `_ADDITIONAL_LIGHTS_VERTEX`, `_ADDITIONAL_LIGHT_SHADOWS`, `_FORWARD_PLUS`, and `_CLUSTER_LIGHT_LOOP`. Compatibility-only URP keywords do not imply runtime behavior in this phase. |
| Non-UI keyword exclusion | Passed | Passed | UI Lit does not declare `_CASTING_PUNCTUAL_LIGHT_SHADOW`, `_SHADOW_CASTER_ALPHA_TEST_ENABLED`, `EVALUATE_SH_MIXED`, or `EVALUATE_SH_VERTEX`. |
| Shader compiler diagnostics | Passed | Passed | Diagnostics cover default import, base map 2D, alpha test, tint, `UNITY_UI_CLIP_RECT`, stencil property state, major Lit compatibility keywords, URP compatibility keywords, and a representative combined UI Lit keyword state. |
| Full Canvas `Mask` / `RectMask2D` parity | Pending | Pending | Phase 7.1 uses the existing controlled mesh-based UI smoke harness. Full uGUI Canvas mask parity remains pending because the package still does not add a new `com.unity.ugui` dependency. |
| Golden visual parity | Pending | Pending | No golden images were added or updated. |

Phase 7.1 compatibility annotations:

- `Nova/UIParticles/UberLit` now has a minimum UI ForwardBase lit approximation, not full UI Lit parity.
- The UI Lit pass intentionally uses local Built-in helpers only. It does not use URP shader libraries, `UniversalForward`, `UniversalFragmentPBR`, `RenderPipeline` Universal tags, RenderGraph APIs, or URP C# runtime APIs.
- Runtime-active behavior in this phase is limited to base map 2D lit albedo, all-area tint color, vertex color, vertex alpha, alpha test, UI stencil, `_ColorMask`, local rect clipping, main directional light diffuse, and ambient / SH approximation.
- Unsupported or runtime-pending Lit behavior summary for UI Lit: metallic workflow runtime, specular workflow runtime, normal map runtime, smoothness map runtime, receive shadows runtime, environment reflections runtime, additional lights / `ForwardAdd`, Forward+, cluster light loop, probe volumes, foveated rendering, UI Lit ShadowCaster, UI Lit DepthOnly, UI Lit DepthNormals, UI Lit SceneSelectionPass, UI Lit Picking, full Canvas `Mask` / `RectMask2D` parity, full UI UberUnlit feature stack parity, full UI Lit PBR parity, full visual parity, and golden image visual regression.
- Distortion runtime remains Pending.
- Optimizer/replacer runtime behavior remains Pending.
- Final visual regression remains Pending, golden image baselines were not added, and no existing golden images were updated.
- The next recommended phase is Phase 7.2A: `Nova/UIParticles/UberLit` metallic/specular workflow, normal map, smoothness map, and basic specular approximation.

## Phase 7.2A UI UberLit Surface Approximation Status

Phase 7.2A expands only the primary `Nova/UIParticles/UberLit` Built-in `ForwardBase` path. It adds a UI-scoped Built-in approximation for Lit metallic/specular workflow inputs, 2D metallic/specular/smoothness/normal maps, smoothness, normal bump scale, and a basic specular approximation. It does not attempt URP `UniversalFragmentPBR` parity, does not add receive shadows, environment reflections, additional lights, `ForwardAdd`, UI Lit ShadowCaster / DepthOnly / DepthNormals / SceneSelectionPass / Picking, Distortion runtime, optimizer/replacer runtime behavior, golden image visual regression, 2D array Lit surface maps, or 3D Lit surface maps.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| UI metallic workflow | Passed | Passed | `_LitWorkflowMode` metallic mode uses `_Metallic`, and `_METALLIC_MAP_ENABLED` samples `_MetallicMap` 2D with `_MetallicMapChannelsX`. The result affects a Built-in specular approximation, not URP PBR. |
| UI metallic map 2D/channel | Passed | Passed | 2D metallic map sampling and channel selection are covered by PlayMode smokes. 2D array and 3D metallic maps remain runtime-pending. |
| UI specular workflow | Passed | Passed | `_SPECULAR_SETUP` uses `_SpecularColor`, and `_SPECULAR_MAP_ENABLED` samples `_SpecularMap` 2D RGB multiplied by `_SpecularColor`. |
| UI specular map 2D/RGB semantics | Passed | Passed | Specular map 2D uses original RGB semantics for color. `_SpecularMapChannelsX` remains preserved as a material property but is not used by the original specular RGB path. |
| UI specular highlights | Passed | Passed | `_SPECULAR_HIGHLIGHTS_ENABLED` gates the basic highlight contribution. `_SpecularHighlights` is still driven by the existing material setup path. |
| UI smoothness scalar | Passed | Passed | `_Smoothness` shapes/intensifies the basic specular approximation. |
| UI smoothness map 2D/channel | Passed | Passed | `_SMOOTHNESS_MAP_ENABLED` samples `_SmoothnessMap` 2D, selects `_SmoothnessMapChannelsX`, and multiplies by `_Smoothness`. 2D array and 3D smoothness maps remain runtime-pending. |
| UI normal map 2D | Passed | Passed | `_NORMAL_MAP_ENABLED` samples `_NormalMap` 2D, unpacks tangent-space XY, reconstructs Z, and transforms through mesh tangent/bitangent/normal data. |
| UI normal bump scale | Passed | Passed | `_NormalMapBumpScale` scales the unpacked tangent-space XY normal before reconstruction. |
| Basic specular approximation | Passed | Passed | Uses a simple Built-in Blinn-Phong-style direct main-light highlight. Smoothness controls exponent, metallic controls specular color/strength in metallic workflow, and specular workflow uses `_SpecularColor`. |
| UI BRDF limitation | Passed | Passed | The result is approximate and intentionally does not claim URP `UniversalFragmentPBR`, image-based lighting, reflection probe, shadow, Forward+, or clustered-light parity. |
| UI alpha / tint / vertex color regression | Passed | Passed | Surface-feature smokes cover alpha test, tint plus vertex color, and vertex alpha blending. |
| UI stencil / ColorMask / rect clip regression | Passed | Passed | Surface-feature smokes cover controlled stencil/mask behavior, `ColorMask [_ColorMask]`, and local `UNITY_UI_CLIP_RECT` behavior. Full Canvas `Mask` / `RectMask2D` parity remains pending. |
| UI property preservation | Passed | Passed | The Phase 7.1 Lit/UI property inventory remains unchanged, including runtime-pending 2D array/3D surface maps and Lit toggles. |
| UI Lit keyword compile-compatibility | Passed | Passed | Compile diagnostics include `_SPECULAR_SETUP`, `_NORMAL_MAP_ENABLED`, `_SPECULAR_MAP_ENABLED`, `_METALLIC_MAP_ENABLED`, `_SMOOTHNESS_MAP_ENABLED`, `_SPECULAR_HIGHLIGHTS_ENABLED`, combined Phase 7.2A states, `UNITY_UI_CLIP_RECT`, alpha test, and stencil property states. Compatibility-only URP keywords remain compile-only. |
| UI pass topology | Passed | Passed | UI Lit remains a one-pass `ForwardBase` shader. It has no `ForwardAdd`, `ShadowCaster`, `DepthOnly`, `DepthNormals`, `SceneSelectionPass`, `Picking`, `UniversalForward`, or Universal Render Pipeline tag. |
| Non-UI keyword exclusion | Passed | Passed | UI Lit still excludes `_CASTING_PUNCTUAL_LIGHT_SHADOW`, `_SHADOW_CASTER_ALPHA_TEST_ENABLED`, `EVALUATE_SH_MIXED`, and `EVALUATE_SH_VERTEX`. |

Phase 7.2A compatibility annotations:

- `Nova/UIParticles/UberLit` now has a UI metallic workflow approximation, UI specular workflow approximation, UI 2D normal/metallic/specular/smoothness map runtime, UI normal bump scale runtime, and UI basic specular approximation on the one-pass Built-in `ForwardBase` path only.
- The UI Lit surface helper is intentionally separate from the non-UI `Nova/Particles/UberLit` helper so Phase 7.2A does not promote UI receive shadows, UI environment reflections, UI additional lights, or UI `ForwardAdd`.
- UI normal/specular/metallic/smoothness 2D array and 3D maps remain runtime-pending and are the intended scope of the next phase.
- Unsupported or runtime-pending UI Lit behavior summary: 2D array / 3D normal/specular/metallic/smoothness maps, receive shadows, environment reflections, additional lights / `ForwardAdd`, Forward+, cluster light loop, probe volumes, foveated rendering, UI Lit pass topology beyond one-pass forward, UI Lit ShadowCaster, UI Lit DepthOnly, UI Lit DepthNormals, UI Lit SceneSelectionPass, UI Lit Picking, full Canvas `Mask` / `RectMask2D` parity, full UI UberUnlit feature stack parity, full UI Lit PBR parity, full visual parity, and golden image visual regression.
- Distortion runtime, Optimized Shader Generator/Replacer runtime behavior, and final visual regression remain outside Phase 7.2A scope and are not promoted by this phase.
- The next recommended phase is Phase 7.2B: `Nova/UIParticles/UberLit` texture-mode expansion for 2D array / 3D normal, metallic, specular, and smoothness maps.

## Phase 7.2B UI UberLit Texture Mode Status

Phase 7.2B expands only the primary `Nova/UIParticles/UberLit` one-pass Built-in `ForwardBase` surface approximation. Lit-specific normal, metallic, specular, and smoothness maps now follow the original editor/runtime convention that surface-map texture mode is selected by `_BaseMapMode`: single texture samples the 2D map, flip-book samples the 2D array map, and flip-book blending samples the 3D map. This phase does not add UI receive shadows, environment reflections, additional lights, `ForwardAdd`, Forward+, cluster light loop, probe volumes, foveated rendering, UI Lit ShadowCaster / DepthOnly / DepthNormals / SceneSelectionPass / Picking, Distortion runtime, Optimized Shader Generator/Replacer runtime behavior, golden image visual regression, full Canvas `Mask` / `RectMask2D` parity, or URP PBR parity.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| UI normal map 2D array | Passed | Passed | `_NORMAL_MAP_ENABLED` samples `_NormalMap2DArray` when `_BASE_MAP_MODE_2D_ARRAY` is active, unpacks tangent-space normals, applies `_NormalMapBumpScale`, and affects the UI direct-light/specular approximation. |
| UI normal map 3D | Passed | Passed | `_NORMAL_MAP_ENABLED` samples `_NormalMap3D` when `_BASE_MAP_MODE_3D` is active. This remains Approximate because 3D texture sampling and tangent-space normal precision are backend-dependent. |
| UI metallic map 2D array | Passed | Passed | `_METALLIC_MAP_ENABLED` samples `_MetallicMap2DArray`, selects `_MetallicMapChannelsX`, multiplies by `_Metallic`, and affects the local specular response. |
| UI metallic map 3D | Passed | Passed | `_METALLIC_MAP_ENABLED` samples `_MetallicMap3D`, preserves `_MetallicMapChannelsX` channel selection, multiplies by `_Metallic`, and affects the local specular response. |
| UI metallic map channel status | Passed | Passed | `_MetallicMapChannelsX` is runtime-verified for 2D and 2D array paths and preserved for 3D through the same helper. |
| UI specular map 2D array | Passed | Passed | `_SPECULAR_MAP_ENABLED` samples `_SpecularMap2DArray` in specular workflow and multiplies RGB by `_SpecularColor`. |
| UI specular map 3D | Passed | Passed | `_SPECULAR_MAP_ENABLED` samples `_SpecularMap3D` in specular workflow and preserves `_SpecularColor` tinting. |
| UI specular map semantics | Passed | Passed | Specular maps remain RGB-based for 2D, 2D array, and 3D modes. `_SpecularMapChannelsX` is preserved as a public property but is not used by the original Lit RGB specular sampling path. |
| UI smoothness map 2D array | Passed | Passed | `_SMOOTHNESS_MAP_ENABLED` samples `_SmoothnessMap2DArray`, selects `_SmoothnessMapChannelsX`, multiplies by `_Smoothness`, and affects specular shape/intensity. |
| UI smoothness map 3D | Passed | Passed | `_SMOOTHNESS_MAP_ENABLED` samples `_SmoothnessMap3D`, preserves `_SmoothnessMapChannelsX` channel selection, multiplies by `_Smoothness`, and affects specular shape/intensity. |
| UI smoothness map channel status | Passed | Passed | `_SmoothnessMapChannelsX` is runtime-verified for 2D and 2D array paths and preserved for 3D through the same helper. |
| Texture array / 3D texture platform status | Approximate | Approximate | The shader now uses `#pragma target 3.5` and `#pragma require 2darray`, matching the original UI Lit shader's texture-array requirement. Runtime support still depends on the target backend. |
| Shader target / pragma requirements | Passed | Passed | UI Lit was raised from target 3.0 to target 3.5 with `#pragma require 2darray` for array sampling; no URP includes or RenderPipeline tags were added. |
| UI lit surface interaction smoke coverage | Passed | Passed | Representative 2D array and 3D surface stacks cover normal plus metallic plus smoothness interactions without combinatorial expansion. |
| UI alpha / tint / vertex color / vertex alpha regression | Passed | Passed | Phase 7.2B smokes cover alpha test, tint, vertex color, and vertex alpha while array/3D Lit maps are active. |
| UI stencil / ColorMask / rect clip regression | Passed | Passed | Phase 7.2B smokes cover controlled stencil/mask behavior, `ColorMask [_ColorMask]`, and `UNITY_UI_CLIP_RECT` with active array/3D Lit maps. Full Canvas `Mask` / `RectMask2D` parity remains pending. |
| UI property preservation | Passed | Passed | No new public material properties were added. The audited Lit map properties and UI stencil properties remain preserved. |
| UI Lit keyword compile-compatibility | Passed | Passed | Compile diagnostics cover normal/metallic/specular/smoothness 2D array and 3D modes, representative combinations, alpha test, stencil state, `UNITY_UI_CLIP_RECT`, and compatibility-only shadow/additional-light/Forward+ keywords. |
| UI pass topology | Passed | Passed | UI Lit remains a single `ForwardBase` pass with no `ForwardAdd`, `ShadowCaster`, `DepthOnly`, `DepthNormals`, `SceneSelectionPass`, `Picking`, `UniversalForward`, or Universal Render Pipeline tag. |
| Non-UI keyword exclusion | Passed | Passed | UI Lit still excludes `_CASTING_PUNCTUAL_LIGHT_SHADOW`, `_SHADOW_CASTER_ALPHA_TEST_ENABLED`, `EVALUATE_SH_MIXED`, and `EVALUATE_SH_VERTEX`. |

Phase 7.2B compatibility annotations:

- `Nova/UIParticles/UberLit` now implements UI normal map 2D array, UI normal map 3D, UI metallic map 2D array, UI metallic map 3D, UI specular map 2D array, UI specular map 3D, UI smoothness map 2D array, and UI smoothness map 3D runtime sampling on the one-pass Built-in `ForwardBase` approximation.
- Metallic and smoothness channel selection uses `_MetallicMapChannelsX` and `_SmoothnessMapChannelsX` for 2D, 2D array, and 3D maps. Specular map sampling intentionally keeps RGB semantics for all texture modes because the original Lit forward helper samples specular RGB directly.
- Texture selection is coupled to `_BaseMapMode`, matching the original UI Lit material UI and forward helper behavior. No `_NormalMapMode`, `_MetallicMapMode`, `_SpecularMapMode`, `_SmoothnessMapMode`, or new progress/slice properties were introduced.
- Texture array and 3D support remain Approximate at the platform level because backend texture support and sampling precision can differ, but the Unity editor paths covered by smoke tests pass.
- Runtime-pending UI Lit behavior summary: UI receive shadows, UI environment reflections, UI additional lights / `ForwardAdd`, Forward+, cluster light loop, probe volumes, foveated rendering, UI Lit pass topology beyond one-pass forward, UI Lit ShadowCaster, UI Lit DepthOnly, UI Lit DepthNormals, UI Lit SceneSelectionPass, UI Lit Picking, full Canvas `Mask` / `RectMask2D` parity, full UI UberUnlit feature stack parity, full UI Lit PBR parity, full visual parity, and golden image visual regression.
- Distortion runtime, Optimized Shader Generator/Replacer runtime behavior, and final visual regression remain outside Phase 7.2B scope and are not promoted by this phase.
- The next recommended phase is Phase 7.3A: `Nova/UIParticles/UberLit` receive shadows in the Built-in/UI ForwardBase approximation.

## Phase 7.3A UI UberLit Receive Shadow Status

Phase 7.3A expands only the primary `Nova/UIParticles/UberLit` one-pass Built-in `ForwardBase` approximation. UI receive shadows now use Built-in `AutoLight.cginc` shadow coordinates and attenuation for the main directional light. This phase does not add UI environment reflections, UI additional lights, `ForwardAdd`, Forward+, cluster light loop, probe volumes, foveated rendering, UI Lit ShadowCaster / DepthOnly / DepthNormals / SceneSelectionPass / Picking, Distortion runtime, Optimized Shader Generator/Replacer runtime behavior, golden image visual regression, full Canvas `Mask` / `RectMask2D` parity, or URP PBR/shadow parity.

| Verification item | Unity 2022.3.22f1 | Unity 6000.0.74f1 | Notes |
| --- | --- | --- | --- |
| UI receive shadows runtime | Passed | Passed | `_RECEIVE_SHADOWS_ENABLED` and `_LitReceiveShadows > 0.5` apply Built-in `AutoLight.cginc` `ForwardBase` shadow attenuation to direct diffuse/specular lighting in `Nova/UIParticles/UberLit`. |
| `_LitReceiveShadows` property behavior | Passed | Passed | Disabling `_LitReceiveShadows` through the existing material setup path bypasses the direct-light shadow attenuation and visibly brightens the shadowed sample. |
| `_RECEIVE_SHADOWS_ENABLED` keyword behavior | Passed | Passed | Disabling the keyword after setup bypasses the direct-light shadow attenuation and visibly brightens the shadowed sample. |
| Main directional shadow approximation | Passed | Passed | A deterministic Built-in scene with a directional light, opaque shadow caster, and UI Lit receiver mesh verifies the shadowed region is darker than a nearby lit region. |
| Soft and cascade compatibility surface | Passed | Passed | Compile diagnostics cover `_MAIN_LIGHT_SHADOWS`, `_MAIN_LIGHT_SHADOWS_CASCADE`, `_SHADOWS_SOFT`, and combinations with `_RECEIVE_SHADOWS_ENABLED`. Runtime soft/cascade behavior remains controlled by Built-in light and `QualitySettings` rather than URP keywords. |
| Surface feature interaction | Passed | Passed | Receive-shadow smokes cover base map 2D, tint, vertex color, vertex alpha, alpha test, metallic workflow, specular workflow, normal map, and representative 2D array Lit map states. |
| UI stencil / ColorMask / rect clip interaction | Passed | Passed | Controlled mesh-based smokes verify stencil masking, `ColorMask [_ColorMask]`, and local `UNITY_UI_CLIP_RECT` still work with receive shadows. Full Canvas `Mask` / `RectMask2D` parity remains pending. |
| Ambient contribution | Passed | Passed | Ambient remains unshadowed. Shadow attenuation applies only to the direct diffuse/specular term, so shadowed pixels do not collapse to black when ambient is configured. |
| Shader compiler diagnostics | Passed | Passed | Compile coverage includes receive shadows alone, receive shadows plus main-light/cascade/soft shadow keywords, receive shadows plus metallic/specular/normal/smoothness/alpha-test states, `UNITY_UI_CLIP_RECT`, stencil properties, and representative 2D array / 3D UI Lit map states. |
| UI pass topology | Passed | Passed | UI Lit still has no `ForwardAdd`, `ShadowCaster`, `DepthOnly`, `DepthNormals`, `SceneSelectionPass`, or `Picking`; the runtime path remains one `ForwardBase` pass. |
| Non-UI keyword exclusion | Passed | Passed | UI Lit still excludes `_CASTING_PUNCTUAL_LIGHT_SHADOW`, `_SHADOW_CASTER_ALPHA_TEST_ENABLED`, `EVALUATE_SH_MIXED`, and `EVALUATE_SH_VERTEX`. |

Phase 7.3A compatibility annotations:

- UI receive shadows are Approximate. The Built-in port uses Built-in `ForwardBase` shadow variants and `AutoLight.cginc` wrappers; it does not reproduce URP shadow bias, URP screen-space shadows, URP cascade data, URP punctual shadows, or `UniversalFragmentPBR` lighting behavior.
- `_LitReceiveShadows` and `_RECEIVE_SHADOWS_ENABLED` gate only the direct diffuse/specular shadow attenuation. Ambient remains unshadowed in this approximation.
- Soft shadows and cascades are compile-compatible through the Built-in shadow path, but runtime behavior is governed by Built-in `Light` and `QualitySettings` configuration, not URP `_MAIN_LIGHT_SHADOWS_CASCADE` / `_SHADOWS_SOFT` semantics.
- UI stencil, `ColorMask`, and `UNITY_UI_CLIP_RECT` coverage uses the controlled mesh-based UI shader smoke harness. Full Canvas `Mask` / `RectMask2D` parity remains pending because this phase does not add new package dependencies or Canvas-specific tests.
- Runtime-pending UI Lit behavior summary: UI environment reflections, UI additional lights / `ForwardAdd`, Forward+, cluster light loop, probe volumes, foveated rendering, UI Lit pass topology beyond one-pass forward, UI Lit ShadowCaster, UI Lit DepthOnly, UI Lit DepthNormals, UI Lit SceneSelectionPass, UI Lit Picking, full Canvas `Mask` / `RectMask2D` parity, full UI UberUnlit feature stack parity, full UI Lit PBR parity, full visual parity, and golden image visual regression.
- Distortion runtime, Optimized Shader Generator/Replacer runtime behavior, and final visual regression remain outside Phase 7.3A scope and are not promoted by this phase.
- The next recommended phase is Phase 7.3B: define the next UI Lit runtime subset, likely UI environment reflections or UI additional-light behavior, without changing the one-pass topology unless explicitly scoped.

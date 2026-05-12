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
| Lighting scaffolding | `NovaBuiltInLighting.cginc` | Verified | Pending | Provides main-light direction/color access, ambient sampling, and placeholder surface data for future Lit work. Full UberLit shading remains Phase 6 and no original PBR parity is claimed. |
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

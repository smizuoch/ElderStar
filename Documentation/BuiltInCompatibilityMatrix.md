# Built-in Compatibility Matrix

This is the current feature assessment for the Built-in port of NOVA 3.6.0.
`Exact` means the preserved material/editor contract or pipeline-independent math
matches the original on the verified Windows graphics paths. It does not claim
bit-identical output on every GPU. `Approximate` identifies a working Built-in
substitute and its practical limits. `Unsupported` identifies behavior with no
implemented equivalent; the last column gives a fallback. `Not Applicable`
identifies URP infrastructure or functionality absent from the original shaders.

The verification scope is Unity 2022.3.22f1 and 6000.3.23f1, Windows Direct3D11/12.
Shader diagnostics are accompanied by real rendering tests. The repository's
`BuiltInVerificationHistory.md` retains earlier phase reports as historical evidence;
those reports do not override this matrix. No original golden image was changed.

## Package, materials and editor

| Feature | Status | Evidence | Reason / fallback |
| --- | --- | --- | --- |
| Package import | Exact | Phase1 and isolated external/tarball package fixtures | Runtime and editor assemblies use Built-in Unity APIs; no URP, SRP Core or RenderGraph dependency. |
| Shader names | Exact | Phase1 discovery and standalone shader-retention checks | All six original names are preserved, including Hidden/Nova/Particles/ApplyDistortion. |
| Material properties | Exact | All 889 original ShaderLab declarations compared, including attributes, ranges, order and defaults | Original public names and defaults remain, including obsolete compatibility properties. |
| Custom editors | Exact | 525 public/protected declarations, 14 original Inspector/helper source contracts, real OnGUI, multi-edit and Undo/Redo tests | Original class names, labels, CustomEditor declarations, foldout icons and saved preferences remain. Batch GUI tests cover Layout/Repaint and property edits; native mouse input has not been automated. |
| Material setup | Approximate | 735 differential state transitions against original processors, raw/validated snapshots and shader reassignment | Render state, keyword setup and particle streams remain. Uber raw materials retain queue 2000. Distortion defaults to queue 3100 so shader-only background capture occurs after transparents; an explicit material queue still wins. Original URP could keep queue 2000 because its renderer feature drew the effect separately. |
| Material migration | Approximate | Six direct original-GUID imports, six legacy aliases, optional normalizer, unchanged Fire001 assets | Original standard shader references need no conversion, including embedded/binary references. Effect Import omits bundled NOVA source; ordinary all-selected Unity import can conflict. Old Built-in aliases remain usable; custom/optimized shader GUIDs require a port or regeneration. See Migration.md. |
| Samples | Exact | Showcase tests plus generated scene image | Package Manager imports a playable scene with four particle variants and moving distortion; the generator preserves the active saved scene and refuses overwrites. |

## Common particle features

These rows apply to all four Uber shaders unless a pass or renderer is named.
The shared color suite runs its material cases against all four names, including
Lit with explicitly configured diffuse lighting. Texture-mode tests supply the
corresponding 2D, array or 3D resource rather than relying on a 2D-only fixture.

| Feature | Status | Evidence | Reason / fallback |
| --- | --- | --- | --- |
| Render state | Approximate | MaterialSetup, minimum rendering, interaction and imported MIT sample comparisons | Material blend/cull/queue/depth controls match. Equal-depth transparent renderers with tied sort keys can composite differently across pipelines (heal02 sample). Set distinct renderer sortingOrder values when deterministic overlap is required; this is an explicit authoring fallback, not automatic asset conversion. |
| Vertex color | Exact | Shared color and actual ParticleSystem tests | RGB and alpha streams affect all four Uber shaders. |
| Vertex alpha progress | Exact | Shared color, particle Custom1/Custom2 and interaction tests | Vertex alpha can drive transition progress while retaining the original alpha interpretation. |
| Alpha clipping | Exact | Color, DepthOnly, ShadowCaster and selection rendering tests | Cutoff and alpha-modulate behavior are preserved in applicable passes. |
| Base texture modes | Exact | 2D/array/3D color and original-image comparisons | Slice/progress selection is implemented for all four shaders on capable hardware; see Platform scope. |
| UV transforms and custom coordinates | Exact | Coordinate, deformation and ParticleSystem tests; 24 original-order Base UV graphics cases and four Lit alpha-pass texture-transform cases | Base rotations/pivots precede tiling/offset, followed by custom offsets. Array/volume transforms reach Lit depth/shadow passes. Original property mappings and UI-specific stream layout are retained. |
| Random row selection | Exact | Base/tint/transition/emission row tests, interaction suite and eight Lit depth/shadow custom-progress/row cases | Original row count and custom/random coordinate selection remain, including parallax and second transition. Base progress is computed in the vertex stage as in original NOVA. |
| Mirror samplers | Approximate | Original-image comparison and 36 Lit surface-sampler cases | D3D11/12 uses matching point/linear/trilinear mirror samplers. Coupled-sampler backends use mirrored UVs and texture-import filtering; configure matching filters on those platforms. |
| Channel extraction | Exact | Shared base/channel tests and visual capture | Original color channel selection and texture-channel controls remain. |
| Tritone and pentone | Exact | Shared tone tests and original-image comparison | Tone thresholds, colors and custom coordinates retain the original math. |
| Tint color and maps | Exact | Shared 2D/array/3D tint, row and interaction tests | Constant/map tint, blend controls and obsolete 3D aliases are retained. |
| Tint rim | Approximate | Shared rim/tint rendering tests | Uses Built-in world normal/view interpolation. Orthographic view direction matches the original; tune rim sharpness for platform/interpolation differences. |
| Flow targets | Exact | Shared Base/Tint/Emission/AlphaTransition tests | Signed XY offsets, channel selection, rotation and target flags are implemented. Lit surface maps keep their original pre-flow UVs. |
| Parallax targets | Approximate | 36 mode/target cases and orthographic reference captures | Base/Tint/Emission support 2D/array/3D height maps with the original one-step offset. Built-in tangent/view interpolation can differ; supply correct tangent streams and tune strength. |
| Greyscale and gradient | Exact | Shared color-correction tests and visual capture | Original luminance and gradient lookup operations remain. |
| Alpha transition | Exact | Shared fade/dissolve, texture-mode and flow tests | Fade/dissolve thresholds, sharpness, texture modes and progress controls are implemented. |
| Second alpha transition | Exact | Interaction tests for array/3D, random rows, average/multiply | Both transition maps combine using the original blend controls and alpha semantics. |
| Emission | Exact | Shared area/map/color/intensity/edge tests and visual capture | All/alpha/map areas, explicit/base/ramp colors, 2D/array/3D maps and edge transparency remain. As in original Lit, the common stack supplies lit albedo. |
| Rim and luminance transparency | Approximate | Shared transparency and interaction tests | Luminance math is preserved; rim uses Built-in normal/view interpolation. Use the retained progress/sharpness/inverse controls to match the target scene. |
| Soft particles | Approximate | Camera-depth and distortion rendering tests | Requires a Built-in camera depth texture containing opaque casters. Enable DepthTextureMode.Depth; transparent depth writes do not provide the same URP input. |
| Depth fade | Approximate | Camera distance/range and combined-feature tests | Uses explicit eye depth and the original near/far/width controls. Camera projection differs by pipeline; tune the same eye-space distances after migration. |
| Vertex deformation | Exact | Forward/depth/shadow/selection and custom-coordinate tests | Texture-driven position deformation and stream-driven offsets are carried into applicable passes. |
| Billboard particles | Exact | Actual ParticleSystem vertex-color/custom-stream rendering | Configure the original NOVA vertex streams using the Inspector. |
| Mesh particle instancing | Exact | Actual instanced mesh ParticleSystem rendering for all five visible shaders | Unity's Built-in particle instancing buffer provides transforms, colors and custom data; procedural instancing variants are compiled in applicable passes. |
| Instanced stereo | Approximate | Four stereo shadowed ForwardAdd compiler regressions and thirteen GPU texture-array tests: four Uber color passes, per-eye soft-particle depth, particle DepthOnly/ShadowCaster, and distortion eye textures | Built-in stereo macros carry the vertex eye index to fragments and depth slices. Tested with explicit XR constant buffers on D3D11/12 without a headset; actual VRChat PCVR/headset verification remains separate. |
| Fog | Approximate | 24 perspective/orthographic Linear/Exp/Exp2 cases and ForwardAdd diagnostics | Evaluates near-plane-relative view depth per fragment instead of URP's interpolated clip fog. ForwardAdd fades toward black; match scene fog distances when migrating. |

## Lighting and passes

| Feature | Status | Evidence | Reason / fallback |
| --- | --- | --- | --- |
| Metallic and specular workflows | Approximate | Analytic GPU tests and 72 original/port PBR frame comparisons | Original GGX direct BRDF, diffuse energy, F0 and roughness math are preserved without URP includes. Light selection/attenuation and ambient data still come from Built-in; match scene light/probe settings after migration. |
| Normal maps | Approximate | Imported DXT5 packing, normal scale and 2D/array/3D tests | Unity normal packing is decoded before XY scale, as in the original. Built-in tangent/normal interpolation differs; use correct imported normals and particle tangents. |
| Surface maps | Approximate | Metallic/specular/smoothness channel and texture-mode tests, original PBR captures | Sampling, channel selection, shared base filtering and original BRDF response are retained. Final lighting uses Built-in light/probe inputs; match those scene inputs when migrating. |
| Main light | Approximate | Direction/color/intensity, normal-map rendering and automatic initial lighting and Undo tests | Built-in ForwardBase supplies the main directional light. URP light selection differs. Configure a directional key light and initial setup now preserves original intensity values automatically; Match Original Lighting Settings can reapply them; see Migration.md. |
| Additional lights | Approximate | Shared point/spot/range/direction tests for both Lit shaders | ForwardAdd uses Built-in attenuation and shadows, with alpha and UI clipping preserved. Set per-pixel lights/Quality pixel-light count appropriately; it is not a Forward+ light loop. |
| Receive shadows | Approximate | Lit/UI directional receiver, toggles and combined-feature tests | Built-in attenuation affects direct light, leaving ambient/reflections separate. Bias, cascades and filtering follow Built-in Light/Quality settings; retune them after migration. |
| Ambient and light probes | Approximate | Ambient, normal and reflection interaction tests | Built-in SH with a flat-ambient floor replaces URP SH/APV sampling. Use Built-in light probes or ambient lighting; APV data is not consumed. |
| Environment reflections | Approximate | Both Lit shaders: analytic roughness response and original captures with mip-colored cubemaps/view angles | Original perceptual-roughness mip selection, surface reduction and grazing Fresnel are preserved. Probe assignment/capture uses Built-in. Original NOVA also omits probe-blending/box-projection shader variants; use one suitable probe or a custom/default cubemap. |
| Lightmaps and vertex lighting | Not Applicable | Original ParticlesUberLitForward.hlsl input/GI initialization | Original NOVA uses SH (or Unity 6 APV), has no lightmap UV path and sets vertexLighting to zero. The port does not add those absent paths. |
| Scene selection and picking | Approximate | Particle Uber and Distortion editor-pass rendering | Original pass names and selection IDs remain. Alpha/deformation are rendered, but Unity's editor invocation differs by pipeline; use normal Built-in selection tooling. |
| Shadow caster | Approximate | Caster alpha/deformation, receive-shadow and compile tests | Uses Built-in ShadowCaster bias/variants. Enable the original caster controls and renderer shadow casting; retune light bias. UI shaders originally had no caster pass. |
| DepthOnly | Approximate | Explicit depth-pass rendering and optimizer tests | The pass can be drawn explicitly, but Built-in camera depth normally comes from ShadowCaster. Retain that pass and enable camera depth for depth effects. |
| DepthNormals | Unsupported | Pass-topology and optimizer tests | No URP DepthNormals prepass contract is implemented. Use Built-in depth/ShadowCaster or a separate replacement shader if an effect requires encoded depth normals. |
| Forward+ | Unsupported | No URP/cluster dependency checks | Built-in has no URP Forward+ loop. Use ForwardBase/ForwardAdd and manage per-pixel lights. |
| Probe volumes | Unsupported | Original APV audit and no-URP compile checks | URP adaptive probe-volume APIs/data are unavailable. Use Built-in light probes or ambient lighting. |
| Foveated rendering | Unsupported | Original SRP include audit and dependency checks | URP foveated-coordinate transforms are not implemented. Use ordinary mono rendering; validate any platform XR/foveation integration separately. |
| Debug display | Unsupported | Original DEBUG_DISPLAY audit and dependency checks | There is no URP debug-display integration. Inspect shaders through the Built-in Frame Debugger and material controls. |

## UI, distortion and optimization

| Feature | Status | Evidence | Reason / fallback |
| --- | --- | --- | --- |
| UI stencil and ColorMask | Exact | Real Canvas Mask plus mesh stencil/ColorMask tests | Original stencil names and material contract are retained. UGUI is a test-project dependency, not a package dependency. |
| UI rect clipping | Exact | Real Canvas RectMask2D plus mesh clipping tests | UNITY_UI_CLIP_RECT and the original clip rectangle work on both UI shaders. |
| UI lighting | Approximate | UI Lit common, shadow, reflection and additional-light suites | Native CanvasRenderer uses the base ambient/main-light approximation and does not schedule the mesh ForwardAdd loop. Use MeshRenderer-based particles when per-pixel additional lights are required. |
| Camera distortion | Approximate | 19 camera-compositor and 19 shader-only visibility/control cases, plus per-eye texture sampling | The shader automatically uses a named GrabPass when no supported camera compositor is active. VRChat avatars need no custom component. An enabled ScreenSpaceDistortion component retains the ordinary mono camera-wide compositor. |
| Distortion buffer | Approximate | Positive/negative/vertical/off-center offset tests | The camera compositor accumulates signed offsets in RGHalf (ARGBHalf fallback). The shader-only path captures background once per camera using a named GrabPass and distorts per surface; overlapping offsets are not accumulated across objects, and the extra scene copy has a GPU cost. |
| Distortion material controls | Exact | Material setup, shader diagnostics and camera tests | Original Base/Flow/AlphaTransition, normal unpack, mirror/channel/rotation/custom coordinates and scalar/XY intensity controls remain. |
| Distortion depth and visibility | Approximate | Layer/culling/frustum, localized occlusion and depth-effect tests on both paths | The compositor compares opaque camera depth. Shader-only distortion uses hardware ZTest; soft particles additionally need the world's camera depth texture. Avatar shaders cannot require arbitrary VRChat worlds to provide depth. |
| Distortion cleanup | Exact | Disable/destroy behavior and runtime implementation review | Temporary targets, command buffers and materials are released. The camera depth flag intentionally remains for other effects. |
| Distortion camera types | Approximate | Runtime bypass guards, shader-only rendering, per-eye texture tests | XR, reflection and preview cameras use the shader-only approximation rather than the MonoBehaviour compositor. Scene-view compositor application is configurable. Actual headset/mirror combinations need client verification; a nested camera during another camera's compositor can inherit its suppression flag. |
| RenderGraph | Not Applicable | Runtime/editor assembly and source dependency checks | Built-in does not schedule URP RenderGraph passes; the camera effect replaces their visible function. |
| URP compatibility mode | Not Applicable | Original version-specific renderer API audit | Built-in has no URP render-graph compatibility toggle. |
| Optimizer enum and names | Exact | Optimizer tests for all 48 generated names | Original OptionalShaderPass API and 2 shaders x 3 render types x 8 optional-pass naming combinations remain. |
| Optimizer pass stripping | Approximate | All 48 outputs compiled; rendering equivalence and opaque keyword-reduction tests | Original opaque depth/shadow variant reduction is preserved. DepthOnly/ShadowCaster are stripped as requested; the DepthNormals flag preserves naming but adds no URP pass. Keep ShadowCaster for Built-in camera depth. |
| Optimizer includes | Approximate | Package-cache compilation and moved-output-folder test | Includes resolve through the current Built-in package asset path rather than original URP includes. Regenerate after moving/replacing the source package; output must be under Assets. |
| Material replacement | Exact | Scoped replacement, persistence and Undo tests | Original names are resolved after generation; material values, keywords, queue and Undo are preserved. |
| Platform scope | Approximate | Windows graphics-enabled editor/player, image comparisons, stereo GPU tests and VRChat SDK 3.10.5 PC Build/Build & Test | Verified on Direct3D11/12 with SM3.5, arrays/3D textures and half-float targets. VRChat PC build validation is separate from actual server upload and client/headset rendering. Other APIs/mobile/Web require separate validation; Android avatar custom shaders are outside scope. |

## Visual acceptance and evidence

`Tests/Visual` captures 24 controlled configurations for each original URP and
ported Uber shader in isolated Unity 6000.3.23f1 projects using Direct3D12/Gamma.
All 96 common-surface comparisons must satisfy normalized mean RGB error <= 0.01,
maximum channel error <= 0.08 and <= 1% of pixels with mean RGB error > 0.02.
Missing frames, mismatched dimensions and invisible base geometry fail the check.
The 48 Lit frames use a diffuse dielectric with specular/reflections disabled;
they gate common-surface behavior without claiming full URP BRDF equality.
Lighting, shadows and reflection acceptance additionally comes from PlayMode assertions.
Another 72 original/port captures exercise both Lit shaders with metallic/specular
workflows, three smoothness values, three light/view angles, direct GGX and
environment Fresnel/mip response. All use the same image thresholds above.
The PBR suite also passes all 72 comparisons in Linear color space after the
explicit lighting-settings migration aligns both pipelines' light inputs.
Existing golden images are untouched; captures are freshly generated evidence.

Additional 0.1.2 verification imports all 13 original MIT effect prefabs unchanged
and exercises 96 CC0 textures on all four Uber shaders, with fresh/validated material
snapshots in Gamma and Linear. The strict expanded corpus is not a universal parity
pass: `pfb_eff_heal02` has six transparent-order differences per comparison run.
Individual renderer captures match; tied transparency sorting is documented above.
`pfb_eff_fire` also crashes natively in both original and port on Unity 6000.3.23f1,
inside ParticleSystemRenderer::PrepareForRender; it is explicitly excluded only on
that editor. Unity 2022.3.22f1 renders all 13. Use the verified 2022 editor for that
prefab until the native engine fault is resolved. This is distinct from the supplied
N2Studio `Fire001`, which passes its separate import/render/player checks.
No manual material/shader conversion is used. Unity can serialize old material
defaults during normal import/test startup in either pipeline; source input hashes
and post-import differences are recorded separately. All 13 effect prefab files
remain byte-identical. Original URP demo scenes are outside this prefab test scope.

The full test commands and package/player fixtures are described in the repository's
`Documentation/Verification.md`. The release verification report records actual
run results. Historical phase counts are not current release acceptance results.

VRChat PC / AAO 1.9.20: default Trace And Optimize preserves the tested Fire001
particles and all five visible NOVA shader families. Actual SDK bundles with AAO
off/on preserve NOVA material properties/keywords/texture dimensions and particle
vertex streams. Eight added skinned probes exercise mesh merging, vertex color,
UV1/UV2 custom coordinates and vertex deformation. Skinned renderer count falls
23 to 19; three rendered frames differ by at most 1/255 in a channel. No AAO patch
or shader-information registration is required for this tested default path.
Unknown-shader handling is conservative: optional unused-UV/texture optimization
is not guaranteed. Advanced AAO settings, arbitrary third-party plugins/materials,
actual headset behavior and VRChat-world camera inputs need separate validation.
See the repository's `Documentation/VRChatPCVerification.md` for the fixtures,
root-cause isolation and upload/client verification status.

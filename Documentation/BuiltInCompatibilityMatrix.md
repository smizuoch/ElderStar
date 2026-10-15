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
| Render state | Approximate | MaterialSetup, minimum rendering, Lit multiply-albedo-before-lighting regression, interaction and imported MIT sample comparisons | Material blend/cull/queue/depth controls match. Equal-depth transparent renderers with tied sort keys can composite differently across pipelines (heal02 sample). Set distinct renderer sortingOrder values when deterministic overlap is required; this is an explicit authoring fallback, not automatic asset conversion. |
| Vertex color | Exact | Shared color and actual ParticleSystem tests | RGB and alpha streams affect all four Uber shaders. |
| Vertex alpha progress | Exact | Shared color, particle Custom1/Custom2 and interaction tests | Vertex alpha can drive transition progress while retaining the original alpha interpretation. |
| Alpha clipping | Exact | Color, DepthOnly, ShadowCaster and selection rendering tests | Cutoff and alpha-modulate behavior are preserved in applicable passes. |
| Base texture modes | Exact | 2D/array/3D color and original-image comparisons | Slice/progress selection is implemented for all four shaders on capable hardware; see Platform scope. |
| UV transforms and custom coordinates | Exact | Coordinate, deformation and ParticleSystem tests; 24 original-order Base UV graphics cases and four Lit alpha-pass texture-transform cases | Base rotations/pivots precede tiling/offset, followed by custom offsets. Array/volume transforms reach Lit depth/shadow passes. Original property mappings and UI-specific stream layout are retained. |
| Random row selection | Exact | Base/tint/transition/emission row tests, interaction suite, eight Lit alpha-pass cases and out-of-range Custom Data GPU probes | Original row count and custom/random coordinate selection remain, including parallax and second transition. Base progress is computed in the vertex stage as in original NOVA. Combined progress is normalized by the selected row before clamping, including values outside 0..1. |
| Mirror samplers | Exact | Original-image comparison, 36 Lit surface-sampler cases and the 2026-10-15 source reassessment | Native point/linear/trilinear mirror sampler states match on the matrix's Windows D3D11/12 scope. Coupled-sampler backends outside this scope retain mirrored UVs and texture-import filtering; they are not certified exact. See ExactParityProgress.md. |
| Channel extraction | Exact | Shared base/channel tests and visual capture | Original color channel selection and texture-channel controls remain. |
| Tritone and pentone | Exact | Shared tone tests and original-image comparison | Tone thresholds, colors and custom coordinates retain the original math. |
| Tint color and maps | Exact | Shared 2D/array/3D tint, row and interaction tests | Constant/map tint, blend controls and obsolete 3D aliases are retained. |
| Tint rim | Exact | 96 retained original/port rim images across both editors/color spaces, plus 128 seeded GPU curve probes per graphics path | Original normalized world normal/view interpolation, abs(dot), progress/sharpness/inverse and tint math match on verified D3D paths. Degenerate normal/view vectors remain outside a portable normalization contract; see ExactParityProgress.md. |
| Flow targets | Exact | Shared Base/Tint/Emission/AlphaTransition tests | Signed XY offsets, channel selection, rotation and target flags are implemented. Lit surface maps keep their original pre-flow UVs. |
| Parallax targets | Approximate | 36 mode/target cases, signed back-facing GPU probes and randomized reference captures | Base/Tint/Emission support 2D/array/3D height maps with the original one-step offset. The signed offset denominator matches original NOVA except for a +/-1e-4 singularity guard. The tangent-space view is now formed per vertex and interpolated, including original odd-scale handling. Avoid only the guarded near-zero denominator; see LowCostLightingParity.md. |
| Greyscale and gradient | Exact | Shared color-correction tests and visual capture | Original luminance and gradient lookup operations remain. |
| Alpha transition | Exact | Shared fade/dissolve, texture-mode and flow tests | Fade/dissolve thresholds, sharpness, texture modes and progress controls are implemented. |
| Second alpha transition | Exact | Interaction tests for array/3D, random rows, average/multiply | Both transition maps combine using the original blend controls and alpha semantics. |
| Emission | Exact | Shared area/map/color/intensity/edge tests and visual capture | All/alpha/map areas, explicit/base/ramp colors, 2D/array/3D maps and edge transparency remain. As in original Lit, the common stack supplies lit albedo. |
| Rim and luminance transparency | Exact | Shared transparency/interaction rendering plus 128 seeded GPU comparisons with original GetRimValue | Original luminance weights, rim normal/view interpolation, inverse/progress/sharpness and alpha multiplication are preserved. Source lighting and transparent ordering remain assessed in their own rows. See ExactParityProgress.md. |
| Soft particles | Approximate | Camera-depth, stereo and URP-version sampler GPU tests; randomized visible-depth witness | Unity 6 matches URP 17 PointClamp; Unity 2022 retains URP 14 texture sampling. Requires a Built-in camera depth texture containing opaque casters. Enable DepthTextureMode.Depth; transparent depth writes do not provide the same URP input. |
| Depth fade | Exact | 35 original-formula GPU image cases covering all five visible shaders, both editors and D3D11/12; legacy generated-helper regression | Matches original projected-depth linearization and near/far/signed-width math, including orthographic/oblique behavior and widths below 1e-4. No extra varying, pass or sample. Zero-width singularities retain GPU arithmetic. Old generated source retains its vector helper and needs regeneration for the new behavior; see ExactParityProgress.md. |
| Vertex deformation | Exact | Forward/depth/shadow/selection and custom-coordinate tests | Texture-driven position deformation and stream-driven offsets are carried into applicable passes. |
| Billboard particles | Exact | Actual ParticleSystem vertex-color/custom-stream rendering | Configure the original NOVA vertex streams using the Inspector. |
| Mesh particle instancing | Exact | Actual instanced mesh ParticleSystem rendering for all five visible shaders | Unity's Built-in particle instancing buffer provides transforms, colors and custom data; procedural instancing variants are compiled in applicable passes. |
| Instanced stereo | Approximate | Four stereo shadowed ForwardAdd compiler regressions and fifteen GPU texture-array tests: four Uber color passes, per-eye soft-particle depth, particle DepthOnly/DepthNormals/ShadowCaster, and distortion eye textures | Built-in stereo macros carry the vertex eye index to fragments and depth slices. Tested with explicit XR constant buffers on D3D11/12 without a headset; actual VRChat PCVR/headset verification remains separate. |
| Fog | Exact | 288 retained original/port fog images across both editors/color spaces, plus ForwardAdd tests | Original clip-Z vertex coefficient/interpolation and Linear/Exp/Exp2 mixing match; ForwardAdd fades to black. Matching camera/fog inputs are required. Original orthographic/oblique behavior is intentionally retained. See ExactParityProgress.md. |

## Lighting and passes

| Feature | Status | Evidence | Reason / fallback |
| --- | --- | --- | --- |
| Metallic and specular workflows | Approximate | Analytic GPU tests and 72 original/port PBR frame comparisons | Original GGX direct BRDF, diffuse energy, F0 and roughness math are preserved without URP includes. Light selection/attenuation and ambient data still come from Built-in; match scene light/probe settings after migration. |
| Normal maps | Exact | 48 retained original/port Lit normal images across both editors/color spaces, DXT5 packing/scale and 2D/array/3D tests | Original unpacking and XY scale, vertex tangent/binormal generation, interpolation and final normalization order match. Normal-map handedness remains distinct from parallax as in original NOVA. Supply valid tangents/normals; lighting inputs are assessed separately. See ExactParityProgress.md. |
| Surface maps | Approximate | Metallic/specular/smoothness channel and texture-mode tests, original PBR captures | Sampling, channel selection, shared base filtering and original BRDF response are retained. Final lighting uses Built-in light/probe inputs; match those scene inputs when migrating. |
| Main light | Approximate | Direction/color/intensity, normal-map rendering and automatic initial lighting and Undo tests | Built-in ForwardBase supplies the main directional light. URP light selection differs. Configure a directional key light and initial setup now preserves original intensity values automatically; Match Original Lighting Settings can reapply them; see Migration.md. |
| Additional lights | Approximate | Shared point/spot/range/direction tests for both Lit shaders | ForwardAdd uses Built-in attenuation and shadows, with alpha and UI clipping preserved. Set per-pixel lights/Quality pixel-light count appropriately; it is not a Forward+ light loop. |
| Receive shadows | Approximate | Lit/UI directional receiver, toggles and combined-feature tests | Built-in attenuation affects direct light, leaving ambient/reflections separate. Bias, cascades and filtering follow Built-in Light/Quality settings; retune them after migration. |
| Ambient and light probes | Approximate | 1,008 nonblack-ambient/original image comparisons plus 12 focused GPU tests and eight spatial LPPV cases | SH/proxy irradiance no longer has a scene-color floor. Reflection-map-off glossy DC, scene reflection intensity, sRGB low-light and HDR conversion match the tested original inputs; the previous 22/192 failures are resolved. The scriptless fallback recovers renderer SH DC, while URP supplies global ambient-probe DC separately. Local/custom probes can therefore differ from the global fallback; match probe inputs or enable reflection maps. See AmbientLighting.md and the repository AmbientLightingParity.md report. |
| Environment reflections | Approximate | Both Lit shaders: analytic roughness response and original captures with mip-colored cubemaps/view angles | Original perceptual-roughness mip selection, surface reduction and grazing Fresnel are preserved. Disabling reflection maps retains glossy ambient as in original NOVA (0.1.6). Probe assignment/capture uses Built-in. Original NOVA also omits probe-blending/box-projection shader variants; use one suitable probe or a custom/default cubemap. |
| Lightmaps and vertex lighting | Not Applicable | Original ParticlesUberLitForward.hlsl input/GI initialization | Original NOVA uses SH (or Unity 6 APV), has no lightmap UV path and sets vertexLighting to zero. The port does not add those absent paths. |
| Scene selection and picking | Approximate | Particle Uber and Distortion editor-pass rendering | Original pass names and selection IDs remain. Alpha/deformation are rendered, but Unity's editor invocation differs by pipeline; use normal Built-in selection tooling. |
| Shadow caster | Approximate | Caster alpha/deformation, receive-shadow and compile tests | Uses Built-in ShadowCaster bias/variants. Enable the original caster controls and renderer shadow casting; retune light bias. UI shaders originally had no caster pass. |
| DepthOnly | Approximate | Explicit depth-pass rendering and optimizer tests | The pass can be drawn explicitly, but Built-in camera depth normally comes from ShadowCaster. Retain that pass and enable camera depth for depth effects. |
| DepthNormals | Approximate | 128 original/port image pairs, seven camera/pass tests, two stereo array cases and optimizer outputs | Particle shaders provide encoded world normals plus coverage through an explicit pass and optional NovaDepthNormals camera buffer. It does not replace world depth normals or implement the URP prepass invocation/format. Use the dedicated Texture API; see DepthNormals.md. |
| Forward+ | Approximate | Existing multi-light/shadow ForwardAdd suites and Canvas selection/rendering tests | Mesh/particle renderers retain ForwardBase/ForwardAdd; optional Canvas lighting evaluates up to eight selected lights in one pass. There is no tiled/clustered renderer. Control pixel lights and the Canvas limit; Canvas list shadows/cookies are not sampled. |
| Probe volumes | Approximate | Eight spatial LPPV GPU tests on both Lit shaders, local/world coordinates and fallback | Native Built-in LPPV replaces ordinary SH L0/L1 while retaining renderer L2, without double addition. APV file formats, streaming, adaptive placement and scenario changes are not supported. Use world-supplied Built-in probes/LPPV; avatars cannot require a world to provide them. |
| Foveated rendering | Approximate | URP 17.3 official include audit and D3D mono/stereo depth/screen-UV regressions | URP non-uniform-raster coordinate remapping is disabled on the verified Windows D3D path, so its transforms are identity there. Existing per-eye UV/depth paths remain. This does not implement non-uniform remapping for Metal/PS5 or control XR runtime VRS; physical runtime/headset verification remains separate. |
| Debug display | Approximate | 32 GPU view assertions and two actual diagnostic-variant AssetBundle stripping tests | Editor views cover albedo, world normals/tangents, eye depth, fog, direct/indirect light and additional count. Scoped global state restores after rendering; Player/bundle builds strip dedicated variants. This is not the URP debug API. Use Tools/NOVA/Rendering Diagnostics. |

## UI, distortion and optimization

| Feature | Status | Evidence | Reason / fallback |
| --- | --- | --- | --- |
| UI stencil and ColorMask | Exact | Real Canvas Mask plus mesh stencil/ColorMask tests | Original stencil names and material contract are retained. UGUI is a test-project dependency, not a package dependency. |
| UI rect clipping | Exact | Real Canvas RectMask2D plus mesh clipping tests | UNITY_UI_CLIP_RECT and the original clip rectangle work on both UI shaders. |
| UI lighting | Approximate | 49 optional Canvas lighting/mask tests, plus ordinary mesh/particle UI Lit suites | NovaCanvasLighting retains native Canvas and evaluates its selected direct lights in one base pass. Owned drawing materials disable ForwardAdd to prevent duplicate light contribution. Requires optional uGUI helper; no scene search each frame, source material mutation, list shadows or cookies. VRChat uses allowed mesh/particle renderers with no helper. Overlay material/lifecycle is tested; offscreen Overlay pixels are not. |
| Camera distortion | Approximate | 19 camera-compositor and 19 shader-only visibility/control cases, per-eye texture sampling, and 14 original-name API graphics/lifetime cases | The shader automatically uses a named GrabPass when no supported camera compositor is active. VRChat avatars need no custom component. The mono camera compositor and restored ApplyDistortionPass/DistortedUvBufferPass execute real Built-in commands. Five original constructor/setup/reset signatures are callable; URP inheritance and engine-specific parameters still require adaptation. See RuntimeAPICompatibility.md and BuiltInRenderingAPI.md. |
| Distortion buffer | Approximate | Positive/negative/vertical/off-center tests; 256 randomized original/port cases across two editors, two color spaces and two anisotropy settings (all pass) | The camera compositor accumulates signed offsets in RGHalf (ARGBHalf fallback). Forced anisotropy uses an explicit 8x sampler, the nearest supported inline value to original forced 9x. Other quality settings retain ordinary sampling. Raster/depth rounding and 8x/9x filtering can still differ; Disable is a tested explicit quality option when forced filtering is unwanted. No project quality setting or material is rewritten. The shader-only path captures background once per camera using a named GrabPass and distorts per surface; overlapping nonzero offsets are not accumulated across objects, and the extra scene copy has a GPU cost. Zero-offset surfaces discard, preserving distortion underneath without another screen copy (ordinary and reflected-camera regressions). Uniform-zero strength with unused Custom Data coordinates rejects geometry before rasterization; active-surface overhead and invisible-layer savings were measured on D3D11. |
| Distortion material controls | Exact | Material setup, shader diagnostics and camera tests | Original Base/Flow/AlphaTransition, normal unpack, mirror/channel/rotation/custom coordinates and scalar/XY intensity controls remain. |
| Distortion depth and visibility | Approximate | Layer/culling/frustum, localized occlusion and depth-effect tests on both paths | The compositor compares opaque camera depth. Shader-only distortion uses hardware ZTest; soft particles additionally need the world's camera depth texture. Avatar shaders cannot require arbitrary VRChat worlds to provide depth. |
| Distortion cleanup | Exact | Disable/destroy behavior and runtime implementation review | Temporary targets, command buffers and materials are released. The camera depth flag intentionally remains for other effects. |
| Distortion camera types | Approximate | Mono image-effect, SceneView option, XR/preview/reflection guards and seven actual nested-camera/disable rendering regressions on both editors and D3D11/12 | Unsupported compositor cameras use shader-only distortion. Camera-wide scoped state now prevents unconfigured nested Game/Reflection/Preview cameras from inheriting another compositor's suppression flag, and restores the parent/external state after nesting or disable. Actual XR/runtime invocation remains separate; see ExactParityProgress.md. |
| RenderGraph | Not Applicable | Runtime/editor assembly and source dependency checks | Built-in does not schedule URP RenderGraph passes; the camera effect replaces their visible function. |
| URP compatibility mode | Not Applicable | Original version-specific renderer API audit | Built-in has no URP render-graph compatibility toggle. |
| Optimizer enum and names | Exact | Optimizer tests for all 48 generated names | Original OptionalShaderPass API and 2 shaders x 3 render types x 8 optional-pass naming combinations remain. |
| Optimizer pass stripping | Approximate | All 48 outputs compiled, DepthNormals flags and opaque normal-array rendering regression | DepthOnly, ShadowCaster and the real DepthNormals pass are retained/removed as requested. DepthNormals preserves UV/normal texture variants needed even for opaque surfaces. Built-in invocation/encoded output differ from URP; keep ShadowCaster for ordinary camera depth. |
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

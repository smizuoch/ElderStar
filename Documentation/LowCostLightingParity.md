# Low-cost lighting parity — implementation record

Follow-up: [0.1.6 ambient parity](AmbientLightingParity.md) resolves the 22 retained
nonblack-ambient failures and records fresh tests and performance measurements.

The approved 2026-10-14 worklist is in BuiltInPortingSpec.md. This report records
scoped Built-in alternatives and their measured limits, not complete URP/API compatibility.

## Interpolation change

- Original `fragUnlit(in out Varyings)` normalizes its interpolated normal before
  `fragLit` uses it. The tangent and binormal remain unnormalized. Normalize only
  the final mapped normal after combining that basis.
- Normal-map binormal uses tangent.w; parallax additionally uses odd negative scale.
  Preserve both contracts. Parallax view direction is calculated per vertex, as in
  the original, retaining the existing signed 1e-4 denominator guard.
- Fog uses the original clip-Z vertex coefficient, including the EXP coefficient,
  then interpolates. The original can produce no fog on the tested orthographic,
  near-clip and oblique projections. ForwardAdd still fades toward black.
- Pack map UV/progress carriers to retain the basis/view direction within the
  SM3.5 stereo/shadowed ForwardAdd budget. UI clip coordinates remain explicit;
  they are not reconstructed with a potentially different particle instance matrix.
- No added ordinary color pass, texture sample or screen copy. The GPU gate below
  measures the actual Player, including clipped UI Lit with eight lights.

### Reproduction and results

`Tests/Visual/NovaInterpolationCapture.cs` runs against original NOVA 3.6.0 and the
port. Define `NOVA_VISUAL_REFERENCE` only in the URP project. Run the graphics
PlayMode test `CaptureInterpolation`, passing `-novaVisualOutput <directory>` and
`-novaBlackAmbient`. Keep D3D graphics enabled. Compare with:

```text
python Tests/Visual/compare_interpolation.py <original> <port> --output <report.json>
```

192 images = four Uber shaders × eight feature groups × six projections/scales.
The mesh is coarse, nonplanar, has varying normals/tangents and nonuniform scale.
Features include control, rim, parallax, normal maps, Linear/Exp/Exp2 fog and their
combination. Views include perspective, orthographic, reflected, negative scale,
near-plane crossing and an oblique near plane.

Unity 2022.3.22f1 / D3D11 / Gamma: baseline c38b1f1 fails **56/192** comparisons;
candidate passes **192/192**, using unchanged thresholds and no golden update.
The isolated interpolation commit passes **450 EditMode + 866 PlayMode** tests.
The shader budget test also enables UI clipping, map arrays, parallax, normal map,
stereo, screen shadows and a directional cookie in ForwardAdd.

Raw metrics: `Verification/LightingParity20261014/interpolation2022.json`.
The new fixture originally mismatched Gamma light-intensity settings; both sides
now use URP's `lightsUseLinearIntensity = (activeColorSpace == Linear)` behavior.
The recorded baseline/candidate numbers above were recaptured with that correction.
Black ambient isolates this interpolation change. Nonblack flat ambient has a
separate 22/192 mismatch: URP supplies a global glossy-ambient fallback even when
material environment reflections are disabled. Built-in ambient/probe inputs are
different. Trials using unity_IndirectSpecColor and reconstructed per-renderer SH
did not reduce the failures, so those replacements were not adopted. No independent rim mismatch was
observed in this sample. These captures are not an all-input guarantee.

## Final regression scope

Direct/interpolation comparisons pass **768/768**: 192 each on Unity 2022.3.22f1
and 6000.3.23f1, Gamma and Linear, D3D11. Dedicated DepthNormals comparisons pass
**128/128**, with the format conversion explicitly performed by the comparison
fixture. No golden image or acceptance threshold was changed. An additional
**832/832** seeded CC0 material comparisons pass in Linear / forced anisotropy,
with all input-state JSON equal and 722 image pairs pixel-identical. These reuse
the pinned original `Reference-linear-green` captures; older five-line metadata
captures were rejected instead of silently accepting missing anisotropy settings.

Full **492/492 EditMode and 933/933 graphics PlayMode** results per editor,
package import tests and failures
reproduced before fixes are retained in `Verification/LightingParity20261014/tests.json`.
The final tarball is also installed in an isolated project without uGUI or URP;
its core/editor assemblies and 48 optimizer outputs compile. Optional UI assemblies
are excluded when uGUI is absent. Python contract, documentation, image-gate,
shell-runner and performance tests are separate; shell-runner tests run in WSL.

An actual SDK build exposed the clipped-UI + stereo + Point/Spot-shadow vertex
limit missed by the earlier directional-cookie test. Six additional compiler
cases reproduced the issue. For clipped UI ForwardAdd only, the light coordinate
is reconstructed from the existing interpolated world position; shadow coordinates
and original attenuation/cookie formulas remain. The LPPV control flow also
uses an explicitly initialized merged result: two regression tests reproduce and
remove the SDK compiler's potentially-uninitialized-value warning. This removes a varying without
adding samples or passes. Four GPU clipping assertions preserve inside lighting
and reject outside addition; an alpha-tested material is required because original
opaque blending ignores alpha-only clipping.

## Optional implementation and boundaries

- [Canvas lighting](CanvasLighting.md): native Canvas, bounded cached light list,
  source-material ownership, 0/1/4/8/over-limit cases, masks and cleanup. The base
  pass owns the complete direct list and disables its drawing material's ForwardAdd.
  Overlay material/lifecycle is tested; offscreen Overlay pixels are not.
- [DepthNormals](DepthNormals.md): real particle passes plus an optional separate
  camera texture, clipping/deformation/custom data, occlusion, stereo slices and
  cleanup. It does not replace the world's depth normals or URP prepass contract.
- [LPPV and diagnostics](ProbeVolumesAndDiagnostics.md): spatial native SH and
  eight Editor views. No APV formats, streaming or tiled Forward+ implementation.
- [Distortion API](BuiltInRenderingAPI.md): executable caller-owned command/texture
  API; original ScreenSpaceDistortion.Create() remains. No fake URP type facade.
- URP 17.3 `FoveatedRenderingKeywords.hlsl` enables non-uniform coordinate remapping
  for specific Metal/PS5 paths, not Windows D3D. The verified D3D transforms are
  identity. Explicit per-eye depth/screen-UV tests do not prove a physical headset
  or runtime VRS configuration. Avatar shaders do not control runtime density.

## Measured cost

RTX 5070 Ti, Windows D3D11, Unity 2022.3.22f1 Development Player, 1920x1080,
offscreen timestamp/disjoint queries. No visible measurement window was opened.
Other validation Editors were closed during timing; the user's VRChat/SteamVR
were left running. Three ABBA repetitions, 64 warmup frames and 128 CPU/GPU samples
per block; 23 scenarios / 276 blocks. CPU is command submission and Camera.Render
wall time, not whole-frame CPU; memory is active native resources, not total VRAM.

All **18 ordinary scenarios** pass the unchanged `max(5%, 0.02 ms)` gate with
unchanged draw count. Maximum paired median increase: **0.009680 ms**. Clipped UI
with eight lights takes the same eight draws (128 with eight overlaps and two
cameras), and passes the relative gate too. These are bounded scene measurements,
not a claim of universal VRC frame cost.

| Optional workload | GPU off → on (ms) | CPU off → on (ms) | Draws off → on |
| --- | ---: | ---: | ---: |
| Canvas 0 lights | 0.018464 → 0.015712 | 0.09015 → 0.09775 | 1 → 1 |
| Canvas 4 lights | 0.046784 → 0.032640 | 0.09700 → 0.10690 | 4 → 1 |
| Canvas 8 lights | 0.085536 → 0.048416 | 0.12920 → 0.13295 | 8 → 1 |
| DepthNormals, 8 renderers | 0.258592 → 0.357824 | 0.24000 → 0.32750 | 1 → 9 |
| LPPV | 0.017760 → 0.021024 | 0.03350 → 0.04275 | 1 → 1 |

Canvas adds 26,734 native material bytes in this one-Graphic fixture, with no new
render target. DepthNormals adds 33,178,072 measured native RT bytes at this size.
LPPV's supplied 32×2×2 test texture/buffer is separately recorded in raw blocks.
Optional costs are not folded into the ordinary-path gate. Canvas off/on uses
different light attenuation/scheduling; its speedup is not a pixel-identity claim.
Raw timings, counters and source hashes are under `Verification/LightingParity20261014`.

## SDK / AAO and remaining acceptance

Package 0.1.5 is installed in test-VRCLens with SDK 3.10.5 / AAO 1.9.20. Both
AAO-off/on SDK bundles render the mesh/particle avatar path, with zero recorded
SDK/shader errors and zero shader compiler warnings. AAO reduces 23 skinned meshes to 19 and retains 17 NOVA materials
and five active Fire001 particle renderers. Source/bundle material states and
particle streams match. Thirty source/bundle/overlap/mirror image pairs pass;
the largest AAO-off/on channel difference is 1/255. No AAO code change is required.

This release's verification is SDK bundle rendering in Unity. Earlier 0.1.3
Private upload/client evidence remains historical; this release is not newly
uploaded or verified with a physical headset. The world must supply required
camera-depth/probe inputs. Forced distortion anisotropy, transparent sort ties,
nonblack ambient inputs and other graphics backends retain documented differences.

## Changed files and delivery

- Shader math/passes: `NovaBuiltInParticleFeatures.cginc`, `NovaBuiltInLighting.cginc`,
  the two Lit includes and four Uber shader files, plus particle legacy aliases.
- Optional helpers: `Runtime/UI/NovaCanvasLighting.cs`, `NovaCanvasLightMaterial.cs`,
  `Runtime/Core/Scripts/NovaDepthNormals.cs`, `BuiltInDistortionPass.cs` and the
  retained `ScreenSpaceDistortion.cs` integration.
- Editor: `NovaCanvasLightingEditor.cs`, `NovaShaderDiagnostics.cs` and
  `OptimizedShaderGenerator.cs`; UI assemblies remain optional.
- Verification: targeted EditMode/PlayMode tests, original captures and strict
  comparison scripts, native offscreen benchmark fixture/analyzer, current
  compatibility/API inventory and this report. Source changes use Japanese
  feature commits on `port/builtin-lighting-parity`.

The 0.1.5 archive contains 244 files, all matching package source byte-for-byte.
Its SHA-256 is `f17854ab3e72ea30f392c68be4c54b226ea7f1a122d0110eb1d3c080b8bf7a93`.
The local delivery is `Builds/Release/jp.co.cyberagent.nova.builtin-0.1.5.tgz`;
build outputs and proprietary avatar/effect assets are not committed. The same
archive remains installed in the authorized test-VRCLens project.

Next validation TODO, outside this completed scoped implementation: a newly
uploaded 0.1.5 avatar in actual VRChat world/headset conditions, additional GPUs
and non-Windows APIs. Remaining approximations are listed above and in the matrix;
this release is not described as complete URP feature/API equivalence.

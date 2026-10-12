# VRChat PC and Avatar Optimizer verification

Date: 2026-10-12. Unity 2022.3.22f1, Windows D3D11, VRChat SDK 3.10.5,
Avatar Optimizer (AAO) 1.9.20. Test project: the user-authorized test-VRCLens.
The package has no VRChat, AAO, NDMF or URP runtime dependency.

## Tested scope and result

Both SDK bundles build with zero captured errors and zero shader diagnostics.
The bundle itself is loaded and rendered, not just the authoring scene.
Default AAO Trace And Optimize passes the comparison. No AAO source was changed.

| Check | AAO off | AAO on |
|---|---:|---:|
| Source skinned renderers | 23 | 23 |
| Built skinned renderers | 23 | 19 |
| Active Fire001 particle renderers | 5 | 5 |
| NOVA material instances | 12 | 12 |
| SDK bundle bytes | 26,746,150 | 22,583,453 |
| Captured errors / shader errors | 0 / 0 | 0 / 0 |
| Error-magenta pixels | 0 | 0 |

The avatar combines a copy of the user's Shinano avatar, unchanged imported Fire001,
five visible shader families, and eight extra skinned quads (two per Uber shader).
The quads carry vertex colors, four-component UV1/UV2 custom data, a checker texture,
custom UV offsets and texture-driven vertex deformation. AAO demonstrably merges
these into four renderers. All active NOVA material properties, keywords, queues,
texture dimensions and texture transforms match their source snapshots; active
particle vertex-stream layouts are unchanged. Texture sampling is checked visually.

At simulation times 0.4, 1.2 and 2.1 seconds, AAO-on/off 768-square bundle renders
have mean normalized RGB error 2.6595e-8, maximum channel error 1/255, and zero
pixels exceeding mean error 0.02. Each passes the existing thresholds (mean <=0.01,
max <=0.08, bad-pixel fraction <=0.01). Existing golden images are untouched.
Screenshots and avatar bundles remain in the user's local Exports/NovaVRCValidation;
proprietary avatar content and account identifiers are not committed.
Machine-readable evidence: [AAO metrics](Verification/VRChatAAO20261012.json).

This verifies the default AAO path, not every advanced setting or every possible
material/plugin combination. AAO handles unregistered shaders conservatively;
optional unused-UV/texture removal may be unavailable. No shader-information
registration was added merely to increase optimization aggressiveness.

## Failures isolated and corrected

1. A previous experimental validation helper called ShaderUtil.ClearCachedData
   immediately before building. Unity 2022 serialized zero GPU programs despite a
   successful SDK build, making the loaded bundle's NOVA shaders unsupported.
   A direct AssetBundle test with no SDK or AAO reproduces: reimport=supported,
   clear-only=unsupported, reimport=supported. The helper was never shipped;
   it has been removed. The fixture uses ForceUpdate reimport when requested.
   [Isolated evidence](Verification/VRChatShaderCache20261012.json).
2. UI UberLit retained eight global URP compatibility multi_compile flags in each
   forward pass. These multiplied unused vertex combinations; a representative
   SDK ForwardBase vertex pass fell from 196,608 to 768 variants after matching
   particle UberLit's local fragment declarations. All keyword names remain.
3. Real SDK stereo + shadowed additional-light variants exceeded vs_4_0's 16
   vertex outputs. Four added tests reproduced the compiler failure. Packing
   the normal into unused UV .w components restores the original SM3.5 target;
   no material property, vertex stream or lighting feature is removed.
4. Validation-only repeat captures changed a running particle system's seed,
   and SDK scene saves accumulated temporary probes. The fixture now stops and
   clears particles first, removes previous owned probes, and restores the owned
   scene after comparison. Neither was an AAO defect.

## Regression results

Both Unity 2022.3.22f1 and 6000.3.23f1 pass 447 EditMode and 839 graphics PlayMode
cases after the shader fixes. The four new shadowed stereo compiler cases fail
before packing and pass afterward. Existing GPU tests include thirteen stereo
texture-array/depth/shadow/distortion cases and 38 compositor/scriptless distortion
cases. Original declaration/editor parity passes all four Python contract tests.
Logs: Logs/Completion/<version>/vrc-packed-edit.xml and vrc-packed-play.xml.

The final 0.1.3 tarball was installed through Package Manager in both isolated
editors (16/16 installed-package tests each) and in test-VRCLens. Both AAO SDK
builds and the three image comparisons were repeated successfully from that tarball.
All 216 archive files match package source byte-for-byte. SHA-256:
`e6eff62bc0a04cf2d18129a5dce5c66e27f9227807b7035ae76962c51899eaf3`.
Local archive: Builds/Release/jp.co.cyberagent.nova.builtin-0.1.3.tgz.

## Reproduction

Copy Tests/VRChat/Editor/NovaVrcAcceptance.cs into the dedicated Editor folder in
an SDK 3.10.5 avatar project; this fixture is not distributed inside Assets/Nova.
Prepare creates the owned scene and imports Fire001 through the package importer.
Validate configures it, backs up texture metadata, enables required streaming
mip maps and invokes the public SDK build API. Existing source avatar prefabs are
not edited. The user's validation run required 31 importer metadata changes;
originals are under Exports/NovaVRCValidation/Backup-20261012/TextureMeta.

Run the 2022 editor with -executeMethod NovaVrcAcceptance.CompareAAO, then repeat
with -novaAAO. Optional -novaRefresh reimports NOVA shader sources. Do not use
-quit; the async fixture writes aao-off/on.json and exits after completion.
Use graphics; -nographics cannot validate rendering. This user project needs a
normal Editor process because its pre-existing editor toolbox throws in batchmode.
The generic standalone tests use graphics-enabled batchmode successfully.

Compare the three aao-off/on-bundle*.png pairs with measure/within_thresholds from
Tests/Visual/compare_frames.py. Require successful reports, zero errors, equal
source populations, preserved states/streams and a smaller AAO output population.
NovaShaderCacheRepro.Run separately demonstrates the cache issue in a throwaway
project with the Unity AssetBundle module and without SDK/AAO. Never clear shader
cache in the acceptance build path.

## Upload and client status

Actual new Private upload is explicitly authorized by the user. Final release
installation, server acceptance and real VRChat-client rendering are separate
remaining gates at the time of this report section; a successful local SDK build
alone is not a completed upload or a client-rendering result. The earlier SDK Test
Avatar was not visible to the user. A physical PCVR/headset/mirror test has not run.
This section will be updated only with observed results.

## Official references and limits

- [VRChat public SDK API](https://creators.vrchat.com/sdk/public-sdk-api/): Build,
  BuildAndTest and BuildAndUpload are separate operations.
- [Allowed avatar components](https://creators.vrchat.com/avatars/whitelisted-avatar-components/whitelisted-avatar-components/):
  avatar distortion uses a shader GrabPass; it requires no NOVA MonoBehaviour.
- [Avatar size limits](https://creators.vrchat.com/avatars/avatar-size-limits/):
  PC upload limits are enforced by the publishing path; local tests do not prove them.
- [Shader fallback](https://creators.vrchat.com/avatars/shader-fallback-system/):
  safety settings can replace custom shaders; distortion's fallback hides its mesh.
- [AAO Trace And Optimize](https://vpm.anatawa12.com/avatar-optimizer/en/docs/reference/trace-and-optimize/):
  this verification uses defaults, with no advanced zero-size-polygon option.
- [AAO shader information](https://vpm.anatawa12.com/avatar-optimizer/en/docs/developers/shader-information/)
  and [system assumptions](https://vpm.anatawa12.com/avatar-optimizer/en/docs/developers/system-assumptions/):
  unknown shaders receive conservative handling; vertex order may change. NOVA
  does not use SV_VertexID to index per-vertex data.
- [Unity stereo instancing](https://docs.unity3d.com/2022.3/Documentation/Manual/SinglePassInstancing.html):
  stereo indices and screen/depth textures require eye-aware shader paths.

World-provided camera depth still governs soft particles. GrabPass distortion can
cost extra copies and differs from accumulated URP screen-space distortion where
surfaces overlap. Pipeline approximations and unsupported features remain explicit
in [the compatibility matrix](BuiltInCompatibilityMatrix.md).

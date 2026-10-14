# Ambient lighting parity — 0.1.6

The previously reported **22/192 nonblack-ambient failures are resolved** against
the retained original images, without changing those images or their thresholds.
The implementation is shader-only and preserves shader names, properties, defaults
and public Editor APIs. No new pass, screen copy, texture fetch or avatar component
is added.

## Cause and correction

Original NOVA invokes URP's PBR lighting. Turning off environment reflection maps
still leaves a glossy ambient contribution, multiplied by the BRDF and scene
reflection intensity. The port previously removed that contribution completely.
The original behavior is visible in Unity's
[GlobalIllumination.hlsl](https://github.com/Unity-Technologies/Graphics/blob/2022.3/staging/Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl)
and the ambient-probe constant setup in
[UniversalRenderPipeline.cs](https://github.com/Unity-Technologies/Graphics/blob/2022.3/staging/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipeline.cs).

The corrected shader recovers DC from the renderer SH constants, including the L2
correction needed by Unity's packed representation. Built-in's
unity_IndirectSpecColor RGB is unsuitable as the sole input: it stayed nonzero
after the test changed from sky-probe lighting to Flat/Trilight. The first trial
using it failed 12 of the expanded 60 comparisons and was rejected. Its native
alpha tracks reflection intensity on the tested D3D path.

Diffuse irradiance now clamps negative SH results without raising them to a scene
ambient-color floor. Two GPU tests reproduced that floor incorrectly lighting a
dark custom probe red. Two further low-light tests reproduced the loss of sRGB's
linear segment; two HDR tests reproduced the distinction between diffuse shader
sRGB and the C# Color.gamma conversion used for the glossy constant. Both paths
now use their corresponding original conversion.

Unity documents the renderer SH and legacy environment variables in its
[Built-in shader variable reference](https://docs.unity3d.com/2022.3/Documentation/Manual/SL-UnityShaderVariables.html).
The source inspection was cross-checked against the installed URP shader libraries
and actual GPU output, including ambient-mode changes.

## Reproduction and coverage

- The retained 192-case original comparison fails 22 cases before correction and
  passes 192/192 after correction. These images use Linear; comparing them to Gamma
  captures is invalid and was rejected during verification.
- Fresh original/port captures pass **1,008/1,008** comparisons: four editor/color
  space combinations, each containing 192 coarse-mesh interpolation cases and 60
  isolated ambient cases. Editors are 2022.3.22f1 and 6000.3.23f1, D3D11.
- Ambient cases cover both Lit shaders; black, flat, colored, gradient and supplied
  sky-probe lighting; reflection intensities zero/one; diffuse, dielectric and metal
  workflows. The sequence explicitly switches from sky-probe back to flat modes.
- Twelve focused graphics tests additionally cover custom probes darker than scene
  ambient, reflection intensity 0/0.35/1, low-light sRGB and HDR conversion.
- Full EditMode passes **500/500 per editor**, including worst-case stereo output budgets.
- Full graphics PlayMode passes **945/945 per editor**, including Canvas, LPPV,
  stereo, masks, shadows and optimizer rendering.

Two existing tests were corrected to isolate the quantity they claim to test:
the no-diffuse-energy test and proxy-SH replacement test now set glossy reflection
intensity to zero. Original NOVA does not remove glossy DC just because reflection
maps are disabled. Their numerical assertions and image thresholds were not relaxed.
The existing UI compile-source check now recognizes the native L0/L1 and L2 helpers
used for explicit color conversion instead of requiring the old ShadeSH9 wrapper.

Run NovaAmbientCapture and NovaInterpolationCapture in isolated original/port
projects, passing `-novaVisualOutput <directory>`; define NOVA_VISUAL_REFERENCE only
for the original fixture. Compare with `Tests/Visual/compare_ambient.py` (the ambient
subdirectory) and `compare_interpolation.py` (the output root). Both retain the
existing mean/max/bad-pixel thresholds. Fresh evidence is under
`Verification/AmbientParity20261014`; original reference images remain untouched.

## Performance

The final shader source was measured against main 0035171 on RTX 5070 Ti, D3D11,
Unity 2022.3.22f1 Development Player, 1920×1080. D3D timestamp/disjoint queries ran
offscreen; no visible benchmark window was opened. Validation Editors were closed
for measurement; VRChat/SteamVR were left running.

The initial fragment-only implementation failed four ordinary-path scenarios:
the maximum paired median increase was 0.120440 ms against a 0.02 ms allowance.
It was not accepted for main. Vertex-only conversion still failed four scenarios;
factoring material-invariant BRDF terms, removing repeated normalization and using
the existing view varying resolved the remaining cost. The normalized main light
follows the engine's `UnityStandardCore.MainLight` contract. World-space view vectors
are interpolated only when the parallax field is unused; this affine operation
retains the same unnormalized vector. Curved/normal-mapped SH stays per pixel.
Clipped UI retains its pixel glossy fallback to respect SM3.5/stereo output limits.
No constant-ambient approximation or benchmark-specific light branch was retained.

**All 18 ordinary-path scenarios pass the unchanged max(5%, 0.02 ms) gate.**
The largest paired median increase is **0.004712 ms**. Draw counts, active Material
bytes and RenderTexture bytes are unchanged for every ordinary scenario. Shader
source hashes in the measurement match the released source.

| Ordinary workload | Baseline GPU ms | Candidate GPU ms | Paired median delta ms |
| --- | ---: | ---: | ---: |
| Particle Lit, one layer | 0.018624 | 0.018272 | -0.000368 |
| Particle Lit, eight layers + mirror | 0.187312 | 0.191952 | +0.004712 |
| Particle Lit, combined features + overlap/mirror | 0.504288 | 0.502368 | -0.001720 |
| UI Lit, eight layers + mirror | 0.187040 | 0.190880 | +0.003824 |
| UI Lit, clipped + eight lights + overlap/mirror | 4.199104 | 4.145184 | -0.050416 |

The paired ABBA difference is intentionally separate from subtracting pooled medians.
Optional helpers remain separately measured:

| Helper disabled to enabled | GPU ms | CPU submission ms | Draws | Native allocation delta |
| --- | --- | --- | --- | --- |
| Canvas, 0 lights | 0.018464 to 0.018048 | 0.07305 to 0.08420 | 1 to 1 | Material +26,762 B |
| Canvas, 4 lights | 0.046688 to 0.034688 | 0.07985 to 0.10005 | 4 to 1 | Material +26,762 B |
| Canvas, 8 lights | 0.083232 to 0.050912 | 0.06490 to 0.11030 | 8 to 1 | Material +26,762 B |
| Dedicated DepthNormals, eight meshes | 0.254720 to 0.353328 | 0.06690 to 0.11450 | 1 to 9 | RenderTexture +33,178,072 B |
| LPPV | 0.017728 to 0.021696 | 0.01985 to 0.02020 | 1 to 1 | Proxy texture/buffer recorded in raw blocks |

Three ABBA repetitions use 64 warmup frames and 128 samples per block. Five optional
helper workloads are recorded separately, for 23 scenarios / 276 blocks total.
These are bounded offscreen camera measurements, not whole-VRChat FPS or headset
latency. CPU command submission and active native allocations are in the raw data.

## SDK, AAO and package delivery

The exact 0.1.6 archive was installed into the user's test-VRCLens project and
built through SDK **3.10.5**, with AAO **1.9.20** disabled and enabled.
Both completed with **zero captured errors, C# errors, shader errors and shader
warnings**. The source's 23 SkinnedMeshRenderers remain 23 without AAO and become
19 with AAO. Both preserve 17 NOVA materials, all material properties/keywords/
textures and particle vertex streams, including five active Fire001 renderers
across six particle systems. The built avatar and reflected-camera images were
inspected; the unchanged **30/30** AAO comparison pairs pass the image thresholds.

| SDK bundle | Bytes | Shader warnings/errors | NOVA materials |
| --- | ---: | --- | ---: |
| AAO off | 26,893,650 | 0 / 0 | 17 |
| AAO on | 22,738,586 | 0 / 0 | 17 |

Sanitized bundle/image hashes and assertions are in `sdk-aao.json`. Proprietary
avatar images and bundles remain in the user's project under
`Exports/NovaVRCValidation/Release016Verified`; they are not published here.
This is a local SDK build-and-render test, not a new upload or client/HMD test.

Tarball tests pass **3/3 on each editor**: shader discovery/compilation, 48 optimizer
outputs and the imported showcase. Reusing a 2022 Library while switching the same
package version between local archive locations initially produced stale-cache
CS2001 diagnostics. A fresh validation project without that Library passed with
zero C# or shader errors; the final SDK runs also have zero such diagnostics.
No user Library or shader cache was deleted. The avatar project now references
the permanent archive in its Exports directory rather than the temporary staging
location.

The tarball contains **245 files**, all byte-matched to `Assets/Nova`:
`jp.co.cyberagent.nova.builtin-0.1.6.tgz`, SHA-256
`20412f975496a359e2ee489ccf18478173710411217a1478033de4c925f2cc31`.
A second copy is in the checkout's ignored `Builds/Release` directory.
Python checks pass Docs 4, Parity 8, Visual 19, Performance 11 and Shell 2.

## Remaining boundary

URP supplies **scene-wide ambient-probe DC** separately from renderer SH. The
scriptless Built-in fallback uses **renderer SH DC**; custom/local probes can
therefore produce a different glossy fallback when they differ from the global
ambient probe. There is no claim that removing the 22 demonstrated failures proves
every probe arrangement identical. Matching those inputs or enabling reflection
maps is the practical fallback. This does not reintroduce a scene-color floor or
add local-probe diffuse light twice to LPPV.

Physical VRChat/HMD testing, arbitrary worlds and other graphics backends remain
separate validation TODOs. No new upload is implied by a successful SDK bundle
build/render check.

## Changed files

- NovaBuiltInLighting.cginc: unclamped-to-scene SH, glossy DC and color conversions.
- NovaBuiltInParticleFeatures.cginc and Lit shader declarations: uniform vertex
  work, affine view interpolation and matching keyword stages within SM3.5 budgets.
- NovaBuiltInUberLit.cginc / NovaBuiltInUIParticlesUberLit.cginc: glossy fallback
  through the existing environment BRDF, with no added texture access.
- NovaAmbientLightingTests.cs, ambient capture/comparator and focused test fixtures:
  failing reproductions, original comparisons and regression checks.
- Compatibility matrix, API evidence inventory and package documentation: precise
  scope and probe-input limitations; package version 0.1.6.

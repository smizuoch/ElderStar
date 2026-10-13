# Shader-only compatibility research (2026-10-13)

## Conclusion and scope

There are practical alternatives to runtime C# and to inline sampler limitations.
This investigation does not ship a new rendering path: production stays at
`8064c6a`. Its purpose is to test feasibility and avoid adding cost without a
demonstrated compatibility benefit. The target remains PC VRChat avatars with
unchanged original effect authoring wherever possible.

The earlier 8x/9x statement needs qualification. An inline `Aniso9` name is rejected
by Unity, but that does **not** mean sampler setting 9 is inaccessible. A texture
sampler can be shared. Also, a requested maximum anisotropy is not a guarantee of
that many hardware taps or a visible difference between adjacent settings.

## Executed GPU experiment

The isolated fixture in `Tests/Research/SamplerSharing` compares a source texture's
sampler with a sampler borrowed from a 2x2 texture. It tests settings 2, 9 and 16,
both Texture2D and RenderTexture inputs, and three oriented/scaled footprints.
The input is deterministic 512x512 noise; output is 64x64 RGBA8 with no mip chain.
Quality is Per Texture. These conditions are intentionally narrower than a real
avatar, mipmapped material, camera compositor or VR stereo validation.

On RTX 5070 Ti / Windows D3D11, Unity 2022.3.22f1 and 6000.3.23f1 both show:

| Requested setting | Shared sampler vs direct | Inline 8 vs direct, maximum channel difference | Negative control: sampler 1 vs direct |
| --- | ---: | ---: | ---: |
| 2 | 0/255 | 149/255 | 94/255 |
| 9 | 0/255 | 0/255 | 157/255 |
| 16 | 0/255 | 59/255 | 157/255 |

The same values occur for Texture2D and RenderTexture inputs. All 12 behavioral
tests pass; two additional tests export the compiled fragment program. Changing
to sampler 1 visibly changes the result, preventing a blank/unused comparison
from passing. Shader compiler diagnostics must also be error-free.

The first attempt failed compilation in both editors because the donor texture
was unused and optimized away. The retained version references it through a
uniform branch that is not taken on the measured sampling paths. D3D11 compiled
programs show a conditional donor `ld`, followed by mutually exclusive sampling
branches: the shared-sampler path executes one `sample` then returns. This is
instruction-level evidence, **not a GPU-millisecond benchmark**.

The production camera compositor currently uses inline 8 only under ForceEnable.
The experiment does not prove all 8/9 filtering is identical on every GPU or that
all of the earlier camera residuals are filtering errors. It specifically shows
that changing 8 to 9 has no measured benefit in these cases on this GPU. No new
production overhead is justified by this result alone.

## Alternatives and cost assessment

| Approach | What it can reproduce | Cost / important constraint | Decision |
| --- | --- | --- | --- |
| Bake constants, curves or lookup data during authoring/build | Pipeline-independent calculations and known material/effect data without runtime C# | Extra data size and possibly texture fetches; bake only stable inputs, preserve animated values | Preferred when an observed mismatch requires it; no manual conversion step should be added |
| Reuse a texture sampler | Requested filtering settings not expressible with inline names | Prototype adds no pass/copy and keeps one sampled source lookup; adds a tiny texture binding and uniform branching | Feasible, measured above; validate material serialization, build stripping and AAO before avatar use |
| Keep a donor texture alive with a real lookup | Robust binding without a control branch that a material optimizer might remove | At least one extra cached lookup; a 2x2 RGBA8 payload is 16 bytes, excluding Unity/driver allocation overhead | Alternative to test only if sampler sharing fixes a real mismatch |
| Explicit derivatives / sampling math | Some UV, LOD and interpolation mismatches | Additional arithmetic, usually no extra scene copy; depends on the actual discrepancy | Prefer a focused failing probe before changing sampling |
| Named GrabPass and existing particle data | Single-surface screen distortion, camera-relative shader math | Existing scene copy and overdraw remain; a shared captured background does not sum all objects' offsets | Already implemented; preserve its copy count by default |
| Multiple GrabPass stages / framebuffer encoding | Some controlled overlapping or staged effects | Extra full-screen copies, precision/color-space constraints, render-order interference, stereo/mirror complications | Not a general default; only consider a bounded opt-in prototype with measured benefit |
| Small Custom Render Texture | Shader-updated state, precomputed tables or a constrained simulation | Update work/storage; double buffering may copy every swap; updates are not equivalent to per-camera geometry accumulation | Consider for small data, not automatically a screen-sized distortion buffer |
| Avatar Camera plus RenderTexture | A separate rendered buffer without a custom runtime component | Extra scene rendering; remote-camera restrictions and camera/eye synchronization | Poor default for this goal; requires explicit scene/avatar-specific validation |
| EWA/manual anisotropic filtering | Mathematically defined filtering in shader code | Multiple taps, weights and bandwidth; does not automatically match the original GPU sampler | Research fallback, not a default replacement for one hardware sample |

Static data can be prepared by editor/build code and shipped as ordinary assets;
that is different from requiring a C# component to execute in VRChat. A texture
cannot contain future information from an arbitrary world, another avatar or a
camera pass that was never rendered. A shader can consume available depth and
camera inputs, but cannot create missing world depth merely by storing constants.

Original NOVA sums distortion offsets into a separate RGHalf buffer, then samples
the background once using that sum. Sequential refraction and overwriting the
same captured background have different math. Therefore more GrabPasses alone
are not evidence of correct overlap. Matching this requires tests with multiple
overlapping particles, reversed order, both eyes, mirrors and multiple avatars.

## API compatibility and test coverage

URP entry points such as `AddRenderPasses(... RenderingData ...)` belong to URP's
type/lifecycle contract. Their visual work can be reimplemented; putting data in
a shader does not make Built-in invoke a URP callback or provide the original
type identity. No-op lookalike APIs would not constitute compatibility.

The previous warning about all API inputs is a test-coverage qualification, not
proof of a missing feature or an impossibility claim. Improve it with exhaustive
finite enums, boundary-value cases, differential fuzzing, invariant/idempotence
checks and measured t-way combination coverage. This increases confidence without
claiming that every texture, float, animation sequence and graphics environment
has been enumerated. NIST describes combinatorial testing as a way to systematically
cover parameter interactions; it is not a universal correctness proof.

## Adoption gates / next TODO

- [x] Check official sampler, GrabPass, CRT and VRChat component constraints.
- [x] Reproduce failed donor binding before retaining it; compare settings on both editors.
- [x] Inspect compiled GPU branches and source-sample count.
- [x] Record that setting 9 and inline 8 matched in this experiment; do not attribute all previous residuals to 8/9.
- [ ] Isolate any remaining mismatch that matters in the avatar path before choosing a replacement.
- [ ] If sampler sharing is useful, verify baked/material texture binding survives SDK build and AAO, with no runtime C#.
- [ ] Profile the proposed final shader against the current one at matched resolution, coverage, overlapping effect counts and stereo/mirror views. Report GPU time and noise, copies, draws, texture samples and memory separately.
- [ ] Reject an expensive default when the observed visual benefit does not justify its VRChat frame-time cost.

No GPU timing improvement or zero-overhead claim is made here. No new avatar
upload, AAO build, physical headset test or product-wide regression run is implied
by these isolated experiments. Production shaders and previous image baselines
were not changed.

## Sources

- [Unity: texture/sampler sharing and inline samplers](https://docs.unity3d.com/2022.3/Documentation/Manual/SL-SamplerStates.html).
- [Unity: texture anisotropy settings](https://docs.unity3d.com/ScriptReference/Texture-anisoLevel.html) and [Direct3D sampler descriptor](https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ns-d3d11-d3d11_sampler_desc).
- [Unity: GrabPass](https://docs.unity3d.com/2022.3/Documentation/Manual/SL-GrabPass.html) and [Custom Render Textures, including double-buffer copy cost](https://docs.unity3d.com/2022.3/Documentation/Manual/class-CustomRenderTexture.html).
- [VRChat: allowed avatar components and remote camera restrictions](https://creators.vrchat.com/avatars/whitelisted-avatar-components/whitelisted-avatar-components/).
- [Greene and Heckbert, EWA filtering, 1986: author institution abstract](https://www.ri.cmu.edu/publications/creating-raster-omnimax-images-from-multiple-perspective-views-using-the-elliptical-weighted-average-filter/) and [PBRT: EWA implementation](https://pbr-book.org/3ed-2018/Texture/Image_Texture#EllipticallyWeightedAverage). The paper's abstract and the published PBRT implementation were consulted; no claim of reproducing a hardware vendor's private filter kernel is made.
- [NIST: combinatorial testing](https://www.nist.gov/publications/combinatorial-testing?pub_id=910001).

## Reproduction

Copy both `Tests/Research/SamplerSharing` files into the existing isolated project's
PlayMode test assembly. Do not install them in the user avatar project or shipped
package. Run separately with each installed editor, without `-nographics`:

```text
unity test <isolated-project> --editor-version <version> --mode PlayMode --filter NovaSamplerSharingResearch --output <result.xml> --format json -- -force-d3d11 -novaShaderBinary <fragment.bin> -logFile <log>
```

Each editor must report seven passing tests. `fragment.bin` contains D3D bytecode
exported through `ShaderData.Pass.CompileVariant(..., forExternalTool: true)`;
`D3DDisassemble` was used to inspect it. Evidence and the tested source hashes are
in `Documentation/Verification/ShaderOnlyResearch20261013`.

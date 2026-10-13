# Avatar distortion parity and GPU cost (2026-10-14)

Version 0.1.4 fixes an avatar-only overlap defect without another screen copy,
sampler, pass, material property, or avatar runtime component. A surface whose
final offset is exactly zero now discards instead of writing the old shared grab
over distortion behind it. Completed fade, dissolve and depth fade also benefit.
There is no epsilon cutoff: small nonzero effects remain visible.

Uniform zero intensity with unused Custom Data coordinates is rejected in the
vertex shader, avoiding pixel work. Single and XY intensity modes are handled
separately; a selected Custom Data coordinate prevents this optimization. Other
zero offsets still use the fragment test. A named GrabPass may still execute when
only zero-strength renderers are present: this does **not** make them free.

## Reproduction and rendering

- Before the fix, eight new overlap cases failed in each supported Editor:
  4,096 pixels lost their underlying distortion. Cases are zero strength,
  completed fade, completed dissolve and depth fade, each with ordinary and
  reflected camera matrices. The original URP passes the same behavioral checks.
- Six additional cases ensure zero material intensity with nonzero Custom Data
  still renders (single/X/Y, avatar and camera-compositor paths).
- Existing stereo tests now use nonzero intensity to exercise their red/green
  eye textures. Expected eye colors and golden images are unchanged.
- The differential fixture uses unchanged original NOVA and the existing strict
  image thresholds: all 32 pairs pass across Unity 2022.3.22f1 and 6000.3.23f1.
  Final EditMode tests pass 448/448 and PlayMode tests pass 866/866 per Editor,
  including shader diagnostics and synthetic stereo. Source, bundle and comparison details are in
  `Verification/AvatarParity20261014/verification.json`.
- `Tests/VRChat/Editor/NovaVrcDistortionProbe.cs` serializes all five relevant
  material configurations through the real SDK build, then renders the actual
  bundle materials. This detects missing/stripped variants and lost settings.
  Full-avatar renders also cover Fire001 at three times and a reflected view.
- SDK 3.10.5 builds pass with AAO 1.9.20 disabled and with default Trace And
  Optimize enabled: zero SDK/shader/logged errors and zero magenta pixels.
  AAO reduces skinned meshes from 23 to 19; all 17 NOVA material states and five
  active Fire001 particle vertex-stream configurations survive serialization.
  All 16 AAO-off/on image pairs pass (maximum channel difference 1/255).
  Final bundle sizes are 26,684,457 and 22,583,268 bytes, respectively.

The SDK acceptance setup initially hit stale Bee references to the replaced
package cache and a native Editor crash involving the shared white texture. The
old Bee directory was backed up before regeneration. The new fixture now owns
its transition texture instead of serializing Unity's shared `whiteTexture` into
a bundle it later unloads. The owned scene's earlier Private blueprint reference
and AAO component were restored locally from the completed upload receipt; no
remote avatar was changed. Future comparisons also save an on-disk scene backup
before SDK work, because a managed finally cannot recover from a native crash.
These were validation-environment/fixture repairs; the final reports have zero
SDK, shader and logged errors.

## Measured GPU cost

Unity 2022.3.22f1, D3D11, NVIDIA RTX 5070 Ti, driver 32.0.16.1742. Both variants
are in the same release Player. A quad covers 81% of a 1920x1080 ARGB32 target;
HDR and MSAA are off. The baseline is `8064c6a`. Two completed final runs contain
15,360 accepted GPU intervals, alternating baseline/candidate in ABBA order.
Other validation Editors were closed for these runs. The user's VRChat, SteamVR
and ordinary desktop applications remained running; GPU clocks/load are not
controlled. The raw paired block variation is retained, not presented as a
universal confidence interval.

| Workload | Baseline median | Candidate median | Paired median change, two runs |
| --- | ---: | ---: | ---: |
| 1 active surface | 0.0387 ms | 0.0402–0.0404 ms | +0.0015–0.0016 ms |
| 8 active surfaces | 0.1120–0.1121 ms | 0.1150–0.1152 ms | +0.0028–0.0033 ms |
| 8 active + 32 uniform-zero surfaces | 0.4469–0.4473 ms | 0.1152–0.1154 ms | −0.3312–0.3316 ms |
| 32 active surfaces | 0.3669–0.3672 ms | 0.3743–0.3744 ms | +0.0074 ms |
| 8 active + 16 uniform-zero surfaces, two cameras | 0.5547–0.5549 ms | 0.2259 ms | −0.3289–0.3293 ms |

The active-surface path has a small measured cost (up to about 2% here), not
zero overhead. Invisible-layer stress improves about 74%; the two-camera case
improves about 59%. No default high-tap filter or repeated screen copy was added.
The zero-culling performance gate was set before vertex optimization: adding 32
uniform-zero surfaces must cost less than 50% of eight active surfaces. The
fragment-only implementation failed at 0.40746 ms; both final runs pass near
0.115 ms.

The initial hidden FrameTimingManager run returned no GPU samples and was
rejected. A validation-only native plugin instead timestamps the offscreen
camera commands using D3D11 timestamp/disjoint queries, rejecting invalid results
and keeping block IDs across delayed readback. Both variants use the same
instrumentation. PPM witness captures verify actual rendering. These values are
**offscreen camera GPU intervals, not VRChat FPS, total frame times, or HMD
latency**. The timer is not part of the package or avatar. See the reproducible
[benchmark instructions](../Tests/Performance/README.md) and raw reports.

## Deliberate remaining differences

- Two overlapping **nonzero** avatar offsets still do not accumulate like the
  original URP RGHalf buffer. A shared background sample per surface cannot read
  future surfaces' offsets. Exact accumulation needs a separately scheduled
  offset target and composition; per-surface color-difference blending is only a
  first-order approximation and adds sampling cost. The fix above removes the
  zero-contribution defect without introducing that new approximation. Ordinary
  Built-in cameras can use the existing compositor; avatar-only worlds retain
  the documented shader path.
- The forced camera-compositor sampler remains 8x versus original 9x. The
  [sampler-donor experiment](ShaderOnlyCompatibilityResearch.md) showed no image
  improvement on this GPU, so its extra texture/branch was not adopted.
- URP renderer-feature/RenderGraph API inheritance cannot be supplied without
  those assemblies. Their Built-in visual substitutes and public API dispositions
  remain explicit in the compatibility matrix.
- Reflected Unity cameras, built avatar bundles and synthetic stereo slices are
  verified. This run does not upload a new remote avatar or manipulate the user's
  running VRChat session. The previous 0.1.3 desktop client/mirror acceptance is
  historical, not proof of this binary in the client. Actual updated-client mirror
  and physical PCVR/HMD checks remain the next manual acceptance scope.

Sources: [VRChat shader globals](https://creators.vrchat.com/worlds/udon/vrc-graphics/vrchat-shader-globals/),
[VRChat mirror component](https://creators.vrchat.com/worlds/components/vrc_mirrorreflection/),
[avatar component whitelist](https://creators.vrchat.com/avatars/whitelisted-avatar-components/whitelisted-avatar-components/),
[D3D11 timestamp queries](https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_query).

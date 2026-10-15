# Avatar distortion GPU comparison

Use a disposable Built-in project with Unity 2022.3.22f1 on Windows/D3D11.
None of these scripts or the native timer belongs in `Assets/Nova`, an avatar,
or the VRChat project. The native DLL is built locally, not committed.

1. Export `Assets/Nova/Runtime/Core/Shaders/ParticlesDistortion.shader` and
   `Assets/Nova/Runtime/Core/Shaders/BuiltIn/NovaBuiltInDistortion.cginc` from
   commit `8064c6a` with `git show`, preserving UTF-8 bytes.
2. Run `python Tests/Performance/prepare.py --project <isolated-project>
   --baseline-shader <exported.shader> --baseline-include <exported.cginc>`.
   This copies the candidate package, creates a separately named baseline shader,
   and records SHA-256 source hashes. Baseline shader names/include paths alone
   are rewritten; its shader math is unchanged. Common includes are shared because
   this change modifies only the distortion include.
3. Run `Tests/Performance/Native/build-native.ps1 -ProjectPath <isolated-project>`.
   Override EditorPath, VisualStudioPath and WindowsSdkVersion if needed. It uses
   the installed Unity PluginAPI headers and MSVC/Windows SDK; no download needed.
4. Build through Unity CLI with `--target StandaloneWindows64 --execute-method
   NovaAvatarGpuBenchmarkBuilder.Build --args "-force-d3d11 -novaBenchmarkPlayer
   <absolute-player.exe>"`. Keep its build log and source hashes.
5. Run the executable with `-force-d3d11 -novaOffscreen -novaPerformanceOutput
   <absolute-report.json> -logFile <absolute-log>`. On Windows, use
   `Start-Process -WindowStyle Hidden`. Do not pass `-nographics`. Close other
   validation Editors before measurement; do not close user applications without
   authorization. Record concurrent graphics workloads.
6. Run `python Tests/Performance/analyze_gpu.py <report.json> <summary.json>
   --check-zero-culling`. Run its unit tests with
   `python -m unittest discover -s Tests/Performance`.

Five scenarios cover 1, 8 and 32 active overlapping quads, 8 active plus 32
uniform-zero quads, and 8 active plus 16 uniform-zero quads rendered by two
cameras (one reflected). Each quad covers 81% of a 1920x1080 target. HDR, MSAA,
vSync and frame rate limits are disabled. Each scenario has three ABBA
repetitions, 64 warmup frames/block and 128 valid GPU samples/block. PPM witness
images are captured before warmup; compare them to verify visible rendering.

Offscreen measurements bracket the camera render commands with D3D11 timestamp
queries on Unity's render thread. Disjoint timestamps and unfinished results are
rejected; block IDs prevent delayed results leaking into a later variant. Both
variants use the same query/flush instrumentation. This measures an offscreen
camera workload, **not whole VRChat frame time, client FPS, or headset latency**.
The second camera models mirror rendering cost; it is not an actual VRChat mirror.
CPU timing arrays are empty in this mode, intentionally.

The optional FrameTimingManager mode requires a rendering/presenting window. An
initial hidden run returned no GPU samples; a zero value is never accepted as a
performance result. Offscreen mode avoids displaying or manipulating a window.

The zero-culling gate was set before implementing vertex rejection: 32 additional
uniform-zero surfaces must cost less than 50% of the eight-active workload. The
fragment-only fix failed (0.40746 ms versus a 0.17088 ms ceiling on the test GPU).
The gate targets this optimization; it does not assert a universal avatar budget.
Report active-surface overhead separately, including raw paired block variation.

References: [Unity native rendering callbacks](https://docs.unity3d.com/2022.3/Documentation/Manual/NativePluginInterface.html),
[D3D11 timestamp/disjoint queries](https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ne-d3d11-d3d11_query),
[FrameTimingManager limitations](https://docs.unity3d.com/2022.3/Documentation/Manual/frame-timing-manager.html).

## Lighting and optional helpers (0.1.5)

Export the complete `Assets/Nova/Runtime/Core/Shaders` tree from `c38b1f1`.
Use `prepare_lighting.py --project <closed-disposable-project> --baseline-tree
<export-root>`, then the same native timer build. Unlike the earlier distortion
fixture, baseline includes are completely independent from candidate includes.
Build `NovaLightingGpuBenchmarkBuilder.Build` with `-novaBenchmarkPlayer <exe>`.
Run the Player with `-batchmode -force-d3d11 -novaPerformanceOutput <json>` and
`-WindowStyle Hidden`; do not use `-nographics`. Analyze with
`python Tests/Performance/analyze_lighting.py <json> <summary.json>`.

Eighteen ordinary scenarios compare four shader families, textured diffuse
controls / normal-rim-parallax-fog combinations, one / eight overlapping surfaces
with a reflected camera, and clipped UI Lit with eight scene lights. The controls
are configured diffuse materials, not untouched fresh material defaults. Five
optional scenarios compare Canvas 0/4/8 selected lights, an eight-renderer normal
buffer and spatial LPPV. Candidate helper off/on uses the same candidate shader.
Each scenario has three ABBA repetitions, 64 warmup frames and 128 samples per
block at 1920x1080. Source hashes, actual draw counters, CPU command-submission
intervals and active native material/RT/probe sizes accompany GPU timestamps.
The Development Player is used for counters; this is not total VRChat frame time,
managed-heap profiling or total device VRAM consumption.

The ordinary gate is unchanged draw count and GPU increase <= max(5%, 0.02 ms).
Optional costs are reported separately and never interpreted as passing that gate.
Canvas off uses Built-in light attenuation/ForwardAdd, while on uses the selected
URP-style single-pass list: timing differences do not prove identical pictures.
Keep ordinary scene layers/shadows and other running GPU applications in the report.
See `Documentation/LowCostLightingParity.md` for measured results and limitations.

## Projected depth-fade follow-up (2026-10-15)

Export the complete shader tree from `d9636c0` as the independent baseline and
prepare the existing disposable lighting project with `prepare_lighting.py`.
Pass `-novaDepthFadeBenchmark` to `NovaLightingGpuBenchmarkBuilder.Build`.
The eight configured default controls keep depth fade off; combined scenarios
and clipped UI with eight lights enable it (near 1, far 10, width 3). The same
18 ordinary ABBA workloads run; unrelated optional helper workloads are omitted.
The original cost gate and sampling counts are unchanged. This measures the
four Uber shader families, not distortion GrabPass or camera callback CPU cost.

## Soft-particle / ambient follow-up (0.1.7)

Use main `faad69ee` as the independent baseline and build with
`-novaSoftAmbientBenchmark`. Combined ordinary materials enable soft particles
(intensity 0.5); default controls leave them off. Eighteen ordinary conditions
retain the same gate and sample schedule. Two optional ambient-camera conditions
measure helper off/on using the same candidate shader. All forty before/after
witness images must be visible. See `SoftParticleAmbientAcceptance.md` for the
measurement scope, CPU intervals and unchanged native material/target sizes.

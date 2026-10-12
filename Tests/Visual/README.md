# Controlled original / Built-in comparison

`NovaVisualCapture.cs` captures 24 fixed material configurations for each of the
four Uber shaders. Run it only in isolated test projects: it changes their scene
and pipeline settings. It writes PPM images and capture metadata; it never reads
or updates the original project's golden images.

Prepare two Unity 6000.3 projects with identical ProjectSettings and a PlayMode
test assembly referencing `UnityEngine.TestRunner` and `nunit.framework.dll`.
Copy this capture script into that assembly. The Built-in project contains the
ported NOVA package and no URP. The reference project contains a separate copy of
original NOVA 3.6.0 and URP 17.3.0, installed through Package Manager. Prefix only
the reference copy with `#define NOVA_VISUAL_REFERENCE` and add assembly references
to `Unity.RenderPipelines.Core.Runtime` and `Unity.RenderPipelines.Universal.Runtime`.

Run each editor with graphics enabled, identical graphics API and color space:

```text
Unity.exe -batchmode -projectPath <project> -runTests -testPlatform PlayMode -testFilter CaptureOriginalFeatureFrames -testResults <fresh-results.xml> -novaVisualOutput <capture-directory> -logFile <log>
```

Require a passing capture result and exactly 96 images from each project. Then,
with Python and Pillow installed:

```text
python Tests/Visual/test_compare_frames.py
python Tests/Visual/compare_frames.py --reference <original-captures> --actual <builtin-captures> --report <report-directory>
```

The comparator fails on missing frames, mismatched dimensions, invisible base
geometry, or any of the 96 common-surface differences above the documented thresholds. It outputs JSON,
Markdown and contact sheets showing original, port and amplified difference.
Lit frames use a controlled diffuse dielectric with specular/reflections disabled.
Their common material stack must meet the same image thresholds; the dedicated
lighting rendering tests additionally check light scheduling, shadows and reflections.

For PBR parity, repeat each capture command with `-novaPbr` and a separate empty
capture directory. This produces 72 frames: two Lit shaders, two workflows, three
smoothness values (0, 0.3, 0.7), three light/view angles (0, 30, 60 degrees), and
direct/environment lighting. The environment cubemap has a different color per
mip, so the comparison checks roughness mip selection as well as Fresnel.
Run the same comparator with `--pbr`. Every frame must contain visible geometry
and meet the unchanged thresholds. These controlled inputs isolate the original
BRDF math; they do not establish equality of pipeline light/probe assignment.

Repeat the PBR capture in Linear color space with `-novaMatchLighting` on both
commands. The Built-in fixture invokes the package's explicit lighting-settings
migration action; URP already sets these flags every frame. Capture metadata
records both flags. Using the default Built-in gamma-intensity setting in a Linear
project is a different lighting input, not a valid equal-input BRDF comparison.
Also repeat the common capture without `-novaPbr`, retaining `-novaMatchLighting`,
to verify all 96 common-feature frames in Linear space. Full acceptance therefore
covers 336 comparisons: 96 common and 72 PBR in each color space.

## Unconverted Fire001 effect

`NovaImportedEffectCapture.cs` validates the externally supplied Fire001 prefab.
The copyrighted third-party effect assets are not included in this repository.
Use the package hash in `Documentation/DirectImportVerification.md` to identify
the tested input. Prepare isolated projects as above; the reference uses the NOVA
source bundled inside Fire001, not the later 3.6 source. The Built-in project uses
`BuiltInEffectPackageImporter.Import(Preview(path), false)`; `NovaEffectAcceptance.cs`
is an Editor entry point for this operation (`-executeMethod NovaEffectAcceptance.Import
-novaEffectPackage <original.unitypackage>`).

Copy the effect capture into each PlayMode test assembly, defining
`NOVA_VISUAL_REFERENCE` only in the original project. Run with
`-testFilter CaptureImportedEffectFrames -novaEffectPrefab Assets/N2Studio/Prefabs/Fire001.prefab
-novaVisualOutput <output>`, graphics enabled. Repeat in Gamma and Linear.
The fixture does not run any material postprocessor or reassign any shader.
It seeds only instantiated particle systems, simulates six timestamps, and renders
three views at 384 x 384. Disabled renderers stay disabled, as in the original prefab.

Compare each color space with:

```text
python Tests/Visual/compare_effect_frames.py --reference <reference> --actual <builtin> --output <metrics.json>
```

Require passing capture XML, 18 images per side, identical particle state at every
timestamp, unchanged source effect bytes, and the same image-error thresholds as
the common comparison. These tests certify the prefab under matched settings,
not the bundled URP demo scene or unrelated URP dependencies.

For Windows player acceptance, copy `Tests/PackageImport/Runtime/NovaImportedEffectPlayerSmoke.cs`
and `Tests/PackageImport/Editor/NovaImportedEffectPlayerBuilder.cs` into the isolated
package project's Runtime and Editor test harness folders. Invoke
`-executeMethod NovaImportedEffectPlayerBuilder.Build` with graphics enabled.
Run `Build/Fire001/Fire001Acceptance.exe -batchmode -force-d3d11 -novaReport <absolute-report.txt>`.
Require exit code 0, PASS, and a visible PPM frame. The builder creates its own
acceptance scene; it neither edits the source prefab nor imports the URP demo scene.

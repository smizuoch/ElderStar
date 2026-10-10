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
lighting rendering tests additionally check the documented BRDF approximation,
shadows and reflections. This does not claim full URP PBR equality.

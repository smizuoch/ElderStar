# Release verification — 2026-10-12

NovaShaderBuilt-in 0.1.0 ports NOVA 3.6.0 to the Built-in Render Pipeline.
This follow-up restores practical authoring and lighting parity and supersedes
the 2026-10-10 acceptance report. All checks below passed; existing golden images
and baseline thresholds were not changed. The [compatibility matrix](BuiltInCompatibilityMatrix.md)
resolves all 65 feature groups with explicit reasons and fallbacks.

## Changes and original contracts

- Restored the original Inspector foldout PNGs and import settings.
- Compared all 889 ShaderLab properties (attributes, labels, types, ranges, defaults,
  order), CustomEditor declarations, 525 public/protected editor declarations and
  14 original Inspector/helper source token streams.
- Differential-tested 720 material-state transitions against original processors;
  restored legacy second-transition texture migration and dormant shadow-alpha state.
- Executed five real ShaderGUI paths with populated effects/texture modes,
  multi-material edits, Undo/Redo, persistent foldouts and 16 shader reassignments.
  Layout/Repaint and property operations are covered; native mouse clicks are not automated.
- Restored original opaque depth/shadow keyword reduction and Assets-root optimizer output.
- Replaced the former normalized Blinn-Phong approximation with original GGX direct
  response and environment roughness, Fresnel and mip selection, without SRP includes.
- Added explicit, undoable **Match Original Lighting Settings** for Linear migration.
  It changes project-wide light settings only when invoked; package import leaves them alone.
- Fixed the packaged migration document's relative compatibility link.

Changed implementation lives under `Assets/Nova/Editor/Core/` and
`Assets/Nova/Runtime/Core/Shaders/BuiltIn/`. Regression coverage is under
`Assets/Tests/Editor`, `Assets/Tests/PlayMode`, `Tests/Parity`, `Tests/Visual` and `Tests/Docs`.
See [contract provenance and TDD evidence](../Tests/Parity/README.md).

## Revisions and delivered package

- Validated implementation and fixtures: `457b079c4426e254bfab0d3ed7ce26e10cfd401f`.
- Package Git tree: `b50bc9db8877c412eb6ed14278704e1a447c8a76` (`Assets/Nova`).
- Archive: `jp.co.cyberagent.nova.builtin-0.1.0.tgz`, 149,929 bytes.
- SHA-256: `68e2c12213bbe0367ad4b38365888d6b3d06bec3c64da55fa7219e661c2fd57e`.
- All 195 archived files match package source byte-for-byte.
  The archive was produced by `PackageManager.Client.Pack` and installed in both isolated projects.
- Local [release tarball](../Builds/Release/jp.co.cyberagent.nova.builtin-0.1.0.tgz).
  Build outputs are ignored by Git; source and verification evidence are committed.

## Editor, package and player acceptance

| Check | Unity 2022.3.22f1 | Unity 6000.3.23f1 |
| --- | ---: | ---: |
| Full EditMode | 416/416 passed | 416/416 passed |
| Full graphics PlayMode | 763/763 passed | 763/763 passed |
| Final tarball package tests | 3/3 passed | 3/3 passed |
| Windows standalone | Exit 0, PASS | Exit 0, PASS |
| Editor graphics API | Direct3D11 | Direct3D12 |

The canonical suites cover shader discovery/properties/APIs, compiler diagnostics,
material setup, real camera/particle/Canvas rendering, instancing, lights/shadows,
reflections, depth/fog, distortion, optimization, migration and the packaged sample.
Both isolated package projects have no URP dependency. Package tests compile all
six shaders and all 48 optimizer outputs and import the sample through Package Manager.

Each final-tarball player found all six shaders, passed four Uber color renders and
distortion enable/disable checks, loaded the packaged showcase, simulated five
particle systems and verified its rendered frame on Direct3D11. See
[Unity 2022 PASS](Validation/Players/Unity2022.txt), [frame](Validation/Players/Unity2022.png),
[Unity 6 PASS](Validation/Players/Unity6.txt), [frame](Validation/Players/Unity6.png).

Additional checks: shell environment regression; 2 runner tests; 4 documentation
tests; 4 original-contract tests; 7 comparator tests, including deliberate mismatch
and blank-PBR rejection. Shader validation was accompanied by real rendering;
no visual PlayMode command used `-nographics`.

## Original / port visual regression

Fresh isolated NOVA 3.6.0 / URP 17.3.0 and Built-in captures used Unity 6000.3.23f1,
Direct3D12. Every comparison passed unchanged normalized thresholds: mean RGB error
at most 0.01, channel error at most 0.08, and at most 1% of pixels with mean RGB error
above 0.02. Missing frames or invisible required geometry fail acceptance.

| Group | Comparisons | Largest mean error | Largest channel error | Bad pixel fraction |
| --- | ---: | ---: | ---: | ---: |
| [Common stack, Gamma](Validation/Visual/report.md) | 96 passed | 0.000994913 | 0.003921569 | 0 |
| [Common stack, Linear](Validation/VisualLinear/report.md) | 96 passed | 0.00010978349673202615 | 0.00392156862745098 | 0 |
| [PBR, Gamma](Validation/VisualPbr/report.md) | 72 passed | 0 | 0 | 0 |
| [PBR, Linear](Validation/VisualPbrLinear/report.md) | 72 passed | 0 | 0 | 0 |

Each group of 96 common frames covers all four Uber shaders in Gamma or Linear color space. Each PBR group covers both Lit
shaders, two workflows, three smoothness values, three light/view angles, direct
GGX and mip-colored cubemap reflection/Fresnel. Controlled PBR outputs match exactly
at 8-bit readback in these cases; this is not a claim of equality in arbitrary scenes.

The initial Linear comparison failed 36 direct-light frames because a fresh Built-in
project uses gamma light intensity while URP enables linear intensity. Applying the
same shipped lighting migration action aligned inputs and all 72 passed. Both flag
values are recorded in capture metadata. See [migration instructions](Migration.md).
Older surface/light tests read HDR targets to avoid clipping the GGX peak; their
comparison thresholds were not relaxed. No original golden image was updated.

## Artifacts, logs and remaining scope

The [Windows x64 demo ZIP](../Builds/Release/NovaShaderBuiltIn-Showcase-Windows-x64.zip)
contains the Unity 6 player, runtime files, NOVA license and `LaunchShowcase.cmd`.
Extract all files and run that launcher; close the window to finish. ZIP integrity
passed. Size: 65,364,125 bytes. SHA-256: `871fd829c6a6d141cf320fce51db33524a80e6ebd70c9826b673e9d768add2fa`.

NUnit attributes and log paths/hashes are preserved in [results.json](Validation/results.json).
Canonical XML/logs are in `Logs/Unity/<version>/`; final package logs use
`Logs/package-parity-{pack2022,install6,tests2022,tests6,build2022,build6,player2022,player6}`.
Use [Verification.md](Verification.md) and [visual instructions](../Tests/Visual/README.md)
to reproduce the checks.

Built-in still has different light scheduling, attenuation, shadow/probe assignment,
depth and camera integration. URP Forward+, APV, RenderGraph, debug/foveated paths and
the URP DepthNormals contract cannot be reproduced by this package's Built-in-only
architecture. Distortion is a mono camera effect. Original NOVA also omits probe
blending/box-projection variants. Other graphics APIs/mobile/Web/XR need platform
acceptance; native mouse-driven Inspector interaction is not covered by these tests.

Every limitation has a documented fallback in the matrix. All required TODOs for
the documented Windows scope and this authoring follow-up are complete. The next
optional work is target-platform/XR acceptance and native interactive UI validation.

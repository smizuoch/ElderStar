# Release verification — 2026-10-10

NovaShaderBuilt-in 0.1.0 ports NOVA 3.6.0 to the Built-in Render Pipeline.
This report records graphics-enabled editor tests, fresh original/port image
comparisons, installation of the actual release tarball, and standalone execution.
The [compatibility matrix](BuiltInCompatibilityMatrix.md) resolves all 65 feature
groups; its documented approximations and unsupported pipeline features define
the release's limits.

## Delivered files

- `Assets/Nova/Runtime/Core/Shaders/`: six original shader assets and the Built-in
  compatibility, material features, lighting, depth, shadow, fog and instancing includes.
- `Assets/Nova/Runtime/Core/Scripts/ScreenSpaceDistortion.cs`: camera distortion.
- `Assets/Nova/Editor/Core/Scripts/`: original inspectors/material processors,
  optimizer/replacer, `BuiltInMaterialMigration.cs` and `BuiltInShowcase.cs`.
- `Assets/Nova/Samples~/BuiltInShowcase/`: playable scene, materials and textures.
- `Assets/Tests/Editor/`, `Assets/Tests/PlayMode/`, `Tests/PackageImport/`,
  `Tests/Visual/`, `Tests/Docs/` and `Tests/Shell/`: acceptance and regression coverage.
- `README.md`, `Documentation/`, `Assets/Nova/Documentation~/` and `Scripts/`:
  installation, compatibility, migration, reproducible verification and evidence.

## Revisions and package

- Validated implementation and fixtures: `a745555bb3419ec2e97c778b9d3e95aed13d1618`.
- Package Git tree: `cedb48812f9f1f114a6aa7ac0f11f7c75319999f` (`Assets/Nova`).
- Archive: `jp.co.cyberagent.nova.builtin-0.1.0.tgz`, 145,018 bytes.
- SHA-256: `aece4276a560fbdc4de48c15a79cbebec021b956f21ea9346bd53b3442b0fcda`.
- All 188 archived files were compared byte-for-byte with the package source;
  no mismatches were found. The archive was produced by `PackageManager.Client.Pack`.
- Local delivery: [release tarball](../Builds/Release/jp.co.cyberagent.nova.builtin-0.1.0.tgz).
  Build outputs are intentionally ignored by Git; the package source and evidence
  remain versioned. See [Verification.md](Verification.md) to reproduce the package.

## Editor and package acceptance

| Check | Unity 2022.3.22f1 | Unity 6000.3.23f1 |
| --- | ---: | ---: |
| Full EditMode | 143/143 passed | 143/143 passed |
| Full graphics PlayMode | 727/727 passed | 727/727 passed |
| Final tarball package tests | 3/3 passed | 3/3 passed |
| Editor graphics API | Direct3D11 | Direct3D12 |

The canonical suites exercise all six original shader names and their public
property/editor contracts, shader compiler diagnostics, material setup, all four
Uber rendering paths, actual particle systems, mesh instancing, Canvas clipping
and stencil, depth, fog, lights, shadows, reflection controls, distortion,
optimization, migration backups/rollback, and sample generation.

The isolated package projects have no URP dependency. Their three tests verify
PackageCache installation, compile all six original shaders and all 48 optimizer
outputs, and import the sample through Package Manager with resolved materials
and five particle systems. Compile success is accompanied by the PlayMode and
standalone rendering results below.

Local NUnit XML and logs:

- `Logs/Unity/2022.3.22f1/{EditMode,PlayMode}.xml`
- `Logs/Unity/6000.3.23f1/{EditMode,PlayMode}.xml`
- `Logs/package-release-tests2022.xml` and `Logs/package-release-tests6.xml`
- `Logs/package-release-pack2022.log` and `Logs/package-release-install6.log`

Additional checks passed: shell environment regression script; 2 runner regression
tests; 3 release-documentation tests; 6 visual-comparator unit tests. The comparator's
negative test deliberately injects a Lit mismatch and requires failure.

## Fresh visual regression

Original NOVA 3.6.0 with URP 17.3.0 and the Built-in port were captured in isolated
Unity 6000.3.23f1 projects, both using Direct3D12 and Gamma color space. Each capture
fixture passed and produced 96 images: 24 configurations for each of four shaders.
No existing golden image or baseline was modified.

All 96 comparisons passed the same normalized thresholds:

- Mean RGB error at most 0.01.
- Maximum channel error at most 0.08.
- At most 1% of pixels having mean RGB error above 0.02.

| Capture group | Comparisons | Largest mean RGB error | Largest channel error | Largest bad-pixel fraction |
| --- | ---: | ---: | ---: | ---: |
| Particle/UI Unlit | 48 passed | 0 | 0 | 0 |
| Particle/UI Lit | 48 passed | 0.000994913 | 0.003921569 | 0 |

The Lit captures use a controlled diffuse dielectric with specular and reflections
disabled. They verify the common material stack; they do not establish equality
between Built-in Blinn-Phong and URP GGX lighting. Dedicated PlayMode rendering
assertions additionally cover normal/surface maps, metallic/specular workflows,
lights, shadows and reflections.

See the [per-image report](Validation/Visual/report.md),
[machine-readable metrics](Validation/Visual/metrics.json), and the 16 adjacent
contact sheets (original / Built-in / amplified difference). These are generated
validation evidence, not newly adopted golden images.

## Standalone execution

Both players were built from the final tarball identified above. Both builds
reported `Build Finished, Result: Success.` and both automated executions returned
exit code 0 on Direct3D11. Each player found the six original shader names, passed
four Uber color renders and distortion enable/disable checks, loaded the imported
showcase, simulated five particle systems, and checked its rendered image.

| Unity | Player result | Rendered sample |
| --- | --- | --- |
| 2022.3.22f1 | [PASS](Validation/Players/Unity2022.txt) | [960 x 640 frame](Validation/Players/Unity2022.png) |
| 6000.3.23f1 | [PASS](Validation/Players/Unity6.txt) | [960 x 640 frame](Validation/Players/Unity6.png) |

The repeatable automated path used `-batchmode -force-d3d11`, with graphics
enabled. The separate `-novaShowcase` branch also stayed running during a six-second
launch check without logged exceptions or shader errors; that check was explicitly
stopped after observation. The automated player captures above provide the image
checks and normal exit results.

Local build/run logs are `Logs/package-release-build2022.log`,
`Logs/package-release-build6.log`, `Logs/package-release-player2022.log`, and
`Logs/package-release-player6.log`. NUnit result attributes and source hashes are
preserved in [results.json](Validation/results.json).

A [Windows x64 demo ZIP](../Builds/Release/NovaShaderBuiltIn-Showcase-Windows-x64.zip)
contains the Unity 6 player, required runtime files, NOVA license and launcher.
Extract all files and run `LaunchShowcase.cmd`; close the window to finish.
ZIP integrity was checked. Size: 65,343,669 bytes. SHA-256:
`2ef26391214299761b41b351355846dadb14b2648ea8f09e19a95579b17342cb`.

## Remaining limits and handoff

Windows Direct3D11/12 are the verified graphics paths. Other graphics APIs, mobile,
Web and XR require their own platform acceptance. Lit uses the documented
Built-in lighting approximation; URP Forward+, probe volumes, probe blending/box
projection and renderer infrastructure have no exact equivalent. Distortion uses
mono camera image effects and opaque depth; stereo/reflection/preview cameras
bypass it. Migration supports the six original shader GUIDs in text materials.

Every approximation or unsupported feature has a reason and fallback in the
[matrix](BuiltInCompatibilityMatrix.md). Installation, camera setup, material
migration and running the sample are described in the [README](../README.md).

All release TODO items are complete for the documented scope. There is no next
required porting TODO. Additional platforms or closer URP lighting parity are
future extensions, not unreported acceptance failures. No branch was pushed or merged.

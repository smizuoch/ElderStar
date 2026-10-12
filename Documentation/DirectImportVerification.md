# Unconverted Fire001 acceptance — 2026-10-12

This page preserves the 0.1.1 acceptance evidence. Current 0.1.2 delivery and
additional validation are in [the settings/asset report](AssetCorpusVerification.md).

Version **0.1.1** imports the supplied Fire001 effect without changing its
materials, prefab, textures or scene. The previous 0.1.0 release required material
GUID conversion; its earlier parity report did not certify direct effect imports.

Install 0.1.1, choose **Assets > Import Package > NOVA Effect...**, select the
original `Fire001.unitypackage`, and click **Import**. Put
`Assets/N2Studio/Prefabs/Fire001.prefab` in a Built-in scene. No migration,
shader reassignment or material setup is needed.

This menu excludes the 63 bundled original NOVA implementation assets and imports
the nine effect files unchanged. Standard Unity Custom Package / double-click
import does not filter dependencies; deselect bundled original NOVA source when
using that route. See [the complete import instructions](Migration.md).

## Input and implementation

- Input SHA-256: `1bf7b7b503bf2dd5b648c306e21230fbdc41d13610961f0a75d65ea30d8a1063`.
- Reference: the actual NOVA source bundled in that package, with URP 17.3.0.
  It differs from the later 3.6 source in 25 files; comparisons use the bundled version.
- All six original NOVA shader GUIDs resolve to the original-named Built-in shaders.
- Six previous Built-in GUIDs remain valid via hidden compatibility shaders.
  Eight rendering comparisons check their opaque/transparent output against the
  canonical four Uber shaders. Existing files need no conversion.
- The import filter copies retained archive entries verbatim to Temp and uses
  Unity's importer. It has no material or prefab rewriting path.

The third-party effect and its original source are **not redistributed** in this
repository or the release tarball. [Metadata](Validation/Fire001/metadata.json)
records the original input and the nine unchanged asset hashes.

## Results

| Check | Unity 2022.3.22f1 | Unity 6000.3.23f1 |
| --- | ---: | ---: |
| Full EditMode | 425 passed | 425 passed |
| Full graphics PlayMode | 771 passed | 771 passed |
| Installed tarball tests | 12 passed | 12 passed |
| Actual Fire001 import from installed package | Passed; 9 unchanged files | Passed; 9 unchanged files |
| Fire001 rendering from installed package | 18 frames passed | 18 frames passed |

Installed-package tests cover the original three acceptance checks (all shaders,
48 optimized outputs and sample import), six original/legacy GUID cases, and three
effect-import checks including Unity's real selection dialog and stale-input rejection.
The projects have no URP package. Shader diagnostics accompany the rendering checks.

Original/port comparison uses Unity 6000.3.23f1, Windows Direct3D11, 384 x 384 pixels,
Gamma and Linear color spaces, six timestamps (0.25, 0.75, 1.5, 3, 5, 8 seconds),
and three views (0, 35, 80 degrees). Only instantiated particle random seeds are
fixed; original material files and shader keywords remain untouched.

**All 36 frames are pixel-identical**: mean RGB error, maximum channel error and
bad-pixel fraction are all zero. Particle positions, velocities, remaining lifetimes
and random seeds match at all timestamps. See [Gamma metrics](Validation/Fire001/gamma.json)
and [Linear metrics](Validation/Fire001/linear.json).

The comparator retains the existing mean <= 0.01, maximum <= 0.08 and bad-pixel
fraction <= 0.01 thresholds. It additionally rejects missing/blank captures and
different particle simulation. Ten comparator tests, four original-contract tests
and four documentation tests pass. Existing golden images were not modified.
Reproduction instructions are in [Tests/Visual](../Tests/Visual/README.md).

A Windows x64 player built with Unity 6000.3.23f1 from the installed 0.1.1 tarball
also renders the untouched prefab successfully on Direct3D11: exit code 0,
six particle systems, five enabled renderers and 4,726 visible fire pixels.
See [the player result](Validation/Fire001/player.txt). The editor build log reports
`Build Finished, Result: Success.` and the player has no shader/runtime errors.
This standalone check is Unity 6 only; Unity 2022 is covered by the graphics
PlayMode and installed-package effect checks above.

## Delivery

- Local archive: [jp.co.cyberagent.nova.builtin-0.1.1.tgz](../Builds/Release/jp.co.cyberagent.nova.builtin-0.1.1.tgz).
- 159,795 bytes; 212 files matched package source byte-for-byte.
- SHA-256: `b89c8cfacb593f523c0a5eedfd5a2cf20f14ea565617f7d05263cfcb3f14e86b`.
- Packed and installed through Unity Package Manager on both tested editor versions.
  Local build outputs are ignored by Git; source and numeric evidence are committed.

## Scope and remaining differences

This certifies the **Fire001 prefab**, with its five enabled particle renderers
and one disabled parent renderer, under matched camera/color-space settings.
The disabled parent retains an unused reference to URP's default particle material;
that missing Built-in dependency does not render and was not silently rewritten.

The supplied `N2Studio_Demo.unity` is a URP scene: it references additional camera
and light data and an SRP Volume. It is preserved as data, but its URP components
and post-processing are not reproduced by this shader port. Use the prefab in a
Built-in scene. This result also does not establish identical Lit/shadow/distortion
behavior for arbitrary effects; see the [compatibility matrix](BuiltInCompatibilityMatrix.md).

Next optional acceptance: additional third-party effects with custom/optimized
shaders and other target graphics APIs. Such assets need separate evidence.

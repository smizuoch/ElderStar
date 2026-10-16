# ElderStar

ElderStar is the display name of NovaShaderBuilt-in. Package ID, shader names,
GUIDs and public APIs remain unchanged for existing materials and projects.

Independent Built-in Render Pipeline port of CyberAgent NOVA Shader 3.6.0, under
the MIT license. Requires Unity 2022.3 or later and a Built-in project. No URP,
SRP Core or RenderGraph dependency is included.

Remove original NOVA before installation: shader names and public editor classes
deliberately match. Clear the Render Pipeline Asset in Graphics and all Quality levels.

Import **Built-in Showcase** in Package Manager, open `NovaShowcase.unity` and press
Play. Alternatively use **Tools > ElderStar > Built-in > Create Showcase Scene**.

Select `Nova/Particles/UberUnlit`, `Nova/Particles/UberLit`,
`Nova/UIParticles/UberUnlit`, `Nova/UIParticles/UberLit` or
`Nova/Particles/Distortion` in a material's Shader menu. The original NOVA
inspectors configure keywords and particle vertex streams.

Soft particles need a camera depth texture. Distance-based depth fade uses eye
distance and does not need scene depth. Distortion works without a camera component
using a named GrabPass, including on PC VRChat avatars. For an ordinary mono
project, **ElderStar > Screen Space Distortion** optionally provides the accumulated
camera compositor and enables camera depth automatically. See the documented
differences for overlapping distortion, scene depth and stereo validation.

Original NOVA shader GUIDs resolve directly; material conversion is unnecessary.
For effect packages that bundle original NOVA source, use **Assets > Import Package >
ElderStar Effect...**, select the original unitypackage and click Import. This retains
the effect assets unchanged and omits conflicting bundled NOVA dependencies.
Ordinary Unity import requires deselecting those bundled dependencies manually.
Previous Built-in GUIDs remain usable through hidden shader aliases. See
[import instructions](Documentation~/Migration.md) and [compatibility](Documentation~/Compatibility.md).

Lit preserves the original GGX and environment Fresnel/roughness response.
Version 0.1.6 restores glossy ambient with reflection maps disabled and corrects
dark-probe and Gamma/HDR handling; see [ambient lighting](Documentation~/AmbientLighting.md).
First import and color-space changes automatically match original intensity/color-
temperature defaults for all project lights. Later manual edits survive reloads.
Use **Tools > ElderStar > Built-in > Automatic Initial Lighting Setup** to opt out,
or **Match Original Lighting Settings** to reapply with Undo support.

Optional normal-Unity helpers add [Canvas lighting](Documentation~/CanvasLighting.md),
[a dedicated depth-normal buffer](Documentation~/DepthNormals.md) and
[explicit distortion commands](Documentation~/BuiltInRenderingAPI.md).
The distortion API also restores original pass constructors, target setup and
context reset using real Built-in resources; engine inheritance remains different.
Lit can consume Built-in LPPV, and Editor-only diagnostics expose eight views;
see [probe volumes and diagnostics](Documentation~/ProbeVolumesAndDiagnostics.md).
The shader-only VRChat path needs none of these components. Tiled Forward+,
APV formats/streaming and URP C# types are not implemented. Original NOVA also
omits reflection-probe blending/box-projection variants.

Windows Direct3D11/12 is the verified platform scope, on Unity 2022.3.22f1 and
6000.3.23f1. Other graphics APIs require their own platform validation.

Additional MIT/CC0 sample comparisons expose a remaining transparent-sort tie in
the original heal02 prefab; distinct renderer sorting orders give a deterministic
fallback. The official sample pfb_eff_fire crashes natively in both pipelines on
6000.3.23f1; use the verified 2022.3.22f1 for that sample. These limits do not affect
the separate supplied N2Studio Fire001 acceptance. See the compatibility matrix.

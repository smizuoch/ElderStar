# NOVA Shader Built-in

Independent Built-in Render Pipeline port of CyberAgent NOVA Shader 3.6.0, under
the MIT license. Requires Unity 2022.3 or later and a Built-in project. No URP,
SRP Core or RenderGraph dependency is included.

Remove original NOVA before installation: shader names and public editor classes
deliberately match. Clear the Render Pipeline Asset in Graphics and all Quality levels.

Import **Built-in Showcase** in Package Manager, open `NovaShowcase.unity` and press
Play. Alternatively use **Tools > NOVA > Built-in > Create Showcase Scene**.

Select `Nova/Particles/UberUnlit`, `Nova/Particles/UberLit`,
`Nova/UIParticles/UberUnlit`, `Nova/UIParticles/UberLit` or
`Nova/Particles/Distortion` in a material's Shader menu. The original NOVA
inspectors configure keywords and particle vertex streams.

Enable `DepthTextureMode.Depth` for soft particles and depth fade. Add **NOVA >
Screen Space Distortion** to each distortion camera, before other image effects;
it enables depth automatically. Mono game cameras are supported; XR stereo and
reflection/preview cameras bypass this effect.

Original NOVA shader GUIDs resolve directly; material conversion is unnecessary.
For effect packages that bundle original NOVA source, use **Assets > Import Package >
NOVA Effect...**, select the original unitypackage and click Import. This retains
the effect assets unchanged and omits conflicting bundled NOVA dependencies.
Ordinary Unity import requires deselecting those bundled dependencies manually.
Previous Built-in GUIDs remain usable through hidden shader aliases. See
[import instructions](Documentation~/Migration.md) and [compatibility](Documentation~/Compatibility.md).

Lit preserves the original GGX and environment Fresnel/roughness response. Use
**Tools > NOVA > Built-in > Match Original Lighting Settings** to preserve original
light intensities in Linear projects. This explicit, undoable action affects all
project lights; importing the package does not change Graphics Settings.

Forward+, probe volumes, URP DepthNormals/debug and URP foveated rendering are not
reproduced. The first Built-in reflection probe, forward lights and camera depth
provide the documented fallbacks. Original NOVA also omits reflection-probe
blending/box-projection variants.

Windows Direct3D11/12 is the verified platform scope, on Unity 2022.3.22f1 and
6000.3.23f1. Other graphics APIs require their own platform validation.

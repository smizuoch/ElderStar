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

Use **Tools > NOVA > Built-in > Material Migration** for a read-only preview and
backed-up conversion of original text material GUIDs. See
[migration](Documentation~/Migration.md) and [compatibility](Documentation~/Compatibility.md)
for conversion steps and rendering differences.

Lit uses a normalized Blinn-Phong approximation instead of URP GGX. Reflection
probe blending/box projection, Forward+, probe volumes, URP DepthNormals/debug
and URP foveated rendering are not reproduced. The first Built-in reflection
probe, forward lights and normal Built-in camera depth provide the documented fallbacks.

Windows Direct3D11/12 is the verified platform scope, on Unity 2022.3.22f1 and
6000.3.23f1. Other graphics APIs require their own platform validation.

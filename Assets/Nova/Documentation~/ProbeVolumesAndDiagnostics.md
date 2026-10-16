# Probe volumes and Editor diagnostics

## Spatial indirect lighting

Both Lit shaders use Built-in Light Probe Proxy Volumes when the renderer's
`unity_ProbeVolumeParams.x` enables them. The spatial L0/L1 sample replaces the
ordinary SH L0/L1 term; renderer L2 remains. It is not added over ordinary SH,
and the flat-ambient floor is not added over the proxy result. Without an enabled
proxy the previous ordinary-SH path remains. The proxy path adds three 3D texture
samples only while enabled; it adds no pass or screen copy.

Configure Unity's Light Probe Proxy Volume and the renderer's light-probe usage
normally. This consumes Unity's native buffer/texture bindings without NOVA runtime
C# on the renderer. A VRChat avatar cannot require arbitrary worlds to supply
an LPPV. APV files, adaptive placement, streaming and scenario switching are not
implemented; ordinary light probes remain the fallback.

Eight GPU tests exercise both Lit shaders with local/world coordinates, spatial
red/green SH data and ordinary blue SH. They verify enabled spatial variation,
disabled fallback and absence of double L0/L1 contribution. These deterministic
buffer tests are not an APV importer or a light-baking test.

## Diagnostics

Open **Tools / ElderStar / Rendering Diagnostics** for Scene View modes: Albedo,
world Normal, world Tangent, linear eye Depth / far clip, Fog visibility,
Direct light, Indirect light and Additional light count / 8. The Canvas count is
its complete selected direct-light list; MeshRenderer counts contributing
ForwardAdd passes. Unlit direct/indirect views are black. Diagnostics retain the
material's blending and clipping, so overlapping transparent surfaces can blend
the diagnostic values too.

Editor scripts can call `NovaShaderDiagnostics.Render(Camera, NovaDebugView)`.
The scoped call restores the prior global keyword and mode even on failure and
never changes a source material. Normal variants have no diagnostic calculations.
Diagnostic-only variants target SM4 and are stripped from Player and AssetBundle
builds, including generated `Hidden/Nova/` shaders. Original `DEBUG_DISPLAY`
declarations remain separate. There is no URP debug-renderer API facade.

32 GPU view assertions and two actual AssetBundle stripping tests pass on Unity
2022. Full Unity 2022/6 results and costs are in LowCostLightingParity.md.

Reference: [Unity LPPV](https://docs.unity3d.com/2022.3/Documentation/Manual/class-LightProbeProxyVolume.html).

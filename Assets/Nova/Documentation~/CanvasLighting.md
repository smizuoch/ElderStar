# Optional Canvas lighting

Add **NOVA / Canvas Lighting** to an ordinary Unity Canvas using `Nova/UIParticles/UberLit`. Use **Collect Scene Lights** in the Inspector to capture light references at authoring time. Runtime-created lights can use `RegisterLight(Light)` / `UnregisterLight(Light)`; there is no per-frame scene search.

The selected list supplies all direct lighting for those Graphics, including the directional key light, to avoid adding the same main light twice. The default limit is four lights per Graphic; `MaximumLights` is clamped to 0–8. The provider filters disabled/inactive lights, range, spot direction and culling layers, and chooses the largest estimated contributions using each Graphic's bounds. The Inspector shows the largest omitted count. Directional, Point and Spot use NOVA's BRDF and URP-style distance/spot attenuation in one color pass. Shadows and light cookies are deliberately not sampled by this optional list.

The provider uses uGUI's Graphic registry and `IMaterialModifier`. It creates an owned drawing material after the existing modifiers, preserving stencil, Mask, RectMask2D, CanvasGroup and ordering. It never edits the source material. Disabling the provider releases its drawing materials and temporary modifiers. Shared source materials can be used by independent Canvases. Normal/tangent Canvas shader channels are enabled when the component is enabled.

World Space Canvas can schedule ForwardAdd. The owned drawing material explicitly disables that pass, so selected lights are evaluated once and unregistered scene lights cannot leak into the result. An unregister regression failed in eight rendering conditions before this fix; all 49 Canvas/mask tests pass afterward on Unity 2022.

`Nova.UI` and `Nova.UI.Editor` compile only when `com.unity.ugui` is installed; it is not added as a package dependency. The provider supports the original UI Lit shader. Mesh/particle renderers continue to use ForwardBase/ForwardAdd. Custom or generated UI shader names require their own integration.

This component is for normal Unity scenes. VRChat avatars use the allowed MeshRenderer/SkinnedMeshRenderer/ParticleSystemRenderer path with UI NOVA shaders, and require no NOVA runtime component. Native avatar Canvas and custom scripts are not covered by the VRChat component allowlist.

## Verification

Unity 2022.3.22f1 and 6000.3.23f1: 43 Canvas lighting tests plus six Mask/RectMask2D tests passed. They cover 0/1/4/8/10 lights, both 4/8 limits, disabled and out-of-cone lights, layer changes, material regeneration, CanvasGroup and independent Canvases/reparenting. World Space and Screen Space Camera have actual pixel assertions. Overlay checks the material/selection/cleanup contract; it is not an offscreen pixel comparison. The whole PlayMode suite passes 929 cases on each editor at this checkpoint. Performance acceptance is recorded separately in `LowCostLightingParity.md`.

References: [IMaterialModifier](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.UI.IMaterialModifier.html), [VRChat allowed avatar components](https://creators.vrchat.com/avatars/whitelisted-avatar-components/whitelisted-avatar-components/).

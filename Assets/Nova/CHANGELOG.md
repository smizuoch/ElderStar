# Changelog

## Unreleased

- Restore the original pipeline-independent public deprecation-warning type and constant.

- Apply multiply blending to Lit albedo before lighting, preserving the original alpha-zero behavior.
- Normalize random-row Custom Data progress before clamping, including values outside 0..1.
- Preserve the sign of back-facing parallax offsets, retaining the near-singular denominator guard.
- Match URP 17 PointClamp camera-depth sampling on Unity 6, including stereo arrays.
- Approximate original forced 9x camera-distortion anisotropy with the nearest supported inline sampler (8x); other quality settings retain ordinary sampling.
- Add reproducible licensed-texture/API comparisons: 3,328 image pairs pass unchanged thresholds; distinguish declaration coverage, exercised behavior and remaining approximations.

## 0.1.3

- Carry the stereo eye index through particle color, depth and shadow passes, including per-eye depth texture sampling.
- Add shader-only distortion for PC avatars through a named GrabPass; no camera component is needed. Keep the existing mono camera compositor for ordinary projects.
- Use queue 3100 for fresh distortion materials so background capture occurs after transparents. Explicit material queues remain unchanged.
- Hide distortion geometry when VRChat applies its shader fallback.
- Pack Lit world normals into unused UV components so stereo shadowed additional lights compile within the original SM3.5 vertex-output limit.
- Limit UI Lit legacy URP compatibility keywords to local fragment variants, matching particle Lit and avoiding unused global/vertex permutations.
- Add real SDK bundle and AAO comparison fixtures, with shader support, material-state, vertex-stream and rendered-image checks.

## 0.1.2

- Match original light intensity/color-temperature defaults on first import and color-space changes; preserve later manual edits and provide a project-local opt-out.
- Save only GraphicsSettings when applying lighting defaults, leaving unrelated unsaved assets untouched.
- Restore original fresh material render queues and Inspector preview/projector tags.
- Restore Base Map rotation, pivots and Custom Data UV offsets on all four Uber shaders, including applicable depth/shadow passes.
- Use selected array/volume texture transforms in Lit depth and shadow alpha tests.
- Restore Custom Data flipbook progress and random-row selection in Lit depth and shadow passes.
- Validate untouched MIT NOVA samples and 96 CC0 Kenney textures against original NOVA; document transparent sort ties and a Unity 6 native particle crash shared by both pipelines.

## 0.1.1

- Resolve original NOVA shader GUIDs without rewriting imported material or prefab assets.
- Retain previous Built-in GUIDs through hidden compatibility shaders; keep optional normalization with backups.
- Import unchanged effect packages while omitting bundled original NOVA implementation dependencies.
- Verify Fire001 against its bundled original shader with deterministic particle simulation and pixel comparisons.

## 0.1.0

- Match all 889 original property declarations and 525 public/protected editor declarations; restore foldout icons and legacy material-state transitions.
- Preserve original opaque optimizer keyword reduction and Assets-root output; verify Inspector drawing, multiple editing, Undo/Redo and shader reassignment.
- Match original GGX and environment Fresnel/roughness response in controlled Gamma/Linear captures; add opt-in original lighting settings with Undo.
- Port all six original shader names, material properties and public editor APIs to Built-in.
- Implement common particle features, lit forward shading, UI masks, particle instancing and camera distortion.
- Generate and replace all 48 optimized particle shader names.
- Add preview-based material migration with backups and a playable packaged showcase.
- Verify both supported editor versions with shader diagnostics, rendering tests, isolated package import and standalone player fixtures.
- Document Built-in lighting, depth, camera and platform differences explicitly.

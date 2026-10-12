# Changelog

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

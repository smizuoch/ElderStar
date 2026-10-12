# Changelog

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

# Migrating original NOVA materials

Use a version-controlled copy of the project. Remove the original NOVA package
before installing this port: their assembly names, shader names and public editor
classes deliberately match. Set the project and every Quality level to Built-in
Rendering by clearing the Render Pipeline Asset. URP renderer features cannot be
attached to Built-in cameras.

## Use original effects without material conversion

The six original NOVA 3.6 shader GUIDs now resolve directly to the Built-in
implementation. Original materials (including embedded/binary references) need
no migration, shader reassignment, keyword setup or reserialization when they
reference those shaders. Material values and prefab/texture references stay intact.

For effect `.unitypackage` files such as **Fire001.unitypackage**, choose
**Assets > Import Package > NOVA Effect...** (also available under **Tools > NOVA >
Built-in > Import Effect Package...**), select the original downloaded file, and
click **Import** in Unity's normal selection dialog. Drag its prefab into a scene.
There is no conversion step and no replacement package for the user to prepare.

Some effects bundle the original NOVA implementation. The importer recognizes
its asset GUIDs and omits those dependencies, using the installed Built-in
implementation instead. It copies all retained archive entries byte-for-byte
into a temporary archive under `Temp/NovaEffectImports`; Unity performs the normal
import and overwrite handling. It does not rewrite materials, prefabs, textures
or the downloaded file. Modified source with an original NOVA GUID is also
considered a bundled dependency; custom shader forks with new GUIDs are retained.

**The ordinary Custom Package / double-click import does not apply this filter.**
When using that route, deselect the bundled original NOVA source folder. Importing
all bundled URP scripts alongside the Built-in port causes compilation conflicts.
Packages without bundled NOVA code can use normal Unity import directly. This
importer accepts normal GUID-based unitypackage archives with assets under Assets;
it rejects nonstandard archive entries or paths outside Assets before importing.

Arbitrary optimized/custom shader GUIDs, URP-only scripts in the effect itself,
renderer features and unrelated URP materials are not automatically ported.
Regenerate optimized shaders using the Built-in generator, or port such custom
dependencies. A NOVA package cannot reproduce arbitrary URP scenes/post-processing.

## Previously released Built-in materials

The six old 0.1.0 Built-in GUIDs are retained as hidden compatibility shaders that
reuse the canonical passes and original property declarations. Existing material
files continue rendering without a conversion. Their Shader field displays
`Hidden/Nova/LegacyBuiltIn/...`; newly imported original effects use the original
`Nova/...` names. To normalize old aliases before using tools that identify shader
names, optionally use **Tools > NOVA > Built-in > Material Migration**: choose an
Assets folder, Preview, then Apply. This now maps old Built-in GUIDs to the original
canonical GUIDs. Original NOVA materials are skipped because they already work.

The optional normalizer preserves all other bytes and stores backups under
`Library/NovaBuiltInMigration/<timestamp-id>/Assets/...`. A changed material
invalidates the plan before any write. To revert, close Unity and restore the
backed-up `.mat` files while retaining their `.meta` files. Keep backups outside
Library if needed across cache deletion. Binary/embedded legacy materials remain
usable through the aliases; this optional text normalizer skips them.

For soft particles, enable `DepthTextureMode.Depth` on an ordinary game camera.
The distance-based depth-fade control does not need a scene depth texture.
Distortion now renders directly through a named GrabPass, with no camera script.
Its default queue is 3100 so the background exists before capture. Original material
values, textures and GUIDs remain; an explicit custom queue is still respected and
must be placed after the background. This queue difference is necessary because
Built-in has no URP renderer feature to schedule the effect separately.

For the accumulated, camera-wide distortion approximation in ordinary projects,
add **NOVA > Screen Space Distortion** to a mono game camera. It enables depth and
suppresses the shader-only surface pass while compositing. Put it before other image
effects. Camera layers and opaque depth participation still apply.

For **VRChat PC avatars**, attach particle renderers/materials to the avatar without
the camera component or other NOVA runtime scripts. The SDK does not allow arbitrary
avatar scripts. Soft particles depend on the current world's depth texture; the
avatar cannot force that world setting. Shader-only distortion samples the captured
scene per surface, so overlapping distortions do not accumulate exactly like URP.
Custom shaders/particles can also be hidden by viewers' VRChat Safety settings.
See the official [allowed components](https://creators.vrchat.com/avatars/whitelisted-avatar-components/whitelisted-avatar-components/)
and [shader fallback](https://creators.vrchat.com/avatars/shader-fallback-system/) references.

VRChat SDK 3.10.5 requires mipmapped avatar textures to enable **Streaming Mip Maps**.
This is a Texture Importer setting, not image/material conversion. The validation
fixture backs up changed `.meta` files before enabling it on avatar dependencies.
Use the SDK's validation and Build & Test on each target avatar; installing NOVA
does not certify unrelated avatar assets, world depth, network behavior or PCVR hardware.

After import, inspect lighting and blending in the target scene. Lit shading,
reflection probes, shadows, fog and camera distortion use the documented Built-in
approximations in [the compatibility matrix](Compatibility.md).
In particular, a fully metallic or white-specular material with both reflection
options disabled has no diffuse light, as in the original BRDF workflow.

The first import automatically applies the original light intensity defaults:
linear intensity in Linear color space, gamma intensity in Gamma color space,
and color temperature enabled. This affects all project lights, including other
materials. The settings persist into standalone builds. The setup runs again
after changing color space, but does not overwrite later manual edits or Undo
on ordinary reloads. It does not change the project's color space itself.

The project-local **Tools > NOVA > Built-in > Automatic Initial Lighting Setup**
checkbox opts out; the state lives in `ProjectSettings/NovaBuiltInSettings.asset`
and can be versioned with the project. **Match Original Lighting Settings**
remains an explicit, undoable reapply action. No material conversion is involved.
See [Unity's light-intensity API documentation](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Rendering.GraphicsSettings-lightsUseLinearIntensity.html).

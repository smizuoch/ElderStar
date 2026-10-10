# Migrating original NOVA materials

Use a version-controlled copy of the project. Remove the original NOVA package
before installing this port: their assembly names, shader names and public editor
classes deliberately match. Set the project and every Quality level to Built-in
Rendering by clearing the Render Pipeline Asset. URP renderer features cannot be
attached to Built-in cameras.

Open **Tools > NOVA > Built-in > Material Migration**. Choose the Assets folder
containing the original materials, click **Preview**, review the listed files,
then **Apply preview and create backups**. The tool recognizes the six original
NOVA 3.6.0 shader GUIDs, including materials whose shader became missing after
removing the original package. It changes only that shader reference. Material
properties, texture references, keywords, render queues, material GUIDs, encoding
and line endings stay intact. Repeating the preview after migration is harmless.

The window displays the backup directory under
`Library/NovaBuiltInMigration/<timestamp-id>/Assets/...`. To revert, close Unity
and copy the desired original `.mat` files back to the corresponding Assets paths;
leave their `.meta` files untouched. Keep backups outside Library if they must
survive clearing the project cache. A material changed after preview invalidates
the entire plan before any material is written; run Preview again.

The tool reports and skips non-text material files. Convert those to text
serialization in the original project first. It does not follow linked folders
or migrate embedded model materials, scene/prefab-embedded materials, or arbitrary
generated optimized shader GUIDs. For those, extract a material asset if necessary,
choose the corresponding original-named Built-in NOVA shader in the Inspector,
then regenerate optimized shaders with the port's generator. Keep the material
asset itself so scene and prefab references remain valid.

For soft particles and depth fade, enable `DepthTextureMode.Depth` on the camera.
For distortion, add **NOVA > Screen Space Distortion** to each mono game camera;
it enables depth automatically. Place it before other image effects in the camera's
component order. Camera layers must include the distortion renderer and the opaque
background must participate in the depth texture. Transparent depth is not used.

After conversion, inspect lighting and blending in the target scene. Lit shading,
reflection probes, shadows, fog and camera distortion use the documented Built-in
approximations in [the compatibility matrix](BuiltInCompatibilityMatrix.md).
In particular, a fully metallic or white-specular material with both reflection
options disabled has no diffuse light, as in the original BRDF workflow.

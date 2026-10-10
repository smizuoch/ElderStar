# Built-in Showcase

Open `NovaShowcase.unity` in a Built-in Render Pipeline project and press Play.
The four colored particle systems use particle and UI UberUnlit/UberLit shaders.
Moving distortion particles warp the lower grid. Toggle **Screen Space Distortion**
on the camera to compare with the undistorted background.

All textures and materials are included. No URP, external images, scripts or UGUI
components are needed. The sample's UI shaders are on particle meshes; real Canvas
Mask/RectMask2D support is verified by the project's dedicated rendering tests.

Use **Tools > NOVA > Built-in > Create Showcase Scene** to generate another copy
in your own folder. Save untitled scenes first when Unity prompts. Existing scenes
and sample asset folders are not overwritten.

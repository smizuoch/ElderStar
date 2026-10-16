# ElderStar Demo Scene

**Docs** ([English](../../README.md), [日本語](../../README_JA.md)) | **Samples** ([English](../Samples/README.md), [日本語](../Samples/README_JA.md)) | **Demo** ([English](README.md), [日本語](README_JA.md))

## Overview

The bundled **Built-in Showcase** is a playable scene for checking ElderStar's basic rendering.
Four colored particle systems demonstrate Particle Uber Unlit/Lit and UI Uber Unlit/Lit, while moving distortion particles warp the lower grid.
Textures and materials are included, so you can run it without downloading additional effect assets or installing URP.

<p align="center">
  <img width="90%" src="../Images/Showcase.png" alt="The included Built-in Showcase rendered after importing the ElderStar unitypackage"><br>
  <sub>The included scene in Unity 2022.3.22f1, captured during package installation verification.</sub>
</p>

## How to Use

1. [Install ElderStar](../../README.md#setup) in a Built-in project.
2. If installed through Package Manager, open ElderStar's **Samples** section and import **Built-in Showcase**. Open `NovaShowcase.unity` from the imported sample folder under `Assets/Samples`.
3. If installed from the `.unitypackage`, open `Assets/Nova/Samples/BuiltInShowcase/NovaShowcase.unity`.
4. Press Play. Check the four particle panels and the distorted grid below them.

Toggle **Screen Space Distortion** on the scene's camera to compare camera composition with the undistorted background.
The sample's UI shaders are applied to particle meshes; it does not demonstrate a real Canvas or its masking.
For Canvas setup, see the [uGUI guide](../../README.md#ugui).

## Create a Separate Demo Scene

Choose **`Tools > ElderStar > Built-in > Create Showcase Scene`** and select a new destination.
Save an untitled scene first if Unity prompts. The generator does not overwrite existing scenes or sample asset folders.

This is ElderStar's Built-in demo. For the barrier, ice and other authored effect examples, see [Samples](../Samples/README.md).
Rendering depends on scene lighting and camera inputs; see [compatibility and verification](../../README.md#compatibility) for the tested scope.

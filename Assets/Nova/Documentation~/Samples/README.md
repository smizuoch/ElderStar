# ElderStar Sample Effects

**Docs** ([English](../../README.md), [日本語](../../README_JA.md)) | **Samples** ([English](README.md), [日本語](README_JA.md)) | **Demo** ([English](../Demo/README.md), [日本語](../Demo/README_JA.md))

## Overview

NOVA's sample assets demonstrate effects made by combining the shader's features.
The following examples were rendered and captured with ElderStar in a Built-in project.
These effect assets are available from the upstream repository; the ElderStar package includes a separate [Built-in Showcase demo](../Demo/README.md).

<p align="center">
  <img width="80%" src="../Images/barrier.gif" alt="Barrier sample rendered with ElderStar"><br>
  <sub>pfb_eff_barrier01</sub>
</p>
<p align="center">
  <img width="80%" src="../Images/ice.gif" alt="Ice sample rendered with ElderStar"><br>
  <sub>pfb_eff_ice02</sub>
</p>
<p align="center">
  <img width="49%" src="../Images/slash.gif" alt="Slash sample rendered with ElderStar">
  <img width="49%" src="../Images/heal.gif" alt="Healing sample rendered with ElderStar"><br>
  <sub>pfb_eff_slash01 / pfb_eff_heal01</sub>
</p>

## How to Use

1. [Install ElderStar](../../README.md#setup) in a Built-in project.
2. Download or clone the [NOVA source revision used for these captures](https://github.com/CyberAgentGameEntertainment/NovaShader/tree/1c6b88729f91c852c164ef71a32cc2bce83a1892) outside your Unity project.
3. Copy its `Assets/Samples` folder, including `.meta` files, into your project's `Assets` folder. Keep the original `Assets/Nova` implementation out of this project.
4. Place an effect prefab from the imported samples in your Built-in scene and press Play. The standard NOVA material references resolve to ElderStar without conversion or shader reassignment.

For an effect distributed as a `.unitypackage`, use **`Assets > Import Package > ElderStar Effect...`** instead. This imports the effect while excluding any bundled original NOVA implementation.

The upstream scenes' URP rendering configuration is separate from their effect assets. Use a Built-in camera and configure lighting, reflections and camera depth for your scene.
Soft Particles require a depth texture. Bloom and other post-processing effects are not added by the shader.
See [depth and distortion setup](../../README.md#usage) and [compatibility](../../README.md#compatibility) for details.

## Author and Capture Conditions

The sample effects were created by [@Ugokashiya](https://twitter.com/Ugokashiya) and published in CyberAgent's NOVA Shader repository under the MIT License.
The images above are our ElderStar captures, with fixed particle seeds and time in Unity 2022.3.22f1 / Direct3D 11 / Gamma.
They are examples of rendering, not a guarantee of identical appearance in every scene.
See [image credits and capture conditions](../Images/README.md).

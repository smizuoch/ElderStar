# Image Credits and Capture Conditions

[English](README.md) | [日本語](README_JA.md)

The example effects and Inspector images were captured with ElderStar.
Images and GIFs from the upstream README have not been reused as ElderStar screenshots.

## Example Effects

The MIT-licensed [NOVA Shader samples](https://github.com/CyberAgentGameEntertainment/NovaShader/tree/1c6b88729f91c852c164ef71a32cc2bce83a1892/Assets/Samples)
are rendered using ElderStar's Built-in shaders.
The effects were created by [@Ugokashiya](https://twitter.com/Ugokashiya) and published by CyberAgent, Inc.

| Image | Source prefab |
| --- | --- |
| barrier.gif | pfb_eff_barrier01 |
| ice.gif | pfb_eff_ice02 |
| slash.gif | pfb_eff_slash01 |
| heal.gif | pfb_eff_heal01 |

Captured in Unity 2022.3.22f1 on Windows / Direct3D 11, with Gamma color space.
A camera renders to a RenderTexture at fixed particle seeds and times; the PNG frames are then encoded as GIFs.
GIF color and frame timing limits apply. These images are not visual regression baselines.
The background, camera and lighting are presentation settings; no post-processing such as Bloom is applied.
Camera depth is enabled, and reflections use the upstream sample's cubemap.

## Inspector

The actual ElderStar Inspector drawing code is shown section by section in a capture EditorWindow,
and the Unity window is captured. These are not redrawn UI mockups.
The settings illustrate available controls; not every image represents the defaults.

The capture script is [Scripts/Unity/ElderStarReadmeCapture.cs](https://github.com/smizuoch/ElderStar/blob/main/Scripts/Unity/ElderStarReadmeCapture.cs).

## Bundled Demo

`Showcase.png` is the Built-in Showcase rendered after installing the actual `.unitypackage` in a clean Unity 2022.3.22f1 project.
It is copied unchanged from the [distribution verification capture](https://github.com/smizuoch/ElderStar/blob/main/Documentation/Verification/Distribution20261016/assets2022.png).
The scene retains its original NOVA title; the shaders are ElderStar's Built-in implementation.

Sources, capture methods and SHA-256 hashes are recorded per image in [captures.json](captures.json).
See [LICENSE.md](../../LICENSE.md) for the original license.

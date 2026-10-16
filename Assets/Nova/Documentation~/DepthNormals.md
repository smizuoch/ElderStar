# Optional NOVA depth normals

Particle UberLit and UberUnlit have a real `DepthNormals` pass. The optimizer
retains/removes it according to `OptionalShaderPass.DepthNormals`, including
normal-map texture-mode variants on opaque materials. Original shader names and
legacy Built-in aliases remain usable.

For ordinary Built-in Unity cameras, add **ElderStar / Depth Normals** and register
renderers with `RegisterRenderer`, or collect them once through the component's
context menu. Runtime-created renderers use `RegisterRenderer` /
`UnregisterRenderer`; the render loop does not search the scene.

`NovaDepthNormals.Texture` is an owned ARGBHalf texture, RGB = world normal * 0.5
+ 0.5, A = valid coverage. Its separate 24-bit depth attachment resolves selected
NOVA surfaces; the ordinary camera depth rejects occluded fragments. Transparent
world geometry absent from camera depth cannot provide that occlusion. The camera's
global depth-normal texture is never replaced. The camera depth flag remains set
on disable because other effects may share it.

The texture is created only when rendering, reused across manual camera renders,
resized with the target and released when the helper is disabled/destroyed or the
camera changes from enabled to disabled. A disabled camera explicitly rendered
with `Camera.Render()` can allocate it again. A call to `Texture` before the first
render returns null. Consumers must not retain a released texture.

This is not the URP prepass contract: original NOVA outputs raw world normals,
whereas this dedicated buffer encodes normals and coverage. Original alpha, UV,
deformation, Custom Data and normal flipbook behavior are preserved, including
the original opaque/alpha-tested differences. The helper is optional and adds
one draw per selected material/submesh plus its own render target. It belongs in
normal Unity scenes, not VRChat avatars. Merely installing the shader adds no
camera pass.

## Verification

`NovaDepthNormalsTests`: seven actual rendering/lifecycle cases, plus two stereo
array cases in `NovaBuiltInStereoTests`. The original-versus-port capture fixture
`Tests/Visual/NovaDepthNormalsCapture.cs` covers 64 cases per Unity version:
two shaders, opaque/cutout, positive/negative nonuniform scale, normal maps,
arrays, legacy/current flow flags, channel selection, deformation and Custom Data.
The initial port failed 12 of 64; the corrected implementation passes all 64 on
2022.3.22f1 and all 64 on 6000.3.23f1, with the existing image thresholds unchanged.
These explicit stereo-buffer tests are not a physical-headset camera test.

See [Unity camera depth textures](https://docs.unity3d.com/2022.3/Documentation/Manual/SL-CameraDepthTexture.html).

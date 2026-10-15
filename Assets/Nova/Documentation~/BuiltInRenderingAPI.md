# Built-in rendering APIs

These APIs use Unity Built-in types. They are optional normal-Unity camera helpers,
not VRChat avatar components. Avatar distortion uses the shader-only path.
Original NOVA class names and pipeline-independent call signatures are restored
where they can perform real work without a pipeline dependency. Their base types
and pipeline callbacks remain different from URP.

## Distortion

`Nova.Runtime.Core.Scripts.ScreenSpaceDistortion.Create()` remains available. The existing component owns and executes a `BuiltInDistortionPass`. Its camera visibility, depth, signed-offset accumulation, anisotropy and fallback behavior are unchanged.

A caller can own the two stages explicitly:

```csharp
using Nova.Runtime.Core.Scripts;
using UnityEngine.Rendering;

// Keep this object alive while any recorded command buffer refers to its material.
var distortion = new BuiltInDistortionPass();
var commands = new CommandBuffer { name = "Custom NOVA distortion" };
distortion.RecordOffsets(commands, camera, offsetTexture, registeredRenderers);
distortion.RecordComposite(commands, sourceTexture, destinationTexture, offsetTexture);
Graphics.ExecuteCommandBuffer(commands);
commands.Dispose();
distortion.Dispose();
```

`RecordOffsets` clears a caller-owned linear RGHalf/ARGBHalf 2D target to neutral (0.5, 0.5), filters the explicit renderer list by visibility, layer, frustum and pass enablement, and draws `DistortedUvBuffer`. It does not search the scene. The caller supplies camera depth for depth-aware materials and restores render-target/view/projection state when recording further geometry. Record after the source camera has supplied its depth/global camera inputs. XR and reflection cameras use shader-only distortion; this explicit mono API rejects those cameras and active SRPs.

`RecordComposite` records texture bindings into the command buffer, so recording a second camera cannot mutate the first camera's inputs. Source and destination must be different textures. A null destination targets the backbuffer. Global texture/anisotropy values are restored to their values at **record time**; callers with their own deferred global-state changes should establish those bindings after this call. Keep all textures and renderers alive until GPU use completes. Neither stage owns the supplied textures or command buffer.

The default camera component retains its automatic renderer discovery for compatibility. Custom integrations can retain a renderer list and use the explicit API to avoid that search.

## Original pass names

The following original call forms are available in `Nova.Runtime.Core.Scripts`:

```csharp
var offsets = new DistortedUvBufferPass("DistortedUvBuffer");
var apply = new ApplyDistortionPass(true, Shader.Find("Hidden/Nova/Particles/ApplyDistortion"));
apply.Setup(new RenderTargetIdentifier(offsetTexture));
```

`DistortedUvBufferPass` selects the actual **LightMode tag**, including when its
pass Name differs. Its Built-in target setup uses a `RenderTexture`:

```csharp
offsets.Setup(offsetTexture);
offsets.Execute(commands, camera, registeredRenderers);
apply.Execute(commands, camera, sourceTexture, destinationTexture);
Graphics.ExecuteCommandBuffer(commands);
// Dispose only after any recorded commands using these objects have completed.
offsets.Dispose();
apply.Dispose();
```

`ApplyDistortionPass.Setup(RenderTargetIdentifier)` also accepts a target obtained
with `CommandBuffer.GetTemporaryRT`. Allocate it before the recorded composite and
release it afterwards. Reconfiguring the pass does not change commands already
recorded. With `applyToSceneView: false`, Execute copies the source unchanged for
a SceneView camera; a separate destination is thus initialized correctly.

Both passes reject missing setup and use after disposal. Offset rendering owns no
GPU resources; the apply pass owns one hidden material and releases it on disposal.
The combined `BuiltInDistortionPass` uses these same stages. No extra screen copy,
target, draw or texture fetch is introduced by restoring the original names.

`DistortionContextItem` retains its constructor, `DistortedUvTexture` field name
and `Reset()` method. In Built-in the field is a real `RenderTexture`, replacing
the pipeline-specific handle. This is an explicit type change, not handle/source
compatibility. Reset clears the reference without releasing the caller's texture:

```csharp
var context = new DistortionContextItem { DistortedUvTexture = offsetTexture };
offsets.Setup(context.DistortedUvTexture);
apply.Setup(new RenderTargetIdentifier(context.DistortedUvTexture));
// Record and execute the passes as above; release the texture when GPU use ends.
context.Reset();
```

These classes are **not assignable to URP's ScriptableRenderPass**. Calls accepting
`RenderingData`, `RTHandle`, renderer features and graph contexts still require a
pipeline integration change. Use the explicit Camera/CommandBuffer execution above;
there are no successful no-op callbacks or duplicate Unity engine type definitions.

## Verification

The 2026-10-15 follow-up adds 14 GPU/API cases on Unity 2022.3.22f1 and
6000.3.23f1: original constructor/setup/reset calls, deferred targets, temporary
target identifiers, LightMode selection, scene-view options, disposal and context
ownership. The combined facade's neutral-offset/composite fixture records 400
command bytes before/after on Unity 2022 and 520 before/after on Unity 6.

Existing camera and shader-only tests cover occlusion, localized/negative/vertical
offsets, particles, Custom Data and independently recorded cameras. The repository's
RuntimeAPICompatibility.md records final full-suite results and remaining source
compatibility limits. These tests do not certify arbitrary URP callbacks or every
possible caller/input combination.

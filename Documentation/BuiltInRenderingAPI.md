# Built-in rendering APIs

These APIs use Unity Built-in types; they do not emulate URP renderer feature or render-pass types. They are optional normal-Unity camera helpers, not VRChat avatar components. Avatar distortion uses the shader-only path.

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

## Verification

Unity 2022.3.22f1, D3D11: 45 tests passed (`NovaBuiltInDistortionTests` and `NovaDistortionApiTests`). Coverage includes the existing camera and shader-only paths, occlusion, localized/negative/vertical offsets, particles, Custom Data and two command buffers recorded together then executed in reverse order. This is not URP C# API signature compatibility. Unity 6 and final release acceptance are tracked in `LowCostLightingParity.md`.

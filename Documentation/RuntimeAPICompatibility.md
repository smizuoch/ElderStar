# Original runtime API compatibility

This follow-up restores useful original NOVA call forms with actual Built-in
rendering. It does not redefine Unity engine types or introduce a URP dependency.
The original namespace, `Nova.Runtime.Core.Scripts`, is retained.

## Restored calls

| Original call | Built-in behavior | Remaining type difference |
| --- | --- | --- |
| `new ApplyDistortionPass(bool, Shader)` | Owns one hidden composite material; honors the SceneView switch | Does not derive from ScriptableRenderPass |
| `ApplyDistortionPass.Setup(RenderTargetIdentifier)` | Records a real texture or command-buffer temporary target | Execution uses explicit Camera/CommandBuffer arguments |
| `new DistortedUvBufferPass(string)` | Selects the actual LightMode tag and draws an explicit visible renderer list | Setup takes RenderTexture instead of RTHandle; no ScriptableRenderPass base |
| `new DistortionContextItem()` | Holds a real per-camera RenderTexture reference | Plain object instead of engine ContextItem; field type changes |
| `DistortionContextItem.Reset()` | Clears the texture binding without destroying caller-owned resources; remains overridable | Does not participate in the engine's graph lifecycle |

`ScreenSpaceDistortion.Create()` and every existing `BuiltInDistortionPass` public
method remain. The combined API delegates to the restored stages, with no extra
texture, draw, screen copy or `Shader.Find` call per frame. Shader sources, properties,
keywords and avatar components are unchanged by this runtime API restoration.
See the shipped [execution examples](../Assets/Nova/Documentation~/BuiltInRenderingAPI.md).

Both stages explicitly reject unsupported cameras, missing setup and use after
disposal. The apply stage rejects a source/offset target that aliases its destination.
Commands capture the setup target at recording time, so a second camera's Setup
cannot mutate the first camera's recorded inputs. Global bindings are restored to
their values at recording time, as in the existing Built-in API.

## Why some original code still needs adaptation

Unity's [ScriptableRenderPass](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/api/UnityEngine.Rendering.Universal.ScriptableRenderPass.html)
belongs to the URP runtime assembly and runs in its renderer lifecycle. Built-in
uses [CommandBuffer integration](https://docs.unity3d.com/2022.3/Documentation/Manual/GraphicsCommandBuffers.html).
Matching the spelling of an engine class would not provide its assembly identity,
renderer state, culling results, resource lifetimes or execution scheduling.

The remaining original declarations rely on ScriptableRenderPass/Feature,
RenderingData, RTHandle, ContextItem, TextureHandle or graph callbacks. Their
exact base types and argument types are not available in this dependency-free
Built-in package. The API inventory therefore retains **13 source-incompatible
declarations**, alongside **2 Exact and 6 Approximate**. Five declarations moved
from source-incompatible to Approximate; the restored signatures do not imply
compatibility of the whole containing type with URP inheritance.

The ContextItem field keeps its name but stores a RenderTexture. Callers that
expect a graph TextureHandle must adapt that binding. Constructor and Reset callers
that only use the NOVA class and Built-in texture can retain those call forms.
No fake URP base classes or successful no-op callbacks are provided.

For VRChat avatars, these C# stages are not added to the avatar. The existing
shader-only path remains active. VRChat's [allowed component policy](https://creators.vrchat.com/avatars/whitelisted-avatar-components/whitelisted-avatar-components/)
does not execute arbitrary custom avatar scripts. Restoring a normal-Unity C# API
therefore does not add runtime C# capability to an uploaded avatar.

## Verification record

The focused GPU fixture covers original call signatures, deferred setup changes,
temporary RenderTargetIdentifier bindings, SceneView enable/disable, LightMode tags
different from pass names, disabled passes, alias rejection, unsupported cameras,
resource ownership, disposal, context reset and a command-stream size observation.

Before implementation, 12 of the initial 13 focused cases failed because the pass
classes were absent. The later context case failed because its class was absent;
an additional assertion caught that Reset must remain virtual. The SceneView
fixture expectation was corrected from a .2 to a .4 UV shift: the existing shader
decodes stored .7 as `.7 * 2 - 1`. No image threshold or golden was changed.

The existing facade's neutral-offset/composite command stream measures **400 bytes
before and after on Unity 2022**, and **520 bytes before and after on Unity 6**.
The old facade was run in the same Unity 6 fixture to avoid comparing command
serialization sizes across different engine versions.
Both stages still record the same shader work for ordinary NOVA
materials. This is a command-stream observation, not a new GPU-millisecond benchmark
or a claim about arbitrary caller shaders. Explicit helpers do no scene search and
own no render targets; each active apply stage owns one material.

Final Unity 2022.3.22f1 and 6000.3.23f1 runs each pass **500/500 EditMode** and
**959/959 graphics PlayMode** tests, including all 14 focused API cases. C# and
shader error diagnostics are zero. The runtime/test sources in both Windows
test mirrors match the recorded repository SHA-256 values. Python Parity 18/18
and Docs 4/4 also pass.

Run counts, failing reproductions, diagnostics and source hashes are recorded in
[the verification JSON](Verification/RuntimeAPI20261015/summary.json).
No new SDK/AAO build, VRChat upload or GPU timing measurement was performed in this
follow-up. Avatar shader source is unchanged from the validated ambient release.
The remaining TODO is an integration for a concrete caller of one of the 13
pipeline-specific declarations; whole-engine type compatibility is not claimed.

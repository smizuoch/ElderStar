# Editor API Reference

[English](EditorAPI.md) | [日本語](EditorAPI_JA.md)

Use these APIs from scripts in an `Editor` folder or an Editor-only assembly definition.
Class names and namespaces are retained so existing NOVA extensions can use them.

## RendererErrorHandler

Namespace: `Nova.Editor.Core.Scripts`

| Method | Description |
| --- | --- |
| `bool CheckErrorWithMaterial(Material material)` | Returns `true` if ParticleSystemRenderers using this material have errors such as an incorrect vertex stream layout. |
| `void FixErrorWithMaterial(Material material)` | Configures required vertex streams and related settings on Renderers using this material, like Fix Now in the Inspector. |

If no Renderer in the scene uses the material, the check returns `false` and the fix does nothing.
Both regular materials and Trail Materials are checked.

```csharp
using Nova.Editor.Core.Scripts;
using UnityEngine;

public static class EffectStreams
{
    public static void Configure(Material material)
    {
        if (RendererErrorHandler.CheckErrorWithMaterial(material))
            RendererErrorHandler.FixErrorWithMaterial(material);
    }
}
```

## OptimizedShaderGenerator

Namespace: `Nova.Editor.Core.Scripts.Optimizer`

`void Generate(string outputFolderPath)` generates optimized Particle Uber Lit/Unlit shaders.
Choose an output folder under `Assets`. The three Render Types and OptionalShaderPass combinations produce 48 shaders in total.
UI and Distortion shaders are not included.

```csharp
OptimizedShaderGenerator.Generate("Assets/ElderStarOptimized");
```

Generated includes refer to the installed ElderStar package location.
Regenerate if you move the source package. Existing files with the same names are updated while retaining their GUIDs.

## OptimizedShaderReplacer

In the same namespace, `void Replace(Settings settings)` replaces materials with matching generated shaders.
Run `Generate` first. If a matching shader is missing, the replacer logs a warning and skips that material.

| Settings property | Description |
| --- | --- |
| `TargetFolderPath` | The material folder under `Assets`. Set it explicitly: the default searches all of `Assets`. |
| `OpaqueRequiredPasses` | Optional passes to retain for Opaque materials. |
| `CutoutRequiredPasses` | Optional passes to retain for Cutout materials. |
| `TransparentRequiredPasses` | Optional passes to retain for Transparent materials. |

`OptionalShaderPass` includes `None`, `DepthOnly`, `DepthNormals` and `ShadowCaster`.
Combine flags with expressions such as `OptionalShaderPass.DepthNormals | OptionalShaderPass.ShadowCaster`.
Built-in camera depth can require ShadowCaster, so do not remove it based only on whether visible shadows are needed.
See the [dedicated buffer guide](DepthNormals.md) for DepthNormals usage.

Replacement preserves material values, keywords and Render Queue, and supports Undo.
The [main guide](../README.md#optimization) includes a complete example.

## Runtime Rendering API

See [Built-in Rendering API](BuiltInRenderingAPI.md) for distortion with Camera, CommandBuffer and RenderTexture.
These APIs do not accept URP ScriptableRendererFeature, ScriptableRenderPass or RenderGraph types directly.

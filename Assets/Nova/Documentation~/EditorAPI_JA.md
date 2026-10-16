# Editor APIリファレンス

[English](EditorAPI.md) | [日本語](EditorAPI_JA.md)

Editor用スクリプトから利用するAPIです。コードは`Editor`フォルダー、またはEditor専用asmdefに置いてください。
既存のNOVA拡張を利用できるよう、クラス名・名前空間を維持しています。

## RendererErrorHandler

名前空間：`Nova.Editor.Core.Scripts`

| メソッド | 説明 |
| --- | --- |
| `bool CheckErrorWithMaterial(Material material)` | 指定したマテリアルを使用するParticleSystemRendererに頂点ストリームなどのエラーがある場合、`true`を返します。 |
| `void FixErrorWithMaterial(Material material)` | そのマテリアルを使用するRendererの必要な頂点ストリームなどを設定します。InspectorのFix Nowに相当します。 |

シーン内でそのマテリアルを使用するRendererがなければ、チェックは`false`、修正は何もしません。
通常のマテリアルとTrail Materialの両方が対象です。

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

名前空間：`Nova.Editor.Core.Scripts.Optimizer`

`void Generate(string outputFolderPath)`は、Particle Uber Lit/Unlitから最適化シェーダーを生成します。
生成先は`Assets`以下を指定します。Render Typeの3種類とOptionalShaderPassの組み合わせにより、計48種類を生成します。
UI・Distortionシェーダーは生成対象ではありません。

```csharp
OptimizedShaderGenerator.Generate("Assets/ElderStarOptimized");
```

生成されるincludeは、導入済みのElderStarのパスを参照します。
ソースパッケージの配置を変えた場合は再生成してください。生成済みの同名ファイルは更新され、GUIDは維持されます。

## OptimizedShaderReplacer

同じ名前空間の`void Replace(Settings settings)`で、マテリアルを対応する生成済みシェーダーへ差し替えます。
先に`Generate`を実行してください。該当するシェーダーがない場合は警告を出し、そのマテリアルをスキップします。

| Settingsプロパティ | 説明 |
| --- | --- |
| `TargetFolderPath` | 対象マテリアルのフォルダー。`Assets`以下を指定します。省略時は`Assets`全体が対象になるため、対象を明示してください。 |
| `OpaqueRequiredPasses` | Opaqueで保持する任意パス。 |
| `CutoutRequiredPasses` | Cutoutで保持する任意パス。 |
| `TransparentRequiredPasses` | Transparentで保持する任意パス。 |

`OptionalShaderPass`は`None`・`DepthOnly`・`DepthNormals`・`ShadowCaster`です。
複数のパスは`OptionalShaderPass.DepthNormals | OptionalShaderPass.ShadowCaster`のように指定します。
Built-inのカメラ深度にはShadowCasterが必要になるため、画面上の影の有無だけで削除しないでください。
DepthNormalsの使い方は[専用バッファの説明](DepthNormals.md)を参照してください。

差し替えではマテリアルの値・キーワード・Render Queueを維持し、Undoに対応します。
完全な使用例は[README](../README_JA.md#optimization)にあります。

## 実行時の描画API

Camera・CommandBuffer・RenderTextureを使う歪み処理は[Built-in Rendering API](BuiltInRenderingAPI.md)を参照してください。
URPのScriptableRendererFeature・ScriptableRenderPass・RenderGraph型をそのまま使用するAPIではありません。

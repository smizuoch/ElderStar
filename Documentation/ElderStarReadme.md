# ElderStar README・実画像の更新記録

2026-10-16。描画・Editor APIの実装変更はなく、README、同梱リファレンス、紹介画像、撮影用ツールを更新した。

## 変更

- リポジトリとパッケージ内のREADMEを同じ内容に統一。本家の「作例 → 導入 → 各シェーダーの設定表 → Custom Data → 最適化 → API」の流れを基に、日本語の機能リファレンスを掲載した。
- `me.smizuoch.elderstar`のインストール、元のNOVA素材の直接インポート、VRChat PCでの使用、Built-inの補助機能と制約を追加した。配布前の旧IDからの移行案内は追加していない。
- 本家ドキュメントのMITライセンス・著作者表示を保持。Soft Particlesの表記、Depth Fadeの深度テクスチャ要否、最適化ReplacerのAPI名などは実装に合わせて訂正した。本家のメモリ削減率を移植版の実測値として引用していない。
- Inspector 18枚、作例GIF 4本を新たに撮影した。全22画像はローカルファイルで、[取得方法とSHA-256](../Assets/Nova/Documentation~/Images/captures.json)、[素材の出典](../Assets/Nova/Documentation~/Images/README.md)を同梱した。
- [Editor API](../Assets/Nova/Documentation~/EditorAPI.md)を追加。通常Unity用の補助機能は既存の個別ガイドへ接続した。

## 撮影

隔離したBuilt-inプロジェクトに現在のElderStarとNOVAのMITサンプルを配置し、Unity 2022.3.22f1 / Direct3D 11 / Gammaで撮影した。サンプルの参照元はNOVAコミット`1c6b88729f91c852c164ef71a32cc2bce83a1892`。

Inspectorは[撮影用EditorWindow](../Scripts/Unity/ElderStarReadmeCapture.cs)から実際のElderStar Inspector描画コードを呼び出し、許可された可視ウィンドウの領域を取得した。非表示GUIの取得では空白になるため、掲載画像には使用していない。プロパティを見せるための設定例で、初期値一覧ではない。

4種類のPrefabは固定乱数・固定時刻で各48フレーム、合計192フレームをCameraから960×600のRenderTextureへ描画した。Soft Particles用のカメラ深度を有効化し、反射にはサンプルのCubemapを使用した。Bloom・画像の描き足し・補正は行っていない。GIF化による色数と時間分解能の差はある。

再撮影には隔離プロジェクトの`Assets/Editor`へ撮影スクリプトをコピーし、Unityを次の引数で起動する。作業中のシーンを持つプロジェクトでは実行しない。

```text
-projectPath <isolated-project>
-executeMethod ElderStarReadmeCapture.Capture
-elderstarCaptureOutput <new-output-directory>
-logFile <capture-log>
```

`current.txt`に表示された各セクションのウィンドウを`<section>.png`として取得し、取得後に同名の`<section>.ack`を作ると次へ進む。18枚が完了すると作例を描画して終了する。部分撮影は`-elderstarSections Distortion`などで指定できる。

今回の梱包では18枚を撮ったディレクトリと、Distortion見出し・カメラ構図を調整した最終撮影ディレクトリを指定した。

```text
python Scripts/Documentation/package_readme_media.py \
  --inspectors <inspector-directory> \
  --effects <final-capture-directory> \
  --distortion <final-capture-directory> \
  --log <final-capture-log>
```

PillowとNumPyを使用する。PNGはそのままコピーし、GIFは各2秒で繰り返す。画像総量は5,136,073 bytes。撮影中は一時的にUnityウィンドウを表示し、完了後にそのEditorを終了した。

## 検証結果

README構成・リンク・ローカル画像の来歴に対する失敗するテストを先に追加し、本文・画像・manifestを揃えて成功させた。

| 確認 | 結果 |
| --- | --- |
| `python3 -m unittest discover -s Tests/Docs -v` | 8 / 8 pass |
| `python3 -m unittest discover -s Tests/Parity -v` | 24 / 24 pass |
| Unityでの撮影・READMEの最適化サンプルコードのコンパイル | 完了、C#コンパイルエラー0 |
| 撮影対象シェーダーの`ShaderUtil.GetShaderMessages` | エラー0 |
| GIF入力フレーム・画像SHA-256 | 192フレーム、22画像を検査 |
| ローカルのMarkdown描画プレビュー | 全25画像（バッジ3枚を含む）読み込み成功、目次のリンク切れ0、見出しが設定表内へ入り込まないことを確認 |
| `git diff --check` | pass |

UnityログにはLicensing Clientの既存の検証・トークン警告と、撮影用`RenderSettings.customReflection`の非推奨警告がある。撮影完了は確認しているが、ログ全体が警告・エラー表示なしとは扱わない。

## 残る範囲

この変更はドキュメント用の実描画確認であり、画像回帰基準の更新やURPとの新しい一致率の測定ではない。製品のシェーダー・実行時処理・Editor実装・GUIDは変更していないため、SDK/AAO・GPU性能の全回帰は再実行していない。READMEの検証概要は既存の[検証記録](Verification.md)に基づく。

次のTODOは、既存の実機PCVR・最新状態の実クライアント確認を継続すること。README更新に関する未完了項目はない。

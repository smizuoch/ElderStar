# ElderStar配布パッケージ

2026-10-16、バージョン0.1.8。配布先は[GitHub Releases](https://github.com/smizuoch/ElderStar/releases/tag/v0.1.8)。ユーザーの指定によりリポジトリの非公開設定を維持し、閲覧権限を持つ人向けにリリースへ添付する。

## ファイル

| ファイル | 用途 | サイズ |
| --- | --- | ---: |
| ElderStar-0.1.8.unitypackage | Unityで開き、Assets/NovaへImport | 5,247,977 bytes |
| me.smizuoch.elderstar-0.1.8.tgz | Package ManagerのAdd package from tarball... | 5,045,202 bytes |
| SHA256SUMS.txt | 上記2ファイルのSHA-256 | 2行 |

同じ本体なので、二つの形式を同時に導入しない。元のNOVA本体との重複も避ける。導入・機能・制限は[README](../README.md)と[リリースノート](Releases/0.1.8.md)に記載した。

UPM版はパッケージ内279ファイルをすべてバイト単位で保持する。Assets版はUnityが無視する`Documentation~`と`Samples~`を`Documentation`と`Samples`へ配置し、説明書の相対リンクとpackage.jsonのサンプルパスだけを対応させる。ランタイム・Editorの100ファイルと各metaは元ソースと完全一致し、GUIDを維持する。説明書・サンプル用の不足metaは隔離したUnityで生成する。

本体・説明書・サンプル・MITライセンスだけが対象で、他のプロジェクト素材、VRChatアバター、テスト、検証用スクリプトは同梱しない。`.unitypackage`の全170パスが`Assets/Nova`内であることを確認済み。

## 作成方法

1. 空のBuilt-inプロジェクトを用意する。作業中のシーンがあるプロジェクトでは実行しない。
2. [prepare_release.py](../Scripts/Distribution/prepare_release.py)でAssets用とUPM用のステージを新しいディレクトリへ作る。既存ディレクトリの上書きと未確認のルートファイルは拒否する。

```text
python Scripts/Distribution/prepare_release.py --mode assets --output <export-project>/Assets/Nova
python Scripts/Distribution/prepare_release.py --mode upm --output <upm-source>
```

3. 元の`Assets/Nova.meta`を同じ場所へコピーし、[ElderStarDistribution.cs](../Scripts/Unity/ElderStarDistribution.cs)を隔離プロジェクトの`Assets/Editor`へ置く。
4. Unity 2022.3.22f1を次の引数で起動する。非同期のUPM Pack完了を待つため`-quit`は付けない。

```text
-batchmode -force-d3d11 -projectPath <export-project>
-executeMethod ElderStarDistribution.Export
-elderstarOutput <output-directory>
-elderstarUpmSource <upm-source>
-logFile <export-log>
```

Unity公式の[AssetDatabase.ExportPackage](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AssetDatabase.ExportPackage.html)をRecurseのみで使用し、依存アセットの自動取り込みは行わない。UPM版は[Client.Pack](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PackageManager.Client.Pack.html)で作る。

Unityの初回インポートでは`.cginc`へGLES除外行が追加され、最小限のmetaへ既定情報が補完される場合がある。初回書き出しは元ファイルとの照合で不一致になったため採用せず、最終版ではインポートを一時停止して製品ファイルを原文へ戻してから書き出す。利用者側のUnityが導入時に通常のアップグレードを行うことまでは抑止しない。

## 検証

対象は配布する最終アーカイブそのもの。隔離プロジェクトに実際に導入してから検査した。

| Unity | .unitypackage | UPM .tgz |
| --- | ---: | ---: |
| 2022.3.22f1 / D3D11 | 4 / 4 pass | 4 / 4 pass |
| 6000.3.23f1 / D3D11 | 4 / 4 pass | 4 / 4 pass |

各組み合わせで、URPなし・元の6シェーダーの検索と全パスのコンパイル、オフライン説明書とライセンス、同梱サンプルの参照と実描画、48種類の最適化シェーダー生成とinclude解決を確認した。実描画は960×640で色付き画素が6,000以上あることを検査し、結果画像を保存・目視確認した。既存の基準画像は変更していない。

テスト用プロジェクトだけにTest Framework 1.1.33とuGUI（2022では1.0.0、Unity 6では2.0.0）を追加した。製品の依存関係は変えていない。テストは[Tests/Distribution](../Tests/Distribution)、環境構築は[ElderStarDistributionBootstrap.cs](../Scripts/Unity/ElderStarDistributionBootstrap.cs)を使用する。

```text
python3 -m unittest discover -s Tests/Distribution -v
python3 -m unittest discover -s Tests/Docs -v
python3 -m unittest discover -s Tests/Parity -v
unity test <fixture> --editor-version <version> --mode EditMode --filter ElderStarDistributionTests --output <results.xml> -- -force-d3d11
```

Pythonは配布4件、文書8件、互換性24件の計36件成功。最終Unity検査は16件成功、製品のC#コンパイル・シェーダーエラー0。初回のテストコードにはPackageInfoの名前空間の曖昧さがあり、明示的なaliasで修正してから全件を再実行した。Unityのライセンス・Androidツール初期化メッセージ、パッケージ監視のバッファ警告は別途記録し、ログ全体が警告なしとは扱わない。

Unity 2022で初期アーカイブから最終UPMアーカイブへ差し替えた際、一時的に旧キャッシュを参照するCS2001が8件記録された。自動再コンパイル後のテストは成功したが、このログを最終のエラー0判定には使用せず、Editorを再起動して全4項目を再実行し、C#・シェーダーエラー0を確認した。

[SHA-256と内容照合](Verification/Distribution20261016/artifacts.json)、[結果一覧・圧縮XML・実描画画像](Verification/Distribution20261016/summary.json)を保存した。ダウンロード後も添付のSHA256SUMSで一致を確認できる。

## 残る検証

製品の描画・Editorコードには変更がないため、今回はSDK/AAOとGPU性能の全回帰を再実行していない。既存の[受け入れ検証](SoftParticleAmbientAcceptance.md)と[互換性表](BuiltInCompatibilityMatrix.md)の制限を引き継ぐ。

配布パッケージ作成のTODOは完了。次は既存の実機PCVR・最新実クライアントでの確認を継続する。初回配布はPre-releaseとし、非公開リポジトリの閲覧権限は拡張しない。

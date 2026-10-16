# 掲載画像について

READMEの作例とInspector画像は、ElderStarで新たに撮影しています。
本家READMEの画像・GIFをElderStarの動作画像として流用していません。

## 作例

[NOVA Shaderのサンプル](https://github.com/CyberAgentGameEntertainment/NovaShader/tree/1c6b88729f91c852c164ef71a32cc2bce83a1892/Assets/Samples)
（MITライセンス）を、ElderStarのBuilt-inシェーダーで描画しています。
素材制作者は[@Ugokashiya](https://twitter.com/Ugokashiya)、原典はCyberAgent, Inc.です。

| 画像 | 使用したPrefab |
| --- | --- |
| barrier.gif | pfb_eff_barrier01 |
| ice.gif | pfb_eff_ice02 |
| slash.gif | pfb_eff_slash01 |
| heal.gif | pfb_eff_heal01 |

Unity 2022.3.22f1、Windows / Direct3D 11、Gammaで撮影しました。
固定したパーティクルの乱数と時刻でCameraをRenderTextureへ描画し、PNGの連続フレームをGIFにしています。
GIFの色数・フレーム時間には形式上の制限があります。比較用の基準画像ではありません。
背景・カメラ・照明は紹介用の撮影設定で、Bloomなどの後処理は使用していません。
カメラの深度テクスチャを有効にし、反射には本家サンプルのCubemapを使用しています。

## Inspector

実際のElderStarのInspector描画コードを撮影用EditorWindowで機能ごとに表示し、
Unityのウィンドウをキャプチャしています。UIを描き直したモック画像ではありません。
項目を見せるための設定例であり、すべての画像が初期値を表すわけではありません。

撮影スクリプトはリポジトリの[Scripts/Unity/ElderStarReadmeCapture.cs](https://github.com/smizuoch/ElderStar/blob/main/Scripts/Unity/ElderStarReadmeCapture.cs)、
画像ごとの出典・取得方法・SHA-256は[captures.json](captures.json)に記録しています。
元のライセンスは[LICENSE.md](../../LICENSE.md)を参照してください。

# ElderStarのサンプルエフェクト

**Docs** ([English](../../README.md), [日本語](../../README_JA.md)) | **Samples** ([English](README.md), [日本語](README_JA.md)) | **Demo** ([English](../Demo/README.md), [日本語](../Demo/README_JA.md))

## 概要

NOVAのサンプル素材では、シェーダーの機能を組み合わせたエフェクトを確認できます。
以下の作例は、Built-inプロジェクトでElderStarを使って描画・撮影したものです。
素材は本家リポジトリで配布されています。ElderStarのパッケージには、これとは別に[Built-in Showcaseデモ](../Demo/README_JA.md)を同梱しています。

<p align="center">
  <img width="80%" src="../Images/barrier.gif" alt="ElderStarで描画したバリアのサンプル"><br>
  <sub>pfb_eff_barrier01</sub>
</p>
<p align="center">
  <img width="80%" src="../Images/ice.gif" alt="ElderStarで描画した氷のサンプル"><br>
  <sub>pfb_eff_ice02</sub>
</p>
<p align="center">
  <img width="49%" src="../Images/slash.gif" alt="ElderStarで描画した斬撃のサンプル">
  <img width="49%" src="../Images/heal.gif" alt="ElderStarで描画した回復のサンプル"><br>
  <sub>pfb_eff_slash01 / pfb_eff_heal01</sub>
</p>

## 使い方

1. Built-inプロジェクトへ[ElderStarを導入](../../README_JA.md#setup)します。
2. [今回の撮影に使用したNOVAのソース](https://github.com/CyberAgentGameEntertainment/NovaShader/tree/1c6b88729f91c852c164ef71a32cc2bce83a1892)を、Unityプロジェクトの外へダウンロードまたはcloneします。
3. その中の`Assets/Samples`フォルダーを、`.meta`ファイルごと自分のプロジェクトの`Assets`へコピーします。本家の実装が入った`Assets/Nova`はコピーしません。
4. 取り込んだサンプルのエフェクトPrefabをBuilt-inのシーンに配置し、再生します。標準NOVAマテリアルの参照は、変換やシェーダーの付け替えなしでElderStarにつながります。

`.unitypackage`で配布されているエフェクトには、**`Assets > Import Package > ElderStar Effect...`**を使います。同梱された本家のNOVA実装を除外し、素材を取り込みます。

本家シーンのURP向け描画設定は、エフェクト素材とは別です。Built-inのカメラを使い、シーンに合わせて照明・反射・カメラ深度を設定してください。
Soft Particlesには深度テクスチャが必要です。Bloomなどのポストプロセスはシェーダーから追加されません。
詳細は[深度・歪みの設定](../../README_JA.md#usage)と[互換性](../../README_JA.md#compatibility)を参照してください。

## 制作者・撮影条件

サンプルエフェクトは[@Ugokashiya](https://twitter.com/Ugokashiya)氏の制作で、CyberAgentのNOVA ShaderリポジトリにMITライセンスで公開されています。
上の画像はUnity 2022.3.22f1 / Direct3D 11 / Gammaで、パーティクルの乱数と時刻を固定してElderStarで撮影したものです。
描画の作例であり、すべてのシーンで同じ見た目になることを保証するものではありません。
[画像のクレジットと撮影条件](../Images/README_JA.md)も参照してください。

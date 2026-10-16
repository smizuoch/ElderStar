# ElderStar: Built-in向け多機能パーティクルシェーダー

[![MIT License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-green.svg)](#requirements)
[![Built-in](https://img.shields.io/badge/Render%20Pipeline-Built--in-blue.svg)](#requirements)

**使い方・機能リファレンス**（このページ） | **[サンプル](#samples)** | **[互換性](#compatibility)** | **[Editor API](Documentation~/EditorAPI.md)**

**ElderStar**は、UnityのBuilt-in Render Pipelineで動作する、Particle System向けの多機能シェーダーです。
[NOVA Shader](https://github.com/CyberAgentGameEntertainment/NovaShader) 3.6.0をベースに、NOVA用のエフェクトをBuilt-inプロジェクトやVRChatのPCアバターで使えるようにしています。

<p align="center">
  <img width="80%" src="Documentation~/Images/barrier.gif" alt="ElderStarで描画したNOVAサンプルのバリアエフェクト"><br>
  <sub>バリア。NOVAのサンプル素材をElderStarで描画。</sub>
</p>
<p align="center">
  <img width="80%" src="Documentation~/Images/ice.gif" alt="ElderStarで描画したNOVAサンプルの氷エフェクト"><br>
  <sub>氷。サンプルエフェクト制作：<a href="https://twitter.com/Ugokashiya">@Ugokashiya</a>／Built-inで再撮影。</sub>
</p>

Flow Map、Flip Book、Dissolve、Fade、テクスチャ回転、リム、Emission、Distortionなど、エフェクト制作でよく使う機能を組み合わせられます。
ライティングを受けるUber Lit、色をそのまま描くUber Unlit、uGUI用シェーダー、Custom Dataとの連携、最適化シェーダーの生成にも対応しています。

**NOVAの標準シェーダーを参照するマテリアル・Prefabは、素材変換やシェーダーの付け替えなしで利用できます。**
元のNOVA本体が同梱された`.unitypackage`も、[専用のインポートメニュー](#nova-assets)から元ファイルを選んで取り込めます。

<p align="center">
  <img width="49%" src="Documentation~/Images/slash.gif" alt="ElderStarで描画した斬撃エフェクト">
  <img width="49%" src="Documentation~/Images/heal.gif" alt="ElderStarで描画した回復エフェクト"><br>
  <sub>斬撃と回復。掲載画像・GIFはElderStarの実描画です。<a href="Documentation~/Images/README.md">撮影条件・素材クレジット</a></sub>
</p>

パッケージID：`me.smizuoch.elderstar` · 開発・メンテナンス：[smizuoch](https://smizuoch.me)

## 目次

<details>
<summary>開く</summary>

- [セットアップ](#setup)
- [NOVA用の素材をそのまま使う](#nova-assets)
- [使い方](#usage)
- [Uber Unlitシェーダー](#uber-unlit)
  - Render Settings / Vertex Deformation / Base Map / Tint Color
  - Flow Map / Parallax Map / Color Correction / Alpha Transition / Emission / Transparency
- [Uber Litシェーダー](#uber-lit)
- [Distortionシェーダー](#distortion)
- [uGUI用シェーダー](#ugui)
- [影を落とす機能](#shadow-caster)
- [Custom Vertex Streamsとの連携](#custom-data)
- [Mesh GPU Instancing](#gpu-instancing)
- [Custom Vertex Streamsの自動設定](#fix-now)
- [不要なパラメータ参照の削除](#unused-references)
- [最適化シェーダー](#optimization)
- [Built-in向けの補助機能](#builtin-helpers)
- [互換性と動作確認](#compatibility)
- [Editor API](#editor-api)
- [ライセンス](#license)

</details>

<a id="setup"></a>
## セットアップ

<a id="requirements"></a>
### 要件

- Unity **2022.3 LTS以降**。動作確認：2022.3.22f1 / 6000.3.23f1。
- **Built-in Render Pipeline**。URP・HDRPの追加は不要です。
- Shader Model **3.5**。Mesh GPU Instancingは**4.5**が必要です。
- 検証環境はWindows / Direct3D 11・12。VRChatでは**PCアバター**を対象としています。

Mirror SamplingにはInline Sampler States、Texture2DArray・Texture3Dには対応GPUと対応テクスチャ形式が必要です。
VRChatプロジェクトでは、VCC / ALCOMとVRChat SDKが指定するUnityバージョンを使用してください。

### インストール

1. `Window > Package Manager`を開きます。
2. `+ > Add package from git URL...`を選びます。
3. 次のURLを入力します。

```text
https://github.com/smizuoch/ElderStar.git?path=/Assets/Nova
```

特定の状態に固定する場合は、URL末尾に`#コミットSHA`を付けます。
ローカルのソースを使う場合は`Add package from disk...`で`Assets/Nova/package.json`を選択できます。

プロジェクトのGraphics設定と各Quality設定のRender Pipeline Assetは`None`にしてください。
元のNOVA本体とは同時に導入しないでください。素材参照を引き継ぐため、シェーダー名・GUID・API名を共有しています。

<a id="nova-assets"></a>
## NOVA用の素材をそのまま使う

NOVA 3.6.0の標準シェーダーを使用した素材は、元のマテリアル設定・テクスチャ・Prefab参照を引き継げます。
たとえば`Fire001.unitypackage`のようなエフェクトは、次の手順で取り込みます。

1. ElderStarをインストールします。
2. **`Assets > Import Package > ElderStar Effect...`**を選びます。
3. ダウンロードした元の`.unitypackage`を選び、Unityの画面で`Import`します。
4. 取り込んだPrefabをシーン、またはアバターの子に配置します。

このメニューは同梱された元のNOVA実装だけを除外し、素材ファイルを変更せずに取り込みます。
変換済みのパッケージを別途作る必要はありません。
NOVA本体が含まれていない素材は、Unityの通常のインポートでも取り込めます。

**元のNOVA本体を含むパッケージをダブルクリックで取り込む場合は、その本体フォルダーを選択から外してください。**
URP専用スクリプトや独自シェーダーまで自動でBuilt-inに変換する機能ではありません。
元の最適化シェーダーを同梱した素材は、ElderStarでの再生成が必要です。

<a id="usage"></a>
## 使い方

### マテリアルを作成・アサインする

`Create > Material`でマテリアルを作成し、Shader欄から用途に応じたシェーダーを選びます。
既存のNOVA素材を扱いやすくするため、Shader欄の名前は`Nova/...`のままです。

| シェーダー | 用途 |
| --- | --- |
| `Nova/Particles/UberUnlit` | 光の影響を受けない炎・光・魔法などのエフェクト |
| `Nova/Particles/UberLit` | ライティングを受けるメッシュ・パーティクル |
| `Nova/Particles/Distortion` | 熱波・衝撃波などの画面の歪み |
| `Nova/UIParticles/UberUnlit` | uGUI向けのUnlitエフェクト |
| `Nova/UIParticles/UberLit` | uGUI向けのLitエフェクト |

Particle SystemのRendererモジュールにマテリアルを設定し、Inspectorでテクスチャや色を調整します。
頂点ストリームの警告が出たら、マテリアルInspector下部の**Fix Now**で必要なストリームを設定してください。

### 深度・歪みの設定

**Distortionはカメラ用スクリプトを追加せずに動作します。**
通常のUnityプロジェクトで歪みをカメラ全体で合成したい場合は、カメラに
`ElderStar > Screen Space Distortion`を追加できます。URPのRenderer Featureは使いません。

**Soft Particles**にはカメラの深度テクスチャが必要です。
通常のUnityでは`Camera.depthTextureMode |= DepthTextureMode.Depth`を設定するか、上記カメラコンポーネントを使用します。
カメラとの距離でフェードする**Depth Fade**には、シーン深度テクスチャは不要です。

初回導入時には、本家と同じ光強度・色温度の設定がプロジェクト全体に適用されます。
以後の手動変更は保持されます。自動設定を使わない場合は
`Tools > ElderStar > Built-in > Automatic Initial Lighting Setup`をオフにできます。

### VRChatのPCアバターで使う

エフェクトのParticle SystemやMeshRendererをアバターに配置します。
ElderStarのカメラ・Canvas補助コンポーネントをアバターに付ける必要はありません。
アバターでは任意の実行時C#を使用できないため、シェーダーだけで動作する経路を使います。
コンポーネントの範囲は[VRChat公式の一覧](https://creators.vrchat.com/avatars/whitelisted-avatar-components/whitelisted-avatar-components/)に従ってください。

Soft Particlesの効き方はワールドの深度テクスチャに依存します。
閲覧者の[Safety設定](https://creators.vrchat.com/avatars/shader-fallback-system/)によって、カスタムシェーダーが置き換えられる場合もあります。
SDKの検証と`Build & Test`で、使用するアバター・ワールド・鏡での表示を確認してください。

<a id="samples"></a>
### サンプルを動かす

Package ManagerのElderStarから**Built-in Showcase**をImportし、`NovaShowcase.unity`を開いて再生します。
4種類のパーティクルと歪みを確認できます。
`Tools > ElderStar > Built-in > Create Showcase Scene`から、保存先を指定してシーンを作ることもできます。

冒頭の作例は[本家のサンプル素材](https://github.com/CyberAgentGameEntertainment/NovaShader/tree/main/Assets/Samples)をElderStarで再撮影したもので、同梱Showcaseとは別です。

<a id="uber-unlit"></a>

## Uber Unlitシェーダー
Uber Unlitシェーダはライティングが反映されない多機能シェーダです。
発光するエフェクトなど、ライティングが不要なパーティクルにはこのシェーダを使用します。

Uber Unlitシェーダを使用するには、マテリアルに `Nova/Particles/UberUnlit` シェーダをアサインします。
Inspectorから設定可能な各プロパティの説明は以下の通りです。

#### Render Settings
Render Settingsではマテリアルの描画方法を制御できます。

<p align="center">
  <img width="70%" src="Documentation~/Images/RenderSettings.png" alt="ElderStar Inspector: RenderSettings"><br>
  <sub>Render Settings</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Render Type</b></td><td>
<p>
描画方法を以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Opaque: 不透明描画</li>
<li>Cutout: 不透明描画で、透明部分のピクセルを破棄する</li>
<li>Transparent: 半透明描画（デフォルト）</li>
</ul>
</p>
<p>
Cutoutを選択した場合には、CutOffプロパティが表示されます。<br>
Transparentを選択した場合には、Blend Modeプロパティが表示されます。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>CutOff</b></td><td>
<p>
<b>Render TypeをCutoutに設定した時のみ表示されます。</b>
</p>
<p>
半透明部分を切り取る際の閾値を制御します。<br>
この値が大きいほど、よりアルファ値が大きい部分まで切り取ります。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Blend Mode</b></td><td>
<p>
<b>Render TypeをTransparentに設定した時のみ表示されます。</b>
</p>
<p>
色の合成方法を以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Alpha: アルファブレンディング（デフォルト）</li>
<li>Additive: 加算</li>
<li>Multiply: 乗算</li>
</ul>
</p>
</td></tr>
<tr><td colspan="3"><b>Render Face</b></td><td>
<p>
描画する面を以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Front: 前面のみ描画（デフォルト）</li>
<li>Back: 背面のみ描画</li>
<li>Both: 両面を描画</li>
</ul>
</p>
</td></tr>
<tr><td colspan="3"><b>Render Priority</b></td><td>
<p>
描画の優先度を指定できます。<br>
Render Typeが同じ場合、Render Priorityが小さいものから先に描画されます。
</p>
</td></tr>
<tr><td colspan="3"><b>Vertex Alpha Mode</b></td><td>
<p>
頂点カラーのアルファ値の用途を以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Alpha: アルファ値として扱う（デフォルト）</li>
<li>Transition Progress: Alpha Transition機能（後述）のProgressとして使う</li>
</ul>
</p>
</td></tr>
<tr><td colspan="3"><b>ZWrite</b></td><td>
<p>
ZWriteを設定できます。
</p>
<p>
<ul>
<li>Auto: Render TypeがTransparentの時にはZWriteを無効に、それ以外の時にはZWriteを有効に設定する（デフォルト）</li>
<li>Off: ZWriteを常に無効化する</li>
<li>On: ZWriteを常に有効化する</li>
</ul>
</p>
</td></tr>
<tr><td colspan="3"><b>ZTest</b></td><td>
<p>
ZTestを設定できます。
</p>
<p>
<ul>
<li>Disabled</li>
<li>Never</li>
<li>Less</li>
<li>Equal</li>
<li>Less Equal（デフォルト）</li>
<li>Greater</li>
<li>Not Equal</li>
<li>Greater Equal</li>
<li>Always</li>
</ul>
</p>
</td></tr>
</tbody>
</table>

#### Vertex Deformation
Vertex Deformationでは頂点の変形を制御できます。

<p align="center">
  <img width="70%" src="Documentation~/Images/VertexDeformation.png" alt="ElderStar Inspector: VertexDeformation"><br>
  <sub>Vertex Deformation</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Texture</b></td><td>
<p>
頂点変形マップを設定します。頂点変形マップの仕様は以下の通りです。
</p>
<p>
<ul>
<li>指定された1個のテクスチャのチャンネルの値に基づいて、頂点をオブジェクト空間の法線方向に押し出します</li>
<li>チャンネルの指定はChannelsプロパティで行えます</li>
<li>テクスチャの値がゼロの部分は押し出されず、値が大きくなるほど外側に押し出されます</li>
</ul>
色ではなく値としてテクスチャを使うので、テクスチャ設定のsRGB Colorのチェックを外す必要がある点に注意してください。
</p>
</td></tr>
<tr><td colspan=3><b>Intensity</b></td><td>
<p>
頂点変形を適用する際の強度を設定します。
</p>
</td></tr>
<tr><td colspan=3><b>Base Value</b></td><td>
<p>
テクスチャの値に対してのオフセットです。<br>
これよりも低い値は内側に、大きい値は外側に押し出されます。
</p>
</td></tr>
</tbody>
</table>

#### Base Map
Base Mapでは色のベースとなるテクスチャを制御できます。

<p align="center">
  <img width="70%" src="Documentation~/Images/BaseMap.png" alt="ElderStar Inspector: BaseMap"><br>
  <sub>Base Map</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Mode</b></td><td>
<p>
ベースマップのモードを以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Single Texture: 通常の2Dテクスチャ（デフォルト）</li>
<li>Flip Book: Flip-Bookアニメーション</li>
<li>Flip Book Blending: ブレンドありのFlip-Bookアニメーション</li>
</ul>
</p>
<p>
選択したモードによってテクスチャの型が変わります。<br>
なお、ElderStarではParticle SystemのTexture Sheet Animationを利用せず、代わりにFlip Bookを利用してください。<br>
</p>
</td></tr>
<tr><td colspan="3"><b>Texture</b></td><td>
<p>
ベースマップを設定します。
</p>
<p>
ModeにFlip Bookを指定した場合には、<a href="https://docs.unity3d.com/2020.3/Documentation/Manual/class-Texture2DArray.html">Texture2DArray</a>を設定する必要があります。<br>
ModeにFlip Book Blendingを指定した場合には、<a href="https://docs.unity3d.com/2020.3/Documentation/Manual/class-Texture3D.html">Texture3D</a>を設定する必要があります。
</p>
</td></tr>
<tr><td colspan="3"><b>Channel</b></td><td>
<p>
Base Mapのどの色チャンネルを描画に使用するかを選択します。単一チャンネル（Red/Green/Blue）が選択された場合、選択されたチャンネル値がRGB成分に適用され、元のアルファチャンネルは保持されます。利用可能なオプション：
</p>
<p>
<ul>
<li>RGB: 全ての色チャンネルをそのまま使用（デフォルト）</li>
<li>Red: 赤チャンネルをグレースケールに変換</li>
<li>Green: 緑チャンネルをグレースケールに変換</li>
<li>Blue: 青チャンネルをグレースケールに変換</li>
</ul>
</td></tr>
<tr><td colspan="3"><b>Rotation</b></td><td>
<p>
ベースマップの回転度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Offset</b></td><td>
<p>
ベースマップを回転する際の中心座標のオフセットです。
</p>
</td></tr>
<tr><td colspan="3"><b>Mirror Sampling</b></td><td>
<p>
チェックをつけるとテクスチャがミラーサンプリングされます。
</p>
</td></tr>
<tr><td colspan="3"><b>Flip-Book Progress</b></td><td>
<p>
<b>ModeをFlip BookあるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
Flip-BookあるいはFlip-Book Blendingの進行度を設定します。
</p>
</td></tr>
<tr><td colspan="3"><b>Random Row Selection</b></td><td>
<p>
<b>ModeをFlip BookあるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
UnityのTexture Sheet AnimationのRow Mode > Random機能と同等の機能です。有効にすると、各パーティクルがライフタイム中にランダムに1つの行を選択・維持してアニメーションを再生します。
</p>
<p>
ランダム値の入力にUnity Particle SystemのCustom Coordを使用します。まずRow Countをテクスチャシートの行数に合わせて設定し、その後任意のCustom Coordチャンネルを「Random Between Two Constants」モード（範囲：0からRow Count）で設定し割り当てます。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Row Count</b></td><td>
<p>
<b>Random Row Selectionが有効な時のみ表示されます。</b>
</p>
<p>
テクスチャシートの行数を設定します（例：4×4のテクスチャシートの場合、Row Countを4に設定）。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Random Coord</b></td><td>
<p>
<b>Random Row Selectionが有効な時のみ表示されます。</b>
</p>
<p>
ランダム値の入力に使用するCustom Coordチャンネルを選択します。Unity Particle SystemのCustom Dataで「Random Between Two Constants」モードに設定したチャンネルと対応させます。
</p>
</td></tr>
<tr><td colspan="3"><b>Tone Mode</b></td><td>
<p>
輝度値に基づく色調調整のためのトーンマッピングを制御します。以下のオプションから選択します：
</p>
<p>
<ul>
<li><b>None:</b> トーンマッピングを適用しません（デフォルト）。</li>
<li><b>Tritone:</b> Highlights、Midtones、Shadowsの3段階トーンマッピング。</li>
<li><b>Pentone:</b> Highlights、Brights、Midtones、Darktones、Shadowsの5段階トーンマッピング。</li>
</ul>
</p>
<p>
境界値によって領域の閾値が決定され、設定された色にマッピングします。
各境界間の色は線形補間され、滑らかなトーンマッピングが適用されます。
なお、この線形補間はシェーダーで行われるため、プロジェクトの色空間を考慮してパラメータ設定を行う必要があることに注意してください。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Tone Channel</b></td><td>
<p>
<b>Tone ModeがNoneでない場合に表示されます。</b>
</p>
<p>
トーンマッピングの輝度計算に使用するカラーチャンネルを選択します。Red、Green、Blue、Alpha（デフォルト）から選択可能です。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Highlights Color</b></td><td>
<p>
<b>Tone ModeがTritoneまたはPentoneの時に表示されます。</b>
</p>
<p>
ハイライト領域に適用される色を設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Brights Color</b></td><td>
<p>
<b>Tone ModeがPentoneの時のみ表示されます。</b>
</p>
<p>
Brights領域（HighlightsとMidtonesの間）に適用される色を設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Midtones Color</b></td><td>
<p>
<b>Tone ModeがTritoneまたはPentoneの時に表示されます。</b>
</p>
<p>
ミッドトーン領域に適用される色を設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Darktones Color</b></td><td>
<p>
<b>Tone ModeがPentoneの時のみ表示されます。</b>
</p>
<p>
Darktones領域（MidtonesとShadowsの間）に適用される色を設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Shadows Color</b></td><td>
<p>
<b>Tone ModeがTritoneまたはPentoneの時に表示されます。</b>
</p>
<p>
シャドウ領域に適用される色を設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Highlights</b></td><td>
<p>
<b>Tone ModeがTritoneまたはPentoneの時に表示されます。</b>
</p>
<p>
ハイライトの輝度境界値を設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Brights</b></td><td>
<p>
<b>Tone ModeがPentoneの時のみ表示されます。</b>
</p>
<p>
MidtonesとHighlights間のBrights境界位置を制御します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Midtones</b></td><td>
<p>
<b>Tone ModeがTritoneまたはPentoneの時に表示されます。</b>
</p>
<p>
ShadowsとHighlights間の中間点位置を決定することで、トーン領域間のバランスを制御します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Darktones</b></td><td>
<p>
<b>Tone ModeがPentoneの時のみ表示されます。</b>
</p>
<p>
ShadowsとMidtones間のDarktones境界位置を制御します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Shadows</b></td><td>
<p>
<b>Tone ModeがTritoneまたはPentoneの時に表示されます。</b>
</p>
<p>
シャドウの輝度境界値を設定します。
</p>
</td></tr>
</tbody>
</table>

#### Tint Color
Tint Colorでは乗算色を指定できます。

<p align="center">
  <img width="70%" src="Documentation~/Images/TintColor.png" alt="ElderStar Inspector: TintColor"><br>
  <sub>Tint Color</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Mode</b></td><td>
<p>
乗算色の適用範囲を以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>None: なし（デフォルト）</li>
<li>All: 全面に適用</li>
<li>Rim: リムに適用</li>
</ul>
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Progress</b></td><td>
<p>
<b>ModeをRimに設定した時のみ表示されます。</b>
</p>
<p>
リムの範囲を設定します。<br>
この値が大きくすると、よりエッジに近い部分だけが着色されます。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Sharpness</b></td><td>
<p>
<b>ModeをRimに設定した時のみ表示されます。</b>
</p>
<p>
この値が大きいほどリムのエッジが鋭くなります。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Inverse</b></td><td>
<p>
<b>ModeをRimに設定した時のみ表示されます。</b>
</p>
<p>
リムの範囲を逆転させます。
</p>
</td></tr>
<tr><td colspan="3"><b>Color Mode</b></td><td>
<p>
乗算色の指定方法を以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Single Color: 単色（デフォルト）</li>
<li>Single Texture: テクスチャで指定</li>
<li>Flip Book: フリップブックアニメーション</li>
<li>Flip Book Blending: フリップブックアニメーション（補間あり）</li>
</ul>
</p>
<p>
選択したモードによって必要なテクスチャタイプが変わります。
</p>
</td></tr>
<tr><td colspan=3><b>Color</b></td><td>
<p>
<b>Color ModeをSingle Colorに設定した時のみ表示されます。</b>
</p>
<p>
乗算色を指定します。
</p>
</td></tr>
<tr><td colspan=3><b>Texture</b></td><td>
<p>
<b>Color ModeをSingle Texture、Flip Book、あるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
乗算色を表すテクスチャを設定します。
</p>
<p>
Color ModeをFlip Bookに設定した場合は<a href="https://docs.unity3d.com/2020.3/Documentation/Manual/class-Texture2DArray.html">Texture2DArray</a>を設定する必要があります。
Flip Book Blendingに設定した場合は<a href="https://docs.unity3d.com/2020.3/Documentation/Manual/class-Texture3D.html">Texture3D</a>を設定する必要があります。
</p>
</td></tr>
<tr><td colspan="3"><b>Rotation</b></td><td>
<p>
<b>Color ModeをSingle Texture、Flip Book、あるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
ティントマップの回転度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Offset</b></td><td>
<p>
ティントマップを回転する際の中心座標のオフセットです。
</p>
</td></tr>
<tr><td colspan=3><b>Flip-Book Progress</b></td><td>
<p>
<b>Color ModeをFlip BookあるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
フリップブックまたはフリップブックブレンディングの進行度合いです。
</p>
</td></tr>
<tr><td colspan=3><b>Blend Rate</b></td><td>
<p>
乗算色の適用率です。
</p>
</td></tr>
<tr><td colspan="3"><b>Random Row Selection</b></td><td>
<p>
<b>Color ModeをFlip BookあるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
UnityのTexture Sheet AnimationのRow Mode > Random機能と同等の機能です。有効にすると、各パーティクルがライフタイム中にランダムに1つの行を選択・維持してアニメーションを再生します。
</p>
<p>
ランダム値の入力にUnity Particle SystemのCustom Coordを使用します。まずRow Countをテクスチャシートの行数に合わせて設定し、その後任意のCustom Coordチャンネルを「Random Between Two Constants」モード（範囲：0からRow Count）で設定し割り当てます。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Row Count</b></td><td>
<p>
<b>Random Row Selectionが有効な時のみ表示されます。</b>
</p>
<p>
テクスチャシートの行数を設定します（例：4×4のテクスチャシートの場合、Row Countを4に設定）。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Random Coord</b></td><td>
<p>
<b>Random Row Selectionが有効な時のみ表示されます。</b>
</p>
<p>
ランダム値の入力に使用するCustom Coordチャンネルを選択します。Unity Particle SystemのCustom Dataで「Random Between Two Constants」モードに設定したチャンネルと対応させます。
</p>
</td></tr>
</tbody>
</table>

#### Flow Map
Flow Mapを使うとベースマップを指定した方向に歪ませることができます。

<p align="center">
  <img width="70%" src="Documentation~/Images/FlowMap.png" alt="ElderStar Inspector: FlowMap"><br>
  <sub>Flow Map</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Texture</b></td><td>
<p>
フローマップを設定します。フローマップの仕様は以下の通りです。
</p>
<p>
<ul>
<li>指定された2個のチャンネルのカラーの値に基づいて、サンプリングするUV値をずらします</li>
<li>チャンネルの指定はChannelsプロパティのXとYで行えます</li>
<li>値は0.5を基準として、小さいほどUV値がマイナス方向に、大きいほどプラス方向にずれます</li>
</ul>
色ではなく値としてテクスチャを使うので、テクスチャ設定のsRGB Colorのチェックを外す必要がある点に注意してください。
</p>
</td></tr>
<tr><td colspan="3"><b>Rotation</b></td><td>
<p>
フローマップの回転度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Offset</b></td><td>
<p>
フローマップを回転する際の中心座標のオフセットです。
</p>
</td></tr>
<tr><td colspan=3><b>Intensity</b></td><td>
<p>
フローマップを適用する際の強度を設定します。
</p>
</td></tr>
<tr><td colspan=3><b>Targets</b></td><td>
<p>
フローマップを適用する対象を設定します（複数選択可能）。

* Base Map
* Tint Map
* Alpha Transition Map
* Emission Map
</p>
</td></tr>
</tbody>
</table>

#### Parallax Map
Parallax Mapを使うと視差効果が出せます

<p align="center">
  <img width="70%" src="Documentation~/Images/ParallaxMap.png" alt="ElderStar Inspector: ParallaxMap"><br>
  <sub>Parallax Map</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Mode</b></td><td>
<p>
視差マップのモードを以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Single Texture: 通常の2Dテクスチャ（デフォルト）</li>
<li>Flip Book: Flip-Bookアニメーション</li>
<li>Flip Book Blending: ブレンドありのFlip-Bookアニメーション</li>
</ul>
</p>
<p>
選択したモードによってテクスチャの型が変わります。
</p>
</td></tr>
<tr><td colspan="3"><b>Texture</b></td><td>
<p>
視差マップを設定します。<br>
</p>
<p>
ModeにFlip Bookを指定した場合には、<a href="https://docs.unity3d.com/2020.3/Documentation/Manual/class-Texture2DArray.html">Texture2DArray</a>を設定する必要があります。<br>
ModeにFlip Book Blendingを指定した場合には、<a href="https://docs.unity3d.com/2020.3/Documentation/Manual/class-Texture3D.html">Texture3D</a>を設定する必要があります。<br>
</p>
視差マップの仕様は以下の通りです。
<p>
<ul>
<li>指定されたチャンネルのカラーの値に基づいて、サーフェスの凹み具合を変えます</li>
<li>値は0が元の状態で、1に近いほど凹み具合が上がります</li>
</ul>
色ではなく値としてテクスチャを使うので、テクスチャ設定のsRGB Colorのチェックを外す必要がある点に注意してください。
</p>
</td></tr>
<tr><td colspan="3"><b>Flip-Book Progress</b></td><td>
<p>
<b>ModeをFlip BookあるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
フリップブックまたはフリップブックブレンディングの進行度合いです。
</p>
</td></tr>
<tr><td colspan="3"><b>Random Row Selection</b></td><td>
<p>
<b>ModeをFlip BookあるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
UnityのTexture Sheet AnimationのRow Mode > Random機能と同等の機能です。有効にすると、各パーティクルがライフタイム中にランダムに1つの行を選択・維持してアニメーションを再生します。
</p>
<p>
ランダム値の入力にUnity Particle SystemのCustom Coordを使用します。まずRow Countをテクスチャシートの行数に合わせて設定し、その後任意のCustom Coordチャンネルを「Random Between Two Constants」モード（範囲：0からRow Count）で設定し割り当てます。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Row Count</b></td><td>
<p>
<b>Random Row Selectionが有効な時のみ表示されます。</b>
</p>
<p>
テクスチャシートの行数を設定します（例：4×4のテクスチャシートの場合、Row Countを4に設定）。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Random Coord</b></td><td>
<p>
<b>Random Row Selectionが有効な時のみ表示されます。</b>
</p>
<p>
ランダム値の入力に使用するCustom Coordチャンネルを選択します。Unity Particle SystemのCustom Dataで「Random Between Two Constants」モードに設定したチャンネルと対応させます。
</p>
</td></tr>
<tr><td colspan=3><b>Strength</b></td><td>
<p>
視差マップを適用する際の強度を設定します。
</p>
</td></tr>
<tr><td colspan=3><b>Targets</b></td><td>
<p>
視差マップを適用する対象を設定します（複数選択可能）。

* Base Map
* Tint Map
* Emission Map
</p>
</td></tr>
</tbody>
</table>

#### Color Correction
Color Correctionはここまでの色を補正します。

<p align="center">
  <img width="70%" src="Documentation~/Images/ColorCorrection.png" alt="ElderStar Inspector: ColorCorrection"><br>
  <sub>Color Correction</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Mode</b></td><td>
<p>
色調補正のモードを以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>None: 色調補正なし（デフォルト）</li>
<li>Greyscale: グレースケール化</li>
<li>Gradient Map: グラデーションマップを使って補正</li>
</ul>
</p>
</td></tr>
<tr><td><td colspan=2><b>Texture</b></td><td>
<p>
<b>ModeをGradient Mapに設定した時のみ表示されます。</b>
</p>
<p>
グラデーションマップを設定します。グラデーションマップの仕様は以下の通りです。
<ul>
<li>ここまでの色の輝度をグラデーションマップの色に置き換えます</li>
<li>輝度に応じてグラデーションマップをサンプリングする際のU値を変えます</li>
<li>輝度が最大の場合にグラデーションマップの右端がサンプリングされます</li>
<li>従って横方向にグラデーションがかかったテクスチャを使う必要があります</li>
</ul>
</p>
</td></tr>
</tbody>
</table>

#### Alpha Transition
Alpha Transitionは徐々に消えていく表現に使用します。

<p align="center">
  <img width="70%" src="Documentation~/Images/AlphaTransition.png" alt="ElderStar Inspector: AlphaTransition"><br>
  <sub>Alpha Transition</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Mode</b></td><td>
<p>
アルファトランジションのモードを以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>None: なし（デフォルト）</li>
<li>Fade: 薄くなって消えていくモード</li>
<li>Dissolve: 削れて消えていくモード</li>
</ul>
</p>
</td></tr>
<td colspan=3><b>Map Mode</b></td><td>
<p>
アルファトランジションマップのモードを以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Single Texture: 通常の2Dテクスチャ（デフォルト）</li>
<li>Flip Book: Flip-Bookアニメーション</li>
<li>Flip Book Blending: ブレンドありのFlip-Bookアニメーション</li>
</ul>
</p>
<p>
選択したモードによってテクスチャの型が変わります。
</p>
</td></tr>
<tr><td colspan="3"><b>Texture</b></td><td>
<p>
アルファトランジションマップを設定します。アルファトランジションマップの仕様は以下の通りです。
<ul>
<li>指定された1個のカラーチャンネルの値に基づいてアルファ値を変更します</li>
<li>チャンネルの指定はChannlesプロパティで行えます</li>
<li>値が小さい部分ほど消えやすく、大きい部分ほど最後まで残ります</li>
</ul>
</p>
<p>
ModeにFlip Bookを指定した場合には、<a href="https://docs.unity3d.com/2020.3/Documentation/Manual/class-Texture2DArray.html">Texture2DArray</a>を設定する必要があります。<br>
ModeにFlip Book Blendingを指定した場合には、<a href="https://docs.unity3d.com/2020.3/Documentation/Manual/class-Texture3D.html">Texture3D</a>を設定する必要があります。
</p>
<p>
また色ではなく値としてテクスチャを使うので、テクスチャ設定のsRGB Colorのチェックを外す必要がある点に注意してください。
</p>
</td></tr>
<tr><td colspan="3"><b>Rotation</b></td><td>
<p>
アルファトランジションマップの回転度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Offset</b></td><td>
<p>
アルファトランジションマップを回転する際の中心座標のオフセットです。
</p>
</td></tr>
<tr><td colspan="3"><b>Flip-Book Progress</b></td><td>
<p>
<b>Map ModeをFlip BookあるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
Flip-BookあるいはFlip-Book Blendingの進行度を設定します。
</p>
</td></tr>
<tr><td colspan="3"><b>Random Row Selection</b></td><td>
<p>
<b>Map ModeをFlip BookあるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
UnityのTexture Sheet AnimationのRow Mode > Random機能と同等の機能です。有効にすると、各パーティクルがライフタイム中にランダムに1つの行を選択・維持してアニメーションを再生します。
</p>
<p>
ランダム値の入力にUnity Particle SystemのCustom Coordを使用します。まずRow Countをテクスチャシートの行数に合わせて設定し、その後任意のCustom Coordチャンネルを「Random Between Two Constants」モード（範囲：0からRow Count）で設定し割り当てます。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Row Count</b></td><td>
<p>
<b>Random Row Selectionが有効な時のみ表示されます。</b>
</p>
<p>
テクスチャシートの行数を設定します（例：4×4のテクスチャシートの場合、Row Countを4に設定）。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Random Coord</b></td><td>
<p>
<b>Random Row Selectionが有効な時のみ表示されます。</b>
</p>
<p>
ランダム値の入力に使用するCustom Coordチャンネルを選択します。Unity Particle SystemのCustom Dataで「Random Between Two Constants」モードに設定したチャンネルと対応させます。
</p>
</td></tr>
<tr><td colspan="3"><b>Transition Progress</b></td><td>
<p>
トランジションの進行度を設定します。
</p>
</td></tr>
<tr><td colspan="3"><b>2nd Texture Blend Mode</b></td><td>
<p>
2枚目のテクスチャの合成方法を以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>None: 利用しない（デフォルト）</li>
<li>Average: 平均。2つのテクスチャの平均値を参照します。</li>
<li>Multiply: 乗算</li>
</ul>
</p>
</td></tr>
<tr><td colspan="3"><b>2nd Texture Map Mode</b></td><td>
<p>
<b>2nd Texture Blend ModeをAverageあるいはMultiplyに設定した時のみ表示されます。</b>
</p>
<p>
2枚目のアルファトランジションマップのMap Modeを1枚目のテクスチャとは独立して指定できます。以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Single Texture: 通常の2Dテクスチャ（デフォルト）</li>
<li>Flip Book: Flip-Bookアニメーション</li>
<li>Flip Book Blending: ブレンドありのFlip-Bookアニメーション</li>
</ul>
</p>
<p>
選択したモードによってテクスチャの型が変わります。
</p>
</td></tr>
<tr><td colspan="3"><b>2nd Texture Rotation</b></td><td>
<p>
<b>2nd Texture Blend ModeをAverageあるいはMultiplyに設定した時のみ表示されます。</b>
</p>
<p>
2枚目のアルファトランジションマップの回転度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Offset</b></td><td>
<p>
2枚目のアルファトランジションマップを回転する際の中心座標のオフセットです。
</p>
</td></tr>
<tr><td colspan="3"><b>Edge Sharpness</b></td><td>
<p>
<b>ModeをDissolveに設定した時のみ表示されます。</b>
</p>
<p>
エッジの鋭さを設定します。
</p>
</td></tr>
</tbody>
</table>

#### Emission
Emissionは発光する表現のために使用します。

<p align="center">
  <img width="70%" src="Documentation~/Images/Emission.png" alt="ElderStar Inspector: Emission"><br>
  <sub>Emission</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Mode</b></td><td>
<p>
エミッションのモードを以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>None: なし（デフォルト）</li>
<li>All: 全面をエミッションさせます</li>
<li>By Texture: エミッションさせる範囲をテクスチャにより指定します</li>
<li>Edge: エッジ（アルファ値が0より大きく1より小さい部分）をエミッションさせます</li>
</ul>
</p>
</td></tr>
<td colspan=3><b>Map Mode</b></td><td>
<p>
<b>ModeをBy Textureに設定した時のみ表示されます。</b>
</p>
<p>
エミッションマップのモードを以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Single Texture: 通常の2Dテクスチャ（デフォルト）</li>
<li>Flip Book: Flip-Bookアニメーション</li>
<li>Flip Book Blending: ブレンドありのFlip-Bookアニメーション</li>
</ul>
</p>
<p>
選択したモードによってテクスチャの型が変わります。
</p>
</td></tr>
<tr><td colspan="3"><b>Texture</b></td><td>
<p>
<b>ModeをBy Textureに設定した時のみ表示されます。</b>
</p>
<p>
エミッションマップを設定します。エミッションマップの仕様は以下の通りです。
<ul>
<li>指定された1個のチャンネルのカラーの値に基づいてエミッションのしやすさ変更します</li>
<li>チャンネルの指定はChannelsプロパティで行えます。</li>
<li>値が大きいほどエミッションしやすくなります</li>
</ul>
</p>
<p>
ModeにFlip Bookを指定した場合には、<a href="https://docs.unity3d.com/2020.3/Documentation/Manual/class-Texture2DArray.html">Texture2DArray</a>を設定する必要があります。<br>
ModeにFlip Book Blendingを指定した場合には、<a href="https://docs.unity3d.com/2020.3/Documentation/Manual/class-Texture3D.html">Texture3D</a>を設定する必要があります。
</p>
<p>
また色ではなく値としてテクスチャを使うので、テクスチャ設定のsRGB Colorのチェックを外す必要がある点に注意してください。
</p>
</td></tr>
<tr><td colspan="3"><b>Rotation</b></td><td>
<p>
<b>ModeをBy Textureに設定した時のみ表示されます。</b>
</p>
<p>
エミッションマップの回転度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Offset</b></td><td>
<p>
エミッションマップを回転する際の中心座標のオフセットです。
</p>
</td></tr>
<tr><td colspan="3"><b>Flip-Book Progress</b></td><td>
<p>
<b>ModeをBy Textureに設定し、Map ModeをFlip BookあるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
Flip-BookあるいはFlip-Book Blendingの進行度を設定します。
</p>
</td></tr>
<tr><td colspan="3"><b>Random Row Selection</b></td><td>
<p>
<b>ModeをBy Textureに設定し、Map ModeをFlip BookあるいはFlip Book Blendingに設定した時のみ表示されます。</b>
</p>
<p>
UnityのTexture Sheet AnimationのRow Mode > Random機能と同等の機能です。有効にすると、各パーティクルがライフタイム中にランダムに1つの行を選択・維持してアニメーションを再生します。
</p>
<p>
ランダム値の入力にUnity Particle SystemのCustom Coordを使用します。まずRow Countをテクスチャシートの行数に合わせて設定し、その後任意のCustom Coordチャンネルを「Random Between Two Constants」モード（範囲：0からRow Count）で設定し割り当てます。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Row Count</b></td><td>
<p>
<b>Random Row Selectionが有効な時のみ表示されます。</b>
</p>
<p>
テクスチャシートの行数を設定します（例：4×4のテクスチャシートの場合、Row Countを4に設定）。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Random Coord</b></td><td>
<p>
<b>Random Row Selectionが有効な時のみ表示されます。</b>
</p>
<p>
ランダム値の入力に使用するCustom Coordチャンネルを選択します。Unity Particle SystemのCustom Dataで「Random Between Two Constants」モードに設定したチャンネルと対応させます。
</p>
</td></tr>
<tr><td colspan="3"><b>Color Type</b></td><td>
<p>
エミッションの色を以下の選択肢から指定できます。
<ul>
<li>Color: 指定した単色をエミッション色として使用します</li>
<li>Base Color: ここまでのRGB値をエミッション色として使用します</li>
<li>Gradient Map: グラデーションマップを使ってエミッション色を指定します</li>
</ul>
Gradient MapはModeをBy TextureかEdgeに設定した時のみ選択可能です。
</p>
</td></tr>
<tr><td colspan="3"><b>Color</b></td><td>
<p>
<b>Color TypeをColorに設定した時のみ表示されます。</b>
</p>
<p>
エミッションの色をHDRカラーで指定します。
</p>
</td></tr>
<tr><td colspan="3"><b>Keep Edge Transparency</b></td><td>
<p>
<b>ModeをEdgeに設定した時のみ表示されます。</b>
</p>
<p>
チェックをつけると、エッジが透過しなくなります。
</p>
</td></tr>
<tr><td colspan="3"><b>Gradient Map</b></td><td>
<p>
<b>Color TypeをGradient Mapに設定した時のみ表示されます。</b>
</p>
<p>
グラデーションマップを設定します。グラデーションマップの仕様は以下の通りです。
<ul>
<li>ModeがBy Textureの場合: エミッションマップのR値をグラデーションマップのU値としてサンプリングします</li>
<li>ModeがEdgeの場合: アルファ値をグラデーションマップのU値としてサンプリングします</li>
<li>横方向にグラデーションがかかったテクスチャを使う必要があります</li>
</ul>
</p>
</td></tr>
<tr><td colspan="3"><b>Intensity</b></td><td>
<p>
エミッションの強さを設定します。
</p>
</td></tr>
</tbody>
</table>

#### Transparency
Transparencyは透明度を調整できます。

<p align="center">
  <img width="70%" src="Documentation~/Images/Transparency.png" alt="ElderStar Inspector: Transparency"><br>
  <sub>Transparency</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Rim</b></td><td>
<p>
チェックをつけるとリムを透過できます。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Progress</b></td><td>
<p>
透過の進行度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Sharpness</b></td><td>
<p>
この値が大きいほど透過領域のエッジが鋭くなります。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Inverse</b></td><td>
<p>
透過範囲を逆転させます。
</p>
</td></tr>
<tr><td colspan="3"><b>Luminance</b></td><td>
<p>
チェックをつけると輝度が小さい部分を透過させます。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Progress</b></td><td>
<p>
透過の進行度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Sharpness</b></td><td>
<p>
この値が大きいほど透過領域のエッジが鋭くなります。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Inverse</b></td><td>
<p>
透過範囲を逆転させます。
</p>
</td></tr>
<tr><td colspan="3"><b>Soft Particles</b></td><td>
<p>
チェックをつけるとソフトパーティクルを有効にします。<br>
Soft Particlesにはカメラの深度テクスチャが必要です。VRChatではワールド側の設定に依存します。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Intensity</b></td><td>
<p>
この値が大きいほど、透過する範囲が大きくなります。
</p>
</td></tr>
<tr><td colspan="3"><b>Depth Fade</b></td><td>
<p>
カメラに近い部分と遠い部分を透過させます。<br>
カメラとの距離を使用するため、シーン深度テクスチャは不要です。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Distance</b></td><td>
<p>
透過する範囲を設定します。<br>
カメラから見てNearより近い部分、Farより遠い部分が透過されます。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Width</b></td><td>
<p>
透過し始めてから完全に透過するまでの距離です。
</p>
</td></tr>
</tbody>
</table>

<a id="uber-lit"></a>
## Uber Litシェーダー
Uber LitシェーダーはNOVAのBRDFを使用し、Built-inのライト・影・環境光を反映する多機能シェーダーです。メッシュエフェクトなどのライティングの影響を受けたいパーティクルにはこのシェーダーを使用します。

Uber Litシェーダーを使用するには、マテリアルに`Nova/Particles/UberLit`シェーダーをアサインします。

Uber LitシェーダーはUber Unlitシェーダーにライティングのための処理とプロパティを追加したシェーダーです。<br>
追加されたプロパティの説明は以下の通りです。<br>


#### Render Settings
Render Settingsにはライティング用のプロパティが追加されています。

<p align="center">
  <img width="70%" src="Documentation~/Images/LitSettings.png" alt="ElderStar Inspector: LitSettings"><br>
  <sub>Render Settings</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Work Flow Mode</b></td><td>
<p>
PBRライティングのワークフローを指定できます。<br>
</p>
<p>
<ul>
<li>Specular</li>
<li>Metallic</li>
</ul>
</p>
なお、ワークフローに関する詳細は下記のURLを参照してください。
https://docs.unity3d.com/ja/2018.4/Manual/StandardShaderMetallicVsSpecular.html
https://docs.unity3d.com/ja/2018.4/Manual/StandardShaderMaterialParameterSpecular.html
https://docs.unity3d.com/ja/2018.4/Manual/StandardShaderMaterialParameterMetallic.html
<tr><td colspan="3"><b>Receive Shadows</b></td><td>
<p>
チェックをつけるとライトによる影を受けます。影を落とす設定はShadow Casterで行います。<br>
</p>
<tr><td colspan="3"><b>Specular Highlights</b></td><td>
<p>
チェックをつけるとスペキュラハイライトが有効になります。<br>
</p>
<tr><td colspan="3"><b>Environment Reflections</b></td><td>
<p>
チェックをつけるとReflection ProbeやSkyboxによる環境光の影響を受けるようになります。<br>
環境光の影響に関する詳細は下記URLの「Environment Reflection」を参照してください。<br>
https://docs.unity3d.com/ja/2018.4/Manual/GlobalIllumination.html
</p>
</tbody>
</table>

#### Surface Maps
ライティングのために必要なサーフェイスに関する各種情報の設定が追加されています。

<p align="center">
  <img width="70%" src="Documentation~/Images/SurfaceMaps.png" alt="ElderStar Inspector: SurfaceMaps"><br>
  <sub>Surface Maps</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Normal Map</b></td><td>
<p>
法線マップを設定します。法線マップの仕様は以下の通りです。<br>
<ul>
<li>設定できる法線マップはタンジェントスペース法線マップ</li>
<li>マップが指定されていない場合は頂点法線が使用される</li>
<li>法線のスケール値を指定できる。1.0で等倍となる</li>
</ul>
法線マップの詳細については下記のURLを参照してください。<br>
https://docs.unity3d.com/ja/2021.3/Manual/StandardShaderMaterialParameterNormalMap.html
</p>
<tr><td colspan="3"><b>Metallic Map</b></td><td>
<b>Work Flow ModeにMetallicを設定すると表示されます。</b>

メタリックマップを設定します。メタリックマップの仕様は以下の通りです。<br>
<ul>
<li>メタリックマップが設定されていない場合は、metallicプロパティの値が一律の金属度として使用される</li>
<li>メタリックマップが設定されている場合は、metallicプロパティが金属度の乗算値として使用される</li>
<li>Channelsで金属度が格納されているチャンネルを指定できる。デフォルトはRチャンネル</li>
</ul>
メタリックマップの詳細については下記のURLを参照してください。<br>
https://docs.unity3d.com/ja/2018.4/Manual/StandardShaderMaterialParameterMetallic.html
</ul>

<tr><td colspan="3"><b>Specular Map</b></td><td>
<b>Work Flow ModeにSpecularを設定すると表示されます。</b>

スペキュラマップを設定します。スペキュラマップの仕様は以下の通りです。<br>
<ul>
<li>スペキュラマップが設定されていない場合は、Specularプロパティのカラーの値が一律のスペキュラカラーとして使用される</li>
<li>スペキュラマップが設定されている場合は、Specularプロパティのカラーの値が乗算カラーとして使用される</li>
</ul>
スペキュラマップの詳細については下記のURLを参照してください。<br>
https://docs.unity3d.com/ja/2018.4/Manual/StandardShaderMaterialParameterSpecular.html
</ul>

<tr><td colspan="3"><b>Smoothness Map</b></td><td>
スムースネスマップを設定します。スムースネスマップの仕様は以下の通りです。<br>
<br>
<ul>
<li>スムースネスマップが設定されていない場合は、Smoothnessプロパティの値が一律の滑らかさとして使用される</li>
<li>スムースネスマップが設定されている場合は、Smoothnessプロパティの値が乗算値として使用される</li>
<li>Channelsで滑らかさが格納されているチャンネルを指定できる。デフォルトはαチャンネル</li>
</ul>
</tbody>
</table>

<a id="distortion"></a>
## Distortionシェーダー
Distortionは画面に対して歪み効果をかけるためのシェーダです。
熱波など、歪み効果が必要なエフェクトにはこのシェーダを使用します。

Distortionシェーダを使用するには、マテリアルに `Nova/Particles/Distortion` シェーダをアサインします。
Inspectorから設定可能な各プロパティの説明は以下の通りです。

#### Render Settings
Render Settingsではマテリアルの描画方法を制御できます。



<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Render Face</b></td><td>
<p>
描画する面を以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>Front: 前面のみ描画（デフォルト）</li>
<li>Back: 背面のみ描画</li>
<li>Both: 両面を描画</li>
</ul>
</p>
</td></tr>
<tr><td colspan="3"><b>ZTest</b></td><td>
<p>
ZTestを設定できます。
</p>
<p>
<ul>
<li>Disabled</li>
<li>Never</li>
<li>Less</li>
<li>Equal</li>
<li>Less Equal（デフォルト）</li>
<li>Greater</li>
<li>Not Equal</li>
<li>Greater Equal</li>
<li>Always</li>
</ul>
</p>
</td></tr>
</tbody>
</table>

#### Distortion
Distortionでは歪ませ方を設定できます。

<p align="center">
  <img width="70%" src="Documentation~/Images/Distortion.png" alt="ElderStar Inspector: Distortion"><br>
  <sub>Distortion</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Texture</b></td><td>
<p>
ディストーションマップを設定します。ディストーションマップの仕様は以下の通りです。
</p>
<p>
<ul>
<li>指定された2個のチャンネルのカラーの値に基づいて画面を歪ませます</li>
<li>チャンネルの指定はChannelsプロパティのXとYで行います</li>
<li>値は0.5を基準値として、基準値から離れるほど強く歪みます</li>
</ul>
色ではなく値としてテクスチャを使うので、テクスチャ設定のsRGB Colorのチェックを外す必要がある点に注意してください。
</p>
</td></tr>
<tr><td colspan="3"><b>Intensity Mode</b></td><td>
<p>
歪み強度の制御方法を指定します。
</p>
<p>
<ul>
<li>Single: X/Y共通の単一値で強度を制御します（デフォルト）。</li>
<li>XY: X軸とY軸で個別に強度を制御します。</li>
</ul>
</p>
</td></tr>
<tr><td colspan=3><b>Intensity</b></td><td>
<p>
<b>Intensity Mode が Single の場合にのみ表示されます。</b>
</p>
<p>
歪みの強さを設定します。
</p>
</td></tr>
<tr><td colspan=3><b>Intensity X / Intensity Y</b></td><td>
<p>
<b>Intensity Mode が XY の場合にのみ表示されます。</b>
</p>
<p>
各軸の歪み強度を個別に設定します。
</p>
</td></tr>
<tr><td colspan="3"><b>Rotation</b></td><td>
<p>
ディストーションマップの回転度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Offset</b></td><td>
<p>
ディストーションマップを回転する際の中心座標のオフセットです。
</p>
</td></tr>
<tr><td colspan="3"><b>Mirror Sampling</b></td><td>
<p>
チェックをつけるとテクスチャがミラーサンプリングされます。
</p>
</td></tr>
<tr><td colspan="3"><b>Use As Normal Map</b></td><td>
<p>
チェックをつけるとTexture TypeをNormal Mapとみなし、Unpackしてサンプリングされます。<br>
また、Textureには正規化されたNormal Mapを指定してください。そうでない場合はプラットフォームよって描画結果が変わる可能性があります。
</p>
</td></tr>
</tbody>
</table>

#### Flow Mapping
Flow Mapを使うとディストーションマップを指定した方向に歪ませることができます。

<p align="center">
  <img width="70%" src="Documentation~/Images/DistortionFlow.png" alt="ElderStar Inspector: DistortionFlow"><br>
  <sub>Flow Map</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Texture</b></td><td>
<p>
フローマップを設定します。フローマップの仕様は以下の通りです。
</p>
<p>
<ul>
<li>指定された2個のチャンネルのカラーの値に基づいて、サンプリングするUV値をずらします</li>
<li>チャンネルの指定は、ChannlesプロパティのXとYで行えます</li>
<li>値は0.5を基準として、小さいほどUV値がマイナス方向に、大きいほどプラス方向にずれます</li>
</ul>
色ではなく値としてテクスチャを使うので、テクスチャ設定のsRGB Colorのチェックを外す必要がある点に注意してください。
</p>
</td></tr>
<tr><td colspan="3"><b>Rotation</b></td><td>
<p>
フローマップの回転度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Offset</b></td><td>
<p>
フローマップを回転する際の中心座標のオフセットです。
</p>
</td></tr>
<tr><td colspan=3><b>Intensity</b></td><td>
<p>
フローマップを適用する際の強度を設定します。
</p>
</td></tr>
</tbody>
</table>

#### Alpha Transition
Alpha Transitionは徐々に消えていく表現に使用します。

<p align="center">
  <img width="70%" src="Documentation~/Images/DistortionAlpha.png" alt="ElderStar Inspector: DistortionAlpha"><br>
  <sub>Alpha Transition</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Mode</b></td><td>
<p>
アルファトランジションのモードを以下の選択肢から指定できます。
</p>
<p>
<ul>
<li>None: なし（デフォルト）</li>
<li>Fade: 薄くなって消えていくモード</li>
<li>Dissolve: 削れて消えていくモード</li>
</ul>
</p>
</td></tr>
<tr><td colspan="3"><b>Texture</b></td><td>
<p>
アルファトランジションマップを設定します。アルファトランジションマップの仕様は以下の通りです。
<ul>
<li>指定された1個のチャンネルのカラーの値に基づいてアルファ値を変更します</li>
<li>チャンネルの指定は、ChannlesプロパティのXで行えます</li>
<li>R値が小さい部分ほど消えやすく、大きい部分ほど最後まで残ります</li>
</ul>
</p>
<p>
色ではなく値としてテクスチャを使うので、テクスチャ設定のsRGB Colorのチェックを外す必要がある点に注意してください。
</p>
</td></tr>
<tr><td colspan="3"><b>Rotation</b></td><td>
<p>
アルファトランジションマップの回転度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan=2><b>Offset</b></td><td>
<p>
アルファトランジションマップを回転する際の中心座標のオフセットです。
</p>
</td></tr>
<tr><td colspan="3"><b>Progress</b></td><td>
<p>
トランジションの進行度を設定します。
</p>
</td></tr>
<tr><td colspan="3"><b>Edge Sharpness</b></td><td>
<p>
<b>ModeをDissolveに設定した時のみ表示されます。</b>
</p>
<p>
エッジの鋭さを設定します。
</p>
</td></tr>
</tbody>
</table>

#### Transparency
Transparencyは透明度を調整できます。

<p align="center">
  <img width="70%" src="Documentation~/Images/DistortionTransparency.png" alt="ElderStar Inspector: DistortionTransparency"><br>
  <sub>Transparency</sub>
</p>

<table width="100%">
<thead>
<tr><td colspan="3"><b>プロパティ名</b></td><td><b>説明</b></td></tr>
</thead>
<tbody>
<tr><td colspan="3"><b>Rim</b></td><td>
<p>
チェックをつけるとリムを透過できます。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Progress</b></td><td>
<p>
透過の進行度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Sharpness</b></td><td>
<p>
この値が大きいほど透過領域のエッジが鋭くなります。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Inverse</b></td><td>
<p>
透過範囲を逆転させます。
</p>
</td></tr>
<tr><td colspan="3"><b>Luminance</b></td><td>
<p>
チェックをつけると輝度が小さい部分を透過させます。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Progress</b></td><td>
<p>
透過の進行度合いを設定します。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Sharpness</b></td><td>
<p>
この値が大きいほど透過領域のエッジが鋭くなります。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Inverse</b></td><td>
<p>
透過範囲を逆転させます。
</p>
</td></tr>
<tr><td colspan="3"><b>Soft Particles</b></td><td>
<p>
チェックをつけるとソフトパーティクルを有効にします。<br>
Soft Particlesにはカメラの深度テクスチャが必要です。VRChatではワールド側の設定に依存します。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Intensity</b></td><td>
<p>
この値が大きいほど、透過する範囲が大きくなります。
</p>
</td></tr>
<tr><td colspan="3"><b>Depth Fade</b></td><td>
<p>
カメラに近い部分と遠い部分を透過させます。<br>
カメラとの距離を使用するため、シーン深度テクスチャは不要です。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Distance</b></td><td>
<p>
透過する範囲を設定します。<br>
カメラから見てNearより近い部分、Farより遠い部分が透過されます。
</p>
</td></tr>
<tr><td></td><td colspan="2"><b>Width</b></td><td>
<p>
透過し始めてから完全に透過するまでの距離です。
</p>
</td></tr>
</tbody>
</table>


<a id="ugui"></a>
## Uber Unlit/Litシェーダー（uGUI）

uGUI用には`Nova/UIParticles/UberUnlit`と`Nova/UIParticles/UberLit`を使用します。
マテリアルの設定は通常のUber Unlit/Litと共通で、Mask・RectMask2D・CanvasGroupに対応します。
Particle SystemをCanvas上に描画する仕組み自体は、別途用意してください。

Custom Dataを利用する場合は、Canvasの`Additional Shader Channels`に`TexCoord1`と`TexCoord2`を追加します。
uGUIではz・w成分が保持されないため、Custom Coordにはx・yを使ってください。LitではNormal・Tangentも必要です。

Canvas上の追加ライトには、任意の**`ElderStar > Canvas Lighting`**コンポーネントを利用できます。
`Collect Scene Lights`でライトを登録すると、Directional・Point・Spotから寄与の大きいライトを選び、1パスで評価します。
既定は4灯、最大8灯。元のマテリアルを変更せず、マスクや描画順を維持します。

<p align="center">
  <img width="70%" src="Documentation~/Images/CanvasLighting.png" alt="ElderStarのCanvas Lighting Inspector"><br>
  <sub>Canvas Lighting。通常のUnityシーン向けの任意機能です。</sub>
</p>

このライト一覧には影・Cookieは含まれません。[設定と制限](Documentation~/CanvasLighting.md)を参照してください。
VRChatアバターでは実Canvasやこの補助スクリプトを使わず、許可されたメッシュ／パーティクルの描画経路を使います。

<a id="shadow-caster"></a>
## 影を落とす機能について

Particle Uber Unlit/Litの**Shadow Caster**を有効にすると、エフェクトから影を落とせます。
Renderer側のCast Shadows、Light側のShadowsも設定してください。

<p align="center">
  <img width="70%" src="Documentation~/Images/ShadowCaster.png" alt="ElderStarのShadow Caster設定"><br>
  <sub>Shadow Caster</sub>
</p>

| プロパティ | 説明 |
| --- | --- |
| Enable | ShadowCasterパスを有効にします。 |
| Apply Vertex Deformation | 頂点変形を影にも反映します。 |
| Alpha Test Enable / Cutoff | アルファ値による影の切り抜き。色の描画とは別のしきい値です。 |
| Alpha Affected By | Tint Color、Flow Map、Alpha Transition Map、Transparency Luminanceのどれを影のアルファに反映するか選びます。 |

影の解像度・Bias・CascadeはBuilt-inのLight / Quality設定に従います。

<a id="custom-data"></a>
## Custom Vertex Streamsとの連携

Particle SystemのCustom Dataを使うと、粒ごとにマテリアルの値を変えたり、寿命に合わせてアニメーションさせたりできます。
通常のパーティクルとTrailの頂点ストリームに対応しています。以下はテクスチャ回転の例です。

### Custom Dataを設定する

Particle Systemの`Custom Data`モジュールを有効にし、`Custom1`をVectorに設定します。
X成分に、寿命の開始時に0、終了時に1となるカーブを設定します。

### Custom Vertex Streamsを設定する

Rendererの`Custom Vertex Streams`を有効にします。
Position・Normal・Color・UVに加え、**UV2**で`TEXCOORD0.zw`を埋めてからCustom1を配置し、
一覧の表示で`Custom1.x`が`TEXCOORD1.x`に割り当てられていることを確認します。
構成は描画モードによって変わるため、[Fix Now](#fix-now)を使うのが簡単です。

### マテリアルのプロパティを設定する

Base Mapの`Rotation`右側にあるCustom Coordから**COORD 1X**を選びます。
以後は各パーティクルのCustom1.xが回転量を駆動します。
同様にProgress、Intensity、UV Offsetなど、Custom Coord欄があるプロパティを制御できます。

Flip BookでRandom Row Selectionを使用する場合は、Row Countに行数を設定し、
別のCustom Data成分を`Random Between Two Constants`の0〜Row Countに設定してRandom Coordへ割り当てます。

<a id="gpu-instancing"></a>
## Mesh GPU Instancingを使う

Particle SystemのRendererで`Render Mode = Mesh`にし、`Enable Mesh GPU Instancing`を有効にします。
マテリアル側のGPU Instancingも有効にしてください。Shader Model 4.5対応環境が必要です。

Custom Vertex Streamsは、`INSTANCED1.xyzw`と`INSTANCED2.xyzw`を過不足なく埋める構成にします。
Custom1・Custom2を使用する一般的な構成はFix Nowで設定できます。独自ストリームを使う場合は、割り当て表示を確認してください。

詳細は[UnityのParticle System GPU Instancing](https://docs.unity3d.com/2022.3/Documentation/Manual/PartSysInstancing.html)を参照してください。

<a id="fix-now"></a>
## Custom Vertex Streamsを自動的に設定する

マテリアルが必要とする頂点ストリームとRendererの設定が異なる場合、Inspector下部に警告と**Fix Now**が表示されます。
クリックすると、そのマテリアルを使うRendererに必要な標準構成が設定されます。
通常のマテリアルとTrail Materialの両方を確認するため、手動でストリームを組む前にこの機能を利用してください。

<a id="unused-references"></a>
## 不要なパラメータ参照を削除する

Single Textureに切り替えたあとも、使わないTexture2DArrayなどへの参照が残ることがあります。
不要な参照はビルドに含まれる素材を増やす原因になります。

Projectビューで対象マテリアルを選び、**`Tools > ElderStar > RemoveUnusedReferences`**を実行します。
削除された参照はConsoleに表示されます。再び別のモードで使う予定があるテクスチャは、設定を戻す際に再指定してください。

<a id="optimization"></a>
## 最適化シェーダーの利用によるメモリ使用量の削減

Uberシェーダーは多くのキーワードとパスを持ちます。
使用するRender Typeと必要なパスを絞ったシェーダーを生成し、マテリアルを差し替えることで、不要なバリアントを減らせます。
削減量は使用する機能・プラットフォーム・ビルド設定に依存します。

| API | 説明 |
| --- | --- |
| `OptimizedShaderGenerator.Generate(outputFolderPath)` | Particle Uber Lit/Unlitの最適化シェーダーを生成します。 |
| `OptimizedShaderReplacer.Replace(settings)` | 指定フォルダー内のマテリアルを対応する最適化シェーダーへ差し替えます。 |

### 生成と適用

生成と適用には次のEditor APIを使用します。先に生成し、その後でマテリアルを差し替えます。
スクリプトは`Editor`フォルダーに置き、対象フォルダーを指定してからメニューを実行してください。

```csharp
using Nova.Editor.Core.Scripts.Optimizer;
using UnityEditor;

public static class EffectShaderOptimization
{
    [MenuItem("Tools/My Effects/Optimize")]
    public static void Run()
    {
        OptimizedShaderGenerator.Generate("Assets/ElderStarOptimized");
        OptimizedShaderReplacer.Replace(new OptimizedShaderReplacer.Settings
        {
            TargetFolderPath = "Assets/MyEffects/Materials",
            OpaqueRequiredPasses = OptionalShaderPass.ShadowCaster,
            CutoutRequiredPasses = OptionalShaderPass.ShadowCaster,
            TransparentRequiredPasses = OptionalShaderPass.None
        });
    }
}
```

生成先・対象フォルダーは実際のプロジェクトに合わせてください。
Built-inではカメラ深度の生成にもShadowCasterが使われるため、影を表示しない場合でも必要なことがあります。
DepthNormalsの出力を使う場合は、そのパスも残してください。UI・Distortionはこの生成対象に含まれません。
詳細は[Editor APIリファレンス](Documentation~/EditorAPI.md)を参照してください。

<a id="builtin-helpers"></a>
## Built-in向けの補助機能

通常のUnityプロジェクトでは、必要に応じて次の機能を追加できます。
いずれもVRChatアバターの基本的な描画には不要です。

| 機能 | 用途 |
| --- | --- |
| [Screen Space Distortion](Documentation~/BuiltInRenderingAPI.md) | モノラルカメラで歪みをまとめて合成。Camera・CommandBuffer・RenderTextureを使うAPIもあります。 |
| [Canvas Lighting](Documentation~/CanvasLighting.md) | 通常のCanvasに最大8灯を1パスで反映。 |
| [DepthNormals](Documentation~/DepthNormals.md) | NOVAオブジェクトの法線・有効領域を専用テクスチャへ出力。世界全体の深度法線テクスチャは上書きしません。 |
| [Ambient Lighting](Documentation~/AmbientLighting.md) | ローカルのLight Probeと全体の環境反射用の環境光を分離して渡します。 |
| [LPPV・Rendering Diagnostics](Documentation~/ProbeVolumesAndDiagnostics.md) | 空間的な間接光、Albedo・法線・深度・光など8種類のEditor表示。 |

<a id="compatibility"></a>
## 互換性と動作確認

NOVA 3.6.0の標準シェーダー名・素材プロパティ・GUID・公開Editor APIを維持しています。
一般的なマテリアル操作とエフェクト制作の流れは本家と共通です。
描画基盤がBuilt-inになるため、次の点には違いがあります。

| 項目 | ElderStarでの扱い |
| --- | --- |
| ライト・影・環境光 | Built-inのライト・Shadow・Probeを使用します。同じ素材でもシーン側の入力により見た目が変わります。 |
| 歪みの重なり | シェーダー単独では各面から背景を参照します。複数の非ゼロの歪みをURPと同じ方法で累積するわけではありません。通常Unityにはカメラ合成の選択肢があります。 |
| Soft Particles | カメラの深度テクスチャが必要です。VRChatアバターからワールドの深度設定を強制できません。 |
| 透明物の描画順 | 同じ深度・同じソート条件では順序が変わる場合があります。必要な場合はRendererのSorting Orderを明示します。 |
| Forward+ / APV | Built-inの追加ライト・LPPVで対応します。URPのタイル処理やAPVデータ形式の互換実装ではありません。 |
| URP専用C# API | Renderer Feature・RenderGraphなどを呼ぶコードにはBuilt-in用の書き換えが必要です。素材の直接読み込みとは別の範囲です。 |

Unity 2022.3 / Unity 6でEditor・描画・画像比較・パッケージ導入を検証しています。
VRChat PCのSDKビルドとAAO既定設定での最適化後の描画も検証していますが、すべてのアバター・ワールド・GPUでの一致を保証するものではありません。
実機PCVRと最新変更を反映した実クライアントでの確認は継続項目です。

詳しい条件と機能別の対応状況は[互換性表](Documentation~/Compatibility.md)、
検証の記録は[リポジトリの検証ドキュメント](https://github.com/smizuoch/ElderStar/blob/main/Documentation/Verification.md)を参照してください。

<a id="editor-api"></a>
## Editor APIリファレンス

[RendererErrorHandler・OptimizedShaderGenerator・OptimizedShaderReplacer](Documentation~/EditorAPI.md)の使い方をまとめています。
既存コードとの互換性のため、名前空間は`Nova.Editor.Core.Scripts`などの元の名前を維持しています。

<a id="license"></a>
## ライセンス

ElderStarは[MITライセンス](LICENSE.md)で提供します。
NOVA Shaderを基にした独立したBuilt-in移植であり、CyberAgent公式の製品ではありません。

シェーダー・Editor・機能リファレンスの原典：
[NOVA Shader / CyberAgent, Inc.](https://github.com/CyberAgentGameEntertainment/NovaShader)
（Copyright 2021 CyberAgent, Inc.）。本READMEの機能表・説明は同プロジェクトのMITライセンスのドキュメントを基に調整しています。
作例素材のクレジットと再撮影条件は[画像について](Documentation~/Images/README.md)に記載しています。
素材を別途導入する場合は、その素材のライセンスにも従ってください。

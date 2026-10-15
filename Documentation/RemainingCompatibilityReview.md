# Remaining compatibility review — 2026-10-15

全29項目を再評価した記録です。29件すべてを解消したという報告ではありません。
今回修正した計算差と、描画基盤・世界の入力・実機検証に依存する残件を区別します。
Exactは34/63、Approximateは29/63のままです。全入力の再現率は定義できません。

| Feature | 今回の結果 | Exactにするため残る条件 |
|---|---|---|
| Material setup | 既存の初期値・設定遷移テストを再実行。歪みの3100キューは背景取得に必要なまま。 | URP separately schedules distortion; a shader-only GrabPass is invoked at its material queue. |
| Material migration | 標準GUID・Fire001直接利用をSDKで再検証。任意の独自URPシェーダーを無変換では実行できない。 | A shader GUID cannot safely identify both original URP code and a different Built-in implementation in one import. |
| Render state | 全描画回帰とSDK/AAOの重なりを再検証。heal02の同順位ソートは過去の反例として保持。 | Stable ordering of equal queue/distance/sorting keys belongs to each engine renderer, beyond an individual fragment shader. |
| Parallax targets | 曲面・負スケール・符号を含む既存回帰を再実行。依頼済みの有限値ガードを維持。 | The requested finite 1e-4 guard is intentionally different from original singular arithmetic. |
| Soft particles | 本家数式との35描画比較で20失敗を再現し修正。正投影・斜交・負強度・微小強度が一致。 | A shader cannot recover opaque/transparent geometry absent from the world camera depth texture. |
| Instanced stereo | 15件の左右眼配列描画と最悪条件のコンパイルを再検証。実HMDは未実施。 | Texture-array simulation and compilation do not prove the real XR runtime path. |
| Metallic and specular workflows | 両Litの既存BRDF回帰を再実行。世界のライト割当の差は数式の変更では解消できない。 | Final workflow output still consumes different engine light/probe assignments. |
| Surface maps | 既存の2D/Array/3D表面マップ描画とSDKの素材保持を再検証。最終照明入力の差は残る。 | Map sampling is preserved; complete surface output depends on independently approximate scene lighting. |
| Main light | 既存ライト回帰を再実行。アバターへ届かないライト一覧から本家の主光源を再選択できない。 | Built-in and URP select and bind main lights differently; avatars do not own the scene light list. |
| Additional lights | ForwardAdd・UI補助・影の回帰を再実行。エンジンによるライト制限・Cookie入力の差は残る。 | ForwardAdd scheduling/attenuation differs from URP and a finite single-pass Canvas list omits shadows/cookies. |
| Receive shadows | 既存の受影・無効化・組合せを再検証。世界のシャドウアトラスとPCFの完全置換は未実装。 | The Built-in shadow atlas, bias and cascade selection cannot be replaced by an avatar shader alone. |
| Ambient and light probes | 局所の赤いSHが全体の反射光に混入する差を再現。任意カメラ補助で分離しFlat/Trilight/Skyboxと状態復元を検証。VRCの無スクリプト経路では差が残る。 | Global ambient-probe DC is not separately supplied to the scriptless avatar shader. |
| Environment reflections | 既存の反射・粗さ回帰を再実行。今回の全体環境光補助はエンジンの反射プローブ選択は変更しない。 | Engine probe selection/capture differs; NOVA itself does not provide blending/box-projection variants. |
| Scene selection and picking | 明示パスの描画とEditor API回帰を再実行。ネイティブ画面の選択イベントは新規検証していない。 | Explicit pass drawing already works; native Editor invocation is a separate integration contract. |
| Shadow caster | 既存Caster描画・コンパイルを再実行。世界側バイアス・アトラス差は残る。 | World-owned shadow scheduling/atlas conventions differ. |
| DepthOnly | 既存明示パスとクリップ回帰を再実行。自動深度生成がShadowCasterを用いる仕様は変えない。 | Built-in auto-depth uses ShadowCaster; an explicit DepthOnly pass is not an automatically scheduled URP prepass. |
| DepthNormals | カメラ補助・左右眼・生成シェーダーの回帰を再実行。物理深度の計算は今回のSoftParticles修正から分離した。 | NOVA dedicated texture is implemented, but does not own or replace world depth normals. |
| Forward+ | 既存ForwardAdd/Canvas代替の回帰を再実行。クラスタ型描画基盤は今回の承認計画の対象外。 | No tiled/clustered renderer is shipped; ForwardAdd is not algorithm/API-compatible Forward+. |
| Probe volumes | 既存LPPV空間読み取りの回帰を再実行。APVのファイル形式・ストリーミングは承認計画の対象外。 | LPPV does not implement APV placement, data formats, streaming or scenarios; world data is required. |
| Foveated rendering | D3Dの座標変換が恒等の範囲を維持。実XRランタイムの密度制御は未検証。 | D3D remapping is identity, but shader tests cannot establish runtime density-control compatibility. |
| Debug display | 8表示とビルド時除去の回帰を再実行。URPデバッグサービスの型や呼出し互換は提供しない。 | The implemented eight views use a Built-in Editor helper; URP debug service/API is absent. |
| UI lighting | 既存Canvas・Mask・光源選択の回帰を再実行。Overlay実画面・追加光の影/Cookieは未解消。 | The optional eight-light list has no shadow atlas/cookie manager; real Canvas/custom runtime components are outside VRC avatar scope. |
| Camera distortion | 既存実行可能APIとカメラ回帰を再実行。URP型を引数にするAPIは無依存パッケージの制約と両立しない。 | Executable Built-in APIs preserve selected call forms but cannot implement URP base classes/parameters without URP. |
| Distortion buffer | SDK/AAO後のゼロ強度重なり・鏡を再検証。非ゼロ歪みの全物体加算と8x/9x差は残る。 | A shared GrabPass lacks a per-camera additive offset texture; extra copies or rendering have real cost. Inline8 vs forced9 remains. |
| Distortion depth and visibility | ソフト粒子の計算を修正し既存可視性回帰を再実行。世界の深度にない遮蔽物は復元できない。 | Missing world camera depth cannot be reconstructed from only the avatar surface. |
| Distortion camera types | 既存ネスト/反射/Preview回帰を再実行。実XR・Editorの統合イベント網羅ではない。 | Nested mono Game/Reflection/Preview now pass; unsupported compositor camera types still select the shader fallback. |
| Optimizer pass stripping | 48生成名・パス保持除去・コンパイルの回帰を再実行。パスの自動呼出しは各エンジンのまま。 | Pass presence/strip flags work; pipeline invocation differences remain. |
| Optimizer includes | 既存のパッケージ参照・出力移動テストを再実行。元パッケージ移動時の参照修復は未実装。 | Previously generated shaders contain include paths; preserving legacy helper signatures does not repair moved package paths. |
| Platform scope | Unity 2022/6・D3D11/12・SDK/AAOを再検証。別GPU・PCVR実機は未検証。 | Windows D3D coverage cannot establish every Unity GPU/platform behavior. |

URP型そのものへの依存、APVのデータ読み込み、世界の深度/照明をアバターだけで所有することは、現在の制約を保ったまま同等にはできません。代替機能が動くことと、元API・全入力でExactであることを同一視しません。

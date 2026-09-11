# 最終演出・ゲームオーバーの設定ガイド

2026-09-11 更新。対象シーン：Assets/Scenes/MainScene.unity。

## 最初に開く設定

Projectで Assets/Player/Forms/FinalEventTimeline.asset を選択してください。
開始・終了時刻は小節／拍／拍内分割の1始まりで指定します。
1小節4拍、BPM170、譜面の先頭は音源先頭から約1.412秒です。
アセットの Song は MainSceneSong のまま使用してください。

| 内容 | 初期設定 | 主な項目 |
|---|---|---|
| ボス登場 | 85小節1拍→86小節1拍 | Boss Enter Start / End、Boss Enter Viewport |
| 黒塗り解除 | 101小節1拍→102小節1拍 | Boss Reveal Start / End |
| ボスの浮遊 | 幅0.15、4拍で1往復 | Boss Float Amplitude / Period Beats |
| 自動取得 | 113小節1拍（448拍） | Item Collect Timing |
| アイテム接近 | 取得の2拍前から | Item Homing Duration Beats / Start Viewport / Curve |
| ビーム拡大 | 取得時→114小節4拍 | Beam Full Timing、最大長・太さ、カーブ |
| 1回目の白転 | 114小節4拍→115小節1拍→115小節3拍 | First White Start / Peak / End |
| プレイヤー接近 | 115小節3拍→117小節1拍 | Approach Start / Contact Timing / Curve |
| 2回目の白転 | 116小節4拍→117小節1拍 | Second White Start / Credits Start |
| テキスト | 117小節から4項目 | Credits |
| スコア表示 | 121小節から | Creditsの4項目目 |
| 1枚目のスチル | 125小節1拍 | First Still Timing |
| 2枚目のスチル | 1枚目から2拍後 | Second Still Delay Beats |
| リトライ解禁 | 曲終了時 | GameFlowControllerが制御 |

現在の曲終了は MainSceneSong の Game End Beat = 504（126小節分）です。
117～122小節の指定と最後の125～126小節の間をつなぐため、123～124小節にも最後のスコア付きテキストを保持します。
制作名は未指定のため「制作：クレジットを入力してください」は仮文です。CreditsのMessageを編集してください。

## 表示のしくみと調整

ボスは黒色＋透明から、画面上方より移動しつつ表示します。
黒塗り解除はSpriteRendererのRGBを黒→白へ変化させ、元画像の色を戻します。
上下運動は曲の拍を使うため、専用AnimationClipは不要です。
取得時のボス位置を引き継ぎ、Boss Settle Beatsを使って浮遊幅を0へ戻します。

Viewport座標は左下が(0,0)、中央が(0.5,0.5)、右上が(1,1)です。
Player Target Viewport、Boss Target Viewportで構図を変更できます。
移動の緩急は各Curveで調整します。

ビームに溜め光・命中光・発光画像は使用しません。Square.pngを拡大します。
Beam Maximum Length、Beam Extend Beatsで長さと伸びる時間を指定します。
Beam Start Thickness、Beam Maximum Thickness、Beam Full Timingで太さを設定します。
拡大中は同じ進行率で、ビームを薄くしながらキャラクター背面の白背景をフェードインします。
Beam Fade Curveはビームの透明度（初期値1→0）、White Backdrop Fade Curveは白背景の透明度（初期値0→1）です。
Cover Screen Height / Widthが有効な場合、設定した最大寸法よりも画面を覆う寸法を優先します。
幅の自動拡大は発射端も左へ広げます。固定射程にしたい場合はCover Screen Widthを外してください。
最終演出とゲームオーバー中はカメラのPostProcessingを停止し、発光や露出補正の影響を除いています。リトライで元の設定に戻ります。

1回目の白転が完全に白になる時点で、ビームを消し、背景を白にし、
ボスをboss_damage_normal、プレイヤーをplayer_idle_magicへ変更します。
接近は接触点同士がContact Timingに一致するように計算します。
見た目に合わせた接触位置は下記ContactPointのTransformで調整してください。
2回目の白転完了後、キャラを非表示にして白背景上のテキストに切り替えます。

Creditsの各要素で開始／終了、フェード時間、本文、文字サイズ、色、位置を設定できます。
本文の {score} が今回の確定スコアに置き換わります。
時間が重なる場合は後ろの項目が優先です。
最後のテキストは1枚目のスチル開始から同時にフェードアウトします。
1枚目／2枚目の切替は Cut / Fade / Dissolve を選択できます。

## MainSceneのHierarchyと役割

| 階層 | 役割／設定 |
|---|---|
| FinalEventRoot | FinalEventController。曲・プレイヤー・UGUIなどの参照をInspectorから接続 |
| FinalEventRoot/BossRoot | ボス全体の登場・演出移動。旧SpriteRendererは重複表示防止のため無効 |
| FinalEventRoot/BossRoot/BossVisual | ボスの画像、サイズ、浮遊、色。Scaleで大きさを調整 |
| FinalEventRoot/BossRoot/BossVisual/ContactPoint | ボス側の接触位置。Local Positionで調整 |
| PlayerRoot/VisualRoot/FinalContactPoint | プレイヤー側の接触位置。Local Positionで調整 |
| FinalEventRoot/BossRoot/BossAura | 黒い四角のParticleSystem。最大48粒、放出20粒/秒。サイズ・寿命・範囲を調整可能 |
| FinalEventRoot/FinalBeam | 発光なしビーム。実行中のサイズはTimelineが制御 |
| FinalEventRoot/ChargeFlash、ImpactFlash | 旧光演出。削除せず無効化して保管 |
| FinalEventRoot/SolidBackdropCanvas/SolidBackdrop | キャラより後ろに描画する白／黒の背景。白転・ゲームオーバー用 |
| FinalEventRoot/EndingCanvas/WhiteoutOverlay | キャラより前面の白転画像 |
| FinalEventRoot/EndingCanvas/EventStill | Ending_01の表示先 |
| FinalEventRoot/EndingCanvas/EndingStill | Ending_02の表示先。曲終了後も保持 |
| FinalEventRoot/EndingCanvas/EventText | クレジット表示先。Fontと文字領域はここ、本文・サイズ・位置はTimelineで設定 |
| FinalEventRoot/EndingCanvas/FullscreenRetry | 透明な全画面Image＋Button。RetryButtonの遷移先をInspectorで変更可能 |
| FinalEventRoot/EndingCanvas/EventBackground | 旧終幕背景。無効化して保管 |
| CanvasBack/BossRoot (1) | 旧重複ボス。無効化して保管 |
| Items/StandaloneItems | 以前ルート直下にあった単体アイテムを格納。Items全体の非表示と連動 |
| BackgroundSystems以下の各背景Image | ImageDissolveControllerを追加。背景全レイヤーのディゾルブに対応 |

UGUIはすべてシーンに事前配置し、実行中のUI生成は行いません。
パーティクルは1つのParticleSystemで管理し、発射時に放出停止と粒子消去を行います。
ディゾルブ用Materialだけは画像ごとの進行率を独立させるため実行時に複製し、終了時に解放します。

## ゲームオーバー

敵・アイテム・通常UI・通常背景・ボスを隠し、黒背景とプレイヤーだけを表示します。
画面全体の透明ボタンでMainSceneへリトライできます。文字やボタン背景は出しません。
カメラはその場で進行を停止し、プレイヤーを画面横30%、縦50%に配置します。
ゲームオーバーと曲終了の両方でスコア送信が予約されます。

## ディゾルブの使い方

背景：BackgroundSystemsのBackgroundTransitionControllerにある
SwitchBackgroundDissolve(int)をSongSequenceControllerのUnityEventに登録し、
背景セット番号を引数に指定します。Dissolve Durationは秒です。
既存の背景イベントは勝手にディゾルブへ変更していません。

単体画像：ImageDissolveControllerのTarget ImageとDissolve MaterialをInspectorで指定します。
MaterialはAssets/Materials/FinaleUIDissolve.matです。
ShowDissolve / HideDissolveをUnityEventから呼び出せます。
Duration、Noise Scale、Softness、Reverseで見え方を変更できます。
FinalEventTimelineのスチルはFirst/Second Still TransitionをDissolveにするだけで使用できます。
UITransitionControllerのTransition Styleとは別コンポーネントなので、同じ画像の同時操作は避けてください。

## unityroomランキング

GameManager > ScoreRankingSenderで Board No = 1、Write Mode = HighScoreDescを使用します。
高いスコアのみ記録する設定を維持しています。得点の表示と送信には同じ確定値を使います。
クリア時はクレジット開始時に予約し、曲終了時の二重予約を防ぎます。
ゲームオーバー時にも予約します。
導入済みクライアントが通信と再試行を担当します。パッケージ本体と認証キーは変更していません。

Submission Stateで結果を確認できます。
- Queued：送信予約済み
- EditorSimulation：Editorでの模擬送信。サイトへは送っていません
- Succeeded：クライアントが通信成功を通知
- Failed：通信失敗（クライアントの再試行中を含む）
- NotImproved：クライアントがハイスコア未更新と判断

確認済み：ボード番号・書込ルール、認証キーの設定とBase64形式、
クリア時の送信経路、ゲームオーバー時の12345点の模擬送信。
未確認：unityroom公開版の実通信とランキング画面への反映。
公開版で失敗する場合はブラウザConsoleの[unityroom]スコア送信失敗ログとHTTP応答を確認してください。
既存ハイスコア以下の場合、降順ハイスコアの仕様により記録は更新されません。
認証キーはConsoleや共有資料へ貼り付けないでください。

## 保存場所と後からの移動

変更したスクリプト：
- Assets/Scripts/FinalEventController.cs
- Assets/Scripts/FinalEventDefinition.cs
- Assets/Scripts/FinalItemAutoCollector.cs（処理は継続利用。今回のコード変更なし）
- Assets/Scripts/GameFlowController.cs
- Assets/Scripts/ScoreRankingSender.cs
- Assets/Scripts/Audio/BeatTiming.cs
- Assets/Scripts/Background/BackgroundTransitionController.cs

新規スクリプト：Assets/Scripts/UI/ImageDissolveController.cs
新規Shader：Assets/Shaders/UIFinaleDissolve.shader
新規フォルダ：Assets/Shaders、Assets/Materials
新規Material：FinaleUIDissolve.mat、FinaleSpriteUnlit.mat、FinaleAura.mat（Assets/Materials内）

スクリプト・Shader・MaterialはUnityのProjectウィンドウで移動すれば参照を維持できます。
エクスプローラーで移動する場合は必ず.metaも一緒に移動してください。
通常スクリプトをEditorフォルダや別のasmdef配下へ移すとビルド対象や参照が変わるため避けてください。
実行時コードは保存先パスを直書きして読み込んでいません。

## 動作確認

- Unityコンパイル：エラー0。既存の管理名未使用警告3件
- アイテム自動取得経路から最終演出開始、ボス・プレイヤー・ビーム表示
- ビーム拡大終点で画面全体を白い帯が覆う
- 白転のピークでSprite切替、粒子数0
- 接触予定拍でContactPoint間の距離0
- スコア付きテキスト、Ending_02、曲終了後の保持
- 中央／画面左上でEventSystem経由の透明リトライ
- 黒背景とプレイヤーのみのゲームオーバー
- 背景12画像のディゾルブ進行と背景セット切替完了
- スチルのディゾルブ途中の画面確認
- Editor内のスコア送信予約・模擬送信

確認はPlayModeの音楽時計を一時固定した場面検証を含みます。
実WebGL端末での性能測定とunityroomの実ランキング更新は未実施です。

# 最終イベント設定ガイド

最終イベントの調整は `Assets/Player/Forms/FinalEventTimeline.asset` のInspectorから行います。

## 最終アイテムの取得拍

- `Item Collect Beat`: 曲の先頭から数えた、最終アイテムを取得する絶対拍です。
- `Item Homing Duration Beats`: 取得の何拍前からホーミングを始めるかを指定します。
- `Item Homing Start Viewport`: アイテムが現れる画面内座標です。Xを1より大きくすると画面右外から現れます。
- `Item Homing Curve`: 0から1までの移動速度変化です。Inspectorのカーブを編集できます。

`MainScene / PowerUps / item_Gear_LastAttack` の `FinalItemAutoCollector` が上記設定を読みます。通常接触用Colliderは実行時に無効になり、指定拍に必ず自動取得されます。

## 取得後の相対拍

以下の拍数は、最終アイテムを取得した瞬間を0拍とした相対拍です。

- `Player Move Start Beat` / `Player Move Duration Beats`: プレイヤー移動の開始拍と長さ
- `Player Target Viewport`: プレイヤーの到達位置
- `Player Move Curve`: プレイヤー移動の速度変化
- `Boss Move Start Beat` / `Boss Move Duration Beats`: ボス移動の開始拍と長さ
- `Boss Start Viewport` / `Boss Target Viewport`: ボスの開始位置と到達位置
- `Boss Move Curve`: ボス移動の速度変化

Viewportは画面左下が `(0, 0)`、右上が `(1, 1)` です。

## プレイヤーとボスの画像アニメーション

- `Player Move Frames`: プレイヤー移動中に繰り返す画像
- `Player Attack Switch Beat`: 攻撃画像へ切り替える拍
- `Player Attack Frames`: 攻撃中に繰り返す画像
- `Player Frame Duration Beats`: 1枚を表示する拍数
- `Boss Move Frames`: ボス移動中に繰り返す画像
- `Boss Damage Switch Beat`: ボス被弾画像へ切り替える拍
- `Boss Damage Frames`: ボス被弾中に繰り返す画像
- `Boss Frame Duration Beats`: 1枚を表示する拍数

各Framesには最低1枚を登録してください。1枚なら静止画、2枚以上なら指定拍間隔で繰り返すアニメーションになります。

## ビームと光

- `Charge Start Beat` / `Charge Duration Beats`: 溜め光の開始拍と長さ
- `Beam Start Beat` / `Beam Duration Beats`: ビームの開始拍と長さ
- `Beam Extend Beats`: 最大長まで伸びる拍数
- `Beam Start Thickness`: 発射直後の太さ（ワールド単位）
- `Beam End Thickness`: 終了直前の太さ（ワールド単位）
- `Beam Start Offset`: プレイヤー中心から発射口までの右方向距離
- `Impact Start Beat` / `Impact Duration Beats`: 命中光とボス振動の開始拍と長さ
- `Flash Diameter`: 発射光と命中光の大きさ
- `Boss Shake Distance`: ボスの振動幅

ビームの太さはTransformの倍率ではなく `SpriteRenderer.size.y` へ直接反映されます。

## ゲームUIと演出用UGUI

- `Hide Gameplay Ui Beat`: ライフ、スコア、デバッグ表示を含む通常Canvasを隠す拍
- `Image Cues`: 背景、演出用スチル、最後のスチルを表示するリスト
- `Text Cues`: 演出文と最後のメッセージを表示するリスト

Image Cueでは次を設定します。

- `Layer`: `Background`、`Event Still`、`Ending Still` の表示先
- `Start Beat` / `End Beat`: 表示開始拍と表示終了拍
- `Fade In Beats`: 表示開始から不透明になるまでの拍数。0なら即時表示です。
- `Sprite` / `Color`: 表示画像と色

Text Cueでは開始拍、終了拍、フェード時間、文章、文字サイズ、色、画面内位置を指定できます。同じレイヤーまたはテキストで時間が重なった場合は、リストの後ろにある設定を優先します。

## Hierarchy上の演出オブジェクト

- `MainScene / FinalEventRoot`: 最終イベント全体を管理
- `BossRoot`: ボス画像
- `FinalBeam`: `effect_beam` の表示
- `ChargeFlash`: 発射前の光
- `ImpactFlash`: 命中時の光
- `EndingCanvas / EventBackground`: 演出用背景
- `EndingCanvas / EventStill`: イベント途中のスチル
- `EndingCanvas / EndingStill`: 最後のスチル
- `EndingCanvas / EventText`: 演出文と最後のメッセージ

最終アイテムは取得音を鳴らしません。取得時に再生中のSEも停止し、その後は通常攻撃と被弾が止まるためSEは発生しません。BGMはそのまま進行します。

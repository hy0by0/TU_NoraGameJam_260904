# ② 盗賊形態の5連撃

## 動作

FiveHit形態で1回攻撃すると、0・0.25・0.5・0.75・1拍の位置で計5回、既存のAttackAreaを有効にします。各発の判定は0.125拍で閉じ、5発目が終わってから1拍待機します。初期設定では開始から次の攻撃まで2.125拍です。

「1拍に4体の間隔」に合わせた初期値です。以前の「5/32」の表現を固定値としては採用していません。

各発ごとに命中／空振り音と得点を確定します。別々の発で1体ずつ5体を倒した場合は通常の5体分の得点です。同じ発で複数を倒した場合には既存の同時撃破倍率が適用されます。5体をまとめて倒した扱いにはしません。

連撃中の追加入力は受け付けません。同形態の再取得では連撃を維持し、別形態への変更・被弾・Player無効化では残りの連撃と斬撃表示を停止します。別形態への変更では攻撃後の待機を維持します。

通常形態・Finaleはこれまでの単発攻撃です。Beamは③で専用ビームに対応しました（BeamAttackGuide.mdを参照）。

## 調整する場所

Projectで `Assets/Player/Forms/FiveHit.asset` を選択してください。

| Inspector項目 | 初期値 | 意味 |
|---|---:|---|
| Combo Spacing Beats | 0.25 | 各発の開始間隔 |
| Combo Hit Duration Beats | 0.125 | 各発の判定時間 |
| Combo Cooldown Beats | 1 | 5発目終了後の待機時間 |

回数は5回固定です。判定の切れ目を保つため、判定時間には開始間隔の95%を上限として適用します。これらの設定はFiveHitだけに使用されます。PlayerControllerの既存のAttack Duration Beats / Attack Interval Beatsは他形態の単発攻撃用です。

## アニメーション

通常は `player_idle_thief` を表示します。

5連撃終了後のインターバル中は `player_attack_thief` を静止表示し、待機終了後に `player_idle_thief` へ戻ります。被弾時には被弾表示を優先します。インターバル中の攻撃判定と斬撃エフェクトは無効です。

`Assets/Player/Forms/FiveHitAttack.anim` に、前半が `player_attack_thief`、後半が `player_idle_thief` の1周期を作成しています。この1周期を各発に合わせて5回再生します。Animationウィンドウで編集できます。

連撃時はBGM時計に合わせて再生位置を指定するため、AnimatorのSpeedが0になるのは正常です。判定時間を変更した場合も、前半と後半がそれぞれ判定中／判定の切れ目に合うように再生します。通常攻撃や被弾に戻ると通常の再生速度制御へ復帰します。

## Hierarchyの追加・変更

MainSceneの `PlayerRoot/VisualRoot/SlashEffect` に斬撃を追加しました。Player Prefabでは `Player/VisualRoot/SlashEffect` です。最初は非アクティブで、連撃中だけ表示します。実行時には生成しません。

| オブジェクト | 役割・設定 |
|---|---|
| PlayerRoot | PlayerControllerのSlash Effectに斬撃、Damage Colliderに本体Colliderを接続。攻撃・被弾の制御 |
| PlayerRoot/VisualRoot | 既存PlayerAnimationControllerで盗賊の交互表示をBGMと同期 |
| PlayerRoot/VisualRoot/SlashEffect | 新規。Transformで相対位置・大きさ・回転、SpriteRendererで色・描画順、Animatorで使用するアニメーションを調整 |
| PlayerRoot/CollisionRoot | 既存配置を維持。PlayerControllerから被弾用Colliderとして参照 |
| PlayerRoot/AttackArea | 配置・サイズを維持。既存の判定を5回開閉 |

SlashEffectのローカル位置は `(3, 0, 0)`、ローカルScaleは `(0.8, 1.1, 1)` です。VisualRootのScaleを引き継ぐため、この値はワールド座標の大きさとは異なります。見た目の斬撃は攻撃判定を持たず、当たり判定の大きさはAttackAreaで調整してください。

`Assets/Player/Forms/Combo/SlashLoop.anim` に `effect_slash_01` と `effect_slash_02` の交互表示を設定しています。同じフォルダーの `SlashEffect.controller` で使用しています。1発の開始間隔で1周期を繰り返し、判定の切れ目も含めて連撃終了まで表示します。

## 変更したスクリプトとファイル整理

- 新規 `Assets/Scripts/Player/PlayerSlashEffect.cs`：配置済み斬撃の表示／非表示と曲に同期した再生。
- 変更 `Assets/Scripts/Player/PlayerController.cs`：5連撃、待機と中断、本体接触と攻撃範囲接触の区別。
- 変更 `Assets/Scripts/Player/PlayerFormDefinition.cs`：連撃の間隔・判定時間・待機時間。
- 変更 `Assets/Scripts/Player/PlayerAnimationController.cs`：連撃の周期に合わせた再生位置指定。

新規フォルダーは `Assets/Player/Forms/Combo` です。すべてInspector参照のため、UnityのProjectウィンドウから移動して整理できます。外部で移動するときは.metaも一緒に移動してください。スクリプトをEditorフォルダーや別asmdefへ移す場合は実行環境が変わるため別途調整が必要です。DOTween本体は変更していません。

## 検証

Unityコンパイルはエラー0件、既存背景スクリプトの警告3件。

Play Modeを一時停止し、BGM時刻・コルーチン・Physics2Dを細かく進める17項目の検証が成功しました。5回の判定時刻、等間隔の5体の撃破と500点の加点、交互画像、斬撃2枚、待機、本体接触による1回の被弾、各中断処理を確認しています。判定タイミングの検証では、1発ずつの撃破を区別するため検証中だけ判定幅を狭くしました。保存済みAttackAreaのサイズは変更していません。

Game Viewで盗賊の攻撃絵と斬撃の重なりを確認済み。修正後の実行エラーは0件。検証用の敵と座標変更はPlay Mode内のみで、MainSceneには保存していません。

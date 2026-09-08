# 敵・攻撃の設定ガイド

## 今回の動作
- 敵は一撃で倒れます。HPはありません。
- 攻撃範囲が閉じた時に「今回倒した敵の基礎点合計 × 倍率」を加算します。被弾で中断しても、撃破済みの点数は加算します。
- 初期値は敵1体100点、1体なら1倍、同じ攻撃で2体以上なら2倍です。100点の敵2体なら400点、3体なら600点です。
- 「同時」は同じ1回の攻撃範囲が有効な間に倒した敵として集計します。異なる攻撃の撃破数は合算しません。
- 命中音は最初の敵を倒した時に1回だけ鳴ります。敵を倒さなかった時だけ、攻撃範囲が閉じた時に空振り音が鳴ります。
- 同じ敵の複数Colliderが当たっても、撃破・スコア・命中通知は重複しません。

## MainSceneのHierarchyとInspector

| 対象 | 役割・設定 |
| --- | --- |
| PlayerRoot / PlayerController | Multi Kill Threshold：倍率が変わる撃破数（初期2）。Single Kill Multiplier：しきい値未満の倍率（初期1）。Multi Kill Multiplier：しきい値以上の倍率（初期2）。Normal Hit/Miss SE：通常の命中／空振り音。Enhanced Hit/Miss SE：強化時の命中／空振り音。Is Enhanced：現在の強化状態。 |
| PlayerRoot/AttackArea / AttackHitBox | 1回の攻撃の撃破を集計。Player Controllerに通知先を紐づけ。既存のBoxCollider2Dで攻撃範囲を調整できます。 |
| Enemies/Enemy_normal (5)～(11) / Enemy | Enemy Type：敵の種類。Score Value：敵ごとの基礎点（初期100）。 |
| 同じ7体 / EnemyMovement（今回追加したコンポーネント） | 下記の移動設定。Music ControllerとEnemy Rigidbodyはシーンで紐づけ済みです。 |
| Canvas/ScoreText / ScoreUI（既存） | 現在スコアを6桁で表示。今回の加点も既存の表示へ反映します。 |

新しいGameObjectやUIは作成していません。既存の敵へコンポーネントを追加しました。AudioSystemの既存SESourceを利用します。

## 敵の移動設定

- Movement Style
  - Stationary：移動しない。
  - Constant：Direction方向へSpeed（ワールド単位/秒）の一定速度で移動し続ける。
  - Harmonic：Direction方向に単振動する。Distanceが振幅、Duration Secondsが一周期の秒数。開始位置から正方向へ動き、正負両側を往復する。
  - Eased：Direction方向にDistanceだけ移動する。Duration Secondsが片道の秒数。Easingで加減速を選ぶ。Ping Pongが有効なら往復し続け、無効なら終点で止まる。
- Directionは方向だけを使用します。上は(0, 1)、左は(-1, 0)。長さを変えても速さは変わりません。(0, 0)なら移動しません。
- Use Placed Positionが有効なら、Hierarchyで配置したワールド位置から開始します。無効ならStart Positionのワールド座標へゲーム開始時に配置します。
- Start Timingは既存の曲設定に合わせた、小節・拍・拍内分割の1始まり指定です。開始前は開始位置で待機します。
- Music Controllerが停止・一時停止している間は移動しません。位置は曲の再生秒から計算します。
- DOTweenの既存のイージング計算を利用しています。DOTween本体は変更していません。

配置済みの例（すべて開始は1小節1拍1分割、シーン配置位置を使用）:

| Hierarchy（Enemies配下） | 動き |
| --- | --- |
| Enemy_normal (5)、(6)、(7)、(8) | 静止 |
| Enemy_normal (9) | 上方向へ毎秒0.25単位の等速移動 |
| Enemy_normal (10) | 上下に振幅1単位・周期2秒の単振動 |
| Enemy_normal (11) | 上方向へ1単位、片道2秒、InOutSineで往復 |

新しい移動敵は、このシーンの敵を複製すると参照も引き継げます。Enemy_normal.prefabから新規配置する場合は、必要な敵にEnemyMovementを追加し、Music ControllerへAudioSystem、Enemy Rigidbodyへその敵のRigidbody2DをInspectorから設定してください。物理移動にはKinematicのRigidbody2Dを使用します。

## 後から追加するプレイヤー・強化状態

各プレイヤーのPlayerControllerに4種類の音を設定できます。今回、通常命中には「攻撃ヒット.ogg」、通常空振りには「空振り.ogg」を設定しました。強化専用の素材はまだないため、強化側にも同じ2素材を仮設定しています。後からEnhanced Hit SE / Enhanced Miss SEを差し替えてください。

強化アイテムなどの処理からSetEnhanced(true)、解除時はSetEnhanced(false)を呼び出します。音は攻撃開始時の強化状態を使うため、攻撃途中に強化状態が変わっても今回の攻撃音は変わりません。Player.prefabにも音を設定済みです。異なるプレイヤーの実装や強化アイテム自体の処理は今回の対象外です。

## 変更ファイル

- Assets/Scripts/GImmik&Enemy/Enemy.cs（変更）
- Assets/Scripts/GImmik&Enemy/EnemyMovement.cs（新規）
- Assets/Scripts/Player/AttackHitBox.cs（変更）
- Assets/Scripts/Player/PlayerController.cs（変更）
- Assets/Scenes/MainScene.unity（参照・移動設定を保存）
- Assets/Prefabs/Player.prefab（効果音・倍率設定を保存）

新しいフォルダは作成していません。EnemyMovement.csはUnityのProjectウィンドウ内で移動すれば、.metaのGUIDが保持されるため参照を維持できます。ファイルだけをOS上で移動して.metaを失うことや、別のasmdefのフォルダへ移動することは避けてください。

## 確認結果

Unity CLI Loopによるコンパイル：エラー0、既存の背景スクリプトの未使用変数警告3件。
再生中の確認：実際のPhysics2Dで完全に重ねた敵2体を一撃で撃破し400点・命中通知1回。1体への重複Collider通知は100点のみ。EndAttackを繰り返しても追加加点なし。空振りは0点。攻撃中断時も撃破済み100点を保持。既存UGUIに000600を表示。等速・単振動・イージングの座標計算を確認。実行時エラー0。

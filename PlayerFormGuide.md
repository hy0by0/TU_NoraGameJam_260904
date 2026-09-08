# プレイヤー形態切り替え（①）

## 今回できること

強化アイテムを取得すると、指定した形態の通常・攻撃・被弾アニメーション、上下移動速度、攻撃の命中音／空振り音へ切り替わります。被弾音は全形態で共通です。

攻撃判定は全形態とも既存の単発攻撃です。5連撃、ビーム、倍率変更、自動発射、終盤イベントは②以降で実装します。通常アイテム・ハートの効果は従来どおり加点のみで、今回回復機能は追加していません。

## 形態を調整する場所

Projectの `Assets/Player/Forms` にある設定アセットを選択します。

| アセット | 形態 | 上下移動速度の初期値 | 見た目 |
|---|---|---:|---|
| Normal.asset | 通常 | 10 | 既存の通常素材を保持 |
| FiveHit.asset | 5連撃用（盗賊） | 12 | thiefの通常／攻撃素材 |
| Beam.asset | ビーム用（魔法） | 10 | magicの通常／攻撃素材 |
| Finale.asset | 終盤用 | 10 | player_LastAttackを仮設定 |

速度12は比較用の仮設定です。`Move Speed` で自由に変更できます。横方向のステージ進行速度には影響しません。

- `Kind`：形態の識別名。①ではこの値による特殊攻撃は行いません。
- `Animations`：通常・攻撃・被弾のクリップをまとめたAnimator Override Controller。
- `Hit SE`：この形態の攻撃が最初に命中した時の音。
- `Miss SE`：この形態の攻撃が敵を倒さず終了した時の音。

専用の強化攻撃音は未指定のため、現在は全形態に既存の「攻撃ヒット」「空振り」を割り当てています。別の音声を各アセットにドラッグすれば切り替わります。

## アニメーションの変更

各 `～Animations.overrideController` を選び、`PlayerMoveAnimation`（通常）、`PlayerAttackAnimation`（攻撃）、`PlayerDamageAnimation`（被弾）の置換先を変更します。

現在の通常・攻撃クリップは既存仕様に合わせた1枚絵のアニメーションです。被弾クリップには各形態の通常画像と点滅を設定しています。専用の連番アニメーションができたらクリップを置き換えられます。再生速度は設定した攻撃時間／無敵時間に合わせて計算されます。

終盤形態の3クリップには同じ `player_LastAttack` を仮に使用しています。被弾だけ点滅します。専用演出の実装は④です。既存のPlayerMissAnimationは変更していません。

## HierarchyとInspector

MainSceneの配置位置は変更していません。GameObjectの新規追加もありません。

| 場所 | 追加・変更した役割 | 設定できる内容 |
|---|---|---|
| `MainScene / PlayerRoot` | PlayerFormControllerを追加。PlayerControllerに参照を接続 | Initial Formで開始形態。PlayerControllerのDamage SEで共通被弾音。既存の攻撃時間・待機時間・無敵時間も引き続き設定可能 |
| `MainScene / PlayerRoot / VisualRoot` | PlayerAnimationControllerを追加 | AnimatorとSpriteRendererの参照、使用するステート名と元クリップ名。通常は変更不要 |
| `MainScene / PowerUps / item_Gear_thief` と同名の `(1)` ～ `(6)` | 既存7個のItemへFiveHit.assetを接続 | Item Type、Score Value、Hit SE（取得音）、Target Form（取得後の形態） |

`Assets/Prefabs/Player.prefab` の `Player` と `Player/VisualRoot` にも同じコンポーネントと参照を保存しています。曲などシーン固有の参照は従来どおりシーン側で設定します。

別の強化形態を試すには、配置済み強化アイテムの `Target Form` を `Beam.asset` または `Finale.asset` へ変更してください。`Item Type` は Wide / Speed / FinalSword のいずれかが強化対象です。変更先は名前から自動判定せず、Target Formの参照に従います。Normal / Heartは形態を変更しません。

既存の取得音は保持しています。必要に応じて各ItemのHit SEに「強化アイテム取得」を指定してください。

## 状態が重なる場合

- 同じ形態の再取得：加点と取得音だけ処理し、攻撃や無敵時間をリセットしません。
- 攻撃中に別形態を取得：旧攻撃の獲得済み得点・音を一度だけ確定し、判定を閉じます。その時点から既存の攻撃後待機時間を適用します。
- 攻撃後の待機中に取得：残り待機を維持します。既存Prefabの待機設定は0拍のため、待機を使う場合はAttack Interval Beatsを調整してください。
- 被弾中に取得：残り無敵時間・攻撃禁止時間と被弾アニメーションの進行率を維持し、新しい姿へ変更します。
- 無敵中に攻撃が解禁された場合：攻撃表示を優先します。無敵終了が攻撃表示を上書きすることはありません。
- 被弾しても強化は解除しません。被弾による攻撃中断の扱いは既存の仕様を維持しています。
- 無効化時にはコルーチンと攻撃判定を終了し、再有効化時に表示を復帰します。

## スクリプトの場所と整理

新規：

- `Assets/Scripts/Player/PlayerFormDefinition.cs`：形態設定アセットの定義。
- `Assets/Scripts/Player/PlayerFormController.cs`：初期形態と現在形態の保持。
- `Assets/Scripts/Player/PlayerAnimationController.cs`：表示と再生速度の一元管理。

変更：

- `Assets/Scripts/Player/PlayerController.cs`：形態設定の利用、共通被弾音、攻撃中断と表示復帰。
- `Assets/Scripts/GImmik&Enemy/Item.cs`：取得の一本化と重複防止、形態変更。

新規フォルダーは `Assets/Player/Forms` です。設定アセット4個、Override Controller4個、アニメーションクリップ12個を保存しています。

参照に固定ファイルパスは使用していません。UnityのProjectウィンドウからファイルやフォルダーを移動すれば参照を維持できます。外部で整理する場合は.metaも一緒に移動してください。スクリプトをEditorフォルダーや別のasmdef配下へ移す変更は実行環境が変わるため対象外です。

## 確認結果

- Unityコンパイル：エラー0件。既存の背景スクリプトの未使用フィールド警告3件。
- Play Modeで状態を直接操作する検証25項目：全件成功。全4形態の切り替え、重複取得、攻撃中変更、同形態再取得、待機中変更、被弾中変更、無敵中攻撃、被弾終了後の表示と不透明度復帰を確認。
- 実際のPhysics2D接触から、盗賊形態への変更・一度だけの加点・取得物の無効化を確認。
- Game Viewで盗賊の表示を確認。実行後のConsoleエラー0件。

被弾・攻撃の終了順の検証は一時停止中にコルーチンを操作して行っています。5連撃のリズムやビームの見た目・音の最終調整は今回の確認対象ではありません。

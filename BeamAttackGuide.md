# ③ 魔法形態のビーム

## 動作

Target Formが `Beam.asset` の強化アイテムを取得すると魔法形態になります。

- 通常：`player_idle_magic`
- 攻撃：`player_attack_magic`
- ビーム：`effect_beam` を右方向へ伸ばし、徐々に細くして消します。
- 発射光：発射位置に水色の光を表示し、伸び切るまでに消します。Unity組み込みの丸いスプライトを代用しています。

表示と判定は0.35拍、0.08拍で最大長になり、終了後は1拍待機します。次の攻撃は開始から約1.35拍後です。発射中はプレイヤーの位置に追従し、上下移動も引き続き可能です。ビームは敵を貫通します。壁による遮断は追加していません。

ビームと通常のAttackAreaは同時に使いません。被弾・別形態への変更・Playerの無効化でビームと発射光と判定を消します。途中までの撃破は一度だけ加点します。同形態の再取得では発射をリセットしません。別形態への変更では待機時間を維持します。

## 設定する場所

Projectで `Assets/Player/Forms/Beam.asset` を選びます。

| Inspector項目 | 初期値 | 意味 |
|---|---:|---|
| Beam Duration Beats | 0.35 | ビームの表示・判定時間 |
| Beam Extend Beats | 0.08 | 最大長まで伸びる時間。表示時間が上限 |
| Beam Cooldown Beats | 1 | ビームが消えてからの待機時間 |
| Beam Length | 8 | 最大射程。ワールド単位 |
| Beam Thickness | 0.8 | 開始時の最大の太さ。ワールド単位 |
| Beam Single Kill Multiplier | 1 | 1体撃破時の倍率 |
| Beam Multi Kill Multiplier | 3 | 複数撃破時の倍率 |
| Hit SE | 既存の攻撃ヒット | 最初の命中で一度だけ鳴らす音 |
| Miss SE | 既存の空振り | 一体も倒さなかった発射の終了時に鳴らす音 |

時間・射程・倍率は仮の初期値です。通常・連撃の設定には影響しません。

複数撃破の判定数は、既存のPlayerControllerのMulti Kill Thresholdを使用します（通常は2）。判定期間全体の撃破数を一発として集計するため、伸びる途中で倒した敵も含みます。

例：100点の敵を3体倒すと `(100 + 100 + 100) × 3 = 900点` です。既存の通常攻撃倍率と二重に掛けません。

現状のEnemyには個別の撃破音再生がなく、プレイヤー側が最初の命中で一度だけ音を鳴らします。同じ敵の複数Colliderや、物理接触と範囲検査の両方で検出した場合も撃破・音・加点を重複させません。空振り音は命中した発射では鳴りません。

## Hierarchy

MainSceneでは `PlayerRoot`、Prefabでは `Assets/Prefabs/Player.prefab` の `Player` に反映しています。

| オブジェクト | 役割・設定できる内容 |
|---|---|
| PlayerRoot | PlayerControllerのBeam AttackにBeamRootを紐づけ。形態によって攻撃方式を選択 |
| PlayerRoot/BeamRoot | 新規。発射位置。TransformのLocal Positionでプレイヤーからの位置を調整。初期値は `(6,0,0)`。PlayerのScaleを引き継ぐためワールド座標では約0.75右。PlayerBeamAttackのFlash Sizeで発射光の最大直径を調整 |
| PlayerRoot/BeamRoot/BeamVisual | 新規。SpriteRenderer、BoxCollider2D、AttackHitBoxを配置。SpriteRendererで素材・色・描画順を設定。素材は右へ向けるためFlip Xを有効化 |
| PlayerRoot/BeamRoot/MuzzleFlash | 新規。発射光。Local Positionで発射光だけの位置、SpriteRendererで色・素材・描画順を調整 |

BeamRootは初期状態では非アクティブです。全オブジェクトはHierarchyに配置済みで、ゲーム中には生成しません。

BeamVisualの位置・Scale・ColliderのサイズとOffsetは表示に合わせて毎回更新します。射程や太さはTransformではなくBeam.assetで調整してください。発射光のScaleと透明度も毎回更新されます。

判定は表示の外接矩形に合わせたBoxCollider2Dです。画像内の透明部分までピクセル単位で判定する仕組みではありません。光の尾の細かい凹凸は矩形で近似します。

## スクリプト

- 新規 `Assets/Scripts/Player/PlayerBeamAttack.cs`：伸長・細まり・発射光と判定寸法の更新。
- 変更 `Assets/Scripts/Player/PlayerController.cs`：ビーム攻撃の開始・終了・待機・中断、ビーム専用倍率。
- 変更 `Assets/Scripts/Player/PlayerFormDefinition.cs`：ビームの設定項目。
- 変更 `Assets/Scripts/Player/AttackHitBox.cs`：伸縮後の即時範囲検査。既存の一度だけの撃破集計を共用。

BeamMove.animとBeamAttack.animに指定の魔法画像を設定しています。今回、新しいアセット用フォルダーは作成していません。

参照はInspectorで設定しています。UnityのProjectウィンドウから移動して整理できます。外部で移動するときは.metaも一緒に移動してください。スクリプトをEditorフォルダーや別asmdefへ移す場合は実行環境が変わるため別途調整が必要です。

表示と判定を同じBGM時刻で更新するため、今回はDOTweenを使用していません。導入済みDOTween本体は変更していません。

## 検証

コンパイルはエラー0件、既存の背景スクリプトの警告3件です。

Play ModeでBGM時刻とコルーチンを進める21項目の検証が成功しました。伸長前の非命中、射程と太さ、表示と判定のXY寸法一致、範囲内3体の撃破、範囲外の生存、複数Colliderの重複防止、命中通知1回、倍率加点、空振り、待機、形態変更・実際の本体接触・無効化による中断を確認しています。

Game Viewで魔法形態から右へ伸びるビームを確認しました。検証後の実行エラーは0件です。確認用の敵・座標・時刻変更はPlay Mode内のみで、シーンへ保存していません。

④の自動発射・ボス演出・終盤イベントは今回の対象外です。

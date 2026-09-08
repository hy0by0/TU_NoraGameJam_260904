using System;
using UnityEngine;

/// <summary>最終イベントで画像を表示するUGUIレイヤーです。</summary>
public enum FinalEventImageLayer { Background, EventStill, EndingStill }

/// <summary>指定した相対拍の間だけ、指定レイヤーへ画像を表示する設定です。</summary>
[Serializable]
public class FinalEventImageCue
{
    public FinalEventImageLayer layer;
    [Min(0f)] public float startBeat;
    [Min(0f)] public float endBeat = 1f;
    [Min(0f)] public float fadeInBeats = 0.25f;
    public Sprite sprite;
    public Color color = Color.white;
}

/// <summary>指定した相対拍の間だけ、最終イベント用UGUIテキストを表示する設定です。</summary>
[Serializable]
public class FinalEventTextCue
{
    [Min(0f)] public float startBeat;
    [Min(0f)] public float endBeat = 1f;
    [Min(0f)] public float fadeInBeats = 0.25f;
    [TextArea(2, 5)] public string message;
    [Min(1)] public int fontSize = 40;
    public Color color = Color.white;
    public Vector2 anchoredPosition = new Vector2(0f, -190f);
}

/// <summary>最終アイテムの自動取得から終幕表示までを、拍単位で調整する設定アセットです。</summary>
[CreateAssetMenu(menuName = "Final Event/進行設定", fileName = "FinalEventTimeline")]
public class FinalEventDefinition : ScriptableObject
{
    [Header("最終アイテム自動取得（曲先頭からの絶対拍）")]
    [SerializeField, Min(0f)] private float itemCollectBeat = 490f;
    [SerializeField, Min(0.01f)] private float itemHomingDurationBeats = 2f;
    [SerializeField] private Vector2 itemHomingStartViewport = new Vector2(1.08f, 0.65f);
    [SerializeField] private AnimationCurve itemHomingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("プレイヤー移動（取得時を0拍とした相対拍）")]
    [SerializeField, Min(0f)] private float playerMoveStartBeat;
    [SerializeField, Min(0.01f)] private float playerMoveDurationBeats = 2f;
    [SerializeField] private Vector2 playerTargetViewport = new Vector2(0.25f, 0.42f);
    [SerializeField] private AnimationCurve playerMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("ボス移動（取得時を0拍とした相対拍）")]
    [SerializeField, Min(0f)] private float bossMoveStartBeat;
    [SerializeField, Min(0.01f)] private float bossMoveDurationBeats = 2f;
    [SerializeField] private Vector2 bossStartViewport = new Vector2(1.2f, 0.52f);
    [SerializeField] private Vector2 bossTargetViewport = new Vector2(0.78f, 0.52f);
    [SerializeField] private AnimationCurve bossMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("プレイヤー画像アニメーション")]
    [SerializeField] private Sprite[] playerMoveFrames;
    [SerializeField, Min(0f)] private float playerAttackSwitchBeat = 2f;
    [SerializeField] private Sprite[] playerAttackFrames;
    [SerializeField, Min(0.01f)] private float playerFrameDurationBeats = 0.25f;

    [Header("ボス画像アニメーション")]
    [SerializeField] private Sprite[] bossMoveFrames;
    [SerializeField, Min(0f)] private float bossDamageSwitchBeat = 4f;
    [SerializeField] private Sprite[] bossDamageFrames;
    [SerializeField, Min(0.01f)] private float bossFrameDurationBeats = 0.25f;

    [Header("溜め光・ビーム・命中光")]
    [SerializeField, Min(0f)] private float chargeStartBeat = 2f;
    [SerializeField, Min(0.01f)] private float chargeDurationBeats = 1f;
    [SerializeField, Min(0f)] private float beamStartBeat = 3f;
    [SerializeField, Min(0.01f)] private float beamDurationBeats = 1f;
    [SerializeField, Min(0.01f)] private float beamExtendBeats = 0.25f;
    [SerializeField, Min(0.01f)] private float beamStartThickness = 1.2f;
    [SerializeField, Min(0.01f)] private float beamEndThickness = 0.3f;
    [SerializeField, Min(0f)] private float beamStartOffset = 0.55f;
    [SerializeField, Min(0f)] private float impactStartBeat = 4f;
    [SerializeField, Min(0.01f)] private float impactDurationBeats = 2f;
    [SerializeField, Min(0.01f)] private float flashDiameter = 1.2f;
    [SerializeField, Min(0f)] private float bossShakeDistance = 0.22f;

    [Header("ゲームUI")]
    [SerializeField, Min(0f)] private float hideGameplayUiBeat;
    [Header("演出用UGUI画像（取得時を0拍とした相対拍）")]
    [SerializeField] private FinalEventImageCue[] imageCues;
    [Header("演出用UGUIテキスト（取得時を0拍とした相対拍）")]
    [SerializeField] private FinalEventTextCue[] textCues;

    public float ItemCollectBeat => itemCollectBeat;
    public float ItemHomingDurationBeats => Mathf.Max(0.01f, itemHomingDurationBeats);
    public Vector2 ItemHomingStartViewport => itemHomingStartViewport;
    public AnimationCurve ItemHomingCurve => itemHomingCurve;
    public float PlayerMoveStartBeat => playerMoveStartBeat;
    public float PlayerMoveDurationBeats => Mathf.Max(0.01f, playerMoveDurationBeats);
    public Vector2 PlayerTargetViewport => playerTargetViewport;
    public AnimationCurve PlayerMoveCurve => playerMoveCurve;
    public float BossMoveStartBeat => bossMoveStartBeat;
    public float BossMoveDurationBeats => Mathf.Max(0.01f, bossMoveDurationBeats);
    public Vector2 BossStartViewport => bossStartViewport;
    public Vector2 BossTargetViewport => bossTargetViewport;
    public AnimationCurve BossMoveCurve => bossMoveCurve;
    public Sprite[] PlayerMoveFrames => playerMoveFrames;
    public float PlayerAttackSwitchBeat => playerAttackSwitchBeat;
    public Sprite[] PlayerAttackFrames => playerAttackFrames;
    public float PlayerFrameDurationBeats => Mathf.Max(0.01f, playerFrameDurationBeats);
    public Sprite[] BossMoveFrames => bossMoveFrames;
    public float BossDamageSwitchBeat => bossDamageSwitchBeat;
    public Sprite[] BossDamageFrames => bossDamageFrames;
    public float BossFrameDurationBeats => Mathf.Max(0.01f, bossFrameDurationBeats);
    public float ChargeStartBeat => chargeStartBeat;
    public float ChargeDurationBeats => Mathf.Max(0.01f, chargeDurationBeats);
    public float BeamStartBeat => beamStartBeat;
    public float BeamDurationBeats => Mathf.Max(0.01f, beamDurationBeats);
    public float BeamExtendBeats => Mathf.Max(0.01f, beamExtendBeats);
    public float BeamStartThickness => Mathf.Max(0.01f, beamStartThickness);
    public float BeamEndThickness => Mathf.Max(0.01f, beamEndThickness);
    public float BeamStartOffset => beamStartOffset;
    public float ImpactStartBeat => impactStartBeat;
    public float ImpactDurationBeats => Mathf.Max(0.01f, impactDurationBeats);
    public float FlashDiameter => Mathf.Max(0.01f, flashDiameter);
    public float BossShakeDistance => Mathf.Max(0f, bossShakeDistance);
    public float HideGameplayUiBeat => hideGameplayUiBeat;
    public FinalEventImageCue[] ImageCues => imageCues;
    public FinalEventTextCue[] TextCues => textCues;
}

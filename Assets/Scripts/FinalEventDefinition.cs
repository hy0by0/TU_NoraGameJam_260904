using System;
using UnityEngine;

/// <summary>画像の切替方法。Dissolveはノイズ状に画像を出現させます。</summary>
public enum FinaleImageTransition { Cut, Fade, Dissolve }

/// <summary>小節・拍で指定するクレジット1枚分の設定です。</summary>
[Serializable]
public class FinaleCreditCue
{
    public BeatTiming start = new BeatTiming(117);
    public BeatTiming end = new BeatTiming(119);
    [Min(0f)] public float fadeInBeats = 0.5f;
    [Min(0f)] public float fadeOutBeats = 0.5f;
    [TextArea(2, 6), Tooltip("{score} を今回の確定スコアに置き換えます。")]
    public string message = "クレジット";
    [Min(1)] public int fontSize = 40;
    public Color color = Color.black;
    public Vector2 anchoredPosition;
}

/// <summary>ボス登場から終幕までの構図と時刻を、小節・拍で調整します。</summary>
[CreateAssetMenu(menuName = "Final Event/進行設定", fileName = "FinalEventTimeline")]
public class FinalEventDefinition : ScriptableObject
{
    [Header("基準の曲（小節・拍は1始まり）")]
    public SongDefinition song;
    [Header("アイテム取得")]
    public BeatTiming itemCollectTiming = new BeatTiming(113);
    [SerializeField, Min(0.01f)] private float itemHomingDurationBeats = 2f;
    [SerializeField] private Vector2 itemHomingStartViewport = new Vector2(1.08f, 0.65f);
    [SerializeField] private AnimationCurve itemHomingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("ボス登場と黒塗り解除")]
    public BeatTiming bossEnterStart = new BeatTiming(85);
    public BeatTiming bossEnterEnd = new BeatTiming(86);
    public BeatTiming bossRevealStart = new BeatTiming(101);
    public BeatTiming bossRevealEnd = new BeatTiming(102);
    public Vector2 bossEnterViewport = new Vector2(0.78f, 1.3f);
    public Vector2 bossTargetViewport = new Vector2(0.78f, 0.52f);
    public AnimationCurve bossMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Min(0f)] public float bossFloatAmplitude = 0.15f;
    [Min(0.01f)] public float bossFloatPeriodBeats = 4f;
    [Min(0.01f)] public float bossSettleBeats = 2f;
    public Sprite bossIdleSprite;
    public Sprite bossDamageSprite;

    [Header("取得後のプレイヤー位置")]
    public Vector2 playerTargetViewport = new Vector2(0.25f, 0.42f);
    [Min(0.01f)] public float playerMoveDurationBeats = 2f;
    public AnimationCurve playerMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public Sprite playerAttackSprite;
    public Sprite playerAfterBeamSprite;

    [Header("ビーム（取得時に自動発射・光なし）")]
    [Min(0.01f)] public float beamExtendBeats = 2f;
    [Min(0.01f)] public float beamMaximumLength = 20f;
    [Min(0.01f)] public float beamStartThickness = 0.1f;
    [Min(0.01f)] public float beamMaximumThickness = 16f;
    public bool coverScreenHeight = true;
    [Tooltip("画面を覆う場合、発射端も左端まで拡大します。")]
    public bool coverScreenWidth = true;
    public BeatTiming beamFullTiming = new BeatTiming(114, 4);
    public AnimationCurve beamGrowthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("ビーム拡大中の透明度です。横軸0が取得時、1が拡大完了時です。")]
    public AnimationCurve beamFadeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [Tooltip("ビーム拡大中に背面へ表示する白背景の透明度です。")]
    public AnimationCurve whiteBackdropFadeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Min(0f)] public float beamStartOffset = 0.55f;

    [Header("1回目の白転：完全な白の時点で画像を切替")]
    public BeatTiming firstWhiteStart = new BeatTiming(114, 4);
    public BeatTiming firstWhitePeak = new BeatTiming(115);
    public BeatTiming firstWhiteEnd = new BeatTiming(115, 3);
    [Header("接近と2回目の白転")]
    public BeatTiming approachStart = new BeatTiming(115, 3);
    public BeatTiming contactTiming = new BeatTiming(117);
    public AnimationCurve approachCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public BeatTiming secondWhiteStart = new BeatTiming(116, 4);
    public BeatTiming creditsStart = new BeatTiming(117);

    [Header("白背景のテキスト（後ろの項目を優先）")]
    public FinaleCreditCue[] credits = new FinaleCreditCue[0];
    [Header("最後のスチル")]
    public BeatTiming firstStillTiming = new BeatTiming(125);
    [Min(0.01f)] public float secondStillDelayBeats = 2f;
    [Min(0f)] public float firstStillFadeBeats = 1f;
    [Min(0f)] public float secondStillFadeBeats = 0.5f;
    public FinaleImageTransition firstStillTransition = FinaleImageTransition.Fade;
    public FinaleImageTransition secondStillTransition = FinaleImageTransition.Cut;
    public Sprite firstStill;
    public Sprite secondStill;

    public float ItemCollectBeat => Beat(itemCollectTiming);
    public float ItemHomingDurationBeats => Mathf.Max(0.01f, itemHomingDurationBeats);
    public Vector2 ItemHomingStartViewport => itemHomingStartViewport;
    public AnimationCurve ItemHomingCurve => itemHomingCurve;

    /// <summary>小節・拍を音楽時計と比較する絶対拍へ変換します。</summary>
    public float Beat(BeatTiming timing) => (float)timing.ToBeatPosition(song);

    /// <summary>指定開始・終了時刻の間の進行率を求めます。</summary>
    public float Progress(float beat, BeatTiming start, BeatTiming end)
        => Mathf.Clamp01((beat - Beat(start)) / Mathf.Max(0.001f, Beat(end) - Beat(start)));
}

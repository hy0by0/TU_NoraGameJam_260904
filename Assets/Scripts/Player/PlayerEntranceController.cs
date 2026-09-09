using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 現在拍からプレイヤーの表示透明度を求め、ステージ進行に同期したX座標を提供するクラスです。
/// </summary>
[DefaultExecutionOrder(-700)]
public class PlayerEntranceController : MonoBehaviour
{
    [Header("フェード・操作タイミング")]
    [FormerlySerializedAs("entranceStartBeatTiming")]
    [SerializeField, InspectorName("フェード開始タイミング")] private BeatTiming fadeStartBeatTiming = new BeatTiming();
    [FormerlySerializedAs("referenceArrivalBeatTiming")]
    [SerializeField, InspectorName("フェード完了タイミング")] private BeatTiming fadeCompleteBeatTiming = new BeatTiming();
    [SerializeField, InspectorName("操作開始タイミング")] private BeatTiming inputStartBeatTiming = new BeatTiming();

    [Header("ゲームプレイ位置")]
    [SerializeField] private Vector3 gameplayReferencePosition = new Vector3(-2f, -1.51f, 0f);
    [FormerlySerializedAs("movementCurve")]
    [SerializeField, InspectorName("フェード変化カーブ")] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("参照")]
    [SerializeField] private SongDefinition songDefinition;
    [SerializeField] private GameMusicController musicController;
    [SerializeField] private StageProgressController stageProgressController;
    [SerializeField] private SpriteRenderer playerRenderer;

    private float fadeProgress;
    private bool hasCompletedFade;
    private bool hasReleasedRendererAlpha;

    public float FadeProgress => fadeProgress;
    public float GameplayWorldX => gameplayReferencePosition.x + stageProgressController.ProgressDistance;
    public bool IsInputEnabled => musicController.HasPlaybackStarted
        && musicController.CurrentBeatFloat >= inputStartBeatTiming.ToBeatPosition(songDefinition);

    /// <summary>
    /// プレイヤーをゲームプレイ基準位置へ置き、透明な開始状態へ揃えます。
    /// </summary>
    private void Awake()
    {
        transform.position = gameplayReferencePosition;
        fadeProgress = 0f;
        hasCompletedFade = false;
        hasReleasedRendererAlpha = false;
        ApplyVisibility(0f);
    }

    /// <summary>
    /// 経過時間を加算せず、現在拍からフェード進行率を決定します。
    /// </summary>
    private void Update()
    {
        if (!musicController.HasPlaybackStarted)
        {
            fadeProgress = 0f;
            hasCompletedFade = false;
            hasReleasedRendererAlpha = false;
            return;
        }

        double currentBeat = musicController.CurrentBeatFloat;
        double fadeStartBeat = fadeStartBeatTiming.ToBeatPosition(songDefinition);
        double fadeCompleteBeat = fadeCompleteBeatTiming.ToBeatPosition(songDefinition);
        fadeProgress = Mathf.InverseLerp((float)fadeStartBeat, (float)fadeCompleteBeat, (float)currentBeat);
        hasCompletedFade = currentBeat >= fadeCompleteBeat;
    }

    /// <summary>
    /// Animatorが更新した後にフェードAlphaを反映し、完了後は通常アニメーションへ制御を戻します。
    /// </summary>
    private void LateUpdate()
    {
        if (!hasCompletedFade)
        {
            ApplyVisibility(fadeCurve.Evaluate(fadeProgress));
            return;
        }

        if (!hasReleasedRendererAlpha)
        {
            ApplyVisibility(1f);
            hasReleasedRendererAlpha = true;
        }
    }

    /// <summary>
    /// プレイヤー画像のRGBを保ったまま表示透明度だけを変更します。
    /// </summary>
    private void ApplyVisibility(float alpha)
    {
        Color color = playerRenderer.color;
        color.a = Mathf.Clamp01(alpha);
        playerRenderer.color = color;
    }
}

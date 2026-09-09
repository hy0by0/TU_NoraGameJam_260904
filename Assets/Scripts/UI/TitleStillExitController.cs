using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>配置済みのタイトル画像を初期表示し、指定した曲位置から左の画面外へ退場させます。</summary>
public class TitleStillExitController : MonoBehaviour
{
    [Header("Inspectorから紐づける参照")]
    [SerializeField] private GameMusicController musicController;
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image targetImage;
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private AnimationClip animationClip;

    [Header("曲に同期するタイミング")]
    [SerializeField, InspectorName("アニメーション開始タイミング")] private BeatTiming animationStartTiming = new BeatTiming();
    [SerializeField, InspectorName("退場・フェード開始タイミング")] private BeatTiming fadeStartTiming = new BeatTiming();
    [SerializeField, InspectorName("退場・フェード完了タイミング")] private BeatTiming fadeCompleteTiming = new BeatTiming();

    [Header("退場設定")]
    [SerializeField, Min(0f), InspectorName("画面外の余白（Canvas単位）")] private float outsideMargin = 20f;
    [SerializeField, InspectorName("移動カーブ")] private Ease ease = Ease.InOutSine;

    private Vector2 initialPosition;

    /// <summary>編集時の配置を記録し、Animatorの自動再生とループを止めて初期表示します。</summary>
    private void Awake()
    {
        targetAnimator.enabled = false;
        animationClip.SampleAnimation(targetImage.gameObject, 0f);
        initialPosition = targetRect.anchoredPosition;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        targetImage.raycastTarget = false;
    }

    /// <summary>音楽の再生位置からAnimationClipと退場状態を直接評価します。</summary>
    private void LateUpdate()
    {
        float playbackTime = musicController.PlaybackTimeSeconds;
        float animationStartTime = (float)animationStartTiming.ToPlaybackTimeSeconds(musicController.SongDefinition);
        float animationTime = Mathf.Clamp(playbackTime - animationStartTime, 0f, animationClip.length);
        animationClip.SampleAnimation(targetImage.gameObject, animationTime);

        float fadeStartTime = (float)fadeStartTiming.ToPlaybackTimeSeconds(musicController.SongDefinition);
        float fadeCompleteTime = (float)fadeCompleteTiming.ToPlaybackTimeSeconds(musicController.SongDefinition);
        float fadeProgress = Mathf.InverseLerp(fadeStartTime, fadeCompleteTime, playbackTime);
        EvaluateExit(fadeProgress);
    }

    /// <summary>アニメーション後の座標から左画面外へ移動し、同時に透明化します。</summary>
    public void EvaluateExit(float progress)
    {
        Vector2 animatedPosition = targetRect.anchoredPosition;
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, targetRect);
        float distance = Mathf.Max(0f, bounds.max.x - canvasRect.rect.xMin + outsideMargin);
        Vector3 worldOffset = canvasRect.TransformVector(Vector3.left * distance);
        Vector3 localOffset = targetRect.parent.InverseTransformVector(worldOffset);
        float clampedProgress = Mathf.Clamp01(progress);
        float eased = DOVirtual.EasedValue(0f, 1f, clampedProgress, ease);
        targetRect.anchoredPosition = animatedPosition + new Vector2(localOffset.x, localOffset.y) * eased;
        canvasGroup.alpha = 1f - clampedProgress;
        targetImage.enabled = clampedProgress < 1f;
    }
}

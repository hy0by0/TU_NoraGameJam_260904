using DG.Tweening;
using UnityEngine;

/// <summary>
/// Inspectorで接続したUGUIグループの表示・非表示トランジションを管理するクラスです。
/// </summary>
public class UITransitionController : MonoBehaviour
{
    public enum TransitionStyle
    {
        Fade,
        SlideFromTop,
        SlideFromBottom,
        SlideFromLeft,
        SlideFromRight,
        Scale,
        FadeAndSlide
    }

    [Header("対象UGUI")]
    [SerializeField] private CanvasGroup targetCanvasGroup;
    [SerializeField] private RectTransform targetRectTransform;

    [Header("通常の切り替え設定")]
    [SerializeField] private TransitionStyle transitionStyle = TransitionStyle.Fade;
    [SerializeField, Min(0f)] private float duration = 0.35f;
    [SerializeField, Min(0f)] private float slideDistance = 180f;
    [SerializeField, Range(0.01f, 1f)] private float hiddenScale = 0.8f;
    [SerializeField] private Ease showEase = Ease.OutCubic;
    [SerializeField] private Ease hideEase = Ease.InCubic;
    [SerializeField] private bool visibleOnAwake;

    private Vector2 visiblePosition;

    /// <summary>
    /// シーンに配置された位置を表示位置として記録し、初期表示状態を反映します。
    /// </summary>
    private void Awake()
    {
        visiblePosition = targetRectTransform.anchoredPosition;
        SetVisibleImmediate(visibleOnAwake);
    }

    /// <summary>
    /// Inspectorで選択した方法で表示します。
    /// </summary>
    public void Show()
    {
        PlayShow(transitionStyle);
    }

    /// <summary>
    /// Inspectorで選択した方法で非表示にします。
    /// </summary>
    public void Hide()
    {
        PlayHide(transitionStyle);
    }

    public void ShowFade() => PlayShow(TransitionStyle.Fade);
    public void HideFade() => PlayHide(TransitionStyle.Fade);
    public void ShowFromTop() => PlayShow(TransitionStyle.SlideFromTop);
    public void HideToTop() => PlayHide(TransitionStyle.SlideFromTop);
    public void ShowFromBottom() => PlayShow(TransitionStyle.SlideFromBottom);
    public void HideToBottom() => PlayHide(TransitionStyle.SlideFromBottom);
    public void ShowFromLeft() => PlayShow(TransitionStyle.SlideFromLeft);
    public void HideToLeft() => PlayHide(TransitionStyle.SlideFromLeft);
    public void ShowFromRight() => PlayShow(TransitionStyle.SlideFromRight);
    public void HideToRight() => PlayHide(TransitionStyle.SlideFromRight);
    public void ShowScale() => PlayShow(TransitionStyle.Scale);
    public void HideScale() => PlayHide(TransitionStyle.Scale);
    public void ShowFadeAndSlide() => PlayShow(TransitionStyle.FadeAndSlide);
    public void HideFadeAndSlide() => PlayHide(TransitionStyle.FadeAndSlide);

    /// <summary>
    /// アニメーションせず即座に表示します。
    /// </summary>
    public void ShowImmediate()
    {
        SetVisibleImmediate(true);
    }

    /// <summary>
    /// アニメーションせず即座に非表示にします。
    /// </summary>
    public void HideImmediate()
    {
        SetVisibleImmediate(false);
    }

    /// <summary>
    /// 指定した方法の表示アニメーションを再生します。
    /// </summary>
    private void PlayShow(TransitionStyle style)
    {
        KillTweens();
        targetCanvasGroup.blocksRaycasts = true;
        targetCanvasGroup.interactable = true;

        if (UsesFade(style))
        {
            targetCanvasGroup.alpha = 0f;
            targetCanvasGroup.DOFade(1f, duration).SetEase(showEase).SetId(this);
        }
        else
        {
            targetCanvasGroup.alpha = 1f;
        }

        if (style == TransitionStyle.Scale)
        {
            targetRectTransform.localScale = Vector3.one * hiddenScale;
            targetRectTransform.DOScale(Vector3.one, duration).SetEase(showEase).SetId(this);
        }
        else
        {
            targetRectTransform.localScale = Vector3.one;
        }

        if (UsesSlide(style))
        {
            targetRectTransform.anchoredPosition = GetHiddenPosition(style);
            targetRectTransform.DOAnchorPos(visiblePosition, duration).SetEase(showEase).SetId(this);
        }
        else
        {
            targetRectTransform.anchoredPosition = visiblePosition;
        }
    }

    /// <summary>
    /// 指定した方法の非表示アニメーションを再生します。
    /// </summary>
    private void PlayHide(TransitionStyle style)
    {
        KillTweens();
        Sequence sequence = DOTween.Sequence().SetId(this);

        if (UsesFade(style))
        {
            sequence.Join(targetCanvasGroup.DOFade(0f, duration).SetEase(hideEase));
        }

        if (style == TransitionStyle.Scale)
        {
            sequence.Join(targetRectTransform.DOScale(Vector3.one * hiddenScale, duration).SetEase(hideEase));
        }

        if (UsesSlide(style))
        {
            sequence.Join(targetRectTransform.DOAnchorPos(GetHiddenPosition(style), duration).SetEase(hideEase));
        }

        sequence.OnComplete(CompleteHide);
    }

    /// <summary>
    /// 表示状態をアニメーションなしで反映します。
    /// </summary>
    private void SetVisibleImmediate(bool isVisible)
    {
        KillTweens();
        targetCanvasGroup.alpha = isVisible ? 1f : 0f;
        targetCanvasGroup.interactable = isVisible;
        targetCanvasGroup.blocksRaycasts = isVisible;
        targetRectTransform.anchoredPosition = visiblePosition;
        targetRectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// 非表示完了後に入力判定を止めます。
    /// </summary>
    private void CompleteHide()
    {
        targetCanvasGroup.interactable = false;
        targetCanvasGroup.blocksRaycasts = false;
    }

    private bool UsesFade(TransitionStyle style)
    {
        return style == TransitionStyle.Fade || style == TransitionStyle.FadeAndSlide;
    }

    private bool UsesSlide(TransitionStyle style)
    {
        return style == TransitionStyle.SlideFromTop
            || style == TransitionStyle.SlideFromBottom
            || style == TransitionStyle.SlideFromLeft
            || style == TransitionStyle.SlideFromRight
            || style == TransitionStyle.FadeAndSlide;
    }

    /// <summary>
    /// 表示位置と方向設定から画面外側の位置を求めます。
    /// </summary>
    private Vector2 GetHiddenPosition(TransitionStyle style)
    {
        switch (style)
        {
            case TransitionStyle.SlideFromTop:
                return visiblePosition + Vector2.up * slideDistance;
            case TransitionStyle.SlideFromBottom:
                return visiblePosition + Vector2.down * slideDistance;
            case TransitionStyle.SlideFromRight:
                return visiblePosition + Vector2.right * slideDistance;
            default:
                return visiblePosition + Vector2.left * slideDistance;
        }
    }

    /// <summary>
    /// このControllerが開始した途中のTweenを停止します。
    /// </summary>
    private void KillTweens()
    {
        DOTween.Kill(this);
        targetCanvasGroup.DOKill();
        targetRectTransform.DOKill();
    }
}

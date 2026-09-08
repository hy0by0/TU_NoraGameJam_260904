using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hierarchyへ事前配置したImageの表示、AnimationClip再生、非表示を管理するクラスです。
/// </summary>
public class ImageAnimationUIController : MonoBehaviour
{
    [Header("事前配置した参照")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private AnimationClip animationClip;
    [SerializeField] private CanvasGroup targetCanvasGroup;

    [Header("表示設定")]
    [SerializeField, Min(0f)] private float fadeDuration = 0.2f;
    [SerializeField] private bool hideOnAwake = true;

    /// <summary>
    /// 開始時の表示状態をInspector設定から反映します。
    /// </summary>
    private void Awake()
    {
        targetImage.raycastTarget = false;
        targetCanvasGroup.alpha = hideOnAwake ? 0f : 1f;
        targetCanvasGroup.blocksRaycasts = false;
        targetCanvasGroup.interactable = false;
    }

    /// <summary>
    /// 画像をフェード表示します。
    /// </summary>
    public void Show()
    {
        targetCanvasGroup.DOKill();
        targetCanvasGroup.DOFade(1f, fadeDuration).SetId(this);
    }

    /// <summary>
    /// AnimationClipを先頭から再生します。
    /// </summary>
    public void PlayAnimation()
    {
        targetAnimator.Play(animationClip.name, 0, 0f);
    }

    /// <summary>
    /// 画像の表示とAnimationClip再生を同時に開始します。
    /// </summary>
    public void ShowAndPlay()
    {
        Show();
        PlayAnimation();
    }

    /// <summary>
    /// 画像をフェード非表示にします。
    /// </summary>
    public void Hide()
    {
        targetCanvasGroup.DOKill();
        targetCanvasGroup.DOFade(0f, fadeDuration).SetId(this);
    }

    public void ShowImmediate()
    {
        targetCanvasGroup.DOKill();
        targetCanvasGroup.alpha = 1f;
    }

    public void HideImmediate()
    {
        targetCanvasGroup.DOKill();
        targetCanvasGroup.alpha = 0f;
    }
}

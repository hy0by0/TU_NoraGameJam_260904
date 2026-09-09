using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// タイトル画面のボタンへ、ホバー拡大と操作SEを付けるクラスです。
/// </summary>
public class TitleButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("参照")]
    [SerializeField] private RectTransform targetRectTransform;
    [SerializeField] private AudioClip hoverSe;
    [SerializeField] private AudioClip clickSe;

    [Header("ホバー演出")]
    [SerializeField, Min(1f)] private float hoverScale = 1.08f;
    [SerializeField, Min(0f)] private float scaleDuration = 0.15f;

    private Vector3 initialScale;

    /// <summary>
    /// Inspectorで設定された通常時の大きさを記録します。
    /// </summary>
    private void Awake()
    {
        initialScale = targetRectTransform.localScale;
    }

    /// <summary>
    /// マウスがボタンへ入ったとき、SEを鳴らして少し拡大します。
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySE(hoverSe);
        targetRectTransform.DOKill();
        targetRectTransform.DOScale(initialScale * hoverScale, scaleDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    /// <summary>
    /// マウスがボタンから出たとき、通常の大きさへ戻します。
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        targetRectTransform.DOKill();
        targetRectTransform.DOScale(initialScale, scaleDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    /// <summary>
    /// ボタンがクリックされたとき、決定用SEを鳴らします。
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySE(clickSe);
    }

    /// <summary>
    /// オブジェクトが非表示になる際にTweenを停止し、通常の大きさへ戻します。
    /// </summary>
    private void OnDisable()
    {
        targetRectTransform.DOKill();
        targetRectTransform.localScale = initialScale;
    }
}

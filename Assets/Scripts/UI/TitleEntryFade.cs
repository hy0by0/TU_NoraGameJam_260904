using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトルを開いたとき、事前配置した白いUGUI画像を透明にして画面を見せるクラスです。
/// </summary>
public class TitleEntryFade : MonoBehaviour
{
    [SerializeField] private Image whiteOverlay;
    [SerializeField, Min(0f)] private float fadeDuration = 0.8f;

    /// <summary>白い画像からタイトル画面へフェードします。</summary>
    private void Start()
    {
        whiteOverlay.color = Color.white;
        whiteOverlay.raycastTarget = true;
        whiteOverlay.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .SetLink(whiteOverlay.gameObject, LinkBehaviour.KillOnDestroy)
            .OnComplete(() => whiteOverlay.raycastTarget = false);
    }
}

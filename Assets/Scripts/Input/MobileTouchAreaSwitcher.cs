using UnityEngine;

/// <summary>
/// タッチ操作で始めたプレイ中だけ、事前配置した左右の操作領域を有効にするクラスです。
/// </summary>
public class MobileTouchAreaSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject moveArea;
    [SerializeField] private GameObject attackArea;
    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        moveArea.SetActive(PlayInputMode.IsTouch);
        attackArea.SetActive(PlayInputMode.IsTouch);
    }

    /// <summary>終了演出やゲームオーバーでは操作領域を外し、画面ボタンを押せるようにします。</summary>
    private void Update()
    {
        bool shouldAcceptTouch = PlayInputMode.IsTouch
            && playerController.IsGameplayEnabled
            && !playerController.IsFinalEventActive;
        if (moveArea.activeSelf != shouldAcceptTouch) moveArea.SetActive(shouldAcceptTouch);
        if (attackArea.activeSelf != shouldAcceptTouch) attackArea.SetActive(shouldAcceptTouch);
    }
}

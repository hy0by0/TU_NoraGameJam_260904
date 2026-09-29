using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 最初の画面でタップとクリックを見分け、操作方法を確定してタイトルへ進めるクラスです。
/// </summary>
public class FirstInputGate : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private string titleSceneName = "Title";
    private bool isLoading;

    /// <summary>最初に押された入力元で操作方法を確定します。</summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isLoading) return;

        bool isTouch = eventData is ExtendedPointerEventData inputEvent
            && inputEvent.pointerType == UIPointerType.Touch;
        PlayInputMode.Select(isTouch
            ? PlayInputMode.Mode.Touch
            : PlayInputMode.Mode.Desktop);
        isLoading = true;
        SceneManager.LoadSceneAsync(titleSceneName);
    }
}

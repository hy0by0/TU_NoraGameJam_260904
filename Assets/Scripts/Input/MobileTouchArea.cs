using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// 画面左のドラッグ移動と、画面右のタップ攻撃を指ごとに受け取るUGUI領域です。
/// </summary>
public class MobileTouchArea : MonoBehaviour, IPointerDownHandler, IDragHandler,
    IPointerUpHandler, IInitializePotentialDragHandler
{
    public enum AreaKind { Move, Attack }

    [SerializeField] private AreaKind areaKind;
    [SerializeField] private PlayerController playerController;
    [SerializeField, Min(0f), Tooltip("画面の高さ一杯に指を動かしたとき、プレイヤーを動かすワールドY距離です。")]
    private float dragWorldUnitsPerScreen = 8f;

    private bool isTracking;
    private int trackedPointerId;
    private float previousScreenY;

    /// <summary>小さな指の動きからでも移動を始められるようにします。</summary>
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    /// <summary>左では移動用の指を記録し、右では攻撃を試みます。</summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        bool isTouch = eventData is ExtendedPointerEventData inputEvent
            && inputEvent.pointerType == UIPointerType.Touch;
        if (!PlayInputMode.IsTouch || !isTouch) return;

        if (areaKind == AreaKind.Attack)
        {
            playerController.TryAttackFromTouch();
            return;
        }

        if (isTracking) return;
        isTracking = true;
        trackedPointerId = eventData.pointerId;
        previousScreenY = eventData.position.y;
    }

    /// <summary>移動を始めた指だけを追い、画面サイズで補正した移動量を渡します。</summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isTracking || eventData.pointerId != trackedPointerId) return;

        float deltaY = eventData.position.y - previousScreenY;
        previousScreenY = eventData.position.y;
        playerController.ApplyTouchDrag(deltaY / Screen.height * dragWorldUnitsPerScreen);
    }

    /// <summary>移動に使っていた指を離したら追跡を終えます。</summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isTracking && eventData.pointerId == trackedPointerId) isTracking = false;
    }

    private void OnDisable()
    {
        isTracking = false;
    }
}

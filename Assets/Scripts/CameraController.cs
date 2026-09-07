using UnityEngine;

/// <summary>
/// PlayerをX方向へ追従し、指定範囲内にカメラ位置を制限するクラスです。
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("追従対象")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private float focusOffsetX;

    [Header("スクロール範囲")]
    [SerializeField] private float leftScrollLimit;
    [SerializeField] private float rightScrollLimit;

    /// <summary>
    /// Rigidbody2Dの更新と補間が終わった後に、カメラを追従対象へ合わせます。
    /// </summary>
    private void LateUpdate()
    {
        float targetX = followTarget.position.x + focusOffsetX;
        float clampedX = Mathf.Clamp(targetX, leftScrollLimit, rightScrollLimit);

        transform.position = new Vector3(
            clampedX,
            transform.position.y,
            transform.position.z
        );
    }
}

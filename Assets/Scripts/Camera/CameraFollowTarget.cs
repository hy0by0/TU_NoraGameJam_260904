using UnityEngine;

/// <summary>
/// Playerの補間後X座標を使い、上下固定のCinemachine追従点を管理するクラスです。
/// </summary>
[DefaultExecutionOrder(-750)]
public class CameraFollowTarget : MonoBehaviour
{
    [Header("追従対象")]
    [SerializeField] private Transform playerFollowTarget;

    [Header("固定座標")]
    [SerializeField] private Vector3 stageStartPosition = new Vector3(-2f, -1.51f, 0f);

    private bool isFollowingEnabled = true;

    public bool IsFollowingEnabled => isFollowingEnabled;

    /// <summary>
    /// Rigidbody2Dの補間後に表示されるPlayerのXを取得し、YとZは固定します。
    /// </summary>
    private void LateUpdate()
    {
        if (!isFollowingEnabled)
        {
            return;
        }

        transform.position = new Vector3(
            playerFollowTarget.position.x,
            stageStartPosition.y,
            stageStartPosition.z);
    }

    /// <summary>
    /// Cinemachine GameplayCamera向け追従点の更新を切り替えます。
    /// </summary>
    public void SetFollowingEnabled(bool isEnabled)
    {
        isFollowingEnabled = isEnabled;
    }
}

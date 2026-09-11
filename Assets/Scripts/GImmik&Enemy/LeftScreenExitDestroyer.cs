using UnityEngine;

/// <summary>
/// 描画範囲全体がメインカメラの左端を通過した敵・アイテムを削除するクラスです。
/// </summary>
[DisallowMultipleComponent]
public class LeftScreenExitDestroyer : MonoBehaviour
{
    [Header("画面外判定に使う描画対象（Inspectorから設定）")]
    [SerializeField] private Renderer targetRenderer;

    private Camera gameplayCamera;

    /// <summary>
    /// Prefabからシーン上のカメラを直接参照できないため、MainCameraタグから取得します。
    /// </summary>
    private void Awake()
    {
        gameplayCamera = Camera.main;
    }

    /// <summary>
    /// カメラ移動後の境界を使い、描画範囲の右端まで画面外へ抜けた時だけ削除します。
    /// </summary>
    private void LateUpdate()
    {
        float distanceFromCamera = Mathf.Abs(transform.position.z - gameplayCamera.transform.position.z);
        float cameraLeftEdgeX = gameplayCamera.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, distanceFromCamera)).x;

        if (targetRenderer.bounds.max.x >= cameraLeftEdgeX)
        {
            return;
        }

        Destroy(gameObject);
    }
}

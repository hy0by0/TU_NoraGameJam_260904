using UnityEngine;

/// <summary>
/// タイトル画面の遠景を一定速度で横方向へループさせるクラスです。
/// </summary>
public class TitleBackgroundLoopController : MonoBehaviour
{
    [Header("遠景ループ設定")]
    [SerializeField, InspectorName("移動させる遠景Root")] private RectTransform loopRoot;
    [SerializeField, Min(0.01f), InspectorName("画像1枚分の幅")] private float loopWidth = 1920f;
    [SerializeField, Min(0f), InspectorName("移動速度（1秒あたりのピクセル数）")] private float scrollSpeed = 30f;
    [SerializeField, InspectorName("移動方向")] private Vector2 moveDirection = Vector2.left;

    private Vector2 initialPosition;
    private float travelledDistance;

    /// <summary>
    /// Inspectorで配置した遠景Rootの初期位置を記録します。
    /// </summary>
    private void Awake()
    {
        initialPosition = loopRoot.anchoredPosition;
        travelledDistance = 0f;
    }

    /// <summary>
    /// 経過時間から遠景Rootの位置を更新し、画像1枚分の距離で循環させます。
    /// </summary>
    private void Update()
    {
        travelledDistance += scrollSpeed * Time.unscaledDeltaTime;
        float loopOffset = Mathf.Repeat(travelledDistance, loopWidth);
        loopRoot.anchoredPosition = initialPosition + moveDirection.normalized * loopOffset;
    }
}

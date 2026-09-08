using UnityEngine;

/// <summary>最終アイテムを画面右側からプレイヤーへホーミングさせ、指定拍で自動取得します。</summary>
[DefaultExecutionOrder(-100)]
public class FinalItemAutoCollector : MonoBehaviour
{
    [SerializeField] private FinalEventDefinition timeline;
    [SerializeField] private GameMusicController musicController;
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Item item;
    [SerializeField] private Collider2D itemCollider;

    private Vector3 homingStartPosition;
    private bool isHoming;
    private bool isCollected;

    public bool IsHoming => isHoming;
    public bool IsCollected => isCollected;

    /// <summary>通常接触による早取りを防ぎ、取得拍までは専用処理だけで移動させます。</summary>
    private void Awake()
    {
        itemCollider.enabled = false;
    }

    /// <summary>取得拍のN拍前からホーミングし、取得拍に到達したフレームで直接取得します。</summary>
    private void Update()
    {
        if (isCollected || !musicController.HasPlaybackStarted) return;
        float currentBeat = musicController.CurrentBeatFloat;
        float homingStartBeat = timeline.ItemCollectBeat - timeline.ItemHomingDurationBeats;
        if (currentBeat < homingStartBeat) return;

        if (!isHoming)
        {
            isHoming = true;
            homingStartPosition = ViewportToWorld(timeline.ItemHomingStartViewport);
            transform.position = homingStartPosition;
        }

        float progress = Mathf.Clamp01((currentBeat - homingStartBeat) / timeline.ItemHomingDurationBeats);
        float easedProgress = timeline.ItemHomingCurve.Evaluate(progress);
        transform.position = Vector3.LerpUnclamped(homingStartPosition, playerController.transform.position, easedProgress);

        if (currentBeat >= timeline.ItemCollectBeat)
        {
            isCollected = true;
            transform.position = playerController.transform.position;
            item.Collect(playerController);
        }
    }

    /// <summary>画面比率が変わっても同じ方向から登場するようViewport座標をワールド座標へ変換します。</summary>
    private Vector3 ViewportToWorld(Vector2 viewport)
    {
        Vector3 point = gameplayCamera.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, -gameplayCamera.transform.position.z));
        point.z = 0f;
        return point;
    }
}

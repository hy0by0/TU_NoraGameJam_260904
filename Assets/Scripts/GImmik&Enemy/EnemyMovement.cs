using DG.Tweening;
using UnityEngine;

// シーンに置いた敵を曲の指定タイミングから移動させるクラスです。
[RequireComponent(typeof(Enemy), typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour
{
    public enum MovementStyle { Stationary, Constant, Harmonic, Eased }

    [Header("参照（Inspectorから設定）")]
    [SerializeField] private GameMusicController musicController;
    [SerializeField] private Rigidbody2D enemyRigidbody;
    [Header("開始位置とタイミング")]
    [SerializeField] private bool usePlacedPosition = true;
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private BeatTiming startTiming = new BeatTiming();
    [Header("移動方法")]
    [SerializeField] private MovementStyle movementStyle;
    [SerializeField] private Vector2 direction = Vector2.left;
    [SerializeField, Min(0f)] private float speed = 2f;
    [SerializeField, Min(0f)] private float distance = 2f;
    [SerializeField, Min(0.01f)] private float durationSeconds = 2f;
    [SerializeField] private Ease easing = Ease.InOutSine;
    [SerializeField] private bool pingPong = true;
    private Vector2 origin;

    // シーン配置位置または明示したワールド座標を移動の基準にします。
    private void Awake()
    {
        origin = usePlacedPosition ? enemyRigidbody.position : startPosition;
        enemyRigidbody.position = origin;
    }

    // 曲の再生位置から座標を計算し、ポーズ中は移動を止めます。
    private void FixedUpdate()
    {
        if (movementStyle == MovementStyle.Stationary || !musicController.IsGameRunning) return;
        float elapsed = musicController.PlaybackTimeSeconds
            - (float)startTiming.ToPlaybackTimeSeconds(musicController.SongDefinition);
        enemyRigidbody.MovePosition(EvaluatePosition(elapsed));
    }

    // 単振動はdistanceが振幅、durationSecondsが一周期。イージングでは距離と片道時間です。
    public Vector2 EvaluatePosition(float elapsed)
    {
        if (elapsed < 0f) return origin;
        float offset = 0f;
        switch (movementStyle)
        {
            case MovementStyle.Constant:
                offset = speed * elapsed;
                break;
            case MovementStyle.Harmonic:
                offset = distance * Mathf.Sin(elapsed * 2f * Mathf.PI / durationSeconds);
                break;
            case MovementStyle.Eased:
                float progress = pingPong ? Mathf.PingPong(elapsed / durationSeconds, 1f)
                    : Mathf.Clamp01(elapsed / durationSeconds);
                offset = DOVirtual.EasedValue(0f, distance, progress, easing);
                break;
        }
        return origin + direction.normalized * offset;
    }
}

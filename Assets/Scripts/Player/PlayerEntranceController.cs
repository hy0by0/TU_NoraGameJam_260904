using UnityEngine;

/// <summary>
/// 現在拍から登場位置と表示Scaleを求め、登場完了後の進行X座標をPlayerControllerへ提供するクラスです。
/// </summary>
[DefaultExecutionOrder(-700)]
public class PlayerEntranceController : MonoBehaviour
{
    [Header("登場タイミング")]
    [SerializeField] private BeatTiming entranceStartBeatTiming = new BeatTiming();
    [SerializeField] private BeatTiming referenceArrivalBeatTiming = new BeatTiming();
    [SerializeField] private BeatTiming inputStartBeatTiming = new BeatTiming();

    [Header("登場位置")]
    [SerializeField] private Vector3 offscreenStartPosition = new Vector3(-12f, -1.51f, 0f);
    [SerializeField] private Vector3 gameplayReferencePosition = new Vector3(-2f, -1.51f, 0f);
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("登場中のScale演出")]
    [SerializeField] private AnimationCurve entranceScaleCurve = new AnimationCurve(
        new Keyframe(0f, 0.8f),
        new Keyframe(0.75f, 1.08f),
        new Keyframe(1f, 1f));

    [Header("参照")]
    [SerializeField] private SongDefinition songDefinition;
    [SerializeField] private GameMusicController musicController;
    [SerializeField] private StageProgressController stageProgressController;
    [SerializeField] private Transform playerBeatAnchor;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Rigidbody2D playerRigidbody;

    private Vector3 baseVisualScale;
    private float entranceProgress;

    public float EntranceProgress => entranceProgress;
    public Vector3 PlayerBeatAnchorPosition => playerBeatAnchor.position;
    public float GameplayWorldX => gameplayReferencePosition.x + stageProgressController.ProgressDistance;
    public bool HasReachedReferencePosition => musicController.HasPlaybackStarted
        && musicController.CurrentBeatFloat >= referenceArrivalBeatTiming.ToBeatPosition(songDefinition);
    public bool IsInputEnabled => musicController.HasPlaybackStarted
        && musicController.CurrentBeatFloat >= inputStartBeatTiming.ToBeatPosition(songDefinition);

    /// <summary>
    /// VisualRootの通常Scaleを保存し、登場開始地点へ初期配置します。
    /// </summary>
    private void Awake()
    {
        baseVisualScale = visualRoot.localScale;
        transform.position = offscreenStartPosition;
        ApplyVisualScale(0f);
    }

    /// <summary>
    /// 経過時間を加算せず、現在拍から登場率と表示Scaleを決定します。
    /// </summary>
    private void Update()
    {
        if (!musicController.HasPlaybackStarted)
        {
            entranceProgress = 0f;
            ApplyVisualScale(entranceProgress);
            return;
        }

        double currentBeat = musicController.CurrentBeatFloat;
        double entranceStartBeat = entranceStartBeatTiming.ToBeatPosition(songDefinition);
        double arrivalBeat = referenceArrivalBeatTiming.ToBeatPosition(songDefinition);
        entranceProgress = Mathf.InverseLerp((float)entranceStartBeat, (float)arrivalBeat, (float)currentBeat);

        if (currentBeat < arrivalBeat)
        {
            ApplyVisualScale(entranceProgress);
            return;
        }

        visualRoot.localScale = baseVisualScale;
    }

    /// <summary>
    /// 登場中のPlayerRootを、物理更新のタイミングで基準位置まで移動します。
    /// </summary>
    private void FixedUpdate()
    {
        if (!musicController.HasPlaybackStarted)
        {
            playerRigidbody.MovePosition(offscreenStartPosition);
            return;
        }

        double currentBeat = musicController.CurrentBeatFloat;
        double entranceStartBeat = entranceStartBeatTiming.ToBeatPosition(songDefinition);
        double arrivalBeat = referenceArrivalBeatTiming.ToBeatPosition(songDefinition);

        if (currentBeat < arrivalBeat)
        {
            float progress = Mathf.InverseLerp((float)entranceStartBeat, (float)arrivalBeat, (float)currentBeat);
            float curvedProgress = movementCurve.Evaluate(progress);
            Vector2 entrancePosition = Vector2.LerpUnclamped(
                offscreenStartPosition,
                gameplayReferencePosition,
                curvedProgress);
            playerRigidbody.MovePosition(entrancePosition);
        }
    }

    /// <summary>
    /// 登場用カーブをVisualRootだけへ適用し、同期基準や当たり判定のTransformを変えません。
    /// </summary>
    private void ApplyVisualScale(float progress)
    {
        visualRoot.localScale = baseVisualScale * entranceScaleCurve.Evaluate(progress);
    }
}

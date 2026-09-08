using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 曲の小節・拍・拍内分割位置を通過したとき、Inspector登録済みの演出を実行するクラスです。
/// </summary>
[DefaultExecutionOrder(-900)]
public class SongSequenceController : MonoBehaviour
{
    /// <summary>
    /// Inspectorから登録する、曲中の演出イベント1件分の設定です。
    /// </summary>
    [Serializable]
    public class SequenceEvent
    {
        [SerializeField, InspectorName("管理用イベント名")] private string eventName = "新しいイベント";
        [SerializeField, InspectorName("実行タイミング")] private BeatTiming timing = new BeatTiming();
        [SerializeField, InspectorName("有効")] private bool isEnabled = true;
        [SerializeField, InspectorName("同時タイミング内の実行順")] private int executionOrder;
        [SerializeField, InspectorName("一度だけ実行")] private bool executeOnce = true;
        [SerializeField, InspectorName("実行内容")] private UnityEvent unityEvent = new UnityEvent();

        [NonSerialized] private bool hasExecuted;

        public string EventName => eventName;
        public BeatTiming Timing => timing;
        public bool IsEnabled => isEnabled;
        public int ExecutionOrder => executionOrder;
        public bool ExecuteOnce => executeOnce;
        public bool HasExecuted => hasExecuted;

        /// <summary>
        /// 有効状態と一度だけ実行の条件を満たす場合にUnityEventを呼び出します。
        /// </summary>
        public void Invoke()
        {
            if (!isEnabled || (executeOnce && hasExecuted))
            {
                return;
            }

            unityEvent.Invoke();
            hasExecuted = true;
        }

        /// <summary>
        /// リトライに備えて、このイベントの実行済み状態を解除します。
        /// </summary>
        public void ResetExecutionState()
        {
            hasExecuted = false;
        }
    }

    /// <summary>
    /// 曲内秒とInspector登録順を含む、実行判定用の並び替え済みデータです。
    /// </summary>
    private sealed class ScheduledEvent
    {
        public SequenceEvent SequenceEvent { get; }
        public double PlaybackTimeSeconds { get; }
        public int RegistrationIndex { get; }

        public ScheduledEvent(SequenceEvent sequenceEvent, double playbackTimeSeconds, int registrationIndex)
        {
            SequenceEvent = sequenceEvent;
            PlaybackTimeSeconds = playbackTimeSeconds;
            RegistrationIndex = registrationIndex;
        }
    }

    [Header("参照")]
    [SerializeField] private GameMusicController musicController;

    [Header("曲中イベント")]
    [SerializeField] private List<SequenceEvent> sequenceEvents = new List<SequenceEvent>();

    private readonly List<ScheduledEvent> scheduledEvents = new List<ScheduledEvent>();
    private double previousPlaybackTimeSeconds;
    private bool hasPreviousPlaybackTime;
    private bool wasInPreRoll;
    private bool isSequenceEnabled = true;

    public bool IsSequenceEnabled => isSequenceEnabled;

    /// <summary>
    /// Inspectorのイベントを、曲内位置・実行順・登録順の優先度で並べます。
    /// </summary>
    private void Awake()
    {
        RebuildSchedule();
        ResetExecutionState();
    }

    /// <summary>
    /// AudioSourceの前回位置から現在位置までに通過したイベントを実行します。
    /// </summary>
    private void Update()
    {
        if (!isSequenceEnabled)
        {
            hasPreviousPlaybackTime = false;
            return;
        }

        bool isInPreRoll = musicController.State == GameMusicController.PlaybackState.Scheduled
            || musicController.State == GameMusicController.PlaybackState.PreRoll;

        if (isInPreRoll && !wasInPreRoll)
        {
            ResetExecutionState();
        }

        wasInPreRoll = isInPreRoll;

        if (!musicController.IsGameRunning)
        {
            hasPreviousPlaybackTime = false;
            return;
        }

        double currentPlaybackTimeSeconds = musicController.PlaybackTimeSeconds;
        double rangeStart = hasPreviousPlaybackTime ? previousPlaybackTimeSeconds : -double.Epsilon;

        ExecutePassedEvents(rangeStart, currentPlaybackTimeSeconds);

        previousPlaybackTimeSeconds = currentPlaybackTimeSeconds;
        hasPreviousPlaybackTime = true;
    }

    /// <summary>
    /// リトライ時に全イベントの実行済み状態と曲位置の記録をリセットします。
    /// UnityEventやゲーム管理側からも呼び出せます。
    /// </summary>
    public void ResetExecutionState()
    {
        for (int index = 0; index < sequenceEvents.Count; index++)
        {
            sequenceEvents[index].ResetExecutionState();
        }

        previousPlaybackTimeSeconds = 0d;
        hasPreviousPlaybackTime = false;
    }

    /// <summary>
    /// 終了演出中に未実行イベントが発火しないよう、曲中イベントの監視を切り替えます。
    /// </summary>
    public void SetSequenceEnabled(bool isEnabled)
    {
        isSequenceEnabled = isEnabled;
        hasPreviousPlaybackTime = false;
    }

    /// <summary>
    /// Inspector設定から判定用スケジュールを再構築します。
    /// </summary>
    private void RebuildSchedule()
    {
        scheduledEvents.Clear();

        for (int index = 0; index < sequenceEvents.Count; index++)
        {
            SequenceEvent sequenceEvent = sequenceEvents[index];
            double playbackTimeSeconds = sequenceEvent.Timing.ToPlaybackTimeSeconds(musicController.SongDefinition);
            scheduledEvents.Add(new ScheduledEvent(sequenceEvent, playbackTimeSeconds, index));
        }

        scheduledEvents.Sort(CompareScheduledEvents);
    }

    /// <summary>
    /// 同時刻では実行順を優先し、同じ実行順ならInspectorの登録順を維持します。
    /// </summary>
    private static int CompareScheduledEvents(ScheduledEvent left, ScheduledEvent right)
    {
        int timeComparison = left.PlaybackTimeSeconds.CompareTo(right.PlaybackTimeSeconds);
        if (timeComparison != 0)
        {
            return timeComparison;
        }

        int orderComparison = left.SequenceEvent.ExecutionOrder.CompareTo(right.SequenceEvent.ExecutionOrder);
        return orderComparison != 0
            ? orderComparison
            : left.RegistrationIndex.CompareTo(right.RegistrationIndex);
    }

    /// <summary>
    /// 「前回位置 &lt; イベント位置 &lt;= 現在位置」を満たすすべてのイベントを実行します。
    /// </summary>
    private void ExecutePassedEvents(double rangeStart, double rangeEnd)
    {
        if (rangeEnd < rangeStart)
        {
            return;
        }

        for (int index = 0; index < scheduledEvents.Count; index++)
        {
            ScheduledEvent scheduledEvent = scheduledEvents[index];
            if (rangeStart < scheduledEvent.PlaybackTimeSeconds
                && scheduledEvent.PlaybackTimeSeconds <= rangeEnd)
            {
                scheduledEvent.SequenceEvent.Invoke();
            }
        }
    }
}

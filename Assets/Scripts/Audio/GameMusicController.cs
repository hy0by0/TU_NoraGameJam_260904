using System.Collections;
using UnityEngine;

/// <summary>
/// DSP時計とAudioSourceのサンプル位置を基準に、BGMとゲームの拍進行を管理するクラスです。
/// </summary>
[DefaultExecutionOrder(-1000)]
public class GameMusicController : MusicConductor
{
    public enum PlaybackState
    {
        Stopped,
        Scheduled,
        PreRoll,
        Playing,
        Paused,
        Finished
    }

    [Header("MainScene用の曲設定")]
    [SerializeField] private SongDefinition songDefinition;

    private PlaybackState state = PlaybackState.Stopped;
    private double scheduledStartDspTime;
    private double pausedAtDspTime;
    private double pausedPreRollSeconds;
    private float playbackTimeSeconds;
    private bool pausedDuringPreRoll;
    private bool hasObservedPlayback;
    private Coroutine slowStopCoroutine;
    private bool hasSlowStopCoroutine;

    public SongDefinition SongDefinition => songDefinition;
    public PlaybackState State => state;
    public double ScheduledStartDspTime => scheduledStartDspTime;
    public override float Bpm => songDefinition.Bpm;
    public override float ScrollSpeed => songDefinition.BaseScrollSpeed;
    public override float SecondsPerBeat => songDefinition.SecondsPerBeat;
    public override float DistancePerBeat => ScrollSpeed * SecondsPerBeat;
    public override float PlaybackTimeSeconds => playbackTimeSeconds;
    public bool IsScheduled => state == PlaybackState.Scheduled || state == PlaybackState.PreRoll;
    public bool HasPlaybackStarted => state == PlaybackState.Playing || (state == PlaybackState.Paused && !pausedDuringPreRoll);
    public bool IsStopped => state == PlaybackState.Stopped;
    public bool IsFinished => state == PlaybackState.Finished;
    public float CurrentPitch => bgmSource.pitch;
    public bool IsAudioPlaying => bgmSource.isPlaying;
    public override bool IsGameRunning => state == PlaybackState.Playing;
    public float PreRollRemainingSeconds => state == PlaybackState.Paused && pausedDuringPreRoll
        ? (float)pausedPreRollSeconds
        : IsScheduled
            ? Mathf.Max(0f, (float)(scheduledStartDspTime - AudioSettings.dspTime))
            : 0f;
    public float PreRollRemainingBeats => PreRollRemainingSeconds / SecondsPerBeat;

    private float MusicalTimeSeconds => Mathf.Max(0f, playbackTimeSeconds - songDefinition.FirstBeatOffsetSeconds);
    public float CurrentBeatFloat => MusicalTimeSeconds / SecondsPerBeat;
    public int CurrentBar => HasPlaybackStarted ? Mathf.FloorToInt(CurrentBeatFloat / songDefinition.BeatsPerBar) + 1 : 0;
    public new int CurrentBeat => HasPlaybackStarted ? Mathf.FloorToInt(CurrentBeatFloat) % songDefinition.BeatsPerBar + 1 : 0;
    public int CurrentSubdivision => HasPlaybackStarted
        ? Mathf.FloorToInt((CurrentBeatFloat - Mathf.Floor(CurrentBeatFloat)) * songDefinition.SubdivisionsPerBeat) + 1
        : 0;

    /// <summary>
    /// Inspectorで設定された曲をAudioSourceへ反映し、停止状態へ初期化します。
    /// </summary>
    private void Awake()
    {
        bgmSource.playOnAwake = false;
        bgmSource.loop = false;
        bgmSource.clip = songDefinition.GameAudioClip;
        bgmSource.pitch = 1f;
        state = PlaybackState.Stopped;
    }

    /// <summary>
    /// DSP時計とAudioSourceのサンプル位置から、現在状態と拍位置を更新します。
    /// </summary>
    protected override void Update()
    {
        if (state == PlaybackState.Scheduled)
        {
            state = PlaybackState.PreRoll;
        }

        if (state == PlaybackState.PreRoll && AudioSettings.dspTime >= scheduledStartDspTime)
        {
            state = PlaybackState.Playing;
        }

        if (state != PlaybackState.Playing)
        {
            return;
        }

        playbackTimeSeconds = (float)bgmSource.timeSamples / bgmSource.clip.frequency;
        hasObservedPlayback |= bgmSource.isPlaying;

        if (CurrentBeatFloat >= songDefinition.GameEndBeat || (hasObservedPlayback && !bgmSource.isPlaying))
        {
            FinishMusic();
        }
    }

    /// <summary>
    /// PreRoll拍数を秒へ変換し、その時間後のDSP時刻へBGMを予約します。
    /// </summary>
    public void ScheduleMusicWithPreRoll()
    {
        CancelSlowStop();
        bgmSource.Stop();
        bgmSource.clip = songDefinition.GameAudioClip;
        bgmSource.timeSamples = 0;
        bgmSource.pitch = 1f;
        playbackTimeSeconds = 0f;
        pausedDuringPreRoll = false;
        hasObservedPlayback = false;

        scheduledStartDspTime = AudioSettings.dspTime + songDefinition.PreRollSeconds;
        state = PlaybackState.Scheduled;
        bgmSource.PlayScheduled(scheduledStartDspTime);
    }

    /// <summary>
    /// 従来コードとの互換用に、PreRoll付き予約再生を開始します。
    /// </summary>
    public override void PlayMusic()
    {
        ScheduleMusicWithPreRoll();
    }

    /// <summary>
    /// BGMを通常停止し、拍進行を先頭へ戻します。
    /// </summary>
    public override void StopMusic()
    {
        CancelSlowStop();
        bgmSource.Stop();
        bgmSource.pitch = 1f;
        playbackTimeSeconds = 0f;
        pausedDuringPreRoll = false;
        state = PlaybackState.Stopped;
    }

    /// <summary>
    /// PreRollまたは再生中のBGMを一時停止します。
    /// </summary>
    public void PauseMusic()
    {
        if (state == PlaybackState.PreRoll || state == PlaybackState.Scheduled)
        {
            pausedPreRollSeconds = PreRollRemainingSeconds;
            pausedDuringPreRoll = true;
            bgmSource.Stop();
        }
        else if (state == PlaybackState.Playing)
        {
            pausedDuringPreRoll = false;
            bgmSource.Pause();
        }
        else
        {
            return;
        }

        pausedAtDspTime = AudioSettings.dspTime;
        state = PlaybackState.Paused;
    }

    /// <summary>
    /// 一時停止したPreRollまたはBGM再生を同じ位置から再開します。
    /// </summary>
    public void ResumeMusic()
    {
        if (state != PlaybackState.Paused)
        {
            return;
        }

        if (pausedDuringPreRoll)
        {
            scheduledStartDspTime = AudioSettings.dspTime + pausedPreRollSeconds;
            bgmSource.PlayScheduled(scheduledStartDspTime);
            state = PlaybackState.PreRoll;
            return;
        }

        scheduledStartDspTime += AudioSettings.dspTime - pausedAtDspTime;
        bgmSource.UnPause();
        state = PlaybackState.Playing;
    }

    /// <summary>
    /// ゲームオーバー演出向けに、速度を徐々に落として停止します。
    /// </summary>
    public virtual void BeginSlowStop(float durationSeconds)
    {
        CancelSlowStop();
        slowStopCoroutine = StartCoroutine(SlowStopRoutine(durationSeconds));
        hasSlowStopCoroutine = true;
    }

    /// <summary>
    /// BGM速度を徐々に下げる処理です。派生クラスで演出を拡張できます。
    /// </summary>
    protected virtual IEnumerator SlowStopRoutine(float durationSeconds)
    {
        float elapsed = 0f;
        float startPitch = bgmSource.pitch;

        while (elapsed < durationSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.pitch = Mathf.Lerp(startPitch, 0f, elapsed / durationSeconds);
            yield return null;
        }

        hasSlowStopCoroutine = false;
        StopMusic();
    }

    /// <summary>
    /// 曲の終了状態を確定し、AudioSourceを停止します。
    /// </summary>
    private void FinishMusic()
    {
        bgmSource.Stop();
        state = PlaybackState.Finished;
    }

    /// <summary>
    /// 実行中のスロー停止処理だけを解除します。
    /// </summary>
    private void CancelSlowStop()
    {
        if (hasSlowStopCoroutine)
        {
            StopCoroutine(slowStopCoroutine);
            hasSlowStopCoroutine = false;
        }
    }
}

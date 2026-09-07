using UnityEngine;

/// <summary>
/// 既存シーン向けに、BGM再生とゲーム共通のリズム・移動設定を管理する基準時計です。
/// MainSceneでは派生クラスのGameMusicControllerを使用します。
/// </summary>
[DefaultExecutionOrder(-1000)]
public class MusicConductor : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] protected AudioSource bgmSource;

    [Header("ゲーム全体のタイミング設定")]
    [SerializeField, Min(1f)] private float bpm = 170f;
    [SerializeField, Min(0f)] private float scrollSpeed = 10f;
    [SerializeField] protected float songOriginX = -2f;
    [SerializeField, Min(0f)] private float gameStartDelaySeconds;

    private bool isMusicScheduled;
    private double scheduledStartDspTime;
    private float playbackTimeSeconds;

    public virtual float Bpm => bpm;
    public virtual float ScrollSpeed => scrollSpeed;
    public virtual float SongOriginX => songOriginX;
    public virtual float SecondsPerBeat => 60f / Bpm;
    public virtual float DistancePerBeat => ScrollSpeed * SecondsPerBeat;
    public virtual bool IsGameRunning => isMusicScheduled
        && AudioSettings.dspTime >= scheduledStartDspTime
        && bgmSource.isPlaying;
    public virtual float PlaybackTimeSeconds => playbackTimeSeconds;
    public float CurrentBeat => PlaybackTimeSeconds / SecondsPerBeat;

    /// <summary>
    /// AudioSourceのサンプル位置から、既存シーン用の再生経過時間を更新します。
    /// </summary>
    protected virtual void Update()
    {
        playbackTimeSeconds = IsGameRunning
            ? (float)bgmSource.timeSamples / bgmSource.clip.frequency
            : 0f;
    }

    /// <summary>
    /// 拍数を共通BPMに基づく秒数へ変換します。
    /// </summary>
    public float BeatsToSeconds(float beats)
    {
        return beats * SecondsPerBeat;
    }

    /// <summary>
    /// 拍位置を共通スクロール設定に基づくワールドX座標へ変換します。
    /// </summary>
    public float BeatToWorldX(float beat)
    {
        return SongOriginX + beat * DistancePerBeat;
    }

    /// <summary>
    /// ワールドX座標を拍位置へ変換します。
    /// </summary>
    public float WorldXToBeat(float worldX)
    {
        return (worldX - SongOriginX) / DistancePerBeat;
    }

    /// <summary>
    /// 既存シーン用の待機秒数後へBGMを予約します。
    /// </summary>
    public virtual void PlayMusic()
    {
        bgmSource.Stop();
        bgmSource.timeSamples = 0;
        playbackTimeSeconds = 0f;
        scheduledStartDspTime = AudioSettings.dspTime + gameStartDelaySeconds;
        isMusicScheduled = true;
        bgmSource.PlayScheduled(scheduledStartDspTime);
    }

    /// <summary>
    /// BGMと既存シーン用の基準時計を停止します。
    /// </summary>
    public virtual void StopMusic()
    {
        bgmSource.Stop();
        isMusicScheduled = false;
        playbackTimeSeconds = 0f;
    }
}

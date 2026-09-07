using UnityEngine;

/// <summary>
/// 曲の再生位置を基準に、ステージの進行距離と拍ガイドのワールド位置を管理するクラスです。
/// </summary>
[DefaultExecutionOrder(-800)]
public class StageProgressController : MonoBehaviour
{
    [Header("スクロール設定")]
    [SerializeField] private BeatTiming scrollStartBeatTiming = new BeatTiming();
    [SerializeField] private float scrollStartReferenceX = -2f;
    [SerializeField, Min(0f)] private float baseScrollSpeed = 10f;

    [Header("参照")]
    [SerializeField] private SongDefinition songDefinition;
    [SerializeField] private GameMusicController musicController;

    private float progressDistance;
    private bool isFrozen;
    private bool wasPreparingPlayback;

    public float ProgressDistance => progressDistance;
    public bool IsFrozen => isFrozen;
    public double ScrollStartBeatPosition => scrollStartBeatTiming.ToBeatPosition(songDefinition);
    public float DistancePerBeat => baseScrollSpeed * songDefinition.SecondsPerBeat;
    public bool HasStartedScrolling => musicController.HasPlaybackStarted
        && musicController.CurrentBeatFloat >= ScrollStartBeatPosition;

    /// <summary>
    /// 現在の曲位置から進行距離を直接計算します。
    /// </summary>
    private void Update()
    {
        bool isPreparingPlayback = musicController.State == GameMusicController.PlaybackState.Scheduled
            || musicController.State == GameMusicController.PlaybackState.PreRoll;

        if (isPreparingPlayback && !wasPreparingPlayback)
        {
            ResetProgress();
        }

        wasPreparingPlayback = isPreparingPlayback;

        if (isFrozen)
        {
            return;
        }

        if (musicController.IsFinished)
        {
            CalculateProgressFromCurrentSongPosition();
            FreezeProgress();
            return;
        }

        if (!HasStartedScrolling)
        {
            progressDistance = 0f;
            return;
        }

        CalculateProgressFromCurrentSongPosition();
    }

    /// <summary>
    /// AudioSource由来の現在拍と開始拍の差から、進行距離を直接求めます。
    /// </summary>
    private void CalculateProgressFromCurrentSongPosition()
    {
        double elapsedBeats = musicController.CurrentBeatFloat - ScrollStartBeatPosition;
        progressDistance = Mathf.Max(0f, (float)elapsedBeats * DistancePerBeat);
    }

    /// <summary>
    /// 指定した拍のガイド線Xを、スクロール開始拍との距離から求めます。
    /// </summary>
    public float BeatToWorldX(double targetBeat)
    {
        return scrollStartReferenceX
            + (float)(targetBeat - ScrollStartBeatPosition) * DistancePerBeat;
    }

    /// <summary>
    /// ワールドX座標を曲先頭からの拍位置へ戻します。
    /// </summary>
    public double WorldXToBeat(float worldX)
    {
        return ScrollStartBeatPosition + (worldX - scrollStartReferenceX) / DistancePerBeat;
    }

    /// <summary>
    /// ゲームオーバーなどの時点で、現在の進行距離を固定します。
    /// </summary>
    public void FreezeProgress()
    {
        isFrozen = true;
    }

    /// <summary>
    /// リトライ開始時に進行距離と固定状態を初期化します。
    /// </summary>
    public void ResetProgress()
    {
        progressDistance = 0f;
        isFrozen = false;
    }
}

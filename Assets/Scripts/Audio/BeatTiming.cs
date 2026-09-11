using System;
using UnityEngine;

/// <summary>
/// 曲中の小節・拍・拍内分割位置を、すべて1始まりで保持するデータです。
/// </summary>
[Serializable]
public class BeatTiming
{
    [SerializeField, Min(1)] private int bar = 1;
    [SerializeField, Min(1)] private int beat = 1;
    [SerializeField, Min(1)] private int subdivision = 1;

    public int Bar => bar;
    public int Beat => beat;
    public int Subdivision => subdivision;

    // 初期値もInspectorと同じ1始まりで指定します。
    public BeatTiming(int bar = 1, int beat = 1, int subdivision = 1)
    {
        this.bar = bar;
        this.beat = beat;
        this.subdivision = subdivision;
    }

    /// <summary>
    /// 1始まりの指定位置を、曲の先頭拍からの0始まり拍位置へ変換します。
    /// </summary>
    public double ToBeatPosition(SongDefinition songDefinition)
    {
        int zeroBasedBar = Mathf.Max(0, bar - 1);
        int zeroBasedBeat = Mathf.Clamp(beat - 1, 0, songDefinition.BeatsPerBar - 1);
        int zeroBasedSubdivision = Mathf.Clamp(subdivision - 1, 0, songDefinition.SubdivisionsPerBeat - 1);

        return zeroBasedBar * songDefinition.BeatsPerBar
            + zeroBasedBeat
            + (double)zeroBasedSubdivision / songDefinition.SubdivisionsPerBeat;
    }

    /// <summary>
    /// 指定位置を、AudioSource上の曲再生秒へ変換します。
    /// </summary>
    public double ToPlaybackTimeSeconds(SongDefinition songDefinition)
    {
        return songDefinition.FirstBeatOffsetSeconds + ToBeatPosition(songDefinition) * songDefinition.SecondsPerBeat;
    }
}

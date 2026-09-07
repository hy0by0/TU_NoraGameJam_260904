using UnityEngine;

/// <summary>
/// 1曲分の音源、拍構造、ゲーム進行設定をまとめて保持するデータです。
/// </summary>
[CreateAssetMenu(fileName = "SongDefinition", menuName = "NoraGameJam/Song Definition")]
public class SongDefinition : ScriptableObject
{
    [Header("音源")]
    [SerializeField] private AudioClip gameAudioClip;

    [Header("拍設定")]
    [SerializeField, Min(1f)] private float bpm = 170f;
    [SerializeField, Min(1)] private int beatsPerBar = 4;
    [SerializeField, Min(1)] private int subdivisionsPerBeat = 4;
    [SerializeField, Min(0f)] private float firstBeatOffsetSeconds;
    [SerializeField, Min(0)] private int preRollBeats = 4;
    [SerializeField, Min(1)] private int gameEndBeat = 288;

    [Header("ゲーム進行")]
    [SerializeField, Min(0f)] private float baseScrollSpeed = 10f;

    public AudioClip GameAudioClip => gameAudioClip;
    public float Bpm => bpm;
    public int BeatsPerBar => beatsPerBar;
    public int SubdivisionsPerBeat => subdivisionsPerBeat;
    public float FirstBeatOffsetSeconds => firstBeatOffsetSeconds;
    public int PreRollBeats => preRollBeats;
    public int GameEndBeat => gameEndBeat;
    public float BaseScrollSpeed => baseScrollSpeed;
    public float SecondsPerBeat => 60f / bpm;
    public float PreRollSeconds => SecondsPerBeat * preRollBeats;
}

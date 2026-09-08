using UnityEngine;

/// <summary>
/// 現在のシーンに配置されたAudioSourceを使ってSEを再生するクラスです。
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource seSource;

    /// <summary>
    /// 現在のシーンのAudioManagerを共有参照として登録します。
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 指定したSEを一度再生します。
    /// </summary>
    public void PlaySE(AudioClip clip)
    {
        seSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 指定した経過秒数に再生位置を合わせ、時間軸に沿ってSEを再生します。
    /// </summary>
    public void PlaySEFromTime(AudioClip clip, float elapsedSeconds)
    {
        float playbackPosition = Mathf.Clamp(elapsedSeconds, 0f, clip.length);
        if (playbackPosition >= clip.length) return;

        seSource.clip = clip;
        seSource.time = playbackPosition;
        seSource.Play();
    }

    /// <summary>
    /// 最終イベントなど、以降のSEを完全に止める場面で再生中のSEを停止します。
    /// </summary>
    public void StopSE()
    {
        seSource.Stop();
    }
}

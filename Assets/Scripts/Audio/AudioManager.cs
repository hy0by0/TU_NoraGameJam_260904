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
    /// 最終イベントなど、以降のSEを完全に止める場面で再生中のSEを停止します。
    /// </summary>
    public void StopSE()
    {
        seSource.Stop();
    }
}

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// BGMとSEのスライダー値をAudioMixerの音量へ反映するクラス。
/// </summary>
public class AudioVolume : MonoBehaviour
{
    private const string BgmVolumeParameter = "BGM";
    private const string SeVolumeParameter = "SE";
    private const float MinimumVolumeDecibels = -80f;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    private void Start()
    {
        // スライダーを動かしたときに、それぞれの音量を更新します。
        bgmSlider.onValueChanged.AddListener(SetAudioMixerBGM);
        seSlider.onValueChanged.AddListener(SetAudioMixerSE);

        // ゲーム開始時にも、現在表示されているスライダー値を音量へ反映します。
        SetAudioMixerBGM(bgmSlider.value);
        SetAudioMixerSE(seSlider.value);
    }

    /// <summary>
    /// BGMスライダーの値をAudioMixerのBGM音量へ反映します。
    /// </summary>
    public void SetAudioMixerBGM(float value)
    {
        SetVolume(BgmVolumeParameter, value, bgmSlider.maxValue);
    }

    /// <summary>
    /// SEスライダーの値をAudioMixerのSE音量へ反映します。
    /// </summary>
    public void SetAudioMixerSE(float value)
    {
        SetVolume(SeVolumeParameter, value, seSlider.maxValue);
    }

    /// <summary>
    /// スライダー値を0～1へ正規化し、聞こえ方に合うデシベル値へ変換します。
    /// </summary>
    private void SetVolume(string parameterName, float sliderValue, float sliderMaxValue)
    {
        float normalizedValue = sliderValue / sliderMaxValue;
        float volumeDecibels = normalizedValue > 0f
            ? Mathf.Log10(normalizedValue) * 20f
            : MinimumVolumeDecibels;

        audioMixer.SetFloat(parameterName, volumeDecibels);
    }
}

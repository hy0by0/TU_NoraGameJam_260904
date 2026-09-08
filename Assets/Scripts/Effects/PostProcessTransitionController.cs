using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Inspectorで接続した3種類のVolume Weightを切り替えるクラスです。
/// </summary>
public class PostProcessTransitionController : MonoBehaviour
{
    [Header("事前配置Volume")]
    [SerializeField] private Volume normalVolume;
    [SerializeField] private Volume brightAdaptationVolume;
    [SerializeField] private Volume whiteoutVolume;

    [Header("切り替え時間")]
    [SerializeField, Min(0f)] private float transitionDuration = 0.35f;
    [SerializeField] private Ease transitionEase = Ease.InOutSine;

    /// <summary>
    /// 通常のVolumeへ滑らかに切り替えます。
    /// </summary>
    public void TransitionToNormal()
    {
        TransitionWeights(1f, 0f, 0f);
    }

    /// <summary>
    /// 明順応演出用のVolumeへ滑らかに切り替えます。
    /// </summary>
    public void TransitionToBrightAdaptation()
    {
        TransitionWeights(0f, 1f, 0f);
    }

    /// <summary>
    /// ホワイトアウト用のVolumeへ滑らかに切り替えます。
    /// </summary>
    public void TransitionToWhiteout()
    {
        TransitionWeights(0f, 0f, 1f);
    }

    public void SetNormalImmediate() => SetWeightsImmediate(1f, 0f, 0f);
    public void SetBrightAdaptationImmediate() => SetWeightsImmediate(0f, 1f, 0f);
    public void SetWhiteoutImmediate() => SetWeightsImmediate(0f, 0f, 1f);

    /// <summary>
    /// UnityEventのfloat引数から明順応VolumeのWeightだけを変更します。
    /// </summary>
    public void SetBrightAdaptationWeight(float weight)
    {
        brightAdaptationVolume.weight = Mathf.Clamp01(weight);
    }

    /// <summary>
    /// UnityEventのfloat引数からホワイトアウトVolumeのWeightだけを変更します。
    /// </summary>
    public void SetWhiteoutWeight(float weight)
    {
        whiteoutVolume.weight = Mathf.Clamp01(weight);
    }

    /// <summary>
    /// 3つのWeightを同時にTweenします。
    /// </summary>
    private void TransitionWeights(float normal, float bright, float whiteout)
    {
        DOTween.Kill(this);
        DOTween.To(() => normalVolume.weight, value => normalVolume.weight = value, normal, transitionDuration).SetEase(transitionEase).SetId(this);
        DOTween.To(() => brightAdaptationVolume.weight, value => brightAdaptationVolume.weight = value, bright, transitionDuration).SetEase(transitionEase).SetId(this);
        DOTween.To(() => whiteoutVolume.weight, value => whiteoutVolume.weight = value, whiteout, transitionDuration).SetEase(transitionEase).SetId(this);
    }

    /// <summary>
    /// 3つのWeightをアニメーションなしで設定します。
    /// </summary>
    private void SetWeightsImmediate(float normal, float bright, float whiteout)
    {
        DOTween.Kill(this);
        normalVolume.weight = normal;
        brightAdaptationVolume.weight = bright;
        whiteoutVolume.weight = whiteout;
    }
}

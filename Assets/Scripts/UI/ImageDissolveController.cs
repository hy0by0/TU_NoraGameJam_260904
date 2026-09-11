using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Inspectorで指定したUGUI画像をノイズ状に表示・非表示にします。</summary>
[RequireComponent(typeof(Image))]
public class ImageDissolveController : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Material dissolveMaterial;
    [SerializeField, Min(0f)] private float duration = 0.6f;
    [SerializeField, Min(1f)] private float noiseScale = 70f;
    [SerializeField, Range(0.001f, 0.5f)] private float softness = 0.06f;
    [SerializeField] private bool reverse;
    private Material instanceMaterial;
    private bool initialized;
    private float progress = 1f;

    /// <summary>共有アセットを変更しないよう、画像ごとにMaterialだけを複製します。</summary>
    private void Initialize()
    {
        if (initialized) return;
        instanceMaterial = new Material(dissolveMaterial);
        targetImage.material = instanceMaterial;
        initialized = true;
    }

    private void Awake() => SetProgress(1f);

    /// <summary>0で非表示、1で完全表示になるよう進行率を指定します。</summary>
    public void SetProgress(float value)
    {
        Initialize();
        progress = Mathf.Clamp01(value);
        instanceMaterial.SetFloat("_Progress", progress);
        instanceMaterial.SetFloat("_NoiseScale", noiseScale);
        instanceMaterial.SetFloat("_Softness", softness);
        instanceMaterial.SetFloat("_Reverse", reverse ? 1f : 0f);
    }

    /// <summary>UnityEventからディゾルブ表示を開始します。</summary>
    public void ShowDissolve()
    {
        DOTween.Kill(this);
        SetProgress(0f);
        DOTween.To(() => progress, SetProgress, 1f, duration).SetId(this);
    }

    /// <summary>UnityEventからディゾルブ非表示を開始します。</summary>
    public void HideDissolve()
    {
        DOTween.Kill(this);
        DOTween.To(() => progress, SetProgress, 0f, duration).SetId(this);
    }

    /// <summary>画像専用に作ったMaterialとTweenを解放します。</summary>
    private void OnDestroy()
    {
        DOTween.Kill(this);
        if (initialized) Destroy(instanceMaterial);
    }
}

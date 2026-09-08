using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Inspectorで登録したSpriteRenderer群の色と透明度をまとめて変更するクラスです。
/// </summary>
public class CharacterColorController : MonoBehaviour
{
    [Serializable]
    public class ColorPreset
    {
        [SerializeField] private string presetName = "新しい色";
        [SerializeField] private Color color = Color.white;

        public string PresetName => presetName;
        public Color Color => color;
    }

    [Header("色を変更するRenderer")]
    [SerializeField] private List<SpriteRenderer> targetRenderers = new List<SpriteRenderer>();

    [Header("UnityEvent用の色プリセット")]
    [SerializeField] private List<ColorPreset> colorPresets = new List<ColorPreset>();
    [SerializeField, Min(0f)] private float transitionDuration = 0.2f;

    private readonly List<Color> initialColors = new List<Color>();

    /// <summary>
    /// Inspectorで設定された初期色を復元用に記録します。
    /// </summary>
    private void Awake()
    {
        initialColors.Clear();
        for (int index = 0; index < targetRenderers.Count; index++)
        {
            initialColors.Add(targetRenderers[index].color);
        }
    }

    /// <summary>
    /// UnityEventの整数引数で指定した色プリセットを即時適用します。
    /// </summary>
    public void ApplyPresetImmediate(int presetIndex)
    {
        ApplyColorImmediate(colorPresets[presetIndex].Color);
    }

    /// <summary>
    /// UnityEventの整数引数で指定した色プリセットへ変化させます。
    /// </summary>
    public void TransitionToPreset(int presetIndex)
    {
        TransitionToColor(colorPresets[presetIndex].Color);
    }

    /// <summary>
    /// UnityEventのfloat引数で全対象のAlphaを即時変更します。
    /// </summary>
    public void SetAlpha(float alpha)
    {
        float clampedAlpha = Mathf.Clamp01(alpha);
        for (int index = 0; index < targetRenderers.Count; index++)
        {
            SpriteRenderer target = targetRenderers[index];
            Color current = target.color;
            target.color = new Color(current.r, current.g, current.b, clampedAlpha);
        }
    }

    /// <summary>
    /// Inspectorで記録した開始時の色へ戻します。
    /// </summary>
    public void RestoreInitialColors()
    {
        for (int index = 0; index < targetRenderers.Count; index++)
        {
            targetRenderers[index].DOKill();
            targetRenderers[index].color = initialColors[index];
        }
    }

    /// <summary>
    /// 指定色を全Rendererへ即時適用します。
    /// </summary>
    public void ApplyColorImmediate(Color color)
    {
        for (int index = 0; index < targetRenderers.Count; index++)
        {
            targetRenderers[index].DOKill();
            targetRenderers[index].color = color;
        }
    }

    /// <summary>
    /// 指定色へ全Rendererを滑らかに変化させます。
    /// </summary>
    public void TransitionToColor(Color color)
    {
        for (int index = 0; index < targetRenderers.Count; index++)
        {
            targetRenderers[index].DOKill();
            targetRenderers[index].DOColor(color, transitionDuration);
        }
    }
}

using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトル再現背景・固定背景・Parallax背景セットの切り替え演出を管理するクラスです。
/// </summary>
[DefaultExecutionOrder(-700)]
public class BackgroundTransitionController : MonoBehaviour
{
    /// <summary>
    /// 固定背景とParallax背景セットの組み合わせを表す、拍イベント向けの背景設定です。
    /// </summary>
    [Serializable]
    public class BackgroundSet
    {
        [SerializeField, InspectorName("管理用背景名")] private string backgroundName = "ゲーム背景";
        [SerializeField, InspectorName("固定背景Sprite")] private Sprite fixedBackgroundSprite;
        [SerializeField, Min(0), InspectorName("Parallaxセット番号")] private int parallaxSetIndex;

        public Sprite FixedBackgroundSprite => fixedBackgroundSprite;
        public int ParallaxSetIndex => parallaxSetIndex;
    }

    [Header("固定背景")]
    [SerializeField] private Image titleReplicaBackground;
    [SerializeField] private Image fixedBackgroundCurrent;
    [SerializeField] private Image fixedBackgroundNext;

    [Header("Parallax背景")]
    [SerializeField] private CanvasGroup parallaxBackgroundCanvasGroup;
    [SerializeField] private ParallaxController parallaxController;

    [Header("切り替え演出")]
    [SerializeField] private Image flashOverlay;
    [SerializeField, Min(0f)] private float crossFadeDuration = 1f;
    [SerializeField, Min(0f)] private float flashInDuration = 0.12f;
    [SerializeField, Min(0f)] private float flashOutDuration = 0.28f;

    [Header("切り替え可能な背景")]
    [SerializeField] private List<BackgroundSet> backgroundSets = new List<BackgroundSet>();
    [SerializeField, Min(0)] private int initialBackgroundSetIndex;
    [SerializeField, InspectorName("開始直後から背景セットを表示")] private bool showBackgroundSetOnAwake;
    [SerializeField, Min(0), InspectorName("ゲームオーバー背景セット番号")] private int gameOverBackgroundSetIndex = 3;

    public int CurrentBackgroundSetIndex { get; private set; }

    /// <summary>
    /// MainScene開始時の表示方式と背景セットをInspectorから反映します。
    /// </summary>
    private void Awake()
    {
        BackgroundSet initialSet = backgroundSets[initialBackgroundSetIndex];
        fixedBackgroundCurrent.sprite = initialSet.FixedBackgroundSprite;
        fixedBackgroundCurrent.color = new Color(1f, 1f, 1f, 0f);
        fixedBackgroundNext.color = new Color(1f, 1f, 1f, 0f);
        titleReplicaBackground.color = Color.white;
        parallaxBackgroundCanvasGroup.alpha = 0f;
        flashOverlay.color = new Color(0f, 0f, 0f, 0f);
        flashOverlay.raycastTarget = false;
        parallaxController.ApplySet(initialSet.ParallaxSetIndex);
        CurrentBackgroundSetIndex = initialBackgroundSetIndex;
        if (showBackgroundSetOnAwake)
        {
            TransitionFromTitleImmediate();
        }
    }

    /// <summary>
    /// Title Scene再現背景を消し、初期ゲーム背景を即時表示します。
    /// </summary>
    public void TransitionFromTitleImmediate()
    {
        KillTransitionTweens();
        titleReplicaBackground.color = new Color(1f, 1f, 1f, 0f);
        fixedBackgroundCurrent.color = Color.white;
        parallaxBackgroundCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Title Scene再現背景から初期ゲーム背景へクロスフェードします。
    /// </summary>
    public void TransitionFromTitleCrossFade()
    {
        KillTransitionTweens();
        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Join(titleReplicaBackground.DOFade(0f, crossFadeDuration));
        sequence.Join(fixedBackgroundCurrent.DOFade(1f, crossFadeDuration));
        sequence.Join(parallaxBackgroundCanvasGroup.DOFade(1f, crossFadeDuration));
    }

    /// <summary>
    /// 黒フラッシュを挟んでTitle Scene再現背景からゲーム背景へ切り替えます。
    /// </summary>
    public void TransitionFromTitleWithBlackFlash()
    {
        TransitionFromTitleWithFlash(Color.black);
    }

    /// <summary>
    /// 白フラッシュを挟んでTitle Scene再現背景からゲーム背景へ切り替えます。
    /// </summary>
    public void TransitionFromTitleWithWhiteFlash()
    {
        TransitionFromTitleWithFlash(Color.white);
    }

    /// <summary>
    /// UnityEventから指定番号の固定背景とParallax背景を即時切り替えます。
    /// </summary>
    public void SwitchBackgroundImmediate(int backgroundSetIndex)
    {
        KillTransitionTweens();
        ApplyBackgroundSet(backgroundSetIndex);
    }

    /// <summary>
    /// UnityEventから指定番号の固定背景とParallax背景へクロスフェードします。
    /// </summary>
    public void SwitchBackgroundCrossFade(int backgroundSetIndex)
    {
        KillTransitionTweens();
        BackgroundSet targetSet = backgroundSets[backgroundSetIndex];
        fixedBackgroundNext.sprite = targetSet.FixedBackgroundSprite;
        fixedBackgroundNext.color = new Color(1f, 1f, 1f, 0f);

        float halfDuration = crossFadeDuration * 0.5f;
        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Join(fixedBackgroundCurrent.DOFade(0f, crossFadeDuration));
        sequence.Join(fixedBackgroundNext.DOFade(1f, crossFadeDuration));
        sequence.AppendCallback(() => CompleteFixedBackgroundSwap(backgroundSetIndex));
        sequence.Insert(0f, parallaxBackgroundCanvasGroup.DOFade(0f, halfDuration));
        sequence.InsertCallback(halfDuration, () => parallaxController.ApplySet(targetSet.ParallaxSetIndex));
        sequence.Insert(halfDuration, parallaxBackgroundCanvasGroup.DOFade(1f, halfDuration));
    }

    /// <summary>
    /// UnityEventから黒フラッシュを挟んで指定番号の背景へ切り替えます。
    /// </summary>
    public void SwitchBackgroundWithBlackFlash(int backgroundSetIndex)
    {
        SwitchBackgroundWithFlash(backgroundSetIndex, Color.black);
    }

    /// <summary>
    /// UnityEventから白フラッシュを挟んで指定番号の背景へ切り替えます。
    /// </summary>
    public void SwitchBackgroundWithWhiteFlash(int backgroundSetIndex)
    {
        SwitchBackgroundWithFlash(backgroundSetIndex, Color.white);
    }

    /// <summary>
    /// UnityEventからParallax背景セットだけを即時切り替えます。
    /// </summary>
    public void SwitchParallaxSetImmediate(int parallaxSetIndex)
    {
        parallaxController.ApplySet(parallaxSetIndex);
    }

    /// <summary>
    /// Inspectorで指定したゲームオーバー用背景へクロスフェードします。
    /// </summary>
    public void TransitionToGameOverBackground()
    {
        SwitchBackgroundCrossFade(gameOverBackgroundSetIndex);
    }

    /// <summary>
    /// 指定色のフラッシュ頂点でTitle Scene再現背景をゲーム背景へ置き換えます。
    /// </summary>
    private void TransitionFromTitleWithFlash(Color flashColor)
    {
        KillTransitionTweens();
        PrepareFlash(flashColor);

        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Append(flashOverlay.DOFade(1f, flashInDuration));
        sequence.AppendCallback(TransitionFromTitleImmediateWithoutKilling);
        sequence.Append(flashOverlay.DOFade(0f, flashOutDuration));
    }

    /// <summary>
    /// 指定色のフラッシュ頂点で固定背景とParallax背景を同時に切り替えます。
    /// </summary>
    private void SwitchBackgroundWithFlash(int backgroundSetIndex, Color flashColor)
    {
        KillTransitionTweens();
        PrepareFlash(flashColor);

        Sequence sequence = DOTween.Sequence().SetId(this);
        sequence.Append(flashOverlay.DOFade(1f, flashInDuration));
        sequence.AppendCallback(() => ApplyBackgroundSet(backgroundSetIndex));
        sequence.Append(flashOverlay.DOFade(0f, flashOutDuration));
    }

    /// <summary>
    /// 固定背景とParallax背景を指定セットへ即時反映します。
    /// </summary>
    private void ApplyBackgroundSet(int backgroundSetIndex)
    {
        BackgroundSet targetSet = backgroundSets[backgroundSetIndex];
        fixedBackgroundCurrent.sprite = targetSet.FixedBackgroundSprite;
        fixedBackgroundCurrent.color = Color.white;
        fixedBackgroundNext.color = new Color(1f, 1f, 1f, 0f);
        parallaxController.ApplySet(targetSet.ParallaxSetIndex);
        CurrentBackgroundSetIndex = backgroundSetIndex;
    }

    /// <summary>
    /// クロスフェード完了後、Nextの画像をCurrentへ引き継いで次回に備えます。
    /// </summary>
    private void CompleteFixedBackgroundSwap(int backgroundSetIndex)
    {
        fixedBackgroundCurrent.sprite = fixedBackgroundNext.sprite;
        fixedBackgroundCurrent.color = Color.white;
        fixedBackgroundNext.color = new Color(1f, 1f, 1f, 0f);
        CurrentBackgroundSetIndex = backgroundSetIndex;
    }

    /// <summary>
    /// タイトル用画像を消し、ゲーム背景を表示状態にします。
    /// </summary>
    private void TransitionFromTitleImmediateWithoutKilling()
    {
        titleReplicaBackground.color = new Color(1f, 1f, 1f, 0f);
        fixedBackgroundCurrent.color = Color.white;
        parallaxBackgroundCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// フラッシュ画像へ色を設定し、透明な開始状態へ戻します。
    /// </summary>
    private void PrepareFlash(Color flashColor)
    {
        flashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }

    /// <summary>
    /// このControllerが開始した途中の切り替え演出を停止します。
    /// </summary>
    private void KillTransitionTweens()
    {
        DOTween.Kill(this);
        fixedBackgroundCurrent.DOKill();
        fixedBackgroundNext.DOKill();
        titleReplicaBackground.DOKill();
        parallaxBackgroundCanvasGroup.DOKill();
        flashOverlay.DOKill();
    }
}

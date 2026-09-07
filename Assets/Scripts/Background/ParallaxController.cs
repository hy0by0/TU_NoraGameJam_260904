using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステージ進行距離から、多重スクロール背景の位置と表示セットを管理するクラスです。
/// </summary>
public class ParallaxController : MonoBehaviour
{
    /// <summary>
    /// 1レイヤー分の移動量と、Hierarchyへ事前配置したループ画像をまとめた設定です。
    /// </summary>
    [Serializable]
    public class LayerSettings
    {
        [SerializeField, InspectorName("レイヤー名")] private string layerName = "背景レイヤー";
        [SerializeField, InspectorName("移動させるRoot")] private RectTransform layerRoot;
        [SerializeField, InspectorName("事前配置したループ画像")] private Image[] loopImages = Array.Empty<Image>();
        [SerializeField, InspectorName("スクロール倍率")] private float scrollMultiplier = 1f;
        [SerializeField, Min(0.01f), InspectorName("ループ画像の幅")] private float loopImageWidth = 1920f;
        [SerializeField, InspectorName("移動方向")] private Vector2 moveDirection = Vector2.left;
        [SerializeField, InspectorName("Rootの初期位置")] private Vector2 initialPosition;

        public Image[] LoopImages => loopImages;

        /// <summary>
        /// 累積加算を使わず、進行距離を画像幅で割った余りからRoot位置を決定します。
        /// </summary>
        public void UpdatePosition(float progressDistance)
        {
            Vector2 normalizedDirection = moveDirection.normalized;
            float loopOffset = Mathf.Repeat(progressDistance * scrollMultiplier, loopImageWidth);
            layerRoot.anchoredPosition = initialPosition + normalizedDirection * loopOffset;
        }

        /// <summary>
        /// このレイヤーに事前配置された全画像へ同じSpriteを設定します。
        /// </summary>
        public void ApplySprite(Sprite sprite)
        {
            for (int index = 0; index < loopImages.Length; index++)
            {
                loopImages[index].sprite = sprite;
            }
        }
    }

    /// <summary>
    /// Far・Middle・Nearの3レイヤーに適用するSpriteセットです。
    /// </summary>
    [Serializable]
    public class ParallaxBackgroundSet
    {
        [SerializeField, InspectorName("管理用セット名")] private string setName = "背景セット";
        [SerializeField, InspectorName("Far Sprite")] private Sprite farSprite;
        [SerializeField, InspectorName("Middle Sprite")] private Sprite middleSprite;
        [SerializeField, InspectorName("Near Sprite")] private Sprite nearSprite;

        public Sprite FarSprite => farSprite;
        public Sprite MiddleSprite => middleSprite;
        public Sprite NearSprite => nearSprite;
    }

    [Header("参照")]
    [SerializeField] private StageProgressController stageProgressController;

    [Header("スクロールレイヤー")]
    [SerializeField] private LayerSettings farLayer = new LayerSettings();
    [SerializeField] private LayerSettings middleLayer = new LayerSettings();
    [SerializeField] private LayerSettings nearLayer = new LayerSettings();

    [Header("切り替え可能な背景セット")]
    [SerializeField] private List<ParallaxBackgroundSet> backgroundSets = new List<ParallaxBackgroundSet>();
    [SerializeField, Min(0)] private int initialSetIndex;

    public int CurrentSetIndex { get; private set; }

    /// <summary>
    /// Inspectorで指定した初期セットと初期位置を反映します。
    /// </summary>
    private void Awake()
    {
        ApplySet(initialSetIndex);
        UpdateLayerPositions();
    }

    /// <summary>
    /// 毎フレーム、StageProgressの絶対値を基準に各レイヤー位置を再計算します。
    /// </summary>
    private void LateUpdate()
    {
        UpdateLayerPositions();
    }

    /// <summary>
    /// UnityEventから指定番号のParallax背景セットへ即時切り替えます。
    /// </summary>
    public void ApplySet(int setIndex)
    {
        ParallaxBackgroundSet backgroundSet = backgroundSets[setIndex];
        farLayer.ApplySprite(backgroundSet.FarSprite);
        middleLayer.ApplySprite(backgroundSet.MiddleSprite);
        nearLayer.ApplySprite(backgroundSet.NearSprite);
        CurrentSetIndex = setIndex;
    }

    /// <summary>
    /// 現在の進行距離から全レイヤーの表示位置を更新します。
    /// </summary>
    private void UpdateLayerPositions()
    {
        float progressDistance = stageProgressController.ProgressDistance;
        farLayer.UpdatePosition(progressDistance);
        middleLayer.UpdatePosition(progressDistance);
        nearLayer.UpdatePosition(progressDistance);
    }
}

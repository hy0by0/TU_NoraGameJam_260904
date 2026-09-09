using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステージ進行距離から、多重スクロール背景の位置と表示セットを管理するクラスです。
/// </summary>
[DefaultExecutionOrder(-750)]
public class ParallaxController : MonoBehaviour
{
    /// <summary>指定拍以降の速度を、各レイヤーの通常速度に対する倍率で指定します。</summary>
    [Serializable]
    public class SpeedCue
    {
        [SerializeField, InspectorName("変更タイミング")] private BeatTiming timing = new BeatTiming();
        [SerializeField, InspectorName("速度倍率（X遠景・Y中景・Z近景）")] private Vector3 multipliers = Vector3.one;
        public BeatTiming Timing => timing;
        public Vector3 Multipliers => multipliers;
    }

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
    [SerializeField] private SongDefinition songDefinition;

    [Header("曲中の背景速度変更（1=通常、0=停止、2=倍速）")]
    [SerializeField, InspectorName("初期速度倍率（X遠景・Y中景・Z近景）")] private Vector3 initialSpeedMultipliers = Vector3.one;
    [SerializeField, InspectorName("速度変更一覧")] private List<SpeedCue> speedCues = new List<SpeedCue>();
    private readonly List<SpeedCue> orderedSpeedCues = new List<SpeedCue>();

    [Header("スクロールレイヤー")]
    [SerializeField] private LayerSettings farLayer = new LayerSettings();
    [SerializeField] private LayerSettings middleLayer = new LayerSettings();
    [SerializeField] private LayerSettings nearLayer = new LayerSettings();

    [Header("切り替え可能な背景セット")]
    [SerializeField] private List<ParallaxBackgroundSet> backgroundSets = new List<ParallaxBackgroundSet>();
    [SerializeField, Min(0)] private int initialSetIndex;

    private bool isScrollingEnabled = true;

    public int CurrentSetIndex { get; private set; }
    public bool IsScrollingEnabled => isScrollingEnabled;

    /// <summary>
    /// Inspectorで指定した初期セットと初期位置を反映します。
    /// </summary>
    private void Awake()
    {
        orderedSpeedCues.Clear();
        orderedSpeedCues.AddRange(speedCues);
        // 同時刻の設定はInspectorで後に登録したものを優先します。
        for (int i = 1; i < orderedSpeedCues.Count; i++)
        {
            SpeedCue cue = orderedSpeedCues[i];
            int j = i - 1;
            while (j >= 0 && orderedSpeedCues[j].Timing.ToBeatPosition(songDefinition) > cue.Timing.ToBeatPosition(songDefinition))
            {
                orderedSpeedCues[j + 1] = orderedSpeedCues[j];
                j--;
            }
            orderedSpeedCues[j + 1] = cue;
        }
        ApplySet(initialSetIndex);
        UpdateLayerPositions();
    }

    /// <summary>
    /// 毎フレーム、StageProgressの絶対値を基準に各レイヤー位置を再計算します。
    /// </summary>
    private void LateUpdate()
    {
        if (!isScrollingEnabled)
        {
            return;
        }

        UpdateLayerPositions();
    }

    /// <summary>
    /// 終了演出中に背景位置を固定するため、Parallax位置更新を切り替えます。
    /// </summary>
    public void SetScrollingEnabled(bool isEnabled)
    {
        isScrollingEnabled = isEnabled;

        if (isEnabled)
        {
            UpdateLayerPositions();
        }
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
        Vector3 distances = EvaluateScrollDistances(progressDistance);
        farLayer.UpdatePosition(distances.x);
        middleLayer.UpdatePosition(distances.y);
        nearLayer.UpdatePosition(distances.z);
    }

    /// <summary>速度区間ごとの距離を合計し、速度変更・一時停止・巻き戻しでも位置を連続させます。</summary>
    public Vector3 EvaluateScrollDistances(float progressDistance)
    {
        float end = Mathf.Max(0f, progressDistance);
        float previous = 0f;
        Vector3 distance = Vector3.zero;
        Vector3 speed = initialSpeedMultipliers;
        foreach (SpeedCue cue in orderedSpeedCues)
        {
            float changeDistance = Mathf.Max(0f, (float)(cue.Timing.ToBeatPosition(songDefinition)
                - stageProgressController.ScrollStartBeatPosition) * stageProgressController.DistancePerBeat);
            if (changeDistance > end) break;
            distance += speed * (changeDistance - previous);
            previous = changeDistance;
            speed = cue.Multipliers;
        }
        return distance + speed * (end - previous);
    }
}

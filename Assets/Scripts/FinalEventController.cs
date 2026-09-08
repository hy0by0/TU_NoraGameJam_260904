using UnityEngine;
using UnityEngine.UI;

/// <summary>最終アイテム取得後の移動・画像アニメーション・自動ビーム・UGUI演出を拍同期で進めます。</summary>
public class FinalEventController : MonoBehaviour
{
    [Header("進行設定と参照")]
    [SerializeField] private FinalEventDefinition timeline;
    [SerializeField] private GameMusicController musicController;
    [SerializeField] private StageProgressController stageProgressController;
    [SerializeField] private CameraFollowTarget cameraFollowTarget;
    [SerializeField] private GameObject enemiesRoot;
    [SerializeField] private GameObject itemsRoot;
    [SerializeField] private GameObject powerUpsRoot;
    [SerializeField] private GameObject gameplayUiRoot;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Transform playerVisual;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Camera gameplayCamera;

    [Header("ボス")]
    [SerializeField] private Transform bossRoot;
    [SerializeField] private SpriteRenderer bossRenderer;

    [Header("最終ビーム")]
    [SerializeField] private Transform finalBeamRoot;
    [SerializeField] private SpriteRenderer finalBeamRenderer;
    [SerializeField] private Transform chargeFlash;
    [SerializeField] private SpriteRenderer chargeFlashRenderer;
    [SerializeField] private Transform impactFlash;
    [SerializeField] private SpriteRenderer impactFlashRenderer;

    [Header("終盤演出UGUI")]
    [SerializeField] private Canvas eventCanvas;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image eventStillImage;
    [SerializeField] private Image endingStillImage;
    [SerializeField] private Text eventText;

    private float lastKnownRelativeBeat;
    private float fallbackElapsedTime;
    private Vector3 playerStartPosition;
    private Vector3 playerTargetPosition;
    private Vector3 bossStartPosition;
    private Vector3 bossTargetPosition;
    private Vector3 playerBaseScale;
    private Vector3 bossBaseScale;
    private bool isRunning;
    private bool hasFired;
    private bool isShowingEnding;

    public bool IsRunning => isRunning;
    public bool HasFired => hasFired;
    public bool IsShowingEnding => isShowingEnding;

    /// <summary>シーン配置済みの演出物を非表示へ初期化します。</summary>
    private void Awake()
    {
        bossRoot.gameObject.SetActive(false);
        finalBeamRoot.gameObject.SetActive(false);
        chargeFlash.gameObject.SetActive(false);
        impactFlash.gameObject.SetActive(false);
        eventCanvas.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false);
        eventStillImage.gameObject.SetActive(false);
        endingStillImage.gameObject.SetActive(false);
        eventText.gameObject.SetActive(false);
        finalBeamRenderer.drawMode = SpriteDrawMode.Sliced;
        finalBeamRoot.localScale = Vector3.one;
        playerBaseScale = playerVisual.localScale;
        bossBaseScale = bossRoot.localScale;
    }

    /// <summary>Finale形態への切り替えから一度だけ最終イベントを開始します。</summary>
    public void BeginEvent()
    {
        if (isRunning) return;
        isRunning = true;
        hasFired = false;
        isShowingEnding = false;
        AudioManager.Instance.StopSE();
        playerController.BeginFinalEventState();
        stageProgressController.FreezeProgress();
        cameraFollowTarget.enabled = false;
        enemiesRoot.SetActive(false);
        itemsRoot.SetActive(false);
        powerUpsRoot.SetActive(false);
        playerAnimator.enabled = false;
        lastKnownRelativeBeat = Mathf.Max(0f, musicController.CurrentBeatFloat - timeline.ItemCollectBeat);
        fallbackElapsedTime = 0f;
        playerStartPosition = playerRigidbody.position;
        playerTargetPosition = ViewportToWorld(timeline.PlayerTargetViewport);
        bossStartPosition = ViewportToWorld(timeline.BossStartViewport);
        bossTargetPosition = ViewportToWorld(timeline.BossTargetViewport);
        playerVisual.localScale = playerBaseScale;
        bossRoot.position = bossStartPosition;
        bossRoot.localScale = bossBaseScale;
        bossRoot.gameObject.SetActive(true);
        eventCanvas.gameObject.SetActive(true);
        EvaluateEvent(lastKnownRelativeBeat);
    }

    /// <summary>曲終了またはゲームオーバー時に最終演出の更新と専用表示を停止します。</summary>
    public void StopForGameFlow()
    {
        isRunning = false;
        finalBeamRoot.gameObject.SetActive(false);
        chargeFlash.gameObject.SetActive(false);
        impactFlash.gameObject.SetActive(false);
        eventCanvas.gameObject.SetActive(false);
    }

    /// <summary>BGM再生中は曲の絶対拍、曲終了後は実時間を補助に使って演出を最後まで進めます。</summary>
    private void Update()
    {
        if (!isRunning) return;
        float relativeBeat = musicController.CurrentBeatFloat - timeline.ItemCollectBeat;
        if (relativeBeat > lastKnownRelativeBeat)
        {
            lastKnownRelativeBeat = relativeBeat;
            fallbackElapsedTime = 0f;
        }
        else
        {
            fallbackElapsedTime += Time.unscaledDeltaTime;
        }
        EvaluateEvent(lastKnownRelativeBeat + fallbackElapsedTime / musicController.SecondsPerBeat);
    }

    /// <summary>アイテム取得を0拍とした任意の相対拍から、すべての表示と位置を決定します。</summary>
    public void EvaluateEvent(float relativeBeat)
    {
        float playerMoveProgress = SegmentProgress(relativeBeat, timeline.PlayerMoveStartBeat, timeline.PlayerMoveDurationBeats);
        float bossMoveProgress = SegmentProgress(relativeBeat, timeline.BossMoveStartBeat, timeline.BossMoveDurationBeats);
        SetPlayerPosition(Vector3.LerpUnclamped(playerStartPosition, playerTargetPosition, timeline.PlayerMoveCurve.Evaluate(playerMoveProgress)));
        bossRoot.position = Vector3.LerpUnclamped(bossStartPosition, bossTargetPosition, timeline.BossMoveCurve.Evaluate(bossMoveProgress));

        Sprite[] playerFrames = relativeBeat >= timeline.PlayerAttackSwitchBeat ? timeline.PlayerAttackFrames : timeline.PlayerMoveFrames;
        float playerAnimationStart = relativeBeat >= timeline.PlayerAttackSwitchBeat ? timeline.PlayerAttackSwitchBeat : timeline.PlayerMoveStartBeat;
        SampleSprite(playerRenderer, playerFrames, relativeBeat - playerAnimationStart, timeline.PlayerFrameDurationBeats);

        Sprite[] bossFrames = relativeBeat >= timeline.BossDamageSwitchBeat ? timeline.BossDamageFrames : timeline.BossMoveFrames;
        float bossAnimationStart = relativeBeat >= timeline.BossDamageSwitchBeat ? timeline.BossDamageSwitchBeat : timeline.BossMoveStartBeat;
        SampleSprite(bossRenderer, bossFrames, relativeBeat - bossAnimationStart, timeline.BossFrameDurationBeats);

        float chargeProgress = SegmentProgress(relativeBeat, timeline.ChargeStartBeat, timeline.ChargeDurationBeats);
        bool charging = IsInsideSegment(relativeBeat, timeline.ChargeStartBeat, timeline.ChargeDurationBeats);
        chargeFlash.gameObject.SetActive(charging);
        if (charging)
        {
            SetFlash(chargeFlash, chargeFlashRenderer, playerTargetPosition, timeline.FlashDiameter * Mathf.Sin(chargeProgress * Mathf.PI), 1f);
            playerVisual.localScale = playerBaseScale * (1f + 0.08f * Mathf.Sin(chargeProgress * Mathf.PI * 4f));
        }
        else
        {
            playerVisual.localScale = playerBaseScale;
        }

        bool firing = IsInsideSegment(relativeBeat, timeline.BeamStartBeat, timeline.BeamDurationBeats);
        finalBeamRoot.gameObject.SetActive(firing);
        if (firing)
        {
            hasFired = true;
            SetFinalBeam(relativeBeat - timeline.BeamStartBeat);
        }

        float impactProgress = SegmentProgress(relativeBeat, timeline.ImpactStartBeat, timeline.ImpactDurationBeats);
        bool impacting = IsInsideSegment(relativeBeat, timeline.ImpactStartBeat, timeline.ImpactDurationBeats);
        impactFlash.gameObject.SetActive(impacting);
        if (impacting)
        {
            float shake = Mathf.Sin(impactProgress * Mathf.PI * 16f) * timeline.BossShakeDistance * (1f - impactProgress);
            bossRoot.position += Vector3.right * shake;
            SetFlash(impactFlash, impactFlashRenderer, bossTargetPosition, timeline.FlashDiameter * (1.5f + impactProgress), 1f - impactProgress);
        }

        gameplayUiRoot.SetActive(relativeBeat < timeline.HideGameplayUiBeat);
        EvaluateImageLayer(backgroundImage, FinalEventImageLayer.Background, relativeBeat);
        EvaluateImageLayer(eventStillImage, FinalEventImageLayer.EventStill, relativeBeat);
        EvaluateImageLayer(endingStillImage, FinalEventImageLayer.EndingStill, relativeBeat);
        EvaluateText(relativeBeat);
        isShowingEnding = endingStillImage.gameObject.activeSelf;
    }

    /// <summary>SpriteRenderer.sizeを直接使い、Inspectorの太さをワールド単位で正確に反映します。</summary>
    private void SetFinalBeam(float elapsedBeats)
    {
        Vector3 start = playerTargetPosition + Vector3.right * timeline.BeamStartOffset;
        Vector3 end = bossTargetPosition;
        float fullLength = end.x - start.x;
        float extensionProgress = Mathf.Clamp01(elapsedBeats / timeline.BeamExtendBeats);
        float visibleLength = fullLength * Mathf.Max(0.001f, Mathf.SmoothStep(0f, 1f, extensionProgress));
        float attackProgress = Mathf.Clamp01(elapsedBeats / timeline.BeamDurationBeats);
        float thickness = Mathf.Lerp(timeline.BeamStartThickness, timeline.BeamEndThickness, attackProgress);
        finalBeamRoot.position = start + Vector3.right * (visibleLength * 0.5f);
        finalBeamRenderer.size = new Vector2(visibleLength, thickness);
    }

    /// <summary>指定レイヤーで現在有効な最後の画像キューをUGUIへ反映します。</summary>
    private void EvaluateImageLayer(Image image, FinalEventImageLayer layer, float relativeBeat)
    {
        image.gameObject.SetActive(false);
        for (int index = 0; index < timeline.ImageCues.Length; index++)
        {
            FinalEventImageCue cue = timeline.ImageCues[index];
            if (cue.layer != layer || relativeBeat < cue.startBeat || relativeBeat >= cue.endBeat) continue;
            image.gameObject.SetActive(true);
            image.sprite = cue.sprite;
            Color color = cue.color;
            color.a *= FadeProgress(relativeBeat, cue.startBeat, cue.fadeInBeats);
            image.color = color;
        }
    }

    /// <summary>現在有効な最後のテキストキューを、内容・位置・色とともにUGUIへ反映します。</summary>
    private void EvaluateText(float relativeBeat)
    {
        eventText.gameObject.SetActive(false);
        for (int index = 0; index < timeline.TextCues.Length; index++)
        {
            FinalEventTextCue cue = timeline.TextCues[index];
            if (relativeBeat < cue.startBeat || relativeBeat >= cue.endBeat) continue;
            eventText.gameObject.SetActive(true);
            eventText.text = cue.message;
            eventText.fontSize = cue.fontSize;
            eventText.rectTransform.anchoredPosition = cue.anchoredPosition;
            Color color = cue.color;
            color.a *= FadeProgress(relativeBeat, cue.startBeat, cue.fadeInBeats);
            eventText.color = color;
        }
    }

    /// <summary>複数画像を指定拍間隔で繰り返し、プレイヤーとボスの簡易アニメーションを作ります。</summary>
    private void SampleSprite(SpriteRenderer renderer, Sprite[] frames, float elapsedBeats, float frameDurationBeats)
    {
        int frameIndex = Mathf.FloorToInt(Mathf.Max(0f, elapsedBeats) / frameDurationBeats) % frames.Length;
        renderer.sprite = frames[frameIndex];
    }

    /// <summary>最終イベント中は物理位置と表示位置を同じフレームで揃えます。</summary>
    private void SetPlayerPosition(Vector3 position)
    {
        playerRigidbody.position = position;
        playerRigidbody.transform.position = position;
    }

    /// <summary>指定位置に発射光または命中光を表示します。</summary>
    private void SetFlash(Transform flash, SpriteRenderer renderer, Vector3 position, float diameter, float alpha)
    {
        flash.position = position;
        Vector3 spriteSize = renderer.sprite.bounds.size;
        flash.localScale = new Vector3(diameter / spriteSize.x, diameter / spriteSize.y, 1f);
        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }

    /// <summary>現在拍が指定区間内にあるか判定します。</summary>
    private bool IsInsideSegment(float beat, float startBeat, float durationBeats)
    {
        return beat >= startBeat && beat < startBeat + durationBeats;
    }

    /// <summary>指定区間内の進行率を0から1へ変換します。</summary>
    private float SegmentProgress(float beat, float startBeat, float durationBeats)
    {
        return Mathf.Clamp01((beat - startBeat) / durationBeats);
    }

    /// <summary>0拍フェードも指定できるよう、開始拍からの透明度を求めます。</summary>
    private float FadeProgress(float beat, float startBeat, float fadeBeats)
    {
        return fadeBeats <= 0f ? 1f : Mathf.Clamp01((beat - startBeat) / fadeBeats);
    }

    /// <summary>画面比率が変わっても同じ構図になるようViewport座標をワールド座標へ変換します。</summary>
    private Vector3 ViewportToWorld(Vector2 viewport)
    {
        Vector3 point = gameplayCamera.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, -gameplayCamera.transform.position.z));
        point.z = 0f;
        return point;
    }
}

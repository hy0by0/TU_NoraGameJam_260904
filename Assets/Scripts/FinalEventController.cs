using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>ボス登場・ビーム・白転・接近・クレジットを音楽時計に同期させます。</summary>
[DefaultExecutionOrder(200)]
public class FinalEventController : MonoBehaviour
{
    [Header("進行とゲームの参照")]
    [SerializeField] private FinalEventDefinition timeline;
    [SerializeField] private GameMusicController musicController;
    [SerializeField] private StageProgressController stageProgressController;
    [SerializeField] private CameraFollowTarget cameraFollowTarget;
    [SerializeField] private SongSequenceController songSequenceController;
    [SerializeField] private ScoreRankingSender scoreRankingSender;
    [SerializeField] private PostProcessTransitionController postProcess;
    [SerializeField] private GameObject enemiesRoot;
    [SerializeField] private GameObject itemsRoot;
    [SerializeField] private GameObject gameplayUiRoot;
    [SerializeField] private GameObject[] additionalGameplayObjects;
    [SerializeField] private GameObject[] backgroundRoots;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerEntranceController playerEntrance;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Transform playerVisual;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private UnityEngine.Rendering.Universal.UniversalAdditionalCameraData cameraPostProcessing;
    [SerializeField] private Material presentationMaterial;
    [Header("事前配置されたボス・接触位置・ビーム")]
    [SerializeField] private Transform bossRoot;
    [SerializeField] private SpriteRenderer bossRenderer;
    [SerializeField] private Transform bossVisual;
    [SerializeField] private Transform playerContact;
    [SerializeField] private Transform bossContact;
    [SerializeField] private ParticleSystem bossAura;
    [SerializeField] private Transform finalBeamRoot;
    [SerializeField] private SpriteRenderer finalBeamRenderer;
    [Header("事前配置UGUI")]
    [SerializeField] private Image solidBackdrop;
    [SerializeField] private Canvas eventCanvas;
    [SerializeField] private Image whiteOverlay;
    [SerializeField] private Image eventStillImage;
    [SerializeField] private Image endingStillImage;
    [SerializeField] private Text eventText;
    [SerializeField] private Button retryButton;
    [SerializeField] private ImageDissolveController firstStillDissolve;
    [SerializeField] private ImageDissolveController secondStillDissolve;

    private Vector3 playerStartPosition;
    private Vector3 bossStartPosition;
    private Vector3 bossFloatAtCollect;
    private bool hasBegun;
    private bool isRunning;
    private bool stopped;
    private bool auraStarted;
    public bool IsRunning => isRunning;
    public bool HasFired => hasBegun;
    public bool IsShowingEnding => endingStillImage.gameObject.activeSelf;

    /// <summary>演出オブジェクトを非表示にして、音楽開始を待ちます。</summary>
    private void Awake()
    {
        bossRoot.gameObject.SetActive(false);
        bossAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        finalBeamRoot.gameObject.SetActive(false);
        solidBackdrop.gameObject.SetActive(false);
        eventCanvas.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        finalBeamRenderer.drawMode = SpriteDrawMode.Sliced;
        finalBeamRoot.localScale = Vector3.one;
    }

    /// <summary>通常カメラの移動後に画面内のボスと演出位置を更新します。</summary>
    private void LateUpdate()
    {
        if (stopped || !musicController.HasPlaybackStarted) return;
        float beat = musicController.CurrentBeatFloat;
        if (isRunning) EvaluateAbsoluteBeat(beat);
        else EvaluateBossEntrance(beat);
    }

    /// <summary>85小節から黒いボスを出現させ、拍周期で上下に浮遊させます。</summary>
    public void EvaluateBossEntrance(float beat)
    {
        bool visible = beat >= timeline.Beat(timeline.bossEnterStart);
        bossRoot.gameObject.SetActive(visible);
        if (!visible) return;
        float progress = timeline.bossMoveCurve.Evaluate(timeline.Progress(beat, timeline.bossEnterStart, timeline.bossEnterEnd));
        bossRoot.position = Vector3.Lerp(World(timeline.bossEnterViewport), World(timeline.bossTargetViewport), progress);
        float phase = (beat - timeline.Beat(timeline.bossEnterStart)) / Mathf.Max(0.01f, timeline.bossFloatPeriodBeats);
        bossVisual.localPosition = Vector3.up * (Mathf.Sin(phase * Mathf.PI * 2f) * timeline.bossFloatAmplitude);
        bossRenderer.sprite = timeline.bossIdleSprite;
        float reveal = timeline.Progress(beat, timeline.bossRevealStart, timeline.bossRevealEnd);
        bossRenderer.color = new Color(reveal, reveal, reveal, progress);
        if (!auraStarted) { bossAura.Play(); auraStarted = true; }
    }

    /// <summary>取得時に通常処理を止め、現在位置から最終演出へ引き継ぎます。</summary>
    public void BeginEvent()
    {
        if (hasBegun || stopped) return;
        EvaluateBossEntrance(musicController.CurrentBeatFloat);
        hasBegun = true;
        isRunning = true;
        AudioManager.Instance.StopSE();
        playerController.BeginFinalEventState();
        stageProgressController.FreezeProgress();
        cameraFollowTarget.SetFollowingEnabled(false);
        songSequenceController.SetSequenceEnabled(false);
        playerEntrance.enabled = false;
        playerAnimator.enabled = false;
        playerVisual.DOKill();
        playerVisual.localRotation = Quaternion.identity;
        playerRenderer.DOKill();
        playerRenderer.color = Color.white;
        playerRenderer.sharedMaterial = presentationMaterial;
        postProcess.SetNormalImmediate();
        cameraPostProcessing.renderPostProcessing = false;
        HideGameplayObjects();
        playerStartPosition = playerRigidbody.transform.position;
        playerStartPosition.z = 0f;
        bossStartPosition = bossRoot.position;
        bossFloatAtCollect = bossVisual.localPosition;
        bossAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        eventCanvas.gameObject.SetActive(true);
        EvaluateAbsoluteBeat(musicController.CurrentBeatFloat);
    }

    /// <summary>旧デバッグ呼出しと互換の、取得時からの相対拍による評価です。</summary>
    public void EvaluateEvent(float relativeBeat) => EvaluateAbsoluteBeat(timeline.ItemCollectBeat + relativeBeat);

    /// <summary>絶対拍から表示を直接決め、フレーム落ちでも時刻を蓄積しません。</summary>
    public void EvaluateAbsoluteBeat(float beat)
    {
        float elapsed = beat - timeline.ItemCollectBeat;
        float settle = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, timeline.bossSettleBeats));
        Vector3 playerTarget = World(timeline.playerTargetViewport);
        bossRoot.gameObject.SetActive(beat < timeline.Beat(timeline.creditsStart));
        bossRoot.position = Vector3.Lerp(bossStartPosition, World(timeline.bossTargetViewport), settle);
        bossVisual.localPosition = Vector3.Lerp(bossFloatAtCollect, Vector3.zero, settle);
        bool afterWhite = beat >= timeline.Beat(timeline.firstWhitePeak);
        playerRenderer.sprite = afterWhite ? timeline.playerAfterBeamSprite : timeline.playerAttackSprite;
        bossRenderer.sprite = afterWhite ? timeline.bossDamageSprite : timeline.bossIdleSprite;
        playerRenderer.enabled = beat < timeline.Beat(timeline.creditsStart);
        // 完全な白で隠した瞬間に画像を交換し、白が引く時間と同じ時間で2人を表示します。
        float characterAlpha = afterWhite
            ? timeline.Progress(beat, timeline.firstWhitePeak, timeline.firstWhiteEnd)
            : 1f;
        playerRenderer.color = new Color(1f, 1f, 1f, characterAlpha);
        bossRenderer.color = new Color(1f, 1f, 1f, characterAlpha);
        Vector3 position = Vector3.Lerp(playerStartPosition, playerTarget,
            timeline.playerMoveCurve.Evaluate(Mathf.Clamp01(elapsed / Mathf.Max(0.01f, timeline.playerMoveDurationBeats))));
        // 画像中心ではなく、Inspectorで置いた接触点同士を時刻ぴったりに合わせます。
        if (beat >= timeline.Beat(timeline.approachStart))
        {
            Vector3 offset = playerContact.position - playerRigidbody.transform.position;
            Vector3 contactPosition = bossContact.position - offset;
            position = Vector3.Lerp(playerTarget, contactPosition,
                timeline.approachCurve.Evaluate(timeline.Progress(beat, timeline.approachStart, timeline.contactTiming)));
        }
        SetPlayerPosition(position);
        float beamGrowth = GetBeamGrowth(beat);
        finalBeamRoot.gameObject.SetActive(!afterWhite);
        if (!afterWhite) EvaluateBeam(elapsed, position, beamGrowth);
        // ビーム発射から完全な白までの共通進行率で、背面背景と前面の白転を同時に進めます。
        float firstWhiteProgress = timeline.Progress(beat, timeline.itemCollectTiming, timeline.firstWhitePeak);
        SetBackdrop(true, new Color(1f, 1f, 1f, firstWhiteProgress));
        if (afterWhite) HideBackgrounds();
        float white = beat < timeline.Beat(timeline.firstWhitePeak)
            ? firstWhiteProgress
            : 1f - timeline.Progress(beat, timeline.firstWhitePeak, timeline.firstWhiteEnd);
        // 1回目が引き切った後、接近に合わせて再び白くし、接触時に完全な白へ戻します。
        float secondStart = Mathf.Max(timeline.Beat(timeline.firstWhiteEnd), timeline.Beat(timeline.secondWhiteStart));
        white = Mathf.Max(white, Fade(beat - secondStart, timeline.Beat(timeline.contactTiming) - secondStart));
        // クレジットでは前面の白を外し、同じ白色の背面背景へ継ぎ目なく引き継ぎます。
        if (beat >= timeline.Beat(timeline.creditsStart)) white = 0f;
        whiteOverlay.color = new Color(1f, 1f, 1f, white);
        whiteOverlay.gameObject.SetActive(white > 0f);
        EvaluateCredits(beat);
        EvaluateStills(beat);
    }

    /// <summary>長さと太さを独立して拡大し、必要ならカメラ全面まで広げます。</summary>
    private void EvaluateBeam(float elapsed, Vector3 playerPosition, float growth)
    {
        Vector3 start = playerPosition + Vector3.right * timeline.beamStartOffset;
        float maxLength = timeline.beamMaximumLength;
        float maxThickness = timeline.beamMaximumThickness;
        if (timeline.coverScreenWidth)
        {
            start.x = Mathf.Lerp(start.x, World(Vector2.zero).x - 0.1f, growth);
            maxLength = Mathf.Max(maxLength, World(Vector2.one).x - start.x + 0.1f);
        }
        if (timeline.coverScreenHeight)
            maxThickness = Mathf.Max(maxThickness, 2f * Mathf.Max(Mathf.Abs(World(Vector2.zero).y - start.y), Mathf.Abs(World(Vector2.one).y - start.y)) + 0.2f);
        float length = Mathf.Lerp(0.01f, maxLength, Mathf.SmoothStep(0f, 1f, elapsed / Mathf.Max(0.01f, timeline.beamExtendBeats)));
        finalBeamRoot.position = start + Vector3.right * (length * 0.5f);
        finalBeamRenderer.size = new Vector2(length, Mathf.Lerp(timeline.beamStartThickness, maxThickness, growth));
        float alpha = Mathf.Clamp01(timeline.beamFadeCurve.Evaluate(growth));
        finalBeamRenderer.color = new Color(1f, 1f, 1f, alpha);
    }

    /// <summary>取得から拡大完了までを、Inspectorのカーブを通した0～1へ変換します。</summary>
    private float GetBeamGrowth(float beat)
    {
        float duration = Mathf.Max(0.01f, timeline.Beat(timeline.beamFullTiming) - timeline.ItemCollectBeat);
        float progress = Mathf.Clamp01((beat - timeline.ItemCollectBeat) / duration);
        return Mathf.Clamp01(timeline.beamGrowthCurve.Evaluate(progress));
    }

    /// <summary>クレジットの表示区間を評価し、確定スコアを本文へ差し込みます。</summary>
    private void EvaluateCredits(float beat)
    {
        eventText.gameObject.SetActive(false);
        if (beat < timeline.Beat(timeline.creditsStart)) return;
        scoreRankingSender.SubmitCurrentScore();
        foreach (var cue in timeline.credits)
        {
            float start = timeline.Beat(cue.start), end = timeline.Beat(cue.end);
            if (beat < start || beat >= end) continue;
            eventText.gameObject.SetActive(true);
            string message = cue.message.Replace("{score}", scoreRankingSender.FinalScore.ToString());
            if (eventText.text != message) eventText.text = message;
            eventText.fontSize = cue.fontSize;
            eventText.rectTransform.anchoredPosition = cue.anchoredPosition;
            Color color = cue.color;
            color.a *= Mathf.Min(Fade(beat - start, cue.fadeInBeats), Fade(end - beat, cue.fadeOutBeats));
            if (beat >= timeline.Beat(timeline.firstStillTiming))
                color.a *= 1f - Fade(beat - timeline.Beat(timeline.firstStillTiming), timeline.firstStillFadeBeats);
            eventText.color = color;
        }
    }

    /// <summary>白背景から1枚目へ、指定拍後に2枚目へ切り替えます。</summary>
    private void EvaluateStills(float beat)
    {
        float first = timeline.Beat(timeline.firstStillTiming);
        eventStillImage.gameObject.SetActive(beat >= first);
        endingStillImage.gameObject.SetActive(beat >= first + timeline.secondStillDelayBeats);
        eventStillImage.sprite = timeline.firstStill;
        endingStillImage.sprite = timeline.secondStill;
        ApplyStill(eventStillImage, firstStillDissolve, timeline.firstStillTransition, Fade(beat - first, timeline.firstStillFadeBeats));
        ApplyStill(endingStillImage, secondStillDissolve, timeline.secondStillTransition, Fade(beat - first - timeline.secondStillDelayBeats, timeline.secondStillFadeBeats));
    }

    /// <summary>画像の透明度またはディゾルブ進行率を設定します。</summary>
    private void ApplyStill(Image image, ImageDissolveController dissolve, FinaleImageTransition style, float progress)
    {
        dissolve.SetProgress(style == FinaleImageTransition.Dissolve ? progress : 1f);
        image.color = new Color(1f, 1f, 1f, style == FinaleImageTransition.Fade ? progress : 1f);
    }

    /// <summary>曲終了後も2枚目を残して全画面リトライを有効にします。</summary>
    public void CompleteEnding()
    {
        stopped = true;
        isRunning = false;
        HideGameplayObjects();
        HideBackgrounds();
        bossRoot.gameObject.SetActive(false);
        bossAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        finalBeamRoot.gameObject.SetActive(false);
        playerRenderer.enabled = false;
        SetBackdrop(true, Color.white);
        eventCanvas.gameObject.SetActive(true);
        whiteOverlay.gameObject.SetActive(false);
        eventText.gameObject.SetActive(false);
        eventStillImage.gameObject.SetActive(false);
        endingStillImage.gameObject.SetActive(true);
        endingStillImage.sprite = timeline.secondStill;
        secondStillDissolve.SetProgress(1f);
        endingStillImage.color = Color.white;
        retryButton.gameObject.SetActive(true);
        retryButton.interactable = true;
    }

    /// <summary>黒背景とプレイヤーのみを表示し、透明ボタンでリトライできます。</summary>
    public void ShowGameOver()
    {
        StopForGameFlow();
        HideGameplayObjects();
        HideBackgrounds();
        postProcess.SetNormalImmediate();
        cameraPostProcessing.renderPostProcessing = false;
        playerEntrance.enabled = false;
        playerAnimator.enabled = false;
        playerVisual.DOKill();
        playerRenderer.DOKill();
        playerVisual.localRotation = Quaternion.identity;
        playerRenderer.enabled = true;
        playerRenderer.color = Color.white;
        playerRenderer.sharedMaterial = presentationMaterial;
        SetPlayerPosition(World(new Vector2(0.3f, 0.5f)));
        SetBackdrop(true, Color.black);
        eventCanvas.gameObject.SetActive(true);
        whiteOverlay.gameObject.SetActive(false);
        eventStillImage.gameObject.SetActive(false);
        endingStillImage.gameObject.SetActive(false);
        eventText.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
        retryButton.interactable = true;
    }

    /// <summary>演出更新と専用表示を停止します。</summary>
    public void StopForGameFlow()
    {
        stopped = true;
        isRunning = false;
        bossRoot.gameObject.SetActive(false);
        bossAura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        finalBeamRoot.gameObject.SetActive(false);
        eventCanvas.gameObject.SetActive(false);
    }

    // 個別配置アイテムもInspectorに登録してまとめて隠します。
    private void HideGameplayObjects()
    {
        enemiesRoot.SetActive(false);
        itemsRoot.SetActive(false);
        gameplayUiRoot.SetActive(false);
        foreach (var target in additionalGameplayObjects) target.SetActive(false);
    }

    private void HideBackgrounds()
    {
        foreach (var target in backgroundRoots) target.SetActive(false);
    }

    private void SetBackdrop(bool visible, Color color)
    {
        solidBackdrop.gameObject.SetActive(visible);
        solidBackdrop.color = color;
    }

    private void SetPlayerPosition(Vector3 position)
    {
        position.z = 0f;
        playerRigidbody.position = position;
        playerRigidbody.transform.position = position;
    }

    private float Fade(float elapsed, float duration) => duration <= 0f ? (elapsed >= 0f ? 1f : 0f) : Mathf.Clamp01(elapsed / duration);

    // Viewportは左下(0,0)、右上(1,1)です。
    private Vector3 World(Vector2 viewport)
    {
        Vector3 position = gameplayCamera.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, -gameplayCamera.transform.position.z));
        position.z = 0f;
        return position;
    }
}

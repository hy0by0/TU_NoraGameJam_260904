using UnityEngine;

/// <summary>
/// MainSceneの準備から曲終了・ゲームオーバーまで、ゲーム全体の状態遷移を一元管理するクラスです。
/// </summary>
[DefaultExecutionOrder(-600)]
public class GameFlowController : MonoBehaviour
{
    public enum GameFlowState
    {
        Preparing,
        PreRoll,
        Intro,
        Playing,
        GameOver,
        Finished
    }

    [Header("ゲーム進行の参照")]
    [SerializeField] private GameMusicController musicController;
    [SerializeField] private SongSequenceController songSequenceController;
    [SerializeField] private StageProgressController stageProgressController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private FinalEventController finalEventController;
    [SerializeField] private ScoreRankingSender scoreRankingSender;

    [Header("カメラ・背景の参照")]
    [SerializeField] private GameCameraController cameraController;
    [SerializeField] private CameraFollowTarget cameraFollowTarget;
    [SerializeField] private ParallaxController parallaxController;
    [SerializeField] private BackgroundTransitionController backgroundTransitionController;

    [Header("UI・PostProcessingの参照")]
    [SerializeField] private GameObject gameplayUiRoot;
    [SerializeField] private UITransitionController hudTransition;
    [SerializeField] private UITransitionController gameOverTransition;
    [SerializeField] private UITransitionController resultTransition;
    [SerializeField] private PostProcessTransitionController postProcessTransitionController;

    [Header("ゲームオーバー演出")]
    [SerializeField, Min(0f)] private float gameOverSlowStopDuration = 1.5f;

    public GameFlowState CurrentState { get; private set; }

    /// <summary>
    /// MainSceneの全システムを初期状態へ揃え、PreRoll付きでBGMを予約します。
    /// </summary>
    private void Start()
    {
        CurrentState = GameFlowState.Preparing;
        playerController.SetGameplayEnabled(true);
        stageProgressController.ResetProgress();
        songSequenceController.ResetExecutionState();
        songSequenceController.SetSequenceEnabled(true);
        cameraFollowTarget.SetFollowingEnabled(true);
        parallaxController.SetScrollingEnabled(true);
        gameplayUiRoot.SetActive(true);
        cameraController.ShowGameplay();
        // 曲中イベントからフェードインさせるまで、HUDは非表示で待機させます。
        hudTransition.HideImmediate();
        gameOverTransition.HideImmediate();
        resultTransition.HideImmediate();
        postProcessTransitionController.SetNormalImmediate();

        musicController.ScheduleMusicWithPreRoll();
        CurrentState = GameFlowState.PreRoll;
    }

    /// <summary>
    /// 音楽状態・スクロール開始・残りライフからゲーム全体の状態を更新します。
    /// </summary>
    private void Update()
    {
        if (CurrentState == GameFlowState.GameOver || CurrentState == GameFlowState.Finished)
        {
            return;
        }

        if (playerController.currentLife <= 0)
        {
            EnterGameOver();
            return;
        }

        if (musicController.IsFinished)
        {
            EnterFinished();
            return;
        }

        if (CurrentState == GameFlowState.PreRoll && musicController.HasPlaybackStarted)
        {
            CurrentState = GameFlowState.Intro;
        }

        if (CurrentState == GameFlowState.Intro && stageProgressController.HasStartedScrolling)
        {
            CurrentState = GameFlowState.Playing;
        }
    }

    /// <summary>
    /// ライフが尽きた時点の進行を固定し、ゲームオーバー演出へ切り替えます。
    /// </summary>
    private void EnterGameOver()
    {
        CurrentState = GameFlowState.GameOver;
        StopGameplaySystems();
        gameplayUiRoot.SetActive(true);
        cameraController.ShowGameOver();
        backgroundTransitionController.TransitionToGameOverBackground();
        hudTransition.Hide();
        resultTransition.HideImmediate();
        gameOverTransition.Show();
        musicController.BeginSlowStop(gameOverSlowStopDuration);
    }

    /// <summary>
    /// 曲終了時点の進行を固定し、リザルト画面へ切り替えます。
    /// </summary>
    private void EnterFinished()
    {
        CurrentState = GameFlowState.Finished;
        StopGameplaySystems();
        scoreRankingSender.SubmitCurrentScore();
        gameplayUiRoot.SetActive(true);
        cameraController.ShowResult();
        hudTransition.Hide();
        gameOverTransition.HideImmediate();
        resultTransition.Show();
    }

    /// <summary>
    /// 終了後に進行してはいけない入力・移動・追従・背景・拍イベントをまとめて停止します。
    /// </summary>
    private void StopGameplaySystems()
    {
        finalEventController.StopForGameFlow();
        playerController.SetGameplayEnabled(false);
        stageProgressController.FreezeProgress();
        cameraFollowTarget.SetFollowingEnabled(false);
        parallaxController.SetScrollingEnabled(false);
        songSequenceController.SetSequenceEnabled(false);
    }
}

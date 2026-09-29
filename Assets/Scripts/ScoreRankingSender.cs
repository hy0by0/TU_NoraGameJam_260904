using UnityEngine;
using unityroom.Api;

/// <summary>
/// 曲終了時のプレイヤースコアをunityroomランキングへ一度だけ送信するクラスです。
/// </summary>
public class ScoreRankingSender : MonoBehaviour
{
    public enum SubmissionState { NotQueued, Queued, EditorSimulation, Succeeded, Failed, NotImproved, Unconfigured }
    [Header("ランキング設定")]
    [SerializeField, Min(1), Tooltip("unityroomのスコアボード一覧に表示されるボード番号です。")]
    private int boardNo = 1;
    [SerializeField, Min(0), Tooltip("スマホ用のunityroomボード番号を後から入力します。0の間はスマホ用スコアを送信しません。")]
    private int touchBoardNo;
    [SerializeField, Tooltip("unityroom側のスコアボードと同じ記録ルールを選択してください。")]
    private ScoreboardWriteMode writeMode = ScoreboardWriteMode.HighScoreDesc;

    [Header("スコアの参照")]
    [SerializeField] private PlayerController playerController;

    private bool hasSubmitted;
    private int activeBoardNo;
    public int FinalScore { get; private set; }
    public bool HasQueuedScore => hasSubmitted && submissionState != SubmissionState.Unconfigured;
    [SerializeField, Tooltip("実行時の通信状態です。EditorSimulationは実送信していません。")]
    private SubmissionState submissionState;
    public SubmissionState State => submissionState;

    // 導入済みクライアントの結果ログを監視します。認証キーは扱いません。
    private void OnEnable() => Application.logMessageReceived += ObserveClientLog;
    private void OnDisable() => Application.logMessageReceived -= ObserveClientLog;

    /// <summary>予約と実際の送信結果を区別し、Inspectorで確認できるようにします。</summary>
    private void ObserveClientLog(string message, string stackTrace, LogType type)
    {
        if (!message.StartsWith("[unityroom]") || !message.Contains($"BoardNo={activeBoardNo} ")) return;
        if (message.Contains("スコア送信予約")) submissionState = SubmissionState.Queued;
        else if (message.Contains("アップロードすると")) submissionState = SubmissionState.EditorSimulation;
        else if (message.Contains("スコア送信成功")) submissionState = SubmissionState.Succeeded;
        else if (message.Contains("スコア送信失敗")) submissionState = SubmissionState.Failed;
        else if (message.Contains("未更新")) submissionState = SubmissionState.NotImproved;
    }

    /// <summary>
    /// 現在のスコアをランキングへ送信します。同じプレイ中は重複送信しません。
    /// </summary>
    public void SubmitCurrentScore()
    {
        if (hasSubmitted)
        {
            return;
        }

        FinalScore = Mathf.Max(0, playerController.currentScore);
        activeBoardNo = PlayInputMode.IsTouch ? touchBoardNo : boardNo;
        if (activeBoardNo == 0)
        {
            submissionState = SubmissionState.Unconfigured;
            hasSubmitted = true;
            Debug.LogWarning("[Ranking] スマホ用ボード番号が未設定のため、スコアは送信していません。");
            return;
        }

        // SendScoreは送信予約です。実際の成功・失敗と再試行はクライアントのログで確認します。
        UnityroomApiClient.Instance.SendScore(activeBoardNo, FinalScore, writeMode);
        hasSubmitted = true;
        Debug.Log($"[Ranking] 送信予約 Board={activeBoardNo} Score={FinalScore} Mode={writeMode}");
    }
}

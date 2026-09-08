using UnityEngine;
using unityroom.Api;

/// <summary>
/// 曲終了時のプレイヤースコアをunityroomランキングへ一度だけ送信するクラスです。
/// </summary>
public class ScoreRankingSender : MonoBehaviour
{
    [Header("ランキング設定")]
    [SerializeField, Min(1), Tooltip("unityroomのスコアボード一覧に表示されるボード番号です。")]
    private int boardNo = 1;
    [SerializeField, Tooltip("unityroom側のスコアボードと同じ記録ルールを選択してください。")]
    private ScoreboardWriteMode writeMode = ScoreboardWriteMode.HighScoreDesc;

    [Header("スコアの参照")]
    [SerializeField] private PlayerController playerController;

    private bool hasSubmitted;

    /// <summary>
    /// 現在のスコアをランキングへ送信します。同じプレイ中は重複送信しません。
    /// </summary>
    public void SubmitCurrentScore()
    {
        if (hasSubmitted)
        {
            return;
        }

        hasSubmitted = true;
        UnityroomApiClient.Instance.SendScore(boardNo, playerController.currentScore, writeMode);
    }
}

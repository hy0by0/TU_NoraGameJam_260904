using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの現在スコアを6桁のテキストとして表示するクラスです。
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("参照先")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Text scoreText;

    private int displayedScore = -1;

    /// <summary>
    /// ゲーム開始時に現在のスコア表示を反映します。
    /// </summary>
    private void Start()
    {
        RefreshScore();
    }

    /// <summary>
    /// スコアが変化したときだけ表示を更新します。
    /// </summary>
    private void Update()
    {
        if (displayedScore != playerController.currentScore)
        {
            RefreshScore();
        }
    }

    /// <summary>
    /// プレイヤーの現在スコアを6桁のゼロ埋め表示へ変換します。
    /// </summary>
    private void RefreshScore()
    {
        displayedScore = playerController.currentScore;
        scoreText.text = displayedScore.ToString("D6");
    }
}

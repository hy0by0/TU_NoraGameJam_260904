using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SongSequenceControllerのテストイベントから、ログ出力とUGUI表示を操作する確認用クラスです。
/// </summary>
public class SongSequenceDebugTarget : MonoBehaviour
{
    [Header("テスト表示の参照")]
    [SerializeField] private GameObject displayRoot;
    [SerializeField] private Text displayText;

    /// <summary>
    /// 同じ拍に登録した1番目のイベントが実行されたことをConsoleへ出力します。
    /// </summary>
    public void LogFirstSameTimingEvent()
    {
        Debug.Log("[SongSequence] 同拍イベント 1（実行順 0）を実行しました。");
    }

    /// <summary>
    /// 同じ拍に登録した2番目のイベントが実行されたことをConsoleへ出力します。
    /// </summary>
    public void LogSecondSameTimingEvent()
    {
        Debug.Log("[SongSequence] 同拍イベント 2（実行順 10）を実行しました。");
    }

    /// <summary>
    /// 動作確認用UGUIを表示します。
    /// </summary>
    public void ShowTestDisplay()
    {
        displayText.text = "SongSequence Event!\n1小節目・1拍目で表示";
        displayRoot.SetActive(true);
        Debug.Log("[SongSequence] テストUGUIを表示しました。");
    }

    /// <summary>
    /// 動作確認用UGUIを非表示にします。
    /// </summary>
    public void HideTestDisplay()
    {
        displayRoot.SetActive(false);
        Debug.Log("[SongSequence] テストUGUIを非表示にしました。");
    }
}

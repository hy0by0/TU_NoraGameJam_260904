using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameMusicControllerの状態と拍情報を、開発確認用UGUIへ表示するクラスです。
/// </summary>
public class GameMusicDebugView : MonoBehaviour
{
    [Header("表示設定")]
    [SerializeField] private bool showDebugDisplay = true;
    [SerializeField] private GameObject debugRoot;

    [Header("参照")]
    [SerializeField] private GameMusicController musicController;
    [SerializeField] private Text debugText;

    /// <summary>
    /// Inspectorの表示設定をUGUIへ反映します。
    /// </summary>
    private void Awake()
    {
        debugRoot.SetActive(showDebugDisplay);
    }

    /// <summary>
    /// 現在状態、PreRoll残り拍、小節、拍、拍内分割を毎フレーム表示します。
    /// </summary>
    private void Update()
    {
        debugRoot.SetActive(showDebugDisplay);

        if (!showDebugDisplay)
        {
            return;
        }

        debugText.text =
            $"State: {musicController.State}\n" +
            $"PreRoll Beats: {musicController.PreRollRemainingBeats:F2}\n" +
            $"Bar: {musicController.CurrentBar}\n" +
            $"Beat: {musicController.CurrentBeat}\n" +
            $"Subdivision: {musicController.CurrentSubdivision}";
    }
}

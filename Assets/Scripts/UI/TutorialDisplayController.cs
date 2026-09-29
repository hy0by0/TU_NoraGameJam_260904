using UnityEngine;

/// <summary>
/// タイトル画面で選んだ設定に応じて、MainSceneの操作ガイド表示イベントを実行するクラスです。
/// </summary>
public class TutorialDisplayController : MonoBehaviour
{
    [Header("既存の表示演出")]
    [SerializeField] private UITransitionController tutorialTransition;

    [SerializeField, Tooltip("このガイドをタッチ操作のときに表示する場合に有効にします。")]
    private bool forTouchMode;

    /// <summary>
    /// 表示設定が有効な場合だけ、既存のフェード表示を再生します。
    /// </summary>
    public void ShowIfEnabled()
    {

        if (TutorialDisplaySettings.IsEnabled && PlayInputMode.IsTouch == forTouchMode)
        {
            tutorialTransition.ShowFade();
        }
    }
}

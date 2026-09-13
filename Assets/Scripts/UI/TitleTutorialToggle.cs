using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトル画面のボタンで操作ガイドの表示を切り替え、有効なときだけ文字を見せるクラスです。
/// </summary>
public class TitleTutorialToggle : MonoBehaviour
{
    [Header("タイトル画面の表示")]
    [SerializeField] private Text stateText;

    /// <summary>
    /// タイトル画面を開いたら、何も押していない初期状態を「表示」にします。
    /// </summary>
    private void Awake()
    {
        TutorialDisplaySettings.ResetToDefault();
    }

    /// <summary>
    /// 現在の設定をボタン内の文字へ反映します。
    /// </summary>
    private void OnEnable()
    {
        RefreshLabel();
    }

    /// <summary>
    /// ボタンを押すたびに設定を切り替えます。
    /// </summary>
    public void Toggle()
    {
        TutorialDisplaySettings.SetEnabled(!TutorialDisplaySettings.IsEnabled);
        RefreshLabel();
    }

    /// <summary>
    /// 操作ガイドが有効な間だけ、ボタン内の文字を表示します。
    /// </summary>
    private void RefreshLabel()
    {
        stateText.enabled = TutorialDisplaySettings.IsEnabled;
    }
}

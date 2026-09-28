/// <summary>
/// タイトル画面で選んだ操作ガイドの表示設定を、現在のプレイ中だけシーン間で共有するクラスです。
/// </summary>
public static class TutorialDisplaySettings
{
    private static bool isEnabled = true;

    // タイトルを経由せずMainSceneを開いた場合も、操作ガイドを表示します。
    public static bool IsEnabled => isEnabled;

    /// <summary>
    /// タイトル画面を開いたときの初期値を「表示」に戻します。
    /// </summary>
    public static void ResetToDefault()
    {
        isEnabled = true;
    }

    /// <summary>
    /// 現在のプレイで使う表示設定を変更します。
    /// </summary>
    public static void SetEnabled(bool isEnabled)
    {
        TutorialDisplaySettings.isEnabled = isEnabled;
    }
}

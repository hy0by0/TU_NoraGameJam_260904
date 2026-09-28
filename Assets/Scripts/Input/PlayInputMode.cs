/// <summary>
/// 最初の画面で選ばれた操作方法を、シーンをまたいで共有するクラスです。
/// </summary>
public static class PlayInputMode
{
    public enum Mode { Desktop, Touch }

    public static Mode Current { get; private set; } = Mode.Desktop;
    public static bool IsTouch => Current == Mode.Touch;

    /// <summary>最初の入力で操作方法を確定します。</summary>
    public static void Select(Mode mode)
    {
        Current = mode;
    }

    /// <summary>新しくゲームを起動した際、Editorで前回の選択が残らないようにします。</summary>
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnLaunch()
    {
        Current = Mode.Desktop;
    }
}

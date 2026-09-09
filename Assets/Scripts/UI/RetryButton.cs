using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// GameOverまたはResultの全画面UGUI Buttonから、Inspectorで選んだシーンへ遷移するクラスです。
/// </summary>
[RequireComponent(typeof(Button))]
public class RetryButton : MonoBehaviour
{
    /// <summary>
    /// Build Settingsへ登録済みの遷移先候補です。
    /// </summary>
    public enum SceneDestination
    {
        Title,
        MainScene
    }

    [Header("画面遷移設定")]
    [SerializeField, InspectorName("遷移先シーン")] private SceneDestination destinationScene = SceneDestination.MainScene;

    private bool isLoading;

    public SceneDestination DestinationScene => destinationScene;

    /// <summary>
    /// Buttonのクリックを受け取り、重複読込を防ぎながら選択されたシーンを読み込みます。
    /// </summary>
    public void LoadSelectedScene()
    {
        if (isLoading)
        {
            return;
        }

        isLoading = true;
        GetComponent<Button>().interactable = false;
        SceneManager.LoadSceneAsync(destinationScene.ToString());
    }

    /// <summary>
    /// 既存のButtonイベント参照を維持するため、選択されたシーンへ遷移します。
    /// </summary>
    public void RetryMainScene()
    {
        LoadSelectedScene();
    }
}

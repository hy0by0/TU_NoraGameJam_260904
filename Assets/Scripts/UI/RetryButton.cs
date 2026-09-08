using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// GameOverまたはResultの全画面UGUI ButtonからMainSceneを再読み込みするクラスです。
/// </summary>
[RequireComponent(typeof(Button))]
public class RetryButton : MonoBehaviour
{
    [Header("再読み込み設定")]
    [SerializeField] private string mainSceneName = "MainScene";

    private bool isLoading;

    /// <summary>
    /// Buttonのクリックを受け取り、重複読込を防ぎながらMainSceneを再読み込みします。
    /// </summary>
    public void RetryMainScene()
    {
        if (isLoading)
        {
            return;
        }

        isLoading = true;
        GetComponent<Button>().interactable = false;
        SceneManager.LoadSceneAsync(mainSceneName);
    }
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// タイトルメニューの退出演出とMainSceneへの遷移を管理するクラスです。
/// </summary>
public class TitleFlowController : MonoBehaviour
{
    [Header("タイトルUI")]
    [SerializeField] private RectTransform titleMenuRoot;
    [SerializeField] private CanvasGroup titleMenuCanvasGroup;
    [SerializeField] private Button startButton;

    [Header("タイトルBGM")]
    [SerializeField] private AudioSource titleBgmSource;

    [Header("退出演出")]
    [SerializeField, Min(0f)] private float uiExitDuration = 0.8f;
    [SerializeField] private float uiExitDistance = -2000f;
    [SerializeField, Min(0f)] private float bgmFadeDuration = 0.8f;

    [Header("遷移先")]
    [SerializeField] private string mainSceneName = "MainScene";

    private bool isTransitioning;

    /// <summary>
    /// タイトルメニューを操作可能な初期状態へ整えます。
    /// </summary>
    private void Awake()
    {
        isTransitioning = false;
        startButton.interactable = true;
        titleMenuCanvasGroup.interactable = true;
        titleMenuCanvasGroup.blocksRaycasts = true;
        titleMenuCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// タイトルUIとBGMの退出演出を開始し、完了後にMainSceneを非同期で読み込みます。
    /// </summary>
    public void StartGame()
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        startButton.interactable = false;
        titleMenuCanvasGroup.interactable = false;
        titleMenuCanvasGroup.blocksRaycasts = false;

        Vector2 exitPosition = titleMenuRoot.anchoredPosition
            + new Vector2(uiExitDistance, 0f);

        Sequence exitSequence = DOTween.Sequence();
        exitSequence.Join(
            titleMenuRoot.DOAnchorPos(exitPosition, uiExitDuration)
                .SetEase(Ease.InCubic)
        );
        exitSequence.Join(
            titleBgmSource.DOFade(0f, bgmFadeDuration)
                .SetEase(Ease.Linear)
        );
        exitSequence.OnComplete(LoadMainScene);
    }

    /// <summary>
    /// 退出済みのタイトルUIを非表示にし、MainSceneを非同期で読み込みます。
    /// </summary>
    private void LoadMainScene()
    {
        titleMenuCanvasGroup.alpha = 0f;
        titleMenuRoot.gameObject.SetActive(false);
        SceneManager.LoadSceneAsync(mainSceneName);
    }
}

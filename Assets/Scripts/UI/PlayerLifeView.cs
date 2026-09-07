using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの現在ライフに合わせて、ライフアイコンの画像を切り替えるクラスです。
/// </summary>
public class PlayerLifeView : MonoBehaviour
{
    [Header("参照先")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Image[] lifeIcons;

    [Header("ライフ画像")]
    [SerializeField] private Sprite lifeMaxSprite;
    [SerializeField] private Sprite lifeLowSprite;

    private int displayedCurrentLife = -1;
    private int displayedMaxLife = -1;

    /// <summary>
    /// ゲーム開始時に現在のライフ表示を反映します。
    /// </summary>
    private void Start()
    {
        RefreshLifeIcons();
    }

    /// <summary>
    /// ライフの値が変化したときだけ表示を更新します。
    /// </summary>
    private void Update()
    {
        if (displayedCurrentLife != playerController.currentLife || displayedMaxLife != playerController.maxLife)
        {
            RefreshLifeIcons();
        }
    }

    /// <summary>
    /// 現在ライフが残っている位置は満タン画像、失われた位置は空画像へ切り替えます。
    /// </summary>
    private void RefreshLifeIcons()
    {
        int visibleIconCount = Mathf.Clamp(playerController.maxLife, 0, lifeIcons.Length);
        int currentLife = Mathf.Clamp(playerController.currentLife, 0, visibleIconCount);

        for (int index = 0; index < lifeIcons.Length; index++)
        {
            bool isWithinMaxLife = index < visibleIconCount;
            lifeIcons[index].gameObject.SetActive(isWithinMaxLife);

            if (isWithinMaxLife)
            {
                lifeIcons[index].sprite = index < currentLife ? lifeMaxSprite : lifeLowSprite;
            }
        }

        displayedCurrentLife = playerController.currentLife;
        displayedMaxLife = playerController.maxLife;
    }
}

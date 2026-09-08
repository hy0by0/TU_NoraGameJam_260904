using UnityEngine;

// アイテムの取得を一度だけ処理し、加点・取得音・指定形態への変更を行います。
public class Item : MonoBehaviour
{
    // 保存済みシーンとPrefabの番号を維持します。
    public enum ItemType { Normal = 0, Heart = 1, Wide = 2, Speed = 3, FinalSword = 4 }

    public ItemType itemType;
    public int scoreValue = 100;
    [SerializeField] private AudioClip hitSE;
    [Header("強化アイテム（Wide / Speed / FinalSword）の変更先")]
    [SerializeField] private PlayerFormDefinition targetForm;
    private bool isCollected;

    // 子Colliderで触れた場合も親のPlayerControllerへ取得を通知します。
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        Collect(collision.GetComponentInParent<PlayerController>());
    }

    // 複数のColliderで接触しても、効果と音と加点を一度だけ実行します。
    public void Collect(PlayerController player)
    {
        if (isCollected) return;
        isCollected = true;
        player.currentScore += scoreValue;
        if (itemType == ItemType.Wide || itemType == ItemType.Speed || itemType == ItemType.FinalSword)
            player.ChangeForm(targetForm);
        // 最終イベントでは、取得以降にSEを鳴らさない仕様です。
        if (itemType != ItemType.FinalSword)
            AudioManager.Instance.PlaySE(hitSE);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}

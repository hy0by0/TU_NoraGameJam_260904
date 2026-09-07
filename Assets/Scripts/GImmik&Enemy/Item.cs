using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType
    {
        Normal,
        Heart,
        Wide,
        Speed,
        FinalSword
    }

    public ItemType itemType;
    public int scoreValue = 100; //アイテムのスコア値
    [SerializeField] private AudioClip hitSE; //攻撃を受けた時のSE

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Get！");
            AudioManager.Instance.PlaySE(hitSE); //SEを再生
            Destroy(gameObject); //アイテムを削除

            switch (itemType)
            {
                case ItemType.Normal:
                    // 通常アイテムの処理
                    //GameManager.Instance.AddScore(scoreValue);
                    break;
                case ItemType.Heart:
                    // ハートアイテムの処理
                    //PlayerController.currentLife++;
                    break;
                case ItemType.Wide:
                    // ワイドアイテムの処理
                    //GameManager.Instance.AddScore(scoreValue);
                    break;
                case ItemType.Speed:
                    // スピードアイテムの処理
                    //GameManager.Instance.AddScore(scoreValue);
                    break;
                case ItemType.FinalSword:
                    // ファイナルソードアイテムの処理
                    //GameManager.Instance.AddScore(scoreValue);
                    break;
            }
        }
    }

}

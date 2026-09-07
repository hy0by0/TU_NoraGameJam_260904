using DG.Tweening;
using UnityEngine;


//敵の種類
public enum EnemyType
{
    Normal,
    Shooter,
    Parry,
    Boss
}


public class Enemy : MonoBehaviour
{
    public EnemyType enemyType; //敵の種類
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
        if (collision.gameObject.tag == "Attack")
        {
            Debug.Log("Hit！");
            AudioManager.Instance.PlaySE(hitSE); //SEを再生
            Destroy(gameObject); //敵を破壊する
        }
    }

}

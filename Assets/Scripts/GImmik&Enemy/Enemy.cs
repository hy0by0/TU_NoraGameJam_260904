using UnityEngine;

// 敵の種類です。
public enum EnemyType { Normal, Shooter, Parry, Boss }

// 一撃で倒れる敵の基礎スコアと、重複撃破の防止を管理します。
public class Enemy : MonoBehaviour
{
    public EnemyType enemyType;
    [SerializeField, Min(0)] private int scoreValue = 100;
    private bool isDefeated;
    public int ScoreValue => scoreValue;

    // 複数の当たり判定に触れても、一度だけ撃破を受け付けます。
    public bool TryDefeat()
    {
        if (isDefeated) return false;
        isDefeated = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }
}

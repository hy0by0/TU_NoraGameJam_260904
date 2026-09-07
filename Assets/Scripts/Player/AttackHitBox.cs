using UnityEngine;

/// <summary>
/// 1回の攻撃につき、最初に触れた敵タイプだけをPlayerControllerへ通知するクラスです。
/// </summary>
public class AttackHitBox : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private bool hasDetectedEnemy;

    /// <summary>
    /// 新しい攻撃を開始し、敵タイプを再び1件だけ取得できる状態にします。
    /// </summary>
    public void BeginAttack()
    {
        hasDetectedEnemy = false;
    }

    /// <summary>
    /// 攻撃範囲に入った最初の敵から種類を取得して通知します。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDetectedEnemy)
        {
            return;
        }

        if (other.TryGetComponent(out Enemy enemy))
        {
            hasDetectedEnemy = true;
            playerController.ReceiveAttackHit(enemy.enemyType);
        }
    }
}

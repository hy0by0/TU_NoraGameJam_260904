using UnityEngine;

// 一回の攻撃の撃破数を集計し、命中音と加点をプレイヤーへ通知します。
public class AttackHitBox : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    private bool isAttackActive;
    private int defeatedCount;
    private int baseScore;

    // 新しい攻撃の集計を開始します。
    public void BeginAttack()
    {
        defeatedCount = 0;
        baseScore = 0;
        isAttackActive = true;
    }

    // 通常終了・被弾中断のどちらでも加点を一度だけ確定します。
    public void EndAttack()
    {
        if (!isAttackActive) return;
        isAttackActive = false;
        playerController.CompleteAttack(defeatedCount, baseScore);
    }

    // 外部から範囲を閉じた場合も獲得済みスコアを失いません。
    private void OnDisable()
    {
        EndAttack();
    }

    // 親にEnemyがある複数Collider構成も、一体として撃破します。
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAttackActive) return;
        Enemy[] enemies = other.GetComponentsInParent<Enemy>();
        if (enemies.Length == 0) return;
        Enemy enemy = enemies[0];
        if (!enemy.TryDefeat()) return;
        defeatedCount++;
        baseScore += enemy.ScoreValue;
        if (defeatedCount == 1) playerController.ReceiveAttackHit(enemy.enemyType);
    }

    // 攻撃開始前から重なっていた敵も感知します。
    private void OnTriggerStay2D(Collider2D other)
    {
        OnTriggerEnter2D(other);
    }
}

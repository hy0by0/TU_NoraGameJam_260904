using NUnit.Framework.Internal;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// プレイヤーの移動と、BPMに同期した攻撃の進行を管理するクラスです。
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 5f;

    [Header("共通タイミング設定")]
    [SerializeField] private MusicConductor musicConductor;
    [SerializeField] private PlayerEntranceController entranceController;

    [Header("攻撃タイミング")]
    [SerializeField, Min(0.01f)] private float attackDurationBeats = 0.5f;
    [SerializeField, Min(0f)] private float attackIntervalBeats = 1f;

    [Header("攻撃に使用するオブジェクト")]
    [SerializeField] private AttackHitBox attackHitBox;
    [SerializeField] private BoxCollider2D attackRangeCollider;
    [FormerlySerializedAs("attackAnimationClip")]
    [SerializeField] private AnimationClip PlayerAttackAnimation;
    [SerializeField] private AnimationClip playerDamageAnimation;
    [SerializeField] private Animator playerAnimator;

    [Header("ライフ関連")]
    public int maxLife = 6;
    public int currentLife = 6;
    [SerializeField, Min(0.01f)] private float invincibleTimeBeats = 1f; // 被弾後の無敵時間（拍数）
    [FormerlySerializedAs("NoAttackTimeBeats")]
    [SerializeField, Min(0f)] private float noAttackTimeBeats = 0.5f; // 被弾後の攻撃不可時間（拍数）
    private bool isDamaged; // 被弾による無敵時間中かどうか
    private bool isAttackDisabled; // 被弾による攻撃不可時間中かどうか

    [Header("スコア関連")]
    public int currentScore = 0;


    [Header("アニメーション名")]
    public string normalAnime = "PlayerMoveAnimation";
    public string normalAtackAnime = "PlayerAttackAnimation";
    public string normalDamageAnime = "PlayerDamageAnimation";
    public string deadAnime = "PlayerMissAnimation";

    [Header("攻撃ヒット時の通知")]
    [SerializeField] private UnityEvent<EnemyType> onAttackHit = new UnityEvent<EnemyType>();

    [SerializeField] private AudioClip testSE;
    private bool isAttackLocked; // 攻撃中かどうかを示すフラグ
    private Coroutine attackCoroutine; // 実行中の攻撃処理
    private bool didHitThisAttack; // 攻撃中に敵にヒットしたかどうかを示すフラグ
    private EnemyType lastHitEnemyType;
    private Rigidbody2D playerRigidbody;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

    public bool DidHitThisAttack => didHitThisAttack;
    public EnemyType LastHitEnemyType => lastHitEnemyType;


    /// <summary>
    /// 必要なコンポーネントと入力を初期化し、攻撃範囲を無効にします。
    /// </summary>
    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        attackHitBox.gameObject.SetActive(false);
        inputActions = new InputSystem_Actions();
        playerAnimator.Play(normalAnime);
    }

    /// <summary>
    /// プレイヤー入力を有効にします。
    /// </summary>
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    /// <summary>
    /// プレイヤー入力を無効にし、攻撃途中でも攻撃範囲を閉じます。
    /// </summary>
    private void OnDisable()
    {
        inputActions.Player.Disable();
        attackHitBox.gameObject.SetActive(false);
        isAttackLocked = false;
        isAttackDisabled = false;
        isDamaged = false;
        playerRigidbody.linearVelocity = Vector2.zero;
        playerAnimator.speed = 1f;
    }

    /// <summary>
    /// 移動入力と攻撃入力を読み取ります。
    /// </summary>
    private void Update()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        // Input Actions の Attack に割り当てられた左クリックで攻撃します。
        if (entranceController.IsInputEnabled
            && inputActions.Player.Attack.WasPressedThisFrame()
            && !isAttackLocked
            && !isAttackDisabled)
        {
            AudioManager.Instance.PlaySE(testSE); //空振り音の時のSEを流す？
            attackCoroutine = StartCoroutine(AttackCoroutine());
        }

    }

    /// <summary>
    /// 登場完了後、ステージ進行X座標と上下入力をまとめてRigidbody2Dへ反映します。
    /// </summary>
    private void FixedUpdate()
    {
        if (!entranceController.IsInputEnabled || !entranceController.HasReachedReferencePosition)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            return;
        }

        float targetY = playerRigidbody.position.y + moveInput.y * moveSpeed * Time.fixedDeltaTime;
        Vector2 targetPosition = new Vector2(entranceController.GameplayWorldX, targetY);
        playerRigidbody.MovePosition(targetPosition);
    }

    /// <summary>
    /// BPMから攻撃時間を求め、攻撃範囲とアニメーションを同じ時間だけ有効にします。
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        isAttackLocked = true;
        didHitThisAttack = false;

        float attackDuration = musicConductor.BeatsToSeconds(attackDurationBeats);
        float attackInterval = musicConductor.BeatsToSeconds(attackIntervalBeats);
        float attackAnimationSpeed = PlayerAttackAnimation.length / attackDuration;

        attackHitBox.BeginAttack();
        attackHitBox.gameObject.SetActive(true);
        playerAnimator.speed = attackAnimationSpeed;
        playerAnimator.Play(normalAtackAnime, 0, 0f); //後にプレイヤーの取得装備状態に応じた攻撃アニメーションを再生するようにする。それに伴い、有効長さも変更があるかもしれません。

        yield return WaitForMusicSeconds(attackDuration);

        attackHitBox.gameObject.SetActive(false);
        playerAnimator.speed = 1f;
        playerAnimator.Play(normalAnime, 0, 0f); //後にプレイヤーの取得装備状態に応じて戻すアニメーションも変えるようにする

        yield return WaitForMusicSeconds(attackInterval);

        isAttackLocked = false;
    }

    /// <summary>
    /// AttackHitBoxが最初に感知した敵タイプを保存し、効果音などの後続処理へ通知します。
    /// </summary>
    public void ReceiveAttackHit(EnemyType enemyType)
    {
        didHitThisAttack = true;
        lastHitEnemyType = enemyType;
        onAttackHit.Invoke(enemyType);
    }

    /// <summary>
    /// 敵との接触を検知し、ライフ減少と被弾状態を開始します。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //被弾時
        if (collision.CompareTag("Enemy") && !isDamaged)
        {
            Debug.Log("Player hit by enemy!");
            currentLife = Mathf.Max(0, currentLife - 1);

            //被弾時のSEを再生する

            float invincibleDuration = musicConductor.BeatsToSeconds(invincibleTimeBeats);
            float noAttackDuration = musicConductor.BeatsToSeconds(noAttackTimeBeats);

            // 攻撃中に被弾した場合は、実行中の攻撃だけを終了します。
            if (isAttackLocked)
            {
                StopCoroutine(attackCoroutine);
                attackHitBox.gameObject.SetActive(false);
                isAttackLocked = false;
            }

            isDamaged = true;
            isAttackDisabled = true;
            StartCoroutine(InvincibilityCoroutine(invincibleDuration));
            StartCoroutine(AttackDisableCoroutine(noAttackDuration));
        }

        //アイテム取得時
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            //取得アイテムごとの処理を記載予定。
            //switch (item.itemType)
            //Item.ItemType.Heart:
            currentScore += item.scoreValue;
        }
    }

    /// <summary>
    /// 指定秒数の間、無敵状態と被弾アニメーションを継続します。
    /// </summary>
    private IEnumerator InvincibilityCoroutine(float duration)
    {
        playerAnimator.speed = playerDamageAnimation.length / duration;
        playerAnimator.Play(normalDamageAnime, 0, 0f);

        yield return WaitForMusicSeconds(duration);

        isDamaged = false;
        playerAnimator.speed = 1f;
        playerAnimator.Play(normalAnime, 0, 0f);
    }

    /// <summary>
    /// 指定秒数の間、攻撃入力を受け付けない状態にします。
    /// </summary>
    private IEnumerator AttackDisableCoroutine(float duration)
    {
        yield return WaitForMusicSeconds(duration);
        isAttackDisabled = false;
    }

    /// <summary>
    /// BGMの再生位置を基準に、指定秒数が経過するまで待機します。
    /// </summary>
    private IEnumerator WaitForMusicSeconds(float duration)
    {
        float endTime = musicConductor.PlaybackTimeSeconds + duration;

        while (musicConductor.IsGameRunning && musicConductor.PlaybackTimeSeconds < endTime)
        {
            yield return null;
        }
    }
}

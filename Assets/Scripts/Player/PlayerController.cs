using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// プレイヤーの移動と、BPMに同期した攻撃の進行を管理するクラスです。
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("形態と表示")]
    [SerializeField] private PlayerFormController formController;
    [SerializeField] private PlayerAnimationController animationController;
    public float moveSpeed => formController.Current.MoveSpeed;
    public PlayerFormDefinition CurrentForm => formController.Current;

    [Header("共通タイミング設定")]
    [SerializeField] private MusicConductor musicConductor;
    [SerializeField] private PlayerEntranceController entranceController;

    [Header("攻撃タイミング")]
    [SerializeField, Min(0.01f)] private float attackDurationBeats = 0.5f;
    [SerializeField, Min(0f)] private float attackIntervalBeats = 1f;

    [Header("攻撃に使用するオブジェクト")]
    [SerializeField] private AttackHitBox attackHitBox;

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
    [SerializeField, Min(2)] private int multiKillThreshold = 2;
    [SerializeField, Min(0f)] private float singleKillMultiplier = 1f;
    [SerializeField, Min(0f)] private float multiKillMultiplier = 2f;



    [Header("攻撃ヒット時の通知")]
    [SerializeField] private UnityEvent<EnemyType> onAttackHit = new UnityEvent<EnemyType>();

    [Header("全形態共通の被弾音")]
    [SerializeField] private AudioClip damageSE;
    private PlayerFormDefinition attackForm;
    private bool isAttacking;
    private float attackStartedAt;
    private float damageStartedAt;
    private float activeAttackDuration;
    private float activeDamageDuration;
    private float activeAttackInterval;
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
        formController.Initialize();
        RefreshAnimation();
    }

    /// <summary>
    /// プレイヤー入力を有効にします。
    /// </summary>
    private void OnEnable()
    {
        inputActions.Player.Enable();
        RefreshAnimation();
    }

    /// <summary>
    /// プレイヤー入力を無効にし、攻撃途中でも攻撃範囲を閉じます。
    /// </summary>
    private void OnDisable()
    {
        inputActions.Player.Disable();
        StopAllCoroutines();
        isAttacking = false;
        attackHitBox.gameObject.SetActive(false);
        isAttackLocked = false;
        isAttackDisabled = false;
        isDamaged = false;
        playerRigidbody.linearVelocity = Vector2.zero;
        animationController.ResetPresentation();
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
        attackForm = formController.Current;
        isAttacking = true;
        attackStartedAt = musicConductor.PlaybackTimeSeconds;

        float attackDuration = musicConductor.BeatsToSeconds(attackDurationBeats);
        float attackInterval = musicConductor.BeatsToSeconds(attackIntervalBeats);
        activeAttackDuration = attackDuration;
        activeAttackInterval = attackInterval;

        attackHitBox.BeginAttack();
        attackHitBox.gameObject.SetActive(true);
        RefreshAnimation();

        yield return WaitForMusicSeconds(attackDuration);

        attackHitBox.gameObject.SetActive(false);
        isAttacking = false;
        RefreshAnimation();

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
        AudioManager.Instance.PlaySE(attackForm.HitSE);
        onAttackHit.Invoke(enemyType);
    }

    // 空振り音、または今回倒した敵の基礎点合計に倍率を掛けた加点を確定します。
    public void CompleteAttack(int defeatedCount, int baseScore)
    {
        if (defeatedCount == 0)
        {
            AudioManager.Instance.PlaySE(attackForm.MissSE);
            return;
        }
        float multiplier = defeatedCount >= multiKillThreshold ? multiKillMultiplier : singleKillMultiplier;
        currentScore += Mathf.RoundToInt(baseScore * multiplier);
    }

    // 進行中の攻撃は旧形態の設定で一度だけ精算し、形態を切り替えます。
    public void ChangeForm(PlayerFormDefinition nextForm)
    {
        if (formController.Current == nextForm) return;
        if (isAttacking)
        {
            StopCoroutine(attackCoroutine);
            attackHitBox.gameObject.SetActive(false);
            isAttacking = false;
            attackCoroutine = StartCoroutine(AttackCooldownCoroutine());
        }
        formController.ChangeForm(nextForm);
        RefreshAnimation();
    }

    // 形態変更で攻撃を中断しても、攻撃後の待機時間は省略しません。
    private IEnumerator AttackCooldownCoroutine()
    {
        yield return WaitForMusicSeconds(activeAttackInterval);
        isAttackLocked = false;
    }

    // 終了処理ごとに現在の状態を確認し、被弾終了で攻撃表示を上書きしません。
    private void RefreshAnimation()
    {
        float time = musicConductor.PlaybackTimeSeconds;
        float attackProgress = isAttacking ? (time - attackStartedAt) / activeAttackDuration : 0f;
        float damageProgress = isDamaged ? (time - damageStartedAt) / activeDamageDuration : 0f;
        animationController.Refresh(formController.Current, isAttacking, isDamaged,
            attackProgress, damageProgress, activeAttackDuration, activeDamageDuration);
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

            AudioManager.Instance.PlaySE(damageSE);

            float invincibleDuration = musicConductor.BeatsToSeconds(invincibleTimeBeats);
            float noAttackDuration = musicConductor.BeatsToSeconds(noAttackTimeBeats);

            // 攻撃中に被弾した場合は、実行中の攻撃だけを終了します。
            if (isAttackLocked)
            {
                StopCoroutine(attackCoroutine);
                attackHitBox.gameObject.SetActive(false);
                isAttackLocked = false;
                isAttacking = false;
            }

            isDamaged = true;
            isAttackDisabled = true;
            StartCoroutine(InvincibilityCoroutine(invincibleDuration));
            StartCoroutine(AttackDisableCoroutine(noAttackDuration));
        }

    }

    /// <summary>
    /// 指定秒数の間、無敵状態と被弾アニメーションを継続します。
    /// </summary>
    private IEnumerator InvincibilityCoroutine(float duration)
    {
        damageStartedAt = musicConductor.PlaybackTimeSeconds;
        activeDamageDuration = duration;
        RefreshAnimation();

        yield return WaitForMusicSeconds(duration);

        isDamaged = false;
        RefreshAnimation();
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

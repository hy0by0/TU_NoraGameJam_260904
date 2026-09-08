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

    [Header("上下移動範囲")]
    [SerializeField, Tooltip("プレイヤーが移動できる最も下のワールドY座標です。")]
    private float verticalLowerLimit = -4f;
    [SerializeField, Tooltip("プレイヤーが移動できる最も上のワールドY座標です。")]
    private float verticalUpperLimit = 4f;

    [Header("攻撃タイミング")]
    [SerializeField, Min(0.01f)] private float attackDurationBeats = 0.5f;
    [SerializeField, Min(0f)] private float attackIntervalBeats = 1f;

    [Header("攻撃に使用するオブジェクト")]
    [SerializeField] private AttackHitBox attackHitBox;
    [SerializeField] private PlayerSlashEffect slashEffect;
    [SerializeField] private PlayerBeamAttack beamAttack;
    [SerializeField] private FinalEventController finalEventController;

    [Header("ライフ関連")]
    [SerializeField] private Collider2D damageCollider;
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
    private int comboStrikeIndex; // 現在処理している5連撃の段数（0始まり）
    private bool comboHitSEStarted; // 5連撃中にヒット音の時間軸へ切り替えたかどうか
    private Rigidbody2D playerRigidbody;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool isFinalEventActive;
    private bool isGameplayEnabled = true;

    public bool DidHitThisAttack => didHitThisAttack;
    public EnemyType LastHitEnemyType => lastHitEnemyType;
    public bool IsFinalEventActive => isFinalEventActive;
    public bool IsGameplayEnabled => isGameplayEnabled;


    /// <summary>
    /// 必要なコンポーネントと入力を初期化し、攻撃範囲を無効にします。
    /// </summary>
    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        attackHitBox.gameObject.SetActive(false);
        inputActions = new InputSystem_Actions();
        slashEffect.Hide();
        formController.Initialize();
        beamAttack.End();
        RefreshAnimation();
    }

    /// <summary>
    /// プレイヤー入力を有効にします。
    /// </summary>
    private void OnEnable()
    {
        if (isGameplayEnabled)
        {
            inputActions.Player.Enable();
        }
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
        slashEffect.Hide();
        attackHitBox.gameObject.SetActive(false);
        beamAttack.End();
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
        if (!isGameplayEnabled)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (isFinalEventActive)
        {
            moveInput = Vector2.zero;
            return;
        }
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
        if (!isGameplayEnabled)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            return;
        }

        if (isFinalEventActive)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            return;
        }
        if (!entranceController.IsInputEnabled || !entranceController.HasReachedReferencePosition)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            return;
        }

        float targetY = playerRigidbody.position.y + moveInput.y * moveSpeed * Time.fixedDeltaTime;
        targetY = Mathf.Clamp(targetY, verticalLowerLimit, verticalUpperLimit);
        Vector2 targetPosition = new Vector2(entranceController.GameplayWorldX, targetY);
        playerRigidbody.MovePosition(targetPosition);
    }

    /// <summary>
    /// BPMから攻撃時間を求め、攻撃範囲とアニメーションを同じ時間だけ有効にします。
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        if (formController.Current.Kind == PlayerFormKind.Beam)
        {
            yield return BeamAttackCoroutine();
            yield break;
        }
        if (formController.Current.Kind == PlayerFormKind.FiveHit)
        {
            yield return ComboAttackCoroutine();
            yield break;
        }
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

    // 攻撃開始からの絶対時刻で5回の判定を開閉し、フレームごとの待機誤差を蓄積させません。
    private IEnumerator ComboAttackCoroutine()
    {
        isAttackLocked = true;
        isAttacking = true;
        didHitThisAttack = false;
        comboStrikeIndex = 0;
        comboHitSEStarted = false;
        attackForm = formController.Current;
        attackStartedAt = musicConductor.PlaybackTimeSeconds;
        float spacing = musicConductor.BeatsToSeconds(attackForm.ComboSpacingBeats);
        float hitDuration = musicConductor.BeatsToSeconds(attackForm.ComboHitDurationBeats);
        activeAttackDuration = spacing * (PlayerFormDefinition.ComboHitCount - 1) + hitDuration;
        activeAttackInterval = musicConductor.BeatsToSeconds(attackForm.ComboCooldownBeats);
        RefreshAnimation();

        for (int index = 0; index < PlayerFormDefinition.ComboHitCount; index++)
        {
            comboStrikeIndex = index;
            float start = attackStartedAt + index * spacing;
            yield return WaitForComboTime(start, spacing);
            if (!musicConductor.IsGameRunning) break;
            attackHitBox.BeginAttack();
            attackHitBox.gameObject.SetActive(true);
            yield return WaitForComboTime(start + hitDuration, spacing);
            // 各発ごとに得点を精算します。効果音は連撃全体の時間軸で管理し、判定は次の発まで閉じます。
            attackHitBox.gameObject.SetActive(false);
        }

        slashEffect.Hide();
        isAttacking = false;
        RefreshAnimation();
        yield return WaitForMusicSeconds(activeAttackInterval);
        isAttackLocked = false;
        RefreshAnimation();
    }

    // 魔法形態は専用ビームだけを使い、1発分の終了後に指定拍数だけ待機します。
    private IEnumerator BeamAttackCoroutine()
    {
        isAttackLocked = true;
        isAttacking = true;
        didHitThisAttack = false;
        attackForm = formController.Current;
        attackStartedAt = musicConductor.PlaybackTimeSeconds;
        activeAttackDuration = musicConductor.BeatsToSeconds(attackForm.BeamDurationBeats);
        activeAttackInterval = musicConductor.BeatsToSeconds(attackForm.BeamCooldownBeats);
        RefreshAnimation();
        beamAttack.Begin(attackForm);
        while (musicConductor.IsGameRunning && musicConductor.PlaybackTimeSeconds < attackStartedAt + activeAttackDuration)
        {
            beamAttack.Sample((musicConductor.PlaybackTimeSeconds - attackStartedAt) / activeAttackDuration);
            yield return null;
        }
        beamAttack.End();
        isAttacking = false;
        RefreshAnimation();
        yield return WaitForMusicSeconds(activeAttackInterval);
        isAttackLocked = false;
        RefreshAnimation();
    }

    // 待機中も通常絵との交互表示と斬撃2枚を、同じBGM時計で進めます。
    private IEnumerator WaitForComboTime(float endTime, float spacing)
    {
        while (musicConductor.IsGameRunning && musicConductor.PlaybackTimeSeconds < endTime)
        {
            float progress = (musicConductor.PlaybackTimeSeconds - attackStartedAt) / spacing;
            // 判定時間を調整しても、クリップ前半の攻撃絵と後半の通常絵を判定に合わせます。
            float phase = Mathf.Repeat(progress, 1f);
            float hitRatio = attackForm.ComboHitDurationBeats / attackForm.ComboSpacingBeats;
            float animationPhase = phase < hitRatio
                ? phase / hitRatio * 0.5f
                : 0.5f + (phase - hitRatio) / (1f - hitRatio) * 0.5f;
            animationController.SampleCombo(animationPhase);
            slashEffect.Show(progress);
            yield return null;
        }
    }

    /// <summary>
    /// AttackHitBoxが最初に感知した敵タイプを保存し、効果音などの後続処理へ通知します。
    /// </summary>
    public void ReceiveAttackHit(EnemyType enemyType)
    {
        didHitThisAttack = true;
        lastHitEnemyType = enemyType;

        if (attackForm.Kind == PlayerFormKind.FiveHit)
        {
            // 1段目の命中は音源の先頭から、途中の初命中は連撃の経過位置から一度だけ再生します。
            if (!comboHitSEStarted)
            {
                float elapsedSeconds = comboStrikeIndex == 0
                    ? 0f
                    : Mathf.Max(0f, musicConductor.PlaybackTimeSeconds - attackStartedAt);
                AudioManager.Instance.PlaySEFromTime(attackForm.HitSE, elapsedSeconds);
                comboHitSEStarted = true;
            }
        }
        else
        {
            AudioManager.Instance.PlaySE(attackForm.HitSE);
        }

        onAttackHit.Invoke(enemyType);
    }

    // 空振り音、または今回倒した敵の基礎点合計に倍率を掛けた加点を確定します。
    public void CompleteAttack(int defeatedCount, int baseScore)
    {
        if (defeatedCount == 0)
        {
            if (attackForm.Kind == PlayerFormKind.FiveHit)
            {
                // 1段目が空振りした時だけ空振り音の時間軸を開始し、各段での重複再生を防ぎます。
                if (comboStrikeIndex == 0 && !comboHitSEStarted)
                {
                    float elapsedSeconds = Mathf.Max(0f, musicConductor.PlaybackTimeSeconds - attackStartedAt);
                    AudioManager.Instance.PlaySEFromTime(attackForm.MissSE, elapsedSeconds);
                }
                return;
            }

            AudioManager.Instance.PlaySE(attackForm.MissSE);
            return;
        }
        float multiplier = defeatedCount >= multiKillThreshold ? multiKillMultiplier : singleKillMultiplier;
        if (attackForm.Kind == PlayerFormKind.Beam)
            multiplier = defeatedCount >= multiKillThreshold ? attackForm.BeamMultiKillMultiplier : attackForm.BeamSingleKillMultiplier;
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
            beamAttack.End();
            isAttacking = false;
            slashEffect.Hide();
            attackCoroutine = StartCoroutine(AttackCooldownCoroutine());
        }
        formController.ChangeForm(nextForm);
        RefreshAnimation();
        if (nextForm.Kind == PlayerFormKind.Finale)
            finalEventController.BeginEvent();
    }

    // 最終イベント開始時に通常入力・攻撃・被弾を止め、専用演出へ制御を渡します。
    public void BeginFinalEventState()
    {
        StopAllCoroutines();
        attackHitBox.gameObject.SetActive(false);
        slashEffect.Hide();
        beamAttack.End();
        isAttacking = false;
        isAttackLocked = true;
        isAttackDisabled = true;
        isDamaged = false;
        isFinalEventActive = true;
        moveInput = Vector2.zero;
        playerRigidbody.linearVelocity = Vector2.zero;
        RefreshAnimation();
    }

    /// <summary>
    /// ゲーム進行から入力と移動を一括で有効・無効にします。
    /// </summary>
    public void SetGameplayEnabled(bool isEnabled)
    {
        isGameplayEnabled = isEnabled;

        if (isEnabled)
        {
            inputActions.Player.Enable();
            return;
        }

        inputActions.Player.Disable();
        StopAllCoroutines();
        moveInput = Vector2.zero;
        isAttacking = false;
        isAttackLocked = false;
        isAttackDisabled = true;
        attackHitBox.gameObject.SetActive(false);
        slashEffect.Hide();
        beamAttack.End();
        playerRigidbody.linearVelocity = Vector2.zero;
        RefreshAnimation();
    }

    // 形態変更で攻撃を中断しても、攻撃後の待機時間は省略しません。
    private IEnumerator AttackCooldownCoroutine()
    {
        yield return WaitForMusicSeconds(activeAttackInterval);
        isAttackLocked = false;
        RefreshAnimation();
    }

    // 終了処理ごとに現在の状態を確認し、被弾終了で攻撃表示を上書きしません。
    private void RefreshAnimation()
    {
        float time = musicConductor.PlaybackTimeSeconds;
        float attackProgress = isAttacking ? (time - attackStartedAt) / activeAttackDuration : 0f;
        float damageProgress = isDamaged ? (time - damageStartedAt) / activeDamageDuration : 0f;
        animationController.Refresh(formController.Current, isAttacking, isDamaged,
            attackProgress, damageProgress, activeAttackDuration, activeDamageDuration,
            isAttackLocked && !isAttacking && formController.Current.Kind == PlayerFormKind.FiveHit
                && formController.Current == attackForm);
    }

    /// <summary>
    /// 敵との接触を検知し、ライフ減少と被弾状態を開始します。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //被弾時
        // 攻撃範囲から親Rigidbody2Dへ届いた通知を、本体の被弾と取り違えないようにします。
        if (collision.CompareTag("Enemy") && !isDamaged && !isFinalEventActive && damageCollider.IsTouching(collision))
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
                beamAttack.End();
                isAttackLocked = false;
                isAttacking = false;
                slashEffect.Hide();
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

using UnityEngine;
using UnityEngine.Serialization;

// プレイヤーの形態名です。FiveHitは5連撃、Beamはビームを使用します。
public enum PlayerFormKind { Normal, FiveHit, Beam, Finale }

// 一つの形態の見た目・上下移動速度・攻撃音・専用タイミングを保存する設定アセットです。
[CreateAssetMenu(menuName = "Player/形態設定", fileName = "PlayerForm")]
public class PlayerFormDefinition : ScriptableObject
{
    [SerializeField] private PlayerFormKind kind;
    [SerializeField, Min(0f)] private float moveSpeed = 10f;
    [SerializeField] private AnimatorOverrideController animations;
    [SerializeField] private AudioClip hitSE;
    [SerializeField] private AudioClip missSE;

    [Header("通常攻撃（Normal・Finale）")]
    [SerializeField, Min(0.01f), InspectorName("攻撃表示・アニメーション時間（拍）"),
     Tooltip("瞬間的な攻撃判定が終わった後も、攻撃姿勢とアニメーションを表示する時間です。")]
    private float attackVisualDurationBeats = 0.5f;

    [Header("5連撃（FiveHitのみ）")]
    [SerializeField, Min(0.01f), Tooltip("各攻撃の開始間隔。0.25なら1拍に4回の間隔です。")]
    private float comboSpacingBeats = 0.25f;
    [SerializeField, Min(0.01f), Tooltip("1発の判定時間。開始間隔より短くして判定の切れ目を作ります。")]
    private float comboHitDurationBeats = 0.125f;
    [SerializeField, Min(0f), Tooltip("5発目の判定が終わってから再攻撃できるまでの拍数です。")]
    private float comboCooldownBeats = 1f;

    [Header("ビーム（Beamのみ）")]
    [FormerlySerializedAs("beamDurationBeats")]
    [SerializeField, Min(0.01f), InspectorName("表示・アニメーション時間（拍）"),
     Tooltip("瞬間判定後も、ビームが表示されて徐々に細くなる時間です。攻撃判定時間には影響しません。")]
    private float beamVisualDurationBeats = 0.5f;
    [SerializeField, Min(0.001f), InspectorName("発射光の表示時間（拍）")]
    private float beamExtendBeats = 0.08f;
    [SerializeField, Min(0f)] private float beamCooldownBeats = 1f;
    [SerializeField, Min(0.01f), Tooltip("ワールド単位の最大射程です。")]
    private float beamLength = 8f;
    [SerializeField, Min(0.01f), Tooltip("ワールド単位の最大の太さです。瞬間判定にはこの太さを使い、表示だけ徐々に細くなります。")]
    private float beamThickness = 0.8f;
    [SerializeField, Min(0f)] private float beamSingleKillMultiplier = 1f;
    [SerializeField, Min(0f)] private float beamMultiKillMultiplier = 3f;

    public float BeamVisualDurationBeats => Mathf.Max(0.01f, beamVisualDurationBeats);
    public float BeamExtendBeats => Mathf.Clamp(beamExtendBeats, 0.001f, BeamVisualDurationBeats);
    public float BeamCooldownBeats => Mathf.Max(0f, beamCooldownBeats);
    public float BeamLength => Mathf.Max(0.01f, beamLength);
    public float BeamThickness => Mathf.Max(0.01f, beamThickness);
    public float BeamSingleKillMultiplier => Mathf.Max(0f, beamSingleKillMultiplier);
    public float BeamMultiKillMultiplier => Mathf.Max(0f, beamMultiKillMultiplier);

    public const int ComboHitCount = 5;
    public float AttackVisualDurationBeats => Mathf.Max(0.01f, attackVisualDurationBeats);
    public float ComboSpacingBeats => Mathf.Max(0.01f, comboSpacingBeats);
    public float ComboHitDurationBeats => Mathf.Clamp(comboHitDurationBeats, 0.001f, ComboSpacingBeats * 0.95f);
    public float ComboCooldownBeats => Mathf.Max(0f, comboCooldownBeats);

    public PlayerFormKind Kind => kind;
    public float MoveSpeed => moveSpeed;
    public AnimatorOverrideController Animations => animations;
    public AudioClip HitSE => hitSE;
    public AudioClip MissSE => missSE;
}

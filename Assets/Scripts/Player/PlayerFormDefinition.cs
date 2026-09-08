using UnityEngine;

// プレイヤーの形態名です。FiveHitは5連撃、Beamはビームを使用します。
public enum PlayerFormKind { Normal, FiveHit, Beam, Finale }

// 一つの形態の見た目・上下移動速度・攻撃音を保存する設定アセットです。
[CreateAssetMenu(menuName = "Player/形態設定", fileName = "PlayerForm")]
public class PlayerFormDefinition : ScriptableObject
{
    [SerializeField] private PlayerFormKind kind;
    [SerializeField, Min(0f)] private float moveSpeed = 10f;
    [SerializeField] private AnimatorOverrideController animations;
    [SerializeField] private AudioClip hitSE;
    [SerializeField] private AudioClip missSE;

    [Header("5連撃（FiveHitのみ）")]
    [SerializeField, Min(0.01f), Tooltip("各攻撃の開始間隔。0.25なら1拍に4回の間隔です。")]
    private float comboSpacingBeats = 0.25f;
    [SerializeField, Min(0.01f), Tooltip("1発の判定時間。開始間隔より短くして判定の切れ目を作ります。")]
    private float comboHitDurationBeats = 0.125f;
    [SerializeField, Min(0f), Tooltip("5発目の判定が終わってから再攻撃できるまでの拍数です。")]
    private float comboCooldownBeats = 1f;

    [Header("ビーム（Beamのみ）")]
    [SerializeField, Min(0.01f)] private float beamDurationBeats = 0.35f;
    [SerializeField, Min(0.001f)] private float beamExtendBeats = 0.08f;
    [SerializeField, Min(0f)] private float beamCooldownBeats = 1f;
    [SerializeField, Min(0.01f), Tooltip("ワールド単位の最大射程です。")]
    private float beamLength = 8f;
    [SerializeField, Min(0.01f), Tooltip("ワールド単位の最大の太さです。見た目と判定を一緒に縮めます。")]
    private float beamThickness = 0.8f;
    [SerializeField, Min(0f)] private float beamSingleKillMultiplier = 1f;
    [SerializeField, Min(0f)] private float beamMultiKillMultiplier = 3f;

    public float BeamDurationBeats => Mathf.Max(0.01f, beamDurationBeats);
    public float BeamExtendBeats => Mathf.Clamp(beamExtendBeats, 0.001f, BeamDurationBeats);
    public float BeamCooldownBeats => Mathf.Max(0f, beamCooldownBeats);
    public float BeamLength => Mathf.Max(0.01f, beamLength);
    public float BeamThickness => Mathf.Max(0.01f, beamThickness);
    public float BeamSingleKillMultiplier => Mathf.Max(0f, beamSingleKillMultiplier);
    public float BeamMultiKillMultiplier => Mathf.Max(0f, beamMultiKillMultiplier);

    public const int ComboHitCount = 5;
    public float ComboSpacingBeats => Mathf.Max(0.01f, comboSpacingBeats);
    public float ComboHitDurationBeats => Mathf.Clamp(comboHitDurationBeats, 0.001f, ComboSpacingBeats * 0.95f);
    public float ComboCooldownBeats => Mathf.Max(0f, comboCooldownBeats);

    public PlayerFormKind Kind => kind;
    public float MoveSpeed => moveSpeed;
    public AnimatorOverrideController Animations => animations;
    public AudioClip HitSE => hitSE;
    public AudioClip MissSE => missSE;
}

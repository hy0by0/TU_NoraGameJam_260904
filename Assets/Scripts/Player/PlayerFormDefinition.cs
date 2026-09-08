using UnityEngine;

// プレイヤーの形態名です。攻撃方式の実装は②以降で追加します。
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

    public PlayerFormKind Kind => kind;
    public float MoveSpeed => moveSpeed;
    public AnimatorOverrideController Animations => animations;
    public AudioClip HitSE => hitSE;
    public AudioClip MissSE => missSE;
}

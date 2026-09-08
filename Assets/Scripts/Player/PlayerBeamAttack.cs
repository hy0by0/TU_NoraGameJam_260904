using UnityEngine;

// シーンに配置したビームと発射光を伸縮し、同じ形状の当たり判定で撃破を集計します。
public class PlayerBeamAttack : MonoBehaviour
{
    [SerializeField] private SpriteRenderer beamRenderer;
    [SerializeField] private BoxCollider2D beamCollider;
    [SerializeField] private AttackHitBox hitBox;
    [SerializeField] private Transform muzzleFlash;
    [SerializeField] private SpriteRenderer flashRenderer;
    [SerializeField, Min(0.01f), Tooltip("発射光の最大直径（ワールド単位）です。")]
    private float flashSize = 0.9f;
    private PlayerFormDefinition activeForm;

    // 攻撃開始時の設定を保持し、一発分の集計を開始します。
    public void Begin(PlayerFormDefinition form)
    {
        activeForm = form;
        hitBox.BeginAttack();
        gameObject.SetActive(true);
        Sample(0f);
    }

    // 曲の進行率に合わせて伸長・細まり・発射光を更新します。
    public void Sample(float progress)
    {
        float phase = Mathf.Clamp01(progress);
        float extensionRatio = activeForm.BeamExtendBeats / activeForm.BeamDurationBeats;
        float extension = Mathf.Clamp01(phase / extensionRatio);
        float length = activeForm.BeamLength * Mathf.Max(0.001f, Mathf.SmoothStep(0f, 1f, extension));
        float thickness = activeForm.BeamThickness * Mathf.Max(0.001f, 1f - phase);
        Vector3 parentScale = transform.lossyScale;
        Vector3 spriteSize = beamRenderer.sprite.bounds.size;
        Transform visual = beamRenderer.transform;
        visual.localScale = new Vector3(length / (spriteSize.x * Mathf.Abs(parentScale.x)),
            thickness / (spriteSize.y * Mathf.Abs(parentScale.y)), 1f);
        // 素材のPivot位置にかかわらず、発射位置をビーム左端に固定します。
        Vector3 center = beamRenderer.sprite.bounds.center;
        visual.localPosition = new Vector3(length / (2f * Mathf.Abs(parentScale.x)) - center.x * visual.localScale.x,
            -center.y * visual.localScale.y, 0f);
        beamCollider.size = spriteSize;
        beamCollider.offset = center;
        float flashProgress = Mathf.Clamp01(phase / extensionRatio);
        float diameter = flashSize * Mathf.Lerp(1f, 0.2f, flashProgress);
        Vector3 flashSpriteSize = flashRenderer.sprite.bounds.size;
        muzzleFlash.localScale = new Vector3(diameter / (flashSpriteSize.x * Mathf.Abs(parentScale.x)),
            diameter / (flashSpriteSize.y * Mathf.Abs(parentScale.y)), 1f);
        Color color = flashRenderer.color;
        color.a = 1f - flashProgress;
        flashRenderer.color = color;
        // 物理更新を待たず、今表示している範囲で敵を検査します。
        Physics2D.SyncTransforms();
        hitBox.CollectOverlaps(beamCollider);
    }

    // 通常終了・被弾・形態変更のどれでも表示と判定を閉じ、集計を一度だけ確定します。
    public void End()
    {
        gameObject.SetActive(false);
    }
}

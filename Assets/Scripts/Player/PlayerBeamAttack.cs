using UnityEngine;

// シーンに配置した最大長のビームで瞬間判定し、その後の細まりと発射光を表示します。
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

    // 攻撃開始時に最大範囲を一度だけ判定し、その後は見た目だけを残します。
    public void Begin(PlayerFormDefinition form)
    {
        activeForm = form;
        hitBox.BeginAttack();
        gameObject.SetActive(true);
        beamCollider.enabled = true;
        Sample(0f);

        // 入力直後の最大範囲にいる敵をまとめて検出し、この時点で命中・空振りを確定します。
        Physics2D.SyncTransforms();
        hitBox.CollectOverlaps(beamCollider);
        hitBox.EndAttack();
        beamCollider.enabled = false;
    }

    // 最大長を維持したまま、曲の進行率に合わせてビームの太さと発射光を更新します。
    public void Sample(float progress)
    {
        float phase = Mathf.Clamp01(progress);
        float extensionRatio = activeForm.BeamExtendBeats / activeForm.BeamVisualDurationBeats;
        float length = activeForm.BeamLength;
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
    }

    // 通常終了・被弾・形態変更のどれでも表示を閉じ、Colliderを無効状態に戻します。
    public void End()
    {
        beamCollider.enabled = false;
        gameObject.SetActive(false);
    }
}

using UnityEngine;

// 形態と行動状態から表示を一元管理し、被弾終了による攻撃表示の上書きを防ぎます。
public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string normalState = "PlayerMoveAnimation";
    [SerializeField] private string attackState = "PlayerAttackAnimation";
    [SerializeField] private string damageState = "PlayerDamageAnimation";
    [SerializeField] private string normalClip = "PlayerMoveAnimation";
    [SerializeField] private string attackClip = "PlayerAttackAnimation";
    [SerializeField] private string damageClip = "PlayerDamageAnimation";
    private PlayerFormDefinition displayedForm;
    private int displayedState = -1;

    // 行動が変わった時だけ再生し、形態変更中は動作の進行率を引き継ぎます。
    public void Refresh(PlayerFormDefinition form, bool attacking, bool damaged,
        float attackProgress, float damageProgress, float attackDuration, float damageDuration)
    {
        int state = attacking ? 1 : damaged ? 2 : 0;
        if (displayedForm == form && displayedState == state) return;
        playerAnimator.runtimeAnimatorController = form.Animations;
        displayedForm = form;
        displayedState = state;
        // 被弾点滅の半透明が通常・攻撃表示に残らないよう明示的に戻します。
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
        string stateName = state == 1 ? attackState : state == 2 ? damageState : normalState;
        string clipName = state == 1 ? attackClip : state == 2 ? damageClip : normalClip;
        float duration = state == 1 ? attackDuration : damageDuration;
        playerAnimator.speed = state == 0 ? 1f : form.Animations[clipName].length / duration;
        float progress = state == 1 ? attackProgress : state == 2 ? damageProgress : 0f;
        playerAnimator.Play(stateName, 0, Mathf.Clamp01(progress));
        playerAnimator.Update(0f);
    }

    // 無効化後も同じ状態を正しく再表示できるよう再生情報を破棄します。
    public void ResetPresentation()
    {
        displayedState = -1;
        playerAnimator.speed = 1f;
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
    }
}

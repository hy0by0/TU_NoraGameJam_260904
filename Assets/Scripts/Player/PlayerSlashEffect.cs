using UnityEngine;

// Hierarchyに配置した斬撃アニメーションを、連撃中だけ曲に同期して表示します。
public class PlayerSlashEffect : MonoBehaviour
{
    [SerializeField] private Animator effectAnimator;
    [SerializeField] private string animationState = "SlashLoop";

    // 動的生成せず、配置済みオブジェクトの表示と再生位置だけを更新します。
    public void Show(float cycleProgress)
    {
        gameObject.SetActive(true);
        effectAnimator.speed = 0f;
        effectAnimator.Play(animationState, 0, Mathf.Repeat(cycleProgress, 1f));
        effectAnimator.Update(0f);
    }

    // 正常終了・被弾・形態変更・無効化で、斬撃を確実に消します。
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

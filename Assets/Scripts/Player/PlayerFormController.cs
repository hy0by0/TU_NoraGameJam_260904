using UnityEngine;

// Inspectorで指定した初期形態と、取得後の現在形態を管理します。
public class PlayerFormController : MonoBehaviour
{
    [SerializeField] private PlayerFormDefinition initialForm;
    public PlayerFormDefinition Current { get; private set; }

    // プレイヤーの初期化時に初期形態を適用します。
    public void Initialize()
    {
        Current = initialForm;
    }

    // 同じ設定の再取得では表示や時間をリセットしません。
    public bool ChangeForm(PlayerFormDefinition nextForm)
    {
        if (Current == nextForm) return false;
        Current = nextForm;
        return true;
    }
}

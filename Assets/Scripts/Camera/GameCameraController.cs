using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// ゲーム進行に合わせて4種類のCinemachine Cameraを切り替えるクラスです。
/// </summary>
public class GameCameraController : MonoBehaviour
{
    [Header("Cinemachine Camera参照")]
    [SerializeField] private CinemachineCamera introCamera;
    [SerializeField] private CinemachineCamera gameplayCamera;
    [SerializeField] private CinemachineCamera gameOverCamera;
    [SerializeField] private CinemachineCamera resultCamera;

    [Header("切り替え設定")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority;

    [Header("進行停止")]
    [SerializeField] private StageProgressController stageProgressController;

    public enum CameraMode
    {
        Intro,
        Gameplay,
        GameOver,
        Result
    }

    public CameraMode CurrentMode { get; private set; }

    /// <summary>
    /// シーン開始時はIntroCameraを有効にします。
    /// </summary>
    private void Awake()
    {
        ShowIntro();
    }

    /// <summary>
    /// 導入演出用の固定カメラへ切り替えます。UnityEventから呼び出せます。
    /// </summary>
    public void ShowIntro()
    {
        SwitchTo(CameraMode.Intro, introCamera);
    }

    /// <summary>
    /// ステージ進行へ追従するカメラへ切り替えます。UnityEventから呼び出せます。
    /// </summary>
    public void ShowGameplay()
    {
        SwitchTo(CameraMode.Gameplay, gameplayCamera);
    }

    /// <summary>
    /// 進行距離を固定し、ゲームオーバー用カメラへ切り替えます。UnityEventから呼び出せます。
    /// </summary>
    public void ShowGameOver()
    {
        stageProgressController.FreezeProgress();
        SwitchTo(CameraMode.GameOver, gameOverCamera);
    }

    /// <summary>
    /// 進行距離を固定し、リザルト用カメラへ切り替えます。UnityEventから呼び出せます。
    /// </summary>
    public void ShowResult()
    {
        stageProgressController.FreezeProgress();
        SwitchTo(CameraMode.Result, resultCamera);
    }

    /// <summary>
    /// 指定したカメラだけを最優先にします。
    /// </summary>
    private void SwitchTo(CameraMode mode, CinemachineCamera activeCamera)
    {
        introCamera.Priority = inactivePriority;
        gameplayCamera.Priority = inactivePriority;
        gameOverCamera.Priority = inactivePriority;
        resultCamera.Priority = inactivePriority;

        activeCamera.Priority = activePriority;
        CurrentMode = mode;
    }
}

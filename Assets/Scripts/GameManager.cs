using UnityEngine;

/// <summary>
/// MainSceneの開始処理を管理するクラスです。
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private MusicConductor musicConductor;

    /// <summary>
    /// BGMの予約再生を開始します。実際のゲーム開始時刻はMusicConductorが管理します。
    /// </summary>
    private void Start()
    {
        musicConductor.PlayMusic();
    }
}

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// StageProgressControllerの開始拍とSongDefinitionを使って、シーン上に拍位置のガイドを表示するクラスです。
/// </summary>
public class BeatGrid : MonoBehaviour
{
    [Header("進行と曲の設定")]
    [SerializeField] private StageProgressController stageProgressController;
    [SerializeField] private SongDefinition songDefinition;

    [Header("グリッド表示")]
    public int beatsPerBar = 4;
    public int drawBeatCount = 64;

    /// <summary>
    /// 指定した拍位置をワールドX座標へ変換します。
    /// </summary>
    public float BeatToX(float beat)
    {
        return stageProgressController.BeatToWorldX(beat);
    }

    /// <summary>
    /// 指定したワールドX座標を拍位置へ変換します。
    /// </summary>
    public float XToBeat(float x)
    {
        return (float)stageProgressController.WorldXToBeat(x);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Sceneビューへ拍線と小節番号を描画します。
    /// </summary>
    private void OnDrawGizmos()
    {
        beatsPerBar = songDefinition.BeatsPerBar;

        for (int beat = 0; beat <= drawBeatCount; beat++)
        {
            float x = BeatToX(beat);

            Vector3 bottom = new Vector3(x, -10f, 0f);
            Vector3 top = new Vector3(x, 10f, 0f);

            Handles.DrawLine(bottom, top);

            int bar = beat / beatsPerBar + 1;
            int beatInBar = beat % beatsPerBar + 1;

            Handles.Label(top, $"{bar}:{beatInBar}");
        }
    }
#endif
}

using UnityEditor;
using UnityEngine;

/// <summary>
/// BeatTimingを、1小節目・1拍目・1分割目から指定できる日本語表示にするInspector拡張です。
/// </summary>
[CustomPropertyDrawer(typeof(BeatTiming))]
public class BeatTimingDrawer : PropertyDrawer
{
    private const float LineCount = 3f;

    /// <summary>
    /// BeatTimingの3項目を、すべて1始まりであることが分かるラベルで描画します。
    /// </summary>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(lineRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(lineRect, property.FindPropertyRelative("bar"), new GUIContent("小節（1始まり）"));
            lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(lineRect, property.FindPropertyRelative("beat"), new GUIContent("拍（1始まり）"));
            lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(lineRect, property.FindPropertyRelative("subdivision"), new GUIContent("拍内分割位置（1始まり）"));
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    /// <summary>
    /// 折りたたみ状態に合わせてInspector上の高さを返します。
    /// </summary>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        return EditorGUIUtility.singleLineHeight * (LineCount + 1f)
            + EditorGUIUtility.standardVerticalSpacing * LineCount;
    }
}

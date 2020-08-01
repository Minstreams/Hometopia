using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinsHeaderAttribute))]
public class MinsHeaderDrawer : DecoratorDrawer
{
    private MinsHeaderAttribute sa {get{return attribute as MinsHeaderAttribute; } }
    public override float GetHeight()
    {
        GUIStyle style = sa.Style;
        return style.CalcSize(new GUIContent(sa.Summary)).y;
    }
    public override void OnGUI(Rect position)
    {
        GUIContent summary = new GUIContent(sa.Summary);
        GUIStyle style = sa.Style;
        var h = style.CalcHeight(summary, position.width);
        Rect headerRect = new Rect(position.x, position.y, position.width, h);
        EditorGUI.LabelField(headerRect, summary, style);
    }
}

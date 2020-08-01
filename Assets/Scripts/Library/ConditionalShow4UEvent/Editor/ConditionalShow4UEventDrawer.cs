using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalShow4UEventAttribute))]
public class ConditionalShow4UEventDrawer : UnityEditorInternal.UnityEventDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (IsConditionMet(property))
        {
            //条件满足，开始绘制
            bool wasEnabled = GUI.enabled;
            GUI.enabled = true;
            base.OnGUI(position, property, label);
            GUI.enabled = wasEnabled;
        }
    }
    private bool IsConditionMet(SerializedProperty property)
    {
        ConditionalShow4UEventAttribute condSAtt = (ConditionalShow4UEventAttribute)attribute;
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(condSAtt.ConditionalIntField);
        if (sourcePropertyValue == null)
        {
            Debug.LogWarning("ConditionalShow4UEventAttribute 指向了一个不存在的条件字段: " + condSAtt.ConditionalIntField);
            return false;
        }
        for(int i = 0; i < condSAtt.ExpectedValues.Length; i++)
        {
            if (condSAtt.ExpectedValues[i] == sourcePropertyValue.intValue) return true;
        }
        return false;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (IsConditionMet(property)) return base.GetPropertyHeight(property, label);
        return -EditorGUIUtility.standardVerticalSpacing;
    }
}

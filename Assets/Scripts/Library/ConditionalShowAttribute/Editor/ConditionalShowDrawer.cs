using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalShowAttribute))]
public class ConditionalShowDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (IsConditionMet(property))
        {
            //条件满足，开始绘制
            bool wasEnabled = GUI.enabled;
            GUI.enabled = true;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = wasEnabled;
        }
    }
    private bool IsConditionMet(SerializedProperty property)
    {
        ConditionalShowAttribute condSAtt = (ConditionalShowAttribute)attribute;
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(condSAtt.ConditionalIntField);
        if (sourcePropertyValue == null)
        {
            Debug.LogWarning("ConditionalShowAttribute 指向了一个不存在的条件字段: " + condSAtt.ConditionalIntField);
            return false;
        }
        return condSAtt.ExpectedValue == sourcePropertyValue.intValue;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (IsConditionMet(property)) return EditorGUI.GetPropertyHeight(property, label);
        return -EditorGUIUtility.standardVerticalSpacing;
    }
}

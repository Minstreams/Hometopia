using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class EnumMapDrawer<ET> : PropertyDrawer
{
    private bool enabled = false;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var list = GetList(property);
        int count = list.arraySize;
        return enabled ? (count == 0 ? 2 : count + 1) * EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight;
    }

    private SerializedProperty GetList(SerializedProperty property)
    {
        var count = System.Enum.GetNames(typeof(ET)).Length;
        var list = property.FindPropertyRelative("list");
        while (list.arraySize < count)
        {
            list.InsertArrayElementAtIndex(0);
        }
        return list;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position.height = EditorGUIUtility.singleLineHeight;
        enabled = EditorGUI.Foldout(position, enabled, label);

        if (enabled)
        {
            var list = GetList(property);
            int count = list.arraySize;
            if (count == 0)
            {
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(position, "Empty...");
            }
            else
            {
                var enums = System.Enum.GetNames(typeof(ET));
                for (int i = 0; i < enums.Length; ++i)
                {
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(position, list.GetArrayElementAtIndex(i), new GUIContent(enums[i]));
                }
            }
        }
        EditorGUI.EndProperty();
    }
}

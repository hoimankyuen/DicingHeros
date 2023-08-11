using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RangeFloat2))]
public class RangeFloat2PropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty minProp = property.FindPropertyRelative("min");
        SerializedProperty maxProp = property.FindPropertyRelative("max");

        Rect pos = EditorGUI.PrefixLabel(position, label);
        Rect labelPos = pos;
        labelPos.width = 28f;
        Rect fieldPos = pos;
        fieldPos.width = fieldPos.width * 0.5f - labelPos.width - 5f;
        fieldPos.x += labelPos.width;

        EditorGUI.LabelField(labelPos, new GUIContent("Min"));
        minProp.floatValue = EditorGUI.FloatField(fieldPos, minProp.floatValue);

        labelPos.x += pos.width * 0.5f;
        fieldPos.x += pos.width * 0.5f;
        fieldPos.x += 5f;

        EditorGUI.LabelField(labelPos, new GUIContent("Max"));
        maxProp.floatValue = EditorGUI.FloatField(fieldPos, maxProp.floatValue);

        EditorGUI.EndProperty();
    }
}
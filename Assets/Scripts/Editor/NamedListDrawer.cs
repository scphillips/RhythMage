// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, January 2022

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(NamedListAttribute))]
public class NamedListDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        try
        {
            EditorGUI.BeginProperty(rect, label, property);

            // Draw label
            GUIContent directionLabel = new GUIContent(((RhythMage.Direction)property.FindPropertyRelative("direction").enumValueIndex).ToString());
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), directionLabel);
            
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("gameObject"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
        catch
        {
        }
    }
}

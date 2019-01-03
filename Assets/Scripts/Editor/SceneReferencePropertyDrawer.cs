using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferencePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var sceneAssetProperty = property.FindPropertyRelative("m_sceneAsset");
        
        EditorGUI.BeginProperty(position, GUIContent.none, property);
        EditorGUI.BeginChangeCheck();
        var selectedObject = EditorGUI.ObjectField(position, label, sceneAssetProperty.objectReferenceValue, typeof(SceneAsset), false);

        if (EditorGUI.EndChangeCheck())
        {
            sceneAssetProperty.objectReferenceValue = selectedObject;

            // If no valid scene asset was selected, reset the stored path accordingly
            if (selectedObject == null)
            {
                var scenePath = property.FindPropertyRelative("m_scenePath");
                scenePath.stringValue = string.Empty;
            }
        }

        EditorGUI.EndProperty();
    }
}

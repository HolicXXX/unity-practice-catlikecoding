using UnityEngine;
using UnityEditor;

namespace ObjectManagement
{
    [CustomPropertyDrawer(typeof(FloatRange))]
    [CanEditMultipleObjects]
    public class FloatRangeDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int originIndentLevel = EditorGUI.indentLevel;
            float originLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            position.width /= 2f;
            EditorGUIUtility.labelWidth = position.width / 2f;
            EditorGUI.indentLevel = 1;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("min"));
            position.x += position.width;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("max"));
            EditorGUI.EndProperty();

            EditorGUI.indentLevel = originIndentLevel;
            EditorGUIUtility.labelWidth = originLabelWidth;
        }
    }
}

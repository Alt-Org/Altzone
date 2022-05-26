using Prg.Scripts.Common.Unity.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Editors
{
    /// <summary>
    /// Property drawer for <c>ReadOnly</c> attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            object propValue = null;
            if (EditorApplication.isPlaying)
            {
                var targetObject = property.serializedObject.targetObject;
                var field = targetObject.GetType().GetField(property.propertyPath);
                propValue = field.GetValue(targetObject);
            }
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.LabelField(position, $"{propValue}");
        }
    }
}
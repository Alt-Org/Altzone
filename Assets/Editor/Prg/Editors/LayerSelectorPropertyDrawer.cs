using Prg.Scripts.Common.Unity;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Editors
{
    /// <summary>
    /// Property drawer for <c>LayerSelector</c> attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(LayerSelectorAttribute))]
    public class LayerSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.BeginProperty(position, label, property);

                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                property.intValue = EditorGUI.LayerField(position, property.intValue);

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}
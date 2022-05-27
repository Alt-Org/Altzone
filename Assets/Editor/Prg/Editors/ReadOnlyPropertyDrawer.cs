using System;
using System.Reflection;
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
        private const BindingFlags AllowFieldAccess = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            object propValue = null;
            if (EditorApplication.isPlaying)
            {
                try
                {
                    var targetObject = property.serializedObject.targetObject;
                    var field = targetObject.GetType().GetField(property.propertyPath, AllowFieldAccess);
                    propValue = field?.GetValue(targetObject) ?? "?";
                }
                catch (Exception e)
                {
                    propValue = e.Message;
                }
            }
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.LabelField(position, $"{propValue}");
        }
    }
}
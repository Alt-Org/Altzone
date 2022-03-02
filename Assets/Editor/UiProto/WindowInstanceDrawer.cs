using UiProto.Scripts.Window;
using UnityEditor;
using UnityEngine;

namespace Editor.UiProto
{
    [CustomPropertyDrawer(typeof(WindowInstance), true)]
    public class WindowInstanceDrawer : PropertyDrawer
    {
        private const float lines = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            {
                // Draw label
                var windowName = property.FindPropertyRelative("window")?.FindPropertyRelative("windowId")?.intValue.ToString();
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(windowName));

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                {
                    drawProperty(position, property);
                }
                // Set indent back to what it was
                EditorGUI.indentLevel = indent;
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // This is two line property
            return lines * (base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing);
        }
        private static void drawProperty(Rect position, SerializedProperty property)
        {
            // Calculate rects
            var lineWidth = position.width;
            var lineHeight = position.height / lines;
            var line1 = new Rect(position.x, position.y + 0f * lineHeight, lineWidth, lineHeight);
            var line2 = new Rect(position.x, position.y + 1f * lineHeight, lineWidth, lineHeight);

            // Draw fields
            EditorGUI.PropertyField(line1, property.FindPropertyRelative("window"), GUIContent.none);
            EditorGUI.PropertyField(line2, property.FindPropertyRelative("windowTemplate"), GUIContent.none);
        }
    }
}

using Editor.Prg;
using System.Collections.Generic;
using System.Linq;
using Editor.Prg.Editors;
using UiProto.Scripts.Window;
using UnityEditor;
using UnityEngine;

namespace Editor.UiProto
{
    [CustomPropertyDrawer(typeof(WindowId), true)]
    public class WindowIdDrawer : PropertyDrawer
    {
        private static List<WindowIdDef> windows;
        private static string[] windowNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            windows = WindowNames.loadWindowNameDefs();
            windows.Sort((a, b) => EditorCultureInfo.SortComparer.Compare(a.windowName, b.windowName));
            windowNames = windows.Select(x => x.windowName).ToArray();

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            {
                // Draw label
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

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
            // This is one line property
            return 1f * (base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing);
        }

        private static void drawProperty(Rect position, SerializedProperty property)
        {
            // Calculate rects
            var lineWidth = position.width;
            var lineHeight = position.height;
            var line1 = new Rect(position.x, position.y, lineWidth, lineHeight);

            var windowIdProp = property.FindPropertyRelative("windowId");

            var windowId = windowIdProp.intValue;

            var itemIndex = windows.FindIndex(x => x.windowId == windowId);
            if (itemIndex == -1)
            {
                windowNames = addItem(windowNames, $"< {windowId} < Invalid");
                itemIndex = windows.FindIndex(x => x.windowId == windowId);
            }
            var newItemIndex = EditorGUI.Popup(line1, itemIndex, windowNames, EditorStyles.popup);
            if (newItemIndex != itemIndex)
            {
                windowIdProp.intValue = windows[newItemIndex].windowId;
            }
        }

        private static T[] addItem<T>(T[] array, T item)
        {
            var list = array.ToList();
            list.Add(item);
            return list.ToArray();
        }
    }
}

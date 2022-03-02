using Editor.Prg;
using System.Collections.Generic;
using System.Linq;
using Editor.Prg.Editors;
using UiProto.Scripts.Window;
using UnityEditor;
using UnityEngine;

namespace Editor.UiProto
{
    [CustomPropertyDrawer(typeof(LevelId), true)]
    public class LevelIdDrawer : PropertyDrawer
    {
        private static readonly GUIContent networkLabel = new GUIContent("Is Network");
        private static readonly GUIContent excludedLabel = new GUIContent("Is Excluded");

        private static List<LevelIdDef> levels;
        private static string[] levelNames;

        private static string format(LevelIdDef x)
        {
            if (x.levelId == 0)
            {
                return $"Current (level) [{x.levelId}]";
            }
            return $"{x.levelName} ({x.unityName}) [{x.levelId}]";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            levels = LevelNames.loadLevelDefs();
            levels.Sort((a, b) => EditorCultureInfo.SortComparer.Compare(a.levelName, b.levelName));
            levelNames = levels.Select(format).ToArray();

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

            var levelIdProp = property.FindPropertyRelative("levelId");

            var levelId = levelIdProp.intValue;

            var itemIndex = levels.FindIndex(x => x.levelId == levelId);
            if (itemIndex == -1)
            {
                var levelName = levelId == 0 ? "current (0)" : $"<< {levelId} <<";
                levelNames = addItem(levelNames, levelName);
                itemIndex = levelNames.Length - 1;
            }
            var newItemIndex = EditorGUI.Popup(line1, itemIndex, levelNames, EditorStyles.popup);
            if (newItemIndex != itemIndex)
            {
                levelIdProp.intValue = levels[newItemIndex].levelId;
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
using Editor.Prg.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UiProto.Scripts.Window;
using UnityEditor;
using UnityEngine;

namespace Editor.UiProto
{
    [CustomPropertyDrawer(typeof(LevelIdDef), true)]
    public class LevelIdDefDrawer : PropertyDrawer
    {
        private static readonly GUIContent networkLabel = new GUIContent("Is Network");
        private static readonly GUIContent excludedLabel = new GUIContent("Is Excluded");

        private const float lines = 4.25f; // This is four line property /with some space between items
        private static string[] unityNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneNames = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes.Where(x => x.enabled))
            {
                // We use just the level name without path and extension, duplicate level names should not be used
                var tokens = scene.path.Split('/');
                var levelName = tokens[tokens.Length - 1].Split('.')[0];
                sceneNames.Add(levelName);
            }
            sceneNames.Sort();
            unityNames = sceneNames.ToArray();

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
            return lines * (base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing);
        }

        private static void drawProperty(Rect position, SerializedProperty property)
        {
            // Calculate rects
            var lineWidth = position.width;
            var halfLine = lineWidth / 2f;
            var lineHeight = position.height / lines;
            var line1 = new Rect(position.x, position.y, lineWidth, lineHeight);
            var line2 = new Rect(position.x, position.y + 1f * lineHeight, lineWidth, lineHeight);
            var line3 = new Rect(position.x, position.y + 2f * lineHeight, lineWidth, lineHeight);
            var line41 = new Rect(position.x, position.y + 3f * lineHeight, halfLine, lineHeight);
            var line42 = new Rect(position.x + halfLine, position.y + 3f * lineHeight, halfLine, lineHeight);

            var levelNameProp = property.FindPropertyRelative("levelName");
            var unityNameProp = property.FindPropertyRelative("unityName");
            var isNetworkProp = property.FindPropertyRelative("isNetwork");
            var isExcludedProp = property.FindPropertyRelative("isExcluded");
            var levelIdProp = property.FindPropertyRelative("levelId");

            var levelName = levelNameProp.stringValue;
            var unityName = unityNameProp.stringValue;
            var isNetwork = isNetworkProp.boolValue;
            var isExcluded = isExcludedProp.boolValue;
            var levelId = levelIdProp.intValue;

            var unityNameIndex = Array.IndexOf(unityNames, unityName);
            if (unityNameIndex == -1)
            {
                var item = new LevelIdDef
                {
                    isExcluded = false,
                    isNetwork = false,
                    levelId = levelId,
                    levelName = levelName,
                    unityName = $"<< {unityName} <<",
                };
                unityNames = addItem(unityNames, item.unityName);
                unityNameIndex = unityNames.Length - 1;
            }

            var newLevelName = EditorGUI.TextField(line1, levelName);
            if (newLevelName != levelName)
            {
                levelNameProp.stringValue = newLevelName;
            }
            var newLevelId = EditorGUI.TextField(line2, levelId.ToString());
            if (newLevelId != levelId.ToString() && int.TryParse(newLevelId, out levelId))
            {
                levelIdProp.intValue = levelId;
            }
            var newItemIndex = EditorGUI.Popup(line3, unityNameIndex, unityNames, EditorStyles.popup);
            if (newItemIndex != unityNameIndex)
            {
                unityNameProp.stringValue = unityNames[newItemIndex];
            }

            var newIsNetwork = EditorGUI.ToggleLeft(line41, networkLabel, isNetwork);
            if (newIsNetwork != isNetwork)
            {
                isNetworkProp.boolValue = newIsNetwork;
            }
            var newIsExcluded = EditorGUI.ToggleLeft(line42, excludedLabel, isExcluded);
            if (newIsExcluded != isExcluded)
            {
                isExcludedProp.boolValue = newIsExcluded;
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
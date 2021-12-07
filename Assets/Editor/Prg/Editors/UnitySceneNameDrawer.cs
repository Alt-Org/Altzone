using System;
using System.Collections.Generic;
using System.Linq;
using Prg.Scripts.Common.Unity;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Editors
{
    [CustomPropertyDrawer(typeof(UnitySceneName), true)]
    public class UnitySceneNameDrawer : PropertyDrawer
    {
        private static List<Tuple<string, bool, int>> sceneList;
        private static string[] sceneDisplayNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            sceneList = new List<Tuple<string, bool, int>>();
            var scenes = EditorBuildSettings.scenes;
            var usedIndex = -1;
            for (var i = 0; i < scenes.Length; ++i)
            {
                var scene = scenes[i];
                // We use just the level name without path and extension, duplicate level names should not be used
                var tokens = scene.path.Split('/');
                var sceneName = tokens[tokens.Length - 1].Split('.')[0];
                var sceneIndex = scene.enabled ? ++usedIndex : -1;
                sceneList.Add(new Tuple<string, bool, int>(sceneName, scene.enabled, sceneIndex));
            }
            sceneList.Sort((a, b) => string.Compare(a.Item1, b.Item1, StringComparison.Ordinal));
            sceneDisplayNames = new string[sceneList.Count];
            for (var i = 0; i < sceneDisplayNames.Length; ++i)
            {
                var tuple = sceneList[i];
                sceneDisplayNames[i] = $"{sceneList[i].Item1} [{tuple.Item3}]";
            }

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

            var sceneNameProp = property.FindPropertyRelative("sceneName");

            var sceneName = sceneNameProp.stringValue;

            var itemIndex = sceneList.FindIndex(x => x.Item1 == sceneName);
            if (itemIndex == -1)
            {
                var levelName = $"<< {sceneName} <<";
                sceneDisplayNames = addItem(sceneDisplayNames, levelName);
                itemIndex = sceneDisplayNames.Length - 1;
            }
            var newItemIndex = EditorGUI.Popup(line1, itemIndex, sceneDisplayNames, EditorStyles.popup);
            if (newItemIndex != itemIndex)
            {
                sceneNameProp.stringValue = sceneList[newItemIndex].Item1;
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
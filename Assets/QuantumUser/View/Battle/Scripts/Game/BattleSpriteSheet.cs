using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Battle.View
{
    [Serializable]
    public struct BattleSpriteSheet
    {
        public Sprite[] Array;
        public Sprite ReferenceSprite;
    }

    [CustomPropertyDrawer(typeof(BattleSpriteSheet))]
    public class BattleSpriteSheetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            //Get BattleSpriteSheet struct values as properties
            SerializedProperty spritePropertyArray = property.FindPropertyRelative("Array");
            SerializedProperty spriteReference = property.FindPropertyRelative("ReferenceSprite");

            if (_spriteIndexRegexPattern == null)
            {
                _spriteIndexRegexPattern = new Regex(@"_([0-9]+)$");
            }

            // UI

            EditorGUI.BeginProperty(rect, label, property);

            EditorGUI.LabelField(rect, string.Format("{0}: {1}", label.text, _currentSpriteSheetPath));

            //Draw property field for reference sprite only when array is empty
            if (spritePropertyArray.arraySize == 0)
            {   
                EditorGUI.BeginChangeCheck();

                spriteReference.objectReferenceValue = null;

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(spriteReference, new GUIContent("Reference Sprite"));
                EditorGUI.indentLevel--;

                if (EditorGUI.EndChangeCheck())
                {
                    HandlePlayerSpriteSheetProperty(property, spritePropertyArray, spriteReference);
                }
            }
            else
            {
                HandlePlayerSpriteSheetProperty(property, spritePropertyArray, spriteReference);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(spritePropertyArray, new GUIContent("Sprites"), true);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        private string _currentSpriteSheetPath = null;
        private Regex _spriteIndexRegexPattern;

        private void HandlePlayerSpriteSheetProperty(SerializedProperty property, SerializedProperty spritePropertyArray, SerializedProperty spriteReference)
        {
            string currentSpriteSheetPathCopy = _currentSpriteSheetPath;
            _currentSpriteSheetPath = null;

            // get spritesheet path
            Sprite firstSprite = (Sprite)spriteReference.objectReferenceValue;
            if (firstSprite == null) return;
            string spriteSheetPath = AssetDatabase.GetAssetPath(firstSprite);

            _currentSpriteSheetPath = currentSpriteSheetPathCopy;

            // check for changes
            if (spriteSheetPath == currentSpriteSheetPathCopy) return;
            _currentSpriteSheetPath = spriteSheetPath;

            // load spritesheet
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath).OfType<Sprite>().ToArray();
            spritePropertyArray.ClearArray();
            int i = 0;
            foreach (Sprite sprite in sprites.OrderBy(sprite =>
            {
                Match match = _spriteIndexRegexPattern.Match(sprite.name);
                if (!match.Success) return 0;
                return int.Parse(match.Groups[1].Value);
            }))
            {
                spritePropertyArray.InsertArrayElementAtIndex(i);
                spritePropertyArray.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
                i++;
            }
        }
    }
}

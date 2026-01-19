
// System usings
using System;
using System.Linq;
using System.Text.RegularExpressions;

// Unity usings
using UnityEditor;
using UnityEngine;

namespace Battle.View
{
    [Serializable]
    public struct BattleSpriteSheet
    {
        public Sprite[] Array;
    }

    [CustomPropertyDrawer(typeof(BattleSpriteSheet))]
    public class BattleSpriteSheetDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            //Get BattleSpriteSheet struct values as properties
            SerializedProperty spritePropertyArray = property.FindPropertyRelative("Array");

            if (_spriteIndexRegexPattern == null)
            {
                _spriteIndexRegexPattern = new Regex(@"_([0-9]+)$");
            }

            // UI

            //Draw property field for reference sprite only when array is empty
            if (spritePropertyArray.arraySize == 0)
            {   
                EditorGUI.BeginChangeCheck();
                _referenceSprite = null;
                _currentSpriteSheetPath = string.Empty;
                EditorGUI.LabelField(rect, string.Format("{0}: {1}", label.text, _currentSpriteSheetPath));
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;
                EditorGUILayout.PrefixLabel("Reference Sprite");
                EditorGUI.indentLevel--;
                _referenceSprite = (Sprite)EditorGUILayout.ObjectField(_referenceSprite, typeof(Sprite), false);
                
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    HandlePlayerSpriteSheetProperty(property, spritePropertyArray, _referenceSprite);
                }
            }
            else
            {
                if (_currentSpriteSheetPath == string.Empty)
                {
                    _currentSpriteSheetPath = GetPath((Sprite)spritePropertyArray.GetArrayElementAtIndex(0).objectReferenceValue);
                }
                EditorGUI.LabelField(rect, string.Format("{0}: {1}", label.text, _currentSpriteSheetPath));
                HandlePlayerSpriteSheetProperty(property, spritePropertyArray, _referenceSprite);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(spritePropertyArray, new GUIContent("Sprites"), true);
                EditorGUI.indentLevel--;
            }
        }

        private string _currentSpriteSheetPath = string.Empty;
        private Regex _spriteIndexRegexPattern;
        private Sprite _referenceSprite;

        private void HandlePlayerSpriteSheetProperty(SerializedProperty property, SerializedProperty spritePropertyArray, Sprite spriteReference)
        {
            string currentSpriteSheetPathCopy = _currentSpriteSheetPath;
            _currentSpriteSheetPath = string.Empty;

            // get spritesheet path
            if (spriteReference == null) return;
            string spriteSheetPath = GetPath(spriteReference);

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
        private string GetPath(Sprite sprite)
        {
            return AssetDatabase.GetAssetPath(sprite);
        }
    }
}

///@file BattleSpriteSheet.cs
///<summary>
///Contains @cref {Battle.View,BattleSpriteSheetDrawer} class which handles drawing a custom
///inspector for the BattleSpriteSheet struct.
///</summary>

// System usings
using System;
using System.Linq;
using System.Text.RegularExpressions;

// Unity usings
using UnityEditor;
using UnityEngine;

namespace Battle.View
{
    public interface IBattleSpriteSheetMap
    {
        public int GetIndex();
    }
    [Serializable]
    public struct BattleSpriteSheet
    {
        public Sprite[] Array;

        public Sprite GetSprite<T>(T SpriteMapValue) where T : IBattleSpriteSheetMap
        {
            int spriteIndex = SpriteMapValue.GetIndex();
            return Array[spriteIndex];
        }
    }

    [CustomPropertyDrawer(typeof(BattleSpriteSheet))]
    /// <summary>
    /// Handles drawing a custom inspector property for the BattleSpriteSheet struct.
    /// </summary>
    public class BattleSpriteSheetDrawer : PropertyDrawer
    {
        /// <summary>
        /// Override method that handles drawing the custom inspector property.
        /// </summary>
        /// <param name="rect">rect parameter</param>
        /// <param name="property">SerializedProperty parameter</param>
        /// <param name="label">GUIContent parameter</param>
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
                    HandlePlayerSpriteSheetProperty(spritePropertyArray, _referenceSprite);
                }
            }
            else
            {
                if (_currentSpriteSheetPath == string.Empty)
                {
                    _currentSpriteSheetPath = GetPath((Sprite)spritePropertyArray.GetArrayElementAtIndex(0).objectReferenceValue);
                }
                EditorGUI.LabelField(rect, string.Format("{0}: {1}", label.text, _currentSpriteSheetPath));
                HandlePlayerSpriteSheetProperty(spritePropertyArray, _referenceSprite);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(spritePropertyArray, new GUIContent("Sprites"), true);
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// A string that includes the path for the used spritesheet, empty by default.
        /// </summary>
        private string _currentSpriteSheetPath = string.Empty;

        /// <summary>
        /// Regex for sorting the spritesheet.
        /// </summary>
        private Regex _spriteIndexRegexPattern;

        /// <summary>
        /// Reference sprite to get the rest of the sprites included in the same spritesheet
        /// </summary>
        private Sprite _referenceSprite;

        /// <summary>
        /// Method that handles loading the spritesheet from the reference sprite and sorting it
        /// </summary>
        /// <param name="property"></param>
        /// <param name="spritePropertyArray"></param>
        /// <param name="spriteReference"></param>
        private void HandlePlayerSpriteSheetProperty(SerializedProperty spritePropertyArray, Sprite spriteReference)
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

        /// <summary>
        /// Helper method for getting the path a sprite is at.
        /// </summary>
        /// <param name="sprite">Sprite to get the path from.</param>
        /// <returns>Path</returns>
        private string GetPath(Sprite sprite)
        {
            return AssetDatabase.GetAssetPath(sprite);
        }
    }
}

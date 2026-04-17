/// @file BattleSpriteSheet.cs
/// <summary>
/// Contains @cref{Battle.View,BattleSpriteSheetDrawer} class which handles drawing a custom
/// inspector for the BattleSpriteSheet struct.
/// </summary>

// System usings
using System;
using System.Linq;
using System.Text.RegularExpressions;

// Unity usings
using UnityEngine;
using Battle.QSimulation;
using UnityEngine.Serialization;


// Unity editor using
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Battle.View
{
    /// <summary>
    /// Interface that a BattleSpriteSheetMap should implement.
    /// </summary>
    public interface IBattleSpriteSheetMap
    {
        /// <summary>
        /// Method for getting an index to get the correct sprite from the BattleSpriteSheet.
        /// </summary>
        ///
        /// <returns>An index for the BattleSpriteSheet.</returns>
        public int GetIndex();

        /// <summary>
        /// Protected helper method for checking the spritesheet's <paramref name="count"/> with the %SpriteSheetMap's <paramref name="expectedCount"/>.
        /// </summary>
        ///
        /// <param name="expectedCount">Expected count of the %SpriteSheetMap.</param>
        /// <param name="count">Count of the given spritesheet.</param>
        ///
        /// <returns>True if the given parameters match.</returns>
        protected static bool ValidateCount(int expectedCount, int count)
        {
            if (count != expectedCount)
            {
                if(count == 0) return false;
                BattleDebugLogger.ErrorFormat(nameof(IBattleSpriteSheetMap), "Invalid number of sprites in spriteSheet\nCount: {0}\nExpected count: {1}",
                    count, expectedCount
                );
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Struct that holds the Spritesheet array and handles getting a sprite from it based on a %SpriteSheetMap.
    /// </summary>
    [Serializable]
    public struct BattleSpriteSheet
    {
        /// <summary>Internal array that holds the spritesheet.</summary>
        [SerializeField] private Sprite[] _array;

        /// <summary>The number of sprites in this spritesheet.</summary>
        public readonly int Count => _array.Length;

        /// <summary>
        /// Fetches a sprite from the spritesheet at <paramref name="spriteMapValue"/> using given %SpriteSheetMap <typeparamref name="T"/>.
        /// </summary>
        ///
        /// See [{BattleSpriteSheetMap}](#page-concepts-battle-sprite-sheet-sprite-sheet-map) for more info.
        ///
        /// <typeparam name="T">%SpriteSheetMap used to fetch a sprite.</typeparam>
        /// <param name="spriteMapValue">MapValue of the desired sprite.</param>
        ///
        /// <returns>The fetched sprite from the spritesheet.</returns>
        public readonly Sprite GetSprite<T>(T spriteMapValue) where T : IBattleSpriteSheetMap
        {
            int spriteIndex = spriteMapValue.GetIndex();
            return _array[spriteIndex];
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Handles drawing a custom inspector property for the BattleSpriteSheet struct.
    /// </summary>
    ///
    /// See [{BattleSpriteSheet}](#page-concepts-battle-sprite-sheet-sprite-sheet) for more info.
    [CustomPropertyDrawer(typeof(BattleSpriteSheet))]
    public class BattleSpriteSheetDrawer : PropertyDrawer
    {
        /// <summary>
        /// Override method that handles drawing the custom inspector property.
        /// </summary>
        ///
        /// <param name="rect">Rectangle on the screen where the property is drawn.</param>
        /// <param name="property">Property that is drawn.</param>
        /// <param name="label">Label of the drawn property.</param>
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            // Get BattleSpriteSheet struct values as properties
            SerializedProperty spritePropertyArray = property.FindPropertyRelative("_array");

            // UI

            // Draw property field for reference sprite only when array is empty
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
                    _currentSpriteSheetPath = AssetDatabase.GetAssetPath((Sprite)spritePropertyArray.GetArrayElementAtIndex(0).objectReferenceValue);
                }
                EditorGUI.LabelField(rect, string.Format("{0}: {1}", label.text, _currentSpriteSheetPath));
                HandlePlayerSpriteSheetProperty(spritePropertyArray, _referenceSprite);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(spritePropertyArray, new GUIContent("Sprites"), true);
                EditorGUI.indentLevel--;
            }

            // UI
        }

        /// <summary>
        /// A string that includes the path for the used spritesheet, empty by default.
        /// </summary>
        private string _currentSpriteSheetPath = string.Empty;

        /// <summary>
        /// Regex for extracting index from sprite name.
        /// </summary>
        private static readonly Regex s_spriteIndexRegexPattern = new(@"_([0-9]+)$");

        /// <summary>
        /// Reference sprite to get the rest of the sprites included in the same spritesheet
        /// </summary>
        private Sprite _referenceSprite;

        /// <summary>
        /// Method that handles loading the spritesheet from the reference sprite and sorting it
        /// </summary>
        ///
        /// <param name="spritePropertyArray">Serialized property that holds sprite array.</param>
        /// <param name="spriteReference">Reference sprite.</param>
        private void HandlePlayerSpriteSheetProperty(SerializedProperty spritePropertyArray, Sprite spriteReference)
        {
            string currentSpriteSheetPathCopy = _currentSpriteSheetPath;
            _currentSpriteSheetPath = string.Empty;

            // get spritesheet path
            if (spriteReference == null) return;
            string spriteSheetPath = AssetDatabase.GetAssetPath(spriteReference);

            _currentSpriteSheetPath = currentSpriteSheetPathCopy;

            // check for changes
            if (spriteSheetPath == currentSpriteSheetPathCopy) return;
            _currentSpriteSheetPath = spriteSheetPath;

            // load spritesheet
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath).OfType<Sprite>().ToArray();
            spritePropertyArray.arraySize = sprites.Length;
            int i = 0;
            foreach (Sprite sprite in sprites.OrderBy(sprite =>
            {
                Match match = s_spriteIndexRegexPattern.Match(sprite.name);
                if (!match.Success) return 0;
                return int.Parse(match.Groups[1].Value);
            }))
            {
                spritePropertyArray.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
                i++;
            }
        }
    }
#endif
}

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Prg.Scripts.Common.PubSub;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Assertions;

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
            SerializedProperty spritePropertyArray = property.FindPropertyRelative("Array");
            SerializedProperty spriteReference = property.FindPropertyRelative("ReferenceSprite");

            if (_spriteIndexRegexPattern == null)
            {
                _spriteIndexRegexPattern = new Regex(@"_([0-9]+)$");
            }

            // UI

            EditorGUI.BeginProperty(rect, label, property);

            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, string.Format("{0}: {1}", label.text, _currentSpriteSheetPath), true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            if (spritePropertyArray.arraySize == 0)
            {   
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(spriteReference, new GUIContent("Reference Sprite"));

                if (EditorGUI.EndChangeCheck())
                {
                    HandleTestPlayerSpriteSheetProperty(property, spritePropertyArray, spriteReference);
                }
            }
            else
            {
                HandleTestPlayerSpriteSheetProperty(property, spritePropertyArray, spriteReference);
            }
            EditorGUILayout.PropertyField(spritePropertyArray, new GUIContent("Sprites"), true);
            EditorGUI.EndProperty();
        }

        private string _currentSpriteSheetPath = "";
        private Regex _spriteIndexRegexPattern;

        public static T GetObjectReferenceValue<T>(string name, SerializedProperty property)
        {
            Assert.IsTrue(property.propertyType == SerializedPropertyType.ObjectReference, string.Format("Property \"{0}\" is not {1}", name, nameof(SerializedPropertyType.ObjectReference)));
            object value = property.objectReferenceValue;
            if (value == null) return default;
            Assert.IsTrue(value.GetType() == typeof(T), string.Format("Property \"{0}\" is not type of \"{1}\"", name, typeof(T)));
            return (T)value;
        }

        private void HandleTestPlayerSpriteSheetProperty(SerializedProperty property, SerializedProperty spritePropertyArray, SerializedProperty spriteReference)
        {
            string currentSpriteSheetPathCopy = _currentSpriteSheetPath;
            _currentSpriteSheetPath = null;

            // get spritesheet path
            Sprite firstSprite = GetObjectReferenceValue<Sprite>(property.name, spriteReference);
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
                Debug.Log(sprite.name);
                Debug.Log(match);
                if (!match.Success) return 0;
                return int.Parse(match.Groups[1].Value);
            }))
            {
                spritePropertyArray.InsertArrayElementAtIndex(i);
                spritePropertyArray.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
                i++;
            }
            spriteReference.objectReferenceValue = null;
        }
    }
}

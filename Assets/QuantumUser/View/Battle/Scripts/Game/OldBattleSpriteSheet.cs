using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.View
{

    public class BattleSpriteSheetDrawerOld : Editor
    {
        CustomSerializedProperty playerSpriteSheetProperty = new CustomSerializedProperty("_playerSpriteSheet");

        void OnEnable()
        {
            playerSpriteSheetProperty.Load(serializedObject);
            _spriteIndexRegexPattern = new Regex(@"_([0-9]+)$");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            HandleTestPlayerSpriteSheetProperty();
            serializedObject.ApplyModifiedProperties();
            DrawDefaultInspector();
        }

        private string _currentSpriteSheetPath = null;
        private Regex _spriteIndexRegexPattern;

        private struct CustomSerializedProperty
        {
            public string Name;
            public SerializedProperty Property;

            public static T GetObjectReferenceValue<T>(string name, SerializedProperty property)
            {
                Assert.IsTrue(property.propertyType == SerializedPropertyType.ObjectReference, string.Format("Property \"{0}\" is not {1}", name, nameof(SerializedPropertyType.ObjectReference)));
                object value = property.objectReferenceValue;
                if (value == null) return default;
                Assert.IsTrue(value.GetType() == typeof(T), string.Format("Property \"{0}\" is not type of \"{1}\"", name, typeof(T)));
                return (T)value;
            }
            public CustomSerializedProperty(string name)
            {
                Name = name;
                Property = null;
            }
            public void Load(SerializedObject @object)
            {
                Property = @object.FindProperty(Name);
            }
        }
        private void HandleTestPlayerSpriteSheetProperty()
        {
            // invalid type handling
            Assert.IsTrue(playerSpriteSheetProperty.Property.isArray, string.Format("Property \"{0}\" is not an array", playerSpriteSheetProperty.Name));

            string currentSpriteSheetPathCopy = _currentSpriteSheetPath;
            _currentSpriteSheetPath = null;

            // check size
            if (playerSpriteSheetProperty.Property.arraySize < 1) return;

            // get spritesheet path
            SerializedProperty firstSpriteProperty = playerSpriteSheetProperty.Property.GetArrayElementAtIndex(0);
            Sprite firstSprite = CustomSerializedProperty.GetObjectReferenceValue<Sprite>(playerSpriteSheetProperty.Name, firstSpriteProperty);
            if (firstSprite == null) return;
            string spriteSheetPath = AssetDatabase.GetAssetPath(firstSprite);

            _currentSpriteSheetPath = currentSpriteSheetPathCopy;

            // check for changes
            if (spriteSheetPath == currentSpriteSheetPathCopy) return;
            _currentSpriteSheetPath = spriteSheetPath;

            // load spritesheet
            Sprite[] spriteSheet = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath).OfType<Sprite>().ToArray();
            playerSpriteSheetProperty.Property.ClearArray();
            int i = 0;
            foreach (Sprite sprite in spriteSheet.OrderBy(sprite =>
            {
                Match match = _spriteIndexRegexPattern.Match(sprite.name);
                Debug.Log(sprite.name);
                Debug.Log(match);
                if (!match.Success) return 0;
                return int.Parse(match.Groups[1].Value);
            }))
            {
                playerSpriteSheetProperty.Property.InsertArrayElementAtIndex(i);
                playerSpriteSheetProperty.Property.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
                i++;
            }
        }
    }
}

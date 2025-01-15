using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Game;
using UnityEditor;
using UnityEngine;

namespace Altzone.Scripts.Config.ScriptableObjects
{
    /// <summary>
    /// Altzone game player character 'specification' instance and references to all in-game resources attached to it.
    /// </summary>
    [CreateAssetMenu(menuName = "ALT-Zone/CharacterSpec", fileName = nameof(CharacterSpec) + "_ID")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CharacterSpec : ScriptableObject
    {
        /// <summary>
        /// Character id, specified externally.
        /// </summary>
        [Header("Character Basic Data"), ReadOnly] public string Id;

        public CharacterID CharacterId;

        /// <summary>
        /// Character name.
        /// </summary>
        /// <remarks>
        /// When game support localization this will be localization id for this player character.
        /// </remarks>
        public string Name;

        /// <summary>
        /// Is this player character approved for production.
        /// </summary>
        public bool IsApproved;

        /// <summary>
        /// Player character class.
        /// </summary>
        public CharacterClassID ClassType;

        /// <summary>
        /// Gallery image for something.
        /// TODO: add relevant doc comment here!
        /// </summary>
        [Header("Menu UI")] public Sprite GalleryImage;

        /// <summary>
        /// Battle sprite sheet for something.
        /// TODO: add relevant doc comment here!
        /// </summary>
        [Header("Battle Graphics")] public Sprite BattleSprite;

        public override string ToString()
        {
            return $"{Id}:{ClassType}:{Name}" +
                   $"-{ResName(nameof(GalleryImage), GalleryImage)}" +
                   $"-{ResName(nameof(BattleSprite), BattleSprite)}";

            string ResName(string instanceName, Object instance) =>
                $"{(instance == null ? "null" : instance.name)}";
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CharacterSpec))]
    public class CharacterSpecEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ReadOnlyCollection<CharacterSpec> characters= (ReadOnlyCollection<CharacterSpec>)PlayerCharacters.Characters;

            CharacterSpec script = (CharacterSpec)target;

            script.Id = ((int)script.CharacterId).ToString();

            /*if (PlayerCharacters.GetCharacter(((int)script.CharacterId).ToString()) == null)
            {
                script.Id = ((int)script.CharacterId).ToString();
            }
            else
            {
                script.CharacterId = 0;
            }*/
        }
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif
}

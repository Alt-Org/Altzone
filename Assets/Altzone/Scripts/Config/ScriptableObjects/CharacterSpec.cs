using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Game;
using UnityEditor;
using UnityEngine;

namespace Altzone.Scripts.Config.ScriptableObjects
{
    /// <summary>
    /// Altzone game player character 'specification' implementation and references to all in-game resources attached to it.<br />
    /// <b>Note</b> that this class should not be edited spontaneously but only using relevant change management process!<br />
    /// See <c>_readme.md</c> and/or related WIKI page for more instructions.
    /// </summary>
    [CreateAssetMenu(menuName = "ALT-Zone/CharacterSpec", fileName = nameof(CharacterSpec) + "_ID")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CharacterSpec : ScriptableObject
    {
        #region Metadata

        /// <summary>
        /// Character id, specified externally.
        /// </summary>
        [Header("Character Basic Data"),] public string Id;

        public CharacterID CharacterId;

        /// <summary>
        /// Is this player character approved for production.
        /// </summary>
        public bool IsApproved;

        #endregion

        #region General attributes

        /// <summary>
        /// Character name.
        /// </summary>
        /// <remarks>
        /// When game support localization this will be localization id for this player character.
        /// </remarks>
        [Header("General Attributes")] public string Name;

        /// <summary>
        /// Player character class.
        /// </summary>
        public CharacterClassID ClassType;

        #endregion

        #region General graphical assets and prefabs

        /// <summary>
        /// Gallery image for something.
        /// TODO: add relevant doc comment here!
        /// </summary>
        [Header("General Graphics")] public Sprite GalleryImage;

        #endregion

        #region Battle attributes

        [Header("Battle Attributes")] public float Hp;
        public float Speed;
        public float Resistance;
        public float Attack;
        public float Defence;

        #endregion

        #region Battle graphical assets and prefabs

        /// <summary>
        /// Battle sprite sheet for something.
        /// TODO: add relevant doc comment here!
        /// </summary>
        [Header("Battle Graphics")] public Sprite BattleSprite;

        #endregion

        public override string ToString()
        {
            return $"{Id}:{ClassType}:{Name}" +
                   $"-{ResName(GalleryImage)}" +
                   $"-{ResName(BattleSprite)}";

            string ResName(Object instance) => $"{(instance == null ? "null" : instance.name)}";
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CharacterSpec))]
    public class CharacterSpecEditor : Editor
    {
        private CharacterID _prevID = CharacterID.None;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ReadOnlyCollection<CharacterSpec> characters =
                (ReadOnlyCollection<CharacterSpec>)PlayerCharacters.Characters;

            CharacterSpec script = (CharacterSpec)target;

            if (_prevID != script.CharacterId)
            {
                _prevID = script.CharacterId;
                script.Id = ((int)script.CharacterId).ToString();
            }

            if (int.TryParse(script.Id, out int result) && System.Enum.IsDefined(typeof(CharacterID), result))
            {
                script.CharacterId = (CharacterID)int.Parse(script.Id);
                _prevID = script.CharacterId;
            }

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

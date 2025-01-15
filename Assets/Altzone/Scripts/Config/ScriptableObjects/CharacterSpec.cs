using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Game;
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
        #region Metadata

        /// <summary>
        /// Character id, specified externally.
        /// </summary>
        [Header("Character Basic Data")] public string Id;

        /// <summary>
        /// Is this player character approved for production.
        /// </summary>
        public bool IsApproved;

        #endregion

        #region Content

        /// <summary>
        /// Character name.
        /// </summary>
        /// <remarks>
        /// When game support localization this will be localization id for this player character.
        /// </remarks>
        public string Name;

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

        #endregion

        public override string ToString()
        {
            return $"{Id}:{ClassType}:{Name}" +
                   $"-{ResName(GalleryImage)}" +
                   $"-{ResName(BattleSprite)}";

            string ResName(Object instance) => $"{(instance == null ? "null" : instance.name)}";
        }
    }
}

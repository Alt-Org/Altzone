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
        /// <summary>
        /// Character id, specified externally.
        /// </summary>
        public string Id;

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

        public override string ToString()
        {
            return $"{Id}:{ClassType}:{Name}";
        }
    }
}

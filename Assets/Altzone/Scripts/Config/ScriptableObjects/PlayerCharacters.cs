using System.Collections.Generic;
using Altzone.Scripts.PlayerCharacter;

namespace Altzone.Scripts.Config.ScriptableObjects
{
    /// <summary>
    /// Common access point for <c>PlayerCharacterPrototype</c>s.
    /// </summary>
    public static class PlayerCharacters
    {
        #region Public (static) API

        /// <summary>
        /// Gets <c>PlayerCharacterPrototype</c> by its id.
        /// </summary>
        /// <param name="id">the character id</param>
        /// <returns>the PlayerCharacterPrototype or null if not found</returns>
        public static PlayerCharacterPrototype GetCharacter(string id) =>
            PlayerCharacterPrototypes.Instance.GetCharacter(id);

        /// <summary>
        /// Lists current (configured) player character prototypes in the game.
        /// </summary>
        public static IEnumerable<PlayerCharacterPrototype> Prototypes => PlayerCharacterPrototypes.Instance.Prototypes;

        #endregion
    }
}

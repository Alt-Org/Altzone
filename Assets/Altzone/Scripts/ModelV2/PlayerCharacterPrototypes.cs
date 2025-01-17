using System.Collections.Generic;

namespace Altzone.Scripts.ModelV2
{
    /// <summary>
    /// Common access point for player character prototypes.
    /// </summary>
    public static class PlayerCharacterPrototypes
    {
        #region Public (static) API

        /// <summary>
        /// Gets <c>PlayerCharacterPrototype</c> by its id.
        /// </summary>
        /// <param name="id">the character id</param>
        /// <returns>the PlayerCharacterPrototype or null if not found</returns>
        public static PlayerCharacterPrototype GetCharacter(string id) =>
            Internal.PlayerCharacterPrototype.Instance.GetCharacter(id);

        /// <summary>
        /// Lists current (configured) player character prototypes in the game.
        /// </summary>
        public static IEnumerable<PlayerCharacterPrototype> Prototypes => Internal.PlayerCharacterPrototype.Instance.Prototypes;

        #endregion
    }
}

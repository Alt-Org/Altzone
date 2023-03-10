using System.Collections.Generic;
using Altzone.Scripts.Model.Poco;

namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Initializes game model objects to a known state for a new player (installation).
    /// </summary>
    internal static class CreateDefaultModels
    {
        /// <summary>
        /// Character classes are permanent and immutable that can be added but never deleted after game has been published.
        /// </summary>
        /// <returns></returns>
        internal static List<CharacterClass> CreateCharacterClasses()
        {
            return new List<CharacterClass>
            {
                new(1, "Koulukiusaaja", Defence.Desensitisation, 3, 9, 7, 3),
                new(2, "Vitsiniekka", Defence.Deflection, 9, 3, 3, 4),
                new(3, "Pappi", Defence.Introjection, 5, 5, 4, 4),
                new(4, "Taiteilija", Defence.Projection, 4, 2, 9, 5),
                new(5, "Hodariläski", Defence.Retroflection, 3, 7, 2, 9),
                new(6, "Älykkö", Defence.Egotism, 6, 2, 6, 5),
                new(7, "Tytöt", Defence.Confluence, 5, 6, 2, 6)
            };
        }

        /// <summary>
        /// Player custom character classes are created by the player itself (or given to the player by the game).<br />
        /// This collection should be the initial set of custom character classes the player has when game is started first time.
        /// </summary>
        /// <returns></returns>
        internal static List<CustomCharacter> CreateCustomCharacters()
        {
            return new List<CustomCharacter>
            {
                new(1, 1, "1", "Desensitisation", 0, 0, 0, 0),
                new(2, 2, "2", "Deflection", 0, 0, 0, 0),
                new(3, 3, "3", "Introjection", 0, 0, 0, 0),
                new(4, 4, "4", "Projection", 0, 0, 0, 0),
                new(5, 5, "5", "Retroflection", 0, 0, 0, 0),
                new(6, 6, "6", "Egotism", 0, 0, 0, 0),
                new(7, 7, "7", "Confluence", 0, 0, 0, 06)
            };
        }
    }
}
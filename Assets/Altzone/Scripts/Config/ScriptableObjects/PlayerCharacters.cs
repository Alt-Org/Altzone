using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Altzone.Scripts.Config.ScriptableObjects
{
    /// <summary>
    /// This (singleton) list (UNITY asset) contains all playable characters in the game (set in UNITY Editor).
    /// </summary>
    //[CreateAssetMenu(menuName = "ALT-Zone/PlayerCharacters", fileName = nameof(PlayerCharacters))]
    public class PlayerCharacters : ScriptableObject
    {
        /// <summary>
        /// Loads <c>PlayerCharacters</c> instance from Resources folder.
        /// </summary>
        /// <remarks>
        /// This instance should be cached inside a method or class.
        /// </remarks>
        /// <returns>the PlayerCharacters instance or null</returns>
        public static PlayerCharacters Get() => Resources.Load<PlayerCharacters>(nameof(PlayerCharacters));

        [SerializeField] private List<CharacterSpec> _characters;

        /// <summary>
        /// Gets <c>CharacterSpec</c> by its id.
        /// </summary>
        /// <param name="id">the character id</param>
        /// <returns>the CharacterSpec or null if not found</returns>
        public CharacterSpec GetCharacter(string id) => Characters.FirstOrDefault(x => x.Id == id);

        public IEnumerable<CharacterSpec> Characters =>
#if UNITY_EDITOR
            _characters.AsReadOnly();
#else
            _characters;
#endif

        [Conditional("UNITY_EDITOR")]
        public static void Verify()
        {
            var playerCharacters = Get();
            var uniqueIds = new HashSet<string>();
            var uniqueNames = new HashSet<string>();
            foreach (var character in playerCharacters.Characters
                         .OrderBy(x => x.Id)
                         .ThenBy(x => x.name))
            {
                Debug.Log($"{character}");
                if (character.ClassType == CharacterClassID.None)
                {
                    Debug.LogError($"invalid ClassType {character}");
                }
                if (!uniqueIds.Add(character.Id))
                {
                    Debug.LogError($"duplicate character ID {character}");
                }
                if (!uniqueNames.Add(character.Name))
                {
                    Debug.LogError($"duplicate character Name {character}");
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Altzone.Scripts.Config.ScriptableObjects
{
    /// <summary>
    /// This (singleton) list (UNITY asset) contains all playable characters in the game (set in UNITY Editor).
    /// </summary>
    /// <remarks>
    /// Note that
    /// </remarks>
    //[CreateAssetMenu(menuName = "ALT-Zone/PlayerCharacters", fileName = nameof(PlayerCharacters))]
    public class PlayerCharacters : ScriptableObject
    {
        #region UNITY Singleton Pattern

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
            _hasInstance = false;
        }

        private static PlayerCharacters _instance;
        private static bool _hasInstance;

        private static PlayerCharacters Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<PlayerCharacters>(nameof(PlayerCharacters));
                    _hasInstance = _instance != null;
                    if (_hasInstance)
                    {
                        var isProduction = !Application.isEditor || _instance._forceProduction;
                        _instance._liveCharacters = isProduction
                            ? _instance._characters
                                .Where(x => x.IsApproved)
                                .ToList()
                                .AsReadOnly()
                            : _instance._characters.AsReadOnly();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Public (static) API

        /// <summary>
        /// Gets <c>CharacterSpec</c> by its id.
        /// </summary>
        /// <param name="id">the character id</param>
        /// <returns>the CharacterSpec or null if not found</returns>
        public static CharacterSpec GetCharacter(string id) => Characters.FirstOrDefault(x => x.Id == id);

        /// <summary>
        /// Current player characters in the game.
        /// </summary>
        public static IEnumerable<CharacterSpec> Characters => Instance._liveCharacters;

        #endregion

        #region ScriptableObject 'singleton' implementation

        [SerializeField, Header("Force Production In Editor")] private bool _forceProduction;

        [SerializeField, Header("All Player Characters")] private List<CharacterSpec> _characters;

        // ReSharper disable once NotAccessedField.Global
        [Header("Current Live Player Characters"), TextArea(10, 100)] public string _liveCharactersText;

        private ReadOnlyCollection<CharacterSpec> _liveCharacters;

        [Conditional("UNITY_EDITOR")]
        public static void Verify()
        {
            var uniqueIds = new HashSet<string>();
            var uniqueNames = new HashSet<string>();
            var builder = new StringBuilder();
            foreach (var character in Characters
                         .OrderBy(x => x.Id)
                         .ThenBy(x => x.name))
            {
                Debug.Log($"{character}", character);
                if (character.ClassType == CharacterClassID.None)
                {
                    Debug.LogError($"invalid ClassType {character}", character);
                    continue;
                }
                if (!uniqueIds.Add(character.Id))
                {
                    Debug.LogError($"duplicate character ID {character}", character);
                    continue;
                }
                if (!uniqueNames.Add(character.Name))
                {
                    Debug.LogError($"duplicate character Name {character}", character);
                    continue;
                }
                builder.AppendLine(character.ToString());
            }
            if (uniqueIds.Count == 0)
            {
                Debug.LogError($"no characters found", _instance);
            }
            builder.AppendLine($"total {uniqueIds.Count}");
            _instance._liveCharactersText = builder.ToString();
        }

        #endregion
    }
}

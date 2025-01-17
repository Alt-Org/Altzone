using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Altzone.Scripts.Config.ScriptableObjects;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Altzone.Scripts.PlayerCharacter
{
    /// <summary>
    /// This (singleton) list (UNITY asset) contains all player character prototype in the game (set in UNITY Editor).<br />
    /// By default only 'approved' player character prototypes are returned.
    /// </summary>
    /// <remarks>
    /// Note that only 'valid' player character prototype are returned.
    /// </remarks>
    //[CreateAssetMenu(menuName = "ALT-Zone/PlayerCharacterPrototypes", fileName = nameof(PlayerCharacterPrototypes))]
    internal class PlayerCharacterPrototypes : ScriptableObject
    {
        #region UNITY Singleton Pattern

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
            _hasInstance = false;
            VerifyCharacterPrototypes();
        }

        private static PlayerCharacterPrototypes _instance;
        private static bool _hasInstance;

        public static PlayerCharacterPrototypes Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<PlayerCharacterPrototypes>(nameof(PlayerCharacterPrototypes));
                    _hasInstance = _instance != null;
                    if (_hasInstance)
                    {
                        _instance._runtimePrototypes = _instance._approvedOnly
                            ? _instance._characters
                                .Where(SelectApproved)
                                .Select(x => new PlayerCharacterPrototype(x))
                                .ToList()
                                .AsReadOnly()
                            : _instance._characters
                                .Where(SelectAll)
                                .Select(x => new PlayerCharacterPrototype(x))
                                .ToList()
                                .AsReadOnly();
                    }
                }
                return _instance;

                bool SelectApproved(CharacterSpec characterSpec) => characterSpec.IsValid && characterSpec.IsApproved;

                bool SelectAll(CharacterSpec characterSpec) => characterSpec.IsValid;
            }
        }

        #endregion

        #region Public getters

        /// <summary>
        /// Gets <c>PlayerCharacterPrototype</c> by its id.
        /// </summary>
        /// <param name="id">the character id</param>
        /// <returns>the PlayerCharacterPrototype or null if not found</returns>
        public PlayerCharacterPrototype GetCharacter(string id) => _runtimePrototypes.FirstOrDefault(x => x.Id == id);

        /// <summary>
        /// Gets current (configured) player character prototypes in the game.
        /// </summary>
        public IEnumerable<PlayerCharacterPrototype> Prototypes => _runtimePrototypes;

        private ReadOnlyCollection<PlayerCharacterPrototype> _runtimePrototypes;

        #endregion

        #region ScriptableObject 'inspector' implementation

        [SerializeField, Header("For Production")] private bool _approvedOnly;

        [SerializeField, Header("All Player Characters")] private List<CharacterSpec> _characters;

        // ReSharper disable once NotAccessedField.Global
        [Header("Current Live Player Characters"), TextArea(10, 100)] public string _liveCharactersText;

        #endregion

        [Conditional("UNITY_EDITOR")]
        private static void VerifyCharacterPrototypes()
        {
            var instance = Instance;
            // Verify.
            var uniqueIds = new HashSet<string>();
            var uniqueNames = new HashSet<string>();
            foreach (var character in instance._characters)
            {
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
                }
            }
            // Get runtime list (sorted).
            var builder =
                new StringBuilder(instance._approvedOnly ? "approved characters" : "all configured characters")
                    .AppendLine();
            foreach (var character in instance._runtimePrototypes
                         .OrderBy(x => x.Id)
                         .ThenBy(x => x.Name))
            {
                builder.AppendLine(character.ToString());
            }
            builder.AppendLine($"character count {uniqueIds.Count}");
            _instance._liveCharactersText = builder.ToString();
        }
    }
}

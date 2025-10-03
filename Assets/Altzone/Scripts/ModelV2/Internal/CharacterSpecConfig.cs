using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Altzone.Scripts.ModelV2.Internal
{
    /// <summary>
    /// This (singleton) list (UNITY asset) contains all player character prototypes in the game (set in UNITY Editor).<br />
    /// By default only 'approved' player character prototypes are returned.<br />
    /// Partially filled (invalid) player character prototypes are ignored always,
    /// associated <c>CharacterSpec</c> knows whether it is valid or not to be included.
    /// </summary>
    //[CreateAssetMenu(menuName = "ALT-Zone/CharacterSpecConfig", fileName = nameof(CharacterSpecConfig))]
    public class CharacterSpecConfig : ScriptableObject
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

        private static CharacterSpecConfig _instance;
        private static bool _hasInstance;

        public static CharacterSpecConfig Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<CharacterSpecConfig>(nameof(CharacterSpecConfig));
                    _hasInstance = _instance != null;
                    if (_hasInstance)
                    {
                        _instance._runtimePrototypes = _instance._approvedOnly
                            ? _instance._characters
                                .Where(SelectApproved)
                                .Select(x => new ModelV2.PlayerCharacterPrototype(x))
                                .ToList()
                                .AsReadOnly()
                            : _instance._characters
                                .Where(SelectAll)
                                .Select(x => new ModelV2.PlayerCharacterPrototype(x))
                                .ToList()
                                .AsReadOnly();
                        _instance._fallBackPrototype = new ModelV2.PlayerCharacterPrototype(_instance._characters.FirstOrDefault(x => x.Id == "0"));
                    }
                }
                return _instance;

                bool SelectApproved(CharacterSpec characterSpec) => characterSpec.IsValid && characterSpec.IsApproved && (!IsTesting(characterSpec) || _instance.AllowTestCharacters);

                bool SelectAll(CharacterSpec characterSpec) => characterSpec.IsValid;

                bool IsTesting(CharacterSpec characterSpec) => (int)characterSpec.CharacterId % 100 == 0;
            }
        }

        #endregion

        #region Public getters

        /// <summary>
        /// Gets <c>PlayerCharacterPrototype</c> by its id.
        /// </summary>
        /// <param name="id">the character id</param>
        /// <returns>the PlayerCharacterPrototype or null if not found</returns>
        public ModelV2.PlayerCharacterPrototype GetCharacter(string id, bool fallback) =>
            _runtimePrototypes.FirstOrDefault(x => x.Id == id) ?? (fallback ? _fallBackPrototype: null);

        /// <summary>
        /// Gets current (configured) player character prototypes in the game.
        /// </summary>
        public IEnumerable<ModelV2.PlayerCharacterPrototype> Prototypes => _runtimePrototypes;

        private ReadOnlyCollection<ModelV2.PlayerCharacterPrototype> _runtimePrototypes;
        private PlayerCharacterPrototype _fallBackPrototype;

        /// <summary>
        /// Gets whether test characters are allowed in the current build.
        /// </summary>
        public bool AllowTestCharacters => _allowTestCharacters && (AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild);

        #endregion

        #region ScriptableObject 'inspector' implementation

        [SerializeField, Header("For Production")] private bool _approvedOnly;

        [SerializeField, Tooltip("Allow Test Characters in test environment (Editor or Dev Build). Production doesn't allow test characters.")] private bool _allowTestCharacters;

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
                if (character.ClassType == CharacterClassType.None && character.CharacterId != CharacterID.Test)
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
                new StringBuilder(instance._approvedOnly
                        ? $"approved characters {instance._runtimePrototypes.Count} (configured {instance._characters.Count})"
                        : $"configured characters {instance._characters}")
                    .AppendLine();
            foreach (var character in instance._runtimePrototypes
                         .OrderBy(x => x.Id)
                         .ThenBy(x => x.Name))
            {
                Debug.Log($"{character}", instance);
                builder.AppendLine(character.ToString());
            }
            _instance._liveCharactersText = builder.ToString();
        }
    }
}

using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(menuName = "ALT-Zone/ClassReference", fileName = "ClassReference")]
    public class ClassReference : ScriptableObject
    {
        [SerializeField] private string _desensitizerNameFinnish;
        [SerializeField] private string _desensitizerNameEnglish;
        [SerializeField] private Color _desensitizerColor;
        [SerializeField] private Color _desensitizerAlternativeColor;
        [SerializeField] private Sprite _desensitizerFrame;
        [SerializeField] private Sprite _desensitizerCornerIcon;
        [SerializeField] private Sprite _desensitizerResistanceIcon;

        [Space, SerializeField] private string _tricksterNameFinnish;
        [SerializeField] private string _tricksterNameEnglish;
        [SerializeField] private Color _tricksterColor;
        [SerializeField] private Color _tricksterAlternativeColor;
        [SerializeField] private Sprite _tricksterFrame;
        [SerializeField] private Sprite _tricksterCornerIcon;
        [SerializeField] private Sprite _tricksterResistanceIcon;

        [Space, SerializeField] private string _obedientNameFinnish;
        [SerializeField] private string _obedientNameEnglish;
        [SerializeField] private Color _obedientColor;
        [SerializeField] private Color _obedientAlternativeColor;
        [SerializeField] private Sprite _obedientFrame;
        [SerializeField] private Sprite _obedientCornerIcon;
        [SerializeField] private Sprite _obedientResistanceIcon;

        [Space, SerializeField] private string _projectorNameFinnish;
        [SerializeField] private string _projectorNameEnglish;
        [SerializeField] private Color _projectorColor;
        [SerializeField] private Color _projectorAlternativeColor;
        [SerializeField] private Sprite _projectorFrame;
        [SerializeField] private Sprite _projectorCornerIcon;
        [SerializeField] private Sprite _projectorResistanceIcon;

        [Space, SerializeField] private string _retroflectorNameFinnish;
        [SerializeField] private string _retroflectorNameEnglish;
        [SerializeField] private Color _retroflectorColor;
        [SerializeField] private Color _retroflectorAlternativeColor;
        [SerializeField] private Sprite _retroflectorFrame;
        [SerializeField] private Sprite _retroflectorCornerIcon;
        [SerializeField] private Sprite _retroflectorResistanceIcon;

        [Space, SerializeField] private string _confluentNameFinnish;
        [SerializeField] private string _confluentNameEnglish;
        [SerializeField] private Color _confluentColor;
        [SerializeField] private Color _confluentAlternativeColor;
        [SerializeField] private Sprite _confluentFrame;
        [SerializeField] private Sprite _confluentCornerIcon;
        [SerializeField] private Sprite _confluentResistanceIcon;

        [Space, SerializeField] private string _intellectualizerNameFinnish;
        [SerializeField] private string _intellectualizerNameEnglish;
        [SerializeField] private Color _intellectualizerColor;
        [SerializeField] private Color _intellectualizerAlternativeColor;
        [SerializeField] private Sprite _intellectualizerFrame;
        [SerializeField] private Sprite _intellectualizerCornerIcon;
        [SerializeField] private Sprite _intellectualizerResistanceIcon;

        private static ClassReference _instance;
        private static bool _hasInstance;

        public static ClassReference Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<ClassReference>("Characters/"+nameof(ClassReference));
                    _hasInstance = _instance != null;
                }
                return _instance;
            }
        }

        /// <summary>
        /// Get character class name.
        /// </summary>
        /// <param name="classType">The class id which name to get.</param>
        /// <returns>Class name as string.</returns>
        public string GetName(CharacterClassType classType)
        {
            return SettingsCarrier.Instance.Language switch
            {
                SettingsCarrier.LanguageType.Finnish => classType switch
                {
                    CharacterClassType.Desensitizer => _desensitizerNameFinnish,
                    CharacterClassType.Trickster => _tricksterNameFinnish,
                    CharacterClassType.Obedient => _obedientNameFinnish,
                    CharacterClassType.Projector => _projectorNameFinnish,
                    CharacterClassType.Retroflector => _retroflectorNameFinnish,
                    CharacterClassType.Confluent => _confluentNameFinnish,
                    CharacterClassType.Intellectualizer => _intellectualizerNameFinnish,
                    _ => "No class name",
                },
                SettingsCarrier.LanguageType.English => classType switch
                {
                    CharacterClassType.Desensitizer => _desensitizerNameEnglish,
                    CharacterClassType.Trickster => _tricksterNameEnglish,
                    CharacterClassType.Obedient => _obedientNameEnglish,
                    CharacterClassType.Projector => _projectorNameEnglish,
                    CharacterClassType.Retroflector => _retroflectorNameEnglish,
                    CharacterClassType.Confluent => _confluentNameEnglish,
                    CharacterClassType.Intellectualizer => _intellectualizerNameEnglish,
                    _ => "No class name",
                },
                _ => "No class name",
            };
        }


        /// <summary>
        /// Get character class color.
        /// </summary>
        /// <param name="classType">The class id which color to get.</param>
        /// <returns>Class color.</returns>
        public Color GetColor(CharacterClassType classType)
        {
            switch (classType)
            {
                case CharacterClassType.Desensitizer:
                    return _desensitizerColor;
                case CharacterClassType.Trickster:
                    return _tricksterColor;
                case CharacterClassType.Obedient:
                    return _obedientColor;
                case CharacterClassType.Projector:
                    return _projectorColor;
                case CharacterClassType.Retroflector:
                    return _retroflectorColor;
                case CharacterClassType.Confluent:
                    return _confluentColor;
                case CharacterClassType.Intellectualizer:
                    return _intellectualizerColor;
            }
            return Color.gray;
        }


        /// <summary>
        /// Get character class alternative (lighter) color.
        /// </summary>
        /// <param name="classType">The class id which alternative color to get.</param>
        /// <returns>Class alternative color.</returns>
        public Color GetAlternativeColor(CharacterClassType classType)
        {
            switch (classType)
            {
                case CharacterClassType.Desensitizer:
                    return _desensitizerAlternativeColor;
                case CharacterClassType.Trickster:
                    return _tricksterAlternativeColor;
                case CharacterClassType.Obedient:
                    return _obedientAlternativeColor;
                case CharacterClassType.Projector:
                    return _projectorAlternativeColor;
                case CharacterClassType.Retroflector:
                    return _retroflectorAlternativeColor;
                case CharacterClassType.Confluent:
                    return _confluentAlternativeColor;
                case CharacterClassType.Intellectualizer:
                    return _intellectualizerAlternativeColor;
            }
            return Color.gray;
        }

        /// <summary>
        /// Get character class frame.
        /// </summary>
        /// <param name="classType">The class id which frame to get.</param>
        /// <returns>Class frame.</returns>
        public Sprite GetFrame(CharacterClassType classType)
        {
            switch (classType)
            {
                case CharacterClassType.Desensitizer:
                    return _desensitizerFrame;
                case CharacterClassType.Trickster:
                    return _tricksterFrame;
                case CharacterClassType.Obedient:
                    return _obedientFrame;
                case CharacterClassType.Projector:
                    return _projectorFrame;
                case CharacterClassType.Retroflector:
                    return _retroflectorFrame;
                case CharacterClassType.Confluent:
                    return _confluentFrame;
                case CharacterClassType.Intellectualizer:
                    return _intellectualizerFrame;
            }
            return null;
        }

        /// <summary>
        /// Get character class corner icon.
        /// </summary>
        /// <param name="classType">The class id which corner icon to get.</param>
        /// <returns>Class corner icon.</returns>
        public Sprite GetCornerIcon(CharacterClassType classType)
        {
            switch (classType)
            {
                case CharacterClassType.Desensitizer:
                    return _desensitizerCornerIcon;
                case CharacterClassType.Trickster:
                    return _tricksterCornerIcon;
                case CharacterClassType.Obedient:
                    return _obedientCornerIcon;
                case CharacterClassType.Projector:
                    return _projectorCornerIcon;
                case CharacterClassType.Retroflector:
                    return _retroflectorCornerIcon;
                case CharacterClassType.Confluent:
                    return _confluentCornerIcon;
                case CharacterClassType.Intellectualizer:
                    return _intellectualizerCornerIcon;
            }
            return null;
        }

        /// <summary>
        /// Get character class resistance icon.
        /// </summary>
        /// <param name="classType">The class id which resistance icon to get.</param>
        /// <returns>Class resistance icon.</returns>
        public Sprite GetResistanceIcon(CharacterClassType classType)
        {
            switch (classType)
            {
                case CharacterClassType.Desensitizer:
                    return _desensitizerResistanceIcon;
                case CharacterClassType.Trickster:
                    return _tricksterResistanceIcon;
                case CharacterClassType.Obedient:
                    return _obedientResistanceIcon;
                case CharacterClassType.Projector:
                    return _projectorResistanceIcon;
                case CharacterClassType.Retroflector:
                    return _retroflectorResistanceIcon;
                case CharacterClassType.Confluent:
                    return _confluentResistanceIcon;
                case CharacterClassType.Intellectualizer:
                    return _intellectualizerResistanceIcon;
            }
            return null;
        }
    }
}

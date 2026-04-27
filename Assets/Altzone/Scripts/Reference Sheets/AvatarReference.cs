using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    [CreateAssetMenu(menuName = "Avatar/AvatarReference", fileName = "AvatarReference")]
    public class AvatarReference : ScriptableObject
    {
        [SerializeField] private AvatarClass _iraAvatar;

        [Space, SerializeField] private AvatarClass _avaritiaAvatar;

        [Space, SerializeField] private string _obedientNameFinnish;
        [SerializeField] private string _obedientNameEnglish;
        [SerializeField] private Color _obedientColor;
        [SerializeField] private Color _obedientAlternativeColor;
        [SerializeField] private Sprite _obedientFrame;
        [SerializeField] private Sprite _obedientCornerIcon;
        [SerializeField] private Sprite _obedientCharacter;
        [SerializeField] private string _obedientDescFinnish;
        [SerializeField] private string _obedientDescEnglish;

        [Space, SerializeField] private string _projectorNameFinnish;
        [SerializeField] private string _projectorNameEnglish;
        [SerializeField] private Color _projectorColor;
        [SerializeField] private Color _projectorAlternativeColor;
        [SerializeField] private Sprite _projectorFrame;
        [SerializeField] private Sprite _projectorCornerIcon;
        [SerializeField] private Sprite _projectorCharacter;
        [SerializeField] private string _projectorDescFinnish;
        [SerializeField] private string _projectorDescEnglish;

        [Space, SerializeField] private string _retroflectorNameFinnish;
        [SerializeField] private string _retroflectorNameEnglish;
        [SerializeField] private Color _retroflectorColor;
        [SerializeField] private Color _retroflectorAlternativeColor;
        [SerializeField] private Sprite _retroflectorFrame;
        [SerializeField] private Sprite _retroflectorCornerIcon;
        [SerializeField] private Sprite _retroflectorCharacter;
        [SerializeField] private string _retroflectorDescFinnish;
        [SerializeField] private string _retroflectorDescEnglish;

        [Space, SerializeField] private string _confluentNameFinnish;
        [SerializeField] private string _confluentNameEnglish;
        [SerializeField] private Color _confluentColor;
        [SerializeField] private Color _confluentAlternativeColor;
        [SerializeField] private Sprite _confluentFrame;
        [SerializeField] private Sprite _confluentCornerIcon;
        [SerializeField] private Sprite _confluentCharacter;
        [SerializeField] private string _confluentDescFinnish;
        [SerializeField] private string _confluentDescEnglish;

        [Space, SerializeField] private string _intellectualizerNameFinnish;
        [SerializeField] private string _intellectualizerNameEnglish;
        [SerializeField] private Color _intellectualizerColor;
        [SerializeField] private Color _intellectualizerAlternativeColor;
        [SerializeField] private Sprite _intellectualizerFrame;
        [SerializeField] private Sprite _intellectualizerCornerIcon;
        [SerializeField] private Sprite _intellectualizerCharacter;
        [SerializeField] private string _intellectualizerDescFinnish;
        [SerializeField] private string _intellectualizerDescEnglish;

        private static AvatarReference _instance;
        private static bool _hasInstance;

        public static AvatarReference Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<AvatarReference>("Characters/" + nameof(AvatarReference));
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
                    CharacterClassType.Desensitizer => _iraAvatar.NameFinnish,
                    CharacterClassType.Trickster => _avaritiaAvatar.NameFinnish,
                    CharacterClassType.Obedient => _obedientNameFinnish,
                    CharacterClassType.Projector => _projectorNameFinnish,
                    CharacterClassType.Retroflector => _retroflectorNameFinnish,
                    CharacterClassType.Confluent => _confluentNameFinnish,
                    CharacterClassType.Intellectualizer => _intellectualizerNameFinnish,
                    _ => "No class name",
                },
                SettingsCarrier.LanguageType.English => classType switch
                {
                    CharacterClassType.Desensitizer => _iraAvatar.NameEnglish,
                    CharacterClassType.Trickster => _avaritiaAvatar.NameEnglish,
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
            return classType switch
            {
                CharacterClassType.Desensitizer => _iraAvatar.EyeColor,
                CharacterClassType.Trickster => _avaritiaAvatar.EyeColor,
                CharacterClassType.Obedient => _obedientColor,
                CharacterClassType.Projector => _projectorColor,
                CharacterClassType.Retroflector => _retroflectorColor,
                CharacterClassType.Confluent => _confluentColor,
                CharacterClassType.Intellectualizer => _intellectualizerColor,
                _ => Color.gray,
            };
        }


        /// <summary>
        /// Get character class alternative (lighter) color.
        /// </summary>
        /// <param name="classType">The class id which alternative color to get.</param>
        /// <returns>Class alternative color.</returns>
        public Color GetAlternativeColor(CharacterClassType classType)
        {
            return classType switch
            {
                CharacterClassType.Desensitizer => _iraAvatar.SkinColor,
                CharacterClassType.Trickster => _avaritiaAvatar.SkinColor,
                CharacterClassType.Obedient => _obedientAlternativeColor,
                CharacterClassType.Projector => _projectorAlternativeColor,
                CharacterClassType.Retroflector => _retroflectorAlternativeColor,
                CharacterClassType.Confluent => _confluentAlternativeColor,
                CharacterClassType.Intellectualizer => _intellectualizerAlternativeColor,
                _ => Color.gray,
            };
        }

        /// <summary>
        /// Get character class frame.
        /// </summary>
        /// <param name="classType">The class id which frame to get.</param>
        /// <returns>Class frame.</returns>
        public Sprite GetFrame(CharacterClassType classType)
        {
            return classType switch
            {
                CharacterClassType.Desensitizer => _iraAvatar.Frame,
                CharacterClassType.Trickster => _avaritiaAvatar.Frame,
                CharacterClassType.Obedient => _obedientFrame,
                CharacterClassType.Projector => _projectorFrame,
                CharacterClassType.Retroflector => _retroflectorFrame,
                CharacterClassType.Confluent => _confluentFrame,
                CharacterClassType.Intellectualizer => _intellectualizerFrame,
                _ => null,
            };
        }

        /// <summary>
        /// Get character class corner icon.
        /// </summary>
        /// <param name="classType">The class id which corner icon to get.</param>
        /// <returns>Class corner icon.</returns>
        public Sprite GetCornerIcon(CharacterClassType classType)
        {
            return classType switch
            {
                CharacterClassType.Desensitizer => _iraAvatar.CornerIcon,
                CharacterClassType.Trickster => _avaritiaAvatar.CornerIcon,
                CharacterClassType.Obedient => _obedientCornerIcon,
                CharacterClassType.Projector => _projectorCornerIcon,
                CharacterClassType.Retroflector => _retroflectorCornerIcon,
                CharacterClassType.Confluent => _confluentCornerIcon,
                CharacterClassType.Intellectualizer => _intellectualizerCornerIcon,
                _ => null,
            };
        }

        /// <summary>
        /// Get character icon.
        /// </summary>
        /// <param name="classType">The class id which character sprite to get.</param>
        /// <returns>Class character sprite.</returns>
        public Sprite GetCharacterSprite(CharacterClassType classType)
        {
            return classType switch
            {
                CharacterClassType.Desensitizer => _iraAvatar.Character,
                CharacterClassType.Trickster => _avaritiaAvatar.Character,
                CharacterClassType.Obedient => _obedientCharacter,
                CharacterClassType.Projector => _projectorCharacter,
                CharacterClassType.Retroflector => _retroflectorCharacter,
                CharacterClassType.Confluent => _confluentCharacter,
                CharacterClassType.Intellectualizer => _intellectualizerCharacter,
                _ => null,
            };
        }

        /// <summary>
        /// Get character class name.
        /// </summary>
        /// <param name="classType">The class id which name to get.</param>
        /// <returns>Class name as string.</returns>
        public string GetDescription(CharacterClassType classType)
        {
            return SettingsCarrier.Instance.Language switch
            {
                SettingsCarrier.LanguageType.Finnish => classType switch
                {
                    CharacterClassType.Desensitizer => _iraAvatar.DescFinnish,
                    CharacterClassType.Trickster => _avaritiaAvatar.DescFinnish,
                    CharacterClassType.Obedient => _obedientDescFinnish,
                    CharacterClassType.Projector => _projectorDescFinnish,
                    CharacterClassType.Retroflector => _retroflectorDescFinnish,
                    CharacterClassType.Confluent => _confluentDescFinnish,
                    CharacterClassType.Intellectualizer => _intellectualizerDescFinnish,
                    _ => "No class description",
                },
                SettingsCarrier.LanguageType.English => classType switch
                {
                    CharacterClassType.Desensitizer => _iraAvatar.DescEnglish,
                    CharacterClassType.Trickster => _avaritiaAvatar.DescEnglish,
                    CharacterClassType.Obedient => _obedientDescEnglish,
                    CharacterClassType.Projector => _projectorDescEnglish,
                    CharacterClassType.Retroflector => _retroflectorDescEnglish,
                    CharacterClassType.Confluent => _confluentDescEnglish,
                    CharacterClassType.Intellectualizer => _intellectualizerDescEnglish,
                    _ => "No class description",
                },
                _ => "No class description",
            };
        }
    }

    [Serializable]
    public class AvatarClass
    {
        [SerializeField] private string _nameFinnish;
        [SerializeField] private string _nameEnglish;
        [SerializeField] private Color _eyeColor;
        [SerializeField] private Color _skinColor;
        [SerializeField] private Sprite _frame;
        [SerializeField] private Sprite _cornerIcon;
        [SerializeField] private Sprite _character;
        [SerializeField] private string _descFinnish;
        [SerializeField] private string _descEnglish;

        public string NameFinnish { get => _nameFinnish; }
        public string NameEnglish { get => _nameEnglish; }
        public Color EyeColor { get => _eyeColor; }
        public Color SkinColor { get => _skinColor; }
        public Sprite Frame { get => _frame; }
        public Sprite CornerIcon { get => _cornerIcon; }
        public Sprite Character { get => _character; }
        public string DescFinnish { get => _descFinnish; }
        public string DescEnglish { get => _descEnglish; }
    }
}

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
        [Space, SerializeField] private AvatarClass _invidiaAvatar;
        [Space, SerializeField] private AvatarClass _luxuriaAvatar;
        [Space, SerializeField] private AvatarClass _gulaAvatar;
        [Space, SerializeField] private AvatarClass _acediaAvatar;
        [Space, SerializeField] private AvatarClass _superbiaAvatar;

        private static AvatarReference _instance;
        private static bool _hasInstance;

        public static AvatarReference Instance
        {
            get
            {
                if (!_hasInstance)
                {
                    _instance = Resources.Load<AvatarReference>(nameof(AvatarReference));
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
                    CharacterClassType.Obedient => _invidiaAvatar.NameFinnish,
                    CharacterClassType.Projector => _luxuriaAvatar.NameFinnish,
                    CharacterClassType.Retroflector => _gulaAvatar.NameFinnish,
                    CharacterClassType.Confluent => _acediaAvatar.NameFinnish,
                    CharacterClassType.Intellectualizer => _superbiaAvatar.NameFinnish,
                    _ => "No class name",
                },
                SettingsCarrier.LanguageType.English => classType switch
                {
                    CharacterClassType.Desensitizer => _iraAvatar.NameEnglish,
                    CharacterClassType.Trickster => _avaritiaAvatar.NameEnglish,
                    CharacterClassType.Obedient => _invidiaAvatar.NameEnglish,
                    CharacterClassType.Projector => _luxuriaAvatar.NameEnglish,
                    CharacterClassType.Retroflector => _gulaAvatar.NameEnglish,
                    CharacterClassType.Confluent => _acediaAvatar.NameEnglish,
                    CharacterClassType.Intellectualizer => _superbiaAvatar.NameEnglish,
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
        public Color GetColour(CharacterClassType classType)
        {
            return classType switch
            {
                CharacterClassType.Desensitizer => _iraAvatar.EyeColor,
                CharacterClassType.Trickster => _avaritiaAvatar.EyeColor,
                CharacterClassType.Obedient => _invidiaAvatar.EyeColor,
                CharacterClassType.Projector => _luxuriaAvatar.EyeColor,
                CharacterClassType.Retroflector => _gulaAvatar.EyeColor,
                CharacterClassType.Confluent => _acediaAvatar.EyeColor,
                CharacterClassType.Intellectualizer => _superbiaAvatar.EyeColor,
                _ => Color.gray,
            };
        }


        /// <summary>
        /// Get character class alternative (lighter) color.
        /// </summary>
        /// <param name="classType">The class id which alternative color to get.</param>
        /// <returns>Class alternative color.</returns>
        public Color GetAlternativeColour(CharacterClassType classType)
        {
            return classType switch
            {
                CharacterClassType.Desensitizer => _iraAvatar.SkinColor,
                CharacterClassType.Trickster => _avaritiaAvatar.SkinColor,
                CharacterClassType.Obedient => _invidiaAvatar.SkinColor,
                CharacterClassType.Projector => _luxuriaAvatar.SkinColor,
                CharacterClassType.Retroflector => _gulaAvatar.SkinColor,
                CharacterClassType.Confluent => _acediaAvatar.SkinColor,
                CharacterClassType.Intellectualizer => _superbiaAvatar.SkinColor,
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
                CharacterClassType.Obedient => _invidiaAvatar.Frame,
                CharacterClassType.Projector => _luxuriaAvatar.Frame,
                CharacterClassType.Retroflector => _gulaAvatar.Frame,
                CharacterClassType.Confluent => _acediaAvatar.Frame,
                CharacterClassType.Intellectualizer => _superbiaAvatar.Frame,
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
                CharacterClassType.Obedient => _invidiaAvatar.CornerIcon,
                CharacterClassType.Projector => _luxuriaAvatar.CornerIcon,
                CharacterClassType.Retroflector => _gulaAvatar.CornerIcon,
                CharacterClassType.Confluent => _acediaAvatar.CornerIcon,
                CharacterClassType.Intellectualizer => _superbiaAvatar.CornerIcon,
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
                CharacterClassType.Obedient => _invidiaAvatar.Character,
                CharacterClassType.Projector => _luxuriaAvatar.Character,
                CharacterClassType.Retroflector => _gulaAvatar.Character,
                CharacterClassType.Confluent => _acediaAvatar.Character,
                CharacterClassType.Intellectualizer => _superbiaAvatar.Character,
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
                    CharacterClassType.Obedient => _invidiaAvatar.DescFinnish,
                    CharacterClassType.Projector => _luxuriaAvatar.DescFinnish,
                    CharacterClassType.Retroflector => _gulaAvatar.DescFinnish,
                    CharacterClassType.Confluent => _acediaAvatar.DescFinnish,
                    CharacterClassType.Intellectualizer => _superbiaAvatar.DescFinnish,
                    _ => "No class description",
                },
                SettingsCarrier.LanguageType.English => classType switch
                {
                    CharacterClassType.Desensitizer => _iraAvatar.DescEnglish,
                    CharacterClassType.Trickster => _avaritiaAvatar.DescEnglish,
                    CharacterClassType.Obedient => _invidiaAvatar.DescEnglish,
                    CharacterClassType.Projector => _luxuriaAvatar.DescEnglish,
                    CharacterClassType.Retroflector => _gulaAvatar.DescEnglish,
                    CharacterClassType.Confluent => _acediaAvatar.DescEnglish,
                    CharacterClassType.Intellectualizer => _superbiaAvatar.DescEnglish,
                    _ => "No class description",
                },
                _ => "No class description",
            };
        }

        public AvatarDefault GetDefaultAvatar(CharacterClassType classType)
        {
            return classType switch
            {
                CharacterClassType.Desensitizer => new AvatarDefault(_iraAvatar.Default, _iraAvatar.SkinColor, GetName(CharacterClassType.Desensitizer)),
                CharacterClassType.Trickster => new AvatarDefault(_avaritiaAvatar.Default, _avaritiaAvatar.SkinColor, GetName(CharacterClassType.Trickster)),
                CharacterClassType.Obedient => new AvatarDefault(_invidiaAvatar.Default, _invidiaAvatar.SkinColor, GetName(CharacterClassType.Obedient)),
                CharacterClassType.Projector => new AvatarDefault(_luxuriaAvatar.Default, _luxuriaAvatar.SkinColor, GetName(CharacterClassType.Projector)),
                CharacterClassType.Retroflector => new AvatarDefault(_gulaAvatar.Default, _gulaAvatar.SkinColor, GetName(CharacterClassType.Retroflector)),
                CharacterClassType.Confluent => new AvatarDefault(_acediaAvatar.Default, _acediaAvatar.SkinColor, GetName(CharacterClassType.Confluent)),
                CharacterClassType.Intellectualizer => new AvatarDefault(_superbiaAvatar.Default, _superbiaAvatar.SkinColor, GetName(CharacterClassType.Intellectualizer)),
                _ => new AvatarDefault(_superbiaAvatar.Default, _superbiaAvatar.SkinColor, GetName(CharacterClassType.Intellectualizer)),
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
        [Header("Avatar Defaults")]
        [SerializeField] private AvatarDefaultParts _default;

        public string NameFinnish { get => _nameFinnish; }
        public string NameEnglish { get => _nameEnglish; }
        public Color EyeColor { get => _eyeColor; }
        public Color SkinColor { get => _skinColor; }
        public Sprite Frame { get => _frame; }
        public Sprite CornerIcon { get => _cornerIcon; }
        public Sprite Character { get => _character; }
        public string DescFinnish { get => _descFinnish; }
        public string DescEnglish { get => _descEnglish; }
        public AvatarDefaultParts Default { get => _default; }
    }

    public class AvatarDefault
    {
        private string _name = "";
        private Color _skinColour = Color.white;
        private string _hairId = "";
        private Color _hairColour = Color.white;
        private string _eyesId = "";
        private Color _eyesColour = Color.white;
        private string _noseId = "";
        private Color _noseColour = Color.white;
        private string _mouthId = "";
        private Color _mouthColour = Color.white;
        private string _bodyId = "";
        private Color _bodyColour = Color.white;
        private string _handsId = "";
        private Color _handsColour = Color.white;
        private string _feetId = "";
        private Color _feetColour = Color.white;

        public AvatarDefault(AvatarDefaultParts defaultParts, Color skinColour, string name)
        {
            _name = name;
            _skinColour = skinColour;
            _hairId = defaultParts.HairId;
            _hairColour = defaultParts.HairColour;
            _eyesId = defaultParts.EyesId;
            _eyesColour = defaultParts.EyesColour;
            _noseId = defaultParts.NoseId;
            _noseColour = defaultParts.NoseColour;
            _mouthId = defaultParts.MouthId;
            _mouthColour = defaultParts.MouthColour;
            _bodyId = defaultParts.BodyId;
            _bodyColour = defaultParts.BodyColour;
            _handsId = defaultParts.HandsId;
            _handsColour = defaultParts.HandsColour;
            _feetId = defaultParts.FeetId;
            _feetColour= defaultParts.FeetColour;
        }

        public string Name { get => _name; }
        public Color SkinColour { get => _skinColour; }
        public string HairId { get => _hairId; }
        public Color HairColour { get => _hairColour; }
        public string EyesId { get => _eyesId; }
        public Color EyesColour { get => _eyesColour; }
        public string NoseId { get => _noseId; }
        public Color NoseColour { get => _noseColour; }
        public string MouthId { get => _mouthId; }
        public Color MouthColour { get => _mouthColour; }
        public string BodyId { get => _bodyId; }
        public Color BodyColour { get => _bodyColour; }
        public string HandsId { get => _handsId; }
        public Color HandsColour { get => _handsColour; }
        public string FeetId { get => _feetId; }
        public Color FeetColour { get => _feetColour; }
    }
}

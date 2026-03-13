using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using Object = UnityEngine.Object;
using Quantum;

namespace Altzone.Scripts.ModelV2.Internal
{
    /// <summary>
    /// Numeric attribute with level number and coefficient for character progression logic.
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class NumAttribute
    {
        public int Level;
        public ValueStrength Coefficient;
    }

    /// <summary>
    /// Altzone game player character 'specification' implementation and references to all in-game resources attached to it.<br />
    /// <b>Note</b> that this class should not be edited spontaneously but only using relevant change management process!<br />
    /// See <c>_readme.md</c> and/or related WIKI page for more instructions.
    /// </summary>
    [CreateAssetMenu(menuName = "ALT-Zone/CharacterSpec", fileName = nameof(CharacterSpec) + "_ID")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CharacterSpec : ScriptableObject
    {
        #region Metadata

        /// <summary>
        /// Character id, specified externally.
        /// </summary>
        [Header("Character Basic Data")] public string Id;
        /// <summary>
        /// Character id in Enum form, this allows the programmers to specify certain IDs in code that aren't changed.
        /// </summary>
        public CharacterID CharacterId;
        /// <summary>
        /// Is this player character approved for production.
        /// </summary>
        public bool IsApproved;

        #endregion

        #region General attributes

        /// <summary>
        /// Character name.
        /// </summary>
        /// <remarks>
        /// When game support localization this will be localization id for this player character.
        /// </remarks>
        [Header("General Attributes")] public string FinnishName;
        [Header("General Attributes")] public string EnglishName;

        /// <summary>
        /// Player character class.
        /// </summary>
        public CharacterClassType ClassType;
        /// <summary>
        /// A long description of the character. If you want a short description use <see cref="CharacterShortDescription"/>.
        /// </summary>
        [TextArea(10, 20)]
        public string CharacterDescriptionFinnish;
        [TextArea(10, 20)]
        public string CharacterDescriptionEnglish;
        /// <summary>
        /// A short few word description of the character.
        /// </summary>
        public string CharacterShortDescriptionFinnish;
        public string CharacterShortDescriptionEnglish;

        [TextArea(5, 10)]
        public string CharacterAbilityDescriptionFinnish;
        [TextArea(5, 10)]
        public string CharacterAbilityDescriptionEnglish;

        #endregion

        #region Special attributes

        [Header("Special Attributes")]
        public BaseCharacter CharacterStats;
        public NumAttribute Hp;
        public NumAttribute Speed;
        public NumAttribute CharacterSize;
        public NumAttribute Attack;
        public NumAttribute Defence;

        #endregion

        #region General Asset References

        /// <summary>
        /// Gallery image for something.
        /// TODO: add relevant doc comment here!
        /// </summary>
        [Header("General Asset References")] public Sprite GalleryImage;

        public Sprite GalleryHeadImage;

        public Sprite CharPhotoSeries;

        #endregion

        #region Battle Asset References

        /// <summary>
        /// Battle sprite sheet for something.
        /// TODO: add relevant doc comment here!
        /// </summary>
        [Header("Battle Asset References")]
        public AssetRef<EntityPrototype> BattleCharacterEntityPrototype;
        public AssetRef<EntityPrototype>[] BattleShieldEntityPrototypes;
        public Sprite BattleUiSprite;

        #endregion

        /// <summary>
        /// Gets player character validity state for the game.
        /// </summary>
        /// <remarks>
        /// Missing fields or values makes player character invalid because
        /// they can cause e.g. undefined behaviour or NRE at runtime.
        /// </remarks>

        public string Name
        {
            get
            {
                if(SettingsCarrier.Instance == null) return FinnishName;
                switch (SettingsCarrier.Instance.Language)
                {
                    case SettingsCarrier.LanguageType.Finnish:
                        return FinnishName;
                    case SettingsCarrier.LanguageType.English:
                        return EnglishName;
                    default:
                        return FinnishName;
                }
            }
        }

        public string CharacterDescription
        {
            get
            {
                switch (SettingsCarrier.Instance.Language)
                {
                    case SettingsCarrier.LanguageType.Finnish:
                        return CharacterDescriptionFinnish;
                    case SettingsCarrier.LanguageType.English:
                        return CharacterDescriptionEnglish;
                    default:
                        return FinnishName;
                }
            }
        }
        public string CharacterShortDescription
        {
            get
            {
                switch (SettingsCarrier.Instance.Language)
                {
                    case SettingsCarrier.LanguageType.Finnish:
                        return CharacterShortDescriptionFinnish;
                    case SettingsCarrier.LanguageType.English:
                        return CharacterShortDescriptionEnglish;
                    default:
                        return CharacterShortDescriptionFinnish;
                }
            }
        }

        public string CharacterAbilityDescription
        {
            get
            {
                switch (SettingsCarrier.Instance.Language)
                {
                    case SettingsCarrier.LanguageType.Finnish:
                        return CharacterAbilityDescriptionFinnish;
                    case SettingsCarrier.LanguageType.English:
                        return CharacterAbilityDescriptionEnglish;
                    default:
                        return CharacterAbilityDescriptionFinnish;
                }
            }
        }

        public bool IsValid => (ClassType != CharacterClassType.None || CharacterId == CharacterID.Test) 
                               && !string.IsNullOrWhiteSpace(Id)
                               && !string.IsNullOrWhiteSpace(name);

        public override string ToString()
        {
            return $"{Id}:{ClassType}:{Name}" +
                   $", {UResName(GalleryImage)}" +
                   $", {QResName(BattleCharacterEntityPrototype)}" +
                   $", {UResName(BattleUiSprite)}";

            string UResName(Object instance) => $"{(instance == null ? "null" : instance.name)}";
            string QResName<T>(AssetRef<T> instance) where T : AssetObject => $"{(instance == null ? "null" : instance.ToString())}";
        }
    }
}

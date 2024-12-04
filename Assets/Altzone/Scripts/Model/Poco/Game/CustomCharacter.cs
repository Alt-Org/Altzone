using System;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Attributes;
using Altzone.Scripts.Model.Poco.Player;
using Prg;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// Player created custom 'game' character based on given <c>CharacterClass</c>.
    /// </summary>
    [MongoDbEntity, Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CustomCharacter
    {
        [PrimaryKey] public CharacterID Id;
        public string CharacterName => GetCharacterName(Id);
        public string CharacterClassAndName => GetCharacterClassAndName(Id);
        public CharacterClassID CharacterClassID => GetClassID(Id);
        public int InsideCharacterID => GetInsideCharacterID(Id);

        public BaseCharacter CharacterBase { get => _characterBase;}

        private BaseCharacter _characterBase;
        /// <summary>
        /// This can be used for example to load UNITY assets by name for UI at runtime.
        /// </summary>
        //[Optional] public int UnityKey = -1;

        [Mandatory] public string Name;
        public int Hp;
        public int HpSegmentCount;
        public int Speed;
        public int SpeedSegmentCount;
        public int Resistance;
        public int ResistanceSegmentCount;
        public int Attack;
        public int AttackSegmentCount;
        public int Defence;
        public int DefenceSegmentCount;

        public CustomCharacter(CharacterID id, int hp, int speed, int resistance, int attack, int defence)
        {
            //Assert.AreNotEqual(CharacterID.None, id);
            //Assert.IsTrue(characterClassId.IsMandatory());
            //Assert.IsTrue(name.IsMandatory());
            Id = id;
            Name = GetCharacterName(id);
            Hp = hp;
            HpSegmentCount = 0;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        public CustomCharacter(BaseCharacter character)
        {
            Assert.AreNotEqual(CharacterID.None, character.Id);
            _characterBase = character;
            Id = character.Id;
            Name = GetCharacterName(character.Id);
            Hp = character.Hp;
            HpSegmentCount = 0;
            Speed = character.Speed;
            SpeedSegmentCount = 0;
            Resistance = character.Resistance;
            ResistanceSegmentCount = 0;
            Attack = character.Attack;
            AttackSegmentCount = 0;
            Defence = character.Defence;
            DefenceSegmentCount = 0;
        }

        internal static CustomCharacter CreateEmpty()
        {
            //Assert.AreEqual(customCharacter.CharacterClassId, characterClass.Id, "CharacterClassId mismatch");
            return new CustomCharacter(
                CharacterID.None,
                0,
                0,
                0,
                0,
                0);
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}" +
                   $", {nameof(Name)}: {Name}" +
                   $", {nameof(Hp)}: {Hp}, {nameof(Speed)}: {Speed}, {nameof(Resistance)}: {Resistance}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }

        private static string GetCharacterName(CharacterID id)
        {
            switch (id)
            {
                case CharacterID.DesensitizerBodybuilder:
                    return "Bodybuilder";
                case CharacterID.TricksterComedian:
                    return "Comedian";
                case CharacterID.TricksterConman:
                    return "Conman";
                case CharacterID.ObedientPreacher:
                    return "Preacher";
                case CharacterID.ProjectorGrafitiartist:
                    return "Grafitiartist";
                case CharacterID.RetroflectorOvereater:
                    return "Overeater";
                case CharacterID.RetroflectorAlcoholic:
                    return "Alcoholic";
                case CharacterID.ConfluentBesties:
                    return "Besties";
                case CharacterID.IntellectualizerResearcher:
                    return "Researcher";
                default:
                    return "Error";
            }
        }

        public bool IncreaseStat(StatType statType, int count)
        {
            if (count < 1) { Debug.LogError("Invalid upgrade count."); return false; }
            PlayerData playerData = null;
            Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);
            bool increase = false;
            switch (statType)
            {
                case StatType.Attack:
                    while (true)
                    {
                        if (playerData.DiamondAttack >= BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Attack + 1))
                        {
                            increase = true;
                            AttackSegmentCount++;
                            count--;
                        }
                        else break;
                        int segmentsForNext = BaseCharacter.GetSegmentAmount(_characterBase, statType, Attack + 1);
                        if (segmentsForNext <= AttackSegmentCount) { AttackSegmentCount -= segmentsForNext; Attack++; }
                        if (count < 1) break;
                    }
                    break;
                case StatType.Defence:
                    break;
                case StatType.Resistance:
                    break;
                case StatType.Hp:
                    break;
                case StatType.Speed:
                    break;
                default:
                    Debug.LogError("Invalid stat type. Provide proper stat type.");
                    return false;
            }
            if (increase) return true;
            else return false;
        }

        public int GetPriceToNextLevel(StatType statType)
        {
            switch (statType)
            {
                case StatType.Attack:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Attack + 1) * (BaseCharacter.GetSegmentAmount(_characterBase, statType, Attack + 1)- AttackSegmentCount);
                case StatType.Defence:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Defence + 1) * (BaseCharacter.GetSegmentAmount(_characterBase, statType, Defence + 1) - DefenceSegmentCount);
                case StatType.Resistance:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Resistance + 1) * (BaseCharacter.GetSegmentAmount(_characterBase, statType, Resistance + 1) - ResistanceSegmentCount);
                case StatType.Hp:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Hp + 1) * (BaseCharacter.GetSegmentAmount(_characterBase, statType, Hp + 1) - HpSegmentCount);
                case StatType.Speed:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Speed + 1) * (BaseCharacter.GetSegmentAmount(_characterBase, statType, Speed + 1) - SpeedSegmentCount);
                default:
                    Debug.LogError("StatType not provided. Please give a valid StatType.");
                    return -1;
            }
        }

        public int GetNextSegmentPrice(StatType statType)
        {
            switch (statType)
            {
                case StatType.Attack:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Attack+1);
                case StatType.Defence:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Defence+1);
                case StatType.Resistance:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Resistance+1);
                case StatType.Hp:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Hp+1);
                case StatType.Speed:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, Speed+1);
                default:
                    Debug.LogError("StatType not provided. Please give a valid StatType.");
                    return -1;
            }
        }

        public static string GetCharacterClassAndName(CharacterID id)
        {
            CharacterClassID classId = GetClassID(id);

            string className = CharacterClass.GetClassName(classId);

            return className+GetCharacterName(id);
        }

        public static CharacterClassID GetClassID(CharacterID id)
        {
            CharacterClassID ClassId = (CharacterClassID)((int)id & 0b1111_1111__0000_0000);
            return ClassId;
        }

        public static int GetInsideCharacterID(CharacterID id)
        {
            int characterId = (int)id & 0b0000_0000__1111_1111;
            return characterId;
        }

    }
}

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using Prg;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = Prg.Debug;

namespace Altzone.Scripts.Model.Poco.Game
{
    /// <summary>
    /// Player created custom 'game' character based on given <c>CharacterClass</c>.
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CustomCharacter
    {
        public CharacterID Id;
        public string CharacterName => GetCharacterName(Id);
        public string CharacterClassAndName => GetCharacterClassAndName(Id);
        public CharacterClassType CharacterClassType => GetClass(Id);
        public int InsideCharacterID => GetInsideCharacterID(Id);

        public BaseCharacter CharacterBase { get => _characterBase;
            set
            {
                if(_characterBase == null)
                _characterBase = value;
            }
        }

        public int Hp { get => _hp;}
        public int Speed { get => _speed;}
        public int CharacterSize { get => _characterSize;}
        public int Attack { get => _attack;}
        public int Defence { get => _defence;}

        private BaseCharacter _characterBase;
        /// <summary>
        /// This can be used for example to load UNITY assets by name for UI at runtime.
        /// </summary>
        public string ServerID = "-1";

        public string Name;
        private int _hp;
        public int HpSegmentCount;
        private int _speed;
        public int SpeedSegmentCount;
        private int _characterSize;
        public int CharacterSizeSegmentCount;
        private int _attack;
        public int AttackSegmentCount;
        private int _defence;
        public int DefenceSegmentCount;

        public const int STATMAXCOMBINED = 50;
        public const int STATMAXLEVEL = 6;
        public const int STATMINLEVEL = 1;

        public CustomCharacter(CharacterID id, int hp, int speed, int resistance, int attack, int defence)
        {
            //Assert.AreNotEqual(CharacterID.None, id);
            //Assert.IsTrue(characterClassId.IsMandatory());
            //Assert.IsTrue(name.IsMandatory());
            Id = id;
            Name = GetCharacterName(id);
            _hp = hp;
            HpSegmentCount = 0;
            _speed = speed;
            _characterSize = resistance;
            _attack = attack;
            _defence = defence;
        }

        public CustomCharacter(BaseCharacter character)
        {
            Assert.AreNotEqual(CharacterID.None, character.Id);
            _characterBase = character;
            Id = character.Id;
            Name = GetCharacterName(character.Id);
            _hp = character.Hp;
            HpSegmentCount = 0;
            _speed = character.Speed;
            SpeedSegmentCount = 0;
            _characterSize = character.CharacterSize;
            CharacterSizeSegmentCount = 0;
            _attack = character.Attack;
            AttackSegmentCount = 0;
            _defence = character.Defence;
            DefenceSegmentCount = 0;
        }

        public CustomCharacter(ServerCharacter character)
        {
            CharacterID id = CharacterID.None;
            if (Enum.IsDefined(typeof(CharacterID), int.Parse(character.characterId)))
                id =(CharacterID)int.Parse(character.characterId);
            Assert.AreNotEqual(CharacterID.None, id);

            var store = Storefront.Get();

            ReadOnlyCollection<BaseCharacter> allItems = null;
            store.GetAllBaseCharacterYield(result => allItems = result);

            foreach(var item in allItems)
            {
                if(item.Id.Equals(id)) _characterBase = item;
            }

            if(_characterBase == null)
            {
                Debug.LogError("Unable to find matching BaseCharacter. Check the id that is being fetched from the server.");
                return;
            }

            Id = id;
            ServerID = character._id;
            Name = string.IsNullOrEmpty(character.name) ? character.name : GetCharacterName(Id);
            _hp = character.hp;
            HpSegmentCount = 0;
            _speed = _characterBase.Speed;
            SpeedSegmentCount = 0;
            _characterSize = character.size;
            CharacterSizeSegmentCount = 0;
            _attack = character.attack;
            AttackSegmentCount = 0;
            _defence = character.defence;
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
                   $", {nameof(CharacterBase)}: {CharacterBase}" +
                   $", {nameof(Hp)}: {Hp}, {nameof(Speed)}: {Speed}, {nameof(CharacterSize)}: {CharacterSize}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }

        public static string GetCharacterName(CharacterID id)
        {
            switch (id)
            {
                case CharacterID.Bodybuilder:
                    return "Bodybuilder";
                case CharacterID.Joker:
                    return "Comedian";
                case CharacterID.Conman:
                    return "Conman";
                case CharacterID.Religious:
                    return "Preacher";
                case CharacterID.Artist:
                    return "Grafitiartist";
                case CharacterID.Overeater:
                    return "Overeater";
                case CharacterID.Alcoholic:
                    return "Alcoholic";
                case CharacterID.Soulsisters:
                    return "Besties";
                case CharacterID.Booksmart:
                    return "Researcher";
                default:
                    return "Error";
            }
        }

        public bool IncreaseStat(StatType statType, int count = 1)
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
                        if (playerData.DiamondSpeed >= GetPriceToNextLevel(statType)) // Change this back to DiamondAttack if we move back to using separate resources.
                        {
                            increase = true;
                            _attack/*SegmentCount*/++;
                            playerData.DiamondSpeed -= GetPriceToNextLevel(statType);
                            count--;
                        }
                        else break;
                        //int segmentsForNext = BaseCharacter.GetSegmentAmount(_characterBase, statType, Attack + 1);
                        //if (segmentsForNext <= AttackSegmentCount) { AttackSegmentCount -= segmentsForNext; Attack++; }
                        if (count < 1) break;
                    }
                    break;
                case StatType.Defence:
                    while (true)
                    {
                        if (playerData.DiamondSpeed >= GetPriceToNextLevel(statType)) // Change this back to DiamondDefence if we move back to using separate resources.
                        {
                            increase = true;
                            _defence/*SegmentCount*/++;
                            playerData.DiamondSpeed -= GetPriceToNextLevel(statType);
                            count--;
                        }
                        else break;
                        //int segmentsForNext = BaseCharacter.GetSegmentAmount(_characterBase, statType, Attack + 1);
                        //if (segmentsForNext <= AttackSegmentCount) { AttackSegmentCount -= segmentsForNext; Attack++; }
                        if (count < 1) break;
                    }
                    break;
                case StatType.CharacterSize:
                    Debug.LogError("CharacterSize cannot be increased.");
                    break;
                case StatType.Hp:
                    while (true)
                    {
                        if (playerData.DiamondSpeed >= GetPriceToNextLevel(statType)) // Change this back to DiamondHP if we move back to using separate resources.
                        {
                            increase = true;
                            _hp/*SegmentCount*/++;
                            playerData.DiamondSpeed -= GetPriceToNextLevel(statType);
                            count--;
                        }
                        else break;
                        //int segmentsForNext = BaseCharacter.GetSegmentAmount(_characterBase, statType, Attack + 1);
                        //if (segmentsForNext <= AttackSegmentCount) { AttackSegmentCount -= segmentsForNext; Attack++; }
                        if (count < 1) break;
                    }
                    break;
                case StatType.Speed:
                    Debug.LogError("Speed cannot be increased.");
                    break;
                default:
                    Debug.LogError("Invalid stat type. Provide proper stat type.");
                    return false;
            }
            if (increase) return true;
            else return false;
        }

        public bool DecreaseStat(StatType statType, int count = 1)
        {
            if (count < 1) { Debug.LogError("Invalid upgrade count."); return false; }
            PlayerData playerData = null;
            Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);
            bool decrease = false;
            switch (statType)
            {
                case StatType.Attack:
                    while (true)
                    {
                        if (playerData.Eraser >= 1) // Change this back to DiamondAttack if we move back to using separate resources.
                        {
                            decrease = true;
                            _attack/*SegmentCount*/--;
                            count--;
                        }
                        else break;
                        //int segmentsForNext = BaseCharacter.GetSegmentAmount(_characterBase, statType, Attack + 1);
                        //if (segmentsForNext <= AttackSegmentCount) { AttackSegmentCount -= segmentsForNext; Attack++; }
                        if (count < 1) break;
                    }
                    break;
                case StatType.Defence:
                    while (true)
                    {
                        if (playerData.Eraser >= 1) // Change this back to DiamondDefence if we move back to using separate resources.
                        {
                            decrease = true;
                            _defence/*SegmentCount*/--;
                            count--;
                        }
                        else break;
                        //int segmentsForNext = BaseCharacter.GetSegmentAmount(_characterBase, statType, Attack + 1);
                        //if (segmentsForNext <= AttackSegmentCount) { AttackSegmentCount -= segmentsForNext; Attack++; }
                        if (count < 1) break;
                    }
                    break;
                case StatType.CharacterSize:
                    Debug.LogError("CharacterSize cannot be decreased.");
                    break;
                case StatType.Hp:
                    while (true)
                    {
                        if (playerData.Eraser >= 1) // Change this back to DiamondHP if we move back to using separate resources.
                        {
                            decrease = true;
                            _hp/*SegmentCount*/--;
                            count--;
                        }
                        else break;
                        //int segmentsForNext = BaseCharacter.GetSegmentAmount(_characterBase, statType, Attack + 1);
                        //if (segmentsForNext <= AttackSegmentCount) { AttackSegmentCount -= segmentsForNext; Attack++; }
                        if (count < 1) break;
                    }
                    break;
                case StatType.Speed:
                    Debug.LogError("Speed cannot be decreased.");
                    break;
                default:
                    Debug.LogError("Invalid stat type. Provide proper stat type.");
                    return false;
            }
            if (decrease) return true;
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
                case StatType.CharacterSize:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, CharacterSize + 1) * (BaseCharacter.GetSegmentAmount(_characterBase, statType, CharacterSize + 1) - CharacterSizeSegmentCount);
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
                case StatType.CharacterSize:
                    return BaseCharacter.GetStatSegmentPrice(_characterBase, statType, CharacterSize+1);
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
            CharacterClassType classType = GetClass(id);

            string className = Game.CharacterClass.GetClassName(classType);

            return className+GetCharacterName(id);
        }

        public static CharacterClassType GetClass(CharacterID id) => BaseCharacter.GetClass(id);

        public static int GetInsideCharacterID(CharacterID id)
        {
            int characterId = (int)id % 100;
            return characterId;
        }

        public static bool IsTestCharacter(CharacterID id)
        {
            return (int)id % 100 == 0;
        }

        public bool IsTestCharacter()
        {
            return (int)Id % 100 == 0;
        }
    }
}

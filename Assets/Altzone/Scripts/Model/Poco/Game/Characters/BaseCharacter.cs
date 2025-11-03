using System.Runtime.CompilerServices;
using Photon.Deterministic;
using UnityEditor;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public enum StatType
    {
        None,
        Attack,
        Defence,
        CharacterSize,
        Hp,
        Speed
    }

    public enum ValueStrength
    {
        None,
        VeryStrong,
        Strong,
        SemiStrong,
        Medium,
        SemiWeak,
        Weak,
        VeryWeak
    }

    public abstract class BaseCharacter : ScriptableObject
    {
        [SerializeField] protected CharacterID _id = CharacterID.None;
        [SerializeField] protected bool active = true;

        [Header("HP"), SerializeField] protected int _defaultHp;
        protected int _hp;
        [SerializeField] protected ValueStrength _hpStrength = ValueStrength.None;

        [Header("Speed"), SerializeField] protected int _defaultSpeed;
        protected int _speed;
        [SerializeField] protected ValueStrength _speedStrength = ValueStrength.None;

        [Header("Character Size"), SerializeField] protected int _defaultCharacterSize;
        protected int _characterSize;
        [SerializeField] protected ValueStrength _characterSizeStrength = ValueStrength.None;

        [Header("Attack"), SerializeField] protected int _defaultAttack;
        protected int _attack;
        [SerializeField] protected ValueStrength _attackStrength = ValueStrength.None;

        [Header("Defence"), SerializeField] protected int _defaultDefence;
        protected int _defence;
        [SerializeField] protected ValueStrength _defenceStrength = ValueStrength.None;

        public CharacterID Id { get => _id;}
        public virtual CharacterClassType ClassType { get => GetClass(Id); }
        public int Hp { get => _hp;}
        public int DefaultHp { get => _defaultHp; set { _defaultHp = value; SaveData(); } }
        public int Speed { get => _speed;}
        public int DefaultSpeed { get => _defaultSpeed; set { _defaultSpeed = value; SaveData(); } }
        public int CharacterSize { get => _characterSize;}
        public int DefaultCharacterSize { get => _defaultCharacterSize; set { _defaultCharacterSize = value; SaveData(); } }
        public int Attack { get => _attack;}
        public int DefaultAttack { get => _defaultAttack; set { _defaultAttack = value; SaveData(); } }
        public int Defence { get => _defence;}
        public int DefaultDefence { get => _defaultDefence; set { _defaultDefence = value; SaveData(); } }
        public ValueStrength HpStrength { get => _hpStrength; }
        public ValueStrength SpeedStrength { get => _speedStrength; }
        public ValueStrength CharacterSizeStrength { get => _characterSizeStrength; }
        public ValueStrength AttackStrength { get => _attackStrength; }
        public ValueStrength DefenceStrength { get => _defenceStrength; }

        protected BaseCharacter()
        {
            InitializeValues();
        }

        protected void InitializeValues()
        {
            _hp = _defaultHp;
            _attack = _defaultAttack;
            _defence = _defaultDefence;
            _characterSize = _defaultCharacterSize;
            _speed = _defaultSpeed;
        }

        private void SaveData()
        {
            #if UNITY_EDITOR
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            #endif
        }

        #region Stat value getters

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetStatValue(StatType type, int level) => (float)GetStatValueFP(type, level);

        public static FP GetStatValueFP(StatType type, int level)
        {
            return type switch
            {
                StatType.None          => (FP)(-1),
                StatType.Attack        => GetAttackValue(level),
                StatType.Defence       => GetDefenceValue(level),
                StatType.CharacterSize => GetCharacterSizeValue(level),
                StatType.Hp            => GetHpValue(level),
                StatType.Speed         => GetSpeedValue(level),

                _ => (FP)(-1),
            };
        }

        private static FP GetAttackValue(int level)
        {
            return level switch
            {
                 1 =>  1,
                 2 =>  2,
                 3 =>  4,
                 4 =>  6,
                 5 =>  8,
                 6 => 10,
                 7 => 12,
                 8 => 14,
                 9 => 16,
                10 => 18,
                11 => 20,
                12 => 22,
                13 => 24,
                14 => 26,
                15 => 28,
                16 => 30,
                17 => 32,
                18 => 34,
                19 => 36,
                20 => 38,
                21 => 40,
                22 => 42,
                23 => 44,
                24 => 46,

                _ => -1,
            };
        }

        private static FP GetDefenceValue(int level)
        {
            return level switch
            {
                 1 =>  50,
                 2 =>  75,
                 3 => 100,
                 4 => 125,
                 5 => 150,
                 6 => 175,
                 7 => 200,
                 8 => 225,
                 9 => 250,
                10 => 275,
                11 => 300,
                12 => 325,
                13 => 350,
                14 => 375,
                15 => 400,
                16 => 425,
                17 => 450,
                18 => 475,
                19 => 500,
                20 => 525,
                21 => 550,
                22 => 575,
                23 => 600,
                24 => 625,

                _ => -1,
            };
        }

        private static FP GetCharacterSizeValue(int level)
        {
            return level switch
            {
                 1 =>  4,
                 2 =>  4,
                 3 =>  4,
                 4 =>  6,
                 5 =>  6,
                 6 =>  6,
                 7 =>  8,
                 8 =>  8,
                 9 =>  8,
                10 =>  8,
                11 => 10,
                12 => 10,
                13 => 10,
                14 => 10,
                15 => 12,
                16 => 12,
                17 => 12,
                18 => 12,
                19 => 14,
                20 => 14,
                21 => 14,
                22 => 16,
                23 => 16,
                24 => 16,

                _ => -1,
            };
        }

        private static FP GetHpValue(int level)
        {
            return level switch
            {
                 1 =>   3,
                 2 =>   6,
                 3 =>  12,
                 4 =>  18,
                 5 =>  24,
                 6 =>  30,
                 7 =>  36,
                 8 =>  42,
                 9 =>  48,
                10 =>  54,
                11 =>  60,
                12 =>  66,
                13 =>  72,
                14 =>  78,
                15 =>  84,
                16 =>  90,
                17 =>  96,
                18 => 102,
                19 => 108,
                20 => 114,
                21 => 120,
                22 => 126,
                23 => 132,
                24 => 138,

                _ => -1,
            };
        }

        private static FP GetSpeedValue(int level)
        {
            return level switch
            {
                 1 =>  10,
                 2 =>  12,
                 3 =>  14,
                 4 =>  16,
                 5 =>  18,
                 6 =>  20,

                _ => -1,
            };
        }
        #endregion

        #region Stat price getters
        public static int GetStatPrice(BaseCharacter character, StatType type, int nextLevel)
        {
            switch (type)
            {
                case StatType.Attack:
                    return GetSegmentAmount(nextLevel - character._defaultAttack) * GetStatSegmentPrice(character, type, nextLevel - character._defaultAttack);
                case StatType.Defence:
                    return GetSegmentAmount(nextLevel - character._defaultDefence) * GetStatSegmentPrice(character, type, nextLevel - character._defaultDefence);
                case StatType.CharacterSize:
                    return GetSegmentAmount(nextLevel - character._defaultCharacterSize) * GetStatSegmentPrice(character, type, nextLevel - character._defaultCharacterSize);
                case StatType.Hp:
                    return GetSegmentAmount(nextLevel - character._defaultHp) * GetStatSegmentPrice(character, type, nextLevel - character._defaultHp);
                case StatType.Speed:
                    return GetSegmentAmount(nextLevel - character._defaultSpeed) * GetStatSegmentPrice(character, type, nextLevel - character._defaultSpeed);
                default:
                    return -1;
            }
        }

        public static int GetStatSegmentPrice(BaseCharacter character, StatType type, int nextLevel)
        {
            switch (type)
            {
                case StatType.Attack:
                    return GetStrengthMulti(character._attackStrength) * GetSegmentPrice(nextLevel - character._defaultAttack);
                case StatType.Defence:
                    return GetStrengthMulti(character._defenceStrength) * GetSegmentPrice(nextLevel - character._defaultDefence);
                case StatType.CharacterSize:
                    return GetStrengthMulti(character._characterSizeStrength) * GetSegmentPrice(nextLevel - character._defaultCharacterSize);
                case StatType.Hp:
                    return GetStrengthMulti(character._hpStrength) * GetSegmentPrice(nextLevel - character._defaultHp);
                case StatType.Speed:
                    return GetStrengthMulti(character._speedStrength) * GetSegmentPrice(nextLevel - character._defaultSpeed);
                default:
                    return -1;
            }
        }

        private static int GetStrengthMulti(ValueStrength type)
        {
            switch (type)
            {
                case ValueStrength.VeryStrong:
                    return 1;
                case ValueStrength.Strong:
                    return 3;
                case ValueStrength.SemiStrong:
                    return 5;
                case ValueStrength.Medium:
                    return 6;
                case ValueStrength.SemiWeak:
                    return 7;
                case ValueStrength.Weak:
                    return 8;
                case ValueStrength.VeryWeak:
                    return 10;
                default:
                    return -1;
            }
        }
        private static int GetSegmentPrice(int nextLevel)
        {
            if (nextLevel < 1) return 0;

            switch (nextLevel)
            {
                case 1:
                    return 5;
                case 2:
                    return 10;
                case 3:
                    return 20;
                case 4:
                    return 30;
                case 5:
                    return 45;
                case 6:
                    return 60;
                case 7:
                    return 80;
                case 8:
                    return 100;
                case 9:
                    return 140;
                case 10:
                    return 200;
                default:
                    return 0;
            }
        }
        public static int GetSegmentAmount(BaseCharacter character, StatType type, int nextLevel)
        {

            switch (type)
            {
                case StatType.Attack:
                    nextLevel = nextLevel - character._defaultAttack;
                    break;
                case StatType.Defence:
                    nextLevel = nextLevel - character._defaultDefence;
                    break;
                case StatType.CharacterSize:
                    nextLevel = nextLevel - character._defaultCharacterSize;
                    break;
                case StatType.Hp:
                    nextLevel = nextLevel - character._defaultHp;
                    break;
                case StatType.Speed:
                    nextLevel = nextLevel - character._defaultSpeed;
                    break;
                default:
                    return -1;
            }
            return GetSegmentAmount(nextLevel);
        }

        private static int GetSegmentAmount(int nextLevel)
        {
            switch (nextLevel)
            {
                case 1:
                    return 5;
                case 2:
                    return 5;
                case 3:
                    return 5;
                case 4:
                    return 10;
                case 5:
                    return 10;
                case 6:
                    return 10;
                case 7:
                    return 15;
                case 8:
                    return 15;
                case 9:
                    return 25;
                case 10:
                    return 35;
                default:
                    return -1;
            }
        }

        public int GetNextLevel(int segmentCount)
        {
            int i = 1;
            while (true)
            {
                int segmentForNext = GetSegmentAmount(i);
                if (segmentForNext > segmentCount) break;
                segmentCount -= segmentForNext;
                i++;
                if(i == 10) break;
            }
            return i;
        }

        #endregion

        public static CharacterClassType GetClass(CharacterID id)
        {
            CharacterClassType ClassType = (CharacterClassType)((((int)id) / 100) * 100);
            return ClassType;
        }
    }
}

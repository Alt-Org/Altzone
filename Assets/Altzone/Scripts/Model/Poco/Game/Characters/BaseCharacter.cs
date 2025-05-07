using System.Runtime.CompilerServices;
using Photon.Deterministic;

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

    public abstract class BaseCharacter
    {
        protected CharacterID _id = CharacterID.None;
        protected int _hp;
        protected int _defaultHp;
        protected ValueStrength _hpStrength = ValueStrength.None;
        protected int _speed;
        protected int _defaultSpeed;
        protected ValueStrength _speedStrength = ValueStrength.None;
        protected int _characterSize;
        protected int _defaultCharacterSize;
        protected ValueStrength _characterSizeStrength = ValueStrength.None;
        protected int _attack;
        protected int _defaultAttack;
        protected ValueStrength _attackStrength = ValueStrength.None;
        protected int _defence;
        protected int _defaultDefence;
        protected ValueStrength _defenceStrength = ValueStrength.None;

        protected bool active = true;

        public CharacterID Id { get => _id;}
        public virtual CharacterClassID ClassID { get => GetClassID(Id); }
        public int Hp { get => _hp;}
        public int DefaultHp { get => _defaultHp; }
        public int Speed { get => _speed;}
        public int DefaultSpeed { get => _defaultSpeed; }
        public int CharacterSize { get => _characterSize;}
        public int DefaultCharacterSize { get => _defaultCharacterSize; }
        public int Attack { get => _attack;}
        public int DefaultAttack { get => _defaultAttack; }
        public int Defence { get => _defence;}
        public int DefaultDefence { get => _defaultDefence; }
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

        #region Stat value getters

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetStatValue(StatType type, int level) => (float)GetStatValueFP(type, level);

        public static FP GetStatValueFP(StatType type, int level)
        {
            return type switch
            {
                StatType.None       => (FP)(-1),
                StatType.Attack     => GetAttackValue(level),
                StatType.Defence    => GetDefenceValue(level),
                StatType.CharacterSize => GetCharacterSizeValue(level),
                StatType.Hp         => GetHpValue(level),
                StatType.Speed      => GetSpeedValue(level),

                _ => (FP)(-1),
            };
        }

        private static FP GetAttackValue(int level)
        {
            return level switch
            {
                 1 =>   5,
                 2 =>  10,
                 3 =>  20,
                 4 =>  30,
                 5 =>  40,
                 6 =>  50,
                 7 =>  60,
                 8 =>  70,
                 9 =>  80,
                10 =>  90,
                11 => 100,
                12 => 110,
                13 => 120,
                14 => 130,
                15 => 140,
                16 => 150,
                17 => 160,
                18 => 170,
                19 => 180,
                20 => 190,
                21 => 200,
                22 => 210,
                23 => 220,
                24 => 230,

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

        private static FP GetSpeedValue(int level)
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

        public static CharacterClassID GetClassID(CharacterID id)
        {
            CharacterClassID ClassId = (CharacterClassID)((((int)id) / 100) * 100);
            return ClassId;
        }
    }
}

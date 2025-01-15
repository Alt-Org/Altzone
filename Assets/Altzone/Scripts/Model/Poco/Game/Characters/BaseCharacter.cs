using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Game
{
    public enum StatType
    {
        None,
        Attack,
        Defence,
        Resistance,
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
        protected int _resistance;
        protected int _defaultResistance;
        protected ValueStrength _resistanceStrength = ValueStrength.None;
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
        public int Resistance { get => _resistance;}
        public int DefaultResistance { get => _defaultResistance; }
        public int Attack { get => _attack;}
        public int DefaultAttack { get => _defaultAttack; }
        public int Defence { get => _defence;}
        public int DefaultDefence { get => _defaultDefence; }
        public ValueStrength HpStrength { get => _hpStrength; }

        protected BaseCharacter()
        {
            InitializeValues();
        }

        protected void InitializeValues()
        {
            _hp = _defaultHp;
            _attack = _defaultAttack;
            _defence = _defaultDefence;
            _resistance = _defaultResistance;
            _speed = _defaultSpeed;
        }

        #region Stat value getters
        public static float GetStatValue(StatType type, int level)
        {
            switch (type)
            {
                case StatType.None:
                    return -1;
                case StatType.Attack:
                    return GetAttackValue(level);
                case StatType.Defence:
                    return GetDefenceValue(level);
                case StatType.Resistance:
                    return GetResistanceValue(level);
                case StatType.Hp:
                    return GetHpValue(level);
                case StatType.Speed:
                    return GetSpeedValue(level);
                default:
                    return -1;
            }
        }
        private static float GetAttackValue(int level)
        {
            return level switch
            {
                1 => 5f,
                2 => 10f,
                3 => 20f,
                4 => 30f,
                5 => 40f,
                6 => 50f,
                7 => 60f,
                8 => 70f,
                9 => 80f,
                10 => 90f,
                11 => 100f,
                12 => 110f,
                13 => 120f,
                14 => 130f,
                15 => 140f,
                16 => 150f,
                17 => 160f,
                18 => 170f,
                19 => 180f,
                20 => 190f,
                21 => 200f,
                22 => 210f,
                23 => 220f,
                24 => 230f,
                _ => -1,
            };
        }

        private static float GetDefenceValue(int level)
        {
            return level switch
            {
                1 => 50f,
                2 => 75f,
                3 => 100f,
                4 => 125f,
                5 => 150f,
                6 => 175f,
                7 => 200f,
                8 => 225f,
                9 => 250f,
                10 => 275f,
                11 => 300f,
                12 => 325f,
                13 => 350f,
                14 => 375f,
                15 => 400f,
                16 => 425f,
                17 => 450f,
                18 => 475f,
                19 => 500f,
                20 => 525f,
                21 => 550f,
                22 => 575f,
                23 => 600f,
                24 => 625f,
                _ => -1,
            };
        }

        private static float GetResistanceValue(int level)
        {
            return level switch
            {
                1 => 4f,
                2 => 4f,
                3 => 4f,
                4 => 6f,
                5 => 6f,
                6 => 6f,
                7 => 8f,
                8 => 8f,
                9 => 8f,
                10 => 8f,
                11 => 10f,
                12 => 10f,
                13 => 10f,
                14 => 10f,
                15 => 12f,
                16 => 12f,
                17 => 12f,
                18 => 12f,
                19 => 14f,
                20 => 14f,
                21 => 14f,
                22 => 16f,
                23 => 16f,
                24 => 16f,
                _ => -1,
            };
        }

        private static float GetHpValue(int level)
        {
            return level switch
            {
                1 => 50f,
                2 => 75f,
                3 => 100f,
                4 => 125f,
                5 => 150f,
                6 => 175f,
                7 => 200f,
                8 => 225f,
                9 => 250f,
                10 => 275f,
                11 => 300f,
                12 => 325f,
                13 => 350f,
                14 => 375f,
                15 => 400f,
                16 => 425f,
                17 => 450f,
                18 => 475f,
                19 => 500f,
                20 => 525f,
                21 => 550f,
                22 => 575f,
                23 => 600f,
                24 => 625f,
                _ => -1,
            };
        }

        private static float GetSpeedValue(int level)
        {
            return level switch
            {
                1 => 4f,
                2 => 4f,
                3 => 4f,
                4 => 6f,
                5 => 6f,
                6 => 6f,
                7 => 8f,
                8 => 8f,
                9 => 8f,
                10 => 8f,
                11 => 10f,
                12 => 10f,
                13 => 10f,
                14 => 10f,
                15 => 12f,
                16 => 12f,
                17 => 12f,
                18 => 12f,
                19 => 14f,
                20 => 14f,
                21 => 14f,
                22 => 16f,
                23 => 16f,
                24 => 16f,
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
                case StatType.Resistance:
                    return GetSegmentAmount(nextLevel - character._defaultResistance) * GetStatSegmentPrice(character, type, nextLevel - character._defaultResistance);
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
                case StatType.Resistance:
                    return GetStrengthMulti(character._resistanceStrength) * GetSegmentPrice(nextLevel - character._defaultResistance);
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
                case StatType.Resistance:
                    nextLevel = nextLevel - character._defaultResistance;
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
            CharacterClassID ClassId = (CharacterClassID)((int)id & 0b1111_1111__0000_0000);
            return ClassId;
        }
    }
}

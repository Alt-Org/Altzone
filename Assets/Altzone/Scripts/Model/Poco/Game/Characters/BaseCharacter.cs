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
            InitilizeValues();
        }

        protected void InitilizeValues()
        {
            _hp = _defaultHp;
            _attack = _defaultAttack;
            _defence = _defaultDefence;
            _resistance = _defaultResistance;
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
            switch (level)
            {
                case 1:
                    return 0.5f;
                case 2:
                    return 1f;
                case 3:
                    return 1.5f;
                case 4:
                    return 2f;
                case 5:
                    return 2.5f;
                case 6:
                    return 3f;
                case 7:
                    return 3.5f;
                case 8:
                    return 4f;
                case 9:
                    return 4.5f;
                case 10:
                    return 5f;
                default:
                    return -1;
            }
        }

        private static float GetDefenceValue(int level)
        {
            switch (level)
            {
                case 1:
                    return 5f;
                case 2:
                    return 4.5f;
                case 3:
                    return 4f;
                case 4:
                    return 3.5f;
                case 5:
                    return 3.25f;
                case 6:
                    return 3f;
                case 7:
                    return 2.75f;
                case 8:
                    return 2.5f;
                case 9:
                    return 2.25f;
                case 10:
                    return 2f;
                default:
                    return -1;
            }
        }

        private static float GetResistanceValue(int level)
        {
            switch (level)
            {
                case 1:
                    return 1f;
                case 2:
                    return 2f;
                case 3:
                    return 3f;
                case 4:
                    return 4f;
                case 5:
                    return 5f;
                case 6:
                    return 6f;
                case 7:
                    return 7f;
                case 8:
                    return 8f;
                case 9:
                    return 9f;
                case 10:
                    return 10f;
                default:
                    return -1;
            }
        }

        private static float GetHpValue(int level)
        {
            if (level < 1) return -1;
            return 50f*level;
        }

        private static float GetSpeedValue(int level)
        {
            switch (level)
            {
                case 1:
                    return 0.25f;
                case 2:
                    return 0.5f;
                case 3:
                    return 0.75f;
                case 4:
                    return 1f;
                case 5:
                    return 1.25f;
                case 6:
                    return 1.5f;
                case 7:
                    return 1.75f;
                case 8:
                    return 2f;
                case 9:
                    return 2.25f;
                case 10:
                    return 2.5f;
                default:
                    return -1;
            }
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
                    nextLevel = GetSegmentPrice(nextLevel - character._defaultAttack);
                    break;
                case StatType.Defence:
                    nextLevel = GetSegmentPrice(nextLevel - character._defaultDefence);
                    break;
                case StatType.Resistance:
                    nextLevel = GetSegmentPrice(nextLevel - character._defaultResistance);
                    break;
                case StatType.Hp:
                    nextLevel = GetSegmentPrice(nextLevel - character._defaultHp);
                    break;
                case StatType.Speed:
                    nextLevel = GetSegmentPrice(nextLevel - character._defaultSpeed);
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

using System.Collections;
using System.Collections.Generic;
using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class BattleCharacterBase
    {
        private int _id;
        private int _characterClassID;

        private FP _hp;
        private FP _speed;
        private FP _resistance;
        private FP _attack;
        private FP _defence;

        public int Id { get => _id; }
        public int CharacterClassID { get => _characterClassID; }
        public FP Hp { get => _hp; }
        public FP Speed { get => _speed; }
        public FP Resistance { get => _resistance; }
        public FP Attack { get => _attack; }
        public FP Defence { get => _defence; }

        public BattleCharacterBase(int id, int classId, FP hp, FP attack, FP defence, FP resistance, FP speed)
        {
            _id = id;
            _characterClassID = classId;
            _hp = hp;
            _attack = attack;
            _defence = defence;
            _resistance = resistance;
            _speed = speed;
        }
    }
}

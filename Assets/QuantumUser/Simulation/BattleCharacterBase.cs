using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public struct BattleCharacterBase
    {
        public int _id;
        public int _characterClassID;

        public FP _hp;
        public FP _speed;
        public FP _resistance;
        public FP _attack;
        public FP _defence;

       /* public int Id { get => _id; }
        public int CharacterClassID { get => _characterClassID; }
        public FP Hp { get => _hp; }
        public FP Speed { get => _speed; }
        public FP Resistance { get => _resistance; }
        public FP Attack { get => _attack; }
        public FP Defence { get => _defence; }*/

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

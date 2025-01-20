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
    }
}

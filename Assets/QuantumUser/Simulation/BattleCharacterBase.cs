using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public struct BattleCharacterBase
    {
        public int Id;
        public int ClassID;

        public FP Hp;
        public FP Speed;
        public FP CharacterSize;
        public FP Attack;
        public FP Defence;
    }
}

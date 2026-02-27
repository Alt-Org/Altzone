using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public struct BattleCharacterBase
    {
        public int Id;
        public int Class;

        public BattlePlayerStats Stats;
    }
}

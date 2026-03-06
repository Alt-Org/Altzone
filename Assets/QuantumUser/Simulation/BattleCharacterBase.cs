using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    public struct BattleCharacterBase
    {
        public BattlePlayerCharacterID Id;
        public BattlePlayerCharacterClass Class;

        public BattlePlayerStats Stats;
    }
}

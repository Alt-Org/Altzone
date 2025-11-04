// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    public class BattlePlayerBotQSpec : AssetObject
    {
        public FP MovementCooldownSecMin;
        public FP MovementCooldownSecMax;
        public FP LookAheadTimeSec;
    }
}

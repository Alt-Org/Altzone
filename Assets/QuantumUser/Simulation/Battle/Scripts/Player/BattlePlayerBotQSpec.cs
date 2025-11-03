using Photon.Deterministic;
using Quantum;

namespace Battle.QSimulation.Player
{
    public class BattlePlayerBotQSpec : AssetObject
    {
        public FP MovementCooldownSecMin;
        public FP MovementCooldownSecMax;
        public FP LookAheadTimeSec;
    }
}

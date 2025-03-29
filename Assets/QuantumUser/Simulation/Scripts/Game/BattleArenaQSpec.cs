using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Game
{
    public class BattleArenaQSpec : AssetObject
    {
        public FP WorldWidth;
        public FP WorldHeight;
        public int GridWidth;
        public int GridHeight;
        public int MiddleAreaHeight;
        public int SoulWallHeight;
        public BattleGridPosition[] PlayerSpawnPositions;
    }
}

using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class BattleArenaSpec : AssetObject
    {
        public FP WorldWidth;
        public FP WorldHeight;
        public int GridWidth;
        public int GridHeight;
        public int MiddleAreaHeight;
        public int SoulWallHeight;
        public GridPosition[] PlayerSpawnPositions;
    }
}

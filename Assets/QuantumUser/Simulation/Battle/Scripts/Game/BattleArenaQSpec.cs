/// @file BattleArenaQSpec.cs
/// <summary>
/// Defines arena specifications and layout used during battle simulations.
/// </summary>
/// 
/// Stores physical dimensions, grid configuration, and spawn templates for use in the Quantum simulation.

using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Game
{
    /// <summary>
    /// Asset that defines the battlefield layout and settings.
    /// Includes dimensions, grid size, and template references for player spawns and SoulWalls.
    /// </summary>
    public class BattleArenaQSpec : AssetObject
    {
        /// <value>Width of the playable arena in world units.</value>
        public FP WorldWidth;
        /// <value>Height of the playable arena in world units.</value>
        public FP WorldHeight;
        /// <value>Number of columns in grid units.</value>
        public int GridWidth;
        /// <value>Number of rows in grid units.</value>
        public int GridHeight;
        /// <value>Vertical space reserved in the center area in grid units.</value>
        public int MiddleAreaHeight;
        /// <value>Height of the center area in grid units.</value>
        public int SoulWallHeight;
        /// <value>Array of spawn <see cref="Quantum.BattleGridPosition">grid positions</see> for each player.</value>
        public BattleGridPosition[] PlayerSpawnPositions;
        /// <value><see cref="Quantum.BattleSoulWallTemplate">Templates defining SoulWall segments</see> for Team Alpha.</value>
        public BattleSoulWallTemplate[] SoulWallTeamAlphaTemplates;
        /// <value><see cref="Quantum.BattleSoulWallTemplate">Templates defining SoulWall segments</see> for Team Beta.</value>
        public BattleSoulWallTemplate[] SoulWallTeamBetaTemplates;
    }
}

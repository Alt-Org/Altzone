/// @file BattleArenaQSpec.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Game,BattleArenaQSpec} class for defining the battlearena layout, settings and spawn positions.
/// </summary>
///
/// @bigtext{Filled with data from @ref BattleArenaQSpec.asset "BattleArenaQSpec" data asset.}

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Game
{
    /// <summary>
    /// Class for defining the battlearena layout, settings and spawn positions.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.<br/>
    /// Can be used to make multiple %BattleArenaQSpec data assets for different arenas.<br/>
    /// @bigtext{Filled with data from @ref BattleArenaQSpec.asset "BattleArenaQSpec" data asset.}
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

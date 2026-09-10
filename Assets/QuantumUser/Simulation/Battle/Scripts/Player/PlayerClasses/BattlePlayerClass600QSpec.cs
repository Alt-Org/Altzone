/// @file BattlePlayerClass600QSpec.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClass600QSpec} class for defining settings related to the 600 player class.
/// </summary>
///
/// @bigtext{Filled with data from @ref BattlePLayerClass600QSpec.asset "BattlePlayerClass600QSpec" data asset.}

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Class for defining settings related to the 600 player class.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.<br/>
    /// @bigtext{Filled with data from @ref BattlePlayerClass600QSpec.asset "BattlePlayerClass600QSpec" data asset.}
    public class BattlePlayerClass600QSpec : AssetObject
    {
        [Tooltip("Minimum time in seconds the projectile is held before releasing.")]
        /// <summary>Minimum time in seconds the projectile is held before releasing.</summary>
        public FP HoldMinDurationSec;

        [Tooltip("Maximum time in seconds the projectile can be held before releasing.")]
        /// <summary>Maximum time in seconds the projectile can be held before releasing.</summary>
        public FP HoldMaxDurationSec;

        [Tooltip("Time in seconds the player can stay still before releasing the projectile.")]
        /// <summary>Time in seconds the player can stay still before releasing the projectile.</summary>
        public FP ReleaseBufferSec;
    }
}

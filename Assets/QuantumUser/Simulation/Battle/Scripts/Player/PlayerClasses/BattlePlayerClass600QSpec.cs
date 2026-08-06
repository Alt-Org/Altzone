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
        [Tooltip("Time in seconds before projectile is released if not moving.")]
        /// <summary>Time in seconds before projectile is released if not moving.</summary>
        public FP HoldMinDurationSec;

        [Tooltip("Time in seconds before projectile is released if moving.")]
        /// <summary>Time in seconds before projectile is released if moving.</summary>
        public FP HoldMaxDurationSec;

        [Tooltip("Time in seconds before projectile is released after stopping.")]
        /// <summary>Time in seconds before projectile is released after stopping.</summary>
        public FP ReleaseBufferSec;
    }
}

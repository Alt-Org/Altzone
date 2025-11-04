/// @file BattlePlayerQSpec.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerQSpec} class for defining constants used in Player code.
/// </summary>
///
/// @bigtext{Filled with data from @ref BattlePlayerQSpec.asset "BattlelayerQSpec" data asset.}

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Class for defining player constants.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.<br/>
    /// Can be used to make multiple %BattlePlayerQSpec data assets.<br/>
    /// @bigtext{Filled with data from @ref BattlePlayerQSpec.asset "BattlePlayerQSpec" data asset.}
    public class BattlePlayerQSpec : AssetObject
    {
        [Header("Universal player constants")]

        [Tooltip("Rotation speed of all player characters")]
        /// <value>Rotation speed of all player characters.</value>
        public FP RotationSpeed;

        [Tooltip("Maximum angle in degrees a player character can be rotated to in either direction")]
        /// <value>Maximum angle in degrees a player character can be rotated to in either direction.</value>
        public FP MaxRotationAngleDeg;

        [Tooltip("Time in seconds before player or shield can take damage again")]
        /// <value>Time in seconds before player can take damage again.</value>
        public FP DamageCooldownSec;

        [Tooltip("Time in seconds before a new character is automatically spawned after one dies")]
        /// <value>Time in seconds before a new character is automatically spawned after one dies.</value>
        public FP AutoRespawnTimeSec;
    }
}

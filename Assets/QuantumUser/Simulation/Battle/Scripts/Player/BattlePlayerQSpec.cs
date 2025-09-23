/// @file BattlePlayerQSpec.cs
/// <summary>
/// Class for defining constants used in Player code.
/// </summary>
///
/// @bigtext{Filled with data from @ref BattlePlayerQSpec.asset "BattlelayerQSpec" data asset.}

using UnityEngine;

using Photon.Deterministic;
using Quantum;

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

        [Tooltip("Maximum angle a player character can be rotated to in either direction")]
        /// <value>Maximum angle a player character can be rotated to in either direction.</value>
        public FP MaxRotationAngle;

        [Tooltip("Time before player or shield can take damage again")]
        /// <value>Time before player can take damage again.</value>
        public FP DamageCooldownSec;

        [Tooltip("Time before a new character is automatically spawned after one dies")]
        /// <value>Time before a new character is automatically spawned after one dies.</value>
        public FP AutoRespawnTimeSec;
    }
}

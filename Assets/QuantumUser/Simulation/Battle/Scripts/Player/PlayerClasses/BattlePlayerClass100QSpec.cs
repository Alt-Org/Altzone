/// @file BattlePlayerClass100QSpec.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClass100QSpec} class for defining settings related to the 100 player class.
/// </summary>
///
/// @bigtext{Filled with data from @ref BattlePLayerClass100QSpec.asset "BattlePlayerClass100QSpec" data asset.}

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Class for defining settings related to the 100 player class.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.<br/>
    /// @bigtext{Filled with data from @ref BattlePlayerClass100QSpec.asset "BattlePlayerClass100QSpec" data asset.}
    public class BattlePlayerClass100QSpec : AssetObject
    {
        [Tooltip("Player class 100 projectile entity prototpe")]
        /// <summary>Player class 100 projectile entity prototpe.</summary>
        public AssetRef<EntityPrototype> ProjectileEntityPrototype;
        [Tooltip("Speed of the projectile")]
        /// <summary>Speed of the projectile.</summary>
        public FP ProjectileSpeed;
        [Tooltip("Distance of how far the projectile is spawned from the player")]
        /// <summary>Distance of how far the projectile is spawned from the player</summary>
        public FP ProjectileSpawnDistance;
        [Tooltip("Time the projectile cannot be spawned again for after spawning one")]
        /// <summary>Time the projectile cannot be spawned again for after spawning one</summary>
        public FP ProjectileSpawnCooldown;
        [Tooltip("Maximum duration for pressing to register as a tap instead of hold")]
        /// <summary>Maximum duration for pressing to register as a tap instead of hold.</summary>
        public FP JoystickTapDurationMax;
        [Tooltip("Maximum distance from joystick center for a press to register as pressing the middle")]
        /// <summary>Maximum distance from joystick center for a press to register as pressing the middle</summary>
        public FP JoystickTapDistanceMax;
        [Tooltip("Time in seconds before character stops moving completely.")]
        /// <summary>Time in seconds before character stops moving completely.</summary>
        public FP PlacementTimeDurationSec;
    }
}

/// @file BattlePlayerClassDesensitizerQSpec.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClassDesensitizerQSpec} class for defining settings related to the desensitizer player class.
/// </summary>
///
/// @bigtext{Filled with data from @ref BattlePLayerClassDesensitizerQSpec.asset "BattlePlayerClassDesensitizerQSpec" data asset.}

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Class for defining settings related to the desensitizer player class.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.<br/>
    /// @bigtext{Filled with data from @ref BattlePlaerClassDesensitizerQSpec.asset "BattlePlayerClassDesensitizerQSpec" data asset.}
    public class BattlePlayerClassDesensitizerQSpec : AssetObject
    {
        [Tooltip("Desensitizer projectile entity prototpe")]
        /// <summary>Desensitizer projectile entity prototpe.</summary>
        public AssetRef<EntityPrototype> ProjectileEntityPrototype;
        [Tooltip("Speed of the projectile")]
        /// <summary>Speed of the projectile.</summary>
        public FP Speed;
        [Tooltip("Maximum duration for pressing to register as a tap instead of hold")]
        /// <summary>Maximum duration for pressing to register as a tap instead of hold.</summary>
        public FP JoystickTapDurationMax;
        [Tooltip("Maximum distance from joystick center for a press to register as pressing the middle")]
        /// <summary>Maximum distance from joystick center for a press to register as pressing the middle</summary>
        public FP JoystickFlickDistanceMax;
        [Tooltip("Distance of how far the projectile is spawned from the player")]
        /// <summary>Distance of how far the projectile is spawned from the player</summary>
        public FP SpawnDistance;
        [Tooltip("Time the projectile cannot be launched again for after launching one")]
        /// <summary>Time the projectile cannot be launched again for after launching one</summary>
        public FP Cooldown;
    }
}

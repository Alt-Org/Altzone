/// @file BattleProjectileQSpec.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Projectile,BattleProjectileQSpec} class for defining the projectile's initial values.
/// </summary>
///
/// @bigtext{Filled with data from @ref BattleProjectileQSpec.asset "BattleProjectileQSpec" data asset.}

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Projectile
{
    /// <summary>
    /// Class for defining the projectile's initial values.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.<br/>
    /// Can be used to make multiple %BattleProjectileQSpec data assets.<br/>
    /// @bigtext{Filled with data from @ref BattleProjectileQSpec.asset "BattleProjectileQSpec" data asset.}
    public class BattleProjectileQSpec: AssetObject
    {
        [Header("Projectile configuration")]

        [Tooltip("Prototype reference to spawn projectile")]
        /// <value>EntityPrototype of the projectile.</value>
        public AssetRef<EntityPrototype> ProjectilePrototype;

        [Tooltip("Speed applied to the projectile when spawned")]
        /// <value>Speed applied to the projectile when it's spawned.</value>
        public FP ProjectileInitialSpeed;

        [Tooltip("Speed multiplier of the projectile depends from BattleEmotionState")]
        /// <value>An array holding values for multiplying speed depending on the projectile's emotion state.</value>
        public FP[] SpeedMultiplierArray;

        [Tooltip("Amount which speedpotential is increased")]
        /// <value>Amount which SpeedPotential is increased.</value>
        public FP SpeedIncrement;

        [Tooltip("Maximum speed the projectile can reach")]
        /// <value>Maximum speed the projectile can reach.</value>
        public FP SpeedMax;

        [Tooltip("Timer for speeding up the ball")]
        /// <value>Timer for speeding up the ball.</value>
        public FP AccelerationTimerDuration;

        [Tooltip("Amount of projectiles spawned in")]
        /// <value>Amount of projectiles spawned in.</value>
        public int InitialProjectilesCount;

        [Tooltip("Projectile's maximum attack damage")]
        /// <value>Projectile's maximum attack damage.</value>
        public FP AttackMax;
    }
}

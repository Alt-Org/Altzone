/// @file BattleProjectileQSpec.cs
/// <summary>
/// Class for defining the projectile's initial values.
/// </summary>
///
/// @bigtext{Filled with data from @ref BattleProjectileQSpec.asset "BattleProjectileQSpec" data asset.}

using UnityEngine;

using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Projectile
{
    /// <summary>
    /// Class for defining the projectile's initial values.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.  
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
        public FP[] SpeedMultiplierArray;

        [Tooltip("Amount which speedpotential is increased")]
        public FP SpeedIncrement;

        [Tooltip("Timer for speeding up the ball")]
        public FP AccelerationTimerDuration;

        [Tooltip("Amount of projectiles spawned in")]
        /// <value>Amount of projectiles spawned in.</value>
        public int InitialProjectilesCount;

        [Tooltip("Projectile's maximum attack damage")]
        public FP AttackMax;
    }
}

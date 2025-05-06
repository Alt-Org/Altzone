/// @file BattleProjectileQSpec.cs
/// <summary>
/// Defines projectile's specifications.
/// </summary>
///
/// This spec defines projectile's Entity Prototype, initial speed and amount.

using UnityEngine;

using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Projectile
{
    /// <summary>
    /// Asset that defines projectile's initial values.
    /// </summary>
    public class BattleProjectileQSpec: AssetObject
    {
        [Header("Projectile configuration")]
        [Tooltip("Prototype reference to spawn projectile")]
        /// <value>EntityPrototype of the projectile.</value>
        public AssetRef<EntityPrototype> ProjectilePrototype;
        [Tooltip("Speed applied to the projectile when spawned")]
        /// <value>Speed applied to the projectile when it's spawned.</value>
        public FP ProjectileInitialSpeed;
        [Tooltip("Amount of projectiles spawned in")]
        /// <value>Amount of projectiles spawned in.</value>
        public int InitialProjectilesCount;
    }
}

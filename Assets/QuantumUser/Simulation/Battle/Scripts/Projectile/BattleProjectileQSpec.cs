using UnityEngine;

using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Projectile
{
    public class BattleProjectileQSpec: AssetObject
    {
        [Header("Projectile configuration")]
        [Tooltip("Prototype reference to spawn projectile")]
        public AssetRef<EntityPrototype> ProjectilePrototype;
        [Tooltip("Speed applied to the projectile when spawned")]
        public FP ProjectileInitialSpeed;
        [Tooltip("Speed multiplier of the projectile depends from BattleEmotionState")]
        public FP[] SpeedMultiplierArray;
        [Tooltip("Amount which speedpotential is increased")]
        public FP SpeedIncrement;
        [Tooltip("Timer for speeding up the ball")]
        public FP AccelerationTimerDuration;
        [Tooltip("Amount of projectile spawned in ")]
        public int InitialProjectilesCount;
        [Tooltip("Projectile's maximum attack damage")]
        public FP AttackMax;
    }
}

using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class ProjectileSpec: AssetObject
    {
        [Header("Projectile configuration")]
        [Tooltip("Prototype reference to spawn projectile")]
        public AssetRef<EntityPrototype> ProjectilePrototype;
        [Tooltip("Speed applied to the projectile when spawned")]
        public FP ProjectileInitialSpeed;
        [Tooltip("Amount of projectile spawned in ")]
        public int InitialProjectilesCount;
    }
}

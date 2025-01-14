using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public class ProjectileGameConfig : AssetObject
    {
            [Header("Projectile configuration")]
            [Tooltip("Prototype reference to spawn projectile")]
            public AssetRef<EntityPrototype> ProjectilePrototype;
            [Tooltip("Speed applied to the projectile when spawned")]
            public FP ProjectileInitialSpeed = 8;
            [Tooltip("Minimum torque applied to the projectile when spawned")]
            public FP ProjectileInitialTorqueMin = 0;
            [Tooltip("Maximum torque applied to the projectile when spawned")]
            public FP ProjectileInitialTorqueMax = 0;
            [Tooltip("Distance to the center of the map. This value is the radius in a random circular location where the projectile is spawned")]
            public FP ProjectileSpawnDistanceToCenter = 0;
            [Tooltip("Amount of projectile spawned in ")]
            public int InitialProjectilesCount = 1;
    }
}

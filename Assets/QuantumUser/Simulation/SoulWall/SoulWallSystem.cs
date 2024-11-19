using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.QuantumUser.Simulation.SoulWall
{
    [Preserve]
    public unsafe class SoulWallSystem : SystemMainThread, ISignalOnCollisionProjectileHitSoulWall
    {

        public struct Filter
        {
            public EntityRef Entity;
            public Quantum.Projectile* Projectile;
        }

        public override void Update(Frame f)
        {
        }


        public void OnCollisionProjectileHitSoulWall(Frame f, CollisionInfo2D info, Quantum.Projectile* projectile, Quantum.SoulWall* soulWall)
        {
            Debug.Log("Soul wall hit");

            // Get the entity reference of the SoulWall involved in the collision
            var soulWallEntity = info.Other;

            // Destroy the SoulWall entity
            f.Destroy(soulWallEntity);
        }
    }
}

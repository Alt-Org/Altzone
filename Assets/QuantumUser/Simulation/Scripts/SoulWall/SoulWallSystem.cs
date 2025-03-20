using UnityEngine;
using UnityEngine.Scripting;

using Quantum.QuantumUser.Simulation.Projectile;

namespace Quantum.QuantumUser.Simulation.SoulWall
{
    [Preserve]
    public unsafe class SoulWallSystem : SystemSignalsOnly, ISignalOnTriggerProjectileHitSoulWall
    {
        public void OnTriggerProjectileHitSoulWall(Frame f, Quantum.Projectile* projectile, EntityRef projectileEntity, Quantum.SoulWall* soulWall, EntityRef soulWallEntity)
        {
            Debug.Log("Soul wall hit");

            if (ProjectileSystem.IsCollisionFlagSet(f, projectile, ProjectileCollisionFlags.SoulWall)) return;

            // Destroy the SoulWall entity
            f.Events.PlaySoundEvent(SoundEffect.WallBroken);
            f.Destroy(soulWallEntity);

            ProjectileSystem.SetCollisionFlag(f, projectile, ProjectileCollisionFlags.SoulWall);
        }
    }
}

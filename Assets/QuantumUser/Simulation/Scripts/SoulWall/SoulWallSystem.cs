using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.QuantumUser.Simulation.SoulWall
{
    [Preserve]
    public unsafe class SoulWallSystem : SystemSignalsOnly, ISignalOnTriggerProjectileHitSoulWall
    {
        public void OnTriggerProjectileHitSoulWall(Frame f, Quantum.Projectile* projectile, EntityRef projectileEntity, Quantum.SoulWall* soulWall, EntityRef soulWallEntity)
        {
            Debug.Log("Soul wall hit");
            // Destroy the SoulWall entity
            if (projectile->CoolDown <= 0)
            {
                f.Events.PlaySoundEvent(SoundEffect.WallBroken);
                f.Destroy(soulWallEntity);
            }
        }
    }
}

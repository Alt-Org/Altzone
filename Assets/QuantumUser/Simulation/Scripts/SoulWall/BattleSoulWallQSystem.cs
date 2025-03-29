using UnityEngine;
using UnityEngine.Scripting;

using Quantum;

using Battle.QSimulation.Projectile;

namespace Battle.QSimulation.SoulWall
{
    [Preserve]
    public unsafe class BattleSoulWallQSystem : SystemSignalsOnly, ISignalOnTriggerProjectileHitSoulWall
    {
        public void OnTriggerProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall, EntityRef soulWallEntity)
        {
            Debug.Log("Soul wall hit");

            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.SoulWall)) return;

            // Destroy the SoulWall entity
            f.Events.PlaySoundEvent(SoundEffect.WallBroken);
            f.Destroy(soulWallEntity);

            BattleProjectileQSystem.SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.SoulWall);
        }
    }
}

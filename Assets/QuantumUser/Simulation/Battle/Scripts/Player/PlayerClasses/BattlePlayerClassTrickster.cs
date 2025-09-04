using Battle.QSimulation.Projectile;
using Photon.Deterministic;
using Quantum;

namespace Battle.QSimulation.Player
{
    public class BattlePlayerClassTrickster : BattlePlayerClassBase<BattlePlayerClassTricksterDataQComponent>
    {
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Trickster;

        public override unsafe void OnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            FPVector2 normal = playerHitbox->Normal;
            FP randomAngleRangeDeg = GetClassData(f, playerHitbox->PlayerEntity)->randomAngleRangeDeg;
            FP randomAngleRad = f.RNG->NextInclusive(-randomAngleRangeDeg / FP._2, randomAngleRangeDeg / FP._2) * FP.Deg2Rad;
            
            normal = FPVector2.Rotate(normal, randomAngleRad);

            BattleProjectileQSystem.HandleIntersection(f, projectile, projectileEntity, playerHitboxEntity, normal, playerHitbox->CollisionMinOffset);
            BattleProjectileQSystem.UpdateVelocity(f, projectile, normal);
        }
    }
}

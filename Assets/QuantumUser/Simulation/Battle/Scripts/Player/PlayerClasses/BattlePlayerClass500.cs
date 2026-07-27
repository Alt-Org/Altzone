// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;
using Battle.QSimulation.Projectile;

namespace Battle.QSimulation.Player
{
    public class BattlePlayerClass500 : BattlePlayerClassBase
    {
        /// <summary>The BattlePlayerCharacterClass this class is for.</summary>
        public override BattlePlayerCharacterClass Class { get; } = BattlePlayerCharacterClass.Class500;

        /// <summary>
        /// Called by BattlePlayerClassManager.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        public override unsafe void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldCollisionData)
        {
            BattleArenaQSpec spec = BattleQConfig.GetArenaSpec(f);

            EntityRef                   projectileRef   = projectileCollisionData->ProjectileEntityRef;
            BattleProjectileQComponent* projectile      = projectileCollisionData->Projectile;
            BattlePlayerEntityRef       playerEntityRef = f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(shieldCollisionData->PlayerShieldHitbox->ParentEntityRef)->PlayerEntityRef;
            BattlePlayerDataQComponent* playerData      = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerEntityRef);

            FP radius = projectileCollisionData->Projectile->Radius;

            BattleTeamNumber teamNumber = BattlePlayerManager.PlayerHandle.GetTeamNumber(playerData->Slot);

            int row = 0;

            FPVector2 direction = FPVector2.Zero;

            FPVector2 offset = new() { Y = radius + BattleGridManager.GridScaleFactor };

            switch (teamNumber)
            {
                case BattleTeamNumber.TeamAlpha:
                    row = 0 + spec.SoulWallHeight;
                    direction = FPVector2.Up;
                    break;
                case BattleTeamNumber.TeamBeta:
                    row = spec.GridHeight - spec.SoulWallHeight;
                    direction = FPVector2.Down;
                    offset = -offset;
                    break;
            };

            BattleGridPosition gridPosition = new()
            {
                Col = spec.GridWidth / 2,
                Row = row
            };

            FPVector2 position = BattleGridManager.GridPositionToWorldPosition(gridPosition) + offset;

            direction.X = f.RNG->NextInclusive(-FP._0_50, FP._0_50);

            BattleEntityManager.TeleportCompound(f, projectileRef, position, FP._0);

            BattleProjectileQSystem.UpdateVelocity(f, projectile, direction, BattleProjectileQSystem.SpeedChange.Increment);
        }
    }
}

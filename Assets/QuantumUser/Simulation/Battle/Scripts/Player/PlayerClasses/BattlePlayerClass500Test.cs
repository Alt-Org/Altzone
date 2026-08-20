/// @file BattlePlayerClass500Test.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClass500Test} class which handles player character class logic for the 500/Reflector class.
/// </summary>

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;
using Battle.QSimulation.Projectile;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// %Player character class logic for the 500 class.
    /// </summary>
    ///
    /// @bigtext{See [{PlayerClass}](#page-concepts-player-simulation-class-playerclass) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext{See [{Player Character Class 500 - Reflector}](#page-concepts-player-class-500) for more info.}<br/>
    public class BattlePlayerClass500Test : BattlePlayerClassBase
    {
        /// <summary>The BattlePlayerCharacterClass this class is for.</summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Class500;

        public override unsafe BattlePlayerClassManager.CreationParameters OnCreate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            BattleCompoundEntityQComponent* compoundEntityComponent = f.Unsafe.GetPointer<BattleCompoundEntityQComponent>(playerEntity);

            foreach(BattleEntityLink link in f.ResolveList(compoundEntityComponent->LinkedEntities))
            {
                BattlePlayerHitboxQComponent* hitBoxComponent = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(link.ERef);
                hitBoxComponent->CollisionType = BattlePlayerCollisionType.None;
            }

            return BattlePlayerClassManager.CreationParameters.Default;
        }

        /// <summary>
        /// Called by BattlePlayerClassManager. Teleports projectile to the soulwall
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        public override unsafe void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldCollisionData)
        {
            BattleArenaQSpec spec = BattleQConfig.GetArenaSpec(f);

            EntityRef                   projectileEntityRef   = projectileCollisionData->ProjectileEntityRef;
            BattleProjectileQComponent* projectile            = projectileCollisionData->Projectile;
            BattlePlayerEntityRef       playerEntityRef       = f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(shieldCollisionData->PlayerShieldHitbox->ParentEntityRef)->PlayerEntityRef;
            BattlePlayerDataQComponent* playerData            = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerEntityRef);

            int       row       = 0;
            FP        yOffset   = projectile->Radius + BattleGridManager.GridScaleFactor;
            FPVector2 direction = FPVector2.Zero;

            switch (playerData->TeamNumber)
            {
                case BattleTeamNumber.TeamAlpha:
                    row       = 0 + spec.SoulWallHeight;
                    direction = FPVector2.Up;
                    break;
                case BattleTeamNumber.TeamBeta:
                    row       = spec.GridHeight - spec.SoulWallHeight;
                    yOffset   = -yOffset;
                    direction = FPVector2.Down;
                    break;
            };

            FPVector2 position = new(
                0,
                BattleGridManager.GridRowToWorldYPosition(row) + yOffset
            );

            position.Y += yOffset;

            direction = FPVector2.Rotate(direction, f.RNG->NextInclusive(-FP.Rad_45, FP.Rad_45));

            BattleEntityManager.TeleportCompound(f, projectileEntityRef, position, FP._0);
            BattleProjectileQSystem.UpdateVelocity(f, projectile, direction, BattleProjectileQSystem.SpeedChange.Increment);
        }
    }
}

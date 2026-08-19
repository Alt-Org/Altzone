/// @file BattlePlayerClass600.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClass600} class which handles player character class logic for the 600/Confluent class.
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
    /// %Player character class logic for the 600/Confluent class.
    /// </summary>
    ///
    /// @bigtext{See [{PlayerClass}](#page-concepts-player-simulation-class-playerclass) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext{See [{Player Character Class 600 - Confluent}](#page-concepts-player-class-600) for more info.}
    public class BattlePlayerClass600 : BattlePlayerClassBase<BattlePlayerClass600DataQComponent>
    {
        /// <summary>The BattlePlayerCharacterClass this class is for.</summary>
        public override BattlePlayerCharacterClass Class { get; } = BattlePlayerCharacterClass.Class600;

        /// <summary>
        /// Called by BattlePlayerClassManager. Holds the projectile where it hits.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        public override unsafe void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldCollisionData)
        {
            BattlePlayerShieldDataQComponent* playerShieldData = f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(shieldCollisionData->PlayerShieldHitbox->ParentEntityRef);

            if (projectileCollisionData->Projectile->IsHeld) return;
            if (projectileCollisionData->Projectile->EmotionCurrent == BattleEmotionState.Love) return;
            if (shieldCollisionData->IsLoveProjectileCollision) return;

            Transform2D* transformProjectile = f.Unsafe.GetPointer<Transform2D>(projectileCollisionData->ProjectileEntityRef);
            Transform2D* transformShield     = ((BattlePlayerShieldEntityRef)shieldCollisionData->PlayerShieldHitbox->ParentEntityRef).GetTransform(f);

            FPVector2 normal = (transformProjectile->Position - transformShield->Position).Normalized;

            BattleProjectileQSystem.HandleIntersection(f,
                projectileCollisionData->Projectile,
                projectileCollisionData->ProjectileEntityRef,
                projectileCollisionData->OtherEntityRef,
                normal,
                shieldCollisionData->PlayerShieldHitbox->CollisionMinOffset * FP._1_10
            );

            if (!playerShieldData->IsAttached)
            {
                FPVector2 direction = FPVector2.Reflect(projectileCollisionData->Projectile->Direction, normal).Normalized;

                BattleProjectileQSystem.UpdateVelocity(f,
                    projectileCollisionData->Projectile,
                    direction,
                    BattleProjectileQSystem.SpeedChange.Increment
                );
                return;
            }

            BattlePlayerClass600QSpec spec = BattleQConfig.GetBattlePlayerClass600Spec(f);

            BattlePlayerEntityRef               playerEntityRef  = playerShieldData->PlayerEntityRef;
            BattlePlayerClass600DataQComponent* classData        = GetClassData(f, playerEntityRef);

            classData->IsHoldingProjectile = true;

            BattleProjectileQSystem.SetHeld(projectileCollisionData->Projectile, true);

            classData->HeldProjectileEntity = projectileCollisionData->ProjectileEntityRef;
            classData->HeldProjectileOffset = transformProjectile->Position - transformShield->Position;

            classData->HoldMinTimer = FrameTimer.FromSeconds(f, spec.HoldMinDurationSec);
            classData->HoldMaxTimer = FrameTimer.FromSeconds(f, spec.HoldMaxDurationSec);
        }

        /// <summary>
        /// Called every frame to update the player. Moves the projectile when it is held, and releases it based on timers.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle for the player.</param>
        /// <param name="playerData">Pointer to player data.</param>
        /// <param name="playerEntity">Entity reference for the player.</param>
        /// <param name="specialInput">Pointer to special input (unused)</param>
        public override unsafe void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, BattlePlayerEntityRef playerEntity, BattleSpecialInput* specialInput)
        {
            BattlePlayerClass600QSpec spec = BattleQConfig.GetBattlePlayerClass600Spec(f);

            BattlePlayerClass600DataQComponent* classData = GetClassData(f, playerEntity);

            if (!classData->IsHoldingProjectile) return;

            playerHandle.AllowCharacterSwapping = false;

            Transform2D* transformShield = f.Unsafe.GetPointer<Transform2D>(playerData->AttachedShield);
            Transform2D* transformProjectile = f.Unsafe.GetPointer<Transform2D>(classData->HeldProjectileEntity);

            FPVector2 projectilePositionNext = transformShield->Position + classData->HeldProjectileOffset;

            BattleEntityManager.MoveCompound(f, classData->HeldProjectileEntity, projectilePositionNext, FP._0);

            if (transformShield->Position != classData->PreviousPosition)
            {
                classData->ReleaseBufferTimer = FrameTimer.FromSeconds(f, spec.ReleaseBufferSec);
            }

            classData->PreviousPosition = transformShield->Position;

            if (classData->HoldMinTimer.IsRunning(f)) return;

            if (classData->HoldMaxTimer.IsRunning(f) && classData->ReleaseBufferTimer.IsRunning(f)) return;

            BattleTeamNumber teamNumber = BattlePlayerManager.PlayerHandle.GetTeamNumber(playerHandle.Slot);

            BattlePlayerShieldDataQComponent* shieldData = f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(playerData->AttachedShield);
            BattleProjectileQComponent*       projectile = f.Unsafe.GetPointer<BattleProjectileQComponent>(classData->HeldProjectileEntity);

            FPVector2 direction = FPVector2.Zero;

            switch (teamNumber)
            {
                case BattleTeamNumber.TeamAlpha:
                    direction = FPVector2.Up;
                    break;
                case BattleTeamNumber.TeamBeta:
                    direction = FPVector2.Down;
                    break;
            }

            BattleProjectileQSystem.UpdateVelocity(f, projectile, direction, BattleProjectileQSystem.SpeedChange.Increment);

            BattleProjectileQSystem.SetHeld(projectile, false);

            classData->IsHoldingProjectile = false;

            playerHandle.AllowCharacterSwapping = true;
        }
    }
}

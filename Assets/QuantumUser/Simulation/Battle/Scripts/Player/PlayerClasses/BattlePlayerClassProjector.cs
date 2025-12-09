/// @file BattlePlayerClassProjector.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClassProjector} class which handles player character class logic for the 400/Projector class.
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
    /// %Player character class logic for the 400/Projector class.
    /// </summary>
    ///
    /// @bigtext{See [{PlayerClass}](#page-concepts-player-simulation-playerclass) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext{See [{Player Character Class 400 - Projector}](#page-concepts-player-class-400-projector) for more info.}
    public class BattlePlayerClassProjector : BattlePlayerClassBase<BattlePlayerClassProjectorDataQComponent>
    {
        /// <summary>
        /// Gets the character class associated with this Class.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.Projector">BattlePlayerCharacterClass.Projector</see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Projector;

        /// <summary>
        /// Called when the player is created.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle for the player.</param>
        /// <param name="playerData">Pointer to player data.</param>
        /// <param name="playerEntity">Entity reference for the player.</param>
        public override unsafe void OnCreate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            BattlePlayerClassProjectorDataQComponent* data = GetClassData(f, playerEntity);
        }

        /// <summary>
        /// Called when a projectile hits a player shield.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        public override unsafe void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldCollisionData)
        {
            if (!shieldCollisionData->PlayerShieldHitbox->IsActive) return;
            if (projectileCollisionData->Projectile->IsHeld) return;

            BattlePlayerClassProjectorDataQComponent* data = GetClassData(f, shieldCollisionData->PlayerShieldHitbox->ParentEntity);
            bool grab = !projectileCollisionData->Projectile->IsPassed;

            if (grab)
            {
                BattlePlayerManager.PlayerHandle teammateHandle = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, f.Unsafe.GetPointer<BattlePlayerDataQComponent>(shieldCollisionData->PlayerShieldHitbox->ParentEntity)->Slot);
                if (!teammateHandle.PlayState.IsInPlay()) grab = false;
            }
            if (grab)
            {
                Transform2D* transformPlayer = f.Unsafe.GetPointer<Transform2D>(shieldCollisionData->PlayerShieldHitbox->ParentEntity);
                Transform2D* transformProjectile = f.Unsafe.GetPointer<Transform2D>(projectileCollisionData->ProjectileEntity);

                FPVector2 toProjectile = transformProjectile->Position - transformPlayer->Position;

                BattleProjectileQSystem.SetHeld(f, projectileCollisionData->Projectile, true);
                data->IsHoldingProjectile = true;
                data->HeldProjectileEntity = projectileCollisionData->ProjectileEntity;
                data->HeldProjectileAngleRadians = FPVector2.RadiansSigned(FPVector2.Up, toProjectile);
                data->HeldProjectileDistance = toProjectile.Magnitude;
                data->HoldStartFrame = f.Number;
            }
            else
            {
                BattleProjectileQSystem.HandleIntersection(f, projectileCollisionData->Projectile, projectileCollisionData->ProjectileEntity, projectileCollisionData->OtherEntity, shieldCollisionData->PlayerShieldHitbox->Normal, shieldCollisionData->PlayerShieldHitbox->CollisionMinOffset);
                BattleProjectileQSystem.UpdateVelocity(f, projectileCollisionData->Projectile, shieldCollisionData->PlayerShieldHitbox->Normal, BattleProjectileQSystem.SpeedChange.Increment);
            }
        }

        /// <summary>
        /// Called every frame to update the player.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle for the player.</param>
        /// <param name="playerData">Pointer to player data.</param>
        /// <param name="playerEntity">Entity reference for the player.</param>
        public override unsafe void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            BattlePlayerClassProjectorDataQComponent* data = GetClassData(f, playerEntity);
            if (!data->IsHoldingProjectile)
            {
                playerHandle.AllowCharacterSwapping = true;
                return;
            }

            playerHandle.AllowCharacterSwapping = false;

            BattlePlayerManager.PlayerHandle teammateHandle = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, playerData->Slot);
            if (teammateHandle.PlayState.IsOutOfPlay()) return;

            EntityRef teammateEntity = BattleEntityManager.Get(f, teammateHandle.SelectedCharacterEntityID);

            if (teammateHandle.PlayState.IsInPlay())
            {
                Transform2D* transformPlayer = f.Unsafe.GetPointer<Transform2D>(playerEntity);
                Transform2D* transformTeammate = f.Unsafe.GetPointer<Transform2D>(teammateEntity);
                Transform2D* transformProjectile = f.Unsafe.GetPointer<Transform2D>(data->HeldProjectileEntity);
                BattleProjectileQComponent* projectile = f.Unsafe.GetPointer<BattleProjectileQComponent>(data->HeldProjectileEntity);

                FPVector2 targetPosition = transformTeammate->Position;
                targetPosition.Y += (playerData->TeamNumber == BattleTeamNumber.TeamAlpha ? FP._4 : -FP._4) * BattleGridManager.GridScaleFactor;

                FPVector2 toTeammate = targetPosition - transformPlayer->Position;
                FP angleTeammate = FPVector2.RadiansSigned(FPVector2.Up, toTeammate);

                FP rotationTime = (f.Number - data->HoldStartFrame) / data->RotationDurationFrames;
                FP newAngle = FPMath.Lerp(data->HeldProjectileAngleRadians, angleTeammate, rotationTime);

                FPVector2 newDirection = FPVector2.Rotate(FPVector2.Up, newAngle);

                transformProjectile->Position = transformPlayer->Position + newDirection * data->HeldProjectileDistance;

                if (newAngle == angleTeammate)
                {
                    BattleProjectileQSystem.UpdateVelocity(f, projectile, newDirection, BattleProjectileQSystem.SpeedChange.Increment, passed: true);
                    BattleProjectileQSystem.SetHeld(f, projectile, false);
                    data->IsHoldingProjectile = false;
                }
            }
        }
    }
}

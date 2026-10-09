/// @file BattlePlayerClass400.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClass400} class which handles player character class logic for the 400/Projector class.
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
    /// @bigtext{See [{PlayerClass}](#page-concepts-player-simulation-class-playerclass) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext{See [{Player Character Class 400 - Projector}](#page-concepts-player-class-400) for more info.}
    public class BattlePlayerClass400 : BattlePlayerClassBase<BattlePlayerClass400DataQComponent>
    {
        /// <summary>
        /// Gets the character class associated with this Class.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.Class400">BattlePlayerCharacterClass.Class400</see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Class400;

        /// <summary>
        /// Called when a projectile hits a player shield.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        /// <param name="selected">Is the character selected or not.</param>
        public override unsafe void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, bool selected)
        {
            BattlePlayerHandle                playerHandle = BattlePlayerHandle.Create(f, projectileCollisionData->OtherEntityRef);
            BattlePlayerShieldHandle shieldHandle = BattlePlayerShieldHandle.Create(f, projectileCollisionData->OtherEntityRef);

            if (projectileCollisionData->Projectile.Data->IsHeld) return;

            bool grab = shieldHandle.ShieldData->IsAttached && !projectileCollisionData->Projectile.Data->IsPassed;
            BattlePlayerClass400DataQComponent* data = null;

            if (grab)
            {
                data = GetClassData(f, playerHandle.LoadedCharacterEntityRef);
                BattlePlayerData.Ref teammateData = BattlePlayerManager.GetPlayerData(f, BattlePlayerManager.GetTeammateSlot(playerHandle.PlayerData.Slot));
                if (!teammateData.PlayState.IsInPlay()) grab = false;
            }
            if (grab)
            {

                FPVector2 toProjectile = projectileCollisionData->Projectile.Transform->Position - playerHandle.LoadedCharacterTransform->Position;

                projectileCollisionData->Projectile.SetHeld(true);
                data->IsHoldingProjectile = true;
                data->HeldProjectileEntity = projectileCollisionData->Projectile.ERef;
                data->HeldProjectileAngleRadians = FPVector2.RadiansSigned(FPVector2.Up, toProjectile);
                data->HeldProjectileDistance = shieldHandle.CollidedHitboxComponent->CollisionMinOffset + projectileCollisionData->Projectile.Data->Radius;
                data->HoldStartFrame = f.Number;
            }
            else
            {
                BattleProjectileQSystem.HandleIntersection(f,
                    projectileCollisionData->Projectile,
                    projectileCollisionData->OtherEntityRef,
                    shieldHandle.CollidedHitboxComponent->CalculateNormal(f),
                    shieldHandle.CollidedHitboxComponent->CollisionMinOffset
                );
                BattleProjectileQSystem.UpdateVelocity(f,
                    projectileCollisionData->Projectile.Data,
                    shieldHandle.CollidedHitboxComponent->CalculateNormal(f),
                    BattleProjectileQSystem.SpeedChange.Increment
                );
            }
        }

        /// <summary>
        /// Called every frame to update the player.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle for the player.</param>
        /// <param name="playerCharacterData">Pointer to player data.</param>
        /// <param name="playerEntity">Entity reference for the player.</param>
        /// <param name="specialInput">Pointer to special input (unused)</param>
        public override unsafe void OnUpdate(Frame f, BattlePlayerHandle playerHandle, BattleSpecialInput* specialInput)
        {
            BattlePlayerClass400DataQComponent* data = GetClassData(f, playerHandle.LoadedCharacterEntityRef);
            if (!data->IsHoldingProjectile) return;

            if (!BattleParameters.GetIsTestFlipperGame(f))
            {
                playerHandle.PlayerData.Low_Level.StateAllowCharacterSwapping = false;
            }

            BattlePlayerHandle teammateHandle = BattlePlayerHandle.Create(f, BattlePlayerManager.GetTeammateSlot(playerHandle.PlayerData.Slot));

            if (teammateHandle.PlayerData.PlayState.IsOutOfPlay()) return;

            if (teammateHandle.PlayerData.PlayState.IsInPlay())
            {
                BattleProjectileHandle projectile = BattleProjectileQSystem.GetProjectileHandle(f);
                FPVector2 targetPosition = teammateHandle.LoadedCharacterTransform->Position;
                targetPosition.Y += (playerHandle.PlayerData.Team == BattleTeamNumber.TeamAlpha ? FP._4 : -FP._4) * BattleGridManager.GridScaleFactor;

                FPVector2 toTeammate = targetPosition - playerHandle.LoadedCharacterTransform->Position;
                FP angleTeammate = FPVector2.RadiansSigned(FPVector2.Up, toTeammate);

                FP rotationTime = (f.Number - data->HoldStartFrame) / data->RotationDurationFrames;
                FP newAngle = FPMath.Lerp(data->HeldProjectileAngleRadians, angleTeammate, rotationTime);

                FPVector2 newDirection = FPVector2.Rotate(FPVector2.Up, newAngle);

                BattleEntityManager.MoveCompound(f, data->HeldProjectileEntity, playerHandle.LoadedCharacterTransform->Position + newDirection * data->HeldProjectileDistance, FP._0);

                if (newAngle == angleTeammate)
                {
                    BattleProjectileQSystem.UpdateVelocity(f, projectile.Data, newDirection, BattleProjectileQSystem.SpeedChange.Increment, passed: true);
                    BattleProjectileQSystem.GetProjectileHandle(f).SetHeld(false);
                    data->IsHoldingProjectile = false;

                    if (!BattleParameters.GetIsTestFlipperGame(f))
                    {
                        playerHandle.PlayerData.Low_Level.StateAllowCharacterSwapping = true;
                    }
                }
            }
        }
    }
}

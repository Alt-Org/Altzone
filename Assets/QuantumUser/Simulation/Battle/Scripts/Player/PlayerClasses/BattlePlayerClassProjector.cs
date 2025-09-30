/// @file BattlePlayerClassProjector.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClassProjector} class,
/// which handles player character class logic for the Projector class.
/// </summary>

using Battle.QSimulation.Projectile;
using Photon.Deterministic;
using Quantum;
using static Battle.QSimulation.Player.BattlePlayerManager;
using static Battle.QSimulation.Projectile.BattleProjectileQSystem;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Player character class logic for the Projector class.
    /// </summary>
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
        /// <param name="projectile">Poiinter to the projectiel.</param>
        /// <param name="projectileEntity">Entity reference for the projectile.</param>
        /// <param name="playerHitbox">Pointer to the player hitbox.</param>
        /// <param name="playerHitboxEntity">Entity reference to the player hitbox.</param>
        public override unsafe void OnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            BattlePlayerClassProjectorDataQComponent* data = GetClassData(f, playerHitbox->PlayerEntity);
            bool grab = !projectile->IsPassed;

            if (grab)
            {
                BattlePlayerManager.PlayerHandle teammateHandle = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity)->Slot);
                if (!teammateHandle.PlayState.IsInPlay()) grab = false;
            }

            if (grab)
            {
                Transform2D* transformPlayer = f.Unsafe.GetPointer<Transform2D>(playerHitbox->PlayerEntity);
                Transform2D* transformProjectile = f.Unsafe.GetPointer<Transform2D>(projectileEntity);

                FPVector2 toProjectile = transformProjectile->Position - transformPlayer->Position;

                BattleProjectileQSystem.SetHeld(f, projectile, true);
                data->IsHoldingProjectile = true;
                data->HeldProjectileEntity = projectileEntity;
                data->HeldProjectileAngleRadians = FPVector2.RadiansSigned(FPVector2.Up, toProjectile);
                data->HeldProjectileDistance = toProjectile.Magnitude;
                data->HoldStartFrame = f.Number;
            }
            else
            {
                BattleProjectileQSystem.HandleIntersection(f, projectile, projectileEntity, playerHitboxEntity, playerHitbox->Normal, playerHitbox->CollisionMinOffset);
                BattleProjectileQSystem.UpdateVelocity(f, projectile, playerHitbox->Normal, SpeedChange.Increment);
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
        public override unsafe void OnUpdate(Frame f, PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
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

            EntityRef teammateEntity = teammateHandle.SelectedCharacterEntity;

            if (teammateHandle.PlayState.IsInPlay())
            {
                Transform2D* transformPlayer = f.Unsafe.GetPointer<Transform2D>(playerEntity);
                Transform2D* transformTeammate = f.Unsafe.GetPointer<Transform2D>(teammateEntity);
                Transform2D* transformProjectile = f.Unsafe.GetPointer<Transform2D>(data->HeldProjectileEntity);
                BattleProjectileQComponent* projectile = f.Unsafe.GetPointer<BattleProjectileQComponent>(data->HeldProjectileEntity);

                FPVector2 toTeammate = transformTeammate->Position - transformPlayer->Position;
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

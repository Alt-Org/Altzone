/// @file BattlePlayerClass600Test.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClass600Test} class which handles player character class logic for the 600/Confluent class.
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
    public class BattlePlayerClass600Test : BattlePlayerClassBase<BattlePlayerClass600DataQComponent>
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
        /// <param name="selected">Is the character selected or not.</param>
        public override unsafe void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, bool selected)
        {
            BattlePlayerCharacterShieldHandle shieldHandle = BattlePlayerCharacterShieldHandle.Create(f, projectileCollisionData->OtherEntityRef);

            if (projectileCollisionData->Projectile.Data->IsHeld) return;
            if (projectileCollisionData->Projectile.Data->EmotionCurrent == BattleEmotionState.Love) return;
            if (projectileCollisionData->IsLoveProjectileCollision) return;

            FPVector2 normal = (projectileCollisionData->Projectile.Transform->Position - shieldHandle.ShieldTransform->Position).Normalized;

            BattleProjectileQSystem.HandleIntersection(f,
                projectileCollisionData->Projectile,
                projectileCollisionData->OtherEntityRef,
                normal,
                shieldHandle.CollidedHitboxComponent->CollisionMinOffset * FP._1_10
            );

            if (!selected || !shieldHandle.ShieldData->IsAttached)
            {
                FPVector2 direction = FPVector2.Reflect(projectileCollisionData->Projectile.Data->Direction, normal).Normalized;

                BattleProjectileQSystem.UpdateVelocity(f,
                    projectileCollisionData->Projectile.Data,
                    direction,
                    BattleProjectileQSystem.SpeedChange.Increment
                );
                return;
            }

            BattlePlayerClass600QSpec spec = BattleQConfig.GetBattlePlayerClass600Spec(f);

            BattlePlayerClass600DataQComponent* classData = GetClassData(f, shieldHandle.ShieldData->PlayerEntityRef);

            classData->IsHoldingProjectile = true;

            projectileCollisionData->Projectile.SetHeld(true);

            classData->HeldProjectileEntity = projectileCollisionData->Projectile.ERef;
            classData->HeldProjectileOffset = projectileCollisionData->Projectile.Transform->Position - shieldHandle.ShieldTransform->Position;

            classData->HoldMinTimer = FrameTimer.FromSeconds(f, spec.HoldMinDurationSec);
            classData->HoldMaxTimer = FrameTimer.FromSeconds(f, spec.HoldMaxDurationSec);
        }

        /// <summary>
        /// Called every frame to update the player. Moves the projectile when it is held, and releases it based on timers.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle for the player.</param>
        /// <param name="playerCharacterData">Pointer to player data.</param>
        /// <param name="playerEntity">Entity reference for the player.</param>
        /// <param name="specialInput">Pointer to special input (unused)</param>
        public override unsafe void OnUpdate(Frame f, BattlePlayerHandle playerHandle, BattleSpecialInput* specialInput)
        {
            BattlePlayerClass600QSpec spec = BattleQConfig.GetBattlePlayerClass600Spec(f);

            BattlePlayerClass600DataQComponent* classData = GetClassData(f, playerHandle.LoadedCharacterEntityRef);

            if (!classData->IsHoldingProjectile) return;

            BattleProjectileHandle projectile = BattleProjectileHandle.Create(f, classData->HeldProjectileEntity);

            if (!BattleParameters.GetIsTestFlipperGame(f))
            {
                playerHandle.PlayerData.Low_Level.StateAllowCharacterSwapping = false;
            }

            Transform2D* transformShield = f.Unsafe.GetPointer<Transform2D>(playerHandle.LoadedCharacterData->AttachedShieldEntityRef);

            FPVector2 projectilePositionNext = transformShield->Position + classData->HeldProjectileOffset;

            BattleEntityManager.MoveCompound(f, projectile.ERef, projectilePositionNext, FP._0);

            if (transformShield->Position != classData->PreviousPosition)
            {
                classData->ReleaseBufferTimer = FrameTimer.FromSeconds(f, spec.ReleaseBufferSec);
            }

            classData->PreviousPosition = transformShield->Position;

            if (classData->HoldMinTimer.IsRunning(f)) return;

            if (classData->HoldMaxTimer.IsRunning(f) && classData->ReleaseBufferTimer.IsRunning(f)) return;

            BattleTeamNumber teamNumber = playerHandle.PlayerData.Team;

            BattlePlayerShieldDataQComponent* shieldData = f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(playerHandle.LoadedCharacterData->AttachedShieldEntityRef);

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

            BattleProjectileQSystem.UpdateVelocity(f, projectile.Data, direction, BattleProjectileQSystem.SpeedChange.Increment);

            projectile.SetHeld(false);

            classData->IsHoldingProjectile = false;

            if (!BattleParameters.GetIsTestFlipperGame(f))
            {
                playerHandle.PlayerData.Low_Level.StateAllowCharacterSwapping = true;
            }
        }
    }
}

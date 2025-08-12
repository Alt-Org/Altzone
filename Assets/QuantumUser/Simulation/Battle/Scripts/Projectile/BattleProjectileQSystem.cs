/// @file BattleProjectileQSystem.cs
/// <summary>
/// Controls projectile's movements and reactions to collisions.
/// </summary>
///
/// This system:<br/>
/// Launches projectile when battle starts and updates its movements.<br/>
/// Handles projectile's collisionflags to ensure projectile doesn't hit more than one SoulWall segment at a time.<br/>
/// Reacts to signals of the projectile colliding with different entities.

using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;
using Battle.QSimulation.Game;
using Battle.QSimulation.Player;

namespace Battle.QSimulation.Projectile
{
    /// <summary>
    /// <span class="brief-h">%Projectile <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
    /// Handles projectile logic, including projectile's movements and reactions to collisionsignals.
    /// </summary>
    [Preserve]
    public unsafe class BattleProjectileQSystem : SystemMainThreadFilter<BattleProjectileQSystem.Filter>, ISignalBattleOnProjectileHitSoulWall, ISignalBattleOnProjectileHitArenaBorder, ISignalBattleOnProjectileHitPlayerShield, ISignalBattleOnGameOver
    {
        /// <summary>
        /// Filter for filtering projectile entities
        /// </summary>
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public BattleProjectileQComponent* Projectile;
        }

        /// <summary>
        /// Checks if a specific collision flag is currently set for the projectile.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="flag">Collision flag to check.</param>
        /// <returns>True if the flag is set; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollisionFlagSet(Frame f, BattleProjectileQComponent* projectile, BattleProjectileCollisionFlags flag) => projectile->CollisionFlags[f.Number % 2].IsFlagSet(flag);

        /// <summary>
        /// Sets a specific collision flag for the current frame.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="flag">Collision flag to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCollisionFlag(Frame f, BattleProjectileQComponent* projectile, BattleProjectileCollisionFlags flag)
        {
            BattleProjectileCollisionFlags flags = projectile->CollisionFlags[f.Number % 2];
            projectile->CollisionFlags[f.Number % 2] = flags.SetFlag(flag);
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
        /// Launches the projectile and sets properties based on the <see cref="Battle.QSimulation.Projectile.BattleProjectileQSpec">Projectile spec.</see>
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter@u-exlink</a>.</param>
        public override void Update(Frame f, ref Filter filter)
        {
            // unpack filter
            BattleProjectileQComponent* projectile = filter.Projectile;
            Transform2D* transform = filter.Transform;

            if (!projectile->IsLaunched)
            {
                // retrieve the projectiles spec
                BattleProjectileQSpec spec = BattleQConfig.GetProjectileSpec(f);

                // copy data from the spec
                projectile->Speed = spec.ProjectileInitialSpeed;
                projectile->SpeedPotential = projectile->Speed;
                projectile->SpeedIncrement = spec.SpeedIncrement;
                projectile->Direction = FPVector2.Rotate(FPVector2.Up, -(FP.Rad_90 + FP.Rad_45));
                projectile->AccelerationTimerDuration = spec.AccelerationTimerDuration;
                projectile->AccelerationTimer = projectile->AccelerationTimerDuration;
                projectile->AttackMax = spec.AttackMax;
                for (int i = 0; i < spec.SpeedMultiplierArray.Length; i++)
                {
                    projectile->SpeedMultiplierArray[i] = spec.SpeedMultiplierArray[i];
                }

                // set emotion and attack
                SetEmotion(f, projectile, BattleParameters.GetProjectileInitialEmotion(f));
                SetAttack(f, projectile, 0);

                // reset CollisionFlags for this frame
                projectile->CollisionFlags[(f.Number) % 2] = 0;

                // set the IsLaunched field to true to ensure it's launched only once
                projectile->IsLaunched = true;

                projectile->IsMoving = true;

                Debug.Log("Projectile Launched");
            }

            if (!projectile->IsMoving) return;

            FP gameTimeSec = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>()->GameTimeSec;

            // every 10 seconds increase the speed potential by a set amount
            if (gameTimeSec >= projectile->AccelerationTimer)
            {
                projectile->SpeedPotential += projectile->SpeedIncrement;
                projectile->AccelerationTimer += projectile->AccelerationTimerDuration;
            }

            // move the projectile
            transform->Position += projectile->Direction * (projectile->Speed * f.DeltaTime);

            // reset CollisionFlags for next frame
            projectile->CollisionFlags[(f.Number + 1) % 2 ] = 0;
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnProjectileHitSoulWall">ISignalBattleOnProjectileHitSoulWall</see> is sent.</span><br/>
        /// Handles behavior when the projectile hits a SoulWall.
        /// Updates projectile's emotion and applies bounce logic.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="soulWall">Pointer to the SoulWall component.</param>
        /// <param name="soulWallEntity">EntityRef of the SoulWall.</param>
        public void BattleOnProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall, EntityRef soulWallEntity)
        {
            // change projectile's emotion to soulwall's emotion
            SetEmotion(f, projectile, soulWall->Emotion);

            ProjectileVelocityUpdate(f, projectile, projectileEntity, soulWallEntity, soulWall->Normal, soulWall->CollisionMinOffset);
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnProjectileHitArenaBorder">ISignalBattleOnProjectileHitArenaBorder</see> is sent.</span><br/>
        /// Handles behavior when the projectile hits the arena border.
        /// Applies bounce logic based on border surface normal.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="arenaBorder">Pointer to the ArenaBorder component.</param>
        /// <param name="arenaBorderEntity">EntityRef of the ArenaBorder.</param>
        public void BattleOnProjectileHitArenaBorder(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleArenaBorderQComponent* arenaBorder, EntityRef arenaBorderEntity)
        {
            ProjectileVelocityUpdate(f, projectile,  projectileEntity, arenaBorderEntity, arenaBorder->Normal, arenaBorder->CollisionMinOffset);
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnProjectileHitPlayerShield">ISignalBattleOnProjectileHitPlayerShield</see> is sent.</span><br/>
        /// Handles behavior when the projectile hits a player shield.
        /// Applies bounce logic based on surface normal of the shield hitbox.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="playerHitbox">Pointer to the PlayerHitbox component.</param>
        /// <param name="playerHitboxEntity">EntityRef of the player hitbox.</param>
        public void BattleOnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            if (projectile->Emotion == BattleEmotionState.Love) return;
            if (playerHitbox->CollisionType == BattlePlayerCollisionType.None) return;

            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);

            // get attack damage from player stats
            SetAttack(f, projectile, playerData->Stats.Attack);
            bool isOnTopOfTeammate = false;

            BattlePlayerManager.PlayerHandle teammateHandle = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, playerData->Slot);

            if (teammateHandle.PlayState == BattlePlayerPlayState.InPlay)
            {
                EntityRef teammateEntity = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, playerData->Slot).SelectedCharacter;

                Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(playerHitbox->PlayerEntity);
                Transform2D* teammateTransform = f.Unsafe.GetPointer<Transform2D>(teammateEntity);

                BattleGridPosition playerGridPosition = BattleGridManager.WorldPositionToGridPosition(playerTransform->Position);
                BattleGridPosition teammateGridPosition = BattleGridManager.WorldPositionToGridPosition(teammateTransform->Position);

                isOnTopOfTeammate = playerGridPosition.Row == teammateGridPosition.Row && playerGridPosition.Col == teammateGridPosition.Col;

            }

            // if player is in the same grid cell as teammate, change the projectile to love emotion
            if (isOnTopOfTeammate)
            {
                Debug.Log("[ProjectileSystem] changing projectile emotion to Love");
                SetEmotion(f, projectile, BattleEmotionState.Love);

                // send a projectileVelocityUpdate with the direction being straight up or down depending on the team
                ProjectileVelocityUpdate(f, projectile, projectileEntity, playerHitboxEntity, playerData->TeamNumber == BattleTeamNumber.TeamAlpha ? FPVector2.Up : FPVector2.Down, playerHitbox->CollisionMinOffset, BattlePlayerCollisionType.Override);
                return;
            }
            ProjectileVelocityUpdate(f, projectile, projectileEntity, playerHitboxEntity, playerHitbox->Normal, playerHitbox->CollisionMinOffset, playerHitbox->CollisionType);
        }

        public unsafe void BattleOnGameOver(Frame f, BattleTeamNumber winningTeam, BattleProjectileQComponent* projectile, EntityRef projectileEntity)
        {
            // stop the projectile
            projectile->IsMoving = false;

            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);

            // move the projectile out of bounds after a goal is scored
            switch (winningTeam)
            {
                case BattleTeamNumber.TeamAlpha:
                    projectileTransform->Position += new FPVector2(0, 10);
                    break;
                case BattleTeamNumber.TeamBeta:
                    projectileTransform->Position += new FPVector2(0, -10);
                    break;
            }
        }

        /// <summary>
        /// Updates projectile's direction depending on other colliding entity's collisionType.<br/>
        /// Reflect: Projectile gets reflected following laws of physics.<br/>
        /// Override: Projectile gets directed to a specific direction set in other entity's component.<br/>
        /// Updates projectile's Speed to SpeedPotential.
        /// @warning
        /// None is not a valid collisionType for this method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="otherEntity">EntityRef of the other colliding entity.</param>
        /// <param name="normal">Normal of the other colliding entity.</param>
        /// <param name="collisionMinOffset">CollisionMinOffset(how far projectile is allowed to penetrate) of the other colliding entity.</param>
        /// <param name="collisionType">CollisionType of the other colliding entity.</param>
        private void ProjectileVelocityUpdate(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, EntityRef otherEntity, FPVector2 normal, FP collisionMinOffset, BattlePlayerCollisionType collisionType = BattlePlayerCollisionType.Reflect)
        {
            if (IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) return;

            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);
            Transform2D* otherTransform = f.Unsafe.GetPointer<Transform2D>(otherEntity);

            // calculate how far off from other entity's position projectile is supposed to hit its surface
            FPVector2 offsetVector = projectileTransform->Position - otherTransform->Position;
            FP collisionOffset = FPVector2.Rotate(offsetVector, -FPVector2.RadiansSigned(FPVector2.Up, normal)).Y;

            // set new projectile direction
            if (collisionType == BattlePlayerCollisionType.Reflect) projectile->Direction = FPVector2.Reflect(projectile->Direction, normal);
            else if (collisionType == BattlePlayerCollisionType.Override) projectile->Direction = normal;

            // update the projectile's speed based on speed potential and multiply by emotion
            projectile->Speed = projectile->SpeedPotential * projectile->SpeedMultiplierArray[(int)projectile->Emotion];

            // if projectile accidentally went inside another entity, lift it out
            if (collisionOffset - projectile->Radius < collisionMinOffset)
            {
                projectileTransform->Position += normal * (collisionMinOffset - collisionOffset + projectile->Radius);
            }

            SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Projectile);
        }

        /// <summary>
        /// Private helper method for setting emotion for the projectile and calling BattleChangeEmotionState event.
        /// </summary>
        ///
        /// <param name="f"></param>
        /// <param name="projectile"></param>
        /// <param name="emotion"></param>
        private void SetEmotion(Frame f, BattleProjectileQComponent* projectile, BattleEmotionState emotion)
        {
            projectile->Emotion = emotion;
            f.Events.BattleChangeEmotionState(projectile->Emotion);
        }

        /// <summary>
        /// Private helper method for setting attack for the projectile and calling BattleProjectileChangeGlowStrength event.
        /// </summary>
        ///
        /// <param name="f"></param>
        /// <param name="projectile"></param>
        /// <param name="attack"></param>
        private void SetAttack(Frame f, BattleProjectileQComponent* projectile, FP attack)
        {
            projectile->Attack = attack;
            f.Events.BattleProjectileChangeGlowStrength(projectile->Attack / projectile->AttackMax);
        }
    }
}

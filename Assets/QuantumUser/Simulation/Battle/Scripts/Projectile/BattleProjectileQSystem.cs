/// @file BattleProjectileQSystem.cs
/// <summary>
/// Controls projectile's movements and reactions to collisions.
/// </summary>
///
/// This system:<br/>
/// Launches projectile when battle starts and updates its movements.<br/>
/// Handles projectile's collisionflags to ensure projectile doesn't hit more than one SoulWall segment at a time.<br/>
/// Contains logic for handling the projectile colliding with different entities.

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
    public unsafe class BattleProjectileQSystem : SystemMainThreadFilter<BattleProjectileQSystem.Filter>, ISignalBattleOnGameOver
    {
        #region Public

        /// <summary>
        /// Filter for filtering projectile entities
        /// </summary>
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public BattleProjectileQComponent* Projectile;
        }

        #region Public - Helper Methods

        /// <summary>
        /// Checks if a specific collision flag is currently set for the projectile.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="flag">Collision flag to check.</param>
        /// <returns>True if the flag is set; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollisionFlagSet(Frame f, BattleProjectileQComponent* projectile, BattleProjectileCollisionFlags flag) => projectile->CollisionFlags[f.Number % 2].IsFlagSet(flag);

        /// <summary>
        /// Sets a specific collision flag for the current frame.
        /// </summary>
        ///
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
        /// Sets whether the projectile is currently held.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="isHeld">True/False : held / not held.</param>
        public static void SetHeld(Frame f, BattleProjectileQComponent* projectile, bool isHeld)
        {
            projectile->IsHeld = isHeld;
        }

        /// <summary>
        /// Sets the emotion state of the projectile and triggers the corresponding event.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="emotion">The new emotion state to assign to the projectile.</param>
        public static void SetEmotion(Frame f, BattleProjectileQComponent* projectile, BattleEmotionState emotion)
        {
            if (emotion != BattleEmotionState.Love)
            {
                projectile->EmotionBase = emotion;
            }
            projectile->EmotionCurrent = emotion;
            f.Events.BattleChangeEmotionState(projectile->EmotionCurrent);
        }

        /// <summary>
        /// Sets the attack value of the projectile and updates its glow strength.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="attack">The new attack value to assign to the projectile.</param>
        public static void SetAttack(Frame f, BattleProjectileQComponent* projectile, FP attack)
        {
            projectile->Attack = attack;
            f.Events.BattleProjectileChangeGlowStrength(projectile->Attack / projectile->AttackMax);
        }

        /// <summary>
        /// Sets the direction of the projectile and recalculates its speed based on emotion.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="direction">The new direction for the projectile.</param>
        /// <param name="speedIncreaseAmount">The amount the projectile's speed should increase.</param>
        /// <param name="resetSpeed">True if the projectile's speed should be reset to base, false if it should remain as is.</param>
        public static void UpdateVelocity(Frame f, BattleProjectileQComponent* projectile, FPVector2 direction, FP speedIncreaseAmount, bool resetSpeed = false)
        {
            // set new projectile direction
            projectile->Direction = direction;

            // update the projectile's speed based on speed potential and multiply by emotion (disabled)
            //projectile->Speed = projectile->SpeedPotential * projectile->SpeedMultiplierArray[(int)projectile->Emotion];

            // increment or reset the speed of the projectile
            if (!resetSpeed)
            {
                projectile->Speed = FPMath.Min(projectile->Speed + speedIncreaseAmount, projectile->SpeedMax);
            }
            else
            {
                projectile->Speed = projectile->SpeedBase;
            }
        }

        /// <summary>
        /// Handles the intersection of a projectile with another entity and corrects its position if necessary.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">The entity representing the projectile.</param>
        /// <param name="otherEntity">The entity the projectile is intersecting with.</param>
        /// <param name="normal">The collision normal of the intersection.</param>
        /// <param name="collisionMinOffset">Minimum allowed offset to prevent the projectile from going inside the other entity.</param>
        public static void HandleIntersection(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, EntityRef otherEntity, FPVector2 normal, FP collisionMinOffset)
        {
            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);
            Transform2D* otherTransform = f.Unsafe.GetPointer<Transform2D>(otherEntity);

            // calculate how far off from other entity's position is the projectile supposed to hit it's surface
            FPVector2 offsetVector = projectileTransform->Position - otherTransform->Position;
            FP collisionOffset = FPVector2.Rotate(offsetVector, -FPVector2.RadiansSigned(FPVector2.Up, normal)).Y;

            // if projectile accidentally went inside another entity, lift it out
            if (collisionOffset - projectile->Radius < collisionMinOffset)
            {
                projectileTransform->Position += normal * (collisionMinOffset - collisionOffset + projectile->Radius);
            }
        }

        #endregion Public - Helper Methods

        #region Public - Gameflow Methods


        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
        /// Launches the projectile if it hasn't been launched yet, moves it if not held,
        /// updates its speed potential over time, and resets collision flags for the next frame. <br/>
        /// Sets properties based on the <see cref="Battle.QSimulation.Projectile.BattleProjectileQSpec">Projectile spec.</see>
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter@u-exlink</a>.</param>
        public override void Update(Frame f, ref Filter filter)
        {
            // unpack filter
            BattleProjectileQComponent* projectile = filter.Projectile;
            Transform2D* transform = filter.Transform;

            if (!projectile->IsLaunched)
            {
                Launch(f, projectile);
            }

            FP gameTimeSec = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>()->GameTimeSec;

            // every 10 seconds increase the speed potential by a set amount (disabled)
            //if (gameTimeSec >= projectile->AccelerationTimer)
            //{
            //    projectile->SpeedPotential += projectile->SpeedIncrement;
            //    projectile->AccelerationTimer += projectile->AccelerationTimerDuration;
            //}

            if (!projectile->IsHeld)
            {
                // move the projectile
                transform->Position += projectile->Direction * (projectile->Speed * f.DeltaTime);
            }

            // reset CollisionFlags for next frame
            projectile->CollisionFlags[(f.Number + 1) % 2 ] = 0;
        }

        /// <summary>
        /// Called by BattleCollisionQSystem. Handles the collision based on the specified collision trigger type, handling the collision differently based on what the projectile has hit.<br/>
        /// The projectile will either reflect off of the arena border, set its emotion and bounce off a soul wall, or run checks on what it should do if it hits a player shield.<br/>
        /// Ultimately sends the projectile in a new direction if it is appropriate to do so.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="otherEntity">The entity the projectile collided with.</param>
        /// <param name="otherComponentPtr">A pointer reference to the other entity.</param>
        /// <param name="collisionTriggerType">The collision type of the collision, informing what the projectile has hit.</param>
        public static void OnProjectileCollision(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, EntityRef otherEntity, void* otherComponentPtr, BattleCollisionTriggerType collisionTriggerType)
        {
            if (projectile->IsHeld) return;
            if (IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) return;

            BattlePlayerCollisionType collisionType = BattlePlayerCollisionType.Reflect;
            FPVector2 normal = FPVector2.Zero;
            FP collisionMinOffset = FP._0;
            bool handleCollision = false;

            FP speedIncrementAmount = 0;
            bool resetSpeed = false;

            switch (collisionTriggerType)
            {
                case BattleCollisionTriggerType.ArenaBorder:
                    BattleArenaBorderQComponent* arenaBorder = (BattleArenaBorderQComponent*)otherComponentPtr;

                    normal = arenaBorder->Normal;
                    collisionMinOffset = arenaBorder->CollisionMinOffset;
                    handleCollision = true;
                    break;

                case BattleCollisionTriggerType.SoulWall:

                    BattleSoulWallQComponent* soulWall = (BattleSoulWallQComponent*)otherComponentPtr;

                    if (projectile->EmotionCurrent == BattleEmotionState.Love)
                    {
                        SetEmotion(f, projectile, projectile->EmotionBase);
                    }

                    normal = soulWall->Normal;
                    collisionMinOffset = soulWall->CollisionMinOffset;
                    resetSpeed = true;
                    handleCollision = true;
                    break;

                case BattleCollisionTriggerType.Shield:
                    BattlePlayerHitboxQComponent* playerHitbox = (BattlePlayerHitboxQComponent*)otherComponentPtr;

                    if (ProjectileHitPlayerShield(f, projectile, projectileEntity, playerHitbox, otherEntity, out normal))
                    {
                        collisionType = playerHitbox->CollisionType;
                        collisionMinOffset = playerHitbox->CollisionMinOffset;
                        speedIncrementAmount = projectile->SpeedIncrement;
                        handleCollision = true;
                    }
                    break;

                default:
                    break;
            }

            if (handleCollision)
            {
                FPVector2 direction = FPVector2.Zero;
                if      (collisionType == BattlePlayerCollisionType.Reflect)  direction = FPVector2.Reflect(projectile->Direction, normal);
                else if (collisionType == BattlePlayerCollisionType.Override) direction = normal;

                HandleIntersection(f, projectile, projectileEntity, otherEntity, normal, collisionMinOffset);
                UpdateVelocity(f, projectile, direction, speedIncrementAmount, resetSpeed);
            }

            SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Projectile);
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnGameOver">ISignalBattleOnGameOver</see> is sent.</span><br/>
        /// Sets the projectile to held state and teleports it out of the arena.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="winningTeam">The BattleTeamNumber of the team that won.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        public unsafe void BattleOnGameOver(Frame f, BattleTeamNumber winningTeam, BattleProjectileQComponent* projectile, EntityRef projectileEntity)
        {
            SetHeld(f, projectile, true);

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

        #endregion Public - Gameflow Methods

        #endregion Public

        #region Private Static Methods

        /// <summary>
        /// Launches the projectile from an unlaunched state, setting its initial values.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        private static void Launch(Frame f, BattleProjectileQComponent* projectile)
        {
            // retrieve the projectiles spec
            BattleProjectileQSpec spec = BattleQConfig.GetProjectileSpec(f);

            // copy data from the spec
            projectile->Speed = spec.ProjectileInitialSpeed;
            projectile->SpeedBase = projectile->Speed;
            projectile->SpeedMax = spec.SpeedMax;
            //projectile->SpeedPotential = projectile->Speed;
            projectile->SpeedIncrement = spec.SpeedIncrement;
            projectile->Direction = FPVector2.Rotate(FPVector2.Up, -(FP.Rad_90 + FP.Rad_45));
            //projectile->AccelerationTimerDuration = spec.AccelerationTimerDuration;
            //projectile->AccelerationTimer = projectile->AccelerationTimerDuration;
            projectile->AttackMax = spec.AttackMax;
            //for (int i = 0; i < spec.SpeedMultiplierArray.Length; i++)
            //{
            //    projectile->SpeedMultiplierArray[i] = spec.SpeedMultiplierArray[i];
            //}

            // set emotion and attack
            SetEmotion(f, projectile, BattleParameters.GetProjectileInitialEmotion(f));
            SetAttack(f, projectile, 0);

            // reset CollisionFlags for this frame
            projectile->CollisionFlags[(f.Number) % 2] = 0;

            // set the IsLaunched field to true to ensure it's launched only once
            projectile->IsLaunched = true;

            SetHeld(f, projectile, false);

            Debug.Log("Projectile Launched");
        }

        /// <summary>
        /// This method checks if the shield hitbox and projectile are in states where they should collide.<br/>
        /// Also sets the projectile to the love emotion state if the condition for that is met.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="playerHitbox">Pointer to the PlayerHitbox component.</param>
        /// <param name="playerHitboxEntity">EntityRef of the player hitbox.</param>
        /// <param name="normal">The direction in which the projectile should be sent.</param>
        private static bool ProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity, out FPVector2 normal)
        {
            normal = FPVector2.Zero;

            if (!playerHitbox->IsActive) return false;
            if (projectile->EmotionCurrent == BattleEmotionState.Love) return false;
            if (playerHitbox->CollisionType == BattlePlayerCollisionType.None) return false;

            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);

            bool isOnTopOfTeammate = false;

            BattlePlayerManager.PlayerHandle teammateHandle = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, playerData->Slot);

            if (teammateHandle.PlayState.IsInPlay())
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

                normal = playerData->TeamNumber == BattleTeamNumber.TeamAlpha ? FPVector2.Up : FPVector2.Down;
                return true;
            }
            normal = playerHitbox->Normal;
            return true;
        }

        #endregion Private Static Methods
    }
}

/// @file BattleProjectileQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Projectile,BattleProjectileQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which controls projectile's movements and reactions to collisions.
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Unity usings
using UnityEngine.Scripting;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;
using Battle.QSimulation.Player;

namespace Battle.QSimulation.Projectile
{
    /// <summary>
    /// <span class="brief-h">%Projectile <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
    /// Handles projectile logic, including projectile's movements and reactions to collisionsignals.
    /// </summary>
    ///
    /// This system:<br/>
    /// Launches projectile when battle starts and updates its movements.<br/>
    /// Handles projectile's collisionflags to ensure projectile doesn't hit more than one SoulWall segment at a time.<br/>
    /// Contains logic for handling the projectile colliding with different entities.
    [Preserve]
    public unsafe class BattleProjectileQSystem : SystemMainThreadFilter<BattleProjectileQSystem.Filter>, ISignalBattleOnGameOver
    {
        #region Public

        /// <summary>
        /// Defines how the projectile's speed should be updated
        /// </summary>
        public enum SpeedChange
        {
            None,
            Increment,
            Reset
        }

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
        /// Sets the direction of the projectile and adjusting its speed depending on the specified <see cref="SpeedChange"/> behavior.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="direction">The new direction for the projectile.</param>
        /// <param name="speedChange">Determines how the projectile's speed should be updated.</param>
        /// <param name="passed">Checking if player has passed the projectile.</param>
        public static void UpdateVelocity(Frame f, BattleProjectileQComponent* projectile, FPVector2 direction, SpeedChange speedChange, bool passed = false)
        {
            // set new projectile direction
            projectile->Direction = direction;

            projectile->IsPassed = passed;

            // update the projectile's speed based on speed potential and multiply by emotion (disabled)
            //projectile->Speed = projectile->SpeedPotential * projectile->SpeedMultiplierArray[(int)projectile->Emotion];

            // if not none; increment or reset the speed of the projectile
            switch (speedChange)
            {
                case SpeedChange.None:
                    break;

                case SpeedChange.Increment:
                    projectile->Speed = FPMath.Min(projectile->Speed + projectile->SpeedIncrement, projectile->SpeedMax);
                    f.Events.BattleProjectileChangeSpeed(projectile->Speed);
                    break;

                case SpeedChange.Reset:
                    projectile->Speed = projectile->SpeedBase;
                    f.Events.BattleProjectileChangeSpeed(projectile->Speed);
                    break;
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
        /// Initializes this classes BattleDebugLogger instance.<br/>
        /// This method is exclusively for debug logging purposes.
        /// </summary>
        public static void Init()
        {
            s_debugLogger = BattleDebugLogger.Create<BattleProjectileQSystem>();
        }

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
            projectile->Position = transform->Position;
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
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="data">Collision data related to the other entity.</param>
        /// <param name="collisionTriggerType">The collision type of the collision, informing what the projectile has hit.</param>
        public static void OnProjectileCollision(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, void* data, BattleCollisionTriggerType collisionTriggerType)
        {
            if (projectileCollisionData->Projectile->IsHeld) return;

            // unpack projectileCollisionData
            BattleProjectileQComponent* projectile       = projectileCollisionData->Projectile;
            EntityRef                   projectileEntity = projectileCollisionData->ProjectileEntity;
            EntityRef                   otherEntity      = projectileCollisionData->OtherEntity;

            // set default values
            BattlePlayerCollisionType collisionType      = BattlePlayerCollisionType.Reflect;
            bool                      handleCollision    = false;
            FPVector2                 normal             = FPVector2.Zero;
            FP                        collisionMinOffset = FP._0;
            SpeedChange               speedChange        = SpeedChange.None;

            // handle the specific collision type
            switch (collisionTriggerType)
            {
                case BattleCollisionTriggerType.ArenaBorder:
                    BattleArenaBorderQComponent* arenaBorder = ((BattleCollisionQSystem.ArenaBorderCollisionData*)data)->ArenaBorder;

                    normal             = arenaBorder->Normal;
                    collisionMinOffset = arenaBorder->CollisionMinOffset;
                    handleCollision    = true;
                    break;

                case BattleCollisionTriggerType.SoulWall:
                    BattleSoulWallQComponent* soulWall = ((BattleCollisionQSystem.SoulWallCollisionData*)data)->SoulWall;

                    if (projectile->EmotionCurrent == BattleEmotionState.Love)
                    {
                        SetEmotion(f, projectile, projectile->EmotionBase);
                    }

                    normal             = soulWall->Normal;
                    collisionMinOffset = soulWall->CollisionMinOffset;
                    speedChange        = SpeedChange.Reset;
                    handleCollision    = true;
                    break;

                case BattleCollisionTriggerType.Shield:
                    BattleCollisionQSystem.PlayerShieldCollisionData* dataPtr = (BattleCollisionQSystem.PlayerShieldCollisionData*)data;
                    BattlePlayerHitboxQComponent* playerShieldHitbox = dataPtr->PlayerShieldHitbox;

                    if (!ProjectileHitPlayerShield(f, projectile, dataPtr, out normal)) break;
                    
                    collisionType      = playerShieldHitbox->CollisionType;
                    collisionMinOffset = playerShieldHitbox->CollisionMinOffset;
                    speedChange        = SpeedChange.Increment;
                    handleCollision    = true;
                    
                    break;

                case BattleCollisionTriggerType.Player:
                    BattlePlayerHitboxQComponent* playerCharacterHitbox = ((BattleCollisionQSystem.PlayerCharacterCollisionData*)data)->PlayerCharacterHitbox;

                    if (projectile->EmotionCurrent == BattleEmotionState.Love) break;

                    if (FPVector2.Dot(playerCharacterHitbox->Normal, projectile->Direction.Normalized) >= 0) break;

                    normal             = playerCharacterHitbox->Normal;
                    collisionType      = playerCharacterHitbox->CollisionType;
                    collisionMinOffset = playerCharacterHitbox->CollisionMinOffset;
                    speedChange        = SpeedChange.Increment;
                    handleCollision    = true;
                    
                    break;

                default:
                    break;
            }

            if (handleCollision)
            {
                FPVector2 direction;
                if (collisionType == BattlePlayerCollisionType.Reflect) direction = FPVector2.Reflect(projectile->Direction, normal).Normalized;
                else                                                    direction = normal;

                HandleIntersection(f, projectile, projectileEntity, otherEntity, normal, collisionMinOffset);
                UpdateVelocity(f, projectile, direction, speedChange);
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
        public unsafe void BattleOnGameOver(Frame f, BattleTeamNumber winningTeam)
        {
            EntityRef projectileEntity = GetProjectileEntity(f);

            BattleProjectileQComponent* projectile          = f.Unsafe.GetPointer<BattleProjectileQComponent>(projectileEntity);
            Transform2D*                projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);

            SetHeld(f, projectile, true);

            // move the projectile out of bounds after a goal is scored
            switch (winningTeam)
            {
                case BattleTeamNumber.TeamAlpha:
                    projectileTransform->Position = new FPVector2(0, 25);
                    break;
                case BattleTeamNumber.TeamBeta:
                    projectileTransform->Position = new FPVector2(0, -25);
                    break;
            }
        }

        #endregion Public - Gameflow Methods

        #endregion Public

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        #region Private Static Methods

        /// <summary>
        /// Private helper method to get projectile entity
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>Returns projectile entity.</returns>
        private static EntityRef GetProjectileEntity(Frame f)
        {
            ComponentFilter<BattleProjectileQComponent> filter = f.Filter<BattleProjectileQComponent>();
            filter.Next(out EntityRef entity, out _);
            return entity;
        }

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

            f.Events.BattleProjectileChangeSpeed(projectile->Speed);

            s_debugLogger.Log(f, "Projectile Launched");
        }

        /// <summary>
        /// This method checks if the shield hitbox and projectile are in states where they should collide.<br/>
        /// Also sets the projectile to the love emotion state if the condition for that is met.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="shieldCollisionData">Collision data related to the shield.</param>
        /// <param name="normal">The direction in which the projectile should be sent.</param>
        private static bool ProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, BattleCollisionQSystem.PlayerShieldCollisionData* shieldCollisionData, out FPVector2 normal)
        {
            normal = FPVector2.Zero;

            if (!shieldCollisionData->PlayerShieldHitbox->IsActive) return false;
            if (projectile->EmotionCurrent == BattleEmotionState.Love) return false;

            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(shieldCollisionData->PlayerShieldHitbox->PlayerEntity);

            bool isOnTopOfTeammate = false;

            BattlePlayerManager.PlayerHandle teammateHandle = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, playerData->Slot);

            if (teammateHandle.PlayState.IsInPlay())
            {
                EntityRef teammateEntity = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, playerData->Slot).SelectedCharacterEntity;

                Transform2D* playerTransform   = f.Unsafe.GetPointer<Transform2D>(shieldCollisionData->PlayerShieldHitbox->PlayerEntity);
                Transform2D* teammateTransform = f.Unsafe.GetPointer<Transform2D>(teammateEntity);

                BattleGridPosition playerGridPosition   = BattleGridManager.WorldPositionToGridPosition(playerTransform->Position);
                BattleGridPosition teammateGridPosition = BattleGridManager.WorldPositionToGridPosition(teammateTransform->Position);

                isOnTopOfTeammate = playerGridPosition.Row == teammateGridPosition.Row && playerGridPosition.Col == teammateGridPosition.Col;

            }

            // if player is in the same grid cell as teammate, change the projectile to love emotion
            if (isOnTopOfTeammate)
            {
                s_debugLogger.Log(f, "changing projectile emotion to Love");
                SetEmotion(f, projectile, BattleEmotionState.Love);
                shieldCollisionData->IsLoveProjectileCollision = true;

                normal = playerData->TeamNumber == BattleTeamNumber.TeamAlpha ? FPVector2.Up : FPVector2.Down;
                return true;
            }

            if (shieldCollisionData->PlayerShieldHitbox->CollisionType == BattlePlayerCollisionType.None) return false;

            normal = shieldCollisionData->PlayerShieldHitbox->Normal;
            return true;
        }

        #endregion Private Static Methods
    }
}

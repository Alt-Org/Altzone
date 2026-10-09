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
        /// Gets the projectile's EntityRef.
        /// </summary>
        ///
        /// Public version of @cref{GetProjectileEntityRef(Frame\, BattleProjectileSystemDataQSingleton*\, bool)}.<br/>
        /// Used outside of @cref{BattleProjectileQSystem}.<br/>
        /// Also used internally when there's no need for a seperate reference to a @cref{Quantum,BattleProjectileSystemDataQSingleton}.
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>The EntityRef of the projectile.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleProjectileHandle GetProjectileHandle(Frame f)
        {
            BattleProjectileSystemDataQSingleton* projectileSystemData = GetProjectileSystemData(f);
            return GetProjectileHandle(f, projectileSystemData);
        }

        #endregion Public - Helper Methods

        #region Public - Control Methods

        /// <summary>
        /// Creates a projectile entity and initializes its components.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public static void CreateProjectile(Frame f)
        {
            BattleProjectileQSpec spec = BattleQConfig.GetProjectileSpec(f);

            // create a new entity based on the provided prototype
            EntityRef projectileEntityRef                                       = f.Create(spec.ProjectilePrototype);
            BattleEntityManager.CompoundEntityTemplate projectileEntityTemplate = BattleEntityManager.CompoundEntityTemplate.Create(projectileEntityRef);

            // initialize projectile link
            BattleProjectileHandle.CreateLink(f, projectileEntityRef, projectileEntityRef);

            // get handle and other components
            BattleProjectileHandle projectileHandle   = BattleProjectileHandle.Create(f, projectileEntityRef);
            PhysicsCollider2D*     projectileCollider = f.Unsafe.GetPointer<PhysicsCollider2D>(projectileEntityRef);

            //{create projectile trigger entity

            EntityRef projectileTriggerEntityRef = CreateProjectileTriggerEntity(f, projectileHandle, projectileCollider);

            // link trigger
            projectileEntityTemplate.Link(projectileTriggerEntityRef, new FPVector2(0, 0));
            BattleProjectileHandle.CreateLink(f, projectileEntityRef, projectileTriggerEntityRef);

            //} create projectile trigger entity

            // set projectile position and initial values
            projectileHandle.Transform->Position  = new FPVector2(0, 0);
            projectileHandle.Data->Radius         = projectileCollider->Shape.Circle.Radius;
            projectileHandle.Data->EmotionCurrent = 0;
            projectileHandle.Data->EmotionBase    = 0;

            BattleEntityID projectileEntityID = BattleEntityManager.RegisterCompound(f, projectileEntityTemplate);
            GetProjectileSystemData(f)->ProjectileEntityID = projectileEntityID;

            projectileHandle.SetHeld(true);
        }

        /// <summary>
        /// Launches the projectile from an unlaunched state, setting its initial values.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public static void Launch(Frame f)
        {
            // get system data
            BattleProjectileSystemDataQSingleton* projectileSystemData = GetProjectileSystemData(f);

            // get references
            BattleProjectileHandle projectile = GetProjectileHandle(f, projectileSystemData, updateViewPlayState: true);

            // retrieve the projectiles spec
            BattleProjectileQSpec spec = BattleQConfig.GetProjectileSpec(f);

            // copy settings from the spec
            projectile.Data->SpeedBase      = spec.ProjectileInitialSpeed;
            projectile.Data->SpeedMax       = spec.SpeedMax;
            projectile.Data->SpeedIncrement = spec.SpeedIncrement;
            projectile.Data->AttackMax      = spec.AttackMax;

            // set speed and direction
            projectile.Data->Speed     = projectile.Data->SpeedBase;
            projectile.Data->Direction = FPVector2.Rotate(FPVector2.Up, -(FP.Rad_90 + FP.Rad_45));

            // set emotion and attack
            projectile.SetEmotion(f, BattleParameters.GetProjectileInitialEmotion(f));
            projectile.SetAttack(f, 0);

            // reset CollisionFlags for this frame
            projectile.Data->CollisionFlags[(f.Number) % 2] = 0;

            projectile.SetHeld(false);

            BattleEntityManager.TeleportCompound(f, projectile.ERef, new FPVector2(0, 0), FP._0);

            f.Events.BattleProjectileChangeSpeed(projectile.Data->Speed);

            s_debugLogger.Log(f, "Projectile Launched");
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
        public static void HandleIntersection(Frame f, BattleProjectileHandle projectile, EntityRef otherEntity, FPVector2 normal, FP collisionMinOffset)
        {
            Transform2D* otherTransform = f.Unsafe.GetPointer<Transform2D>(otherEntity);

            // calculate how far off from other entity's position is the projectile supposed to hit it's surface
            FPVector2 offsetVector = projectile.Transform->Position - otherTransform->Position;
            FP collisionOffset = FPVector2.Rotate(offsetVector, -FPVector2.RadiansSigned(FPVector2.Up, normal)).Y;

            // if projectile accidentally went inside another entity, lift it out
            if (collisionOffset - projectile.Data->Radius < collisionMinOffset)
            {
                projectile.Transform->Position += normal * (collisionMinOffset - collisionOffset + projectile.Data->Radius);
            }
        }

        #endregion Public - Control Methods

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
        /// Moves the projectile if not held and resets collision flags for the next frame.<br/>
        /// Sets properties based on the <see cref="Battle.QSimulation.Projectile.BattleProjectileQSpec">Projectile spec.</see>
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter@u-exlink</a>.</param>
        public override void Update(Frame f, ref Filter filter)
        {
            BattleProjectileHandle projectileHandle = BattleProjectileHandle.Create(filter);
            projectileHandle.Data->Position = projectileHandle.Transform->Position;

            if (!projectileHandle.Data->IsHeld)
            {
                // move the projectile
                FPVector2 newPosition = projectileHandle.Transform->Position + projectileHandle.Data->Direction * (projectileHandle.Data->Speed * f.DeltaTime);
                BattleEntityManager.MoveCompound(f, GetProjectileHandle(f).ERef, newPosition, FP._0);
            }

            // reset CollisionFlags for next frame
            projectileHandle.Data->CollisionFlags[(f.Number + 1) % 2 ] = 0;
        }

        /// <summary>
        /// Called by BattleCollisionQSystem. Handles the collision based on the specified collision trigger type, handling the collision differently based on what the projectile has hit.<br/>
        /// The projectile will either reflect off of the arena border, set its emotion and bounce off a soul wall, or run checks on what it should do if it hits a player shield.<br/>
        /// Ultimately sends the projectile in a new direction if it is appropriate to do so.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="component">Collision data related to the other entity.</param>
        /// <param name="collisionTriggerType">The collision type of the collision, informing what the projectile has hit.</param>
        public static void OnProjectileCollision(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, void* component, BattleCollisionTriggerType collisionTriggerType)
        {
            if (projectileCollisionData->Projectile.Data->IsHeld) return;

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
                    BattleArenaBorderQComponent* arenaBorder = (BattleArenaBorderQComponent*)component;

                    normal             = arenaBorder->Normal;
                    collisionMinOffset = arenaBorder->CollisionMinOffset;
                    handleCollision    = true;
                    break;

                case BattleCollisionTriggerType.SoulWall:
                    BattleSoulWallQComponent* soulWall = (BattleSoulWallQComponent*)component;

                    // disabled for testing
                    /*if (projectile->EmotionCurrent == BattleEmotionState.Love)
                    {
                        SetEmotion(f, projectile, projectile->EmotionBase);
                    }*/

                    // enabled for testing
                    projectileCollisionData->Projectile.SetEmotion(f, soulWall->Emotion);

                    normal             = soulWall->Normal;
                    collisionMinOffset = soulWall->CollisionMinOffset;
                    speedChange        = SpeedChange.Reset;
                    handleCollision    = true;
                    break;

                case BattleCollisionTriggerType.Shield:
                    BattlePlayerHitboxQComponent* playerShieldHitbox = (BattlePlayerHitboxQComponent*)component;

                    if (FPVector2.Dot(playerShieldHitbox->CalculateNormal(f), projectileCollisionData->Projectile.Data->Direction.Normalized) >= 0) break;

                    if (!ProjectileHitPlayerShield(f, projectileCollisionData, out normal)) break;

                    collisionType      = playerShieldHitbox->CollisionType;
                    collisionMinOffset = playerShieldHitbox->CollisionMinOffset;
                    speedChange        = SpeedChange.Increment;
                    handleCollision    = true;

                    break;

                case BattleCollisionTriggerType.Player:
                    BattlePlayerHitboxQComponent* playerCharacterHitbox = (BattlePlayerHitboxQComponent*)component;

                    if (projectileCollisionData->Projectile.Data->EmotionCurrent == BattleEmotionState.Love) break;

                    if (FPVector2.Dot(playerCharacterHitbox->CalculateNormal(f), projectileCollisionData->Projectile.Data->Direction.Normalized) >= 0) break;

                    normal             = playerCharacterHitbox->CalculateNormal(f);
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
                if (collisionType == BattlePlayerCollisionType.Reflect) direction = FPVector2.Reflect(projectileCollisionData->Projectile.Data->Direction, normal).Normalized;
                else                                                    direction = normal;

                HandleIntersection(f, projectileCollisionData->Projectile, projectileCollisionData->OtherEntityRef, normal, collisionMinOffset);
                UpdateVelocity(f, projectileCollisionData->Projectile.Data, direction, speedChange);
            }

            projectileCollisionData->Projectile.SetCollisionFlag(f, BattleProjectileCollisionFlags.Projectile);
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
            BattleProjectileSystemDataQSingleton* projectileSystemData = GetProjectileSystemData(f);

            BattleProjectileHandle projectile = GetProjectileHandle(f, projectileSystemData);

            projectile.SetHeld(true);
            BattleEntityManager.Return(f, projectileSystemData->ProjectileEntityID);
        }

        #endregion Public - Gameflow Methods

        #endregion Public

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        #region Private Static Methods

        /// <summary>
        /// Private helper method for getting the BattleProjectileSystemDataQSingleton from the %Quantum %Frame.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>Pointer to the BattleProjectileSystemData singleton.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattleProjectileSystemDataQSingleton* GetProjectileSystemData(Frame f)
        {
            if (!f.Unsafe.TryGetPointerSingleton(out BattleProjectileSystemDataQSingleton* projectileSystemData))
            {
                s_debugLogger.Error(f, "ProjectileSystemData singleton not found!");
            }

            return projectileSystemData;
        }

        /// <summary>
        /// Gets the projectile's EntityRef.
        /// </summary>
        ///
        /// Internal version of @cref{GetProjectileEntityRef(Frame)}.<br/>
        /// Used when there's already a reference to <paramref name="projectileSystemData"/>.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileSystemData">Pointer to the projectile system data singleton.</param>
        /// <param name="updateViewPlayState">Whether to update the ViewPlayState.</param>
        ///
        /// <returns>The EntityRef of the projectile.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattleProjectileHandle GetProjectileHandle(Frame f, BattleProjectileSystemDataQSingleton* projectileSystemData, bool updateViewPlayState = false)
        {
            return BattleProjectileHandle.Create(f, BattleEntityManager.Get(f, projectileSystemData->ProjectileEntityID, updateViewPlayState));
        }

        private static EntityRef CreateProjectileTriggerEntity(Frame f, BattleProjectileHandle projectileHandle, PhysicsCollider2D* projectileCollider)
        {
            EntityRef projectileTriggerEntityRef = f.Create();

            // initialize projectile trigger collider
            PhysicsCollider2D projectileTriggerCollider = PhysicsCollider2D.Create(f,
                shape: Shape2D.CreateCircle(projectileCollider->Shape.Circle.Radius),
                isTrigger: true
            );

            // initialize projectile collision trigger component
            BattleCollisionTriggerQComponent projectileCollisionTrigger = new();
            projectileCollisionTrigger.Type = BattleCollisionTriggerType.Projectile;

            // initialize projectile trigger entity
            f.Add<Transform2D>(projectileTriggerEntityRef);
            f.Add(projectileTriggerEntityRef, projectileTriggerCollider);
            f.Add(projectileTriggerEntityRef, projectileCollisionTrigger);

            return projectileTriggerEntityRef;
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
        private static bool ProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, out FPVector2 normal)
        {
            BattlePlayerHandle                playerHandle = BattlePlayerHandle.Create(f, projectileCollisionData->OtherEntityRef);
            BattlePlayerShieldHandle shieldHandle = BattlePlayerShieldHandle.Create(f, projectileCollisionData->OtherEntityRef);

            normal = FPVector2.Zero;

            if (projectileCollisionData->Projectile.Data->EmotionCurrent == BattleEmotionState.Love) return false;

            BattlePlayerData.Ref teammateData = BattlePlayerManager.GetPlayerData(f, BattlePlayerManager.GetTeammateSlot(playerHandle.PlayerData.Slot));

            if (!BattleParameters.GetIsTestFlipperGame(f))
            {
                bool isOnTopOfTeammate = false;

                if (teammateData.PlayState.IsInPlay())
                {
                    BattlePlayerHandle teammateHandle = BattlePlayerHandle.Create(f, BattlePlayerManager.GetTeammateSlot(playerHandle.PlayerData.Slot));

                    BattleGridPosition playerGridPosition = BattleGridManager.WorldPositionToGridPosition(playerHandle.LoadedCharacterTransform->Position);
                    BattleGridPosition teammateGridPosition = BattleGridManager.WorldPositionToGridPosition(teammateHandle.LoadedCharacterTransform->Position);

                    isOnTopOfTeammate = playerGridPosition.Row == teammateGridPosition.Row && playerGridPosition.Col == teammateGridPosition.Col;
                }

                // if player is in the same grid cell as teammate, change the projectile to love emotion
                if (isOnTopOfTeammate)
                {
                    s_debugLogger.Log(f, "changing projectile emotion to Love");
                    projectileCollisionData->Projectile.SetEmotion(f, BattleEmotionState.Love);
                    projectileCollisionData->IsLoveProjectileCollision = true;

                    normal = playerHandle.PlayerData.Team == BattleTeamNumber.TeamAlpha ? FPVector2.Up : FPVector2.Down;
                    return true;
                }
            }

            if (shieldHandle.CollidedHitboxComponent->CollisionType == BattlePlayerCollisionType.None) return false;

            normal = shieldHandle.CollidedHitboxComponent->CalculateNormal(f);
            return true;
        }

        #endregion Private Static Methods
    }
}

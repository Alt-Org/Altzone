/// @file BattleDiamondQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Diamond,BattleDiamondQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which handles spawning diamonds, managing their lifetime and destroying them.
/// </summary>

// Unity usings
using UnityEngine;
using UnityEngine.Scripting;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Diamond
{
    /// <summary>
    /// <span class="brief-h">%Diamond <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
    /// Handles spawning diamonds, managing their lifetime and destroying them.
    /// </summary>
    ///
    /// This system:<br/>
    /// Spawns diamonds when BattleCollisionQSystem calls the @cref{BattleDiamondQSystem,OnProjectileHitSoulWall} method upon SoulWall segment's destruction.<br/>
    /// Filters all diamond entities and handles their lifetime.<br/>
    /// Destroys diamonds when player collects them by colliding with them or if diamond's lifetime ends.
    [Preserve]
    public unsafe class BattleDiamondQSystem : SystemMainThreadFilter<BattleDiamondQSystem.Filter>, ISignalBattleOnDiamondHitPlayer, ISignalBattleOnDiamondHitArenaBorder
    {
        /// <summary>
        /// Filter for filtering diamond entities.
        /// </summary>
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public BattleDiamondDataQComponent* DiamondData;
        }

        /// <summary>
        /// A method called by BattleCollisionQSystem when the projectile collides with a soul wall. If the projectile is not in the held state, calls <see cref="CreateDiamonds"/> to spawn a diamond.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="soulWallCollisionData">Collision data related to the soul wall.</param>
        public static void OnProjectileHitSoulWall(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.SoulWallCollisionData* soulWallCollisionData)
        {
            if (projectileCollisionData->Projectile->IsHeld) return;
            BattleDiamondQSpec diamondSpec = BattleQConfig.GetDiamondSpec(f);

            CreateDiamonds(f, f.Unsafe.GetPointer<Transform2D>(projectileCollisionData->OtherEntity)->Position, soulWallCollisionData->SoulWall->Normal, diamondSpec);
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
        /// Updates each diamond's position and speed while they are traveling. Manages each diamond's lifetime and destroys them if players don't gather them quickly enough.
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter@u-exlink</a>.</param>
        public override void Update(Frame f, ref Filter filter)
        {
            // unpack filter
            BattleDiamondDataQComponent* diamondData = filter.DiamondData;
            Transform2D*                 transform   = filter.Transform;

            BattleDiamondQSpec diamondSpec = BattleQConfig.GetDiamondSpec(f);

            if (diamondData->IsTraveling)
            {
                transform->Position += diamondData->TravelDirection * (diamondData->TravelSpeed * f.DeltaTime);

                if (diamondData->TargetDistance -  FPMath.Abs(transform->Position.Y - diamondData->StartPosition.Y) < diamondSpec.BreakDistance)
                {
                    diamondData->TravelSpeed -= diamondSpec.BreakForce * f.DeltaTime;
                }

                if (FPMath.Abs(transform->Position.Y - diamondData->StartPosition.Y) >= diamondData->TargetDistance || diamondData->TravelSpeed <= 0)
                {
                    diamondData->IsTraveling = false;

                    diamondData->LifetimeTimer = FrameTimer.FromSeconds(f, diamondSpec.LifetimeSec);

                    f.Events.BattleDiamondLanded(filter.Entity);
                }
            }
            else if (!diamondData->LifetimeTimer.IsRunning(f))
            {
                f.Destroy(filter.Entity);
            }
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnDiamondHitPlayer">ISignalBattleOnDiamondHitPlayer</see> is sent.</span><br/>
        /// Destroys diamonds when player hits them and increases diamondcounters.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="diamond">Pointer to the diamond component.</param>
        /// <param name="diamondEntity">EntityRef of the diamond.</param>
        /// <param name="playerHitbox">Pointer to the playerHitbox component.</param>
        /// <param name="playerEntity">EntityRef of the player.</param>
        public void BattleOnDiamondHitPlayer(Frame f, BattleDiamondDataQComponent* diamond, EntityRef diamondEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerEntity)
        {
            if (diamond->IsTraveling) return;

            BattleDiamondCounterQSingleton* diamondCounter = f.Unsafe.GetPointerSingleton<BattleDiamondCounterQSingleton>();
            BattlePlayerDataQComponent*     playerData     = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);

            // increase right team's diamondcounter
            if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha) diamondCounter->AlphaDiamonds++;
            else diamondCounter->BetaDiamonds++;

            f.Destroy(diamondEntity);
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnDiamondHitArenaBorder">ISignalBattleOnDiamondHitArenaBorder</see> is sent.</span><br/>
        /// Reflects the diamonds travel direction off of the arena border.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="diamond">Pointer to the diamond component.</param>
        /// <param name="diamondEntity">EntityRef of the diamond.</param>
        /// <param name="arenaBorder">Pointer to the arena border component.</param>
        /// <param name="arenaBorderEntity">EntityRef of the arena border.</param>
        public void BattleOnDiamondHitArenaBorder(Frame f, BattleDiamondDataQComponent* diamond, EntityRef diamondEntity, BattleArenaBorderQComponent* arenaBorder, EntityRef arenaBorderEntity)
        {
            Transform2D* diamondTransform = f.Unsafe.GetPointer<Transform2D>(diamondEntity);
            Transform2D* arenaTransform   = f.Unsafe.GetPointer<Transform2D>(arenaBorderEntity);
            FP diamondRadius              = f.Unsafe.GetPointer<PhysicsCollider2D>(diamondEntity)->Shape.Circle.Radius;

            FPVector2 offsetVector = diamondTransform->Position - arenaTransform->Position;
            FP collisionOffset = FPVector2.Rotate(offsetVector, -FPVector2.RadiansSigned(FPVector2.Up, arenaBorder->Normal)).Y;

            if (collisionOffset - diamondRadius < arenaBorder->CollisionMinOffset)
            {
                diamondTransform->Position += arenaBorder->Normal * (arenaBorder->CollisionMinOffset - collisionOffset + diamondRadius);
            }

            diamond->TravelDirection = FPVector2.Reflect(diamond->TravelDirection, arenaBorder->Normal).Normalized;
        }

        /// <summary>
        /// Creates diamonds.<br/>
        /// Diamonds are assigned a target vertical distance to travel, a starting direction to begin traveling in and spawned at the destroyed soulwalls position.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="wallPosition">Position of the soulwall where the diamonds spawn at.</param>
        /// <param name="wallNormal">Normal of the SoulWall.</param>
        /// <param name="diamondSpec">The DiamondSpec.</param>
        private static void CreateDiamonds(Frame f, FPVector2 wallPosition, FPVector2 wallNormal, BattleDiamondQSpec diamondSpec)
        {
            // diamond temp variables
            FP        diamondTargetDistance;
            FPVector2 diamondLaunchDirection;
            FP        diamondTravelSpeed;

            // diamond variables
            EntityRef                    diamondEntity;
            BattleDiamondDataQComponent* diamondData;
            Transform2D*                 diamondTransform;

            for (int i = 0; i < diamondSpec.SpawnAmount; i++)
            {
                // set diamond temp variables
                diamondTargetDistance = f.RNG->NextInclusive(diamondSpec.TravelDistanceMin, diamondSpec.TravelDistanceMax);
                diamondLaunchDirection = FPVector2.Rotate(wallNormal, FP.Deg2Rad * f.RNG->NextInclusive(-diamondSpec.SpawnAngleDeg / 2, diamondSpec.SpawnAngleDeg / 2));
                diamondTravelSpeed = diamondSpec.TravelSpeed;

                // create diamond
                diamondEntity = f.Create(diamondSpec.DiamondPrototype);

                // get diamondData component
                diamondData = f.Unsafe.GetPointer<BattleDiamondDataQComponent>(diamondEntity);

                // initialize diamondData component
                diamondData->IsTraveling     = true;
                diamondData->TravelDirection = diamondLaunchDirection;
                diamondData->TravelSpeed     = diamondTravelSpeed;
                diamondData->TargetDistance  = diamondTargetDistance;
                diamondData->StartPosition   = wallPosition;

                // teleport diamond to spawn position
                diamondTransform = f.Unsafe.GetPointer<Transform2D>(diamondEntity);
                diamondTransform->Teleport(f, wallPosition, FP._0);
            }
        }
    }
}

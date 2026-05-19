/// @file BattlePlayerClass100ProjectileQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClass100ProjectileQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which controls player class 100's projectile's movements, creation and reactions to collisions.
/// </summary>

// Unity usings
using UnityEngine.Scripting;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle Qsimulation usings
using Battle.QSimulation.Projectile;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// <span class="brief-h">%PlayerClass100Projectile <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
    /// Handles player class 100 projectile logic, including the projectile's movements, creation and reactions to collisions.
    /// </summary>
    ///
    /// This system:<br/>
    /// Creates and launches player class 100 projectile when called by <see cref="BattlePlayerClass100"/> and updates its movements.<br/>
    /// Contains logic for handling the projectile colliding with different entities.
    [Preserve]
    public unsafe class BattlePlayerClass100ProjectileQSystem : SystemMainThreadFilter<BattlePlayerClass100ProjectileQSystem.Filter>
    {
        /// <summary>
        /// Filter for filtering projectile entities
        /// </summary>
        public struct Filter
        {
            public EntityRef EntityRef;
            public Transform2D* Transform;
            public BattlePlayerClass100ProjectileQComponent* Projectile;
        }

        /// <summary>
        /// Creates the projectile with initial values and launches it
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityPrototype">Entity prototype of the player class 100 projectile.</param>
        /// <param name="position">Position of the player class 100 projectile when it spawns.</param>
        /// <param name="direction">Direction of the player class 100 projectile when it spawns.</param>
        /// <param name="speed">Speed of the projectile when it spawns.</param>
        public static void Create(Frame f, EntityPrototype entityPrototype, FPVector2 position, FPVector2 direction, FP speed)
        {
            // create entity
            EntityRef projectileEntityRef = f.Create(entityPrototype);

            // get components
            Transform2D*                              projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntityRef);
            BattlePlayerClass100ProjectileQComponent* projectile          = f.Unsafe.GetPointer<BattlePlayerClass100ProjectileQComponent>(projectileEntityRef);

            // set Initial projectile direction and speed
            projectileTransform->Position = position;
            projectile->Direction         = direction;
            projectile->Speed             = speed;
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method</a> gets called every frame.</span><br/>
        /// Moves the projectile
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter@u-exlink</a>.</param>
        public override void Update(Frame f, ref Filter filter)
        {
            filter.Transform->Position += filter.Projectile->Direction * filter.Projectile->Speed;
        }

        /// <summary>
        /// Updates the emotion projectile's direction to the player class 100 projectile's direction and destroys the player class 100 projectile
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerClass100Projectile">Pointer to the player class 100 projectile's data component</param>
        /// <param name="playerClass100ProjectileEntityRef">Entity reference of the player class 100 projectile</param>
        /// <param name="emotionProjectile">Pointer to the emotion projectile's data component</param>
        public static void OnProjectileHitEmotionProjectile(
            Frame                                     f,
            BattlePlayerClass100ProjectileQComponent* playerClass100Projectile,
            EntityRef                                 playerClass100ProjectileEntityRef,
            BattleProjectileQComponent*               emotionProjectile
        )
        {
            BattleProjectileQSystem.UpdateVelocity(f, emotionProjectile, playerClass100Projectile->Direction, BattleProjectileQSystem.SpeedChange.None);
            f.Destroy(playerClass100ProjectileEntityRef);
        }

        /// <summary>
        /// Destroys the player class 100 projectile.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerClass100ProjectileEntityRef">Entity reference of the player class 100 projectile</param>
        public static void OnProjectileHitObstacle(Frame f, EntityRef playerClass100ProjectileEntityRef)
        {
            f.Destroy(playerClass100ProjectileEntityRef);
        }
    }
}

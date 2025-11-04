/// @file BattleProjectileSpawnerQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Projectile,BattleProjectileSpawnerQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which spawns the projectile when game starts.
/// </summary>

// Unity usings
using UnityEngine;
using UnityEngine.Scripting;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Projectile
{
    /// <summary>
    /// <span class="brief-h">ProjectileSpawner <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
    /// Handles spawning the projectile at the beginning of the game.<br/>
    /// </summary>
    ///
    /// An invisible entity with BattleProjectileSpawnerQComponent is created when system is initiated,
    /// which is then used to spawn the projectile when BattleGameState is changed to "Playing".
    [Preserve]
    public unsafe class BattleProjectileSpawnerQSystem : SystemMainThreadFilter<BattleProjectileSpawnerQSystem.Filter>
    {
        /// <summary>
        /// Filter for filtering entities with ProjectileSpawner component
        /// </summary>
        public struct Filter
        {
            public EntityRef Entity;
            public BattleProjectileSpawnerQComponent* Spawner;
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System OnInit method@u-exlink</a> gets called when the system is initialized.</span><br/>
        /// Initializes the projectile spawner component on a new entity,
        /// adding the SpawnerSystem.qtn component to ProjectileSpawnerSystem to ensure that the filter works
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public override void OnInit(Frame f)
        {
            // create a new entity
            EntityRef entity = f.Create();

            // add the ProjectileSpawner component
            f.Add<BattleProjectileSpawnerQComponent>(entity);

            // initialize the ProjectileSpawner component
            BattleProjectileSpawnerQComponent* spawner = f.Unsafe.GetPointer<BattleProjectileSpawnerQComponent>(entity);
            spawner->HasSpawned = false;

            Debug.Log("ProjectileSpawnerSystem initialized");
            Debug.Log($"Entity created with ProjectileSpawner component: {entity}");
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum system update method@u-exlink</a> gets called every frame.</span><br/>
        /// Spawns a projectile if it's not already spawned and game is in 'Playing' state.
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="filter">Reference to <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum Filter@u-exlink</a>.</param>
        public override void Update(Frame f, ref Filter filter)
        {
            BattleGameSessionQSingleton* gameSession = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>();

            if (gameSession==null) return;
            if (gameSession->State != BattleGameState.Playing) return;

            // ensure the projectile is spawned only once
            if (!filter.Spawner->HasSpawned)
            {
                Debug.Log("Projectile should spawn");
                BattleProjectileQSpec  spec = BattleQConfig.GetProjectileSpec(f);
                SpawnProjectile(f, spec.ProjectilePrototype);

                // mark projectile as spawned
                filter.Spawner->HasSpawned = true;
            }
        }

        /// <summary>
        /// Creates a projectile entity using the specified prototype and initializes its components.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="childPrototype">Reference to the projectile prototype asset.</param>
        private void SpawnProjectile(Frame f, AssetRef<EntityPrototype> childPrototype)
        {
            // create a new entity based on the provided prototype.
            EntityRef projectileEntity = f.Create(childPrototype);

            // get a pointer to the Transform2D component of the created projectile entity.
            BattleProjectileQComponent* projectile = f.Unsafe.GetPointer<BattleProjectileQComponent>(projectileEntity);
            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);
            PhysicsCollider2D* projectileCollider = f.Unsafe.GetPointer<PhysicsCollider2D>(projectileEntity);

            projectile->Radius = projectileCollider->Shape.Circle.Radius;

            projectileTransform->Position = new FPVector2(0,0);

            projectile->EmotionCurrent = 0;
            projectile->EmotionBase = 0;
        }
    }
}

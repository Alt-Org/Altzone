using UnityEngine;
using UnityEngine.Scripting;

using Photon.Deterministic;
using Quantum;
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Projectile
{
    [Preserve]
    public unsafe class BattleProjectileSpawnerQSystem : SystemMainThreadFilter<BattleProjectileSpawnerQSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public BattleProjectileSpawnerQComponent* Spawner;
        }

        /// <summary>
        /// Called once when the Quantum simulation starts.<br/>
        /// Initializes the projectile spawner component on a new entity.<br/>
        /// adding the SpawnerSystem.qtn component to ProjectileSpawnerSystem to ensure that the filter works
        /// </summary>
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
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum system update method</a> gets called every frame.</span><br/>
        /// Spawns a projectile if not already spawned and game is in 'Playing' state.<br/>
        /// @warning This method should only be called by Quantum.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="filter">Filter containing entity and its projectile spawner component.</param>
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

            projectile->Emotion = 0;
        }
    }
}

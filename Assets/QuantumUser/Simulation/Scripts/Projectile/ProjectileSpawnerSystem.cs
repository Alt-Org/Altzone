using Photon.Deterministic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class ProjectileSpawnerSystem :SystemMainThreadFilter<ProjectileSpawnerSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public ProjectileSpawner* Spawner;
        }

        //Adding the SpawnerSystem.qtn component to ProjectileSpawnerSystem to ensure that the filter works
        public override void OnInit(Frame f)
        {
            // Create a new entity
            EntityRef entity = f.Create();

            // Add the ProjectileSpawner component
            f.Add<ProjectileSpawner>(entity);

            // Initialize the ProjectileSpawner component
            ProjectileSpawner* spawner = f.Unsafe.GetPointer<ProjectileSpawner>(entity);
            spawner->HasSpawned = false;

            Debug.Log("ProjectileSpawnerSystem initialized");
            Debug.Log($"Entity created with ProjectileSpawner component: {entity}");
        }

        public override void Update(Frame f, ref Filter filter)
        {
            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();

            if(gameSession==null) return;
            if (gameSession->state != GameState.Playing) return;

            // Ensure the projectile is spawned only once
            if (!filter.Spawner->HasSpawned)
            {
                Debug.Log("Projectile should spawn");
                ProjectileSpec  config = f.FindAsset(f.RuntimeConfig.ProjectileSpec);
                SpawnProjectile(f, config.ProjectilePrototype);

                // Mark projectile as spawned
                filter.Spawner->HasSpawned = true;
            }
        }

        public void SpawnProjectile(Frame f, AssetRef<EntityPrototype> childPrototype)
        {
            // Retrieve the game configuration asset for custom projectile settings.
            ProjectileSpec config = f.FindAsset(f.RuntimeConfig.ProjectileSpec);

            // Create a new entity based on the provided prototype.
            EntityRef projectileEntity = f.Create(childPrototype);

            // Get a pointer to the Transform2D component of the created projectile entity.
            Projectile* projectile = f.Unsafe.GetPointer<Projectile>(projectileEntity);
            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);
            PhysicsCollider2D* projectileCollider = f.Unsafe.GetPointer<PhysicsCollider2D>(projectileEntity);

            projectile->Radius = projectileCollider->Shape.Circle.Radius;

            projectileTransform->Position = new FPVector2(0,0);
        }
    }
}

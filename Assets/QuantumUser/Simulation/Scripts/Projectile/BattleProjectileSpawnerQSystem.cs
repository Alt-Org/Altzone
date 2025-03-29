using UnityEngine;
using UnityEngine.Scripting;

using Photon.Deterministic;
using Quantum;

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

        // adding the SpawnerSystem.qtn component to ProjectileSpawnerSystem to ensure that the filter works
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

        public override void Update(Frame f, ref Filter filter)
        {
            BattleGameSessionQSingleton* gameSession = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>();

            if (gameSession==null) return;
            if (gameSession->State != BattleGameState.Playing) return;

            // ensure the projectile is spawned only once
            if (!filter.Spawner->HasSpawned)
            {
                Debug.Log("Projectile should spawn");
                BatteProjectileQSpec  config = f.FindAsset(f.RuntimeConfig.ProjectileSpec);
                SpawnProjectile(f, config.ProjectilePrototype);

                // mark projectile as spawned
                filter.Spawner->HasSpawned = true;
            }
        }

        private void SpawnProjectile(Frame f, AssetRef<EntityPrototype> childPrototype)
        {
            // retrieve the game configuration asset for custom projectile settings.
            BatteProjectileQSpec config = f.FindAsset(f.RuntimeConfig.ProjectileSpec);

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

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

        public override void Update(Frame f, ref Filter filter)
        {
            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();
            ProjectileSpawner* spawner = filter.Spawner;

            Debug.Log("spawner "+gameSession->state );
            if(gameSession==null) return;

            if (gameSession->state != GameState.Playing)
            {
                Debug.Log("not playing");
                return;
            }

            // Ensure the projectile is spawned only once
            if (!spawner->HasSpawned) // Assuming a flag exists in GameSession
            {
                Debug.Log("projectile should spawn");
                ProjectileGameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);
                SpawnProjectile(f, config.ProjectilePrototype);

                // Mark projectile as spawned
                spawner->HasSpawned = true;
            }


        }

        // Method to spawn a projectile in the simulation frame.
        public void SpawnProjectile(Frame f, AssetRef<EntityPrototype> childPrototype)
        {
            // Retrieve the game configuration asset for custom projectile settings.
            ProjectileGameConfig config = f.FindAsset(f.RuntimeConfig.GameConfig);

            // Create a new entity based on the provided prototype.
            EntityRef projectile = f.Create(childPrototype);

            // Get a pointer to the Transform2D component of the created projectile entity.
            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectile);

            // Set the initial position of the projectile at a random point on a circle's edge.
            projectileTransform->Position = GetRandomEdgePointOnCircle(f, config.ProjectileSpawnDistanceToCenter);

            // Set the initial rotation of the projectile to a random angle. -Maybe later?
            //projectileTransform->Rotation = GetRandomRotation(f);

            // Check if the projectile entity has a PhysicsBody2D component and get its pointer if available.
            if (f.Unsafe.TryGetPointer<PhysicsBody2D>(projectile, out var body))
            {
                // Set the initial velocity of the projectile, moving in the "up" direction.
                body->Velocity = projectileTransform->Up * config.ProjectileInitialSpeed;

                // Apply a random torque to the projectile to give it rotational movement.
                body->AddTorque(f.RNG->Next(config.ProjectileInitialTorqueMin, config.ProjectileInitialTorqueMax));
            }
        }

        // Helper method to get a random rotation in degrees.
        public static FP GetRandomRotation(Frame f)
        {
            // Returns a random value between 0 and 360 degrees.
            return f.RNG->Next(0, 360);
        }

        // Helper method to get a random point on the edge of a circle with a given radius.
        public static FPVector2 GetRandomEdgePointOnCircle(Frame f, FP radius)
        {
            // Rotates the vector (radius, 0) by a random angle between 0 and 2π to place it on the circle's edge.
            return FPVector2.Rotate(FPVector2.Up * radius, f.RNG->Next() * FP.PiTimes2);
        }

        //Adding the SpawnerSystem.qnt component to ProjectileSpawnerSystem to ensure that the filter works
        public override void OnInit(Frame f)
        {
            Debug.Log("ProjectileSpawnerSystem initialized");

            // Create a new entity
            EntityRef entity = f.Create();

            // Add the ProjectileSpawner component
            f.Add<ProjectileSpawner>(entity);

            // Initialize the ProjectileSpawner component
            ProjectileSpawner* spawner = f.Unsafe.GetPointer<ProjectileSpawner>(entity);
            spawner->HasSpawned = false; // Initialize fields as needed

            Debug.Log($"Entity created with ProjectileSpawner component: {entity}");
        }
    }
}

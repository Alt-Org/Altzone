using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.Scripting;

namespace Quantum.QuantumUser.Simulation.Projectile
{
    [Preserve]
    public unsafe class ProjectileSystem : SystemMainThreadFilter<ProjectileSystem.Filter>, ISignalOnCollisionProjectileHitSoulWall, ISignalOnCollisionProjectileHitSomething
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public PhysicsBody2D* Body;
            public Quantum.Projectile* Projectile;
        }

        // Variable to store the projectile speed from config
        private FP defaultProjectileSpeed;

        private void ProjectileBounce(Frame f,CollisionInfo2D info )
        {
            Debug.Log("Projectile hit a wall");

            // Reflect the direction based on the normal of the collision
            var normal = info.ContactNormal;

            // Get the entity reference of the projectile
            var projectileEntity = info.Entity;

            // Get a pointer to the PhysicsBody2D component of the projectile entity
            var body = f.Unsafe.GetPointer<PhysicsBody2D>(projectileEntity);
            if (body != null)
            {
                // Reflect the velocity vector based on the collision normal
                var velocity = body->Velocity;
                var reflectedDirection = velocity - 2 * FPVector2.Dot(velocity, normal) * normal;

                // Normalize the reflected direction
                var normalizedReflectedDirection = reflectedDirection.Normalized;

                // Set the velocity back with the constant speed from the config
                body->Velocity = normalizedReflectedDirection * defaultProjectileSpeed;
            }
        }

        public override void Update(Frame f, ref Filter filter)
        {
            // Retrieve the projectile speed from the config
            var config = f.FindAsset(filter.Projectile->ProjectileConfig);

            if (!filter.Projectile->IsLaunched) // Access the IsLaunched property from the regenerated component
            {
                defaultProjectileSpeed = config.ProjectileSpeed;

                Debug.Log("Projectile Launched");

                // Apply initial velocity to the projectile body
                filter.Body->Velocity = filter.Transform->Up * defaultProjectileSpeed;

                // Set the IsLaunched field to true to ensure it's launched only once
                filter.Projectile->IsLaunched = true;
            }


            if (config.Cooldown > 0)
            {
                config.Cooldown -= f.DeltaTime; // Decrease the cooldown based on frame time
            }
        }
        // Function to adjust the speed of the projectile
        public void AdjustProjectileSpeed(FP newSpeed)
        {
            defaultProjectileSpeed = newSpeed;
            Debug.Log($"Projectile speed adjusted to: {newSpeed}");
        }

        public void OnCollisionProjectileHitSoulWall(Frame f, CollisionInfo2D info, Quantum.Projectile* projectile, Quantum.SoulWall* soulWall)
        {
            ProjectileBounce(f, info);
        }

        public void OnCollisionProjectileHitSomething(Frame f, CollisionInfo2D info, Quantum.Projectile* projectile)
        {
            ProjectileBounce(f, info);
        }

    }

}

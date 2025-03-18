using UnityEngine;
using UnityEngine.Scripting;

using Photon.Deterministic;

namespace Quantum.QuantumUser.Simulation.Projectile
{
    [Preserve]
    public unsafe class ProjectileSystem : SystemMainThreadFilter<ProjectileSystem.Filter>, ISignalOnTriggerProjectileHitSoulWall, ISignalOnTriggerProjectileHitArenaBorder, ISignalOnTriggerProjectileHitPlayer
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public Quantum.Projectile* Projectile;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            // unpack filter
            Quantum.Projectile* projectile = filter.Projectile;
            Transform2D* transform = filter.Transform;

            if (!projectile->IsLaunched)
            {
                // retrieve the projectile speed from the spec
                ProjectileSpec spec = f.FindAsset(f.RuntimeConfig.ProjectileSpec);

                // set the projectile speed and direction
                projectile->Speed = spec.ProjectileInitialSpeed;
                projectile->Direction = FPVector2.Rotate(FPVector2.Up, -(FP.Rad_90 + FP.Rad_45));

                // set the IsLaunched field to true to ensure it's launched only once
                projectile->IsLaunched = true;

                Debug.Log("Projectile Launched");
            }

            // move the projectile
            transform->Position += projectile->Direction * (projectile->Speed * f.DeltaTime);

            // decrease projectiles cooldown based on frame time
            if (projectile->CoolDown > 0)
            {
                projectile->CoolDown -= f.DeltaTime;
            }
        }

        private void ProjectileBounce(Frame f, Quantum.Projectile* projectile, EntityRef projectileEntity, EntityRef otherEntity, FPVector2 normal, FP collisionMinOffset)
        {
            Debug.Log("[ProjectileSystem] Projectile hit a wall");

            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);
            Transform2D* otherTransform = f.Unsafe.GetPointer<Transform2D>(otherEntity);

            FPVector2 offsetVector = projectileTransform->Position - otherTransform->Position;
            FP collisionOffset = FPVector2.Rotate(offsetVector, -FPVector2.RadiansSigned(FPVector2.Up, normal)).Y;

            projectile->Direction = FPVector2.Reflect(projectile->Direction, normal);

            if (collisionOffset - projectile->Radius < collisionMinOffset)
            {
                projectileTransform->Position += normal * (collisionMinOffset - collisionOffset + projectile->Radius);
            }
        }

        public void OnTriggerProjectileHitSoulWall(Frame f, Quantum.Projectile* projectile, EntityRef projectileEntity, Quantum.SoulWall* soulWall, EntityRef soulWallEntity)
        {
            ProjectileBounce(f, projectile, projectileEntity, soulWallEntity, soulWall->Normal, soulWall->CollisionMinOffset);

            if ((int)projectile->Emotion < soulWall->Layer)
            {
                projectile->Emotion = (EmotionState)soulWall->Layer;
                f.Events.ChangeEmotionState(projectile->Emotion);
            }
        }

        public void OnTriggerProjectileHitArenaBorder(Frame f, Quantum.Projectile* projectile, EntityRef projectileEntity, Quantum.ArenaBorder* arenaBorder, EntityRef arenaBorderEntity)
        {
            ProjectileBounce(f, projectile,  projectileEntity, arenaBorderEntity, arenaBorder->Normal, arenaBorder->CollisionMinOffset);
        }

        public void OnTriggerProjectileHitPlayer(Frame f, Quantum.Projectile* projectile, EntityRef projectileEntity, Quantum.PlayerData* playerData, EntityRef playerEntity)
        {
            ProjectileBounce(f, projectile,  projectileEntity, playerEntity, playerData->Normal, playerData->CollisionMinOffset);
        }
    }
}

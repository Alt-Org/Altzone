using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Projectile
{
    [Preserve]
    public unsafe class BattleProjectileQSystem : SystemMainThreadFilter<BattleProjectileQSystem.Filter>, ISignalBattleOnProjectileHitSoulWall, ISignalBattleOnProjectileHitArenaBorder, ISignalBattleOnProjectileHitPlayerHitbox
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public BattleProjectileQComponent* Projectile;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCollisionFlagSet(Frame f, BattleProjectileQComponent* projectile, BattleProjectileCollisionFlags flag) => projectile->CollisionFlags[f.Number % 2].IsFlagSet(flag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCollisionFlag(Frame f, BattleProjectileQComponent* projectile, BattleProjectileCollisionFlags flag)
        {
            BattleProjectileCollisionFlags flags = projectile->CollisionFlags[f.Number % 2];
            projectile->CollisionFlags[f.Number % 2] = flags.SetFlag(flag);
        }

        public override void Update(Frame f, ref Filter filter)
        {
            // unpack filter
            BattleProjectileQComponent* projectile = filter.Projectile;
            Transform2D* transform = filter.Transform;

            if (!projectile->IsLaunched)
            {
                // retrieve the projectile speed from the spec
                BattleProjectileQSpec spec = f.FindAsset(f.RuntimeConfig.BattleProjectileSpec);

                // set the projectile speed and direction
                projectile->Speed = spec.ProjectileInitialSpeed;
                projectile->Direction = FPVector2.Rotate(FPVector2.Up, -(FP.Rad_90 + FP.Rad_45));

                // pick random EmotionState for projectile
                projectile->Emotion = (BattleEmotionState)f.RNG->NextInclusive((int)BattleEmotionState.Sadness,(int)BattleEmotionState.Aggression);
                f.Events.BattleChangeEmotionState(projectile->Emotion);

                // reset CollisionFlags for this frame
                projectile->CollisionFlags[(f.Number) % 2] = 0;

                // set the IsLaunched field to true to ensure it's launched only once
                projectile->IsLaunched = true;

                Debug.Log("Projectile Launched");
            }

            // move the projectile
            transform->Position += projectile->Direction * (projectile->Speed * f.DeltaTime);

            // reset CollisionFlags for next frame
            projectile->CollisionFlags[(f.Number + 1) % 2 ] = 0;
        }

        private void ProjectileBounce(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, EntityRef otherEntity, FPVector2 normal, FP collisionMinOffset)
        {
            Debug.Log("[ProjectileSystem] Projectile hit a wall");

            if (IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) return;

            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);
            Transform2D* otherTransform = f.Unsafe.GetPointer<Transform2D>(otherEntity);

            FPVector2 offsetVector = projectileTransform->Position - otherTransform->Position;
            FP collisionOffset = FPVector2.Rotate(offsetVector, -FPVector2.RadiansSigned(FPVector2.Up, normal)).Y;

            projectile->Direction = FPVector2.Reflect(projectile->Direction, normal);

            if (collisionOffset - projectile->Radius < collisionMinOffset)
            {
                projectileTransform->Position += normal * (collisionMinOffset - collisionOffset + projectile->Radius);
            }

            SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Projectile);
        }

        public void BattleOnProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall, EntityRef soulWallEntity)
        {
            ProjectileBounce(f, projectile, projectileEntity, soulWallEntity, soulWall->Normal, soulWall->CollisionMinOffset);

            // change projectile's emotion to soulwall's emotion
            projectile->Emotion = soulWall->Emotion;
            f.Events.BattleChangeEmotionState(projectile->Emotion);
        }

        public void BattleOnProjectileHitArenaBorder(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleArenaBorderQComponent* arenaBorder, EntityRef arenaBorderEntity)
        {
            ProjectileBounce(f, projectile,  projectileEntity, arenaBorderEntity, arenaBorder->Normal, arenaBorder->CollisionMinOffset);
        }

        public void BattleOnProjectileHitPlayerHitbox(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerEntity)
        {
            ProjectileBounce(f, projectile,  projectileEntity, playerEntity, playerHitbox->Normal, playerHitbox->CollisionMinOffset);
        }
    }
}

using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Projectile
{
    [Preserve]
    public unsafe class BattleProjectileQSystem : SystemMainThreadFilter<BattleProjectileQSystem.Filter>, ISignalBattleOnProjectileHitSoulWall, ISignalBattleOnProjectileHitArenaBorder, ISignalBattleOnProjectileHitPlayerHitbox, ISignalBattleOnGameOver
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
                BattleProjectileQSpec spec = BattleQConfig.GetProjectileSpec(f);

                // set the projectile speed and direction
                projectile->Speed = spec.ProjectileInitialSpeed;
                projectile->Direction = FPVector2.Rotate(FPVector2.Up, -(FP.Rad_90 + FP.Rad_45));

                // set the speed potential and a timer for speeding up the ball
                projectile->SpeedPotential = projectile->Speed;
                projectile->AccelerationTimer = 10;

                // pick random EmotionState for projectile
                projectile->Emotion = (BattleEmotionState)f.RNG->NextInclusive((int)BattleEmotionState.Sadness,(int)BattleEmotionState.Aggression);
                f.Events.BattleChangeEmotionState(projectile->Emotion);

                // reset CollisionFlags for this frame
                projectile->CollisionFlags[(f.Number) % 2] = 0;

                // set the IsLaunched field to true to ensure it's launched only once
                projectile->IsLaunched = true;

                projectile->IsMoving = true;

                Debug.Log("Projectile Launched");
            }

            if (!projectile->IsMoving) return;

            FP gameTimeSec = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>()->GameTimeSec;

            // every 10 seconds increase the speed potential by a set amount
            if (gameTimeSec >= projectile->AccelerationTimer)
            {
                projectile->SpeedPotential += 1;
                projectile->AccelerationTimer += 10;
            }

            // move the projectile
            transform->Position += projectile->Direction * (projectile->Speed * f.DeltaTime);

            // reset CollisionFlags for next frame
            projectile->CollisionFlags[(f.Number + 1) % 2 ] = 0;
        }

        public void BattleOnProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall, EntityRef soulWallEntity)
        {
            ProjectileVelocityUpdate(f, projectile, projectileEntity, soulWallEntity, soulWall->Normal, soulWall->CollisionMinOffset);

            // change projectile's emotion to soulwall's emotion
            projectile->Emotion = soulWall->Emotion;
            f.Events.BattleChangeEmotionState(projectile->Emotion);
        }

        public void BattleOnProjectileHitArenaBorder(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleArenaBorderQComponent* arenaBorder, EntityRef arenaBorderEntity)
        {
            ProjectileVelocityUpdate(f, projectile,  projectileEntity, arenaBorderEntity, arenaBorder->Normal, arenaBorder->CollisionMinOffset);
        }

        public void BattleOnProjectileHitPlayerHitbox(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerEntity)
        {
            if (playerHitbox->CollisionType == BattlePlayerCollisionType.None) return;
            ProjectileVelocityUpdate(f, projectile,  projectileEntity, playerEntity, playerHitbox->Normal, playerHitbox->CollisionMinOffset, playerHitbox->CollisionType);
        }

        private void ProjectileVelocityUpdate(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, EntityRef otherEntity, FPVector2 normal, FP collisionMinOffset, BattlePlayerCollisionType collisionType = BattlePlayerCollisionType.Reflect)
        {
            if (IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) return;

            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);
            Transform2D* otherTransform = f.Unsafe.GetPointer<Transform2D>(otherEntity);

            // calculate how far off from other entity's position is the projectile supposed to hit it's surface
            FPVector2 offsetVector = projectileTransform->Position - otherTransform->Position;
            FP collisionOffset = FPVector2.Rotate(offsetVector, -FPVector2.RadiansSigned(FPVector2.Up, normal)).Y;

            // set new projectile direction
            if      (collisionType == BattlePlayerCollisionType.Reflect)  projectile->Direction = FPVector2.Reflect(projectile->Direction, normal);
            else if (collisionType == BattlePlayerCollisionType.Override) projectile->Direction = normal;

            // update the projectile's speed based on speed potential
            projectile->Speed = projectile->SpeedPotential;

            // if projectile accidentally went inside another entity, lift it out
            if (collisionOffset - projectile->Radius < collisionMinOffset)
            {
                projectileTransform->Position += normal * (collisionMinOffset - collisionOffset + projectile->Radius);
            }

            SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Projectile);
        }

        public unsafe void BattleOnGameOver(Frame f, BattleTeamNumber winningTeam, BattleProjectileQComponent* projectile, EntityRef projectileEntity)
        {
            // stop the projectile
            projectile->IsMoving = false;

            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);

            // move the projectile out of bounds after a goal is scored
            if (winningTeam == BattleTeamNumber.TeamAlpha)
                projectileTransform->Position += new FPVector2(0, -10);
            else if (winningTeam == BattleTeamNumber.TeamBeta)
                projectileTransform->Position += new FPVector2(0, 10);
        }
    }
}

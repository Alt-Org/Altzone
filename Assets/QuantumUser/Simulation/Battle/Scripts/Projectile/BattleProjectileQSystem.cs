using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;
using Battle.QSimulation.Game;
using Battle.QSimulation.Player;

namespace Battle.QSimulation.Projectile
{
    [Preserve]
    public unsafe class BattleProjectileQSystem : SystemMainThreadFilter<BattleProjectileQSystem.Filter>, ISignalBattleOnProjectileHitSoulWall, ISignalBattleOnProjectileHitArenaBorder, ISignalBattleOnProjectileHitPlayerShield, ISignalBattleOnGameOver
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
                // retrieve the projectiles spec
                BattleProjectileQSpec spec = BattleQConfig.GetProjectileSpec(f);

                // copy data from the spec
                projectile->Speed = spec.ProjectileInitialSpeed;
                projectile->SpeedPotential = projectile->Speed;
                projectile->SpeedIncrement = spec.SpeedIncrement;
                projectile->Direction = FPVector2.Rotate(FPVector2.Up, -(FP.Rad_90 + FP.Rad_45));
                projectile->AccelerationTimerDuration = spec.AccelerationTimerDuration;
                projectile->AccelerationTimer = projectile->AccelerationTimerDuration;
                projectile->AttackMax = spec.AttackMax;
                for (int i = 0; i < spec.SpeedMultiplierArray.Length; i++)
                {
                    projectile->SpeedMultiplierArray[i] = spec.SpeedMultiplierArray[i];
                }

                // set emotion and attack
                SetEmotion(f, projectile, BattleParameters.GetProjectileInitialEmotion(f));
                SetAttack(f, projectile, 0);

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
                projectile->SpeedPotential += projectile->SpeedIncrement;
                projectile->AccelerationTimer += projectile->AccelerationTimerDuration;
            }

            // move the projectile
            transform->Position += projectile->Direction * (projectile->Speed * f.DeltaTime);

            // reset CollisionFlags for next frame
            projectile->CollisionFlags[(f.Number + 1) % 2 ] = 0;
        }

        public void BattleOnProjectileHitSoulWall(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleSoulWallQComponent* soulWall, EntityRef soulWallEntity)
        {
            // change projectile's emotion to soulwall's emotion
            SetEmotion(f, projectile, soulWall->Emotion);

            ProjectileVelocityUpdate(f, projectile, projectileEntity, soulWallEntity, soulWall->Normal, soulWall->CollisionMinOffset);
        }

        public void BattleOnProjectileHitArenaBorder(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattleArenaBorderQComponent* arenaBorder, EntityRef arenaBorderEntity)
        {
            ProjectileVelocityUpdate(f, projectile,  projectileEntity, arenaBorderEntity, arenaBorder->Normal, arenaBorder->CollisionMinOffset);
        }

        public void BattleOnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            if (!playerHitbox->IsActive) return;
            if (projectile->Emotion == BattleEmotionState.Love) return;
            if (playerHitbox->CollisionType == BattlePlayerCollisionType.None) return;

            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);

            // get attack damage from player stats
            SetAttack(f, projectile, playerData->Stats.Attack);
            bool isOnTopOfTeammate = false;

            BattlePlayerManager.PlayerHandle teammateHandle = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, playerData->Slot);

            if (teammateHandle.PlayState.IsInPlay())
            {
                EntityRef teammateEntity = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, playerData->Slot).SelectedCharacter;

                Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(playerHitbox->PlayerEntity);
                Transform2D* teammateTransform = f.Unsafe.GetPointer<Transform2D>(teammateEntity);

                BattleGridPosition playerGridPosition = BattleGridManager.WorldPositionToGridPosition(playerTransform->Position);
                BattleGridPosition teammateGridPosition = BattleGridManager.WorldPositionToGridPosition(teammateTransform->Position);

                isOnTopOfTeammate = playerGridPosition.Row == teammateGridPosition.Row && playerGridPosition.Col == teammateGridPosition.Col;

            }

            // if player is in the same grid cell as teammate, change the projectile to love emotion
            if (isOnTopOfTeammate)
            {
                Debug.Log("[ProjectileSystem] changing projectile emotion to Love");
                SetEmotion(f, projectile, BattleEmotionState.Love);

                // send a projectileVelocityUpdate with the direction being straight up or down depending on the team
                ProjectileVelocityUpdate(f, projectile, projectileEntity, playerHitboxEntity, playerData->TeamNumber == BattleTeamNumber.TeamAlpha ? FPVector2.Up : FPVector2.Down, playerHitbox->CollisionMinOffset, BattlePlayerCollisionType.Override);
                return;
            }
            ProjectileVelocityUpdate(f, projectile, projectileEntity, playerHitboxEntity, playerHitbox->Normal, playerHitbox->CollisionMinOffset, playerHitbox->CollisionType);
        }

        public unsafe void BattleOnGameOver(Frame f, BattleTeamNumber winningTeam, BattleProjectileQComponent* projectile, EntityRef projectileEntity)
        {
            // stop the projectile
            projectile->IsMoving = false;

            Transform2D* projectileTransform = f.Unsafe.GetPointer<Transform2D>(projectileEntity);

            // move the projectile out of bounds after a goal is scored
            switch (winningTeam)
            {
                case BattleTeamNumber.TeamAlpha:
                    projectileTransform->Position += new FPVector2(0, 10);
                    break;
                case BattleTeamNumber.TeamBeta:
                    projectileTransform->Position += new FPVector2(0, -10);
                    break;
            }
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

            // update the projectile's speed based on speed potential and multiply by emotion
            projectile->Speed = projectile->SpeedPotential * projectile->SpeedMultiplierArray[(int)projectile->Emotion];

            // if projectile accidentally went inside another entity, lift it out
            if (collisionOffset - projectile->Radius < collisionMinOffset)
            {
                projectileTransform->Position += normal * (collisionMinOffset - collisionOffset + projectile->Radius);
            }

            SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Projectile);
        }

        private void SetEmotion(Frame f, BattleProjectileQComponent* projectile, BattleEmotionState emotion)
        {
            projectile->Emotion = emotion;
            f.Events.BattleChangeEmotionState(projectile->Emotion);
        }

        private void SetAttack(Frame f, BattleProjectileQComponent* projectile, FP attack)
        {
            projectile->Attack = attack;
            f.Events.BattleProjectileChangeGlowStrength(projectile->Attack / projectile->AttackMax);
        }
    }
}

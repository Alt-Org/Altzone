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
    public unsafe class BattleProjectileQSystem : SystemMainThreadFilter<BattleProjectileQSystem.Filter>, ISignalBattleOnGameOver
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

                SetHeld(f, projectile, false);

                Debug.Log("Projectile Launched");
            }

            FP gameTimeSec = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>()->GameTimeSec;

            // every 10 seconds increase the speed potential by a set amount
            if (gameTimeSec >= projectile->AccelerationTimer)
            {
                projectile->SpeedPotential += projectile->SpeedIncrement;
                projectile->AccelerationTimer += projectile->AccelerationTimerDuration;
            }

            if (!projectile->IsHeld)
            {
                // move the projectile
                transform->Position += projectile->Direction * (projectile->Speed * f.DeltaTime);
            }

            // reset CollisionFlags for next frame
            projectile->CollisionFlags[(f.Number + 1) % 2 ] = 0;
        }

        public static void OnProjectileCollision(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, EntityRef otherEntity, void* otherComponentPtr, BattleCollisionTriggerType collisionType)
        {
            if (projectile->IsHeld) return;
            if (IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Projectile)) return;

            FPVector2 normal = FPVector2.Zero;
            FP collisionMinOffset = FP._0;
            bool updateVelocity = false;

            switch (collisionType)
            {
                case BattleCollisionTriggerType.ArenaBorder:
                    BattleArenaBorderQComponent* arenaBorder = (BattleArenaBorderQComponent*)otherComponentPtr;

                    normal = arenaBorder->Normal;
                    collisionMinOffset = arenaBorder->CollisionMinOffset;
                    updateVelocity = true;
                    break;

                case BattleCollisionTriggerType.SoulWall:

                    BattleSoulWallQComponent* soulWall = (BattleSoulWallQComponent*)otherComponentPtr;

                    SetEmotion(f, projectile, soulWall->Emotion);

                    normal = soulWall->Normal;
                    collisionMinOffset = soulWall->CollisionMinOffset;
                    updateVelocity = true;
                    break;

                case BattleCollisionTriggerType.Shield:
                    BattlePlayerHitboxQComponent* playerHitbox = (BattlePlayerHitboxQComponent*)otherComponentPtr;

                    if (ProjectileHitPlayerShield(f, projectile, projectileEntity, playerHitbox, otherEntity, out normal))
                    {
                        collisionMinOffset = playerHitbox->CollisionMinOffset;
                        updateVelocity = true;
                    }
                    break;

                default:
                    break;
            }

            if (updateVelocity)
            {
                ProjectileUpdateVelocity(f, projectile, projectileEntity, otherEntity, normal, collisionMinOffset);
            }

            SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Projectile);
        }

        public unsafe void BattleOnGameOver(Frame f, BattleTeamNumber winningTeam, BattleProjectileQComponent* projectile, EntityRef projectileEntity)
        {
            SetHeld(f, projectile, true);

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

        public static void ProjectileUpdateVelocity(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, EntityRef otherEntity, FPVector2 normal, FP collisionMinOffset, BattlePlayerCollisionType collisionType = BattlePlayerCollisionType.Reflect)
        {
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
        }

        public static void SetHeld(Frame f, BattleProjectileQComponent* projectile, bool isHeld)
        {
            projectile->IsHeld = isHeld;
        }

        public static void SetEmotion(Frame f, BattleProjectileQComponent* projectile, BattleEmotionState emotion)
        {
            projectile->Emotion = emotion;
            f.Events.BattleChangeEmotionState(projectile->Emotion);
        }

        public static void SetAttack(Frame f, BattleProjectileQComponent* projectile, FP attack)
        {
            projectile->Attack = attack;
            f.Events.BattleProjectileChangeGlowStrength(projectile->Attack / projectile->AttackMax);
        }

        private static bool ProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity, out FPVector2 normal)
        {
            normal = FPVector2.Zero;

            if (!playerHitbox->IsActive) return false;
            if (projectile->Emotion == BattleEmotionState.Love) return false;
            if (playerHitbox->CollisionType == BattlePlayerCollisionType.None) return false;

            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);

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

                normal = playerData->TeamNumber == BattleTeamNumber.TeamAlpha ? FPVector2.Up : FPVector2.Down;
                return true;
            }
            normal = playerHitbox->Normal;
            return true;
        }
    }
}

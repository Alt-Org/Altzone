using Battle.QSimulation.Game;
using Battle.QSimulation.Player;
using Battle.QSimulation.Projectile;

using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace Battle.QSimulation.Player
{
    public class BattlePlayerClassConfluent : BattlePlayerClassBase
    {
        public override BattlePlayerCharacterClass Class { get; } = BattlePlayerCharacterClass.Confluent;

        public override unsafe void OnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            if (!playerHitbox->IsActive) return;
            if (projectile->EmotionCurrent == BattleEmotionState.Love) return;

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

            FPVector2 normal;

            // if player is in the same grid cell as teammate, change the projectile to love emotion
            if (isOnTopOfTeammate)
            {
                Debug.Log("[ProjectileSystem] changing projectile emotion to Love");
                BattleProjectileQSystem.SetEmotion(f, projectile, BattleEmotionState.Love);

                normal = playerData->TeamNumber == BattleTeamNumber.TeamAlpha ? FPVector2.Up : FPVector2.Down;
                return;
            }
            normal = f.Unsafe.GetPointer<Transform2D>(projectileEntity)->Position - f.Unsafe.GetPointer<Transform2D>(playerHitbox->PlayerEntity)->Position;

            BattleProjectileQSystem.HandleIntersection(f, projectile, projectileEntity, playerHitboxEntity, normal, playerHitbox->CollisionMinOffset);
            BattleProjectileQSystem.UpdateVelocity(f, projectile, FPVector2.Reflect(projectile->Direction, normal).Normalized, projectile->SpeedIncrement, false);
        }
    }
}

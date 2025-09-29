/// @file BattlePlayerClassConfluent.cs
/// <summary>
/// Contains the logic for Confluent class characters.
/// </summary>
///
/// Contains code overriding shield/projectile collisions to have the projectile always reflect off of the character's shield based on a calculated normal.

using Battle.QSimulation.Game;
using Battle.QSimulation.Projectile;

using Photon.Deterministic;
using Quantum;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Class containing code for Confluent class characters.
    /// </summary>
    public class BattlePlayerClassConfluent : BattlePlayerClassBase
    {
        /// <summary>The BattlePlayerCharacterClass this class is for.</summary>
        public override BattlePlayerCharacterClass Class { get; } = BattlePlayerCharacterClass.Confluent;

        /// <summary>
        /// Called by BattlePlayerClassManager. Reflects the projectile off of the characters hitbox based on a normal calculated from the characters center to the projectiles position.
        /// Does nothing if the player and their teammate are overlapping and the projectile should be set to Love emotion.
        /// </summary>
        /// 
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="projectileEntity">EntityRef of the projectile.</param>
        /// <param name="playerHitbox">Pointer to the PlayerHitbox component.</param>
        /// <param name="playerHitboxEntity">EntityRef of the player hitbox.</param>
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

            if (!isOnTopOfTeammate)
            {
                FPVector2 normal = f.Unsafe.GetPointer<Transform2D>(projectileEntity)->Position - f.Unsafe.GetPointer<Transform2D>(playerHitbox->PlayerEntity)->Position;

                BattleProjectileQSystem.HandleIntersection(f, projectile, projectileEntity, playerHitboxEntity, normal, playerHitbox->CollisionMinOffset);
                BattleProjectileQSystem.UpdateVelocity(f, projectile, FPVector2.Reflect(projectile->Direction, normal).Normalized, projectile->SpeedIncrement, false);
            }
        }
    }
}

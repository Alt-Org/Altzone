using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;
using Battle.QSimulation.Projectile;

namespace Battle.QSimulation.Player
{
    [Preserve]
    public unsafe class BattlePlayerQSystem : SystemMainThread, ISignalBattleOnProjectileHitPlayerHitbox
    {
        //private BattlePlayerDataQComponent* _damagedPlayerData;

        //private FP _damageTaken = FP._0;

        public static void SpawnPlayers(Frame f)
        {
            foreach (BattlePlayerManager.PlayerHandle playerHandle in BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f))
            {
                if (playerHandle.PlayState == BattlePlayerPlayState.NotInGame) continue;

                BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, 0);
            }
        }

        public unsafe void BattleOnProjectileHitPlayerHitbox(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Player)) return;

            BattlePlayerDataQComponent* _damagedPlayerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);
            FP _damageTaken = FP._1;

            int characterNumber = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, _damagedPlayerData->Slot).SelectedCharacterNumber;

            FP newHp = _damagedPlayerData->CurrentHp - _damageTaken;

            if (_damageTaken > FP._0 && _damagedPlayerData->CurrentHp > 0 && !_damagedPlayerData->DamageCooldown.IsRunning(f))
            {
                _damagedPlayerData->CurrentHp = newHp;
                _damageTaken = FP._0;

                _damagedPlayerData->DamageCooldown = FrameTimer.FromSeconds(f, 1);

                f.Events.BattleCharacterTakeDamage(playerHitbox->PlayerEntity, _damagedPlayerData->TeamNumber, _damagedPlayerData->Slot, characterNumber, newHp / _damagedPlayerData->Stats.Hp);
            }

            if (_damagedPlayerData->CurrentHp <= FP._0)
            {
                BattlePlayerManager.DespawnPlayer(f, _damagedPlayerData->Slot);
            }

            BattleProjectileQSystem.SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Player);
        }

        public override void Update(Frame f)
        {
            Input* input;

            EntityRef playerEntity;
            BattlePlayerDataQComponent* playerData;
            Transform2D* playerTransform;

            foreach (BattlePlayerManager.PlayerHandle playerHandle in BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f))
            {
                if (playerHandle.PlayState == BattlePlayerPlayState.NotInGame) continue;

                input = f.GetPlayerInput(playerHandle.PlayerRef);

                if (input->PlayerCharacterNumber > -1)
                {
                    BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, input->PlayerCharacterNumber);
                    continue;
                }

                if (playerHandle.PlayState == BattlePlayerPlayState.OutOfPlay) continue;

                playerEntity = playerHandle.SelectedCharacter;
                playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerEntity);
                playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);

                BattlePlayerMovementController.UpdateMovement(f, playerData, playerTransform, input);
            }
        }
    }
}

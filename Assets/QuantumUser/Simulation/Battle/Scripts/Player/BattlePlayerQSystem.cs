using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;
using UnityEngine;

namespace Battle.QSimulation.Player
{
    [Preserve]
    public unsafe class BattlePlayerQSystem : SystemMainThread, ISignalBattleOnProjectileHitPlayerHitbox
    {
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
            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);

            playerData->CurrentHp -= FP._1;

            if (playerData->CurrentHp <= 0)
            {
                BattlePlayerManager.DespawnPlayer(f, playerData->Slot);
            }
        }

        public override void Update(Frame f)
        {
            Quantum.Input* input;

            EntityRef                   playerEntity;
            BattlePlayerDataQComponent* playerData;
            Transform2D*                playerTransform;

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

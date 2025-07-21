using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

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
            Input* input;

            EntityRef                   playerEntity;
            BattlePlayerDataQComponent* playerData;
            Transform2D*                playerTransform;

            foreach (BattlePlayerManager.PlayerHandle playerHandle in BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f))
            {
                if (playerHandle.PlayState != BattlePlayerPlayState.InPlay) continue;

                input = f.GetPlayerInput(playerHandle.PlayerRef);

                playerEntity    = playerHandle.SelectedCharacter;
                playerData      = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerEntity);
                playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);

                if (input->PlayerCharacterNumber > -1)
                {
                    BattlePlayerManager.SpawnPlayer(f, playerData->Slot, input->PlayerCharacterNumber);
                    continue;
                }

                BattlePlayerMovementController.UpdateMovement(f, playerData, playerTransform, input);
            }
        }
    }
}

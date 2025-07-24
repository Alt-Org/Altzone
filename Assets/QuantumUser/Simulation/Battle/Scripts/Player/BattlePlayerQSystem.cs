using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    [Preserve]
    public unsafe class BattlePlayerQSystem : SystemMainThread
    {
        public static void SpawnPlayers(Frame f)
        {
            foreach (BattlePlayerManager.PlayerHandle playerHandle in BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f))
            {
                if (playerHandle.PlayState == BattlePlayerPlayState.NotInGame) continue;

                BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, 0);
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

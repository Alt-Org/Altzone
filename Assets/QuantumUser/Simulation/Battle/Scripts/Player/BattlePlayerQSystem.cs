using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    [Preserve]
    public unsafe class BattlePlayerQSystem : SystemMainThread
    {
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

                BattlePlayerMovementController.UpdateMovement(f, playerData, playerTransform, input);
            }
        }
    }
}

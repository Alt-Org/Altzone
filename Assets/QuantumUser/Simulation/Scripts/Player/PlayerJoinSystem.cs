using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerJoinSystem  : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            RuntimePlayer data = f.GetPlayerData(player);

            BattlePlayerSlot playerSlot = PlayerManager.InitPlayer(f, player);
            PlayerManager.SpawnPlayer(f, playerSlot, 0);

            f.Events.UpdateDebugStatsOverlay(data.Characters[0]);
        }

    }
}

using UnityEngine.Scripting;
using Quantum;

namespace Battle.QSimulation.Player
{
    [Preserve]
    public unsafe class BattlePlayerJoinQSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            /*
            RuntimePlayer data = f.GetPlayerData(player);

            BattlePlayerSlot playerSlot = BattlePlayerManager.InitPlayer(f, player);
            BattlePlayerManager.SpawnPlayer(f, playerSlot, 0);

            f.Events.BattleDebugUpdateStatsOverlay(data.Characters[0]);
            */
        }
    }
}

using UnityEngine.Scripting;
using Quantum;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Quantum system activated when a new player joins the game.<br/>
    /// Initializes player slots, spawns entities, and updates debug overlays.
    /// </summary>
    [Preserve]
    public unsafe class BattlePlayerJoinQSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        /// <summary>
        /// Called when a player is added.
        /// </summary>
        /// <param name="f">Quantum simulation frame.</param>
        /// <param name="player">Reference to the player.</param>
        /// <param name="firstTime">True if this is the first join.</param> 
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            RuntimePlayer data = f.GetPlayerData(player);

            BattlePlayerSlot playerSlot = BattlePlayerManager.InitPlayer(f, player);
            BattlePlayerManager.SpawnPlayer(f, playerSlot, 0);

            f.Events.BattleDebugUpdateStatsOverlay(data.Characters[0]);
        }
    }
}

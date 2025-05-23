/// @file BattlePlayerJoinQSystem.cs
/// <summary>
/// Handles players joining the game
/// </summary>
///
/// This system handles players joining the game by using BattlePlayerManager to initialize players in playerslots and spawn them.
/// Also updates debug overlay to display player's stats.


using UnityEngine.Scripting;
using Quantum;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// <span class="brief-h">PlayerJoin <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Handles new players joining the game.
    /// Uses BattlePlayerManager to initialize players in playerslots and spawn them.
    /// </summary>
    [Preserve]
    public unsafe class BattlePlayerJoinQSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <a href="https://doc-api.photonengine.com/en/quantum/current/interface_quantum_1_1_i_signal_on_player_added.html">ISignalOnPlayerAdded</a> is sent.</span><br/>
        /// Called when a player is added for the first time. Uses BattlePlayerManager to initialize players in playerslots and spawn them.
        /// @warning
        /// This method should only be called via Quantum signal.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
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

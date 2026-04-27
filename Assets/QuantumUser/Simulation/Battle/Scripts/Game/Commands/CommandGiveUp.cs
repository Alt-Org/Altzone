/// @file CommandGiveUp.cs
/// <summary>
/// A deterministic command that triggers the give up logic for a specific player.
/// </summary>

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// A deterministic command that triggers the give up logic for a specific player.
    /// </summary>
    public class CommandGiveUp : DeterministicCommand
    {
        /// <summary>
        /// Method that is required to be implemented but not needed here.
        /// </summary>
        public override void Serialize(BitStream stream) { }

        /// <summary>
        /// Executes the give up logic for the specified player.
        /// </summary>
        ///
        /// <param name="f">The current simulation frame</param>
        /// <param name="playerHandle">The player handle of the player initiating the command.</param>
        public void Execute(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            BattlePlayerQSystem.HandleGiveUp(f,  playerHandle);
        }
    }
}

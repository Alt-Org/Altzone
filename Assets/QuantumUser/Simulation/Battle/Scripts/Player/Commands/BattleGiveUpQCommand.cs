/// @file BattleGiveUpQCommand.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattleGiveUpQCommand} class which contains a deterministic command that triggers the give up logic for a specific player.
/// </summary>

// Quantum usings
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// A deterministic command that triggers the give up logic for a specific player.
    /// </summary>
    public class BattleGiveUpQCommand : BattleCommand
    {
        public override Type BattleCommandType => Type.GiveUp;

        /// <summary>
        /// Method that is required to be implemented but not needed here.
        /// </summary>
        public override void Serialize(BitStream _) { }
    }
}

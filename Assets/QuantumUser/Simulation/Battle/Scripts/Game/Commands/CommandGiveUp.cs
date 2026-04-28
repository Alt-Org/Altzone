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
    public class BattleGiveUpQCommand : BattleCommand
    {
        public override Type BattleCommandType => Type.GiveUp;

        /// <summary>
        /// Method that is required to be implemented but not needed here.
        /// </summary>
        public override void Serialize(BitStream _) { }
    }
}

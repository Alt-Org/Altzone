/// @file CommandActivateAbility.cs
/// <summary>
/// A deterministic command that triggers the ability activation for a specific player.
/// </summary>

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// A deterministic command that triggers the ability activation for a specific player.
    /// </summary>
    public unsafe class BattleCharacterAbilityQCommand : BattleCommand
    {
        public override Type BattleCommandType => Type.ActivateAbility;
        /// <summary>
        /// Method that is required to be implemented but not needed here.
        /// </summary>
        public override void Serialize(BitStream _) { }
    }
}

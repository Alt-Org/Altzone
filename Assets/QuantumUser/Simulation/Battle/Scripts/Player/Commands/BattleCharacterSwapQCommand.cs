/// @file CommandSwapCharacter.cs
/// <summary>
/// A deterministic command that triggers the character swapping logic for a specific player.
/// </summary>

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// A deterministic command that triggers the character swapping logic for a specific player.
    /// </summary>
    public class BattleCharacterSwapQCommand : BattleCommand
    {
        public override Type BattleCommandType => Type.SwapCharacter;
        /// <summary>
        /// The number of the character the player is attempting to swap to.
        /// </summary>
        public int CharacterNumber;

        /// <summary>
        /// Serializes the character number into the bitstream for network transmission.
        /// </summary>
        /// <param name="stream">The bitstream to write to</param>
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref CharacterNumber);
        }
    }
}

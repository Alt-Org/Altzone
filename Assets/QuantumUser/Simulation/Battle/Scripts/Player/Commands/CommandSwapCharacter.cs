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
    public class CommandSwapCharacter : DeterministicCommand
    {
        /// <summary>
        /// The number of the character the player is attempting to swap to.
        /// </summary>
        public int CharacterNumber;

        /// <summary>
        /// Serializes the character number into the bitstream for network transmission.
        /// </summary>
        /// <param name="stream">The bitstream to write to or read from.</param>
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref CharacterNumber);
        }

        /// <summary>
        /// Executes the character swapping logic for the specified player.
        /// </summary>
        ///
        /// <param name="f">The current simulation frame.</param>
        /// <param name="playerHandle">The player handle of the player initiating the command.</param>
        public void Execute(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            BattlePlayerQSystem.HandleCharacterSwapping(f, playerHandle, CharacterNumber);
        }
    }
}

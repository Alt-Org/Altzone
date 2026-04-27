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
    public unsafe class CommandActivateAbility : DeterministicCommand
    {
        /// <summary>
        /// Method that is required to be implemented but not needed here.
        /// </summary>
        public override void Serialize(BitStream stream) { }

        /// <summary>
        /// Executes the ability activation logic by setting an input buffer for the specified player.
        /// </summary>
        ///
        /// <param name="f">The current simulation frame.</param>
        /// <param name="playerHandle">The player handle of the player initiating the command.</param>
        public void Execute(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHandle.SelectedCharacterEntity);
            playerData->AbilityActivateBufferSec = FrameTimer.FromSeconds(f, FP._0_50);
        }
    }
}

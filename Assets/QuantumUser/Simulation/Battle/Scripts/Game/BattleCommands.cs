/// @file BattleCommands.cs
/// <summary>
/// Contains Battlecommand class and all battle commands
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Game
{
    public abstract class BattleCommand : DeterministicCommand
    {
        public enum Type
        {
            None,
            GiveUp,
            ActivateAbility,
            SwapCharacter
        }

        public abstract Type BattleCommandType { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetCommand(Frame f, PlayerRef playerRef, out BattleCommand commandData)
        {
            commandData = (BattleCommand)f.GetPlayerCommand(playerRef);
            if (commandData == null) return Type.None;
            return commandData.BattleCommandType;
        }
    }
}

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// A deterministic command that triggers the ability activation for a specific player.
    /// </summary>
    public class BattleCharacterAbilityQCommand : BattleCommand
    {
        public override Type BattleCommandType => Type.ActivateAbility;
        /// <summary>
        /// Method that is required to be implemented but not needed here.
        /// </summary>
        public override void Serialize(BitStream _) { }
    }

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

/// @file BattleCommands.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Game,BattleCommand} class and all battle commands
///
/// See [{Custom Quantum Commands}](#page-concepts-commands) for more info.
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Game
{
    /// <summary>
    /// Abstract base class for all deterministic battle commands.<br/>
    /// All new commands must be added to the <see cref="Type"/> enum.
    /// </summary>
    ///
    /// See [{BattleCommand (base class)}](#page-concepts-commands-battle-qcommand) for more info.
    public abstract class BattleCommand : DeterministicCommand
    {
        /// <summary>
        /// Type of the command. None if no command.
        /// </summary>
        public enum Type
        {
            /// <summary>No command</summary>
            None,
            /// <summary><See cref="BattleGiveUpQCommand"/></summary>
            GiveUp,
            /// <summary><See cref="BattleCharacterAbilityQCommand"/></summary>
            ActivateAbility,
            /// <summary><See cref="BattleCharacterSwapQCommand"/></summary>
            SwapCharacter
        }

        public abstract Type BattleCommandType { get; }

        /// <summary>
        /// Fetches a command for a <paramref name="playerRef"/>  and returns its <see cref="Type"/>. The <paramref name="commandData"/> out parameter contains the command itself.
        /// </summary>
        ///
        /// <param name="f">The current simulation frame.</param>
        /// <param name="playerRef">Reference to the player whose command is fetched.</param>
        /// <param name="commandData">Contains the command, or null if there isn't one. (<b>out param</b>)</param>
        ///
        /// <returns>
        /// The <see cref="Type"/> of the command, or <see cref="Type.None"/> if no command was found.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetCommand(Frame f, PlayerRef playerRef, out BattleCommand commandData)
        {
            commandData = (BattleCommand)f.GetPlayerCommand(playerRef);
            if (commandData == null) return Type.None;
            return commandData.BattleCommandType;
        }
    }

    /// <summary>
    /// A deterministic command that triggers the ability activation for a specific player.
    /// </summary>
    ///
    /// See [{Custom Quantum Commands}](#page-concepts-commands) for more info.
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
    ///
    /// See [{Custom Quantum Commands}](#page-concepts-commands) for more info.
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
    ///
    /// See [{Custom Quantum Commands}](#page-concepts-commands) for more info.
    public class BattleGiveUpQCommand : BattleCommand
    {
        public override Type BattleCommandType => Type.GiveUp;

        /// <summary>
        /// Method that is required to be implemented but not needed here.
        /// </summary>
        public override void Serialize(BitStream _) { }
    }
}

using System.Runtime.CompilerServices;
using Photon.Deterministic;
using Quantum;

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

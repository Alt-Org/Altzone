// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    public class CommandSwapCharacter : DeterministicCommand
    {
        public int CharacterNumber;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref CharacterNumber);
        }

        public void Execute(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            BattlePlayerQSystem.HandleCharacterSwapping(f, playerHandle, CharacterNumber);
        }
    }
}

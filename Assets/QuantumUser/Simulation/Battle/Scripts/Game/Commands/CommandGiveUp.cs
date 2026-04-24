// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    public class CommandGiveUp : DeterministicCommand
    {
        public override void Serialize(BitStream stream) { }

        public void Execute(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            BattlePlayerQSystem.HandleGiveUp(f,  playerHandle);
        }
    }
}

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    public unsafe class CommandActivateAbility : DeterministicCommand
    {
        public override void Serialize(BitStream stream) { }

        public void Execute(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHandle.SelectedCharacterEntity);
            playerData->AbilityActivateBufferSec = FrameTimer.FromSeconds(f, FP._0_50);
        }
    }
}

// System usings
using System.Collections.Generic;
using Battle.QSimulation.Player;

// Quantum usings
using Photon.Deterministic;

namespace Quantum
{
    public static partial class DeterministicCommandSetup
    {
        static partial void AddCommandFactoriesUser(ICollection<IDeterministicCommandFactory> factories, RuntimeConfig gameConfig, SimulationConfig simulationConfig)
        {
            // Add or remove commands to the collection.
            // factories.Add(new NavMeshAgentTestSystem.RunTest());

            factories.Add(new CommandGiveUp());
            factories.Add(new CommandSwapCharacter());
        }
    }
}

/// @file CommandSetup.User.cs
/// <summary>
/// Registers the game's custom deterministic commands with the %Quantum simulation.
/// </summary>

// System usings
using System.Collections.Generic;
using Battle.QSimulation.Player;

// Quantum usings
using Photon.Deterministic;

namespace Quantum
{
    /// <summary>
    /// Contains all project-specific commands.
    /// </summary>
    public static partial class DeterministicCommandSetup
    {
        /// <summary>
        /// Registers the game's custom <see cref="IDeterministicCommandFactory"/> instances with
        /// the %Quantum command system. Called once by the framework during simulation
        /// initialization. Should not be called manually.
        /// </summary>
        ///
        /// <param name="factories">The command factory collection to register commands into</param>
        /// <param name="gameConfig">The runtime game configuration for the current session</param>
        /// <param name="simulationConfig">The simulation configuration for the current session</param>
        static partial void AddCommandFactoriesUser(ICollection<IDeterministicCommandFactory> factories, RuntimeConfig gameConfig, SimulationConfig simulationConfig)
        {
            // Add or remove commands to the collection.
            // factories.Add(new NavMeshAgentTestSystem.RunTest());

            factories.Add(new BattleGiveUpQCommand());
            factories.Add(new BattleCharacterSwapQCommand());
            factories.Add(new BattleCharacterAbilityQCommand());
        }
    }
}

/// @file CommandSetup.User.cs
/// <summary>
/// Contains @cref{Quantum,DeterministicCommandSetup} class contains all project-specific commands.
/// </summary>

// System usings
using System.Collections.Generic;

// Quantum usings
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Quantum
{
    /// <summary>
    /// Contains all project-specific commands.
    /// </summary>
    public static partial class DeterministicCommandSetup
    {
        /// <summary>
        /// Registers the game's custom <see cref="IDeterministicCommandFactory"/> instances with
        /// the %Quantum command system. Called once during the simulation initialization.
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        ///
        /// <param name="factories">The command factory collection to register commands into</param>
        /// <param name="gameConfig">The runtime game configuration for the current session</param>
        /// <param name="simulationConfig">The simulation configuration for the current session</param>
        static partial void AddCommandFactoriesUser(ICollection<IDeterministicCommandFactory> factories, RuntimeConfig gameConfig, SimulationConfig simulationConfig)
        {
            factories.Add(new BattleGiveUpQCommand());
            factories.Add(new BattleCharacterSwapQCommand());
            factories.Add(new BattleCharacterAbilityQCommand());
        }
    }
}

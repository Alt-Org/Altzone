/// @file BattlePlayerPlayStateExtension.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerPlayStateExtension} class which has extension methods for BattlePlayerPlayState enum.
/// </summary>

using Quantum;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Extension class for BattlePlayerPlayState enum.<br/>
    /// Implements extension methods for BattlePlayerPlayState enum.
    /// </summary>
    ///
    /// This is used to ease checking the subsstates of the BattlePlayerPlayState enum.<br/>
    /// Checking superstates also checks if the current state is one of the substates of the superstate.<br/>
    /// Checks for states that have no substates are also implemented for consistency.
    public static class BattlePlayerPlayStateExtension
    {
        /// <summary>
        /// Checks if the play state of player is <see cref="Quantum.BattlePlayerPlayState.NotInGame">NotInGame</see>.
        /// </summary>
        ///
        /// <param name="state">Player play state.</param>
        ///
        /// <returns>True if play state is <see cref="Quantum.BattlePlayerPlayState.NotInGame">NotInGame</see>.</returns>
        public static bool IsNotInGame(this BattlePlayerPlayState state) => state is BattlePlayerPlayState.NotInGame;

        /// <summary>
        /// Checks if the play state of player is any of the substates that count as <b>InGame</b>.
        /// </summary>
        ///
        /// <param name="state">Player play state.</param>
        ///
        /// <returns>True if play state is not <see cref="Quantum.BattlePlayerPlayState.NotInGame">NotInGame</see>.</returns>
        public static bool IsInGame(this BattlePlayerPlayState state) => state is not BattlePlayerPlayState.NotInGame;

        /// <summary>
        /// Checks if the play state of player is any of the substates that count as <see cref="Quantum.BattlePlayerPlayState.OutOfPlay">OutOfPlay</see>.
        /// </summary>
        ///
        /// <param name="state">Player play state.</param>
        ///
        /// <returns>
        /// True if play state is
        /// <see cref="Quantum.BattlePlayerPlayState.OutOfPlay">OutOfPlay</see> or
        /// <see cref="Quantum.BattlePlayerPlayState.OutOfPlayRespawning">OutOfPlayRespawning</see> or
        /// <see cref="Quantum.BattlePlayerPlayState.OutOfPlayFinal">OutOfPlayFinal</see>.
        /// </returns>
        public static bool IsOutOfPlay(this BattlePlayerPlayState state) => state is BattlePlayerPlayState.OutOfPlay or BattlePlayerPlayState.OutOfPlayRespawning or BattlePlayerPlayState.OutOfPlayFinal;

        /// <summary>
        /// Checks if the play state of player is <see cref="Quantum.BattlePlayerPlayState.OutOfPlayRespawning">OutOfPlayRespawning</see>.
        /// </summary>
        ///
        /// <param name="state">Player play state.</param>
        ///
        /// <returns>True if play state is <see cref="Quantum.BattlePlayerPlayState.OutOfPlayRespawning">OutOfPlayRespawning</see>.</returns>
        public static bool IsOutOfPlayRespawning(this BattlePlayerPlayState state) => state is BattlePlayerPlayState.OutOfPlayRespawning;

        /// <summary>
        /// Checks if the play state of player is <see cref="Quantum.BattlePlayerPlayState.OutOfPlayFinal">OutOfPlayFinal</see>.
        /// </summary>
        ///
        /// <param name="state">Player play state.</param>
        ///
        /// <returns>True if play state is <see cref="Quantum.BattlePlayerPlayState.OutOfPlayFinal">OutOfPlayFinal</see>.</returns>
        public static bool IsOutOfPlayFinal(this BattlePlayerPlayState state) => state is BattlePlayerPlayState.OutOfPlayFinal;

        /// <summary>
        /// Checks if the play state of player is <see cref="Quantum.BattlePlayerPlayState.InPlay">InPlay</see>.
        /// </summary>
        ///
        /// <param name="state">Player play state.</param>
        ///
        /// <returns>True if play state is <see cref="Quantum.BattlePlayerPlayState.InPlay">InPlay</see>.</returns>
        public static bool IsInPlay(this BattlePlayerPlayState state) => state is BattlePlayerPlayState.InPlay;
    }
}

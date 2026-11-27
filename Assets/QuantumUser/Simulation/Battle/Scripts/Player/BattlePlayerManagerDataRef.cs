
// System usings
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;

namespace Battle.QSimulation.Player
{
    // Adds a private internally used data class
    // Main class definition and documentation in BattlePlayerManager.cs
    public unsafe partial class BattlePlayerManager
    {
        /// <summary>
        /// Private class used internally by <see cref="BattlePlayerManager"/> for accessing <b>%Player</b> related <b>%Data</b>.
        /// </summary>
        ///
        /// This class handles both constant **%Data** access that is not affected by the current state of the game and **%Data** access that is affected by it.<br/>
        /// **%Data** that is affected by the current state of the game is stored in the @cref{Quantum,BattlePlayerManagerDataQSingleton}.
        ///
        /// For simplicity, all **%Data** access methods are implemented statically, but methods that are affected by the current state of the game take the
        /// @cref{Battle.QSimulation.Player.BattlePlayerManager,Data.Ref} struct as an argument.<br/>
        /// The @cref{Battle.QSimulation.Player.BattlePlayerManager,Data.Ref} is a wrapper for the @cref{Quantum,BattlePlayerManagerDataQSingleton}.
        private static class Data
        {
            /// <summary>
            /// A wrapper for the <see cref="BattlePlayerManagerDataQSingleton"/> used in <see cref="BattlePlayerManager.Data"/> methods
            /// that need access to the <see cref="BattlePlayerManagerDataQSingleton"/>.
            /// </summary>
            public struct Ref
            {
                /// <summary>
                /// Pointer to the underlying <see cref="BattlePlayerManagerDataQSingleton"/>.
                /// </summary>
                public BattlePlayerManagerDataQSingleton* Ptr;
            }

            /// <summary>
            /// Creates a <see cref="Data.Ref">Data.Ref</see> struct.
            /// </summary>
            ///
            /// Fetches the @cref{Quantum,BattlePlayerManagerDataQSingleton} and wraps it into the
            /// @cref{Battle.QSimulation.Player.BattlePlayerManager,Data.Ref} struct.
            ///
            /// <param name="f">Current simulation frame.</param>
            ///
            /// <returns>The created <see cref="Ref">Data.Ref</see> struct.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Ref GetRef(Frame f)
            {
                if (!f.Unsafe.TryGetPointerSingleton(out BattlePlayerManagerDataQSingleton* playerManagerData))
                {
                    s_debugLogger.Error(f, "PlayerManagerData singleton not found!");
                }

                return new Ref() { Ptr = playerManagerData };
            }

            //! @anchor BattlePlayerManager-Data-PlayerCountMethods
            //! @name Player Count Methods
            //! Methods for retrieving and setting the %Player Count
            //! @{
            #region PlayerCount Methods

            /// <summary>
            /// Retrieves the current <b>%Player Count</b>.
            /// </summary>
            ///
            /// <param name="ref"><see cref="Data.Ref"/> struct.</param>
            ///
            /// <returns>The retrieved <b>%Player Count</b>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetPlayerCount(Ref @ref) => @ref.Ptr->PlayerCount;

            /// <summary>
            /// Sets <b>%Player Count</b> to given <paramref name="playerCount"/>.
            /// </summary>
            ///
            /// <param name="ref"><see cref="Data.Ref"/> struct.</param>
            /// <param name="playerCount">The new value for <b>%Player Count</b></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetPlayerCount(Ref @ref, int playerCount) => @ref.Ptr->PlayerCount = playerCount;

            #endregion PlayerCount Methods
            //! @}

            //! @anchor BattlePlayerManager-Data-PlayerIndexMethods
            //! @name Player Index Methods
            //! Methods for retrieving the @ref BattlePlayerManager-PlayerIndex "{Player Index}"
            //! @{
            #region PlayerIndex Methods

            /// <summary>
            /// Retrieves the <b>{%Player Index}</b> corresponding to the given <paramref name="slot"/>.
            /// </summary>
            ///
            /// See [{Player Slot}](#page-concepts-player-slots-teams) for more info.<br/>
            /// See @ref BattlePlayerManager-PlayerIndex "{Player Index}" for more info.
            ///
            /// <param name="slot"><b>{%Player Slot}</b> of the <b>%Player</b> whose <b>{%Player Index}</b> is to be retrieved.</param>
            ///
            /// <returns>The retrieved <b>{%Player Index}</b>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetPlayerIndex(BattlePlayerSlot slot)
            {
                int index = slot switch
                {
                    BattlePlayerSlot.Slot1 => 0,
                    BattlePlayerSlot.Slot2 => 1,
                    BattlePlayerSlot.Slot3 => 2,
                    BattlePlayerSlot.Slot4 => 3,

                    _ => -1
                };

                return index;
            }

            /// <summary>
            /// Retrieves the <b>{%Player Index}</b> corresponding to the given <paramref name="playerRef"/>.
            /// </summary>
            ///
            /// See @ref BattlePlayerManager-PlayerIndex "{Player Index}" for more info.
            ///
            /// <param name="ref"><see cref="Data.Ref"/> struct.</param>
            /// <param name="playerRef"><b>PlayerRef</b> of the <b>%Player</b> whose <b>{%Player Index}</b> is to be retrieved.</param>
            ///
            /// <returns>The retrieved <b>{%Player Index}</b>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetPlayerIndex(Ref @ref, PlayerRef playerRef)
            {
                for (int i = 0; i < Constants.BATTLE_PLAYER_SLOT_COUNT; i++)
                {
                    if (@ref.Ptr->PlayerArray[i].PRef == playerRef) return i;
                }
                return -1;
            }

            #endregion PlayerIndex Methods
            //! @}

            //! @anchor BattlePlayerManager-Data-PlayerSlotMethods
            //! @name Player Slot and Team Methods
            //! Methods for retrieving [{Player Slots and Teams}](#page-concepts-player-slots-teams).
            //! @{
            #region Slot and Team Methods

            /// <summary>
            /// Retrieves the <b>{%Player Slot}</b> corresponding to the given <paramref name="playerRef"/>.
            /// </summary>
            ///
            /// See [{Player Slot}](#page-concepts-player-slots-teams) for more info.
            ///
            /// <param name="ref"><see cref="Data.Ref"/> struct.</param>
            /// <param name="playerRef"><b>PlayerRef</b> of the <b>%Player</b> whose <b>{%Player Slot}</b> is to be retrieved.</param>
            ///
            /// <returns>The retrieved <b>{%Player Slot}</b>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattlePlayerSlot GetSlot(Ref @ref, PlayerRef playerRef)
            {
                return GetSlot(GetPlayerIndex(@ref, playerRef));
            }

            /// <summary>
            /// Retrieves the <b>{%Player Slot}</b> corresponding to the given <paramref name="index"/>.
            /// </summary>
            ///
            /// See [{Player Slot}](#page-concepts-player-slots-teams) for more info.<br/>
            /// See @ref BattlePlayerManager-PlayerIndex "{Player Index}" for more info.
            ///
            /// <param name="index"><b>{%Player Index}</b> of the <b>%Player</b> whose <b>{%Player Slot}</b> is to be retrieved.</param>
            ///
            /// <returns>The retrieved <b>{%Player Slot}</b>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattlePlayerSlot GetSlot(int index)
            {
                return index switch
                {
                    0 => BattlePlayerSlot.Slot1,
                    1 => BattlePlayerSlot.Slot2,
                    2 => BattlePlayerSlot.Slot3,
                    3 => BattlePlayerSlot.Slot4,

                    _ => BattlePlayerSlot.Spectator
                };
            }

            /// <summary>
            /// Retrieves the <b>{%Player Teammate Slot}</b> of the given <paramref name="slot"/>.
            /// </summary>
            ///
            /// See [{Player Slot}](#page-concepts-player-slots-teams) for more info.<br/>
            /// See [{Player Teams}](#page-concepts-player-slots-teams) for more info.
            ///
            /// <param name="slot"><b>{PlayerSlot}</b> of the <b>%Player</b> whose <b>{%Player Teammate Slot}</b> is to be retrieved.</param>
            ///
            /// <returns>The retrieved <b>{%Player Slot}</b>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattlePlayerSlot GetTeammateSlot(BattlePlayerSlot slot)
            {
                return slot switch
                {
                    BattlePlayerSlot.Slot1 => BattlePlayerSlot.Slot2,
                    BattlePlayerSlot.Slot2 => BattlePlayerSlot.Slot1,
                    BattlePlayerSlot.Slot3 => BattlePlayerSlot.Slot4,
                    BattlePlayerSlot.Slot4 => BattlePlayerSlot.Slot3
                };
            }

            /// <summary>
            /// Retrieves the <b>{TeamNumber}</b> of the given <paramref name="slot"/>.
            /// </summary>
            ///
            /// See [{Player Teams}](#page-concepts-player-slots-teams) for more info.<br/>
            /// See [{Player Slot}](#page-concepts-player-slots-teams) for more info.
            ///
            /// <param name="slot"><b>{PlayerSlot}</b> of the <b>%Player</b> whose <b>{TeamNumber}</b> is to be retrieved.</param>
            ///
            /// <returns>The retrieved <b>{TeamNumber}</b>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattleTeamNumber GetTeam(BattlePlayerSlot slot)
            {
                return slot switch
                {
                    BattlePlayerSlot.Slot1 => BattleTeamNumber.TeamAlpha,
                    BattlePlayerSlot.Slot2 => BattleTeamNumber.TeamAlpha,
                    BattlePlayerSlot.Slot3 => BattleTeamNumber.TeamBeta,
                    BattlePlayerSlot.Slot4 => BattleTeamNumber.TeamBeta,

                    _ => BattleTeamNumber.NoTeam
                };
            }

            /// <summary>
            /// Retrieves the <b>{Opposing Team}</b> of the given <paramref name="team"/>.
            /// </summary>
            ///
            /// See [{Player Teams}](#page-concepts-player-slots-teams) for more info.<br/>
            /// See [{Opposing Team}](#page-concepts-player-slots-teams) for more info.
            ///
            /// <param name="team"><b>{TeamNumber}</b> of the <b>{Team}</b> whose <b>{Opposing Team}</b> is to be retrieved.</param>
            ///
            /// <returns>The <b>{TeamNumber}</b> of the <b>{Opposing Team}</b>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattleTeamNumber GetOpposingTeam(BattleTeamNumber team)
            {
                return team switch
                {
                    BattleTeamNumber.TeamAlpha => BattleTeamNumber.TeamBeta,
                    BattleTeamNumber.TeamBeta  => BattleTeamNumber.TeamAlpha,
                };
            }

            #endregion Slot and Team Methods
            //! @}

            //! @anchor BattlePlayerManager-Data-PlayerDataMethods
            //! @name PlayerData Methods
            //! Methods for retrieving [{Player Data}](#page-concepts-player-data).
            //! @{
            #region PlayerData Methods

            /// <summary>
            /// Retrieves <b>{%Player %Data}</b> based on <paramref name="index"/>.
            /// </summary>
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.<br/>
            /// See @ref BattlePlayerManager-PlayerIndex "{Player Index}" for more info.
            ///
            /// <param name="ref"><see cref="Data.Ref"/> struct.</param>
            /// <param name="index"><b>{%Player Index}</b> of the <b>%Player</b> whose <b>{%Player %Data}</b> is to be retrieved.</param>
            ///
            /// <returns>The retrieved <b>{%Player %Data}</b>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattlePlayerData.Ref GetPlayerData(Ref @ref, int index)
            {
                return new BattlePlayerData.Ref(@ref.Ptr, index);
            }

            #endregion PlayerData Methods
            //! @}
        }
    }
}

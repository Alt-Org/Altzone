/// @file BattlePlayerData.cs
/// <summary>
/// TO-DO ADD DOCUMENTATION
/// </summary>

// System usings
using System.Diagnostics;
using System.Runtime.CompilerServices;

// QSimulation usings
using Battle.QSimulation;
using Battle.QSimulation.Game;
using Battle.QSimulation.Player;

namespace Quantum
{
    // Adds constants and static methods
    // Defines an inner Ref type
    // Main struct definition in BattlePlayerData.qtn
    // Main struct documentation in qtn-BattlePlayerData.dox
    public unsafe partial struct BattlePlayerData
    {
        /// <summary>
        /// A reference and wrapper to the <see cref="BattlePlayerData">BattlePlayerData</see> struct that exposes <b>Gameplay Level</b> data and grants access to the <b>Low Level</b> data.
        /// </summary>
        /// See [{Player Data}](#page-concepts-player-data) for more info.
        public struct Ref
        {
            /// @anchor BattlePlayerDataRef-Attributes
            /// @name Immutable Attributes
            /// Immutable data accessed but not meant to be modified by gameplay logic.<br/>
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.
            ///
            /// @{
            #region Attributes

            /// <summary>PlayerRef used to identify the player in %Quantum.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-Attributes "Immutable Attributes"
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.
            public PlayerRef PRef
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.PRef; }

            /// <summary>Which PlayerSlot the player is in.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-Attributes "Immutable Attributes"
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.<br/>
            /// See [{Player Slots}](#page-concepts-player-slots-teams) for more info.
            public BattlePlayerSlot Slot
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.Slot; }

            /// <summary>Which Team the player is on.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-Attributes "Immutable Attributes"
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.<br/>
            /// See [{Player Teams}](#page-concepts-player-slots-teams) for more info.
            public BattleTeamNumber Team
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.Team; }

            /// <summary>Whether the player is a bot or not.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-Attributes "Immutable Attributes"
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.
            public bool IsBot
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.IsBot; }

            /// <summary>Whether the player has abandoned the game or not.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-Attributes "Immutable Attributes"
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.
            public bool IsAbandoned
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.IsAbandoned; }

            /// <summary>The player's current play state.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-Attributes "Immutable Attributes"
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.<br/>
            /// See [{Player PlayState}](#page-concepts-player-playstate) for more info.
            public BattlePlayerPlayState PlayState
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.PlayState; }

            /// <summary>Player's currently selected character's character number.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-Attributes "Immutable Attributes"
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.<br/>
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number) for more info.
            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Low_Level.SelectedCharacterNumber; }

            #endregion Attributes
            /// @}

            /// @anchor BattlePlayerDataRef-State
            /// @name Mutable State
            /// Mutable data accessed and modified as needed by gameplay logic.<br/>
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.
            ///
            /// @{
            #region State

            /// <summary>Whether character swapping is allowed or not.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-State "Mutable State"
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.
            public ref QBoolean StateAllowCharacterSwapping
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref Low_Level.StateAllowCharacterSwapping; }

            /// <summary>Whether the player has given up or not.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-State "Mutable State"
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.
            public ref QBoolean StatePlayerGiveUp
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref Low_Level.StatePlayerGiveUp; }

            /// <summary>Player's respawn timer.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-State "Mutable State"
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.
            public ref FrameTimer StateRespawnTimer
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref Low_Level.StateRespawnTimer; }

            #endregion State
            /// @}

            /// @anchor BattlePlayerDataRef-LowLevel
            /// @name Low Level
            /// Low level access.<br/>
            /// @note Only meant for use by "low level" code.<br/>
            ///
            /// See [{Player Data}](#page-concepts-player-data) for more info.
            ///
            /// @{
            #region Low Level

            /// <summary>Low level access. @note Only meant for use by "low level" code.</summary>
            ///
            /// Part of @ref BattlePlayerDataRef-LowLevel "Low Level"
            ///
            /// <br/>See [{Player Data}](#page-concepts-player-data) for more info.
            public ref BattlePlayerData Low_Level
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _playerManagerData->PlayerArray[_index]; }

            #endregion Low Level
            /// @}

            public Ref(BattlePlayerManagerDataQSingleton* playerManagerData, int index)
            {
                _playerManagerData = playerManagerData;
                _index = index;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetOutOfPlayRespawning()
            {
                if (!PlayState.IsOutOfPlay())
                {
                    s_debugLogger.Error("Can not set player that is not OutOfPlay as OutOfPlayRespawning");
                    return;
                }
                Low_Level.PlayState = BattlePlayerPlayState.OutOfPlayRespawning;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetOutOfPlayFinal()
            {
                if (!PlayState.IsOutOfPlay())
                {
                    s_debugLogger.Error("Can not set player that is not OutOfPlay as OutOfPlayFinal");
                    return;
                }
                Low_Level.PlayState = BattlePlayerPlayState.OutOfPlayFinal;
            }

            public int GetIndex() => _index;

            private BattlePlayerManagerDataQSingleton* _playerManagerData;

            private int _index;
        }

        /// @name test
        /// @{

        public const int NoSelectedCharacter = -1;
        public const int NoAttachedShield = -1;

        /// @}

        #region Public Static Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidCharacterNumber(int characterNumber)
        {
            return characterNumber >= 0 && characterNumber < BattlePlayerManager.PlayerCharacterCount;
        }

        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertIsValidCharacterNumber(string source, int characterNumber)
        {
            BattleDebugLogger.DevAssertFormat(source, IsValidCharacterNumber(characterNumber), "Invalid characterNumber = {0}", characterNumber);
        }

        public static bool IsValidShieldNumber(BattlePlayerHandle playerHandle, int shieldNumber)
        {
            return shieldNumber >= 0 && shieldNumber < playerHandle.LoadedCharacterData->ShieldCount;
        }

        [Conditional("UNITY_EDITOR")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertIsValidShieldNumber(string source, BattlePlayerHandle playerHandle, int shieldNumber)
        {
            BattleDebugLogger.DevAssertFormat(
                source,
                IsValidShieldNumber(playerHandle, shieldNumber),
                "Invalid shieldNumber {0} for character number {1}",
                shieldNumber,
                playerHandle.LoadedCharacterData->Number
            );
        }

        #endregion Public Static Methods

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;
    }
}

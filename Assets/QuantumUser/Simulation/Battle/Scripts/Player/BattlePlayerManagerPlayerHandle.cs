/// @file BattlePlayerManagerPlayerHandle.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerManager} partial class which contains PlayerHandle and PlayerHandleInternal structs.<br/>
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Contains the public and private PlayerHandle structs.
    /// </summary>
    public static unsafe partial class BattlePlayerManager
    {
        /// <summary>
        /// Public helper struct for getting player information.
        /// </summary>
        ///
        /// [{Player Overview}](#page-concepts-player-overview)<br/>
        /// [{Player Simulation Code Overview}](#page-concepts-player-simulation-overview)
        ///
        /// This is a public wrapper for the private PlayerHandleInternal that is used by the BattlePlayerManager internally.<br/>
        /// This only exposes the parts of the PlayerHandleInternal that is meant to be accessible outside of BattlePlayerManager.
        public struct PlayerHandle
        {
            #region Public Static Methods

            public static BattlePlayerSlot GetSlot(Frame f, PlayerRef playerRef)
            {
                BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);
                int playerIndex = PlayerHandleInternal.GetPlayerIndex(playerManagerData, playerRef);
                return PlayerHandleInternal.GetSlot(playerIndex);
            }

            /// <summary>
            /// Retrieves team number based on slot.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattleTeamNumber GetTeamNumber(BattlePlayerSlot slot) => PlayerHandleInternal.GetTeamNumber(slot);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsValidCharacterNumber(int characterNumber) => PlayerHandleInternal.IsValidCharacterNumber(characterNumber);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandle GetPlayerHandle(Frame f, BattlePlayerSlot slot)
            {
                BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);
                return new PlayerHandle(PlayerHandleInternal.GetPlayerHandle(playerManagerData, slot));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandle GetTeammateHandle(Frame f, BattlePlayerSlot slot)
            {
                BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);
                return new PlayerHandle(PlayerHandleInternal.GetTeammateHandle(playerManagerData, slot));
            }

            public static PlayerHandle[] GetPlayerHandleArray(Frame f)
            {
                BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);
                PlayerHandle[] array = new PlayerHandle[Constants.BATTLE_PLAYER_SLOT_COUNT];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new PlayerHandle(new PlayerHandleInternal(playerManagerData, i));
                }
                return array;
            }

            #region Public static methods - Low level

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandle Low_GetPlayerHandleFromInternal(BattlePlayerManagerDataQSingleton* playerManagerData, int playerIndex)
            {
                return new PlayerHandle(new PlayerHandleInternal(playerManagerData, playerIndex));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Low_GetPlayerIndex(BattlePlayerSlot slot) => PlayerHandleInternal.GetPlayerIndex(slot);

            #endregion Public static methods - Low level

            #endregion Public Static Methods

            #region Public Properties

            public BattlePlayerPlayState PlayState
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.PlayState; }

            public BattlePlayerSlot Slot
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => PlayerHandleInternal.GetSlot(_internalHandle.Index); }

            public PlayerRef PlayerRef
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.PlayerRef; }

            public bool IsBot
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.IsBot; }

            /// <summary>
            /// Public getter for IsAbandoned.
            /// </summary>
            public bool IsAbandoned
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.IsAbandoned; }

            public FrameTimer RespawnTimer
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _internalHandle.RespawnTimer;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _internalHandle.RespawnTimer = value;
            }

            public bool AllowCharacterSwapping
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _internalHandle.AllowCharacterSwapping;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _internalHandle.AllowCharacterSwapping = value;
            }

            public BattleEntityID SelectedCharacterEntityID
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacterEntityID; }

            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacterNumber; }

            public BattlePlayerCharacterState SelectedCharacterState
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacterState; }

            public bool PlayerGiveUpState
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _internalHandle.PlayerGiveUpState;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _internalHandle.PlayerGiveUpState = value;
            }

            #endregion Public Properties

            #region Public Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BattlePlayerCharacterState GetCharacterState(int characterNumber) => _internalHandle.GetCharacterState(characterNumber);

            public void SetOutOfPlayRespawning()
            {
                if (!_internalHandle.PlayState.IsOutOfPlay())
                {
                    s_debugLogger.Error("Can not set player that is not OutOfPlay as OutOfPlayRespawning");
                    return;
                }
                _internalHandle.PlayState = BattlePlayerPlayState.OutOfPlayRespawning;
            }

            public void SetOutOfPlayFinal()
            {
                if (!_internalHandle.PlayState.IsOutOfPlay())
                {
                    s_debugLogger.Error("Can not set player that is not OutOfPlay as OutOfPlayFinal");
                    return;
                }
                _internalHandle.PlayState = BattlePlayerPlayState.OutOfPlayFinal;
            }

            #endregion Public Methods

            #region Private

            private PlayerHandleInternal _internalHandle;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private PlayerHandle(PlayerHandleInternal internalHandle)
            {
                _internalHandle = internalHandle;
            }

            #endregion Private
        }

        /// <summary>
        /// Internal helper struct for player operations and state management in BattlePlayerManager.<br/>
        /// Has static helper methods for player operations.<br/>
        /// Can be instantiated to handle specific player's data stored in BattlePlayerManagerDataQSingleton.
        /// </summary>
        private struct PlayerHandleInternal
        {
            #region Public Static Methods

            // Exposed in public PlayerHandle
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattlePlayerSlot GetSlot(int playerIndex)
            {
                return playerIndex switch
                {
                    0 => BattlePlayerSlot.Slot1,
                    1 => BattlePlayerSlot.Slot2,
                    2 => BattlePlayerSlot.Slot3,
                    3 => BattlePlayerSlot.Slot4,

                    _ => BattlePlayerSlot.Spectator
                };
            }

            /// <summary>
            /// Retrieves team number based on slot.
            /// </summary>
            ///
            /// Exposed in public PlayerHandle
            ///
            /// <param name="slot">The slot of the player.</param>
            /// <returns>The BattleTeamNumber of the given player.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattleTeamNumber GetTeamNumber(BattlePlayerSlot slot)
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
            /// Retrieves player index based on slot.
            /// </summary>
            ///
            /// Exposed in public PlayerHandle as low level
            ///
            /// <param name="slot">The slot of the player.</param>
            /// <returns>The index of the given player.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetPlayerIndex(BattlePlayerSlot slot)
            {
                return slot switch
                {
                    BattlePlayerSlot.Slot1 => 0,
                    BattlePlayerSlot.Slot2 => 1,
                    BattlePlayerSlot.Slot3 => 2,
                    BattlePlayerSlot.Slot4 => 3,

                    _ => -1
                };
            }

            /// <summary>
            /// Retrieves player index from a PlayerRef.
            /// </summary>
            ///
            /// <param name="playerManagerData">Pointer reference to the player manager data.</param>
            /// <param name="playerRef">PlayerRef of the player.</param>
            /// <returns>The index of the given player.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetPlayerIndex(BattlePlayerManagerDataQSingleton* playerManagerData, PlayerRef playerRef)
            {
                for (int i = 0; i < Constants.BATTLE_PLAYER_SLOT_COUNT; i++)
                {
                    if (playerManagerData->PlayerRefs[i] == playerRef) return i;
                }
                return -1;
            }

            /// <summary>
            /// Retrieves the index of the teammate of a player based on slot.
            /// </summary>
            ///
            /// <param name="slot">The slot of the player.</param>
            /// <returns>The index of the given player's teammate.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetTeammatePlayerIndex(BattlePlayerSlot slot)
            {
                return slot switch
                {
                    BattlePlayerSlot.Slot1 => 1,
                    BattlePlayerSlot.Slot2 => 0,
                    BattlePlayerSlot.Slot3 => 3,
                    BattlePlayerSlot.Slot4 => 2,

                    _ => -1
                };
            }

            /// <summary>
            /// Retrieves PlayerHandle based on slot.
            /// </summary>
            ///
            /// <param name="playerManagerData">Pointer reference to the player manager data.</param>
            /// <param name="slot">The slot of the player.</param>
            /// <returns>A PlayerHandle for the given player.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandleInternal GetPlayerHandle(BattlePlayerManagerDataQSingleton* playerManagerData, BattlePlayerSlot slot)
            {
                int playerIndex = GetPlayerIndex(slot);
                return new PlayerHandleInternal(playerManagerData, playerIndex);
            }

            /// <summary>
            /// Retrieves PlayerHandle of the teammate of a player based on slot.
            /// </summary>
            ///
            /// <param name="playerManagerData">Pointer reference to the player manager data.</param>
            /// <param name="slot">The slot of the player.</param>
            /// <returns>A PlayerHandle for the given player's teammate.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandleInternal GetTeammateHandle(BattlePlayerManagerDataQSingleton* playerManagerData, BattlePlayerSlot slot)
            {
                int playerIndex = GetTeammatePlayerIndex(slot);
                return new PlayerHandleInternal(playerManagerData, playerIndex);
            }

            /// <summary>
            /// Sets all players' play states to a given state.
            /// </summary>
            ///
            /// <param name="playerManagerData">Pointer reference to the player manager data.</param>
            /// <param name="playerPlayState">The state that all players will be set to.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetAllPlayStates(BattlePlayerManagerDataQSingleton* playerManagerData, BattlePlayerPlayState playerPlayState)
            {
                for (int i = 0; i < Constants.BATTLE_PLAYER_SLOT_COUNT; i++)
                {
                    playerManagerData->PlayStates[i] = playerPlayState;
                }
            }

            /// <summary>
            /// Checks if a given character number is valid.
            /// </summary>
            ///
            /// Exposed in public PlayerHandle
            ///
            /// <param name="characterNumber">The character number to verify.</param>
            /// <returns>True if the given character number is valid, false if it is not.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsValidCharacterNumber(int characterNumber)
            {
                if (!(characterNumber >= 0 && characterNumber < Constants.BATTLE_PLAYER_CHARACTER_COUNT))
                {
                    s_debugLogger.ErrorFormat("Character number {1} is not valid", characterNumber);
                    return false;
                }
                return true;
            }

            #endregion Public Static Methods

            #region Public Properties

            public int Index { get; set; }

            /// <summary>
            /// Gets/Sets player's PlayState.
            /// </summary>
            ///
            /// Getter exposed in public PlayerHandle
            public BattlePlayerPlayState PlayState
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->PlayStates[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->PlayStates[Index] = value;
            }

            /// <summary>
            /// Gets/Sets player's PlayerRef.
            /// </summary>
            ///
            /// Getter exposed in public PlayerHandle
            public PlayerRef PlayerRef
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->PlayerRefs[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->PlayerRefs[Index] = value;
            }

            // Getter exposed in public PlayerHandle
            public bool IsBot
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->IsBot[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->IsBot[Index] = value;
            }

            /// <summary>
            /// Gets and sets player's IsAbandoned state.
            /// </summary>
            ///
            /// Getter exposed in public PlayerHandle
            public bool IsAbandoned
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->IsAbandoned[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->IsAbandoned[Index] = value;
            }

            // Getter exposed in public PlayerHandle
            // Setter exposed in public PlayerHandle
            public FrameTimer RespawnTimer
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->RespawnTimer[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->RespawnTimer[Index] = value;
            }

            // Getter exposed in public PlayerHandle
            // Setter exposed in public PlayerHandle
            public bool AllowCharacterSwapping
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->AllowCharacterSwapping[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->AllowCharacterSwapping[Index] = value;
            }

            /// <summary>
            /// Gets player's SelectedCharacterEntity.<br/>
            /// The SelectedCharacterEntity is a EntityRef to the character that is currently in play.
            /// </summary>
            ///
            /// Exposed in public PlayerHandle
            public BattleEntityID SelectedCharacterEntityID
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacterEntityIDs[Index]; }

            /// <summary>
            /// Gets player's SelectedCharacterNumber.<br/>
            /// The SelectedCharacterNumber is the number of the character that is currently in play.
            /// </summary>
            ///
            /// Exposed in public PlayerHandle
            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacterNumbers[Index]; }

            // Getter exposed in public PlayerHandle
            public BattlePlayerCharacterState SelectedCharacterState
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => GetCharacterState(SelectedCharacterNumber);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetCharacterState(SelectedCharacterNumber, value);
            }

            // Getter exposed in public PlayerHandle
            // Setter exposed in public PlayerHandle
            public bool PlayerGiveUpState
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->PlayerGiveUpStates[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->PlayerGiveUpStates[Index] = value;
            }

            public FPVector2 SpawnPosition
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => s_spawnPoints[Index]; }

            #endregion Public Properties

            /// <summary>
            /// Constructor for PlayerHandleInternal.
            /// </summary>
            ///
            /// <param name="playerManagerData">Pointer to BattlePlayerManagerDataQSingleton.</param>
            /// <param name="playerIndex">Index of the player that you want the handle to.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PlayerHandleInternal(BattlePlayerManagerDataQSingleton* playerManagerData, int playerIndex)
            {
                Index = playerIndex;
                _playerManagerData = playerManagerData;
            }

            #region Public Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PlayerHandle ConvertToPublic()
            {
                return PlayerHandle.Low_GetPlayerHandleFromInternal(_playerManagerData, Index);
            }

            /// <summary>
            /// Gets an EntityRef to a player's Character by characterNumber.
            /// </summary>
            ///
            /// <param name="characterNumber">CharacterNumber of the player's character you want to get.</param>
            /// <returns>EntityRef to a player's Character.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BattleEntityID GetCharacterEntityID(int characterNumber) => _playerManagerData->AllCharacterIDs[GetCharacterIndex(characterNumber)];

            /// <summary>
            /// Saves player's created character EntityRefs to BattlePlayerManagerDataQSingleton.
            /// </summary>
            ///
            /// <param name="entityIDArray">The Character entity IDs as an array.</param>
            public void SetCharacterEntityIDs(BattleEntityID[] entityIDArray)
            {
                int characterOffset = GetCharacterOffset();
                for (int i = 0; i < Constants.BATTLE_PLAYER_CHARACTER_COUNT; i++)
                {
                    _playerManagerData->AllCharacterIDs[characterOffset + i] = entityIDArray[i];
                }
            }

            /// <summary>
            /// Sets player's SelectedCharacter and updates SelectedCharacterNumber based on <paramref name="characterNumber"/>.<br/>
            /// The SelectedCharacter is a EntityRef to the character that is currently in play.
            /// </summary>
            ///
            /// Exposed in public PlayerHandle
            ///
            /// <param name="characterNumber">CharacterNumber of the player's character you want to set.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BattlePlayerCharacterState GetCharacterState(int characterNumber) => _playerManagerData->AllCharactersStates[GetCharacterIndex(characterNumber)];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetCharacterState(int characterNumber, BattlePlayerCharacterState state) => _playerManagerData->AllCharactersStates[GetCharacterIndex(characterNumber)] = state;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSelectedCharacterID(int characterNumber)
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = characterNumber;
                _playerManagerData->SelectedCharacterEntityIDs[Index] = _playerManagerData->AllCharacterIDs[GetCharacterIndex(characterNumber)];
            }

            /// <summary>
            /// Unsets player's SelectedCharacterID and updates SelectedCharacterNumber.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsetSelectedCharacterID()
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = -1;
                _playerManagerData->SelectedCharacterEntityIDs[Index] = (BattleEntityID)(-1);
            }

            /// <summary>
            /// Generates a position for the player's character that is out of play.<br/>
            /// Each character of each player has a unique position that is used when the character is out of play.
            /// </summary>
            ///
            /// <param name="characterNumber">CharacterNumber of the player's character that is moved out of play.</param>
            /// <param name="teamNumber">TeamNumber of the player whose character that is moved out of play.</param>
            /// <returns>The generated position.</returns>
            public FPVector2 GetOutOfPlayPosition(int characterNumber, BattleTeamNumber teamNumber)
            {
                int row = 0, column = 0;

                switch (teamNumber)
                {
                    case BattleTeamNumber.TeamAlpha:
                        row = 0 - 10 * (characterNumber + 1);
                        column = -5 - 10 * Index;
                        break;
                    case BattleTeamNumber.TeamBeta:
                        row = BattleGridManager.Rows - 1 + 10 * (characterNumber + 1);
                        column = BattleGridManager.Columns + 4 + 10 * (Index - 2);
                        break;

                }

                return new FPVector2
                (
                    BattleGridManager.GridColToWorldXPosition(column),
                    BattleGridManager.GridRowToWorldYPosition(row)
                );
            }

            #endregion Public Methods

            #region Private Fields

            /// <summary>Pointer to the BattlePlayerManagerDataQSingleton.</summary>
            private BattlePlayerManagerDataQSingleton* _playerManagerData;

            #endregion Private Fields

            #region Private Methods

            /// <summary>Calculates the index where player's characters start in the BattlePlayerManagerDataQSingleton.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private int GetCharacterOffset() => Index * Constants.BATTLE_PLAYER_CHARACTER_COUNT;

            /// <summary>Calculates the index of player's character in the BattlePlayerManagerDataQSingleton.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private int GetCharacterIndex(int characterNumber) => GetCharacterOffset() + characterNumber;

            #endregion Private Methods
        }
    }
}

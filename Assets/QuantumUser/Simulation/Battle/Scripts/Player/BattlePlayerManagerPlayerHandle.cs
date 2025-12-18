/// @file BattlePlayerManagerPlayerHandle.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerManager} partial class which contains PlayerHandle and PlayerHandleInternal structs.<br/>
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    // Documentation in BattlePlayerManager.cs
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

            /// <summary>
            /// Retrieves slot based on <paramref name="playerRef"/>.
            /// </summary>
            ///
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            /// 
            /// <param name="f">Current simulation frame.</param>
            /// <param name="playerRef">PlayerRef of the player for which slot will be retrieved.</param>
            /// 
            /// <returns>The slot of the given player.</returns>
            public static BattlePlayerSlot GetSlot(Frame f, PlayerRef playerRef)
            {
                BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);
                int playerIndex = PlayerHandleInternal.GetPlayerIndex(playerManagerData, playerRef);
                return PlayerHandleInternal.GetSlot(playerIndex);
            }

            /// <summary>
            /// Retrieves team number based on <paramref name="slot"/>.
            /// </summary>
            ///
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            ///
            /// <param name="slot">The slot of the player.</param>
            /// 
            /// <returns>The BattleTeamNumber of the given player.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BattleTeamNumber GetTeamNumber(BattlePlayerSlot slot) => PlayerHandleInternal.GetTeamNumber(slot);

            /// <summary>
            /// Checks if a given character number is valid.
            /// </summary>
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
            ///
            /// <param name="characterNumber">The character number to verify.</param>
            /// 
            /// <returns>True if the given character number is valid, false if it is not.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsValidCharacterNumber(int characterNumber) => PlayerHandleInternal.IsValidCharacterNumber(characterNumber);

            /// <summary>
            /// Retrieves PlayerHandle based on <paramref name="slot"/>.
            /// </summary>
            ///
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            ///
            /// <param name="f">Current simulation frame.</param>
            /// <param name="slot">The slot of the player.</param>
            /// 
            /// <returns>A PlayerHandle for the given player.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandle GetPlayerHandle(Frame f, BattlePlayerSlot slot)
            {
                BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);
                return new PlayerHandle(PlayerHandleInternal.GetPlayerHandle(playerManagerData, slot));
            }

            /// <summary>
            /// Retrieves PlayerHandle of the teammate of a player based on <paramref name="slot"/>.
            /// </summary>
            ///
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            ///
            /// <param name="f">Current simulation frame.</param>
            /// <param name="slot">The slot of the player.</param>
            /// 
            /// <returns>A PlayerHandle for the given player's teammate.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandle GetTeammateHandle(Frame f, BattlePlayerSlot slot)
            {
                BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);
                return new PlayerHandle(PlayerHandleInternal.GetTeammateHandle(playerManagerData, slot));
            }

            /// <summary>
            /// Retrieves an array containing one PlayerHandle for each slot, including players, bots and empty slots.
            /// </summary>
            /// 
            /// <param name="f">Current simulation frame.</param>
            /// 
            /// <returns>The array containing the PlayerHandles.</returns>
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

            /// <summary>
            /// Retrieves PlayerHandle based on the <paramref name="playerManagerData"/> reference and given <paramref name="playerIndex"/>.
            /// @note Low level method! Only meant for use by <see cref="PlayerHandleInternal"/>.
            /// </summary>
            /// 
            /// <param name="playerManagerData">Pointer reference to the player manager data.</param>
            /// <param name="playerIndex">The index of the player.</param>
            /// 
            /// <returns>A PlayerHandle for the given player.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandle Low_GetPlayerHandleFromInternal(BattlePlayerManagerDataQSingleton* playerManagerData, int playerIndex)
            {
                return new PlayerHandle(new PlayerHandleInternal(playerManagerData, playerIndex));
            }

            /// <summary>
            /// Retrieves player index based on given <paramref name="slot"/>.
            /// @note Low level method! <b>%Player index</b> is an internal concept to <b>PlayerManager</b>.<br/>
            /// <b>%Player slot</b> is preferred when referencing specific players.
            /// </summary>
            ///
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            /// 
            /// <param name="slot">The slot of the player.</param>
            /// 
            /// <returns>The player index of the given player.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Low_GetPlayerIndex(BattlePlayerSlot slot) => PlayerHandleInternal.GetPlayerIndex(slot);

            #endregion Public static methods - Low level

            #endregion Public Static Methods

            #region Public Properties

            /// <summary>
            /// Public getter for PlayState.
            /// </summary>
            public BattlePlayerPlayState PlayState
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.PlayState; }

            /// <summary>
            /// Public getter for Slot.
            /// </summary>
            ///
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            public BattlePlayerSlot Slot
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => PlayerHandleInternal.GetSlot(_internalHandle.Index); }

            /// <summary>
            /// Public getter for PlayerRef.
            /// </summary>
            public PlayerRef PlayerRef
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.PlayerRef; }

            /// <summary>
            /// Public getter for IsBot.
            /// </summary>
            public bool IsBot
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.IsBot; }

            /// <summary>
            /// Public getter for IsAbandoned.
            /// </summary>
            public bool IsAbandoned
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.IsAbandoned; }

            /// <summary>
            /// Public getter and setter for RespawnTimer.
            /// </summary>
            public FrameTimer RespawnTimer
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _internalHandle.RespawnTimer;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _internalHandle.RespawnTimer = value;
            }

            /// <summary>
            /// Public getter and setter for AllowCharacterSwapping.
            /// </summary>
            public bool AllowCharacterSwapping
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _internalHandle.AllowCharacterSwapping;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _internalHandle.AllowCharacterSwapping = value;
            }

            /// <summary>
            /// Public getter for SelectedCharacterEntityID.
            /// </summary>
            public BattleEntityID SelectedCharacterEntityID
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacterEntityID; }

            /// <summary>
            /// Public getter for SelectedCharacterNumber.
            /// </summary>
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacterNumber; }

            /// <summary>
            /// Public getter for SelectedCharacterState.
            /// </summary>
            public BattlePlayerCharacterState SelectedCharacterState
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacterState; }

            /// <summary>
            /// Public getter and setter for PlayerGiveUpState.
            /// </summary>
            public bool PlayerGiveUpState
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _internalHandle.PlayerGiveUpState;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _internalHandle.PlayerGiveUpState = value;
            }

            #endregion Public Properties

            #region Public Methods

            /// <summary>
            /// Retrieves player's CharacterState based on <paramref name="characterNumber"/>.
            /// </summary>
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
            ///
            /// <param name="characterNumber">CharacterNumber of the player's character you want to set.</param>
            ///
            /// <returns>The CharacterState of the given player.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BattlePlayerCharacterState GetCharacterState(int characterNumber) => _internalHandle.GetCharacterState(characterNumber);

            /// <summary>
            /// Sets PlayState to OutOfPlayRespawning.
            /// </summary>
            ///
            /// See [{Player PlayState}](#page-concepts-player-playstate)
            public void SetOutOfPlayRespawning()
            {
                if (!_internalHandle.PlayState.IsOutOfPlay())
                {
                    s_debugLogger.Error("Can not set player that is not OutOfPlay as OutOfPlayRespawning");
                    return;
                }
                _internalHandle.PlayState = BattlePlayerPlayState.OutOfPlayRespawning;
            }

            /// <summary>
            /// Sets PlayState to OutOfPlayFinal.
            /// </summary>
            ///
            /// See [{Player PlayState}](#page-concepts-player-playstate)
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

            /// <summary>
            /// <see cref="PlayerHandleInternal"/> containing all the data for a player, some of which is exposed to and used by PlayerHandle.
            /// </summary>
            private PlayerHandleInternal _internalHandle;

            /// <summary>
            /// Constructor method for PlayerHandle.
            /// </summary>
            /// 
            /// <param name="internalHandle"><see cref="PlayerHandleInternal"/> based on which PlayerHandle will be created.</param>
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

            /// <summary>
            /// Retrieves slot based on <paramref name="playerIndex"/>.
            /// </summary>
            ///
            /// @clink{Exposed:PlayerHandle.GetSlot} in public @cref{PlayerHandle}
            /// 
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            /// 
            /// <param name="playerIndex"></param>
            /// 
            /// <returns>The slot of the given player.</returns>
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
            /// Retrieves team number based on <paramref name="slot"/>.
            /// </summary>
            ///
            /// @clink{Exposed:PlayerHandle.GetTeamNumber} in public @cref{PlayerHandle}
            /// 
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            ///
            /// <param name="slot">The slot of the player.</param>
            /// 
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
            /// Retrieves player index based on <paramref name="slot"/>.
            /// </summary>
            ///
            /// @clink{Exposed:PlayerHandle.Low_GetPlayerIndex} in public @cref{PlayerHandle} as low level.
            /// 
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
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
            /// Retrieves player index based on <paramref name="playerRef"/>.
            /// </summary>
            ///
            /// <param name="playerManagerData">Pointer reference to the player manager data.</param>
            /// <param name="playerRef">PlayerRef of the player.</param>
            /// 
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
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            ///
            /// <param name="slot">The slot of the player.</param>
            /// 
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
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            ///
            /// <param name="playerManagerData">Pointer reference to the player manager data.</param>
            /// <param name="slot">The slot of the player.</param>
            /// 
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
            /// See [{Player Slots and Teams}](#page-concepts-player-slots-teams)
            ///
            /// <param name="playerManagerData">Pointer reference to the player manager data.</param>
            /// <param name="slot">The slot of the player.</param>
            /// 
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
            /// @clink{Exposed:PlayerHandle.IsValidCharacterNumber} in public @cref{PlayerHandle}
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
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

            /// <summary>
            /// Gets/Sets player's Index.
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Gets/Sets player's PlayState.
            /// </summary>
            ///
            /// Getter @clink{exposed:PlayerHandle.PlayState} in public @cref{PlayerHandle}
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
            /// Getter @clink{exposed:PlayerHandle.PlayerRef} in public @cref{PlayerHandle}
            public PlayerRef PlayerRef
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->PlayerRefs[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->PlayerRefs[Index] = value;
            }

            /// <summary>
            /// Gets/Sets IsBot state.
            /// </summary>
            ///
            /// Getter @clink{exposed:PlayerHandle.IsBot} in public @cref{PlayerHandle}
            public bool IsBot
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->IsBot[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->IsBot[Index] = value;
            }

            /// <summary>
            /// Gets/sets player's IsAbandoned state.
            /// </summary>
            ///
            /// Getter @clink{exposed:PlayerHandle.IsAbandoned} in public @cref{PlayerHandle}
            public bool IsAbandoned
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->IsAbandoned[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->IsAbandoned[Index] = value;
            }

            /// <summary>
            /// Gets/Sets player's RespawnTimer.
            /// </summary>
            ///
            /// Getter @clink{exposed:PlayerHandle.RespawnTimer} in public @cref{PlayerHandle}
            /// setter @clink{exposed:PlayerHandle.RespawnTimer} in public @cref{PlayerHandle}
            public FrameTimer RespawnTimer
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->RespawnTimer[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->RespawnTimer[Index] = value;
            }

            /// <summary>
            /// Gets/Sets player's AllowCharacterSwapping state.
            /// </summary>
            ///
            /// Getter @clink{exposed:PlayerHandle.AllowCharacterSwapping} in public @cref{PlayerHandle}
            /// setter @clink{exposed:PlayerHandle.AllowCharacterSwapping} in public @cref{PlayerHandle}
            public bool AllowCharacterSwapping
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->AllowCharacterSwapping[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->AllowCharacterSwapping[Index] = value;
            }

            /// <summary>
            /// Gets player's SelectedCharacterEntityID.<br/>
            /// The SelectedCharacterID is the entity ID of the character that is currently in play.
            /// </summary>
            ///
            /// @clink{Exposed:PlayerHandle.SelectedCharacterEntityID} in public @cref{PlayerHandle}
            public BattleEntityID SelectedCharacterEntityID
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacterEntityIDs[Index]; }

            /// <summary>
            /// Gets player's SelectedCharacterNumber.
            /// </summary>
            ///
            /// @clink{Exposed:PlayerHandle.SelectedCharacterNumber} in public @cref{PlayerHandle}
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacterNumbers[Index]; }

            /// <summary>
            /// Gets/Sets player's SelectedCharacterState.
            /// </summary>
            ///
            /// Getter @clink{exposed:PlayerHandle.SelectedCharacterState} in public @cref{PlayerHandle}
            public BattlePlayerCharacterState SelectedCharacterState
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => GetCharacterState(SelectedCharacterNumber);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => SetCharacterState(SelectedCharacterNumber, value);
            }

            /// <summary>
            /// Gets/Sets player's PlayerGiveUpState.
            /// </summary>
            ///
            /// Getter @clink{exposed:PlayerHandle.PlayerGiveUpState} in public @cref{PlayerHandle}
            /// Setter @clink{exposed:PlayerHandle.PlayerGiveUpState} in public @cref{PlayerHandle}
            public bool PlayerGiveUpState
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->PlayerGiveUpStates[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->PlayerGiveUpStates[Index] = value;
            }

            /// <summary>
            /// Gets player's SpawnPosition.
            /// </summary>
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

            /// <summary>
            /// Converts PlayerHandleInternal to PlayerHandle
            /// </summary>
            /// 
            /// <returns>This PlayerHandleInternal converted to a PlayerHandle.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PlayerHandle ConvertToPublic()
            {
                return PlayerHandle.Low_GetPlayerHandleFromInternal(_playerManagerData, Index);
            }

            /// <summary>
            /// Gets player's Character entity ID based on <paramref name="characterNumber"/>.
            /// </summary>
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
            ///
            /// <param name="characterNumber">CharacterNumber of the player's character you want to get.</param>
            /// 
            /// <returns>Entity ID of given Character.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BattleEntityID GetCharacterEntityID(int characterNumber) => _playerManagerData->AllCharacterIDs[GetCharacterIndex(characterNumber)];

            /// <summary>
            /// Saves player's Character entity IDs to BattlePlayerManagerDataQSingleton.
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
            /// Retrieves a player's Character's CharacterState based on <paramref name="characterNumber"/>.
            /// </summary>
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
            ///
            /// <param name="characterNumber">CharacterNumber of the player's character you want to set.</param>
            ///
            /// <returns>The CharacterState of the given player.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BattlePlayerCharacterState GetCharacterState(int characterNumber) => _playerManagerData->AllCharactersStates[GetCharacterIndex(characterNumber)];

            /// <summary>
            /// Sets a player's Character's Character state to given <paramref name="state"/> based on <paramref name="characterNumber"/>.
            /// </summary>
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
            /// 
            /// <param name="characterNumber">CharacterNumber of the player's character you want to set.</param>
            /// <param name="state"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetCharacterState(int characterNumber, BattlePlayerCharacterState state) => _playerManagerData->AllCharactersStates[GetCharacterIndex(characterNumber)] = state;

            /// <summary>
            /// Sets player's SelectedCharacterID and updates SelectedCharacterNumber based on <paramref name="characterNumber"/>.<br/>
            /// The SelectedCharacterID is the entity ID of the character that is currently in play.
            /// </summary>
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
            ///
            /// <param name="characterNumber">CharacterNumber of the player's character you want to set.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSelectedCharacterID(int characterNumber)
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = characterNumber;
                _playerManagerData->SelectedCharacterEntityIDs[Index] = _playerManagerData->AllCharacterIDs[GetCharacterIndex(characterNumber)];
            }

            /// <summary>
            /// Unsets player's SelectedCharacterID and updates SelectedCharacterNumber.
            /// </summary>
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsetSelectedCharacterID()
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = -1;
                _playerManagerData->SelectedCharacterEntityIDs[Index] = (BattleEntityID)(-1);
            }

            #endregion Public Methods

            #region Private Fields

            /// <summary>
            /// Pointer to the BattlePlayerManagerDataQSingleton.
            /// </summary>
            private BattlePlayerManagerDataQSingleton* _playerManagerData;

            #endregion Private Fields

            #region Private Methods

            /// <summary>
            /// Calculates the index where player's characters start in the BattlePlayerManagerDataQSingleton.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private int GetCharacterOffset() => Index * Constants.BATTLE_PLAYER_CHARACTER_COUNT;

            /// <summary>
            /// Calculates the index of player's character in the BattlePlayerManagerDataQSingleton.
            /// </summary>
            ///
            /// See [{Player Character Number}](#page-concepts-player-character-entity-character-number)
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private int GetCharacterIndex(int characterNumber) => GetCharacterOffset() + characterNumber;

            #endregion Private Methods
        }
    }
}

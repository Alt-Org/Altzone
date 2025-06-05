#define DEBUG_PLAYER_STAT_OVERRIDE

using System.Runtime.CompilerServices;
using UnityEngine;

using Quantum;
using Quantum.Collections;
using Photon.Deterministic;

using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Provides static methods to initialize, spawn, despawn, and query player-related data.
    /// </summary>
    public static unsafe class BattlePlayerManager
    {
        #region Public

        #region Public - Static Methods

        public static void Init(Frame f, BattleArenaQSpec battleArenaSpec)
        {
            Debug.Log("[PlayerManager] Init");

            for (int i = 0; i < s_spawnPoints.Length; i++)
            {
                s_spawnPoints[i] = BattleGridManager.GridPositionToWorldPosition(battleArenaSpec.PlayerSpawnPositions[i]);
            }

            BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);

            PlayerHandleInternal.SetAllPlayStates(playerManagerData, BattlePlayerPlayState.NotInGame);
            playerManagerData->PlayerCount = 0;

            {
                BattleParameters.PlayerType[] playerSlotTypes = BattleParameters.GetPlayerSlotTypes(f);
                int                           playerCount     = BattleParameters.GetPlayerCount(f);

                int playerCountCheckNumber = 0;
                foreach (BattleParameters.PlayerType playerSlotType in playerSlotTypes)
                {
                    if (playerSlotType == BattleParameters.PlayerType.Player) playerCountCheckNumber++;
                }

                if (playerCountCheckNumber != playerCount)
                {
                    Debug.LogErrorFormat("[PlayerManager] BattleParameters player count does not match the number of player slots with type of Player\n"
                        + "BattleParameters player count {0}, Counted {1}",
                        playerCount,
                        playerCountCheckNumber
                    );

                    // this will prevent the game from starting
                    playerManagerData->PlayerCount = -100;
                }
            }
        }

        /// <summary>
        /// Registers player.
        /// </summary>
        /// <param name="f">Current %Quantum %Frame.</param>
        /// <param name="playerRef">Reference to the player.</param>
        public static void RegisterPlayer(Frame f, PlayerRef playerRef)
        {
            string[] playerSlotUserIDs = BattleParameters.GetPlayerSlotUserIDs(f);
            BattleParameters.PlayerType[] playerSlotTypes = BattleParameters.GetPlayerSlotTypes(f);
            BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);

            RuntimePlayer data = f.GetPlayerData(playerRef);

            string playerUserID = data.UserID;
            BattlePlayerSlot playerSlot = data.PlayerSlot;
            PlayerHandleInternal playerHandle = PlayerHandleInternal.GetPlayerHandle(playerManagerData, playerSlot);
            BattleParameters.PlayerType playerSlotType = playerSlotTypes[playerHandle.Index];

            if (playerSlotType != BattleParameters.PlayerType.Player)
            {
                Debug.LogErrorFormat("[PlayerManager] Player is in {0} which is type of {1}",
                    playerSlot,
                    playerSlotType
                );
                return;
            }

            if (playerSlotUserIDs[playerHandle.Index] != playerUserID)
            {
                Debug.LogErrorFormat("[PlayerManager] Player in {0} has incorrect UsedID",
                    playerSlot
                );
                return;
            }

            playerHandle.PlayerRef = playerRef;
            playerManagerData->PlayerCount++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAllPlayersRegistered(Frame f)
        {
            return GetPlayerManagerData(f)->PlayerCount == BattleParameters.GetPlayerCount(f);
        }

        public static void CreatePlayers(Frame f)
        {
            BattleParameters.PlayerType[]      playerSlotTypes   = BattleParameters.GetPlayerSlotTypes(f);
            BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);

            for (int playerIndex = 0; playerIndex < Constants.BATTLE_PLAYER_SLOT_COUNT; playerIndex++)
            {
                if (playerSlotTypes[playerIndex] != BattleParameters.PlayerType.Player) continue;

                PlayerHandleInternal playerHandle = new(playerManagerData, playerIndex);

                BattlePlayerSlot playerSlot = PlayerHandleInternal.GetSlot(playerIndex);
                BattleTeamNumber teamNumber = PlayerHandleInternal.GetTeamNumber(playerSlot);

                RuntimePlayer data = f.GetPlayerData(playerHandle.PlayerRef);

                // TODO: Fetch EntityPrototype for each character based on the BattleCharacterBase Id
                EntityPrototype entityPrototypeAsset = f.FindAsset(data.PlayerAvatar);

                EntityRef[] playerCharacterEntityArray = new EntityRef[Constants.BATTLE_PLAYER_CHARACTER_COUNT];

                // create playerEntity for each characters
                {
                    //{ player temp variables
                    BattlePlayerDataTemplateQComponent* playerDataTemplate;
                    FPVector2                           playerSpawnPosition;
                    FP                                  playerRotationBase;
                    int                                 playerGridExtendTop;
                    int                                 playerGridExtendBottom;
                    bool                                playerFlipped;
                    // player - hitBox temp variables
                    QList<BattlePlayerHitboxTemplate> playerHitboxListShieldTemplate;
                    QList<BattlePlayerHitboxTemplate> playerHitboxListCharacterTemplate;
                    QList<BattlePlayerHitboxTemplate> playerHitboxListSourceTemplate;
                    int                               playerHitboxListShieldTemplateCount;
                    int                               playerHitboxListCharacterTemplateCount;
                    QList<BattlePlayerHitboxLink>     playerHitboxListTarget;
                    BattlePlayerHitboxType            playerHitboxType;
                    FPVector2                         playerHitboxPosition;
                    FP                                playerHitboxExtents;
                    //} player temp variables

                    //{ set player common temp variables (used for all characters)

                    playerHitboxExtents = BattleGridManager.GridScaleFactor * FP._0_50;

                    if (teamNumber == BattleTeamNumber.TeamAlpha)
                    {
                        playerRotationBase = FP._0;
                        playerFlipped = false;
                    }
                    else
                    {
                        playerRotationBase = FP.Rad_180;
                        playerFlipped = true;
                    }

                    //} set player common temp variables

                    //{ player variables
                    EntityRef                  playerEntity;
                    BattlePlayerDataQComponent playerData;
                    Transform2D*               playerTransform;
                    // player - hitBox variables
                    QList<BattlePlayerHitboxLink> playerHitboxListAll;
                    QList<BattlePlayerHitboxLink> playerHitboxListShield;
                    QList<BattlePlayerHitboxLink> playerHitboxListCharacter;
                    BattlePlayerHitboxLink        playerHitboxLink;
                    EntityRef                     playerHitboxEntity;
                    BattlePlayerHitboxQComponent  playerHitbox;
                    PhysicsCollider2D             playerHitboxCollider;
                    // player - hitBox - collisionTrigger variables
                    BattleCollisionTriggerQComponent collisionTrigger;
                    //} player variables

                    for (int playerCharacterNumber = 0; playerCharacterNumber < playerCharacterEntityArray.Length; playerCharacterNumber++)
                    {

                        // create entity
                        playerEntity = f.Create(entityPrototypeAsset);

                        // get template data
                        playerDataTemplate                     = f.Unsafe.GetPointer<BattlePlayerDataTemplateQComponent>(playerEntity);
                        playerHitboxListShieldTemplateCount    = f.TryResolveList(playerDataTemplate->HitboxListShield,    out playerHitboxListShieldTemplate   ) ? playerHitboxListShieldTemplate    .Count : 0;
                        playerHitboxListCharacterTemplateCount = f.TryResolveList(playerDataTemplate->HitboxListCharacter, out playerHitboxListCharacterTemplate) ? playerHitboxListCharacterTemplate .Count : 0;

                        //{ set temp variables

                        playerSpawnPosition = playerHandle.GetOutOfPlayPosition(playerCharacterNumber, teamNumber);

                        if (!playerFlipped)
                        {
                            playerGridExtendTop    = playerDataTemplate->GridExtendTop;
                            playerGridExtendBottom = playerDataTemplate->GridExtendBottom;
                        }
                        else
                        {
                            playerGridExtendTop    = playerDataTemplate->GridExtendBottom;
                            playerGridExtendBottom = playerDataTemplate->GridExtendTop;
                        }

                        //} set temp variables

                        // allocate playerHitboxLists
                        if (playerHitboxListShieldTemplateCount + playerHitboxListCharacterTemplateCount > 0) playerHitboxListAll       = f.AllocateList<BattlePlayerHitboxLink>(playerHitboxListShieldTemplateCount + playerHitboxListCharacterTemplateCount);
                        if (playerHitboxListShieldTemplateCount                                          > 0) playerHitboxListShield    = f.AllocateList<BattlePlayerHitboxLink>(playerHitboxListShieldTemplateCount                                         );
                        if (                                      playerHitboxListCharacterTemplateCount > 0) playerHitboxListCharacter = f.AllocateList<BattlePlayerHitboxLink>(                                      playerHitboxListCharacterTemplateCount);

                        // initialize playerData
                        playerData = new BattlePlayerDataQComponent
                        {
                            PlayerRef           = PlayerRef.None,
                            Slot                = playerSlot,
                            TeamNumber          = teamNumber,
                            CharacterId         = data.Characters[playerCharacterNumber].Id,
                            CharacterClass      = data.Characters[playerCharacterNumber].Class,

                            StatHp              = data.Characters[playerCharacterNumber].Hp,
                            StatSpeed           = data.Characters[playerCharacterNumber].Speed,
                            StatCharacterSize   = data.Characters[playerCharacterNumber].CharacterSize,
                            StatAttack          = data.Characters[playerCharacterNumber].Attack,
                            StatDefence         = data.Characters[playerCharacterNumber].Defence,

                            GridExtendTop       = playerGridExtendTop,
                            GridExtendBottom    = playerGridExtendBottom,

                            TargetPosition      = playerSpawnPosition,
                            RotationBase        = playerRotationBase,
                            RotationOffset      = FP._0,

                            HitboxListAll       = playerHitboxListAll,
                            HitboxListShield    = playerHitboxListShield,
                            HitboxListCharacter = playerHitboxListCharacter
                        };

    #if DEBUG_PLAYER_STAT_OVERRIDE
                        playerData.StatHp            = FP.FromString( "1.0");
                        playerData.StatSpeed         = FP.FromString("20.0");
                        playerData.StatCharacterSize = FP.FromString( "1.0");
                        playerData.StatAttack        = FP.FromString( "1.0");
                        playerData.StatDefence       = FP.FromString( "1.0");
    #endif

                        // create hitBoxes
                        for (int i2 = 0; i2 < 2; i2++)
                        {
                            switch (i2)
                            {
                                case 0:
                                    if (playerHitboxListShieldTemplateCount <= 0) continue;
                                    playerHitboxType = BattlePlayerHitboxType.Shield;
                                    playerHitboxListSourceTemplate = playerHitboxListShieldTemplate;
                                    playerHitboxListTarget = playerHitboxListShield;
                                    break;

                                case 1:
                                    if (playerHitboxListCharacterTemplateCount <= 0) continue;
                                    playerHitboxType = BattlePlayerHitboxType.Character;
                                    playerHitboxListSourceTemplate = playerHitboxListCharacterTemplate;
                                    playerHitboxListTarget = playerHitboxListCharacter;
                                    break;

                                default:
                                    playerHitboxType = (BattlePlayerHitboxType)(-1);
                                    break;
                            }

                            foreach (BattlePlayerHitboxTemplate playerHitboxTemplate in playerHitboxListSourceTemplate)
                            {
                                // initialize hitBox component
                                playerHitbox = new BattlePlayerHitboxQComponent
                                {
                                    PlayerEntity       = playerEntity,
                                    HitboxType         = playerHitboxType,
                                    CollisionType      = playerHitboxTemplate.CollisionType,
                                    Normal             = FPVector2.Rotate(FPVector2.Down, playerRotationBase - playerHitboxTemplate.NormalAngle * FP.Deg2Rad),
                                    CollisionMinOffset = playerHitboxExtents
                                };

                                // initialize collisionTrigger component
                                collisionTrigger = new BattleCollisionTriggerQComponent
                                {
                                    Type = BattleCollisionTriggerType.Player
                                };

                                // initialize hitBox position
                                playerHitboxPosition = new FPVector2(
                                    (FP)playerHitboxTemplate.Position.X * BattleGridManager.GridScaleFactor,
                                    (FP)playerHitboxTemplate.Position.Y * BattleGridManager.GridScaleFactor
                                );

                                // initialize hitBox collider
                                playerHitboxCollider = PhysicsCollider2D.Create(f,
                                    shape: Shape2D.CreateBox(new FPVector2(playerHitboxExtents)),
                                    isTrigger: true
                                );

                                // create hitBox entity
                                playerHitboxEntity = f.Create();
                                f.Add(playerHitboxEntity, playerHitbox);
                                f.Add<Transform2D>(playerHitboxEntity);
                                f.Add(playerHitboxEntity, playerHitboxCollider);
                                f.Add(playerHitboxEntity, collisionTrigger);

                                // create hitBox link
                                playerHitboxLink = new BattlePlayerHitboxLink
                                {
                                    Entity = playerHitboxEntity,
                                    Position = playerHitboxPosition
                                };

                                // save hitBox link
                                playerHitboxListTarget.Add(playerHitboxLink);
                                playerHitboxListAll.Add(playerHitboxLink);
                            }
                        }

                        //{ initialize entity

                        f.Remove<BattlePlayerDataTemplateQComponent>(playerEntity);
                        f.Add(playerEntity, playerData, out BattlePlayerDataQComponent* playerDataPtr);

                        playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
                        BattlePlayerMovementController.Teleport(f, playerDataPtr, playerTransform,
                            playerSpawnPosition,
                            playerRotationBase
                        );

                        //} initialize entity

                        // initialize view
                        f.Events.BattlePlayerViewInit(playerEntity, playerSlot, BattleGridManager.GridScaleFactor);

                        // save entity
                        playerCharacterEntityArray[playerCharacterNumber] = playerEntity;
                    }
                }

                // set playerManagerData for player
                playerHandle.PlayState = BattlePlayerPlayState.OutOfPlay;
                playerHandle.SetCharacters(playerCharacterEntityArray);
            }
        }

        #region Public - Static Methods - Spawn/Despawn

        /// <summary>
        /// Spawns a player entity into the game.
        /// </summary>
        public static void SpawnPlayer(Frame f, BattlePlayerSlot slot, int characterNumber)
        {
            PlayerHandleInternal playerHandle = PlayerHandleInternal.GetPlayerHandle(GetPlayerManagerData(f), slot);

            if (playerHandle.PlayState == BattlePlayerPlayState.NotInGame)
            {
                Debug.LogError("[PlayerManager] Can not spawn player that is not in game");
                return;
            }

            if (!PlayerHandleInternal.IsValidCharacterNumber(characterNumber))
            {
                Debug.LogErrorFormat("[PlayerManager] Invalid characterNumber = {0}", characterNumber);
                return;
            }

            SpawnPlayer(f, playerHandle, characterNumber);
        }


        /// <summary>
        /// Despawns a player entity from the game.
        /// </summary>
        public static void DespawnPlayer(Frame f, BattlePlayerSlot slot)
        {
            PlayerHandleInternal playerHandle = PlayerHandleInternal.GetPlayerHandle(GetPlayerManagerData(f), slot);

            if (playerHandle.PlayState != BattlePlayerPlayState.InPlay)
            {
                Debug.LogError("[PlayerManager] Can not despawn player that is not in play");
                return;
            }

            DespawnPlayer(f, playerHandle);
        }

        #endregion Public - Static Methods - Spawn/Despawn

        #endregion Public - Static Methods

        #region Public - PlayerHandle struct

        /// <summary>
        /// Public helper struct for getting player information.
        /// </summary>
        ///
        /// This is a public wrapper for the private PlayerHandleInternal that is used by the BattlePlayerManager internally.<br/>
        /// This only exposes the parts of the PlayerHandleInternal that is meant to be accessible outside of BattlePlayerManager.
        public struct PlayerHandle
        {
            //{ Public Static Methods

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

            //} Public Static Methods

            //{ Public Properties

            public BattlePlayerPlayState PlayState
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.PlayState; }

            public BattlePlayerSlot Slot
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => PlayerHandleInternal.GetSlot(_internalHandle.Index); }

            public PlayerRef PlayerRef
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.PlayerRef; }

            public EntityRef SelectedCharacter
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacter; }

            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacterNumber; }

            //} Public Properties

            //{ Private

            private PlayerHandleInternal _internalHandle;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private PlayerHandle(PlayerHandleInternal internalHandle)
            {
                _internalHandle = internalHandle;
            }

            //} Private
        }

        #endregion Public - PlayerHandle struct

        #endregion Public

        #region Private

        private static readonly FPVector2[] s_spawnPoints = new FPVector2[Constants.BATTLE_PLAYER_SLOT_COUNT];

        #region Private - PlayerHandleInternal struct

        /// <summary>
        /// Internal helper struct for player operations and state management in BattlePlayerManager.
        /// Has static helper methods for player operations.
        /// Can be instantiated to handle specific player's data stored in BattlePlayerManagerDataQSingleton.
        /// </summary>
        private struct PlayerHandleInternal
        {
            //{ Public Static Methods

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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetPlayerIndex(BattlePlayerManagerDataQSingleton* playerManagerData, PlayerRef playerRef)
            {
                for (int i = 0; i < Constants.BATTLE_PLAYER_SLOT_COUNT; i++)
                {
                    if (playerManagerData->PlayerRefs[i] == playerRef) return i;
                }
                return -1;
            }

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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandleInternal GetPlayerHandle(BattlePlayerManagerDataQSingleton* playerManagerData, BattlePlayerSlot slot)
            {
                int playerIndex = GetPlayerIndex(slot);
                return new PlayerHandleInternal(playerManagerData, playerIndex);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandleInternal GetTeammateHandle(BattlePlayerManagerDataQSingleton* playerManagerData, BattlePlayerSlot slot)
            {
                int playerIndex = GetTeammatePlayerIndex(slot);
                return new PlayerHandleInternal(playerManagerData, playerIndex);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetAllPlayStates(BattlePlayerManagerDataQSingleton* playerManagerData, BattlePlayerPlayState playerPlayState)
            {
                for (int i = 0; i < Constants.BATTLE_PLAYER_SLOT_COUNT; i++)
                {
                    playerManagerData->PlayStates[i] = playerPlayState;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsValidCharacterNumber(int characterNumber) => characterNumber >= 0 && characterNumber < Constants.BATTLE_PLAYER_CHARACTER_COUNT;

            //} Public Static Methods

            //{ Public Properties

            public int Index { get; set; }

            /// <summary>
            /// Gets/Sets player's PlayState.
            /// </summary>
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
            public PlayerRef PlayerRef
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _playerManagerData->PlayerRefs[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _playerManagerData->PlayerRefs[Index] = value;
            }

            /// <summary>
            /// Gets player's SelectedCharacter.
            /// The SelectedCharacter is a EntityRef to the character that is currently in play.
            /// </summary>
            public EntityRef SelectedCharacter
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacters[Index]; }

            /// <summary>
            /// Gets player's SelectedCharacterNumber.
            /// The SelectedCharacterNumber is the number of the character that is currently in play.
            /// </summary>
            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacterNumbers[Index]; }

            public FPVector2 SpawnPosition
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => s_spawnPoints[Index]; }

            //} Public Properties

            /// <summary>
            /// Constructor for PlayerHandleInternal.
            /// </summary>
            /// <param name="playerManagerData">Pointer to BattlePlayerManagerDataQSingleton.</param>
            /// <param name="playerIndex">Index of the player that you want the handle to.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PlayerHandleInternal(BattlePlayerManagerDataQSingleton* playerManagerData, int playerIndex)
            {
                Index = playerIndex;
                _playerManagerData = playerManagerData;
            }

            //{ Public Methods

            /// <summary>
            /// Gets an EntityRef to a player's Character by characterNumber.
            /// </summary>
            /// <param name="characterNumber">CharacterNumber of the player's character you want to get.</param>
            /// <returns>EntityRef to a player's Character.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityRef GetCharacter(int characterNumber) => _playerManagerData->AllCharacters[GetCharacterIndex(characterNumber)];

            /// <summary>
            /// Saves player's created character EntityRefs to BattlePlayerManagerDataQSingleton.
            /// </summary>
            /// <param name="entityRefArray">The Character EntityRefs as an array.</param>
            public void SetCharacters(EntityRef[] entityRefArray)
            {
                int characterOffset = GetCharacterOffset();
                for (int i = 0; i < Constants.BATTLE_PLAYER_CHARACTER_COUNT; i++)
                {
                    _playerManagerData->AllCharacters[characterOffset + i] = entityRefArray[i];
                }
            }

            /// <summary>
            /// Sets player's SelectedCharacter and updates SelectedCharacterNumber based on <paramref name="characterNumber"/>.
            /// The SelectedCharacter is a EntityRef to the character that is currently in play.
            /// </summary>
            /// <param name="characterNumber">CharacterNumber of the player's character you want to set.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSelectedCharacter(int characterNumber)
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = characterNumber;
                _playerManagerData->SelectedCharacters[Index] = _playerManagerData->AllCharacters[GetCharacterIndex(characterNumber)];
            }

            /// <summary>
            /// Unsets player's SelectedCharacter and updates SelectedCharacterNumber.
            /// The SelectedCharacter is a EntityRef to the character that is currently in play.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsetSelectedCharacter()
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = -1;
                _playerManagerData->SelectedCharacters[Index] = EntityRef.None;
            }

            /// <summary>
            /// Generates a position for the player's character that is out of play
            /// Each character of each player has a unique position that is used when the character is out of play.
            /// </summary>
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
                        column = 10 * Index;
                        break;
                    case BattleTeamNumber.TeamBeta:
                        row = BattleGridManager.Rows - 1 + 10 * (characterNumber + 1);
                        column = BattleGridManager.Columns - 1 - 10 * (Index - 2);
                        break;

                }

                return new FPVector2
                (
                    BattleGridManager.GridColToWorldXPosition(column),
                    BattleGridManager.GridRowToWorldYPosition(row)
                );
            }

            //} Public Methods

            //{ Private Fields
            /// <summary>Pointer to the BattlePlayerManagerDataQSingleton.</summary>
            private BattlePlayerManagerDataQSingleton* _playerManagerData;
            //} Private Fields

            //{ Private Methods

            /// <summary>Calculates the index where player's characters start in the BattlePlayerManagerDataQSingleton.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private int GetCharacterOffset() => Index * Constants.BATTLE_PLAYER_CHARACTER_COUNT;

            /// <summary>Calculates the index of player's character is in the BattlePlayerManagerDataQSingleton.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private int GetCharacterIndex(int characterNumber) => GetCharacterOffset() + characterNumber;

            //} Private Methods
        }

        #endregion Private - PlayerHandleInternal struct

        #region Private - Static Methods

        /// <summary>
        /// Private helper method for getting the BattlePlayerManagerDataQSingleton from the %Quantum %Frame.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private static BattlePlayerManagerDataQSingleton* GetPlayerManagerData(Frame f)
        {
            BattlePlayerManagerDataQSingleton* playerManagerData;
            bool isFound = f.Unsafe.TryGetPointerSingleton(out playerManagerData);
            if (isFound)
            {
                return playerManagerData;
            }
            else
            {
                Debug.LogFormat("[PlayerManager] Couldn't find PlayerManagerData singleton");
                return null;
            }
        }

        private static void SpawnPlayer(Frame f, PlayerHandleInternal playerHandle, int characterNumber)
        {
            EntityRef character = playerHandle.GetCharacter(characterNumber);
            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(character);
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(character);

            FPVector2 worldPosition;

            if (playerHandle.PlayState == BattlePlayerPlayState.InPlay)
            {
                worldPosition = f.Unsafe.GetPointer<Transform2D>(playerHandle.SelectedCharacter)->Position;
                DespawnPlayer(f, playerHandle);
            }
            else
            {
                worldPosition = playerHandle.SpawnPosition;
            }

            playerData->PlayerRef = playerHandle.PlayerRef;

            BattlePlayerMovementController.Teleport(f, playerData, playerTransform,
                worldPosition,
                playerData->RotationBase
            );

            playerData->TargetPosition = worldPosition;

            playerHandle.SetSelectedCharacter(characterNumber);
            playerHandle.PlayState = BattlePlayerPlayState.InPlay;
        }

        private static void DespawnPlayer(Frame f, PlayerHandleInternal playerHandle)
        {
            EntityRef selectedCharacter = playerHandle.SelectedCharacter;
            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(selectedCharacter);
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(selectedCharacter);

            FPVector2 worldPosition = playerHandle.GetOutOfPlayPosition(playerHandle.SelectedCharacterNumber, playerData->TeamNumber);

            playerData->PlayerRef = PlayerRef.None;

            BattlePlayerMovementController.Teleport(f, playerData, playerTransform,
                worldPosition,
                playerData->RotationBase
            );

            playerData->TargetPosition = worldPosition;

            playerHandle.UnsetSelectedCharacter();
            playerHandle.PlayState = BattlePlayerPlayState.OutOfPlay;
        }

        #endregion Private - Static Methods

        #endregion Private
    }
}

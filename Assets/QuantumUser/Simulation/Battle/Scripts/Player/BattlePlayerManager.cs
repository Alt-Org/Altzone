/// @file BattlePlayerManager.cs
/// <summary>
/// The manager script for player logic.
/// </summary>
///
/// The manager handles initializing players that are present in the game, as well as spawning and despawning player characters.<br/>
/// This script also contains the public and private PlayerHandle structs.

//#define DEBUG_PLAYER_STAT_OVERRIDE

using System.Runtime.CompilerServices;
using UnityEngine;

using Quantum;
using Quantum.Collections;
using Photon.Deterministic;

using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    public static class BattlePlayerPlayStateExtension
    {
        public static bool IsNotInGame(this BattlePlayerPlayState state)           => state is BattlePlayerPlayState.NotInGame;
        public static bool IsOutOfPlay(this BattlePlayerPlayState state)           => state is BattlePlayerPlayState.OutOfPlay or BattlePlayerPlayState.OutOfPlayRespawning or BattlePlayerPlayState.OutOfPlayFinal;
        public static bool IsOutOfPlayRespawning(this BattlePlayerPlayState state) => state is BattlePlayerPlayState.OutOfPlayRespawning;
        public static bool IsOutOfPlayFinal(this BattlePlayerPlayState state)      => state is BattlePlayerPlayState.OutOfPlayFinal;
        public static bool IsInPlay(this BattlePlayerPlayState state)              => state is BattlePlayerPlayState.InPlay;
    }

    /// <summary>
    /// Provides static methods to initialize, spawn, despawn, and query player-related data.
    /// </summary>
    public static unsafe class BattlePlayerManager
    {
        #region Public

        #region Public - Static Methods

        /// <summary>
        /// Initializes the spawn positions for players. <br/>
        /// Sets the initial play state of all players to not in game. <br/>
        /// Prevents the game from starting if the amount of players is not what it should be.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="battleArenaSpec">The spec of the arena.</param>
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
                    Error(f, "BattleParameters player count does not match the number of player slots with type of Player\n"
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
        ///
        /// <param name="f">Current %Quantum %Frame.</param>
        /// <param name="playerRef">Reference to the player.</param>
        public static void RegisterPlayer(Frame f, PlayerRef playerRef)
        {
            string[]                           playerSlotUserIDs = BattleParameters.GetPlayerSlotUserIDs(f);
            BattleParameters.PlayerType[]      playerSlotTypes   = BattleParameters.GetPlayerSlotTypes(f);
            BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);

            RuntimePlayer data = f.GetPlayerData(playerRef);

            string                      playerUserID   = data.UserID;
            BattlePlayerSlot            playerSlot     = data.PlayerSlot;
            PlayerHandleInternal        playerHandle   = PlayerHandleInternal.GetPlayerHandle(playerManagerData, playerSlot);
            BattleParameters.PlayerType playerSlotType = playerSlotTypes[playerHandle.Index];

            if (playerSlotType != BattleParameters.PlayerType.Player)
            {
                Error(f, "Player is in {0} which is type of {1}",
                    playerSlot,
                    playerSlotType
                );
                return;
            }

            if (playerSlotUserIDs[playerHandle.Index] != playerUserID)
            {
                Error(f, "Player in {0} has incorrect UsedID",
                    playerSlot
                );
                return;
            }

            playerHandle.PlayerRef = playerRef;
            playerManagerData->PlayerCount++;

            f.Events.BattleViewPlayerConnected(data);
        }

        /// <summary>
        /// Verifies that all players in the game have been registered.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <returns>True if all players have been registered, false if any have not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAllPlayersRegistered(Frame f)
        {
            return GetPlayerManagerData(f)->PlayerCount == BattleParameters.GetPlayerCount(f);
        }

        /// <summary>
        /// Creates all character entities for each player in the game, inititalizing data, hitboxes and view components.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
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

                EntityRef[] playerCharacterEntityArray = new EntityRef[Constants.BATTLE_PLAYER_CHARACTER_COUNT];

                // create playerEntity for each characters
                {
                    //{ player temp variables
                    BattlePlayerCharacterClass          playerClass;
                    AssetRef<EntityPrototype>           playerEntityPrototype;
                    BattlePlayerDataTemplateQComponent* playerDataTemplate;
                    FPVector2                           playerSpawnPosition;
                    FP                                  playerRotationBase;
                    int                                 playerGridExtendTop;
                    int                                 playerGridExtendBottom;
                    bool                                playerFlipped;
                    // player - hitBox temp variables
                    QList<BattlePlayerHitboxColliderTemplate> playerHitboxListShieldColliderTemplate;
                    QList<BattlePlayerHitboxColliderTemplate> playerHitboxListCharacterColliderTemplate;
                    QList<BattlePlayerHitboxColliderTemplate> playerHitboxListSourceColliderTemplate;
                    int                                       playerHitboxListShieldColliderTemplateCount;
                    int                                       playerHitboxListCharacterColliderTemplateCount;
                    EntityRef                                 playerHitboxTargetEntity;
                    BattlePlayerHitboxType                    playerHitboxType;
                    BattlePlayerCollisionType                 playerHitboxCollisionType;
                    FPVector2                                 playerHitboxPosition;
                    FPVector2                                 playerHitboxExtents;
                    FPVector2                                 playerHitboxNormal;
                    int                                       playerHitboxHeight;
                    Shape2D                                   playerHitboxColliderPart;
                    //} player temp variables

                    //{ set player common temp variables (used for all characters)

                    //playerHitboxExtents = BattleGridManager.GridScaleFactor * FP._0_50;

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
                    EntityRef                     playerHitboxShieldEntity    = EntityRef.None;
                    EntityRef                     playerHitboxCharacterEntity = EntityRef.None;
                    BattlePlayerHitboxQComponent  playerHitbox;
                    PhysicsCollider2D             playerHitboxCollider;
                    // player - hitBox - collisionTrigger variables
                    BattleCollisionTriggerQComponent collisionTrigger;
                    //} player variables

                    for (int playerCharacterNumber = 0; playerCharacterNumber < playerCharacterEntityArray.Length; playerCharacterNumber++)
                    {
                        // entity prototype
                        playerEntityPrototype = BattleAltzoneLink.GetCharacterPrototype(data.Characters[playerCharacterNumber].Id);
                        if (playerEntityPrototype == null)
                        {
                            playerEntityPrototype = BattleAltzoneLink.GetCharacterPrototype(0);
                        }

                        // create entity
                        playerEntity = f.Create(playerEntityPrototype);

                        // get template data
                        playerDataTemplate                     = f.Unsafe.GetPointer<BattlePlayerDataTemplateQComponent>(playerEntity);
                        playerHitboxListShieldColliderTemplateCount    = f.TryResolveList(playerDataTemplate->HitboxShield.ColliderTemplateList,    out playerHitboxListShieldColliderTemplate   ) ? playerHitboxListShieldColliderTemplate    .Count : 0;
                        playerHitboxListCharacterColliderTemplateCount = f.TryResolveList(playerDataTemplate->HitboxCharacter.ColliderTemplateList, out playerHitboxListCharacterColliderTemplate) ? playerHitboxListCharacterColliderTemplate .Count : 0;

                        //{ set temp variables

                        playerClass = (BattlePlayerCharacterClass)data.Characters[playerCharacterNumber].Class;

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

                        // load class
                        BattlePlayerClassManager.LoadClass(playerClass);

                        // create hitBoxes
                        for (int i = 0; i < 2; i++)
                        {
                            // create hitBox entity
                            playerHitboxTargetEntity = f.Create();

                            //{ initialize collisionTrigger component

                            collisionTrigger = new BattleCollisionTriggerQComponent();

                            switch (i)
                            {
                                case 0:
                                    if (playerHitboxListShieldColliderTemplateCount <= 0)
                                    {
                                        playerHitboxShieldEntity = EntityRef.None;
                                        continue;
                                    };

                                    playerHitboxType                       = BattlePlayerHitboxType.Shield;
                                    playerHitboxCollisionType              = playerDataTemplate->HitboxShield.CollisionType;
                                    playerHitboxListSourceColliderTemplate = playerHitboxListShieldColliderTemplate;
                                    playerHitboxShieldEntity               = playerHitboxTargetEntity;
                                    playerHitboxNormal                     = FPVector2.Rotate(FPVector2.Up, FP.Deg2Rad * playerDataTemplate->HitboxShield.NormalAngleDeg);

                                    collisionTrigger.Type = BattleCollisionTriggerType.Shield;
                                    break;

                                case 1:
                                    if (playerHitboxListCharacterColliderTemplateCount <= 0)
                                    {
                                        playerHitboxCharacterEntity = EntityRef.None;
                                        continue;
                                    };

                                    playerHitboxType                       = BattlePlayerHitboxType.Character;
                                    playerHitboxCollisionType              = playerDataTemplate->HitboxCharacter.CollisionType;
                                    playerHitboxListSourceColliderTemplate = playerHitboxListCharacterColliderTemplate;
                                    playerHitboxCharacterEntity            = playerHitboxTargetEntity;
                                    playerHitboxNormal                     = FPVector2.Rotate(FPVector2.Up, FP.Deg2Rad * playerDataTemplate->HitboxCharacter.NormalAngleDeg);

                                    collisionTrigger.Type = BattleCollisionTriggerType.Player;
                                    break;

                                default:
                                    playerHitboxType          = (BattlePlayerHitboxType)(-1);
                                    playerHitboxCollisionType = (BattlePlayerCollisionType)(-1);
                                    playerHitboxNormal        = FPVector2.Up;
                                    break;
                            }

                            // initialize hitBox collider
                            playerHitboxCollider = PhysicsCollider2D.Create(f,
                                shape: Shape2D.CreatePersistentCompound(),
                                isTrigger: true
                            );

                            // inititalize hitbox height
                            playerHitboxHeight = 0;

                            foreach (BattlePlayerHitboxColliderTemplate playerHitboxColliderTemplate in playerHitboxListSourceColliderTemplate)
                            {
                                playerHitboxHeight = Mathf.Max(playerHitboxColliderTemplate.Position.Y, playerHitboxHeight);

                                playerHitboxExtents = new FPVector2(
                                    (FP)playerHitboxColliderTemplate.Size.X * BattleGridManager.GridScaleFactor * FP._0_50,
                                    (FP)playerHitboxColliderTemplate.Size.Y * BattleGridManager.GridScaleFactor * FP._0_50
                                );

                                playerHitboxPosition = new FPVector2(
                                    ((FP)playerHitboxColliderTemplate.Position.X - FP._0_50) * BattleGridManager.GridScaleFactor + playerHitboxExtents.X,
                                    ((FP)playerHitboxColliderTemplate.Position.Y + FP._0_50) * BattleGridManager.GridScaleFactor - playerHitboxExtents.Y
                                );

                                playerHitboxColliderPart = Shape2D.CreateBox(playerHitboxExtents, playerHitboxPosition);
                                playerHitboxCollider.Shape.Compound.AddShape(f, ref playerHitboxColliderPart);
                            }

                            // initialize hitBox component
                            playerHitbox = new BattlePlayerHitboxQComponent
                            {
                                PlayerEntity       = playerEntity,
                                IsActive           = true,
                                HitboxType         = playerHitboxType,
                                CollisionType      = playerHitboxCollisionType,
                                Normal             = playerHitboxNormal,
                                NormalBase         = playerHitboxNormal,
                                CollisionMinOffset = ((FP)playerHitboxHeight + FP._0_50) * BattleGridManager.GridScaleFactor
                            };

                            //} initialize collisionTrigger component

                            f.Add(playerHitboxTargetEntity, playerHitbox);
                            f.Add<Transform2D>(playerHitboxTargetEntity);
                            f.Add(playerHitboxTargetEntity, playerHitboxCollider);
                            f.Add(playerHitboxTargetEntity, collisionTrigger);
                        }

                        //{ initialize playerData

                        playerData = new BattlePlayerDataQComponent
                        {
                            PlayerRef         = PlayerRef.None,
                            Slot              = playerSlot,
                            TeamNumber        = teamNumber,
                            CharacterId       = data.Characters[playerCharacterNumber].Id,
                            CharacterClass    = playerClass,

                            Stats             = data.Characters[playerCharacterNumber].Stats,

                            GridExtendTop     = playerGridExtendTop,
                            GridExtendBottom  = playerGridExtendBottom,

                            TargetPosition    = playerSpawnPosition,
                            RotationBase      = playerRotationBase,
                            RotationOffset    = FP._0,

                            CurrentHp         = FP._0,
                            CurrentDefence    = FP._0,

                            HitboxShieldEntity      = playerHitboxShieldEntity,
                            HitboxCharacterEntity   = playerHitboxCharacterEntity,

                            DisableRotation   = playerDataTemplate->DisableRotation
                        };

#if DEBUG_PLAYER_STAT_OVERRIDE
                        playerData.Stats.Hp            = FP.FromString("3.0");
                        playerData.Stats.Speed         = FP.FromString("20.0");
                        playerData.Stats.CharacterSize = FP.FromString("1.0");
                        playerData.Stats.Attack        = FP.FromString("1.0");
                        playerData.Stats.Defence       = FP.FromString("1.0");
#endif
                        playerData.CurrentHp = playerData.Stats.Hp;
                        playerData.CurrentDefence = playerData.Stats.Defence;

                        //} initialize playerData

                        //{ initialize entity

                        f.Remove<BattlePlayerDataTemplateQComponent>(playerEntity);
                        f.Add(playerEntity, playerData, out BattlePlayerDataQComponent* playerDataPtr);

                        playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
                        BattlePlayerMovementController.Teleport(f, playerDataPtr, playerTransform,
                            playerSpawnPosition,
                            playerRotationBase
                        );

                        BattlePlayerClassManager.OnCreate(f, playerHandle.ConvertToPublic(), playerDataPtr, playerEntity);

                        //} initialize entity

                        // initialize view
                        f.Events.BattlePlayerViewInit(playerEntity, playerSlot, BattleGridManager.GridScaleFactor);

                        // save entity
                        playerCharacterEntityArray[playerCharacterNumber] = playerEntity;

                        // set playerManagerData for player character
                        playerHandle.SetCharacterState(playerCharacterNumber, playerData.CurrentHp > 0 ? BattlePlayerCharacterState.Alive : BattlePlayerCharacterState.Dead);
                    }
                }

                // set playerManagerData for player
                playerHandle.PlayState = BattlePlayerPlayState.OutOfPlay;
                playerHandle.AllowCharacterSwapping = true;
                playerHandle.SetCharacters(playerCharacterEntityArray);
            }
        }

        #region Public - Static Methods - Spawn/Despawn

        /// <summary>
        /// Spawns a player character entity into the game. <br/>
        /// Verifies that the player is in the game and the character to be spawned is valid. <br/>
        /// Actual spawning handled by a separate private method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="slot">The slot of the player for which the character is to be spawned.</param>
        /// <param name="characterNumber">The character number of the character to be spawned.</param>
        public static void SpawnPlayer(Frame f, BattlePlayerSlot slot, int characterNumber)
        {
            PlayerHandleInternal playerHandle = PlayerHandleInternal.GetPlayerHandle(GetPlayerManagerData(f), slot);

            if (playerHandle.PlayState.IsNotInGame())
            {
                Debug.LogError("[PlayerManager] Can not spawn player that is not in game");
                return;
            }

            if (!PlayerHandleInternal.IsValidCharacterNumber(characterNumber))
            {
                Debug.LogErrorFormat("[PlayerManager] Invalid characterNumber = {0}", characterNumber);
                return;
            }

            if (playerHandle.GetCharacterState(characterNumber) == BattlePlayerCharacterState.Dead)
            {
                Debug.LogFormat("[PlayerManager] Player character {0} is dead and will not be spawned", characterNumber);
                return;
            }

            SpawnPlayer(f, playerHandle, characterNumber);
        }


        /// <summary>
        /// Despawns a player's active character entity from the game. <br/>
        /// Verifies that the player has a character in play. <br/>
        /// If <paramref name="kill"/> is set to true, the character's state is marked as dead prior to despawning. <br/>
        /// Actual despawning handled by a separate private method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="slot">The slot of the player for which the character is to be despawned.</param>
        /// <param name="kill">If true, marks the character as dead.</param>
        public static void DespawnPlayer(Frame f, BattlePlayerSlot slot, bool kill = false)
        {
            PlayerHandleInternal playerHandle = PlayerHandleInternal.GetPlayerHandle(GetPlayerManagerData(f), slot);

            if (!playerHandle.PlayState.IsInPlay())
            {
                Debug.LogError("[PlayerManager] Can not despawn player that is not in play");
                return;
            }

            if (kill) playerHandle.SelectedCharacterState = BattlePlayerCharacterState.Dead;
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PlayerHandle GetPlayerHandleFromInternal(BattlePlayerManagerDataQSingleton* playerManagerData, int playerIndex)
            {
                return new PlayerHandle(new PlayerHandleInternal(playerManagerData, playerIndex));
            }

            //} Public Static Methods

            //{ Public Properties

            public BattlePlayerPlayState PlayState
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.PlayState; }

            public BattlePlayerSlot Slot
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => PlayerHandleInternal.GetSlot(_internalHandle.Index); }

            public PlayerRef PlayerRef
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.PlayerRef; }

            public FrameTimer RespawnTimer
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.RespawnTimer;
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _internalHandle.RespawnTimer = value;
            }

            public bool AllowCharacterSwapping
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.AllowCharacterSwapping;
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _internalHandle.AllowCharacterSwapping = value;
            }

            public EntityRef SelectedCharacter
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacter; }

            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacterNumber; }

            public BattlePlayerCharacterState SelectedCharacterState
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _internalHandle.SelectedCharacterState; }

            //} Public Properties

            //{ Public Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BattlePlayerCharacterState GetCharacterState(int characterNumber) => _internalHandle.GetCharacterState(characterNumber);

            public void SetOutOfPlayRespawning()
            {
                if (!_internalHandle.PlayState.IsOutOfPlay())
                {
                    Debug.LogError("[PlayerManager] Can not set player that is not OutOfPlay as OutOfPlayRespawning");
                    return;
                }
                _internalHandle.PlayState = BattlePlayerPlayState.OutOfPlayRespawning;
            }

            public void SetOutOfPlayFinal()
            {
                if (!_internalHandle.PlayState.IsOutOfPlay())
                {
                    Debug.LogError("[PlayerManager] Can not set player that is not OutOfPlay as OutOfPlayFinal");
                    return;
                }
                _internalHandle.PlayState = BattlePlayerPlayState.OutOfPlayFinal;
            }

            //} Public Methods

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
        /// Internal helper struct for player operations and state management in BattlePlayerManager.<br/>
        /// Has static helper methods for player operations.<br/>
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
            /// <param name="characterNumber">The character number to verify.</param>
            /// <returns>True if the given character number is valid, false if it is not.</returns>
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
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->PlayStates[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _playerManagerData->PlayStates[Index] = value;
            }

            /// <summary>
            /// Gets/Sets player's PlayerRef.
            /// </summary>
            public PlayerRef PlayerRef
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->PlayerRefs[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _playerManagerData->PlayerRefs[Index] = value;
            }

            public FrameTimer RespawnTimer
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->RespawnTimer[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _playerManagerData->RespawnTimer[Index] = value;
            }

            public bool AllowCharacterSwapping
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->AllowCharacterSwapping[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _playerManagerData->AllowCharacterSwapping[Index] = value;
            }

            /// <summary>
            /// Gets player's SelectedCharacter.<br/>
            /// The SelectedCharacter is a EntityRef to the character that is currently in play.
            /// </summary>
            public EntityRef SelectedCharacter
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacters[Index]; }

            /// <summary>
            /// Gets player's SelectedCharacterNumber.<br/>
            /// The SelectedCharacterNumber is the number of the character that is currently in play.
            /// </summary>
            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacterNumbers[Index]; }

            public BattlePlayerCharacterState SelectedCharacterState
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetCharacterState(SelectedCharacterNumber);
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetCharacterState(SelectedCharacterNumber, value);
            }

            public FPVector2 SpawnPosition
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => s_spawnPoints[Index]; }

            //} Public Properties

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

            //{ Public Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PlayerHandle ConvertToPublic()
            {
                return PlayerHandle.GetPlayerHandleFromInternal(_playerManagerData, Index);
            }

            /// <summary>
            /// Gets an EntityRef to a player's Character by characterNumber.
            /// </summary>
            ///
            /// <param name="characterNumber">CharacterNumber of the player's character you want to get.</param>
            /// <returns>EntityRef to a player's Character.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityRef GetCharacter(int characterNumber) => _playerManagerData->AllCharacters[GetCharacterIndex(characterNumber)];

            /// <summary>
            /// Saves player's created character EntityRefs to BattlePlayerManagerDataQSingleton.
            /// </summary>
            ///
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
            /// Sets player's SelectedCharacter and updates SelectedCharacterNumber based on <paramref name="characterNumber"/>.<br/>
            /// The SelectedCharacter is a EntityRef to the character that is currently in play.
            /// </summary>
            ///
            /// <param name="characterNumber">CharacterNumber of the player's character you want to set.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BattlePlayerCharacterState GetCharacterState(int characterNumber) => _playerManagerData->AllCharactersStates[GetCharacterIndex(characterNumber)];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetCharacterState(int characterNumber, BattlePlayerCharacterState state) => _playerManagerData->AllCharactersStates[GetCharacterIndex(characterNumber)] = state;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSelectedCharacter(int characterNumber)
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = characterNumber;
                _playerManagerData->SelectedCharacters[Index] = _playerManagerData->AllCharacters[GetCharacterIndex(characterNumber)];
            }

            /// <summary>
            /// Unsets player's SelectedCharacter and updates SelectedCharacterNumber.<br/>
            /// The SelectedCharacter is a EntityRef to the character that is currently in play.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsetSelectedCharacter()
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = -1;
                _playerManagerData->SelectedCharacters[Index] = EntityRef.None;
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
                        row    = 0 - 10 * (characterNumber + 1);
                        column = -5 - 10 * Index;
                        break;
                    case BattleTeamNumber.TeamBeta:
                        row    = BattleGridManager.Rows - 1 + 10 * (characterNumber + 1);
                        column = BattleGridManager.Columns + 4 + 10 * (Index - 2);
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
        ///
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

        /// <summary>
        /// Spawns a player character entity into the game.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">PlayerHandle of the player the character will be spawned for.</param>
        /// <param name="characterNumber">The character number of the character to be spawned.</param>
        private static void SpawnPlayer(Frame f, PlayerHandleInternal playerHandle, int characterNumber)
        {
            EntityRef character = playerHandle.GetCharacter(characterNumber);
            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(character);
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(character);

            FPVector2 worldPosition;

            if (playerHandle.PlayState.IsInPlay())
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
            f.Events.BattleDebugUpdateStatsOverlay(playerData->Slot, playerData->Stats);

            playerHandle.PlayState = BattlePlayerPlayState.InPlay;

            f.Events.BattleViewSetRotationJoystickVisibility(!playerData->DisableRotation, playerData->Slot);

            BattlePlayerClassManager.OnSpawn(f, playerHandle.ConvertToPublic(), playerData, character);
        }

        /// <summary>
        /// Despawns a player's active character entity from the game.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">PlayerHandle of the player the character will be spawned for.</param>
        private static void DespawnPlayer(Frame f, PlayerHandleInternal playerHandle)
        {
            EntityRef selectedCharacter = playerHandle.SelectedCharacter;
            BattlePlayerDataQComponent* playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(selectedCharacter);
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(selectedCharacter);

            FPVector2 worldPosition = playerHandle.GetOutOfPlayPosition(playerHandle.SelectedCharacterNumber, playerData->TeamNumber);

            BattlePlayerClassManager.OnDespawn(f, playerHandle.ConvertToPublic(), playerData, selectedCharacter);

            playerData->PlayerRef = PlayerRef.None;

            BattlePlayerMovementController.Teleport(f, playerData, playerTransform,
                worldPosition,
                playerData->RotationBase
            );

            playerData->TargetPosition = worldPosition;

            playerHandle.UnsetSelectedCharacter();
            playerHandle.PlayState = BattlePlayerPlayState.OutOfPlay;
        }

        private static void Error(Frame f, string messageformat, params object[] args)
        {
            string message = string.Format(messageformat, args);
            Debug.LogError("[PlayerManager] " + message);
            f.Events.BattleDebugOnScreenMessage(message);
        }

        #endregion Private - Static Methods

        #endregion Private
    }
}

/// @file BattlePlayerManager.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerManager} partial class which handles player logic.<br/>
/// </summary>

//#define DEBUG_PLAYER_STAT_OVERRIDE

// System usings
using System.Runtime.CompilerServices;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Quantum.Collections;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// PlayerManager handles player management, allowing other classes to focus on gameplay logic.<br/>
    /// Provides static methods to initialize, spawn, despawn, and query player-related data.
    /// </summary>
    ///
    /// [{Player Overview}](#page-concepts-player-overview)<br/>
    /// [{Player Simulation Code Overview}](#page-concepts-player-simulation-overview)
    ///
    /// Handles initializing players that are present in the game, as well as spawning and despawning player characters.<br/>
    /// Also contains the public and private PlayerHandle structs.
    public static unsafe partial class BattlePlayerManager
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
            s_debugLogger = BattleDebugLogger.Create(typeof(BattlePlayerManager));

            s_debugOverlayStats = BattleDebugOverlayLink.AddEntries(new string[]
            {
                "Stat Hp",
                "Stat Speed",
                "Stat CharacterSize",
                "Stat Attack",
                "Stat Defence"
            });

            s_debugLogger.Log(f, "Init");

            for (int i = 0; i < s_spawnPoints.Length; i++)
            {
                s_spawnPoints[i] = BattleGridManager.GridPositionToWorldPosition(battleArenaSpec.PlayerSpawnPositions[i]);
            }

            BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);

            PlayerHandleInternal.SetAllPlayStates(playerManagerData, BattlePlayerPlayState.NotInGame);
            playerManagerData->PlayerCount = 0;

            {
                string[]                      playerSlotUserIDs = BattleParameters.GetPlayerSlotUserIDs(f);
                BattleParameters.PlayerType[] playerSlotTypes   = BattleParameters.GetPlayerSlotTypes(f);
                int                           playerCount       = BattleParameters.GetPlayerCount(f);

                string[] playerDebugStrings = new string[playerSlotTypes.Length];

                int playerCountCheckNumber = 0;
                for (int i = 0; i < playerSlotTypes.Length; i++)
                {
                    if (playerSlotTypes[i] == BattleParameters.PlayerType.Player)
                    {
                        playerDebugStrings[i] = string.Format("{0}({1})", playerSlotTypes[i], playerSlotUserIDs[i]);
                        playerCountCheckNumber++;
                    }
                    else
                    {
                        playerDebugStrings[i] = playerSlotTypes[i].ToString();
                    }
                }

                s_debugLogger.LogFormat(f, "Expected players: {{ {0}, {1}, {2}, {3} }}", playerDebugStrings[0], playerDebugStrings[1], playerDebugStrings[2], playerDebugStrings[3]);
                s_debugLogger.LogFormat(f, "Expected player count: {0}", playerCount);

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

            s_debugLogger.LogFormat(f, "Registering Player({0}) in {1}", playerUserID, playerSlot);

            if (playerSlotType != BattleParameters.PlayerType.Player)
            {
                Error(f, "Player({0}) is registered in {1} which is of type {2}", playerUserID, playerSlot, playerSlotType);
                return;
            }

            if (playerSlotUserIDs[playerHandle.Index] != playerUserID)
            {
                Error(f, "Player({0}) in {1} has incorrect UsedID, expected Player({2})", playerUserID, playerSlot, playerSlotUserIDs[playerHandle.Index]);
                return;
            }

            playerHandle.PlayerRef = playerRef;
            playerManagerData->PlayerCount++;

            f.Events.BattleViewPlayerConnected(data);
        }

        /// <summary>
        /// Marks the player as abandoned.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerRef">Reference to the player.</param>
        public static void MarkAbandoned(Frame f, PlayerRef playerRef)
        {
            BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);
            PlayerHandleInternal playerHandle = new(playerManagerData, PlayerHandleInternal.GetPlayerIndex(playerManagerData, playerRef));
            playerHandle.IsAbandoned = true;
            BattlePlayerQSystem.HandlePlayerAbandoned(f, playerHandle.ConvertToPublic());
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
        /// Creates all character entities for each player in the game, initializing data, hitboxes and view components.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public static void CreatePlayers(Frame f)
        {
            BattleParameters.PlayerType[]      playerSlotTypes   = BattleParameters.GetPlayerSlotTypes(f);
            BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);

            for (int playerIndex = 0; playerIndex < Constants.BATTLE_PLAYER_SLOT_COUNT; playerIndex++)
            {
                BattlePlayerSlot playerSlot = PlayerHandleInternal.GetSlot(playerIndex);
                BattleTeamNumber teamNumber = PlayerHandleInternal.GetTeamNumber(playerSlot);

                s_debugLogger.LogFormat(f, "({0}) Creating player, type: {1}", playerSlot, playerSlotTypes[playerIndex]);

                if (playerSlotTypes[playerIndex] == BattleParameters.PlayerType.None)
                {
                    s_debugLogger.LogFormat(f, "({0}) Skipping player creation, as type is None", playerSlot);
                    continue;
                }

                bool isBot = playerSlotTypes[playerIndex] == BattleParameters.PlayerType.Bot;

                PlayerHandleInternal playerHandle = new(playerManagerData, playerIndex);

                BattleCharacterBase[] battleBaseCharacters = !isBot
                                                           ? f.GetPlayerData(playerHandle.PlayerRef).Characters
                                                           : BattlePlayerBotController.GetBotCharacters(f);

                BattleEntityManager.CompoundEntityTemplate[] playerCharacterEntityArray = new BattleEntityManager.CompoundEntityTemplate[Constants.BATTLE_PLAYER_CHARACTER_COUNT];

                // create playerEntity for each character
                {
                    //{ player temp variables
                    FP   playerRotationBase;
                    int  playerGridExtendTop;
                    int  playerGridExtendBottom;
                    bool playerFlipped;
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

                    for (int playerCharacterNumber = 0; playerCharacterNumber < playerCharacterEntityArray.Length; playerCharacterNumber++)
                    {
                        // set id and class
                        BattlePlayerCharacterID playerCharacterId = battleBaseCharacters[playerCharacterNumber].Id;
                        BattlePlayerCharacterClass playerClass    = (BattlePlayerCharacterClass)battleBaseCharacters[playerCharacterNumber].Class;

                        s_debugLogger.LogFormat(f, "({0}) Creating character, number {1}\n" +
                                                "Character ID:    {2},\n" +
                                                "Character Class: {3}",
                                                playerSlot,
                                                playerCharacterNumber,
                                                playerCharacterId,
                                                playerClass
                        );

                        // get entity prototypes
                        AssetRef<EntityPrototype> playerCharacterEntityPrototype = BattleAltzoneLink.GetCharacterPrototype(playerCharacterId);
                        if (playerCharacterEntityPrototype == null)
                        {
                            const int FallbackId = 0;

                            s_debugLogger.ErrorFormat(f, "({0}) Failed to fetch player character entity prototype ID {1}\nUsing fallback ID {2}", playerSlot, playerCharacterId, FallbackId);

                            playerCharacterId     = FallbackId;
                            playerClass           = BattlePlayerCharacterClass.None;
                            playerCharacterEntityPrototype = BattleAltzoneLink.GetCharacterPrototype(playerCharacterId);

                            s_debugLogger.LogFormat(f, "({0}) Creating fallback character, number {1}\n" +
                                                    "Character ID:    {2},\n" +
                                                    "Character Class: {3}",
                                                    playerSlot,
                                                    playerCharacterNumber,
                                                    playerCharacterId,
                                                    playerClass
                            );
                        }

                        // create entity
                        BattlePlayerEntityRef playerCharacterEntity = BattlePlayerEntityRef.Create(f, playerCharacterEntityPrototype);

                        BattleEntityManager.CompoundEntityTemplate playerCharacterEntityTemplate = BattleEntityManager.CompoundEntityTemplate.Create(playerCharacterEntity, 1);

                        // get template data
                        BattlePlayerDataTemplateQComponent* playerCharacterDataTemplate = f.Unsafe.GetPointer<BattlePlayerDataTemplateQComponent>(playerCharacterEntity);
                        int playerHitboxListCharacterColliderTemplateCount              = f.TryResolveList(playerCharacterDataTemplate->HitboxCharacter.ColliderTemplateList, out QList<BattlePlayerHitboxColliderTemplate> playerHitboxListCharacterColliderTemplate) ? playerHitboxListCharacterColliderTemplate.Count : 0;

                        //{ set temp variables

                        if (!playerFlipped)
                        {
                            playerGridExtendTop    = playerCharacterDataTemplate->GridExtendTop;
                            playerGridExtendBottom = playerCharacterDataTemplate->GridExtendBottom;
                        }
                        else
                        {
                            playerGridExtendTop    = playerCharacterDataTemplate->GridExtendBottom;
                            playerGridExtendBottom = playerCharacterDataTemplate->GridExtendTop;
                        }

                        //} set temp variables

                        // load class
                        BattlePlayerClassManager.LoadClass(playerClass);

                        //{ create player hitBox

                        // create hitBox entity

                        EntityRef playerHitboxCharacterEntity = f.Create();

                        //{ initialize collisionTrigger component

                        if (playerHitboxListCharacterColliderTemplateCount <= 0)
                        {
                            playerHitboxCharacterEntity = EntityRef.None;
                            continue;
                        };

                        BattlePlayerHitboxType playerHitboxType             = BattlePlayerHitboxType.Character;
                        BattlePlayerCollisionType playerHitboxCollisionType = playerCharacterDataTemplate->HitboxCharacter.CollisionType;
                        FP playerHitboxAngleRad                             = FP.Deg2Rad * playerCharacterDataTemplate->HitboxCharacter.NormalAngleDeg;

                        BattleCollisionTriggerQComponent collisionTrigger = BattleCollisionQSystem.CreateCollisionTriggerComponent(BattleCollisionTriggerType.Player);

                        // initialize hitBox collider
                        PhysicsCollider2D playerHitboxCollider = PhysicsCollider2D.Create(f,
                            shape: Shape2D.CreatePersistentCompound(),
                            isTrigger: true
                        );

                        // initialize hitbox height
                        int playerHitboxHeight = 0;

                        foreach (BattlePlayerHitboxColliderTemplate playerHitboxColliderTemplate in playerHitboxListCharacterColliderTemplate)
                        {
                            playerHitboxHeight = Mathf.Max(playerHitboxColliderTemplate.Position.Y, playerHitboxHeight);

                            FPVector2 playerHitboxExtents = new(
                                (FP)playerHitboxColliderTemplate.Size.X * BattleGridManager.GridScaleFactor * FP._0_50,
                                (FP)playerHitboxColliderTemplate.Size.Y * BattleGridManager.GridScaleFactor * FP._0_50
                            );

                            FPVector2 playerHitboxPosition = new(
                                ((FP)playerHitboxColliderTemplate.Position.X - FP._0_50) * BattleGridManager.GridScaleFactor + playerHitboxExtents.X,
                                ((FP)playerHitboxColliderTemplate.Position.Y + FP._0_50) * BattleGridManager.GridScaleFactor - playerHitboxExtents.Y
                            );

                            Shape2D playerHitboxColliderPart = Shape2D.CreateBox(playerHitboxExtents, playerHitboxPosition);
                            playerHitboxCollider.Shape.Compound.AddShape(f, ref playerHitboxColliderPart);
                        }

                        // initialize hitBox component
                        BattlePlayerHitboxQComponent playerHitbox = new()
                        {
                            ParentEntityRef    = playerCharacterEntity,
                            HitboxType         = playerHitboxType,
                            CollisionType      = playerHitboxCollisionType,
                            NormalAngleRad     = playerHitboxAngleRad,
                            CollisionMinOffset = ((FP)playerHitboxHeight + FP._0_50) * BattleGridManager.GridScaleFactor
                        };

                        //} initialize collisionTrigger component

                        f.Add(playerHitboxCharacterEntity, playerHitbox);
                        f.Add<Transform2D>(playerHitboxCharacterEntity);
                        f.Add(playerHitboxCharacterEntity, playerHitboxCollider);
                        f.Add(playerHitboxCharacterEntity, collisionTrigger);

                        //} create player hitBox

                        // create player shields
                        int playerCharacterShieldCount = BattlePlayerShieldManager.CreateShields(f, playerSlot, playerCharacterNumber, playerCharacterId, playerClass, playerCharacterEntity);

                        // save entity
                        playerCharacterEntityTemplate.Link(playerHitboxCharacterEntity, new FPVector2(0, 0));
                        playerCharacterEntityArray[playerCharacterNumber] = playerCharacterEntityTemplate;

                        //{ initialize playerData

                        BattlePlayerDataQComponent playerData = new()
                        {
                            // player's ref's and IDs
                            PlayerRef              = PlayerRef.None,
                            Slot                   = playerSlot,
                            TeamNumber             = teamNumber,
                            CharacterId            = playerCharacterId,
                            CharacterClass         = playerClass,

                            // player's stats
                            Stats                  = battleBaseCharacters[playerCharacterNumber].Stats,

                            // player's attributes
                            GridExtendTop          = playerGridExtendTop,
                            GridExtendBottom       = playerGridExtendBottom,
                            DisableRotation        = playerCharacterDataTemplate->DisableRotation,

                            // player's current state related data
                            MovementEnabled        = true,
                            RotationEnabled        = !playerCharacterDataTemplate->DisableRotation,
                            CurrentDefence         = FP._0,

                            // player's movement related data
                            TargetPosition         = playerCharacterEntity.GetTransform(f)->Position,
                            RotationBaseRad        = playerRotationBase,
                            RotationOffsetRad      = FP._0,

                            // player's shield related data
                            ShieldCount            = playerCharacterShieldCount,
                            AttachedShieldNumber   = 0,

                            // bot related data
                            BotMovementCooldownSec = FP._0
                        };

#if DEBUG_PLAYER_STAT_OVERRIDE
                        s_debugLogger.Warning(f, "DEBUG_PLAYER_STAT_OVERRIDE enabled!");

                        playerData.Stats.Speed         = FP.FromString("20.0");
                        playerData.Stats.CharacterSize = FP.FromString("1.0");
                        playerData.Stats.Attack        = FP.FromString("1.0");
                        playerData.Stats.Defence       = FP.FromString("1.0");

                        s_debugLogger.WarningFormat("Using Speed {0} override", playerData.Stats.Speed);
                        s_debugLogger.WarningFormat("Using CharacterSize {0} override", playerData.Stats.CharacterSize);
                        s_debugLogger.WarningFormat("Using Attack {0} override", playerData.Stats.Attack);
                        s_debugLogger.WarningFormat("Using Defence {0} override", playerData.Stats.Defence);
#endif
                        playerData.CurrentDefence = playerData.Stats.Defence;

                        s_debugLogger.LogFormat(f, "({0}) Character number {1} stats:\n" +
                                                "Speed:         {2}\n" +
                                                "CharacterSize: {3}\n" +
                                                "Attack:        {4}\n" +
                                                "Defence:       {5}",
                                                playerSlot,
                                                playerCharacterNumber,
                                                playerData.Stats.Speed,
                                                playerData.Stats.CharacterSize,
                                                playerData.Stats.Attack,
                                                playerData.Stats.Defence
                                                );

                        //} initialize playerData

                        // initialize entity
                        f.Remove<BattlePlayerDataTemplateQComponent>(playerCharacterEntity);
                        f.Add(playerCharacterEntity, playerData, out BattlePlayerDataQComponent* playerDataPtr);

                        BattlePlayerClassManager.CreationParameters creationParameters = BattlePlayerClassManager.OnCreate(f, playerHandle.ConvertToPublic(), playerDataPtr, playerCharacterEntity);

                        // attach shield
                        if (creationParameters.AttachedShieldNumber >= 0)
                        {
                            BattlePlayerShieldManager.AttachShield(f, playerSlot, playerCharacterNumber, creationParameters.AttachedShieldNumber, teleport: false);
                        }

                        // initialize view
                        f.Events.BattlePlayerCharacterViewInit(playerCharacterEntity, playerSlot, playerCharacterId, playerClass, BattleGridManager.GridScaleFactor);

                        // set playerManagerData for player character
                        playerHandle.SetCharacterState(playerCharacterNumber, BattlePlayerCharacterState.Alive);
                    }
                }

                BattleEntityID characterEntityGroupID = BattleEntityManager.RegisterCompound(f, playerCharacterEntityArray);

                // set playerManagerData for player
                playerHandle.PlayState = BattlePlayerPlayState.OutOfPlay;
                playerHandle.IsBot = isBot;
                playerHandle.AllowCharacterSwapping = true;
                playerHandle.PlayerGiveUpState = false;
                playerHandle.SetCharacterEntityGroupID(characterEntityGroupID);

                s_debugLogger.LogFormat(f, "({0}) Player created successfully", playerSlot);
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
                s_debugLogger.Error(f, "Can not spawn player that is not in game");
                return;
            }

            if (!PlayerHandleInternal.IsValidCharacterNumber(characterNumber))
            {
                s_debugLogger.ErrorFormat(f, "Invalid characterNumber = {0}", characterNumber);
                return;
            }

            if (playerHandle.GetCharacterState(characterNumber) == BattlePlayerCharacterState.Dead)
            {
                s_debugLogger.LogFormat(f, "Player character {0} is dead and will not be spawned", characterNumber);
                return;
            }

            SpawnPlayer(f, playerHandle, characterNumber);
        }


        /// <summary>
        /// Despawns a player's active character entity from the game. <br/>
        /// Verifies that the player has a character in play. <br/>
        /// If <paramref name="kill"/> is set to true, the character's state is marked as dead prior to despawning.<br/>
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
                s_debugLogger.Error(f, "Can not despawn player that is not in play");
                return;
            }

            if (kill) playerHandle.SelectedCharacterState = BattlePlayerCharacterState.Dead;
            DespawnPlayer(f, playerHandle);
        }

        #endregion Public - Static Methods - Spawn/Despawn

        #endregion Public - Static Methods

        #endregion Public

        #region Private

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        /// <summary>Debug overlay entry number for stats.</summary>
        private static int s_debugOverlayStats;

        private static readonly FPVector2[] s_spawnPoints = new FPVector2[Constants.BATTLE_PLAYER_SLOT_COUNT];

        #region Private - Static Methods

        /// <summary>
        /// Private helper method for getting the BattlePlayerManagerDataQSingleton from the %Quantum %Frame.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>Pointer reference to the PlayerManagerData singleton.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattlePlayerManagerDataQSingleton* GetPlayerManagerData(Frame f)
        {
            if (!f.Unsafe.TryGetPointerSingleton(out BattlePlayerManagerDataQSingleton* playerManagerData))
            {
                s_debugLogger.Error(f, "PlayerManagerData singleton not found!");
            }

            return playerManagerData;
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
            BattlePlayerEntityRef character = (BattlePlayerEntityRef)BattleEntityManager.Get(f, playerHandle.CharacterEntityGroupID, characterNumber, updateViewPlayState: true);
            BattlePlayerDataQComponent* playerData = character.GetDataQComponent(f);
            Transform2D* playerTransform = character.GetTransform(f);
            Transform2D* shieldTransform = playerData->AttachedShield.GetTransform(f);

            FPVector2 worldPosition;

            if (playerHandle.PlayState.IsInPlay())
            {
                worldPosition = f.Unsafe.GetPointer<Transform2D>(BattleEntityManager.Get(f, playerHandle.CharacterEntityGroupID, playerHandle.SelectedCharacterNumber))->Position;
                DespawnPlayer(f, playerHandle);
            }
            else
            {
                worldPosition = playerHandle.SpawnPosition;
            }

            s_debugLogger.LogFormat(f, "({0}) Spawning character number: {1}", playerData->Slot, characterNumber);

            playerData->PlayerRef = playerHandle.PlayerRef;

            BattlePlayerMovementController.Teleport(f, playerData, character, worldPosition);

            playerData->TargetPosition = worldPosition;

            playerHandle.SetSelectedCharacterNumber(characterNumber);

            BattleDebugOverlayLink.SetEntries(playerData->Slot, s_debugOverlayStats, new object[]
            {
                playerData->Stats.Speed,
                playerData->Stats.CharacterSize,
                playerData->Stats.Attack,
                playerData->Stats.Defence
            });

            f.Events.BattleCharacterSelected(playerData->Slot, characterNumber);

            playerData->AbilityCooldownSec = FrameTimer.FromSeconds(f, FP._3);
            playerData->AbilityActivateBufferSec = FrameTimer.FromSeconds(f, FP._0);

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
            BattlePlayerEntityRef selectedCharacter = (BattlePlayerEntityRef)BattleEntityManager.Get(f, playerHandle.CharacterEntityGroupID, playerHandle.SelectedCharacterNumber);
            BattlePlayerDataQComponent* playerData = selectedCharacter.GetDataQComponent(f);
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(selectedCharacter);

            s_debugLogger.LogFormat(f, "({0}) Despawning character number: {1}", playerData->Slot, playerHandle.SelectedCharacterNumber);

            BattlePlayerClassManager.OnDespawn(f, playerHandle.ConvertToPublic(), playerData, selectedCharacter);

            playerData->PlayerRef = PlayerRef.None;

            BattleEntityManager.Return(f, playerHandle.CharacterEntityGroupID, playerHandle.SelectedCharacterNumber);

            if(playerData->AttachedShield.ERef != EntityRef.None)
            {
                BattleEntityManager.Return(f, BattlePlayerShieldManager.Low_GetShieldEntityGroupID(f, playerData->Slot, playerHandle.SelectedCharacterNumber), playerData->AttachedShieldNumber);
            }

            playerData->TargetPosition = selectedCharacter.GetTransform(f)->Position;

            playerHandle.UnsetSelectedCharacterNumber();
            playerHandle.PlayState = BattlePlayerPlayState.OutOfPlay;

            f.Events.BattleCharacterSelected(playerData->Slot, -1);
        }

        private static void Error(Frame f, string messageformat, params object[] args)
        {
            string message = string.Format(messageformat, args);
            s_debugLogger.Error(f, message);
            f.Events.BattleDebugOnScreenMessage(message);
        }

        #endregion Private - Static Methods

        #endregion Private
    }
}

/// @file BattlePlayerManager.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerManager} partial class which handles player logic.<br/>
/// </summary>

//#define DEBUG_PLAYER_STAT_OVERRIDE

// System usings
using System.Collections.Generic;
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
    /// @bigtext{See [{PlayerManager}](#page-concepts-player-simulation-management-playermanager) for more info.}<br/>
    /// @bigtext{See [{Player Overview}](#page-concepts-player-overview) for more info.}<br/>
    /// @bigtext{See [{Player Simulation Code Overview}](#page-concepts-player-simulation-overview) for more info.}<br/>
    ///
    /// Handles initializing players that are present in the game, as well as spawning and despawning player characters.<br/>
    /// The @cref{Battle.QSimulation.Player.BattlePlayerManager,PlayerHandle} and @cref{Battle.QSimulation.Player.BattlePlayerManager,PlayerHandleInternal} structs are used internally.
    ///
    /// @anchor BattlePlayerManager-PlayerIndex
    /// @bigtext{**%Player Index**}
    ///
    /// %Player Index is used internally to index player data arrays in @cref{Quantum,BattlePlayerManagerDataQSingleton}.
    /// %Player [{Slots}](#page-concepts-player-slots-teams) are mapped to the internal %Player Indices using the @cref{Battle.QSimulation.Player.BattlePlayerManager,PlayerHandleInternal}
    /// @ref BattlePlayerManager-PlayerHandleInternal-PublicStaticMethods-PlayerIndexGetters "Player Index Getter" methods.
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

            s_debugLogger.Log(f, "Init");

            s_debugOverlayStats = BattleDebugOverlayLink.AddEntries(new string[]
            {
                "Stat Speed",
                "Stat CharacterSize",
                "Stat Attack",
                "Stat Defence"
            });

            // get slot information and player count
            string[]                      slotUserIDs = BattleParameters.GetPlayerSlotUserIDs(f);
            BattleParameters.PlayerType[] slotTypes   = BattleParameters.GetPlayerSlotTypes(f);
            int                           playerCount = BattleParameters.GetPlayerCount(f);

            Data.Ref dataRef = Data.GetRef(f);

            dataRef.PlayerCount        = 0;
            int playerCountCheckNumber = 0;

            string[] slotDebugStrings = new string[slotTypes.Length];

            // initialize and check slots
            for (int playerIndex = 0; playerIndex < Constants.BATTLE_PLAYER_SLOT_COUNT; playerIndex++)
            {
                //{ initialize slot

                BattlePlayerData.Ref    playerData      = Data.GetPlayerData(dataRef, playerIndex);
                BattleArenaQSpec.Player arenaPlayerSpec = battleArenaSpec.Players[playerIndex];

                BattlePlayerSlot      playerSlot      = Data.GetPlayerSlot(playerIndex);
                BattleTeamNumber      playerTeam      = Data.GetTeam(playerSlot);
                BattlePlayerPlayState playerPlayState = BattlePlayerPlayState.NotInGame;
                bool                  playerIsBot     = false;

                switch (slotTypes[playerIndex])
                {
                    case BattleParameters.PlayerType.Player:
                        playerPlayState = BattlePlayerPlayState.OutOfPlay;
                        playerCountCheckNumber++;

                        // debug info: slot type (ID)
                        slotDebugStrings[playerIndex] = string.Format("{0}({1})", slotTypes[playerIndex], slotUserIDs[playerIndex]);
                        break;
                    case BattleParameters.PlayerType.Bot:
                        playerPlayState = BattlePlayerPlayState.OutOfPlay;
                        playerIsBot = true;

                        goto case BattleParameters.PlayerType.None;
                    case BattleParameters.PlayerType.None:
                        // debug info: slot type
                        slotDebugStrings[playerIndex] = slotTypes[playerIndex].ToString();
                        break;
                    default:
                        return;
                }

                playerData.Low_Level.Slot                    = playerSlot;
                playerData.Low_Level.Team                    = playerTeam;
                playerData.Low_Level.PlayState               = playerPlayState;
                playerData.Low_Level.IsBot                   = playerIsBot;
                playerData.Low_Level.SelectedCharacterNumber = BattlePlayerData.NoSelectedCharacter;

                for (int i = 0; i < Constants.BATTLE_PLAYER_CHARACTER_COUNT; i++)
                {
                    FPVector2 spawnPosition = BattleGridManager.GridPositionToWorldPosition(arenaPlayerSpec.DefaultCharacterSpawnPositions[i]);
                    playerData.Low_Level.CharacterDefaultSpawnPositions[i] = spawnPosition;
                }

                //} initialize slot
            }

            // log debug information
            s_debugLogger.LogFormat(f, "Expected players: {{ {0}, {1}, {2}, {3} }}", slotDebugStrings[0], slotDebugStrings[1], slotDebugStrings[2], slotDebugStrings[3]);
            s_debugLogger.LogFormat(f, "Expected player count: {0}", playerCount);

            // validate player count
            if (playerCountCheckNumber != playerCount)
            {
                OnScreenError(f, "BattleParameters player count does not match the number of player slots with type of Player\n"
                    + "BattleParameters player count {0}, Counted {1}",
                    playerCount,
                    playerCountCheckNumber
                );

                // this will prevent the game from starting
                dataRef.PlayerCount = -100;
            }
        }

        //{ player management

        /// <summary>
        /// Registers player.
        /// </summary>
        ///
        /// <param name="f">Current %Quantum %Frame.</param>
        /// <param name="playerRef">Reference to the player.</param>
        public static void RegisterPlayer(Frame f, PlayerRef playerRef)
        {
            // slot information
            string[]                      slotUserIDs = BattleParameters.GetPlayerSlotUserIDs(f);
            BattleParameters.PlayerType[] slotTypes   = BattleParameters.GetPlayerSlotTypes(f);

            Data.Ref dataRef = Data.GetRef(f);

            // incoming player information
            RuntimePlayer    quantumPlayerData     = f.GetPlayerData(playerRef);
            string           playerUserID          = quantumPlayerData.UserID;
            BattlePlayerSlot playerTargetSlot      = quantumPlayerData.PlayerSlot;
            int              playerTargetSlotIndex = Data.GetPlayerIndex(playerTargetSlot);

            // target slot information
            BattleParameters.PlayerType targetSlotType           = slotTypes[playerTargetSlotIndex];
            string                      targetSlotExpectedUserID = slotUserIDs[playerTargetSlotIndex];

            s_debugLogger.LogFormat(f, "Registering Player({0}) in {1}", playerUserID, playerTargetSlot);

            //{ validate player registration

            if (targetSlotType != BattleParameters.PlayerType.Player)
            {
                OnScreenError(f, "Player({0}) is trying to register to {1} which is of type {2}", playerUserID, playerTargetSlot, targetSlotType);
                return;
            }

            if (targetSlotExpectedUserID != playerUserID)
            {
                OnScreenError(f, "Player({0}) is trying to register to {1} which is expecting Player({2})", playerUserID, playerTargetSlot, targetSlotExpectedUserID);
                return;
            }

            //} validate player registration

            //{ register player

            BattlePlayerData.Ref playerData = Data.GetPlayerData(dataRef, playerTargetSlotIndex);

            playerData.Low_Level.PRef = playerRef;
            dataRef.PlayerCount++;

            //} register player

            f.Events.BattleViewPlayerConnected(quantumPlayerData);
        }

        /// <summary>
        /// Marks the player as abandoned.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerRef">Reference to the player.</param>
        public static void MarkPlayerAbandoned(Frame f, PlayerRef playerRef)
        {
            BattlePlayerData.Ref playerData = Data.GetPlayerData(Data.GetRef(f), playerRef);
            playerData.Low_Level.IsAbandoned = true;
            BattlePlayerQSystem.HandlePlayerAbandoned(f, playerData);
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
            return Data.GetRef(f).PlayerCount == BattleParameters.GetPlayerCount(f);
        }

        //} player management

        /// <summary>
        /// Creates all character entities for each player in the game, initializing data, hitboxes and view components.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public static void CreatePlayerCharacters(Frame f)
        {
            // get slot information
            BattleParameters.PlayerType[] slotTypes = BattleParameters.GetPlayerSlotTypes(f);

            Data.Ref dataRef = Data.GetRef(f);

            for (int playerIndex = 0; playerIndex < Constants.BATTLE_PLAYER_SLOT_COUNT; playerIndex++)
            {
                BattlePlayerData.Ref playerData = Data.GetPlayerData(dataRef, playerIndex);

                s_debugLogger.LogFormat(f, "({0}) Creating player, type: {1}", playerData.Slot, slotTypes[playerIndex]);

                if (slotTypes[playerIndex] == BattleParameters.PlayerType.None)
                {
                    s_debugLogger.LogFormat(f, "({0}) Skipping player creation, as type is None", playerData.Slot);
                    continue;
                }

                PlayerCreationParameters parameters = new();

                BattleCharacterBase[] battleBaseCharacters = !playerData.IsBot
                                                           ? f.GetPlayerData(playerData.PRef).Characters
                                                           : BattlePlayerBotController.GetBotCharacters(f);

                BattleEntityManager.CompoundEntityTemplate[] playerCharacterEntityArray = new BattleEntityManager.CompoundEntityTemplate[Constants.BATTLE_PLAYER_CHARACTER_COUNT];

                //{ create playerEntity for each character


                //{ set player parameters (used for all characters)

                if (playerData.Team == BattleTeamNumber.TeamAlpha)
                {
                    parameters.PlayerRotationBase = FP._0;
                    parameters.PlayerFlipped = false;
                }
                else
                {
                    parameters.PlayerRotationBase = FP.Rad_180;
                    parameters.PlayerFlipped = true;
                }

                //} set player parameters

                for (int playerCharacterNumber = 0; playerCharacterNumber < playerCharacterEntityArray.Length; playerCharacterNumber++)
                {
                    // set id and class
                    parameters.PlayerCharacterNumber = playerCharacterNumber;
                    parameters.PlayerCharacterId     = battleBaseCharacters[playerCharacterNumber].Id;
                    parameters.PlayerCharacterClass  = battleBaseCharacters[playerCharacterNumber].Class;
                    parameters.PlayerCharacterStats  = battleBaseCharacters[playerCharacterNumber].Stats;

                    s_debugLogger.LogFormat(f, "({0}) Creating character, number {1}\n" +
                                            "Character ID:    {2},\n" +
                                            "Character Class: {3}",
                                            playerData.Slot,
                                            parameters.PlayerCharacterNumber,
                                            parameters.PlayerCharacterId,
                                            parameters.PlayerCharacterClass
                    );

                    // get entity prototype
                    AssetRef<EntityPrototype> playerCharacterEntityPrototype = BattleAltzoneLink.GetCharacterPrototype(parameters.PlayerCharacterId);
                    if (playerCharacterEntityPrototype == null)
                    {
                        const int FallbackId = 0;

                        s_debugLogger.ErrorFormat(f, "({0}) Failed to fetch player character entity prototype ID {1}\nUsing fallback ID {2}", playerData.Slot, parameters.PlayerCharacterId, FallbackId);

                        parameters.PlayerCharacterId    = FallbackId;
                        parameters.PlayerCharacterClass = BattlePlayerCharacterClass.None;
                        playerCharacterEntityPrototype  = BattleAltzoneLink.GetCharacterPrototype(parameters.PlayerCharacterId);

                        s_debugLogger.LogFormat(f, "({0}) Creating fallback character, number {1}\n" +
                                                "Character ID:    {2},\n" +
                                                "Character Class: {3}",
                                                playerData.Slot,
                                                playerCharacterNumber,
                                                parameters.PlayerCharacterId,
                                                parameters.PlayerCharacterClass
                        );
                    }

                    // load class
                    BattlePlayerClassManager.LoadClass(f, parameters.PlayerCharacterClass);

                    // create entity
                    EntityRef playerCharacterEntityRef = f.Create(playerCharacterEntityPrototype);
                    parameters.PlayerCharacterEntityTemplate = BattleEntityManager.CompoundEntityTemplate.Create(playerCharacterEntityRef, 1);

                    // initialize entity
                    InitializePlayerCharacterEntity(f, playerData, parameters);

                    // save entity
                    playerCharacterEntityArray[playerCharacterNumber] = parameters.PlayerCharacterEntityTemplate;
                }

                //} create playerEntity for each character

                BattleEntityID characterEntityGroupID = BattleEntityManager.RegisterCompound(f, playerCharacterEntityArray);
                playerData.Low_Level.CharacterEntityGroupID = characterEntityGroupID;

                //{ post registration setup

                for (int playerCharacterNumber = 0; playerCharacterNumber < playerCharacterEntityArray.Length; playerCharacterNumber++)
                {
                    BattlePlayerHandle playerHandle = BattlePlayerHandle.Create(playerData);
                    playerHandle.LoadCharacter(f, playerCharacterNumber);

                    SetupPlayerCharacter(f, playerHandle);
                }

                //} post registration setup

                // set playerData for player
                playerData.Low_Level.StateAllowCharacterSwapping = true;
                playerData.Low_Level.StatePlayerGiveUp           = false;

                s_debugLogger.LogFormat(f, "({0}) Player created successfully", playerData.Slot);
            }
        }

        #region Public - Static Methods - Spawn/Despawn

        /// <summary>
        /// This documentation is not entirely up to date. Behavior varies depending on if <see cref="BattleParameters.IsTestFlipperGame"/> is on.<br/>
        /// Spawns a player character entity into the game. <br/>
        /// Verifies that the player is in the game and the character to be spawned is valid. <br/>
        /// Actual spawning handled by a separate private method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="slot">The slot of the player for which the character is to be spawned.</param>
        /// <param name="characterNumber">The character number of the character to be spawned.</param>
        /// <param name="select">Whether to set the spawned character as selected or not.</param>
        public static void SpawnPlayerCharacter(Frame f, BattlePlayerSlot slot, int characterNumber, bool select = false)
        {
            BattlePlayerCharacterState unSelectedCharacterState = BattleParameters.GetIsTestFlipperGame(f)
                                                                ? BattlePlayerCharacterState.InPlay
                                                                : BattlePlayerCharacterState.OutOfPlay;

            BattlePlayerClassManager.DespawnEventType unSelectedCharacterDespawnEventType = BattleParameters.GetIsTestFlipperGame(f)
                                                                                          ? BattlePlayerClassManager.DespawnEventType.UnSelect
                                                                                          : BattlePlayerClassManager.DespawnEventType.DespawnUnSelect;

            BattlePlayerHandle playerHandleNewCharacter = BattlePlayerHandle.Create(f,slot);
            playerHandleNewCharacter.LoadCharacter(f, characterNumber);

            if (playerHandleNewCharacter.PlayerData.PlayState.IsNotInGame())
            {
                s_debugLogger.Error(f, "Can not spawn player that is not in game");
                return;
            }

            if (!BattlePlayerData.IsValidCharacterNumber(characterNumber))
            {
                s_debugLogger.ErrorFormat(f, "Invalid characterNumber = {0}", characterNumber);
                return;
            }

            if (playerHandleNewCharacter.LoadedCharacterData->State == BattlePlayerCharacterState.OutOfPlayDead)
            {
                s_debugLogger.LogFormat(f, "Player character {0} is dead and will not be spawned", characterNumber);
                return;
            }

            int selectedCharacterNumber = playerHandleNewCharacter.PlayerData.SelectedCharacterNumber;

            BattlePlayerHandle? playerHandlePreviousCharacter = playerHandleNewCharacter.PlayerHasSelectedCharacter ? playerHandleNewCharacter.LoadCharacterCopy(f, selectedCharacterNumber) : null;

            bool spawn = !(playerHandleNewCharacter.LoadedCharacterData->State is BattlePlayerCharacterState.InPlay or BattlePlayerCharacterState.InPlaySelected);

            BattlePlayerClassManager.SpawnEventType spawnEventType = BattlePlayerClassManager.SpawnEventType.Spawn;

            if (spawn)
            {
                SpawnPlayerCharacter(f, playerHandleNewCharacter, playerHandlePreviousCharacter);
            }
            else
            {
                if (!select) return;
            }

            playerHandleNewCharacter.PlayerData.Low_Level.PlayState = BattlePlayerPlayState.InPlay;

            if (select)
            {
                if (playerHandlePreviousCharacter != null)
                {
                    playerHandlePreviousCharacter.Value.LoadedCharacterData->State = unSelectedCharacterState;

                    BattlePlayerClassManager.OnDespawn(f, BattlePlayerClassManager.DespawnEventType.UnSelect, playerHandlePreviousCharacter.Value);
                }

                playerHandleNewCharacter.PlayerData.Low_Level.SelectedCharacterNumber  = characterNumber;
                playerHandleNewCharacter.LoadedCharacterData->State = BattlePlayerCharacterState.InPlaySelected;

                spawnEventType = spawn ? BattlePlayerClassManager.SpawnEventType.SpawnSelect : BattlePlayerClassManager.SpawnEventType.Select;

                f.Events.BattleCharacterSelected(slot, characterNumber);
            }
            else
            {
                playerHandleNewCharacter.LoadedCharacterData->State = BattlePlayerCharacterState.InPlay;
            }

            BattlePlayerClassManager.OnSpawn(f, spawnEventType, playerHandleNewCharacter);
        }

        /// <summary>
        /// Despawns a player character entity from the game based on the given <paramref name="characterNumber"/>.<br/>
        /// Verifies that the player has a character in play.<br/>
        /// If <paramref name="kill"/> is set to true, the character's state is marked as dead prior to despawning.<br/>
        /// Actual despawning handled by a separate private method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="slot">The slot of the player for which the character is to be despawned.</param>
        /// <param name="characterNumber">Character number of the character being despawned.</param>
        /// <param name="kill">If true, marks the character as dead.</param>
        public static void DespawnPlayerCharacter(Frame f, BattlePlayerHandle playerHandle, int characterNumber, bool kill = false)
        {
            if (!playerHandle.PlayerData.PlayState.IsInPlay())
            {
                s_debugLogger.Error(f, "Can not despawn player that is not in play");
                return;
            }

            bool selected = playerHandle.LoadedCharacterData->State == BattlePlayerCharacterState.InPlaySelected;

            DespawnPlayerCharacter(f, playerHandle, characterNumber);

            BattlePlayerCharacterState state = kill ? BattlePlayerCharacterState.OutOfPlayDead : BattlePlayerCharacterState.OutOfPlay;

            playerHandle.LoadedCharacterData->State = state;

            if (selected)
            {
                playerHandle.PlayerData.Low_Level.PlayState = BattlePlayerPlayState.OutOfPlay;
                playerHandle.PlayerData.Low_Level.SelectedCharacterNumber = BattlePlayerData.NoSelectedCharacter;
                f.Events.BattleCharacterSelected(playerHandle.PlayerData.Slot, playerHandle.PlayerData.SelectedCharacterNumber);
            }
        }

        #endregion Public - Static Methods - Spawn/Despawn

        public static BattlePlayerSlot GetSlot(Frame f, PlayerRef playerRef) => Data.GetSlot(Data.GetRef(f), playerRef);

        public static BattlePlayerSlot GetTeammateSlot(BattlePlayerSlot slot) => Data.GetTeammateSlot(slot);

        public static BattleTeamNumber GetTeam(BattlePlayerSlot slot) => Data.GetTeam(slot);

        public static BattleTeamNumber GetOpposingTeam(BattleTeamNumber teamNumber) => Data.GetOpposingTeam(teamNumber);

        public static BattlePlayerData.Ref GetPlayerData(Frame f, BattlePlayerSlot slot) => Data.GetPlayerData(Data.GetRef(f), Data.GetPlayerIndex(slot));

        public static IEnumerable<BattlePlayerData.Ref> GetPlayerDataIterator(Frame f)
        {
            Data.Ref dataRef = Data.GetRef(f);

            for (int playerIndex = 0; playerIndex < Constants.BATTLE_PLAYER_SLOT_COUNT; playerIndex++)
            {
                yield return Data.GetPlayerData(dataRef, playerIndex);
            }
        }

        #endregion Public - Static Methods

        #endregion Public

        #region Private

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        /// <summary>Debug overlay entry number for stats.</summary>
        private static int s_debugOverlayStats;

        private static readonly FPVector2 s_noPreviousPosition = FPVector2.MaxValue;

        #region Private - Static Methods

        private struct PlayerCreationParameters
        {
            public FP PlayerRotationBase;

            public bool PlayerFlipped;

            public int PlayerCharacterNumber;

            public BattlePlayerCharacterID PlayerCharacterId;

            public BattlePlayerCharacterClass PlayerCharacterClass;

            public BattlePlayerStats PlayerCharacterStats;

            public BattleEntityManager.CompoundEntityTemplate PlayerCharacterEntityTemplate;
        }

        /// <summary>
        /// Private helper method for getting the BattlePlayerManagerDataQSingleton from the %Quantum %Frame.
        /// </summary>
        ///
        /// See [{PlayerManagerData}](#page-concepts-player-simulation-management-playermanagerdata) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>Pointer to the PlayerManagerData singleton.</returns>
        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BattlePlayerManagerDataQSingleton* GetPlayerManagerData(Frame f)
        {
            if (!f.Unsafe.TryGetPointerSingleton(out BattlePlayerManagerDataQSingleton* playerManagerData))
            {
                s_debugLogger.Error(f, "PlayerManagerData singleton not found!");
            }

            return playerManagerData;
        }*/

        private static void InitializePlayerCharacterEntity(Frame f, BattlePlayerData.Ref playerData, PlayerCreationParameters parameters)
        {
            int characterGridExtendTop;
            int characterGridExtendBottom;

            // get template data
            BattlePlayerCharacterDataTemplateQComponent* playerCharacterDataTemplate = f.Unsafe.GetPointer<BattlePlayerCharacterDataTemplateQComponent>(parameters.PlayerCharacterEntityTemplate.ParentEntityRef);
            int playerHitboxListCharacterColliderTemplateCount = f.TryResolveList(playerCharacterDataTemplate->Hitbox.ColliderTemplateList, out QList<BattlePlayerHitboxColliderTemplate> playerHitboxListCharacterColliderTemplate) ? playerHitboxListCharacterColliderTemplate.Count : 0;

            //{ set temp variables

            if (!parameters.PlayerFlipped)
            {
                characterGridExtendTop = playerCharacterDataTemplate->GridExtendTop;
                characterGridExtendBottom = playerCharacterDataTemplate->GridExtendBottom;
            }
            else
            {
                characterGridExtendTop = playerCharacterDataTemplate->GridExtendBottom;
                characterGridExtendBottom = playerCharacterDataTemplate->GridExtendTop;
            }

            //} set temp variables

            //{ create player hitBox

            // create hitBox entity
            EntityRef playerCharacterHitboxEntity = f.Create();
            if (playerHitboxListCharacterColliderTemplateCount <= 0)
            {
                playerCharacterHitboxEntity = EntityRef.None;
                return;
            }

            // set hitbox temp variables
            BattlePlayerCollisionType playerHitboxCollisionType = playerCharacterDataTemplate->Hitbox.CollisionType;
            FP playerHitboxAngleRad = FP.Deg2Rad * playerCharacterDataTemplate->Hitbox.NormalAngleDeg;

            //{ initialize hitBox collider

            PhysicsCollider2D playerHitboxCollider = PhysicsCollider2D.Create(f,
                shape: Shape2D.CreatePersistentCompound(),
                isTrigger: true
            );

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

            //} initialize hitBox collider

            // initialize collisionTrigger component
            BattleCollisionTriggerQComponent collisionTrigger = BattleCollisionQSystem.CreateCollisionTriggerComponent(BattleCollisionTriggerType.Player);

            // initialize hitBox component
            BattlePlayerHitboxQComponent playerHitbox = new()
            {
                ParentEntityRef = parameters.PlayerCharacterEntityTemplate.ParentEntityRef,
                HitboxType = BattlePlayerHitboxType.Character,
                CollisionType = playerHitboxCollisionType,
                NormalAngleRad = playerHitboxAngleRad,
                CollisionMinOffset = ((FP)playerHitboxHeight + FP._0_50) * BattleGridManager.GridScaleFactor
            };

            // initialize entity
            f.Add(playerCharacterHitboxEntity, playerHitbox);
            f.Add<Transform2D>(playerCharacterHitboxEntity);
            f.Add(playerCharacterHitboxEntity, playerHitboxCollider);
            f.Add(playerCharacterHitboxEntity, collisionTrigger);

            // link hitbox
            parameters.PlayerCharacterEntityTemplate.Link(playerCharacterHitboxEntity, new FPVector2(0, 0));
            BattlePlayerHandle.CreateLink(f, playerData.Slot, parameters.PlayerCharacterEntityTemplate.ParentEntityRef, PlayerEntityType.Hitbox, playerCharacterHitboxEntity);

            //} create player hitBox

            // create player shields
            int playerCharacterShieldCount = BattlePlayerShieldManager.CreateShields(f, playerData, parameters.PlayerCharacterNumber, parameters.PlayerCharacterId, parameters.PlayerCharacterClass, (BattlePlayerEntityRef)parameters.PlayerCharacterEntityTemplate.ParentEntityRef);

            //{ initialize playerData

            BattlePlayerCharacterDataQComponent playerCharacterData = new()
            {
                // player's ref's and IDs
                Number = parameters.PlayerCharacterNumber,
                Id = parameters.PlayerCharacterId,
                Class = parameters.PlayerCharacterClass,

                Stats = parameters.PlayerCharacterStats,

                // attributes
                AttributeGridExtendTop = characterGridExtendTop,
                AttributeGridExtendBottom = characterGridExtendBottom,
                AttributeDisableMovement = playerCharacterDataTemplate->DisableMovement,
                AttributeDisableRotation = playerCharacterDataTemplate->DisableRotation,
                AttributeSpawnBehaviour = playerCharacterDataTemplate->SpawnBehaviour,

                // state data
                StateMovementEnabled = !playerCharacterDataTemplate->DisableMovement,
                StateRotationEnabled = !playerCharacterDataTemplate->DisableRotation,
                StateDefenceValue = FP._0,

                // player's movement related data
                MovementTargetPosition = FPVector2.Zero,
                MovementRotationBaseRad = parameters.PlayerRotationBase,
                MovementRotationOffsetRad = FP._0,
                MovementPreviousInPlayPosition = s_noPreviousPosition,

                // player's shield related data
                ShieldCount = playerCharacterShieldCount,

                // bot related data
                BotMovementCooldownSec = FP._0,

                // view related data
                ViewPosition = f.Unsafe.GetPointer<Transform2D>(parameters.PlayerCharacterEntityTemplate.ParentEntityRef)->Position,
                ViewMovementVector = FPVector2.Zero
            };

#if DEBUG_PLAYER_STAT_OVERRIDE
                    s_debugLogger.Warning(f, "DEBUG_PLAYER_STAT_OVERRIDE enabled!");

                    playerData.Stats.Speed         = FP.FromString("20.0");
                    playerData.Stats.CharacterSize = FP.FromString("1.0");
                    playerData.Stats.Attack        = FP.FromString("1.0");
                    playerData.Stats.Defence       = FP.FromString("1.0");

                    s_debugLogger.WarningFormat("Using Speed {0} override",         playerData.Stats.Speed);
                    s_debugLogger.WarningFormat("Using CharacterSize {0} override", playerData.Stats.CharacterSize);
                    s_debugLogger.WarningFormat("Using Attack {0} override",        playerData.Stats.Attack);
                    s_debugLogger.WarningFormat("Using Defence {0} override",       playerData.Stats.Defence);
#endif
            playerCharacterData.StateDefenceValue = playerCharacterData.Stats.Defence;

            s_debugLogger.LogFormat(f, "({0}) Character number {1} stats:\n" +
                                    "Speed:         {2}\n" +
                                    "CharacterSize: {3}\n" +
                                    "Attack:        {4}\n" +
                                    "Defence:       {5}",
                                    playerData.Slot,
                                    parameters.PlayerCharacterNumber,
                                    playerCharacterData.Stats.Speed,
                                    playerCharacterData.Stats.CharacterSize,
                                    playerCharacterData.Stats.Attack,
                                    playerCharacterData.Stats.Defence
                                    );

            //} initialize playerData

            // initialize entity
            f.Remove<BattlePlayerCharacterDataTemplateQComponent>(parameters.PlayerCharacterEntityTemplate.ParentEntityRef);
            f.Add(parameters.PlayerCharacterEntityTemplate.ParentEntityRef, playerCharacterData, out BattlePlayerCharacterDataQComponent* playerDataPtr);

            BattlePlayerHandle.CreateLink(f, playerData.Slot, parameters.PlayerCharacterEntityTemplate.ParentEntityRef, PlayerEntityType.Character, parameters.PlayerCharacterEntityTemplate.ParentEntityRef);
        }

        private static void SetupPlayerCharacter(Frame f, BattlePlayerHandle playerHandle)
        {
            DevAssertPlayerHandleHasCharacterLoaded(f, playerHandle, errorContext: "Can not setup player");

            BattlePlayerClassManager.SetupParameters creationParameters = BattlePlayerClassManager.OnCreate(f, playerHandle);

            // attach shield
            if (creationParameters.AttachedShieldNumber >= 0)
            {
                BattlePlayerShieldManager.AttachShield(f, playerHandle, playerHandle.LoadedCharacterData->Number, creationParameters.AttachedShieldNumber, teleport: false);
            }

            // set playerManagerData for player character
            BattlePlayerCharacterState playerCharacterState = playerHandle.LoadedCharacterData->Stats.Defence > 0 ? BattlePlayerCharacterState.OutOfPlay : BattlePlayerCharacterState.OutOfPlayDead;
            playerHandle.LoadedCharacterData->State = playerCharacterState;

            // initialize view
            f.Events.BattlePlayerCharacterViewInit(playerHandle.LoadedCharacterEntityRef, playerHandle.PlayerData.Slot, playerHandle.LoadedCharacterData->Id, playerHandle.LoadedCharacterData->Class, playerHandle.LoadedCharacterData->ShieldCount, BattleGridManager.GridScaleFactor);
        }

        /// <summary>
        /// Spawns a player character entity into the game based on the given <paramref name="characterNumber"/>.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">PlayerHandle of the player the character will be spawned for.</param>
        /// <param name="characterNumber">The character number of the character to be spawned.</param>
        private static void SpawnPlayerCharacter(Frame f, BattlePlayerHandle playerHandle, BattlePlayerHandle? playerHandlePreviousCharacter)
        {
            DevAssertPlayerHandleHasCharacterLoaded(f, playerHandle, errorContext: "Can not spawn player");

            FPVector2 worldPosition = BattleParameters.GetIsTestFlipperGame(f)
                                    ? playerHandle.PlayerData.Low_Level.CharacterDefaultSpawnPositions[playerHandle.LoadedCharacterData->Number]
                                    : playerHandle.PlayerData.Low_Level.CharacterDefaultSpawnPositions[1];

            BattlePlayerSpawnBehaviour spawnBehaviour = BattleParameters.GetIsTestFlipperGame(f) ? BattlePlayerSpawnBehaviour.DefaultPosition : playerHandle.LoadedCharacterData->AttributeSpawnBehaviour;

            switch (spawnBehaviour)
            {
                case BattlePlayerSpawnBehaviour.DefaultPosition:
                    break;
                case BattlePlayerSpawnBehaviour.CharactersPreviousPosition:
                    if (playerHandle.LoadedCharacterData->MovementPreviousInPlayPosition != s_noPreviousPosition)
                    {
                        worldPosition = playerHandle.LoadedCharacterData->MovementPreviousInPlayPosition;
                    }
                    break;
                case BattlePlayerSpawnBehaviour.PreviousCharactersPosition:
                    if (playerHandlePreviousCharacter != null)
                    {
                        DevAssertPlayerHandleHasCharacterLoaded(f, playerHandlePreviousCharacter.Value, errorContext: "Can't read previous character");
                        worldPosition = playerHandlePreviousCharacter.Value.LoadedCharacterTransform->Position;
                    }
                    break;
            }

            if (playerHandlePreviousCharacter != null && !BattleParameters.GetIsTestFlipperGame(f))
            {
                DespawnPlayerCharacter(f, playerHandlePreviousCharacter.Value);
            }

            s_debugLogger.LogFormat(f, "({0}) Spawning character number: {1}", playerHandle.PlayerData.Slot, playerHandle.LoadedCharacterData->Number);

            // update player data
            playerHandle.LoadedCharacterData->AbilityCooldownSec       = FrameTimer.FromSeconds(f, FP._3);
            playerHandle.LoadedCharacterData->AbilityActivateBufferSec = FrameTimer.FromSeconds(f, FP._0);
            playerHandle.LoadedCharacterData->ViewPosition             = worldPosition;

            // update shield if attached
            if (playerHandle.LoadedCharacterHasShieldAttached)
            {
                BattlePlayerCharacterShieldHandle shieldHandle = playerHandle.GetLoadedCharacterAttachedShield(f);

                f.Events.BattlePlayStateUpdate(shieldHandle.ShieldEntityRef, true);
                f.Events.BattleShieldChangeState(playerHandle.LoadedCharacterEntityRef, playerHandle.PlayerData.Team, ShieldAttached: true, shieldHandle.ShieldData->ShieldNumber);
            }

            BattlePlayerMovementController.Teleport(f, playerHandle.LoadedCharacterData, (BattlePlayerEntityRef)playerHandle.LoadedCharacterEntityRef, worldPosition);

            // update debug overlay
            BattleDebugOverlayLink.SetEntries(playerHandle.PlayerData.Slot, s_debugOverlayStats, new object[]
            {
                playerHandle.LoadedCharacterData->Stats.Speed,
                playerHandle.LoadedCharacterData->Stats.CharacterSize,
                playerHandle.LoadedCharacterData->Stats.Attack,
                playerHandle.LoadedCharacterData->Stats.Defence
            });
        }

        /// <summary>
        /// Despawns a player character entity from the game based on the given <paramref name="characterNumber"/>.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">PlayerHandle of the player the character will be spawned for.</param>
        /// <param name="characterNumber">Character number of the character being despawned.</param>
        private static void DespawnPlayerCharacter(Frame f, BattlePlayerHandle playerHandle)
        {
            DevAssertPlayerHandleHasCharacterLoaded(f, playerHandle, errorContext: "Can not despawn player");

            s_debugLogger.LogFormat(f, "({0}) Despawning character number: {1}", playerHandle.PlayerData.Slot, playerHandle.LoadedCharacterData->Number);

            playerHandle.LoadedCharacterData->MovementPreviousInPlayPosition = playerHandle.LoadedCharacterTransform->Position;

            BattlePlayerClassManager.OnDespawn(f, BattlePlayerClassManager.DespawnEventType.DespawnUnSelect, playerHandle);

            BattleEntityManager.Return(f, playerHandle.PlayerData.Low_Level.CharacterEntityGroupID, playerHandle.LoadedCharacterData->Number);

            // return shield if attached
            if (playerHandle.LoadedCharacterData->AttachedShieldEntityRef != EntityRef.None)
            {
                BattlePlayerCharacterShieldHandle shieldHandle = playerHandle.GetLoadedCharacterAttachedShield(f);

                BattleEntityManager.Return(f, playerHandle.PlayerData.Low_Level.PlayerShieldEntityGroupIDs[playerHandle.PlayerData.SelectedCharacterNumber]);
            }

            // update data
            playerHandle.LoadedCharacterData->ViewPosition = playerHandle.LoadedCharacterTransform->Position;
        }

        private static void OnScreenError(Frame f, string messageformat, params object[] args)
        {
            string message = string.Format(messageformat, args);
            s_debugLogger.Error(f, message);
            f.Events.BattleDebugOnScreenMessage(message);
        }

        private static void DevAssertPlayerHandleHasCharacterLoaded(Frame f, BattlePlayerHandle playerHandle, string errorContext)
        {
            s_debugLogger.DevAssertFormat(f, playerHandle.HandleHasCharacterLoaded, "{0}, PlayerHandle has no loaded character", errorContext);
        }

        #endregion Private - Static Methods

        #endregion Private
    }
}

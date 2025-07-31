#define DEBUG_PLAYER_STAT_OVERRIDE

using System.Runtime.CompilerServices;
using UnityEngine;

using Quantum;
using Quantum.Collections;
using Photon.Deterministic;

using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
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

                EntityRef[] playerCharacterEntityArray = new EntityRef[Constants.BATTLE_PLAYER_CHARACTER_COUNT];

                // create playerEntity for each characters
                {
                    //{ player temp variables
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

                                    collisionTrigger.Type = BattleCollisionTriggerType.Player;
                                    break;

                                default:
                                    playerHitboxType = (BattlePlayerHitboxType)(-1);
                                    playerHitboxCollisionType = (BattlePlayerCollisionType)(-1);
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
                                PlayerEntity = playerEntity,
                                HitboxType = playerHitboxType,
                                CollisionType = playerHitboxCollisionType,
                                Normal = FPVector2.Zero,
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
                            CharacterClass    = data.Characters[playerCharacterNumber].Class,

                            Stats             = data.Characters[playerCharacterNumber].Stats,

                            GridExtendTop     = playerGridExtendTop,
                            GridExtendBottom  = playerGridExtendBottom,

                            TargetPosition    = playerSpawnPosition,
                            RotationBase      = playerRotationBase,
                            RotationOffset    = FP._0,

                            CurrentHp         = data.Characters[playerCharacterNumber].Stats.Hp,

                            HitboxShieldEntity      = playerHitboxShieldEntity,
                            HitboxCharacterEntity   = playerHitboxCharacterEntity
                        };

#if DEBUG_PLAYER_STAT_OVERRIDE
                        playerData.Stats.Hp            = FP.FromString("3.0");
                        playerData.Stats.Speed         = FP.FromString("20.0");
                        playerData.Stats.CharacterSize = FP.FromString("1.0");
                        playerData.Stats.Attack        = FP.FromString("1.0");
                        playerData.Stats.Defence       = FP.FromString("1.0");
#endif
                        playerData.CurrentHp = playerData.Stats.Hp;

                        //{ initialize playerData

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

        public struct PlayerHandle
        {
            //{ Public Static Methods

            public static BattlePlayerSlot GetSlot(Frame f, PlayerRef playerRef)
            {
                BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);
                int playerIndex = PlayerHandleInternal.GetPlayerIndex(playerManagerData, playerRef);
                return PlayerHandleInternal.GetSlot(playerIndex);
            }

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

            public BattlePlayerPlayState PlayState
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->PlayStates[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _playerManagerData->PlayStates[Index] = value;
            }

            public PlayerRef PlayerRef
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->PlayerRefs[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _playerManagerData->PlayerRefs[Index] = value;
            }

            public EntityRef SelectedCharacter
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacters[Index]; }

            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacterNumbers[Index]; }

            public FPVector2 SpawnPosition
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => s_spawnPoints[Index]; }

            //} Public Properties

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PlayerHandleInternal(BattlePlayerManagerDataQSingleton* playerManagerData, int playerIndex)
            {
                Index = playerIndex;
                _playerManagerData = playerManagerData;
            }

            //{ Public Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityRef GetCharacter(int characterNumber) => _playerManagerData->AllCharacters[GetCharacterIndex(characterNumber)];

            public void SetCharacters(EntityRef[] entityRefArray)
            {
                int characterOffset = GetCharacterOffset();
                for (int i = 0; i < Constants.BATTLE_PLAYER_CHARACTER_COUNT; i++)
                {
                    _playerManagerData->AllCharacters[characterOffset + i] = entityRefArray[i];
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetSelectedCharacter(int characterNumber)
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = characterNumber;
                _playerManagerData->SelectedCharacters[Index] = _playerManagerData->AllCharacters[GetCharacterIndex(characterNumber)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsetSelectedCharacter()
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = -1;
                _playerManagerData->SelectedCharacters[Index] = EntityRef.None;
            }

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
            private BattlePlayerManagerDataQSingleton* _playerManagerData;
            //} Private Fields

            //{ Private Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)] private int GetCharacterOffset() => Index * Constants.BATTLE_PLAYER_CHARACTER_COUNT;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private int GetCharacterIndex(int characterNumber) => GetCharacterOffset() + characterNumber;

            //} Private Methods
        }

        #endregion Private - PlayerHandleInternal struct

        #region Private - Static Methods

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

            if (playerData->CurrentHp <= 0)
            {
                Debug.LogFormat("[PlayerManager] Player character {0} has no Hp and will not be spawned", characterNumber);
                return;
            }

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
            f.Events.BattleDebugUpdateStatsOverlay(playerData->Slot, playerData->Stats);

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

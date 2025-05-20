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
        }

        public static BattlePlayerSlot InitPlayer(Frame f, PlayerRef playerRef)
        {
            BattlePlayerManagerDataQSingleton* playerManagerData = GetPlayerManagerData(f);

            RuntimePlayer data = f.GetPlayerData(playerRef);

            BattlePlayerSlot playerSlot = data.PlayerSlot;
            BattleTeamNumber teamNumber = PlayerHandleInternal.GetTeamNumber(playerSlot);
            PlayerHandleInternal playerHandle = PlayerHandleInternal.GetPlayerHandle(playerManagerData, playerSlot);

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

                for (int i = 0; i < playerCharacterEntityArray.Length; i++)
                {

                    // create entity
                    playerEntity = f.Create(entityPrototypeAsset);

                    // get template data
                    playerDataTemplate                     = f.Unsafe.GetPointer<BattlePlayerDataTemplateQComponent>(playerEntity);
                    playerHitboxListShieldTemplateCount    = f.TryResolveList(playerDataTemplate->HitboxListShield,    out playerHitboxListShieldTemplate   ) ? playerHitboxListShieldTemplate    .Count : 0;
                    playerHitboxListCharacterTemplateCount = f.TryResolveList(playerDataTemplate->HitboxListCharacter, out playerHitboxListCharacterTemplate) ? playerHitboxListCharacterTemplate .Count : 0;

                    //{ set temp variables

                    playerSpawnPosition = playerHandle.GetOutOfPlayPosition(i, teamNumber);

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
                        CharacterId         = data.Characters[i].Id,
                        CharacterClass      = data.Characters[i].Class,

                        StatHp              = data.Characters[i].Hp,
                        StatSpeed           = data.Characters[i].Speed,
                        StatCharacterSize   = data.Characters[i].CharacterSize,
                        StatAttack          = data.Characters[i].Attack,
                        StatDefence         = data.Characters[i].Defence,

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
                    f.Events.BattlePlayerViewInit(playerEntity, BattleGridManager.GridScaleFactor);

                    // save entity
                    playerCharacterEntityArray[i] = playerEntity;
                }
            }

            // set playerManagerData for player
            playerHandle.PlayState = BattlePlayerPlayState.OutOfPlay;
            playerHandle.PlayerRef = playerRef;
            playerHandle.SetCharacters(playerCharacterEntityArray);

            return playerSlot;
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
                        column = 10 * Index;
                        break;
                    case BattleTeamNumber.TeamBeta:
                        row    = BattleGridManager.Rows - 1 + 10 * (characterNumber + 1);
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

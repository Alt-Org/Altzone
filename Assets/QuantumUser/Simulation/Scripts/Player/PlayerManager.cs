#define DEBUG_PLAYER_STAT_OVERRIDE

using System;
using System.Runtime.CompilerServices;

using UnityEngine;
using Photon.Deterministic;
using Quantum.Collections;

namespace Quantum
{
    public static unsafe class PlayerManager
    {
        #region Public

        #region Public - Static Methods

        public static void Init(Frame f)
        {
            Debug.Log("[PlayerManager] Init");

            BattleArenaSpec battleArenaSpec = f.FindAsset(f.RuntimeConfig.BattleArenaSpec);
            for (int i = 0; i < s_spawnPoints.Length; i++)
            {
                s_spawnPoints[i] = GridManager.GridPositionToWorldPosition(battleArenaSpec.PlayerSpawnPositions[i]);
            }

            PlayerManagerData* playerManagerData = GetPlayerManagerData(f);

            PlayerHandle.SetAllPlayStates(playerManagerData, PlayerPlayState.NotInGame);
        }

        public static BattlePlayerSlot InitPlayer(Frame f, PlayerRef playerRef)
        {
            PlayerManagerData* playerManagerData = GetPlayerManagerData(f);

            RuntimePlayer data = f.GetPlayerData(playerRef);

            BattlePlayerSlot playerSlot = (BattlePlayerSlot)data.PlayerSlot;
            BattleTeamNumber teamNumber = PlayerHandle.GetTeamNumber(playerSlot);
            PlayerHandle playerHandle = new(playerManagerData, playerSlot);

            // TODO: Fetch EntityPrototype for each character based on the BattleCharacterBase Id
            EntityPrototype entityPrototypeAsset = f.FindAsset(data.PlayerAvatar);

            EntityRef[] playerCharacterEntityArray = new EntityRef[Constants.PLAYER_CHARACTER_COUNT];

            // create playerEntity for each characters
            {
                //{ player temp variables
                PlayerDataTemplate* playerDataTemplate;
                FPVector2           playerSpawnPosition;
                FP                  playerRotationBase;
                // player - hitBox temp variables
                QList<PlayerHitboxTemplate> playerHitboxListShieldTemplate;
                QList<PlayerHitboxTemplate> playerHitboxListCharacterTemplate;
                QList<PlayerHitboxTemplate> playerHitboxListSourceTemplate;
                int                         playerHitboxListShieldTemplateCount;
                int                         playerHitboxListCharacterTemplateCount;
                QList<PlayerHitboxLink>     playerHitboxListTarget;
                PlayerHitboxType            playerHitboxType;
                FPVector2                   playerHitboxPosition;
                FP                          playerHitboxExtents;
                //} player temp variables

                //{ set player common temp variables (used for all characters)

                playerHitboxExtents = GridManager.GridScaleFactor * FP._0_50;

                playerRotationBase = teamNumber == BattleTeamNumber.TeamAlpha ? FP._0 : FP.Rad_180;

                //} set player common temp variables

                //{ player variables
                EntityRef    playerEntity;
                PlayerData   playerData;
                Transform2D* playerTransform;
                // player - hitBox variables
                QList<PlayerHitboxLink> playerHitboxListAll;
                QList<PlayerHitboxLink> playerHitboxListShield;
                QList<PlayerHitboxLink> playerHitboxListCharacter;
                PlayerHitboxLink        playerHitboxLink;
                EntityRef               playerHitboxEntity;
                PlayerHitbox            playerHitbox;
                PhysicsCollider2D       playerHitboxCollider;
                //} player variables

                for (int i = 0; i < playerCharacterEntityArray.Length; i++)
                {
                    // set spawnPosition
                    playerSpawnPosition = playerHandle.GetOutOfPlayPosition(i, teamNumber);

                    // create entity
                    playerEntity = f.Create(entityPrototypeAsset);

                    // get template data
                    playerDataTemplate                     = f.Unsafe.GetPointer<PlayerDataTemplate>(playerEntity);
                    playerHitboxListShieldTemplateCount    = f.TryResolveList(playerDataTemplate->HitboxListShield,    out playerHitboxListShieldTemplate   ) ? playerHitboxListShieldTemplate    .Count : 0;
                    playerHitboxListCharacterTemplateCount = f.TryResolveList(playerDataTemplate->HitboxListCharacter, out playerHitboxListCharacterTemplate) ? playerHitboxListCharacterTemplate .Count : 0;

                    //{ allocate playerHitboxLists

                    if (playerHitboxListShieldTemplateCount + playerHitboxListCharacterTemplateCount > 0) playerHitboxListAll       = f.AllocateList<PlayerHitboxLink>(playerHitboxListShieldTemplateCount + playerHitboxListCharacterTemplateCount);
                    if (playerHitboxListShieldTemplateCount                                          > 0) playerHitboxListShield    = f.AllocateList<PlayerHitboxLink>(playerHitboxListShieldTemplateCount                                         );
                    if (                                      playerHitboxListCharacterTemplateCount > 0) playerHitboxListCharacter = f.AllocateList<PlayerHitboxLink>(                                      playerHitboxListCharacterTemplateCount);

                    //} allocate playerHitboxLists

                    // initialize playerData
                    playerData = new PlayerData
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
                                playerHitboxType = PlayerHitboxType.Shield;
                                playerHitboxListSourceTemplate = playerHitboxListShieldTemplate;
                                playerHitboxListTarget = playerHitboxListShield;
                                break;

                            case 1:
                                if (playerHitboxListCharacterTemplateCount <= 0) continue;
                                playerHitboxType = PlayerHitboxType.Character;
                                playerHitboxListSourceTemplate = playerHitboxListCharacterTemplate;
                                playerHitboxListTarget = playerHitboxListCharacter;
                                break;

                            default:
                                playerHitboxType = (PlayerHitboxType)(-1);
                                break;
                        }

                        foreach (PlayerHitboxTemplate playerHitboxTemplate in playerHitboxListSourceTemplate)
                        {
                            // initialize hitBox component
                            playerHitbox = new PlayerHitbox
                            {
                                PlayerEntity       = playerEntity,
                                HitboxType         = playerHitboxType,
                                CollisionType      = playerHitboxTemplate.CollisionType,
                                Normal             = FPVector2.Rotate(FPVector2.Down, playerRotationBase - playerHitboxTemplate.NormalAngle * FP.Deg2Rad),
                                CollisionMinOffset = playerHitboxExtents
                            };

                            // initialize hitBox position
                            playerHitboxPosition = new FPVector2(
                                (FP)playerHitboxTemplate.Position.X * GridManager.GridScaleFactor,
                                (FP)playerHitboxTemplate.Position.Y * GridManager.GridScaleFactor
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

                            // create hitBox link
                            playerHitboxLink = new PlayerHitboxLink
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

                    f.Remove<PlayerDataTemplate>(playerEntity);
                    f.Add(playerEntity, playerData, out PlayerData* playerDataPtr);

                    playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
                    PlayerMovementSystem.Teleport(f, playerDataPtr, playerTransform,
                        playerSpawnPosition,
                        playerRotationBase
                    );

                    //} initialize entity

                    // initialize view
                    f.Events.PlayerViewInit(playerEntity, GridManager.GridScaleFactor);

                    // save entity
                    playerCharacterEntityArray[i] = playerEntity;
                }
            }

            // set playerManagerData for player
            playerHandle.PlayState = PlayerPlayState.OutOfPlay;
            playerHandle.PlayerRef = playerRef;
            playerHandle.SetCharacters(playerCharacterEntityArray);

            return playerSlot;
        }

        #region Public - Static Methods - Spawn/Despawn

        public static void SpawnPlayer(Frame f, BattlePlayerSlot slot, int characterNumber)
        {
            PlayerHandle playerHandle = new(GetPlayerManagerData(f), slot);

            if (playerHandle.PlayState == PlayerPlayState.NotInGame)
            {
                Debug.LogError("[PlayerManager] Can not spawn player that is not in game");
                return;
            }

            if (!PlayerHandle.IsValidCharacterNumber(characterNumber))
            {
                Debug.LogErrorFormat("[PlayerManager] Invalid characterNumber = {0}", characterNumber);
                return;
            }

            SpawnPlayer(f, playerHandle, characterNumber);
        }

        public static void DespawnPlayer(Frame f, BattlePlayerSlot slot)
        {
            PlayerHandle playerHandle = new(GetPlayerManagerData(f), slot);

            if (playerHandle.PlayState != PlayerPlayState.InPlay)
            {
                Debug.LogError("[PlayerManager] Can not despawn player that is not in play");
                return;
            }

            DespawnPlayer(f, playerHandle);
        }

        #endregion Public - Static Methods - Spawn/Despawn

        #region Public - Static Methods - Utility

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleTeamNumber GetPlayerTeamNumber(BattlePlayerSlot slot) => PlayerHandle.GetTeamNumber(slot);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PlayerPlayState GetPlayerPlayState(Frame f, BattlePlayerSlot slot)
        {
            PlayerManagerData* playerManagerData = GetPlayerManagerData(f);
            int playerIndex = PlayerHandle.GetPlayerIndex(slot);
            return PlayerHandle.GetPlayState(playerManagerData, playerIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityRef GetPlayerEntity(Frame f, BattlePlayerSlot slot)
        {
            PlayerManagerData* playerManagerData = GetPlayerManagerData(f);
            int playerIndex = PlayerHandle.GetPlayerIndex(slot);
            return PlayerHandle.GetSelectedCharacter(playerManagerData, playerIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityRef GetTeammateEntity(Frame f, BattlePlayerSlot slot)
        {
            PlayerManagerData* playerManagerData = GetPlayerManagerData(f);
            int teammatePlayerIndex = PlayerHandle.GetTeammatePlayerIndex(slot);
            return PlayerHandle.GetSelectedCharacter(playerManagerData, teammatePlayerIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidCharacterNumber(int characterNumber) => PlayerHandle.IsValidCharacterNumber(characterNumber);

        #endregion Public - Static Methods - Utility

        #endregion Public - Static Methods

        #endregion Public

        #region Private

        private static readonly FPVector2[] s_spawnPoints = new FPVector2[Constants.PLAYER_SLOT_COUNT];

        private struct PlayerHandle
        {
            //{ Public Static Methods

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
            public static PlayerPlayState GetPlayState(PlayerManagerData* playerManagerData, int playerIndex) => playerManagerData->PlayStates[playerIndex];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void SetAllPlayStates(PlayerManagerData* playerManagerData, PlayerPlayState playerPlayState)
            {
                for (int i = 0; i < Constants.PLAYER_SLOT_COUNT; i++)
                {
                    playerManagerData->PlayStates[i] = playerPlayState;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsValidCharacterNumber(int characterNumber) => characterNumber >= 0 && characterNumber < Constants.PLAYER_CHARACTER_COUNT;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static EntityRef GetSelectedCharacter(PlayerManagerData* playerManagerData, int playerIndex) => playerManagerData->SelectedCharacters[playerIndex];

            //} Public Static Methods

            //{ Public Properties

            public int Index { get; private set; }

            public PlayerPlayState PlayState
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetPlayState(_playerManagerData, Index);
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _playerManagerData->PlayStates[Index] = value;
            }

            public PlayerRef PlayerRef
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->PlayerRefs[Index];
                [MethodImpl(MethodImplOptions.AggressiveInlining)] set => _playerManagerData->PlayerRefs[Index] = value;
            }

            public EntityRef SelectedCharacter
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetSelectedCharacter(_playerManagerData, Index); }

            public int SelectedCharacterNumber
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _playerManagerData->SelectedCharacterNumbers[Index]; }

            public FPVector2 SpawnPosition
            { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => s_spawnPoints[Index]; }

            //} Public Properties

            public PlayerHandle(PlayerManagerData* playerManagerData, BattlePlayerSlot slot)
            {
                Index = GetPlayerIndex(slot);
                _playerManagerData = playerManagerData;
            }

            //{ Public Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityRef GetCharacter(int characterNumber) => _playerManagerData->AllCharacters[GetCharacterIndex(characterNumber)];

            public void SetCharacters(EntityRef[] entityRefArray)
            {
                int characterOffset = GetCharacterOffset();
                for (int i = 0; i < Constants.PLAYER_CHARACTER_COUNT; i++)
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
                        row = 0 - 10 * (characterNumber + 1);
                        column = 10 * Index;
                        break;
                    case BattleTeamNumber.TeamBeta:
                        row = GridManager.Rows - 1 + 10 * (characterNumber + 1);
                        column = GridManager.Columns - 1 - 10 * (Index - 2);
                        break;

                }

                return new FPVector2
                (
                    GridManager.GridColToWorldXPosition(column),
                    GridManager.GridRowToWorldYPosition(row)
                );
            }

            //} Public Methods

            //{ Private Fields
            private PlayerManagerData* _playerManagerData;
            //} Private Fields

            //{ Private Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)] private int GetCharacterOffset() => Index * Constants.PLAYER_CHARACTER_COUNT;
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private int GetCharacterIndex(int characterNumber) => GetCharacterOffset() + characterNumber;

            //} Private Methods
        }

        #region Private - Static Methods

        private static PlayerManagerData* GetPlayerManagerData(Frame f)
        {
            PlayerManagerData* playerManagerData;
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

        private static void SpawnPlayer(Frame f, PlayerHandle playerHandle, int characterNumber)
        {
            EntityRef character = playerHandle.GetCharacter(characterNumber);
            PlayerData* playerData = f.Unsafe.GetPointer<PlayerData>(character);
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(character);

            FPVector2 worldPosition;

            if (playerHandle.PlayState == PlayerPlayState.InPlay)
            {
                worldPosition = f.Unsafe.GetPointer<Transform2D>(playerHandle.SelectedCharacter)->Position;
                DespawnPlayer(f, playerHandle);
            }
            else
            {
                worldPosition = playerHandle.SpawnPosition;
            }

            playerData->PlayerRef = playerHandle.PlayerRef;

            PlayerMovementSystem.Teleport(f, playerData, playerTransform,
                worldPosition,
                playerData->RotationBase
            );

            playerData->TargetPosition = worldPosition;

            playerHandle.SetSelectedCharacter(characterNumber);
            playerHandle.PlayState = PlayerPlayState.InPlay;
        }

        private static void DespawnPlayer(Frame f, PlayerHandle playerHandle)
        {
            EntityRef selectedCharacter = playerHandle.SelectedCharacter;
            PlayerData* playerData = f.Unsafe.GetPointer<PlayerData>(selectedCharacter);
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(selectedCharacter);

            FPVector2 worldPosition = playerHandle.GetOutOfPlayPosition(playerHandle.SelectedCharacterNumber, playerData->TeamNumber);

            playerData->PlayerRef = PlayerRef.None;

            PlayerMovementSystem.Teleport(f, playerData, playerTransform,
                worldPosition,
                playerData->RotationBase
            );

            playerData->TargetPosition = worldPosition;

            playerHandle.UnsetSelectedCharacter();
            playerHandle.PlayState = PlayerPlayState.OutOfPlay;
        }

        #endregion Private - Static Methods

        #endregion Private
    }
}

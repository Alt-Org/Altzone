using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public static unsafe class PlayerManager
    {
        private struct PlayerHandle
        {
            public int Index { get; private set; }

            public PlayerPlayState PlayState
            {
                get => _playerManagerData->PlayStates[Index];
                set => _playerManagerData->PlayStates[Index] = value;
            }

            public PlayerRef PlayerRef {
                get => _playerManagerData->PlayerRefs[Index];
                set => _playerManagerData->PlayerRefs[Index] = value;
            }

            public EntityRef SelectedCharacter => _playerManagerData->SelectedCharacters[Index];
            public int SelectedCharacterNumber => _playerManagerData->SelectedCharacterNumbers[Index];

            public PlayerHandle(PlayerManagerData* playerManagerData, BattlePlayerSlot slot)
            {
                Index = GetPlayerIndex(slot);
                _playerManagerData = playerManagerData;
            }

            public static int GetPlayerIndex(BattlePlayerSlot playerSlot)
            {
                return playerSlot switch
                {
                    BattlePlayerSlot.Slot1 => 0,
                    BattlePlayerSlot.Slot2 => 1,
                    BattlePlayerSlot.Slot3 => 2,
                    BattlePlayerSlot.Slot4 => 3,

                    _ => -1
                };
            }

            public void SetSelectedCharacter(int characterNumber)
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = characterNumber;
                _playerManagerData->SelectedCharacters[Index] = _playerManagerData->AllCharacters[GetCharacterIndex(characterNumber)];
            }
            public void UnsetSelectedCharacter()
            {
                _playerManagerData->SelectedCharacterNumbers[Index] = -1;
                _playerManagerData->SelectedCharacters[Index] = EntityRef.None;
            }

            public static void SetAllPlayStates(PlayerManagerData* playerManagerData, PlayerPlayState playerPlayState)
            {
                for (int i = 0; i < Constants.PLAYER_SLOT_COUNT; i++)
                {
                    playerManagerData->PlayStates[i] = playerPlayState;
                }
            }

            private int GetCharacterOffset() => Index * Constants.PLAYER_CHARACTER_COUNT;
            private int GetCharacterIndex(int characterNumber) => GetCharacterOffset() + characterNumber;
            public EntityRef GetCharacter(int characterNumber) => _playerManagerData->AllCharacters[GetCharacterIndex(characterNumber)];


            public void SetCharacters(EntityRef[] entityRefArray)
            {
                int characterOffset = GetCharacterOffset();
                for (int i = 0; i < Constants.PLAYER_CHARACTER_COUNT; i++)
                {
                    _playerManagerData->AllCharacters[characterOffset + i] = entityRefArray[i];
                }
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

            private PlayerManagerData* _playerManagerData;
           
        }

        public static void Init(Frame f)
        {

            Debug.Log("[PlayerManager] Init");

            PlayerManagerData* playerManagerData = GetPlayerManagerData(f);

            PlayerHandle.SetAllPlayStates(playerManagerData, PlayerPlayState.NotInGame);
        }


        public static void InitPlayer(Frame f, PlayerRef player)
        {
            PlayerManagerData* playerManagerData = GetPlayerManagerData(f);
          
            RuntimePlayer data = f.GetPlayerData(player);

            BattlePlayerSlot playerSlot = (BattlePlayerSlot)data.PlayerSlot;
            BattleTeamNumber teamNumber = GetPlayerTeamNumber(playerSlot);
            PlayerHandle playerHandle = new PlayerHandle(playerManagerData, playerSlot);
            

            // TODO: Fetch EntityPrototype for each character based on the BattleCharacterBase Id
            EntityPrototype entityPrototypeAsset = f.FindAsset(data.PlayerAvatar);
            EntityRef[] playerEntityArray = new EntityRef[Constants.PLAYER_CHARACTER_COUNT];


            FPVector2 spawnPosition;
            Transform2D* playerTransform;
            FP rotation;
            FPVector2 normal;

            if (teamNumber == BattleTeamNumber.TeamAlpha)
            {
                rotation = FP._0;
                normal = new FPVector2(0, 1);
            }
            else
            {
                rotation = FP.Rad_180;
                normal = new FPVector2(0, -1);
            }

            for (int i = 0; i < playerEntityArray.Length; i++)
            {
               /* spawnPosition = GetOutOfPlayPosition(playerSlot, i, teamNumber);*/
                spawnPosition = playerHandle.GetOutOfPlayPosition(i, teamNumber);

                playerEntityArray[i] = f.Create(entityPrototypeAsset);
                f.Add(playerEntityArray[i], new PlayerData
                {
                    Player = PlayerRef.None,
                    Slot = playerSlot,
                    TeamNumber = teamNumber,
                    CharacterId = data.Characters[i].Id,
                    CharacterClass = data.Characters[i].Class,

                    StatHp = data.Characters[i].Hp,
                    StatSpeed = data.Characters[i].Speed,
                    StatCharacterSize = data.Characters[i].CharacterSize,
                    StatAttack = data.Characters[i].Attack,
                    StatDefence = data.Characters[i].Defence,

                    Speed = 20,
                    TargetPosition = spawnPosition,
                    MovementRotation = 0,
                    BaseRotation = rotation,
                    Normal = normal,
                    CollisionMinOffset = 1
                });


                playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntityArray[i]);
                playerTransform->Teleport(f, spawnPosition, rotation);
            }
            playerHandle.PlayState = PlayerPlayState.OutOfPlay;
            playerHandle.PlayerRef = player;
            playerHandle.SetCharacters(playerEntityArray);

        }

        public static FPVector2 GetOutOfPlayPosition(BattlePlayerSlot playerSlot, int characterNumber, BattleTeamNumber playerTeamNumber)
        {
            int row = 0, column = 0;
            
            switch(playerTeamNumber)
            {
                case BattleTeamNumber.TeamAlpha:
                    row = 0 - 10 * (characterNumber + 1);
                    column = 10 * GetPlayerIndex(playerSlot);
                    break;
                case BattleTeamNumber.TeamBeta:
                    row = GridManager.Rows + 10 * (characterNumber + 1);
                    column = GridManager.Columns - 10 * GetPlayerIndex(playerSlot);
                    break;

            }

            return new FPVector2
            (
                GridManager.GridColToWorldXPosition(column),
                GridManager.GridRowToWorldYPosition(row)
            );

        }
        
        private static void SpawnPlayer(Frame f, PlayerHandle playerHandle, int characterNumber, FPVector2 worldPosition)
        {
            if (playerHandle.PlayState == PlayerPlayState.InPlay)
            {
                DespawnPlayer(f, playerHandle);
            }

            EntityRef character = playerHandle.GetCharacter(characterNumber);
            PlayerData* playerData = f.Unsafe.GetPointer<PlayerData>(character);
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(character);

            playerData->Player = playerHandle.PlayerRef;
            playerTransform->Teleport(f, worldPosition, playerData->BaseRotation);
            playerData->TargetPosition = worldPosition;

            playerHandle.SetSelectedCharacter(characterNumber);

            playerHandle.PlayState = PlayerPlayState.InPlay;
        }
        public static void SpawnPlayer(Frame f, BattlePlayerSlot slot, int characterNumber, FPVector2 worldPosition)
        {
            PlayerHandle playerHandle = new(GetPlayerManagerData(f), slot);
            if (playerHandle.PlayState == PlayerPlayState.NotInGame)
            {
                Debug.Log("[PlayerManager] Can not spawn player that is not in game");
                return;
            }

            SpawnPlayer(f, playerHandle, characterNumber, worldPosition);
        }
        public static void SpawnPlayer(Frame f, BattlePlayerSlot slot, int characterNumber)
        {
            PlayerHandle playerHandle = new(GetPlayerManagerData(f), slot);
            if (playerHandle.PlayState == PlayerPlayState.NotInGame)
            {
                Debug.Log("[PlayerManager] Can not spawn player that is not in game");
                return;
            }

            if (playerHandle.PlayState == PlayerPlayState.OutOfPlay)
            {
                Debug.Log("[PlayerManager] Can not swap player that is out of play");
                return;
            }

            EntityRef selectedCharacter = playerHandle.SelectedCharacter;
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(selectedCharacter);

            SpawnPlayer(f, playerHandle, characterNumber, playerTransform->Position);
        }


        public static void DespawnPlayer(Frame f, BattlePlayerSlot slot)
        {   
            PlayerHandle playerHandle = new(GetPlayerManagerData(f), slot);
            if (playerHandle.PlayState != PlayerPlayState.InPlay)
            {
                Debug.Log("[PlayerManager] Can not despawn player that is not in play");
                return;
            }
            DespawnPlayer(f, playerHandle);
        }

        private static void DespawnPlayer(Frame f, PlayerHandle playerHandle)
        {
            EntityRef selectedCharacter = playerHandle.SelectedCharacter;
            PlayerData* playerData = f.Unsafe.GetPointer<PlayerData>(selectedCharacter);
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(selectedCharacter);
            playerData->Player = PlayerRef.None;
            playerTransform->Teleport(f, playerHandle.GetOutOfPlayPosition(playerHandle.SelectedCharacterNumber, playerData->TeamNumber), playerData->BaseRotation);

            playerHandle.UnsetSelectedCharacter();

            playerHandle.PlayState = PlayerPlayState.OutOfPlay;
        }
        public static BattleTeamNumber GetPlayerTeamNumber(BattlePlayerSlot playerSlot)
        {
            return playerSlot switch
            {
                BattlePlayerSlot.Slot1 => BattleTeamNumber.TeamAlpha,
                BattlePlayerSlot.Slot2 => BattleTeamNumber.TeamAlpha,
                BattlePlayerSlot.Slot3 => BattleTeamNumber.TeamBeta,
                BattlePlayerSlot.Slot4 => BattleTeamNumber.TeamBeta,

                _ => BattleTeamNumber.NoTeam
            };
        }

        public static EntityRef GetPlayerEntity(Frame f, BattlePlayerSlot slot)
        {
            return default;
        }

        public static EntityRef GetTeammateEnity(Frame f, BattlePlayerSlot slot)
        {
            return default;
        }

        public static void SwitchCharacter()
        {

        }
        public static bool IsValidCharacterIndex(int index) {
            return true;
        }

        public static int GetPlayerIndex(BattlePlayerSlot playerSlot)
        {
            return playerSlot switch
            {
                BattlePlayerSlot.Slot1 => 0,
                BattlePlayerSlot.Slot2 => 1,
                BattlePlayerSlot.Slot3 => 2,
                BattlePlayerSlot.Slot4 => 3,

                _ => -1
            };
        }

        private static int GetCharacterOffset(BattlePlayerSlot playerSlot)
        {
            return GetPlayerIndex(playerSlot) * Constants.PLAYER_CHARACTER_COUNT;
        }

        private static int GetCharacterIndex(BattlePlayerSlot playerSlot, int characterNumber) {
          return GetCharacterOffset(playerSlot) + characterNumber;
        }

        private static EntityRef GetCharacter(PlayerManagerData* playerManagerData ,BattlePlayerSlot playerSlot, int characterNumber)
        {
            return playerManagerData->AllCharacters[GetCharacterIndex(playerSlot, characterNumber)];
        }

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

        private static int GetSelectedCharacterNumber(PlayerManagerData* playerManagerData, BattlePlayerSlot playerSlot)
        {
            return playerManagerData->SelectedCharacterNumbers[GetPlayerIndex(playerSlot)];
        }

        public static int GetSelectedCharacterNumber(Frame f, BattlePlayerSlot playerSlot)
        {
            return GetSelectedCharacterNumber(GetPlayerManagerData(f), playerSlot);
        }
    }
}

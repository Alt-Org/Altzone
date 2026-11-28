using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Lobby.SelectedCharacters;
using MenuUi.Scripts.Signals;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>Shared room entry and character synchronization, independent of the room UI.</summary>
    public abstract class RoomSetupBase : AltMonoBehaviour
    {
        protected const string PlayerPositionKey = PhotonBattleRoom.PlayerPositionKey;
        protected const string PlayerCharactersKey = PhotonLobbyRoom.PlayerPrefabIdsKey;
        protected const string PlayerStatsKey = PhotonBattleRoom.PlayerStatsKey;
        protected const string PlayerReadyKey = PhotonBattleRoom.PlayerReadyKey;
        protected const int PlayerPosition1 = PhotonBattleRoom.PlayerPosition1;
        protected const int PlayerPosition2 = PhotonBattleRoom.PlayerPosition2;
        protected const int PlayerPosition3 = PhotonBattleRoom.PlayerPosition3;
        protected const int PlayerPosition4 = PhotonBattleRoom.PlayerPosition4;

        [SerializeField] protected UnityEngine.UI.Button _buttonStartPlay;
        [SerializeField] private BattlePopupCharacterSlotController _selectedCharactersEditable;
        private Coroutine _onEnableCoroutineHolder;
        private bool _firstOnEnable = true;
        private bool _reloadCharacters;
        private PlayerRole currentRole = PlayerRole.Player;

        public enum PlayerRole { Player, Spectator }

        protected virtual void Awake()
        {
            LobbyManager.LobbyOnLeftRoom += OnLocalPlayerLeftRoom;
            SignalBus.OnReloadCharacterGalleryRequested += OnReloadCharactersRequested;
        }

        protected virtual void OnEnable()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
            if (_buttonStartPlay != null) _buttonStartPlay.interactable = false;

            LobbyManager.LobbyOnPlayerEnteredRoom += OnPlayerEnteredRoom;
            LobbyManager.LobbyOnPlayerLeftRoom += OnPlayerLeftRoom;
            LobbyManager.LobbyOnRoomPropertiesUpdate += OnRoomPropertiesUpdate;
            LobbyManager.LobbyOnPlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
            LobbyManager.LobbyOnMasterClientSwitched += OnMasterClientSwitched;

            PhotonRealtimeClient.AddCallbackTarget(this);
            if (_onEnableCoroutineHolder == null) _onEnableCoroutineHolder = StartCoroutine(OnEnableInRoom());
        }

        protected virtual void OnDisable()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
            LobbyManager.LobbyOnPlayerEnteredRoom -= OnPlayerEnteredRoom;
            LobbyManager.LobbyOnPlayerLeftRoom -= OnPlayerLeftRoom;
            LobbyManager.LobbyOnRoomPropertiesUpdate -= OnRoomPropertiesUpdate;
            LobbyManager.LobbyOnPlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
            LobbyManager.LobbyOnMasterClientSwitched -= OnMasterClientSwitched;

            PhotonRealtimeClient.RemoveCallbackTarget(this);
            if (_onEnableCoroutineHolder != null) StopCoroutine(_onEnableCoroutineHolder);
            _onEnableCoroutineHolder = null;
        }

        protected virtual void OnDestroy()
        {
            LobbyManager.LobbyOnLeftRoom -= OnLocalPlayerLeftRoom;
            SignalBus.OnReloadCharacterGalleryRequested -= OnReloadCharactersRequested;
        }

        private void OnReloadCharactersRequested()
        {
            if (!PhotonRealtimeClient.InRoom) return;

            if (gameObject.activeInHierarchy) UpdateCharactersAndStatsKey();
            else _reloadCharacters = true;
        }

        private IEnumerator OnEnableInRoom()
        {
            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);

            // Getting the local player
            LobbyPlayer player = PhotonRealtimeClient.LocalLobbyPlayer;

            // Getting player data
            PlayerData playerData = null;
            StartCoroutine(GetPlayerData(data => playerData = data));
            yield return new WaitUntil(() => playerData != null);

            // Checking if player is already in the room (can happen if battle popup is minimized while in room)
            if (!_firstOnEnable)
            {
                // If we have to reload characters we call the method to update them else only updatestatus
                if (_reloadCharacters) UpdateCharactersAndStatsKey();
                else UpdateStatus();

                // Stopping coroutine
                _onEnableCoroutineHolder = null;
                yield break;
            }

            InitializeSlots();

            // Setting photon nickname from playerdata name
            PhotonRealtimeClient.NickName = playerData.Name;

            // Reset player custom properties for new game
            player.CustomProperties.Clear();

            // Reserving player position if not a master client
            if (!player.IsMasterClient)
            {
                this.Publish<LobbyManager.ReserveFreePositionEvent>(new());
            }
            else // If player is a master client setting the position which was set to room during creation to player properties too
            {
                player.SetCustomProperties(new LobbyPhotonHashtable(new Dictionary<object, object> {
                    {
                        PlayerPositionKey, PlayerPosition1
                    }
                }));
            }

            yield return new WaitUntil(() => player.GetCustomProperty(PlayerPositionKey, 0) != 0 || !PhotonRealtimeClient.InRoom);
            if (!PhotonRealtimeClient.InRoom) yield break;

            UpdateCharactersAndStatsKey();
            _firstOnEnable = false;
            _onEnableCoroutineHolder = null;
        }

        private void UpdateCharactersAndStatsKey()
        {
            if (!PhotonRealtimeClient.InRoom) return;
            StartCoroutine(GetPlayerData(playerData =>
            {
                // Getting character id and stat int arrays
                int[] characterIds = GetSelectedCharacterIds(playerData);
                int[] characterStats = GetCharactersStatsArray(playerData);

                // Updating player properties
                LobbyPlayer player = PhotonRealtimeClient.LocalLobbyPlayer;
                player.SetCustomProperties(new LobbyPhotonHashtable(new Dictionary<object, object>
                {
                    { PlayerCharactersKey, characterIds },
                    { PlayerStatsKey, characterStats },
                    { "Role", (int)currentRole },
                }));

                // Setting custom characters for quantum
                List<CustomCharacter> selectedCharacters = GetSelectedCustomCharacters(playerData);
                GameType gameType = (GameType)PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty(PhotonBattleRoom.GameTypeKey, (int)GameType.BattlePingPong);
                LobbyManager.Instance.SetPlayerQuantumCharacters(gameType, selectedCharacters);
                if (_selectedCharactersEditable != null) _selectedCharactersEditable.SetCharacters();
                _reloadCharacters = false;
                UpdateStatus();
            }));
        }

        private List<CustomCharacter> GetSelectedCustomCharacters(PlayerData playerData)
        {
            var battleCharacter = playerData.CurrentBattleCharacters;
            List<CustomCharacter> selectedCharacters = new();

            for (int i = 0; i < playerData.CurrentBattleCharacters.Count; i++)
            {
                selectedCharacters.Add(battleCharacter[i]);
            }

            return selectedCharacters;
        }

        private int[] GetSelectedCharacterIds(PlayerData playerData)
        {
            var battleCharacter = playerData.CurrentBattleCharacters;
            int[] characterIds = new int[playerData.CurrentBattleCharacters.Count];

            for (int i = 0; i < playerData.CurrentBattleCharacters.Count; i++)
            {
                characterIds[i] = (int)battleCharacter[i].Id;
            }
            /*foreach (var characterId in characterIds)
            {
                Debug.LogWarning(characterId);
            }*/
            return characterIds;
        }

        // The int array has all current selected characters' stats one after another.
        // Example: [ C1 Hp, C1 Speed, C1 CharacterSize, C1 Attack, C1 Defence, C2 Hp, C2 Speed, C2 CharacterSize, C2 Attack, C2 Defence, C3 Hp, C3 Speed, C3 CharacterSize, C3 Attack, C3 Defence]
        // (Here C means Character so C1 is Character 1)
        private int[] GetCharactersStatsArray(PlayerData playerData)
        {
            var battleCharacter = playerData.CurrentBattleCharacters;
            int[] characterStats = new int[playerData.CurrentBattleCharacters.Count * 5];

            for (int i = 0; i < playerData.CurrentBattleCharacters.Count; i++)
            {
                characterStats[i * 5] = battleCharacter[i].Hp;
                characterStats[i * 5 + 1] = battleCharacter[i].Speed;
                characterStats[i * 5 + 2] = battleCharacter[i].CharacterSize;
                characterStats[i * 5 + 3] = battleCharacter[i].Attack;
                characterStats[i * 5 + 4] = battleCharacter[i].Defence;
            }

            return characterStats;
        }

        void OnPlayerEnteredRoom(LobbyPlayer newPlayer)
        {
            UpdateStatus();
        }

        void OnPlayerLeftRoom(LobbyPlayer otherPlayer)
        {
            UpdateStatus();
        }

        void OnRoomPropertiesUpdate(LobbyPhotonHashtable propertiesThatChanged)
        {
            UpdateStatus();
        }

        void OnPlayerPropertiesUpdate(LobbyPlayer targetPlayer, LobbyPhotonHashtable changedProps)
        {
            UpdateStatus();
        }

        void OnMasterClientSwitched(LobbyPlayer newMasterClient)
        {
            UpdateStatus();
        }

        private void OnLocalPlayerLeftRoom()
        {
            _firstOnEnable = true;
            _reloadCharacters = false;
            ResetSlots();
        }

        protected abstract void InitializeSlots();
        protected abstract void UpdateStatus();
        protected abstract void ResetSlots();
    }
}

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Prg.Scripts.Common.PubSub;

using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;

using MenuUi.Scripts.Lobby.SelectedCharacters;
using MenuUi.Scripts.Signals;
using Altzone.Scripts.Language;
using Prg.Scripts.Common.Unity;

namespace MenuUi.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Prepares players in a room for the game play.
    /// </summary>
    public class RoomSetupManager : AltMonoBehaviour
    {
        private const string PlayerPositionKey = PhotonBattleRoom.PlayerPositionKey;
        private const string PlayerCharactersKey = PhotonLobbyRoom.PlayerPrefabIdsKey;
        private const string PlayerStatsKey = PhotonBattleRoom.PlayerStatsKey;

        private const int PlayerPosition1 = PhotonBattleRoom.PlayerPosition1;
        private const int PlayerPosition2 = PhotonBattleRoom.PlayerPosition2;
        private const int PlayerPosition3 = PhotonBattleRoom.PlayerPosition3;
        private const int PlayerPosition4 = PhotonBattleRoom.PlayerPosition4;

        private string PlayerPositionKey1 = PhotonBattleRoom.PlayerPositionKey1;
        private string PlayerPositionKey2 = PhotonBattleRoom.PlayerPositionKey2;
        private string PlayerPositionKey3 = PhotonBattleRoom.PlayerPositionKey3;
        private string PlayerPositionKey4 = PhotonBattleRoom.PlayerPositionKey4;

        private const string TeamAlphaNameKey = PhotonBattleRoom.TeamAlphaNameKey;
        private const string TeamBetaNameKey = PhotonBattleRoom.TeamBetaNameKey;
        private const int TeamAlphaValue = PhotonBattleRoom.TeamAlphaValue;
        private const int TeamBetaValue = PhotonBattleRoom.TeamBetaValue;

        [Header("Settings"), SerializeField] private TextMeshProUGUI _upperTeamText;
        [SerializeField] private TextMeshProUGUI _lowerTeamText;
        [SerializeField] private Toggle _toggleFillSlots;
        [SerializeField] private Button _buttonPlayerP1;
        [SerializeField] private Button _buttonPlayerP2;
        [SerializeField] private Button _buttonPlayerP3;
        [SerializeField] private Button _buttonPlayerP4;
        [SerializeField] private Toggle _toggleBotPlayerP1;
        [SerializeField] private Toggle _toggleBotPlayerP2;
        [SerializeField] private Toggle _toggleBotPlayerP3;
        [SerializeField] private Toggle _toggleBotPlayerP4;
        [SerializeField] private Button _buttonStartPlay;
        [SerializeField] private Button _buttonRaidTest;

        [Header("Player names")]
        [SerializeField] private TextLanguageSelectorCaller _nameP1;
        [SerializeField] private TextLanguageSelectorCaller _nameP2;
        [SerializeField] private TextLanguageSelectorCaller _nameP3;
        [SerializeField] private TextLanguageSelectorCaller _nameP4;

        [Header("Selected characters")]
        [SerializeField] private BattlePopupCharacterSlotController _selectedCharactersP1;
        [SerializeField] private BattlePopupCharacterSlotController _selectedCharactersP2;
        [SerializeField] private BattlePopupCharacterSlotController _selectedCharactersP3;
        [SerializeField] private BattlePopupCharacterSlotController _selectedCharactersP4;
        [SerializeField] private BattlePopupCharacterSlotController _selectedCharactersEditable;

        [Header("Live Data"), SerializeField] private int _localPlayerPosition;
        [SerializeField] private bool _isLocalPlayerPositionUnique;
        [SerializeField] private int _masterClientPosition;

        private bool _interactablePlayerP1;
        private bool _interactablePlayerP2;
        private bool _interactablePlayerP3;
        private bool _interactablePlayerP4;
        private bool _interactableStartPlay;

        private string _captionPlayerP1;
        private string _captionPlayerP2;
        private string _captionPlayerP3;
        private string _captionPlayerP4;

        private Coroutine _onEnableCoroutineHolder = null;

        PlayerRole currentRole = PlayerRole.Player;

        private bool _firstOnEnable = true;
        private bool _reloadCharacters = false;

        public enum PlayerRole
        {
            Player,
            Spectator
        }

        private void Awake()
        {
            LobbyManager.LobbyOnLeftRoom += OnLocalPlayerLeftRoom;
            SignalBus.OnReloadCharacterGalleryRequested += OnReloadCharactersRequested;
        }

        private void OnEnable()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
            _buttonStartPlay.interactable = false;
            //_buttonRaidTest.interactable = false;

            LobbyManager.LobbyOnPlayerEnteredRoom += OnPlayerEnteredRoom;
            LobbyManager.LobbyOnPlayerLeftRoom += OnPlayerLeftRoom;
            LobbyManager.LobbyOnRoomPropertiesUpdate += OnRoomPropertiesUpdate;
            LobbyManager.LobbyOnPlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
            LobbyManager.LobbyOnMasterClientSwitched += OnMasterClientSwitched;

            _toggleFillSlots.onValueChanged.AddListener(SetFillBotToggle);

            PhotonRealtimeClient.AddCallbackTarget(this);
            if (_onEnableCoroutineHolder == null) _onEnableCoroutineHolder = StartCoroutine(OnEnableInRoom());
        }

        private void OnDisable()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
            LobbyManager.LobbyOnPlayerEnteredRoom -= OnPlayerEnteredRoom;
            LobbyManager.LobbyOnPlayerLeftRoom -= OnPlayerLeftRoom;
            LobbyManager.LobbyOnRoomPropertiesUpdate -= OnRoomPropertiesUpdate;
            LobbyManager.LobbyOnPlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
            LobbyManager.LobbyOnMasterClientSwitched -= OnMasterClientSwitched;
            _toggleFillSlots.onValueChanged.RemoveListener(SetFillBotToggle);
            PhotonRealtimeClient.RemoveCallbackTarget(this);
            if (_onEnableCoroutineHolder != null) StopCoroutine(_onEnableCoroutineHolder);
            _onEnableCoroutineHolder = null;
        }

        private void OnDestroy()
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

            // Getting room and player 
            LobbyRoom room = PhotonRealtimeClient.LobbyCurrentRoom;
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
                LobbyManager.Instance.SetPlayerQuantumCharacters(selectedCharacters);
                _selectedCharactersEditable.SetCharacters();
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
            foreach (var characterId in characterIds)
            {
                Debug.LogWarning(characterId);
            }
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

        private void UpdateStatus()
        {
            if (!enabled || !PhotonRealtimeClient.InRoom)
            {
                return;
            }
            ResetState();
            GameType roomGameType = (GameType)PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.GameTypeKey);

            // We need local player to check against other players
            LobbyPlayer localPlayer = PhotonRealtimeClient.LocalLobbyPlayer;
            _localPlayerPosition = localPlayer.GetCustomProperty(PlayerPositionKey, 0);

            CheckMasterClient();

            //Check if positions have bots
            bool botActive1 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition1);
            _toggleBotPlayerP1.GetComponent<ToggleSliderHandler>().SetState(botActive1);
            if (botActive1) _captionPlayerP1 = "Bot";
            bool botActive2 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition2);
            _toggleBotPlayerP2.GetComponent<ToggleSliderHandler>().SetState(botActive2);
            if (botActive2) _captionPlayerP2 = "Bot";
            bool botActive3 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition3);
            _toggleBotPlayerP3.GetComponent<ToggleSliderHandler>().SetState(botActive3);
            if (botActive3) _captionPlayerP3 = "Bot";
            bool botActive4 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition4);
            _toggleBotPlayerP4.GetComponent<ToggleSliderHandler>().SetState(botActive4);
            if (botActive4) _captionPlayerP4 = "Bot";


            // Check other players first is they have reserved some player positions etc. from the room already.
            foreach (var player in PhotonRealtimeClient.GetCurrentRoomPlayers())
            {
                if (!player.Equals(localPlayer))
                {
                    CheckOtherPlayer(player);
                }
            }
            CheckLocalPlayer(localPlayer);

            // Setting player position buttons active status
            SetButtonActive(_buttonPlayerP1, _interactablePlayerP1 && !botActive1);
            SetButtonActive(_buttonPlayerP2, _interactablePlayerP2 && !botActive2);
            SetButtonActive(_buttonPlayerP3, _interactablePlayerP3 && !botActive3);
            SetButtonActive(_buttonPlayerP4, _interactablePlayerP4 && !botActive4);

            // Setting bot toggle buttons.
            SetButtonActive(_toggleBotPlayerP1, _interactablePlayerP1, localPlayer.IsMasterClient);
            SetButtonActive(_toggleBotPlayerP2, _interactablePlayerP2, localPlayer.IsMasterClient);
            SetButtonActive(_toggleBotPlayerP3, _interactablePlayerP3, localPlayer.IsMasterClient);
            SetButtonActive(_toggleBotPlayerP4, _interactablePlayerP4, localPlayer.IsMasterClient);

            // Setting player name texts
            if (_nameP1 != null) _nameP1.SetText(_captionPlayerP1);
            if (_nameP2 != null) _nameP2.SetText(_captionPlayerP2);
            if (_nameP3 != null) _nameP3.SetText(_captionPlayerP3);
            if (_nameP4 != null) _nameP4.SetText(_captionPlayerP4);

            // Setting start game button interactable status
            _buttonStartPlay.interactable = _interactableStartPlay;

            // Setting team text
            SetTeamText();
        }

        private void SetTeamText()
        {
            LobbyRoom room = PhotonRealtimeClient.LobbyCurrentRoom;
            int masterTeam = GetTeam(_masterClientPosition);
            if (masterTeam == 1)
            {
                if (_upperTeamText != null)
                {
                    string clanName = room.GetCustomProperty<string>(TeamBetaNameKey);
                    if (string.IsNullOrEmpty(clanName))
                    {
                        clanName = "Team Jouko";
                    }
                    _upperTeamText.text = clanName;
                }
                if (_lowerTeamText != null)
                {
                    string clanName = room.GetCustomProperty<string>(TeamAlphaNameKey);
                    if (string.IsNullOrEmpty(clanName))
                    {
                        clanName = "Team Kaarina";
                    }
                    _lowerTeamText.text = clanName;
                }
            }
            else
            {
                if (_upperTeamText != null)
                {
                    string clanName = room.GetCustomProperty<string>(TeamAlphaNameKey);
                    if (string.IsNullOrEmpty(clanName))
                    {
                        clanName = "Team Kaarina";
                    }
                    _upperTeamText.text = clanName;
                }
                if (_lowerTeamText != null)
                {
                    string clanName = room.GetCustomProperty<string>(TeamBetaNameKey);
                    if (string.IsNullOrEmpty(clanName))
                    {
                        clanName = "Team Jouko";
                    }
                    _lowerTeamText.text = clanName;
                }
            }
        }

        private static int GetTeam(int playerPos)
        {
            if (playerPos == PlayerPosition1 || playerPos == PlayerPosition2)
            {
                return TeamAlphaValue;
            }
            if (playerPos == PlayerPosition3 || playerPos == PlayerPosition4)
            {
                return TeamBetaValue;
            }
            return -1;
        }

        private void CheckMasterClient()
        {
            var curValue = PhotonRealtimeClient.LobbyCurrentRoom.GetPlayer(PhotonRealtimeClient.LobbyCurrentRoom.MasterClientId).GetCustomProperty(PlayerPositionKey, 0);
            _masterClientPosition = curValue;
        }

        private void CheckOtherPlayer(LobbyPlayer player)
        {
            if (!player.HasCustomProperty(PlayerPositionKey))
            {
                Debug.LogWarning($"{player.NickName}: Cannot find PlayerPositionKey.");
                return;
            }
            if (!player.HasCustomProperty(PlayerCharactersKey))
            {
                Debug.LogWarning($"{player.NickName}: Cannot find PlayerCharactersKey.");
                return;
            }
            if (!player.HasCustomProperty(PlayerStatsKey))
            {
                Debug.LogWarning($"{player.NickName}: Cannot find PlayerStatsKey.");
                return;
            }

            var playerPosition = player.GetCustomProperty(PlayerPositionKey, 0);
            int[] characters = player.GetCustomProperty(PlayerCharactersKey, new int[3]);
            int[] stats = player.GetCustomProperty(PlayerStatsKey, new int[15]);

            switch (playerPosition)
            {
                case PlayerPosition1:
                    if (!_interactablePlayerP1) { _captionPlayerP1 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP1 = false;
                    if (_captionPlayerP1 != null) _captionPlayerP1 = player.NickName;
                    if (_selectedCharactersP1 != null) _selectedCharactersP1.SetCharacters(characters, stats);
                    break;
                case PlayerPosition2:
                    if (!_interactablePlayerP2) { _captionPlayerP2 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP2 = false;
                    if (_captionPlayerP2 != null) _captionPlayerP2 = player.NickName;
                    if (_selectedCharactersP2 != null) _selectedCharactersP2.SetCharacters(characters, stats);
                    break;
                case PlayerPosition3:
                    if (!_interactablePlayerP3) { _captionPlayerP3 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP3 = false;
                    if (_captionPlayerP3 != null) _captionPlayerP3 = player.NickName;
                    if (_selectedCharactersP3 != null) _selectedCharactersP3.SetCharacters(characters, stats);
                    break;
                case PlayerPosition4:
                    if (!_interactablePlayerP4) { _captionPlayerP4 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP4 = false;
                    if (_captionPlayerP4 != null) _captionPlayerP4 = player.NickName;
                    if (_selectedCharactersP4 != null) _selectedCharactersP4.SetCharacters(characters, stats);
                    break;
            }
        }

        private void CheckLocalPlayer(LobbyPlayer player)
        {
            var playerPosition = player.GetCustomProperty(PlayerPositionKey, 0);

            // Master client can *only* start the game when in room as player!
            _interactableStartPlay = player.IsMasterClient && playerPosition >= PlayerPosition1 && playerPosition <= PlayerPosition4;

            switch (playerPosition)
            {
                case PlayerPosition1:
                    if (!_interactablePlayerP1) { _captionPlayerP1 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP1 = false;
                    if (_captionPlayerP1 != null) _captionPlayerP1 = $"<color=blue>{player.NickName}</color>";
                    if (_selectedCharactersP1 != null) _selectedCharactersP1.SetCharacters();
                    break;
                case PlayerPosition2:
                    if (!_interactablePlayerP2) { _captionPlayerP2 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP2 = false;
                    if (_captionPlayerP2 != null) _captionPlayerP2 = $"<color=blue>{player.NickName}</color>";
                    if (_selectedCharactersP2 != null) _selectedCharactersP2.SetCharacters();
                    break;
                case PlayerPosition3:
                    if (!_interactablePlayerP3) { _captionPlayerP3 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP3 = false;
                    if (_captionPlayerP3 != null) _captionPlayerP3 = $"<color=blue>{player.NickName}</color>";
                    if (_selectedCharactersP4 != null) _selectedCharactersP3.SetCharacters();
                    break;
                case PlayerPosition4:
                    if (!_interactablePlayerP4) { _captionPlayerP4 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP4 = false;
                    if (_captionPlayerP4 != null) _captionPlayerP4 = $"<color=blue>{player.NickName}</color>";
                    if (_selectedCharactersP4 != null) _selectedCharactersP4.SetCharacters();
                    break;
            }
        }

        private void ResetState()
        {
            _interactablePlayerP1 = true;
            _interactablePlayerP2 = true;
            _interactablePlayerP3 = true;
            _interactablePlayerP4 = true;
            _interactableStartPlay = false;

            _captionPlayerP1 = "";
            _captionPlayerP2 = "";
            _captionPlayerP3 = "";
            _captionPlayerP4 = "";
        }

        private void SetFillBotToggle(bool value)
        {
            Debug.Log($"SetFillBotToggle {value}");
            this.Publish(new LobbyManager.BotFillToggleEvent(value));
        }

        private static void SetButtonActive(Selectable selectable, bool active, bool interactable = true)
        {
            if (selectable == null) return;
            selectable.gameObject.SetActive(active);
            selectable.interactable = interactable;
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

            SetButtonActive(_buttonPlayerP1, true, false);
            SetButtonActive(_buttonPlayerP2, true, false);
            SetButtonActive(_buttonPlayerP3, true, false);
            SetButtonActive(_buttonPlayerP4, true, false);
        }
    }
}

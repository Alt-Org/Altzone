using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Config;
using Altzone.Scripts.Config.ScriptableObjects;
using Altzone.Scripts.Language;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Lobby.SelectedCharacters;
using MenuUi.Scripts.Signals;
using Newtonsoft.Json.Linq;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

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
        private const string PlayerReadyKey = PhotonBattleRoom.PlayerReadyKey;

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
        [SerializeField] private SliderToggle _toggleBotFill;
        [SerializeField] private InRoomPlayerSlot _player1Slot;
        [SerializeField] private InRoomPlayerSlot _player2Slot;
        [SerializeField] private InRoomPlayerSlot _player3Slot;
        [SerializeField] private InRoomPlayerSlot _player4Slot;
        [SerializeField] private Button _buttonStartPlay;
        [SerializeField] private Button _buttonRaidTest;

        [SerializeField] private BattlePopupCharacterSlotController _selectedCharactersEditable;

        [Header("Live Data"), SerializeField] private int _localPlayerPosition;
        [SerializeField] private bool _isLocalPlayerPositionUnique;
        [SerializeField] private int _masterClientPosition;
        private bool _isPlayerMaster;

        private bool _interactablePlayerP1;
        private bool _interactablePlayerP2;
        private bool _interactablePlayerP3;
        private bool _interactablePlayerP4;
        private bool _interactableStartPlay;

        private string _captionPlayerP1;
        private string _captionPlayerP2;
        private string _captionPlayerP3;
        private string _captionPlayerP4;

        private bool _player1SlotInUse = false;
        private bool _player2SlotInUse = false;
        private bool _player3SlotInUse = false;
        private bool _player4SlotInUse = false;

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
            if (_buttonStartPlay != null) _buttonStartPlay.interactable = false;
            //_buttonRaidTest.interactable = false;

            LobbyManager.LobbyOnPlayerEnteredRoom += OnPlayerEnteredRoom;
            LobbyManager.LobbyOnPlayerLeftRoom += OnPlayerLeftRoom;
            LobbyManager.LobbyOnRoomPropertiesUpdate += OnRoomPropertiesUpdate;
            LobbyManager.LobbyOnPlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
            LobbyManager.LobbyOnMasterClientSwitched += OnMasterClientSwitched;

            if (_toggleBotFill != null) _toggleBotFill.onValueChanged.AddListener(SetFillBotToggle);

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
            if (_toggleBotFill != null) _toggleBotFill.onValueChanged.RemoveListener(SetFillBotToggle);
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

            if (_player1Slot) _player1Slot.Initialize(0);
            if (_player2Slot) _player2Slot.Initialize(1);
            if (_player3Slot) _player3Slot.Initialize(2);
            if (_player4Slot) _player4Slot.Initialize(3);

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

        private void UpdateStatus()
        {
            if (!enabled || !PhotonRealtimeClient.InRoom)
            {
                return;
            }
            ResetState();
            MatchmakingType roomGameType = (MatchmakingType)PhotonRealtimeClient.LobbyCurrentRoom.GetCustomProperty<int>(PhotonBattleRoom.MatchmakingKey);
            if(roomGameType is MatchmakingType.Custom)
                if (PhotonRealtimeClient.LobbyCurrentRoom.MaxPlayers == 2)
                {
                    _player1Slot.gameObject.SetActive(true);
                    _player2Slot.gameObject.SetActive(false);
                    _player3Slot.gameObject.SetActive(true);
                    _player4Slot.gameObject.SetActive(false);
                }
                else
                {
                    _player1Slot.gameObject.SetActive(true);
                    _player2Slot.gameObject.SetActive(true);
                    _player3Slot.gameObject.SetActive(true);
                    _player4Slot.gameObject.SetActive(true);
                }

                // We need local player to check against other players
                LobbyPlayer localPlayer = PhotonRealtimeClient.LocalLobbyPlayer;
            _localPlayerPosition = localPlayer.GetCustomProperty(PlayerPositionKey, 0);

            CheckMasterClient();

            // Check if bot fill is active
            bool botFillActive = PhotonBattleRoom.IsBotFillActive();
            if (_toggleBotFill != null) _toggleBotFill.SetState(botFillActive);

            //Check if positions have bots
            bool botActive1 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition1);
            if (botActive1)
            {
                _player1Slot.SetBotCharacters(_isPlayerMaster);
                _player1SlotInUse = true;
            }
            bool botActive2 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition2);
            if (botActive2)
            {
                _player2Slot.SetBotCharacters(_isPlayerMaster);
                _player2SlotInUse = true;
            }
            bool botActive3 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition3);
            if (botActive3)
            {
                _player3Slot.SetBotCharacters(_isPlayerMaster);
                _player3SlotInUse = true;
            }
            bool botActive4 = PhotonBattleRoom.CheckIfPositionHasBot(PlayerPosition4);
            if (botActive4)
            {
                _player4Slot.SetBotCharacters(_isPlayerMaster);
                _player4SlotInUse = true;
            }


            // Check other players first is they have reserved some player positions etc. from the room already.
            foreach (var player in PhotonRealtimeClient.GetCurrentRoomPlayers())
            {
                if (!player.Equals(localPlayer))
                {
                    CheckOtherPlayer(player);
                }
            }
            CheckLocalPlayer(localPlayer);

            // Setting start game button interactable status
            if (_buttonStartPlay != null) _buttonStartPlay.interactable = _interactableStartPlay;

            if (_player1Slot && !_player1SlotInUse) _player1Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player2Slot && !_player2SlotInUse) _player2Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player3Slot && !_player3SlotInUse) _player3Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player4Slot && !_player4SlotInUse) _player4Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);

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
            if (PhotonRealtimeClient.LocalLobbyPlayer.UserId == PhotonRealtimeClient.LobbyCurrentRoom.GetPlayer(PhotonRealtimeClient.LobbyCurrentRoom.MasterClientId).UserId) _isPlayerMaster = true;
            else _isPlayerMaster = false;
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
            bool ready = player.GetCustomProperty(PlayerReadyKey, false);

            bool isMasterClient = false;
            if(player.UserId == PhotonRealtimeClient.LobbyCurrentRoom.GetPlayer(PhotonRealtimeClient.LobbyCurrentRoom.MasterClientId).UserId) isMasterClient = true;

            switch (playerPosition)
            {
                case PlayerPosition1:
                    if (!_interactablePlayerP1) { _captionPlayerP1 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP1 = false;
                    if (_captionPlayerP1 != null) _captionPlayerP1 = player.NickName;
                    if (_player1Slot != null)
                    {
                        _player1Slot.SetCharacters(player.UserId, player.NickName, characters, false, isMasterClient, _isPlayerMaster);
                        _player1Slot.SetReadyState(ready);
                        _player1SlotInUse = true;
                    }
                    break;
                case PlayerPosition2:
                    if (!_interactablePlayerP2) { _captionPlayerP2 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP2 = false;
                    if (_captionPlayerP2 != null) _captionPlayerP2 = player.NickName;
                    if (_player2Slot != null)
                    {
                        _player2Slot.SetCharacters(player.UserId, player.NickName, characters, false, isMasterClient, _isPlayerMaster);
                        _player2Slot.SetReadyState(ready);
                        _player2SlotInUse = true;
                    }
                    break;
                case PlayerPosition3:
                    if (!_interactablePlayerP3) { _captionPlayerP3 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP3 = false;
                    if (_captionPlayerP3 != null) _captionPlayerP3 = player.NickName;
                    if (_player3Slot != null)
                    {
                        _player3Slot.SetCharacters(player.UserId, player.NickName, characters, false, isMasterClient, _isPlayerMaster);
                        _player3Slot.SetReadyState(ready);
                        _player3SlotInUse = true;
                    }
                    break;
                case PlayerPosition4:
                    if (!_interactablePlayerP4) { _captionPlayerP4 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP4 = false;
                    if (_captionPlayerP4 != null) _captionPlayerP4 = player.NickName;
                    if (_player4Slot != null)
                    {
                        _player4Slot.SetCharacters(player.UserId, player.NickName, characters, false, isMasterClient, _isPlayerMaster);
                        _player4Slot.SetReadyState(ready);
                        _player4SlotInUse = true;
                    }
                    break;
            }
        }

        private void CheckLocalPlayer(LobbyPlayer player)
        {
            var playerPosition = player.GetCustomProperty(PlayerPositionKey, 0);
            bool ready = player.GetCustomProperty(PlayerReadyKey, false);

            // Master client can *only* start the game when in room as player!
            _interactableStartPlay = player.IsMasterClient && playerPosition >= PlayerPosition1 && playerPosition <= PlayerPosition4;
            int[] characters = new int[3];
            Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, playerData =>
            {
                CustomCharacterListObject[] character = playerData.SelectedCharacterIds;
                int i = 0;
                foreach (CustomCharacterListObject characterId in character)
                {
                    characters[i] = (int)characterId.CharacterID;
                    i++;
                }
            });

            bool isMasterClient = false;
            if (player.UserId == PhotonRealtimeClient.LobbyCurrentRoom.GetPlayer(PhotonRealtimeClient.LobbyCurrentRoom.MasterClientId).UserId) isMasterClient = true;

            switch (playerPosition)
            {
                case PlayerPosition1:
                    if (!_interactablePlayerP1) { _captionPlayerP1 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP1 = false;
                    if (_captionPlayerP1 != null) _captionPlayerP1 = $"<color=blue>{player.NickName}</color>";
                    if (_player1Slot != null)
                    {
                        _player1Slot.SetCharacters(player.UserId, player.NickName, characters, true, isMasterClient, _isPlayerMaster);
                        _player1Slot.SetReadyState(ready);
                        _player1SlotInUse = true;
                    }
                    break;
                case PlayerPosition2:
                    if (!_interactablePlayerP2) { _captionPlayerP2 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP2 = false;
                    if (_captionPlayerP2 != null) _captionPlayerP2 = $"<color=blue>{player.NickName}</color>";
                    if (_player2Slot != null)
                    {
                        _player2Slot.SetCharacters(player.UserId, player.NickName, characters, true, isMasterClient, _isPlayerMaster);
                        _player2Slot.SetReadyState(ready);
                        _player2SlotInUse = true;
                    }
                    break;
                case PlayerPosition3:
                    if (!_interactablePlayerP3) { _captionPlayerP3 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP3 = false;
                    if (_captionPlayerP3 != null) _captionPlayerP3 = $"<color=blue>{player.NickName}</color>";
                    if (_player3Slot != null)
                    {
                        _player3Slot.SetCharacters(player.UserId, player.NickName, characters, true, isMasterClient, _isPlayerMaster);
                        _player3Slot.SetReadyState(ready);
                        _player3SlotInUse = true;
                    }
                    break;
                case PlayerPosition4:
                    if (!_interactablePlayerP4) { _captionPlayerP4 = $"<color=red>Confict Detected!!</color> "; break; }
                    _interactablePlayerP4 = false;
                    if (_captionPlayerP4 != null) _captionPlayerP4 = $"<color=blue>{player.NickName}</color>";
                    if (_player4Slot != null)
                    {
                        _player4Slot.SetCharacters(player.UserId, player.NickName, characters, true, isMasterClient, _isPlayerMaster);
                        _player4Slot.SetReadyState(ready);
                        _player4SlotInUse = true;
                    }
                    break;
            }
        }

        private void ResetState()
        {
            _interactablePlayerP1 = true;
            _interactablePlayerP2 = true;
            _interactablePlayerP3 = true;
            _interactablePlayerP4 = true;

            _player1SlotInUse = false;
            _player2SlotInUse = false;
            _player3SlotInUse = false;
            _player4SlotInUse = false;
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

            ResetState();

            if (_player1Slot && !_player1SlotInUse) _player1Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player2Slot && !_player2SlotInUse) _player2Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player3Slot && !_player3SlotInUse) _player3Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
            if (_player4Slot && !_player4SlotInUse) _player4Slot.SetCharacters(null, null, null, false, false, _isPlayerMaster);
        }
    }
}

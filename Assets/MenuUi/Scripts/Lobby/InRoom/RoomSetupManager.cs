using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Config;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Lobby.SelectedCharacters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Lobby.InRoom
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
        [SerializeField] private Button _buttonPlayerP1;
        [SerializeField] private Button _buttonPlayerP2;
        [SerializeField] private Button _buttonPlayerP3;
        [SerializeField] private Button _buttonPlayerP4;
        [SerializeField] private Button _buttonStartPlay;
        [SerializeField] private Button _buttonRaidTest;

        [Header("Player names")]
        [SerializeField] private TMP_Text _nameP1;
        [SerializeField] private TMP_Text _nameP2;
        [SerializeField] private TMP_Text _nameP3;
        [SerializeField] private TMP_Text _nameP4;

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

        PlayerRole currentRole = PlayerRole.Player;

        public enum PlayerRole
        {
            Player,
            Spectator
        }

        private void Awake()
        {
            _selectedCharactersEditable.SelectedCharactersChanged += UpdateCharactersAndStatsKey;
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

            PhotonRealtimeClient.AddCallbackTarget(this);
            StartCoroutine(OnEnableInRoom());
        }

        private void OnDisable()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
            LobbyManager.LobbyOnPlayerEnteredRoom -= OnPlayerEnteredRoom;
            LobbyManager.LobbyOnPlayerLeftRoom -= OnPlayerLeftRoom;
            LobbyManager.LobbyOnRoomPropertiesUpdate -= OnRoomPropertiesUpdate;
            LobbyManager.LobbyOnPlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
            LobbyManager.LobbyOnMasterClientSwitched -= OnMasterClientSwitched;
            PhotonRealtimeClient.RemoveCallbackTarget(this);
        }

        private void OnDestroy()
        {
            _selectedCharactersEditable.SelectedCharactersChanged -= UpdateCharactersAndStatsKey;
        }

        private IEnumerator OnEnableInRoom()
        {
            yield return new WaitUntil(() => PhotonRealtimeClient.InRoom);

            UpdatePhotonNickname();
            var room = PhotonRealtimeClient.LobbyCurrentRoom;
            var player = PhotonRealtimeClient.LocalLobbyPlayer;
            //PhotonRealtimeClient.NickName = room.GetUniquePlayerNameForRoom(player, PhotonRealtimeClient.NickName, "");
            Debug.Log($"OnEnable InRoom '{room.Name}' as '{PhotonRealtimeClient.NickName}'");

            // Reset player custom properties for new game
            player.CustomProperties.Clear();

            // Getting first free player pos
            var playerPos = PhotonLobbyRoom.GetFirstFreePlayerPos(player);

            StartCoroutine(GetPlayerData(playerData =>
            {
                // Reserving player position inside the room
                LobbyPhotonHashtable propertyToSet = new();
                LobbyPhotonHashtable expectedValue = new();

                switch (playerPos)
                {
                    case PlayerPosition1:
                        propertyToSet = new LobbyPhotonHashtable(new Dictionary<object, object> { { PlayerPositionKey1, playerData.Id } });
                        expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { PlayerPositionKey1, "" } });
                        break;
                    case PlayerPosition2:
                        propertyToSet = new LobbyPhotonHashtable(new Dictionary<object, object> { { PlayerPositionKey2, playerData.Id } });
                        expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { PlayerPositionKey2, "" } });
                        break;
                    case PlayerPosition3:
                        propertyToSet = new LobbyPhotonHashtable(new Dictionary<object, object> { { PlayerPositionKey3, playerData.Id } });
                        expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { PlayerPositionKey3, "" } });
                        break;
                    case PlayerPosition4:
                        propertyToSet = new LobbyPhotonHashtable(new Dictionary<object, object> { { PlayerPositionKey4, playerData.Id } });
                        expectedValue = new LobbyPhotonHashtable(new Dictionary<object, object> { { PlayerPositionKey4, "" } });
                        break;
                }
                room.SetCustomProperties(propertyToSet, expectedValue);

                // Getting character id and stat int arrays
                int[] characterIds = GetSelectedCharacterIds(playerData);
                int[] characterStats = GetCharactersStatsArray(playerData);

                // Creating custom properties
                player.SetCustomProperties(new LobbyPhotonHashtable(new Dictionary<object, object>
                {
                    { PlayerPositionKey, playerPos },
                    { PlayerCharactersKey, characterIds },
                    { PlayerStatsKey, characterStats },
                    { "Role", (int)currentRole }
                }));

                // Setting custom characters for quantum
                List<CustomCharacter> selectedCharacters = GetSelectedCustomCharacters(playerData);
                LobbyManager.Instance.SetPlayerQuantumCharacters(selectedCharacters);

                Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState} {enabled}");

                UpdateStatus();
            }));
        }

        private void UpdateCharactersAndStatsKey()
        {
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
                }));

                // Setting custom characters for quantum
                List<CustomCharacter> selectedCharacters = GetSelectedCustomCharacters(playerData);
                LobbyManager.Instance.SetPlayerQuantumCharacters(selectedCharacters);
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
            // We need local player to check against other players
            LobbyPlayer localPlayer = PhotonRealtimeClient.LocalLobbyPlayer;
            _localPlayerPosition = localPlayer.GetCustomProperty(PlayerPositionKey, 0);
            //currentRole = PlayerRole.Player;
            _isLocalPlayerPositionUnique = true;

            CheckMasterClient();
            // Check other players first is they have reserved some player positions etc. from the room already.
            foreach (var player in PhotonRealtimeClient.GetCurrentRoomPlayers())
            {
                if (!player.Equals(localPlayer))
                {
                    CheckOtherPlayer(player);
                }
            }
            CheckLocalPlayer(localPlayer);

            SetButtonActive(_buttonPlayerP1, _interactablePlayerP1);
            SetButtonActive(_buttonPlayerP2, _interactablePlayerP2);
            SetButtonActive(_buttonPlayerP3, _interactablePlayerP3);
            SetButtonActive(_buttonPlayerP4, _interactablePlayerP4);

            _nameP1.text = _captionPlayerP1;
            _nameP2.text = _captionPlayerP2;
            _nameP3.text = _captionPlayerP3;
            _nameP4.text = _captionPlayerP4;

            _buttonStartPlay.interactable = _interactableStartPlay;
            SetTeamText();
        }

        private void UpdatePhotonNickname()
        {
            var store = Storefront.Get();
            PlayerData playerData = null;
            store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);
            PhotonRealtimeClient.NickName = playerData.Name;
        }

        private void SetTeamText()
        {
            LobbyRoom room = PhotonRealtimeClient.LobbyCurrentRoom;
            int masterTeam = GetTeam(_masterClientPosition);
            if (masterTeam == 0)
            {
                _upperTeamText.text = "Beta joukkue";//$"Joukkue {room.GetCustomProperty<string>(TeamRedNameKey)}";
                _lowerTeamText.text = "Alpha joukkue";//$"Joukkue {room.GetCustomProperty<string>(TeamBlueNameKey)}";
            }
            else
            {
                _upperTeamText.text = "Alpha joukkue";//{room.GetCustomProperty<string>(TeamBlueNameKey)}";
                _lowerTeamText.text = "Beta joukkue";//$"Joukkue {room.GetCustomProperty<string>(TeamRedNameKey)}";
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
            Debug.Log($"checkOtherPlayer {player.GetDebugLabel()}");
            if (!player.HasCustomProperty(PlayerPositionKey) || !player.HasCustomProperty(PlayerCharactersKey) || !player.HasCustomProperty(PlayerStatsKey))
            {
                return;
            }
            var curValue = player.GetCustomProperty(PlayerPositionKey, 0);
            if (_isLocalPlayerPositionUnique && curValue >= PlayerPosition1 && curValue <= PlayerPosition4)
            {
                if (curValue == _localPlayerPosition)
                {
                    Debug.Log("detected conflict");
                    _isLocalPlayerPositionUnique = false; // Conflict with player positions!
                }
            }

            int[] characters = player.GetCustomProperty(PlayerCharactersKey, new int[3]);
            int[] stats = player.GetCustomProperty(PlayerStatsKey, new int[15]);

            switch (curValue)
            {
                case PlayerPosition1:
                    _interactablePlayerP1 = false;
                    _captionPlayerP1 = player.NickName;
                    _selectedCharactersP1.SetCharacters(characters, stats);
                    break;
                case PlayerPosition2:
                    _interactablePlayerP2 = false;
                    _captionPlayerP2 = player.NickName;
                    _selectedCharactersP2.SetCharacters(characters, stats);
                    break;
                case PlayerPosition3:
                    _interactablePlayerP3 = false;
                    _captionPlayerP3 = player.NickName;
                    _selectedCharactersP3.SetCharacters(characters, stats);
                    break;
                case PlayerPosition4:
                    _interactablePlayerP4 = false;
                    _captionPlayerP4 = player.NickName;
                    _selectedCharactersP4.SetCharacters(characters, stats);
                    break;
            }
        }

        private void CheckLocalPlayer(LobbyPlayer player)
        {
            Debug.Log($"checkLocalPlayer {player.GetDebugLabel()} pos={_localPlayerPosition} ok={_isLocalPlayerPositionUnique}");
            var curValue = player.GetCustomProperty(PlayerPositionKey, 0);
            // Master client can *only* start the game when in room as player!
            _interactableStartPlay = player.IsMasterClient && curValue >= PlayerPosition1 && curValue <= PlayerPosition4;

            int[] characters = player.GetCustomProperty(PlayerCharactersKey, new int[3]);
            int[] stats = player.GetCustomProperty(PlayerStatsKey, new int[15]);

            switch (curValue)
            {
                case PlayerPosition1:
                    _interactablePlayerP1 = false;
                    _captionPlayerP1 = $"<color=blue>{player.NickName}</color>";
                    _selectedCharactersP1.SetCharacters(characters, stats);
                    break;
                case PlayerPosition2:
                    _interactablePlayerP2 = false;
                    _captionPlayerP2 = $"<color=blue>{player.NickName}</color>";
                    _selectedCharactersP2.SetCharacters(characters, stats);
                    break;
                case PlayerPosition3:
                    _interactablePlayerP3 = false;
                    _captionPlayerP3 = $"<color=blue>{player.NickName}</color>";
                    _selectedCharactersP3.SetCharacters(characters, stats);
                    break;
                case PlayerPosition4:
                    _interactablePlayerP4 = false;
                    _captionPlayerP4 = $"<color=blue>{player.NickName}</color>";
                    _selectedCharactersP4.SetCharacters(characters, stats);
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

            _captionPlayerP1 = "Pelaaja 1";
            _captionPlayerP2 = "Pelaaja 2";
            _captionPlayerP3 = "Pelaaja 3";
            _captionPlayerP4 = "Pelaaja 4";
        }

        private static void SetButtonActive(Selectable selectable, bool active)
        {
            if (selectable == null) return;
            selectable.gameObject.SetActive(active);
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
    }
}

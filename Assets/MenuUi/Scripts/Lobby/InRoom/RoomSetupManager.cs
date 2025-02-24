using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Config;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Lobby.Wrappers;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Lobby.InRoom
{
    /// <summary>
    /// Prepares players in a room for the game play.
    /// </summary>
    public class RoomSetupManager : MonoBehaviour
    {
        private const string PlayerPositionKey = PhotonBattleRoom.PlayerPositionKey;
        private const string PlayerMainSkillKey = PhotonLobbyRoom.PlayerPrefabIdKey;
        private const string PlayerCharactersKey = PhotonLobbyRoom.PlayerPrefabIdsKey;
        private const string PlayerStatsKey = PhotonBattleRoom.PlayerStatsKey;

        private const int PlayerPositionGuest = PhotonBattleRoom.PlayerPositionGuest;
        private const int PlayerPosition1 = PhotonBattleRoom.PlayerPosition1;
        private const int PlayerPosition2 = PhotonBattleRoom.PlayerPosition2;
        private const int PlayerPosition3 = PhotonBattleRoom.PlayerPosition3;
        private const int PlayerPosition4 = PhotonBattleRoom.PlayerPosition4;
        private const int PlayerPositionSpectator = PhotonBattleRoom.PlayerPositionSpectator;

        private const string TeamBlueNameKey = PhotonBattleRoom.TeamAlphaNameKey;
        private const string TeamRedNameKey = PhotonBattleRoom.TeamBetaNameKey;
        private const int TeamBlueValue = PhotonBattleRoom.TeamAlphaValue;
        private const int TeamRedValue = PhotonBattleRoom.TeamBetaValue;

        [Header("Settings"), SerializeField] private TextMeshProUGUI _upperTeamText;
        [SerializeField] private TextMeshProUGUI _lowerTeamText;
        [SerializeField] private Button _buttonPlayerP1;
        [SerializeField] private Button _buttonPlayerP2;
        [SerializeField] private Button _buttonPlayerP3;
        [SerializeField] private Button _buttonPlayerP4;
        [SerializeField] private Button _buttonGuest;
        [SerializeField] private Button _buttonSpectator;
        [SerializeField] private Button _buttonStartPlay;
        [SerializeField] private Button _buttonRaidTest;

        [Header("Live Data"), SerializeField] private int _localPlayerPosition;
        [SerializeField] private bool _isLocalPlayerPositionUnique;
        [SerializeField] private int _masterClientPosition;


        private bool _interactablePlayerP1;
        private bool _interactablePlayerP2;
        private bool _interactablePlayerP3;
        private bool _interactablePlayerP4;
        private bool _interactableGuest;
        private bool _interactableSpectator;
        private bool _interactableStartPlay;

        private string _captionPlayerP1;
        private string _captionPlayerP2;
        private string _captionPlayerP3;
        private string _captionPlayerP4;
        private string _captionGuest;
        private string _captionSpectator;

        PlayerRole currentRole = PlayerRole.Player;

        public enum PlayerRole
        {
            Player,
            Spectator
        }

        private void OnEnable()
        {
            Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState}");
            _buttonPlayerP1.interactable = false;
            _buttonPlayerP2.interactable = false;
            _buttonPlayerP3.interactable = false;
            _buttonPlayerP4.interactable = false;
            //_buttonGuest.interactable = false;
            //_buttonSpectator.interactable = false;
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
            var playerPos = PhotonLobbyRoom.GetFirstFreePlayerPos(player);
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var store = Storefront.Get();
            store.GetPlayerData(playerGuid, playerData =>
            {
                var battleCharacter = playerData.CurrentBattleCharacters;
                Debug.Log($"{battleCharacter[0]}");
                List<CustomCharacter> selectedCharacters = new();
                int[] characterIds = new int[5];
                int[] characterStats = new int[25];
                for (int i = 0; i < 3; i++)
                {
                    characterIds[i] = (int)battleCharacter[i].Id;
                    characterStats[i * 5] = battleCharacter[i].Hp;
                    characterStats[i * 5 + 1] = battleCharacter[i].Speed;
                    characterStats[i * 5 + 2] = battleCharacter[i].CharacterSize;
                    characterStats[i * 5 + 3] = battleCharacter[i].Attack;
                    characterStats[i * 5 + 4] = battleCharacter[i].Defence;
                    selectedCharacters.Add(battleCharacter[i]);
                }

                //var prefabIndex = PhotonBattle.GetPrefabIndex(battleCharacter[0], 0);
                var prefabIndex = (int)battleCharacter[0].Id;
                Debug.Log($"playerPos {playerPos} prefabIndex {characterIds}");
                player.SetCustomProperties(new LobbyPhotonHashtable(new Dictionary<object, object>
                {
                    { PlayerPositionKey, playerPos },
                    { PlayerMainSkillKey, prefabIndex },
                    { PlayerCharactersKey, characterIds },
                    { PlayerStatsKey, characterStats },
                    { "Role", (int)currentRole }
                }));
                LobbyManager.Instance.SetPlayerQuantumCharacters(selectedCharacters);
                Debug.Log($"{PhotonRealtimeClient.LobbyNetworkClientState} {enabled}");
                UpdateStatus();
            });
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
            _localPlayerPosition = localPlayer.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
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

            SetButton(_buttonPlayerP1, _interactablePlayerP1, _captionPlayerP1);
            SetButton(_buttonPlayerP2, _interactablePlayerP2, _captionPlayerP2);
            SetButton(_buttonPlayerP3, _interactablePlayerP3, _captionPlayerP3);
            SetButton(_buttonPlayerP4, _interactablePlayerP4, _captionPlayerP4);
            SetButton(_buttonGuest, _interactableGuest, _captionGuest);
            SetButton(_buttonSpectator, _interactableSpectator, _captionSpectator);
            SetButton(_buttonStartPlay, _interactableStartPlay, null);
            SetButton(_buttonRaidTest, _interactableStartPlay, null);

            if (_localPlayerPosition >= 0 && _localPlayerPosition <= 3 &&
                _masterClientPosition >= 0 && _masterClientPosition <= 3)
            {
                _upperTeamText.gameObject.SetActive(true);
                _lowerTeamText.gameObject.SetActive(true);
                SetTeamText();
            }
            else
            {
                _upperTeamText.gameObject.SetActive(false);
                _lowerTeamText.gameObject.SetActive(false);
            }
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
                _upperTeamText.text = $"Team {room.GetCustomProperty<string>(TeamRedNameKey)}";
                _lowerTeamText.text = $"Team {room.GetCustomProperty<string>(TeamBlueNameKey)}";
            }
            else
            {
                _upperTeamText.text = $"Team {room.GetCustomProperty<string>(TeamBlueNameKey)}";
                _lowerTeamText.text = $"Team {room.GetCustomProperty<string>(TeamRedNameKey)}";
            }
        }

        private static int GetTeam(int playerPos)
        {
            if (playerPos == PlayerPosition1 || playerPos == PlayerPosition3)
            {
                return TeamBlueValue;
            }
            if (playerPos == PlayerPosition4 || playerPos == PlayerPosition2)
            {
                return TeamRedValue;
            }
            return -1;
        }

        private void CheckMasterClient()
        {
            var curValue = PhotonRealtimeClient.LobbyCurrentRoom.GetPlayer(PhotonRealtimeClient.LobbyCurrentRoom.MasterClientId).GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            _masterClientPosition = curValue;
        }

        private void CheckOtherPlayer(LobbyPlayer player)
        {
            Debug.Log($"checkOtherPlayer {player.GetDebugLabel()}");
            if (!player.HasCustomProperty(PlayerPositionKey))
            {
                return;
            }
            var curValue = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            if (_isLocalPlayerPositionUnique && curValue >= PlayerPosition1 && curValue <= PlayerPosition4)
            {
                if (curValue == _localPlayerPosition)
                {
                    Debug.Log("detected conflict");
                    _isLocalPlayerPositionUnique = false; // Conflict with player positions!
                }
            }
            switch (curValue)
            {
                case PlayerPosition1:
                    _interactablePlayerP1 = false;
                    _captionPlayerP1 = player.NickName;
                    break;
                case PlayerPosition2:
                    _interactablePlayerP2 = false;
                    _captionPlayerP2 = player.NickName;
                    break;
                case PlayerPosition3:
                    _interactablePlayerP3 = false;
                    _captionPlayerP3 = player.NickName;
                    break;
                case PlayerPosition4:
                    _interactablePlayerP4 = false;
                    _captionPlayerP4 = player.NickName;
                    break;
            }
        }

        private void CheckLocalPlayer(LobbyPlayer player)
        {
            Debug.Log($"checkLocalPlayer {player.GetDebugLabel()} pos={_localPlayerPosition} ok={_isLocalPlayerPositionUnique}");
            var curValue = player.GetCustomProperty(PlayerPositionKey, PlayerPositionGuest);
            // Master client can *only* start the game when in room as player!
            _interactableStartPlay = player.IsMasterClient && curValue >= PlayerPosition1 && curValue <= PlayerPosition4;
            switch (curValue)
            {
                case PlayerPosition1:
                    _captionPlayerP1 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPosition2:
                    _captionPlayerP2 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPosition3:
                    _captionPlayerP3 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPosition4:
                    _captionPlayerP4 = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPositionGuest:
                    _interactableGuest = false;
                    _captionGuest = $"<color=blue>{player.NickName}</color>";
                    break;
                case PlayerPositionSpectator:
                    _interactableSpectator = false;
                    _captionSpectator = $"<color=blue>{player.NickName}</color>";
                    break;
            }
        }

        private void ButtonState(int _buttonStateNumber, string _buttonCaption)
        {
            _buttonStateNumber++;
            switch (_buttonStateNumber)
            {
                case 0:
                    _buttonCaption = $"Free";
                    Debug.Log(_buttonCaption);
                    break;

                case 1:
                    _buttonCaption = $"Player Name";
                    Debug.Log(_buttonCaption);
                    break;

                case 2:
                    _buttonCaption = $"Bot";
                    Debug.Log(_buttonCaption);
                    break;

                case 3:
                    _buttonCaption = $"Closed";
                    Debug.Log(_buttonCaption);
                    break;
                case 4:
                    _buttonStateNumber = 0;
                    break;
            }
        }

        private void ResetState()
        {
            _interactablePlayerP1 = true;
            _interactablePlayerP2 = true;
            _interactablePlayerP3 = true;
            _interactablePlayerP4 = true;
            _interactableGuest = true;
            // Spectator does not work with current NetworkSync and RoomLoader2,
            // it is from earlier versions with different implementation how room was initialized.
            _interactableSpectator = false; //true;
            _interactableStartPlay = false;

            _captionPlayerP1 = $"Free";
            _captionPlayerP2 = $"Free";
            _captionPlayerP3 = $"Free";
            _captionPlayerP4 = $"Free";
            _captionGuest = "Guest";
            _captionSpectator = "Spectator";
        }

        private static void SetButton(Selectable selectable, bool interactable, string caption)
        {
            if (selectable == null) return;
            selectable.interactable = interactable;
            if (!string.IsNullOrEmpty(caption))
            {
                selectable.GetComponentInChildren<TextMeshProUGUI>().text = caption;
            }
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

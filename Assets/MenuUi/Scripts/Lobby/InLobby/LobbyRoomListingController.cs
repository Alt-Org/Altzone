using System;
using System.Collections;
using System.Linq;
using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Common.Photon;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Lobby.CreateRoom;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using PopupSignalBus = MenuUI.Scripts.SignalBus;

namespace MenuUi.Scripts.Lobby.InLobby
{
    /// <summary>
    /// Handles calling the photon methods for creating a new room or joining a room, and forwarding the photon room list to RoomSearchPanelController.
    /// </summary>
    public class LobbyRoomListingController : AltMonoBehaviour
    {
        private const string DefaultRoomNameCustom = "Custom ";
        private const string DefaultRoomNameClan2v2 = "Clan 2v2 ";

        [SerializeField] private RoomSearchPanelController _searchPanel;
        [SerializeField] private BattlePopupPanelManager _roomSwitcher;
        [SerializeField] private CreateRoomCustom _createRoomCustom;
        [SerializeField] private PasswordPopup _passwordPopup;
        [SerializeField] private GameObject _creatingRoomText;

        private PhotonRoomList _photonRoomList;

        [HideInInspector] public GameType SelectedGameType;

        private void Awake()
        {
            _photonRoomList = gameObject.GetOrAddComponent<PhotonRoomList>();
            _createRoomCustom.CreateRoomButton.onClick.RemoveAllListeners();
            _createRoomCustom.CreateRoomButton.onClick.AddListener(CreateCustomRoomOnClick);
        }

        public void OnEnable()
        {
            PhotonRealtimeClient.AddCallbackTarget(this);
            Debug.Log($"OnEnable {PhotonRealtimeClient.LobbyNetworkClientState}");
            if (PhotonRealtimeClient.InLobby)
            {
                UpdateStatus();
            }
            _photonRoomList.OnRoomsUpdated += UpdateStatus;
            LobbyManager.LobbyOnJoinedRoom += OnJoinedRoom;
            LobbyWindowNavigationHandler.OnLobbyWindowChangeRequest += SwitchToRoom;
        }

        public void OnDisable()
        {
            PhotonRealtimeClient.RemoveCallbackTarget(this);
            _photonRoomList.OnRoomsUpdated -= UpdateStatus;
            LobbyManager.LobbyOnJoinedRoom -= OnJoinedRoom;
            LobbyWindowNavigationHandler.OnLobbyWindowChangeRequest -= SwitchToRoom;
        }

        private void CreateCustomRoomOnClick()
        {
            var roomName = string.IsNullOrWhiteSpace(_createRoomCustom.RoomName) ? $"{DefaultRoomNameCustom}{DateTime.Now.Second:00}" : _createRoomCustom.RoomName;

            if (_createRoomCustom.IsPrivate && _createRoomCustom.RoomPassword != null && _createRoomCustom.RoomPassword != "")
            {
                PhotonRealtimeClient.CreateLobbyRoom(roomName, GameType.Custom, _createRoomCustom.RoomPassword);
            }
            else
            {
                PhotonRealtimeClient.CreateLobbyRoom(roomName, GameType.Custom);
            }
        }

        /// <summary>
        /// Coroutine to create Clan2v2 room after client is connected to lobby.
        /// </summary>
        /// <returns></returns>
        public IEnumerator StartCreatingClan2v2Room(Action callback)
        {
            _creatingRoomText.SetActive(true);
            bool roomCreated = false;
            do
            {
                if (PhotonRealtimeClient.InLobby)
                {
                    CreateClan2v2Room();
                    roomCreated = true;
                }
            } while (!roomCreated);

            callback();

            yield break;
        }

        private void CreateClan2v2Room()  // soulhome value for matchmaking
        {
            var roomName = $"{DefaultRoomNameClan2v2}{DateTime.Now.Second:00}";
            StartCoroutine(GetClanData( clanData =>
            {
                if (clanData != null)
                {
                    PhotonRealtimeClient.JoinRandomOrCreateLobbyRoom(roomName, GameType.Clan2v2, clanData.Name);
                }
            }));
        }

        private void JoinRoom(string roomName)
        {
            Debug.Log($"{roomName}");
            var rooms = _photonRoomList.CurrentRooms.ToList();
            foreach (var roomInfo in rooms)
            {
                if (roomInfo.Name.Equals(roomName, StringComparison.Ordinal) && !roomInfo.RemovedFromList && roomInfo.IsOpen)
                {
                    // Asking for password if there is one
                    if (roomInfo.CustomProperties.ContainsKey(PhotonBattleRoom.PasswordKey))
                    {
                        string password = (string)roomInfo.CustomProperties[PhotonBattleRoom.PasswordKey];
                        StartCoroutine(_passwordPopup.AskForPassword(passwordInput =>
                        {
                            if (password.Trim() == passwordInput.Trim())
                            {
                                PhotonRealtimeClient.JoinRoom(roomInfo.Name);
                            }
                            else
                            {
                                PopupSignalBus.OnChangePopupInfoSignal("Salasana on väärin.");
                            }
                            _passwordPopup.ClosePopup();
                        }));
                        return;
                    }

                    PhotonRealtimeClient.JoinRoom(roomInfo.Name);
                    break;
                }
            }
        }

        public void OnJoinedRoom()
        {
            if (_creatingRoomText.activeSelf) _creatingRoomText.SetActive(false);
            var room = PhotonRealtimeClient.LobbyCurrentRoom; // hakee pelaajan tiedot // 
            var player = PhotonRealtimeClient.LocalLobbyPlayer;
            //PhotonRealtimeClient.NickName = room.GetUniquePlayerNameForRoom(player, PhotonRealtimeClient.NickName, "");
            Debug.Log($"'{room.Name}' player name '{PhotonRealtimeClient.NickName}'");
            this.Publish(new LobbyManager.StartRoomEvent());
        }

        public void SwitchToRoom()
        {
            _roomSwitcher.SwitchRoom(SelectedGameType);
        }

        private void UpdateStatus()
        {
            var rooms = _photonRoomList.CurrentRooms.ToList();
            rooms.Sort((a, b) =>
            {
                // First open rooms by name, then closed (aka playing) rooms by name
                var strA = $"{(a.IsOpen ? 0 : 1)}{a.Name}";
                var strB = $"{(b.IsOpen ? 0 : 1)}{b.Name}";
                return string.Compare(strA, strB, StringComparison.Ordinal);
            });
            _searchPanel.RoomsData = rooms;
            _searchPanel.SetOnJoinRoom(JoinRoom);
        }
    }
}

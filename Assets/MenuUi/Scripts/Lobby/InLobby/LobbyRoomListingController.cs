using System;
using System.Collections;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Prg.Scripts.Common.PubSub;

using Altzone.Scripts.Battle.Photon;
using Altzone.Scripts.Common.Photon;
using Altzone.Scripts.Lobby;
using ReasonType = Altzone.Scripts.Lobby.LobbyManager.GetKickedEvent.ReasonType;

using MenuUi.Scripts.Lobby.CreateRoom;
using PopupSignalBus = MenuUI.Scripts.SignalBus;
using MenuUi.Scripts.Signals;

namespace MenuUi.Scripts.Lobby.InLobby
{
    /// <summary>
    /// Handles calling the photon methods for creating a new room or joining a room, and forwarding the photon room list to RoomSearchPanelController.
    /// </summary>
    public class LobbyRoomListingController : AltMonoBehaviour
    {
        private const string DefaultRoomNameCustom = "Custom ";

        [SerializeField] private RoomSearchPanelController _searchPanel;
        [SerializeField] private BattlePopupPanelManager _roomSwitcher;
        [SerializeField] private CreateRoomCustom _createRoomCustom;
        [SerializeField] private Button _createRoomFromMainMenuButton;
        [SerializeField] private PasswordPopup _passwordPopup;
        [SerializeField] private GameObject _creatingRoomText;

        private PhotonRoomList _photonRoomList;

        private void Awake()
        {
            _photonRoomList = gameObject.GetOrAddComponent<PhotonRoomList>();
            _createRoomCustom.CreateRoomButton.onClick.AddListener(CreateCustomRoom);
            _createRoomFromMainMenuButton.onClick.AddListener(CreateCustomRoom);
            LobbyManager.OnClanMemberDisconnected += HandleClanMemberDisconnected;
            LobbyManager.OnKickedOutOfTheRoom += HandleKickedOutOfRoom;
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
            LobbyManager.LobbyOnJoinRoomFailed += OnJoinedRoomFailed;
            LobbyWindowNavigationHandler.OnLobbyWindowChangeRequest += SwitchToRoom;
        }

        public void OnDisable()
        {
            PhotonRealtimeClient.RemoveCallbackTarget(this);
            _photonRoomList.OnRoomsUpdated -= UpdateStatus;
            LobbyManager.LobbyOnJoinedRoom -= OnJoinedRoom;
            LobbyWindowNavigationHandler.OnLobbyWindowChangeRequest -= SwitchToRoom;
        }

        private void OnDestroy()
        {
            LobbyManager.OnClanMemberDisconnected -= HandleClanMemberDisconnected;
            LobbyManager.OnKickedOutOfTheRoom -= HandleKickedOutOfRoom;
            _createRoomCustom.CreateRoomButton.onClick.RemoveAllListeners();
            _createRoomFromMainMenuButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Coroutine to create a room after client is connected to lobby.
        /// </summary>
        /// <param name="gameType">Game type of which room to create.</param>
        /// <param name="callback">Callback which is called after room is created.</param>
        /// <returns></returns>
        public IEnumerator StartCreatingRoom(GameType gameType, Action callback)
        {
            _creatingRoomText.SetActive(true);
            bool roomCreated = false;
            do
            {
                yield return null;
                if (PhotonRealtimeClient.InLobby)
                {
                    switch (gameType)
                    {
                        case GameType.Clan2v2:
                            CreateClan2v2Room();
                            break;
                        case GameType.Random2v2:
                            CreateRandom2v2Room();
                            break;
                        case GameType.Custom:
                            if (!_createRoomCustom.IsCustomRoomOptionsReady) _createRoomCustom.InitializeCustomRoomOptions();
                            CreateCustomRoom();
                            break;
                    }
                    roomCreated = true;
                }
            } while (!roomCreated);

            callback();
        }

        private void CreateCustomRoom()
        {
            // int randomNumber = UnityEngine.Random.Range(0, 100) + DateTime.Now.Millisecond;
            string roomName = string.IsNullOrWhiteSpace(_createRoomCustom.RoomName) ? $"{DefaultRoomNameCustom}" : $"{_createRoomCustom.RoomName}";

            if (_createRoomCustom.IsPrivate && _createRoomCustom.RoomPassword != null && _createRoomCustom.RoomPassword != "")
            {
                PhotonRealtimeClient.CreateCustomLobbyRoom(roomName, _createRoomCustom.SelectedMapId, _createRoomCustom.SelectedEmotion, _createRoomCustom.RoomPassword);
            }
            else
            {
                PhotonRealtimeClient.JoinRandomOrCreateCustomRoom(roomName, _createRoomCustom.SelectedMapId, _createRoomCustom.SelectedEmotion);
            }
        }

        private void CreateClan2v2Room()  // soulhome value for matchmaking
        {
            StartCoroutine(GetClanData( clanData =>
            {
                if (clanData != null)
                {
                    PhotonRealtimeClient.JoinRandomOrCreateClan2v2Room(clanData.Name, UnityEngine.Random.Range(0,5001));
                }
            }));
        }

        private void CreateRandom2v2Room()  // soulhome value for matchmaking
        {
            StartCoroutine(GetClanData(clanData =>
            {
                if (clanData != null)
                {
                    PhotonRealtimeClient.CreateRandom2v2LobbyRoom();
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

        public void OnJoinedRoomFailed(short returnCode, string message)
        {
            CreateCustomRoom();
        }

        public void SwitchToRoom()
        {
            _roomSwitcher.SwitchRoom(InLobbyController.SelectedGameType);
        }

        private void UpdateStatus()
        {
            LobbyManager.Instance.CurrentRooms = _photonRoomList.CurrentRooms;
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

        private void HandleClanMemberDisconnected()
        {
            PopupSignalBus.OnChangePopupInfoSignal("Pelin etsiminen lopetetaan. Klaanin jäsen sulki pelin.");
        }

        private void HandleKickedOutOfRoom(ReasonType reason)
        {
            switch (reason)
            {
                case ReasonType.FullRoom:
                    PopupSignalBus.OnChangePopupInfoSignal("Virhe pelin etsimisessä, huone on täysi.");
                    CreateCustomRoom();
                    break;
                case ReasonType.RoomLeader:
                    PopupSignalBus.OnChangePopupInfoSignal("Huoneen johtaja poisti sinut huoneesta.");
                    CreateCustomRoom();
                    break;
            }
            SignalBus.OnCloseBattlePopupRequestedSignal();
        }
    }
}

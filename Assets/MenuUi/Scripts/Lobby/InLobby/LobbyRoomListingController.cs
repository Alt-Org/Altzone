using System;
using System.Linq;
using Altzone.Scripts.Common.Photon;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Lobby.CreateRoom;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Lobby.InLobby
{
    public class LobbyRoomListingController : MonoBehaviour
    {
        private const string DefaultRoomNameName = "Battle ";

        [SerializeField] private RoomSearchPanelController _searchPanel;
        [SerializeField] private BattlePopupPanelManager _roomSwitcher;
        [SerializeField] private CreateRoomCustom _createRoomCustom;

        private PhotonRoomList _photonRoomList;

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
            var roomName = string.IsNullOrWhiteSpace(_createRoomCustom.RoomName) ? $"{DefaultRoomNameName}{DateTime.Now.Second:00}" : _createRoomCustom.RoomName;

            if (_createRoomCustom.IsPrivate && _createRoomCustom.RoomPassword != null && _createRoomCustom.RoomPassword != "")
            {
                PhotonRealtimeClient.CreateLobbyRoom(roomName, _createRoomCustom.RoomPassword);
            }
            else
            {
                PhotonRealtimeClient.CreateLobbyRoom(roomName);
            }
        }


        private void JoinRoom(string roomName)
        {
            Debug.Log($"{roomName}");
            var rooms = _photonRoomList.CurrentRooms.ToList();
            foreach (var roomInfo in rooms)
            {
                if (roomInfo.Name.Equals(roomName, StringComparison.Ordinal) && !roomInfo.RemovedFromList && roomInfo.IsOpen)
                {
                    PhotonRealtimeClient.JoinRoom(roomInfo.Name);
                    break;
                }
            }
        }

        public void OnJoinedRoom()
        {
            var room = PhotonRealtimeClient.LobbyCurrentRoom; // hakee pelaajan tiedot // 
            var player = PhotonRealtimeClient.LocalLobbyPlayer;
            //PhotonRealtimeClient.NickName = room.GetUniquePlayerNameForRoom(player, PhotonRealtimeClient.NickName, "");
            Debug.Log($"'{room.Name}' player name '{PhotonRealtimeClient.NickName}'");
            this.Publish(new LobbyManager.StartRoomEvent());
        }

        public void SwitchToRoom()
        {
            _roomSwitcher.SwitchRoom();
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

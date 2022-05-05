using System;
using System.Linq;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;

namespace Lobby.Scripts.InLobby
{
    public class LobbyRoomListingController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private LobbyRoomListingView _view;
        [SerializeField] private WindowDef _roomWindow;
        
        private PhotonRoomList _photonRoomList;

        private void Awake()
        {
            _photonRoomList = gameObject.GetOrAddComponent<PhotonRoomList>();
            _view.RoomButtonOnClick = CreateRoom;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log($"OnEnable {PhotonNetwork.NetworkClientState}");
            _view.Reset();
            if (PhotonNetwork.InLobby)
            {
                UpdateStatus();
            }
            _photonRoomList.roomsUpdated += UpdateStatus;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _photonRoomList.roomsUpdated -= UpdateStatus;
            _view.Reset();
        }

        private static void CreateRoom()
        {
            var roomName = $"Room{DateTime.Now.Second:00}";
            Debug.Log($"{roomName}");
            PhotonLobby.CreateRoom(roomName);
        }

        private void JoinRoom(string roomName)
        {
            Debug.Log($"{roomName}");
            var rooms = _photonRoomList.currentRooms.ToList();
            foreach (var roomInfo in rooms)
            {
                if (roomInfo.Name.Equals(roomName, StringComparison.Ordinal) && !roomInfo.RemovedFromList && roomInfo.IsOpen)
                {
                    PhotonLobby.JoinRoom(roomInfo);
                    break;
                }
            }
        }
        
        public override void OnJoinedRoom()
        {
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            Debug.Log($"'{room.Name}' player name '{PhotonNetwork.NickName}'");
            WindowManager.Get().ShowWindow(_roomWindow);
        }
        
        private void UpdateStatus()
        {
            if (!PhotonNetwork.InLobby)
            {
                _view.Reset();
                return;
            }
            var rooms = _photonRoomList.currentRooms.ToList();
            rooms.Sort((a, b) =>
            {
                // First open rooms by name, then closed (aka playing) rooms by name
                var strA = $"{(a.IsOpen?0:1)}{a.Name}";
                var strB = $"{(b.IsOpen?0:1)}{b.Name}";
                return string.Compare(strA, strB, StringComparison.Ordinal);
            });
            _view.UpdateStatus(rooms, JoinRoom);
        }
    }
}
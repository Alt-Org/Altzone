#if false
using System;
using System.Linq;
using Battle1.PhotonUnityNetworking.Code;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonBattle = Altzone.Scripts.Battle.Photon.PhotonBattleRoom;
using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using RoomOptions = Battle1.PhotonRealtime.Code.RoomOptions;

namespace Battle1.Scripts.Lobby.InLobby
{
    public class LobbyRoomListingController : MonoBehaviourPunCallbacks
    {
        private const string DefaultRoomNameName = "Battle ";

        [SerializeField] private LobbyRoomListingView _view;
        [SerializeField] private TMP_InputField _roomName;

        private PhotonRoomList _photonRoomList;

        private void Awake()
        {
            _photonRoomList = gameObject.GetOrAddComponent<PhotonRoomList>();
            _view.RoomButtonOnClick = CreateRoomOnClick;
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
            _photonRoomList.OnRoomsUpdated += UpdateStatus;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _photonRoomList.OnRoomsUpdated -= UpdateStatus;
            _view.Reset();
        }

        private void CreateRoomOnClick()
        {
            var roomName = string.IsNullOrWhiteSpace(_roomName.text) ? $"{DefaultRoomNameName}{DateTime.Now.Second:00}" : _roomName.text;
            var roomOptions = new RoomOptions()
            {
                IsVisible = true, // Pit�� muokata varmaankin //
                IsOpen = true,
                MaxPlayers = 4
            };
            Debug.Log($"{roomName}");
            PhotonLobby.CreateRoom(roomName, roomOptions);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"CreateRoomFailed {returnCode} {message}");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"JoinRoomFailed {returnCode} {message}");
        }


        private void JoinRoom(string roomName)
        {
            Debug.Log($"{roomName}");
            var rooms = _photonRoomList.CurrentRooms.ToList();
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
            var room = PhotonNetwork.CurrentRoom; // hakee pelaajan tiedot // 
            var player = PhotonNetwork.LocalPlayer;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            Debug.Log($"'{room.Name}' player name '{PhotonNetwork.NickName}'");
            this.Publish(new LobbyManagerOld.StartRoomEvent());
        }
        
        private void UpdateStatus()
        {
            if (!PhotonNetwork.InLobby)
            {
                _view.Reset();
                return;
            }
            var rooms = _photonRoomList.CurrentRooms.ToList();
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
#endif

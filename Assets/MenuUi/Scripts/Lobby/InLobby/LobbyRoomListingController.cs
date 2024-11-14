using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Common.Photon;
//using Battle1.PhotonUnityNetworking.Code;
using Photon.Realtime;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonBattle = Altzone.Scripts.Battle.Photon.PhotonBattleRoom;
//using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
//using RoomOptions = Battle1.PhotonRealtime.Code.RoomOptions;

namespace MenuUI.Scripts.Lobby.InLobby
{
    public class LobbyRoomListingController : MonoBehaviour, IMatchmakingCallbacks
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

        public void OnEnable()
        {
            PhotonRealtimeClient.Client.AddCallbackTarget(this);
            Debug.Log($"OnEnable {PhotonRealtimeClient.NetworkClientState}");
            _view.Reset();
            if (PhotonRealtimeClient.InLobby)
            {
                UpdateStatus();
            }
            _photonRoomList.OnRoomsUpdated += UpdateStatus;
        }

        public void OnDisable()
        {
            PhotonRealtimeClient.Client.RemoveCallbackTarget(this);
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
                MaxPlayers = 4//,
                //Plugins = new string[] { "QuantumPlugin" }//,
                //PlayerTtl = PhotonRealtimeClient.ServerSettings.PlayerTtlInSeconds * 1000,
                //EmptyRoomTtl = PhotonRealtimeClient.ServerSettings.EmptyRoomTtlInSeconds * 1000
            };
            Debug.Log($"{roomName}");
            PhotonRealtimeClient.CreateRoom(roomName, roomOptions);
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"CreateRoomFailed {returnCode} {message}");
        }

        public void OnJoinRoomFailed(short returnCode, string message)
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
                    PhotonRealtimeClient.JoinRoom(roomInfo.Name);
                    break;
                }
            }
        }

        public void OnJoinedRoom()
        {
            var room = PhotonRealtimeClient.CurrentRoom; // hakee pelaajan tiedot // 
            var player = PhotonRealtimeClient.LocalPlayer;
            //PhotonRealtimeClient.NickName = room.GetUniquePlayerNameForRoom(player, PhotonRealtimeClient.NickName, "");
            Debug.Log($"'{room.Name}' player name '{PhotonRealtimeClient.NickName}'");
            this.Publish(new LobbyManager.StartRoomEvent());
        }

        private void UpdateStatus()
        {
            if (!PhotonRealtimeClient.InLobby)
            {
                _view.Reset();
                return;
            }
            var rooms = _photonRoomList.CurrentRooms.ToList();
            rooms.Sort((a, b) =>
            {
                // First open rooms by name, then closed (aka playing) rooms by name
                var strA = $"{(a.IsOpen ? 0 : 1)}{a.Name}";
                var strB = $"{(b.IsOpen ? 0 : 1)}{b.Name}";
                return string.Compare(strA, strB, StringComparison.Ordinal);
            });
            _view.UpdateStatus(rooms, JoinRoom);
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList) => throw new NotImplementedException();
        public void OnCreatedRoom()
        {
            Debug.Log($"Created room {PhotonRealtimeClient.Client.CurrentRoom.Name}");
        }
        public void OnJoinRandomFailed(short returnCode, string message) => throw new NotImplementedException();
        public void OnLeftRoom()
        {
            Debug.Log($"Left room");
        }
    }
}

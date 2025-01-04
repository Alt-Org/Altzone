#if false
using System;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
/*using Battle1.PhotonUnityNetworking.Code;*/
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using RoomOptions = Battle1.PhotonRealtime.Code.RoomOptions;
using TypedLobby = Battle1.PhotonRealtime.Code.TypedLobby;*/

namespace Battle1.Scripts.Test
{
    /// <summary>
    /// <c>TestRoomLoader</c> creates a new test room unless it is in a room already.
    /// </summary>
    internal class RoomLoaderTest : MonoBehaviourPunCallbacks
    {
        private const string Tooltip1 = "If 'Is Offline Mode' only one player can play";

        [Header("Debug Settings"), Tooltip(Tooltip1), SerializeField] private bool _isOfflineMode;
        [SerializeField] private string _roomName;

        private IEnumerator Start()
        {
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            var playerGuid = playerSettings.PlayerGuid;
            var store = Storefront.Get();
            PlayerData playerData = null;
            store.GetPlayerData(playerGuid, p => playerData = p);
            yield return new WaitUntil(() => playerData != null);
            PhotonNetwork.NickName = string.IsNullOrWhiteSpace(playerData.Name) ? playerData.Name : "Player";
            Debug.Log($"{PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
            PhotonNetwork.OfflineMode = _isOfflineMode;
            for (; enabled;)
            {
                if (PhotonNetwork.InRoom)
                {
                    // We are in a room - validate our player name.
                    var room = PhotonNetwork.CurrentRoom;
                    var player = PhotonNetwork.LocalPlayer;
                    PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
                    enabled = false;
                    yield break;
                }
                if (_isOfflineMode)
                {
                    // JoinRandomRoom -> OnJoinedRoom
                    PhotonNetwork.JoinRandomRoom();
                }
                else if (PhotonWrapper.CanConnect)
                {
                    // Connect -> ConnectedToMasterServer
                    PhotonLobby.Connect(PhotonNetwork.NickName);
                }
                else if (PhotonWrapper.CanJoinLobby)
                {
                    // OnJoinedLobby -> JoinOrCreateRoom -> OnJoinedRoom
                    PhotonLobby.JoinLobby();
                }
                yield return null;
            }
        }

        public override void OnJoinedLobby()
        {
            if (string.IsNullOrWhiteSpace(_roomName))
            {
                _roomName = Environment.MachineName;
            }
            RoomOptions roomOptions = new RoomOptions()
            {
                IsVisible = false,
                IsOpen = false,
                MaxPlayers = 5
            };
            Debug.Log($"JoinOrCreateRoom {PhotonNetwork.NetworkClientState} room {_roomName}");
            PhotonNetwork.JoinOrCreateRoom(_roomName, roomOptions, TypedLobby.Default);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"{PhotonNetwork.NetworkClientState} Error {returnCode} {message}");
            enabled = false;
        }
    }
}
#endif

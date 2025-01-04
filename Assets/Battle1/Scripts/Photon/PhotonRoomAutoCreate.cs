#if false
using System.Collections;
using Battle1.PhotonUnityNetworking.Code;
using Photon.Realtime;
using Prg.Scripts.EditorSupport.Attributes;
using UnityEngine;
using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using RoomOptions = Battle1.PhotonRealtime.Code.RoomOptions;
using TypedLobby = Battle1.PhotonRealtime.Code.TypedLobby;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Create a Photon room (for testing).
    /// </summary>
    public class PhotonRoomAutoCreate : MonoBehaviourPunCallbacks
    {
        private const string DefaultPlayerName = "Player";

        [SerializeField, ReadOnly] private string _roomName;

        private IEnumerator Start()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
            while (enabled)
            {
                if (PhotonNetwork.InRoom)
                {
                    // We are in a room - validate our player name.
                    var room = PhotonNetwork.CurrentRoom;
                    var player = PhotonNetwork.LocalPlayer;
                    PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
                    yield break;
                }
                if (PhotonNetwork.OfflineMode)
                {
                    // JoinRandomRoom -> OnJoinedRoom
                    PhotonNetwork.JoinRandomRoom();
                }
                else if (PhotonWrapper.CanConnect)
                {
                    // Connect -> ConnectedToMasterServer
                    PhotonLobby.Connect(string.IsNullOrEmpty(PhotonNetwork.NickName) ? DefaultPlayerName : PhotonNetwork.NickName);
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
                _roomName = $"Room {Random.Range(100, 999)}";
            }
            RoomOptions roomOptions = new RoomOptions()
            {
                IsVisible = true,
                IsOpen = true,
                MaxPlayers = 0
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

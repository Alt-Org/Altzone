using System.Collections;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity.ToastMessages;
using UnityEngine;

namespace Battle.Test.Scripts.Test
{
    /// <summary>
    /// <c>TestRoomLoader</c> creates a new test room unless it is in a room already.
    /// </summary>
    internal class RoomLoaderTest : MonoBehaviourPunCallbacks
    {
        private const string Tooltip1 = "If 'Is Offline Mode' only one player can play";

        private const string TestRoomName = "TestBattle";

        [Header("Debug Settings"), Tooltip(Tooltip1), SerializeField] private bool _isOfflineMode;

        private IEnumerator Start()
        {
            PhotonNetwork.NickName = PhotonBattle.GetLocalPlayerName();
            Debug.Log($"{PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
            PhotonNetwork.OfflineMode = _isOfflineMode;
            for (; enabled;)
            {
                if (PhotonNetwork.InRoom)
                {
                    // We are in a room.
                    ShowRoomJoinedMessage();
                    enabled = false;
                    yield break;
                }
                if (_isOfflineMode)
                {
                    // JoinRandomRoom -> OnJoinedRoom
                    PhotonNetwork.JoinRandomRoom();
                }
                else if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated || PhotonNetwork.NetworkClientState == ClientState.Disconnected)
                {
                    // Connect -> ConnectedToMasterServer
                    PhotonLobby.Connect(PhotonNetwork.NickName);
                }
                else if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
                {
                    // OnJoinedLobby -> JoinOrCreateRoom -> OnJoinedRoom
                    PhotonLobby.JoinLobby();
                }
                yield return null;
            }
        }

        public override void OnJoinedLobby()
        {
            Debug.Log($"JoinOrCreateRoom {PhotonNetwork.NetworkClientState}");
            PhotonLobby.JoinOrCreateRoom(TestRoomName);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"{PhotonNetwork.NetworkClientState} Error {returnCode} {message}");
            enabled = false;
        }

        private static void ShowRoomJoinedMessage()
        {
            if (AppPlatform.IsEditor || AppPlatform.IsDevelopmentBuild)
            {
                return;
            }
            ScoreFlash.Push("DEVELOPMENT\r\nBUILD\r\nREQUIRED\r\nFOR TESTING");
        }
    }
}
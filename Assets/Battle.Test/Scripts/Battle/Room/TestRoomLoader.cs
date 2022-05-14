using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Room
{
    /// <summary>
    /// <c>TestRoomLoader</c> creates a new test room unless it is in a room already.
    /// </summary>
    internal class TestRoomLoader : MonoBehaviourPunCallbacks
    {
        [Serializable]
        internal class DebugSettings
        {
            public const string TestRoomName = "TestBattle";

            private const string Tooltip1 = "If 'Is Offline Mode' only one player can play";

            [Tooltip(Tooltip1)] public bool _isOfflineMode;
        }

        [SerializeField, Header("Debug Settings")] private DebugSettings _debug;

        private IEnumerator Start()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
            PhotonNetwork.OfflineMode = _debug._isOfflineMode;
            PhotonNetwork.NickName = PhotonBattle.GetLocalPlayerName();
            for (; enabled;)
            {
                if (PhotonNetwork.InRoom)
                {
                    // We are in a room and that's it.
                    enabled = false;
                    yield break;
                }
                if (_debug._isOfflineMode)
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
            PhotonLobby.JoinOrCreateRoom(DebugSettings.TestRoomName);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"{PhotonNetwork.NetworkClientState} Error {returnCode} {message}");
            enabled = false;
        }
    }
}
using System.Collections;
using System.Linq;
using Altzone.Scripts.Battle;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Battle.Scripts.Battle.Room
{
    /// <summary>
    /// Game room loader to establish a well known state for the room before actual gameplay starts.
    /// </summary>
    /// <remarks>
    /// Can create a test room environment in Editor for testing single player stuff.
    /// </remarks>
    internal class RoomLoader : MonoBehaviourPunCallbacks
    {
        [Header("Settings"), SerializeField] private bool _isOfflineMode;
        [SerializeField, Range(1,4)] private int _debugPlayerPos;
        [SerializeField] private GameObject[] _objectsToActivate;

        private void Awake()
        {
            Debug.Log($"Awake {PhotonNetwork.NetworkClientState}");
            if (_objectsToActivate.Any(x => x.activeSelf))
            {
                Debug.LogError("objectsToActivate has active items, disable them and retry");
                enabled = false;
                return;
            }
            if (PhotonNetwork.InRoom)
            {
                // Normal logic is that we are in a room and just do what we must do and continue.
                ContinueToNextStage();
                enabled = false;
                return;
            }
            Debug.Log($"Awake and create test room {PhotonNetwork.NetworkClientState}");
        }

        public override void OnEnable()
        {
            // Create a test room - in offline (faster to create) or online (real thing) mode
            base.OnEnable();
            var state = PhotonNetwork.NetworkClientState;
            var isStateValid = state == ClientState.PeerCreated || state == ClientState.Disconnected;
            if (!isStateValid)
            {
                throw new UnityException($"OnEnable: invalid connection state {PhotonNetwork.NetworkClientState}");
            }
            var playerName = PhotonBattle.GetLocalPlayerName();
            Debug.Log($"connect {PhotonNetwork.NetworkClientState} isOfflineMode {_isOfflineMode} player {playerName}");
            PhotonNetwork.OfflineMode = _isOfflineMode;
            if (_isOfflineMode)
            {
                // JoinRandomRoom -> CreateRoom -> OnJoinedRoom -> OnPlayerPropertiesUpdate -> ContinueToNextStage
                PhotonNetwork.NickName = playerName;
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // Connect -> JoinLobby -> CreateRoom -> OnJoinedRoom -> OnPlayerPropertiesUpdate -> ContinueToNextStage
                PhotonLobby.Connect(playerName);
            }
        }

        private void ContinueToNextStage()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Mark room "closed"
                PhotonLobby.CloseRoom(true);
            }
            // Enable game objects when this room stage is ready to play
            StartCoroutine(ActivateObjects(_objectsToActivate));
        }

        private static IEnumerator ActivateObjects(GameObject[] objectsToActivate)
        {
            // Enable game objects one per frame in array sequence
            for (var i = 0; i < objectsToActivate.LongLength; i++)
            {
                yield return null;
                objectsToActivate[i].SetActive(true);
            }
        }

        public override void OnConnectedToMaster()
        {
            if (!_isOfflineMode)
            {
                Debug.Log($"joinLobby {PhotonNetwork.NetworkClientState}");
                PhotonLobby.JoinLobby();
            }
        }

        public override void OnJoinedLobby()
        {
            Debug.Log($"createRoom {PhotonNetwork.NetworkClientState}");
            PhotonLobby.CreateRoom("testing");
        }

        public override void OnJoinedRoom()
        {
            PhotonBattle.SetDebugPlayerProps(PhotonNetwork.LocalPlayer, _debugPlayerPos);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            Debug.Log($"Start game for {targetPlayer.GetDebugLabel()}");
            ContinueToNextStage();
        }
    }
}
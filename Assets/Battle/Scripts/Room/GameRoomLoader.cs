using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Scene;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Battle.Scripts.Room
{
    /// <summary>
    /// Game room loader to establish a well known state if level is loaded directly from Editor.
    /// </summary>
    public class GameRoomLoader : MonoBehaviourPunCallbacks
    {
        [Header("Settings"), SerializeField] private bool _isOfflineMode;
        [SerializeField, Range(1, 4)] private int _debugPlayerPos;
        [SerializeField] private GameObject[] _objectsToManage;

        private void Awake()
        {
            if (PhotonNetwork.InRoom)
            {
                continueToNextStage();
                return;
            }
            Debug.Log($"Awake: {PhotonNetwork.NetworkClientState}");
            prepareCurrentStage();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected)
            {
                Debug.Log($"connect: {PhotonNetwork.NetworkClientState}");
                var playerData = RuntimeGameConfig.Get().PlayerDataCache;
                //PhotonLobby.isAllowOfflineMode = isOfflineMode;
                PhotonNetwork.OfflineMode = _isOfflineMode;
                if (_isOfflineMode)
                {
                    PhotonNetwork.NickName = playerData.PlayerName;
                    PhotonNetwork.JoinRandomRoom();
                }
                else
                {
                    PhotonLobby.connect(playerData.PlayerName);
                }
                return;
            }
            throw new UnityException($"OnEnable: invalid connection state: {PhotonNetwork.NetworkClientState}");
        }

        private void prepareCurrentStage()
        {
            // Disable game objects until this room stage is ready
            Array.ForEach(_objectsToManage, x => x.SetActive(false));
        }

        private void continueToNextStage()
        {
            enabled = false;
            if (PhotonNetwork.IsMasterClient)
            {
                // Mark room "closed"
                PhotonLobby.closeRoom(keepVisible: true);
            }
            // Create walls
            var gameArena = SceneConfig.Get().gameArena;
            gameArena.makeWalls();
            // Enable game objects when this room stage is ready to play
            Array.ForEach(_objectsToManage, x => x.SetActive(true));
        }

        public override void OnConnectedToMaster()
        {
            if (!_isOfflineMode)
            {
                Debug.Log($"joinLobby: {PhotonNetwork.NetworkClientState}");
                PhotonLobby.joinLobby();
            }
        }

        public override void OnJoinedLobby()
        {
            Debug.Log($"createRoom: {PhotonNetwork.NetworkClientState}");
            PhotonLobby.createRoom("testing");
        }

        public override void OnJoinedRoom()
        {
            PhotonBattle.SetDebugPlayerProps(PhotonNetwork.LocalPlayer, _debugPlayerPos);
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"Start game for: {player.GetDebugLabel()}");
            continueToNextStage();
        }
    }
}
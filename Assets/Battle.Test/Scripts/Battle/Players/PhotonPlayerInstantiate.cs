using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Battle.Scripts.Battle.Factory;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// Class to instantiate local Photon player using <code>PhotonNetwork.Instantiate</code>.
    /// </summary>
    internal class PhotonPlayerInstantiate : MonoBehaviourPunCallbacks
    {
        [Serializable]
        internal class DebugSettings
        {
            [Range(1, 4)] public int _playerPos = 1;
            public Defence _playerMainSkill = Defence.Deflection;
        }

        [Header("Prefab Settings"), SerializeField] private PlayerDriver _photonPrefab;
        [SerializeField] private PlayerActor _playerPrefab;
        
        [Header("Live Data"), SerializeField] private PlayerDriver _localPhotonInstance;
        
        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private bool _isLocalPlayerInstantiated;
        
        public override void OnEnable()
        {
            Assert.IsNotNull(_photonPrefab, "_photonPrefab != null");
            Assert.IsNotNull(_playerPrefab, "_playerPrefab != null");
            base.OnEnable();
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"{PhotonNetwork.NetworkClientState} {player.GetDebugLabel()}");
            if (PhotonNetwork.InRoom)
            {
                OnPhotonPlayerReady();
            }
        }

        public override void OnJoinedRoom()
        {
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"{PhotonNetwork.NetworkClientState} {room.GetDebugLabel()}");
            Debug.Log($"{player.GetDebugLabel()}");
            var playerPos = PhotonBattle.GetPlayerPos(player);
            if (PhotonBattle.IsValidGameplayPos(playerPos))
            {
                OnPhotonPlayerReady();
                return;
            }
            PhotonBattle.SetDebugPlayerProps(player, _debug._playerPos, (int)_debug._playerMainSkill);
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            if (_isLocalPlayerInstantiated)
            {
                return;
            }
            var player = PhotonNetwork.LocalPlayer;
            if (!player.Equals(targetPlayer))
            {
                return;
            }
            Debug.Log($"{player.GetDebugLabel()}");
            var playerPos = PhotonBattle.GetPlayerPos(player);
            if (PhotonBattle.IsValidGameplayPos(playerPos))
            {
                OnPhotonPlayerReady();
            }
        }

        private void OnPhotonPlayerReady()
        {
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            Debug.Log($"{PhotonNetwork.NetworkClientState} {room.GetDebugLabel()}");
            Debug.Log($"{player.GetDebugLabel()}");

            var instance = PhotonNetwork.Instantiate(_photonPrefab.name, Vector3.zero, Quaternion.identity);
            _localPhotonInstance = instance.GetComponent<PlayerDriver>();
            _isLocalPlayerInstantiated = true;
        }

        public PlayerActor OnPhotonPlayerInstantiated(Player player, PlayerDriver playerDriver)
        {
            var room = PhotonNetwork.CurrentRoom;
            Debug.Log($"{PhotonNetwork.NetworkClientState} {room.GetDebugLabel()}");
            Debug.Log($"{player.GetDebugLabel()}");
            
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var instantiationPosition = Context.GetPlayerPlayArea.GetPlayerStartPosition(playerPos);
            var playerTag = $"{playerPos}:{player.NickName}";

            var playerActor = Instantiate(_playerPrefab, instantiationPosition, Quaternion.identity);
            playerActor.name = playerActor.name.Replace("Clone", playerTag);
            playerActor.SetPlayerDriver(playerDriver);
            return playerActor;
        }
    }
}
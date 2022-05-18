using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Player
{
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
        
        [Header("Live Data"), SerializeField] private PlayerDriver _photonInstance;
        [SerializeField] private PlayerActor _playerInstance;
        
        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

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
            Debug.Log($"{PhotonNetwork.NetworkClientState} {room.GetDebugLabel()} {player.GetDebugLabel()}");
            if (_photonPrefab != null)
            {
                var instantiationPosition = Vector3.zero;
                _playerInstance = Instantiate(_playerPrefab, instantiationPosition, Quaternion.identity);
                _playerInstance.SetPhotonPlayerInstantiate(this);

                var instance = PhotonNetwork.Instantiate(_photonPrefab.name, instantiationPosition, Quaternion.identity);
                _photonInstance = instance.GetComponent<PlayerDriver>();
                _photonInstance.SetPhotonPlayerInstantiate(this);
            }
            enabled = false;
        }

        public void OnPhotonPlayerInstantiated(Photon.Realtime.Player player)
        {
            var room = PhotonNetwork.CurrentRoom;
            Debug.Log($"{PhotonNetwork.NetworkClientState} {room.GetDebugLabel()} {player.GetDebugLabel()}");
        }
    }
}
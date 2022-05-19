using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
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

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private bool _isLocalPlayerInstantiated;

        public override void OnEnable()
        {
            Assert.IsNotNull(_photonPrefab, "_photonPrefab != null");
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
            if (PhotonBattle.IsRealPlayer(player))
            {
                OnPhotonPlayerReady();
                return;
            }
            SetDebugPlayer(player);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (_isLocalPlayerInstantiated)
            {
                return;
            }
            if (!targetPlayer.Equals(PhotonNetwork.LocalPlayer))
            {
                return;
            }
            Debug.Log($"{targetPlayer.GetDebugLabel()}");
            if (PhotonBattle.IsRealPlayer(targetPlayer))
            {
                OnPhotonPlayerReady();
            }
            SetDebugPlayer(targetPlayer);
        }

        private void SetDebugPlayer(Player player)
        {
            // TODO: check that player position is valid before assigning it to local player
            // - check below is not perfect but good enough for now
            var realPlayerCount = 1 + PhotonBattle.CountRealPlayers();
            if (realPlayerCount > _debug._playerPos)
            {
                _debug._playerPos = realPlayerCount;
            }
            PhotonBattle.SetDebugPlayerProps(player, realPlayerCount, (int)_debug._playerMainSkill);
        }

        private void OnPhotonPlayerReady()
        {
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            Debug.Log($"{PhotonNetwork.NetworkClientState} {room.GetDebugLabel()}");
            Debug.Log($"{player.GetDebugLabel()}");

            PlayerDriver.Instantiate(player, _photonPrefab.name);
            _isLocalPlayerInstantiated = true;
        }
    }
}
using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

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

        [SerializeField, Header("Debug Settings")] private DebugSettings _debug;

        public override void OnJoinedRoom()
        {
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"{PhotonNetwork.NetworkClientState} {room.GetDebugLabel()}");
            Debug.Log($"{player.GetDebugLabel()}");
            var playerPos = PhotonBattle.GetPlayerPos(player);
            if (PhotonBattle.IsValidGameplayPos(playerPos))
            {
                OnPhotonPlayerInstantiated();
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
            var playerPos = PhotonBattle.GetPlayerPos(player);
            if (PhotonBattle.IsValidGameplayPos(playerPos))
            {
                OnPhotonPlayerInstantiated();
            }
        }

        private void OnPhotonPlayerInstantiated()
        {
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"{player.GetDebugLabel()}");
        }
    }
}
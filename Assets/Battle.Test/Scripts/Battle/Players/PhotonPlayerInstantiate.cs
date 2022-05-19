using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
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

        public override void OnDisable()
        {
            base.OnDisable();
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"{PhotonNetwork.NetworkClientState} {player.GetDebugLabel()}");
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
            SetDebugPlayer(player, _debug._playerPos, (int)_debug._playerMainSkill);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (!targetPlayer.Equals(PhotonNetwork.LocalPlayer))
            {
                return;
            }
            Debug.Log($"{targetPlayer.GetDebugLabel()}");
            if (PhotonBattle.IsRealPlayer(targetPlayer))
            {
                OnPhotonPlayerReady();
            }
        }

        private static void SetDebugPlayer(Player player, int wantedPlayerPos, int playerMainSkill)
        {
            var usedPlayerPositions = new HashSet<int>();
            foreach (var otherPlayer in PhotonNetwork.PlayerListOthers)
            {
                var otherPlayerPos = PhotonBattle.GetPlayerPos(otherPlayer);
                if (PhotonBattle.IsValidPlayerPos(otherPlayerPos))
                {
                    usedPlayerPositions.Add(otherPlayerPos);
                }
            }
            if (usedPlayerPositions.Contains(wantedPlayerPos))
            {
                var playerPositions = new[]
                    { PhotonBattle.PlayerPosition3, PhotonBattle.PlayerPosition2, PhotonBattle.PlayerPosition4, PhotonBattle.PlayerPosition1 };
                foreach (var playerPos in playerPositions)
                {
                    if (!usedPlayerPositions.Contains(playerPos))
                    {
                        wantedPlayerPos = playerPos;
                        break;
                    }
                }
            }
            if (!PhotonBattle.IsValidPlayerPos(wantedPlayerPos))
            {
                wantedPlayerPos = PhotonBattle.PlayerPositionSpectator;
            }
            PhotonBattle.SetDebugPlayerProps(player, wantedPlayerPos, playerMainSkill);
        }

        private void OnPhotonPlayerReady()
        {
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            Debug.Log($"{PhotonNetwork.NetworkClientState} {room.GetDebugLabel()}");
            Debug.Log($"{player.GetDebugLabel()}");

            PlayerDriver.InstantiateLocalPlayer(player, _photonPrefab.name);
            enabled = false;
        }
    }
}
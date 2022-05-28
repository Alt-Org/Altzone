using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

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
            [Min(0)] public float _timeout;
            [Range(1, 4)] public int _playerPos = 1;
            public bool _isAllocateByTeams;
            public bool _isRandomSKill;
            public Defence _playerMainSkill = Defence.Deflection;
        }

        [Header("Prefab Settings"), SerializeField] private PlayerDriverPhoton _photonPrefab;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private bool _isStopListeningForPlayerProperties;
        private bool _hasMainSKill;

        public override void OnEnable()
        {
            Assert.IsNotNull(_photonPrefab, "_photonPrefab != null");
            base.OnEnable();
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"{PhotonNetwork.NetworkClientState} {player.GetDebugLabel()}");
            if (PhotonNetwork.InRoom)
            {
                OnLocalPlayerReady();
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
                OnLocalPlayerReady();
                return;
            }
            // Trigger OnPlayerPropertiesUpdate in order to continue to OnLocalPlayerReady
            SetDebugPlayer(player);
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (_isStopListeningForPlayerProperties)
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
                OnLocalPlayerReady();
            }
        }

        private void OnLocalPlayerReady()
        {
            // Not important but give one frame slack for local player instantiation
            StartCoroutine(OnLocalPlayerReadyDelay());
        }

        private IEnumerator OnLocalPlayerReadyDelay()
        {
            _isStopListeningForPlayerProperties = true;
            yield return null;
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            Debug.Log($"{PhotonNetwork.NetworkClientState} {room.GetDebugLabel()}");
            Debug.Log($"{player.GetDebugLabel()}");

            var waitForPlayerPropertiesToUpdate = new WaitForSeconds(0.5f);
            if (PhotonBattle.IsValidPlayerPos(PhotonBattle.GetPlayerPos(player)))
            {
                SetDebugPlayer(player);
                yield return waitForPlayerPropertiesToUpdate;
            }
            var timeoutTime = Time.time + Mathf.Max(3f, _debug._timeout);
            while (!PhotonBattle.IsPlayerPosAvailable(player))
            {
                if (!PhotonNetwork.InRoom)
                {
                    yield break;
                }
                if (Time.time > timeoutTime)
                {
                    Debug.Log($"No free position found {player.GetDebugLabel()}");
                    yield break;
                }
                Debug.Log($"My player position was taken {player.GetDebugLabel()}");
                SetDebugPlayer(player);
                yield return waitForPlayerPropertiesToUpdate;
            }
            PlayerDriverPhoton.InstantiateLocalPlayer(player, _photonPrefab.name);
            enabled = false;
        }

        private void SetDebugPlayer(Player player)
        {
            if (!_hasMainSKill && _debug._isRandomSKill)
            {
                _hasMainSKill = true;
                _debug._playerMainSkill = (Defence)Random.Range((int)Defence.Desensitisation, (int)Defence.Confluence + 1);
            }
            PhotonBattle.SetDebugPlayer(player, _debug._playerPos, _debug._isAllocateByTeams, (int)_debug._playerMainSkill);
        }
    }
}
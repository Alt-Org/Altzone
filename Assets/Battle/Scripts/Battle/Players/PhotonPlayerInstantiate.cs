using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Unity.ToastMessages;
using UnityEngine;
using UnityEngine.Assertions;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Class to instantiate local Photon player using <code>PhotonNetwork.Instantiate</code>.
    /// </summary>
    internal class PhotonPlayerInstantiate : MonoBehaviourPunCallbacks
    {
        [Serializable]
        internal class DebugSettings
        {
            const string Tooltip1 = "Polling delay for Photon Custom Player propeties update";
            const string Tooltip2 = "Timeout to find free player position quickly";

            [Min(0.1f), Tooltip(Tooltip1)] public float _waitForPlayerPropertiesToUpdate = 1f;
            [Min(1), Tooltip(Tooltip2)] public float _timeoutFastWait = 3f;
            [Range(1, 4)] public int _playerPos = 1;
            public bool _isAllocateByTeams;
            public bool _isRandomSKill;
            public Defence _playerMainSkill = Defence.Projection;
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
            Debug.Log($"work start {PhotonNetwork.NetworkClientState} {player.GetDebugLabel()}");
            if (PhotonNetwork.InRoom)
            {
                OnLocalPlayerReady();
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"work done {PhotonNetwork.NetworkClientState} {player.GetDebugLabel()}");
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
            StartCoroutine(OnLocalPlayerReadyForPlay());
        }

        private IEnumerator OnLocalPlayerReadyForPlay()
        {
            _isStopListeningForPlayerProperties = true;
            yield return null;
            var room = PhotonNetwork.CurrentRoom;
            var player = PhotonNetwork.LocalPlayer;
            PhotonNetwork.NickName = room.GetUniquePlayerNameForRoom(player, PhotonNetwork.NickName, "");
            Debug.Log($"{PhotonNetwork.NetworkClientState} {room.GetDebugLabel()}");
            Debug.Log($"{player.GetDebugLabel()}");

            var delay = new WaitForSeconds(_debug._waitForPlayerPropertiesToUpdate);
            if (PhotonBattle.IsValidPlayerPos(PhotonBattle.GetPlayerPos(player)))
            {
                SetDebugPlayer(player);
                yield return delay;
            }
            var timeoutTime = Time.time + _debug._timeoutFastWait;
            while (!PhotonBattle.IsPlayerPosAvailable(player))
            {
                if (!PhotonNetwork.InRoom)
                {
                    yield break;
                }
                if (Time.time > timeoutTime)
                {
                    Debug.Log($"No free position found {player.GetDebugLabel()}");
                    ScoreFlash.Push("NO FREE PLAYER POSITION FOUND");
                    yield break;
                }
                Debug.Log($"My player position was taken {player.GetDebugLabel()}");
                SetDebugPlayer(player);
                yield return delay;
            }
            InstantiateLocalPlayer(player, _photonPrefab.name);
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

        private static void InstantiateLocalPlayer(Player player, string networkPrefabName)
        {
            Assert.IsTrue(player.IsLocal, "player.IsLocal");
            Debug.Log($"{player.GetDebugLabel()} prefab {networkPrefabName}");
            PhotonNetwork.Instantiate(networkPrefabName, Vector3.zero, Quaternion.identity);
        }
    }
}
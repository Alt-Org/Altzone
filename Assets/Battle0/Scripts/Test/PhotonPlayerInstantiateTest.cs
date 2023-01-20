using System;
using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using Battle0.Scripts.Battle.Players;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Unity.ToastMessages;
using UnityEngine;
using UnityEngine.Assertions;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

namespace Battle0.Scripts.Test
{
    /// <summary>
    /// Class to instantiate local Photon player using <code>PhotonNetwork.Instantiate</code>.
    /// </summary>
    internal class PhotonPlayerInstantiateTest : MonoBehaviourPunCallbacks
    {
        [Serializable]
        internal class DebugSettings
        {
            private const string Tooltip1 = "Polling delay for Photon Custom Player propeties update";
            private const string Tooltip2 = "Timeout to find free player position quickly";
            private const string Tooltip3 = "Preferred player start position for first player to come";
            private const string Tooltip4 = "Allocate new players for one team at a time, default is to use alternating teams";

            [Min(0.1f), Tooltip(Tooltip1)] public float _waitForPlayerPropertiesToUpdate = 1f;
            [Min(1), Tooltip(Tooltip2)] public float _timeoutFastWait = 3f;
            [Range(1, 4), Tooltip(Tooltip3)] public int _playerPos = 1;
            [Tooltip(Tooltip4)] public bool _isAllocateByTeams;
            public bool _isRandomSKill;
            public Defence _playerMainSkill1 = Defence.Introjection;
            public Defence _playerMainSkill2 = Defence.Desensitisation;
            public Defence _playerMainSkill3 = Defence.Projection;
            public Defence _playerMainSkill4 = Defence.Retroflection;
        }

        [Header("Prefab Settings"), SerializeField] private PlayerDriverPhoton _photonPrefab;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private bool _isStopListeningForPlayerProperties;

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
            if (!PhotonBattle.IsValidPlayerPos(PhotonBattle.GetPlayerPos(player)))
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
            Defence mainSKill;
            if (_debug._isRandomSKill)
            {
                mainSKill = (Defence)Random.Range((int)Defence.Desensitisation, (int)Defence.Confluence + 1);
            }
            else
            {
                var number = player.ActorNumber % 4;
                mainSKill = number switch
                {
                    1 => _debug._playerMainSkill1,
                    2 => _debug._playerMainSkill2,
                    3 => _debug._playerMainSkill3,
                    0 => _debug._playerMainSkill4,
                    _ => throw new UnityException()
                };
            }
            PhotonBattle.SetDebugPlayer(player, _debug._playerPos, _debug._isAllocateByTeams, (int)mainSKill);
        }

        private static void InstantiateLocalPlayer(Player player, string networkPrefabName)
        {
            Assert.IsTrue(player.IsLocal, "player.IsLocal");
            Debug.Log($"{player.GetDebugLabel()} prefab {networkPrefabName}");
            PhotonNetwork.Instantiate(networkPrefabName, Vector3.zero, Quaternion.identity);
        }
    }
}
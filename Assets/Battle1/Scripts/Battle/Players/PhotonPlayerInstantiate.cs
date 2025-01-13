#if false
using System;
using System.Collections;
using Battle1.PhotonUnityNetworking.Code;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using Player = Battle1.PhotonRealtime.Code.Player;

namespace Battle1.Scripts.Battle.Players
{
    /// <summary>
    /// Instantiates a Photon player using <code>PhotonNetwork.Instantiate</code>.<br />
    /// Note that Photon player will be instantiated in all game clients that are in the room when this script is enabled in a scene.
    /// </summary>
    /// <remarks>
    /// In production we are already in the room but for testing we wait that a room is created
    /// and then we try to find a free player position to instantiate a Photon player there for this room.
    /// </remarks>
    public class PhotonPlayerInstantiate : MonoBehaviourPunCallbacks
    {
        [Serializable]
        internal class DebugSettings
        {
            private const string Tooltip1 = "Polling delay for Photon Custom Player propeties update";
            private const string Tooltip2 = "Timeout to find free player position quickly";
            private const string Tooltip3 = "Preferred player start position for first player to come";
            private const string Tooltip4 = "Preferred player start position for first player to come";

            [Min(0.1f), Tooltip(Tooltip1)] public float _waitForPlayerPropertiesToUpdate = 1f;
            [Min(1), Tooltip(Tooltip2)] public float _timeoutFastWait = 3f;
            [Range(1, 4), Tooltip(Tooltip3)] public int _playerPos = 1;
            [Range(1, 4), Tooltip(Tooltip4)] public int _playerPrefabId = 1;
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
            PhotonBattle.SetDebugPlayer(player, _debug._playerPos, _debug._playerPrefabId);
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
            var delay = new WaitForSeconds(_debug._waitForPlayerPropertiesToUpdate);
            yield return delay;
            var player = PhotonNetwork.LocalPlayer;
            if (!PhotonBattle.IsValidPlayerPos(PhotonBattle.GetPlayerPos(player)))
            {
                PhotonBattle.SetDebugPlayer(player, _debug._playerPos, _debug._playerPrefabId);
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
                    yield break;
                }
                Debug.Log($"My player position was taken {player.GetDebugLabel()}");
                PhotonBattle.SetDebugPlayer(player, _debug._playerPos, _debug._playerPrefabId);
                yield return delay;
            }
            InstantiateLocalPlayer(player, _photonPrefab.name);
            enabled = false;
        }

        private static void InstantiateLocalPlayer(Player player, string networkPrefabName)
        {
            Assert.IsTrue(player.IsLocal, "player.IsLocal");
            Debug.Log($"{player.GetDebugLabel()} prefab {networkPrefabName}");
            // This prefab will be instantiated in all game clients - but only this one below will handle input!
            var instance = PhotonNetwork.Instantiate(networkPrefabName, Vector3.zero, Quaternion.identity);
            // Connect local player input handler to photon player driver. 
            var playerDriver = instance.GetComponent<PlayerDriverPhoton>();
            Assert.IsNotNull(playerDriver, $"top level 'PlayerDriverPhoton' is missing from prefab {networkPrefabName}");
            var playerInputHandler = Context.GetPlayerInputHandler;
            playerInputHandler._hostForInput = instance;
            playerInputHandler.OnMoveTo = playerDriver.MoveTo;
        }
 
    }
}
#endif

using System.Collections;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Test.Photon
{
    /// <summary>
    /// Test Photon PUN 2 RPC functionality for better understanding how it works.<be />
    /// We use Photon MasterClient instance to send and receive RPC messages over network
    /// and show some (timing) details about this on all clients in the room.
    /// </summary>
    public class PhotonNetworkTest : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private PhotonView _photonView;
        [SerializeField] private bool _isMasterClient;
        [SerializeField] private string _playerName;
        [SerializeField] private PhotonTestController _controller;

        private int _startFrameCount;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            var owner = _photonView.Owner;
            _isMasterClient = owner.IsMasterClient;
            _playerName = owner.GetDebugLabel();
            name = name.Replace("Clone", _playerName);
            Debug.Log($"{_playerName} {PhotonNetwork.NetworkClientState}");
        }

        private IEnumerator Start()
        {
            _startFrameCount = Time.frameCount;
            Debug.Log($"{_playerName} {PhotonNetwork.NetworkClientState} startFrameCount {_startFrameCount}");
            if (!_isMasterClient)
            {
                yield break;
            }
            // Wait until PhotonTestController has been created.
            _controller = PhotonTestController.Get();
            if (_controller == null)
            {
                yield return new WaitUntil(() => (_controller = PhotonTestController.Get()) != null);
            }
            // Photon Master Client instance can send test messages (on behalf of local player).
            _controller.SetMasterClientPhotonView(_photonView);
            _controller.SetMasterClientTestButton(OnTestButton);
        }

        private void OnTestButton()
        {
            var frameCount = Time.frameCount - _startFrameCount;
            var timestamp = PhotonNetwork.ServerTimestamp;
            var lastRoundTripTime = PhotonNetwork.NetworkingClient.LoadBalancingPeer.LastRoundTripTime;
            var localPlayerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            Debug.Log($"SEND frame {frameCount} time {(uint)timestamp} last rtt {lastRoundTripTime} player {localPlayerActorNumber}", this);
            Assert.IsTrue(_isMasterClient);
            _photonView.RPC(nameof(FrameSyncTest), RpcTarget.All, frameCount, timestamp, lastRoundTripTime, localPlayerActorNumber);
        }

        [PunRPC]
        private void FrameSyncTest(int frameCount, int timestamp, int lastRoundTripTime, int localPlayerActorNumber, PhotonMessageInfo info)
        {
            Debug.Log($"RECV frame {frameCount} time {(uint)timestamp} last rtt {lastRoundTripTime} player {localPlayerActorNumber}", this);
            var localPlayer = PhotonNetwork.PlayerList.FirstOrDefault(x => x.ActorNumber == localPlayerActorNumber);
            _controller.ShowRecvFrameSyncTest(frameCount, timestamp, lastRoundTripTime, info, localPlayer);
        }
    }
}

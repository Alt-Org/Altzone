using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Test.Photon
{
    /// <summary>
    /// Test Photon PUN 2 RPC functionality for better understanding how it works.
    /// </summary>
    public class PhotonNetworkTest : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private PhotonView _photonView;
        [SerializeField] private bool _isMasterClient;
        [SerializeField] private bool _isLocalPlayer;
        [SerializeField] private string _playerName;
        [SerializeField] private PhotonTestController _controller;

        private int _startFrameCount;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            var owner = _photonView.Owner;
            _isMasterClient = owner.IsMasterClient;
            _isLocalPlayer = owner.IsLocal;
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
            // We show only Photon Master Client info (local or remote) when PhotonTestController has been created.
            yield return new WaitUntil(() => (_controller = PhotonTestController.Get()) != null);
            _controller.SetPhotonView(_photonView);
            // Any Photon Master Client instance can send test messages!
            _controller.SetTestButton(OnTestButton);
        }

        private void OnTestButton()
        {
            var frameCount = Time.frameCount - _startFrameCount;
            var timestamp = PhotonNetwork.ServerTimestamp;
            var lastRoundTripTime = PhotonNetwork.NetworkingClient.LoadBalancingPeer.LastRoundTripTime;
            Debug.Log($"SEND frame {frameCount} time {(uint)timestamp} last rtt {lastRoundTripTime}", this);
            Assert.IsTrue(_isMasterClient);
            _photonView.RPC(nameof(FrameSyncTest), RpcTarget.All, frameCount, timestamp, lastRoundTripTime);
        }

        [PunRPC]
        private void FrameSyncTest(int frameCount, int timestamp, int lastRoundTripTime, PhotonMessageInfo info)
        {
            Debug.Log($"RECV FrameSyncTest frame {frameCount} time {(uint)timestamp} last rtt {lastRoundTripTime}", this);
            _controller.ShowRecvFrameSyncTest(frameCount, timestamp, lastRoundTripTime, info);
        }
    }
}

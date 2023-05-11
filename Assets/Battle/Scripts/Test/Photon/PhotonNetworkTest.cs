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

        private void OnEnable()
        {
            Debug.Log($"{_playerName} {PhotonNetwork.NetworkClientState}");
            if (!_isMasterClient)
            {
                return;
            }
            // We show only Photon Master Client info (local or remote).
            _controller = PhotonTestController.Get();
            _controller.SetPhotonView(_photonView);
            if (_isLocalPlayer)
            {
                // Only local Photon Master Client can send and receive test messages!
                _controller.SetTestButton(OnTestButton);
            }
        }

        private void OnTestButton()
        {
            var frameCount = Time.frameCount;
            var timestamp = PhotonNetwork.ServerTimestamp;
            Debug.Log($"SEND frame {frameCount} time {(uint)timestamp}", this);
            Assert.IsTrue(_isLocalPlayer);
            Assert.IsTrue(_isMasterClient);
            _photonView.RPC(nameof(FrameSyncTest), RpcTarget.All, frameCount, timestamp);
        }

        [PunRPC]
        private void FrameSyncTest(int frameCount, int timestamp, PhotonMessageInfo info)
        {
            Debug.Log($"RECV FrameSyncTest frame {frameCount} time {(uint)timestamp}", this);
            _controller.ShowRecvFrameSyncTest(frameCount, timestamp, info);
        }
    }
}

using System.Collections;
using System.Linq;
/*using Battle1.PhotonUnityNetworking.Code;*/
using UnityEngine;
using UnityEngine.Assertions;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

namespace Battle1.Scripts.Test.Photon
{
    /// <summary>
    /// Test Photon PUN 2 RPC functionality for better understanding how it works.<be />
    /// We use Photon MasterClient instance to send and receive RPC messages over network
    /// and show some (timing) details about this on all clients in the room.
    /// </summary>
    public class PhotonNetworkTest : MonoBehaviour
    {
        /*[Header("Live Data"), SerializeField] private PhotonView _photonView;*/
        [SerializeField] private bool _isMasterClient;
        [SerializeField] private string _playerName;
        [SerializeField] private PhotonTestController _controller;

        private int _startFrameCount;

        // Improve network marshalling performance and put all parameters with similar type in array.
        private readonly int[] _sendBuffer = new int[4];

        private void Awake()
        {
            /* _photonView = PhotonView.Get(this);
             var owner = _photonView.Owner;*/
            var owner = "IDK";
            /*_isMasterClient = owner.IsMasterClient;
            _playerName = owner.GetDebugLabel();*/
            _isMasterClient = true;
            _playerName = "owner.GetDebugLabel()";
            name = name.Replace("Clone", _playerName);
           /* Debug.Log($"{_playerName} {PhotonNetwork.NetworkClientState}");*/
        }

        private IEnumerator Start()
        {
            _startFrameCount = Time.frameCount;
           /* Debug.Log($"{_playerName} {PhotonNetwork.NetworkClientState} startFrameCount {_startFrameCount}");*/
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
  /*          _controller.SetMasterClientPhotonView(_photonView);*/
            _controller.SetMasterClientTestButton(OnTestButton);
        }

        private void OnTestButton()
        {
            var frameCount = Time.frameCount - _startFrameCount;
            /*var timestamp = PhotonNetwork.ServerTimestamp;
            var lastRoundTripTime = PhotonNetwork.NetworkingClient.LoadBalancingPeer.LastRoundTripTime;
            var localPlayerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;*/
            var timestamp = 0;
            var lastRoundTripTime = 0;
            var localPlayerActorNumber = 0;
           /* Debug.Log($"SEND frame {frameCount} time {(uint)timestamp} last rtt {lastRoundTripTime} player {localPlayerActorNumber}", this);*/
            Assert.IsTrue(_isMasterClient);
            // We use int array to improve marshalling on Photon networking layer - contradictory to KISS principle.
            var index = -1;
            _sendBuffer[++index] = frameCount;
            _sendBuffer[++index] = timestamp;
            _sendBuffer[++index] = lastRoundTripTime;
            _sendBuffer[++index] = localPlayerActorNumber;
            /*_photonView.RPC(nameof(FrameSyncTest), RpcTarget.All, _sendBuffer);*/
        }

        /*[PunRPC]
        private void FrameSyncTest(int[] recvBuffer, PhotonMessageInfo info)
        {
            var index = -1;
            var frameCount = recvBuffer[++index];
            var timestamp = recvBuffer[++index];
            var lastRoundTripTime = recvBuffer[++index];
            var localPlayerActorNumber = recvBuffer[++index];
            Debug.Log($"RECV frame {frameCount} time {(uint)timestamp} last rtt {lastRoundTripTime} player {localPlayerActorNumber}", this);
            var localPlayer = PhotonNetwork.PlayerList.FirstOrDefault(x => x.ActorNumber == localPlayerActorNumber);
            _controller.ShowRecvFrameSyncTest(frameCount, timestamp, lastRoundTripTime, info, localPlayer);
        }*/
    }
}

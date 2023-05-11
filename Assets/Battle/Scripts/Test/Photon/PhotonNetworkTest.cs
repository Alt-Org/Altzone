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
            var controller = PhotonTestController.Get();
            controller.SetPhotonView(_photonView);
            if (_isLocalPlayer && _isMasterClient)
            {
                // Only local Photon Master Client can sen test messages.
                controller.SetTestButton(OnTestButton);
            }
        }

        private void OnTestButton()
        {
            Debug.Log($"{_playerName} send FrameSyncTest");
            Assert.IsTrue(_isLocalPlayer);
            Assert.IsTrue(_isMasterClient);
            _photonView.RPC(nameof(FrameSyncTest), RpcTarget.All);
        }

        [PunRPC]
        private void FrameSyncTest()
        {
            Debug.Log($"{_playerName} recv FrameSyncTest");
        }
    }
}

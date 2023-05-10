using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Test.Photon
{
    public class PhotonNetworkTest : MonoBehaviour
    {
        [Header("Live Data"), SerializeField] private PhotonView _photonView;
        [SerializeField] private bool _isMasterClient;
        [SerializeField] private bool _isLocalPlayer;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            _isMasterClient = _photonView.Owner.IsMasterClient;
            _isLocalPlayer = _photonView.Owner.IsLocal;
            name = name.Replace("Clone", $"Actor {_photonView.OwnerActorNr}");
        }

        private void OnEnable()
        {
            Debug.Log($"state {PhotonNetwork.NetworkClientState}");
            PhotonTestController.SetPhotonViewForUi(_photonView, OnTestButton);
        }

        private void OnTestButton()
        {
            Debug.Log("send FrameSyncTest");
            Assert.IsTrue(PhotonNetwork.IsMasterClient);
            _photonView.RPC(nameof(FrameSyncTest), RpcTarget.All);
        }

        [PunRPC]
        private void FrameSyncTest()
        {
            Debug.Log("recv FrameSyncTest");
        }
    }
}

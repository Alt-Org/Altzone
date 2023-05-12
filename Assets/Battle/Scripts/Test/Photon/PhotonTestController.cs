using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Test.Photon
{
    public class PhotonTestController : MonoBehaviour
    {
        public static PhotonTestController Get() => _instance;

        private static PhotonTestController _instance;

        [SerializeField] private PhotonTestView _view;

        private int _startFrameCount;
        private PhotonView _photonView;

        private void Awake()
        {
            _instance = this;
        }

        private void OnEnable()
        {
            _startFrameCount = Time.frameCount;
            Debug.Log($"{PhotonNetwork.NetworkClientState} startFrameCount {_startFrameCount}");
            _view.ResetView();
        }

        public void SetMasterClientTestButton(Action onTestButton)
        {
            // Only Photon Master Client should do this.
            _view.TestButton.interactable = true;
            _view.TestButton.onClick.AddListener(() => onTestButton());
        }

        public void SetMasterClientPhotonView(PhotonView photonView)
        {
            Assert.IsTrue(photonView.Owner.IsMasterClient);
            _view.SetPhotonView(PhotonNetwork.LocalPlayer, photonView.Owner);
        }

        public void ShowRecvFrameSyncTest(int frameCount, int timestamp, int lastRoundTripTime, PhotonMessageInfo info, Player sendingPlayer)
        {
            _view.ShowRecvFrameSyncTest(
                frameCount,
                timestamp, lastRoundTripTime,
                info.SentServerTimestamp,
                Time.frameCount - _startFrameCount,
                sendingPlayer);
        }
    }
}

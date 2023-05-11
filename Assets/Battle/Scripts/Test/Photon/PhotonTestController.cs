using System;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Test.Photon
{
    public class PhotonTestController : MonoBehaviour
    {
        public static PhotonTestController Get() => _instance;

        private static PhotonTestController _instance;

        [SerializeField] private PhotonTestView _view;

        private bool _isStarted;
        private PhotonView _photonView;

        private void Awake()
        {
            _instance = this;
        }

        private void OnEnable()
        {
            Debug.Log($"{PhotonNetwork.NetworkClientState}");
            _view.ResetView();
            if (!_isStarted)
            {
                _isStarted = true;
            }
        }

        public void SetTestButton(Action onTestButton)
        {
            // Only Photon Master Client should do this.
            _view.TestButton.interactable = true;
            _view.TestButton.onClick.AddListener(() => onTestButton());
        }

        public void SetPhotonView(PhotonView photonView)
        {
            _view.SetPhotonView(photonView);
        }

        public void ShowRecvFrameSyncTest(int frameCount, int timestamp, PhotonMessageInfo info)
        {
            _view.ShowRecvFrameSyncTest(frameCount, timestamp, Time.frameCount, info.SentServerTimestamp);
        }
    }
}

using System;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Test.Photon
{
    public class PhotonTestController : MonoBehaviour
    {
        [SerializeField] private PhotonTestView _view;

        private static PhotonTestController _instance;

        private bool _isStarted;
        private PhotonView _photonView;

        private void Awake()
        {
            _instance = this;
        }

        private void OnEnable()
        {
            _view.ResetView();
            if (!_isStarted)
            {
                _isStarted = true;
            }
        }

        public static void SetPhotonViewForUi(PhotonView photonView, Action onTestButton)
        {
            if (photonView.Owner.IsMasterClient)
            {
                _instance._view.TestButton.onClick.AddListener(() => onTestButton());
            }
            else
            {
                _instance._view.TestButton.interactable = false;
            }
        }
    }
}

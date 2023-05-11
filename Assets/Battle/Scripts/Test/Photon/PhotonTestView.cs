using System;
using System.Collections;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.Test.Photon
{
    public class PhotonTestView : MonoBehaviour
    {
        [SerializeField] private Button _testButton;

        [Header("Player"), SerializeField] private TextMeshProUGUI _playerLabel;
        [SerializeField] private TextMeshProUGUI _playerText;

        [Header("Rpc"), SerializeField] private TextMeshProUGUI _rpcLabel;
        [SerializeField] private TextMeshProUGUI _rpcText1;
        [SerializeField] private TextMeshProUGUI _rpcText2;
        [SerializeField] private TextMeshProUGUI _rpcText3;
        [SerializeField] private TextMeshProUGUI _rpcText4;
        [SerializeField] private TextMeshProUGUI _rpcText5;

        [Header("Player"), SerializeField] private TextMeshProUGUI _pingLabel;
        [SerializeField] private TextMeshProUGUI _pingText;

        public Button TestButton => _testButton;

        private string _currentRegion;

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void ResetView()
        {
            _testButton.interactable = false;
            foreach (var text in new[]
                     {
                         _playerLabel, _playerText,
                         _rpcLabel, _rpcText1, _rpcText2, _rpcText3, _rpcText4, _rpcText5,
                         _pingLabel, _pingText
                     })
            {
                text.text = string.Empty;
            }
            _rpcLabel.text = "Rpc";
            _pingLabel.text = "Ping";
            _currentRegion = string.Empty;
            StopAllCoroutines();
        }

        public void SetPhotonView(PhotonView photonView)
        {
            _currentRegion = PhotonLobby.GetRegion();
            _playerLabel.text = $"{PhotonNetwork.CurrentRoom.Name} {_currentRegion}";
            var playerLabel = photonView.Owner.GetDebugLabel();
            Debug.Log($"{playerLabel}");
            _playerText.text = playerLabel;
            StartCoroutine(PingPoller());
        }

        private IEnumerator PingPoller()
        {
            var delay = new WaitForSeconds(2f);
            yield return delay;
            var peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
            while (enabled)
            {
                _pingText.text = $"{_currentRegion} {peer.RoundTripTime} ms (~{peer.RoundTripTimeVariance} ms)";
                yield return delay;
            }
        }

        public void ShowRecvFrameSyncTest(int rpcFrameCount, int rpcTimestamp, int rpcLastRoundTripTime, int curFrameCount, int msgTimestamp)
        {
            var serverTimestamp = PhotonNetwork.ServerTimestamp;
            var rpcDelta = (uint)serverTimestamp - (uint)rpcTimestamp;
            var delta1 = (uint)msgTimestamp - (uint)rpcTimestamp;
            var delta2 = (uint)serverTimestamp - (uint)msgTimestamp;
            int frameDelta = (int)((uint)curFrameCount - (uint)rpcFrameCount);
            var frameDeltaText = frameDelta >= 0
                ? $"+{frameDelta:000}"
                : $"{frameDelta:000}";
            _rpcLabel.text = $"Rpc: rtt {rpcLastRoundTripTime:000} d{rpcDelta:000}";
            _rpcText1.text = $"t{(uint)rpcTimestamp:0 000 000} rpc <b>sent</b>";
            _rpcText2.text = $"t{(uint)msgTimestamp:0 000 000} msg info : d{delta1:000}";
            _rpcText3.text = $"t{(uint)serverTimestamp:0 000 000} cur recv : d{delta2:000}";
            _rpcText4.text = $"f{(uint)rpcFrameCount:0 000 000} rpc <b>sent</b>";
            _rpcText5.text = $"f{(uint)curFrameCount:0 000 000} cur game : d{frameDeltaText}";
        }
    }
}

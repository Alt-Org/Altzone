using System;
using System.Collections;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using Player = Battle1.PhotonRealtime.Code.Player;*/

namespace Battle1.Scripts.Test.Photon
{
    public class PhotonTestView : MonoBehaviour
    {
        [SerializeField] private Button _testButton;

        [Header("Player"), SerializeField] private TextMeshProUGUI _playerLabel;
        [SerializeField] private TextMeshProUGUI _playerText;
        [SerializeField] private TextMeshProUGUI _masterClientText;

        [Header("Rpc"), SerializeField] private TextMeshProUGUI _rpcLabel;
        [SerializeField] private TextMeshProUGUI _rpcText0;
        [SerializeField] private TextMeshProUGUI _rpcText1;
        [SerializeField] private TextMeshProUGUI _rpcText2;
        [SerializeField] private TextMeshProUGUI _rpcText3;
        [SerializeField] private TextMeshProUGUI _rpcText4;
        [SerializeField] private TextMeshProUGUI _rpcText5;

        [Header("Player"), SerializeField] private TextMeshProUGUI _pingLabel;
        [SerializeField] private TextMeshProUGUI _pingText;

        [Header("Player"), SerializeField] private TextMeshProUGUI _gameInfoTextLines;

        public Button TestButton => _testButton;

        private string _currentRegion;
        private int _currentPlayers;
        private int _minPing;
        private int _maxPing;

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void ResetView()
        {
            _testButton.interactable = false;
            foreach (var text in new[]
                     {
                         _playerLabel, _playerText, _masterClientText,
                         _rpcLabel, _rpcText0, _rpcText1, _rpcText2, _rpcText3, _rpcText4, _rpcText5,
                         _pingLabel, _pingText,
                         _gameInfoTextLines
                     })
            {
                text.text = string.Empty;
            }
            _rpcLabel.text = "Rpc";
            _pingLabel.text = "Ping";
            _currentRegion = string.Empty;
            _currentPlayers = 0;
            _minPing = int.MaxValue;
            _maxPing = int.MinValue;
            StopAllCoroutines();
        }

        public void SetPhotonView(Player localPlayer, Player masterClient)
        {
            /*_currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;*/
            _currentPlayers = 0;
            _currentRegion = PhotonLobby.GetRegion();
            /*_playerLabel.text = $"{_currentPlayers} in {PhotonNetwork.CurrentRoom.Name} {_currentRegion}";*/
            _playerLabel.text = $"{_currentPlayers} in room 0";
            _playerText.text = $"Player: <b>{localPlayer.GetDebugLabel(false)}</b>";
            _masterClientText.text = $"Master: {masterClient.GetDebugLabel(false)}";
            StartCoroutine(PingPoller());
            var refreshRate = Screen.currentResolution.refreshRateRatio.value;
            if (double.IsNaN(refreshRate))
            {
                // Fix Simulator etc.
                refreshRate = 0;
            }
           /* _gameInfoTextLines.text = "<b>Info</b>" +
                                      $"\r\nDate {DateTime.Now:yyyy-dd-MM HH:mm}" +
                                      $"\r\nPhoton ver {PhotonLobby.GameVersion}" +
                                      $"\r\nPhoton send rate {PhotonNetwork.SendRate} Hz" +
                                      $"\r\nGame phys step {1f / Time.fixedDeltaTime} Hz" +
                                      $"\r\nGame frame rate {Application.targetFrameRate} Hz" +
                                      $"\r\nScreen {Screen.currentResolution.width}x{Screen.currentResolution.height} {refreshRate} Hz";
            if (PhotonNetwork.SerializationRate != PhotonNetwork.SendRate)
            {
                _gameInfoTextLines.text += $"\r\nSerialization rate {PhotonNetwork.SerializationRate} Hz";
            }*/
        }

        private IEnumerator PingPoller()
        {
            var delay = new WaitForSeconds(2f);
            yield return delay;
           /* var peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
            while (enabled && PhotonNetwork.InRoom)
            {
                var currentPing = peer.RoundTripTime;
                if (currentPing < _minPing)
                {
                    _minPing = currentPing;
                }
                if (currentPing > _maxPing)
                {
                    _maxPing = currentPing;
                }
                _pingText.text =
                    $"{_currentRegion} {currentPing} [{_minPing}-{_maxPing}] ms (~{peer.RoundTripTimeVariance} ms)";
                if (_currentPlayers != PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    _currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
                    _playerLabel.text = $"{_currentPlayers} in {PhotonNetwork.CurrentRoom.Name} {_currentRegion}";
                }
                yield return delay;
            }*/
        }

        public void ShowRecvFrameSyncTest(
            int rpcFrameCount, int rpcTimestamp, int rpcLastRoundTripTime,
            int msgTimestamp, int curFrameCount, Player sendingPlayer)
        {
            /*var serverTimestamp = PhotonNetwork.ServerTimestamp;*/
            var serverTimestamp = 0;
            var rpcDelta = (uint)serverTimestamp - (uint)rpcTimestamp;
            var delta1 = (uint)msgTimestamp - (uint)rpcTimestamp;
            var delta2 = (uint)serverTimestamp - (uint)msgTimestamp;
            int frameDelta = (int)((uint)curFrameCount - (uint)rpcFrameCount);
            var frameDeltaText = frameDelta >= 0
                ? $"+{frameDelta:000}"
                : $"{frameDelta:000}";
            _rpcLabel.text = $"Rpc: rtt {rpcLastRoundTripTime:000} d{rpcDelta:000}";
            _rpcText0.text = $"From: <b>{sendingPlayer?.NickName ?? "MISSING"}</b>";
            _rpcText1.text = $"t{(uint)rpcTimestamp:0 000 000} rpc <b>sent</b>";
            _rpcText2.text = $"t{(uint)msgTimestamp:0 000 000} msg info : d{delta1:000}";
            _rpcText3.text = $"t{(uint)serverTimestamp:0 000 000} cur recv : d{delta2:000}";
            _rpcText4.text = $"f{(uint)rpcFrameCount:0 000 000} rpc <b>sent</b>";
            _rpcText5.text = $"f{(uint)curFrameCount:0 000 000} cur game : d{frameDeltaText}";
        }
    }
}

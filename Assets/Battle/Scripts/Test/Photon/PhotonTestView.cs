using Photon.Pun;
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

        public Button TestButton => _testButton;

        public void ResetView()
        {
            _testButton.interactable = false;
            foreach (var text in new[]
                     {
                         _playerLabel, _playerText,
                         _rpcLabel, _rpcText1, _rpcText2, _rpcText3, _rpcText4, _rpcText5
                     })
            {
                text.text = string.Empty;
            }
            _rpcLabel.text = "Frame Sync Rpc";
        }

        public void SetPhotonView(PhotonView photonView)
        {
            _playerLabel.text = $"{PhotonNetwork.CurrentRoom.Name} {PhotonNetwork.NetworkingClient.CloudRegion}";
            var playerLabel = photonView.Owner.GetDebugLabel();
            Debug.Log($"{playerLabel}");
            _playerText.text = playerLabel;
        }

        public void ShowRecvFrameSyncTest(int rpcFrameCount, int rpcTimestamp, int curFrameCount, int msgTimestamp)
        {
            var serverTimestamp = PhotonNetwork.ServerTimestamp;
            _rpcText1.text = $"{(uint)rpcTimestamp:0 000 000} rpc sent time";
            _rpcText2.text = $"{(uint)msgTimestamp:0 000 000} msg info time";
            _rpcText3.text = $"{(uint)serverTimestamp:0 000 000} cur recv time";
            _rpcText4.text = $"{rpcFrameCount:0 000 000} rpc sent frame";
            _rpcText5.text = $"{curFrameCount:0 000 000} cur game frame";
        }
    }
}

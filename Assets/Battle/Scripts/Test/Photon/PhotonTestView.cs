using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.Test.Photon
{
    public class PhotonTestView : MonoBehaviour
    {
        [SerializeField] private Button _testButton;

        [SerializeField] private TextMeshProUGUI _playerLabel;
        [SerializeField] private TextMeshProUGUI _playerText;

        public Button TestButton => _testButton;

        public void ResetView()
        {
            _testButton.interactable = false;
        }

        public void SetPhotonView(PhotonView photonView)
        {
            _playerLabel.text = $"{PhotonNetwork.CurrentRoom.Name} {PhotonNetwork.NetworkingClient.CloudRegion}";
            var playerLabel = photonView.Owner.GetDebugLabel();
            Debug.Log($"{playerLabel}");
            _playerText.text = playerLabel;
        }
    }
}

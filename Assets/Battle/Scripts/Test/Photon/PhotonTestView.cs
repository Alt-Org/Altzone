using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.Test.Photon
{
    public class PhotonTestView : MonoBehaviour
    {
        [SerializeField] private Button _testButton;

        public Button TestButton => _testButton;

        public void ResetView()
        {
            _testButton.interactable = false;
        }
    }
}
